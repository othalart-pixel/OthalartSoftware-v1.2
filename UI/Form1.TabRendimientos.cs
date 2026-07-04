using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private DataGridView dgvRendimientosProductivos = new DataGridView();
        private Label lblEstadoRendimientosProductivos = new Label();

        private void ConstruirTabRendimientosProductivos(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 5;
            root.Padding = new Padding(22, 18, 22, 22);
            root.BackColor = Color.White;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titulo = new Label();
            titulo.Text = "Rendimientos productivos";
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.AutoSize = true;

            Label ayuda = new Label();
            ayuda.Text = "Define cuánto produce una persona promedio por día, semana o mes. El desglose productivo usa esta biblioteca para calcular días-persona y costos.";
            ayuda.Font = new Font("Segoe UI", 9.5f);
            ayuda.ForeColor = Color.FromArgb(90, 90, 90);
            ayuda.AutoSize = true;
            ayuda.MaximumSize = new Size(1150, 0);
            ayuda.Margin = new Padding(0, 0, 0, 10);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.Margin = new Padding(0, 6, 0, 14);
            acciones.Padding = new Padding(0, 2, 0, 2);

            Button btnAgregar = CrearBotonRendimiento("Agregar");
            btnAgregar.Click += (s, e) => AgregarRendimientoProductivo();

            Button btnEditar = CrearBotonRendimiento("Editar capacidad");
            btnEditar.Width = 140;
            btnEditar.Click += (s, e) => EditarCapacidadRendimientoSeleccionado();

            Button btnQuitar = CrearBotonRendimiento("Quitar");
            btnQuitar.Click += (s, e) => QuitarRendimientoProductivo();

            Button btnGuardar = CrearBotonRendimiento("Guardar biblioteca");
            btnGuardar.Width = 150;
            btnGuardar.Click += (s, e) => GuardarBibliotecaRendimientosProductivos();

            Button btnRestaurar = CrearBotonRendimiento("Restaurar base");
            btnRestaurar.Width = 130;
            btnRestaurar.Click += (s, e) => RestaurarBibliotecaRendimientosProductivos();

            acciones.Controls.Add(btnAgregar);
            acciones.Controls.Add(btnEditar);
            acciones.Controls.Add(btnQuitar);
            acciones.Controls.Add(btnGuardar);
            acciones.Controls.Add(btnRestaurar);

            ConfigurarGrillaRendimientosProductivos();

            lblEstadoRendimientosProductivos.AutoSize = true;
            lblEstadoRendimientosProductivos.ForeColor = Color.FromArgb(90, 90, 90);
            lblEstadoRendimientosProductivos.Margin = new Padding(0, 8, 0, 0);

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(ayuda, 0, 1);
            root.Controls.Add(acciones, 0, 2);
            root.Controls.Add(dgvRendimientosProductivos, 0, 3);
            root.Controls.Add(lblEstadoRendimientosProductivos, 0, 4);

            tab.Controls.Add(root);

            tab.Enter -= TabRendimientosProductivos_Enter;
            tab.Enter += TabRendimientosProductivos_Enter;
        }

        private void SeleccionarTabRendimientosProductivos()
        {
            AbrirTabPrincipal(tabRendimientosPrincipal, true);
            CargarBibliotecaRendimientosProductivosEnPantalla();
        }

        private Button CrearBotonRendimiento(string texto)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 110;
            boton.Height = 30;
            boton.Margin = new Padding(0, 0, 8, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.BackColor = Color.FromArgb(245, 245, 245);
            boton.ForeColor = Color.Black;
            boton.UseVisualStyleBackColor = false;
            return boton;
        }

        private void ConfigurarGrillaRendimientosProductivos()
        {
            dgvRendimientosProductivos.Dock = DockStyle.Fill;
            dgvRendimientosProductivos.AllowUserToAddRows = false;
            dgvRendimientosProductivos.AllowUserToDeleteRows = false;
            dgvRendimientosProductivos.RowHeadersVisible = false;
            dgvRendimientosProductivos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRendimientosProductivos.MultiSelect = false;
            dgvRendimientosProductivos.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvRendimientosProductivos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvRendimientosProductivos.BackgroundColor = Color.White;
            dgvRendimientosProductivos.BorderStyle = BorderStyle.FixedSingle;
            dgvRendimientosProductivos.GridColor = Color.Gainsboro;
            dgvRendimientosProductivos.EnableHeadersVisualStyles = false;
            dgvRendimientosProductivos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvRendimientosProductivos.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvRendimientosProductivos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgvRendimientosProductivos.Columns.Clear();
            dgvRendimientosProductivos.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Activo", HeaderText = "Activo", Width = 60 });
            dgvRendimientosProductivos.Columns.Add("Etapa", "Etapa");
            dgvRendimientosProductivos.Columns.Add("TipoInterno", "Tipo interno");
            dgvRendimientosProductivos.Columns.Add("Proceso", "Proceso / subtrabajo");
            dgvRendimientosProductivos.Columns.Add("Capacidad", "Capacidad");
            dgvRendimientosProductivos.Columns.Add("Unidad", "Unidad");
            dgvRendimientosProductivos.Columns.Add("Cargo", "Cargo");
            dgvRendimientosProductivos.Columns.Add("CantidadPorPeriodo", "Promedio");

            DataGridViewComboBoxColumn periodo = new DataGridViewComboBoxColumn();
            periodo.Name = "Periodo";
            periodo.HeaderText = "Periodo";
            periodo.Items.AddRange("día", "semana", "mes");
            periodo.FlatStyle = FlatStyle.Flat;
            dgvRendimientosProductivos.Columns.Add(periodo);

            dgvRendimientosProductivos.Columns.Add("Nota", "Nota");

            SetColRend("Etapa", 130);
            SetColRend("TipoInterno", 150);
            SetColRend("Proceso", 310);
            SetColRend("Capacidad", 210);
            SetColRend("Unidad", 95);
            SetColRend("Cargo", 260);
            SetColRend("CantidadPorPeriodo", 95);
            SetColRend("Periodo", 95);
            SetColRend("Nota", 320);

            dgvRendimientosProductivos.Columns["Unidad"].Visible = false;
            dgvRendimientosProductivos.Columns["CantidadPorPeriodo"].Visible = false;
            dgvRendimientosProductivos.Columns["Periodo"].Visible = false;

            dgvRendimientosProductivos.CellDoubleClick -= DgvRendimientosProductivos_CellDoubleClick;
            dgvRendimientosProductivos.CellDoubleClick += DgvRendimientosProductivos_CellDoubleClick;
        }

        private void SetColRend(string nombre, int ancho)
        {
            if (dgvRendimientosProductivos.Columns.Contains(nombre))
            {
                dgvRendimientosProductivos.Columns[nombre].Width = ancho;
            }
        }

        private void TabRendimientosProductivos_Enter(object sender, EventArgs e)
        {
            CargarBibliotecaRendimientosProductivosEnPantalla();
        }

        private void DgvRendimientosProductivos_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e
        )
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            EditarCapacidadRendimientoSeleccionado();
        }

        private void CargarBibliotecaRendimientosProductivosEnPantalla()
        {
            dgvRendimientosProductivos.Rows.Clear();

            foreach (RendimientoProductivo r in BibliotecaRendimientosProductivosJsonService.CargarRendimientos())
            {
                int rowIndex = dgvRendimientosProductivos.Rows.Add();
                DataGridViewRow row = dgvRendimientosProductivos.Rows[rowIndex];
                row.Tag = r;
                row.Cells["Activo"].Value = r.Activo;
                row.Cells["Etapa"].Value = r.Etapa;
                row.Cells["TipoInterno"].Value = r.TipoInterno;
                row.Cells["Proceso"].Value = r.Proceso;
                row.Cells["Capacidad"].Value = FormatearCapacidadRendimiento(r);
                row.Cells["Unidad"].Value = r.Unidad;
                row.Cells["Cargo"].Value = r.Cargo;
                row.Cells["CantidadPorPeriodo"].Value = r.CantidadPorPeriodo.ToString("0.##");
                row.Cells["Periodo"].Value = string.IsNullOrWhiteSpace(r.Periodo) ? "semana" : r.Periodo;
                row.Cells["Nota"].Value = r.Nota;
            }

            lblEstadoRendimientosProductivos.Text =
                "Biblioteca: " + BibliotecaRendimientosProductivosJsonService.ObtenerRutaRendimientos();
        }

        private string FormatearCapacidadRendimiento(RendimientoProductivo r)
        {
            if (r == null)
            {
                return "";
            }

            string unidad = string.IsNullOrWhiteSpace(r.Unidad) ? "unidades" : r.Unidad.Trim();
            string periodo = NormalizarPeriodoRendimiento(r.Periodo);

            double minimo = r.CantidadMinimaPorPeriodo > 0.0
                ? r.CantidadMinimaPorPeriodo
                : r.CantidadPorPeriodo * 0.80;

            double maximo = r.CantidadMaximaPorPeriodo > 0.0
                ? r.CantidadMaximaPorPeriodo
                : r.CantidadPorPeriodo * 1.25;

            return minimo.ToString("0.##") +
                " / " +
                r.CantidadPorPeriodo.ToString("0.##") +
                " / " +
                maximo.ToString("0.##") +
                " " +
                unidad +
                " / " +
                periodo;
        }

        private void ParsearCapacidadRendimiento(
            string texto,
            out double cantidad,
            out string unidad,
            out string periodo
        )
        {
            cantidad = 0.0;
            unidad = "";
            periodo = "semana";

            ParsearCapacidadRendimientoCompleta(
                texto,
                out double _,
                out cantidad,
                out double _,
                out unidad,
                out periodo
            );
        }

        private void ParsearCapacidadRendimientoCompleta(
            string texto,
            out double cantidadMinima,
            out double cantidadEstandar,
            out double cantidadMaxima,
            out string unidad,
            out string periodo
        )
        {
            cantidadMinima = 0.0;
            cantidadEstandar = 0.0;
            cantidadMaxima = 0.0;
            unidad = "";
            periodo = "semana";

            texto = (texto ?? "").Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return;
            }

            string normalizado = texto
                .Replace(",", ".")
                .Replace(" por ", " / ")
                .Replace("\\", "/");

            string[] partes = normalizado.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string izquierda = partes.Length > 0 ? partes[0].Trim() : normalizado;
            string derecha = partes.Length > 1 ? partes[partes.Length - 1].Trim() : "";

            if (partes.Length >= 4)
            {
                cantidadMinima = ParsearNumeroRendimiento(partes[0]);
                cantidadEstandar = ParsearNumeroRendimiento(partes[1]);

                string maxUnidad = partes[2].Trim();
                string[] tokensMax = maxUnidad.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokensMax.Length > 0)
                {
                    cantidadMaxima = ParsearNumeroRendimiento(tokensMax[0]);
                }

                if (tokensMax.Length > 1)
                {
                    unidad = string.Join(" ", tokensMax.Skip(1));
                }

                periodo = NormalizarPeriodoRendimiento(derecha);
                return;
            }

            string[] tokens = izquierda.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length > 0)
            {
                cantidadEstandar = ParsearNumeroRendimiento(tokens[0]);
            }

            if (tokens.Length > 1)
            {
                unidad = string.Join(" ", tokens.Skip(1));
            }

            if (!string.IsNullOrWhiteSpace(derecha))
            {
                periodo = NormalizarPeriodoRendimiento(derecha);
            }

            if (cantidadEstandar > 0.0)
            {
                cantidadMinima = cantidadEstandar * 0.80;
                cantidadMaxima = cantidadEstandar * 1.25;
            }
        }

        private double ParsearNumeroRendimiento(string texto)
        {
            double.TryParse(
                (texto ?? "").Trim().Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double valor
            );

            return valor;
        }

        private void EditarCapacidadRendimientoSeleccionado()
        {
            if (dgvRendimientosProductivos.CurrentRow == null)
            {
                return;
            }

            DataGridViewRow row = dgvRendimientosProductivos.CurrentRow;

            ParsearCapacidadRendimiento(
                Convert.ToString(row.Cells["Capacidad"].Value) ?? "",
                out double cantidadActual,
                out string unidadActual,
                out string periodoActual
            );

            ParsearCapacidadRendimientoCompleta(
                Convert.ToString(row.Cells["Capacidad"].Value) ?? "",
                out double cantidadMinimaActual,
                out double cantidadEstandarActual,
                out double cantidadMaximaActual,
                out string _,
                out string _
            );

            if (cantidadActual <= 0.0)
            {
                cantidadActual = ParsearDoubleDesglose(row.Cells["CantidadPorPeriodo"].Value, 0.0);
            }

            if (cantidadEstandarActual <= 0.0)
            {
                cantidadEstandarActual = cantidadActual;
            }

            if (cantidadMinimaActual <= 0.0)
            {
                cantidadMinimaActual = cantidadEstandarActual * 0.80;
            }

            if (cantidadMaximaActual <= 0.0)
            {
                cantidadMaximaActual = cantidadEstandarActual * 1.25;
            }

            if (string.IsNullOrWhiteSpace(unidadActual))
            {
                unidadActual = Convert.ToString(row.Cells["Unidad"].Value) ?? "unidades";
            }

            if (string.IsNullOrWhiteSpace(periodoActual))
            {
                periodoActual = Convert.ToString(row.Cells["Periodo"].Value) ?? "semana";
            }

            using Form dialogo = new Form();
            dialogo.Text = "Editar capacidad productiva";
            dialogo.StartPosition = FormStartPosition.CenterParent;
            dialogo.ClientSize = new Size(560, 335);
            dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialogo.MinimizeBox = false;
            dialogo.MaximizeBox = false;
            dialogo.AutoScaleMode = AutoScaleMode.None;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(24, 18, 24, 16);
            layout.ColumnCount = 2;
            layout.RowCount = 6;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            NumericUpDown nudMinima = CrearNumericRendimiento(cantidadMinimaActual);
            NumericUpDown nudEstandar = CrearNumericRendimiento(cantidadEstandarActual);
            NumericUpDown nudMaxima = CrearNumericRendimiento(cantidadMaximaActual);

            ComboBox cmbUnidad = new ComboBox();
            cmbUnidad.DropDownStyle = ComboBoxStyle.DropDown;
            cmbUnidad.Items.AddRange(new object[] { "segundos", "personajes", "piezas", "fondos", "planos", "props" });
            cmbUnidad.Text = string.IsNullOrWhiteSpace(unidadActual) ? "unidades" : unidadActual;
            cmbUnidad.Dock = DockStyle.Fill;
            cmbUnidad.Margin = new Padding(0, 5, 0, 8);

            ComboBox cmbPeriodo = new ComboBox();
            cmbPeriodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriodo.Items.AddRange(new object[] { "dia", "semana", "mes" });
            cmbPeriodo.SelectedItem = NormalizarPeriodoRendimiento(periodoActual);
            cmbPeriodo.Dock = DockStyle.Fill;
            cmbPeriodo.Margin = new Padding(0, 5, 0, 8);

            layout.Controls.Add(new Label { Text = "Baja", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) }, 0, 0);
            layout.Controls.Add(nudMinima, 1, 0);
            layout.Controls.Add(new Label { Text = "Promedio", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) }, 0, 1);
            layout.Controls.Add(nudEstandar, 1, 1);
            layout.Controls.Add(new Label { Text = "Alta", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) }, 0, 2);
            layout.Controls.Add(nudMaxima, 1, 2);
            layout.Controls.Add(new Label { Text = "Unidad", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) }, 0, 3);
            layout.Controls.Add(cmbUnidad, 1, 3);
            layout.Controls.Add(new Label { Text = "Por", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) }, 0, 4);
            layout.Controls.Add(cmbPeriodo, 1, 4);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Fill;
            acciones.FlowDirection = FlowDirection.RightToLeft;
            acciones.WrapContents = false;
            acciones.Padding = new Padding(0, 12, 0, 0);

            Button btnGuardar = new Button { Text = "Guardar", Width = 115, Height = 34, DialogResult = DialogResult.OK };
            Button btnCancelar = new Button { Text = "Cancelar", Width = 115, Height = 34, DialogResult = DialogResult.Cancel };
            btnGuardar.Margin = new Padding(8, 0, 0, 0);
            btnCancelar.Margin = new Padding(8, 0, 0, 0);

            acciones.Controls.Add(btnGuardar);
            acciones.Controls.Add(btnCancelar);
            layout.Controls.Add(acciones, 0, 5);
            layout.SetColumnSpan(acciones, 2);

            dialogo.Controls.Add(layout);
            dialogo.AcceptButton = btnGuardar;
            dialogo.CancelButton = btnCancelar;

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string unidad = (cmbUnidad.Text ?? "").Trim();
            string periodo = NormalizarPeriodoRendimiento(Convert.ToString(cmbPeriodo.SelectedItem));
            string capacidad =
                Convert.ToDouble(nudMinima.Value).ToString("0.##") +
                " / " +
                Convert.ToDouble(nudEstandar.Value).ToString("0.##") +
                " / " +
                Convert.ToDouble(nudMaxima.Value).ToString("0.##") +
                " " +
                (string.IsNullOrWhiteSpace(unidad) ? "unidades" : unidad) +
                " / " +
                periodo;

            row.Cells["Capacidad"].Value = capacidad;
            row.Cells["CantidadPorPeriodo"].Value = Convert.ToDouble(nudEstandar.Value).ToString("0.##");
            row.Cells["Unidad"].Value = string.IsNullOrWhiteSpace(unidad) ? "unidades" : unidad;
            row.Cells["Periodo"].Value = periodo;

            GuardarBibliotecaRendimientosProductivos();
            CargarBibliotecaRendimientosProductivosEnPantalla();
        }

        private NumericUpDown CrearNumericRendimiento(double valor)
        {
            NumericUpDown nud = new NumericUpDown();
            nud.DecimalPlaces = 2;
            nud.Minimum = 0;
            nud.Maximum = 100000;
            nud.Value = Convert.ToDecimal(Math.Max(0.0, valor));
            nud.Dock = DockStyle.Fill;
            nud.Margin = new Padding(0, 5, 0, 8);
            return nud;
        }

        private void GuardarBibliotecaRendimientosProductivos()
        {
            dgvRendimientosProductivos.EndEdit();

            List<RendimientoProductivo> lista = new List<RendimientoProductivo>();
            int id = 1;

            foreach (DataGridViewRow row in dgvRendimientosProductivos.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                ParsearCapacidadRendimientoCompleta(
                    Convert.ToString(row.Cells["Capacidad"].Value) ?? "",
                    out double cantidadMinima,
                    out double cantidad,
                    out double cantidadMaxima,
                    out string unidad,
                    out string periodo
                );

                if (cantidad <= 0.0)
                {
                    cantidad = ParsearDoubleDesglose(row.Cells["CantidadPorPeriodo"].Value, 0.0);
                }

                if (string.IsNullOrWhiteSpace(unidad))
                {
                    unidad = Convert.ToString(row.Cells["Unidad"].Value) ?? "";
                }

                if (string.IsNullOrWhiteSpace(periodo))
                {
                    periodo = Convert.ToString(row.Cells["Periodo"].Value) ?? "semana";
                }

                if (cantidadMinima <= 0.0 && cantidad > 0.0)
                {
                    cantidadMinima = cantidad * 0.80;
                }

                if (cantidadMaxima <= 0.0 && cantidad > 0.0)
                {
                    cantidadMaxima = cantidad * 1.25;
                }

                lista.Add(new RendimientoProductivo
                {
                    Id = id++,
                    Activo = Convert.ToBoolean(row.Cells["Activo"].Value ?? false),
                    Etapa = Convert.ToString(row.Cells["Etapa"].Value) ?? "",
                    TipoInterno = Convert.ToString(row.Cells["TipoInterno"].Value) ?? "",
                    Proceso = Convert.ToString(row.Cells["Proceso"].Value) ?? "",
                    Unidad = unidad,
                    Cargo = Convert.ToString(row.Cells["Cargo"].Value) ?? "",
                    CantidadMinimaPorPeriodo = cantidadMinima,
                    CantidadPorPeriodo = cantidad,
                    CantidadMaximaPorPeriodo = cantidadMaxima,
                    Periodo = periodo,
                    Nota = Convert.ToString(row.Cells["Nota"].Value) ?? ""
                });
            }

            BibliotecaRendimientosProductivosJsonService.GuardarRendimientos(lista);

            if (cotizacion != null)
            {
                cotizacion.DesgloseProductivo = null;
            }

            lblEstadoRendimientosProductivos.Text = "Rendimientos guardados. El desglose se recalculará con esta biblioteca.";
        }

        private void AgregarRendimientoProductivo()
        {
            int rowIndex = dgvRendimientosProductivos.Rows.Add();
            DataGridViewRow row = dgvRendimientosProductivos.Rows[rowIndex];
            row.Cells["Activo"].Value = true;
            row.Cells["Etapa"].Value = "Produccion";
            row.Cells["TipoInterno"].Value = "";
            row.Cells["Proceso"].Value = "";
            row.Cells["Capacidad"].Value = "8 / 10 / 12.5 segundos / semana";
            row.Cells["Unidad"].Value = "segundos";
            row.Cells["Cargo"].Value = "";
            row.Cells["CantidadPorPeriodo"].Value = "10";
            row.Cells["Periodo"].Value = "semana";
            row.Cells["Nota"].Value = "";
        }

        private void QuitarRendimientoProductivo()
        {
            if (dgvRendimientosProductivos.CurrentRow != null)
            {
                dgvRendimientosProductivos.Rows.Remove(dgvRendimientosProductivos.CurrentRow);
            }
        }

        private void RestaurarBibliotecaRendimientosProductivos()
        {
            BibliotecaRendimientosProductivosJsonService.RegenerarDesdeBase();
            CargarBibliotecaRendimientosProductivosEnPantalla();
            lblEstadoRendimientosProductivos.Text = "Biblioteca base restaurada.";
        }
    }
}
