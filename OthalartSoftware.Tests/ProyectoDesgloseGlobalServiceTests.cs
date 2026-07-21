using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace OthalartSoftware.Tests;

public class ProyectoDesgloseGlobalServiceTests
{
    [Fact]
    public void Construir_elimina_filas_snapshot_exactamente_duplicadas_pero_conserva_cargos_distintos()
    {
        RequerimientoProduccionInterna baseReq = new RequerimientoProduccionInterna
        {
            ProcesoId = "proc_color",
            EntregableCliente = "Color",
            NombreRequerimiento = "Color / pintura base",
            TipoInterno = "proc_color",
            CargoId = "Colorista",
            Cantidad = 1,
            HorasEstandar = 2.8,
            CostoEstandarCLP = 12652
        };
        Cotizacion snapshot = new Cotizacion
        {
            DesgloseProductivo = new DesgloseProductivoProyecto
            {
                Requerimientos = new List<RequerimientoProduccionInterna>
                {
                    baseReq,
                    new RequerimientoProduccionInterna
                    {
                        ProcesoId = "proc_color",
                        EntregableCliente = "Color",
                        NombreRequerimiento = "Color / pintura base",
                        TipoInterno = "proc_color",
                        CargoId = "Colorista",
                        Cantidad = 1,
                        HorasEstandar = 2.8,
                        CostoEstandarCLP = 12652
                    },
                    new RequerimientoProduccionInterna
                    {
                        ProcesoId = "proc_color",
                        EntregableCliente = "Color",
                        NombreRequerimiento = "Color / pintura base",
                        TipoInterno = "proc_color",
                        CargoId = "Supervisor",
                        Cantidad = 1,
                        HorasEstandar = 2.8,
                        CostoEstandarCLP = 12652
                    }
                }
            }
        };
        ItemProyecto item = new ProductoProyecto
        {
            Id = "item_color",
            Nombre = "Animación 2D",
            Subproductos = new List<SubproductoProyecto>
            {
                new SubproductoProyecto
                {
                    Id = "sub_color",
                    Nombre = "Color",
                    SubproductoBibliotecaId = "proc_color"
                }
            }
        };
        CotizacionItemProyectoAdapterService.CapturarCotizacionEnItem(item, snapshot);
        ProyectoCotizacion proyecto = new ProyectoCotizacion
        {
            Id = "proyecto",
            Grupos = new List<GrupoProyecto>
            {
                new GrupoProyecto
                {
                    Id = "produccion",
                    Items = new List<ItemProyecto> { item }
                }
            }
        };

        DesgloseProductivoProyecto desglose =
            ProyectoDesgloseGlobalService.Construir(proyecto, new Cotizacion());

        Assert.Equal(2, desglose.Requerimientos.Count);
        Assert.Contains(desglose.Requerimientos, r => r.CargoId == "Colorista");
        Assert.Contains(desglose.Requerimientos, r => r.CargoId == "Supervisor");
    }

    [Fact]
    public void Construir_reune_productos_y_aplicar_devuelve_cambios_al_proceso_correcto()
    {
        ProcesoProyecto procesoA = CrearProceso("proc_a", "Rough", 4m);
        ProcesoProyecto procesoB = CrearProceso("proc_b", "Clean up", 7m);
        ItemProyecto itemA = CrearItem("item_a", "Producto A", procesoA);
        ItemProyecto itemB = CrearItem("item_b", "Producto B", procesoB);
        ProyectoCotizacion proyecto = new ProyectoCotizacion
        {
            Id = "proyecto",
            Grupos = new List<GrupoProyecto>
            {
                new GrupoProyecto
                {
                    Id = "produccion",
                    Nombre = "Producción",
                    Items = new List<ItemProyecto> { itemA, itemB }
                }
            }
        };
        Cotizacion contexto = new Cotizacion { DiasHabilesEstudioPorSemana = 5 };

        DesgloseProductivoProyecto desglose =
            ProyectoDesgloseGlobalService.Construir(proyecto, contexto);

        Assert.Equal(2, desglose.Requerimientos.Count);
        Assert.Contains(desglose.Requerimientos, r => r.ItemId == "item_a");
        Assert.Contains(desglose.Requerimientos, r => r.ItemId == "item_b");

        RequerimientoProduccionInterna filaA =
            desglose.Requerimientos.Single(r => r.ProcesoId == "proc_a");
        filaA.HorasEstandar = 2.5;
        filaA.Cantidad = 3;
        filaA.RendimientoCantidad = 6;
        filaA.RendimientoPeriodo = "dia";

        ProyectoDesgloseGlobalService.Aplicar(proyecto, desglose, contexto);

        Assert.Equal(2.5m, procesoA.Resultado.HorasAsignadas);
        Assert.Equal(3m, procesoA.Cantidad);
        Assert.Equal(6m, procesoA.Capacidad);
        Assert.Equal("dia", procesoA.Periodo);
        Assert.Equal(7m, procesoB.Resultado.HorasAsignadas);
    }

    private static ItemProyecto CrearItem(
        string id,
        string nombre,
        ProcesoProyecto proceso)
    {
        return new ProductoProyecto
        {
            Id = id,
            Nombre = nombre,
            Cantidad = 1m,
            Unidad = "segundos",
            Procesos = new List<ProcesoProyecto> { proceso }
        };
    }

    private static ProcesoProyecto CrearProceso(
        string id,
        string nombre,
        decimal horas)
    {
        return new ProcesoProyecto
        {
            Id = id,
            Nombre = nombre,
            Cantidad = 1m,
            Unidad = "segundos",
            MetodoCalculo = MetodoCalculoProceso.Manual,
            Resultado = new ResultadoProcesoProyecto
            {
                HorasCalculadas = horas,
                HorasAsignadas = horas,
                CostoCalculado = horas * 1000m
            }
        };
    }
}
