using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public enum TipoProcesoProductivo
    {
        NoClasificado,
        ProduccionDirecta,
        RevisionControl,
        CorreccionRetrabajo,
        Supervision,
        Direccion,
        GestionCoordinacion,
        EntregaSoporte
    }

    public enum MetodoCalculoProceso
    {
        NoDefinido,
        PorCantidad,
        PorDuracionEntregable,
        PorCapacidad,
        PorEvento,
        PorRevision,
        PorPorcentajeProduccion,
        PorDuracionEtapa,
        PorDuracionProyecto,
        Fijo,
        Manual
    }

    public enum AlcanceTemporalProceso
    {
        NoDefinido,
        Instancia,
        Subproducto,
        Producto,
        Grupo,
        Item,
        Etapa,
        MultiplesEtapas,
        ProyectoCompleto
    }

    public enum TipoGrupoProyecto
    {
        General,
        Preproduccion,
        Produccion,
        Postproduccion,
        DireccionGestion,
        Entrega,
        Personalizado
    }

    public enum TipoItemProyecto
    {
        Producto,
        Servicio
    }

    public enum ModoCantidadSubproducto
    {
        Homogeneo,
        InstanciasIndividuales
    }

    public enum OrigenHorasProductivas
    {
        Calculado,
        Recomendado,
        Manual
    }

    public enum TipoReglaActivacionProceso
    {
        Manual,
        Siempre,
        ProcesoExistente,
        TipoItem,
        Etapa,
        CantidadMinima,
        DuracionMinima,
        Condicion
    }

    public enum ReglaProrrateoProceso
    {
        SinProrratear,
        PorHorasProductivas,
        PorCostoDirecto,
        PorDuracion,
        PorCantidad,
        Manual
    }

    public class ReglaActivacionProceso
    {
        public TipoReglaActivacionProceso Tipo { get; set; } = TipoReglaActivacionProceso.Manual;
        public string ProcesoOrigenId { get; set; } = "";
        public string TipoItem { get; set; } = "";
        public string EtapaId { get; set; } = "";
        public double CantidadMinima { get; set; } = 0.0;
        public double DuracionMinima { get; set; } = 0.0;
        public string Condicion { get; set; } = "";
    }

    public class ProcesoTransversalProyecto
    {
        public string Id { get; set; } = "";
        public string ProcesoBibliotecaId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public TipoProcesoProductivo TipoProceso { get; set; } = TipoProcesoProductivo.NoClasificado;
        public AlcanceTemporalProceso AlcanceTemporal { get; set; } = AlcanceTemporalProceso.NoDefinido;
        public List<string> EtapasCubiertas { get; set; } = new List<string>();
        public string FechaInicio { get; set; } = "";
        public string FechaFin { get; set; } = "";
        public double SemanasActivas { get; set; } = 0.0;
        public double HorasPorSemana { get; set; } = 0.0;
        public double PorcentajeDedicacion { get; set; } = 100.0;
        public string CargoId { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public double HorasCalculadas { get; set; } = 0.0;
        public double HorasAsignadas { get; set; } = 0.0;
        public string OrigenHoras { get; set; } = "";
        public ReglaProrrateoProceso ReglaProrrateo { get; set; } = ReglaProrrateoProceso.SinProrratear;
    }
}
