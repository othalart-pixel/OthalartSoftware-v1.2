namespace Cotizador_animacion_Othalart.Models
{
    public class RendimientoProductivo
    {
        public int Id { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public string Etapa { get; set; } = "";
        public string TipoInterno { get; set; } = "";
        public string Proceso { get; set; } = "";
        public string Unidad { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string NivelCargo { get; set; } = "";
        public double CantidadMinimaPorPeriodo { get; set; } = 0.0;
        public double CantidadPorPeriodo { get; set; } = 0.0;
        public double CantidadMaximaPorPeriodo { get; set; } = 0.0;
        public string Periodo { get; set; } = "semana";
        public string Nota { get; set; } = "";
    }
}
