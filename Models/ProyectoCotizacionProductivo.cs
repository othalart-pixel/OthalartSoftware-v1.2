using System;
using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class ProyectoCotizacion
    {
        public int SchemaVersion { get; set; } = 1;
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string MonedaId { get; set; } = "CLP";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;
        public List<GrupoProyecto> Grupos { get; set; } = new List<GrupoProyecto>();
        public List<ProcesoTransversalProyecto> ProcesosTransversales { get; set; } =
            new List<ProcesoTransversalProyecto>();
        public MetadataProyecto Metadata { get; set; } = new MetadataProyecto();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class MetadataProyecto
    {
        public string Origen { get; set; } = "";
        public string VersionAplicacion { get; set; } = "";
        public string NotasMigracion { get; set; } = "";
        public Dictionary<string, string> Valores { get; set; } = new Dictionary<string, string>();
    }

    public class GrupoProyecto
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Orden { get; set; } = 0;
        public TipoGrupoProyecto Tipo { get; set; } = TipoGrupoProyecto.General;
        public List<ItemProyecto> Items { get; set; } = new List<ItemProyecto>();
        public bool Activo { get; set; } = true;
    }

    public class ItemProyecto
    {
        public string Id { get; set; } = "";
        public TipoItemProyecto Tipo { get; set; } = TipoItemProyecto.Producto;
        public string BibliotecaId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Cantidad { get; set; } = 1m;
        public string Unidad { get; set; } = "";
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public SnapshotItem Snapshot { get; set; } = new SnapshotItem();
        public string CotizacionSnapshotJson { get; set; } = "";
        public DateTime? FechaEdicionSnapshot { get; set; } = null;
        public Dictionary<string, object> Parametros { get; set; } = new Dictionary<string, object>();
        public List<OverrideProductivo> Overrides { get; set; } = new List<OverrideProductivo>();
        public List<SubproductoProyecto> Subproductos { get; set; } = new List<SubproductoProyecto>();
        public List<ProcesoProyecto> Procesos { get; set; } = new List<ProcesoProyecto>();
    }

    public class ProductoProyecto : ItemProyecto
    {
        public ProductoProyecto()
        {
            Tipo = TipoItemProyecto.Producto;
        }
    }

    public class ServicioProyecto : ItemProyecto
    {
        public ServicioProyecto()
        {
            Tipo = TipoItemProyecto.Servicio;
        }
    }

    public class SnapshotItem
    {
        public string NombreBiblioteca { get; set; } = "";
        public string CategoriaBiblioteca { get; set; } = "";
        public string UnidadBase { get; set; } = "";
        public string JsonMinimo { get; set; } = "";
        public DateTime FechaSnapshot { get; set; } = DateTime.Now;
    }

    public class SubproductoProyecto
    {
        public string Id { get; set; } = "";
        public string ProductoProyectoId { get; set; } = "";
        public string SubproductoBibliotecaId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal Cantidad { get; set; } = 1m;
        public string Unidad { get; set; } = "";
        public ModoCantidadSubproducto ModoCantidad { get; set; } = ModoCantidadSubproducto.Homogeneo;
        public Dictionary<string, object> Parametros { get; set; } = new Dictionary<string, object>();
        public List<InstanciaSubproducto> Instancias { get; set; } = new List<InstanciaSubproducto>();
        public List<ProcesoProyecto> Procesos { get; set; } = new List<ProcesoProyecto>();
        public List<OverrideProductivo> Overrides { get; set; } = new List<OverrideProductivo>();
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } = 0;
        public string Notas { get; set; } = "";
    }

    public class InstanciaSubproducto
    {
        public string Id { get; set; } = "";
        public string SubproductoProyectoId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal CantidadEquivalente { get; set; } = 1m;
        public int Orden { get; set; } = 0;
        public Dictionary<string, object> Parametros { get; set; } = new Dictionary<string, object>();
        public List<ProcesoProyecto> Procesos { get; set; } = new List<ProcesoProyecto>();
        public List<AsignacionProductiva> Asignaciones { get; set; } = new List<AsignacionProductiva>();
        public List<OverrideProductivo> Overrides { get; set; } = new List<OverrideProductivo>();
        public string Estado { get; set; } = "Pendiente";
        public string Notas { get; set; } = "";
    }

    public class ProcesoProyecto
    {
        public string Id { get; set; } = "";
        public string ProcesoBibliotecaId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public TipoProcesoProductivo TipoProceso { get; set; } = TipoProcesoProductivo.NoClasificado;
        public MetodoCalculoProceso MetodoCalculo { get; set; } = MetodoCalculoProceso.NoDefinido;
        public AlcanceTemporalProceso AlcanceTemporal { get; set; } = AlcanceTemporalProceso.NoDefinido;
        public string EtapaId { get; set; } = "";
        public string SubetapaId { get; set; } = "";
        public decimal Cantidad { get; set; } = 0m;
        public string Unidad { get; set; } = "";
        public decimal Capacidad { get; set; } = 0m;
        public string Periodo { get; set; } = "";
        public bool Paralelo { get; set; } = false;
        public List<string> Dependencias { get; set; } = new List<string>();
        public List<AsignacionProductiva> Asignaciones { get; set; } = new List<AsignacionProductiva>();
        public ResultadoProcesoProyecto Resultado { get; set; } = new ResultadoProcesoProyecto();
        public bool Activo { get; set; } = true;
    }

    public class AsignacionProductiva
    {
        public string Id { get; set; } = "";
        public string ProcesoProyectoId { get; set; } = "";
        public string CargoId { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public decimal HorasCalculadas { get; set; } = 0m;
        public decimal HorasAsignadas { get; set; } = 0m;
        public OrigenHorasProductivas OrigenHoras { get; set; } = OrigenHorasProductivas.Calculado;
        public decimal DedicacionPorcentaje { get; set; } = 100m;
        public decimal CostoCalculado { get; set; } = 0m;
        public string Notas { get; set; } = "";
    }

    public class ResultadoProcesoProyecto
    {
        public decimal HorasCalculadas { get; set; } = 0m;
        public decimal HorasAsignadas { get; set; } = 0m;
        public decimal CostoCalculado { get; set; } = 0m;
        public decimal DuracionSemanas { get; set; } = 0m;
        public string Diagnostico { get; set; } = "";
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class OverrideProductivo
    {
        public string Id { get; set; } = "";
        public string Campo { get; set; } = "";
        public string ValorJson { get; set; } = "";
        public string Motivo { get; set; } = "";
        public AlcanceModificacion Alcance { get; set; } = AlcanceModificacion.ProductoProyecto;
        public EstadoPersonalizacionProyecto Estado { get; set; } = EstadoPersonalizacionProyecto.PersonalizadoProyecto;
        public string RutaElemento { get; set; } = "";
        public string BibliotecaId { get; set; } = "";
        public string ValorPlantillaJson { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
