using System.Collections.Generic;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        // =========================
        // Moneda visual dependiente de tabla
        // =========================

        private Label lblMonedaVisualizacionActual;
        private bool monedaVisualDesdeTablaInicializada = false;
        private bool actualizandoMonedaVisualDesdeTabla = false;

        private EtapaProyecto? etapaExpandidaEnTabla = null;

        private FlowLayoutPanel panelMonedasClienteRapidas = new FlowLayoutPanel();
        private string monedaClienteSeleccionadaRapida = "CLP";
        private bool modoOscuroActivo = false;

        private enum ModoAplicacion
        {
            Inicio,
            Proyecto,
            Configuracion
        }

        private sealed class EstadoNavegacionPrincipal
        {
            public ModoAplicacion Modo { get; set; }
            public TabPage Tab { get; set; }
        }

        private ModoAplicacion modoAplicacionActual = ModoAplicacion.Inicio;
        private readonly List<EstadoNavegacionPrincipal> historialNavegacionPrincipal =
            new List<EstadoNavegacionPrincipal>();
        private EstadoNavegacionPrincipal estadoNavegacionActual;
        private bool navegandoDesdeHistorial = false;
        private Button btnVolverNavegacion = new Button();
        private Label lblModoAplicacion = new Label();
        private Label lblEstadoGuardadoGlobal = new Label();
        private bool proyectoTieneCambiosPendientes = false;
        private Button btnAlternarPanelLateral = new Button();
        private Panel panelContenidoLateralDerecho = new Panel();
        private Button btnPestanaPanelDerecho = new Button();
        private ToolTip tooltipPanelDerecho = new ToolTip();
        private bool panelLateralCatalogoVisible = true;
        private const int AnchoPestanaPanelDerecho = 32;
        private const int AnchoMinimoPanelDerechoAbierto = 180;
        private const int AnchoInicialPanelDerechoAbierto = 420;
        private int ultimoAnchoPanelDerechoAbierto = AnchoInicialPanelDerechoAbierto;
        private bool anchoPanelDerechoDefinidoPorUsuario = false;
        private bool moviendoSplitterPanelDerecho = false;
        private bool ajustandoSplitterPanelDerechoProgramaticamente = false;
        private enum EstadoPanelDerechoProyecto
        {
            Normal,
            Oculto,
            Expandido,
            Contraido
        }

        private EstadoPanelDerechoProyecto estadoPanelDerechoProyecto = EstadoPanelDerechoProyecto.Oculto;

        private class SubEtapaPreviewRow
        {
            public EtapaProyecto EtapaPadre { get; set; }
            public SubEtapaProyecto SubEtapa { get; set; }
            public int SemanaInicio { get; set; }
            public int SemanaFin { get; set; }

            public SubEtapaPreviewRow(
                EtapaProyecto etapaPadre,
                SubEtapaProyecto subEtapa,
                int semanaInicio,
                int semanaFin
            )
            {
                EtapaPadre = etapaPadre;
                SubEtapa = subEtapa;
                SemanaInicio = semanaInicio;
                SemanaFin = semanaFin;
            }
        }

        private Panel panelPreviewSubEtapas = new Panel();
        private Label lblPreviewSubEtapasTitulo = new Label();
        private ListBox lstPreviewSubEtapas = new ListBox();
        private Button btnIrSubEtapasDesdeEtapas = new Button();
        private DataGridView dgvPreviewSubEtapas = new DataGridView();
        private Panel panelPreviewSubGantt = new Panel();
        private EtapaProyecto? etapaSeleccionadaParaSubGantt = null;

        private DataGridView dgvSubEtapas = new DataGridView();
        private DataGridView dgvRangosSubEtapas = new DataGridView();
        private DataGridView dgvBibliotecaEtapas = new DataGridView();
        private ComboBox cmbFiltroEtapaSubEtapas = new ComboBox();
        private Button btnAplicarSubEtapas = new Button();
        private Button btnRestaurarSubEtapas = new Button();
        private Button btnGuardarBibliotecaEtapas = new Button();
        private Button btnRestaurarBibliotecaEtapas = new Button();
        private Button btnGuardarRangosSubEtapas = new Button();
        private Button btnRestaurarRangosSubEtapas = new Button();

        private List<EtapaDefinicion> bibliotecaEtapas =
            new List<EtapaDefinicion>();

        private List<SubEtapaProyecto> bibliotecaSubEtapas =
            new List<SubEtapaProyecto>();

        private List<CategoriaTrabajador> bibliotecaCargosGenerales =
            new List<CategoriaTrabajador>();

        private List<CategoriaTrabajador> bibliotecaCargosProyectoInforme = null;
        private List<PersonaEquipo> bibliotecaPersonalProyectoInforme = null;

        private const string NombreOpcionCargosGenerales = "General / todas las etapas";

        private class SelectorBibliotecaCargos
        {
            public string Nombre { get; set; } = "";
            public bool EsGeneral { get; set; }
            public EtapaProyecto? Etapa { get; set; }

            public override string ToString()
            {
                return Nombre;
            }
        }

        private Panel panelSiguientePasoMoneda = new Panel();
        private Label lblSiguientePasoMoneda = new Label();

        private Button btnMonedaIrEtapas = new Button();

        // =========================
        // Resumen lateral vivo
        // =========================

        private Panel panelResumenScroll = new Panel();
        private RichTextBox rtbResumen = new RichTextBox();
        private TableLayoutPanel layoutPanelDerecho = new TableLayoutPanel();
        private TableLayoutPanel layoutResumenDerecho = new TableLayoutPanel();

        private System.Windows.Forms.Timer timerResumenFeedback = new System.Windows.Forms.Timer();
        private int pasosFeedbackResumen = 0;
        private bool feedbackResumenConfigurado = false;

        // =========================
        // Easter egg / teclado
        // =========================

        private HashSet<Keys> teclasPresionadas = new HashSet<Keys>();

        // =========================
        // Moneda / tipos de cambio
        // =========================

        private ComboBox cmbMonedaVisualizacion = new ComboBox();
        private DataGridView dgvTiposCambio = new DataGridView();
        private Button btnActualizarTiposCambio = new Button();
        private Button btnAplicarTiposCambio = new Button();
        private Label lblEstadoTiposCambio = new Label();
        private Label lblMonedaClienteActual = new Label();

        // =========================
        // Costos extra
        // =========================

        private ComboBox cmbMonedaCostoExtra = new ComboBox();

        private ComboBox cmbCategoriaCostoExtra = new ComboBox();
        private TextBox txtDescripcionCostoExtra = new TextBox();
        private TextBox txtMontoCostoExtra = new TextBox();
        private ComboBox cmbPeriodicidadCostoExtra = new ComboBox();
        private CheckBox chkCostoVisibleCliente = new CheckBox();

        private Button btnAgregarCostoExtra = new Button();
        private Button btnEliminarCostoExtra = new Button();

        private DataGridView dgvCostosExtra = new DataGridView();
        private Label lblTotalCostosExtra = new Label();

        // =========================
        // Biblioteca de cargos
        // =========================

        private DataGridView dgvBibliotecaCargos = new DataGridView();
        private ComboBox cmbEtapaBibliotecaCargos = new ComboBox();

        private Button btnAgregarBibliotecaCargo = new Button();
        private Button btnEliminarBibliotecaCargo = new Button();
        private Button btnAplicarBibliotecaCargos = new Button();

        // =========================
        // Datos base / estado general
        // =========================

        private const int MaxMesesTabla = 24;

        private Cotizacion cotizacion = new Cotizacion();

        private SplitContainer splitPrincipal = new SplitContainer();
        private TabControl tabs = new TabControl();
        private TabPage tabInicioPrincipal;
        private TabPage tabCatalogoProductosServiciosPrincipal;
        private TabPage tabProyectoPrincipal;
        private TabPage tabDatosPrincipal;
        private TabPage tabProductosPrincipal;
        private TabPage tabMonedaPrincipal;
        private TabPage tabDesgloseProductivoPrincipal;
        private TabPage tabRendimientosPrincipal;
        private TabPage tabEcuacionesPrincipal;
        private TabPage tabGestionesPrincipal;
        private TabPage tabValidacionJsonPrincipal;
        private TabPage tabPersonalPrincipal;
        private TabPage tabSubEtapasPrincipal;
        private TabPage tabRangosPrincipal;
        private TabPage tabCargosPrincipal;
        private TabPage tabManoObraPrincipal;
        private TabPage tabCostosPrincipal;
        private TabPage tabResultadosPrincipal;
        private TabPage tabInformePrincipal;
        private TabPage tabGuardarPrincipal;
        private TabPage tabAdministrativaActiva;
        private Panel panelBarraSuperiorTema = new Panel();
        private Button btnGuardarProyectoRapido = new Button();
        private Button btnCargarProyectoRapido = new Button();
        private Button btnAlternarModoOscuro = new Button();

        private DataGridView dgvValidacionJson = new DataGridView();
        private RichTextBox rtbValidacionJsonResumen = new RichTextBox();

        private DataGridView dgvPersonalEmpresa = new DataGridView();
        private TextBox txtPersonalId = new TextBox();
        private TextBox txtPersonalNombre = new TextBox();
        private ComboBox cmbPersonalCargoPrincipal = new ComboBox();
        private CheckedListBox clbPersonalCargosPosibles = new CheckedListBox();
        private CheckedListBox clbPersonalTrabajosPosibles = new CheckedListBox();
        private NumericUpDown nudPersonalPagoInterno = new NumericUpDown();
        private ComboBox cmbPersonalPeriodoPago = new ComboBox();
        private NumericUpDown nudPersonalHorasSemana = new NumericUpDown();
        private Label lblPersonalCostoHoraCalculado = new Label();
        private NumericUpDown nudPersonalCostoHora = new NumericUpDown();
        private NumericUpDown nudPersonalTarifaHora = new NumericUpDown();
        private NumericUpDown nudPersonalHorasMaximas = new NumericUpDown();
        private CheckBox chkPersonalActivo = new CheckBox();
        private TextBox txtPersonalNotas = new TextBox();
        private Label lblRutaPersonalEmpresa = new Label();
        private bool cargandoPersonalEmpresa = false;

        private bool cargandoTabla = false;
        private bool cargandoCostos = false;
        private bool cambiandoTabInternamente = false;
        private bool formateandoNumero = false;

        // =========================
        // Datos cliente / proyecto
        // =========================

        private TextBox txtNombreCliente = new TextBox();
        private TextBox txtEmpresa = new TextBox();
        private TextBox txtEmail = new TextBox();
        private TextBox txtNombreProyecto = new TextBox();
        private TextBox txtDescripcion = new TextBox();
        private TextBox txtNotas = new TextBox();
        private ComboBox cmbMoneda = new ComboBox();

        private Panel panelSiguientePasoDatos = new Panel();
        private Label lblSiguientePasoDatos = new Label();
        private Button btnDatosIrMoneda = new Button();
        private Button btnDatosIrEtapas = new Button();

        // =========================
        // Etapas / mano de obra
        // =========================

        private DataGridView dgvEtapas = new DataGridView();
        private DataGridView dgvManoObra = new DataGridView();
        private DataGridView dgvAsignacionManoObra = new DataGridView();
        private ComboBox cmbFiltroAsignacionEtapa = new ComboBox();
        private ComboBox cmbFiltroAsignacionCargo = new ComboBox();
        private ComboBox cmbFiltroAsignacionPersona = new ComboBox();
        private NumericUpDown nudHorasTandaGlobalManoObra = new NumericUpDown();
        private Label lblResumenAsignacionManoObra = new Label();

        private ComboBox cmbEtapa = new ComboBox();
        private ComboBox cmbCargo = new ComboBox();

        private Button btnAgregarCargo = new Button();
        private Button btnEliminarCargo = new Button();
        private Button btnRellenarManoObraDesdeDesglose = new Button();
        private Button btnRecalcular = new Button();

        // =========================
        // Costos principales
        // =========================

        private TextBox txtCostoTercerizados = new TextBox();
        private TextBox txtOtrosCostos = new TextBox();
        private TextBox txtTasaImprevistos = new TextBox();

        // =========================
        // Resultados / márgenes
        // =========================

        private TextBox txtMargenObjetivo = new TextBox();
        private TextBox txtPrecioEvaluado = new TextBox();
        private ComboBox cmbPaisImpuestoVenta = new ComboBox();
        private TextBox txtTasaImpuestoVenta = new TextBox();
        private Label lblResultadosDetalle = new Label();

        private Panel panelGraficoMargen = new Panel();
        private DataGridView dgvAnalisisMargen = new DataGridView();
        private Label lblAnalisisMargen = new Label();

        private RadioButton rbModoMargen = new RadioButton();
        private RadioButton rbModoPrecio = new RadioButton();

        // =========================
        // Informe / guardado
        // =========================

        private WebBrowser webInformeCliente = new WebBrowser();

        private Button btnActualizarInforme = new Button();
        private Button btnGuardarInformeHtml = new Button();
        private Button btnGuardarExcelInterno = new Button();
        private Label lblGuardarEstado = new Label();

        // =========================
        // Panel derecho
        // =========================

        private Panel panelGantt = new Panel();

        // =========================
        // Tags internos
        // =========================

        private class ManoObraRowTag
        {
            public EtapaProyecto Etapa { get; set; }
            public CargoPlanMensual Cargo { get; set; }

            public ManoObraRowTag(EtapaProyecto etapa, CargoPlanMensual cargo)
            {
                Etapa = etapa;
                Cargo = cargo;
            }
        }
    }
}
