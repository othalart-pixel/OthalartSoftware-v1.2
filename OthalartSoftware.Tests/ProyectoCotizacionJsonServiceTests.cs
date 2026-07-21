using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace OthalartSoftware.Tests;

public class ProyectoCotizacionJsonServiceTests
{
    [Fact]
    public void Expansion_conserva_cantidad_capacidad_y_periodo_editados_en_proceso_local()
    {
        ProcesoProyecto proceso = new ProcesoProyecto
        {
            Id = "proceso_rough",
            Nombre = "Rough animation",
            Cantidad = 2m,
            Unidad = "segundos",
            Capacidad = 3.5m,
            Periodo = "dia",
            Resultado = new ResultadoProcesoProyecto
            {
                HorasCalculadas = 4m,
                HorasAsignadas = 4m
            }
        };
        SubproductoProyecto subproducto = new SubproductoProyecto
        {
            Id = "sub_rough",
            Nombre = "Rough animation",
            Cantidad = 2m,
            Unidad = "segundos",
            Procesos = new List<ProcesoProyecto> { proceso }
        };
        ProyectoCotizacion proyecto =
            ProyectoCotizacionJsonService.CrearProyectoVacio("Prueba", "Cliente");
        proyecto.Grupos[0].Items.Add(new ProductoProyecto
        {
            Id = "producto",
            Nombre = "Animación",
            Subproductos = new List<SubproductoProyecto> { subproducto }
        });

        FilaProductivaProyecto fila =
            ProyectoProductivoExpansionService.Expandir(proyecto).Filas.Single();

        Assert.Equal(2m, fila.Cantidad);
        Assert.Equal("segundos", fila.Unidad);
        Assert.Equal(3.5m, fila.Capacidad);
        Assert.Equal("dia", fila.Periodo);
    }

    [Fact]
    public void CrearProyectoVacio_no_reutiliza_estado_de_otro_proyecto()
    {
        ProyectoCotizacion anterior =
            ProyectoCotizacionJsonService.CrearProyectoVacio("Anterior", "Cliente A");
        anterior.Grupos[0].Items.Add(new ProductoProyecto
        {
            Id = "producto_anterior",
            Nombre = "Configuración anterior",
            Overrides = new List<OverrideProductivo>
            {
                new OverrideProductivo { Campo = "Cantidad", ValorJson = "12" }
            }
        });
        anterior.Metadata.Valores["TipoProyecto"] = "Animación";
        anterior.Warnings.Add("Advertencia anterior");

        ProyectoCotizacion nuevo =
            ProyectoCotizacionJsonService.CrearProyectoVacio("Nuevo", "Cliente B");

        Assert.NotSame(anterior, nuevo);
        Assert.NotSame(anterior.Grupos, nuevo.Grupos);
        Assert.NotSame(anterior.Metadata, nuevo.Metadata);
        Assert.All(nuevo.Grupos, grupo => Assert.Empty(grupo.Items));
        Assert.Empty(nuevo.Metadata.Valores);
        Assert.Empty(nuevo.Warnings);
        Assert.Equal("Nuevo", nuevo.Nombre);
        Assert.Equal("Cliente B", nuevo.Cliente);
    }
}
