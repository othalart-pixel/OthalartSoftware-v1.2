using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

#pragma warning disable CS0162

namespace Cotizador_animacion_Othalart
{
    public class FormAgregarProductoServicioProyecto : Form
    {
        private enum TipoElementoCotizable
        {
            Producto,
            Servicio,
            Paquete
        }

        private sealed class CatalogItemView
        {
            public Producto2DDefinicion Producto { get; set; }
            public TipoElementoCotizable Tipo { get; set; }
            public string Nombre => Producto?.Nombre ?? "";
            public string Categoria => Producto?.Categoria ?? "";
            public string Industria => Producto?.Industria ?? "";
            public string Unidad => ObtenerUnidadComercial(Producto);
        }

        private sealed class SubproductoItemView
        {
            public Subproducto2D Subproducto { get; set; }
            public string Display { get; set; } = "";
        }

        private readonly List<CatalogItemView> catalogo;
        private readonly TextBox txtBuscar = new TextBox();
        private readonly ComboBox cmbTipo = new ComboBox();
        private readonly ComboBox cmbCategoria = new ComboBox();
        private readonly ComboBox cmbIndustria = new ComboBox();
        private readonly Label lblResultados = new Label();
        private readonly ListBox lstProductos = new ListBox();

        private readonly Label lblNombre = new Label();
        private readonly Label lblMeta = new Label();
        private readonly NumericUpDown nudCantidad = new NumericUpDown();
        private readonly TextBox txtUnidad = new TextBox();
        private readonly NumericUpDown nudDuracion = new NumericUpDown();
        private readonly Label lblErrorCantidad = CrearErrorLabel();
        private readonly Label lblErrorUnidad = CrearErrorLabel();
        private readonly Label lblErrorDuracion = CrearErrorLabel();
        private readonly CheckedListBox clbComponentes = new CheckedListBox();
        private readonly Label lblSinComponentes = new Label();

        private readonly Panel panelAvanzado = new Panel();
        private readonly Button btnAvanzado = new Button();
        private readonly Button btnEditarAntesAgregar = new Button();
        private readonly NumericUpDown nudRevisiones = new NumericUpDown();
        private readonly NumericUpDown nudRetrabajo = new NumericUpDown();

        private readonly TableLayoutPanel resumenPanel = new TableLayoutPanel();
        private readonly Label lblResumenNombre = new Label();
        private readonly Label lblResumenMeta = new Label();
        private readonly Label lblResumenConfiguracion = new Label();
        private readonly Label lblResumenIncluye = new Label();
        private readonly Label lblResumenEstimacion = new Label();

        private readonly Label lblEstadoValidacion = new Label();
        private readonly Button btnAgregar = new Button();
        private readonly Action<string> abrirPipelineExistente;
        private bool _isLoading = false;

        public Producto2DDefinicion ProductoSeleccionado { get; private set; }
        public ProyectoProductoBibliotecaAdapterService.OpcionesAgregarProducto Opciones { get; private set; }

        public FormAgregarProductoServicioProyecto(Action<string> abrirPipelineExistente = null)
        {
            this.abrirPipelineExistente = abrirPipelineExistente;
            catalogo = BibliotecaProductos2DJsonService.CargarProductos()
                .Where(p => p != null)
                .GroupBy(p => NormalizarClave(p.Nombre))
                .Select(g => g.First())
                .Where(p => !EsRegistroTecnico(p))
                .OrderBy(p => p.Nombre)
                .Select(p => new CatalogItemView
                {
                    Producto = p,
                    Tipo = ClasificarTipo(p)
                })
                .ToList();

            Text = "Agregar producto o servicio";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1180;
            Height = 700;
            MinimumSize = new Size(1020, 620);
            BackColor = Color.White;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };

            Construir();
            CargarFiltros();
            FiltrarCatalogo();
            ValidarYRefrescar();
        }

        private void Construir()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(12);
            root.ColumnCount = 3;
            root.RowCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

