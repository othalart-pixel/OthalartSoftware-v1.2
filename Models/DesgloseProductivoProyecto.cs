using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class DesgloseProductivoProyecto
    {
        public List<RequerimientoProduccionInterna> Requerimientos { get; set; }
            = new List<RequerimientoProduccionInterna>();

        public double DiasPersonaMinimos { get; set; } = 0.0;
        public double DiasPersonaEstandar { get; set; } = 0.0;
        public double DiasPersonaHolgura { get; set; } = 0.0;

        public double SemanasMinimas { get; set; } = 0.0;
        public double SemanasEstandar { get; set; } = 0.0;
        public double SemanasHolgura { get; set; } = 0.0;

        public double CostoMinimoCLP { get; set; } = 0.0;
        public double CostoEstandarCLP { get; set; } = 0.0;
        public double CostoHolguraCLP { get; set; } = 0.0;

        public double HorasGestionEstandar { get; set; } = 0.0;
        public double CostoGestionEstandarCLP { get; set; } = 0.0;

        public double HorasProduccionDirecta { get; set; } = 0.0;
        public double HorasRevisionControl { get; set; } = 0.0;
        public double HorasCorreccionRetrabajo { get; set; } = 0.0;
        public double HorasSupervision { get; set; } = 0.0;
        public double HorasDireccion { get; set; } = 0.0;
        public double HorasGestionCoordinacion { get; set; } = 0.0;
        public double HorasEntregaSoporte { get; set; } = 0.0;

        public double CostoProduccionDirectaCLP { get; set; } = 0.0;
        public double CostoRevisionControlCLP { get; set; } = 0.0;
        public double CostoCorreccionRetrabajoCLP { get; set; } = 0.0;
        public double CostoSupervisionCLP { get; set; } = 0.0;
        public double CostoDireccionCLP { get; set; } = 0.0;
        public double CostoGestionCoordinacionCLP { get; set; } = 0.0;
        public double CostoEntregaSoporteCLP { get; set; } = 0.0;
        public double CostoDirectoCLP { get; set; } = 0.0;
        public double CostoIndirectoTransversalCLP { get; set; } = 0.0;

        public List<GestionProductivaCalculada> GestionesCalculadas { get; set; }
            = new List<GestionProductivaCalculada>();

        public List<ProcesoTransversalProyecto> ProcesosTransversales { get; set; }
            = new List<ProcesoTransversalProyecto>();

        public string Diagnostico { get; set; } = "";
    }

    public class GestionProductivaCalculada
    {
        public string Area { get; set; } = "";
        public string Cargo { get; set; } = "";
        public string NivelCargo { get; set; } = "típico";
        public string Descripcion { get; set; } = "";

        public double DiasPersonaMinimosAsociados { get; set; } = 0.0;
        public double DiasPersonaEstandarAsociados { get; set; } = 0.0;
        public double DiasPersonaHolguraAsociados { get; set; } = 0.0;

        public double HorasMinimas { get; set; } = 0.0;
        public double HorasEstandar { get; set; } = 0.0;
        public double HorasHolgura { get; set; } = 0.0;

        public double TarifaHoraCLP { get; set; } = 0.0;

        public double CostoMinimoCLP { get; set; } = 0.0;
        public double CostoEstandarCLP { get; set; } = 0.0;
        public double CostoHolguraCLP { get; set; } = 0.0;
    }
}
