using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class ProyectoProductivoExpandido
    {
        public string ProyectoId { get; set; } = "";
        public List<FilaProductivaProyecto> Filas { get; set; } = new List<FilaProductivaProyecto>();
        public List<string> Diagnosticos { get; set; } = new List<string>();
    }

    public class FilaProductivaProyecto
    {
        public string ProyectoId { get; set; } = "";
        public string GrupoId { get; set; } = "";
        public string ItemId { get; set; } = "";
        public string ProductoProyectoId { get; set; } = "";
        public string SubproductoProyectoId { get; set; } = "";
        public string InstanciaId { get; set; } = "";
        public string ProcesoProyectoId { get; set; } = "";
        public string ProcesoBibliotecaId { get; set; } = "";
        public TipoProcesoProductivo TipoProceso { get; set; } = TipoProcesoProductivo.NoClasificado;
        public MetodoCalculoProceso MetodoCalculo { get; set; } = MetodoCalculoProceso.NoDefinido;
        public AlcanceTemporalProceso AlcanceTemporal { get; set; } = AlcanceTemporalProceso.NoDefinido;
        public string EtapaId { get; set; } = "";
        public string CargoId { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public decimal Cantidad { get; set; } = 0m;
        public string Unidad { get; set; } = "";
        public decimal Capacidad { get; set; } = 0m;
        public string Periodo { get; set; } = "";
        public decimal HorasCalculadas { get; set; } = 0m;
        public decimal HorasAsignadas { get; set; } = 0m;
        public decimal Costo { get; set; } = 0m;
        public List<string> Dependencias { get; set; } = new List<string>();
        public bool Paralelo { get; set; } = false;
        public bool Transversal { get; set; } = false;
        public bool TieneOverrideLocalCalculo { get; set; } = false;
        public string OrigenCalculo { get; set; } = "";
        public string Diagnostico { get; set; } = "";
    }

    public class ProyectoConsolidado
    {
        public string ProyectoId { get; set; } = "";
        public int TotalProductos { get; set; }
        public int TotalSubproductos { get; set; }
        public int TotalInstancias { get; set; }
        public decimal HorasProductivas { get; set; }
        public decimal HorasRevision { get; set; }
        public decimal HorasCorreccion { get; set; }
        public decimal HorasSupervision { get; set; }
        public decimal HorasDireccion { get; set; }
        public decimal HorasGestion { get; set; }
        public decimal CostoDirecto { get; set; }
        public decimal CostoTransversal { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal Precio { get; set; }
        public decimal Margen { get; set; }
        public decimal DuracionSemanas { get; set; }
        public List<ResumenConsolidado> PorGrupo { get; set; } = new List<ResumenConsolidado>();
        public List<ResumenConsolidado> PorProducto { get; set; } = new List<ResumenConsolidado>();
        public List<ResumenConsolidado> PorSubproducto { get; set; } = new List<ResumenConsolidado>();
        public List<ResumenConsolidado> PorCargo { get; set; } = new List<ResumenConsolidado>();
        public List<ResumenConsolidado> PorPersona { get; set; } = new List<ResumenConsolidado>();
        public List<string> Diagnosticos { get; set; } = new List<string>();
    }

    public class ResumenConsolidado
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal Cantidad { get; set; } = 0m;
        public decimal Horas { get; set; } = 0m;
        public decimal Costo { get; set; } = 0m;
        public decimal Precio { get; set; } = 0m;
        public decimal Margen { get; set; } = 0m;
    }
}
