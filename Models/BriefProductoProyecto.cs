using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Models
{
    public class BriefProductoProyecto
    {
        // =========================
        // CONTEXTO DEL CLIENTE
        // =========================

        public string IndustriaCliente { get; set; } = "";
        public string DestinoUso { get; set; } = "";
        public string PlataformaDestino { get; set; } = "";

        // =========================
        // TIPO DE PIEZA / PRODUCTO
        // =========================

        // Compatibilidad:
        // Ahora puede representar el producto principal o "Pedido mixto 2D".
        public string TipoSolicitudProducto { get; set; } = "";
        public string ProductoServicioSeleccionado { get; set; } = "";
        public string TipoProducto { get; set; } = "";
        public string TipoPiezaPrincipal { get; set; } = "";
        public string UsoPrincipal { get; set; } = "";

        // =========================
        // DURACION Y CANTIDAD GENERAL
        // =========================

        // Compatibilidad:
        // La duración real debe venir desde EntregableBrief.
        // Esto puede usarse como resumen o fallback.
        public double DuracionProductoValor { get; set; } = 0.0;
        public string DuracionProductoUnidad { get; set; } = "segundos";
        public double DuracionSolicitudValor { get; set; } = 0.0;
        public string DuracionSolicitudUnidad { get; set; } = "segundos";

        // Compatibilidad:
        // La cantidad real debe venir desde EntregableBrief.
        public int CantidadPiezas { get; set; } = 1;

        // Multiplicador operativo de entrada:
        // "quiero 2 personajes" aplica 2 a las filas del pipeline/producto
        // sin obligar a editar clean up, color, rough, etc. una por una.
        public int CantidadGlobalProducto { get; set; } = 1;

        // =========================
        // DIRECCION VISUAL
        // =========================

        public string EstiloVisual { get; set; } = "";
        public string ReferenciasVisuales { get; set; } = "";
        public string NivelAcabado { get; set; } = "";
        public List<string> DestinosUsoSeleccionados { get; set; } =
            new List<string>();

        // Estado visual de los selectores rápidos del formulario Datos.
        public string PresupuestoClienteModo { get; set; } = "Sin informar";
        public double PresupuestoClienteRapidoCLP { get; set; } = 0.0;
        public string MonedaClienteSeleccionadaRapida { get; set; } = "CLP";

        // =========================
        // ENTREGA TECNICA
        // =========================

        public string FormatoEntrega { get; set; } = "";
        public string RelacionAspecto { get; set; } = "";
        public string ResolucionEntrega { get; set; } = "";

        // =========================
        // PRODUCTOS 2D SOLICITADOS
        // =========================

        // Ejemplos:
        // 30 loops de personaje 2D de 2 segundos c/u.
        // 4 backgrounds 2D.
        // 12 props 2D.
        // 1 trailer 2D de 45 segundos.
        public List<EntregableBrief> EntregablesSeleccionados { get; set; } =
            new List<EntregableBrief>();

        public List<string> ClavesEntregablesSeleccionados { get; set; } =
            new List<string>();

        public List<Pieza2DSeleccionGuardada> Piezas2DSeleccionGuardadas { get; set; } =
            new List<Pieza2DSeleccionGuardada>();

        public bool SeleccionarTodosEntregablesActivo { get; set; } = false;

        public List<SubproductoBrief> SubproductosCalculados { get; set; } =
    new List<SubproductoBrief>();

        // =========================
        // INSUMOS ENTREGADOS POR CLIENTE
        // =========================

        public List<string> InsumosClienteSeleccionados { get; set; } =
            new List<string>();

        // Compatibilidad con lógica anterior.
        public bool ClienteEntregaGuion { get; set; } = false;
        public bool ClienteEntregaEstilo { get; set; } = false;
        public bool ClienteEntregaStoryboard { get; set; } = false;
        public bool ClienteEntregaAnimatic { get; set; } = false;
        public bool ClienteEntregaPersonajes { get; set; } = false;
        public bool ClienteEntregaFondos { get; set; } = false;
        public bool ClienteEntregaProps { get; set; } = false;
        public bool ClienteEntregaAnimacion { get; set; } = false;
        public bool ClienteEntregaAudio { get; set; } = false;
        public bool ClienteEntregaAssetsEditables { get; set; } = false;
        public bool ClienteEntregaMaterialGrabado { get; set; } = false;

        // =========================
        // FLAGS DERIVADOS / COMPATIBILIDAD
        // =========================

        // Estos ya no deberían preguntarse directamente al usuario.
        // Se derivan desde EntregablesSeleccionados.
        public bool RequierePersonajes { get; set; } = false;
        public bool RequiereFondos { get; set; } = false;
        public bool RequiereProps { get; set; } = false;
        public bool RequiereAnimacionPersonajes { get; set; } = false;
        public bool RequiereMotionGraphics { get; set; } = false;
        public bool RequiereEdicion { get; set; } = false;
        public bool RequiereAudio { get; set; } = false;
        public bool RequiereExportFinal { get; set; } = true;

        // =========================
        // NOTAS
        // =========================

        public string NotasBrief { get; set; } = "";
    }
}
