using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Cotizador_animacion_Othalart.Services;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabMoneda(TabPage tab)
        {
            tab.Controls.Clear();

            

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(28, 24, 28, 24);
            layout.ColumnCount = 1;
            layout.RowCount = 5;
            layout.BackColor = Color.Transparent;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // título
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // texto monedas actuales
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));   // controles
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // tabla
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));   // estado / navegación

            Label titulo = new Label();
            titulo.Text = "Moneda y tipos de cambio";
            titulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;
            titulo.ForeColor = Color.Black;

            lblMonedaClienteActual.Dock = DockStyle.Fill;
            lblMonedaClienteActual.ForeColor = Color.DimGray;
            lblMonedaClienteActual.TextAlign = ContentAlignment.MiddleLeft;
            lblMonedaClienteActual.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 4;
            panelSuperior.RowCount = 1;
            panelSuperior.Margin = new Padding(0, 2, 0, 8);
            panelSuperior.BackColor = Color.White;

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label lblMoneda = new Label();
            lblMoneda.Text = "Moneda de visualización:";
            lblMoneda.Dock = DockStyle.Fill;
            lblMoneda.TextAlign = ContentAlignment.MiddleLeft;
            lblMoneda.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblMoneda.ForeColor = Color.Black;

            cmbMonedaVisualizacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonedaVisualizacion.Dock = DockStyle.Fill;
            cmbMonedaVisualizacion.Margin = new Padding(0, 8, 10, 8);
            cmbMonedaVisualizacion.SelectedIndexChanged -= CmbMonedaVisualizacion_SelectedIndexChanged;
            cmbMonedaVisualizacion.SelectedIndexChanged += CmbMonedaVisualizacion_SelectedIndexChanged;

            btnActualizarTiposCambio.Text = "Actualizar online";
            btnActualizarTiposCambio.Dock = DockStyle.None;
            btnActualizarTiposCambio.Width = 140;
            btnActualizarTiposCambio.Height = 28;
            btnActualizarTiposCambio.Margin = new Padding(0, 7, 16, 7);
            btnActualizarTiposCambio.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            btnActualizarTiposCambio.Click -= BtnActualizarTiposCambio_Click;
            btnActualizarTiposCambio.Click += BtnActualizarTiposCambio_Click;

            Label lblAuto = new Label();
            lblAuto.Text = "Los cambios se aplican automáticamente.";
            lblAuto.Dock = DockStyle.Fill;
            lblAuto.ForeColor = Color.DimGray;
            lblAuto.TextAlign = ContentAlignment.MiddleLeft;
            lblAuto.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            panelSuperior.Controls.Add(lblMoneda, 0, 0);
            panelSuperior.Controls.Add(cmbMonedaVisualizacion, 1, 0);
            panelSuperior.Controls.Add(btnActualizarTiposCambio, 2, 0);
            panelSuperior.Controls.Add(lblAuto, 3, 0);

            ConfigurarTablaTiposCambio();

            Panel panelInferior = new Panel();
            panelInferior.Dock = DockStyle.Fill;
            panelInferior.BackColor = Color.FromArgb(248, 248, 248);
            panelInferior.Padding = new Padding(14, 10, 14, 10);
            panelInferior.Margin = new Padding(0, 10, 0, 0);

            TableLayoutPanel layoutInferior = new TableLayoutPanel();
            layoutInferior.Dock = DockStyle.Fill;
            layoutInferior.ColumnCount = 2;
            layoutInferior.RowCount = 1;
            layoutInferior.BackColor = Color.FromArgb(248, 248, 248);

            layoutInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layoutInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 235));

            lblEstadoTiposCambio.Dock = DockStyle.Fill;
            lblEstadoTiposCambio.TextAlign = ContentAlignment.MiddleLeft;
            lblEstadoTiposCambio.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblEstadoTiposCambio.ForeColor = Color.Black;
            lblEstadoTiposCambio.Text = "";

            Button btnIrEtapas = new Button();
            btnIrEtapas.Text = "Avanzar a etapas";
            btnIrEtapas.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnIrEtapas.Dock = DockStyle.Fill;
            btnIrEtapas.Margin = new Padding(12, 4, 0, 4);
            btnIrEtapas.Click += (sender, e) => IrATabPorNombre("Etapas");

            layoutInferior.Controls.Add(lblEstadoTiposCambio, 0, 0);
            layoutInferior.Controls.Add(btnIrEtapas, 1, 0);

            panelInferior.Controls.Add(layoutInferior);

            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(lblMonedaClienteActual, 0, 1);
            layout.Controls.Add(panelSuperior, 0, 2);
            layout.Controls.Add(dgvTiposCambio, 0, 3);
            layout.Controls.Add(panelInferior, 0, 4);

            tab.Controls.Add(layout);

            InicializarTiposCambio();
            CargarCombosMonedas();
            CargarTablaTiposCambio();
            ActualizarTextoMonedasActuales();
            ActivarActualizacionGanttEnVivo();
        }

        private void ConfigurarTablaTiposCambio()
        {
            dgvTiposCambio.Dock = DockStyle.Fill;
            dgvTiposCambio.Margin = new Padding(0, 8, 0, 0);
            dgvTiposCambio.AllowUserToAddRows = false;
            dgvTiposCambio.AllowUserToDeleteRows = false;
            dgvTiposCambio.AllowUserToResizeRows = false;
            dgvTiposCambio.RowHeadersVisible = false;
            dgvTiposCambio.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTiposCambio.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTiposCambio.MultiSelect = false;
            dgvTiposCambio.BackgroundColor = Color.White;
            dgvTiposCambio.BorderStyle = BorderStyle.FixedSingle;
            dgvTiposCambio.GridColor = Color.Gainsboro;
            dgvTiposCambio.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvTiposCambio.EnableHeadersVisualStyles = false;
            dgvTiposCambio.ReadOnly = false;

            dgvTiposCambio.CellValueChanged -= DgvTiposCambio_CellValueChanged;
            dgvTiposCambio.CellBeginEdit -= DgvTiposCambio_CellBeginEdit;
            dgvTiposCambio.CellBeginEdit += DgvTiposCambio_CellBeginEdit;
            dgvTiposCambio.CellFormatting -= DgvTiposCambio_CellFormatting;
            dgvTiposCambio.DataError -= DgvTiposCambio_DataError;

            dgvTiposCambio.Columns.Clear();

            dgvTiposCambio.Columns.Add("Codigo", "Código");
            dgvTiposCambio.Columns.Add("Nombre", "Nombre");
            dgvTiposCambio.Columns.Add("ValorEnCLP", "Valor en CLP");
            dgvTiposCambio.Columns.Add("ValorVisual", "Valor visual");
            dgvTiposCambio.Columns.Add("Fuente", "Fuente");
            dgvTiposCambio.Columns.Add("Fecha", "Fecha actualización");

            foreach (DataGridViewColumn columna in dgvTiposCambio.Columns)
            {
                columna.ReadOnly = true;
            }

            dgvTiposCambio.Columns["ValorEnCLP"].ReadOnly = false;

            dgvTiposCambio.Columns["Codigo"].FillWeight = 75;
            dgvTiposCambio.Columns["Nombre"].FillWeight = 150;
            dgvTiposCambio.Columns["ValorEnCLP"].FillWeight = 115;
            dgvTiposCambio.Columns["ValorVisual"].FillWeight = 120;
            dgvTiposCambio.Columns["Fuente"].FillWeight = 115;
            dgvTiposCambio.Columns["Fecha"].FillWeight = 125;

            dgvTiposCambio.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTiposCambio.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvTiposCambio.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvTiposCambio.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            dgvTiposCambio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvTiposCambio.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            dgvTiposCambio.DefaultCellStyle.BackColor = Color.White;
            dgvTiposCambio.DefaultCellStyle.ForeColor = Color.Black;
            dgvTiposCambio.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvTiposCambio.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvTiposCambio.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            dgvTiposCambio.CellValueChanged += DgvTiposCambio_CellValueChanged;
            dgvTiposCambio.CellBeginEdit += DgvTiposCambio_CellBeginEdit;
            dgvTiposCambio.CellFormatting += DgvTiposCambio_CellFormatting;
            dgvTiposCambio.DataError += DgvTiposCambio_DataError;
        }

        private void DgvTiposCambio_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (dgvTiposCambio.Columns[e.ColumnIndex].Name != "ValorEnCLP")
            {
                e.Cancel = true;
                return;
            }

            DataGridViewRow row = dgvTiposCambio.Rows[e.RowIndex];

            if (row.Tag is not TipoCambio tipo)
            {
                e.Cancel = true;
                return;
            }

            if (!PuedeEditarTipoCambio(tipo.Codigo, tipo.Fuente))
            {
                e.Cancel = true;
            }
        }

        private void InicializarMonedaVisualDependienteDeTabla()
        {
            if (monedaVisualDesdeTablaInicializada)
            {
                ConectarClicksTablaTiposCambio();
                RefrescarTextoMonedaVisual();
                MarcarFilaMonedaVisualActual();
                return;
            }

            monedaVisualDesdeTablaInicializada = true;

            ReemplazarComboMonedaVisualPorLabel();
            ConectarClicksTablaTiposCambio();
            RefrescarTextoMonedaVisual();
            MarcarFilaMonedaVisualActual();
        }

        private void ReemplazarComboMonedaVisualPorLabel()
        {
            if (lblMonedaVisualizacionActual != null)
            {
                return;
            }

            if (cmbMonedaVisualizacion == null)
            {
                return;
            }

            Control parent = cmbMonedaVisualizacion.Parent;

            if (parent == null)
            {
                return;
            }

            lblMonedaVisualizacionActual = new Label();
            lblMonedaVisualizacionActual.Name = "lblMonedaVisualizacionActual";
            lblMonedaVisualizacionActual.AutoSize = false;
            lblMonedaVisualizacionActual.BorderStyle = BorderStyle.FixedSingle;
            lblMonedaVisualizacionActual.Font = cmbMonedaVisualizacion.Font;
            lblMonedaVisualizacionActual.Location = cmbMonedaVisualizacion.Location;
            lblMonedaVisualizacionActual.Size = cmbMonedaVisualizacion.Size;
            lblMonedaVisualizacionActual.BackColor = Color.White;
            lblMonedaVisualizacionActual.ForeColor = Color.FromArgb(20, 20, 20);
            lblMonedaVisualizacionActual.TextAlign = ContentAlignment.MiddleLeft;
            lblMonedaVisualizacionActual.Padding = new Padding(6, 0, 0, 0);

            parent.Controls.Add(lblMonedaVisualizacionActual);
            lblMonedaVisualizacionActual.BringToFront();

            cmbMonedaVisualizacion.Visible = false;
            cmbMonedaVisualizacion.Enabled = false;
            cmbMonedaVisualizacion.TabStop = false;
        }

        private void ConectarClicksTablaTiposCambio()
        {
            if (dgvTiposCambio == null)
            {
                return;
            }

            dgvTiposCambio.CellClick -= TablaTiposCambio_CellClick;
            dgvTiposCambio.CellClick += TablaTiposCambio_CellClick;

            dgvTiposCambio.CellMouseClick -= TablaTiposCambio_CellMouseClick;
            dgvTiposCambio.CellMouseClick += TablaTiposCambio_CellMouseClick;

            dgvTiposCambio.RowHeaderMouseClick -= TablaTiposCambio_RowHeaderMouseClick;
            dgvTiposCambio.RowHeaderMouseClick += TablaTiposCambio_RowHeaderMouseClick;

            dgvTiposCambio.MultiSelect = false;
            dgvTiposCambio.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void TablaTiposCambio_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView grid = sender as DataGridView;

            if (grid == null)
            {
                return;
            }

            /*
             * Si el usuario hace click en ValorEnCLP y la moneda es editable,
             * NO cambiamos moneda visual. Entramos en modo edición.
             */
            if (e.ColumnIndex >= 0 &&
                grid.Columns[e.ColumnIndex].Name == "ValorEnCLP")
            {
                DataGridViewRow row = grid.Rows[e.RowIndex];

                if (row.Tag is TipoCambio tipo &&
                    PuedeEditarTipoCambio(tipo.Codigo, tipo.Fuente))
                {
                    grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    grid.CurrentCell = row.Cells[e.ColumnIndex];
                    grid.BeginEdit(true);
                    return;
                }
            }

            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CambiarMonedaVisualDesdeFilaTabla(grid.Rows[e.RowIndex]);
        }

        private void TablaTiposCambio_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView grid = sender as DataGridView;

            if (grid == null)
            {
                return;
            }

            /*
             * Si el click fue sobre ValorEnCLP editable,
             * dejamos que el usuario edite. No cambiamos moneda visual.
             */
            if (e.ColumnIndex >= 0 &&
                grid.Columns[e.ColumnIndex].Name == "ValorEnCLP")
            {
                DataGridViewRow row = grid.Rows[e.RowIndex];

                if (row.Tag is TipoCambio tipo &&
                    PuedeEditarTipoCambio(tipo.Codigo, tipo.Fuente))
                {
                    grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
                    grid.CurrentCell = row.Cells[e.ColumnIndex];
                    grid.BeginEdit(true);
                    return;
                }
            }

            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            CambiarMonedaVisualDesdeFilaTabla(grid.Rows[e.RowIndex]);
        }

        private void TablaTiposCambio_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView grid = sender as DataGridView;

            if (grid == null)
            {
                return;
            }

            CambiarMonedaVisualDesdeFilaTabla(grid.Rows[e.RowIndex]);
        }

        private void CambiarMonedaVisualDesdeFilaTabla(DataGridViewRow fila)
        {
            if (actualizandoMonedaVisualDesdeTabla)
            {
                return;
            }

            if (fila == null || fila.IsNewRow)
            {
                return;
            }

            string codigo = ObtenerCodigoMonedaDesdeFila(fila);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return;
            }

            codigo = codigo.Trim().ToUpperInvariant();

            if (cotizacion == null)
            {
                return;
            }

            actualizandoMonedaVisualDesdeTabla = true;

            try
            {
                cotizacion.MonedaVisualizacion = codigo;

                if (cmbMonedaVisualizacion != null)
                {
                    cmbMonedaVisualizacion.SelectedItem = codigo;
                    cmbMonedaVisualizacion.Text = codigo;
                }

                RefrescarTextoMonedaVisual();

                if (lblEstadoTiposCambio != null)
                {
                    lblEstadoTiposCambio.Text =
                        $"Moneda de visualización seleccionada desde tabla: {codigo}.";
                }

                RefrescarTodo();

                ConectarClicksTablaTiposCambio();
                RefrescarTextoMonedaVisual();
                MarcarFilaMonedaVisualActual();
            }
            finally
            {
                actualizandoMonedaVisualDesdeTabla = false;
            }
        }

        private string ObtenerCodigoMonedaDesdeFila(DataGridViewRow fila)
        {
            if (fila == null || fila.DataGridView == null)
            {
                return string.Empty;
            }

            DataGridView grid = fila.DataGridView;

            foreach (DataGridViewColumn columna in grid.Columns)
            {
                string nombreColumna = columna.Name ?? string.Empty;
                string textoColumna = columna.HeaderText ?? string.Empty;

                bool esColumnaCodigo =
                    nombreColumna.Equals("Codigo", StringComparison.OrdinalIgnoreCase) ||
                    nombreColumna.Equals("Código", StringComparison.OrdinalIgnoreCase) ||
                    textoColumna.Equals("Codigo", StringComparison.OrdinalIgnoreCase) ||
                    textoColumna.Equals("Código", StringComparison.OrdinalIgnoreCase);

                if (!esColumnaCodigo)
                {
                    continue;
                }

                object valor = fila.Cells[columna.Index].Value;

                if (valor == null)
                {
                    return string.Empty;
                }

                return valor.ToString();
            }

            if (fila.Cells.Count > 0 && fila.Cells[0].Value != null)
            {
                return fila.Cells[0].Value.ToString();
            }

            return string.Empty;
        }

        private void RefrescarTextoMonedaVisual()
        {
            string moneda = "CLP";

            if (cotizacion != null && !string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion))
            {
                moneda = cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();
            }

            if (lblMonedaVisualizacionActual != null)
            {
                lblMonedaVisualizacionActual.Text = moneda;
            }
        }

        private void MarcarFilaMonedaVisualActual()
        {
            if (dgvTiposCambio == null || cotizacion == null)
            {
                return;
            }

            string moneda = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            foreach (DataGridViewRow fila in dgvTiposCambio.Rows)
            {
                if (fila == null || fila.IsNewRow)
                {
                    continue;
                }

                string codigo = ObtenerCodigoMonedaDesdeFila(fila);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    continue;
                }

                if (codigo.Trim().ToUpperInvariant() == moneda)
                {
                    dgvTiposCambio.ClearSelection();
                    fila.Selected = true;

                    if (fila.Cells.Count > 0)
                    {
                        dgvTiposCambio.CurrentCell = fila.Cells[0];
                    }

                    return;
                }
            }
        }

        private void InicializarTiposCambio()
        {
            if (cotizacion == null)
            {
                return;
            }

            MonedaService.InicializarTiposCambio(cotizacion);

            AsegurarTipoCambio("KRW", "Won surcoreano", 0.7, "Respaldo manual");
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorEnCLP, string fuente)
        {
            if (cotizacion == null)
            {
                return;
            }

            if (cotizacion.TiposCambio == null)
            {
                cotizacion.TiposCambio = new System.Collections.Generic.List<TipoCambio>();
            }

            TipoCambio? tipo = cotizacion.TiposCambio
                .FirstOrDefault(t => string.Equals(t.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

            if (tipo == null)
            {
                cotizacion.TiposCambio.Add(new TipoCambio
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    ValorEnCLP = valorEnCLP,
                    Fuente = fuente,
                    FechaActualizacion = DateTime.Now
                });

                return;
            }

            if (string.IsNullOrWhiteSpace(tipo.Codigo))
            {
                tipo.Codigo = codigo;
            }

            if (string.IsNullOrWhiteSpace(tipo.Nombre))
            {
                tipo.Nombre = nombre;
            }

            if (tipo.ValorEnCLP <= 0.0)
            {
                tipo.ValorEnCLP = valorEnCLP;
            }

            if (string.IsNullOrWhiteSpace(tipo.Fuente))
            {
                tipo.Fuente = fuente;
            }

            if (tipo.FechaActualizacion == default)
            {
                tipo.FechaActualizacion = DateTime.Now;
            }
        }

        private void CargarCombosMonedas()
        {
            if (cotizacion == null)
            {
                return;
            }

            InicializarTiposCambio();

            string monedaActual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            cmbMonedaVisualizacion.SelectedIndexChanged -= CmbMonedaVisualizacion_SelectedIndexChanged;

            cmbMonedaVisualizacion.Items.Clear();

            foreach (string codigo in MonedaService.ObtenerCodigosMonedaDisponibles(cotizacion))
            {
                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    cmbMonedaVisualizacion.Items.Add(codigo.Trim().ToUpperInvariant());
                }
            }

            if (cmbMonedaVisualizacion.Items.Contains(monedaActual))
            {
                cmbMonedaVisualizacion.SelectedItem = monedaActual;
            }
            else if (cmbMonedaVisualizacion.Items.Count > 0)
            {
                cmbMonedaVisualizacion.SelectedIndex = 0;
                cotizacion.MonedaVisualizacion = cmbMonedaVisualizacion.SelectedItem?.ToString() ?? "CLP";
            }

            cmbMonedaVisualizacion.SelectedIndexChanged += CmbMonedaVisualizacion_SelectedIndexChanged;

            ActualizarTextoMonedasActuales();
        }

        private void CargarTablaTiposCambio()
        {
            if (dgvTiposCambio == null || dgvTiposCambio.Columns.Count == 0)
            {
                return;
            }

            if (cotizacion == null)
            {
                return;
            }

            InicializarTiposCambio();

            cargandoTabla = true;
            dgvTiposCambio.CellValueChanged -= DgvTiposCambio_CellValueChanged;

            dgvTiposCambio.Rows.Clear();

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            if (monedaVisual == "GBP")
            {
                monedaVisual = "CLP";
                cotizacion.MonedaVisualizacion = "CLP";
            }

            if (dgvTiposCambio.Columns.Contains("ValorVisual"))
            {
                dgvTiposCambio.Columns["ValorVisual"].HeaderText = $"Valor en {monedaVisual}";
            }

            foreach (TipoCambio tipo in cotizacion.TiposCambio.OrderBy(t => OrdenMoneda(t.Codigo)))
            {
                if (tipo == null)
                {
                    continue;
                }

                string codigo = string.IsNullOrWhiteSpace(tipo.Codigo)
                    ? ""
                    : tipo.Codigo.Trim().ToUpperInvariant();

                if (codigo == "GBP")
                {
                    continue;
                }

                int rowIndex = dgvTiposCambio.Rows.Add();
                DataGridViewRow row = dgvTiposCambio.Rows[rowIndex];

                row.Cells["Codigo"].Value = codigo;
                row.Cells["Nombre"].Value = tipo.Nombre;
                row.Cells["ValorEnCLP"].Value = FormatearNumeroTipoCambio(tipo.ValorEnCLP);
                row.Cells["ValorVisual"].Value = FormatearValorConvertidoTipoCambio(tipo.ValorEnCLP, monedaVisual);
                row.Cells["Fuente"].Value = tipo.Fuente;
                row.Cells["Fecha"].Value = tipo.FechaActualizacion == default
                    ? ""
                    : tipo.FechaActualizacion.ToString("dd-MM-yyyy HH:mm");

                row.Tag = tipo;

                bool editable = PuedeEditarTipoCambio(codigo, tipo.Fuente);

                row.Cells["ValorEnCLP"].ReadOnly = !editable;

                if (!editable)
                {
                    row.DefaultCellStyle.ForeColor = Color.DimGray;
                }
            }

            dgvTiposCambio.CellValueChanged += DgvTiposCambio_CellValueChanged;
            cargandoTabla = false;

            ActualizarTextoMonedasActuales();
        }

        private void DgvTiposCambio_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (e.RowIndex >= dgvTiposCambio.Rows.Count)
            {
                return;
            }

            if (dgvTiposCambio.Columns[e.ColumnIndex].Name != "ValorEnCLP")
            {
                return;
            }

            DataGridViewRow row = dgvTiposCambio.Rows[e.RowIndex];

            if (row.Tag is not TipoCambio tipo)
            {
                return;
            }

            if (!PuedeEditarTipoCambio(tipo.Codigo, tipo.Fuente))
            {
                CargarTablaTiposCambio();
                return;
            }

            double valor = ConvertirDoubleMoneda(row.Cells["ValorEnCLP"].Value);

            if (valor <= 0.0)
            {
                MessageBox.Show(
                    "El tipo de cambio debe ser mayor que cero.",
                    "Valor inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                CargarTablaTiposCambio();
                return;
            }

            tipo.ValorEnCLP = valor;

            if (!string.IsNullOrWhiteSpace(tipo.Fuente) &&
                tipo.Fuente.ToLowerInvariant().Contains("interno"))
            {
                tipo.Fuente = "Manual interno";
            }
            else
            {
                tipo.Fuente = "Respaldo manual";
            }

            tipo.FechaActualizacion = DateTime.Now;

            CargarTablaTiposCambio();
            RefrescarCalculosYVista();
        }

        

        private bool PuedeEditarTipoCambio(string? codigo, string? fuente)
        {
            codigo = string.IsNullOrWhiteSpace(codigo)
                ? ""
                : codigo.Trim().ToUpperInvariant();

            fuente = string.IsNullOrWhiteSpace(fuente)
                ? ""
                : fuente.Trim().ToLowerInvariant();

            if (codigo == "CLP")
            {
                return false;
            }

            if (fuente.Contains("manual"))
            {
                return true;
            }

            if (fuente.Contains("respaldo"))
            {
                return true;
            }

            return false;
        }

        private void DgvTiposCambio_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvTiposCambio == null)
            {
                return;
            }

            DataGridViewRow row = dgvTiposCambio.Rows[e.RowIndex];

            if (row.Tag is not TipoCambio tipo)
            {
                return;
            }

            string codigo = string.IsNullOrWhiteSpace(tipo.Codigo)
                ? ""
                : tipo.Codigo.Trim().ToUpperInvariant();

            bool editable = PuedeEditarTipoCambio(tipo.Codigo, tipo.Fuente);

            if (editable)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 225);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(35, 35, 35);
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.FromArgb(80, 80, 80);
            }

            if (cotizacion != null &&
                !string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion) &&
                codigo == cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant())
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(225, 240, 255);
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
        }

        private void DgvTiposCambio_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = true;
        }

        private void CmbMonedaVisualizacion_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cotizacion == null)
            {
                return;
            }

            string moneda = cmbMonedaVisualizacion.SelectedItem?.ToString() ?? "CLP";
            moneda = moneda.Trim().ToUpperInvariant();

            cotizacion.MonedaVisualizacion = moneda;

            CargarTablaTiposCambio();
            RefrescarCalculosYVista();
        }

        private async void BtnActualizarTiposCambio_Click(object? sender, EventArgs e)
        {
            btnActualizarTiposCambio.Enabled = false;
            string textoOriginal = btnActualizarTiposCambio.Text;
            btnActualizarTiposCambio.Text = "Actualizando...";
            lblEstadoTiposCambio.Text = "Consultando tipos de cambio online...";

            bool actualizado = await ActualizarIndicadoresMindicadorConTimeout();

            CargarCombosMonedas();
            CargarTablaTiposCambio();
            RefrescarCalculosYVista();

            lblEstadoTiposCambio.Text = actualizado
                ? "Tipos de cambio actualizados correctamente."
                : "Sin respuesta online. Se mantienen los valores actuales de la biblioteca.";

            btnActualizarTiposCambio.Text = textoOriginal;
            btnActualizarTiposCambio.Enabled = true;
        }

        private async Task<bool> ActualizarIndicadoresMindicadorConTimeout()
        {
            try
            {
                Task<bool> tarea = ActualizarIndicadoresMindicador();
                Task timeout = Task.Delay(9000);

                Task completada = await Task.WhenAny(tarea, timeout);

                if (completada != tarea)
                {
                    return false;
                }

                return await tarea;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ActualizarIndicadoresMindicador()
        {
            if (cotizacion == null)
            {
                return false;
            }

            bool actualizado = await MonedaService.ActualizarDesdeMindicadorAsync(cotizacion);

            AsegurarTipoCambio("JPY", "Yen japonés", 6.5, "Respaldo manual");
            AsegurarTipoCambio("KRW", "Won surcoreano", 0.7, "Respaldo manual");

            return actualizado;
        }

        private async Task<bool> ActualizarIndicadorMindicador(string serie, string codigoMoneda)
        {
            try
            {
                if (cotizacion == null)
                {
                    return false;
                }

                InicializarTiposCambio();

                using HttpClient cliente = new HttpClient();
                cliente.Timeout = TimeSpan.FromSeconds(3);

                string url = $"https://mindicador.cl/api/{serie}";
                string json = await cliente.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);

                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("serie", out JsonElement serieJson))
                {
                    return false;
                }

                if (serieJson.GetArrayLength() == 0)
                {
                    return false;
                }

                JsonElement primerValor = serieJson[0];

                if (!primerValor.TryGetProperty("valor", out JsonElement valorJson))
                {
                    return false;
                }

                double valor = valorJson.GetDouble();

                if (valor <= 0.0)
                {
                    return false;
                }

                TipoCambio? tipo = cotizacion.TiposCambio
                    .FirstOrDefault(t => string.Equals(t.Codigo, codigoMoneda, StringComparison.OrdinalIgnoreCase));

                if (tipo == null)
                {
                    return false;
                }

                tipo.ValorEnCLP = valor;
                tipo.Fuente = "mindicador.cl";
                tipo.FechaActualizacion = DateTime.Now;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ActualizarTextoMonedasActuales()
        {
            if (lblMonedaClienteActual == null || cotizacion == null)
            {
                return;
            }

            string monedaCliente = string.IsNullOrWhiteSpace(cotizacion.MonedaPrecioCliente)
                ? "CLP"
                : cotizacion.MonedaPrecioCliente.Trim().ToUpperInvariant();

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            lblMonedaClienteActual.Text =
                $"Moneda cliente/cotización: {monedaCliente}. " +
                $"Moneda visual activa: {monedaVisual}. " +
                "Base interna contable: CLP.";
        }

        private string FormatearValorConvertidoTipoCambio(double valorCLP, string monedaVisual)
        {
            if (cotizacion == null)
            {
                return FormatearNumeroTipoCambio(valorCLP) + " CLP";
            }

            monedaVisual = string.IsNullOrWhiteSpace(monedaVisual)
                ? "CLP"
                : monedaVisual.Trim().ToUpperInvariant();

            if (monedaVisual == "CLP")
            {
                return FormatearNumeroTipoCambio(valorCLP) + " CLP";
            }

            TipoCambio? tipoVisual = cotizacion.TiposCambio
                .FirstOrDefault(t => string.Equals(t.Codigo, monedaVisual, StringComparison.OrdinalIgnoreCase));

            if (tipoVisual == null || tipoVisual.ValorEnCLP <= 0.0)
            {
                return FormatearNumeroTipoCambio(valorCLP) + " CLP";
            }

            double convertido = valorCLP / tipoVisual.ValorEnCLP;

            if (Math.Abs(convertido) > 0.0 && Math.Abs(convertido) < 0.01)
            {
                return convertido.ToString("0.##E+0", CultureInfo.InvariantCulture) + " " + monedaVisual;
            }

            if (Math.Abs(convertido) >= 1000000.0)
            {
                return convertido.ToString("0.##E+0", CultureInfo.InvariantCulture) + " " + monedaVisual;
            }

            return FormatearNumeroTipoCambio(convertido) + " " + monedaVisual;
        }

        private string FormatearNumeroTipoCambio(double valor)
        {
            CultureInfo cultura = new CultureInfo("es-CL");

            if (Math.Abs(valor % 1.0) < 0.000001)
            {
                return valor.ToString("#,##0", cultura);
            }

            return valor.ToString("#,##0.##", cultura);
        }

        private double ConvertirDoubleMoneda(object? valor)
        {
            if (valor == null)
            {
                return 0.0;
            }

            string texto = valor.ToString() ?? "";
            texto = texto.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return 0.0;
            }

            texto = texto
                .Replace("CLP", "", StringComparison.OrdinalIgnoreCase)
                .Replace("USD", "", StringComparison.OrdinalIgnoreCase)
                .Replace("EUR", "", StringComparison.OrdinalIgnoreCase)
                .Replace("JPY", "", StringComparison.OrdinalIgnoreCase)
                .Replace("KRW", "", StringComparison.OrdinalIgnoreCase)
                .Replace("UF", "", StringComparison.OrdinalIgnoreCase)
                .Replace("$", "")
                .Replace("US", "", StringComparison.OrdinalIgnoreCase)
                .Replace("€", "")
                .Replace("¥", "")
                .Trim();

            texto = texto.Replace(".", "");
            texto = texto.Replace(",", ".");

            double.TryParse(
                texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double resultado
            );

            return resultado;
        }

        private int OrdenMoneda(string? codigo)
        {
            codigo = string.IsNullOrWhiteSpace(codigo)
                ? ""
                : codigo.Trim().ToUpperInvariant();

            if (codigo == "CLP") return 0;
            if (codigo == "USD") return 1;
            if (codigo == "EUR") return 2;
            if (codigo == "JPY") return 3;
            if (codigo == "KRW") return 4;
            if (codigo == "UF") return 5;
            if (codigo == "UTM") return 6;
            if (codigo == "GBP") return 90;

            return 99;
        }
    }
}
