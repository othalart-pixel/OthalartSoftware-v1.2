namespace Cotizador_animacion_Othalart.Models
{
    public class Subproducto2D
    {
        public string Nombre { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Orden { get; set; } = 0;

        public bool RequeridoPorDefecto { get; set; } = true;
        public bool PuedeEntregarCliente { get; set; } = true;

        public string EtapaSugerida { get; set; } = "";
        public string SubEtapaSugerida { get; set; } = "";
        public string DependeDe { get; set; } = "";
        public string CargosSugeridos { get; set; } = "";
        public string EcuacionProductiva { get; set; } = "";
        public string VariablesEcuacion { get; set; } = "";
        public string ImpactoEcuacion { get; set; } = "";

        // NUEVO:
        // Interno = lo hace Othalart.
        // Cliente = viene entregado por cliente.
        // NoAplica = se descarta para este proyecto.
        public string Resolucion { get; set; } = "Interno";

        public string Nota { get; set; } = "";
    }
}
