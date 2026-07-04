namespace Cotizador_animacion_Othalart.Models
{
    public class RecomendacionEtapa
    {
        public string NombreEtapa { get; set; } = "";

        public double PorcentajeMinimo { get; set; }
        public double PorcentajeRecomendado { get; set; }
        public double PorcentajeMaximo { get; set; }

        public double SolapeMinimoPermitido { get; set; }
        public double SolapeRecomendado { get; set; }

        public bool Editable { get; set; } = true;
    }
}