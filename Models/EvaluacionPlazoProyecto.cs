using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class EvaluacionPlazoProyecto
    {
        public double SemanasCliente { get; set; } = 0.0;

        public double SemanasMinimasEstimadas { get; set; } = 0.0;
        public double SemanasEstandarEstimadas { get; set; } = 0.0;
        public double SemanasConHolguraEstimadas { get; set; } = 0.0;

        public string DiagnosticoPlazo { get; set; } = "";

        public List<EvaluacionPlazoSubproducto> Subproductos { get; set; }
            = new List<EvaluacionPlazoSubproducto>();
    }

    public class EvaluacionPlazoSubproducto
    {
        public string NombreProducto { get; set; } = "";

        public double SemanasMinimas { get; set; } = 0.0;
        public double SemanasEstandar { get; set; } = 0.0;
        public double SemanasConHolgura { get; set; } = 0.0;

        public bool PermiteParalelizar { get; set; } = true;
    }
}