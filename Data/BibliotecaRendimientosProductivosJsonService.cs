using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaRendimientosProductivosJsonService
    {
        private const string NombreArchivo = "rendimientos_productivos.json";

        public static string ObtenerRutaRendimientos()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivo);
        }

        public static List<RendimientoProductivo> CargarRendimientos()
        {
            CrearJsonSiNoExiste();

            try
            {
                string json = File.ReadAllText(ObtenerRutaRendimientos());

                List<RendimientoProductivo> rendimientos =
                    JsonSerializer.Deserialize<List<RendimientoProductivo>>(
                        json,
                        CrearOpcionesJson()
                    ) ?? CrearBase();

                if (rendimientos.Count == 0)
                {
                    rendimientos = CrearBase();
                }

                return NormalizarLista(rendimientos);
            }
            catch
            {
                return CrearBase();
            }
        }

        public static void GuardarRendimientos(List<RendimientoProductivo> rendimientos)
        {
            string ruta = ObtenerRutaRendimientos();
            string carpeta = Path.GetDirectoryName(ruta);

            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string json = JsonSerializer.Serialize(
                NormalizarLista(rendimientos),
                CrearOpcionesJson()
            );

            File.WriteAllText(ruta, json);
        }

        public static void RegenerarDesdeBase()
        {
            GuardarRendimientos(CrearBase());
        }

        public static RendimientoProductivo BuscarMejorPara(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null)
            {
                return null;
            }

            string etapa = Normalizar(req.EtapaSugerida + " " + req.BloqueProductivo);
            string tipo = Normalizar(req.TipoInterno);
            string proceso = Normalizar(req.NombreRequerimiento);
            string unidad = Normalizar(req.Unidad);
            CategoriaTrabajador cargoProductivo = ResolverCargoProductivoParaRequerimiento(req);
            CategoriaTrabajador cargoResuelto = cargoProductivo ?? ResolverCargoParaRequerimiento(req);
            string cargo = Normalizar(
                cargoProductivo == null
                    ? req.CargoSugerido
                    : cargoProductivo.NombreCompleto
            );
            bool requiereRendimiento = cargoResuelto == null
                ? ClasificacionCargosService.RequiereRendimientoProductivo(
                    cargo,
                    req.EntregableCliente,
                    req.NombreRequerimiento,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    req.EcuacionUsada + " " + req.Unidad)
                : ClasificacionCargosService.RequiereRendimientoProductivo(
                    cargoResuelto,
                    req.EntregableCliente,
                    req.NombreRequerimiento,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    req.EcuacionUsada + " " + req.Unidad);

            if (!requiereRendimiento)
            {
                return null;
            }

            return CargarRendimientos()
                .Where(r => r != null && r.Activo && r.CantidadPorPeriodo > 0.0)
                .Select(r => new
                {
                    Rendimiento = r,
                    Puntaje = Puntuar(r, etapa, tipo, proceso, unidad, cargo)
                })
                .Where(x => x.Puntaje > 0)
                .OrderByDescending(x => x.Puntaje)
                .Select(x => x.Rendimiento)
                .FirstOrDefault();
        }

        private static CategoriaTrabajador ResolverCargoParaRequerimiento(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CargoSugerido))
            {
                return null;
            }

            return BibliotecaCargosJsonService.BuscarCargo(
                req.CargoSugerido,
                req.EtapaSugerida + " " + req.BloqueProductivo,
                req.NivelCargoSugerido
            );
        }

        private static CategoriaTrabajador ResolverCargoProductivoParaRequerimiento(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CargoSugerido))
            {
                return null;
            }

            foreach (string cargoTexto in SepararCargos(req.CargoSugerido))
            {
                SepararCargoYNivel(cargoTexto, out string cargoNombre, out string nivel);
                if (string.IsNullOrWhiteSpace(cargoNombre))
                {
                    continue;
                }

                CategoriaTrabajador cargo = BibliotecaCargosJsonService.BuscarCargo(
                    cargoNombre,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    string.IsNullOrWhiteSpace(nivel) ? req.NivelCargoSugerido : nivel
                );

                if (cargo != null && ClasificacionCargosService.EsCargoProductivo(cargo))
                {
                    return cargo;
                }
            }

            return null;
        }

        private static IEnumerable<string> SepararCargos(string cargos)
        {
            return (cargos ?? "")
                .Replace("\r", ";")
                .Replace("\n", ";")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c =>
                {
                    string valor = c.Trim();
                    int pipe = valor.LastIndexOf('|');
                    return pipe >= 0 ? valor.Substring(0, pipe).Trim() : valor;
                })
                .Where(c => !string.IsNullOrWhiteSpace(c));
        }

        private static void SepararCargoYNivel(string texto, out string nombre, out string nivel)
        {
            nombre = (texto ?? "").Trim();
            nivel = "";

            int idx = nombre.LastIndexOf(" (", StringComparison.Ordinal);
            if (idx <= 0 || !nombre.EndsWith(")", StringComparison.Ordinal))
            {
                return;
            }

            nivel = nombre.Substring(idx + 2, nombre.Length - idx - 3).Trim();
            nombre = nombre.Substring(0, idx).Trim();
        }

        public static bool AplicarRendimiento(
            RequerimientoProduccionInterna req,
            double diasHabilesSemana
        )
        {
            if (req == null || req.RendimientoCantidad <= 0.0)
            {
                return false;
            }

            double diasPeriodo = ObtenerDiasPeriodo(req.RendimientoPeriodo, diasHabilesSemana);

            if (diasPeriodo <= 0.0)
            {
                return false;
            }

            RendimientoProductivo rendimiento = BuscarMejorPara(req);

            if (rendimiento == null && !EsRendimientoEditadoManualmente(req))
            {
                req.RendimientoCantidad = 0.0;
                req.RendimientoPeriodo = "";
                req.RendimientoOrigen = "";
                return false;
            }

            double capacidadStd = req.RendimientoCantidad;
            double capacidadMin = rendimiento == null ? 0.0 : rendimiento.CantidadMinimaPorPeriodo;
            double capacidadMax = rendimiento == null ? 0.0 : rendimiento.CantidadMaximaPorPeriodo;

            if (capacidadMin <= 0.0)
            {
                capacidadMin = capacidadStd * 0.80;
            }

            if (capacidadMax <= 0.0)
            {
                capacidadMax = capacidadStd * 1.25;
            }

            double cantidad = Math.Max(0.0, req.Cantidad);
            double diasStd = (cantidad / capacidadStd) * diasPeriodo;
            double diasHolgura = (cantidad / capacidadMin) * diasPeriodo;
            double diasMin = (cantidad / capacidadMax) * diasPeriodo;

            req.DiasPersonaStd = diasStd;
            req.DiasPersonaMin = diasMin;
            req.DiasPersonaHolgura = diasHolgura;

            req.CostoMinimoCLP = req.DiasPersonaMin * req.TarifaDiaCargoCLP;
            req.CostoEstandarCLP = req.DiasPersonaStd * req.TarifaDiaCargoCLP;
            req.CostoHolguraCLP = req.DiasPersonaHolgura * req.TarifaDiaCargoCLP;

            return true;
        }

        public static double ObtenerDiasPeriodo(string periodo, double diasHabilesSemana)
        {
            string p = Normalizar(periodo);
            double diasSemana = diasHabilesSemana > 0.0 ? diasHabilesSemana : 5.0;

            if (p.Contains("dia"))
            {
                return 1.0;
            }

            if (p.Contains("mes"))
            {
                return diasSemana * 4.345;
            }

            return diasSemana;
        }

        private static int Puntuar(
            RendimientoProductivo r,
            string etapa,
            string tipo,
            string proceso,
            string unidad,
            string cargo
        )
        {
            bool coincideUnidad = Coincide(r.Unidad, unidad);

            if (!coincideUnidad)
            {
                return 0;
            }

            bool coincideProceso = Coincide(r.Proceso, proceso);
            bool coincideTipo = Coincide(r.TipoInterno, tipo);
            bool coincideCargo = Coincide(r.Cargo, cargo);
            bool coincideEtapa = Coincide(r.Etapa, etapa);

            if (!coincideProceso && !coincideTipo && !coincideCargo && !coincideEtapa)
            {
                return 0;
            }

            int puntaje = 0;

            if (coincideProceso) puntaje += 40;
            if (coincideTipo) puntaje += 25;
            if (coincideCargo) puntaje += 20;
            if (coincideEtapa) puntaje += 10;
            if (coincideUnidad) puntaje += 5;

            return puntaje;
        }

        private static bool EsRendimientoEditadoManualmente(
            RequerimientoProduccionInterna req
        )
        {
            string origen = Normalizar(req == null ? "" : req.RendimientoOrigen);
            return origen.Contains("editadoendesglose");
        }

        private static bool Coincide(string patron, string textoNormalizado)
        {
            string p = Normalizar(patron);

            if (string.IsNullOrWhiteSpace(p) || string.IsNullOrWhiteSpace(textoNormalizado))
            {
                return false;
            }

            return textoNormalizado.Contains(p) || p.Contains(textoNormalizado);
        }

        private static void CrearJsonSiNoExiste()
        {
            if (!File.Exists(ObtenerRutaRendimientos()))
            {
                GuardarRendimientos(CrearBase());
            }
        }

        private static List<RendimientoProductivo> NormalizarLista(
            List<RendimientoProductivo> rendimientos
        )
        {
            rendimientos = rendimientos ?? new List<RendimientoProductivo>();

            int siguienteId = rendimientos
                .Where(r => r != null)
                .Select(r => r.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;

            foreach (RendimientoProductivo r in rendimientos.Where(r => r != null))
            {
                if (r.Id <= 0)
                {
                    r.Id = siguienteId++;
                }

                if (string.IsNullOrWhiteSpace(r.Periodo))
                {
                    r.Periodo = "semana";
                }

                if (r.CantidadPorPeriodo < 0.0)
                {
                    r.CantidadPorPeriodo = 0.0;
                }

                if (r.CantidadMinimaPorPeriodo <= 0.0 && r.CantidadPorPeriodo > 0.0)
                {
                    r.CantidadMinimaPorPeriodo = r.CantidadPorPeriodo * 0.80;
                }

                if (r.CantidadMaximaPorPeriodo <= 0.0 && r.CantidadPorPeriodo > 0.0)
                {
                    r.CantidadMaximaPorPeriodo = r.CantidadPorPeriodo * 1.25;
                }
            }

            return rendimientos
                .Where(r => r != null)
                .OrderBy(r => ObtenerOrdenEtapa(r.Etapa))
                .ThenBy(r => r.Proceso)
                .ThenBy(r => r.Cargo)
                .ToList();
        }

        private static List<RendimientoProductivo> CrearBase()
        {
            return new List<RendimientoProductivo>
            {
                Crear(1, "Preproduccion", "Personaje", "Diseño / preparación de personaje", "personajes", "Diseñador de personajes", 2, "semana"),
                Crear(2, "Produccion", "Rough", "Rough animation / acting base", "segundos", "Animador 2D", 8, "semana"),
                Crear(3, "Produccion", "Clean up", "Clean up / línea final", "segundos", "Clean up artist", 10, "semana"),
                Crear(4, "Produccion", "Color", "Color / pintura base", "segundos", "Colorista", 15, "semana"),
                Crear(5, "Postproduccion", "Export", "Entrega final", "piezas", "Render / export manager", 8, "dia"),
                Crear(6, "Postproduccion", "Control", "Control técnico de entrega", "piezas", "Control de calidad técnico", 12, "dia")
            };
        }

        private static RendimientoProductivo Crear(
            int id,
            string etapa,
            string tipo,
            string proceso,
            string unidad,
            string cargo,
            double cantidad,
            string periodo
        )
        {
            return new RendimientoProductivo
            {
                Id = id,
                Activo = true,
                Etapa = etapa,
                TipoInterno = tipo,
                Proceso = proceso,
                Unidad = unidad,
                Cargo = cargo,
                NivelCargo = "típico",
                CantidadMinimaPorPeriodo = cantidad * 0.80,
                CantidadPorPeriodo = cantidad,
                CantidadMaximaPorPeriodo = cantidad * 1.25,
                Periodo = periodo,
                Nota = "Base editable del estudio."
            };
        }

        private static int ObtenerOrdenEtapa(string etapa)
        {
            string e = Normalizar(etapa);

            if (e.Contains("desarrollo")) return 10;
            if (e.Contains("preproduccion")) return 20;
            if (e.Contains("produccion")) return 30;
            if (e.Contains("postproduccion")) return 40;

            return 90;
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

        private static string QuitarTildes(string texto)
        {
            string descompuesto = (texto ?? "").Normalize(NormalizationForm.FormD);
            StringBuilder resultado = new StringBuilder(descompuesto.Length);

            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(c);
                }
            }

            return resultado.ToString()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n");
        }

        private static string Normalizar(string texto)
        {
            return QuitarTildes(texto ?? "")
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
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}
