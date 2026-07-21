using System;
using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class Cotizacion
    {
        // =========================
        // MONEDA Y TIPO DE CAMBIO
        // =========================

        public string MonedaBase { get; set; } = "CLP";

        public string Moneda { get; set; } = "CLP";

        public string MonedaVisualizacion { get; set; } = "CLP";

        public string MonedaPrecioCliente { get; set; } = "CLP";

        public List<TipoCambio> TiposCambio { get; set; } = new List<TipoCambio>();

        // =========================
        // DATOS GENERALES
        // =========================

        public string NombreCliente { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Email { get; set; } = "";
        public string NombreProyecto { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Notas { get; set; } = "";
        public string NombreArchivo { get; set; } = "";

        // =========================
        // BRIEF DEL PRODUCTO
        // =========================

        public BriefProductoProyecto BriefProducto { get; set; } = new BriefProductoProyecto();

        // =========================
        // ETAPAS DEL PROYECTO
        // =========================

        public List<EtapaProyecto> Etapas { get; set; } = new List<EtapaProyecto>();

        public double AcumuladoMeses { get; set; }

        // =========================
        // DESGLOSE PRODUCTIVO INTERNO
        // =========================

        public DesgloseProductivoProyecto DesgloseProductivo { get; set; }
            = new DesgloseProductivoProyecto();

        public ProyectoCotizacion ProyectoProductivo { get; set; } = null;

        public double HorasProyectoProductivo { get; set; } = 0.0;

        public double CostoProyectoProductivoCLP { get; set; } = 0.0;

        // =========================
        // PLAZO DECLARADO POR CLIENTE
        // =========================

        public DateTime? FechaInicioCliente { get; set; } = null;

        public DateTime? FechaEntregaCliente { get; set; } = null;

        public double PlazoClienteSemanas
        {
            get
            {
                if (!FechaInicioCliente.HasValue || !FechaEntregaCliente.HasValue)
                {
                    return 0.0;
                }

                if (FechaEntregaCliente.Value.Date <= FechaInicioCliente.Value.Date)
                {
                    return 0.0;
                }

                return (FechaEntregaCliente.Value.Date - FechaInicioCliente.Value.Date).TotalDays / 7.0;
            }
        }

        // =========================
        // EVALUACIÓN INTERNA DE PLAZO
        // =========================

        public EvaluacionPlazoProyecto EvaluacionPlazo { get; set; }
            = new EvaluacionPlazoProyecto();

        public double DuracionPlanificadaSemanas { get; set; } = 0.0;

        public double DuracionMinimaTecnicaSemanas { get; set; } = 0.0;

        public double DuracionEstandarTecnicaSemanas { get; set; } = 0.0;

        public double DuracionHolguraTecnicaSemanas { get; set; } = 0.0;

        public double DiasHabilesEstudioPorSemana { get; set; } = 5.0;

        public string DiagnosticoPlazo { get; set; } = "";

        public double FactorPresionPlazo { get; set; } = 1.0;

        // =========================
        // PLAN GENERAL DE MANO DE OBRA
        // =========================

        public List<CargoPlanMensual> PlanGeneralManoObra { get; set; }
            = new List<CargoPlanMensual>();

        public List<AsignacionManoObraProyecto> AsignacionesManoObra { get; set; }
            = new List<AsignacionManoObraProyecto>();

        // =========================
        // COSTOS DE MANO DE OBRA
        // =========================

        public double CostoManoObraEtapas { get; set; }

        public double CostoManoObraGeneral { get; set; }

        public double CostoManoObraTotal { get; set; }

        public double PersonaMesTotal { get; set; }

        public double PersonaMesEtapas { get; set; }

        public double PersonaMesGeneral { get; set; }

        // =========================
        // COSTOS INTERNOS AUTOMÁTICOS
        // =========================

        public double CostoProduccionInterna { get; set; }

        public double CostoAdministrativo { get; set; }

        // =========================
        // COSTOS MANUALES / EXTRA
        // =========================

        public double CostoTercerizados { get; set; }

        public double OtrosCostos { get; set; }

        public List<CostoExtra> CostosExtra { get; set; } = new List<CostoExtra>();

        // =========================
        // PRESUPUESTO DECLARADO POR CLIENTE
        // =========================

        public decimal PresupuestoCliente { get; set; } = 0m;

        public string MonedaPresupuestoCliente { get; set; } = "CLP";

        // =========================
        // COSTO TOTAL
        // =========================

        public double TasaImprevistos { get; set; }

        public double CostoBase { get; set; }

        public double Imprevistos { get; set; }

        public double CostoTotal { get; set; }

        // =========================
        // PRECIO Y RENTABILIDAD
        // =========================

        public double MargenObjetivo { get; set; }

        public double PrecioRecomendado { get; set; }

        public double PrecioVentaEvaluado { get; set; }

        public string PaisImpuestoVenta { get; set; } = "Chile";

        public double TasaImpuestoVenta { get; set; } = 0.19;

        public double ImpuestoVenta { get; set; }

        public double PrecioVentaConImpuesto { get; set; }

        public double UtilidadEvaluada { get; set; }

        public double MargenEvaluado { get; set; }

        public double MarkupEvaluado { get; set; }
    }
}
