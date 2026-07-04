namespace Cotizador_animacion_Othalart.Models
{
    public class Pieza2DSeleccionGuardada
    {
        public string ClaveSeleccion { get; set; } = "";
        public bool Usar { get; set; } = false;

        public string Categoria { get; set; } = "";
        public string Nombre { get; set; } = "";

        public int Cantidad { get; set; } = 1;
        public double DuracionPorUnidad { get; set; } = 0.0;
        public string UnidadDuracion { get; set; } = "segundos";
        public string UnidadCantidad { get; set; } = "unidades";

        public string EtapaSugerida { get; set; } = "";
        public string SubEtapaSugerida { get; set; } = "";
        public string DependeDe { get; set; } = "";
        public string CargosSugeridos { get; set; } = "";
        public string EcuacionProductiva { get; set; } = "";
        public string VariablesEcuacion { get; set; } = "";
        public string ImpactoEcuacion { get; set; } = "";
        public string Nota { get; set; } = "";
    }
}
