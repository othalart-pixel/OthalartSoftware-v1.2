using Cotizador_animacion_Othalart.Models;
using System.Linq;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ServicioCotizacion
    {
        public const double HorasGestionDia = 0.25;
        public const double DiasTrabajoMes = 22.0;
        public const double CostoHoraGestionCLP = 6000.0;
        public const double DolarReferencial = 900.0;

        private static void RecalcularManoObra(Cotizacion cotizacion)
        {
            cotizacion.PersonaMesTotal = 0.0;
            cotizacion.CostoManoObraEtapas = 0.0;

            foreach (EtapaProyecto etapa in cotizacion.Etapas.Where(e => e.Seleccionada))
            {
                foreach (CargoPlanMensual cargo in etapa.Plan)
                {
                    double personaMes = cargo.PersonasPorBloque.Sum();

                    double valorMensualCLP = cargo.SueldoMensualCLPEditable;

                    if (valorMensualCLP <= 0.0 && cargo.Categoria != null)
                    {
                        valorMensualCLP = cargo.Categoria.SueldoMensualCLPTipico;
                        cargo.SueldoMensualCLPEditable = valorMensualCLP;
                    }

                    double costo = valorMensualCLP * personaMes;

                    cargo.PersonaMesTotal = personaMes;
                    cargo.CostoTotal = costo;

                    cotizacion.PersonaMesTotal += personaMes;
                    cotizacion.CostoManoObraEtapas += costo;
                }
            }
        }


        

        public static double CostoHoraGestion(string moneda)
        {
            if (moneda == "CLP")
            {
                return CostoHoraGestionCLP;
            }

            return CostoHoraGestionCLP / DolarReferencial;
        }

        public static double CalcularCostoGestionPorPersonaMes(double personaMesTotal, string moneda)
        {
            double costoHora = CostoHoraGestion(moneda);

            return personaMesTotal
                * HorasGestionDia
                * DiasTrabajoMes
                * costoHora;
        }

        public static void RecalcularCostosInternos(Cotizacion cotizacion)
        {
            cotizacion.CostoProduccionInterna =
                CalcularCostoGestionPorPersonaMes(cotizacion.PersonaMesTotal, cotizacion.Moneda);

            cotizacion.CostoAdministrativo =
                CalcularCostoGestionPorPersonaMes(cotizacion.PersonaMesTotal, cotizacion.Moneda);
        }

        public static void RecalcularCostoTotal(Cotizacion cotizacion)
        {
            double totalCostosExtra = cotizacion.CostosExtra.Sum(costo =>
                costo.MontoCalculado > 0.0 ? costo.MontoCalculado : costo.Monto
            );

            cotizacion.CostoTercerizados = totalCostosExtra;
            cotizacion.OtrosCostos = 0.0;

            cotizacion.CostoBase =
                cotizacion.CostoManoObraEtapas
                + cotizacion.CostoProduccionInterna
                + cotizacion.CostoAdministrativo
                + totalCostosExtra;

            cotizacion.Imprevistos =
                cotizacion.CostoBase * cotizacion.TasaImprevistos;

            cotizacion.CostoTotal =
                cotizacion.CostoBase + cotizacion.Imprevistos;
        }

        public static void RecalcularPrecioRecomendado(Cotizacion cotizacion)
        {
            cotizacion.PrecioRecomendado =
                CalculosFinancierosService.CalcularPrecioPorMargen(
                    cotizacion.CostoTotal,
                    cotizacion.MargenObjetivo
                );
        }

        public static void RecalcularPrecioEvaluado(Cotizacion cotizacion)
        {
            cotizacion.ImpuestoVenta =
                cotizacion.PrecioVentaEvaluado * cotizacion.TasaImpuestoVenta;

            cotizacion.PrecioVentaConImpuesto =
                cotizacion.PrecioVentaEvaluado + cotizacion.ImpuestoVenta;

            cotizacion.UtilidadEvaluada =
                CalculosFinancierosService.CalcularUtilidad(
                    cotizacion.PrecioVentaEvaluado,
                    cotizacion.CostoTotal
                );

            cotizacion.MargenEvaluado =
                CalculosFinancierosService.CalcularMargen(
                    cotizacion.PrecioVentaEvaluado,
                    cotizacion.UtilidadEvaluada
                );

            cotizacion.MarkupEvaluado =
                CalculosFinancierosService.CalcularMarkup(
                    cotizacion.CostoTotal,
                    cotizacion.UtilidadEvaluada
                );
        }

        private static void RecalcularEvaluacionPlazo(Cotizacion cotizacion)
        {
            cotizacion.DuracionPlanificadaSemanas = 0.0;
            cotizacion.DuracionMinimaTecnicaSemanas = 0.0;
            cotizacion.DuracionEstandarTecnicaSemanas = 0.0;
            cotizacion.DuracionHolguraTecnicaSemanas = 0.0;
            cotizacion.DiagnosticoPlazo = "";
            cotizacion.FactorPresionPlazo = 1.0;

            cotizacion.EvaluacionPlazo =
                EvaluadorPlazoProyectoService.Evaluar(cotizacion);

            if (cotizacion.Etapas == null || cotizacion.Etapas.Count == 0)
            {
                cotizacion.DiagnosticoPlazo = "No hay etapas definidas para evaluar plazo.";
                return;
            }

            var etapasSeleccionadas = cotizacion.Etapas
                .Where(e => e.Seleccionada)
                .ToList();

            if (etapasSeleccionadas.Count == 0)
            {
                cotizacion.DiagnosticoPlazo = "No hay etapas seleccionadas para evaluar plazo.";
                return;
            }

            double inicioMinimo = etapasSeleccionadas.Min(e => e.InicioMes);
            double finMaximo = etapasSeleccionadas.Max(e => e.FinMes);

            double duracionMeses = finMaximo - inicioMinimo;

            if (duracionMeses <= 0.0)
            {
                duracionMeses = etapasSeleccionadas.Max(e => e.DuracionMeses);
            }

            if (duracionMeses <= 0.0)
            {
                duracionMeses = 1.0;
            }

            double duracionSemanasPlan = duracionMeses * 4.345;

            cotizacion.DuracionPlanificadaSemanas = duracionSemanasPlan;

            double minimoDesdeBiblioteca = cotizacion.EvaluacionPlazo == null
                ? 0.0
                : cotizacion.EvaluacionPlazo.SemanasMinimasEstimadas;

            double estandarDesdeBiblioteca = cotizacion.EvaluacionPlazo == null
                ? 0.0
                : cotizacion.EvaluacionPlazo.SemanasEstandarEstimadas;

            double holguraDesdeBiblioteca = cotizacion.EvaluacionPlazo == null
                ? 0.0
                : cotizacion.EvaluacionPlazo.SemanasConHolguraEstimadas;

            if (minimoDesdeBiblioteca > 0.0)
            {
                cotizacion.DuracionMinimaTecnicaSemanas = minimoDesdeBiblioteca;
            }
            else
            {
                cotizacion.DuracionMinimaTecnicaSemanas = duracionSemanasPlan * 0.70;
            }

            if (estandarDesdeBiblioteca > 0.0)
            {
                cotizacion.DuracionEstandarTecnicaSemanas = estandarDesdeBiblioteca;
            }
            else
            {
                cotizacion.DuracionEstandarTecnicaSemanas = duracionSemanasPlan;
            }

            if (holguraDesdeBiblioteca > 0.0)
            {
                cotizacion.DuracionHolguraTecnicaSemanas = holguraDesdeBiblioteca;
            }
            else
            {
                cotizacion.DuracionHolguraTecnicaSemanas = duracionSemanasPlan * 1.30;
            }

            double plazoCliente = cotizacion.PlazoClienteSemanas;

            if (plazoCliente <= 0.0)
            {
                cotizacion.DiagnosticoPlazo = "Sin fecha objetivo declarada por el cliente.";
                cotizacion.FactorPresionPlazo = 1.0;
                return;
            }

            if (plazoCliente < cotizacion.DuracionMinimaTecnicaSemanas)
            {
                cotizacion.DiagnosticoPlazo =
                    "INVIABLE EN PLAZO: el plazo cliente está bajo el mínimo técnico estimado.";

                cotizacion.FactorPresionPlazo = 1.60;
                return;
            }

            if (plazoCliente < cotizacion.DuracionEstandarTecnicaSemanas)
            {
                cotizacion.DiagnosticoPlazo =
                    "PLAZO AGRESIVO: posible solo con paralelización, más equipo, reducción de alcance o menos rondas de revisión.";

                cotizacion.FactorPresionPlazo = 1.35;
                return;
            }

            if (plazoCliente < cotizacion.DuracionHolguraTecnicaSemanas)
            {
                cotizacion.DiagnosticoPlazo =
                    "PLAZO VIABLE AJUSTADO: cumple el estándar interno, pero con poca holgura.";

                cotizacion.FactorPresionPlazo = 1.15;
                return;
            }

            cotizacion.DiagnosticoPlazo =
                "PLAZO VIABLE: el plazo cliente permite una planificación con holgura.";

            cotizacion.FactorPresionPlazo = 1.0;
        }

        private static double CalcularFactorPresionPlazo(EvaluacionPlazoProyecto evaluacion)
        {
            if (evaluacion == null)
            {
                return 1.0;
            }

            if (evaluacion.SemanasCliente <= 0.0 ||
                evaluacion.SemanasMinimasEstimadas <= 0.0)
            {
                return 1.0;
            }

            if (evaluacion.SemanasCliente < evaluacion.SemanasMinimasEstimadas)
            {
                return 1.60;
            }

            if (evaluacion.SemanasCliente < evaluacion.SemanasEstandarEstimadas)
            {
                return 1.35;
            }

            if (evaluacion.SemanasCliente < evaluacion.SemanasConHolguraEstimadas)
            {
                return 1.15;
            }

            return 1.0;
        }

        public static void RecalcularCotizacion(Cotizacion cotizacion)
        {
            PlanEtapasService.RecalcularFinesEtapas(cotizacion);

            RecalcularEvaluacionPlazo(cotizacion);

            RecalcularManoObra(cotizacion);
            RecalcularCostosInternos(cotizacion);
            RecalcularCostoTotal(cotizacion);
            RecalcularPrecioRecomendado(cotizacion);

            if (cotizacion.PrecioVentaEvaluado <= 0.0)
            {
                cotizacion.PrecioVentaEvaluado = cotizacion.PrecioRecomendado;
            }

            RecalcularPrecioEvaluado(cotizacion);
        }
    }
}
