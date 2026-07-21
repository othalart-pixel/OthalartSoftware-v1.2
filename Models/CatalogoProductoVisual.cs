using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class CatalogoProductoVisual
    {
        public Producto2DDefinicion Producto { get; set; }
        public List<CatalogoEtapaVisual> Etapas { get; set; } =
            new List<CatalogoEtapaVisual>();
    }

    public class CatalogoEtapaVisual
    {
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int Orden { get; set; } = 0;
        public bool Activa { get; set; } = true;
        public List<CatalogoProcesoVisual> Procesos { get; set; } =
            new List<CatalogoProcesoVisual>();
    }

    public class CatalogoProcesoVisual
    {
        public Subproducto2D Subproducto { get; set; }
        public string Nombre { get; set; } = "";
        public string Proceso { get; set; } = "";
        public string Cargos { get; set; } = "";
        public string Ecuacion { get; set; } = "";
        public string Diagnostico { get; set; } = "";
        public bool TieneDiagnostico { get; set; } = false;
    }
}
