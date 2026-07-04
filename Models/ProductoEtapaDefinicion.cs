namespace Cotizador_animacion_Othalart.Models
{
    public class ProductoEtapaDefinicion
    {
        public string ClaveEtapa { get; set; } = "";
        public string NombreVisible { get; set; } = "";
        public int Orden { get; set; } = 0;
        public bool Activa { get; set; } = true;
        public string DependeDe { get; set; } = "";
        public string Nota { get; set; } = "";
    }
}
