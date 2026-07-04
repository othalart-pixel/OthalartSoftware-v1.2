namespace Cotizador_animacion_Othalart.Models
{
    public class GestionProductivaRegla
    {
        public int Id { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public string Area { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string NivelCargo { get; set; } = "típico";
        public string EtapaReferencia { get; set; } = "General";
        public string TokensAsociados { get; set; } = "";
        public double MinutosPorDiaPersona { get; set; } = 10.0;
        public string Descripcion { get; set; } = "";
    }
}
