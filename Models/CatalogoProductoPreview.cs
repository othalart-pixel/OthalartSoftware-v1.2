using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class CatalogoProductoPreview
    {
        public Producto2DDefinicion Producto { get; set; }
        public double TotalHoras { get; set; }
        public double CostoDirectoCLP { get; set; }
        public double PrecioSugeridoCLP { get; set; }
        public double MargenEstimado { get; set; }
        public int CantidadProcesos { get; set; }
        public int CantidadCargos { get; set; }
        public List<CatalogoProcesoPreview> Procesos { get; set; } =
            new List<CatalogoProcesoPreview>();
        public List<CatalogoEtapaPreview> Etapas { get; set; } =
            new List<CatalogoEtapaPreview>();
        public List<string> Validaciones { get; set; } = new List<string>();
    }

    public class CatalogoProcesoPreview
    {
        public Subproducto2D Subproducto { get; set; }
        public RequerimientoProduccionInterna Requerimiento { get; set; }
        public string Etapa { get; set; } = "";
        public string Proceso { get; set; } = "";
        public string SubproductoNombre { get; set; } = "";
        public string Cargos { get; set; } = "";
        public double Horas { get; set; }
        public double CostoManoObraCLP { get; set; }
        public double CostosAdicionalesCLP { get; set; }
        public double CostoTotalCLP { get; set; }
        public double PrecioSugeridoCLP { get; set; }
        public string Ecuacion { get; set; } = "";
        public string Estado { get; set; } = "OK";
        public List<string> DependenciasFaltantes { get; set; } =
            new List<string>();
        public string DetalleCalculo { get; set; } = "";
        public bool Expandido { get; set; }
    }

    public class CatalogoEtapaPreview
    {
        public string Etapa { get; set; } = "";
        public int CantidadProcesos { get; set; }
        public double Horas { get; set; }
        public double CostoTotalCLP { get; set; }
        public double PrecioSugeridoCLP { get; set; }
        public double PorcentajeCosto { get; set; }
        public List<string> Cargos { get; set; } = new List<string>();
        public string Estado { get; set; } = "OK";
    }
}
