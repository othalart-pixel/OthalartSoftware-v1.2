namespace Cotizador_animacion_Othalart.Models
{
    public static class ModosCalculoProductivo
    {
        public const string Rendimiento = "Rendimiento";
        public const string TiempoAsignado = "TiempoAsignado";

        public static string Normalizar(string valor)
        {
            string texto = (valor ?? "").Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return Rendimiento;
            }

            string normalizado = texto
                .ToLowerInvariant()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "");

            if (normalizado.Contains("tiempoasignado") ||
                normalizado.Contains("horasasignadas") ||
                normalizado.Contains("horasdirectas") ||
                normalizado == "directo")
            {
                return TiempoAsignado;
            }

            return Rendimiento;
        }

        public static bool EsTiempoAsignado(string valor)
        {
            return Normalizar(valor) == TiempoAsignado;
        }
    }
}
