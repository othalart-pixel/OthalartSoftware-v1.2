namespace Cotizador_animacion_Othalart.Models
{
    public class AsignacionManoObraProyecto
    {
        public string IdAsignacion { get; set; } = "";
        public string ClaveLabor { get; set; } = "";
        public string PiezaSubproducto { get; set; } = "";
        public string Etapa { get; set; } = "";
        public string SubEtapaLabor { get; set; } = "";
        public string CargoRequerido { get; set; } = "";
        public double HorasRequeridas { get; set; } = 0.0;
        public string PersonaId { get; set; } = "";
        public string PersonaNombre { get; set; } = "";
        public double HorasAsignadas { get; set; } = 0.0;
        public string TipoAsignacion { get; set; } = "Recurso generico";
        public string Observaciones { get; set; } = "";
    }
}
