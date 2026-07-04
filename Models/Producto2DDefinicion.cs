using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class Producto2DDefinicion
    {
        public string Nombre { get; set; } = "";
        public string Industria { get; set; } = "";
        public string Categoria { get; set; } = "";

        public string UnidadCantidadSugerida { get; set; } = "piezas";
        public string UnidadDuracionSugerida { get; set; } = "segundos";

        public double DuracionSugerida { get; set; } = 0.0; 

        public string Nota { get; set; } = "";

        public List<ProductoEtapaDefinicion> Etapas { get; set; } =
            new List<ProductoEtapaDefinicion>();

        public List<Subproducto2D> Subproductos { get; set; } = new List<Subproducto2D>();
    }
}
    