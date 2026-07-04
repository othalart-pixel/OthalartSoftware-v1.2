using System;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private double ParsearSemanaDesdeCelda(object valor, double valorDefecto)
        {
            if (valor == null)
            {
                return valorDefecto;
            }

            string texto = valor.ToString() ?? "";

            texto = texto
                .Trim()
                .ToUpperInvariant()
                .Replace("SEMANA", "")
                .Replace("SEMANAS", "")
                .Replace("SEM", "")
                .Replace("S", "")
                .Replace(" ", "")
                .Replace(",", ".");

            double resultadoDouble;

            if (double.TryParse(
                texto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out resultadoDouble))
            {
                return Math.Max(0.1, resultadoDouble);
            }

            return valorDefecto;
        }

        private string FormatearSemanaDecimal(double valor)
        {
            return valor.ToString("0.##");
        }

        private string FormatearSemanaVisual(double valor)
        {
            return "S" + valor.ToString("0.##");
        }

        private int ObtenerSemanaVisualInicio(double semana)
        {
            if (semana < 1.0)
            {
                semana = 1.0;
            }

            return (int)Math.Floor(semana);
        }

        private int ObtenerSemanaVisualFinExclusiva(double semanaFin)
        {
            if (semanaFin < 1.0)
            {
                semanaFin = 1.0;
            }

            int semanaVisual = (int)Math.Ceiling(semanaFin);

            if (semanaVisual < 1)
            {
                semanaVisual = 1;
            }

            return semanaVisual;
        }
    }
}