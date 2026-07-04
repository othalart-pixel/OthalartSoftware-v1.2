namespace Cotizador_animacion_Othalart.Models
{
    public class RecomendacionCargoSubEtapa
    {
        public string Etapa { get; set; } = "";
        public string SubEtapa { get; set; } = "";

        public string Cargo { get; set; } = "";
        public string NivelSugerido { get; set; } = "típico";

        public bool EsGeneral { get; set; } = false;
        public bool RecomendadoPorDefecto { get; set; } = true;

        public string Motivo { get; set; } = "";
    }
}