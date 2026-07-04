using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class OpcionesProducto2D
    {
        public List<string> Industrias { get; set; } = new List<string>();
        public List<string> Categorias { get; set; } = new List<string>();
        public List<string> UnidadesCantidad { get; set; } = new List<string>();
        public List<string> UnidadesDuracion { get; set; } = new List<string>();
    }
}
