namespace Cotizador_animacion_Othalart.Services
{
    public static class CalculosFinancierosService
    {
        public static double CalcularPrecioPorMargen(double costoTotal, double margen)
        {
            if (margen >= 1.0)
            {
                return 0.0;
            }

            if (margen < 0.0)
            {
                return costoTotal;
            }

            return costoTotal / (1.0 - margen);
        }

        public static double CalcularUtilidad(double precioVenta, double costoTotal)
        {
            return precioVenta - costoTotal;
        }

        public static double CalcularMargen(double precioVenta, double utilidad)
        {
            if (precioVenta <= 0.0)
            {
                return 0.0;
            }

            return utilidad / precioVenta;
        }

        public static double CalcularMarkup(double costoTotal, double utilidad)
        {
            if (costoTotal <= 0.0)
            {
                return 0.0;
            }

            return utilidad / costoTotal;
        }
    }
}