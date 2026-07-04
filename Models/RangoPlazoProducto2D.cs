using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class RangoPlazoProducto2D
    {
        public string NombreProducto { get; set; } = "";
        public string Categoria { get; set; } = "";

        public List<string> Alias { get; set; } = new List<string>();

        public string UnidadBase { get; set; } = "unidad";
        // Valores esperados:
        // "unidad", "segundo", "minuto"

        public double SemanasMinimas { get; set; } = 1.0;
        public double SemanasEstandar { get; set; } = 2.0;
        public double SemanasConHolgura { get; set; } = 3.0;

        public bool PermiteParalelizar { get; set; } = true;

        public string NotaPlazo { get; set; } = "";
    }
}