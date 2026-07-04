using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaSubEtapasJsonService
    {
        private const string NombreArchivoSubEtapas = "subetapas.json";

        public static string ObtenerRutaSubEtapas()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivoSubEtapas);
        }

        public static void CrearSubEtapasJsonSiNoExiste()
        {
            string ruta = ObtenerRutaSubEtapas();

            if (File.Exists(ruta))
            {
                return;
            }

            GuardarSubEtapas(BibliotecaSubEtapasService.CrearBibliotecaBase());
        }

        public static void RegenerarSubEtapasDesdeBase()
        {
            GuardarSubEtapas(BibliotecaSubEtapasService.CrearBibliotecaBase());
        }

        public static List<SubEtapaProyecto> CargarSubEtapas()
        {
            CrearSubEtapasJsonSiNoExiste();

            try
            {
                string json = File.ReadAllText(ObtenerRutaSubEtapas());

                List<SubEtapaProyecto> subEtapas =
                    JsonSerializer.Deserialize<List<SubEtapaProyecto>>(
                        json,
                        CrearOpcionesJson()
                    );

                subEtapas = subEtapas ?? BibliotecaSubEtapasService.CrearBibliotecaBase();

                if (subEtapas.Count == 0)
                {
                    subEtapas = BibliotecaSubEtapasService.CrearBibliotecaBase();
                }
                else
                {
                    subEtapas = NormalizarSubEtapasEditables(subEtapas);
                }

                GuardarSubEtapas(subEtapas);

                return subEtapas;
            }
            catch
            {
                return BibliotecaSubEtapasService.CrearBibliotecaBase();
            }
        }

        private static List<SubEtapaProyecto> NormalizarSubEtapasEditables(
            List<SubEtapaProyecto> actuales
        )
        {
            actuales = actuales ?? new List<SubEtapaProyecto>();
            int siguienteId = actuales
                .Select(s => s == null ? 0 : s.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (SubEtapaProyecto subEtapa in actuales.Where(s => s != null))
            {
                if (subEtapa.Id <= 0)
                {
                    subEtapa.Id = siguienteId++;
                }
            }

            return actuales
                .Where(s => s != null)
                .OrderBy(s => ObtenerOrdenEtapa(s.EtapaPadre))
                .ThenBy(s => s.Orden)
                .ThenBy(s => s.Nombre)
                .ToList();
        }

        private static int ObtenerOrdenEtapa(string etapa)
        {
            string normalizada = Normalizar(etapa);

            if (normalizada.Contains("desarrollo")) return 10;
            if (normalizada.Contains("preproduccion")) return 20;
            if (normalizada.Contains("produccion")) return 30;
            if (normalizada.Contains("postproduccion")) return 40;

            return 90;
        }

        private static string Normalizar(string texto)
        {
            return (texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("/", "")
                .Replace(".", "")
                .Replace(",", "");
        }

        public static void GuardarSubEtapas(List<SubEtapaProyecto> subEtapas)
        {
            string ruta = ObtenerRutaSubEtapas();
            string carpeta = Path.GetDirectoryName(ruta);

            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string json = JsonSerializer.Serialize(
                subEtapas ?? new List<SubEtapaProyecto>(),
                CrearOpcionesJson()
            );

            File.WriteAllText(ruta, json);
        }

        private static string ObtenerCarpetaBiblioteca()
        {
            string carpeta = Path.Combine(ObtenerRaizAplicacion(), "Bibliotecas", "default");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            return carpeta;
        }

        private static string ObtenerRaizAplicacion()
        {
            string raizConfigurada = Environment.GetEnvironmentVariable("OTHALART_PROJECT_ROOT") ?? "";
            if (!string.IsNullOrWhiteSpace(raizConfigurada) &&
                Directory.Exists(Path.Combine(raizConfigurada, "Bibliotecas")))
            {
                return raizConfigurada;
            }

            string directorioTrabajo = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(directorioTrabajo) &&
                Directory.Exists(Path.Combine(directorioTrabajo, "Bibliotecas")))
            {
                return directorioTrabajo;
            }

            DirectoryInfo actual = new DirectoryInfo(AppContext.BaseDirectory);

            while (actual != null)
            {
                bool tieneCsproj = actual.GetFiles("*.csproj").Length > 0;
                bool tieneCarpetaData = Directory.Exists(Path.Combine(actual.FullName, "Data"));

                if (tieneCsproj || tieneCarpetaData)
                {
                    return actual.FullName;
                }

                actual = actual.Parent;
            }

            return AppContext.BaseDirectory;
        }

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }
    }
}
