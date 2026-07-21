using System.Xml.Linq;
using ClosedXML.Excel;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Reports;

namespace OthalartSoftware.Tests;

public class ExcelInternoExporterTests
{
    [Fact]
    public void GuardarExcelXlsxEnRuta_crea_un_libro_xlsx_real()
    {
        string carpeta = Path.Combine(Path.GetTempPath(), "Othalart Excel Tests",
            Guid.NewGuid().ToString("N"));
        string ruta = Path.Combine(carpeta, "Informe real.xlsx");
        Directory.CreateDirectory(carpeta);
        try
        {
            ExcelInternoExporter.GuardarExcelXlsxEnRuta(
                ruta,
                new Cotizacion { NombreProyecto = "Proyecto XLSX" },
                new ProyectoCotizacion { Nombre = "Proyecto XLSX" });

            Assert.True(File.Exists(ruta));
            Assert.True(new FileInfo(ruta).Length > 1000);
            using XLWorkbook libro = new XLWorkbook(ruta);
            Assert.True(libro.TryGetWorksheet("Resumen", out _));
            Assert.True(libro.TryGetWorksheet("Proyecto global", out _));
            Assert.True(libro.TryGetWorksheet("Desglose productivo", out _));
        }
        finally
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }
    }

    [Fact]
    public void GenerarExcelXml_incluye_desglose_trabajadores_cargos_y_costos_globales()
    {
        ProcesoProyecto proceso = new ProcesoProyecto
        {
            Id = "proc_rough",
            Nombre = "Rough animation",
            EtapaId = "produccion",
            Asignaciones = new List<AsignacionProductiva>
            {
                new AsignacionProductiva
                {
                    ProcesoProyectoId = "proc_rough",
                    CargoId = "Animador 2D",
                    PersonaId = "persona_ana",
                    HorasCalculadas = 8m,
                    HorasAsignadas = 8m,
                    CostoCalculado = 80000m
                }
            }
        };
        ProyectoCotizacion proyecto = new ProyectoCotizacion
        {
            Nombre = "Cortometraje",
            Cliente = "Cliente",
            Grupos = new List<GrupoProyecto>
            {
                new GrupoProyecto
                {
                    Id = "produccion",
                    Nombre = "Producción",
                    Items = new List<ItemProyecto>
                    {
                        new ProductoProyecto
                        {
                            Id = "producto",
                            Nombre = "Animación de personaje",
                            Procesos = new List<ProcesoProyecto> { proceso }
                        }
                    }
                }
            }
        };

        string xml = ExcelInternoExporter.GenerarExcelXml(
            new Cotizacion { NombreProyecto = "Cortometraje" },
            proyecto,
            new[]
            {
                new PersonaEquipo
                {
                    Id = "persona_ana",
                    Nombre = "Ana Pérez",
                    PagoInterno = 1600000m,
                    PeriodoPago = "Mensual",
                    CostoHora = 10000m
                }
            },
            new[]
            {
                new CategoriaTrabajador
                {
                    Id = 301,
                    Nombre = "Animador 2D",
                    Nivel = "típico",
                    SueldoMensualCLPTipico = 1800000
                }
            });

        XDocument.Parse(xml);
        Assert.Contains("ss:Name=\"Proyecto global\"", xml);
        Assert.Contains("ss:Name=\"Desglose productivo\"", xml);
        Assert.Contains("ss:Name=\"Trabajadores\"", xml);
        Assert.Contains("ss:Name=\"Cargos\"", xml);
        Assert.Contains("Ana Pérez", xml);
        Assert.Contains("Rough animation", xml);
        Assert.Contains("Animador 2D (típico)", xml);
        Assert.Contains(">80000.00<", xml);
    }
}
