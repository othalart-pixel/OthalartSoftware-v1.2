using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoOthalartArchivoService
    {
        private const string NombreCarpetaProyectos = "Proyectos";
        private const string NombreCarpetaBibliotecas = "Bibliotecas";

        public static string Serializar(ProyectoOthalartGuardado proyecto)
        {
            if (proyecto == null)
            {
                throw new ArgumentNullException(nameof(proyecto));
            }

            return JsonSerializer.Serialize(proyecto, CrearOpciones());
        }

        public static ProyectoOthalartGuardado Deserializar(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("El archivo de proyecto está vacío.");
            }

            ProyectoOthalartGuardado proyecto =
                JsonSerializer.Deserialize<ProyectoOthalartGuardado>(json, CrearOpciones());

            if (proyecto == null || proyecto.Cotizacion == null)
            {
                throw new InvalidDataException("El archivo no contiene un proyecto Othalart válido.");
            }

            return proyecto;
        }

        public static void Guardar(string ruta, ProyectoOthalartGuardado proyecto)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                throw new ArgumentException("La ruta de guardado no puede estar vacía.", nameof(ruta));
            }

            string rutaCompleta = Path.GetFullPath(ruta);
            string carpeta = Path.GetDirectoryName(rutaCompleta) ?? "";
            if (!string.IsNullOrWhiteSpace(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            File.WriteAllText(rutaCompleta, Serializar(proyecto));
            GuardarBibliotecasDelProyecto(carpeta, proyecto);
        }

        public static ProyectoOthalartGuardado Cargar(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                throw new ArgumentException("La ruta de carga no puede estar vacía.", nameof(ruta));
            }

            return Deserializar(File.ReadAllText(Path.GetFullPath(ruta)));
        }

        public static string ObtenerCarpetaProyectosPrograma()
        {
            string raiz = ObtenerRaizPrograma();
            string carpeta = Path.Combine(raiz, NombreCarpetaProyectos);
            try
            {
                Directory.CreateDirectory(carpeta);
                return carpeta;
            }
            catch
            {
                string alternativa = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Othalart",
                    NombreCarpetaProyectos
                );
                Directory.CreateDirectory(alternativa);
                return alternativa;
            }
        }

        public static string CrearRutaProyectoNuevo(string nombreProyecto)
        {
            string nombreSeguro = LimpiarNombreArchivo(nombreProyecto);
            if (string.IsNullOrWhiteSpace(nombreSeguro))
            {
                nombreSeguro = "Proyecto Othalart";
            }

            string raizProyectos = ObtenerCarpetaProyectosPrograma();
            string carpeta = Path.Combine(raizProyectos, nombreSeguro);
            int sufijo = 2;
            while (Directory.Exists(carpeta) &&
                   Directory.EnumerateFileSystemEntries(carpeta).Any())
            {
                carpeta = Path.Combine(raizProyectos, nombreSeguro + " " + sufijo);
                sufijo++;
            }

            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, nombreSeguro + ".othalart.json");
        }

        public static List<string> ListarRutasProyectosGuardados(
            string carpetaProyectos = null)
        {
            string carpeta = string.IsNullOrWhiteSpace(carpetaProyectos)
                ? ObtenerCarpetaProyectosPrograma()
                : Path.GetFullPath(carpetaProyectos);

            if (!Directory.Exists(carpeta))
            {
                return new List<string>();
            }

            try
            {
                return Directory
                    .EnumerateFiles(
                        carpeta,
                        "*.othalart.json",
                        SearchOption.AllDirectories)
                    .Where(ruta => !EstaDentroDeCarpetaBibliotecas(ruta))
                    .OrderByDescending(File.GetLastWriteTime)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static bool EstaDentroDeCarpetaBibliotecas(string ruta)
        {
            DirectoryInfo carpeta = new FileInfo(ruta).Directory;
            while (carpeta != null)
            {
                if (string.Equals(
                    carpeta.Name,
                    NombreCarpetaBibliotecas,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                carpeta = carpeta.Parent;
            }

            return false;
        }

        private static void GuardarBibliotecasDelProyecto(
            string carpetaProyecto,
            ProyectoOthalartGuardado proyecto)
        {
            if (string.IsNullOrWhiteSpace(carpetaProyecto) || proyecto == null)
            {
                return;
            }

            string carpeta = Path.Combine(carpetaProyecto, NombreCarpetaBibliotecas);
            Directory.CreateDirectory(carpeta);
            JsonSerializerOptions opciones = CrearOpciones();

            GuardarJson(
                Path.Combine(carpeta, "etapas.json"),
                proyecto.BibliotecaEtapas,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "subetapas.json"),
                proyecto.BibliotecaSubEtapas,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "cargos.json"),
                proyecto.BibliotecaCargos,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "personal_empresa.json"),
                proyecto.BibliotecaPersonal,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "productos2d.json"),
                proyecto.BibliotecaProductos2D,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "rendimientos_productivos.json"),
                proyecto.BibliotecaRendimientosProductivos,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "ecuaciones_productivas.json"),
                proyecto.BibliotecaEcuacionesProductivas,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "gestiones_productivas.json"),
                proyecto.BibliotecaGestionesProductivas,
                opciones
            );
            GuardarJson(
                Path.Combine(carpeta, "resoluciones_dependencias.json"),
                proyecto.ResolucionesDependenciasSubEtapas,
                opciones
            );
        }

        private static void GuardarJson<T>(
            string ruta,
            T contenido,
            JsonSerializerOptions opciones)
        {
            File.WriteAllText(ruta, JsonSerializer.Serialize(contenido, opciones));
        }

        private static string ObtenerRaizPrograma()
        {
            string configurada =
                Environment.GetEnvironmentVariable("OTHALART_PROJECT_ROOT") ?? "";
            if (!string.IsNullOrWhiteSpace(configurada))
            {
                return Path.GetFullPath(configurada);
            }

            string trabajo = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(trabajo) &&
                Directory.Exists(Path.Combine(trabajo, "Bibliotecas")))
            {
                return trabajo;
            }

            DirectoryInfo actual = new DirectoryInfo(AppContext.BaseDirectory);
            while (actual != null)
            {
                bool tieneProyecto = actual.GetFiles("*.csproj").Length > 0;
                bool tieneBibliotecas =
                    Directory.Exists(Path.Combine(actual.FullName, "Bibliotecas"));
                if (tieneProyecto || tieneBibliotecas)
                {
                    return actual.FullName;
                }

                actual = actual.Parent;
            }

            return AppContext.BaseDirectory;
        }

        private static string LimpiarNombreArchivo(string nombre)
        {
            string limpio = (nombre ?? "").Trim();
            foreach (char invalido in Path.GetInvalidFileNameChars())
            {
                limpio = limpio.Replace(invalido, '-');
            }

            return limpio.Trim().TrimEnd('.');
        }

        public static JsonSerializerOptions CrearOpciones()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
    }
}
