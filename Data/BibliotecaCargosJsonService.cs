using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaCargosJsonService
    {
        private const string NombreArchivoCargos = "cargos.json";

        // =========================================================
        // RUTAS
        // =========================================================

        public static string ObtenerCarpetaBiblioteca()
        {
            string raizAplicacion = ObtenerRaizAplicacion();

            string carpeta = Path.Combine(
                raizAplicacion,
                "Bibliotecas",
                "default"
            );

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            return carpeta;
        }

        public static string ObtenerRutaCargos()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivoCargos);
        }

        public static string ObtenerRutaBibliotecaActual()
        {
            return ObtenerCarpetaBiblioteca();
        }

        private static string ObtenerRaizAplicacion()
        {
            /*
             * En desarrollo, AppContext.BaseDirectory suele apuntar a:
             * bin/Debug/.../
             *
             * Por eso buscamos hacia arriba hasta encontrar:
             * - un archivo .csproj
             * - o una carpeta Data
             *
             * En versión publicada, si no encuentra eso,
             * usa la carpeta real del ejecutable.
             */

            string baseDir = AppContext.BaseDirectory;
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

            DirectoryInfo actual = new DirectoryInfo(baseDir);

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

            return baseDir;
        }

        // =========================================================
        // CREACIÓN INICIAL
        // =========================================================

        public static void CrearCargosJsonSiNoExiste()
        {
            string ruta = ObtenerRutaCargos();

            if (File.Exists(ruta))
            {
                return;
            }

            List<CategoriaTrabajador> cargosSemilla = CargosSeed.CrearBibliotecaCompleta();
            GuardarCargos(cargosSemilla);
        }

        public static void RegenerarCargosDesdeSeed()
        {
            List<CategoriaTrabajador> cargosSemilla = CargosSeed.CrearBibliotecaCompleta();
            GuardarCargos(cargosSemilla);
        }

        // =========================================================
        // CARGA
        // =========================================================

        public static List<CategoriaTrabajador> CargarCargos()
        {
            CrearCargosJsonSiNoExiste();

            string ruta = ObtenerRutaCargos();

            try
            {
                string json = File.ReadAllText(ruta);

                List<CategoriaTrabajador> cargos = JsonSerializer.Deserialize<List<CategoriaTrabajador>>(
                    json,
                    CrearOpcionesJson()
                );

                cargos = cargos ?? new List<CategoriaTrabajador>();

                if (cargos.Count == 0)
                {
                    cargos = CargosSeed.CrearBibliotecaCompleta();
                    GuardarCargos(cargos);
                }

                bool corrigioEscala = NormalizarEscalaSueldos(cargos);
                bool corrigioTipoCargo = NormalizarTiposCargo(cargos);

                if (corrigioEscala || corrigioTipoCargo)
                {
                    GuardarCargos(cargos);
                }

                return cargos;
            }
            catch
            {
                /*
                 * Si el JSON está roto, no matamos la app.
                 * Usamos la semilla en memoria para que el software siga funcionando.
                 */
                return CargosSeed.CrearBibliotecaCompleta();
            }
        }

        public static List<CategoriaTrabajador> CargarCargosPorBloque(string bloque)
        {
            string b = Normalizar(bloque);

            return CargarCargos()
                .Where(c => Normalizar(c.Bloque) == b)
                .ToList();
        }

        // =========================================================
        // GUARDADO
        // =========================================================

        public static void GuardarCargos(List<CategoriaTrabajador> cargos)
        {
            string ruta = ObtenerRutaCargos();

            string carpeta = Path.GetDirectoryName(ruta);

            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string json = JsonSerializer.Serialize(
                PrepararCargosParaGuardar(cargos),
                CrearOpcionesJson()
            );

            File.WriteAllText(ruta, json);
        }

        private static List<CategoriaTrabajador> PrepararCargosParaGuardar(
            List<CategoriaTrabajador> cargos
        )
        {
            cargos = cargos ?? new List<CategoriaTrabajador>();
            NormalizarEscalaSueldos(cargos);
            NormalizarTiposCargo(cargos);
            return cargos;
        }

        private static bool NormalizarTiposCargo(List<CategoriaTrabajador> cargos)
        {
            bool cambio = false;

            foreach (CategoriaTrabajador cargo in cargos ?? new List<CategoriaTrabajador>())
            {
                if (cargo == null)
                {
                    continue;
                }

                string tipoNormalizado = ClasificacionCargosService.NormalizarTipoCargo(cargo.TipoCargo);
                if (string.IsNullOrWhiteSpace(tipoNormalizado))
                {
                    tipoNormalizado = ClasificacionCargosService.InferirTipoCargo(cargo.NombreCompleto);
                }

                if (cargo.TipoCargo != tipoNormalizado)
                {
                    cargo.TipoCargo = tipoNormalizado;
                    cambio = true;
                }
            }

            return cambio;
        }

        private static bool NormalizarEscalaSueldos(List<CategoriaTrabajador> cargos)
        {
            bool cambio = false;

            foreach (CategoriaTrabajador cargo in cargos ?? new List<CategoriaTrabajador>())
            {
                if (cargo == null)
                {
                    continue;
                }

                double clpMin = NormalizarMontoSueldo(cargo.SueldoMensualCLPMin);
                double clpTipico = NormalizarMontoSueldo(cargo.SueldoMensualCLPTipico);
                double clpMax = NormalizarMontoSueldo(cargo.SueldoMensualCLPMax);

                if (clpMin != cargo.SueldoMensualCLPMin ||
                    clpTipico != cargo.SueldoMensualCLPTipico ||
                    clpMax != cargo.SueldoMensualCLPMax)
                {
                    cargo.SueldoMensualCLPMin = clpMin;
                    cargo.SueldoMensualCLPTipico = clpTipico;
                    cargo.SueldoMensualCLPMax = clpMax;
                    cambio = true;
                }
            }

            return cambio;
        }

        private static double NormalizarMontoSueldo(double monto)
        {
            if (monto <= 0.0 || monto >= 10000.0)
            {
                return monto;
            }

            return monto * 1000.0;
        }

        private static bool BibliotecaEstaIncompleta(List<CategoriaTrabajador> cargos)
        {
            if (cargos == null || cargos.Count == 0)
            {
                return true;
            }

            HashSet<string> bloques = cargos
                .Where(c => c != null)
                .Select(c => Normalizar(c.Bloque))
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToHashSet();

            return
                !bloques.Contains("general") ||
                !bloques.Contains("desarrollo") ||
                !bloques.Contains("preproduccion") ||
                !bloques.Contains("produccion") ||
                !bloques.Contains("postproduccion");
        }

        private static List<CategoriaTrabajador> CompletarBibliotecaConSeed(
            List<CategoriaTrabajador> cargosActuales
        )
        {
            List<CategoriaTrabajador> cargosCompletos = CargosSeed.CrearBibliotecaCompleta();

            if (cargosActuales == null || cargosActuales.Count == 0)
            {
                return cargosCompletos;
            }

            Dictionary<int, CategoriaTrabajador> actualesPorId = cargosActuales
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .ToDictionary(g => g.Key, g => g.First());

            for (int i = 0; i < cargosCompletos.Count; i++)
            {
                CategoriaTrabajador cargoSemilla = cargosCompletos[i];

                if (cargoSemilla == null)
                {
                    continue;
                }

                if (actualesPorId.TryGetValue(cargoSemilla.Id, out CategoriaTrabajador cargoActual))
                {
                    if (string.IsNullOrWhiteSpace(cargoActual.Bloque))
                    {
                        cargoActual.Bloque = cargoSemilla.Bloque;
                    }

                    if (string.IsNullOrWhiteSpace(cargoActual.TipoCargo))
                    {
                        cargoActual.TipoCargo = cargoSemilla.TipoCargo;
                    }

                    cargosCompletos[i] = cargoActual;
                }
            }

            IEnumerable<CategoriaTrabajador> cargosPersonalizados = cargosActuales
                .Where(c => c != null)
                .Where(c => !cargosCompletos.Any(seed => seed != null && seed.Id == c.Id));

            cargosCompletos.AddRange(cargosPersonalizados);

            return cargosCompletos
                .Where(c => c != null)
                .OrderBy(c => c.Id)
                .ToList();
        }

        // =========================================================
        // BÚSQUEDA
        // =========================================================

        public static CategoriaTrabajador BuscarCargo(
            string nombre,
            string bloque,
            string nivel
        )
        {
            List<CategoriaTrabajador> cargos = CargarCargos();

            CategoriaTrabajador encontradoEnBloque = BuscarEnLista(cargos, nombre, bloque, nivel);

            if (encontradoEnBloque != null)
            {
                return encontradoEnBloque;
            }

            /*
             * Fallback:
             * Si no existe en ese bloque, busca global.
             */
            return BuscarEnLista(cargos, nombre, "", nivel);
        }

        private static CategoriaTrabajador BuscarEnLista(
            List<CategoriaTrabajador> cargos,
            string nombre,
            string bloque,
            string nivel
        )
        {
            string n = Normalizar(nombre);
            string b = Normalizar(bloque);
            string nv = Normalizar(nivel);

            if (string.IsNullOrWhiteSpace(n))
            {
                return null;
            }

            IEnumerable<CategoriaTrabajador> query = cargos ?? new List<CategoriaTrabajador>();

            if (!string.IsNullOrWhiteSpace(b))
            {
                query = query.Where(c => Normalizar(c.Bloque) == b);
            }

            // 1) Nombre exacto + nivel pedido.
            CategoriaTrabajador exactoNivel = query.FirstOrDefault(c =>
                Normalizar(c.Nombre) == n &&
                Normalizar(c.Nivel) == nv
            );

            if (exactoNivel != null)
            {
                return exactoNivel;
            }

            // 2) Nombre exacto + típico.
            CategoriaTrabajador exactoTipico = query.FirstOrDefault(c =>
                Normalizar(c.Nombre) == n &&
                Normalizar(c.Nivel) == "tipico"
            );

            if (exactoTipico != null)
            {
                return exactoTipico;
            }

            // 3) Nombre exacto cualquier nivel.
            CategoriaTrabajador exactoCualquiera = query.FirstOrDefault(c =>
                Normalizar(c.Nombre) == n
            );

            if (exactoCualquiera != null)
            {
                return exactoCualquiera;
            }

            // 4) Contiene nombre + nivel pedido.
            CategoriaTrabajador contieneNivel = query.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(n) &&
                Normalizar(c.Nivel) == nv
            );

            if (contieneNivel != null)
            {
                return contieneNivel;
            }

            // 5) Contiene nombre + típico.
            CategoriaTrabajador contieneTipico = query.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(n) &&
                Normalizar(c.Nivel) == "tipico"
            );

            if (contieneTipico != null)
            {
                return contieneTipico;
            }

            // 6) Contiene nombre cualquier nivel.
            return query.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(n)
            );
        }

        // =========================================================
        // VALIDACIÓN SIMPLE
        // =========================================================

        public static bool ExisteArchivoCargos()
        {
            return File.Exists(ObtenerRutaCargos());
        }

        public static bool JsonCargosEsValido()
        {
            try
            {
                string ruta = ObtenerRutaCargos();

                if (!File.Exists(ruta))
                {
                    return false;
                }

                string json = File.ReadAllText(ruta);

                List<CategoriaTrabajador> cargos = JsonSerializer.Deserialize<List<CategoriaTrabajador>>(
                    json,
                    CrearOpcionesJson()
                );

                return cargos != null;
            }
            catch
            {
                return false;
            }
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
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
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "");
        }
    }
}