            root.Controls.Add(CrearCatalogo(), 0, 0);
            root.Controls.Add(CrearConfiguracion(), 1, 0);
            root.Controls.Add(CrearResumen(), 2, 0);
            root.Controls.Add(CrearBarraInferior(), 0, 1);
            root.SetColumnSpan(root.Controls[root.Controls.Count - 1], 3);
            Controls.Add(root);
        }

        private Control CrearCatalogo()
        {
            TableLayoutPanel panel = CrearColumna("Catalogo");
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            AgregarCampoCatalogo(panel, "Buscar", txtBuscar);
            txtBuscar.PlaceholderText = "Buscar productos o servicios...";
            txtBuscar.TextChanged += (s, e) => FiltrarCatalogo();

            AgregarCampoCatalogo(panel, "Tipo", cmbTipo);
            AgregarCampoCatalogo(panel, "Categoria", cmbCategoria);
            AgregarCampoCatalogo(panel, "Industria", cmbIndustria);
            foreach (ComboBox combo in new[] { cmbTipo, cmbCategoria, cmbIndustria })
            {
                combo.DropDownStyle = ComboBoxStyle.DropDownList;
                combo.SelectedIndexChanged += (s, e) => FiltrarCatalogo();
            }

            lblResultados.AutoSize = true;
            lblResultados.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            lblResultados.Margin = new Padding(0, 8, 0, 4);
            panel.Controls.Add(lblResultados, 0, panel.RowCount++);
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            lstProductos.Dock = DockStyle.Fill;
            lstProductos.DrawMode = DrawMode.OwnerDrawVariable;
            lstProductos.MeasureItem += (s, e) => e.ItemHeight = 58;
            lstProductos.DrawItem += LstProductos_DrawItem;
            lstProductos.SelectedIndexChanged += (s, e) => ProductoCambiado();
            panel.Controls.Add(lstProductos, 0, panel.RowCount++);
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return panel;
        }

        private Control CrearConfiguracion()
        {
            TableLayoutPanel outer = CrearColumna("Configurar elemento");
            Panel scroll = new Panel();
            scroll.Dock = DockStyle.Fill;
            scroll.AutoScroll = true;
            scroll.BackColor = Color.White;

            FlowLayoutPanel content = new FlowLayoutPanel();
            content.Dock = DockStyle.Top;
            content.AutoSize = true;
            content.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            content.FlowDirection = FlowDirection.TopDown;
            content.WrapContents = false;
            content.Padding = new Padding(0);

            lblNombre.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblNombre.AutoSize = true;
            lblNombre.MaximumSize = new Size(520, 0);
            lblNombre.Text = "Selecciona un elemento";

            lblMeta.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            lblMeta.ForeColor = Color.FromArgb(85, 85, 85);
            lblMeta.AutoSize = true;
            lblMeta.MaximumSize = new Size(520, 0);

            content.Controls.Add(lblNombre);
            content.Controls.Add(lblMeta);
            content.Controls.Add(CrearBloquePrincipal());
            content.Controls.Add(CrearBloqueComponentes());
            content.Controls.Add(CrearBloqueAvanzado());

            scroll.Controls.Add(content);
            outer.Controls.Add(scroll, 0, outer.RowCount++);
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return outer;
        }

        private Control CrearBloquePrincipal()
        {
            TableLayoutPanel bloque = CrearBloque("Configuracion principal");
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Top;
            grid.AutoSize = true;
            grid.ColumnCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            nudCantidad.DecimalPlaces = 2;
            nudCantidad.Minimum = 0.01m;
            nudCantidad.Maximum = 100000m;
            nudCantidad.Value = 1m;
            nudCantidad.Width = 140;
            nudCantidad.ValueChanged += (s, e) =>
            {
                if (!_isLoading) ValidarYRefrescar();
            };

            txtUnidad.Width = 170;
            txtUnidad.TextChanged += (s, e) =>
            {
                if (!_isLoading) ValidarYRefrescar();
            };

            nudDuracion.DecimalPlaces = 2;
            nudDuracion.Maximum = 100000m;
            nudDuracion.Width = 140;
            nudDuracion.ValueChanged += (s, e) =>
            {
                if (!_isLoading) ValidarYRefrescar();
            };

            grid.Controls.Add(CrearCampo("Cantidad", nudCantidad, lblErrorCantidad), 0, 0);
            grid.Controls.Add(CrearCampo("Unidad", txtUnidad, lblErrorUnidad), 1, 0);
            grid.Controls.Add(CrearCampo("Duracion", nudDuracion, lblErrorDuracion), 0, 1);
            bloque.Controls.Add(grid, 0, 1);
            return bloque;
        }

        private Control CrearBloqueComponentes()
        {
            TableLayoutPanel bloque = CrearBloque("Componentes incluidos");
            clbComponentes.CheckOnClick = true;
            clbComponentes.DisplayMember = "Display";
            clbComponentes.Height = 230;
            clbComponentes.Width = 500;
            clbComponentes.ItemCheck += ClbComponentes_ItemCheck;

            lblSinComponentes.Text = "Este elemento no tiene componentes configurados.";
            lblSinComponentes.AutoSize = true;
            lblSinComponentes.ForeColor = Color.FromArgb(90, 90, 90);
            lblSinComponentes.Visible = false;

            bloque.Controls.Add(clbComponentes, 0, 1);
            bloque.Controls.Add(lblSinComponentes, 0, 2);
            return bloque;
        }

        private Control CrearBloqueAvanzado()
        {
            TableLayoutPanel bloque = CrearBloque("");
            btnAvanzado.Text = "Opciones avanzadas";
            btnAvanzado.Width = 180;
            btnAvanzado.Height = 28;
            btnAvanzado.FlatStyle = FlatStyle.Flat;
            btnAvanzado.BackColor = Color.White;
            btnAvanzado.Click += (s, e) =>
            {
                panelAvanzado.Visible = !panelAvanzado.Visible;
                btnAvanzado.Text = panelAvanzado.Visible ? "Ocultar opciones avanzadas" : "Opciones avanzadas";
            };

            panelAvanzado.Visible = false;
            panelAvanzado.AutoSize = true;
            panelAvanzado.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelAvanzado.BackColor = Color.White;

            TableLayoutPanel grid = new TableLayoutPanel();
            grid.AutoSize = true;
            grid.ColumnCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            nudRevisiones.Minimum = 0;
            nudRevisiones.Maximum = 20;
            nudRevisiones.Value = 1;
            nudRevisiones.Width = 120;
            nudRevisiones.ValueChanged += (s, e) =>
            {
                if (!_isLoading) ValidarYRefrescar();
            };

            nudRetrabajo.Minimum = 0;
            nudRetrabajo.Maximum = 100;
            nudRetrabajo.Value = 10;
            nudRetrabajo.Width = 120;
            nudRetrabajo.ValueChanged += (s, e) =>
            {
                if (!_isLoading) ValidarYRefrescar();
            };

            grid.Controls.Add(CrearCampo("Revisiones", nudRevisiones, null), 0, 0);
            grid.Controls.Add(CrearCampo("Retrabajo %", nudRetrabajo, null), 1, 0);
            panelAvanzado.Controls.Add(grid);

            btnEditarAntesAgregar.Text = "Editar pipeline";
            btnEditarAntesAgregar.Width = 210;
            btnEditarAntesAgregar.Height = 30;
            btnEditarAntesAgregar.FlatStyle = FlatStyle.Flat;
            btnEditarAntesAgregar.BackColor = Color.White;
            btnEditarAntesAgregar.Margin = new Padding(0, 10, 0, 0);
            btnEditarAntesAgregar.Click += (s, e) => AbrirPipelineExistente();

            bloque.Controls.Add(btnAvanzado, 0, 0);
            bloque.Controls.Add(panelAvanzado, 0, 1);
            bloque.Controls.Add(btnEditarAntesAgregar, 0, 2);
            return bloque;
        }

        private Control CrearResumen()
        {
            TableLayoutPanel outer = CrearColumna("Resumen de cotizacion");
            resumenPanel.Dock = DockStyle.Fill;
            resumenPanel.AutoScroll = true;
            resumenPanel.ColumnCount = 1;
            resumenPanel.RowCount = 5;
            resumenPanel.Padding = new Padding(0);
            resumenPanel.BackColor = Color.White;

            ConfigurarResumenLabel(lblResumenNombre, 13f, true);
            ConfigurarResumenLabel(lblResumenMeta, 9f, false);
            ConfigurarResumenLabel(lblResumenConfiguracion, 9f, false);
            ConfigurarResumenLabel(lblResumenIncluye, 9f, false);
            ConfigurarResumenLabel(lblResumenEstimacion, 9f, false);

            resumenPanel.Controls.Add(lblResumenNombre, 0, 0);
            resumenPanel.Controls.Add(lblResumenMeta, 0, 1);
            resumenPanel.Controls.Add(lblResumenConfiguracion, 0, 2);
            resumenPanel.Controls.Add(lblResumenIncluye, 0, 3);
            resumenPanel.Controls.Add(lblResumenEstimacion, 0, 4);
            outer.Controls.Add(resumenPanel, 0, outer.RowCount++);
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return outer;
        }

        private Control CrearBarraInferior()
        {
            TableLayoutPanel bar = new TableLayoutPanel();
            bar.Dock = DockStyle.Fill;
            bar.ColumnCount = 2;
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bar.Padding = new Padding(0, 8, 0, 0);

            lblEstadoValidacion.AutoSize = true;
            lblEstadoValidacion.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblEstadoValidacion.Margin = new Padding(0, 9, 0, 0);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.FlowDirection = FlowDirection.RightToLeft;

            btnAgregar.Text = "Agregar";
            ConfigurarBoton(btnAgregar);
            btnAgregar.Click += (s, e) => Confirmar();

            Button cancelar = new Button { Text = "Cancelar" };
            ConfigurarBoton(cancelar);
            cancelar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            acciones.Controls.Add(btnAgregar);
            acciones.Controls.Add(cancelar);
            bar.Controls.Add(lblEstadoValidacion, 0, 0);
            bar.Controls.Add(acciones, 1, 0);
            return bar;
        }

        private void CargarFiltros()
        {
            cmbTipo.Items.Clear();
            cmbTipo.Items.AddRange(new object[] { "Todos", "Productos", "Servicios", "Paquetes" });
            cmbTipo.SelectedIndex = 0;

            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("Todas");
            foreach (string categoria in catalogo.Select(c => c.Categoria).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c))
            {
                cmbCategoria.Items.Add(categoria);
            }
            cmbCategoria.SelectedIndex = 0;

            cmbIndustria.Items.Clear();
            cmbIndustria.Items.Add("Todas");
            foreach (string industria in catalogo.Select(c => c.Industria).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c))
            {
                cmbIndustria.Items.Add(industria);
            }
            cmbIndustria.SelectedIndex = 0;
        }

        private void FiltrarCatalogo()
        {
            string busqueda = (txtBuscar.Text ?? "").Trim().ToLowerInvariant();
            string tipo = Convert.ToString(cmbTipo.SelectedItem) ?? "Todos";
            string categoria = Convert.ToString(cmbCategoria.SelectedItem) ?? "Todas";
            string industria = Convert.ToString(cmbIndustria.SelectedItem) ?? "Todas";

            List<CatalogItemView> filtrados = catalogo
                .Where(c => string.IsNullOrWhiteSpace(busqueda) ||
                    c.Nombre.ToLowerInvariant().Contains(busqueda) ||
                    c.Categoria.ToLowerInvariant().Contains(busqueda) ||
                    (c.Producto?.Nota ?? "").ToLowerInvariant().Contains(busqueda))
                .Where(c => tipo == "Todos" || tipo == c.Tipo + "s")
                .Where(c => categoria == "Todas" || string.Equals(c.Categoria, categoria, StringComparison.OrdinalIgnoreCase))
                .Where(c => industria == "Todas" || string.Equals(c.Industria, industria, StringComparison.OrdinalIgnoreCase))
                .ToList();

            lstProductos.DataSource = null;
            lstProductos.DataSource = filtrados;
            lblResultados.Text = "Resultados · " + filtrados.Count;
            if (filtrados.Count > 0)
            {
                lstProductos.SelectedIndex = 0;
            }
            else
            {
                ProductoCambiado();
            }
        }

        private void ProductoCambiado()
        {
            LoadSelectedCatalogItem();
            return;

            CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
            clbComponentes.Items.Clear();

            if (item == null || item.Producto == null)
            {
                lblNombre.Text = "Selecciona un elemento";
                lblMeta.Text = "";
                lblResumenNombre.Text = "Sin seleccion";
                ValidarYRefrescar();
                return;
            }

            Producto2DDefinicion producto = item.Producto;
            lblNombre.Text = producto.Nombre;
            lblMeta.Text = item.Tipo + " · " + producto.Categoria + " · " + ObtenerUnidadComercial(producto);
            txtUnidad.Text = string.IsNullOrWhiteSpace(ObtenerUnidadComercial(producto)) ? "unidad" : ObtenerUnidadComercial(producto);
            nudDuracion.Value = Convert.ToDecimal(Math.Max(0.0, Math.Min((double)nudDuracion.Maximum, producto.DuracionSugerida)));

            List<Subproducto2D> componentes = (producto.Subproductos ?? new List<Subproducto2D>())
                .Where(s => s != null)
                .GroupBy(s => NormalizarClave(s.Nombre))
                .Select(g => g.First())
                .OrderBy(s => s.Orden)
                .ToList();

            foreach (Subproducto2D sub in componentes)
            {
                clbComponentes.Items.Add(new SubproductoItemView
                {
                    Subproducto = sub,
                    Display = ConstruirDisplaySubproducto(sub)
                }, sub.RequeridoPorDefecto);
            }

            lblSinComponentes.Visible = componentes.Count == 0;
            clbComponentes.Visible = componentes.Count > 0;
            ValidarYRefrescar();
        }

        private void ValidarYRefrescar()
        {
            List<string> errores = ValidarFormulario();
            RefrescarResumen();
            btnAgregar.Enabled = errores.Count == 0;
            lblEstadoValidacion.Text = errores.Count == 0 ? "Configuracion valida." : errores[0];
            lblEstadoValidacion.ForeColor = errores.Count == 0 ? Color.FromArgb(48, 105, 70) : Color.FromArgb(180, 50, 40);
        }

        private void LoadSelectedCatalogItem()
        {
            _isLoading = true;
            try
            {
                CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
                clbComponentes.Items.Clear();

                if (item == null || item.Producto == null)
                {
                    ProductoSeleccionado = null;
                    lblNombre.Text = "Selecciona un elemento";
                    lblMeta.Text = "";
                    lblResumenNombre.Text = "Sin seleccion";
                    lblSinComponentes.Visible = true;
                    clbComponentes.Visible = false;
                    return;
                }

                Producto2DDefinicion producto = item.Producto;
                ProductoSeleccionado = producto;
                lblNombre.Text = producto.Nombre;
                lblMeta.Text = item.Tipo + " Â· " + producto.Categoria + " Â· " + ObtenerUnidadComercial(producto);
                txtUnidad.Text = string.IsNullOrWhiteSpace(ObtenerUnidadComercial(producto)) ? "unidad" : ObtenerUnidadComercial(producto);
                nudCantidad.Value = Math.Max(nudCantidad.Minimum, Math.Min(nudCantidad.Maximum, 1m));
                nudDuracion.Value = Convert.ToDecimal(Math.Max(0.0, Math.Min((double)nudDuracion.Maximum, producto.DuracionSugerida)));

                List<Subproducto2D> componentes = (producto.Subproductos ?? new List<Subproducto2D>())
                    .Where(s => s != null)
                    .GroupBy(s => NormalizarClave(s.Nombre))
                    .Select(g => g.First())
                    .OrderBy(s => s.Orden)
                    .ToList();

                RefreshComponents(componentes);
                lblSinComponentes.Visible = componentes.Count == 0;
                clbComponentes.Visible = componentes.Count > 0;
            }
            finally
            {
                _isLoading = false;
                RefreshAll();
            }
        }

        private void RefreshComponents(List<Subproducto2D> componentes)
        {
            clbComponentes.BeginUpdate();
            try
            {
                clbComponentes.Items.Clear();
                foreach (Subproducto2D sub in componentes ?? new List<Subproducto2D>())
                {
                    clbComponentes.Items.Add(new SubproductoItemView
                    {
                        Subproducto = sub,
                        Display = ConstruirDisplaySubproducto(sub)
                    }, sub.RequeridoPorDefecto);
                }
            }
            finally
            {
                clbComponentes.EndUpdate();
            }
        }

        private void RefreshAll()
        {
            ValidarYRefrescar();
        }

        private List<string> ValidarFormulario()
        {
            List<string> errores = new List<string>();
            CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
            lblErrorCantidad.Text = "";
            lblErrorUnidad.Text = "";
            lblErrorDuracion.Text = "";

            if (item == null)
            {
                errores.Add("Selecciona un elemento del catalogo.");
                return errores;
            }

            if (nudCantidad.Value <= 0)
            {
                lblErrorCantidad.Text = "Debe ser mayor que cero.";
                errores.Add("Debe ingresar una cantidad mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(txtUnidad.Text))
            {
                lblErrorUnidad.Text = "Unidad requerida.";
                errores.Add("El producto no tiene una unidad configurada.");
            }

            if (RequiereDuracion(item.Producto) && nudDuracion.Value <= 0)
            {
                lblErrorDuracion.Text = "Duracion requerida.";
                errores.Add("Debe ingresar una duracion valida.");
            }

            if (!ObligatoriosIncluidos())
            {
                errores.Add("Los componentes obligatorios deben estar incluidos.");
            }

            if (clbComponentes.Items.Count > 0 && ObtenerComponentesSeleccionados().Count == 0)
            {
                errores.Add("Selecciona al menos un componente.");
            }

            return errores;
        }

        private void RefrescarResumen()
        {
            CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
            if (item == null)
            {
                lblResumenNombre.Text = "Sin seleccion";
                lblResumenMeta.Text = "";
                lblResumenConfiguracion.Text = "";
                lblResumenIncluye.Text = "";
                lblResumenEstimacion.Text = "";
                return;
            }

            List<Subproducto2D> incluidos = ObtenerComponentesSeleccionados();
            lblResumenNombre.Text = item.Nombre;
            lblResumenMeta.Text = item.Tipo + " · " + item.Categoria;
            lblResumenConfiguracion.Text =
                "CONFIGURACION" + Environment.NewLine +
                "Cantidad: " + nudCantidad.Value.ToString("0.##") + Environment.NewLine +
                "Unidad: " + txtUnidad.Text.Trim() + Environment.NewLine +
                (RequiereDuracion(item.Producto)
                    ? "Duracion: " + nudDuracion.Value.ToString("0.##") + " " + item.Producto.UnidadDuracionSugerida + Environment.NewLine
                    : "") +
                "Revisiones: " + nudRevisiones.Value.ToString("0") + Environment.NewLine;

            lblResumenIncluye.Text =
                "INCLUYE" + Environment.NewLine +
                (incluidos.Count == 0
                    ? "Sin componentes configurados."
                    : string.Join(Environment.NewLine, incluidos.Take(12).Select(s => s.Nombre)));

            lblResumenEstimacion.Text =
                "ESTIMACION" + Environment.NewLine +
                "Horas: -" + Environment.NewLine +
                "Plazo: -" + Environment.NewLine +
                "Costo: -" + Environment.NewLine +
                "Precio: -";
        }

        private void Confirmar()
        {
            List<string> errores = ValidarFormulario();
            if (errores.Count > 0)
            {
                ValidarYRefrescar();
                return;
            }

            CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
            if (item == null)
            {
                return;
            }

            ProductoSeleccionado = item.Producto;
            Opciones = CrearOpcionesActuales();

            DialogResult = DialogResult.OK;
            Close();
        }

        private ProyectoProductoBibliotecaAdapterService.OpcionesAgregarProducto CrearOpcionesActuales()
        {
            return new ProyectoProductoBibliotecaAdapterService.OpcionesAgregarProducto
            {
                Cantidad = nudCantidad.Value,
                Unidad = txtUnidad.Text.Trim(),
                ModoCantidad = ModoCantidadSubproducto.Homogeneo,
                Complejidad = "",
                Acabado = "",
                CiclosRevision = Convert.ToInt32(nudRevisiones.Value),
                TasaRetrabajo = nudRetrabajo.Value,
                Duracion = nudDuracion.Value,
                SubproductosSeleccionados = ObtenerComponentesSeleccionados()
            };
        }

        private void AbrirPipelineExistente()
        {
            CatalogItemView item = lstProductos.SelectedItem as CatalogItemView;
            if (item == null || item.Producto == null)
            {
                ValidarYRefrescar();
                return;
            }

            if (abrirPipelineExistente == null)
            {
                MessageBox.Show(
                    this,
                    "El editor de pipeline no está disponible desde esta ventana.",
                    "Pipeline de producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            Hide();
            try
            {
                abrirPipelineExistente(item.Producto.Nombre);
            }
            finally
            {
                Show();
                Activate();
                Producto2DDefinicion actualizado = BibliotecaProductos2DJsonService.CargarProductos()
                    .FirstOrDefault(p => p != null &&
                        string.Equals(p.Nombre, item.Producto.Nombre, StringComparison.OrdinalIgnoreCase));
                if (actualizado != null)
                {
                    item.Producto = actualizado;
                    ProductoSeleccionado = actualizado;
                }
                LoadSelectedCatalogItem();
            }
        }

        private void ClbComponentes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            if (e.Index < 0 || e.Index >= clbComponentes.Items.Count)
            {
                return;
            }

            SubproductoItemView item = clbComponentes.Items[e.Index] as SubproductoItemView;
            if (item?.Subproducto?.RequeridoPorDefecto == true && e.NewValue == CheckState.Unchecked)
            {
                e.NewValue = CheckState.Checked;
            }

            BeginInvoke(new Action(ValidarYRefrescar));
        }

        private List<Subproducto2D> ObtenerComponentesSeleccionados()
        {
            return clbComponentes.CheckedItems
                .Cast<SubproductoItemView>()
                .Where(i => i?.Subproducto != null)
                .Select(i => i.Subproducto)
                .ToList();
        }

        private bool ObligatoriosIncluidos()
        {
            for (int i = 0; i < clbComponentes.Items.Count; i++)
            {
                SubproductoItemView item = clbComponentes.Items[i] as SubproductoItemView;
                if (item?.Subproducto?.RequeridoPorDefecto == true && !clbComponentes.GetItemChecked(i))
                {
                    return false;
                }
            }
            return true;
        }

        private static TipoElementoCotizable ClasificarTipo(Producto2DDefinicion producto)
        {
            string texto = ((producto?.Categoria ?? "") + " " + (producto?.Nombre ?? "") + " " + (producto?.Nota ?? "")).ToLowerInvariant();
            if (texto.Contains("pack") || texto.Contains("paquete") || texto.Contains("bundle"))
            {
                return TipoElementoCotizable.Paquete;
            }
            if (texto.Contains("servicio") || texto.Contains("direccion") || texto.Contains("consultoria") || texto.Contains("audio") || texto.Contains("sonido"))
            {
                return TipoElementoCotizable.Servicio;
            }
            return TipoElementoCotizable.Producto;
        }

        private static bool EsRegistroTecnico(Producto2DDefinicion producto)
        {
            string nombre = producto?.Nombre ?? "";
            return nombre.EndsWith(" copia", StringComparison.OrdinalIgnoreCase) ||
                nombre.Contains(" temporal", StringComparison.OrdinalIgnoreCase) ||
                nombre.Contains(" prueba", StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerUnidadComercial(Producto2DDefinicion producto)
        {
            if (producto == null)
            {
                return "";
            }
            if (!string.IsNullOrWhiteSpace(producto.UnidadComercialPrincipal))
            {
                return producto.UnidadComercialPrincipal;
            }
            if (!string.IsNullOrWhiteSpace(producto.UnidadDuracionSugerida) &&
                !string.Equals(producto.UnidadDuracionSugerida, "no aplica", StringComparison.OrdinalIgnoreCase))
            {
                return producto.UnidadDuracionSugerida;
            }
            return string.IsNullOrWhiteSpace(producto.UnidadCantidadSugerida) ? "unidad" : producto.UnidadCantidadSugerida;
        }

        private static bool RequiereDuracion(Producto2DDefinicion producto)
        {
            return producto != null &&
                !string.IsNullOrWhiteSpace(producto.UnidadDuracionSugerida) &&
                !string.Equals(producto.UnidadDuracionSugerida, "no aplica", StringComparison.OrdinalIgnoreCase);
        }

        private static string ConstruirDisplaySubproducto(Subproducto2D sub)
        {
            string obligatorio = sub.RequeridoPorDefecto ? "Obligatorio" : "Opcional";
            string etapa = string.IsNullOrWhiteSpace(sub.EtapaSugerida) ? sub.Categoria : sub.EtapaSugerida;
            return sub.Nombre + " · " + etapa + " · " + obligatorio;
        }

        private static string NormalizarClave(string texto)
        {
            return new string((texto ?? "").Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        private static TableLayoutPanel CrearColumna(string titulo)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = new Padding(0, 0, 10, 0);
            panel.Padding = new Padding(10);
            panel.BackColor = Color.FromArgb(248, 248, 248);
            panel.ColumnCount = 1;
            panel.RowCount = 1;
            Label label = new Label();
            label.Text = titulo;
            label.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            label.AutoSize = true;
            label.Margin = new Padding(0, 0, 0, 8);
            panel.Controls.Add(label, 0, 0);
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return panel;
        }

        private static void AgregarCampoCatalogo(TableLayoutPanel panel, string etiqueta, Control control)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            lbl.Margin = new Padding(0, 5, 0, 1);
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(0, 0, 0, 2);
            panel.Controls.Add(lbl, 0, panel.RowCount++);
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(control, 0, panel.RowCount++);
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        private static TableLayoutPanel CrearBloque(string titulo)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Width = 520;
            panel.ColumnCount = 1;
            panel.Margin = new Padding(0, 12, 0, 0);
            panel.Padding = new Padding(10);
            panel.BackColor = Color.White;
            if (!string.IsNullOrWhiteSpace(titulo))
            {
                Label lbl = new Label();
                lbl.Text = titulo;
                lbl.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                lbl.AutoSize = true;
                panel.Controls.Add(lbl, 0, 0);
            }
            return panel;
        }

        private static Control CrearCampo(string etiqueta, Control control, Label error)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.FlowDirection = FlowDirection.TopDown;
            panel.WrapContents = false;
            panel.AutoSize = true;
            panel.Margin = new Padding(0, 0, 16, 8);

            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            panel.Controls.Add(lbl);
            panel.Controls.Add(control);
            if (error != null)
            {
                panel.Controls.Add(error);
            }
            return panel;
        }

        private static Label CrearErrorLabel()
        {
            return new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(180, 50, 40),
                Font = new Font("Segoe UI", 8f)
            };
        }

        private static void ConfigurarResumenLabel(Label label, float size, bool bold)
        {
            label.AutoSize = true;
            label.MaximumSize = new Size(310, 0);
            label.Margin = new Padding(0, 0, 0, 14);
            label.Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        private static void ConfigurarBoton(Button boton)
        {
            boton.Width = 120;
            boton.Height = 32;
            boton.Margin = new Padding(8, 4, 0, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.BackColor = Color.White;
        }

        private void LstProductos_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= lstProductos.Items.Count)
            {
                return;
            }

            CatalogItemView item = lstProductos.Items[e.Index] as CatalogItemView;
            if (item == null)
            {
                return;
            }

            Color texto = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? Color.White : Color.Black;
            using Brush brush = new SolidBrush(texto);
            using Font nombre = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            using Font meta = new Font("Segoe UI", 8.2f);
            e.Graphics.DrawString(item.Nombre, nombre, brush, e.Bounds.Left + 8, e.Bounds.Top + 6);
            e.Graphics.DrawString(item.Tipo + " · " + item.Categoria + " · " + item.Unidad, meta, brush, e.Bounds.Left + 8, e.Bounds.Top + 30);
            e.DrawFocusRectangle();
        }
    }
}
