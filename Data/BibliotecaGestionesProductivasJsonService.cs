using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaGestionesProductivasJsonService
    {
        private const string NombreArchivo = "gestiones_productivas.json";

        public static string ObtenerRutaGestiones()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivo);
        }

        public static List<GestionProductivaRegla> CargarGestiones()
        {
            CrearJsonSiNoExiste();

            try
            {
                string json = File.ReadAllText(ObtenerRutaGestiones());

                List<GestionProductivaRegla> gestiones =
                    JsonSerializer.Deserialize<List<GestionProductivaRegla>>(
                        json,
                        CrearOpcionesJson()
                    ) ?? CrearBase();

                if (gestiones.Count == 0)
                {
                    gestiones = CrearBase();
                }

                return NormalizarLista(gestiones);
            }
            catch
            {
                return CrearBase();
            }
        }

        public static void GuardarGestiones(List<GestionProductivaRegla> gestiones)
        {
            string ruta = ObtenerRutaGestiones();
            string carpeta = Path.GetDirectoryName(ruta);

            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string json = JsonSerializer.Serialize(
                NormalizarLista(gestiones),
                CrearOpcionesJson()
            );

            File.WriteAllText(ruta, json);
        }

        public static void RegenerarDesdeBase()
        {
            GuardarGestiones(CrearBase());
        }

        private static void CrearJsonSiNoExiste()
        {
            if (!File.Exists(ObtenerRutaGestiones()))
            {
                GuardarGestiones(CrearBase());
            }
        }

        private static List<GestionProductivaRegla> NormalizarLista(
            List<GestionProductivaRegla> gestiones
        )
        {
            gestiones = gestiones ?? new List<GestionProductivaRegla>();

            int siguienteId = gestiones
                .Where(g => g != null)
                .Select(g => g.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (GestionProductivaRegla gestion in gestiones.Where(g => g != null))
            {
                if (gestion.Id <= 0)
                {
                    gestion.Id = siguienteId++;
                }

                if (string.IsNullOrWhiteSpace(gestion.NivelCargo))
                {
                    gestion.NivelCargo = "típico";
                }

                if (string.IsNullOrWhiteSpace(gestion.EtapaReferencia))
                {
                    gestion.EtapaReferencia = "General";
                }

                if (gestion.MinutosPorDiaPersona <= 0.0)
                {
                    gestion.MinutosPorDiaPersona = 10.0;
                }
            }

            return gestiones
                .Where(g => g != null)
                .OrderBy(g => g.Area)
                .ThenBy(g => g.Cargo)
                .ToList();
        }

        private static List<GestionProductivaRegla> CrearBase()
        {
            return new List<GestionProductivaRegla>
            {
                Crear(
                    1,
                    "Coordinacion general",
                    "Productor / Project manager",
                    "típico",
                    "General",
                    "*",
                    "Seguimiento general del proyecto sobre todo el esfuerzo productivo."
                ),
                Crear(
                    2,
                    "Direccion de animacion",
                    "Director/a de animación",
                    "típico",
                    "Produccion",
                    "animacion; rough; acting; clean; composicion; layout",
                    "Revision de animacion, acting, rough, clean up y composicion."
                ),
                Crear(
                    3,
                    "Direccion de arte",
                    "Director de arte",
                    "típico",
                    "Preproduccion",
                    "personaje; fondo; background; prop; objeto; color; arte; visual; estilo; concept",
                    "Revision visual de personajes, fondos, props, estilo y color."
                ),
                Crear(
                    4,
                    "Direccion narrativa",
                    "Director narrativo",
                    "típico",
                    "Desarrollo",
                    "guion; narrativa; escena; storyboard; animatic",
                    "Revision narrativa de guion, escena, storyboard y animatic."
                ),
                Crear(
                    5,
                    "Direccion tecnica",
                    "Director técnico 2D",
                    "típico",
                    "Postproduccion",
                    "export; render; control; entrega; formato; tecnico",
                    "Revision tecnica de export, render, control, entrega y formato."
                )
            };
        }

        private static GestionProductivaRegla Crear(
            int id,
            string area,
            string cargo,
            string nivel,
            string etapa,
            string tokens,
            string descripcion
        )
        {
            return new GestionProductivaRegla
            {
                Id = id,
                Activo = true,
                Area = area,
                Cargo = cargo,
                NivelCargo = nivel,
                EtapaReferencia = etapa,
                TokensAsociados = tokens,
                MinutosPorDiaPersona = 10.0,
                Descripcion = descripcion
            };
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
                bool tieneData = Directory.Exists(Path.Combine(actual.FullName, "Data"));

                if (tieneCsproj || tieneData)
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
