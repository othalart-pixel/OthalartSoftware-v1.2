namespace Cotizador_animacion_Othalart.Models
{
    public class Subproducto2D
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Orden { get; set; } = 0;

        public bool RequeridoPorDefecto { get; set; } = true;
        public bool PuedeEntregarCliente { get; set; } = true;

        public string EtapaSugerida { get; set; } = "";
        public string SubEtapaSugerida { get; set; } = "";
        public string DependeDe { get; set; } = "";
        public string CargosSugeridos { get; set; } = "";
        public string EquationKey { get; set; } = "";
        public string EcuacionProductiva { get; set; } = "";
        public string VariablesEcuacion { get; set; } = "";
        public string ImpactoEcuacion { get; set; } = "";
        public string ModoCalculoProductivo { get; set; } = "Rendimiento";
        public double Cantidad { get; set; } = 1.0;
        public string Unidad { get; set; } = "unidad";
        public string TipoComportamientoCalculo { get; set; } = "Suma de procesos";
        public System.Collections.Generic.List<ProcesoProductivo2D> Procesos { get; set; } =
            new System.Collections.Generic.List<ProcesoProductivo2D>();
        public double HorasAsignadasMin { get; set; } = 0.0;
        public double HorasAsignadasStd { get; set; } = 0.0;
        public double HorasAsignadasHolgura { get; set; } = 0.0;

        // NUEVO:
        // Interno = lo hace Othalart.
        // Cliente = viene entregado por cliente.
        // NoAplica = se descarta para este proyecto.
        public string Resolucion { get; set; } = "Interno";

        public string Nota { get; set; } = "";
    }
}
