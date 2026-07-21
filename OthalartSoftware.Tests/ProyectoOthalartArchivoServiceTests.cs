using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace OthalartSoftware.Tests;

public class ProyectoOthalartArchivoServiceTests
{
    [Fact]
    public void Guardar_y_cargar_conserva_modificaciones_locales_del_proyecto()
    {
        string carpeta = CrearCarpetaTemporal();
        try
        {
            string ruta = Path.Combine(carpeta, "proyecto.othalart.json");
            ProyectoOthalartGuardado original = CrearProyecto("Proyecto A", 3.5m, "Animador 2D");

            ProyectoOthalartArchivoService.Guardar(ruta, original);
            ProyectoOthalartGuardado cargado = ProyectoOthalartArchivoService.Cargar(ruta);

            ProcesoProyecto proceso = cargado.ProyectoProductivo
                .Grupos.Single()
                .Items.Single()
                .Procesos.Single();

            Assert.Equal("Proyecto A", cargado.Cotizacion.NombreProyecto);
            Assert.Equal(3.5m, proceso.Resultado.HorasAsignadas);
            Assert.Equal(3.5m, proceso.Asignaciones.Single().HorasAsignadas);
            Assert.Equal("Animador 2D", proceso.Asignaciones.Single().CargoId);
            Assert.Equal(OrigenHorasProductivas.Manual, proceso.Asignaciones.Single().OrigenHoras);
            Assert.Equal("Trabajadora del proyecto", cargado.BibliotecaPersonal.Single().Nombre);

            string bibliotecas = Path.Combine(carpeta, "Bibliotecas");
            Assert.True(Directory.Exists(bibliotecas));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "etapas.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "subetapas.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "cargos.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "personal_empresa.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "productos2d.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "rendimientos_productivos.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "ecuaciones_productivas.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "gestiones_productivas.json")));
            Assert.True(File.Exists(Path.Combine(bibliotecas, "resoluciones_dependencias.json")));
        }
        finally
        {
            Directory.Delete(carpeta, true);
        }
    }

    [Fact]
    public void Dos_proyectos_se_guardan_en_archivos_independientes()
    {
        string carpeta = CrearCarpetaTemporal();
        try
        {
            string rutaA = Path.Combine(carpeta, "a.othalart.json");
            string rutaB = Path.Combine(carpeta, "b.othalart.json");

            ProyectoOthalartArchivoService.Guardar(rutaA, CrearProyecto("A", 3.5m, "Animador 2D"));
            ProyectoOthalartArchivoService.Guardar(rutaB, CrearProyecto("B", 8m, "Supervisor"));

            ProyectoOthalartGuardado proyectoA = ProyectoOthalartArchivoService.Cargar(rutaA);
            ProyectoOthalartGuardado proyectoB = ProyectoOthalartArchivoService.Cargar(rutaB);

            Assert.Equal("A", proyectoA.Cotizacion.NombreProyecto);
            Assert.Equal(3.5m, ObtenerProceso(proyectoA).Resultado.HorasAsignadas);
            Assert.Equal("B", proyectoB.Cotizacion.NombreProyecto);
            Assert.Equal(8m, ObtenerProceso(proyectoB).Resultado.HorasAsignadas);
        }
        finally
        {
            Directory.Delete(carpeta, true);
        }
    }

    [Fact]
    public void Listado_encuentra_todos_los_proyectos_guardados_en_sus_carpetas()
    {
        string carpeta = CrearCarpetaTemporal();
        try
        {
            string proyectoA = Path.Combine(carpeta, "Proyecto A");
            string proyectoB = Path.Combine(carpeta, "Proyecto B");
            Directory.CreateDirectory(proyectoA);
            Directory.CreateDirectory(proyectoB);

            string rutaA = Path.Combine(proyectoA, "Proyecto A.othalart.json");
            string rutaB = Path.Combine(proyectoB, "Proyecto B.othalart.json");
            ProyectoOthalartArchivoService.Guardar(
                rutaA,
                CrearProyecto("Proyecto A", 3m, "Animador"));
            ProyectoOthalartArchivoService.Guardar(
                rutaB,
                CrearProyecto("Proyecto B", 6m, "Supervisor"));

            List<string> encontrados =
                ProyectoOthalartArchivoService.ListarRutasProyectosGuardados(carpeta);

            Assert.Equal(2, encontrados.Count);
            Assert.Contains(Path.GetFullPath(rutaA), encontrados);
            Assert.Contains(Path.GetFullPath(rutaB), encontrados);
        }
        finally
        {
            Directory.Delete(carpeta, true);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-es-json")]
    public void Archivo_invalido_genera_error_controlado(string contenido)
    {
        Assert.ThrowsAny<Exception>(() =>
            ProyectoOthalartArchivoService.Deserializar(contenido));
    }

    private static ProyectoOthalartGuardado CrearProyecto(
        string nombre,
        decimal horas,
        string cargo)
    {
        ProcesoProyecto proceso = new ProcesoProyecto
        {
            Id = "proceso_1",
            Nombre = "Correcciones",
            MetodoCalculo = MetodoCalculoProceso.Manual,
            Resultado = new ResultadoProcesoProyecto
            {
                HorasCalculadas = horas,
                HorasAsignadas = horas,
                CostoCalculado = horas * 5000m
            },
            Asignaciones = new List<AsignacionProductiva>
            {
                new AsignacionProductiva
                {
                    Id = "asignacion_1",
                    ProcesoProyectoId = "proceso_1",
                    CargoId = cargo,
                    HorasCalculadas = horas,
                    HorasAsignadas = horas,
                    OrigenHoras = OrigenHorasProductivas.Manual,
                    CostoCalculado = horas * 5000m
                }
            }
        };

        ProyectoCotizacion proyecto = new ProyectoCotizacion
        {
            Id = "proyecto_" + nombre,
            Nombre = nombre,
            Grupos = new List<GrupoProyecto>
            {
                new GrupoProyecto
                {
                    Id = "grupo_1",
                    Nombre = "Producción",
                    Items = new List<ItemProyecto>
                    {
                        new ItemProyecto
                        {
                            Id = "item_1",
                            Nombre = "Animación",
                            Procesos = new List<ProcesoProyecto> { proceso },
                            Overrides = new List<OverrideProductivo>
                            {
                                new OverrideProductivo
                                {
                                    Campo = "Proceso.proceso_1.Horas",
                                    ValorJson = horas.ToString(
                                        System.Globalization.CultureInfo.InvariantCulture)
                                }
                            }
                        }
                    }
                }
            }
        };

        return new ProyectoOthalartGuardado
        {
            Cotizacion = new Cotizacion
            {
                NombreProyecto = nombre,
                ProyectoProductivo = proyecto
            },
            ProyectoProductivo = proyecto,
            BibliotecaPersonal = new List<PersonaEquipo>
            {
                new PersonaEquipo
                {
                    Id = "persona_proyecto",
                    Nombre = "Trabajadora del proyecto",
                    PagoInterno = 1500000m,
                    PeriodoPago = "Mensual"
                }
            }
        };
    }

    private static ProcesoProyecto ObtenerProceso(ProyectoOthalartGuardado proyecto)
    {
        return proyecto.ProyectoProductivo.Grupos.Single().Items.Single().Procesos.Single();
    }

    private static string CrearCarpetaTemporal()
    {
        string ruta = Path.Combine(
            Path.GetTempPath(),
            "OthalartTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ruta);
        return ruta;
    }
}
