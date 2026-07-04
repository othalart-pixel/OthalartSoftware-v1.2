namespace Cotizador_animacion_Othalart.Models
{
    public class ResolucionDependenciaSubEtapa
    {
        public string EtapaRequisito { get; set; } = "";
        public string SubEtapaRequisito { get; set; } = "";

        /*
         * Interna:
         * la hacemos nosotros.
         *
         * EntregadaPorCliente:
         * el cliente la entrega.
         *
         * YaExiste:
         * ya viene hecha.
         *
         * NoAplica:
         * no corresponde para este proyecto.
         *
         * RiesgoAceptado:
         * falta, pero el usuario acepta continuar.
         */
        public string ModoResolucion { get; set; } = "Pendiente";

        public string Nota { get; set; } = "";
    }
}