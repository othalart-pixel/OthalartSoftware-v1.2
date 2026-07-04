using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaEtapasJsonService
    {
        private const string NombreArchivoEtapas = "etapas.json";

        public static string ObtenerRutaEtapas()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivoEtapas);
        }

        public static List<EtapaDefinicion> CargarEtapas()
        {
            try
            {
                AsegurarArchivo();

                string json = File.ReadAllText(ObtenerRutaEtapas());
                List<EtapaDefinicion> etapas =
                    JsonSerializer.Deserialize<List<EtapaDefinicion>>(
                        json,
                        CrearOpcionesJson()
                    ) ?? CrearEtapasBase();

                NormalizarEtapas(etapas);
                return etapas;
            }
            catch
            {
                return CrearEtapasBase();
            }
        }

        public static void GuardarEtapas(List<EtapaDefinicion> etapas)
        {
            if (etapas == null)
            {
                etapas = CrearEtapasBase();
            }

            NormalizarEtapas(etapas);

            string ruta = ObtenerRutaEtapas();
            string directorio = Path.GetDirectoryName(ruta) ?? "";

            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            string json = JsonSerializer.Serialize(etapas, CrearOpcionesJson());
            File.WriteAllText(ruta, json);
        }

        public static void RegenerarEtapasBase()
        {
            GuardarEtapas(CrearEtapasBase());
        }

        public static List<EtapaDefinicion> CrearEtapasBase()
        {
            return new List<EtapaDefinicion>
            {
                new EtapaDefinicion
                {
                    Clave = "Desarrollo",
                    Nombre = "Desarrollo",
                    Orden = 10,
                    Activa = true,
                    ColorArgb = unchecked((int)0xFF4CAF50)
                },
                new EtapaDefinicion
                {
                    Clave = "Preproduccion",
                    Nombre = "Preproducción",
                    Orden = 20,
                    Activa = true,
                    ColorArgb = unchecked((int)0xFFFFC107)
                },
                new EtapaDefinicion
                {
                    Clave = "Produccion",
                    Nombre = "Producción",
                    Orden = 30,
                    Activa = true,
                    ColorArgb = unchecked((int)0xFFF44336)
                },
                new EtapaDefinicion
                {
                    Clave = "Postproduccion",
                    Nombre = "Postproducción",
                    Orden = 40,
                    Activa = true,
                    ColorArgb = unchecked((int)0xFF2196F3)
                }
            };
        }

        private static void AsegurarArchivo()
        {
            if (File.Exists(ObtenerRutaEtapas()))
            {
                return;
            }

            GuardarEtapas(CrearEtapasBase());
        }

        private static void NormalizarEtapas(List<EtapaDefinicion> etapas)
        {
            int ordenFallback = 10;

            foreach (EtapaDefinicion etapa in etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(etapa.Clave))
                {
                    etapa.Clave = etapa.Nombre;
                }

                if (string.IsNullOrWhiteSpace(etapa.Nombre))
                {
                    etapa.Nombre = etapa.Clave;
                }

                if (etapa.Orden <= 0)
                {
                    etapa.Orden = ordenFallback;
                }

                if (etapa.ColorArgb == 0)
                {
                    etapa.ColorArgb = unchecked((int)0xFFB4B4B4);
                }

                ordenFallback += 10;
            }
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
