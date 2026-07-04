using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class CargoPlanMensual
    {
        public CategoriaTrabajador Categoria { get; set; } = new CategoriaTrabajador();

        // Persona o recurso asignado.
        // Puede ser "Por definir", "Mario", "Micael", "Freelancer 1", etc.
        public string NombrePersona { get; set; } = "Por definir";

        // Valor mensual editable para esta persona/recurso.
        // Por defecto se copia desde el valor típico del cargo.
        public double SueldoMensualCLPEditable { get; set; }
        public double SueldoMensualUSDEditable { get; set; }

        // Dedicación por mes del proyecto.
        // 1.0 = jornada completa ese mes
        // 0.5 = media jornada
        // 0.25 = cuarto de jornada
        // 2.0 = dos jornadas equivalentes
        public List<double> PersonasPorBloque { get; set; } = new List<double>();

        // Trazabilidad desde Desglose productivo.
        // Permite explicar por qué este cargo aparece en Mano de obra y qué labores lo alimentan.
        public List<string> ActividadesDesglose { get; set; } = new List<string>();
        public double HorasRequeridasDesdeDesglose { get; set; }
        public double DiasPersonaDesdeDesglose { get; set; }

        public double PersonaMesTotal { get; set; }
        public double CostoTotal { get; set; }
    }
}
