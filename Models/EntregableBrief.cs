namespace Cotizador_animacion_Othalart.Models
{
    public class EntregableBrief
    {
        public string Categoria { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string ClaveSeleccion { get; set; } = "";

        // Ejemplo:
        // 30 loops, 4 fondos, 12 props, 1 trailer.
        public int Cantidad { get; set; } = 1;

        // Ejemplo:
        // Loop de personaje 2D: 2 segundos c/u.
        // Trailer 2D: 45 segundos.
        // Fondo 2D: 0 porque no aplica duración.
        public double DuracionPorUnidad { get; set; } = 0.0;

        // segundos, minutos, frames, no aplica.
        public string UnidadDuracion { get; set; } = "segundos";

        // loops, fondos, props, assets, escenas, planos, unidades.
        public string UnidadCantidad { get; set; } = "unidades";

        public string Nota { get; set; } = "";

        public string EtapaSugerida { get; set; } = "";

        public string SubEtapaSugerida { get; set; } = "";

        public string DependeDe { get; set; } = "";

        public string CargosSugeridos { get; set; } = "";

        public string CargosParticipantesJson { get; set; } = "";

        public string EcuacionProductiva { get; set; } = "";

        public string VariablesEcuacion { get; set; } = "";

        public string ImpactoEcuacion { get; set; } = "";

        // =========================
        // SUPUESTOS PRELIMINARES DE PRODUCCIÓN
        // =========================

        public int? PlanosEstimados { get; set; } = null;

        public int? BackgroundsEstimados { get; set; } = null;

        public int? PersonajesEstimados { get; set; } = null;

        public int? PropsEstimados { get; set; } = null;

        public double? SegundosAnimadosEfectivos { get; set; } = null;

        public string NivelCalidadEstimado { get; set; } = "Estándar";
        // Valores sugeridos: "Baja", "Estándar", "Alta", "Premium"

        public int RevisionesIncluidas { get; set; } = 2;

        public bool SupuestosProduccionEditadosManualmente { get; set; } = false;
    }
}
