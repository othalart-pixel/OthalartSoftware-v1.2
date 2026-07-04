namespace Cotizador_animacion_Othalart.Models
{
    public class RequerimientoProduccionInterna
    {
        public string EntregableCliente { get; set; } = "";
        public string CategoriaEntregable { get; set; } = "";

        public string EcuacionUsada { get; set; } = "";

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

        public string Nota { get; set; } = "";
    }
}
