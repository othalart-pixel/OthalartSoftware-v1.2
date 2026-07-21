namespace Cotizador_animacion_Othalart.Models
{
    public class RequerimientoProduccionInterna
    {
        public string EntregableCliente { get; set; } = "";
        public string CategoriaEntregable { get; set; } = "";

        public string EcuacionUsada { get; set; } = "";
        public string ProyectoId { get; set; } = "";
        public string GrupoId { get; set; } = "";
        public string ItemId { get; set; } = "";
        public string SubproductoProyectoId { get; set; } = "";
        public string InstanciaId { get; set; } = "";
        public string ProcesoId { get; set; } = "";
        public TipoProcesoProductivo TipoProceso { get; set; } = TipoProcesoProductivo.NoClasificado;
        public MetodoCalculoProceso MetodoCalculo { get; set; } = MetodoCalculoProceso.NoDefinido;
        public AlcanceTemporalProceso AlcanceTemporal { get; set; } = AlcanceTemporalProceso.NoDefinido;
        public string CargoId { get; set; } = "";
        public string PersonaId { get; set; } = "";
        public string DependenciasProcesoJson { get; set; } = "";
        public bool PuedeEjecutarseEnParalelo { get; set; } = false;

        public string TipoInterno { get; set; } = "";
        public string NombreRequerimiento { get; set; } = "";

        public double Cantidad { get; set; } = 0.0;
        public string Unidad { get; set; } = "";

        public string EtapaSugerida { get; set; } = "";
        public string Calidad { get; set; } = "Estándar";

        // =========================
        // PLANIFICACIÓN PRODUCTIVA
        // =========================

        public string BloqueProductivo { get; set; } = "";
        public string ModoPlanificacion { get; set; } = "Secuencial";
        public string DependeDe { get; set; } = "";

        // =========================
        // CARGO ASOCIADO
        // =========================

        public string CargoSugerido { get; set; } = "";
        public string NivelCargoSugerido { get; set; } = "típico";
        public string AreaCargoSugerida { get; set; } = "";

        public double SueldoMensualCargoCLP { get; set; } = 0.0;
        public double TarifaDiaCargoCLP { get; set; } = 0.0;

        // =========================
        // ESFUERZO
        // =========================

        public double RendimientoCantidad { get; set; } = 0.0;
        public string RendimientoPeriodo { get; set; } = "";
        public string RendimientoOrigen { get; set; } = "";
        public string ModoCalculoProductivo { get; set; } = "Rendimiento";
        public double HorasMinimas { get; set; } = 0.0;
        public double HorasEstandar { get; set; } = 0.0;
        public double HorasHolgura { get; set; } = 0.0;
        public string OrigenHoras { get; set; } = "";

        public double DiasPersonaMin { get; set; } = 0.0;
        public double DiasPersonaStd { get; set; } = 0.0;
        public double DiasPersonaHolgura { get; set; } = 0.0;

        // =========================
        // COSTO CALCULADO DESDE CARGO
        // =========================

        public double CostoMinimoCLP { get; set; } = 0.0;
        public double CostoEstandarCLP { get; set; } = 0.0;
        public double CostoHolguraCLP { get; set; } = 0.0;

        public bool ParametrosCompletos { get; set; } = true;

        public string DiagnosticoParametros { get; set; } = "";

        public bool EditadoManualmente { get; set; } = false;

        public bool TieneOverrideLocalCalculo { get; set; } = false;
        public string CargosParticipantesOverrideJson { get; set; } = "";
        public double RendimientoCantidadOverride { get; set; } = 0.0;
        public string RendimientoPeriodoOverride { get; set; } = "";

        public string Nota { get; set; } = "";
    }
}
