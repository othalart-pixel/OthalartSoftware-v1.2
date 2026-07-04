using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class PersonaEquipo
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string CargoPrincipal { get; set; } = "";
        public List<string> CargosPosibles { get; set; } = new List<string>();
        public List<string> TrabajosPosibles { get; set; } = new List<string>();
        public decimal PagoInterno { get; set; }
        public string PeriodoPago { get; set; } = "Mensual";
        public decimal HorasTrabajoSemana { get; set; } = 42m;
        public decimal CostoHora { get; set; }
        public decimal TarifaHora { get; set; }
        public decimal HorasMaximasPorTanda { get; set; } = 42m;
        public bool Activo { get; set; } = true;
        public string Notas { get; set; } = "";
    }
}
