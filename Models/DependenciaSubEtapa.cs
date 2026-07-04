namespace Cotizador_animacion_Othalart.Models
{
    public class DependenciaSubEtapa
    {
        public string EtapaObjetivo { get; set; } = "";
        public string SubEtapaObjetivo { get; set; } = "";

        public string EtapaRequisito { get; set; } = "";
        public string SubEtapaRequisito { get; set; } = "";

        public string Motivo { get; set; } = "";

        public bool Bloqueante { get; set; } = false;
        public string Severidad { get; set; } = "Advertencia";

        /*
         * Si true:
         * Si el requisito está activo, el objetivo no puede partir antes
         * de que el requisito termine.
         *
         * Si false:
         * El requisito es un insumo conceptual/creativo,
         * pero no necesariamente fuerza orden semanal.
         */
        public bool RequiereOrdenTemporal { get; set; } = true;

        /*
         * Semanas extra entre requisito y objetivo.
         * Ejemplo:
         * Guion termina S2, holgura 0 => Storyboard mínimo S3.
         * Guion termina S2, holgura 1 => Storyboard mínimo S4.
         */
        public int HolguraSemanas { get; set; } = 0;

        /*
         * Valores sugeridos:
         * InsumoPrevio
         * InsumoCreativo
         * MaterialCliente
         * GateFinal
         */
        public string TipoRequisito { get; set; } = "InsumoPrevio";
    }
}