namespace Cotizador_animacion_Othalart.Models
{
    public class SubproductoBrief
    {
        public string ProductoOrigen { get; set; } = "";
        public string Nombre { get; set; } = "";

        public string EtapaSugerida { get; set; } = "";
        public string SubEtapaSugerida { get; set; } = "";
        public string EcuacionProductiva { get; set; } = "";
        public string VariablesEcuacion { get; set; } = "";
        public string ImpactoEcuacion { get; set; } = "";

        public bool Requerido { get; set; } = true;
        public bool PuedeEntregarCliente { get; set; } = true;

        // Interno = lo hace Othalart.
        // Cliente = lo entrega el cliente.
        // NoAplica = no se considera.
        public string Resolucion { get; set; } = "Interno";

        public string Nota { get; set; } = "";
    }
}
