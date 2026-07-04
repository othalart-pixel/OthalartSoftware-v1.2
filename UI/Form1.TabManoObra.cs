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
        private const string NombrePlanGeneralManoObra = "General / proyecto completo";
        private const double HorasDiaEstandarManoObra = 8.0;
        private bool aplicandoPlanDesdeAsignacionManoObra;

        private void ConstruirTabManoObra(TabPage tab)
        {
            tab.Controls.Clear();

            TabControl tabsManoObra = new TabControl();
            tabsManoObra.Dock = DockStyle.Fill;

            TabPage tabPlanMensual = new TabPage("Plan mensual");
            TabPage tabAsignacion = new TabPage("Asignacion por persona");

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(15);
            layout.RowCount = 2;
            layout.ColumnCount = 1;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 10;
            panelSuperior.RowCount = 2;

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));

            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            Label titulo = new Label();
            titulo.Text = "Plan de mano de obra";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            panelSuperior.Controls.Add(titulo, 0, 0);
            panelSuperior.SetColumnSpan(titulo, 8);

            Label lblEtapa = new Label();
            lblEtapa.Text = "Bloque:";
            lblEtapa.Dock = DockStyle.Fill;
            lblEtapa.TextAlign = ContentAlignment.MiddleLeft;

            cmbEtapa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEtapa.Dock = DockStyle.Fill;
            cmbEtapa.SelectedIndexChanged -= CmbEtapa_SelectedIndexChanged;
            cmbEtapa.SelectedIndexChanged += CmbEtapa_SelectedIndexChanged;

            Label lblCargo = new Label();
            lblCargo.Text = "Cargo:";
            lblCargo.Dock = DockStyle.Fill;
            lblCargo.TextAlign = ContentAlignment.MiddleLeft;

            cmbCargo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCargo.Dock = DockStyle.Fill;

            btnAgregarCargo.Text = "Agregar cargo";
            btnAgregarCargo.Width = 120;
            btnAgregarCargo.Height = 30;
            btnAgregarCargo.Click -= BtnAgregarCargo_Click;
            btnAgregarCargo.Click += BtnAgregarCargo_Click;

            btnEliminarCargo.Text = "Eliminar cargo";
            btnEliminarCargo.Width = 120;
            btnEliminarCargo.Height = 30;
            btnEliminarCargo.Click -= BtnEliminarCargo_Click;
            btnEliminarCargo.Click += BtnEliminarCargo_Click;

            btnRellenarManoObraDesdeDesglose.Text = "Rellenar desde desglose";
            btnRellenarManoObraDesdeDesglose.Width = 180;
            btnRellenarManoObraDesdeDesglose.Height = 30;
            btnRellenarManoObraDesdeDesglose.Click -= BtnRellenarManoObraDesdeDesglose_Click;
            btnRellenarManoObraDesdeDesglose.Click += BtnRellenarManoObraDesdeDesglose_Click;

            btnRecalcular.Text = "Recalcular";
            btnRecalcular.Width = 100;
            btnRecalcular.Height = 30;
            btnRecalcular.Click -= BtnRecalcular_Click;
            btnRecalcular.Click += BtnRecalcular_Click;

            panelSuperior.Controls.Add(lblEtapa, 0, 1);
            panelSuperior.Controls.Add(cmbEtapa, 1, 1);
            panelSuperior.Controls.Add(lblCargo, 2, 1);
            panelSuperior.Controls.Add(cmbCargo, 3, 1);
            panelSuperior.Controls.Add(btnAgregarCargo, 4, 1);
            panelSuperior.Controls.Add(btnEliminarCargo, 5, 1);
            panelSuperior.Controls.Add(btnRellenarManoObraDesdeDesglose, 6, 1);
            panelSuperior.Controls.Add(btnRecalcular, 7, 1);

            dgvManoObra.Dock = DockStyle.Fill;
            dgvManoObra.AllowUserToAddRows = false;
            dgvManoObra.AllowUserToDeleteRows = false;
            dgvManoObra.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvManoObra.ScrollBars = ScrollBars.Both;
            dgvManoObra.RowHeadersVisible = false;
            dgvManoObra.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvManoObra.Columns.Clear();

            dgvManoObra.Columns.Add("Etapa", "Bloque");
            dgvManoObra.Columns.Add("Cargo", "Cargo");
            dgvManoObra.Columns.Add("Persona", "Persona");
            dgvManoObra.Columns.Add("ActividadesDesglose", "Actividades desde desglose");
            dgvManoObra.Columns.Add("HorasDesglose", "Horas req.");
            dgvManoObra.Columns.Add("DiasDesglose", "Días-persona");

            dgvManoObra.Columns.Add("ValorMensual", "Valor mensual CLP");
            dgvManoObra.Columns.Add("ValorMensualVisual", "Valor mensual visual");

            for (int i = 1; i <= MaxMesesTabla; i++)
            {
                dgvManoObra.Columns.Add("M" + i, "M" + i);
            }

            dgvManoObra.Columns.Add("PersonaMes", "Persona-mes");
            dgvManoObra.Columns.Add("Costo", "Costo CLP");
            dgvManoObra.Columns.Add("CostoVisual", "Costo visual");

            dgvManoObra.Columns["Etapa"].ReadOnly = true;
            dgvManoObra.Columns["Cargo"].ReadOnly = true;
            dgvManoObra.Columns["Persona"].ReadOnly = false;
            dgvManoObra.Columns["ActividadesDesglose"].ReadOnly = true;
            dgvManoObra.Columns["HorasDesglose"].ReadOnly = true;
            dgvManoObra.Columns["DiasDesglose"].ReadOnly = true;

            dgvManoObra.Columns["ValorMensual"].ReadOnly = false;
            dgvManoObra.Columns["ValorMensualVisual"].ReadOnly = true;

            dgvManoObra.Columns["PersonaMes"].ReadOnly = true;
            dgvManoObra.Columns["Costo"].ReadOnly = true;
            dgvManoObra.Columns["CostoVisual"].ReadOnly = true;

            dgvManoObra.Columns["Etapa"].Width = 160;
            dgvManoObra.Columns["Cargo"].Width = 220;
            dgvManoObra.Columns["Persona"].Width = 160;
            dgvManoObra.Columns["ActividadesDesglose"].Width = 360;
            dgvManoObra.Columns["HorasDesglose"].Width = 90;
            dgvManoObra.Columns["DiasDesglose"].Width = 95;
            dgvManoObra.Columns["ValorMensual"].Width = 130;
            dgvManoObra.Columns["ValorMensualVisual"].Width = 140;
            dgvManoObra.Columns["PersonaMes"].Width = 100;
            dgvManoObra.Columns["Costo"].Width = 130;
            dgvManoObra.Columns["CostoVisual"].Width = 140;

            for (int i = 1; i <= MaxMesesTabla; i++)
            {
                dgvManoObra.Columns["M" + i].Width = 65;
            }

            dgvManoObra.CellValueChanged -= DgvManoObra_CellValueChanged;
            dgvManoObra.CellValueChanged += DgvManoObra_CellValueChanged;

            dgvManoObra.CurrentCellDirtyStateChanged -= DgvManoObra_CurrentCellDirtyStateChanged;
            dgvManoObra.CurrentCellDirtyStateChanged += DgvManoObra_CurrentCellDirtyStateChanged;

            layout.Controls.Add(panelSuperior, 0, 0);
            layout.Controls.Add(dgvManoObra, 0, 1);

            tabPlanMensual.Controls.Add(layout);
            tabAsignacion.Controls.Add(CrearPanelAsignacionManoObra());

            tabsManoObra.TabPages.Add(tabPlanMensual);
            tabsManoObra.TabPages.Add(tabAsignacion);
            tab.Controls.Add(tabsManoObra);

            foreach (DataGridViewColumn columna in dgvManoObra.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private Control CrearPanelAsignacionManoObra()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.Padding = new Padding(15);
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Asignacion de personas a labores productivas";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            Label ayuda = new Label();
            ayuda.Text = "Carga las labores desde el desglose, elige personas compatibles por cargo y reparte horas. Si no hay persona compatible, usa un recurso generico por cargo.";
            ayuda.ForeColor = Color.DimGray;
            ayuda.Dock = DockStyle.Fill;
            ayuda.TextAlign = ContentAlignment.TopLeft;

            Panel encabezado = new Panel();
            encabezado.Dock = DockStyle.Fill;
            encabezado.Controls.Add(ayuda);
            encabezado.Controls.Add(titulo);
            titulo.Height = 30;
            titulo.Top = 0;
            ayuda.Top = 32;
            ayuda.Left = 0;
            ayuda.Width = 1000;

            FlowLayoutPanel filtros = new FlowLayoutPanel();
            filtros.Dock = DockStyle.Fill;
            filtros.WrapContents = false;
            filtros.AutoScroll = true;
            filtros.Padding = new Padding(0, 6, 0, 0);

            cmbFiltroAsignacionEtapa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroAsignacionEtapa.Width = 180;
            cmbFiltroAsignacionEtapa.SelectedIndexChanged -= FiltroAsignacionManoObra_Changed;
            cmbFiltroAsignacionEtapa.SelectedIndexChanged += FiltroAsignacionManoObra_Changed;

            cmbFiltroAsignacionCargo.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroAsignacionCargo.Width = 230;
            cmbFiltroAsignacionCargo.SelectedIndexChanged -= FiltroAsignacionManoObra_Changed;
            cmbFiltroAsignacionCargo.SelectedIndexChanged += FiltroAsignacionManoObra_Changed;

            cmbFiltroAsignacionPersona.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFiltroAsignacionPersona.Width = 230;
            cmbFiltroAsignacionPersona.SelectedIndexChanged -= FiltroAsignacionManoObra_Changed;
            cmbFiltroAsignacionPersona.SelectedIndexChanged += FiltroAsignacionManoObra_Changed;

            nudHorasTandaGlobalManoObra.Minimum = 1;
            nudHorasTandaGlobalManoObra.Maximum = 120;
            nudHorasTandaGlobalManoObra.DecimalPlaces = 1;
            nudHorasTandaGlobalManoObra.Value = 42;
            nudHorasTandaGlobalManoObra.Width = 75;
            nudHorasTandaGlobalManoObra.ValueChanged -= NudHorasTandaGlobalManoObra_ValueChanged;
            nudHorasTandaGlobalManoObra.ValueChanged += NudHorasTandaGlobalManoObra_ValueChanged;

            Button cargarDesdeDesglose = new Button();
            cargarDesdeDesglose.Text = "Cargar labores desde desglose";
            cargarDesdeDesglose.Width = 190;
            cargarDesdeDesglose.Height = 30;
            cargarDesdeDesglose.Click += (s, e) =>
            {
                GenerarAsignacionesManoObraDesdeDesglose(true);
                CargarFiltrosAsignacionManoObra();
                CargarTablaAsignacionManoObra();
            };

            Button agregarDivision = new Button();
            agregarDivision.Text = "Agregar division";
            agregarDivision.Width = 130;
            agregarDivision.Height = 30;
            agregarDivision.Click += (s, e) => AgregarDivisionAsignacionManoObra();

            Button eliminarFila = new Button();
            eliminarFila.Text = "Eliminar fila";
            eliminarFila.Width = 105;
            eliminarFila.Height = 30;
            eliminarFila.Click += (s, e) => EliminarFilaAsignacionManoObra();

            Button guardar = new Button();
            guardar.Text = "Guardar asignaciones";
            guardar.Width = 145;
            guardar.Height = 30;
            guardar.Click += (s, e) =>
            {
                GuardarAsignacionesManoObraDesdeTabla(false);
                AplicarPlanMensualDesdeAsignacionesManoObra(true);
            };

            Button aplicarAlPlan = new Button();
            aplicarAlPlan.Text = "Aplicar al plan mensual";
            aplicarAlPlan.Width = 165;
            aplicarAlPlan.Height = 30;
            aplicarAlPlan.Click += (s, e) =>
            {
                GuardarAsignacionesManoObraDesdeTabla(false);
                AplicarPlanMensualDesdeAsignacionesManoObra(true);
            };

            filtros.Controls.Add(CrearEtiquetaInlineManoObra("Etapa"));
            filtros.Controls.Add(cmbFiltroAsignacionEtapa);
            filtros.Controls.Add(CrearEtiquetaInlineManoObra("Cargo"));
            filtros.Controls.Add(cmbFiltroAsignacionCargo);
            filtros.Controls.Add(CrearEtiquetaInlineManoObra("Persona"));
            filtros.Controls.Add(cmbFiltroAsignacionPersona);
            filtros.Controls.Add(CrearEtiquetaInlineManoObra("Horas/tanda"));
            filtros.Controls.Add(nudHorasTandaGlobalManoObra);
            filtros.Controls.Add(cargarDesdeDesglose);
            filtros.Controls.Add(agregarDivision);
            filtros.Controls.Add(eliminarFila);
            filtros.Controls.Add(guardar);
            filtros.Controls.Add(aplicarAlPlan);

            lblResumenAsignacionManoObra.Dock = DockStyle.Fill;
            lblResumenAsignacionManoObra.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblResumenAsignacionManoObra.ForeColor = Color.FromArgb(40, 40, 40);
            lblResumenAsignacionManoObra.TextAlign = ContentAlignment.MiddleLeft;
            lblResumenAsignacionManoObra.Padding = new Padding(4, 0, 0, 0);

            ConfigurarGrillaAsignacionManoObra();

            root.Controls.Add(encabezado, 0, 0);
            root.Controls.Add(filtros, 0, 1);
            root.Controls.Add(lblResumenAsignacionManoObra, 0, 2);
            root.Controls.Add(dgvAsignacionManoObra, 0, 3);

            CargarFiltrosAsignacionManoObra();
            CargarTablaAsignacionManoObra();

            return root;
        }

        private Label CrearEtiquetaInlineManoObra(string texto)
        {
            Label label = new Label();
            label.Text = texto + ":";
            label.Width = 62;
            label.Height = 30;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(8, 0, 0, 0);
            return label;
        }

        private void ConfigurarGrillaAsignacionManoObra()
        {
            dgvAsignacionManoObra.Dock = DockStyle.Fill;
            dgvAsignacionManoObra.AllowUserToAddRows = false;
            dgvAsignacionManoObra.AllowUserToDeleteRows = false;
            dgvAsignacionManoObra.RowHeadersVisible = false;
            dgvAsignacionManoObra.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvAsignacionManoObra.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvAsignacionManoObra.ScrollBars = ScrollBars.Both;
            dgvAsignacionManoObra.Columns.Clear();

            dgvAsignacionManoObra.Columns.Add("IdAsignacion", "Id");
            dgvAsignacionManoObra.Columns.Add("ClaveLabor", "Clave");
            dgvAsignacionManoObra.Columns.Add("PersonaId", "PersonaId");
            dgvAsignacionManoObra.Columns.Add("PiezaSubproducto", "Pieza / subproducto");
            dgvAsignacionManoObra.Columns.Add("Etapa", "Etapa");
            dgvAsignacionManoObra.Columns.Add("SubEtapaLabor", "Subetapa / labor");
            dgvAsignacionManoObra.Columns.Add("CargoRequerido", "Cargo requerido");
            dgvAsignacionManoObra.Columns.Add("HorasRequeridas", "Horas requeridas");

            DataGridViewComboBoxColumn persona = new DataGridViewComboBoxColumn();
            persona.Name = "PersonaAsignada";
            persona.HeaderText = "Persona asignada";
            persona.FlatStyle = FlatStyle.Flat;
            dgvAsignacionManoObra.Columns.Add(persona);

            dgvAsignacionManoObra.Columns.Add("HorasAsignadas", "Horas asignadas");
            dgvAsignacionManoObra.Columns.Add("HorasPendientes", "Horas pendientes");
            dgvAsignacionManoObra.Columns.Add("TipoAsignacion", "Tipo asignacion");
            dgvAsignacionManoObra.Columns.Add("Observaciones", "Observaciones");

            dgvAsignacionManoObra.Columns["IdAsignacion"].Visible = false;
            dgvAsignacionManoObra.Columns["ClaveLabor"].Visible = false;
            dgvAsignacionManoObra.Columns["PersonaId"].Visible = false;

            dgvAsignacionManoObra.Columns["PiezaSubproducto"].Width = 210;
            dgvAsignacionManoObra.Columns["Etapa"].Width = 120;
            dgvAsignacionManoObra.Columns["SubEtapaLabor"].Width = 190;
            dgvAsignacionManoObra.Columns["CargoRequerido"].Width = 220;
            dgvAsignacionManoObra.Columns["HorasRequeridas"].Width = 105;
            dgvAsignacionManoObra.Columns["PersonaAsignada"].Width = 240;
            dgvAsignacionManoObra.Columns["HorasAsignadas"].Width = 105;
            dgvAsignacionManoObra.Columns["HorasPendientes"].Width = 105;
            dgvAsignacionManoObra.Columns["TipoAsignacion"].Width = 130;
            dgvAsignacionManoObra.Columns["Observaciones"].Width = 260;

            foreach (DataGridViewColumn columna in dgvAsignacionManoObra.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvAsignacionManoObra.Columns["PiezaSubproducto"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["Etapa"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["SubEtapaLabor"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["CargoRequerido"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["HorasRequeridas"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["HorasPendientes"].ReadOnly = true;
            dgvAsignacionManoObra.Columns["TipoAsignacion"].ReadOnly = true;

            dgvAsignacionManoObra.CellBeginEdit -= DgvAsignacionManoObra_CellBeginEdit;
            dgvAsignacionManoObra.CellBeginEdit += DgvAsignacionManoObra_CellBeginEdit;
            dgvAsignacionManoObra.CellValueChanged -= DgvAsignacionManoObra_CellValueChanged;
            dgvAsignacionManoObra.CellValueChanged += DgvAsignacionManoObra_CellValueChanged;
            dgvAsignacionManoObra.CurrentCellDirtyStateChanged -= DgvAsignacionManoObra_CurrentCellDirtyStateChanged;
            dgvAsignacionManoObra.CurrentCellDirtyStateChanged += DgvAsignacionManoObra_CurrentCellDirtyStateChanged;
            dgvAsignacionManoObra.DataError -= DgvAsignacionManoObra_DataError;
            dgvAsignacionManoObra.DataError += DgvAsignacionManoObra_DataError;
        }

        private void DgvAsignacionManoObra_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void DgvAsignacionManoObra_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvAsignacionManoObra.IsCurrentCellDirty)
            {
                dgvAsignacionManoObra.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvAsignacionManoObra_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 || dgvAsignacionManoObra.Columns[e.ColumnIndex].Name != "PersonaAsignada")
            {
                return;
            }

            string cargo = Convert.ToString(dgvAsignacionManoObra.Rows[e.RowIndex].Cells["CargoRequerido"].Value) ?? "";
            DataGridViewComboBoxCell cell = dgvAsignacionManoObra.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewComboBoxCell;

            if (cell == null)
            {
                return;
            }

            string valorActual = Convert.ToString(cell.Value) ?? "";
            cell.Items.Clear();
            cell.Items.Add("");

            foreach (string opcion in ObtenerOpcionesPersonaParaCargo(cargo))
            {
                cell.Items.Add(opcion);
            }

            if (!string.IsNullOrWhiteSpace(valorActual) && !cell.Items.Contains(valorActual))
            {
                cell.Items.Add(valorActual);
            }
        }

        private void DgvAsignacionManoObra_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvAsignacionManoObra.Rows[e.RowIndex];
            string nombreColumna = dgvAsignacionManoObra.Columns[e.ColumnIndex].Name;

            if (nombreColumna == "PersonaAsignada")
            {
                ActualizarPersonaAsignacionDesdeFila(row);
            }

            RecalcularPendientesAsignacionManoObra();
            GuardarAsignacionesManoObraDesdeTabla(false);
        }

        private void NudHorasTandaGlobalManoObra_ValueChanged(object sender, EventArgs e)
        {
            RecalcularPendientesAsignacionManoObra();
        }

        private void FiltroAsignacionManoObra_Changed(object sender, EventArgs e)
        {
            if (cargandoTabla)
            {
                return;
            }

            CargarTablaAsignacionManoObra();
        }

        private void CargarFiltrosAsignacionManoObra()
        {
            if (cmbFiltroAsignacionEtapa == null)
            {
                return;
            }

            cargandoTabla = true;

            string etapa = cmbFiltroAsignacionEtapa.SelectedItem?.ToString() ?? "";
            string cargo = cmbFiltroAsignacionCargo.SelectedItem?.ToString() ?? "";
            string persona = cmbFiltroAsignacionPersona.SelectedItem?.ToString() ?? "";

            List<AsignacionManoObraProyecto> asignaciones = ObtenerAsignacionesManoObraProyecto();

            CargarFiltroSimpleManoObra(cmbFiltroAsignacionEtapa, asignaciones.Select(a => a.Etapa), etapa);
            CargarFiltroSimpleManoObra(cmbFiltroAsignacionCargo, asignaciones.Select(a => a.CargoRequerido), cargo);
            CargarFiltroSimpleManoObra(cmbFiltroAsignacionPersona, ObtenerOpcionesPersonasTodasAsignacion(), persona);

            cargandoTabla = false;
        }

        private void CargarFiltroSimpleManoObra(ComboBox combo, IEnumerable<string> valores, string seleccionAnterior)
        {
            combo.Items.Clear();
            combo.Items.Add("Todos");

            foreach (string valor in valores.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().OrderBy(v => v))
            {
                combo.Items.Add(valor);
            }

            int indice = combo.Items.IndexOf(seleccionAnterior);
            combo.SelectedIndex = indice >= 0 ? indice : 0;
        }

        private List<AsignacionManoObraProyecto> ObtenerAsignacionesManoObraProyecto()
        {
            if (cotizacion.AsignacionesManoObra == null)
            {
                cotizacion.AsignacionesManoObra = new List<AsignacionManoObraProyecto>();
            }

            return cotizacion.AsignacionesManoObra;
        }

        private void CargarTablaAsignacionManoObra()
        {
            if (dgvAsignacionManoObra == null || dgvAsignacionManoObra.Columns.Count == 0)
            {
                return;
            }

            cargandoTabla = true;
            dgvAsignacionManoObra.Rows.Clear();

            IEnumerable<AsignacionManoObraProyecto> asignaciones = ObtenerAsignacionesManoObraProyecto();
            string filtroEtapa = cmbFiltroAsignacionEtapa.SelectedItem?.ToString() ?? "Todos";
            string filtroCargo = cmbFiltroAsignacionCargo.SelectedItem?.ToString() ?? "Todos";
            string filtroPersona = cmbFiltroAsignacionPersona.SelectedItem?.ToString() ?? "Todos";

            if (filtroEtapa != "Todos")
            {
                asignaciones = asignaciones.Where(a => a.Etapa == filtroEtapa);
            }

            if (filtroCargo != "Todos")
            {
                asignaciones = asignaciones.Where(a => a.CargoRequerido == filtroCargo);
            }

            if (filtroPersona != "Todos")
            {
                asignaciones = asignaciones.Where(a => a.PersonaNombre == filtroPersona);
            }

            foreach (AsignacionManoObraProyecto asignacion in asignaciones)
            {
                int rowIndex = dgvAsignacionManoObra.Rows.Add(
                    asignacion.IdAsignacion,
                    asignacion.ClaveLabor,
                    asignacion.PersonaId,
                    asignacion.PiezaSubproducto,
                    asignacion.Etapa,
                    asignacion.SubEtapaLabor,
                    asignacion.CargoRequerido,
                    FormatearHorasManoObra(asignacion.HorasRequeridas),
                    asignacion.PersonaNombre,
                    FormatearHorasManoObra(asignacion.HorasAsignadas),
                    "",
                    asignacion.TipoAsignacion,
                    asignacion.Observaciones
                );

                ConfigurarOpcionesPersonaFila(dgvAsignacionManoObra.Rows[rowIndex]);
            }

            cargandoTabla = false;
            RecalcularPendientesAsignacionManoObra();
        }

        private void ConfigurarOpcionesPersonaFila(DataGridViewRow row)
        {
            string cargo = Convert.ToString(row.Cells["CargoRequerido"].Value) ?? "";
            string valorActual = Convert.ToString(row.Cells["PersonaAsignada"].Value) ?? "";
            DataGridViewComboBoxCell cell = row.Cells["PersonaAsignada"] as DataGridViewComboBoxCell;

            if (cell == null)
            {
                return;
            }

            cell.Items.Clear();
            cell.Items.Add("");

            foreach (string opcion in ObtenerOpcionesPersonaParaCargo(cargo))
            {
                cell.Items.Add(opcion);
            }

            if (!string.IsNullOrWhiteSpace(valorActual) && !cell.Items.Contains(valorActual))
            {
                cell.Items.Add(valorActual);
            }
        }

        private List<string> ObtenerOpcionesPersonasTodasAsignacion()
        {
            List<string> opciones = CargarPersonalActivoManoObra()
                .Select(p => p.Nombre)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            opciones.AddRange(ObtenerAsignacionesManoObraProyecto()
                .Where(a => a.TipoAsignacion == "Recurso generico")
                .Select(a => a.PersonaNombre)
                .Where(n => !string.IsNullOrWhiteSpace(n)));

            return opciones.Distinct().OrderBy(n => n).ToList();
        }

        private List<string> ObtenerOpcionesPersonaParaCargo(string cargo)
        {
            List<string> opciones = CargarPersonalActivoManoObra()
                .Where(p => PersonaCompatibleConCargo(p, cargo))
                .Select(p => p.Nombre)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            string generico = CrearNombreRecursoGenericoCargo(cargo);
            if (!opciones.Contains(generico))
            {
                opciones.Add(generico);
            }

            return opciones;
        }

        private List<PersonaEquipo> CargarPersonalActivoManoObra()
        {
            try
            {
                return BibliotecaPersonalEmpresaJsonService.CargarPersonal()
                    .Where(p => p != null && p.Activo)
                    .ToList();
            }
            catch
            {
                return new List<PersonaEquipo>();
            }
        }

        private bool PersonaCompatibleConCargo(PersonaEquipo persona, string cargo)
        {
            if (persona == null || string.IsNullOrWhiteSpace(cargo))
            {
                return false;
            }

            string cargoNormalizado = NormalizarTextoManoObra(cargo);

            return persona.CargosPosibles != null &&
                persona.CargosPosibles.Any(c =>
                {
                    string posible = NormalizarTextoManoObra(c);
                    return posible == cargoNormalizado ||
                           posible.Contains(cargoNormalizado) ||
                           cargoNormalizado.Contains(posible);
                });
        }

        private string CrearNombreRecursoGenericoCargo(string cargo)
        {
            string nombre = string.IsNullOrWhiteSpace(cargo) ? "Recurso" : cargo.Trim();
            return nombre + " requerido";
        }

        private void ActualizarPersonaAsignacionDesdeFila(DataGridViewRow row)
        {
            string personaNombre = Convert.ToString(row.Cells["PersonaAsignada"].Value) ?? "";
            string cargo = Convert.ToString(row.Cells["CargoRequerido"].Value) ?? "";
            PersonaEquipo persona = CargarPersonalActivoManoObra()
                .FirstOrDefault(p => p.Nombre == personaNombre && PersonaCompatibleConCargo(p, cargo));

            if (persona != null)
            {
                row.Cells["PersonaId"].Value = persona.Id;
                row.Cells["TipoAsignacion"].Value = "Persona real";
                return;
            }

            row.Cells["PersonaId"].Value = "";
            row.Cells["TipoAsignacion"].Value = string.IsNullOrWhiteSpace(personaNombre)
                ? ""
                : "Recurso generico";
        }

        private void GenerarAsignacionesManoObraDesdeDesglose(bool mostrarMensaje)
        {
            if (cotizacion == null)
            {
                return;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                GenerarDesgloseProductivoDesdeEcuaciones();
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                if (mostrarMensaje)
                {
                    MessageBox.Show("No hay desglose productivo para crear asignaciones.", "Sin desglose", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return;
            }

            List<AsignacionManoObraProyecto> asignaciones = ObtenerAsignacionesManoObraProyecto();
            List<LaborManoObraDesdeDesglose> labores = CrearLaboresManoObraDesdeDesglose();
            HashSet<string> clavesActuales = new HashSet<string>(
                labores.Select(l => l.Clave).Where(c => !string.IsNullOrWhiteSpace(c)),
                StringComparer.OrdinalIgnoreCase
            );

            asignaciones.RemoveAll(a =>
                a == null ||
                string.IsNullOrWhiteSpace(a.ClaveLabor) ||
                !clavesActuales.Contains(a.ClaveLabor)
            );

            int creadas = 0;

            foreach (LaborManoObraDesdeDesglose labor in labores)
            {
                if (labor == null || string.IsNullOrWhiteSpace(labor.Clave))
                {
                    continue;
                }

                foreach (AsignacionManoObraProyecto existente in asignaciones.Where(a => a.ClaveLabor == labor.Clave))
                {
                    existente.PiezaSubproducto = labor.Requerimiento == null ? "" : labor.Requerimiento.EntregableCliente;
                    existente.Etapa = labor.Etapa;
                    existente.SubEtapaLabor = NombreRequerimientoManoObra(labor.Requerimiento);
                    existente.CargoRequerido = labor.Cargo;
                    existente.HorasRequeridas = labor.Horas;
                }

                if (asignaciones.Any(a => a.ClaveLabor == labor.Clave))
                {
                    continue;
                }

                string nombreGenerico = CrearNombreRecursoGenericoCargo(labor.Cargo);
                bool hayCompatible = CargarPersonalActivoManoObra().Any(p => PersonaCompatibleConCargo(p, labor.Cargo));

                asignaciones.Add(new AsignacionManoObraProyecto
                {
                    IdAsignacion = Guid.NewGuid().ToString("N"),
                    ClaveLabor = labor.Clave,
                    PiezaSubproducto = labor.Requerimiento == null ? "" : labor.Requerimiento.EntregableCliente,
                    Etapa = labor.Etapa,
                    SubEtapaLabor = NombreRequerimientoManoObra(labor.Requerimiento),
                    CargoRequerido = labor.Cargo,
                    HorasRequeridas = labor.Horas,
                    PersonaNombre = hayCompatible ? "" : nombreGenerico,
                    TipoAsignacion = hayCompatible ? "" : "Recurso generico",
                    HorasAsignadas = 0.0
                });

                creadas++;
            }

            if (mostrarMensaje)
            {
                MessageBox.Show(
                    "Asignaciones actualizadas desde desglose. Nuevas filas: " + creadas.ToString() + ".",
                    "Asignacion de mano de obra",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private string ConstruirClaveAsignacionManoObra(RequerimientoProduccionInterna req, string cargo)
        {
            return string.Join("|", new[]
            {
                NormalizarTextoManoObra(req.EntregableCliente),
                NormalizarTextoManoObra(req.NombreRequerimiento),
                NormalizarTextoManoObra(req.TipoInterno),
                NormalizarTextoManoObra(cargo)
            });
        }

        private void AgregarDivisionAsignacionManoObra()
        {
            if (dgvAsignacionManoObra.CurrentRow == null)
            {
                return;
            }

            GuardarAsignacionesManoObraDesdeTabla(false);
            string id = Convert.ToString(dgvAsignacionManoObra.CurrentRow.Cells["IdAsignacion"].Value) ?? "";
            AsignacionManoObraProyecto origen = ObtenerAsignacionesManoObraProyecto()
                .FirstOrDefault(a => a.IdAsignacion == id);

            if (origen == null)
            {
                return;
            }

            ObtenerAsignacionesManoObraProyecto().Add(new AsignacionManoObraProyecto
            {
                IdAsignacion = Guid.NewGuid().ToString("N"),
                ClaveLabor = origen.ClaveLabor,
                PiezaSubproducto = origen.PiezaSubproducto,
                Etapa = origen.Etapa,
                SubEtapaLabor = origen.SubEtapaLabor,
                CargoRequerido = origen.CargoRequerido,
                HorasRequeridas = origen.HorasRequeridas,
                HorasAsignadas = 0.0,
                TipoAsignacion = "",
                Observaciones = "Division manual"
            });

            CargarFiltrosAsignacionManoObra();
            CargarTablaAsignacionManoObra();
        }

        private void EliminarFilaAsignacionManoObra()
        {
            if (dgvAsignacionManoObra.CurrentRow == null)
            {
                return;
            }

            string id = Convert.ToString(dgvAsignacionManoObra.CurrentRow.Cells["IdAsignacion"].Value) ?? "";
            ObtenerAsignacionesManoObraProyecto().RemoveAll(a => a.IdAsignacion == id);
            CargarTablaAsignacionManoObra();
        }

        private void GuardarAsignacionesManoObraDesdeTabla(bool mostrarMensaje)
        {
            if (dgvAsignacionManoObra == null || dgvAsignacionManoObra.Columns.Count == 0 || cotizacion == null)
            {
                return;
            }

            dgvAsignacionManoObra.EndEdit();

            Dictionary<string, AsignacionManoObraProyecto> existentes = ObtenerAsignacionesManoObraProyecto()
                .Where(a => !string.IsNullOrWhiteSpace(a.IdAsignacion))
                .ToDictionary(a => a.IdAsignacion, a => a);

            foreach (DataGridViewRow row in dgvAsignacionManoObra.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string id = Convert.ToString(row.Cells["IdAsignacion"].Value) ?? "";
                if (string.IsNullOrWhiteSpace(id) || !existentes.ContainsKey(id))
                {
                    continue;
                }

                AsignacionManoObraProyecto asignacion = existentes[id];
                asignacion.PersonaId = Convert.ToString(row.Cells["PersonaId"].Value) ?? "";
                asignacion.PersonaNombre = Convert.ToString(row.Cells["PersonaAsignada"].Value) ?? "";
                asignacion.TipoAsignacion = Convert.ToString(row.Cells["TipoAsignacion"].Value) ?? "";
                asignacion.HorasAsignadas = LeerDoubleCeldaManoObra(row.Cells["HorasAsignadas"].Value);
                asignacion.Observaciones = Convert.ToString(row.Cells["Observaciones"].Value) ?? "";
            }

            if (mostrarMensaje)
            {
                MessageBox.Show("Asignaciones guardadas en el proyecto actual.", "Mano de obra", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool AplicarPlanMensualDesdeAsignacionesManoObra(bool mostrarMensaje)
        {
            if (cotizacion == null || aplicandoPlanDesdeAsignacionManoObra)
            {
                return false;
            }

            List<AsignacionManoObraProyecto> asignaciones = ObtenerAsignacionesManoObraProyecto();
            if (asignaciones.Count == 0)
            {
                GenerarAsignacionesManoObraDesdeDesglose(false);
                asignaciones = ObtenerAsignacionesManoObraProyecto();
            }

            if (asignaciones.Count == 0)
            {
                if (mostrarMensaje)
                {
                    MessageBox.Show(
                        "No hay asignaciones para transformar en plan mensual. Primero carga labores desde el desglose.",
                        "Mano de obra",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                return false;
            }

            aplicandoPlanDesdeAsignacionManoObra = true;

            try
            {
                SincronizarEtapasInternasDesdeDesgloseProductivo();

                List<CategoriaTrabajador> bibliotecaCargos = Cargos.CrearBibliotecaCompleta();
                Dictionary<string, PersonaEquipo> personasPorId = CargarPersonalActivoManoObra()
                    .Where(p => !string.IsNullOrWhiteSpace(p.Id))
                    .GroupBy(p => p.Id)
                    .ToDictionary(g => g.Key, g => g.First());
                Dictionary<string, PersonaEquipo> personasPorNombre = CargarPersonalActivoManoObra()
                    .Where(p => !string.IsNullOrWhiteSpace(p.Nombre))
                    .GroupBy(p => NormalizarTextoManoObra(p.Nombre))
                    .ToDictionary(g => g.Key, g => g.First());

                Dictionary<string, GrupoPlanAsignacionManoObra> grupos = new Dictionary<string, GrupoPlanAsignacionManoObra>();
                List<string> faltantes = new List<string>();

                foreach (AsignacionManoObraProyecto asignacion in asignaciones)
                {
                    if (asignacion == null || string.IsNullOrWhiteSpace(asignacion.CargoRequerido))
                    {
                        continue;
                    }

                    double horas = asignacion.HorasAsignadas > 0.0
                        ? asignacion.HorasAsignadas
                        : asignacion.HorasRequeridas;

                    if (horas <= 0.0)
                    {
                        continue;
                    }

                    CategoriaTrabajador categoria = BuscarCargoRenderizadoEcuacion(
                        bibliotecaCargos,
                        asignacion.CargoRequerido
                    );

                    if (categoria == null)
                    {
                        faltantes.Add(asignacion.CargoRequerido + " (" + asignacion.SubEtapaLabor + ")");
                        continue;
                    }

                    PersonaEquipo persona = ObtenerPersonaAsignadaManoObra(
                        asignacion,
                        personasPorId,
                        personasPorNombre
                    );

                    string personaVisible = ObtenerNombrePersonaPlanManoObra(asignacion, persona);
                    string etapa = NormalizarEtapaProductivaInterna(asignacion.Etapa);
                    string clave = etapa + "|" + categoria.Id.ToString() + "|" + NormalizarTextoManoObra(personaVisible);

                    if (!grupos.ContainsKey(clave))
                    {
                        grupos[clave] = new GrupoPlanAsignacionManoObra
                        {
                            Etapa = etapa,
                            Categoria = categoria,
                            Persona = personaVisible,
                            PersonaEquipo = persona
                        };
                    }

                    GrupoPlanAsignacionManoObra grupo = grupos[clave];
                    grupo.Horas += horas;

                    string actividad = (string.IsNullOrWhiteSpace(asignacion.PiezaSubproducto) ? "Sin pieza" : asignacion.PiezaSubproducto) +
                        " > " +
                        (string.IsNullOrWhiteSpace(asignacion.SubEtapaLabor) ? "Sin labor" : asignacion.SubEtapaLabor) +
                        " - " + FormatearHorasManoObra(horas);

                    if (!grupo.Actividades.Any(a => string.Equals(a, actividad, StringComparison.OrdinalIgnoreCase)))
                    {
                        grupo.Actividades.Add(actividad);
                    }
                }

                if (grupos.Count == 0)
                {
                    if (mostrarMensaje)
                    {
                        MessageBox.Show(
                            "Las asignaciones existen, pero no pude vincular cargos válidos para crear el plan mensual.",
                            "Mano de obra",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }

                    return false;
                }

                foreach (EtapaProyecto etapa in cotizacion.Etapas.Where(e => EtapaListaParaManoObra(e)))
                {
                    etapa.Plan.Clear();
                    etapa.UsaPlanDetallado = false;
                }

                int planesCreados = 0;
                double diasHabilesMes = ObtenerDiasHabilesMesManoObra();

                foreach (GrupoPlanAsignacionManoObra grupo in grupos.Values.OrderBy(g => g.Etapa).ThenBy(g => g.Categoria.NombreCompleto).ThenBy(g => g.Persona))
                {
                    EtapaProyecto etapa = ObtenerOCrearEtapaInterna(grupo.Etapa);
                    etapa.Seleccionada = true;
                    etapa.UsaPlanDetallado = true;

                    double personaMes = grupo.Horas / Math.Max(1.0, diasHabilesMes * HorasDiaEstandarManoObra);
                    double duracionMinima = Math.Max(DuracionMinimaEtapaMesesInterna, personaMes > 0.0 ? 1.0 : 0.0);
                    if (etapa.DuracionMeses <= 0.0)
                    {
                        etapa.DuracionMeses = duracionMinima;
                    }

                    CargoPlanMensual plan = new CargoPlanMensual();
                    plan.Categoria = grupo.Categoria;
                    plan.NombrePersona = grupo.Persona;
                    plan.SueldoMensualCLPEditable = ObtenerSueldoMensualCLPAsignacionManoObra(grupo.PersonaEquipo, grupo.Categoria);
                    plan.SueldoMensualUSDEditable = grupo.Categoria.SueldoMensualUSDTipico;
                    plan.HorasRequeridasDesdeDesglose = grupo.Horas;
                    plan.DiasPersonaDesdeDesglose = grupo.Horas / HorasDiaEstandarManoObra;
                    plan.ActividadesDesglose = grupo.Actividades
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(a => a)
                        .ToList();

                    AplicarCargaMensualDesdePersonaMes(plan, personaMes, etapa.DuracionMeses);

                    if (!etapa.Biblioteca.Any(c => c != null && c.Id == grupo.Categoria.Id))
                    {
                        etapa.Biblioteca.Add(grupo.Categoria);
                    }

                    PlanEtapasService.RecalcularCargo(plan, etapa.DuracionMeses, cotizacion.Moneda);
                    etapa.Plan.Add(plan);
                    PlanEtapasService.RecalcularEtapa(etapa, cotizacion.Moneda);
                    planesCreados++;
                }

                ServicioCotizacion.RecalcularCotizacion(cotizacion);
                CargarCombos();
                CargarTablaManoObra();
                CargarFiltrosAsignacionManoObra();
                CargarTablaAsignacionManoObra();
                RefrescarResumen();
                RefrescarResultadosDetalle();
                RefrescarGanttGrandeEtapas();

                if (panelGantt != null)
                {
                    panelGantt.Invalidate();
                }

                if (mostrarMensaje)
                {
                    string mensaje = "Plan mensual actualizado desde asignaciones: " + planesCreados.ToString() + " fila(s).";
                    if (faltantes.Count > 0)
                    {
                        mensaje += "\n\nCargos no vinculados:\n- " + string.Join("\n- ", faltantes.Distinct().Take(8));
                    }

                    MessageBox.Show(mensaje, "Mano de obra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            finally
            {
                aplicandoPlanDesdeAsignacionManoObra = false;
            }
        }

        private sealed class GrupoPlanAsignacionManoObra
        {
            public string Etapa { get; set; } = "";
            public CategoriaTrabajador Categoria { get; set; }
            public string Persona { get; set; } = "Por definir";
            public PersonaEquipo PersonaEquipo { get; set; }
            public double Horas { get; set; }
            public List<string> Actividades { get; set; } = new List<string>();
        }

        private PersonaEquipo ObtenerPersonaAsignadaManoObra(
            AsignacionManoObraProyecto asignacion,
            Dictionary<string, PersonaEquipo> personasPorId,
            Dictionary<string, PersonaEquipo> personasPorNombre
        )
        {
            if (asignacion == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(asignacion.PersonaId) && personasPorId.ContainsKey(asignacion.PersonaId))
            {
                return personasPorId[asignacion.PersonaId];
            }

            string claveNombre = NormalizarTextoManoObra(asignacion.PersonaNombre);
            if (!string.IsNullOrWhiteSpace(claveNombre) && personasPorNombre.ContainsKey(claveNombre))
            {
                return personasPorNombre[claveNombre];
            }

            return null;
        }

        private string ObtenerNombrePersonaPlanManoObra(AsignacionManoObraProyecto asignacion, PersonaEquipo persona)
        {
            if (persona != null && !string.IsNullOrWhiteSpace(persona.Nombre))
            {
                return persona.Nombre;
            }

            if (asignacion != null && !string.IsNullOrWhiteSpace(asignacion.PersonaNombre))
            {
                return asignacion.PersonaNombre;
            }

            return CrearNombreRecursoGenericoCargo(asignacion == null ? "" : asignacion.CargoRequerido);
        }

        private double ObtenerSueldoMensualCLPAsignacionManoObra(PersonaEquipo persona, CategoriaTrabajador categoria)
        {
            if (persona != null)
            {
                if (persona.CostoHora > 0)
                {
                    return NormalizarSueldoMensualManoObra((double)persona.CostoHora * HorasDiaEstandarManoObra * ObtenerDiasHabilesMesManoObra());
                }

                if (persona.PagoInterno > 0 && string.Equals(persona.PeriodoPago, "Mensual", StringComparison.OrdinalIgnoreCase))
                {
                    return NormalizarSueldoMensualManoObra((double)persona.PagoInterno);
                }
            }

            if (categoria == null)
            {
                return 0.0;
            }

            return NormalizarSueldoMensualManoObra(categoria.SueldoMensualCLPTipico);
        }

        private void RecalcularPendientesAsignacionManoObra()
        {
            if (dgvAsignacionManoObra == null || dgvAsignacionManoObra.Columns.Count == 0)
            {
                return;
            }

            Dictionary<string, double> requeridasPorClave = new Dictionary<string, double>();
            Dictionary<string, double> asignadasPorClave = new Dictionary<string, double>();
            Dictionary<string, double> asignadasPorPersona = new Dictionary<string, double>();

            foreach (DataGridViewRow row in dgvAsignacionManoObra.Rows)
            {
                string clave = Convert.ToString(row.Cells["ClaveLabor"].Value) ?? "";
                if (string.IsNullOrWhiteSpace(clave))
                {
                    continue;
                }

                double requeridas = LeerDoubleCeldaManoObra(row.Cells["HorasRequeridas"].Value);
                double asignadas = LeerDoubleCeldaManoObra(row.Cells["HorasAsignadas"].Value);

                if (!requeridasPorClave.ContainsKey(clave))
                {
                    requeridasPorClave[clave] = requeridas;
                    asignadasPorClave[clave] = 0.0;
                }

                asignadasPorClave[clave] += asignadas;

                string persona = Convert.ToString(row.Cells["PersonaAsignada"].Value) ?? "";
                string tipo = Convert.ToString(row.Cells["TipoAsignacion"].Value) ?? "";
                if (!string.IsNullOrWhiteSpace(persona) && tipo == "Persona real")
                {
                    if (!asignadasPorPersona.ContainsKey(persona))
                    {
                        asignadasPorPersona[persona] = 0.0;
                    }

                    asignadasPorPersona[persona] += asignadas;
                }
            }

            Dictionary<string, PersonaEquipo> personas = CargarPersonalActivoManoObra()
                .GroupBy(p => p.Nombre)
                .ToDictionary(g => g.Key, g => g.First());

            int genericos = 0;

            foreach (DataGridViewRow row in dgvAsignacionManoObra.Rows)
            {
                string clave = Convert.ToString(row.Cells["ClaveLabor"].Value) ?? "";
                double pendientes = requeridasPorClave.ContainsKey(clave)
                    ? Math.Max(0.0, requeridasPorClave[clave] - asignadasPorClave[clave])
                    : 0.0;
                row.Cells["HorasPendientes"].Value = FormatearHorasManoObra(pendientes);

                string tipo = Convert.ToString(row.Cells["TipoAsignacion"].Value) ?? "";
                string persona = Convert.ToString(row.Cells["PersonaAsignada"].Value) ?? "";

                row.DefaultCellStyle.BackColor = Color.White;

                if (tipo == "Recurso generico")
                {
                    genericos++;
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                }

                if (tipo == "Persona real" && personas.ContainsKey(persona))
                {
                    double limite = personas[persona].HorasMaximasPorTanda > 0
                        ? (double)personas[persona].HorasMaximasPorTanda
                        : (double)nudHorasTandaGlobalManoObra.Value;

                    if (asignadasPorPersona.ContainsKey(persona) && asignadasPorPersona[persona] > limite)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 225, 220);
                    }
                }
            }

            double totalRequerido = requeridasPorClave.Values.Sum();
            double totalAsignado = asignadasPorClave.Values.Sum();
            double totalPendiente = Math.Max(0.0, totalRequerido - totalAsignado);
            int laboresConDiferencia = requeridasPorClave.Count(p =>
            {
                double asignadas = asignadasPorClave.ContainsKey(p.Key) ? asignadasPorClave[p.Key] : 0.0;
                return Math.Abs(p.Value - asignadas) > 0.01;
            });
            int sobrecargadas = asignadasPorPersona.Count(p =>
            {
                PersonaEquipo persona = personas.ContainsKey(p.Key) ? personas[p.Key] : null;
                double limite = persona != null && persona.HorasMaximasPorTanda > 0
                    ? (double)persona.HorasMaximasPorTanda
                    : (double)nudHorasTandaGlobalManoObra.Value;
                return p.Value > limite;
            });

            lblResumenAsignacionManoObra.Text =
                "Horas requeridas: " + FormatearHorasManoObra(totalRequerido) +
                " | Asignadas: " + FormatearHorasManoObra(totalAsignado) +
                " | Pendientes: " + FormatearHorasManoObra(totalPendiente) +
                " | Labores sin cuadrar: " + laboresConDiferencia.ToString() +
                " | Personas sobrecargadas: " + sobrecargadas.ToString() +
                " | Recursos genericos: " + genericos.ToString();
        }

        private double LeerDoubleCeldaManoObra(object valor)
        {
            string texto = Convert.ToString(valor) ?? "";
            texto = texto.Replace("h", "").Trim();

            double numero;
            if (double.TryParse(texto, out numero))
            {
                return numero;
            }

            return 0.0;
        }

        private string FormatearHorasManoObra(double horas)
        {
            return horas.ToString("0.##") + " h";
        }

        private void CargarCombos()
        {
            string seleccionAnterior = cmbEtapa.SelectedItem?.ToString() ?? "";

            cmbEtapa.Items.Clear();

            cmbEtapa.Items.Add(NombrePlanGeneralManoObra);

            foreach (EtapaProyecto etapa in cotizacion.Etapas.Where(e => EtapaListaParaManoObra(e)))
            {
                cmbEtapa.Items.Add(etapa);
            }

            cmbEtapa.DisplayMember = "Nombre";

            if (cmbEtapa.Items.Count > 0)
            {
                int indiceRestaurado = -1;

                for (int i = 0; i < cmbEtapa.Items.Count; i++)
                {
                    string textoItem = cmbEtapa.Items[i]?.ToString() ?? "";

                    if (textoItem == seleccionAnterior)
                    {
                        indiceRestaurado = i;
                        break;
                    }

                    EtapaProyecto etapa = cmbEtapa.Items[i] as EtapaProyecto;

                    if (etapa != null && etapa.Nombre == seleccionAnterior)
                    {
                        indiceRestaurado = i;
                        break;
                    }
                }

                cmbEtapa.SelectedIndex = indiceRestaurado >= 0 ? indiceRestaurado : 0;
            }

            bool hayProyecto = DuracionVisibleProyecto() > 0.0;

            cmbEtapa.Enabled = hayProyecto;
            cmbCargo.Enabled = hayProyecto;
            btnAgregarCargo.Enabled = hayProyecto;
            btnEliminarCargo.Enabled = hayProyecto;
            btnRellenarManoObraDesdeDesglose.Enabled = cotizacion != null;
            btnRecalcular.Enabled = hayProyecto;

            CargarComboCargos();
        }

        private void CargarComboCargos()
        {
            cmbCargo.Items.Clear();

            MigrarCargosGeneralesDesdeEtapas();

            if (EsSeleccionPlanGeneral())
            {
                foreach (CategoriaTrabajador cargo in ObtenerBibliotecaCargosGenerales())
                {
                    bool cargoYaAgregado = cotizacion.PlanGeneralManoObra.Any(plan =>
                        plan.Categoria != null &&
                        plan.Categoria.Id == cargo.Id
                    );

                    if (!cargoYaAgregado)
                    {
                        cmbCargo.Items.Add(cargo);
                    }
                }

                cmbCargo.DisplayMember = "NombreCompleto";

                if (cmbCargo.Items.Count > 0)
                {
                    cmbCargo.SelectedIndex = 0;
                    cmbCargo.Enabled = true;
                    btnAgregarCargo.Enabled = true;
                }
                else
                {
                    cmbCargo.Enabled = false;
                    btnAgregarCargo.Enabled = false;
                }

                return;
            }

            EtapaProyecto etapa = cmbEtapa.SelectedItem as EtapaProyecto;

            if (etapa == null)
            {
                cmbCargo.Enabled = false;
                btnAgregarCargo.Enabled = false;
                return;
            }

            foreach (CategoriaTrabajador cargo in etapa.Biblioteca)
            {
                if (EsCargoGeneral(cargo))
                {
                    continue;
                }

                bool cargoYaAgregado = etapa.Plan.Any(plan =>
                    plan.Categoria != null &&
                    plan.Categoria.Id == cargo.Id
                );

                if (!cargoYaAgregado)
                {
                    cmbCargo.Items.Add(cargo);
                }
            }

            cmbCargo.DisplayMember = "NombreCompleto";

            if (cmbCargo.Items.Count > 0)
            {
                cmbCargo.SelectedIndex = 0;
                cmbCargo.Enabled = true;
                btnAgregarCargo.Enabled = true;
            }
            else
            {
                cmbCargo.Enabled = false;
                btnAgregarCargo.Enabled = false;
            }
        }

        private List<CategoriaTrabajador> ObtenerBibliotecaCargosGenerales()
        {
            return cotizacion.Etapas
                .SelectMany(e => e.Biblioteca)
                .Where(c => c != null)
                .Where(c => EsCargoGeneral(c))
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.NombreCompleto)
                .ToList();
        }

        private void CmbEtapa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarComboCargos();
        }

        private bool EsSeleccionPlanGeneral()
        {
            return cmbEtapa.SelectedItem != null &&
                   cmbEtapa.SelectedItem.ToString() == NombrePlanGeneralManoObra;
        }

        private void CargarTablaManoObra()
        {

            MigrarCargosGeneralesDesdeEtapas();

            if (dgvManoObra == null || dgvManoObra.Columns.Count == 0)
            {
                return;
            }

            cargandoTabla = true;

            dgvManoObra.Rows.Clear();

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion;

            bool mostrarVisual = monedaVisual != "CLP";

            if (dgvManoObra.Columns.Contains("ValorMensual"))
            {
                dgvManoObra.Columns["ValorMensual"].HeaderText = "Valor mensual base CLP";
                dgvManoObra.Columns["ValorMensual"].Visible = !mostrarVisual;
            }

            if (dgvManoObra.Columns.Contains("ValorMensualVisual"))
            {
                dgvManoObra.Columns["ValorMensualVisual"].HeaderText = "Valor mensual " + monedaVisual;
                dgvManoObra.Columns["ValorMensualVisual"].Visible = mostrarVisual;
            }

            if (dgvManoObra.Columns.Contains("Costo"))
            {
                dgvManoObra.Columns["Costo"].HeaderText = "Costo base CLP";
                dgvManoObra.Columns["Costo"].Visible = !mostrarVisual;
            }

            if (dgvManoObra.Columns.Contains("CostoVisual"))
            {
                dgvManoObra.Columns["CostoVisual"].HeaderText = "Costo " + monedaVisual;
                dgvManoObra.Columns["CostoVisual"].Visible = mostrarVisual;
            }

            int mesesVisibles = (int)Math.Ceiling(DuracionVisibleProyecto());

            if (mesesVisibles < 1)
            {
                mesesVisibles = 1;
            }

            ConfigurarColumnasMesesVisibles(mesesVisibles);

            CargarFilasCargosGenerales(mesesVisibles);
            CargarFilasCargosPorEtapa(mesesVisibles);

            cargandoTabla = false;

            CargarFiltrosAsignacionManoObra();
            CargarTablaAsignacionManoObra();
        }

        private void ConfigurarColumnasMesesVisibles(int mesesVisibles)
        {
            foreach (DataGridViewColumn columna in dgvManoObra.Columns)
            {
                string nombreColumna = columna.Name;

                if (string.IsNullOrWhiteSpace(nombreColumna))
                {
                    continue;
                }

                if (!nombreColumna.StartsWith("M"))
                {
                    continue;
                }

                string numeroTexto = nombreColumna.Substring(1);

                int numeroMes;

                if (!int.TryParse(numeroTexto, out numeroMes))
                {
                    continue;
                }

                columna.Visible = numeroMes >= 1 && numeroMes <= mesesVisibles;
            }
        }

        private void CargarFilasCargosGenerales(int mesesVisibles)
        {
            if (cotizacion.PlanGeneralManoObra == null)
            {
                cotizacion.PlanGeneralManoObra = new List<CargoPlanMensual>();
            }

            foreach (CargoPlanMensual cargo in cotizacion.PlanGeneralManoObra)
            {
                if (cargo == null || cargo.Categoria == null)
                {
                    continue;
                }

                int rowIndex = dgvManoObra.Rows.Add();
                DataGridViewRow row = dgvManoObra.Rows[rowIndex];

            row.Cells["Etapa"].Value = NombrePlanGeneralManoObra;
            row.Cells["Cargo"].Value = cargo.Categoria.NombreCompleto;
            row.Cells["Persona"].Value = cargo.NombrePersona;
            CargarTrazabilidadDesgloseManoObra(row, cargo);

            CargarValoresCargoBase(row, cargo);
                AsegurarBloquesCargoGeneral(cargo, mesesVisibles);
                CargarMesesCargoGeneral(row, cargo, mesesVisibles);
                CargarTotalesCargo(row, cargo);

                row.Tag = new ManoObraRowTag(null, cargo);
            }
        }

        private void CargarFilasCargosPorEtapa(int mesesVisibles)
        {
            foreach (EtapaProyecto etapa in cotizacion.Etapas.Where(e => e.Seleccionada))
            {
                foreach (CargoPlanMensual cargo in etapa.Plan)
                {
                    if (cargo == null || cargo.Categoria == null)
                    {
                        continue;
                    }

                    int rowIndex = dgvManoObra.Rows.Add();
                    DataGridViewRow row = dgvManoObra.Rows[rowIndex];

            row.Cells["Etapa"].Value = etapa.Nombre;
            row.Cells["Cargo"].Value = cargo.Categoria.NombreCompleto;
            row.Cells["Persona"].Value = cargo.NombrePersona;
            CargarTrazabilidadDesgloseManoObra(row, cargo);

            CargarValoresCargoBase(row, cargo);
                    CargarMesesCargoPorEtapa(row, etapa, cargo, mesesVisibles);
                    CargarTotalesCargo(row, cargo);

                    row.Tag = new ManoObraRowTag(etapa, cargo);
                }
            }
        }

        private void CargarValoresCargoBase(DataGridViewRow row, CargoPlanMensual cargo)
        {
            double valorMensualCLP = cargo.SueldoMensualCLPEditable;

            if (valorMensualCLP <= 0.0 && cargo.Categoria != null)
            {
                valorMensualCLP = cargo.Categoria.SueldoMensualCLPTipico;
                cargo.SueldoMensualCLPEditable = valorMensualCLP;
            }

            valorMensualCLP = NormalizarSueldoMensualManoObra(valorMensualCLP);
            cargo.SueldoMensualCLPEditable = valorMensualCLP;

            row.Cells["ValorMensual"].Value = valorMensualCLP > 0.0
                ? FormatearMiles(valorMensualCLP)
                : "";

            if (dgvManoObra.Columns.Contains("ValorMensualVisual"))
            {
                row.Cells["ValorMensualVisual"].Value = valorMensualCLP > 0.0
                    ? FormatearValorVisual(valorMensualCLP)
                    : "";
            }
        }

        private void CargarTotalesCargo(DataGridViewRow row, CargoPlanMensual cargo)
        {
            row.Cells["PersonaMes"].Value = cargo.PersonaMesTotal.ToString("0.00");

            row.Cells["Costo"].Value = cargo.CostoTotal > 0.0
                ? FormatearMiles(cargo.CostoTotal)
                : "";

            if (dgvManoObra.Columns.Contains("CostoVisual"))
            {
                row.Cells["CostoVisual"].Value = cargo.CostoTotal > 0.0
                    ? FormatearValorVisual(cargo.CostoTotal)
                    : "";
            }
        }

        private void AsegurarBloquesCargoGeneral(CargoPlanMensual cargo, int mesesVisibles)
        {
            while (cargo.PersonasPorBloque.Count < mesesVisibles)
            {
                cargo.PersonasPorBloque.Add(1.0);
            }

            while (cargo.PersonasPorBloque.Count > mesesVisibles)
            {
                cargo.PersonasPorBloque.RemoveAt(cargo.PersonasPorBloque.Count - 1);
            }
        }

        private void CargarMesesCargoGeneral(
            DataGridViewRow row,
            CargoPlanMensual cargo,
            int mesesVisibles
        )
        {
            for (int mes = 1; mes <= mesesVisibles; mes++)
            {
                string nombreColumna = "M" + mes;

                if (!dgvManoObra.Columns.Contains(nombreColumna))
                {
                    continue;
                }

                int indice = mes - 1;
                double valor = cargo.PersonasPorBloque[indice];

                row.Cells[nombreColumna].Value = valor == 0.0
                    ? ""
                    : valor.ToString("0.##");

                row.Cells[nombreColumna].ReadOnly = false;
                row.Cells[nombreColumna].Style.BackColor = Color.White;
            }
        }

        private void CargarMesesCargoPorEtapa(
            DataGridViewRow row,
            EtapaProyecto etapa,
            CargoPlanMensual cargo,
            int mesesVisibles
        )
        {
            int inicioEntero = (int)Math.Floor(etapa.InicioMes);

            for (int mes = 1; mes <= mesesVisibles; mes++)
            {
                string nombreColumna = "M" + mes;

                if (!dgvManoObra.Columns.Contains(nombreColumna))
                {
                    continue;
                }

                int indiceLocal = mes - inicioEntero - 1;

                if (indiceLocal >= 0 && indiceLocal < cargo.PersonasPorBloque.Count)
                {
                    double valor = cargo.PersonasPorBloque[indiceLocal];

                    row.Cells[nombreColumna].Value = valor == 0.0
                        ? ""
                        : valor.ToString("0.##");

                    row.Cells[nombreColumna].ReadOnly = false;
                    row.Cells[nombreColumna].Style.BackColor = Color.White;
                }
                else
                {
                    row.Cells[nombreColumna].Value = "";
                    row.Cells[nombreColumna].ReadOnly = true;
                    row.Cells[nombreColumna].Style.BackColor = Color.Gainsboro;
                }
            }
        }

        private void DgvManoObra_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvManoObra.IsCurrentCellDirty)
            {
                dgvManoObra.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvManoObra_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0)
            {
                return;
            }

            string columnName = dgvManoObra.Columns[e.ColumnIndex].Name;
            DataGridViewRow row = dgvManoObra.Rows[e.RowIndex];

            ManoObraRowTag tag = row.Tag as ManoObraRowTag;

            if (tag == null)
            {
                return;
            }

            if (columnName == "Persona")
            {
                tag.Cargo.NombrePersona = row.Cells["Persona"].Value?.ToString()?.Trim() ?? "Por definir";

                if (string.IsNullOrWhiteSpace(tag.Cargo.NombrePersona))
                {
                    tag.Cargo.NombrePersona = "Por definir";
                }

                RefrescarCalculosYVista();
                return;
            }

            if (columnName == "ValorMensual")
            {
                double valorCLP = ConvertirDouble(row.Cells["ValorMensual"].Value);
                valorCLP = NormalizarSueldoMensualManoObra(valorCLP);

                tag.Cargo.SueldoMensualCLPEditable = valorCLP;

                RefrescarCalculosYVista();
                return;
            }

            if (!columnName.StartsWith("M"))
            {
                return;
            }

            int mesProyecto = ObtenerIndiceBloque(columnName);

            if (mesProyecto < 0)
            {
                return;
            }

            int indiceLocal;

            if (tag.Etapa == null)
            {
                indiceLocal = mesProyecto;
            }
            else
            {
                int inicioEntero = (int)Math.Floor(tag.Etapa.InicioMes);
                indiceLocal = mesProyecto - inicioEntero;
            }

            if (indiceLocal < 0)
            {
                return;
            }

            double nuevoValor = ConvertirDouble(row.Cells[columnName].Value);

            while (tag.Cargo.PersonasPorBloque.Count <= indiceLocal)
            {
                tag.Cargo.PersonasPorBloque.Add(0.0);
            }

            tag.Cargo.PersonasPorBloque[indiceLocal] = nuevoValor;

            EtapaProyecto etapaActual = tag.Etapa;
            CargoPlanMensual cargoActual = tag.Cargo;
            int filaActual = e.RowIndex;
            int columnaActual = e.ColumnIndex;

            RefrescarCalculosYVista();

            BeginInvoke(new Action(() =>
            {
                MoverASiguienteCeldaEditable(etapaActual, cargoActual, filaActual, columnaActual);
            }));
        }

        private void MoverASiguienteCeldaEditable(
            EtapaProyecto etapaActual,
            CargoPlanMensual cargoActual,
            int filaAnterior,
            int columnaAnterior
        )
        {
            if (dgvManoObra.Rows.Count == 0)
            {
                return;
            }

            int filaReconstruida = BuscarFilaManoObra(etapaActual, cargoActual);

            if (filaReconstruida < 0)
            {
                filaReconstruida = Math.Min(filaAnterior, dgvManoObra.Rows.Count - 1);
            }

            for (int col = columnaAnterior + 1; col < dgvManoObra.Columns.Count; col++)
            {
                if (CeldaManoObraEditable(filaReconstruida, col))
                {
                    SeleccionarCeldaManoObra(filaReconstruida, col);
                    return;
                }
            }

            for (int fila = filaReconstruida + 1; fila < dgvManoObra.Rows.Count; fila++)
            {
                for (int col = 0; col < dgvManoObra.Columns.Count; col++)
                {
                    if (CeldaManoObraEditable(fila, col))
                    {
                        SeleccionarCeldaManoObra(fila, col);
                        return;
                    }
                }
            }

            for (int fila = 0; fila <= filaReconstruida; fila++)
            {
                for (int col = 0; col < dgvManoObra.Columns.Count; col++)
                {
                    if (CeldaManoObraEditable(fila, col))
                    {
                        SeleccionarCeldaManoObra(fila, col);
                        return;
                    }
                }
            }
        }

        private int BuscarFilaManoObra(EtapaProyecto etapaActual, CargoPlanMensual cargoActual)
        {
            for (int i = 0; i < dgvManoObra.Rows.Count; i++)
            {
                ManoObraRowTag tag = dgvManoObra.Rows[i].Tag as ManoObraRowTag;

                if (tag != null &&
                    object.ReferenceEquals(tag.Etapa, etapaActual) &&
                    object.ReferenceEquals(tag.Cargo, cargoActual))
                {
                    return i;
                }
            }

            return -1;
        }

        

        private string NormalizarTextoManoObra(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto.Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }

        private void MigrarCargosGeneralesDesdeEtapas()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            if (cotizacion.PlanGeneralManoObra == null)
            {
                cotizacion.PlanGeneralManoObra = new List<CargoPlanMensual>();
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null || etapa.Plan == null)
                {
                    continue;
                }

                List<CargoPlanMensual> cargosParaMover = etapa.Plan
                    .Where(c => c != null && EsCargoGeneral(c.Categoria))
                    .ToList();

                foreach (CargoPlanMensual cargoEtapa in cargosParaMover)
                {
                    if (cargoEtapa.Categoria == null)
                    {
                        etapa.Plan.Remove(cargoEtapa);
                        continue;
                    }

                    CargoPlanMensual cargoGeneralExistente = cotizacion.PlanGeneralManoObra
                        .FirstOrDefault(c =>
                            c.Categoria != null &&
                            c.Categoria.Id == cargoEtapa.Categoria.Id
                        );

                    if (cargoGeneralExistente == null)
                    {
                        CargoPlanMensual cargoGeneral = new CargoPlanMensual();
                        cargoGeneral.Categoria = cargoEtapa.Categoria;
                        cargoGeneral.NombrePersona = string.IsNullOrWhiteSpace(cargoEtapa.NombrePersona)
                            ? "Por definir"
                            : cargoEtapa.NombrePersona;

                        cargoGeneral.SueldoMensualCLPEditable = cargoEtapa.SueldoMensualCLPEditable;

                        if (cargoGeneral.SueldoMensualCLPEditable <= 0.0 && cargoGeneral.Categoria != null)
                        {
                            cargoGeneral.SueldoMensualCLPEditable = cargoGeneral.Categoria.SueldoMensualCLPTipico;
                        }

                        int mesesVisibles = (int)Math.Ceiling(DuracionVisibleProyecto());

                        if (mesesVisibles < 1)
                        {
                            mesesVisibles = 1;
                        }

                        cargoGeneral.PersonasPorBloque.Clear();

                        for (int i = 0; i < mesesVisibles; i++)
                        {
                            cargoGeneral.PersonasPorBloque.Add(1.0);
                        }

                        cotizacion.PlanGeneralManoObra.Add(cargoGeneral);
                    }

                    etapa.Plan.Remove(cargoEtapa);
                }
            }
        }

        private bool CeldaManoObraEditable(int fila, int columna)
        {
            if (fila < 0 || fila >= dgvManoObra.Rows.Count)
            {
                return false;
            }

            if (columna < 0 || columna >= dgvManoObra.Columns.Count)
            {
                return false;
            }

            DataGridViewColumn col = dgvManoObra.Columns[columna];

            if (!col.Name.StartsWith("M"))
            {
                return false;
            }

            DataGridViewCell cell = dgvManoObra.Rows[fila].Cells[columna];

            if (cell.ReadOnly)
            {
                return false;
            }

            if (!cell.Visible)
            {
                return false;
            }

            return true;
        }

        private void SeleccionarCeldaManoObra(int fila, int columna)
        {
            dgvManoObra.ClearSelection();

            dgvManoObra.CurrentCell = dgvManoObra.Rows[fila].Cells[columna];
            dgvManoObra.Rows[fila].Cells[columna].Selected = true;

            dgvManoObra.BeginEdit(true);
        }

        private void BtnAgregarCargo_Click(object sender, EventArgs e)
        {
            CategoriaTrabajador categoria = cmbCargo.SelectedItem as CategoriaTrabajador;

            if (EsSeleccionPlanGeneral())
            {
                AgregarCargoGeneral(categoria);
                return;
            }

            EtapaProyecto etapa = cmbEtapa.SelectedItem as EtapaProyecto;

            AgregarCargoPorEtapa(etapa, categoria);
        }

        private void AgregarCargoGeneral(CategoriaTrabajador categoria)
        {
            if (categoria == null)
            {
                MessageBox.Show("Seleccione un cargo general.");
                return;
            }

            if (cotizacion.PlanGeneralManoObra == null)
            {
                cotizacion.PlanGeneralManoObra = new List<CargoPlanMensual>();
            }

            bool yaExiste = cotizacion.PlanGeneralManoObra.Any(plan =>
                plan.Categoria != null &&
                plan.Categoria.Id == categoria.Id
            );

            if (yaExiste)
            {
                MessageBox.Show(
                    "Este cargo general ya existe. Puede editar su dotación directamente en la tabla.",
                    "Cargo general existente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            CargoPlanMensual cargoGeneral = new CargoPlanMensual();
            cargoGeneral.Categoria = categoria;
            cargoGeneral.NombrePersona = "Por definir";
            cargoGeneral.SueldoMensualCLPEditable = categoria.SueldoMensualCLPTipico;

            int mesesVisibles = (int)Math.Ceiling(DuracionVisibleProyecto());

            if (mesesVisibles < 1)
            {
                mesesVisibles = 1;
            }

            cargoGeneral.PersonasPorBloque.Clear();

            for (int i = 0; i < mesesVisibles; i++)
            {
                cargoGeneral.PersonasPorBloque.Add(1.0);
            }

            cotizacion.PlanGeneralManoObra.Add(cargoGeneral);

            RefrescarCalculosYVista();
        }

        private void AgregarCargoPorEtapa(EtapaProyecto etapa, CategoriaTrabajador categoria)
        {
            if (etapa == null)
            {
                MessageBox.Show("Seleccione una etapa.");
                return;
            }

            if (EsCargoGeneral(categoria))
            {
                MessageBox.Show(
                    "Este cargo es general del proyecto. Agrégalo desde el bloque 'General / proyecto completo', no desde una etapa.",
                    "Cargo general",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (categoria == null)
            {
                MessageBox.Show(
                    "No quedan cargos disponibles para agregar en esta etapa. Puede editar la dotación directamente en la tabla.",
                    "Sin cargos disponibles",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (!EtapaListaParaManoObra(etapa))
            {
                MessageBox.Show(
                    "Primero debe existir una etapa activa calculada desde el desglose productivo.",
                    "Etapa no válida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                tabs.SelectedIndex = 1;
                return;
            }

            if (PlanEtapasService.CargoExisteEnEtapa(etapa, categoria.Id))
            {
                MessageBox.Show(
                    "Este cargo ya existe en la etapa. Puede editar su dotación directamente en la tabla.",
                    "Cargo existente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            PlanEtapasService.AgregarCargoAEtapa(etapa, categoria, cotizacion.Moneda);
            RefrescarCalculosYVista();
        }

        private void BtnEliminarCargo_Click(object sender, EventArgs e)
        {
            if (dgvManoObra.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una fila de mano de obra para eliminar.");
                return;
            }

            ManoObraRowTag tag = dgvManoObra.CurrentRow.Tag as ManoObraRowTag;

            if (tag == null)
            {
                MessageBox.Show("La fila seleccionada no contiene un cargo válido.");
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Eliminar el cargo seleccionado?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            if (tag.Etapa == null)
            {
                cotizacion.PlanGeneralManoObra.Remove(tag.Cargo);
            }
            else
            {
                PlanEtapasService.EliminarCargoDeEtapa(tag.Etapa, tag.Cargo, cotizacion.Moneda);
            }

            RefrescarCalculosYVista();
        }

        private void CargarTrazabilidadDesgloseManoObra(DataGridViewRow row, CargoPlanMensual cargo)
        {
            if (row == null || cargo == null)
            {
                return;
            }

            if (dgvManoObra.Columns.Contains("ActividadesDesglose"))
            {
                List<string> actividades = cargo.ActividadesDesglose ?? new List<string>();
                row.Cells["ActividadesDesglose"].Value = actividades.Count == 0
                    ? "Sin vínculo directo"
                    : string.Join("; ", actividades.Take(6)) + (actividades.Count > 6 ? "; ..." : "");
                row.Cells["ActividadesDesglose"].ToolTipText = actividades.Count == 0
                    ? "Este cargo fue agregado manualmente o no trae trazabilidad desde desglose productivo."
                    : string.Join(Environment.NewLine, actividades);
            }

            if (dgvManoObra.Columns.Contains("HorasDesglose"))
            {
                row.Cells["HorasDesglose"].Value = cargo.HorasRequeridasDesdeDesglose > 0.0
                    ? cargo.HorasRequeridasDesdeDesglose.ToString("0.##") + " h"
                    : "";
            }

            if (dgvManoObra.Columns.Contains("DiasDesglose"))
            {
                row.Cells["DiasDesglose"].Value = cargo.DiasPersonaDesdeDesglose > 0.0
                    ? cargo.DiasPersonaDesdeDesglose.ToString("0.##")
                    : "";
            }
        }

        private void BtnRellenarManoObraDesdeDesglose_Click(object sender, EventArgs e)
        {
            RellenarManoObraDesdeDesgloseProductivo(true);
        }

        private bool RellenarManoObraDesdeDesgloseProductivo(bool mostrarMensaje)
        {
            if (cotizacion == null)
            {
                return false;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                GenerarDesgloseProductivoDesdeEcuaciones();
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                if (mostrarMensaje)
                {
                    MessageBox.Show(
                        "Todavía no hay desglose productivo para transformar en mano de obra.",
                        "Sin desglose",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                return false;
            }

            int asignacionesAntes = ObtenerAsignacionesManoObraProyecto().Count;
            GenerarAsignacionesManoObraDesdeDesglose(false);
            bool aplicado = AplicarPlanMensualDesdeAsignacionesManoObra(false);
            int asignacionesDespues = ObtenerAsignacionesManoObraProyecto().Count;

            if (mostrarMensaje)
            {
                MessageBox.Show(
                    "Mano de obra sincronizada desde el desglose final.\n" +
                    "Asignaciones antes: " + asignacionesAntes.ToString() + "\n" +
                    "Asignaciones actuales: " + asignacionesDespues.ToString(),
                    "Mano de obra desde desglose",
                    MessageBoxButtons.OK,
                    aplicado ? MessageBoxIcon.Information : MessageBoxIcon.Warning
                );
            }

            return aplicado;
        }

        private sealed class LaborManoObraDesdeDesglose
        {
            public RequerimientoProduccionInterna Requerimiento { get; set; }
            public string Cargo { get; set; } = "";
            public string Etapa { get; set; } = "";
            public double Dias { get; set; }
            public double Horas { get; set; }
            public string Clave { get; set; } = "";
            public string Actividad { get; set; } = "";
        }

        private List<LaborManoObraDesdeDesglose> CrearLaboresManoObraDesdeDesglose()
        {
            List<LaborManoObraDesdeDesglose> labores = new List<LaborManoObraDesdeDesglose>();

            if (cotizacion == null ||
                cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null)
            {
                return labores;
            }

            foreach (RequerimientoProduccionInterna req in cotizacion.DesgloseProductivo.Requerimientos)
            {
                if (req == null)
                {
                    continue;
                }

                double dias = ObtenerDiasEstandarManoObra(req);
                if (dias <= 0.0)
                {
                    continue;
                }

                List<CargoPonderadoEcuacion> cargos = ObtenerCargosPonderadosManoObra(req.CargoSugerido);
                if (cargos.Count == 0)
                {
                    cargos.Add(new CargoPonderadoEcuacion { Cargo = "Cargo por definir", Ponderador = 1.0 });
                }

                bool tieneDedicacionExplicita = (req.CargoSugerido ?? "").Contains("|");
                double factorSinDedicacion = cargos.Count > 1 && !tieneDedicacionExplicita
                    ? 1.0 / cargos.Count
                    : 1.0;

                foreach (CargoPonderadoEcuacion cargo in cargos)
                {
                    double factor = tieneDedicacionExplicita
                        ? Math.Max(0.0, cargo.Ponderador)
                        : factorSinDedicacion;

                    double diasAsignables = dias * factor;
                    double horasAsignables = diasAsignables * HorasDiaEstandarManoObra;

                    if (horasAsignables <= 0.0)
                    {
                        continue;
                    }

                    string etapa = NormalizarEtapaProductivaInterna(req.EtapaSugerida);
                    string actividad = (string.IsNullOrWhiteSpace(req.EntregableCliente) ? "Sin pieza" : req.EntregableCliente) +
                        " > " +
                        NombreRequerimientoManoObra(req) +
                        " - " +
                        FormatearHorasManoObra(horasAsignables);

                    labores.Add(new LaborManoObraDesdeDesglose
                    {
                        Requerimiento = req,
                        Cargo = cargo.Cargo,
                        Etapa = etapa,
                        Dias = diasAsignables,
                        Horas = horasAsignables,
                        Clave = ConstruirClaveAsignacionManoObra(req, cargo.Cargo),
                        Actividad = actividad
                    });
                }
            }

            return labores;
        }

        private double ObtenerDiasEstandarManoObra(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return 0.0;
            }

            if (req.DiasPersonaStd > 0.0)
            {
                return req.DiasPersonaStd;
            }

            if (req.DiasPersonaMin > 0.0)
            {
                return req.DiasPersonaMin;
            }

            return req.DiasPersonaHolgura;
        }

        private double ObtenerPersonaMesEstandarManoObra(
            RequerimientoProduccionInterna req,
            CargoPonderadoEcuacion cargoTexto,
            List<CargoPonderadoEcuacion> cargosRequerimiento,
            CategoriaTrabajador categoria,
            double diasFallback
        )
        {
            if (req == null || cargoTexto == null || categoria == null)
            {
                return 0.0;
            }

            double sueldoMensual = NormalizarSueldoMensualManoObra(categoria.SueldoMensualCLPTipico);

            if (sueldoMensual <= 0.0)
            {
                sueldoMensual = NormalizarSueldoMensualManoObra(categoria.SueldoMensualCLP);
            }

            double pesoCargo = Math.Max(0.0, cargoTexto.Ponderador);

            // La dedicacion no reparte un 100% entre cargos: cada cargo suma su propia carga.
            // Ej.: artista 100% + supervisor 10% => dias*1.0 para artista y dias*0.1 para supervisor.
            double diasHabilesMes = ObtenerDiasHabilesMesManoObra();
            return diasHabilesMes <= 0.0
                ? 0.0
                : (diasFallback * pesoCargo) / diasHabilesMes;
        }

        private List<CargoPonderadoEcuacion> ObtenerCargosPonderadosManoObra(string cargosTexto)
        {
            List<CargoPonderadoEcuacion> cargos = new List<CargoPonderadoEcuacion>();

            if (string.IsNullOrWhiteSpace(cargosTexto))
            {
                return cargos;
            }

            string normalizado = cargosTexto
                .Replace(" y ", ";")
                .Replace("\r", ";")
                .Replace("\n", ";");

            foreach (string item in normalizado.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                CargoPonderadoEcuacion cargo = ParsearCargoPonderadoEcuacion(item.Trim());

                if (!string.IsNullOrWhiteSpace(cargo.Cargo))
                {
                    cargos.Add(cargo);
                }
            }

            return cargos
                .GroupBy(c => NormalizarTextoManoObra(c.Cargo))
                .Select(g => new CargoPonderadoEcuacion
                {
                    Cargo = g.First().Cargo,
                    Ponderador = g.Sum(c => c.Ponderador)
                })
                .ToList();
        }

        private void AplicarCargaMensualDesdePersonaMes(
            CargoPlanMensual plan,
            double personaMesTotal,
            double duracionMeses
        )
        {
            PlanEtapasService.AsegurarCantidadBloques(plan, duracionMeses);

            int bloquesActivos = Math.Max(1, plan.PersonasPorBloque.Count);
            double personasPorBloque = personaMesTotal / bloquesActivos;

            for (int i = 0; i < plan.PersonasPorBloque.Count; i++)
            {
                plan.PersonasPorBloque[i] = personasPorBloque;
            }
        }

        private double ObtenerDiasHabilesMesManoObra()
        {
            double diasSemana = cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0
                ? cotizacion.DiasHabilesEstudioPorSemana
                : 5.0;

            return Math.Max(1.0, diasSemana * 4.0);
        }

        private double NormalizarSueldoMensualManoObra(double valorMensualCLP)
        {
            if (valorMensualCLP > 0.0 && valorMensualCLP < 10000.0)
            {
                return valorMensualCLP * 1000.0;
            }

            return valorMensualCLP;
        }

        private string NombreRequerimientoManoObra(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "requerimiento sin nombre";
            }

            if (!string.IsNullOrWhiteSpace(req.NombreRequerimiento))
            {
                return req.NombreRequerimiento;
            }

            if (!string.IsNullOrWhiteSpace(req.EntregableCliente))
            {
                return req.EntregableCliente;
            }

            return "requerimiento sin nombre";
        }

        private void BtnRecalcular_Click(object sender, EventArgs e)
        {
            GuardarAsignacionesManoObraDesdeTabla(false);
            AplicarPlanMensualDesdeAsignacionesManoObra(false);
            RefrescarCalculosYVista();
        }

        private int ObtenerIndiceBloque(string columnName)
        {
            string numero = columnName.Replace("M", "");

            if (int.TryParse(numero, out int mes))
            {
                return mes - 1;
            }

            return -1;
        }

        private bool EsColumnaMesManoObra(string nombreColumna)
        {
            if (string.IsNullOrWhiteSpace(nombreColumna))
            {
                return false;
            }

            if (!nombreColumna.StartsWith("M"))
            {
                return false;
            }

            string numero = nombreColumna.Substring(1);

            int resultado;

            return int.TryParse(numero, out resultado);
        }

        private double ObtenerBloque(CargoPlanMensual cargo, int indice)
        {
            if (indice < cargo.PersonasPorBloque.Count)
            {
                return cargo.PersonasPorBloque[indice];
            }

            return 0.0;
        }
    }
}
