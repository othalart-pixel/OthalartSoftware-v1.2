using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class EscenarioOfertaDesdeDesgloseService
    {
        private const double MargenBaseEstandar = 0.15;

        public static ResultadoEscenarioOfertaDesglose Calcular(Cotizacion cotizacion)
        {
            ResultadoEscenarioOfertaDesglose r =
                new ResultadoEscenarioOfertaDesglose();

            r.MargenBase = MargenBaseEstandar;

            if (cotizacion == null)
            {
                r.Diagnostico = "No hay cotización activa.";
                return r;
            }

            if (cotizacion.DesgloseProductivo == null)
            {
                r.Diagnostico = "No hay desglose productivo generado.";
                return r;
            }

            r.PresupuestoClienteCLP = (double)cotizacion.PresupuestoCliente;

            r.CostoMinimoCLP = cotizacion.DesgloseProductivo.CostoMinimoCLP;
            r.CostoEstandarCLP = cotizacion.DesgloseProductivo.CostoEstandarCLP;
            r.CostoHolgadoCLP = cotizacion.DesgloseProductivo.CostoHolguraCLP;

            r.PrecioMinimoCLP = CalcularPrecioVenta(r.CostoMinimoCLP, r.MargenBase);
            r.PrecioEstandarCLP = CalcularPrecioVenta(r.CostoEstandarCLP, r.MargenBase);
            r.PrecioHolgadoCLP = CalcularPrecioVenta(r.CostoHolgadoCLP, r.MargenBase);

            r.EscenarioRecomendado = ElegirEscenario(r);

            r.Diagnostico = ConstruirDiagnostico(r);

            return r;
        }

        private static double CalcularPrecioVenta(double costo, double margen)
        {
            if (costo <= 0.0)
            {
                return 0.0;
            }

            if (margen <= 0.0 || margen >= 0.80)
            {
                margen = MargenBaseEstandar;
            }

            return costo / (1.0 - margen);
        }

        private static EscenarioPlanificacionDesglose ElegirEscenario(
            ResultadoEscenarioOfertaDesglose r
        )
        {
            if (r == null)
            {
                return EscenarioPlanificacionDesglose.Estandar;
            }

            if (r.PresupuestoClienteCLP <= 0.0)
            {
                return EscenarioPlanificacionDesglose.Estandar;
            }

            if (r.PresupuestoClienteCLP >= r.PrecioHolgadoCLP &&
                r.PrecioHolgadoCLP > 0.0)
            {
                return EscenarioPlanificacionDesglose.Holgado;
            }

            if (r.PresupuestoClienteCLP >= r.PrecioEstandarCLP &&
                r.PrecioEstandarCLP > 0.0)
            {
                return EscenarioPlanificacionDesglose.Estandar;
            }

            return EscenarioPlanificacionDesglose.Minimo;
        }

        private static string ConstruirDiagnostico(ResultadoEscenarioOfertaDesglose r)
        {
            if (r == null)
            {
                return "";
            }

            if (r.PresupuestoClienteCLP <= 0.0)
            {
                return "Sin presupuesto cliente: se recomienda escenario estándar como base.";
            }

            if (r.PrecioMinimoCLP > 0.0 &&
                r.PresupuestoClienteCLP < r.PrecioMinimoCLP)
            {
                return "El presupuesto cliente no alcanza el precio mínimo ofertable. Se aplica mínimo, pero requiere ajuste de alcance, plazo o negociación.";
            }

            if (r.EscenarioRecomendado == EscenarioPlanificacionDesglose.Holgado)
            {
                return "El presupuesto permite una planificación holgada.";
            }

            if (r.EscenarioRecomendado == EscenarioPlanificacionDesglose.Estandar)
            {
                return "El presupuesto permite una planificación estándar.";
            }

            return "El presupuesto permite solo una planificación mínima/agresiva.";
        }
    }
}