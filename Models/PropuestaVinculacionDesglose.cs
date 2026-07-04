using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class PropuestaVinculacionDesglose
    {
        public string Diagnostico { get; set; } = "";

        public double DiasPersonaTotales { get; set; } = 0.0;

        public List<PropuestaEtapaDesglose> Etapas { get; set; } =
            new List<PropuestaEtapaDesglose>();
    }

    public class PropuestaEtapaDesglose
    {
        public string Etapa { get; set; } = "";

        public double DiasPersona { get; set; } = 0.0;

        public double SemanasSugeridas { get; set; } = 0.0;

        public List<PropuestaSubEtapaDesglose> SubEtapas { get; set; } =
            new List<PropuestaSubEtapaDesglose>();
    }

    public class PropuestaSubEtapaDesglose
    {
        public string EtapaPadre { get; set; } = "";

        public string NombreSubEtapa { get; set; } = "";

        public double DiasPersona { get; set; } = 0.0;

        public double SemanasSugeridas { get; set; } = 0.0;

        public string Estado { get; set; } = "";

        public string Justificacion { get; set; } = "";

        public List<PropuestaRequerimientoVinculado> Requerimientos { get; set; } =
            new List<PropuestaRequerimientoVinculado>();
    }

    public class PropuestaRequerimientoVinculado
    {
        public string EntregableCliente { get; set; } = "";

        public string TipoInterno { get; set; } = "";

        public string NombreRequerimiento { get; set; } = "";

        public double Cantidad { get; set; } = 0.0;

        public string Unidad { get; set; } = "";

        public string CargoSugerido { get; set; } = "";

        public double DiasPersona { get; set; } = 0.0;
    }
}