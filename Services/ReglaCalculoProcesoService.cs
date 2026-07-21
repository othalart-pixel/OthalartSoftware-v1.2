using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ReglaCalculoProcesoService
    {
        public static double ObtenerFactorPorcentaje(
            EcuacionProductivaDefinicion proceso,
            double fallbackRevision = 0.10,
            double fallbackCorreccion = 0.15
        )
        {
            string texto = (proceso == null
                ? ""
                : proceso.FormulaReferencia + " " + proceso.Variables).ToLowerInvariant();

            Match porcentaje = Regex.Match(texto, @"(\d+(?:[\.,]\d+)?)\s*%");
            if (porcentaje.Success &&
                IntentarParsearNumero(porcentaje.Groups[1].Value, out double valorPorcentaje))
            {
                return Math.Max(0.0, valorPorcentaje / 100.0);
            }

            Match multiplicador = Regex.Match(texto, @"(?:\*|x)\s*(\d+(?:[\.,]\d+)?)");
            if (multiplicador.Success &&
                IntentarParsearNumero(multiplicador.Groups[1].Value, out double valorMultiplicador))
            {
                return Math.Max(0.0, valorMultiplicador);
            }

            return proceso != null &&
                   proceso.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo
                ? fallbackCorreccion
                : fallbackRevision;
        }

        public static double ObtenerHorasPorSemana(
            EcuacionProductivaDefinicion proceso,
            double fallbackGestion = 4.0,
            double fallbackOtro = 8.0
        )
        {
            string texto = proceso == null ? "" : proceso.FormulaReferencia;
            Match horas = Regex.Match(texto, @"(?:\*|x)\s*(\d+(?:[\.,]\d+)?)");
            if (horas.Success &&
                IntentarParsearNumero(horas.Groups[1].Value, out double valorHoras))
            {
                return Math.Max(0.0, valorHoras);
            }

            return proceso != null &&
                   proceso.TipoProceso == TipoProcesoProductivo.GestionCoordinacion
                ? fallbackGestion
                : fallbackOtro;
        }

        public static string CrearDependenciasJson(IEnumerable<string> dependencias)
        {
            List<string> limpias = (dependencias ?? Enumerable.Empty<string>())
                .Select(d => (d ?? "").Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return limpias.Count == 0
                ? ""
                : JsonSerializer.Serialize(limpias, new JsonSerializerOptions { WriteIndented = true });
        }

        public static List<string> LeerDependenciasJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                return (JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>())
                    .Select(d => (d ?? "").Trim())
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static bool IntentarParsearNumero(string texto, out double valor)
        {
            return double.TryParse(
                (texto ?? "").Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out valor);
        }
    }
}
