using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cotizador_animacion_Othalart.Data
{
    public sealed class ProyectoReciente
    {
        public string Ruta { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Cliente { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }

    public static class ProyectosRecientesService
    {
        private const int MaxRecientes = 12;

        public static List<ProyectoReciente> Cargar()
        {
            try
            {
                string ruta = ObtenerRutaIndice();
                if (!File.Exists(ruta))
                {
                    return new List<ProyectoReciente>();
                }

                List<ProyectoReciente> recientes =
                    JsonSerializer.Deserialize<List<ProyectoReciente>>(File.ReadAllText(ruta)) ??
                    new List<ProyectoReciente>();

                // Normalización de rutas para evitar duplicidad entre versiones (v1.2 y v1.3)
                string currentProyectosFolder = Cotizador_animacion_Othalart.Services.ProyectoOthalartArchivoService.ObtenerCarpetaProyectosPrograma();
                List<ProyectoReciente> listadoNormalizado = new List<ProyectoReciente>();
                HashSet<string> rutasUnicas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var p in recientes)
                {
                    if (p == null || string.IsNullOrWhiteSpace(p.Ruta))
                    {
                        continue;
                    }

                    string pathOriginal = p.Ruta;
                    string pathNormalizado = pathOriginal;

                    // Buscar si la ruta contiene la carpeta de proyectos de una versión anterior o alterna
                    string token = "\\Proyectos\\";
                    int idx = pathOriginal.LastIndexOf(token, StringComparison.OrdinalIgnoreCase);
                    if (idx == -1)
                    {
                        token = "/Proyectos/";
                        idx = pathOriginal.LastIndexOf(token, StringComparison.OrdinalIgnoreCase);
                    }

                    if (idx != -1)
                    {
                        string relativePath = pathOriginal.Substring(idx + token.Length);
                        string pathEnVersionActual = Path.Combine(currentProyectosFolder, relativePath);
                        if (File.Exists(pathEnVersionActual))
                        {
                            pathNormalizado = pathEnVersionActual;
                        }
                    }

                    if (File.Exists(pathNormalizado))
                    {
                        p.Ruta = pathNormalizado;

                        // Si la ruta cambió a la de la versión actual, actualizamos el nombre y cliente
                        // leyendo directamente desde el archivo para corregir nombres huérfanos/desactualizados.
                        if (!string.Equals(pathOriginal, pathNormalizado, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                string jsonContenido = File.ReadAllText(pathNormalizado);
                                using (JsonDocument doc = JsonDocument.Parse(jsonContenido))
                                {
                                    if (doc.RootElement.TryGetProperty("Cotizacion", out JsonElement cotElement))
                                    {
                                        if (cotElement.TryGetProperty("NombreProyecto", out JsonElement nameElement))
                                        {
                                            string nombreReal = nameElement.GetString();
                                            if (!string.IsNullOrWhiteSpace(nombreReal))
                                            {
                                                p.Nombre = nombreReal;
                                            }
                                        }
                                        if (cotElement.TryGetProperty("NombreCliente", out JsonElement clientElement))
                                        {
                                            p.Cliente = clientElement.GetString() ?? "";
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Silencioso si no se puede leer el archivo.
                            }
                        }

                        string pathFullPath = Path.GetFullPath(pathNormalizado);
                        if (rutasUnicas.Add(pathFullPath))
                        {
                            listadoNormalizado.Add(p);
                        }
                    }
                }

                // Guardar la lista normalizada de vuelta para limpiar el recent-projects.json
                bool huboCambios = listadoNormalizado.Count != recientes.Count;
                if (!huboCambios)
                {
                    for (int i = 0; i < listadoNormalizado.Count; i++)
                    {
                        if (!string.Equals(listadoNormalizado[i].Ruta, recientes[i].Ruta, StringComparison.OrdinalIgnoreCase))
                        {
                            huboCambios = true;
                            break;
                        }
                    }
                }

                if (huboCambios)
                {
                    Guardar(listadoNormalizado);
                }

                return listadoNormalizado
                    .OrderByDescending(p => p.Fecha)
                    .Take(MaxRecientes)
                    .ToList();
            }
            catch
            {
                return new List<ProyectoReciente>();
            }
        }

        public static void Registrar(string ruta, string nombre, string cliente)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                return;
            }

            List<ProyectoReciente> recientes = Cargar();
            recientes.RemoveAll(p => string.Equals(p.Ruta, ruta, StringComparison.OrdinalIgnoreCase));
            recientes.Insert(0, new ProyectoReciente
            {
                Ruta = ruta,
                Nombre = string.IsNullOrWhiteSpace(nombre) ? Path.GetFileNameWithoutExtension(ruta) : nombre,
                Cliente = cliente ?? "",
                Fecha = DateTime.Now
            });

            Guardar(recientes.Take(MaxRecientes).ToList());
        }

        private static void Guardar(List<ProyectoReciente> recientes)
        {
            string ruta = ObtenerRutaIndice();
            string carpeta = Path.GetDirectoryName(ruta) ?? "";
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            File.WriteAllText(
                ruta,
                JsonSerializer.Serialize(recientes, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        private static string ObtenerRutaIndice()
        {
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(baseDir, "Othalart", "recent-projects.json");
        }
    }
}
