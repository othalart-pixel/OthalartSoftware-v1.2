using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Datos;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class BibliotecaRecomendacionesCargoSubEtapaService
    {
        public static List<RecomendacionCargoSubEtapa> ObtenerRecomendacionesParaSubEtapasActivas(
            List<SubEtapaProyecto> subEtapas
        )
        {
            List<RecomendacionCargoSubEtapa> resultado =
                new List<RecomendacionCargoSubEtapa>();

            List<RecomendacionCargoSubEtapa> biblioteca =
                BibliotecaRecomendacionesCargoSubEtapa.CrearBase();

            if (subEtapas == null || subEtapas.Count == 0)
            {
                return resultado;
            }

            bool haySubEtapasActivas = subEtapas.Any(s => s != null && s.Activa);

            if (!haySubEtapasActivas)
            {
                return resultado;
            }

            /*
             * Cargos generales recomendados para cualquier proyecto con alcance activo.
             */
            resultado.AddRange(
                biblioteca.Where(r => r != null && r.EsGeneral)
            );

            /*
             * Cargos específicos según subetapas activas.
             */
            foreach (SubEtapaProyecto sub in subEtapas.Where(s => s != null && s.Activa))
            {
                string etapaSub = Normalizar(sub.EtapaPadre);
                string nombreSub = Normalizar(sub.Nombre);

                var recomendacionesSub = biblioteca
                    .Where(r => r != null && !r.EsGeneral)
                    .Where(r =>
                        Normalizar(r.Etapa) == etapaSub &&
                        CoincideNombre(Normalizar(r.SubEtapa), nombreSub))
                    .ToList();

                resultado.AddRange(recomendacionesSub);
            }

            return resultado
                .GroupBy(r =>
                    Normalizar(r.Etapa) + "|" +
                    Normalizar(r.SubEtapa) + "|" +
                    Normalizar(r.Cargo))
                .Select(g => g.First())
                .ToList();
        }

        private static bool CoincideNombre(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
            {
                return false;
            }

            return a.Contains(b) || b.Contains(a);
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
                .Replace("/", "");
        }
    }
}