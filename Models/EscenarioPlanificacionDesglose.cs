namespace Cotizador_animacion_Othalart.Models
{
    public enum EscenarioPlanificacionDesglose
    {
        Minimo,
        Estandar,
        Holgado,
        Recomendado
    }

    public class ResultadoEscenarioOfertaDesglose
    {
        public double MargenBase { get; set; } = 0.15;

        public double PresupuestoClienteCLP { get; set; } = 0.0;

        public double CostoMinimoCLP { get; set; } = 0.0;
        public double CostoEstandarCLP { get; set; } = 0.0;
        public double CostoHolgadoCLP { get; set; } = 0.0;

        public double PrecioMinimoCLP { get; set; } = 0.0;
        public double PrecioEstandarCLP { get; set; } = 0.0;
        public double PrecioHolgadoCLP { get; set; } = 0.0;

        public EscenarioPlanificacionDesglose EscenarioRecomendado { get; set; }
            = EscenarioPlanificacionDesglose.Estandar;

        public string Diagnostico { get; set; } = "";
    }
}