using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Reports;

namespace OthalartSoftware.Tests;

public class InformeProyectoBuilderTests
{
    [Fact]
    public void GenerarHtml_muestra_trabajadores_cargos_sueldos_y_jerarquia_productiva()
    {
        ProcesoProyecto proceso = new ProcesoProyecto
        {
            Id = "proc_rough",
            Nombre = "Rough animation",
            EtapaId = "produccion",
            Resultado = new ResultadoProcesoProyecto
            {
                HorasCalculadas = 8m,
                HorasAsignadas = 8m,
                CostoCalculado = 80000m
            },
            Asignaciones = new List<AsignacionProductiva>
            {
                new AsignacionProductiva
                {
                    Id = "asig_ana",
                    ProcesoProyectoId = "proc_rough",
                    PersonaId = "persona_ana",
                    CargoId = "Animador 2D",
                    HorasCalculadas = 8m,
                    HorasAsignadas = 8m,
                    CostoCalculado = 80000m
                }
            }
        };
        SubproductoProyecto subproducto = new SubproductoProyecto
        {
            Id = "sub_rough",
            ProductoProyectoId = "producto_personaje",
            Nombre = "Rough",
            Cantidad = 1m,
            Unidad = "segundos",
            Procesos = new List<ProcesoProyecto> { proceso }
        };
        ProyectoCotizacion proyecto = new ProyectoCotizacion
        {
            Id = "proyecto_informe",
            Nombre = "Cortometraje",
            Cliente = "Cliente ejemplo",
            Grupos = new List<GrupoProyecto>
            {
                new GrupoProyecto
                {
                    Id = "grupo_produccion",
                    Nombre = "Producción",
                    Items = new List<ItemProyecto>
                    {
                        new ProductoProyecto
                        {
                            Id = "producto_personaje",
                            Nombre = "Animación de personaje 2D",
                            Cantidad = 1m,
                            Unidad = "segundos",
                            Subproductos = new List<SubproductoProyecto> { subproducto }
                        }
                    }
                }
            }
        };
        PersonaEquipo ana = new PersonaEquipo
        {
            Id = "persona_ana",
            Nombre = "Ana Pérez",
            CargoPrincipal = "Animador 2D",
            PagoInterno = 1600000m,
            PeriodoPago = "Mensual",
            CostoHora = 10000m
        };
        CategoriaTrabajador cargo = new CategoriaTrabajador
        {
            Id = 301,
            Nombre = "Animador 2D",
            Nivel = "típico",
            SueldoMensualCLPTipico = 1800000
        };

        string html = InformeProyectoBuilder.GenerarHtml(
            proyecto,
            new[] { ana },
            new[] { cargo });

        Assert.Contains("Ana Pérez", html);
        Assert.Contains("Animador 2D (típico)", html);
        Assert.Contains("1.600.000 CLP", html);
        Assert.Contains("80.000 CLP", html);
        Assert.Contains("Animación de personaje 2D", html);
        Assert.Contains("Rough", html);
        Assert.Contains("Rough animation", html);
        Assert.Contains("8,00 h", html);
    }
}
