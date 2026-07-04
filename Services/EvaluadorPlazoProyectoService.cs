using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class EvaluadorPlazoProyectoService
    {
        public static EvaluacionPlazoProyecto Evaluar(Cotizacion cotizacion)
        {
            EvaluacionPlazoProyecto evaluacion = new EvaluacionPlazoProyecto();

            if (cotizacion == null)
            {
                evaluacion.DiagnosticoPlazo = "No hay cotización disponible.";
                return evaluacion;
            }

            evaluacion.SemanasCliente = cotizacion.PlazoClienteSemanas;

            DesgloseProductivoProyecto desglose =
                DesgloseProductivoService.Generar(cotizacion);

            cotizacion.DesgloseProductivo = desglose;

            if (desglose == null || desglose.Requerimientos == null || desglose.Requerimientos.Count == 0)
            {
                evaluacion.DiagnosticoPlazo =
                    "No hay desglose productivo suficiente para evaluar el plazo.";

                return evaluacion;
            }

            evaluacion.SemanasMinimasEstimadas = desglose.SemanasMinimas;
            evaluacion.SemanasEstandarEstimadas = desglose.SemanasEstandar;
            evaluacion.SemanasConHolguraEstimadas = desglose.SemanasHolgura;

            foreach (RequerimientoProduccionInterna req in desglose.Requerimientos)
            {
                evaluacion.Subproductos.Add(new EvaluacionPlazoSubproducto
                {
                    NombreProducto = req.EntregableCliente + " → " + req.NombreRequerimiento,
                    SemanasMinimas = req.DiasPersonaMin / 5.0,
                    SemanasEstandar = req.DiasPersonaStd / 5.0,
                    SemanasConHolgura = req.DiasPersonaHolgura / 5.0,
                    PermiteParalelizar = true
                });
            }

            evaluacion.DiagnosticoPlazo = desglose.Diagnostico;

            return evaluacion;
        }
    }
}