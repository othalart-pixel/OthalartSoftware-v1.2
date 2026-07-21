using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private TextBox txtBuscarCatalogoProductos = new TextBox();
        private ComboBox cmbCategoriaCatalogoProductos = new ComboBox();
        private ListBox lstCatalogoProductos = new ListBox();
        private Label lblCatalogoNombreProducto = new Label();
        private Label lblCatalogoMetaProducto = new Label();
        private Label lblCatalogoDescripcionProducto = new Label();
        private FlowLayoutPanel panelCatalogoKpis = new FlowLayoutPanel();
        private DataGridView dgvCatalogoProcesos = new DataGridView();
        private FlowLayoutPanel panelCatalogoEtapas = new FlowLayoutPanel();
        private TableLayoutPanel panelConfiguracionCotizacionCatalogo = new TableLayoutPanel();
        private Label lblResumenSimulacionCatalogo = new Label();
        private NumericUpDown nudCantidadSimulacionCatalogo = new NumericUpDown();
        private ComboBox cmbUnidadComercialCatalogo = new ComboBox();
        private List<Producto2DDefinicion> productosCatalogo = new List<Producto2DDefinicion>();
        private Producto2DDefinicion productoCatalogoVistaPrevia = null;
        private CatalogoProductoPreview previewCatalogoActual = null;
        private ProductQuoteConfiguration configuracionCatalogoActual = null;
        private Dictionary<string, Control> controlesParametrosCatalogo =
            new Dictionary<string, Control>();
        private bool cargandoConfiguracionCatalogo = false;
        private System.Windows.Forms.Timer temporizadorRecalculoCatalogo = new System.Windows.Forms.Timer();
        private int indiceArrastreProcesoCatalogo = -1;
        private Rectangle rectArrastreProcesoCatalogo = Rectangle.Empty;
        private bool cargandoCatalogoProductos = false;

        private void ConstruirTabCatalogoProductosServicios(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = ObtenerFondoCatalogo();
            tab.Padding = new Padding(16);

            productosCatalogo = BibliotecaProductos2DJsonService.CargarProductos()
                .Where(p => p != null)
                .OrderBy(p => p.Nombre)
                .ToList();

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.BackColor = ObtenerFondoCatalogo();

            Label titulo = new Label();
            titulo.Text = "Catalogo de productos y servicios";
            titulo.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            titulo.ForeColor = ObtenerTextoPrincipalCatalogo();
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 12);
            root.Controls.Add(titulo, 0, 0);
            root.SetColumnSpan(titulo, 2);

            root.Controls.Add(CrearPanelListadoCatalogo(), 0, 1);
            root.Controls.Add(CrearPanelDetalleCatalogo(), 1, 1);

            tab.Controls.Add(root);
            CargarFiltrosCatalogoProductos();
            FiltrarCatalogoProductos();
        }

        private Control CrearPanelListadoCatalogo()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = ObtenerPanelCatalogo();
            panel.Padding = new Padding(12);
            panel.ColumnCount = 1;
            panel.RowCount = 5;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Margin = new Padding(0, 0, 12, 0);

            Label ayuda = new Label();
            ayuda.Text = "Revisa plantillas sin crear un proyecto.";
            ayuda.AutoSize = true;
            ayuda.Font = new Font("Segoe UI", 9.0f);
            ayuda.ForeColor = ObtenerTextoSecundarioCatalogo();
            ayuda.Margin = new Padding(0, 0, 0, 10);

            txtBuscarCatalogoProductos.PlaceholderText = "Buscar producto o servicio";
            txtBuscarCatalogoProductos.Dock = DockStyle.Top;
            txtBuscarCatalogoProductos.Margin = new Padding(0, 0, 0, 8);
            txtBuscarCatalogoProductos.TextChanged -= TxtBuscarCatalogoProductos_TextChanged;
            txtBuscarCatalogoProductos.TextChanged += TxtBuscarCatalogoProductos_TextChanged;

            cmbCategoriaCatalogoProductos.Dock = DockStyle.Top;
            cmbCategoriaCatalogoProductos.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoriaCatalogoProductos.Margin = new Padding(0, 0, 0, 8);
            cmbCategoriaCatalogoProductos.SelectedIndexChanged -= CmbCategoriaCatalogoProductos_SelectedIndexChanged;
            cmbCategoriaCatalogoProductos.SelectedIndexChanged += CmbCategoriaCatalogoProductos_SelectedIndexChanged;

            lstCatalogoProductos.Dock = DockStyle.Fill;
            lstCatalogoProductos.DisplayMember = "Nombre";
            lstCatalogoProductos.BorderStyle = BorderStyle.FixedSingle;
            lstCatalogoProductos.BackColor = modoOscuroActivo ? Color.FromArgb(45, 45, 48) : Color.White;
            lstCatalogoProductos.ForeColor = ObtenerTextoPrincipalCatalogo();
            lstCatalogoProductos.SelectedIndexChanged -= LstCatalogoProductos_SelectedIndexChanged;
            lstCatalogoProductos.SelectedIndexChanged += LstCatalogoProductos_SelectedIndexChanged;

            Button recargar = CrearBotonCatalogo("Recargar catalogo", false, 160);
            recargar.Dock = DockStyle.Top;
            recargar.Click += (s, e) => RecargarCatalogoDesdeDisco();

            panel.Controls.Add(ayuda, 0, 0);
            panel.Controls.Add(txtBuscarCatalogoProductos, 0, 1);
            panel.Controls.Add(cmbCategoriaCatalogoProductos, 0, 2);
            panel.Controls.Add(lstCatalogoProductos, 0, 3);
            panel.Controls.Add(recargar, 0, 4);
            return panel;
        }

        private Control CrearPanelDetalleCatalogo()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = ObtenerPanelCatalogo();
            panel.Padding = new Padding(16);
            panel.ColumnCount = 1;
            panel.RowCount = 5;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Control ficha = CrearFichaProductoCatalogo();
            Control configuracion = CrearPanelConfiguracionCotizacionCatalogo();

            panelCatalogoKpis.Dock = DockStyle.Top;
            panelCatalogoKpis.AutoSize = true;
            panelCatalogoKpis.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelCatalogoKpis.FlowDirection = FlowDirection.LeftToRight;
            panelCatalogoKpis.WrapContents = true;
            panelCatalogoKpis.Margin = new Padding(0, 0, 0, 10);
            panelCatalogoKpis.BackColor = ObtenerPanelCatalogo();
            panelCatalogoKpis.SizeChanged -= PanelCatalogoResponsive_SizeChanged;
            panelCatalogoKpis.SizeChanged += PanelCatalogoResponsive_SizeChanged;

            TableLayoutPanel cuerpo = new TableLayoutPanel();
            cuerpo.Dock = DockStyle.Fill;
            cuerpo.ColumnCount = 1;
            cuerpo.RowCount = 4;
            cuerpo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            cuerpo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cuerpo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            cuerpo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            cuerpo.BackColor = ObtenerPanelCatalogo();

            Label tituloProcesos = CrearTituloSeccionCatalogo("Desglose de procesos");
            ConfigurarGrillaProcesosCatalogo();

            Label tituloEtapas = CrearTituloSeccionCatalogo("Resumen por etapa");
            panelCatalogoEtapas.Dock = DockStyle.Top;
            panelCatalogoEtapas.AutoSize = true;
            panelCatalogoEtapas.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelCatalogoEtapas.FlowDirection = FlowDirection.LeftToRight;
            panelCatalogoEtapas.WrapContents = true;
            panelCatalogoEtapas.BackColor = ObtenerPanelCatalogo();
            panelCatalogoEtapas.SizeChanged -= PanelCatalogoResponsive_SizeChanged;
            panelCatalogoEtapas.SizeChanged += PanelCatalogoResponsive_SizeChanged;

            cuerpo.Controls.Add(tituloProcesos, 0, 0);
            cuerpo.Controls.Add(dgvCatalogoProcesos, 0, 1);
            cuerpo.Controls.Add(tituloEtapas, 0, 2);
            cuerpo.Controls.Add(panelCatalogoEtapas, 0, 3);

            panel.Controls.Add(ficha, 0, 0);
            panel.Controls.Add(configuracion, 0, 1);
            panel.Controls.Add(panelCatalogoKpis, 0, 2);
            panel.Controls.Add(cuerpo, 0, 3);
            panel.Controls.Add(CrearBarraAccionesCatalogo(), 0, 4);
            ConfigurarTemporizadorCatalogo();
            return panel;
        }

        private Control CrearFichaProductoCatalogo()
        {
            TableLayoutPanel ficha = new TableLayoutPanel();
            ficha.Dock = DockStyle.Top;
            ficha.AutoSize = true;
            ficha.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ficha.ColumnCount = 1;
            ficha.RowCount = 5;
            ficha.Margin = new Padding(0, 0, 0, 8);

            lblCatalogoNombreProducto.AutoSize = true;
            lblCatalogoNombreProducto.Font = new Font("Segoe UI", 17.0f, FontStyle.Bold);
            lblCatalogoNombreProducto.ForeColor = ObtenerTextoPrincipalCatalogo();
            lblCatalogoNombreProducto.Margin = new Padding(0, 0, 0, 4);

            lblCatalogoMetaProducto.AutoSize = true;
            lblCatalogoMetaProducto.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            lblCatalogoMetaProducto.ForeColor = Color.FromArgb(41, 171, 135);
            lblCatalogoMetaProducto.Margin = new Padding(0, 0, 0, 8);

            lblCatalogoDescripcionProducto.AutoSize = true;
            lblCatalogoDescripcionProducto.MaximumSize = new Size(900, 0);
            lblCatalogoDescripcionProducto.Font = new Font("Segoe UI", 9.4f);
            lblCatalogoDescripcionProducto.ForeColor = ObtenerTextoSecundarioCatalogo();
            lblCatalogoDescripcionProducto.Margin = new Padding(0, 0, 0, 14);

            Label nota = new Label();
            nota.Text = "Los valores mostrados corresponden a la plantilla y parametros predeterminados. Al crear un proyecto podran variar segun cantidad, complejidad, acabado, plazo y equipo asignado.";
            nota.AutoSize = true;
            nota.MaximumSize = new Size(980, 0);
            nota.Font = new Font("Segoe UI", 8.8f, FontStyle.Italic);
            nota.ForeColor = ObtenerTextoSecundarioCatalogo();
            nota.Margin = new Padding(0, 0, 0, 8);

            ficha.Controls.Add(lblCatalogoNombreProducto, 0, 0);
            ficha.Controls.Add(lblCatalogoMetaProducto, 0, 1);
            ficha.Controls.Add(lblCatalogoDescripcionProducto, 0, 2);
            ficha.Controls.Add(nota, 0, 3);
            return ficha;
        }

        private Label CrearTituloSeccionCatalogo(string texto)
        {
            Label label = new Label();
            label.Text = texto;
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 11.2f, FontStyle.Bold);
            label.ForeColor = ObtenerTextoPrincipalCatalogo();
            label.Margin = new Padding(0, 2, 0, 6);
            return label;
        }

        private Control CrearPanelConfiguracionCotizacionCatalogo()
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Top;
            card.AutoSize = true;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            card.Padding = new Padding(10);
            card.Margin = new Padding(0, 0, 0, 10);
            card.BackColor = modoOscuroActivo ? Color.FromArgb(45, 45, 48) : Color.FromArgb(248, 249, 251);
            card.BorderStyle = BorderStyle.FixedSingle;

            panelConfiguracionCotizacionCatalogo.Dock = DockStyle.Top;
            panelConfiguracionCotizacionCatalogo.AutoSize = true;
            panelConfiguracionCotizacionCatalogo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelConfiguracionCotizacionCatalogo.ColumnCount = 1;
            panelConfiguracionCotizacionCatalogo.RowCount = 1;
            panelConfiguracionCotizacionCatalogo.Margin = new Padding(0);

            card.Controls.Add(panelConfiguracionCotizacionCatalogo);
            return card;
        }

        private void RenderizarConfiguracionCotizacionCatalogo()
        {
            panelConfiguracionCotizacionCatalogo.Controls.Clear();
            panelConfiguracionCotizacionCatalogo.RowStyles.Clear();
            controlesParametrosCatalogo.Clear();

            if (configuracionCatalogoActual == null)
            {
                return;
            }

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Top;
            root.AutoSize = true;
            root.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.Margin = new Padding(0);

            Label titulo = new Label();
            titulo.Text = "Configuracion de cotizacion";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 10.8f, FontStyle.Bold);
            titulo.ForeColor = ObtenerTextoPrincipalCatalogo();
            titulo.Margin = new Padding(0, 0, 0, 4);

            lblResumenSimulacionCatalogo.AutoSize = true;
            lblResumenSimulacionCatalogo.Font = new Font("Segoe UI", 8.7f, FontStyle.Italic);
            lblResumenSimulacionCatalogo.ForeColor = ObtenerTextoSecundarioCatalogo();
            lblResumenSimulacionCatalogo.Text = "Simulacion de cotizacion: estos cambios no modifican la plantilla global.";
            lblResumenSimulacionCatalogo.Margin = new Padding(0, 0, 0, 8);

            FlowLayoutPanel filaBase = new FlowLayoutPanel();
            filaBase.Dock = DockStyle.Top;
            filaBase.AutoSize = true;
            filaBase.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            filaBase.WrapContents = true;
            filaBase.FlowDirection = FlowDirection.LeftToRight;
            filaBase.Margin = new Padding(0, 0, 0, 8);

            filaBase.Controls.Add(CrearCampoUnidadCantidadCatalogo());

            FlowLayoutPanel parametros = new FlowLayoutPanel();
            parametros.Dock = DockStyle.Top;
            parametros.AutoSize = true;
            parametros.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            parametros.WrapContents = true;
            parametros.FlowDirection = FlowDirection.LeftToRight;
            parametros.Margin = new Padding(0, 0, 0, 8);

            foreach (QuoteParameterDefinition parametro in configuracionCatalogoActual.Parameters.OrderBy(p => p.DisplayOrder))
            {
                parametros.Controls.Add(CrearCampoParametroCatalogo(parametro));
            }

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Top;
            acciones.AutoSize = true;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = true;
            acciones.Margin = new Padding(0);

            Button recalcular = CrearBotonCatalogo("Recalcular", false, 110);
            recalcular.Click += (s, e) => RecalcularCatalogoDesdeConfiguracion();
            Button restablecer = CrearBotonCatalogo("Restablecer valores", false, 150);
            restablecer.Click += (s, e) => RestablecerConfiguracionCatalogo();
            Button planos = CrearBotonCatalogo("Desglosar por planos", false, 160);
            planos.Visible = EsUnidadDuracion(configuracionCatalogoActual.CommercialUnit);
            planos.Click += (s, e) => AlternarDesglosePlanosCatalogo();

            acciones.Controls.Add(recalcular);
            acciones.Controls.Add(restablecer);
            acciones.Controls.Add(planos);

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(lblResumenSimulacionCatalogo, 0, 1);
            root.Controls.Add(filaBase, 0, 2);
            root.Controls.Add(parametros, 0, 3);
            root.Controls.Add(acciones, 0, 4);
            panelConfiguracionCotizacionCatalogo.Controls.Add(root, 0, 0);
        }

        private Control CrearCampoUnidadCantidadCatalogo()
        {
            FlowLayoutPanel panel = CrearContenedorCampoCatalogo("Cantidad simulada");
            nudCantidadSimulacionCatalogo.DecimalPlaces = 2;
            nudCantidadSimulacionCatalogo.Minimum = 0;
            nudCantidadSimulacionCatalogo.Maximum = 100000;
            nudCantidadSimulacionCatalogo.Width = 86;
            nudCantidadSimulacionCatalogo.ValueChanged -= ControlConfiguracionCatalogo_Changed;
            nudCantidadSimulacionCatalogo.Value = Math.Max(0, Math.Min(nudCantidadSimulacionCatalogo.Maximum, configuracionCatalogoActual.Quantity));
            nudCantidadSimulacionCatalogo.ValueChanged += ControlConfiguracionCatalogo_Changed;

            cmbUnidadComercialCatalogo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUnidadComercialCatalogo.Width = 125;
            cmbUnidadComercialCatalogo.Items.Clear();
            foreach (string unidad in ObtenerUnidadesComercialesCatalogo())
            {
                cmbUnidadComercialCatalogo.Items.Add(unidad);
            }
            if (!cmbUnidadComercialCatalogo.Items.Contains(configuracionCatalogoActual.CommercialUnit))
            {
                cmbUnidadComercialCatalogo.Items.Add(configuracionCatalogoActual.CommercialUnit);
            }
            cmbUnidadComercialCatalogo.SelectedItem = configuracionCatalogoActual.CommercialUnit;
            cmbUnidadComercialCatalogo.SelectedIndexChanged -= ControlConfiguracionCatalogo_Changed;
            cmbUnidadComercialCatalogo.SelectedIndexChanged += ControlConfiguracionCatalogo_Changed;

            panel.Controls.Add(nudCantidadSimulacionCatalogo);
            panel.Controls.Add(cmbUnidadComercialCatalogo);
            return panel;
        }

        private Control CrearCampoParametroCatalogo(QuoteParameterDefinition parametro)
        {
            FlowLayoutPanel panel = CrearContenedorCampoCatalogo(parametro.Label);
            Control editor;
            string valor = configuracionCatalogoActual.Values.TryGetValue(parametro.Id, out string actual)
                ? actual
                : parametro.DefaultValue;

            if (parametro.DataType == "selector")
            {
                ComboBox combo = new ComboBox();
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.Width = 150;
                foreach (string opcion in parametro.Options)
                {
                    combo.Items.Add(opcion);
                }
                if (!string.IsNullOrWhiteSpace(valor) && !combo.Items.Contains(valor))
                {
                    combo.Items.Add(valor);
                }
                combo.SelectedItem = string.IsNullOrWhiteSpace(valor) ? parametro.Options.FirstOrDefault() : valor;
                combo.SelectedIndexChanged += ControlConfiguracionCatalogo_Changed;
                editor = combo;
            }
            else if (parametro.DataType == "checkbox")
            {
                CheckBox check = new CheckBox();
                check.Text = parametro.Unit;
                check.Checked = string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase);
                check.CheckedChanged += ControlConfiguracionCatalogo_Changed;
                editor = check;
            }
            else
            {
                NumericUpDown nud = new NumericUpDown();
                nud.DecimalPlaces = parametro.DataType == "integer" ? 0 : 2;
                nud.Minimum = (decimal)parametro.MinValue;
                nud.Maximum = (decimal)Math.Max(parametro.MinValue + 1, parametro.MaxValue);
                nud.Width = 90;
                if (decimal.TryParse(valor, out decimal decimalValor))
                {
                    nud.Value = Math.Max(nud.Minimum, Math.Min(nud.Maximum, decimalValor));
                }
                nud.ValueChanged += ControlConfiguracionCatalogo_Changed;
                editor = nud;
            }

            editor.Tag = parametro;
            controlesParametrosCatalogo[parametro.Id] = editor;
            panel.Controls.Add(editor);
            if (!string.IsNullOrWhiteSpace(parametro.Unit) && parametro.DataType != "checkbox")
            {
                Label unidad = new Label();
                unidad.Text = parametro.Unit;
                unidad.AutoSize = true;
                unidad.Margin = new Padding(4, 7, 0, 0);
                unidad.ForeColor = ObtenerTextoSecundarioCatalogo();
                panel.Controls.Add(unidad);
            }

            if (!string.IsNullOrWhiteSpace(parametro.HelpText))
            {
                ToolTip tt = new ToolTip();
                tt.SetToolTip(panel, parametro.HelpText);
                tt.SetToolTip(editor, parametro.HelpText);
            }

            return panel;
        }

        private FlowLayoutPanel CrearContenedorCampoCatalogo(string etiqueta)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            panel.Margin = new Padding(0, 0, 12, 6);

            Label label = new Label();
            label.Text = etiqueta + ":";
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            label.ForeColor = ObtenerTextoPrincipalCatalogo();
            label.Margin = new Padding(0, 7, 5, 0);
            panel.Controls.Add(label);
            return panel;
        }

        private List<string> ObtenerUnidadesComercialesCatalogo()
        {
            return new List<string>
            {
                "segundos",
                "minutos",
                "ilustraciones",
                "personajes",
                "backgrounds",
                "planos",
                "viñetas",
                "sprites",
                "assets",
                "piezas",
                "packs",
                "unidades"
            };
        }

        private void ConfigurarGrillaProcesosCatalogo()
        {
            dgvCatalogoProcesos.Dock = DockStyle.Fill;
            dgvCatalogoProcesos.AllowUserToAddRows = false;
            dgvCatalogoProcesos.AllowUserToDeleteRows = false;
            dgvCatalogoProcesos.ReadOnly = true;
            dgvCatalogoProcesos.RowHeadersVisible = false;
            dgvCatalogoProcesos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCatalogoProcesos.MultiSelect = false;
            dgvCatalogoProcesos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCatalogoProcesos.ScrollBars = ScrollBars.Vertical;
            dgvCatalogoProcesos.BackgroundColor = ObtenerPanelCatalogo();
            dgvCatalogoProcesos.BorderStyle = BorderStyle.FixedSingle;
            dgvCatalogoProcesos.GridColor = modoOscuroActivo ? Color.FromArgb(70, 70, 70) : Color.Gainsboro;
            dgvCatalogoProcesos.EnableHeadersVisualStyles = false;
            dgvCatalogoProcesos.ColumnHeadersDefaultCellStyle.BackColor = modoOscuroActivo ? Color.FromArgb(48, 48, 52) : Color.FromArgb(235, 235, 235);
            dgvCatalogoProcesos.ColumnHeadersDefaultCellStyle.ForeColor = ObtenerTextoPrincipalCatalogo();
            dgvCatalogoProcesos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            dgvCatalogoProcesos.DefaultCellStyle.BackColor = ObtenerPanelCatalogo();
            dgvCatalogoProcesos.DefaultCellStyle.ForeColor = ObtenerTextoPrincipalCatalogo();
            dgvCatalogoProcesos.DefaultCellStyle.SelectionBackColor = modoOscuroActivo ? Color.FromArgb(70, 70, 74) : Color.FromArgb(220, 235, 250);
            dgvCatalogoProcesos.DefaultCellStyle.SelectionForeColor = ObtenerTextoPrincipalCatalogo();
            dgvCatalogoProcesos.RowTemplate.Height = 30;
            dgvCatalogoProcesos.Columns.Clear();

            AgregarColumnaCatalogo("Proceso", "Proceso", 170, 1.25f);
            AgregarColumnaCatalogo("Cargos", "Cargo(s)", 220, 1.8f);
            AgregarColumnaCatalogo("Horas", "Tiempo", 80, 0.55f);
            AgregarColumnaCatalogo("Costo", "Costo", 95, 0.75f);
            AgregarColumnaCatalogo("Precio", "Precio", 95, 0.75f);
            AgregarColumnaCatalogo("Etapa", "Etapa", 90, 0.65f);
            AgregarColumnaCatalogo("Estado", "Estado", 95, 0.7f);

            DataGridViewTextBoxColumn subproducto = new DataGridViewTextBoxColumn();
            subproducto.Name = "Subproducto";
            subproducto.Visible = false;
            dgvCatalogoProcesos.Columns.Add(subproducto);
            DataGridViewTextBoxColumn manoObra = new DataGridViewTextBoxColumn();
            manoObra.Name = "ManoObra";
            manoObra.Visible = false;
            dgvCatalogoProcesos.Columns.Add(manoObra);
            DataGridViewTextBoxColumn extras = new DataGridViewTextBoxColumn();
            extras.Name = "Extras";
            extras.Visible = false;
            dgvCatalogoProcesos.Columns.Add(extras);
            DataGridViewTextBoxColumn ecuacion = new DataGridViewTextBoxColumn();
            ecuacion.Name = "Ecuacion";
            ecuacion.Visible = false;
            dgvCatalogoProcesos.Columns.Add(ecuacion);

            foreach (string col in new[] { "Horas", "Costo", "Precio" })
            {
                dgvCatalogoProcesos.Columns[col].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            dgvCatalogoProcesos.CellDoubleClick -= DgvCatalogoProcesos_CellDoubleClick;
            dgvCatalogoProcesos.CellDoubleClick += DgvCatalogoProcesos_CellDoubleClick;
            dgvCatalogoProcesos.CellFormatting -= DgvCatalogoProcesos_CellFormatting;
            dgvCatalogoProcesos.CellFormatting += DgvCatalogoProcesos_CellFormatting;
            dgvCatalogoProcesos.CellMouseDown -= DgvCatalogoProcesos_CellMouseDown;
            dgvCatalogoProcesos.CellMouseDown += DgvCatalogoProcesos_CellMouseDown;
            dgvCatalogoProcesos.KeyDown -= DgvCatalogoProcesos_KeyDown;
            dgvCatalogoProcesos.KeyDown += DgvCatalogoProcesos_KeyDown;
            dgvCatalogoProcesos.MouseDown -= DgvCatalogoProcesos_MouseDown;
            dgvCatalogoProcesos.MouseDown += DgvCatalogoProcesos_MouseDown;
            dgvCatalogoProcesos.MouseMove -= DgvCatalogoProcesos_MouseMove;
            dgvCatalogoProcesos.MouseMove += DgvCatalogoProcesos_MouseMove;
            dgvCatalogoProcesos.DragOver -= DgvCatalogoProcesos_DragOver;
            dgvCatalogoProcesos.DragOver += DgvCatalogoProcesos_DragOver;
            dgvCatalogoProcesos.DragDrop -= DgvCatalogoProcesos_DragDrop;
            dgvCatalogoProcesos.DragDrop += DgvCatalogoProcesos_DragDrop;
            dgvCatalogoProcesos.AllowDrop = true;
        }

        private void AgregarColumnaCatalogo(string nombre, string titulo, int ancho, float peso)
        {
            DataGridViewTextBoxColumn columna = new DataGridViewTextBoxColumn();
            columna.Name = nombre;
            columna.HeaderText = titulo;
            columna.MinimumWidth = Math.Min(ancho, 70);
            columna.FillWeight = peso * 100f;
            columna.SortMode = DataGridViewColumnSortMode.Automatic;
            dgvCatalogoProcesos.Columns.Add(columna);
        }


        private Control CrearBarraAccionesCatalogo()
        {
            TableLayoutPanel barra = new TableLayoutPanel();
            barra.Dock = DockStyle.Top;
            barra.AutoSize = true;
            barra.ColumnCount = 3;
            barra.RowCount = 1;
            barra.Padding = new Padding(0, 12, 0, 0);
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            barra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            FlowLayoutPanel accionesIzquierda = new FlowLayoutPanel();
            accionesIzquierda.AutoSize = true;
            accionesIzquierda.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            accionesIzquierda.FlowDirection = FlowDirection.LeftToRight;
            accionesIzquierda.WrapContents = false;
            accionesIzquierda.Margin = new Padding(0);

            Button editar = CrearBotonCatalogo("Editar plantilla", false, 150);
            editar.Click += (s, e) => EditarPlantillaCatalogoSeleccionada();

            Button pipeline = CrearBotonCatalogo("Abrir pipeline", false, 150);
            pipeline.Click += (s, e) => AbrirPipelineCatalogoSeleccionado(null);

            Button nuevoProyecto = CrearBotonCatalogo("Crear proyecto", true, 160);
            nuevoProyecto.Click += (s, e) => CrearProyectoDesdeProductoCatalogo();

            accionesIzquierda.Controls.Add(editar);
            accionesIzquierda.Controls.Add(pipeline);
            barra.Controls.Add(accionesIzquierda, 0, 0);
            barra.Controls.Add(new Label(), 1, 0);
            barra.Controls.Add(nuevoProyecto, 2, 0);
            return barra;
        }

        private Button CrearBotonCatalogo(string texto, bool primario, int ancho)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = ancho;
            boton.Height = 34;
            boton.Margin = new Padding(0, 0, 8, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            boton.BackColor = primario
                ? Color.FromArgb(32, 160, 120)
                : modoOscuroActivo ? Color.FromArgb(45, 45, 48) : Color.White;
            boton.ForeColor = primario
                ? Color.White
                : ObtenerTextoPrincipalCatalogo();
            boton.FlatAppearance.BorderColor = primario
                ? Color.FromArgb(32, 160, 120)
                : modoOscuroActivo ? Color.FromArgb(85, 85, 85) : Color.FromArgb(150, 150, 150);
            return boton;
        }

        private void TxtBuscarCatalogoProductos_TextChanged(object sender, EventArgs e)
        {
            FiltrarCatalogoProductos();
        }

        private void CmbCategoriaCatalogoProductos_SelectedIndexChanged(object sender, EventArgs e)
        {
            FiltrarCatalogoProductos();
        }

        private void LstCatalogoProductos_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostrarProductoCatalogoSeleccionado();
        }

        private void CargarFiltrosCatalogoProductos()
        {
            cargandoCatalogoProductos = true;
            string anterior = cmbCategoriaCatalogoProductos.SelectedItem?.ToString() ?? "Todas";
            cmbCategoriaCatalogoProductos.Items.Clear();
            cmbCategoriaCatalogoProductos.Items.Add("Todas");
            foreach (string categoria in productosCatalogo
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c))
            {
                cmbCategoriaCatalogoProductos.Items.Add(categoria);
            }

            int indice = cmbCategoriaCatalogoProductos.Items.IndexOf(anterior);
            cmbCategoriaCatalogoProductos.SelectedIndex = indice >= 0 ? indice : 0;
            cargandoCatalogoProductos = false;
        }

        private void FiltrarCatalogoProductos()
        {
            if (cargandoCatalogoProductos)
            {
                return;
            }

            Producto2DDefinicion seleccionadoAnterior = lstCatalogoProductos.SelectedItem as Producto2DDefinicion;
            string nombreAnterior = seleccionadoAnterior?.Nombre ?? productoCatalogoVistaPrevia?.Nombre ?? "";
            string busqueda = (txtBuscarCatalogoProductos.Text ?? "").Trim().ToLowerInvariant();
            string categoria = cmbCategoriaCatalogoProductos.SelectedItem?.ToString() ?? "Todas";

            List<Producto2DDefinicion> filtrados = productosCatalogo
                .Where(p => categoria == "Todas" || string.Equals(p.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
                .Where(p => string.IsNullOrWhiteSpace(busqueda) ||
                    (p.Nombre ?? "").ToLowerInvariant().Contains(busqueda) ||
                    (p.Nota ?? "").ToLowerInvariant().Contains(busqueda) ||
                    (p.Industria ?? "").ToLowerInvariant().Contains(busqueda))
                .ToList();

            lstCatalogoProductos.DataSource = null;
            lstCatalogoProductos.DisplayMember = "Nombre";
            lstCatalogoProductos.DataSource = filtrados;

            if (filtrados.Count == 0)
            {
                MostrarProductoCatalogo(null);
                return;
            }

            int indice = filtrados.FindIndex(p => string.Equals(p.Nombre, nombreAnterior, StringComparison.OrdinalIgnoreCase));
            Producto2DDefinicion producto = filtrados[indice >= 0 ? indice : 0];
            MostrarProductoCatalogo(producto);
            SeleccionarProductoCatalogoSeguro(producto.Nombre);
        }

        private void MostrarProductoCatalogoSeleccionado()
        {
            MostrarProductoCatalogo(ObtenerProductoCatalogoSeleccionado());
        }

        private void MostrarProductoCatalogo(Producto2DDefinicion producto)
        {
            productoCatalogoVistaPrevia = producto;
            previewCatalogoActual = null;
            panelCatalogoKpis.Controls.Clear();
            panelCatalogoEtapas.Controls.Clear();
            dgvCatalogoProcesos.Rows.Clear();

            if (producto == null)
            {
                lblCatalogoNombreProducto.Text = "Selecciona un producto o servicio";
                lblCatalogoMetaProducto.Text = "";
                lblCatalogoDescripcionProducto.Text = "No hay productos para el filtro actual.";
                panelGantt?.Invalidate();
                return;
            }

            lblCatalogoNombreProducto.Text = producto.Nombre;
            lblCatalogoMetaProducto.Text =
                TextoCatalogo("Categoria", producto.Categoria) + "   |   " +
                TextoCatalogo("Industria", producto.Industria) + "   |   " +
                TextoCatalogo("Unidad", producto.UnidadCantidadSugerida) + "   |   " +
                "Duracion sugerida: " + producto.DuracionSugerida.ToString("0.##") + " " + producto.UnidadDuracionSugerida;
            lblCatalogoDescripcionProducto.Text = string.IsNullOrWhiteSpace(producto.Nota)
                ? "Sin descripcion."
                : producto.Nota;

            configuracionCatalogoActual = ProductQuoteConfigurationService.Crear(producto);
            cargandoConfiguracionCatalogo = true;
            RenderizarConfiguracionCotizacionCatalogo();
            cargandoConfiguracionCatalogo = false;
            previewCatalogoActual = CatalogoProductoPreviewService.Calcular(producto, cotizacion, configuracionCatalogoActual);
            ActualizarResumenSimulacionCatalogo();
            RenderizarKpisCatalogo(previewCatalogoActual);
            RenderizarProcesosCatalogo(previewCatalogoActual);
            RenderizarEtapasCatalogo(previewCatalogoActual);
            AjustarTarjetasResponsivasCatalogo();
            panelGantt?.Invalidate();
        }

        private void ConfigurarTemporizadorCatalogo()
        {
            temporizadorRecalculoCatalogo.Stop();
            temporizadorRecalculoCatalogo.Interval = 250;
            temporizadorRecalculoCatalogo.Tick -= TemporizadorRecalculoCatalogo_Tick;
            temporizadorRecalculoCatalogo.Tick += TemporizadorRecalculoCatalogo_Tick;
        }

        private void TemporizadorRecalculoCatalogo_Tick(object sender, EventArgs e)
        {
            temporizadorRecalculoCatalogo.Stop();
            RecalcularCatalogoDesdeConfiguracion();
        }

        private void ControlConfiguracionCatalogo_Changed(object sender, EventArgs e)
        {
            if (cargandoConfiguracionCatalogo)
            {
                return;
            }

            CapturarConfiguracionCatalogoDesdeControles();
            temporizadorRecalculoCatalogo.Stop();
            temporizadorRecalculoCatalogo.Start();
        }

        private void CapturarConfiguracionCatalogoDesdeControles()
        {
            if (configuracionCatalogoActual == null)
            {
                return;
            }

            configuracionCatalogoActual.Quantity = nudCantidadSimulacionCatalogo.Value;
            configuracionCatalogoActual.CommercialUnit =
                cmbUnidadComercialCatalogo.SelectedItem?.ToString() ?? configuracionCatalogoActual.CommercialUnit;

            foreach (KeyValuePair<string, Control> par in controlesParametrosCatalogo)
            {
                string valor = "";
                if (par.Value is ComboBox combo)
                {
                    valor = combo.SelectedItem?.ToString() ?? combo.Text;
                }
                else if (par.Value is NumericUpDown nud)
                {
                    valor = nud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (par.Value is CheckBox check)
                {
                    valor = check.Checked ? "true" : "false";
                }

                configuracionCatalogoActual.Values[par.Key] = valor;
            }
        }

        private void RecalcularCatalogoDesdeConfiguracion()
        {
            Producto2DDefinicion producto = productoCatalogoVistaPrevia;
            if (producto == null || configuracionCatalogoActual == null)
            {
                return;
            }

            CapturarConfiguracionCatalogoDesdeControles();
            previewCatalogoActual = CatalogoProductoPreviewService.Calcular(producto, cotizacion, configuracionCatalogoActual);
            ActualizarResumenSimulacionCatalogo();
            RenderizarKpisCatalogo(previewCatalogoActual);
            RenderizarProcesosCatalogo(previewCatalogoActual);
            RenderizarEtapasCatalogo(previewCatalogoActual);
            AjustarTarjetasResponsivasCatalogo();
            panelGantt?.Invalidate();
        }

        private void RestablecerConfiguracionCatalogo()
        {
            Producto2DDefinicion producto = productoCatalogoVistaPrevia;
            if (producto == null)
            {
                return;
            }

            configuracionCatalogoActual = ProductQuoteConfigurationService.Crear(producto);
            cargandoConfiguracionCatalogo = true;
            RenderizarConfiguracionCotizacionCatalogo();
            cargandoConfiguracionCatalogo = false;
            RecalcularCatalogoDesdeConfiguracion();
        }

        private void ActualizarResumenSimulacionCatalogo()
        {
            if (configuracionCatalogoActual == null || lblResumenSimulacionCatalogo == null)
            {
                return;
            }

            double cantidad = (double)Math.Max(0.0001m, configuracionCatalogoActual.Quantity);
            string unidad = configuracionCatalogoActual.CommercialUnit;
            string precioUnitario = previewCatalogoActual == null || previewCatalogoActual.PrecioSugeridoCLP <= 0.0
                ? "No calculable"
                : FormatearMonedaCatalogo(previewCatalogoActual.PrecioSugeridoCLP / cantidad) + " / " + unidad;
            string tiempoUnitario = previewCatalogoActual == null || previewCatalogoActual.TotalHoras <= 0.0
                ? "No calculable"
                : (previewCatalogoActual.TotalHoras / cantidad).ToString("0.##") + " h / " + unidad;

            lblResumenSimulacionCatalogo.Text =
                "Simulacion de cotizacion | Unidad: " + unidad +
                " | Cantidad: " + configuracionCatalogoActual.Quantity.ToString("0.##") + " " + unidad +
                " | Precio promedio: " + precioUnitario +
                " | Tiempo promedio: " + tiempoUnitario +
                ". No modifica la plantilla global.";
        }

        private void AlternarDesglosePlanosCatalogo()
        {
            if (configuracionCatalogoActual == null)
            {
                return;
            }

            configuracionCatalogoActual.HasShotBreakdown = !configuracionCatalogoActual.HasShotBreakdown;
            if (configuracionCatalogoActual.HasShotBreakdown && configuracionCatalogoActual.Shots.Count == 0)
            {
                double total = (double)Math.Max(1m, configuracionCatalogoActual.Quantity);
                configuracionCatalogoActual.Shots.Add(new ProductShotBreakdownItem
                {
                    Name = "Plano 01",
                    DurationSeconds = total,
                    Density = "Media",
                    Complexity = "Media",
                    Characters = 1
                });
                MessageBox.Show(
                    "Desglose por planos activado con un plano inicial. La edición detallada de planos queda preparada para el siguiente paso; la validación ya usa esta estructura.",
                    "Desglose por planos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            RecalcularCatalogoDesdeConfiguracion();
        }

        private bool EsUnidadDuracion(string unidad)
        {
            string u = (unidad ?? "").Trim().ToLowerInvariant();
            return u.Contains("segundo") || u == "s" || u.Contains("minuto") || u.Contains("frame");
        }

        private void SeleccionarProductoCatalogoSeguro(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || lstCatalogoProductos == null)
            {
                return;
            }

            for (int i = 0; i < lstCatalogoProductos.Items.Count; i++)
            {
                Producto2DDefinicion producto = lstCatalogoProductos.Items[i] as Producto2DDefinicion;
                if (producto == null || !string.Equals(producto.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    if (i >= 0 && i < lstCatalogoProductos.Items.Count)
                    {
                        lstCatalogoProductos.SelectedIndex = i;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Durante la construcción del TabPage el CurrencyManager puede no aceptar
                    // aún la selección visual. La ficha ya fue renderizada arriba.
                }
                return;
            }
        }

        private void RenderizarKpisCatalogo(CatalogoProductoPreview preview)
        {
            panelCatalogoKpis.Controls.Clear();
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Tiempo total", FormatearHorasCatalogo(preview.TotalHoras), preview.TotalHoras <= 0.0 ? "Falta rendimiento o tiempo asignado." : ""));
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Costo directo", FormatearMonedaCatalogo(preview.CostoDirectoCLP), preview.CostoDirectoCLP <= 0.0 ? "Falta cargo, valor hora o rendimiento." : ""));
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Precio sugerido", FormatearMonedaCatalogo(preview.PrecioSugeridoCLP), preview.PrecioSugeridoCLP <= 0.0 ? "No calculable sin costo directo." : ""));
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Margen", preview.PrecioSugeridoCLP <= 0.0 ? "No calculable" : (preview.MargenEstimado * 100.0).ToString("0.#") + "%", ""));
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Procesos", preview.CantidadProcesos.ToString("0"), ""));
            panelCatalogoKpis.Controls.Add(CrearKpiCatalogo("Cargos", preview.CantidadCargos.ToString("0"), preview.CantidadCargos == 0 ? "Falta cargo asociado." : ""));
        }

        private Control CrearKpiCatalogo(string titulo, string valor, string tooltip)
        {
            Panel card = new Panel();
            card.Width = 150;
            card.Height = 68;
            card.Margin = new Padding(0, 0, 8, 8);
            card.Padding = new Padding(9, 7, 9, 7);
            card.BackColor = modoOscuroActivo ? Color.FromArgb(45, 45, 48) : Color.FromArgb(248, 249, 251);
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.AutoEllipsis = true;
            lblTitulo.Width = 122;
            lblTitulo.Height = 18;
            lblTitulo.Font = new Font("Segoe UI", 8.2f, FontStyle.Bold);
            lblTitulo.ForeColor = ObtenerTextoSecundarioCatalogo();
            lblTitulo.Location = new Point(8, 7);

            Label lblValor = new Label();
            lblValor.Text = string.IsNullOrWhiteSpace(valor) ? "Pendiente" : valor;
            lblValor.AutoEllipsis = true;
            lblValor.Width = 122;
            lblValor.Height = 30;
            lblValor.Font = new Font("Segoe UI", 10.2f, FontStyle.Bold);
            lblValor.ForeColor = ObtenerTextoPrincipalCatalogo();
            lblValor.Location = new Point(8, 30);

            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);

            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                ToolTip tt = new ToolTip();
                tt.SetToolTip(card, tooltip);
                tt.SetToolTip(lblValor, tooltip);
            }

            return card;
        }

        private void RenderizarProcesosCatalogo(CatalogoProductoPreview preview)
        {
            dgvCatalogoProcesos.Rows.Clear();
            if (preview == null || preview.Procesos.Count == 0)
            {
                int vacio = dgvCatalogoProcesos.Rows.Add();
                DataGridViewRow row = dgvCatalogoProcesos.Rows[vacio];
                row.Cells["Proceso"].Value = "No existen procesos configurados para este producto.";
                row.DefaultCellStyle.ForeColor = ObtenerTextoSecundarioCatalogo();
                return;
            }

            foreach (CatalogoProcesoPreview proceso in preview.Procesos)
            {
                AgregarFilaProcesoCatalogo(proceso);
            }
        }

        private void AgregarFilaProcesoCatalogo(CatalogoProcesoPreview proceso)
        {
            int index = dgvCatalogoProcesos.Rows.Add();
            DataGridViewRow row = dgvCatalogoProcesos.Rows[index];
            row.Tag = proceso;
            row.Cells["Etapa"].Value = proceso.Etapa;
            row.Cells["Proceso"].Value = proceso.Proceso;
            row.Cells["Subproducto"].Value = proceso.SubproductoNombre;
            row.Cells["Cargos"].Value = string.IsNullOrWhiteSpace(proceso.Cargos) ? "Sin cargos asignados" : proceso.Cargos;
            row.Cells["Horas"].Value = FormatearHorasCatalogo(proceso.Horas);
            row.Cells["ManoObra"].Value = FormatearMonedaCatalogo(proceso.CostoManoObraCLP);
            row.Cells["Extras"].Value = FormatearMonedaCatalogo(proceso.CostosAdicionalesCLP);
            row.Cells["Costo"].Value = FormatearMonedaCatalogo(proceso.CostoTotalCLP);
            row.Cells["Precio"].Value = FormatearMonedaCatalogo(proceso.PrecioSugeridoCLP);
            row.Cells["Ecuacion"].Value = string.IsNullOrWhiteSpace(proceso.Ecuacion) ? "Sin ecuacion vinculada" : proceso.Ecuacion;
            row.Cells["Estado"].Value = proceso.Estado;

            string tooltip = proceso.DependenciasFaltantes.Count == 0
                ? proceso.DetalleCalculo
                : string.Join("\n", proceso.DependenciasFaltantes) + "\n\n" + proceso.DetalleCalculo;
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.ToolTipText = tooltip;
            }
        }

        private void RenderizarEtapasCatalogo(CatalogoProductoPreview preview)
        {
            panelCatalogoEtapas.Controls.Clear();
            foreach (CatalogoEtapaPreview etapa in preview.Etapas)
            {
                panelCatalogoEtapas.Controls.Add(CrearResumenEtapaCatalogo(etapa));
            }
        }

        private Control CrearResumenEtapaCatalogo(CatalogoEtapaPreview etapa)
        {
            Panel card = new Panel();
            card.Width = 230;
            card.Height = 88;
            card.Margin = new Padding(0, 0, 8, 8);
            card.Padding = new Padding(9);
            card.BackColor = modoOscuroActivo ? Color.FromArgb(45, 45, 48) : Color.FromArgb(248, 249, 251);
            card.BorderStyle = BorderStyle.FixedSingle;

            Label titulo = new Label();
            titulo.Text = etapa.Etapa;
            titulo.AutoEllipsis = true;
            titulo.Width = card.Width - 22;
            titulo.Height = 18;
            titulo.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            titulo.ForeColor = ObtenerColorEtapaCatalogo(etapa.Etapa);
            titulo.Location = new Point(9, 7);
            card.Controls.Add(titulo);

            Label detalle = new Label();
            detalle.AutoEllipsis = true;
            detalle.Width = card.Width - 22;
            detalle.Height = card.Height - 28;
            detalle.Font = new Font("Segoe UI", 8.2f);
            detalle.ForeColor = ObtenerTextoPrincipalCatalogo();
            detalle.Location = new Point(9, 28);
            detalle.Text = etapa.CantidadProcesos == 0
                ? "Sin procesos configurados"
                : etapa.CantidadProcesos + " procesos | " +
                  FormatearHorasCatalogo(etapa.Horas) + "\n" +
                  FormatearMonedaCatalogo(etapa.CostoTotalCLP) + " | " +
                  (etapa.PorcentajeCosto * 100.0).ToString("0.#") + "% costo\n" +
                  (etapa.Cargos.Count == 0 ? "Sin cargos" : string.Join(", ", etapa.Cargos.Take(2)));
            card.Controls.Add(detalle);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(card, etapa.Estado + "\nCargos: " + (etapa.Cargos.Count == 0 ? "Sin cargos" : string.Join(", ", etapa.Cargos)));
            return card;
        }

        private void PanelCatalogoResponsive_SizeChanged(object sender, EventArgs e)
        {
            AjustarTarjetasResponsivasCatalogo();
        }

        private void AjustarTarjetasResponsivasCatalogo()
        {
            AjustarAnchoTarjetas(panelCatalogoKpis, 150, 6);
            AjustarAnchoTarjetas(panelCatalogoEtapas, 210, 4);
        }

        private void AjustarAnchoTarjetas(FlowLayoutPanel panel, int minimo, int idealPorFila)
        {
            if (panel == null || panel.ClientSize.Width <= 0 || panel.Controls.Count == 0)
            {
                return;
            }

            int anchoDisponible = Math.Max(minimo, panel.ClientSize.Width - 12);
            int porFila = Math.Min(idealPorFila, Math.Max(1, anchoDisponible / minimo));
            int ancho = Math.Max(minimo, (anchoDisponible / porFila) - 8);

            foreach (Control control in panel.Controls)
            {
                control.Width = ancho;
            }
        }

        private void DgvCatalogoProcesos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            CatalogoProcesoPreview proceso = dgvCatalogoProcesos.Rows[e.RowIndex].Tag as CatalogoProcesoPreview;
            if (proceso != null)
            {
                MostrarDetalleProcesoCatalogo(proceso);
            }
        }

        private void DgvCatalogoProcesos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || dgvCatalogoProcesos.CurrentRow == null)
            {
                return;
            }

            CatalogoProcesoPreview proceso = dgvCatalogoProcesos.CurrentRow.Tag as CatalogoProcesoPreview;
            if (proceso != null)
            {
                e.Handled = true;
                MostrarDetalleProcesoCatalogo(proceso);
            }
        }

        private void DgvCatalogoProcesos_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.Button != MouseButtons.Right)
            {
                return;
            }

            dgvCatalogoProcesos.ClearSelection();
            dgvCatalogoProcesos.Rows[e.RowIndex].Selected = true;
            dgvCatalogoProcesos.CurrentCell = dgvCatalogoProcesos.Rows[e.RowIndex].Cells["Proceso"];
            CatalogoProcesoPreview proceso = dgvCatalogoProcesos.Rows[e.RowIndex].Tag as CatalogoProcesoPreview;
            if (proceso == null)
            {
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Ver detalle", null, (s, args) => MostrarDetalleProcesoCatalogo(proceso));
            menu.Items.Add("Ver ecuacion", null, (s, args) => AbrirEcuacionProcesoCatalogo(proceso));
            menu.Items.Add("Abrir en pipeline", null, (s, args) => AbrirProcesoEnPipelineCatalogo(proceso));
            menu.Items.Add("Editar proceso", null, (s, args) => AbrirProcesoEnPipelineCatalogo(proceso));
            menu.Items.Add("Ver desglose del calculo", null, (s, args) => MostrarDetalleProcesoCatalogo(proceso));
            menu.Show(dgvCatalogoProcesos, dgvCatalogoProcesos.PointToClient(Cursor.Position));
        }

        private void DgvCatalogoProcesos_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            DataGridView.HitTestInfo hit = dgvCatalogoProcesos.HitTest(e.X, e.Y);
            indiceArrastreProcesoCatalogo = hit.RowIndex;
            if (indiceArrastreProcesoCatalogo < 0 ||
                !(dgvCatalogoProcesos.Rows[indiceArrastreProcesoCatalogo].Tag is CatalogoProcesoPreview))
            {
                indiceArrastreProcesoCatalogo = -1;
                rectArrastreProcesoCatalogo = Rectangle.Empty;
                return;
            }

            Size dragSize = SystemInformation.DragSize;
            rectArrastreProcesoCatalogo = new Rectangle(
                e.X - dragSize.Width / 2,
                e.Y - dragSize.Height / 2,
                dragSize.Width,
                dragSize.Height
            );
        }

        private void DgvCatalogoProcesos_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left ||
                indiceArrastreProcesoCatalogo < 0 ||
                rectArrastreProcesoCatalogo.Contains(e.Location))
            {
                return;
            }

            dgvCatalogoProcesos.DoDragDrop(
                dgvCatalogoProcesos.Rows[indiceArrastreProcesoCatalogo],
                DragDropEffects.Move
            );
        }

        private void DgvCatalogoProcesos_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void DgvCatalogoProcesos_DragDrop(object sender, DragEventArgs e)
        {
            Point client = dgvCatalogoProcesos.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo hit = dgvCatalogoProcesos.HitTest(client.X, client.Y);
            int destino = hit.RowIndex;
            if (indiceArrastreProcesoCatalogo < 0 || destino < 0 || destino == indiceArrastreProcesoCatalogo)
            {
                return;
            }

            CatalogoProcesoPreview origen =
                dgvCatalogoProcesos.Rows[indiceArrastreProcesoCatalogo].Tag as CatalogoProcesoPreview;
            CatalogoProcesoPreview procesoDestino =
                dgvCatalogoProcesos.Rows[destino].Tag as CatalogoProcesoPreview;
            if (origen == null || procesoDestino == null)
            {
                return;
            }

            if (!string.Equals(origen.Etapa, procesoDestino.Etapa, StringComparison.OrdinalIgnoreCase))
            {
                DialogResult r = MessageBox.Show(
                    "Esta moviendo '" + origen.Proceso + "' desde " + origen.Etapa +
                    " a " + procesoDestino.Etapa + ".\n\n" +
                    "Esto solo cambiara el orden visual y la etapa del proceso; no actualizara dependencias automaticamente.\n\n" +
                    "Desea continuar?",
                    "Mover proceso entre etapas",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (r != DialogResult.Yes)
                {
                    return;
                }
            }

            ReordenarProcesoCatalogo(origen, procesoDestino, destino);
        }

        private void ReordenarProcesoCatalogo(
            CatalogoProcesoPreview origen,
            CatalogoProcesoPreview destino,
            int indiceDestino
        )
        {
            Producto2DDefinicion producto = productoCatalogoVistaPrevia;
            if (producto == null || producto.Subproductos == null ||
                origen.Subproducto == null || destino.Subproducto == null)
            {
                return;
            }

            List<Subproducto2D> lista = producto.Subproductos
                .Where(s => s != null)
                .OrderBy(s => s.Orden <= 0 ? int.MaxValue : s.Orden)
                .ThenBy(s => s.Nombre)
                .ToList();

            Subproducto2D subOrigen = lista.FirstOrDefault(s => ReferenceEquals(s, origen.Subproducto) || s.Nombre == origen.Subproducto.Nombre);
            Subproducto2D subDestino = lista.FirstOrDefault(s => ReferenceEquals(s, destino.Subproducto) || s.Nombre == destino.Subproducto.Nombre);
            if (subOrigen == null || subDestino == null)
            {
                return;
            }

            if (!string.Equals(origen.Etapa, destino.Etapa, StringComparison.OrdinalIgnoreCase))
            {
                subOrigen.EtapaSugerida = subDestino.EtapaSugerida;
                subOrigen.Categoria = subDestino.Categoria;
            }

            lista.Remove(subOrigen);
            int nuevoIndice = Math.Max(0, lista.IndexOf(subDestino));
            if (indiceDestino > indiceArrastreProcesoCatalogo)
            {
                nuevoIndice++;
            }
            nuevoIndice = Math.Max(0, Math.Min(lista.Count, nuevoIndice));
            lista.Insert(nuevoIndice, subOrigen);

            for (int i = 0; i < lista.Count; i++)
            {
                lista[i].Orden = (i + 1) * 10;
            }

            producto.Subproductos = lista;
            PersistirOrdenProductoCatalogo(producto);
            RecargarCatalogoDesdeDisco();
            SeleccionarProductoCatalogoPorNombre(producto.Nombre);
        }

        private void PersistirOrdenProductoCatalogo(Producto2DDefinicion producto)
        {
            List<Producto2DDefinicion> productos = BibliotecaProductos2DJsonService.CargarProductos();
            Producto2DDefinicion destino = productos.FirstOrDefault(p =>
                p != null &&
                string.Equals(p.Nombre, producto.Nombre, StringComparison.OrdinalIgnoreCase));
            if (destino == null)
            {
                return;
            }

            destino.Subproductos = producto.Subproductos;
            BibliotecaProductos2DJsonService.GuardarProductos(productos);
        }

        private void DgvCatalogoProcesos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvCatalogoProcesos.Rows[e.RowIndex];
            if (row.Tag is string)
            {
                row.DefaultCellStyle.BackColor = modoOscuroActivo ? Color.FromArgb(40, 40, 44) : Color.FromArgb(246, 247, 249);
                row.DefaultCellStyle.ForeColor = ObtenerTextoSecundarioCatalogo();
                return;
            }

            CatalogoProcesoPreview proceso = row.Tag as CatalogoProcesoPreview;
            if (proceso == null)
            {
                return;
            }

            row.DefaultCellStyle.BackColor = MezclarConBlanco(ObtenerColorEtapaCatalogo(proceso.Etapa), modoOscuroActivo ? 0.78 : 0.92);
            if (dgvCatalogoProcesos.Columns[e.ColumnIndex].Name == "Estado")
            {
                if (proceso.Estado == "OK")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(24, 128, 74);
                }
                else if (proceso.Estado == "Incompleto")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(174, 105, 18);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(190, 54, 54);
                }
                e.CellStyle.Font = new Font("Segoe UI", 8.6f, FontStyle.Bold);
            }
        }

        private void AlternarDetalleProcesoCatalogo(int rowIndex, CatalogoProcesoPreview proceso)
        {
            if (rowIndex < 0 || proceso == null)
            {
                return;
            }

            if (rowIndex + 1 < dgvCatalogoProcesos.Rows.Count &&
                dgvCatalogoProcesos.Rows[rowIndex + 1].Tag is string tag &&
                tag == "detalle")
            {
                dgvCatalogoProcesos.Rows.RemoveAt(rowIndex + 1);
                proceso.Expandido = false;
                return;
            }

            dgvCatalogoProcesos.Rows.Insert(rowIndex + 1, 1);
            int detalleIndex = rowIndex + 1;
            DataGridViewRow detalle = dgvCatalogoProcesos.Rows[detalleIndex];
            detalle.Tag = "detalle";
            detalle.Height = 92;
            detalle.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            detalle.DefaultCellStyle.BackColor = modoOscuroActivo ? Color.FromArgb(36, 36, 38) : Color.FromArgb(252, 252, 252);
            detalle.Cells["Proceso"].Value = proceso.DetalleCalculo;
            detalle.Cells["Proceso"].ToolTipText = proceso.DetalleCalculo;
            proceso.Expandido = true;
        }

        private void MostrarDetalleProcesoCatalogo(CatalogoProcesoPreview proceso)
        {
            if (proceso == null)
            {
                return;
            }

            string faltantes = proceso.DependenciasFaltantes.Count == 0
                ? "Sin dependencias faltantes"
                : string.Join("\n- ", proceso.DependenciasFaltantes);

            MessageBox.Show(
                "Proceso: " + proceso.Proceso + "\n" +
                "Etapa: " + proceso.Etapa + "\n" +
                "Subproducto: " + proceso.SubproductoNombre + "\n" +
                "Cargos: " + (string.IsNullOrWhiteSpace(proceso.Cargos) ? "Sin cargos" : proceso.Cargos) + "\n" +
                "Estado: " + proceso.Estado + "\n\n" +
                proceso.DetalleCalculo + "\n\n" +
                "Dependencias faltantes:\n- " + faltantes,
                "Detalle del proceso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void AbrirProcesoEnPipelineCatalogo(CatalogoProcesoPreview proceso)
        {
            AbrirPipelineCatalogoSeleccionado(proceso?.Subproducto);
        }

        private void AbrirEcuacionProcesoCatalogo(CatalogoProcesoPreview proceso)
        {
            if (proceso == null || string.IsNullOrWhiteSpace(proceso.Ecuacion))
            {
                MessageBox.Show(
                    "Este proceso no tiene una ecuacion vinculada.",
                    "Ecuacion productiva",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            AbrirTabEcuacionesProductivas(proceso.Ecuacion);
        }

        private string FormatearHorasCatalogo(double horas)
        {
            return horas <= 0.0 ? "No calculable" : horas.ToString("0.##") + " h";
        }

        private string FormatearMonedaCatalogo(double valorCLP)
        {
            if (valorCLP <= 0.0)
            {
                return "No calculable";
            }

            return MonedaService.FormatearMoneda(cotizacion, valorCLP);
        }

        private Color ObtenerColorEtapaCatalogo(string etapa)
        {
            string normalizada = (etapa ?? "").ToLowerInvariant();
            if (normalizada.Contains("desarrollo"))
            {
                return Color.FromArgb(34, 145, 85);
            }

            if (normalizada.Contains("pre"))
            {
                return Color.FromArgb(214, 142, 26);
            }

            if (normalizada.Contains("post"))
            {
                return Color.FromArgb(42, 128, 214);
            }

            return Color.FromArgb(212, 75, 75);
        }

        private string TextoCatalogo(string etiqueta, string valor)
        {
            return etiqueta + ": " + (string.IsNullOrWhiteSpace(valor) ? "No informado" : valor);
        }

        private Producto2DDefinicion ObtenerProductoCatalogoSeleccionado()
        {
            return lstCatalogoProductos.SelectedItem as Producto2DDefinicion;
        }

        private void RecargarCatalogoDesdeDisco()
        {
            string seleccionado = ObtenerProductoCatalogoSeleccionado()?.Nombre ?? "";
            productosCatalogo = BibliotecaProductos2DJsonService.CargarProductos()
                .Where(p => p != null)
                .OrderBy(p => p.Nombre)
                .ToList();
            CargarFiltrosCatalogoProductos();
            FiltrarCatalogoProductos();

            if (!string.IsNullOrWhiteSpace(seleccionado))
            {
                SeleccionarProductoCatalogoPorNombre(seleccionado);
            }
        }

        private void SeleccionarProductoCatalogoPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return;
            }

            for (int i = 0; i < lstCatalogoProductos.Items.Count; i++)
            {
                Producto2DDefinicion producto = lstCatalogoProductos.Items[i] as Producto2DDefinicion;
                if (producto != null && string.Equals(producto.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    SeleccionarProductoCatalogoSeguro(nombre);
                    return;
                }
            }
        }

        private void EditarPlantillaCatalogoSeleccionada()
        {
            Producto2DDefinicion producto = ObtenerProductoCatalogoSeleccionado();
            if (producto == null)
            {
                return;
            }

            AbrirTabProductos2D(producto.Nombre);
        }

        private void EditarProcesoCatalogo(CatalogoProcesoVisual proceso)
        {
            AbrirProcesoEnPipelineCatalogo(proceso);
        }

        private void AbrirProcesoEnPipelineCatalogo(CatalogoProcesoVisual proceso)
        {
            AbrirPipelineCatalogoSeleccionado(proceso?.Subproducto);
        }

        private void AbrirPipelineCatalogoSeleccionado(Subproducto2D subproducto)
        {
            Producto2DDefinicion producto = ObtenerProductoCatalogoSeleccionado();
            if (producto == null)
            {
                return;
            }

            SincronizarProductoSeleccionadoParaPipeline(producto.Nombre);
            BtnEditarPipelineProducto_Click(this, EventArgs.Empty);
            RecargarCatalogoDesdeDisco();
            SeleccionarProductoCatalogoPorNombre(producto.Nombre);
        }

        private void SincronizarProductoSeleccionadoParaPipeline(string nombreProducto)
        {
            if (cmbProductoServicio == null || string.IsNullOrWhiteSpace(nombreProducto))
            {
                return;
            }

            if (!cmbProductoServicio.Items.Contains(nombreProducto))
            {
                cmbProductoServicio.Items.Add(nombreProducto);
            }

            cmbProductoServicio.SelectedItem = nombreProducto;
        }

        private void AbrirEcuacionProcesoCatalogo(CatalogoProcesoVisual proceso)
        {
            if (proceso == null || string.IsNullOrWhiteSpace(proceso.Ecuacion))
            {
                MessageBox.Show(
                    "Este proceso no tiene una ecuacion vinculada.",
                    "Ecuacion productiva",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            AbrirTabEcuacionesProductivas(proceso.Ecuacion);
        }

        private void CrearProyectoDesdeProductoCatalogo()
        {
            Producto2DDefinicion producto = ObtenerProductoCatalogoSeleccionado();
            if (producto == null)
            {
                return;
            }

            NuevoProyectoDesdeInicio();
            if (proyectoCotizacionActual == null)
            {
                return;
            }

            GrupoProyecto grupo = proyectoCotizacionActual.Grupos.FirstOrDefault() ?? new GrupoProyecto
            {
                Id = "grp_produccion",
                Nombre = "Produccion",
                Tipo = TipoGrupoProyecto.Produccion
            };
            if (!proyectoCotizacionActual.Grupos.Contains(grupo))
            {
                proyectoCotizacionActual.Grupos.Add(grupo);
            }

            ProductoProyecto item = ProyectoProductoBibliotecaAdapterService.CrearProductoDesdeBiblioteca(
                producto,
                new ProyectoProductoBibliotecaAdapterService.OpcionesAgregarProducto
                {
                    Cantidad = configuracionCatalogoActual == null ? 1 : configuracionCatalogoActual.Quantity,
                    Unidad = configuracionCatalogoActual == null
                        ? producto.UnidadCantidadSugerida
                        : configuracionCatalogoActual.CommercialUnit,
                    Duracion = configuracionCatalogoActual != null && EsUnidadDuracion(configuracionCatalogoActual.CommercialUnit)
                        ? configuracionCatalogoActual.Quantity
                        : Convert.ToDecimal(producto.DuracionSugerida),
                    SubproductosSeleccionados = (producto.Subproductos ?? new List<Subproducto2D>())
                        .Where(s => s != null && s.RequeridoPorDefecto)
                        .ToList()
                },
                grupo.Items.Count
            );
            grupo.Items.Add(item);
            MarcarProyectoConCambiosPendientes();
            AplicarModoAplicacion(ModoAplicacion.Proyecto);
            if (tabs != null && tabProyectoPrincipal != null)
            {
                tabs.SelectedTab = tabProyectoPrincipal;
            }
        }

        private Color ObtenerFondoCatalogo()
        {
            return modoOscuroActivo ? Color.FromArgb(30, 30, 30) : Color.FromArgb(246, 247, 249);
        }

        private Color ObtenerPanelCatalogo()
        {
            return modoOscuroActivo ? Color.FromArgb(37, 37, 38) : Color.White;
        }

        private Color ObtenerTextoPrincipalCatalogo()
        {
            return modoOscuroActivo ? Color.FromArgb(235, 235, 235) : Color.FromArgb(22, 24, 29);
        }

        private Color ObtenerTextoSecundarioCatalogo()
        {
            return modoOscuroActivo ? Color.FromArgb(190, 190, 190) : Color.FromArgb(82, 90, 105);
        }
    }
}
