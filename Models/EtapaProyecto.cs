using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class EtapaProyecto
    {
        public string Nombre { get; set; } = "";

        public bool Seleccionada { get; set; }
        public bool UsaPlanDetallado { get; set; }

        public double DuracionMeses { get; set; }
        public double InicioMes { get; set; }
        public double FinMes { get; set; }

        public List<CategoriaTrabajador> Biblioteca { get; set; } = new List<CategoriaTrabajador>();
        public List<CargoPlanMensual> Plan { get; set; } = new List<CargoPlanMensual>();

        public double CantidadPromedioPersonas { get; set; }
        public double SueldoPromedioMensual { get; set; }

        public double PersonaMesTotal { get; set; }
        public double CostoTotal { get; set; }
    }
}