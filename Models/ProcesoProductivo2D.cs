namespace Cotizador_animacion_Othalart.Models
{
    public class ProcesoProductivo2D
    {
        public string Id { get; set; } = "";
        public string ParentSubproductId { get; set; } = "";
        public int Orden { get; set; } = 0;
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool Activo { get; set; } = true;
        public string EtapaProductiva { get; set; } = "";
        public string SubEtapa { get; set; } = "";
        public string EquationKey { get; set; } = "";
        public string EcuacionProductiva { get; set; } = "";
        public string ModoCalculoProductivo { get; set; } = "Rendimiento";
        public double Cantidad { get; set; } = 1.0;
        public string Unidad { get; set; } = "unidad";
        public double HorasAsignadasStd { get; set; } = 0.0;
        public string CargosSugeridos { get; set; } = "";
        public string VariablesEcuacion { get; set; } = "";
        public string ImpactoEcuacion { get; set; } = "";
        public string DependencyId { get; set; } = "";
        public string AssociatedRoles { get; set; } = "";
        public string Nota { get; set; } = "";
    }
}
