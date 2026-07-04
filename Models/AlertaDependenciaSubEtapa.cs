namespace Cotizador_animacion_Othalart.Models
{
    public class AlertaDependenciaSubEtapa
    {
        public string EtapaObjetivo { get; set; } = "";
        public string SubEtapaObjetivo { get; set; } = "";

        public string EtapaRequisito { get; set; } = "";
        public string SubEtapaRequisito { get; set; } = "";

        public string Motivo { get; set; } = "";
        public string Severidad { get; set; } = "";

        public bool Bloqueante { get; set; } = false;
        public bool RequiereOrdenTemporal { get; set; } = true;

        public string TipoRequisito { get; set; } = "InsumoPrevio";

        public string Mensaje
        {
            get
            {
                string tipo = Bloqueante ? "Bloqueante" : Severidad;

                return tipo + ": " +
                       SubEtapaObjetivo +
                       " requiere " +
                       SubEtapaRequisito +
                       ". " +
                       Motivo;
            }
        }
    }
}