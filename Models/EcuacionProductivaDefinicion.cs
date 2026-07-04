namespace Cotizador_animacion_Othalart.Models
{
    public class EcuacionProductivaDefinicion
    {
        public bool Activa { get; set; } = true;
        public string Clave { get; set; } = "";
        public string NombreVisible { get; set; } = "";
        public string TipoEcuacion { get; set; } = "Variante";
        public string EcuacionBase { get; set; } = "";
        public string Etapa { get; set; } = "";
        public string SubEtapa { get; set; } = "";
        public string Tokens { get; set; } = "";
        public string Variables { get; set; } = "";
        public string CargosPermitidos { get; set; } = "";
        public string CargosParticipantesJson { get; set; } = "";
        public string Impacto { get; set; } = "";
        public string FormulaReferencia { get; set; } = "";
        public string Numerador { get; set; } = "";
        public string Denominador { get; set; } = "";
        public string Nota { get; set; } = "";
    }
}
