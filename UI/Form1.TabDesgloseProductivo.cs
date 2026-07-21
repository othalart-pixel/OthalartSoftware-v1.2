using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;
using System.Collections.Generic;



namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private DataGridView dgvDesgloseProductivo = new DataGridView();
        private RichTextBox rtbResumenDesgloseProductivo = new RichTextBox();

        private Panel panelResumenVisualDesglose = new Panel();
        private Panel panelEditorRendimientoDesglose = new Panel();
        private Label lblRendimientoFilaDesglose = new Label();
        private Label lblUnidadRendimientoFilaDesglose = new Label();
        private NumericUpDown nudRendimientoFilaDesglose = new NumericUpDown();
        private ComboBox cmbPeriodoRendimientoFilaDesglose = new ComboBox();
        private Button btnAplicarRendimientoFilaDesglose = new Button();
        private Button btnGuardarRendimientoBibliotecaDesglose = new Button();

        private FlowLayoutPanel panelBotonesEscenarioDesglose = new FlowLayoutPanel();

        private Button btnEscenarioMinimoDesglose = new Button();
        private Button btnEscenarioEstandarDesglose = new Button();
        private Button btnEscenarioHolgadoDesglose = new Button();
        private Button btnEscenarioRecomendadoDesglose = new Button();

        private Button btnAplicarPropuestaEtapasDesglose = new Button();
        private Button btnEditarRendimientosDesglose = new Button();

        private EscenarioPlanificacionDesglose escenarioActivoDesglose =
            EscenarioPlanificacionDesglose.Estandar;

        private bool cargandoDesgloseProductivo = false;
        private bool aplicandoCambiosDesgloseProductivo = false;

        private enum AlcanceCambioDetalleCalculo
        {
            Cancelado,
            SoloFila,
            Proceso
        }

        private enum AccionCambiosPendientesDetalleCalculo
        {
            Cancelado,
            Guardar,
            Descartar
        }

        private System.Windows.Forms.Timer temporizadorRecalculoDesgloseProductivo =
    new System.Windows.Forms.Timer();

        private void ConstruirTabDesgloseProductivo(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            tab.Enter -= TabDesgloseProductivo_Enter;
            tab.Enter += TabDesgloseProductivo_Enter;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 5;
            root.Padding = new Padding(22, 18, 22, 22);
            root.BackColor = Color.White;

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 48));

            Label titulo = new Label();
            titulo.Text = "Desglose productivo interno";
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(25, 25, 25);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 4);

            Label subtitulo = new Label();
            subtitulo.Text =
                "Desglose global de todos los productos del proyecto. Puedes editar cantidad, calidad, cargos, rendimiento y horas; cada cambio vuelve al producto y proceso correspondiente.";
            subtitulo.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            subtitulo.ForeColor = Color.FromArgb(90, 90, 90);
            subtitulo.AutoSize = true;
            subtitulo.MaximumSize = new Size(1200, 0);
            subtitulo.Margin = new Padding(0, 0, 0, 10);

            ConfigurarGrillaDesgloseProductivo();
            ConfigurarResumenDesgloseProductivo();
            ConfigurarTemporizadorRecalculoDesgloseProductivo();
            ConfigurarBotonesEscenarioDesglose();

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(subtitulo, 0, 1);
            root.Controls.Add(panelBotonesEscenarioDesglose, 0, 2);
            root.Controls.Add(dgvDesgloseProductivo, 0, 3);
            root.Controls.Add(panelResumenVisualDesglose, 0, 4);

            tab.Controls.Add(root);
        }

        private void TabDesgloseProductivo_Enter(object sender, EventArgs e)
        {
            if (cotizacion == null)
            {
                return;
            }

            if (EsContextoDesgloseProyectoGlobal())
            {
                cotizacion.DesgloseProductivo =
                    ProyectoDesgloseGlobalService.Construir(
                        proyectoCotizacionActual,
                        cotizacion);
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                GenerarDesgloseProductivoDesdeEcuaciones();
            }

            CargarDesgloseProductivoEnPantalla();
            RefrescarResumenDesgloseProductivo();
        }

        private void ConfigurarGrillaDesgloseProductivo()
        {
            dgvDesgloseProductivo.Dock = DockStyle.Fill;
            dgvDesgloseProductivo.AllowUserToAddRows = false;
            dgvDesgloseProductivo.AllowUserToDeleteRows = false;
            dgvDesgloseProductivo.AllowUserToResizeRows = false;
            dgvDesgloseProductivo.RowHeadersVisible = false;

            dgvDesgloseProductivo.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvDesgloseProductivo.MultiSelect = false;
            dgvDesgloseProductivo.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvDesgloseProductivo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDesgloseProductivo.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvDesgloseProductivo.ScrollBars = ScrollBars.Both;

            dgvDesgloseProductivo.BackgroundColor = Color.White;
            dgvDesgloseProductivo.BorderStyle = BorderStyle.FixedSingle;
            dgvDesgloseProductivo.GridColor = Color.Gainsboro;

            dgvDesgloseProductivo.EnableHeadersVisualStyles = false;
            dgvDesgloseProductivo.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvDesgloseProductivo.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvDesgloseProductivo.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvDesgloseProductivo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDesgloseProductivo.ColumnHeadersHeight = 34;

            dgvDesgloseProductivo.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            dgvDesgloseProductivo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvDesgloseProductivo.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgvDesgloseProductivo.Columns.Clear();

            dgvDesgloseProductivo.Columns.Add("ProductoProyecto", "Producto");
            dgvDesgloseProductivo.Columns.Add("EntregableCliente", "Entregable / subproducto");
            dgvDesgloseProductivo.Columns.Add("EcuacionUsada", "Ecuación");
            dgvDesgloseProductivo.Columns.Add("TipoProceso", "Tipo proceso");
            dgvDesgloseProductivo.Columns.Add("TipoInterno", "Tipo interno");
            dgvDesgloseProductivo.Columns.Add("NombreRequerimiento", "Requerimiento");
            dgvDesgloseProductivo.Columns.Add("Cantidad", "Cantidad");
            dgvDesgloseProductivo.Columns.Add("Unidad", "Unidad");
            dgvDesgloseProductivo.Columns.Add("EtapaSugerida", "Etapa");
            dgvDesgloseProductivo.Columns.Add("BloqueProductivo", "Bloque");

            DataGridViewComboBoxColumn colModoPlanificacion = new DataGridViewComboBoxColumn();
            colModoPlanificacion.Name = "ModoPlanificacion";
            colModoPlanificacion.HeaderText = "Flujo";
            colModoPlanificacion.Items.Add("Secuencial");
            colModoPlanificacion.Items.Add("Paralelo");
            colModoPlanificacion.FlatStyle = FlatStyle.Flat;
            dgvDesgloseProductivo.Columns.Add(colModoPlanificacion);

            dgvDesgloseProductivo.Columns.Add("DependeDe", "Depende de");

            DataGridViewComboBoxColumn colCalidad = new DataGridViewComboBoxColumn();
            colCalidad.Name = "Calidad";
            colCalidad.HeaderText = "Calidad";
            colCalidad.Items.Add("Baja / boceto");
            colCalidad.Items.Add("Estándar");
            colCalidad.Items.Add("Alta / pulido");
            colCalidad.Items.Add("Premium");
            colCalidad.FlatStyle = FlatStyle.Flat;
            dgvDesgloseProductivo.Columns.Add(colCalidad);

            DataGridViewComboBoxColumn colCargo = new DataGridViewComboBoxColumn();
            colCargo.Name = "CargoSugerido";
            colCargo.HeaderText = "Trabajo / cargo";
            colCargo.FlatStyle = FlatStyle.Flat;
            CargarOpcionesCargosDesglose(colCargo);
            dgvDesgloseProductivo.Columns.Add(colCargo);

            dgvDesgloseProductivo.Columns.Add("NivelCargoSugerido", "Nivel");
            dgvDesgloseProductivo.Columns.Add("SueldoMensualCargoCLP", "Sueldo cargo");
            dgvDesgloseProductivo.Columns.Add("TarifaDiaCargoCLP", "Costo/día calc.");

            dgvDesgloseProductivo.Columns.Add("RendimientoCantidad", "Capacidad");

            DataGridViewComboBoxColumn colRendimientoPeriodo = new DataGridViewComboBoxColumn();
            colRendimientoPeriodo.Name = "RendimientoPeriodo";
            colRendimientoPeriodo.HeaderText = "Periodo";
            colRendimientoPeriodo.Items.Add("día");
            colRendimientoPeriodo.Items.Add("semana");
            colRendimientoPeriodo.Items.Add("mes");
            colRendimientoPeriodo.FlatStyle = FlatStyle.Flat;
            dgvDesgloseProductivo.Columns.Add(colRendimientoPeriodo);

            dgvDesgloseProductivo.Columns.Add("RendimientoOrigen", "Origen rendimiento");

            DataGridViewComboBoxColumn colModoCalculo = new DataGridViewComboBoxColumn();
            colModoCalculo.Name = "ModoCalculoProductivo";
            colModoCalculo.HeaderText = "Cálculo";
            colModoCalculo.Items.Add(ModosCalculoProductivo.Rendimiento);
            colModoCalculo.Items.Add(ModosCalculoProductivo.TiempoAsignado);
            colModoCalculo.FlatStyle = FlatStyle.Flat;
            dgvDesgloseProductivo.Columns.Add(colModoCalculo);

            dgvDesgloseProductivo.Columns.Add("HorasMinimas", "Horas mín.");
            dgvDesgloseProductivo.Columns.Add("HorasEstandar", "Horas std.");
            dgvDesgloseProductivo.Columns.Add("HorasHolgura", "Horas holg.");
            dgvDesgloseProductivo.Columns.Add("OrigenHoras", "Origen horas");

            dgvDesgloseProductivo.Columns.Add("DiasPersonaMin", "Días min.");
            dgvDesgloseProductivo.Columns.Add("DiasPersonaStd", "Días std.");
            dgvDesgloseProductivo.Columns.Add("DiasPersonaHolgura", "Días holg.");

            dgvDesgloseProductivo.Columns.Add("CostoMinimoCLP", "Costo min.");
            dgvDesgloseProductivo.Columns.Add("CostoEstandarCLP", "Costo std.");
            dgvDesgloseProductivo.Columns.Add("CostoHolguraCLP", "Costo holg.");

            dgvDesgloseProductivo.Columns.Add("Nota", "Nota");

            OcultarColumnaDesglose("EcuacionUsada");
            OcultarColumnaDesglose("NivelCargoSugerido");
            OcultarColumnaDesglose("SueldoMensualCargoCLP");

            // NO ocultar costos.
            // OcultarColumnaDesglose("CostoMinimoCLP");
            // OcultarColumnaDesglose("CostoEstandarCLP");
            // OcultarColumnaDesglose("CostoHolguraCLP");

            MarcarColumnasSoloLecturaDesglose();
            AjustarAnchosColumnasDesglose();
            OrdenarColumnasClaveDesglose();

            dgvDesgloseProductivo.DataError -= DgvDesgloseProductivo_DataError;
            dgvDesgloseProductivo.DataError += DgvDesgloseProductivo_DataError;

            dgvDesgloseProductivo.CellEndEdit -= DgvDesgloseProductivo_CellEndEdit;
            dgvDesgloseProductivo.CellEndEdit += DgvDesgloseProductivo_CellEndEdit;

            dgvDesgloseProductivo.CellValidated -= DgvDesgloseProductivo_CellValidated;
            dgvDesgloseProductivo.CellValidated += DgvDesgloseProductivo_CellValidated;

            dgvDesgloseProductivo.CellValueChanged -= DgvDesgloseProductivo_CellValueChanged;

            dgvDesgloseProductivo.CurrentCellDirtyStateChanged -= DgvDesgloseProductivo_CurrentCellDirtyStateChanged;
            dgvDesgloseProductivo.CurrentCellDirtyStateChanged += DgvDesgloseProductivo_CurrentCellDirtyStateChanged;

            dgvDesgloseProductivo.SelectionChanged -= DgvDesgloseProductivo_SelectionChanged;
            dgvDesgloseProductivo.SelectionChanged += DgvDesgloseProductivo_SelectionChanged;

            dgvDesgloseProductivo.CellDoubleClick -= DgvDesgloseProductivo_CellDoubleClick;
            dgvDesgloseProductivo.CellDoubleClick += DgvDesgloseProductivo_CellDoubleClick;
        }

        private void ConfigurarEditorRendimientoDesglose()
        {
            panelEditorRendimientoDesglose.Dock = DockStyle.Top;
            panelEditorRendimientoDesglose.Height = 54;
            panelEditorRendimientoDesglose.Padding = new Padding(12, 8, 12, 8);
            panelEditorRendimientoDesglose.Margin = new Padding(0, 0, 0, 10);
            panelEditorRendimientoDesglose.BackColor = Color.FromArgb(248, 249, 251);
            panelEditorRendimientoDesglose.BorderStyle = BorderStyle.FixedSingle;

            FlowLayoutPanel fila = new FlowLayoutPanel();
            fila.Dock = DockStyle.Fill;
            fila.FlowDirection = FlowDirection.LeftToRight;
            fila.WrapContents = false;
            fila.AutoScroll = false;

            lblRendimientoFilaDesglose.Text = "Capacidad de la fila seleccionada:";
            lblRendimientoFilaDesglose.AutoSize = true;
            lblRendimientoFilaDesglose.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            lblRendimientoFilaDesglose.Margin = new Padding(0, 7, 10, 0);

            nudRendimientoFilaDesglose.DecimalPlaces = 2;
            nudRendimientoFilaDesglose.Minimum = 0;
            nudRendimientoFilaDesglose.Maximum = 100000;
            nudRendimientoFilaDesglose.Width = 90;
            nudRendimientoFilaDesglose.Margin = new Padding(0, 2, 6, 0);

            lblUnidadRendimientoFilaDesglose.Text = "unidades";
            lblUnidadRendimientoFilaDesglose.AutoSize = true;
            lblUnidadRendimientoFilaDesglose.Margin = new Padding(0, 7, 6, 0);

            Label lblPor = new Label();
            lblPor.Text = "por";
            lblPor.AutoSize = true;
            lblPor.Margin = new Padding(0, 7, 6, 0);

            cmbPeriodoRendimientoFilaDesglose.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriodoRendimientoFilaDesglose.Width = 110;
            cmbPeriodoRendimientoFilaDesglose.Items.Clear();
            cmbPeriodoRendimientoFilaDesglose.Items.Add("dia");
            cmbPeriodoRendimientoFilaDesglose.Items.Add("semana");
            cmbPeriodoRendimientoFilaDesglose.Items.Add("mes");
            cmbPeriodoRendimientoFilaDesglose.SelectedItem = "semana";
            cmbPeriodoRendimientoFilaDesglose.Margin = new Padding(0, 2, 10, 0);

            btnAplicarRendimientoFilaDesglose.Text = "Aplicar";
            btnAplicarRendimientoFilaDesglose.Width = 95;
            btnAplicarRendimientoFilaDesglose.Height = 30;
            btnAplicarRendimientoFilaDesglose.Margin = new Padding(0, 0, 8, 0);
            btnAplicarRendimientoFilaDesglose.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAplicarRendimientoFilaDesglose.Click -= BtnAplicarRendimientoFilaDesglose_Click;
            btnAplicarRendimientoFilaDesglose.Click += BtnAplicarRendimientoFilaDesglose_Click;

            btnGuardarRendimientoBibliotecaDesglose.Text = "Guardar en JSON";
            btnGuardarRendimientoBibliotecaDesglose.Width = 130;
            btnGuardarRendimientoBibliotecaDesglose.Height = 30;
            btnGuardarRendimientoBibliotecaDesglose.Margin = new Padding(0, 0, 8, 0);
            btnGuardarRendimientoBibliotecaDesglose.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnGuardarRendimientoBibliotecaDesglose.Click -= BtnGuardarRendimientoBibliotecaDesglose_Click;
            btnGuardarRendimientoBibliotecaDesglose.Click += BtnGuardarRendimientoBibliotecaDesglose_Click;

            fila.Controls.Add(lblRendimientoFilaDesglose);
            fila.Controls.Add(nudRendimientoFilaDesglose);
            fila.Controls.Add(lblUnidadRendimientoFilaDesglose);
            fila.Controls.Add(lblPor);
            fila.Controls.Add(cmbPeriodoRendimientoFilaDesglose);
            fila.Controls.Add(btnAplicarRendimientoFilaDesglose);
            fila.Controls.Add(btnGuardarRendimientoBibliotecaDesglose);

            panelEditorRendimientoDesglose.Controls.Clear();
            panelEditorRendimientoDesglose.Controls.Add(fila);
        }

        private void OcultarColumnaDesglose(string nombreColumna)
        {
            if (dgvDesgloseProductivo.Columns.Contains(nombreColumna))
            {
                dgvDesgloseProductivo.Columns[nombreColumna].Visible = false;
            }
        }

        private void OrdenarColumnasClaveDesglose()
        {
            SetDisplayIndexDesglose("EntregableCliente", 0);
            SetDisplayIndexDesglose("TipoProceso", 1);
            SetDisplayIndexDesglose("TipoInterno", 2);
            SetDisplayIndexDesglose("NombreRequerimiento", 3);
            SetDisplayIndexDesglose("Cantidad", 4);
            SetDisplayIndexDesglose("Unidad", 5);
            SetDisplayIndexDesglose("RendimientoCantidad", 6);
            SetDisplayIndexDesglose("RendimientoPeriodo", 7);
            SetDisplayIndexDesglose("EtapaSugerida", 8);
            SetDisplayIndexDesglose("BloqueProductivo", 9);
            SetDisplayIndexDesglose("RendimientoOrigen", 10);
            SetDisplayIndexDesglose("ModoCalculoProductivo", 11);
            SetDisplayIndexDesglose("HorasMinimas", 12);
            SetDisplayIndexDesglose("HorasEstandar", 13);
            SetDisplayIndexDesglose("HorasHolgura", 14);
            SetDisplayIndexDesglose("OrigenHoras", 15);
            SetDisplayIndexDesglose("TarifaDiaCargoCLP", 16);
            SetDisplayIndexDesglose("DiasPersonaMin", 17);
            SetDisplayIndexDesglose("DiasPersonaStd", 18);
            SetDisplayIndexDesglose("DiasPersonaHolgura", 19);
            SetDisplayIndexDesglose("CostoMinimoCLP", 20);
            SetDisplayIndexDesglose("CostoEstandarCLP", 21);
            SetDisplayIndexDesglose("CostoHolguraCLP", 22);
            SetDisplayIndexDesglose("ProductoProyecto", 0);
        }

        private void SetDisplayIndexDesglose(string nombreColumna, int indice)
        {
            if (!dgvDesgloseProductivo.Columns.Contains(nombreColumna))
            {
                return;
            }

            try
            {
                dgvDesgloseProductivo.Columns[nombreColumna].DisplayIndex = indice;
            }
            catch
            {
            }
        }

        private void MarcarColumnasSoloLecturaDesglose()
        {
            string[] readOnly =
            {
        "ProductoProyecto",
        "EntregableCliente",
        "EcuacionUsada",
        "TipoProceso",
        "TipoInterno",
        "NombreRequerimiento",
        "Unidad",
        "EtapaSugerida",
        "NivelCargoSugerido",
        "SueldoMensualCargoCLP",
        "TarifaDiaCargoCLP",
        "RendimientoOrigen",
        "OrigenHoras",
        "CostoMinimoCLP",
        "CostoEstandarCLP",
        "CostoHolguraCLP",
        "Nota"
    };

            foreach (string col in readOnly)
            {
                if (dgvDesgloseProductivo.Columns.Contains(col))
                {
                    dgvDesgloseProductivo.Columns[col].ReadOnly = true;
                }
            }

            string[] editables =
            {
        "Cantidad",
        "BloqueProductivo",
        "ModoPlanificacion",
        "DependeDe",
        "Calidad",
        "CargoSugerido",
        "RendimientoCantidad",
        "RendimientoPeriodo",
        "ModoCalculoProductivo",
        "HorasMinimas",
        "HorasEstandar",
        "HorasHolgura",
        "DiasPersonaMin",
        "DiasPersonaStd",
        "DiasPersonaHolgura"
    };

            foreach (string col in editables)
            {
                if (dgvDesgloseProductivo.Columns.Contains(col))
                {
                    dgvDesgloseProductivo.Columns[col].ReadOnly = false;
                    dgvDesgloseProductivo.Columns[col].DefaultCellStyle.BackColor =
                        Color.FromArgb(255, 255, 235);
                }
            }
        }

        private void AjustarAnchosColumnasDesglose()
        {
            SetColumnWidthDesglose("EntregableCliente", 190);
            SetColumnWidthDesglose("ProductoProyecto", 210);
            SetColumnWidthDesglose("TipoProceso", 145);
            SetColumnWidthDesglose("TipoInterno", 155);
            SetColumnWidthDesglose("NombreRequerimiento", 300);
            SetColumnWidthDesglose("Cantidad", 85);
            SetColumnWidthDesglose("Unidad", 95);
            SetColumnWidthDesglose("EtapaSugerida", 135);
            SetColumnWidthDesglose("BloqueProductivo", 145);
            SetColumnWidthDesglose("ModoPlanificacion", 120);
            SetColumnWidthDesglose("DependeDe", 220);
            SetColumnWidthDesglose("Calidad", 130);
            SetColumnWidthDesglose("CargoSugerido", 260);
            SetColumnWidthDesglose("RendimientoCantidad", 95);
            SetColumnWidthDesglose("RendimientoPeriodo", 95);
            SetColumnWidthDesglose("RendimientoOrigen", 190);
            SetColumnWidthDesglose("ModoCalculoProductivo", 135);
            SetColumnWidthDesglose("HorasMinimas", 95);
            SetColumnWidthDesglose("HorasEstandar", 95);
            SetColumnWidthDesglose("HorasHolgura", 95);
            SetColumnWidthDesglose("OrigenHoras", 150);
            SetColumnWidthDesglose("TarifaDiaCargoCLP", 130);

            SetColumnWidthDesglose("DiasPersonaMin", 95);
            SetColumnWidthDesglose("DiasPersonaStd", 95);
            SetColumnWidthDesglose("DiasPersonaHolgura", 105);

            SetColumnWidthDesglose("CostoMinimoCLP", 120);
            SetColumnWidthDesglose("CostoEstandarCLP", 120);
            SetColumnWidthDesglose("CostoHolguraCLP", 125);

            SetColumnWidthDesglose("Nota", 520);
        }

        private void SetColumnWidthDesglose(string columnName, int width)
        {
            if (dgvDesgloseProductivo.Columns.Contains(columnName))
            {
                dgvDesgloseProductivo.Columns[columnName].Width = width;
                dgvDesgloseProductivo.Columns[columnName].MinimumWidth = Math.Min(width, 80);
                dgvDesgloseProductivo.Columns[columnName].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void AjustarColumnasDesgloseAlContenido()
        {
            if (dgvDesgloseProductivo == null || dgvDesgloseProductivo.Columns.Count == 0)
            {
                return;
            }

            try
            {
                dgvDesgloseProductivo.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                AsegurarAnchoMinimoDesglose("EntregableCliente", 210);
                AsegurarAnchoMinimoDesglose("TipoInterno", 135);
                AsegurarAnchoMinimoDesglose("NombreRequerimiento", 285);
                AsegurarAnchoMinimoDesglose("Cantidad", 90);
                AsegurarAnchoMinimoDesglose("Unidad", 100);
                AsegurarAnchoMinimoDesglose("EtapaSugerida", 145);
                AsegurarAnchoMinimoDesglose("Calidad", 140);
                AsegurarAnchoMinimoDesglose("CargoSugerido", 220);
                AsegurarAnchoMinimoDesglose("RendimientoCantidad", 95);
                AsegurarAnchoMinimoDesglose("RendimientoPeriodo", 95);
                AsegurarAnchoMinimoDesglose("RendimientoOrigen", 170);
                AsegurarAnchoMinimoDesglose("DiasPersonaMin", 95);
                AsegurarAnchoMinimoDesglose("DiasPersonaStd", 95);
                AsegurarAnchoMinimoDesglose("DiasPersonaHolgura", 105);
                AsegurarAnchoMinimoDesglose("CostoMinimoCLP", 120);
                AsegurarAnchoMinimoDesglose("CostoEstandarCLP", 120);
                AsegurarAnchoMinimoDesglose("CostoHolguraCLP", 125);
                AsegurarAnchoMinimoDesglose("Nota", 520);
            }
            catch
            {
                AjustarAnchosColumnasDesglose();
            }
        }

        private void AsegurarAnchoMinimoDesglose(string columnName, int minimo)
        {
            if (dgvDesgloseProductivo == null)
            {
                return;
            }

            if (!dgvDesgloseProductivo.Columns.Contains(columnName))
            {
                return;
            }

            DataGridViewColumn col = dgvDesgloseProductivo.Columns[columnName];

            if (!col.Visible)
            {
                return;
            }

            if (col.Width < minimo)
            {
                col.Width = minimo;
            }

            col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void CargarOpcionesCargosDesglose(DataGridViewComboBoxColumn colCargo)
        {
            if (colCargo == null)
            {
                return;
            }

            colCargo.Items.Clear();

            InicializarCargosGenerales();

            if (bibliotecaCargosGenerales == null)
            {
                return;
            }

            foreach (string opcion in bibliotecaCargosGenerales
                .Where(c => c != null)
                .OrderBy(c => ObtenerOrdenEtapaDesdeDesgloseLocal(c.Bloque))
                .ThenBy(c => c.Nombre)
                .ThenBy(c => c.Nivel)
                .Select(c => ObtenerTextoCargoDesglose(c))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct())
            {
                colCargo.Items.Add(opcion);
            }
        }

        private void AsegurarOpcionCargoDesglose(string opcion)
        {
            if (string.IsNullOrWhiteSpace(opcion))
            {
                return;
            }

            DataGridViewComboBoxColumn colCargo =
                dgvDesgloseProductivo.Columns["CargoSugerido"] as DataGridViewComboBoxColumn;

            if (colCargo == null)
            {
                return;
            }

            if (!colCargo.Items.Contains(opcion))
            {
                colCargo.Items.Add(opcion);
            }
        }

        private string ObtenerTextoCargoDesglose(CategoriaTrabajador cargo)
        {
            if (cargo == null)
            {
                return "";
            }

            return ObtenerTextoCargoDesglose(cargo.Nombre, cargo.Nivel);
        }

        private string ObtenerTextoCargoDesglose(string nombre, string nivel)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(nivel))
            {
                return nombre.Trim();
            }

            return nombre.Trim() + " (" + nivel.Trim() + ")";
        }

        private void AplicarSeleccionCargoDesglose(
            RequerimientoProduccionInterna req,
            object valorSeleccionado
        )
        {
            if (req == null || valorSeleccionado == null)
            {
                return;
            }

            string texto = valorSeleccionado.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(texto))
            {
                return;
            }

            CategoriaTrabajador cargo = ObtenerCargoDesdeTextoDesglose(texto);

            if (cargo != null)
            {
                req.CargoSugerido = cargo.Nombre;
                req.CargoId = cargo.Nombre;
                req.NivelCargoSugerido = cargo.Nivel;
                return;
            }

            string nombre = texto.Trim();
            string nivel = "";

            int idx = texto.LastIndexOf(" (", StringComparison.Ordinal);

            if (idx > 0 && texto.EndsWith(")", StringComparison.Ordinal))
            {
                nombre = texto.Substring(0, idx).Trim();
                nivel = texto.Substring(idx + 2, texto.Length - idx - 3).Trim();
            }

            req.CargoSugerido = nombre;
            req.CargoId = nombre;
            req.NivelCargoSugerido = nivel;
        }

        private CategoriaTrabajador ObtenerCargoDesdeTextoDesglose(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            InicializarCargosGenerales();

            string normalizado = NormalizarEtapaDesglose(texto);

            if (bibliotecaCargosGenerales != null)
            {
                CategoriaTrabajador desdeGeneral = bibliotecaCargosGenerales
                    .FirstOrDefault(c =>
                        c != null &&
                        NormalizarEtapaDesglose(ObtenerTextoCargoDesglose(c)) == normalizado);

                if (desdeGeneral != null)
                {
                    return desdeGeneral;
                }
            }

            return null;
        }

        private void ConfigurarBotonesEscenarioDesglose()
        {
            panelBotonesEscenarioDesglose.Controls.Clear();
            panelBotonesEscenarioDesglose.Dock = DockStyle.Fill;
            panelBotonesEscenarioDesglose.FlowDirection = FlowDirection.LeftToRight;
            panelBotonesEscenarioDesglose.AutoSize = true;
            panelBotonesEscenarioDesglose.WrapContents = true;
            panelBotonesEscenarioDesglose.Margin = new Padding(0, 0, 0, 12);

            ConfigurarBotonEscenario(
                btnEscenarioMinimoDesglose,
                "Aplicar mínimo",
                EscenarioPlanificacionDesglose.Minimo
            );

            ConfigurarBotonEscenario(
                btnEscenarioEstandarDesglose,
                "Aplicar estándar",
                EscenarioPlanificacionDesglose.Estandar
            );

            ConfigurarBotonEscenario(
                btnEscenarioHolgadoDesglose,
                "Aplicar holgado",
                EscenarioPlanificacionDesglose.Holgado
            );

            ConfigurarBotonEscenario(
                btnEscenarioRecomendadoDesglose,
                "Aplicar recomendado por presupuesto",
                EscenarioPlanificacionDesglose.Recomendado
            );

            ConfigurarBotonAplicarPropuestaEtapasDesglose();
            ConfigurarBotonEditarRendimientosDesglose();

            panelBotonesEscenarioDesglose.Controls.Add(btnEscenarioMinimoDesglose);
            panelBotonesEscenarioDesglose.Controls.Add(btnEscenarioEstandarDesglose);
            panelBotonesEscenarioDesglose.Controls.Add(btnEscenarioHolgadoDesglose);
            panelBotonesEscenarioDesglose.Controls.Add(btnEscenarioRecomendadoDesglose);
            panelBotonesEscenarioDesglose.Controls.Add(btnAplicarPropuestaEtapasDesglose);
            panelBotonesEscenarioDesglose.Controls.Add(btnEditarRendimientosDesglose);

            RefrescarEstadoBotonesEscenarioDesglose();
        }

        private void ConfigurarBotonAplicarPropuestaEtapasDesglose()
        {
            btnAplicarPropuestaEtapasDesglose.Text = "Aplicar propuesta a etapas";
            btnAplicarPropuestaEtapasDesglose.Width = 230;
            btnAplicarPropuestaEtapasDesglose.Height = 36;
            btnAplicarPropuestaEtapasDesglose.Margin = new Padding(0, 0, 8, 8);
            btnAplicarPropuestaEtapasDesglose.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            btnAplicarPropuestaEtapasDesglose.BackColor = Color.FromArgb(33, 150, 243);
            btnAplicarPropuestaEtapasDesglose.ForeColor = Color.White;

            btnAplicarPropuestaEtapasDesglose.Click -= BtnAplicarPropuestaEtapasDesglose_Click;
            btnAplicarPropuestaEtapasDesglose.Click += BtnAplicarPropuestaEtapasDesglose_Click;
        }


        private Panel CrearBarraRangoVisualDobleReferencia(
    double minimo,
    double estandar,
    double holgado,
    double referenciaActiva,
    double referenciaCliente,
    Color colorActivo
)
        {
            Panel panel = new Panel();
            panel.Height = 48;
            panel.BackColor = Color.White;

            panel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int left = 18;
                int right = panel.Width - 18;
                int y = 24;

                if (right <= left)
                {
                    return;
                }

                Pen lineaBase = new Pen(Color.FromArgb(210, 210, 210), 5);
                g.DrawLine(lineaBase, left, y, right, y);

                int xMin = left;
                int xEst = CalcularXBarra(estandar, minimo, holgado, left, right);
                int xHol = right;
                int xActivo = CalcularXBarra(referenciaActiva, minimo, holgado, left, right);
                int xCliente = CalcularXBarra(referenciaCliente, minimo, holgado, left, right);

                Pen lineaActiva = new Pen(colorActivo, 5);
                g.DrawLine(lineaActiva, left, y, xActivo, y);

                Brush brushMin = new SolidBrush(Color.FromArgb(80, 80, 80));
                Brush brushEst = new SolidBrush(Color.FromArgb(40, 40, 40));
                Brush brushHol = new SolidBrush(Color.FromArgb(80, 80, 80));
                Brush brushActivo = new SolidBrush(colorActivo);
                Brush brushCliente = new SolidBrush(Color.FromArgb(40, 40, 40));

                g.FillEllipse(brushMin, xMin - 5, y - 5, 10, 10);

                Point[] triangulo =
                {
            new Point(xEst, y - 8),
            new Point(xEst - 7, y + 6),
            new Point(xEst + 7, y + 6)
        };

                g.FillPolygon(brushEst, triangulo);

                g.FillRectangle(brushHol, xHol - 5, y - 5, 10, 10);

                Point[] diamanteActivo =
                {
            new Point(xActivo, y - 10),
            new Point(xActivo - 10, y),
            new Point(xActivo, y + 10),
            new Point(xActivo + 10, y)
        };

                g.FillPolygon(brushActivo, diamanteActivo);

                if (referenciaCliente > 0.0)
                {
                    int yCliente = y + 13;

                    Point[] diamanteCliente =
                    {
                new Point(xCliente, yCliente - 8),
                new Point(xCliente - 8, yCliente),
                new Point(xCliente, yCliente + 8),
                new Point(xCliente + 8, yCliente)
            };

                    g.FillPolygon(brushCliente, diamanteCliente);
                }
            };

            return panel;
        }

        private Panel CrearTarjetaRangoDobleReferencia(
    string titulo,
    string subtitulo,
    double minimo,
    double estandar,
    double holgado,
    double referenciaActiva,
    string etiquetaActiva,
    double referenciaCliente,
    string etiquetaCliente,
    string unidad,
    Color color
)
        {
            Color colorFondoEstado = ObtenerColorFondoEstadoRango(
                titulo,
                referenciaActiva,
                referenciaCliente
            );

            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = colorFondoEstado;
            card.Padding = new Padding(0);
            card.Margin = new Padding(14, 0, 14, 12);
            card.BorderStyle = BorderStyle.FixedSingle;
            card.MinimumSize = new Size(520, 205);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 8;
            layout.BackColor = colorFondoEstado;
            layout.Padding = new Padding(18, 10, 18, 10);

            layout.ColumnStyles.Clear();
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 5));       // franja
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // título
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // subtítulo
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));      // barra
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // min/est/holg
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // activo
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // cliente
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));     // leyenda / aire

            Panel franja = new Panel();
            franja.Dock = DockStyle.Fill;
            franja.BackColor = color;
            franja.Margin = new Padding(0, 0, 0, 8);

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 11.0f, FontStyle.Bold);
            lblTitulo.ForeColor = color;
            lblTitulo.Margin = new Padding(0, 8, 0, 0);

            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = subtitulo;
            lblSubtitulo.Dock = DockStyle.Fill;
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new Font("Segoe UI", 8.7f, FontStyle.Regular);
            lblSubtitulo.ForeColor = Color.FromArgb(95, 95, 95);
            lblSubtitulo.Margin = new Padding(0, 0, 0, 4);

            Panel barra = CrearBarraRangoVisualDobleReferencia(
                minimo,
                estandar,
                holgado,
                referenciaActiva,
                referenciaCliente,
                color
            );

            barra.Dock = DockStyle.Fill;
            barra.Margin = new Padding(0, 6, 0, 2);
            barra.BackColor = colorFondoEstado;

            TableLayoutPanel metricas = new TableLayoutPanel();
            metricas.Dock = DockStyle.Top;
            metricas.AutoSize = true;
            metricas.ColumnCount = 3;
            metricas.RowCount = 1;
            metricas.Margin = new Padding(0, 4, 0, 6);

            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            metricas.Controls.Add(
                CrearMiniMetrica("Mínimo", FormatearValorRango(minimo, unidad), Color.FromArgb(85, 85, 85)),
                0,
                0
            );

            metricas.Controls.Add(
                CrearMiniMetrica("Estándar", FormatearValorRango(estandar, unidad), Color.FromArgb(35, 35, 35)),
                1,
                0
            );

            metricas.Controls.Add(
                CrearMiniMetrica("Holgado", FormatearValorRango(holgado, unidad), Color.FromArgb(85, 85, 85)),
                2,
                0
            );

            Label lblActivo = new Label();
            lblActivo.Dock = DockStyle.Top;
            lblActivo.AutoSize = true;
            lblActivo.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            lblActivo.ForeColor = color;
            lblActivo.Margin = new Padding(0, 4, 0, 1);
            lblActivo.Text =
                "◆ " +
                etiquetaActiva +
                ": " +
                FormatearValorRango(referenciaActiva, unidad);

            Label lblCliente = new Label();
            lblCliente.Dock = DockStyle.Top;
            lblCliente.AutoSize = true;
            lblCliente.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            lblCliente.ForeColor = Color.FromArgb(55, 55, 55);
            lblCliente.Margin = new Padding(0, 1, 0, 1);

            if (referenciaCliente <= 0.0)
            {
                lblCliente.ForeColor = Color.FromArgb(160, 95, 20);
                lblCliente.Text = "◇ " + etiquetaCliente + ": no informado";
            }
            else
            {
                lblCliente.Text =
                    "◇ " +
                    etiquetaCliente +
                    ": " +
                    FormatearValorRango(referenciaCliente, unidad);
            }

            Label leyenda = new Label();
            leyenda.Dock = DockStyle.Top;
            leyenda.AutoSize = true;
            leyenda.Font = new Font("Segoe UI", 7.8f, FontStyle.Regular);
            leyenda.ForeColor = Color.FromArgb(115, 115, 115);
            leyenda.Margin = new Padding(0, 3, 0, 0);
            leyenda.Text = "● mín.   ▲ estándar   ■ holgado   ◆ activo   ◇ cliente";

            layout.Controls.Add(franja, 0, 0);
            layout.Controls.Add(lblTitulo, 0, 1);
            layout.Controls.Add(lblSubtitulo, 0, 2);
            layout.Controls.Add(barra, 0, 3);
            layout.Controls.Add(metricas, 0, 4);
            layout.Controls.Add(lblActivo, 0, 5);
            layout.Controls.Add(lblCliente, 0, 6);
            layout.Controls.Add(leyenda, 0, 7);

            card.Controls.Add(layout);

            return card;
        }

        private Panel CrearTarjetaRangoVacia(
            string tituloTexto = " ",
            string subtituloTexto = " ",
            Color? colorBase = null
        )
        {
            Color color = colorBase ?? Color.FromArgb(95, 95, 95);

            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.FromArgb(250, 250, 250);
            card.Padding = new Padding(0);
            card.Margin = new Padding(14, 0, 14, 12);
            card.BorderStyle = BorderStyle.FixedSingle;
            card.MinimumSize = new Size(520, 205);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 8;
            layout.BackColor = card.BackColor;
            layout.Padding = new Padding(18, 10, 18, 10);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 5));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            Panel franja = new Panel();
            franja.Dock = DockStyle.Fill;
            franja.BackColor = color;
            franja.Margin = new Padding(0, 0, 0, 8);

            Label titulo = new Label();
            titulo.Text = string.IsNullOrWhiteSpace(tituloTexto) ? " " : tituloTexto;
            titulo.Dock = DockStyle.Top;
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 11.0f, FontStyle.Bold);
            titulo.ForeColor = color;
            titulo.Margin = new Padding(0, 8, 0, 0);

            Label subtitulo = new Label();
            subtitulo.Text = string.IsNullOrWhiteSpace(subtituloTexto) ? " " : subtituloTexto;
            subtitulo.Dock = DockStyle.Top;
            subtitulo.AutoSize = true;
            subtitulo.Font = new Font("Segoe UI", 8.7f, FontStyle.Regular);
            subtitulo.ForeColor = Color.FromArgb(95, 95, 95);
            subtitulo.Margin = new Padding(0, 0, 0, 4);

            Panel barra = new Panel();
            barra.Dock = DockStyle.Fill;
            barra.BackColor = card.BackColor;
            barra.Margin = new Padding(0, 6, 0, 2);
            barra.Paint += (sender, e) =>
            {
                Rectangle r = barra.ClientRectangle;
                int y = Math.Max(12, r.Height / 2);
                using (Pen pen = new Pen(Color.FromArgb(215, 215, 215), 4))
                {
                    e.Graphics.DrawLine(pen, 22, y, Math.Max(24, r.Width - 22), y);
                }
            };

            TableLayoutPanel metricas = new TableLayoutPanel();
            metricas.Dock = DockStyle.Top;
            metricas.AutoSize = true;
            metricas.ColumnCount = 3;
            metricas.RowCount = 1;
            metricas.Margin = new Padding(0, 4, 0, 6);
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.Controls.Add(CrearMiniMetrica("Mínimo", "No definido", Color.FromArgb(125, 125, 125)), 0, 0);
            metricas.Controls.Add(CrearMiniMetrica("Estándar", "No definido", Color.FromArgb(125, 125, 125)), 1, 0);
            metricas.Controls.Add(CrearMiniMetrica("Holgado", "No definido", Color.FromArgb(125, 125, 125)), 2, 0);

            Label lblActivo = CrearLineaContextoRango("Escenario activo", "No definido");
            lblActivo.ForeColor = Color.FromArgb(135, 135, 135);

            Label lblCliente = CrearLineaContextoRango("Referencia cliente", "No definida");
            lblCliente.ForeColor = Color.FromArgb(135, 135, 135);

            Label leyenda = new Label();
            leyenda.Dock = DockStyle.Top;
            leyenda.AutoSize = true;
            leyenda.Font = new Font("Segoe UI", 7.8f, FontStyle.Regular);
            leyenda.ForeColor = Color.FromArgb(150, 150, 150);
            leyenda.Margin = new Padding(0, 3, 0, 0);
            leyenda.Text = "● mín.   ▲ estándar   ■ holgado   ◆ activo   ◇ cliente";

            layout.Controls.Add(franja, 0, 0);
            layout.Controls.Add(titulo, 0, 1);
            layout.Controls.Add(subtitulo, 0, 2);
            layout.Controls.Add(barra, 0, 3);
            layout.Controls.Add(metricas, 0, 4);
            layout.Controls.Add(lblActivo, 0, 5);
            layout.Controls.Add(lblCliente, 0, 6);
            layout.Controls.Add(leyenda, 0, 7);

            card.Controls.Add(layout);
            return card;
        }

        private Label CrearLineaContextoRango(string etiqueta, string valor)
        {
            Label label = new Label();
            label.Dock = DockStyle.Top;
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(70, 70, 70);
            label.Margin = new Padding(0, 4, 0, 2);
            label.Text = etiqueta + ": " + valor;
            return label;
        }

        private Control CrearPanelResumenVisualVacio()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Top;
            root.AutoSize = true;
            root.ColumnCount = 1;
            root.RowCount = 2;
            root.BackColor = Color.FromArgb(248, 249, 251);
            root.Padding = new Padding(0);
            root.Margin = new Padding(0);

            Label titulo = CrearLabelSimple("Resumen visual de rango productivo y comercial", 13, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(30, 30, 30);
            titulo.Margin = new Padding(0, 0, 0, 10);
            root.Controls.Add(titulo, 0, 0);

            TableLayoutPanel cards = new TableLayoutPanel();
            cards.Dock = DockStyle.Top;
            cards.AutoSize = false;
            cards.Height = 590;
            cards.ColumnCount = 2;
            cards.RowCount = 2;
            cards.Margin = new Padding(0, 0, 0, 16);

            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            cards.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            cards.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));

            cards.Controls.Add(
                CrearTarjetaRangoVacia("Plazo técnico", "Semanas", Color.FromArgb(83, 192, 166)),
                0,
                0
            );
            cards.Controls.Add(
                CrearTarjetaRangoVacia("Costo interno", ObtenerMonedaVisualActual(), Color.FromArgb(255, 193, 7)),
                1,
                0
            );
            cards.Controls.Add(
                CrearTarjetaRangoVacia("Precio ofertable", ObtenerMonedaVisualActual(), Color.FromArgb(238, 30, 91)),
                0,
                1
            );
            cards.Controls.Add(CrearTarjetaRangoVacia(), 1, 1);

            root.Controls.Add(cards, 0, 1);
            return root;
        }

        private Color ObtenerColorFondoEstadoRango(
    string titulo,
    double valorActivo,
    double valorCliente
)
        {
            string t = (titulo ?? "").Trim().ToLowerInvariant();

            bool esPlazo =
                t.Contains("plazo") ||
                t.Contains("semana");

            bool esPrecio =
                t.Contains("precio") ||
                t.Contains("ofertable");

            bool esCosto =
                t.Contains("costo");

            if (esCosto)
            {
                // El costo interno no se compara directo contra cliente acá.
                return Color.FromArgb(255, 252, 238);
            }

            if (valorCliente <= 0.0)
            {
                // Sin referencia cliente: advertencia suave.
                return Color.FromArgb(255, 250, 235);
            }

            if (esPlazo)
            {
                // Bueno: el plazo técnico cabe dentro del plazo cliente.
                if (valorActivo <= valorCliente)
                {
                    return Color.FromArgb(238, 250, 242); // verde suave
                }

                return Color.FromArgb(255, 241, 241); // rojo suave
            }

            if (esPrecio)
            {
                // Bueno: el precio activo cabe dentro del presupuesto cliente.
                if (valorActivo <= valorCliente)
                {
                    return Color.FromArgb(238, 250, 242); // verde suave
                }

                return Color.FromArgb(255, 241, 241); // rojo suave
            }

            return Color.White;
        }
        

        private void BtnAplicarPropuestaEtapasDesglose_Click(object sender, EventArgs e)
        {
            AplicarPropuestaDesgloseAEtapasExistentes(true);
        }

        private bool AplicarEstandarDesgloseYPropuestaAEtapasDesdeDatos()
        {
            if (cotizacion == null)
            {
                return false;
            }

            AplicarEscenarioActivoDesglose(EscenarioPlanificacionDesglose.Estandar);
            return AplicarPropuestaDesgloseAEtapasExistentes(false);
        }

        private bool AplicarPropuestaDesgloseAEtapasExistentes(bool mostrarMensajes)
        {
            if (cotizacion == null)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show(
                        "No hay cotización activa.",
                        "Aplicar propuesta",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                return false;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show(
                        "No hay desglose productivo generado para aplicar a etapas.",
                        "Aplicar propuesta",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                return false;
            }

            EscenarioPlanificacionDesglose escenarioReal = escenarioActivoDesglose;

            if (escenarioReal == EscenarioPlanificacionDesglose.Recomendado)
            {
                ResultadoEscenarioOfertaDesglose oferta =
                    EscenarioOfertaDesdeDesgloseService.Calcular(cotizacion);

                escenarioReal = oferta.EscenarioRecomendado;
                escenarioActivoDesglose = escenarioReal;
            }

            /*
             * NUEVA LÓGICA:
             *
             * El producto ya produjo requerimientos internos reales.
             * Por eso la propuesta no debe depender de que exista una
             * subetapa genérica compatible en la biblioteca.
             *
             * 1. Convertimos cada requerimiento interno activo en subproceso.
             * 2. Calculamos duración desde mínimo / estándar / holgado.
             * 3. Ordenamos por etapa productiva y tipo interno.
             * 4. Actualizamos etapas para que el Gantt use fechas reales.
             */

            int subprocesosAplicados =
                ReconstruirSubprocesosProyectoDesdeDesglose(escenarioReal);

            if (subprocesosAplicados == 0)
            {
                if (mostrarMensajes)
                {
                    MessageBox.Show(
                        "El desglose existe, pero no tiene requerimientos internos con duración para construir la Gantt.",
                        "Aplicar propuesta",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                return false;
            }

            RefrescarEstadoBotonesEscenarioDesglose();
            RefrescarResumenDesgloseProductivo();
            RefrescarTablaSubEtapasSiExiste();
            CargarRangosSubEtapasEnPantalla();
            RefrescarDespuesDeEditarEtapas();

            if (mostrarMensajes)
            {
                MessageBox.Show(
                    "Propuesta aplicada desde entregables internos usando el escenario " +
                    ObtenerNombreEscenarioVisible(escenarioReal) +
                    ". Subprocesos generados: " +
                    subprocesosAplicados.ToString() +
                    ".",
                    "Aplicar propuesta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            return true;
        }

        private void ConfigurarBotonEditarRendimientosDesglose()
        {
            btnEditarRendimientosDesglose.Text = "Editar rendimientos";
            btnEditarRendimientosDesglose.Width = 170;
            btnEditarRendimientosDesglose.Height = 36;
            btnEditarRendimientosDesglose.Margin = new Padding(0, 0, 8, 8);
            btnEditarRendimientosDesglose.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            btnEditarRendimientosDesglose.BackColor = Color.FromArgb(245, 245, 245);
            btnEditarRendimientosDesglose.ForeColor = Color.Black;
            btnEditarRendimientosDesglose.FlatStyle = FlatStyle.Flat;
            btnEditarRendimientosDesglose.UseVisualStyleBackColor = false;

            btnEditarRendimientosDesglose.Click -= BtnEditarRendimientosDesglose_Click;
            btnEditarRendimientosDesglose.Click += BtnEditarRendimientosDesglose_Click;
        }

        private void BtnEditarRendimientosDesglose_Click(object sender, EventArgs e)
        {
            SeleccionarTabRendimientosProductivos();
        }

        private int ReconstruirSubprocesosProyectoDesdeDesglose(
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (cotizacion == null ||
                cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null)
            {
                return 0;
            }

            List<RequerimientoProduccionInterna> requerimientos = cotizacion
                .DesgloseProductivo
                .Requerimientos
                .Where(r => r != null)
                .Where(r => ObtenerDiasReqEscenario(r, escenario) > 0.0)
                .Select(r =>
                {
                    AsegurarPlanificacionRequerimiento(r);
                    return r;
                })
                .OrderBy(r => ObtenerOrdenEtapaDesdeDesgloseLocal(r.EtapaSugerida))
                .ThenBy(r => r.BloqueProductivo)
                .ThenBy(r => ObtenerOrdenTipoInternoDesglose(r.TipoInterno))
                .ThenBy(r => r.NombreRequerimiento)
                .ToList();

            if (requerimientos.Count == 0)
            {
                return 0;
            }

            if (bibliotecaSubEtapas == null)
            {
                bibliotecaSubEtapas = new List<SubEtapaProyecto>();
            }

            bibliotecaSubEtapas.Clear();
            AsegurarEtapasBaseInternas();

            int id = 1;
            double semanaCursor = 0.0;
            Dictionary<string, double> finPorRequerimiento =
                new Dictionary<string, double>();
            Dictionary<string, double> finPorEtapaProducto =
                new Dictionary<string, double>();
            List<ProductoEtapaDefinicion> pipelineProducto =
                ObtenerPipelineProductoActualParaPlanificacion();

            var gruposEtapa = requerimientos
                .GroupBy(r => NormalizarEtapaProductivaInterna(r.EtapaSugerida))
                .OrderBy(g => ObtenerOrdenEtapaDesdeDesgloseLocal(g.Key))
                .ToList();

            foreach (var grupoEtapa in gruposEtapa)
            {
                string nombreEtapa = NormalizarEtapaProductivaInterna(grupoEtapa.Key);
                EtapaProyecto etapa = ObtenerOCrearEtapaInterna(nombreEtapa);
                ProductoEtapaDefinicion etapaProducto =
                    ObtenerEtapaProductoPlanificacion(pipelineProducto, nombreEtapa);

                double inicioEtapaSemana = etapaProducto == null
                    ? semanaCursor
                    : ObtenerInicioEtapaProductoPlanificacion(
                        etapaProducto,
                        finPorEtapaProducto
                    );
                double sumaMin = grupoEtapa.Sum(r => Math.Max(0.0, r.DiasPersonaMin));
                double sumaStd = grupoEtapa.Sum(r => Math.Max(0.0, r.DiasPersonaStd));
                double sumaHolg = grupoEtapa.Sum(r => Math.Max(0.0, r.DiasPersonaHolgura));
                int orden = 1;

                var gruposBloque = grupoEtapa
                    .GroupBy(r => NormalizarClavePlanificacion(r.BloqueProductivo))
                    .OrderBy(g => g.Min(r => ObtenerOrdenTipoInternoDesglose(r.TipoInterno)))
                    .ThenBy(g => g.Key)
                    .ToList();

                foreach (var grupoBloque in gruposBloque)
                {
                    double inicioBloqueSemana = semanaCursor;
                    double cursorBloque = inicioBloqueSemana;
                    double finBloqueSemana = inicioBloqueSemana;
                    int indiceParaleloBloque = 0;
                    double desfaseParaleloSemanas = ObtenerDesfaseParaleloSemanas();

                    List<RequerimientoProduccionInterna> requerimientosBloque = grupoBloque.ToList();

                    foreach (RequerimientoProduccionInterna req in requerimientosBloque
                        .OrderBy(r => CalcularNivelDependenciaRequerimientoPlanificacion(
                            r,
                            requerimientosBloque,
                            new Dictionary<string, int>(),
                            new HashSet<string>()
                        ))
                        .ThenBy(r => ObtenerOrdenTipoInternoDesglose(r.TipoInterno))
                        .ThenBy(r => r.NombreRequerimiento))
                    {
                        double diasEscenario = ObtenerDiasReqEscenario(req, escenario);
                        double duracionSemanas = ConvertirDiasPersonaASemanasCalendario(diasEscenario);
                        bool paralelo = EsModoPlanificacionParalelo(req.ModoPlanificacion);

                        double inicioReqSemana = paralelo
                            ? inicioBloqueSemana + (indiceParaleloBloque * desfaseParaleloSemanas)
                            : cursorBloque;

                        double finDependencia = ObtenerFinDependenciaPlanificacion(
                            req,
                            finPorRequerimiento
                        );

                        if (finDependencia > inicioReqSemana)
                        {
                            inicioReqSemana = finDependencia;
                        }

                        SubEtapaProyecto sub = new SubEtapaProyecto
                        {
                            Id = id++,
                            EtapaPadre = nombreEtapa,
                            Nombre = ConstruirNombreSubprocesoDesdeRequerimiento(req),
                            Orden = orden++,
                            Activa = true,
                            Requerida = true,
                            Editable = true,
                            Requiere = req.EntregableCliente,
                            Entrega = req.NombreRequerimiento,
                            CargosSugeridos = req.CargoSugerido,
                            PorcentajeMinimoEtapa = CalcularProporcionSegura(req.DiasPersonaMin, sumaMin),
                            PorcentajeRecomendadoEtapa = CalcularProporcionSegura(req.DiasPersonaStd, sumaStd),
                            PorcentajeMaximoEtapa = CalcularProporcionSegura(req.DiasPersonaHolgura, sumaHolg),
                            InicioSemana = inicioReqSemana,
                            DuracionSemanas = duracionSemanas,
                            TiposInternosQueProduce = req.TipoInterno,
                            TiposInternosQueConsume = req.EntregableCliente,
                            PalabrasClaveActivacion =
                                req.TipoInterno + "; " +
                                req.NombreRequerimiento + "; " +
                                req.CategoriaEntregable + "; " +
                                req.BloqueProductivo + "; " +
                                req.ModoPlanificacion
                        };

                        OrdenarPonderadoresSubEtapa(sub);
                        bibliotecaSubEtapas.Add(sub);

                        double finReqSemana = sub.FinSemana;
                        finBloqueSemana = Math.Max(finBloqueSemana, finReqSemana);

                        if (!paralelo)
                        {
                            cursorBloque = Math.Max(cursorBloque, finReqSemana);
                        }
                        else
                        {
                            indiceParaleloBloque++;
                        }

                        RegistrarFinRequerimientoPlanificacion(
                            finPorRequerimiento,
                            req,
                            sub.Nombre,
                            finReqSemana
                        );
                    }

                    semanaCursor = Math.Max(semanaCursor, finBloqueSemana);
                }

                double duracionEtapaSemanas = Math.Max(0.1, semanaCursor - inicioEtapaSemana);

                etapa.Seleccionada = true;
                etapa.InicioMes = inicioEtapaSemana / GanttGrandeSemanasPorMes;
                etapa.DuracionMeses = duracionEtapaSemanas / GanttGrandeSemanasPorMes;
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                etapa.CostoTotal = grupoEtapa.Sum(r => ObtenerCostoReqEscenario(r, escenario));

                RegistrarFinEtapaProductoPlanificacion(
                    finPorEtapaProducto,
                    etapaProducto,
                    nombreEtapa,
                    etapa.FinMes * GanttGrandeSemanasPorMes
                );

                semanaCursor = Math.Max(semanaCursor, etapa.FinMes * GanttGrandeSemanasPorMes);

                string clave = NormalizarNombreEtapa(nombreEtapa);

                if (!string.IsNullOrWhiteSpace(clave))
                {
                    etapasExpandidasEnTabla.Add(clave);
                }
            }

            DesactivarEtapasSinSubprocesosDeDesglose();
            RecalcularTotalesDesgloseProductivoDesdeFilas();
            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();
            RefrescarGanttGrandeEtapas();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            return bibliotecaSubEtapas.Count(s => s != null && s.Activa);
        }

        private void DesactivarEtapasSinSubprocesosDeDesglose()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                bool tieneSubprocesos = bibliotecaSubEtapas != null &&
                    bibliotecaSubEtapas.Any(s =>
                        s != null &&
                        s.Activa &&
                        NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre));

                if (tieneSubprocesos)
                {
                    continue;
                }

                etapa.Seleccionada = false;
                etapa.InicioMes = 0.0;
                etapa.DuracionMeses = 0.0;
                etapa.FinMes = 0.0;
                etapa.CostoTotal = 0.0;
            }
        }

        private double ConvertirDiasPersonaASemanasCalendario(double diasPersona)
        {
            if (diasPersona <= 0.0)
            {
                return 0.1;
            }

            double diasHabilesPorSemana = 5.0;
            double personasPlanificadas = ObtenerPersonasPlanificadasDesdeManoObra();

            if (cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0)
            {
                diasHabilesPorSemana = cotizacion.DiasHabilesEstudioPorSemana;
            }

            double semanas = diasPersona / (diasHabilesPorSemana * personasPlanificadas);

            if (semanas < 0.1)
            {
                semanas = 0.1;
            }

            return semanas;
        }

        private double ObtenerDesfaseParaleloSemanas()
        {
            double diasHabilesPorSemana = 5.0;

            if (cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0)
            {
                diasHabilesPorSemana = cotizacion.DiasHabilesEstudioPorSemana;
            }

            return 3.0 / diasHabilesPorSemana;
        }

        private double ObtenerPersonasPlanificadasDesdeManoObra()
        {
            const double respaldo = 2.0;

            if (cotizacion == null ||
                cotizacion.PlanGeneralManoObra == null ||
                cotizacion.PlanGeneralManoObra.Count == 0)
            {
                return respaldo;
            }

            int meses = cotizacion.PlanGeneralManoObra
                .Where(c => c != null && c.PersonasPorBloque != null)
                .Select(c => c.PersonasPorBloque.Count)
                .DefaultIfEmpty(0)
                .Max();

            if (meses <= 0)
            {
                return respaldo;
            }

            double maximoPersonasMes = 0.0;

            for (int i = 0; i < meses; i++)
            {
                double personasMes = cotizacion.PlanGeneralManoObra
                    .Where(c => c != null && c.PersonasPorBloque != null && i < c.PersonasPorBloque.Count)
                    .Sum(c => Math.Max(0.0, c.PersonasPorBloque[i]));

                maximoPersonasMes = Math.Max(maximoPersonasMes, personasMes);
            }

            if (maximoPersonasMes <= 0.0)
            {
                return respaldo;
            }

            return Math.Max(1.0, maximoPersonasMes);
        }

        private double CalcularProporcionSegura(double valor, double total)
        {
            if (valor <= 0.0 || total <= 0.0)
            {
                return 0.0;
            }

            return valor / total;
        }

        private string ConstruirNombreSubprocesoDesdeRequerimiento(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null)
            {
                return "Subproceso interno";
            }

            if (!string.IsNullOrWhiteSpace(req.NombreRequerimiento))
            {
                return req.NombreRequerimiento.Trim();
            }

            if (!string.IsNullOrWhiteSpace(req.TipoInterno))
            {
                return req.TipoInterno.Trim();
            }

            if (!string.IsNullOrWhiteSpace(req.EntregableCliente))
            {
                return req.EntregableCliente.Trim();
            }

            return "Subproceso interno";
        }

        private void AplicarPropuestaVinculacionSobreSubEtapasExistentes(
    PropuestaVinculacionDesglose propuesta
)
        {
            if (propuesta == null ||
                propuesta.Etapas == null ||
                bibliotecaSubEtapas == null)
            {
                return;
            }

            /*
             * Al aplicar una propuesta desde el desglose, primero limpiamos
             * el estado activo/requerido de las subetapas.
             *
             * No destruimos la biblioteca. Solo reconstruimos qué subetapas
             * son necesarias para este proyecto.
             */

            foreach (SubEtapaProyecto sub in bibliotecaSubEtapas)
            {
                if (sub == null)
                {
                    continue;
                }

                sub.Activa = false;
                sub.Requerida = false;
            }

            foreach (PropuestaEtapaDesglose etapaPropuesta in propuesta.Etapas
                .OrderBy(e => ObtenerOrdenEtapaDesdeDesgloseLocal(e.Etapa)))
            {
                if (etapaPropuesta == null || etapaPropuesta.SubEtapas == null)
                {
                    continue;
                }

                /*
                 * Semana 0 representa el origen del proyecto.
                 * No usamos int.
                 * No usamos Math.Ceiling.
                 */
                double semanaCursor = 0.0;

                foreach (PropuestaSubEtapaDesglose subPropuesta in etapaPropuesta.SubEtapas)
                {
                    if (subPropuesta == null)
                    {
                        continue;
                    }

                    SubEtapaProyecto subReal = bibliotecaSubEtapas.FirstOrDefault(s =>
                        s != null &&
                        NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(subPropuesta.EtapaPadre) &&
                        NormalizarNombreEtapa(s.Nombre) == NormalizarNombreEtapa(subPropuesta.NombreSubEtapa)
                    );

                    if (subReal == null)
                    {
                        continue;
                    }

                    subReal.Activa = true;
                    subReal.Requerida = true;

                    subReal.InicioSemana = semanaCursor;

                    double duracionSemanas = subPropuesta.SemanasSugeridas;

                    if (duracionSemanas <= 0.0)
                    {
                        duracionSemanas = 0.1;
                    }

                    subReal.DuracionSemanas = duracionSemanas;

                    semanaCursor += duracionSemanas;
                }
            }

            /*
             * Primera pasada de consistencia:
             * corrige secuencias obvias y gates productivos.
             */
            CorregirDependenciasTemporalesBasicasDesglose();
        }

        private void AplicarTensorTemporalSegunEscenario(
    EscenarioPlanificacionDesglose escenario
)
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                return;
            }

            double plazoBaseSemanas = ObtenerPlazoTecnicoBaseDesdeSubEtapasActivas();

            if (plazoBaseSemanas <= 0.0)
            {
                return;
            }

            double plazoObjetivoSemanas =
                ObtenerPlazoObjetivoSemanasParaEscenario(escenario, plazoBaseSemanas);

            if (plazoObjetivoSemanas <= 0.0)
            {
                plazoObjetivoSemanas = plazoBaseSemanas;
            }

            /*
             * Nunca comprimimos por debajo del mínimo técnico base.
             * Si el cliente pide menos tiempo, el sistema debe marcar presión,
             * no destruir la secuencia productiva.
             */
            if (plazoObjetivoSemanas < plazoBaseSemanas)
            {
                plazoObjetivoSemanas = plazoBaseSemanas;
            }

            double factorTemporal = CalcularFactorTemporalDesglose(
                plazoObjetivoSemanas,
                plazoBaseSemanas
            );

            EscalarSubEtapasActivasPorTensorTemporal(factorTemporal);

            /*
             * Segunda pasada:
             * después de estirar el calendario, volvemos a corregir dependencias.
             */
            CorregirDependenciasTemporalesBasicasDesglose();
        }

        private double CalcularFactorTemporalDesglose(
            double plazoObjetivoSemanas,
            double plazoBaseSemanas
        )
        {
            if (plazoBaseSemanas <= 0.0)
            {
                return 1.0;
            }

            double factor = plazoObjetivoSemanas / plazoBaseSemanas;

            /*
             * Para beta:
             * - No comprimimos agresivamente.
             * - Sí dejamos estirar bastante.
             */
            if (factor < 1.0)
            {
                factor = 1.0;
            }

            if (factor > 4.0)
            {
                factor = 4.0;
            }

            return factor;
        }

        private void EscalarSubEtapasActivasPorTensorTemporal(double factorTemporal)
        {
            if (bibliotecaSubEtapas == null)
            {
                return;
            }

            foreach (SubEtapaProyecto sub in bibliotecaSubEtapas)
            {
                if (sub == null || !sub.Activa)
                {
                    continue;
                }

                sub.InicioSemana = sub.InicioSemana * factorTemporal;
                sub.DuracionSemanas = sub.DuracionSemanas * factorTemporal;

                if (sub.InicioSemana < 0.0)
                {
                    sub.InicioSemana = 0.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }
            }
        }

        private double ObtenerPlazoTecnicoBaseDesdeSubEtapasActivas()
        {
            if (bibliotecaSubEtapas == null)
            {
                return 0.0;
            }

            return bibliotecaSubEtapas
                .Where(s => s != null && s.Activa)
                .Select(s => s.FinSemana)
                .DefaultIfEmpty(0.0)
                .Max();
        }

        private double ObtenerPlazoObjetivoSemanasParaEscenario(
            EscenarioPlanificacionDesglose escenario,
            double plazoBaseSemanas
        )
        {
            if (cotizacion == null || cotizacion.DesgloseProductivo == null)
            {
                return plazoBaseSemanas;
            }

            DesgloseProductivoProyecto d = cotizacion.DesgloseProductivo;

            double objetivo = plazoBaseSemanas;

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                objetivo = Math.Max(plazoBaseSemanas, d.SemanasMinimas);
            }
            else if (escenario == EscenarioPlanificacionDesglose.Estandar)
            {
                objetivo = Math.Max(plazoBaseSemanas, d.SemanasEstandar);
            }
            else if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                objetivo = Math.Max(plazoBaseSemanas, d.SemanasHolgura);
            }
            else if (escenario == EscenarioPlanificacionDesglose.Recomendado)
            {
                double plazoCliente = ObtenerPlazoClienteSemanasDesdeDatos();

                if (plazoCliente > 0.0)
                {
                    objetivo = Math.Max(plazoBaseSemanas, plazoCliente);
                }
                else
                {
                    objetivo = Math.Max(plazoBaseSemanas, d.SemanasEstandar);
                }
            }

            if (objetivo <= 0.0)
            {
                objetivo = plazoBaseSemanas;
            }

            return objetivo;
        }

        private double ObtenerPlazoClienteSemanasDesdeDatos()
        {
            if (cotizacion == null)
            {
                return 0.0;
            }

            /*
             * Preferimos leer desde fechas de la pantalla si están disponibles,
             * porque ahí está el plazo declarado por el cliente.
             */
            if (dtpFechaInicioCliente != null &&
                dtpFechaEntregaCliente != null &&
                dtpFechaInicioCliente.Checked &&
                dtpFechaEntregaCliente.Checked)
            {
                DateTime inicio = dtpFechaInicioCliente.Value.Date;
                DateTime entrega = dtpFechaEntregaCliente.Value.Date;

                double dias = (entrega - inicio).TotalDays;

                if (dias > 0.0)
                {
                    return dias / 7.0;
                }
            }

            /*
             * Fallback por si luego guardas el plazo directamente
             * dentro de cotizacion.
             */
            double semanas = 0.0;

            try
            {
                semanas = Convert.ToDouble(
                    cotizacion.GetType()
                        .GetProperty("PlazoClienteSemanas")
                        ?.GetValue(cotizacion, null)
                );
            }
            catch
            {
                semanas = 0.0;
            }

            if (semanas > 0.0)
            {
                return semanas;
            }

            return 0.0;
        }

        private void CorregirDependenciasTemporalesBasicasDesglose()
        {
            if (bibliotecaSubEtapas == null)
            {
                return;
            }

            /*
             * Primero saneamos valores.
             */
            foreach (SubEtapaProyecto sub in bibliotecaSubEtapas)
            {
                if (sub == null || !sub.Activa)
                {
                    continue;
                }

                if (sub.InicioSemana < 0.0)
                {
                    sub.InicioSemana = 0.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }
            }

            /*
             * Varias pasadas porque mover una subetapa puede afectar
             * a otra que depende de ella.
             */
            for (int i = 0; i < 8; i++)
            {
                AplicarDependenciaTemporalBasica(
                    "Preproduccion",
                    "Storyboard",
                    "Desarrollo",
                    "Guion base",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Preproduccion",
                    "Animatic",
                    "Preproduccion",
                    "Storyboard",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Layout",
                    "Preproduccion",
                    "Storyboard",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Animación",
                    "Preproduccion",
                    "Storyboard",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Animación",
                    "Preproduccion",
                    "Diseño de personajes",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Backgrounds",
                    "Preproduccion",
                    "Diseño de fondos",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Compositing",
                    "Produccion",
                    "Animación",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Produccion",
                    "Compositing",
                    "Produccion",
                    "Backgrounds",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Postproduccion",
                    "Edición final",
                    "Produccion",
                    "Compositing",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Postproduccion",
                    "Correcciones finales",
                    "Postproduccion",
                    "Edición final",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Postproduccion",
                    "Entrega final",
                    "Postproduccion",
                    "Edición final",
                    0.0
                );

                AplicarDependenciaTemporalBasica(
                    "Postproduccion",
                    "Entrega final",
                    "Postproduccion",
                    "Correcciones finales",
                    0.0
                );
            }
        }

        private void AplicarDependenciaTemporalBasica(
            string etapaObjetivo,
            string subObjetivo,
            string etapaRequisito,
            string subRequisito,
            double holguraSemanas
        )
        {
            SubEtapaProyecto objetivo = BuscarSubEtapaActivaPorNombreFlexible(
                etapaObjetivo,
                subObjetivo
            );

            SubEtapaProyecto requisito = BuscarSubEtapaActivaPorNombreFlexible(
                etapaRequisito,
                subRequisito
            );

            if (objetivo == null || requisito == null)
            {
                return;
            }

            double inicioMinimo = requisito.FinSemana + holguraSemanas;

            if (inicioMinimo < 0.0)
            {
                inicioMinimo = 0.0;
            }

            if (objetivo.InicioSemana < inicioMinimo)
            {
                objetivo.InicioSemana = inicioMinimo;
            }

            if (objetivo.DuracionSemanas <= 0.0)
            {
                objetivo.DuracionSemanas = 0.1;
            }
        }

        private SubEtapaProyecto BuscarSubEtapaActivaPorNombreFlexible(
            string etapa,
            string nombreSubEtapa
        )
        {
            if (bibliotecaSubEtapas == null)
            {
                return null;
            }

            string etapaNorm = NormalizarNombreEtapa(etapa);
            string subNorm = NormalizarNombreEtapa(nombreSubEtapa);

            return bibliotecaSubEtapas.FirstOrDefault(s =>
                s != null &&
                s.Activa &&
                NormalizarNombreEtapa(s.EtapaPadre) == etapaNorm &&
                (
                    NormalizarNombreEtapa(s.Nombre) == subNorm ||
                    NormalizarNombreEtapa(s.Nombre).Contains(subNorm) ||
                    subNorm.Contains(NormalizarNombreEtapa(s.Nombre))
                )
            );
        }

        private int ObtenerOrdenEtapaDesdeDesgloseLocal(string nombreEtapa)
        {
            return ObtenerOrdenEtapaGeneral(nombreEtapa);
        }

        private void ConfigurarBotonEscenario(
            Button boton,
            string texto,
            EscenarioPlanificacionDesglose escenario
        )
        {
            boton.Text = texto;
            boton.Width = escenario == EscenarioPlanificacionDesglose.Recomendado ? 260 : 145;
            boton.Height = 36;
            boton.Margin = new Padding(0, 0, 8, 8);
            boton.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            boton.Tag = escenario;

            boton.Click -= BtnEscenarioDesglose_Click;
            boton.Click += BtnEscenarioDesglose_Click;
        }

        private void BtnEscenarioDesglose_Click(object sender, EventArgs e)
        {
            Button boton = sender as Button;

            if (boton == null || boton.Tag == null)
            {
                return;
            }

            EscenarioPlanificacionDesglose escenarioSolicitado =
                (EscenarioPlanificacionDesglose)boton.Tag;

            AplicarEscenarioActivoDesglose(escenarioSolicitado);
        }

        private void AplicarEscenarioActivoDesglose(
            EscenarioPlanificacionDesglose escenarioSolicitado
        )
        {
            if (cotizacion == null)
            {
                return;
            }

            if (escenarioSolicitado == EscenarioPlanificacionDesglose.Recomendado)
            {
                ResultadoEscenarioOfertaDesglose oferta =
                    EscenarioOfertaDesdeDesgloseService.Calcular(cotizacion);

                escenarioActivoDesglose = oferta.EscenarioRecomendado;
            }
            else
            {
                escenarioActivoDesglose = escenarioSolicitado;
            }

            RefrescarEstadoBotonesEscenarioDesglose();
            RefrescarResumenDesgloseProductivo();
            RefrescarDespuesDeEditarDesgloseProductivo();
        }

        private void RefrescarEstadoBotonesEscenarioDesglose()
        {
            PintarBotonEscenario(btnEscenarioMinimoDesglose, EscenarioPlanificacionDesglose.Minimo);
            PintarBotonEscenario(btnEscenarioEstandarDesglose, EscenarioPlanificacionDesglose.Estandar);
            PintarBotonEscenario(btnEscenarioHolgadoDesglose, EscenarioPlanificacionDesglose.Holgado);

            btnEscenarioRecomendadoDesglose.BackColor = Color.FromArgb(245, 245, 245);
            btnEscenarioRecomendadoDesglose.ForeColor = Color.Black;
        }

        private void PintarBotonEscenario(Button boton, EscenarioPlanificacionDesglose escenario)
        {
            if (boton == null)
            {
                return;
            }

            if (escenarioActivoDesglose == escenario)
            {
                boton.BackColor = Color.FromArgb(83, 192, 166);
                boton.ForeColor = Color.Black;
            }
            else
            {
                boton.BackColor = Color.FromArgb(245, 245, 245);
                boton.ForeColor = Color.Black;
            }
        }
        private void ConfigurarResumenDesgloseProductivo()
        {
            panelResumenVisualDesglose.Dock = DockStyle.Fill;
            panelResumenVisualDesglose.AutoScroll = true;
            panelResumenVisualDesglose.BackColor = Color.FromArgb(248, 249, 251);
            panelResumenVisualDesglose.BorderStyle = BorderStyle.FixedSingle;
            panelResumenVisualDesglose.Padding = new Padding(14);
            panelResumenVisualDesglose.Visible = true;

            // Se deja creado por compatibilidad, pero ya no se usa como resumen visual.
            rtbResumenDesgloseProductivo.Visible = false;
        }

        private void DgvDesgloseProductivo_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
        }


        private void ConfigurarTemporizadorRecalculoDesgloseProductivo()
        {
            temporizadorRecalculoDesgloseProductivo.Stop();
            temporizadorRecalculoDesgloseProductivo.Interval = 250;

            temporizadorRecalculoDesgloseProductivo.Tick -= TemporizadorRecalculoDesgloseProductivo_Tick;
            temporizadorRecalculoDesgloseProductivo.Tick += TemporizadorRecalculoDesgloseProductivo_Tick;
        }

        private void TemporizadorRecalculoDesgloseProductivo_Tick(object sender, EventArgs e)
        {
            temporizadorRecalculoDesgloseProductivo.Stop();

            if (cargandoDesgloseProductivo || aplicandoCambiosDesgloseProductivo)
            {
                return;
            }

            AplicarCambiosDesgloseProductivoEnVivo();
        }

        private void ProgramarRecalculoDesgloseProductivo()
        {
            if (cargandoDesgloseProductivo || aplicandoCambiosDesgloseProductivo)
            {
                return;
            }

            if (temporizadorRecalculoDesgloseProductivo == null)
            {
                return;
            }

            temporizadorRecalculoDesgloseProductivo.Stop();
            temporizadorRecalculoDesgloseProductivo.Start();
        }

        private void DgvDesgloseProductivo_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string nombreColumna = dgvDesgloseProductivo.Columns[e.ColumnIndex].Name;

            if (!EsColumnaEditableDesglose(nombreColumna))
            {
                return;
            }

            ProgramarRecalculoDesgloseProductivo();
        }

        private void DgvDesgloseProductivo_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string nombreColumna = dgvDesgloseProductivo.Columns[e.ColumnIndex].Name;

            if (!EsColumnaEditableDesglose(nombreColumna))
            {
                return;
            }

            ProgramarRecalculoDesgloseProductivo();
        }

        private void DgvDesgloseProductivo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Intencionalmente vacío.
            // No recalcular aquí porque corta la escritura de números con más de un dígito.
        }

        private bool EsColumnaEditableDesglose(string nombreColumna)
        {
            if (string.IsNullOrWhiteSpace(nombreColumna))
            {
                return false;
            }

            return nombreColumna == "Cantidad" ||
                   nombreColumna == "Calidad" ||
                   nombreColumna == "CargoSugerido" ||
                   nombreColumna == "RendimientoCantidad" ||
                   nombreColumna == "RendimientoPeriodo" ||
                   nombreColumna == "ModoCalculoProductivo" ||
                   nombreColumna == "HorasMinimas" ||
                   nombreColumna == "HorasEstandar" ||
                   nombreColumna == "HorasHolgura" ||
                   nombreColumna == "DiasPersonaMin" ||
                   nombreColumna == "DiasPersonaStd" ||
                   nombreColumna == "DiasPersonaHolgura";
        }

        private void DgvDesgloseProductivo_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvDesgloseProductivo == null)
            {
                return;
            }

            if (!dgvDesgloseProductivo.IsCurrentCellDirty)
            {
                return;
            }

            if (dgvDesgloseProductivo.CurrentCell is DataGridViewComboBoxCell)
            {
                dgvDesgloseProductivo.CommitEdit(DataGridViewDataErrorContexts.Commit);
                ProgramarRecalculoDesgloseProductivo();
            }
        }

        private void DgvDesgloseProductivo_SelectionChanged(object sender, EventArgs e)
        {
            RefrescarEditorRendimientoDesglose();
        }

        private void DgvDesgloseProductivo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvDesgloseProductivo == null)
            {
                return;
            }

            DataGridViewRow row = dgvDesgloseProductivo.Rows[e.RowIndex];
            RequerimientoProduccionInterna req = row.Tag as RequerimientoProduccionInterna;

            if (req == null)
            {
                return;
            }

            MostrarDetalleCalculoDesgloseProductivo(req);
        }

        private void MostrarDetalleCalculoDesgloseProductivo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return;
            }

            DetalleCalculoProductivoContexto detalle = CrearContextoDetalleCalculoDesglose(req);

            using (Form dialogo = new Form())
            {
                dialogo.Text = "Detalle de cálculo productivo - " + TextoSeguro(NombreFilaDetalleCalculo(req));
                dialogo.StartPosition = FormStartPosition.CenterParent;
                dialogo.Size = new Size(980, 680);
                dialogo.MinimumSize = new Size(860, 560);
                dialogo.BackColor = Color.White;
                dialogo.Padding = new Padding(14);

                TableLayoutPanel root = new TableLayoutPanel();
                root.Dock = DockStyle.Fill;
                root.ColumnCount = 1;
                root.RowCount = 3;
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.BackColor = Color.White;

                Label titulo = new Label();
                titulo.Text = "Radiografía del cálculo";
                titulo.AutoSize = true;
                titulo.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
                titulo.ForeColor = Color.FromArgb(20, 20, 20);
                titulo.Margin = new Padding(0, 0, 0, 4);

                Label bajada = new Label();
                bajada.Text =
                    "Doble click en una fila del desglose abre este detalle: ecuación, rendimiento, cargo, días-persona y costo.";
                bajada.AutoSize = true;
                bajada.Font = new Font("Segoe UI", 9.3f, FontStyle.Regular);
                bajada.ForeColor = Color.FromArgb(80, 80, 80);
                bajada.Margin = new Padding(0, 0, 0, 12);

                FlowLayoutPanel encabezado = new FlowLayoutPanel();
                encabezado.FlowDirection = FlowDirection.TopDown;
                encabezado.WrapContents = false;
                encabezado.AutoSize = true;
                encabezado.Controls.Add(titulo);
                encabezado.Controls.Add(bajada);

                TabControl tabsDetalle = new TabControl();
                tabsDetalle.Dock = DockStyle.Fill;
                tabsDetalle.TabPages.Add(CrearTabResumenCalculoDesglose(req, detalle));
                tabsDetalle.TabPages.Add(CrearTabFormulaCalculoDesglose(req, detalle));
                tabsDetalle.TabPages.Add(CrearTabVariablesCalculoDesglose(req, detalle));
                tabsDetalle.TabPages.Add(CrearTabCargosCalculoDesglose(dialogo, req, detalle));
                tabsDetalle.TabPages.Add(CrearTabEdicionCalculoDesglose(dialogo, req, detalle));

                FlowLayoutPanel botones = new FlowLayoutPanel();
                botones.Dock = DockStyle.Fill;
                botones.FlowDirection = FlowDirection.RightToLeft;
                botones.AutoSize = true;
                botones.Padding = new Padding(0, 12, 0, 0);

                Button cerrar = new Button();
                cerrar.Text = "Cerrar";
                cerrar.Width = 110;
                cerrar.Height = 32;
                cerrar.Click += (s, e) => dialogo.Close();
                botones.Controls.Add(cerrar);

                root.Controls.Add(encabezado, 0, 0);
                root.Controls.Add(tabsDetalle, 0, 1);
                root.Controls.Add(botones, 0, 2);

                dialogo.Controls.Add(root);

                DialogResult resultado = dialogo.ShowDialog(this);
                string accion = dialogo.Tag as string ?? "";

                if (resultado == DialogResult.Retry)
                {
                    AbrirDestinoEdicionCalculoDesglose(accion, req);
                }
            }
        }

        private sealed class DetalleCalculoProductivoContexto
        {
            public List<EcuacionProductivaDefinicion> Biblioteca { get; set; } = new List<EcuacionProductivaDefinicion>();
            public EcuacionProductivaDefinicion Ecuacion { get; set; }
            public EcuacionProductivaRuntimeService.ResultadoPrueba Resultado { get; set; }
            public string ClaveEcuacion { get; set; } = "";
            public string RutaJson { get; set; } = "";
            public List<string> Advertencias { get; set; } = new List<string>();
        }

        private DetalleCalculoProductivoContexto CrearContextoDetalleCalculoDesglose(RequerimientoProduccionInterna req)
        {
            DetalleCalculoProductivoContexto detalle = new DetalleCalculoProductivoContexto();
            detalle.RutaJson = BibliotecaEcuacionesProductivasJsonService.ObtenerRutaEcuaciones();
            detalle.ClaveEcuacion = ExtraerClaveEcuacionPipeline(req == null ? "" : req.EcuacionUsada);
            detalle.Biblioteca = BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones();

            if (!string.IsNullOrWhiteSpace(detalle.ClaveEcuacion))
            {
                detalle.Ecuacion = detalle.Biblioteca.FirstOrDefault(e =>
                    e != null &&
                    string.Equals(e.Clave, detalle.ClaveEcuacion, StringComparison.OrdinalIgnoreCase));
            }

            if (detalle.Ecuacion == null && req != null)
            {
                detalle.Ecuacion = BibliotecaEcuacionesProductivasJsonService.BuscarMejorPara(
                    req.EtapaSugerida,
                    req.NombreRequerimiento,
                    req.EntregableCliente,
                    req.CargoSugerido
                );

                if (detalle.Ecuacion != null)
                {
                    detalle.Advertencias.Add(
                        "No se encontró la clave exacta de la fila; se resolvió por búsqueda en biblioteca: " +
                        detalle.Ecuacion.Clave + "."
                    );
                    detalle.ClaveEcuacion = detalle.Ecuacion.Clave;
                }
            }

            double diasHabilesSemana = cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0
                ? cotizacion.DiasHabilesEstudioPorSemana
                : 5.0;

            detalle.Resultado = CrearResultadoDetalleDesdeFilaFinal(
                req,
                detalle.Ecuacion,
                detalle.Biblioteca,
                diasHabilesSemana
            );
            RegistrarTrazaCalculoDetalle("detalle-final", req, detalle.Resultado);

            if (detalle.Resultado != null)
            {
                detalle.Advertencias.AddRange(detalle.Resultado.Errores);
                detalle.Advertencias.AddRange(detalle.Resultado.Advertencias);
            }

            if (req != null && !string.IsNullOrWhiteSpace(req.DiagnosticoParametros))
            {
                detalle.Advertencias.Add(req.DiagnosticoParametros);
            }

            detalle.Advertencias = detalle.Advertencias
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .Distinct()
                .ToList();

            return detalle;
        }

        private EcuacionProductivaRuntimeService.ResultadoPrueba CrearResultadoDetalleDesdeFilaFinal(
            RequerimientoProduccionInterna req,
            EcuacionProductivaDefinicion ecuacion,
            IEnumerable<EcuacionProductivaDefinicion> biblioteca,
            double diasHabilesSemana
        )
        {
            EcuacionProductivaRuntimeService.ResultadoPrueba resultado =
                new EcuacionProductivaRuntimeService.ResultadoPrueba();

            if (req == null)
            {
                resultado.Errores.Add("No hay fila de desglose para radiografiar.");
                return resultado;
            }

            resultado.Clave = ExtraerClaveEcuacionPipeline(req.EcuacionUsada);
            resultado.Nombre = NombreFilaDetalleCalculo(req);
            resultado.FormulaMadre = ecuacion == null ? "" : ecuacion.EcuacionBase;
            resultado.CantidadPrueba = req.Cantidad;
            resultado.UnidadPrueba = req.Unidad ?? "";

            double diasBase = ObtenerDiasFinalesDetalleCalculo(req);
            double horasBase = diasBase * HorasDiaEstandarManoObra;
            resultado.DiasTecnicos = diasBase;
            resultado.CostoCLP = req.CostoEstandarCLP;

            List<EcuacionProductivaRuntimeService.CargoVector> vector =
                req.TieneOverrideLocalCalculo && !string.IsNullOrWhiteSpace(req.CargosParticipantesOverrideJson)
                    ? EcuacionProductivaRuntimeService.ParsearVectorCargosParticipantesJson(req.CargosParticipantesOverrideJson)
                    : EcuacionProductivaRuntimeService.ParsearVectorCargos(req.CargoSugerido);

            if (vector.Count == 0 && ecuacion != null)
            {
                vector = EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion);
                if (vector.Count > 0)
                {
                    resultado.Advertencias.Add(
                        "La fila no trae cargos guardados; se usó el vector de la ecuación solo para explicar el detalle."
                    );
                }
            }

            if (vector.Count == 0)
            {
                resultado.Errores.Add("La fila no tiene cargos asociados para desglosar.");
                return resultado;
            }

            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();
            foreach (EcuacionProductivaRuntimeService.CargoVector item in vector)
            {
                CategoriaTrabajador cargo = BuscarCargoDetalleCalculo(cargos, item.Cargo);
                double dedicacion = item.Dedicacion <= 0.0 ? 1.0 : item.Dedicacion;
                EcuacionProductivaRuntimeService.ResultadoCargo fila =
                    new EcuacionProductivaRuntimeService.ResultadoCargo
                    {
                        CargoSolicitado = item.Cargo,
                        CargoResuelto = cargo == null ? item.Cargo : cargo.NombreCompleto,
                        CargoExiste = cargo != null,
                        RendimientoExiste = req.RendimientoCantidad > 0.0,
                        RequiereRendimientoProductivo = EsCargoProductivoDetalleCalculo(cargo),
                        Dedicacion = dedicacion,
                        HorasPorDia = HorasDiaEstandarManoObra,
                        CapacidadPorPeriodo = req.RendimientoCantidad,
                        Periodo = req.RendimientoPeriodo,
                        DiasPeriodo = BibliotecaRendimientosProductivosJsonService.ObtenerDiasPeriodo(
                            req.RendimientoPeriodo,
                            diasHabilesSemana > 0.0 ? diasHabilesSemana : 5.0
                        ),
                        CantidadPrueba = req.Cantidad,
                        UnidadPrueba = req.Unidad ?? "",
                        DiasTecnicos = diasBase * dedicacion,
                        HorasTecnicas = horasBase * dedicacion
                    };

                if (cargo != null)
                {
                    fila.TarifaDiaCLP = NormalizarSueldoMensualDetalleCalculo(cargo.SueldoMensualCLPTipico) / 22.0;
                    fila.TarifaHoraCLP = fila.TarifaDiaCLP / fila.HorasPorDia;
                    fila.TarifaDiaPonderadaCLP = fila.TarifaDiaCLP * dedicacion;
                    fila.TarifaHoraPonderadaCLP = fila.TarifaHoraCLP * dedicacion;
                    fila.CostoCLP = fila.HorasTecnicas * fila.TarifaHoraCLP;
                    fila.Diagnostico = dedicacion == 1.0
                        ? "OK. Calculado desde las horas finales de la fila del desglose."
                        : "Cargo de gestion/apoyo. Dedicacion calculada sobre las horas finales de la fila.";
                }
                else
                {
                    fila.Diagnostico = "Cargo no encontrado en biblioteca de cargos.";
                }

                resultado.Cargos.Add(fila);
            }

            double costoCargos = resultado.Cargos.Sum(c => c.CostoCLP);
            if (costoCargos > 0.0)
            {
                resultado.CostoCLP = costoCargos;
            }

            return resultado;
        }

        private double ObtenerDiasFinalesDetalleCalculo(RequerimientoProduccionInterna req)
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

            return Math.Max(0.0, req.DiasPersonaHolgura);
        }

        private CategoriaTrabajador BuscarCargoDetalleCalculo(IEnumerable<CategoriaTrabajador> cargos, string nombre)
        {
            string buscado = NormalizarTextoDetalleCalculo(nombre);
            if (string.IsNullOrWhiteSpace(buscado))
            {
                return null;
            }

            return (cargos ?? Enumerable.Empty<CategoriaTrabajador>())
                .FirstOrDefault(c =>
                    c != null &&
                    (NormalizarTextoDetalleCalculo(c.NombreCompleto) == buscado ||
                     NormalizarTextoDetalleCalculo(c.Nombre) == buscado));
        }

        private bool EsCargoProductivoDetalleCalculo(CategoriaTrabajador cargo)
        {
            if (cargo == null)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(cargo.TipoCargo) ||
                string.Equals(cargo.TipoCargo, "Productivo", StringComparison.OrdinalIgnoreCase);
        }

        private double NormalizarSueldoMensualDetalleCalculo(double valor)
        {
            return valor > 0.0 && valor < 10000.0 ? valor * 1000.0 : valor;
        }

        private string NombreFilaDetalleCalculo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "Sin fila";
            }

            string pieza = TextoSeguro(req.EntregableCliente);
            string proceso = TextoSeguro(req.NombreRequerimiento);
            if (string.IsNullOrWhiteSpace(proceso) || proceso == "No definido")
            {
                return pieza;
            }

            return pieza + " / " + proceso;
        }

        private string NormalizarTextoDetalleCalculo(string texto)
        {
            return (texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");
        }

        private void RegistrarTrazaCalculoDetalle(
            string etapa,
            RequerimientoProduccionInterna req,
            EcuacionProductivaRuntimeService.ResultadoPrueba resultado
        )
        {
            try
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calc-trace.log");
                string linea =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    " [CALC TRACE] " + etapa +
                    " | pieza=" + (req == null ? "" : req.EntregableCliente) +
                    " | req=" + (req == null ? "" : req.NombreRequerimiento) +
                    " | ecuacion=" + (req == null ? "" : req.EcuacionUsada) +
                    " | cantidad=" + (req == null ? "" : req.Cantidad.ToString("0.####")) +
                    " | diasStd=" + (req == null ? "" : req.DiasPersonaStd.ToString("0.####")) +
                    " | horasFinal=" + (req == null ? "" : (ObtenerDiasFinalesDetalleCalculo(req) * HorasDiaEstandarManoObra).ToString("0.####")) +
                    " | resultadoDias=" + (resultado == null ? "" : resultado.DiasTecnicos.ToString("0.####")) +
                    " | cargos=" + (resultado == null ? "" : resultado.Cargos.Count.ToString()) +
                    Environment.NewLine;

                File.AppendAllText(ruta, linea);
            }
            catch
            {
                // La traza es diagnóstica; nunca debe impedir abrir la radiografía.
            }
        }

        private TabPage CrearTabResumenCalculoDesglose(
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TabPage tab = new TabPage("Resumen");
            EcuacionProductivaRuntimeService.ResultadoPrueba resultado = detalle == null ? null : detalle.Resultado;
            EcuacionProductivaDefinicion ecuacion = detalle == null ? null : detalle.Ecuacion;
            tab.Controls.Add(CrearPanelDetalleCalculo(new[]
            {
                new[] { "Entregable cliente", TextoSeguro(req.EntregableCliente) },
                new[] { "Tipo interno", TextoSeguro(req.TipoInterno) },
                new[] { "Requerimiento", TextoSeguro(req.NombreRequerimiento) },
                new[] { "Cantidad solicitada", FormatearNumeroDefinibleDesglose(req, req.Cantidad, true) + " " + TextoSeguro(req.Unidad) },
                new[] { "Ecuación JSON", detalle == null || detalle.Ecuacion == null ? "No definida" : detalle.Ecuacion.Clave + " | " + detalle.Ecuacion.NombreVisible },
                new[] { "Tipo proceso", ecuacion == null ? "No definido" : ecuacion.TipoProceso.ToString() },
                new[] { "Método cálculo", ecuacion == null ? "No definido" : ecuacion.MetodoCalculo.ToString() },
                new[] { "Alcance temporal", ecuacion == null ? "No definido" : ecuacion.AlcanceTemporal.ToString() },
                new[] { "Proceso relacionado", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.IdProceso) ? "No definido" : ecuacion.IdProceso },
                new[] { "Etapa / bloque", TextoSeguro(ObtenerNombreVisibleEtapa(req.EtapaSugerida)) + " / " + TextoSeguro(req.BloqueProductivo) },
                new[] { "Flujo", TextoSeguro(req.ModoPlanificacion) },
                new[] { "Depende de", string.IsNullOrWhiteSpace(req.DependeDe) ? "Inicio de etapa / paralelo" : req.DependeDe },
                new[] { "Estado", req.ParametrosCompletos ? "Completo" : "Faltan parámetros por definir" },
                new[] { "Días técnicos ecuación", resultado == null || resultado.DiasTecnicos <= 0.0 ? "No definido" : resultado.DiasTecnicos.ToString("0.##") + " días" },
                new[] { "Costo ecuación", resultado == null || resultado.CostoCLP <= 0.0 ? "No definido" : FormatearValorVisual(resultado.CostoCLP) },
                new[] { "Costo en desglose", FormatearMontoVisibleCalculo(req, req.CostoEstandarCLP) },
                new[] { "Diagnóstico", string.IsNullOrWhiteSpace(req.DiagnosticoParametros) ? "Sin alertas" : req.DiagnosticoParametros },
                new[] { "Advertencias", detalle == null || detalle.Advertencias.Count == 0 ? "Sin alertas" : string.Join(Environment.NewLine, detalle.Advertencias) },
                new[] { "Nota", string.IsNullOrWhiteSpace(req.Nota) ? "Sin nota" : req.Nota }
            }));
            return tab;
        }

        private TabPage CrearTabFormulaCalculoDesglose(
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TabPage tab = new TabPage("Fórmula");
            EcuacionProductivaDefinicion ecuacion = detalle == null ? null : detalle.Ecuacion;
            string formula = ConstruirFormulaRenderizadaCalculoDesglose(ecuacion);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.BackColor = Color.White;

            layout.Controls.Add(CrearPanelDetalleCalculo(new[]
            {
                new[] { "Ecuación asignada", string.IsNullOrWhiteSpace(req.EcuacionUsada) ? "No definida" : req.EcuacionUsada },
                new[] { "Clave JSON", detalle == null || string.IsNullOrWhiteSpace(detalle.ClaveEcuacion) ? "No definida" : detalle.ClaveEcuacion },
                new[] { "Id proceso", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.IdProceso) ? "No definido" : ecuacion.IdProceso },
                new[] { "Tipo", ecuacion == null ? "No definido" : ecuacion.TipoProceso.ToString() },
                new[] { "Proceso origen", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.DependenciasJson) ? "No definido" : ecuacion.DependenciasJson },
                new[] { "Formula madre", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.EcuacionBase) ? "Sin formula madre" : ecuacion.EcuacionBase },
                new[] { "Formula usada", formula },
                new[] { "Lectura humana", ConstruirLecturaEcuacionCalculo(req) },
                new[] { "Impacto", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.Impacto) ? "No definido" : ecuacion.Impacto }
            }), 0, 0);

            Button verProceso = CrearBotonAccionDetalleCalculo("Ver proceso relacionado", 190);
            verProceso.Anchor = AnchorStyles.Left;
            verProceso.Margin = new Padding(14, 0, 0, 14);
            verProceso.Click += (s, e) => AbrirTabPrincipal(tabEcuacionesPrincipal, true);
            layout.Controls.Add(verProceso, 0, 1);

            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage CrearTabVariablesCalculoDesglose(
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TabPage tab = new TabPage("Variables");
            EcuacionProductivaDefinicion ecuacion = detalle == null ? null : detalle.Ecuacion;
            tab.Controls.Add(CrearPanelDetalleCalculo(new[]
            {
                new[] { "Variables declaradas JSON", ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.Variables) ? "No definidas" : ecuacion.Variables },
                new[] { "Variables usadas fila", ConstruirVariablesVisiblesCalculo(req) },
                new[] { "Rendimiento", FormatearNumeroDefinibleDesglose(req, req.RendimientoCantidad, true) + " " + TextoSeguro(req.Unidad) + " / " + TextoSeguro(req.RendimientoPeriodo) },
                new[] { "Origen rendimiento", string.IsNullOrWhiteSpace(req.RendimientoOrigen) ? "No definido" : req.RendimientoOrigen },
                new[] { "Días min.", FormatearNumeroDefinibleDesglose(req, req.DiasPersonaMin, true) },
                new[] { "Días std.", FormatearNumeroDefinibleDesglose(req, req.DiasPersonaStd, true) },
                new[] { "Días holg.", FormatearNumeroDefinibleDesglose(req, req.DiasPersonaHolgura, true) },
                new[] { "Costo min.", FormatearMontoVisibleCalculo(req, req.CostoMinimoCLP) },
                new[] { "Costo std.", FormatearMontoVisibleCalculo(req, req.CostoEstandarCLP) },
                new[] { "Costo holg.", FormatearMontoVisibleCalculo(req, req.CostoHolguraCLP) }
            }));
            return tab;
        }

        private TabPage CrearTabCargosCalculoDesglose(
            Form dialogo,
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TabPage tab = new TabPage("Cargos y cálculo");
            tab.Controls.Add(CrearGrillaCargosDetalleCalculo(dialogo, req, detalle));
            return tab;
        }

        private TabPage CrearTabEdicionCalculoDesglose(
            Form dialogo,
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TabPage tab = new TabPage("Origen / edición");

            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 9;
            panel.Padding = new Padding(18);
            panel.BackColor = Color.White;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label texto = new Label();
            texto.Text =
                "Estos accesos llevan a las bibliotecas JSON que alimentan esta fila. Editas ahí, vuelves al desglose y recalculas.";
            texto.AutoSize = true;
            texto.MaximumSize = new Size(820, 0);
            texto.Font = new Font("Segoe UI", 9.5f);
            texto.ForeColor = Color.FromArgb(70, 70, 70);
            texto.Margin = new Padding(0, 0, 0, 14);

            panel.Controls.Add(texto, 0, 0);
            panel.Controls.Add(CrearPanelDetalleCalculo(new[]
            {
                new[] { "JSON ecuaciones", detalle == null ? "No definido" : detalle.RutaJson },
                new[] { "Clave ecuación", detalle == null || string.IsNullOrWhiteSpace(detalle.ClaveEcuacion) ? "No definida" : detalle.ClaveEcuacion },
                new[] { "Fuente fila", string.IsNullOrWhiteSpace(req.EcuacionUsada) ? "No definida" : req.EcuacionUsada },
                new[] { "Cargos de la fila", string.IsNullOrWhiteSpace(req.CargoSugerido) ? "No definidos" : req.CargoSugerido },
                new[] { "Rendimiento origen", string.IsNullOrWhiteSpace(req.RendimientoOrigen) ? "No definido" : req.RendimientoOrigen }
            }), 0, 1);
            panel.Controls.Add(CrearBotonDestinoCalculo(dialogo, "Editar ecuación productiva", "ecuaciones"), 0, 2);
            panel.Controls.Add(CrearBotonDestinoCalculo(dialogo, "Editar rendimientos / capacidad", "rendimientos"), 0, 3);
            panel.Controls.Add(CrearBotonDestinoCalculo(dialogo, "Editar cargos y sueldos", "cargos"), 0, 4);
            panel.Controls.Add(CrearBotonDestinoCalculo(dialogo, "Editar subetapas", "subetapas"), 0, 5);

            tab.Controls.Add(panel);
            return tab;
        }

        private Button CrearBotonDestinoCalculo(Form dialogo, string texto, string destino)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 260;
            boton.Height = 34;
            boton.Margin = new Padding(0, 0, 0, 8);
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(10, 0, 0, 0);
            boton.Click += (s, e) =>
            {
                dialogo.Tag = destino;
                dialogo.DialogResult = DialogResult.Retry;
                dialogo.Close();
            };
            return boton;
        }

        private Panel CrearPanelDetalleCalculo(string[][] datos)
        {
            Panel contenedor = new Panel();
            contenedor.Dock = DockStyle.Fill;
            contenedor.AutoScroll = true;
            contenedor.Padding = new Padding(18);
            contenedor.BackColor = Color.White;

            TableLayoutPanel tabla = new TableLayoutPanel();
            tabla.AutoSize = true;
            tabla.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tabla.ColumnCount = 2;
            tabla.Dock = DockStyle.Top;
            tabla.BackColor = Color.White;
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (string[] dato in datos)
            {
                int fila = tabla.RowCount++;
                tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tabla.Controls.Add(CrearEtiquetaDetalleCalculo(dato[0], true), 0, fila);
                tabla.Controls.Add(CrearEtiquetaDetalleCalculo(dato[1], false), 1, fila);
            }

            contenedor.Controls.Add(tabla);
            return contenedor;
        }

        private Label CrearEtiquetaDetalleCalculo(string texto, bool titulo)
        {
            Label label = new Label();
            label.Text = string.IsNullOrWhiteSpace(texto) ? "No definido" : texto;
            label.AutoSize = true;
            label.MaximumSize = titulo ? new Size(200, 0) : new Size(660, 0);
            label.Margin = new Padding(0, 0, 12, 10);
            label.Font = new Font("Segoe UI", 9.5f, titulo ? FontStyle.Bold : FontStyle.Regular);
            label.ForeColor = titulo ? Color.FromArgb(25, 25, 25) : Color.FromArgb(55, 55, 55);
            return label;
        }

        private Control CrearGrillaCargosDetalleCalculo(
            Form dialogo,
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.BackColor = Color.White;
            layout.Padding = new Padding(14);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label estado = new Label();
            estado.AutoSize = true;
            estado.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            estado.ForeColor = req != null && req.TieneOverrideLocalCalculo
                ? Color.FromArgb(160, 105, 0)
                : Color.FromArgb(75, 75, 75);
            estado.Margin = new Padding(0, 0, 0, 8);
            estado.Text = req != null && req.TieneOverrideLocalCalculo
                ? "Esta fila tiene una sobreescritura local guardada."
                : "Sin cambios locales guardados.";

            bool[] actualizando = new[] { false };
            bool[] cambiosPendientes = new[] { false };
            bool[] detalleTecnicoVisible = new[] { false };
            string[] resultadoCierre = new[] { "sin cambios" };

            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grid.RowTemplate.Height = 28;

            DataGridViewComboBoxColumn colCargo = new DataGridViewComboBoxColumn();
            colCargo.Name = "Cargo";
            colCargo.HeaderText = "Cargo";
            colCargo.FlatStyle = FlatStyle.Flat;
            foreach (string cargo in Cargos.CrearBibliotecaCompleta()
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.NombreCompleto))
                .Select(c => c.NombreCompleto)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c))
            {
                colCargo.Items.Add(cargo);
            }
            grid.Columns.Add(colCargo);
            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Activo",
                HeaderText = "Estado",
                Width = 70
            });
            grid.Columns.Add("DedicacionPct", "Dedicación");
            grid.Columns.Add("Capacidad", "Capacidad");
            DataGridViewComboBoxColumn colPeriodo = new DataGridViewComboBoxColumn();
            colPeriodo.Name = "Periodo";
            colPeriodo.HeaderText = "Período";
            colPeriodo.FlatStyle = FlatStyle.Flat;
            foreach (string periodo in new[] { "día", "semana", "mes", "dia" })
            {
                colPeriodo.Items.Add(periodo);
            }
            grid.Columns.Add(colPeriodo);
            grid.Columns.Add("HorasCargo", "Horas");
            grid.Columns.Add("Costo", "Costo");
            grid.Columns.Add("Diagnostico", "Diagnóstico");
            grid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "EditarMaestro",
                HeaderText = "Acción",
                Text = "Editar cargo maestro",
                UseColumnTextForButtonValue = true,
                Width = 145
            });
            grid.Columns.Add("TarifaDia", "Tarifa diaria");
            grid.Columns.Add("HorasDia", "Horas/día");
            grid.Columns.Add("TarifaHora", "Tarifa base/h");
            grid.Columns.Add("Factor", "Factor");
            grid.Columns.Add("HorasBase", "Horas base");
            grid.Columns.Add("CostoHora", "Costo/h efectivo");

            grid.Columns["Cargo"].Width = 250;
            grid.Columns["DedicacionPct"].Width = 95;
            grid.Columns["Capacidad"].Width = 95;
            grid.Columns["Periodo"].Width = 95;
            grid.Columns["HorasCargo"].Width = 95;
            grid.Columns["Costo"].Width = 115;
            grid.Columns["Diagnostico"].Width = 320;
            grid.Columns["TarifaDia"].Width = 115;
            grid.Columns["HorasDia"].Width = 80;
            grid.Columns["TarifaHora"].Width = 115;
            grid.Columns["Factor"].Width = 75;
            grid.Columns["HorasBase"].Width = 95;
            grid.Columns["CostoHora"].Width = 125;

            foreach (string columna in new[] { "HorasCargo", "Costo", "Diagnostico", "TarifaDia", "HorasDia", "TarifaHora", "Factor", "HorasBase", "CostoHora" })
            {
                grid.Columns[columna].ReadOnly = true;
            }

            Action aplicarVisibilidad = () =>
            {
                foreach (string columna in new[] { "TarifaDia", "HorasDia", "TarifaHora", "Factor", "HorasBase", "CostoHora" })
                {
                    grid.Columns[columna].Visible = detalleTecnicoVisible[0];
                }
            };

            FlowLayoutPanel resumenHost = new FlowLayoutPanel();
            resumenHost.Dock = DockStyle.Top;
            resumenHost.AutoSize = true;
            resumenHost.WrapContents = true;
            resumenHost.Margin = new Padding(0, 0, 0, 8);

            Action<string> marcarPendiente = mensaje =>
            {
                if (actualizando[0])
                {
                    return;
                }
                cambiosPendientes[0] = true;
                estado.ForeColor = Color.FromArgb(170, 80, 0);
                estado.Text = "Cambios sin guardar. " + mensaje;
            };

            Func<List<CargoParticipanteFormula>> leerParticipantes = () =>
                grid.Rows.Cast<DataGridViewRow>()
                    .Where(r => r != null && !r.IsNewRow)
                    .Select(r => new CargoParticipanteFormula
                    {
                        Cargo = Convert.ToString(r.Cells["Cargo"].Value) ?? "",
                        Activo = Convert.ToBoolean(r.Cells["Activo"].Value ?? true),
                        DedicacionPorcentaje = Math.Max(0.0, Math.Min(100.0, ParsearDoubleDesglose(r.Cells["DedicacionPct"].Value, 100.0))),
                        HorasPorDia = 8.0
                    })
                    .Where(p => !string.IsNullOrWhiteSpace(p.Cargo))
                    .ToList();

            Action refrescar = () =>
            {
                if (actualizando[0])
                {
                    return;
                }

                actualizando[0] = true;
                try
                {
                    double capacidad = grid.Rows.Cast<DataGridViewRow>()
                        .Where(r => r != null && !r.IsNewRow)
                        .Select(r => ParsearDoubleDesglose(r.Cells["Capacidad"].Value, req.RendimientoCantidad))
                        .FirstOrDefault(v => v > 0.0);
                    string periodo = grid.Rows.Cast<DataGridViewRow>()
                        .Where(r => r != null && !r.IsNewRow)
                        .Select(r => Convert.ToString(r.Cells["Periodo"].Value) ?? req.RendimientoPeriodo)
                        .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)) ?? req.RendimientoPeriodo;

                    RequerimientoProduccionInterna temporal = ClonarRequerimientoDetalleCalculo(req);
                    temporal.RendimientoCantidad = capacidad > 0.0 ? capacidad : req.RendimientoCantidad;
                    temporal.RendimientoPeriodo = string.IsNullOrWhiteSpace(periodo) ? req.RendimientoPeriodo : periodo;
                    temporal.CargoSugerido = EcuacionProductivaRuntimeService.SerializarVectorCargos(
                        leerParticipantes()
                            .Where(p => p.Activo)
                            .Select(p => new EcuacionProductivaRuntimeService.CargoVector
                            {
                                Cargo = p.Cargo,
                                Dedicacion = p.DedicacionPorcentaje / 100.0
                            })
                    );

                    EcuacionProductivaRuntimeService.ResultadoPrueba resultado = CrearResultadoDetalleDesdeFilaFinal(
                        temporal,
                        detalle == null ? null : detalle.Ecuacion,
                        detalle == null ? new List<EcuacionProductivaDefinicion>() : detalle.Biblioteca,
                        cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0 ? cotizacion.DiasHabilesEstudioPorSemana : 5.0
                    );

                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (row == null || row.IsNewRow)
                        {
                            continue;
                        }

                        string cargoFila = Convert.ToString(row.Cells["Cargo"].Value) ?? "";
                        EcuacionProductivaRuntimeService.ResultadoCargo cargo = resultado.Cargos.FirstOrDefault(c =>
                            NormalizarTextoDetalleCalculo(c.CargoSolicitado) == NormalizarTextoDetalleCalculo(cargoFila) ||
                            NormalizarTextoDetalleCalculo(c.CargoResuelto) == NormalizarTextoDetalleCalculo(cargoFila));

                        if (cargo == null)
                        {
                            row.Cells["HorasCargo"].Value = "No definido";
                            row.Cells["Costo"].Value = "No definido";
                            row.Cells["Diagnostico"].Value = "Cargo inactivo o sin resultado.";
                            continue;
                        }

                        double horasBase = cargo.Dedicacion > 0.0 ? cargo.HorasTecnicas / cargo.Dedicacion : cargo.HorasTecnicas;
                        row.Cells["TarifaDia"].Value = cargo.TarifaDiaCLP > 0.0 ? FormatearValorVisual(cargo.TarifaDiaCLP) : "No definido";
                        row.Cells["HorasDia"].Value = cargo.HorasPorDia > 0.0 ? cargo.HorasPorDia.ToString("0.##") : "No definido";
                        row.Cells["TarifaHora"].Value = cargo.TarifaHoraCLP > 0.0 ? FormatearValorVisual(cargo.TarifaHoraCLP) + "/h" : "No definido";
                        row.Cells["Factor"].Value = cargo.Dedicacion.ToString("0.####");
                        row.Cells["HorasBase"].Value = horasBase > 0.0 ? horasBase.ToString("0.##") + " h" : "No definido";
                        row.Cells["HorasCargo"].Value = cargo.HorasTecnicas > 0.0 ? cargo.HorasTecnicas.ToString("0.##") + " h" : "No definido";
                        row.Cells["CostoHora"].Value = cargo.TarifaHoraPonderadaCLP > 0.0 ? FormatearValorVisual(cargo.TarifaHoraPonderadaCLP) + "/h" : "No definido";
                        row.Cells["Costo"].Value = cargo.CostoCLP > 0.0 ? FormatearValorVisual(cargo.CostoCLP) : "No definido";
                        row.Cells["Diagnostico"].Value = string.IsNullOrWhiteSpace(cargo.Diagnostico) ? "Sin diagnóstico" : cargo.Diagnostico;
                        row.DefaultCellStyle.BackColor = !cargo.CargoExiste || (!cargo.RendimientoExiste && cargo.RequiereRendimientoProductivo)
                            ? Color.FromArgb(255, 243, 205)
                            : Color.White;
                    }

                    resumenHost.Controls.Clear();
                    resumenHost.Controls.Add(CrearResumenTotalesCargosDetalleCalculo(resultado.Cargos));
                }
                finally
                {
                    actualizando[0] = false;
                }
            };

            Action<List<CargoParticipanteFormula>, double, string> cargar = (participantes, capacidad, periodo) =>
            {
                actualizando[0] = true;
                try
                {
                    grid.Rows.Clear();
                    foreach (CargoParticipanteFormula participante in participantes)
                    {
                        AsegurarValorComboDetalle(grid.Columns["Cargo"] as DataGridViewComboBoxColumn, participante.Cargo);
                        AsegurarValorComboDetalle(grid.Columns["Periodo"] as DataGridViewComboBoxColumn, periodo);
                        int rowIndex = grid.Rows.Add();
                        DataGridViewRow row = grid.Rows[rowIndex];
                        row.Cells["Cargo"].Value = participante.Cargo;
                        row.Cells["Activo"].Value = participante.Activo;
                        row.Cells["DedicacionPct"].Value = Math.Max(0.0, Math.Min(100.0, participante.DedicacionPorcentaje)).ToString("0.##");
                        row.Cells["Capacidad"].Value = capacidad > 0.0 ? capacidad.ToString("0.##") : "";
                        row.Cells["Periodo"].Value = string.IsNullOrWhiteSpace(periodo) ? "día" : periodo;
                    }
                }
                finally
                {
                    actualizando[0] = false;
                }
                refrescar();
            };

            List<CargoParticipanteFormula> iniciales = ObtenerParticipantesDetalleCalculo(req, detalle);
            cargar(iniciales, req.RendimientoCantidadOverride > 0.0 ? req.RendimientoCantidadOverride : req.RendimientoCantidad,
                string.IsNullOrWhiteSpace(req.RendimientoPeriodoOverride) ? req.RendimientoPeriodo : req.RendimientoPeriodoOverride);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Top;
            acciones.AutoSize = true;
            acciones.WrapContents = true;
            acciones.Margin = new Padding(0, 0, 0, 8);

            ComboBox cmbAgregar = new ComboBox();
            cmbAgregar.DropDownStyle = ComboBoxStyle.DropDown;
            cmbAgregar.Width = 280;
            foreach (object item in (grid.Columns["Cargo"] as DataGridViewComboBoxColumn).Items)
            {
                cmbAgregar.Items.Add(item);
            }

            Button btnAgregar = CrearBotonAccionDetalleCalculo("Agregar cargo", 120);
            btnAgregar.Click += (s, e) =>
            {
                string cargo = (cmbAgregar.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(cargo))
                {
                    return;
                }
                if (grid.Rows.Cast<DataGridViewRow>().Any(r => r != null && !r.IsNewRow &&
                    NormalizarTextoDetalleCalculo(Convert.ToString(r.Cells["Cargo"].Value)) == NormalizarTextoDetalleCalculo(cargo)))
                {
                    MessageBox.Show("Ese cargo ya está asignado.", "Detalle de cálculo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                cargar(leerParticipantes().Concat(new[] { new CargoParticipanteFormula { Cargo = cargo, Activo = true, DedicacionPorcentaje = 100.0 } }).ToList(),
                    ParsearDoubleDesglose(grid.Rows.Count > 0 ? grid.Rows[0].Cells["Capacidad"].Value : req.RendimientoCantidad, req.RendimientoCantidad),
                    grid.Rows.Count > 0 ? Convert.ToString(grid.Rows[0].Cells["Periodo"].Value) : req.RendimientoPeriodo);
                cmbAgregar.Text = "";
                marcarPendiente("Cargo agregado.");
            };

            Button btnQuitar = CrearBotonAccionDetalleCalculo("Quitar cargo", 110);
            btnQuitar.Click += (s, e) =>
            {
                if (grid.CurrentRow == null || grid.CurrentRow.IsNewRow)
                {
                    return;
                }
                if (leerParticipantes().Count(p => p.Activo) <= 1)
                {
                    MessageBox.Show("Vas a eliminar o dejar sin cargos productivos este cálculo.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                grid.Rows.Remove(grid.CurrentRow);
                refrescar();
                marcarPendiente("Cargo quitado.");
            };

            Button btnRecalcular = CrearBotonAccionDetalleCalculo("Recalcular", 100);
            btnRecalcular.Click += (s, e) => refrescar();

            Button btnGuardar = CrearBotonAccionDetalleCalculo("Guardar cambios", 140);
            btnGuardar.Click += (s, e) =>
            {
                AlcanceCambioDetalleCalculo alcance = PreguntarAlcanceDetalleCalculo(dialogo);
                if (alcance == AlcanceCambioDetalleCalculo.Cancelado)
                {
                    return;
                }

                if (alcance == AlcanceCambioDetalleCalculo.SoloFila)
                {
                    GuardarOverrideLocalDetalleCalculo(req, detalle, leerParticipantes(), grid);
                    resultadoCierre[0] = "cambios locales";
                    dialogo.Tag = resultadoCierre[0];
                    cambiosPendientes[0] = false;
                    estado.ForeColor = Color.FromArgb(0, 105, 92);
                    estado.Text = "Cambios locales guardados en esta fila del proyecto.";
                    RefrescarFilasDesgloseProductivoSinReconstruir();
                    RefrescarResumenDesgloseProductivo();
                    RefrescarDespuesDeEditarDesgloseProductivo();
                    return;
                }

                if (alcance == AlcanceCambioDetalleCalculo.Proceso)
                {
                    GuardarCambiosFormulaDetalleCalculo(detalle, leerParticipantes());
                    resultadoCierre[0] = "cambios de fórmula";
                    dialogo.Tag = resultadoCierre[0];
                    cambiosPendientes[0] = false;
                    estado.ForeColor = Color.FromArgb(0, 105, 92);
                    estado.Text = "Cambios guardados en la fórmula/proceso.";
                    GenerarDesgloseProductivoDesdeEcuaciones();
                    CargarDesgloseProductivoEnPantalla();
                    RefrescarResumenDesgloseProductivo();
                    RefrescarDespuesDeEditarDesgloseProductivo();
                }
            };

            Button btnDescartar = CrearBotonAccionDetalleCalculo("Descartar cambios", 145);
            btnDescartar.Click += (s, e) =>
            {
                cargar(iniciales, req.RendimientoCantidadOverride > 0.0 ? req.RendimientoCantidadOverride : req.RendimientoCantidad,
                    string.IsNullOrWhiteSpace(req.RendimientoPeriodoOverride) ? req.RendimientoPeriodo : req.RendimientoPeriodoOverride);
                cambiosPendientes[0] = false;
                resultadoCierre[0] = "sin cambios";
                estado.ForeColor = Color.FromArgb(75, 75, 75);
                estado.Text = "Cambios descartados.";
            };

            Button btnDetalle = CrearBotonAccionDetalleCalculo("Detalle técnico", 120);
            btnDetalle.Click += (s, e) =>
            {
                detalleTecnicoVisible[0] = !detalleTecnicoVisible[0];
                btnDetalle.Text = detalleTecnicoVisible[0] ? "Ocultar detalle" : "Detalle técnico";
                aplicarVisibilidad();
            };

            acciones.Controls.Add(cmbAgregar);
            acciones.Controls.Add(btnAgregar);
            acciones.Controls.Add(btnQuitar);
            acciones.Controls.Add(btnRecalcular);
            acciones.Controls.Add(btnGuardar);
            acciones.Controls.Add(btnDescartar);
            acciones.Controls.Add(btnDetalle);

            grid.CellEndEdit += (s, e) =>
            {
                if (actualizando[0] || e.RowIndex < 0)
                {
                    return;
                }
                if (grid.Columns[e.ColumnIndex].Name == "DedicacionPct")
                {
                    double v = Math.Max(0.0, Math.Min(100.0, ParsearDoubleDesglose(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 100.0)));
                    grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = v.ToString("0.##");
                }
                if (grid.Columns[e.ColumnIndex].Name == "Capacidad" &&
                    ParsearDoubleDesglose(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, 0.0) <= 0.0)
                {
                    MessageBox.Show("La capacidad debe ser mayor que cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = req.RendimientoCantidad.ToString("0.##");
                }
                refrescar();
                marcarPendiente("Asignación editada.");
            };
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    refrescar();
                    marcarPendiente("Estado editado.");
                }
            };
            grid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "EditarMaestro")
                {
                    dialogo.Tag = "cambios maestros";
                    AbrirTabPrincipal(tabCargosPrincipal, true);
                }
            };
            grid.DataError += (s, e) => { e.ThrowException = false; };
            dialogo.FormClosing += (s, e) =>
            {
                if (!cambiosPendientes[0])
                {
                    return;
                }
                AccionCambiosPendientesDetalleCalculo respuesta =
                    PreguntarCambiosPendientesDetalleCalculo(dialogo);
                if (respuesta == AccionCambiosPendientesDetalleCalculo.Cancelado)
                {
                    e.Cancel = true;
                    return;
                }
                if (respuesta == AccionCambiosPendientesDetalleCalculo.Guardar)
                {
                    GuardarOverrideLocalDetalleCalculo(req, detalle, leerParticipantes(), grid);
                    dialogo.Tag = "cambios locales";
                }
            };

            aplicarVisibilidad();
            layout.Controls.Add(estado, 0, 0);
            layout.Controls.Add(acciones, 0, 1);
            layout.Controls.Add(grid, 0, 2);
            layout.Controls.Add(resumenHost, 0, 3);
            return layout;
        }

        private Control CrearResumenTotalesCargosDetalleCalculo(
            List<EcuacionProductivaRuntimeService.ResultadoCargo> cargos
        )
        {
            List<EcuacionProductivaRuntimeService.ResultadoCargo> calculables =
                (cargos ?? new List<EcuacionProductivaRuntimeService.ResultadoCargo>())
                    .Where(c => c != null && c.CargoExiste)
                    .ToList();

            double costoHoraTotal = calculables.Sum(c => c.TarifaHoraPonderadaCLP);
            double horasBaseTotal = calculables.Sum(c =>
                c.Dedicacion > 0.0
                    ? c.HorasTecnicas / c.Dedicacion
                    : c.HorasTecnicas);
            double horasCargoTotal = calculables.Sum(c => c.HorasTecnicas);
            double costoTotal = calculables.Sum(c => c.CostoCLP);

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.WrapContents = true;
            panel.Padding = new Padding(0, 0, 0, 10);
            panel.BackColor = Color.White;

            panel.Controls.Add(CrearChipResumenCargoDetalle(
                "Costo total por hora",
                costoHoraTotal > 0.0 ? FormatearValorVisual(costoHoraTotal) + "/h" : "No definido",
                Color.FromArgb(230, 247, 241)
            ));
            panel.Controls.Add(CrearChipResumenCargoDetalle(
                "Horas base",
                horasBaseTotal > 0.0 ? horasBaseTotal.ToString("0.##") + " h" : "No definido",
                Color.FromArgb(245, 247, 250)
            ));
            panel.Controls.Add(CrearChipResumenCargoDetalle(
                "Horas cargo",
                horasCargoTotal > 0.0 ? horasCargoTotal.ToString("0.##") + " h" : "No definido",
                Color.FromArgb(245, 247, 250)
            ));
            panel.Controls.Add(CrearChipResumenCargoDetalle(
                "Costo derivado total",
                costoTotal > 0.0 ? FormatearValorVisual(costoTotal) : "No definido",
                Color.FromArgb(255, 248, 224)
            ));

            return panel;
        }

        private Control CrearChipResumenCargoDetalle(string titulo, string valor, Color fondo)
        {
            TableLayoutPanel chip = new TableLayoutPanel();
            chip.AutoSize = true;
            chip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            chip.ColumnCount = 1;
            chip.RowCount = 2;
            chip.Padding = new Padding(12, 8, 12, 8);
            chip.Margin = new Padding(0, 0, 10, 0);
            chip.BackColor = fondo;
            chip.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            Label lblTitulo = new Label();
            lblTitulo.AutoSize = true;
            lblTitulo.Text = titulo;
            lblTitulo.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblTitulo.ForeColor = Color.FromArgb(80, 80, 80);
            lblTitulo.Margin = new Padding(0, 0, 0, 2);

            Label lblValor = new Label();
            lblValor.AutoSize = true;
            lblValor.Text = valor;
            lblValor.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblValor.ForeColor = Color.FromArgb(25, 25, 25);
            lblValor.Margin = new Padding(0);

            chip.Controls.Add(lblTitulo, 0, 0);
            chip.Controls.Add(lblValor, 0, 1);
            return chip;
        }

        private Button CrearBotonAccionDetalleCalculo(string texto, int ancho)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = ancho;
            boton.Height = 30;
            boton.Margin = new Padding(0, 0, 8, 6);
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = Color.FromArgb(245, 245, 245);
            boton.ForeColor = Color.FromArgb(25, 25, 25);
            return boton;
        }

        private void AsegurarValorComboDetalle(DataGridViewComboBoxColumn columna, string valor)
        {
            valor = (valor ?? "").Trim();
            if (columna != null && !string.IsNullOrWhiteSpace(valor) && !columna.Items.Contains(valor))
            {
                columna.Items.Add(valor);
            }
        }

        private RequerimientoProduccionInterna ClonarRequerimientoDetalleCalculo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return new RequerimientoProduccionInterna();
            }

            return new RequerimientoProduccionInterna
            {
                EntregableCliente = req.EntregableCliente,
                CategoriaEntregable = req.CategoriaEntregable,
                EcuacionUsada = req.EcuacionUsada,
                ProyectoId = req.ProyectoId,
                GrupoId = req.GrupoId,
                ItemId = req.ItemId,
                InstanciaId = req.InstanciaId,
                ProcesoId = req.ProcesoId,
                TipoProceso = req.TipoProceso,
                MetodoCalculo = req.MetodoCalculo,
                AlcanceTemporal = req.AlcanceTemporal,
                CargoId = req.CargoId,
                PersonaId = req.PersonaId,
                DependenciasProcesoJson = req.DependenciasProcesoJson,
                PuedeEjecutarseEnParalelo = req.PuedeEjecutarseEnParalelo,
                TipoInterno = req.TipoInterno,
                NombreRequerimiento = req.NombreRequerimiento,
                Cantidad = req.Cantidad,
                Unidad = req.Unidad,
                EtapaSugerida = req.EtapaSugerida,
                Calidad = req.Calidad,
                BloqueProductivo = req.BloqueProductivo,
                ModoPlanificacion = req.ModoPlanificacion,
                DependeDe = req.DependeDe,
                CargoSugerido = req.CargoSugerido,
                NivelCargoSugerido = req.NivelCargoSugerido,
                AreaCargoSugerida = req.AreaCargoSugerida,
                SueldoMensualCargoCLP = req.SueldoMensualCargoCLP,
                TarifaDiaCargoCLP = req.TarifaDiaCargoCLP,
                RendimientoCantidad = req.RendimientoCantidad,
                RendimientoPeriodo = req.RendimientoPeriodo,
                RendimientoOrigen = req.RendimientoOrigen,
                ModoCalculoProductivo = req.ModoCalculoProductivo,
                HorasMinimas = req.HorasMinimas,
                HorasEstandar = req.HorasEstandar,
                HorasHolgura = req.HorasHolgura,
                OrigenHoras = req.OrigenHoras,
                DiasPersonaMin = req.DiasPersonaMin,
                DiasPersonaStd = req.DiasPersonaStd,
                DiasPersonaHolgura = req.DiasPersonaHolgura,
                CostoMinimoCLP = req.CostoMinimoCLP,
                CostoEstandarCLP = req.CostoEstandarCLP,
                CostoHolguraCLP = req.CostoHolguraCLP,
                ParametrosCompletos = req.ParametrosCompletos,
                DiagnosticoParametros = req.DiagnosticoParametros,
                EditadoManualmente = req.EditadoManualmente,
                TieneOverrideLocalCalculo = req.TieneOverrideLocalCalculo,
                CargosParticipantesOverrideJson = req.CargosParticipantesOverrideJson,
                RendimientoCantidadOverride = req.RendimientoCantidadOverride,
                RendimientoPeriodoOverride = req.RendimientoPeriodoOverride,
                Nota = req.Nota
            };
        }

        private List<CargoParticipanteFormula> ObtenerParticipantesDetalleCalculo(
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle
        )
        {
            if (req != null && req.TieneOverrideLocalCalculo &&
                !string.IsNullOrWhiteSpace(req.CargosParticipantesOverrideJson))
            {
                List<CargoParticipanteFormula> locales =
                    ParsearParticipantesDetalleCalculo(req.CargosParticipantesOverrideJson);
                if (locales.Count > 0)
                {
                    return locales;
                }
            }

            if (detalle != null && detalle.Ecuacion != null &&
                !string.IsNullOrWhiteSpace(detalle.Ecuacion.CargosParticipantesJson))
            {
                List<CargoParticipanteFormula> participantes =
                    ParsearParticipantesDetalleCalculo(detalle.Ecuacion.CargosParticipantesJson);
                if (participantes.Count > 0)
                {
                    return participantes;
                }
            }

            string cargos = req == null ? "" : req.CargoSugerido;
            if (string.IsNullOrWhiteSpace(cargos) && detalle != null && detalle.Ecuacion != null)
            {
                cargos = detalle.Ecuacion.CargosPermitidos;
            }

            return EcuacionProductivaRuntimeService.ParsearVectorCargos(cargos)
                .Select(c => new CargoParticipanteFormula
                {
                    Cargo = c.Cargo,
                    Activo = true,
                    DedicacionPorcentaje = Math.Max(0.0, Math.Min(100.0, c.Dedicacion * 100.0)),
                    HorasPorDia = 8.0
                })
                .ToList();
        }

        private List<CargoParticipanteFormula> ParsearParticipantesDetalleCalculo(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<List<CargoParticipanteFormula>>(json) ??
                    new List<CargoParticipanteFormula>();
            }
            catch
            {
                return new List<CargoParticipanteFormula>();
            }
        }

        private string SerializarParticipantesDetalleCalculo(IEnumerable<CargoParticipanteFormula> participantes)
        {
            return JsonSerializer.Serialize((participantes ?? new List<CargoParticipanteFormula>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Cargo))
                .ToList());
        }

        private AlcanceCambioDetalleCalculo PreguntarAlcanceDetalleCalculo(IWin32Window owner)
        {
            AlcanceCambioDetalleCalculo alcance = AlcanceCambioDetalleCalculo.Cancelado;

            using (Form dialogo = new Form())
            {
                dialogo.Text = "Alcance del cambio";
                dialogo.StartPosition = FormStartPosition.CenterParent;
                dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialogo.MaximizeBox = false;
                dialogo.MinimizeBox = false;
                dialogo.ShowInTaskbar = false;
                dialogo.AutoSize = true;
                dialogo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialogo.BackColor = Color.White;
                dialogo.Padding = new Padding(18);
                dialogo.KeyPreview = true;

                TableLayoutPanel layout = new TableLayoutPanel();
                layout.AutoSize = true;
                layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                layout.ColumnCount = 1;
                layout.RowCount = 5;
                layout.BackColor = Color.White;
                layout.MaximumSize = new Size(520, 0);

                Label pregunta = new Label();
                pregunta.Text = "¿Dónde quieres guardar estos cambios?";
                pregunta.AutoSize = true;
                pregunta.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
                pregunta.ForeColor = Color.FromArgb(25, 25, 25);
                pregunta.Margin = new Padding(0, 0, 0, 12);

                Button btnSoloFila = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Solo esta fila del proyecto",
                    "Crea una sobreescritura local y no modifica la biblioteca.",
                    true
                );
                Button btnProceso = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Este proceso para futuros cálculos",
                    "Actualiza la fórmula o proceso y afectará nuevos cálculos.",
                    false
                );
                Button btnCancelar = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Cancelar",
                    "Volver al editor sin guardar.",
                    false
                );

                btnSoloFila.Click += (s, e) =>
                {
                    alcance = AlcanceCambioDetalleCalculo.SoloFila;
                    dialogo.DialogResult = DialogResult.OK;
                    dialogo.Close();
                };
                btnProceso.Click += (s, e) =>
                {
                    alcance = AlcanceCambioDetalleCalculo.Proceso;
                    dialogo.DialogResult = DialogResult.OK;
                    dialogo.Close();
                };
                btnCancelar.Click += (s, e) =>
                {
                    alcance = AlcanceCambioDetalleCalculo.Cancelado;
                    dialogo.DialogResult = DialogResult.Cancel;
                    dialogo.Close();
                };

                dialogo.CancelButton = btnCancelar;
                dialogo.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        alcance = AlcanceCambioDetalleCalculo.Cancelado;
                        dialogo.DialogResult = DialogResult.Cancel;
                        dialogo.Close();
                    }
                };
                dialogo.FormClosing += (s, e) =>
                {
                    if (dialogo.DialogResult != DialogResult.OK)
                    {
                        alcance = AlcanceCambioDetalleCalculo.Cancelado;
                    }
                };

                layout.Controls.Add(pregunta, 0, 0);
                layout.Controls.Add(btnSoloFila, 0, 1);
                layout.Controls.Add(btnProceso, 0, 2);
                layout.Controls.Add(btnCancelar, 0, 3);
                dialogo.Controls.Add(layout);
                dialogo.ShowDialog(owner);
            }

            return alcance;
        }

        private AccionCambiosPendientesDetalleCalculo PreguntarCambiosPendientesDetalleCalculo(IWin32Window owner)
        {
            AccionCambiosPendientesDetalleCalculo accion =
                AccionCambiosPendientesDetalleCalculo.Cancelado;

            using (Form dialogo = new Form())
            {
                dialogo.Text = "Cambios sin guardar";
                dialogo.StartPosition = FormStartPosition.CenterParent;
                dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialogo.MaximizeBox = false;
                dialogo.MinimizeBox = false;
                dialogo.ShowInTaskbar = false;
                dialogo.AutoSize = true;
                dialogo.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                dialogo.BackColor = Color.White;
                dialogo.Padding = new Padding(18);
                dialogo.KeyPreview = true;

                TableLayoutPanel layout = new TableLayoutPanel();
                layout.AutoSize = true;
                layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                layout.ColumnCount = 1;
                layout.RowCount = 5;
                layout.BackColor = Color.White;
                layout.MaximumSize = new Size(520, 0);

                Label pregunta = new Label();
                pregunta.Text = "Hay cambios sin guardar en la radiografía.";
                pregunta.AutoSize = true;
                pregunta.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
                pregunta.ForeColor = Color.FromArgb(25, 25, 25);
                pregunta.Margin = new Padding(0, 0, 0, 12);

                Button btnGuardar = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Guardar como sobreescritura local",
                    "Conserva los cambios solo en esta fila del proyecto.",
                    true
                );
                Button btnDescartar = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Descartar cambios",
                    "Cierra la ventana sin guardar esta edición.",
                    false
                );
                Button btnCancelar = CrearBotonOpcionAlcanceDetalleCalculo(
                    "Cancelar",
                    "Volver al editor.",
                    false
                );

                btnGuardar.Click += (s, e) =>
                {
                    accion = AccionCambiosPendientesDetalleCalculo.Guardar;
                    dialogo.DialogResult = DialogResult.OK;
                    dialogo.Close();
                };
                btnDescartar.Click += (s, e) =>
                {
                    accion = AccionCambiosPendientesDetalleCalculo.Descartar;
                    dialogo.DialogResult = DialogResult.OK;
                    dialogo.Close();
                };
                btnCancelar.Click += (s, e) =>
                {
                    accion = AccionCambiosPendientesDetalleCalculo.Cancelado;
                    dialogo.DialogResult = DialogResult.Cancel;
                    dialogo.Close();
                };

                dialogo.CancelButton = btnCancelar;
                dialogo.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        accion = AccionCambiosPendientesDetalleCalculo.Cancelado;
                        dialogo.DialogResult = DialogResult.Cancel;
                        dialogo.Close();
                    }
                };
                dialogo.FormClosing += (s, e) =>
                {
                    if (dialogo.DialogResult != DialogResult.OK)
                    {
                        accion = AccionCambiosPendientesDetalleCalculo.Cancelado;
                    }
                };

                layout.Controls.Add(pregunta, 0, 0);
                layout.Controls.Add(btnGuardar, 0, 1);
                layout.Controls.Add(btnDescartar, 0, 2);
                layout.Controls.Add(btnCancelar, 0, 3);
                dialogo.Controls.Add(layout);
                dialogo.ShowDialog(owner);
            }

            return accion;
        }

        private Button CrearBotonOpcionAlcanceDetalleCalculo(
            string textoPrincipal,
            string textoSecundario,
            bool principal
        )
        {
            Button boton = new Button();
            boton.AutoSize = true;
            boton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            boton.MinimumSize = new Size(460, 54);
            boton.MaximumSize = new Size(500, 0);
            boton.Margin = new Padding(0, 0, 0, 8);
            boton.Padding = new Padding(12, 8, 12, 8);
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            boton.UseVisualStyleBackColor = false;
            boton.BackColor = principal ? Color.FromArgb(0, 150, 120) : Color.FromArgb(247, 248, 250);
            boton.ForeColor = principal ? Color.White : Color.FromArgb(25, 25, 25);
            boton.FlatAppearance.BorderColor = principal
                ? Color.FromArgb(0, 120, 96)
                : Color.FromArgb(210, 215, 222);
            boton.Text = textoPrincipal + Environment.NewLine + textoSecundario;
            return boton;
        }

        private void GuardarOverrideLocalDetalleCalculo(
            RequerimientoProduccionInterna req,
            DetalleCalculoProductivoContexto detalle,
            List<CargoParticipanteFormula> participantes,
            DataGridView grid
        )
        {
            if (req == null)
            {
                return;
            }

            double capacidad = grid.Rows.Cast<DataGridViewRow>()
                .Where(r => r != null && !r.IsNewRow)
                .Select(r => ParsearDoubleDesglose(r.Cells["Capacidad"].Value, req.RendimientoCantidad))
                .FirstOrDefault(v => v > 0.0);
            string periodo = grid.Rows.Cast<DataGridViewRow>()
                .Where(r => r != null && !r.IsNewRow)
                .Select(r => Convert.ToString(r.Cells["Periodo"].Value) ?? req.RendimientoPeriodo)
                .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)) ?? req.RendimientoPeriodo;

            req.TieneOverrideLocalCalculo = true;
            req.CargosParticipantesOverrideJson = SerializarParticipantesDetalleCalculo(participantes);
            req.CargoSugerido = EcuacionProductivaRuntimeService.SerializarVectorCargos(
                participantes
                    .Where(p => p.Activo)
                    .Select(p => new EcuacionProductivaRuntimeService.CargoVector
                    {
                        Cargo = p.Cargo,
                        Dedicacion = p.DedicacionPorcentaje / 100.0
                    })
            );
            req.RendimientoCantidad = capacidad > 0.0 ? capacidad : req.RendimientoCantidad;
            req.RendimientoPeriodo = string.IsNullOrWhiteSpace(periodo) ? req.RendimientoPeriodo : periodo;
            req.RendimientoCantidadOverride = req.RendimientoCantidad;
            req.RendimientoPeriodoOverride = req.RendimientoPeriodo;
            req.EditadoManualmente = true;
            req.Nota = (req.Nota + " Override local de radiografía.").Trim();

            EcuacionProductivaRuntimeService.ResultadoPrueba resultado =
                CrearResultadoDetalleDesdeFilaFinal(
                    req,
                    detalle == null ? null : detalle.Ecuacion,
                    detalle == null ? new List<EcuacionProductivaDefinicion>() : detalle.Biblioteca,
                    cotizacion != null && cotizacion.DiasHabilesEstudioPorSemana > 0.0
                        ? cotizacion.DiasHabilesEstudioPorSemana
                        : 5.0
                );

            if (resultado != null && resultado.Cargos.Count > 0)
            {
                req.DiasPersonaStd = Math.Max(0.0, resultado.DiasTecnicos);
                req.DiasPersonaMin = req.DiasPersonaStd;
                req.DiasPersonaHolgura = req.DiasPersonaStd;
                req.CostoEstandarCLP = Math.Max(0.0, resultado.CostoCLP);
                req.CostoMinimoCLP = req.CostoEstandarCLP;
                req.CostoHolguraCLP = req.CostoEstandarCLP;
                CalculoProductivoResolverService.SincronizarHorasDesdeDias(req);
            }

            RecalcularTotalesDesgloseProductivoDesdeFilas();
        }

        private void GuardarCambiosFormulaDetalleCalculo(
            DetalleCalculoProductivoContexto detalle,
            List<CargoParticipanteFormula> participantes
        )
        {
            if (detalle == null || detalle.Ecuacion == null)
            {
                return;
            }

            EcuacionProductivaDefinicion ecuacion = detalle.Biblioteca.FirstOrDefault(e =>
                e != null &&
                string.Equals(e.Clave, detalle.Ecuacion.Clave, StringComparison.OrdinalIgnoreCase));

            if (ecuacion == null)
            {
                return;
            }

            ecuacion.CargosParticipantesJson = SerializarParticipantesDetalleCalculo(participantes);
            ecuacion.CargosPermitidos = EcuacionProductivaRuntimeService.SerializarVectorCargos(
                participantes
                    .Where(p => p.Activo)
                    .Select(p => new EcuacionProductivaRuntimeService.CargoVector
                    {
                        Cargo = p.Cargo,
                        Dedicacion = p.DedicacionPorcentaje / 100.0
                    })
            );

            BibliotecaEcuacionesProductivasJsonService.GuardarEcuaciones(detalle.Biblioteca);
        }

        private string ConstruirFormulaRenderizadaCalculoDesglose(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "No hay ecuación enlazada para mostrar fórmula.";
            }

            if (!string.IsNullOrWhiteSpace(ecuacion.FormulaReferencia))
            {
                return ecuacion.FormulaReferencia;
            }

            string numerador = string.IsNullOrWhiteSpace(ecuacion.Numerador)
                ? "cantidad"
                : ecuacion.Numerador.Trim();
            string denominador = string.IsNullOrWhiteSpace(ecuacion.Denominador)
                ? "capacidad_por_periodo[cargo]"
                : ecuacion.Denominador.Trim();

            return
                "horas_base[cargo] = " + numerador + " / " + denominador + Environment.NewLine +
                "horas_cargo[cargo] = horas_base[cargo] * dedicacion[cargo]" + Environment.NewLine +
                "costo_derivado = Σ(horas_cargo[cargo] * tarifa_horaria[cargo])";
        }

        private string LimpiarNombreCargoDetalle(string cargoResuelto, string cargoSolicitado)
        {
            string texto = string.IsNullOrWhiteSpace(cargoResuelto) ? cargoSolicitado : cargoResuelto;
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "No definido";
            }

            while (texto.Contains("(típico) (típico)"))
            {
                texto = texto.Replace("(típico) (típico)", "(típico)");
            }

            while (texto.Contains("(tipico) (tipico)"))
            {
                texto = texto.Replace("(tipico) (tipico)", "(tipico)");
            }

            return texto.Trim();
        }

        private void AbrirDestinoEdicionCalculoDesglose(string accion, RequerimientoProduccionInterna req)
        {
            if (accion == "ecuaciones")
            {
                AbrirTabEcuacionesProductivas(req == null ? "" : req.EcuacionUsada);
                return;
            }

            if (accion == "rendimientos")
            {
                AbrirTabPrincipal(tabRendimientosPrincipal, true);
                return;
            }

            if (accion == "cargos")
            {
                AbrirTabPrincipal(tabCargosPrincipal, true);
                return;
            }

            if (accion == "subetapas")
            {
                AbrirTabPrincipal(tabSubEtapasPrincipal, true);
            }
        }

        private string ConstruirVariablesVisiblesCalculo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "No definido";
            }

            return
                "cantidad=" + req.Cantidad.ToString("0.##") +
                "; unidad=" + TextoSeguro(req.Unidad) +
                "; calidad=" + TextoSeguro(req.Calidad) +
                "; capacidad=" + FormatearNumeroDefinibleDesglose(req, req.RendimientoCantidad, true) +
                "; periodo=" + TextoSeguro(req.RendimientoPeriodo) +
                "; cargo=" + TextoSeguro(req.CargoSugerido);
        }

        private string ConstruirLecturaEcuacionCalculo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "No definido";
            }

            if (!req.ParametrosCompletos)
            {
                return "Faltan parámetros: " + TextoSeguro(req.DiagnosticoParametros);
            }

            return "La cantidad solicitada se transforma en días-persona usando la capacidad del rendimiento; esos días se valorizan con la tarifa día del cargo.";
        }

        private string FormatearMontoVisibleCalculo(RequerimientoProduccionInterna req, double valorCLP)
        {
            if (req != null && !req.ParametrosCompletos)
            {
                return "No definido";
            }

            if (valorCLP <= 0.0)
            {
                return "No definido";
            }

            return FormatearValorVisual(valorCLP);
        }

        private string TextoSeguro(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? "No definido" : texto;
        }

        private RequerimientoProduccionInterna ObtenerRequerimientoSeleccionadoDesglose()
        {
            if (dgvDesgloseProductivo == null)
            {
                return null;
            }

            DataGridViewRow row = dgvDesgloseProductivo.CurrentRow;

            if (row == null && dgvDesgloseProductivo.CurrentCell != null)
            {
                row = dgvDesgloseProductivo.Rows[dgvDesgloseProductivo.CurrentCell.RowIndex];
            }

            return row == null ? null : row.Tag as RequerimientoProduccionInterna;
        }

        private void RefrescarEditorRendimientoDesglose()
        {
            RequerimientoProduccionInterna req = ObtenerRequerimientoSeleccionadoDesglose();

            bool habilitado = req != null;
            nudRendimientoFilaDesglose.Enabled = habilitado;
            cmbPeriodoRendimientoFilaDesglose.Enabled = habilitado;
            btnAplicarRendimientoFilaDesglose.Enabled = habilitado;
            btnGuardarRendimientoBibliotecaDesglose.Enabled = habilitado;

            if (!habilitado)
            {
                lblUnidadRendimientoFilaDesglose.Text = "unidades";
                return;
            }

            decimal valor = 0;
            if (req.RendimientoCantidad > 0.0)
            {
                valor = Convert.ToDecimal(req.RendimientoCantidad);
            }

            if (valor < nudRendimientoFilaDesglose.Minimum)
            {
                valor = nudRendimientoFilaDesglose.Minimum;
            }

            if (valor > nudRendimientoFilaDesglose.Maximum)
            {
                valor = nudRendimientoFilaDesglose.Maximum;
            }

            nudRendimientoFilaDesglose.Value = valor;
            lblUnidadRendimientoFilaDesglose.Text =
                string.IsNullOrWhiteSpace(req.Unidad) ? "unidades" : req.Unidad;

            string periodo = NormalizarPeriodoRendimiento(req.RendimientoPeriodo);
            cmbPeriodoRendimientoFilaDesglose.SelectedItem = periodo;
        }

        private void BtnAplicarRendimientoFilaDesglose_Click(object sender, EventArgs e)
        {
            AplicarEditorRendimientoAFilaSeleccionada(false);
        }

        private void BtnGuardarRendimientoBibliotecaDesglose_Click(object sender, EventArgs e)
        {
            AplicarEditorRendimientoAFilaSeleccionada(true);
        }

        private void AplicarEditorRendimientoAFilaSeleccionada(bool guardarEnJson)
        {
            RequerimientoProduccionInterna req = ObtenerRequerimientoSeleccionadoDesglose();

            if (req == null)
            {
                return;
            }

            req.RendimientoCantidad = Convert.ToDouble(nudRendimientoFilaDesglose.Value);
            req.RendimientoPeriodo = NormalizarPeriodoRendimiento(
                Convert.ToString(cmbPeriodoRendimientoFilaDesglose.SelectedItem)
            );
            req.RendimientoOrigen = guardarEnJson
                ? "Biblioteca JSON"
                : "Editado en desglose";

            RecalcularCostoRequerimientoDesdeCargo(req);
            RecalcularRequerimientoDesdeRendimientoSiCorresponde(req);
            NormalizarDiasRequerimiento(req);
            RecalcularTotalesDesgloseProductivoDesdeFilas();

            if (guardarEnJson)
            {
                GuardarRendimientoSeleccionadoEnJson(req);
            }

            RefrescarFilasDesgloseProductivoSinReconstruir();
            RefrescarResumenDesgloseProductivo();
            RefrescarDespuesDeEditarDesgloseProductivo();
        }

        private void GuardarRendimientoSeleccionadoEnJson(RequerimientoProduccionInterna req)
        {
            if (req == null || req.RendimientoCantidad <= 0.0)
            {
                return;
            }

            List<RendimientoProductivo> lista =
                BibliotecaRendimientosProductivosJsonService.CargarRendimientos();

            string claveNueva = CrearClaveRendimientoDesglose(
                req.EtapaSugerida,
                req.TipoInterno,
                req.NombreRequerimiento,
                req.Unidad,
                req.CargoSugerido
            );

            RendimientoProductivo existente = lista.FirstOrDefault(r =>
                CrearClaveRendimientoDesglose(
                    r.Etapa,
                    r.TipoInterno,
                    r.Proceso,
                    r.Unidad,
                    r.Cargo
                ) == claveNueva);

            if (existente == null)
            {
                int siguienteId = lista
                    .Where(r => r != null)
                    .Select(r => r.Id)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                existente = new RendimientoProductivo
                {
                    Id = siguienteId,
                    Activo = true
                };

                lista.Add(existente);
            }

            existente.Etapa = req.EtapaSugerida;
            existente.TipoInterno = req.TipoInterno;
            existente.Proceso = req.NombreRequerimiento;
            existente.Unidad = req.Unidad;
            existente.Cargo = req.CargoSugerido;
            existente.NivelCargo = req.NivelCargoSugerido;
            existente.CantidadPorPeriodo = req.RendimientoCantidad;
            existente.Periodo = req.RendimientoPeriodo;
            existente.Nota = "Editado desde desglose productivo.";

            BibliotecaRendimientosProductivosJsonService.GuardarRendimientos(lista);
        }

        private string CrearClaveRendimientoDesglose(
            string etapa,
            string tipo,
            string proceso,
            string unidad,
            string cargo
        )
        {
            return NormalizarClavePlanificacion(etapa) + "|" +
                NormalizarClavePlanificacion(tipo) + "|" +
                NormalizarClavePlanificacion(proceso) + "|" +
                NormalizarClavePlanificacion(unidad) + "|" +
                NormalizarClavePlanificacion(cargo);
        }

        private string NormalizarPeriodoRendimiento(string periodo)
        {
            string p = (periodo ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("í", "i")
                .Replace("Ã­", "i")
                .Replace("ã­", "i");

            if (p.Contains("dia") || p.Contains("dÃ­a") || p.Contains("dã­a"))
            {
                return "dia";
            }

            if (p.Contains("mes"))
            {
                return "mes";
            }

            return "semana";
        }

        private void AplicarCambiosDesgloseProductivoEnVivo()
        {
            if (cargandoDesgloseProductivo || aplicandoCambiosDesgloseProductivo)
            {
                return;
            }

            if (cotizacion == null || dgvDesgloseProductivo == null)
            {
                return;
            }

            try
            {
                aplicandoCambiosDesgloseProductivo = true;

                GuardarDesgloseProductivoDesdePantalla();
                RecalcularTotalesDesgloseProductivoDesdeFilas();

                RefrescarFilasDesgloseProductivoSinReconstruir();
                RefrescarResumenDesgloseProductivo();
                RefrescarDespuesDeEditarDesgloseProductivo();
            }
            finally
            {
                aplicandoCambiosDesgloseProductivo = false;
            }
        }

        private void RefrescarFilasDesgloseProductivoSinReconstruir()
        {
            if (dgvDesgloseProductivo == null)
            {
                return;
            }

            try
            {
                cargandoDesgloseProductivo = true;

                foreach (DataGridViewRow row in dgvDesgloseProductivo.Rows)
                {
                    if (row == null || row.IsNewRow)
                    {
                        continue;
                    }

                    RequerimientoProduccionInterna req =
                        row.Tag as RequerimientoProduccionInterna;

                    if (req == null)
                    {
                        continue;
                    }

                    AsegurarPlanificacionRequerimiento(req);
                    row.Cells["BloqueProductivo"].Value = req.BloqueProductivo;
                    row.Cells["ModoPlanificacion"].Value = NormalizarModoPlanificacion(req.ModoPlanificacion);
                    row.Cells["DependeDe"].Value = req.DependeDe;

                    row.Cells["Cantidad"].Value = req.Cantidad.ToString("0.##");
                    row.Cells["Calidad"].Value = NormalizarCalidadParaCombo(req.Calidad);

                    row.Cells["RendimientoCantidad"].Value = FormatearNumeroDefinibleDesglose(
                        req,
                        req.RendimientoCantidad,
                        true
                    );
                    row.Cells["RendimientoPeriodo"].Value = string.IsNullOrWhiteSpace(req.RendimientoPeriodo)
                        ? "semana"
                        : req.RendimientoPeriodo;
                    row.Cells["RendimientoOrigen"].Value = string.IsNullOrWhiteSpace(req.RendimientoOrigen)
                        ? (req.ParametrosCompletos ? "" : "No definido")
                        : req.RendimientoOrigen;

                    row.Cells["ModoCalculoProductivo"].Value =
                        ModosCalculoProductivo.Normalizar(req.ModoCalculoProductivo);
                    row.Cells["HorasMinimas"].Value = FormatearHorasDefiniblesDesglose(req.HorasMinimas);
                    row.Cells["HorasEstandar"].Value = FormatearHorasDefiniblesDesglose(req.HorasEstandar);
                    row.Cells["HorasHolgura"].Value = FormatearHorasDefiniblesDesglose(req.HorasHolgura);
                    row.Cells["OrigenHoras"].Value = string.IsNullOrWhiteSpace(req.OrigenHoras)
                        ? ""
                        : req.OrigenHoras;

                    row.Cells["DiasPersonaMin"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaMin, true);
                    row.Cells["DiasPersonaStd"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaStd, true);
                    row.Cells["DiasPersonaHolgura"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaHolgura, true);

                    string cargoVisible = ObtenerTextoCargoDesglose(
                        req.CargoSugerido,
                        req.NivelCargoSugerido
                    );
                    AsegurarOpcionCargoDesglose(cargoVisible);
                    row.Cells["CargoSugerido"].Value = string.IsNullOrWhiteSpace(cargoVisible)
                        ? null
                        : cargoVisible;
                    row.Cells["NivelCargoSugerido"].Value = req.NivelCargoSugerido;
                    row.Cells["SueldoMensualCargoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.SueldoMensualCargoCLP);
                    row.Cells["TarifaDiaCargoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.TarifaDiaCargoCLP);

                    row.Cells["CostoMinimoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoMinimoCLP);
                    row.Cells["CostoEstandarCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoEstandarCLP);
                    row.Cells["CostoHolguraCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoHolguraCLP);

                    row.Cells["Nota"].Value = UnirNotaDiagnosticoDesglose(req);

                    AplicarColorFilaDesgloseProductivo(row, req.EtapaSugerida);
                }
            }
            finally
            {
                cargandoDesgloseProductivo = false;
            }
        }


        private void BtnGenerarDesgloseProductivo_Click(object sender, EventArgs e)
        {
            GenerarDesgloseProductivoDesdeEcuaciones();
            CargarDesgloseProductivoEnPantalla();
            RefrescarResumenDesgloseProductivo();
            RefrescarDespuesDeEditarDesgloseProductivo();
        }

        private void BtnAplicarDesgloseProductivo_Click(object sender, EventArgs e)
        {
            GuardarDesgloseProductivoDesdePantalla();
            RecalcularTotalesDesgloseProductivoDesdeFilas();

            RefrescarResumenDesgloseProductivo();
            CargarDesgloseProductivoEnPantalla();
            RefrescarDespuesDeEditarDesgloseProductivo();
        }

        private void GenerarDesgloseProductivoDesdeEcuaciones()
        {
            if (cotizacion == null)
            {
                return;
            }

            cotizacion.DesgloseProductivo = EsContextoDesgloseProyectoGlobal()
                ? ProyectoDesgloseGlobalService.Construir(
                    proyectoCotizacionActual,
                    cotizacion)
                : DesgloseProductivoService.Generar(cotizacion);

            BibliotecaSubEtapasService.SincronizarDesdeDesgloseProductivo(
                cotizacion,
                bibliotecaSubEtapas,
                true
            );

            cotizacion.EvaluacionPlazo =
                EvaluadorPlazoProyectoService.Evaluar(cotizacion);

            cotizacion.DuracionMinimaTecnicaSemanas =
                cotizacion.EvaluacionPlazo.SemanasMinimasEstimadas;

            cotizacion.DuracionEstandarTecnicaSemanas =
                cotizacion.EvaluacionPlazo.SemanasEstandarEstimadas;

            cotizacion.DuracionHolguraTecnicaSemanas =
                cotizacion.EvaluacionPlazo.SemanasConHolguraEstimadas;

            cotizacion.DiagnosticoPlazo =
                cotizacion.EvaluacionPlazo.DiagnosticoPlazo;
        }

        private void CargarDesgloseProductivoEnPantalla()
        {
            if (dgvDesgloseProductivo == null || cotizacion == null)
            {
                return;
            }

            try
            {
                cargandoDesgloseProductivo = true;

                dgvDesgloseProductivo.Rows.Clear();

                if (cotizacion.DesgloseProductivo == null ||
                    cotizacion.DesgloseProductivo.Requerimientos == null ||
                    cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
                {
                    cotizacion.DesgloseProductivo = EsContextoDesgloseProyectoGlobal()
                        ? ProyectoDesgloseGlobalService.Construir(
                            proyectoCotizacionActual,
                            cotizacion)
                        : DesgloseProductivoService.Generar(cotizacion);

                    BibliotecaSubEtapasService.SincronizarDesdeDesgloseProductivo(
                        cotizacion,
                        bibliotecaSubEtapas,
                        true
                    );
                }

                if (cotizacion.DesgloseProductivo == null ||
                    cotizacion.DesgloseProductivo.Requerimientos == null)
                {
                    return;
                }

                DesgloseProductivoService.ValidarParametrosDesglose(cotizacion.DesgloseProductivo);

                var requerimientosOrdenados = cotizacion.DesgloseProductivo.Requerimientos
                    .Select(r =>
                    {
                        AsegurarPlanificacionRequerimiento(r);
                        return r;
                    })
                    .OrderBy(r => ObtenerOrdenEtapaGeneral(r.EtapaSugerida))
                    .ThenBy(r => r.BloqueProductivo)
                    .ThenBy(r => r.EntregableCliente)
                    .ThenBy(r => ObtenerOrdenTipoInternoDesglose(r.TipoInterno))
                    .ThenBy(r => r.NombreRequerimiento)
                    .ToList();

                foreach (RequerimientoProduccionInterna req in requerimientosOrdenados)
                {
                    int rowIndex = dgvDesgloseProductivo.Rows.Add();
                    DataGridViewRow row = dgvDesgloseProductivo.Rows[rowIndex];

                    row.Tag = req;

                    row.Cells["ProductoProyecto"].Value =
                        ObtenerNombreProductoDesgloseGlobal(req.ItemId);
                    row.Cells["EntregableCliente"].Value = req.EntregableCliente;
                    row.Cells["EcuacionUsada"].Value = req.EcuacionUsada;
                    row.Cells["TipoProceso"].Value = ObtenerNombreTipoProcesoDesglose(req.TipoProceso);
                    row.Cells["TipoInterno"].Value = req.TipoInterno;
                    row.Cells["NombreRequerimiento"].Value = req.NombreRequerimiento;
                    row.Cells["Cantidad"].Value = req.Cantidad.ToString("0.##");
                    row.Cells["Unidad"].Value = req.Unidad;
                    row.Cells["EtapaSugerida"].Value = ObtenerNombreVisibleEtapa(req.EtapaSugerida);
                    AsegurarPlanificacionRequerimiento(req);
                    row.Cells["BloqueProductivo"].Value = req.BloqueProductivo;
                    row.Cells["ModoPlanificacion"].Value = NormalizarModoPlanificacion(req.ModoPlanificacion);
                    row.Cells["DependeDe"].Value = req.DependeDe;
                    row.Cells["Calidad"].Value = NormalizarCalidadParaCombo(req.Calidad);

                    string cargoVisible = ObtenerTextoCargoDesglose(
                        req.CargoSugerido,
                        req.NivelCargoSugerido
                    );
                    AsegurarOpcionCargoDesglose(cargoVisible);
                    row.Cells["CargoSugerido"].Value = string.IsNullOrWhiteSpace(cargoVisible)
                        ? null
                        : cargoVisible;
                    row.Cells["NivelCargoSugerido"].Value = req.NivelCargoSugerido;
                    row.Cells["SueldoMensualCargoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.SueldoMensualCargoCLP);
                    row.Cells["TarifaDiaCargoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.TarifaDiaCargoCLP);

                    row.Cells["RendimientoCantidad"].Value = FormatearNumeroDefinibleDesglose(
                        req,
                        req.RendimientoCantidad,
                        true
                    );
                    row.Cells["RendimientoPeriodo"].Value = string.IsNullOrWhiteSpace(req.RendimientoPeriodo)
                        ? "semana"
                        : req.RendimientoPeriodo;
                    row.Cells["RendimientoOrigen"].Value = string.IsNullOrWhiteSpace(req.RendimientoOrigen)
                        ? (req.ParametrosCompletos ? "" : "No definido")
                        : req.RendimientoOrigen;

                    row.Cells["ModoCalculoProductivo"].Value =
                        ModosCalculoProductivo.Normalizar(req.ModoCalculoProductivo);
                    row.Cells["HorasMinimas"].Value = FormatearHorasDefiniblesDesglose(req.HorasMinimas);
                    row.Cells["HorasEstandar"].Value = FormatearHorasDefiniblesDesglose(req.HorasEstandar);
                    row.Cells["HorasHolgura"].Value = FormatearHorasDefiniblesDesglose(req.HorasHolgura);
                    row.Cells["OrigenHoras"].Value = string.IsNullOrWhiteSpace(req.OrigenHoras)
                        ? ""
                        : req.OrigenHoras;

                    row.Cells["DiasPersonaMin"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaMin, true);
                    row.Cells["DiasPersonaStd"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaStd, true);
                    row.Cells["DiasPersonaHolgura"].Value = FormatearNumeroDefinibleDesglose(req, req.DiasPersonaHolgura, true);

                    row.Cells["CostoMinimoCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoMinimoCLP);
                    row.Cells["CostoEstandarCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoEstandarCLP);
                    row.Cells["CostoHolguraCLP"].Value = FormatearMontoDefinibleDesglose(req, req.CostoHolguraCLP);

                    row.Cells["Nota"].Value = UnirNotaDiagnosticoDesglose(req);

                    AplicarColorFilaDesgloseProductivo(row, req.EtapaSugerida);
                }

                AjustarColumnasDesgloseAlContenido();
            }
            finally
            {
                cargandoDesgloseProductivo = false;
            }
        }

        private void AplicarColorFilaDesgloseProductivo(DataGridViewRow row, string etapa)
        {
            if (row == null)
            {
                return;
            }

            RequerimientoProduccionInterna req = row.Tag as RequerimientoProduccionInterna;

            if (req != null && !req.ParametrosCompletos)
            {
                Color advertencia = Color.FromArgb(255, 245, 214);
                row.DefaultCellStyle.BackColor = advertencia;
                row.DefaultCellStyle.ForeColor = Color.FromArgb(70, 45, 0);
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 211, 92);
                row.DefaultCellStyle.SelectionForeColor = Color.Black;

                if (dgvDesgloseProductivo != null &&
                    dgvDesgloseProductivo.Columns.Contains("Nota"))
                {
                    row.Cells["Nota"].Style.ForeColor = Color.FromArgb(140, 70, 0);
                    row.Cells["Nota"].Style.Font = new Font(dgvDesgloseProductivo.Font, FontStyle.Bold);
                }

                return;
            }

            Color colorFila = ObtenerColorFilaEtapa(etapa);
            Color colorSeleccion = ObtenerColorBarraEtapa(etapa);

            row.DefaultCellStyle.BackColor = colorFila;
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.SelectionBackColor = colorSeleccion;
            row.DefaultCellStyle.SelectionForeColor = Color.Black;

            if (dgvDesgloseProductivo != null &&
                dgvDesgloseProductivo.Columns.Contains("EtapaSugerida"))
            {
                row.Cells["EtapaSugerida"].Style.BackColor =
                    MezclarConBlanco(ObtenerColorBaseEtapa(etapa), 0.35);

                row.Cells["EtapaSugerida"].Style.ForeColor = Color.Black;
                row.Cells["EtapaSugerida"].Style.Font =
                    new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
        }

        private string NormalizarCalidadParaCombo(string calidad)
        {
            if (string.IsNullOrWhiteSpace(calidad))
            {
                return "Estándar";
            }

            string c = calidad.Trim().ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");

            if (c.Contains("baja") || c.Contains("boceto") || c.Contains("rough"))
            {
                return "Baja / boceto";
            }

            if (c.Contains("alta") || c.Contains("pulido"))
            {
                return "Alta / pulido";
            }

            if (c.Contains("premium"))
            {
                return "Premium";
            }

            return "Estándar";
        }

        private void AsegurarPlanificacionRequerimiento(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(req.BloqueProductivo))
            {
                req.BloqueProductivo = ObtenerNombreVisibleEtapa(req.EtapaSugerida);
            }

            req.ModoPlanificacion = NormalizarModoPlanificacion(req.ModoPlanificacion);

            if (req.DependeDe == null)
            {
                req.DependeDe = "";
            }
        }

        private string FormatearNumeroDefinibleDesglose(
            RequerimientoProduccionInterna req,
            double valor,
            bool ceroEsNoDefinido
        )
        {
            if (req != null && !req.ParametrosCompletos && (ceroEsNoDefinido || valor <= 0.0))
            {
                return "No definido";
            }

            if (ceroEsNoDefinido && valor <= 0.0)
            {
                return "No definido";
            }

            return valor.ToString("0.##");
        }

        private string FormatearHorasDefiniblesDesglose(double valor)
        {
            if (valor <= 0.0)
            {
                return "";
            }

            return valor.ToString("0.##");
        }

        private string ObtenerNombreTipoProcesoDesglose(TipoProcesoProductivo tipo)
        {
            switch (tipo)
            {
                case TipoProcesoProductivo.RevisionControl:
                    return "Revisión";
                case TipoProcesoProductivo.CorreccionRetrabajo:
                    return "Corrección";
                case TipoProcesoProductivo.Supervision:
                    return "Supervisión";
                case TipoProcesoProductivo.Direccion:
                    return "Dirección";
                case TipoProcesoProductivo.GestionCoordinacion:
                    return "Gestión";
                case TipoProcesoProductivo.EntregaSoporte:
                    return "Entrega";
                case TipoProcesoProductivo.ProduccionDirecta:
                    return "Producción";
                default:
                    return "No clasificado";
            }
        }

        private string FormatearMontoDefinibleDesglose(
            RequerimientoProduccionInterna req,
            double valor
        )
        {
            if (req != null && !req.ParametrosCompletos)
            {
                return "No definido";
            }

            if (valor <= 0.0)
            {
                return "No definido";
            }

            return FormatearValorVisual(valor);
        }

        private string UnirNotaDiagnosticoDesglose(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(req.DiagnosticoParametros))
            {
                return req.Nota ?? "";
            }

            if (string.IsNullOrWhiteSpace(req.Nota))
            {
                return req.DiagnosticoParametros;
            }

            if (req.Nota.Contains(req.DiagnosticoParametros))
            {
                return req.Nota;
            }

            return req.DiagnosticoParametros + " | " + req.Nota;
        }

        private string NormalizarModoPlanificacion(string modo)
        {
            string normalizado = NormalizarClavePlanificacion(modo);

            if (normalizado.Contains("paralelo"))
            {
                return "Paralelo";
            }

            return "Secuencial";
        }

        private bool EsModoPlanificacionParalelo(string modo)
        {
            return NormalizarModoPlanificacion(modo) == "Paralelo";
        }

        private string NormalizarClavePlanificacion(string texto)
        {
            return (texto ?? "")
                .Trim()
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

        private double ObtenerFinDependenciaPlanificacion(
            RequerimientoProduccionInterna req,
            Dictionary<string, double> finPorRequerimiento
        )
        {
            if (req == null ||
                finPorRequerimiento == null ||
                string.IsNullOrWhiteSpace(req.DependeDe))
            {
                return 0.0;
            }

            double finMaximo = 0.0;

            foreach (string dependencia in SepararDependenciasPlanificacion(req.DependeDe))
            {
                string clave = NormalizarClavePlanificacion(dependencia);
                double fin;

                if (finPorRequerimiento.TryGetValue(clave, out fin))
                {
                    finMaximo = Math.Max(finMaximo, fin);
                    continue;
                }

                foreach (KeyValuePair<string, double> par in finPorRequerimiento)
                {
                    if (par.Key.Contains(clave) || clave.Contains(par.Key))
                    {
                        finMaximo = Math.Max(finMaximo, par.Value);
                    }
                }
            }

            return finMaximo;
        }

        private int CalcularNivelDependenciaRequerimientoPlanificacion(
            RequerimientoProduccionInterna req,
            List<RequerimientoProduccionInterna> requerimientos,
            Dictionary<string, int> cache,
            HashSet<string> visitados
        )
        {
            if (req == null || requerimientos == null)
            {
                return 0;
            }

            string claveReq = NormalizarClavePlanificacion(req.NombreRequerimiento);

            if (string.IsNullOrWhiteSpace(claveReq))
            {
                claveReq = NormalizarClavePlanificacion(req.TipoInterno);
            }

            if (string.IsNullOrWhiteSpace(claveReq))
            {
                return 0;
            }

            if (cache.ContainsKey(claveReq))
            {
                return cache[claveReq];
            }

            if (visitados.Contains(claveReq))
            {
                return 0;
            }

            visitados.Add(claveReq);

            int nivelMaximo = 0;

            foreach (string dependencia in SepararDependenciasPlanificacion(req.DependeDe))
            {
                string claveDependencia = NormalizarClavePlanificacion(dependencia);

                RequerimientoProduccionInterna requisito = requerimientos.FirstOrDefault(r =>
                    r != null &&
                    (
                        NormalizarClavePlanificacion(r.NombreRequerimiento) == claveDependencia ||
                        NormalizarClavePlanificacion(r.TipoInterno) == claveDependencia ||
                        NormalizarClavePlanificacion(r.EntregableCliente) == claveDependencia
                    )
                );

                if (requisito == null)
                {
                    continue;
                }

                int nivel = CalcularNivelDependenciaRequerimientoPlanificacion(
                    requisito,
                    requerimientos,
                    cache,
                    new HashSet<string>(visitados)
                );

                nivelMaximo = Math.Max(nivelMaximo, nivel + 1);
            }

            cache[claveReq] = nivelMaximo;
            return nivelMaximo;
        }

        private List<string> SepararDependenciasPlanificacion(string texto)
        {
            return (texto ?? "")
                .Replace(" y ", ";")
                .Split(new[] { ';', '|', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }

        private void RegistrarFinRequerimientoPlanificacion(
            Dictionary<string, double> finPorRequerimiento,
            RequerimientoProduccionInterna req,
            string nombreSubproceso,
            double finSemana
        )
        {
            if (finPorRequerimiento == null || req == null)
            {
                return;
            }

            RegistrarFinPlanificacion(finPorRequerimiento, req.NombreRequerimiento, finSemana);
            RegistrarFinPlanificacion(finPorRequerimiento, nombreSubproceso, finSemana);
            RegistrarFinPlanificacion(finPorRequerimiento, req.TipoInterno, finSemana);
            RegistrarFinPlanificacion(finPorRequerimiento, req.EntregableCliente, finSemana);
        }

        private void RegistrarFinPlanificacion(
            Dictionary<string, double> finPorRequerimiento,
            string clave,
            double finSemana
        )
        {
            string normalizada = NormalizarClavePlanificacion(clave);

            if (string.IsNullOrWhiteSpace(normalizada))
            {
                return;
            }

            double finActual = finPorRequerimiento.ContainsKey(normalizada)
                ? finPorRequerimiento[normalizada]
                : 0.0;

            finPorRequerimiento[normalizada] = Math.Max(finActual, finSemana);
        }

        private List<ProductoEtapaDefinicion> ObtenerPipelineProductoActualParaPlanificacion()
        {
            string nombreProducto = "";

            if (cotizacion != null &&
                cotizacion.BriefProducto != null &&
                !string.IsNullOrWhiteSpace(cotizacion.BriefProducto.TipoProducto))
            {
                nombreProducto = cotizacion.BriefProducto.TipoProducto;
            }

            if (string.IsNullOrWhiteSpace(nombreProducto) && cmbProductoServicio != null)
            {
                nombreProducto = cmbProductoServicio.SelectedItem?.ToString() ?? "";
            }

            Producto2DDefinicion producto = BibliotecaProductos2D.ObtenerProducto(nombreProducto);

            if (producto == null)
            {
                return new List<ProductoEtapaDefinicion>();
            }

            return producto.Etapas ?? new List<ProductoEtapaDefinicion>();
        }

        private ProductoEtapaDefinicion ObtenerEtapaProductoPlanificacion(
            List<ProductoEtapaDefinicion> pipelineProducto,
            string nombreEtapa
        )
        {
            if (pipelineProducto == null || pipelineProducto.Count == 0)
            {
                return null;
            }

            string clave = NormalizarClavePlanificacion(nombreEtapa);

            return pipelineProducto.FirstOrDefault(e =>
                e != null &&
                e.Activa &&
                (
                    NormalizarClavePlanificacion(e.ClaveEtapa) == clave ||
                    NormalizarClavePlanificacion(e.NombreVisible) == clave
                )
            );
        }

        private double ObtenerInicioEtapaProductoPlanificacion(
            ProductoEtapaDefinicion etapaProducto,
            Dictionary<string, double> finPorEtapaProducto
        )
        {
            if (etapaProducto == null ||
                finPorEtapaProducto == null ||
                string.IsNullOrWhiteSpace(etapaProducto.DependeDe))
            {
                return 0.0;
            }

            double finMaximo = 0.0;

            foreach (string dependencia in SepararDependenciasPlanificacion(etapaProducto.DependeDe))
            {
                string claveDependencia = NormalizarClavePlanificacion(dependencia);
                double fin;

                if (finPorEtapaProducto.TryGetValue(claveDependencia, out fin))
                {
                    finMaximo = Math.Max(finMaximo, fin);
                    continue;
                }

                foreach (KeyValuePair<string, double> par in finPorEtapaProducto)
                {
                    if (par.Key.Contains(claveDependencia) ||
                        claveDependencia.Contains(par.Key))
                    {
                        finMaximo = Math.Max(finMaximo, par.Value);
                    }
                }
            }

            return finMaximo;
        }

        private void RegistrarFinEtapaProductoPlanificacion(
            Dictionary<string, double> finPorEtapaProducto,
            ProductoEtapaDefinicion etapaProducto,
            string nombreEtapa,
            double finSemana
        )
        {
            if (finPorEtapaProducto == null)
            {
                return;
            }

            RegistrarFinPlanificacion(finPorEtapaProducto, nombreEtapa, finSemana);

            if (etapaProducto == null)
            {
                return;
            }

            RegistrarFinPlanificacion(finPorEtapaProducto, etapaProducto.ClaveEtapa, finSemana);
            RegistrarFinPlanificacion(finPorEtapaProducto, etapaProducto.NombreVisible, finSemana);
        }

        private void GuardarDesgloseProductivoDesdePantalla()
        {
            if (dgvDesgloseProductivo == null || cotizacion == null)
            {
                return;
            }

            dgvDesgloseProductivo.EndEdit();

            foreach (DataGridViewRow row in dgvDesgloseProductivo.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                RequerimientoProduccionInterna req =
                    row.Tag as RequerimientoProduccionInterna;

                if (req == null)
                {
                    continue;
                }

                double cantidadAnterior = req.Cantidad;
                double diasMinAnterior = req.DiasPersonaMin;
                double diasStdAnterior = req.DiasPersonaStd;
                double diasHolguraAnterior = req.DiasPersonaHolgura;

                double cantidadNueva =
                    ParsearDoubleDesglose(row.Cells["Cantidad"].Value, req.Cantidad);

                double diasMinPantalla =
                    ParsearDoubleDesglose(row.Cells["DiasPersonaMin"].Value, req.DiasPersonaMin);

                double diasStdPantalla =
                    ParsearDoubleDesglose(row.Cells["DiasPersonaStd"].Value, req.DiasPersonaStd);

                double diasHolguraPantalla =
                    ParsearDoubleDesglose(row.Cells["DiasPersonaHolgura"].Value, req.DiasPersonaHolgura);

                double horasMinPantalla =
                    ParsearDoubleDesglose(row.Cells["HorasMinimas"].Value, req.HorasMinimas);

                double horasStdPantalla =
                    ParsearDoubleDesglose(row.Cells["HorasEstandar"].Value, req.HorasEstandar);

                double horasHolguraPantalla =
                    ParsearDoubleDesglose(row.Cells["HorasHolgura"].Value, req.HorasHolgura);

                double rendimientoPantalla =
                    ParsearDoubleDesglose(row.Cells["RendimientoCantidad"].Value, req.RendimientoCantidad);

                string rendimientoPeriodoPantalla =
                    Convert.ToString(row.Cells["RendimientoPeriodo"].Value) ?? req.RendimientoPeriodo;

                req.ModoCalculoProductivo = ModosCalculoProductivo.Normalizar(
                    Convert.ToString(row.Cells["ModoCalculoProductivo"].Value)
                );

                bool cambioCantidad =
                    Math.Abs(cantidadNueva - cantidadAnterior) > 0.0001;

                bool diasMinEditado =
                    Math.Abs(diasMinPantalla - diasMinAnterior) > 0.0001;

                bool diasStdEditado =
                    Math.Abs(diasStdPantalla - diasStdAnterior) > 0.0001;

                bool diasHolguraEditado =
                    Math.Abs(diasHolguraPantalla - diasHolguraAnterior) > 0.0001;

                req.Cantidad = cantidadNueva;
                req.RendimientoCantidad = rendimientoPantalla;
                req.RendimientoPeriodo = string.IsNullOrWhiteSpace(rendimientoPeriodoPantalla)
                    ? "semana"
                    : rendimientoPeriodoPantalla;
                req.RendimientoOrigen = req.RendimientoCantidad > 0.0
                    ? (string.IsNullOrWhiteSpace(req.RendimientoOrigen)
                        ? "Editado en desglose"
                        : req.RendimientoOrigen)
                    : "";

                object calidadValor = row.Cells["Calidad"].Value;
                if (calidadValor != null)
                {
                    req.Calidad = calidadValor.ToString();
                }

                object cargoValor = row.Cells["CargoSugerido"].Value;
                AplicarSeleccionCargoDesglose(req, cargoValor);
                row.Cells["NivelCargoSugerido"].Value = req.NivelCargoSugerido;

                req.BloqueProductivo = Convert.ToString(row.Cells["BloqueProductivo"].Value) ?? "";
                req.ModoPlanificacion = NormalizarModoPlanificacion(
                    Convert.ToString(row.Cells["ModoPlanificacion"].Value)
                );
                req.DependeDe = Convert.ToString(row.Cells["DependeDe"].Value) ?? "";
                AsegurarPlanificacionRequerimiento(req);

                if (cambioCantidad && EsRequerimientoEscalable(req))
                {
                    double factor = CalcularFactorEscalaCantidad(cantidadAnterior, cantidadNueva);

                    req.DiasPersonaMin = diasMinEditado
                        ? diasMinPantalla
                        : diasMinAnterior * factor;

                    req.DiasPersonaStd = diasStdEditado
                        ? diasStdPantalla
                        : diasStdAnterior * factor;

                    req.DiasPersonaHolgura = diasHolguraEditado
                        ? diasHolguraPantalla
                        : diasHolguraAnterior * factor;
                }
                else
                {
                    req.DiasPersonaMin = diasMinPantalla;
                    req.DiasPersonaStd = diasStdPantalla;
                    req.DiasPersonaHolgura = diasHolguraPantalla;
                }

                NormalizarDiasRequerimiento(req);

                if (ModosCalculoProductivo.EsTiempoAsignado(req.ModoCalculoProductivo))
                {
                    req.HorasMinimas = horasMinPantalla;
                    req.HorasEstandar = horasStdPantalla;
                    req.HorasHolgura = horasHolguraPantalla;

                    if (req.HorasEstandar <= 0.0)
                    {
                        CalculoProductivoResolverService.SincronizarHorasDesdeDias(req);
                    }

                    CalculoProductivoResolverService.AplicarTiempoAsignado(req);
                    RecalcularCostoRequerimientoDesdeCargo(req);
                }
                else
                {
                    RecalcularCostoRequerimientoDesdeCargo(req);
                    RecalcularRequerimientoDesdeRendimientoSiCorresponde(req);
                }

                req.EditadoManualmente = true;
            }

            BibliotecaSubEtapasService.SincronizarDesdeDesgloseProductivo(
                cotizacion,
                bibliotecaSubEtapas,
                false
            );

            if (EsContextoDesgloseProyectoGlobal())
            {
                ProyectoDesgloseGlobalService.Aplicar(
                    proyectoCotizacionActual,
                    cotizacion.DesgloseProductivo,
                    cotizacion);
                cotizacion.ProyectoProductivo = proyectoCotizacionActual;
                MarcarProyectoConCambiosPendientes();
            }
        }

        private bool EsContextoDesgloseProyectoGlobal()
        {
            return modoAplicacionActual == ModoAplicacion.Proyecto &&
                proyectoCotizacionActual != null &&
                itemProyectoEnEdicionActual == null;
        }

        private string ObtenerNombreProductoDesgloseGlobal(string itemId)
        {
            if (proyectoCotizacionActual == null ||
                string.IsNullOrWhiteSpace(itemId))
            {
                return itemProyectoEnEdicionActual?.Nombre ?? "";
            }

            return proyectoCotizacionActual.Grupos
                .Where(g => g != null)
                .SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .FirstOrDefault(i => i != null &&
                    string.Equals(
                        i.Id,
                        itemId,
                        StringComparison.OrdinalIgnoreCase))
                ?.Nombre ?? "";
        }



        private void RecalcularCostoRequerimientoDesdeCargo(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return;
            }

            CategoriaTrabajador cargo = ObtenerCargoParaRequerimientoDesglose(req);

            if (cargo == null)
            {
                req.SueldoMensualCargoCLP = 0.0;
                req.TarifaDiaCargoCLP = 0.0;
                req.CostoMinimoCLP = 0.0;
                req.CostoEstandarCLP = 0.0;
                req.CostoHolguraCLP = 0.0;
                return;
            }

            req.SueldoMensualCargoCLP = cargo.SueldoMensualCLPTipico;
            req.TarifaDiaCargoCLP = cargo.SueldoMensualCLPTipico / 22.0;

            req.CostoMinimoCLP = req.DiasPersonaMin * req.TarifaDiaCargoCLP;
            req.CostoEstandarCLP = req.DiasPersonaStd * req.TarifaDiaCargoCLP;
            req.CostoHolguraCLP = req.DiasPersonaHolgura * req.TarifaDiaCargoCLP;
            CalculoProductivoResolverService.SincronizarHorasDesdeDias(req);
        }

        private void RecalcularRequerimientoDesdeRendimientoSiCorresponde(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null)
            {
                return;
            }

            double diasHabilesSemana = cotizacion != null &&
                cotizacion.DiasHabilesEstudioPorSemana > 0.0
                    ? cotizacion.DiasHabilesEstudioPorSemana
                    : 5.0;

            if (!ModosCalculoProductivo.EsTiempoAsignado(req.ModoCalculoProductivo) &&
                req.RendimientoCantidad <= 0.0)
            {
                return;
            }

            CalculoProductivoResolverService.Aplicar(
                req,
                diasHabilesSemana
            );
        }

        private CategoriaTrabajador ObtenerCargoParaRequerimientoDesglose(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null)
            {
                return null;
            }

            InicializarCargosGenerales();

            string nombre = NormalizarEtapaDesglose(req.CargoSugerido);
            string nivel = NormalizarEtapaDesglose(req.NivelCargoSugerido);

            if (bibliotecaCargosGenerales != null)
            {
                CategoriaTrabajador exacto = bibliotecaCargosGenerales.FirstOrDefault(c =>
                    c != null &&
                    NormalizarEtapaDesglose(c.Nombre) == nombre &&
                    NormalizarEtapaDesglose(c.Nivel) == nivel);

                if (exacto != null)
                {
                    return exacto;
                }

                CategoriaTrabajador porNombre = bibliotecaCargosGenerales.FirstOrDefault(c =>
                    c != null &&
                    NormalizarEtapaDesglose(c.Nombre) == nombre);

                if (porNombre != null)
                {
                    req.NivelCargoSugerido = porNombre.Nivel;
                    return porNombre;
                }
            }

            return BibliotecaCargosProductivos2D.BuscarCargo(
                req.CargoSugerido,
                req.EtapaSugerida,
                req.NivelCargoSugerido
            );
        }


        private bool EsRequerimientoEscalable(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return false;
            }

            string texto = NormalizarEtapaDesglose(
                req.TipoInterno + " " +
                req.NombreRequerimiento + " " +
                req.Unidad
            );

            if (texto.Contains("plano") ||
                texto.Contains("storyboard") ||
                texto.Contains("animatic") ||
                texto.Contains("background") ||
                texto.Contains("fondo") ||
                texto.Contains("personaje") ||
                texto.Contains("prop") ||
                texto.Contains("objeto") ||
                texto.Contains("animacion") ||
                texto.Contains("segundos") ||
                texto.Contains("composicion") ||
                texto.Contains("revision") ||
                texto.Contains("export") ||
                texto.Contains("entrega"))
            {
                return true;
            }

            return false;
        }

        private double CalcularFactorEscalaCantidad(double cantidadAnterior, double cantidadNueva)
        {
            if (cantidadAnterior <= 0.0 && cantidadNueva <= 0.0)
            {
                return 1.0;
            }

            if (cantidadAnterior <= 0.0)
            {
                return cantidadNueva <= 0.0 ? 1.0 : cantidadNueva;
            }

            double factor = cantidadNueva / cantidadAnterior;

            if (factor < 0.0)
            {
                factor = 0.0;
            }

            return factor;
        }

        private void NormalizarDiasRequerimiento(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return;
            }

            if (req.Cantidad < 0.0)
            {
                req.Cantidad = 0.0;
            }

            if (req.DiasPersonaMin < 0.0)
            {
                req.DiasPersonaMin = 0.0;
            }

            if (req.DiasPersonaStd < 0.0)
            {
                req.DiasPersonaStd = 0.0;
            }

            if (req.DiasPersonaHolgura < 0.0)
            {
                req.DiasPersonaHolgura = 0.0;
            }

            if (req.DiasPersonaStd < req.DiasPersonaMin)
            {
                req.DiasPersonaStd = req.DiasPersonaMin;
            }

            if (req.DiasPersonaHolgura < req.DiasPersonaStd)
            {
                req.DiasPersonaHolgura = req.DiasPersonaStd;
            }
        }


        private void RecalcularTotalesDesgloseProductivoDesdeFilas()
        {
            if (cotizacion == null || cotizacion.DesgloseProductivo == null)
            {
                return;
            }

            DesgloseProductivoProyecto d = cotizacion.DesgloseProductivo;

            d.DiasPersonaMinimos = 0.0;
            d.DiasPersonaEstandar = 0.0;
            d.DiasPersonaHolgura = 0.0;

            d.CostoMinimoCLP = 0.0;
            d.CostoEstandarCLP = 0.0;
            d.CostoHolguraCLP = 0.0;
            d.HorasGestionEstandar = 0.0;
            d.CostoGestionEstandarCLP = 0.0;

            if (d.Requerimientos == null)
            {
                return;
            }

            DesgloseProductivoService.ValidarParametrosDesglose(d);

            foreach (RequerimientoProduccionInterna req in d.Requerimientos)
            {
                if (req == null || !req.ParametrosCompletos)
                {
                    continue;
                }

                d.DiasPersonaMinimos += req.DiasPersonaMin;
                d.DiasPersonaEstandar += req.DiasPersonaStd;
                d.DiasPersonaHolgura += req.DiasPersonaHolgura;

                d.CostoMinimoCLP += req.CostoMinimoCLP;
                d.CostoEstandarCLP += req.CostoEstandarCLP;
                d.CostoHolguraCLP += req.CostoHolguraCLP;
            }

            DesgloseProductivoService.AplicarGestionProductivaDerivada(d);

            double capacidadBasePersonas = ObtenerPersonasPlanificadasDesdeManoObra();
            double diasHabilesPorSemana = cotizacion.DiasHabilesEstudioPorSemana > 0.0
                ? cotizacion.DiasHabilesEstudioPorSemana
                : 5.0;

            d.SemanasMinimas = d.DiasPersonaMinimos / (diasHabilesPorSemana * capacidadBasePersonas);
            d.SemanasEstandar = d.DiasPersonaEstandar / (diasHabilesPorSemana * capacidadBasePersonas);
            d.SemanasHolgura = d.DiasPersonaHolgura / (diasHabilesPorSemana * capacidadBasePersonas);

            double plazoCliente = cotizacion.PlazoClienteSemanas;

            if (d.Requerimientos.Any(r => r != null && !r.ParametrosCompletos))
            {
                DesgloseProductivoService.ValidarParametrosDesglose(d);
                cotizacion.EvaluacionPlazo.DiagnosticoPlazo = d.Diagnostico;
                cotizacion.DiagnosticoPlazo = d.Diagnostico;
                return;
            }

            if (plazoCliente <= 0.0)
            {
                d.Diagnostico =
                    "Sin fecha objetivo declarada. Se entrega desglose productivo sin comparación de plazo.";
            }
            else if (plazoCliente < d.SemanasMinimas)
            {
                d.Diagnostico =
                    "INVIABLE: el plazo declarado está bajo el mínimo productivo estimado.";
            }
            else if (plazoCliente < d.SemanasEstandar)
            {
                d.Diagnostico =
                    "PLAZO AGRESIVO: requiere reducir alcance, bajar calidad o aumentar equipo.";
            }
            else if (plazoCliente < d.SemanasHolgura)
            {
                d.Diagnostico =
                    "PLAZO VIABLE AJUSTADO: entra en rango, pero con poca holgura.";
            }
            else
            {
                d.Diagnostico =
                    "PLAZO VIABLE: el plazo declarado permite una planificación preliminar con holgura.";
            }

            cotizacion.EvaluacionPlazo.SemanasMinimasEstimadas = d.SemanasMinimas;
            cotizacion.EvaluacionPlazo.SemanasEstandarEstimadas = d.SemanasEstandar;
            cotizacion.EvaluacionPlazo.SemanasConHolguraEstimadas = d.SemanasHolgura;
            cotizacion.EvaluacionPlazo.DiagnosticoPlazo = d.Diagnostico;

            cotizacion.DuracionMinimaTecnicaSemanas = d.SemanasMinimas;
            cotizacion.DuracionEstandarTecnicaSemanas = d.SemanasEstandar;
            cotizacion.DuracionHolguraTecnicaSemanas = d.SemanasHolgura;
            cotizacion.DiagnosticoPlazo = d.Diagnostico;
        }

        private string NormalizarEtapaDesglose(string texto)
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

        private int ObtenerOrdenTipoInternoDesglose(string tipo)
        {
            string t = NormalizarEtapaDesglose(tipo);

            if (t.Contains("desarrollo") || t.Contains("guion") || t.Contains("idea"))
            {
                return 10;
            }

            if (t.Contains("plano") || t.Contains("storyboard"))
            {
                return 20;
            }

            if (t.Contains("animatic") || t.Contains("previsualizacion"))
            {
                return 30;
            }

            if (t.Contains("personaje") || t.Contains("character"))
            {
                return 40;
            }

            if (t.Contains("background") || t.Contains("fondo") || t.Contains("escenario"))
            {
                return 50;
            }

            if (t.Contains("prop") || t.Contains("objeto"))
            {
                return 60;
            }

            if (t.Contains("animacion") || t.Contains("loop"))
            {
                return 70;
            }

            if (t.Contains("composicion") || t.Contains("integracion"))
            {
                return 80;
            }

            if (t.Contains("revision") || t.Contains("correccion"))
            {
                return 90;
            }

            if (t.Contains("export") || t.Contains("entrega") || t.Contains("render"))
            {
                return 100;
            }

            return 999;
        }




        private void RefrescarResumenDesgloseProductivo()
        {
            if (panelResumenVisualDesglose == null)
            {
                return;
            }

            panelResumenVisualDesglose.Visible = true;
            panelResumenVisualDesglose.BringToFront();
            panelResumenVisualDesglose.AutoScrollPosition = Point.Empty;
            panelResumenVisualDesglose.Controls.Clear();

            if (cotizacion == null)
            {
                panelResumenVisualDesglose.Controls.Add(CrearPanelResumenVisualVacio());
                return;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                panelResumenVisualDesglose.Controls.Add(CrearPanelResumenVisualVacio());
                return;
            }

            DesgloseProductivoProyecto d = cotizacion.DesgloseProductivo;

            ResultadoEscenarioOfertaDesglose oferta =
                EscenarioOfertaDesdeDesgloseService.Calcular(cotizacion);

            EscenarioPlanificacionDesglose escenarioRealActivo = escenarioActivoDesglose;

            if (escenarioRealActivo == EscenarioPlanificacionDesglose.Recomendado)
            {
                escenarioRealActivo = oferta.EscenarioRecomendado;
            }

            ResultadoCapacidadProyecto capacidadMinima =
                PlanCapacidadDesdeDesgloseService.Calcular(
                    cotizacion,
                    ModoDuracionDesglose.Minimo
                );

            ResultadoCapacidadProyecto capacidadEstandar =
                PlanCapacidadDesdeDesgloseService.Calcular(
                    cotizacion,
                    ModoDuracionDesglose.Estandar
                );

            ResultadoCapacidadProyecto capacidadHolgada =
                PlanCapacidadDesdeDesgloseService.Calcular(
                    cotizacion,
                    ModoDuracionDesglose.Holgura
                );

            ResultadoCapacidadProyecto capacidadActiva =
                ObtenerCapacidadPorEscenario(
                    escenarioRealActivo,
                    capacidadMinima,
                    capacidadEstandar,
                    capacidadHolgada
                );

            double diasActivos = ObtenerDiasEscenario(d, escenarioRealActivo);
            double semanasActivas = ObtenerSemanasEscenario(d, escenarioRealActivo);
            double costoActivo = ObtenerCostoEscenario(d, escenarioRealActivo);
            double precioActivo = ObtenerPrecioEscenario(oferta, escenarioRealActivo);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Top;
            root.AutoSize = true;
            root.ColumnCount = 1;
            root.RowCount = 1;
            root.BackColor = Color.FromArgb(248, 249, 251);
            root.Padding = new Padding(0);
            root.Margin = new Padding(0);

            Label titulo = CrearLabelSimple("Resumen visual de rango productivo y comercial", 13, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(30, 30, 30);
            titulo.Margin = new Padding(0, 0, 0, 10);
            root.Controls.Add(titulo);
            root.Controls.Add(CrearPanelCapasProductivas(d));

            TableLayoutPanel cards = new TableLayoutPanel();
            cards.Dock = DockStyle.Top;
            cards.AutoSize = false;
            cards.Height = 590;
            cards.ColumnCount = 2;
            cards.RowCount = 2;
            cards.Margin = new Padding(0, 0, 0, 16);

            cards.ColumnStyles.Clear();
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            cards.RowStyles.Clear();
            cards.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            cards.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));

            cards.Controls.Add(
    CrearTarjetaRangoDobleReferencia(
        "Plazo técnico",
        "Semanas",
        d.SemanasMinimas,
        d.SemanasEstandar,
        d.SemanasHolgura,
        semanasActivas,
        "Escenario activo",
        cotizacion.PlazoClienteSemanas,
        "Plazo cliente",
        "sem",
        Color.FromArgb(83, 192, 166)
    ),
    0,
    0
);

            cards.Controls.Add(
    CrearTarjetaRangoDobleReferencia(
        "Costo interno",
        ObtenerMonedaVisualActual(),
        d.CostoMinimoCLP,
        d.CostoEstandarCLP,
        d.CostoHolguraCLP,
        costoActivo,
        "Costo activo",
        0.0,
        "Sin referencia cliente",
        "MONEDA",
        Color.FromArgb(255, 193, 7)
    ),
    1,
    0
);

            cards.Controls.Add(
    CrearTarjetaRangoDobleReferencia(
           "Precio ofertable",
           ObtenerMonedaVisualActual() + " con margen 15%",
           oferta.PrecioMinimoCLP,
           oferta.PrecioEstandarCLP,
           oferta.PrecioHolgadoCLP,
           precioActivo,
           "Precio activo",
           oferta.PresupuestoClienteCLP,
           "Presupuesto cliente",
           "MONEDA",
           Color.FromArgb(238, 30, 91)
       ),
       0,
       1
   );

            cards.Controls.Add(CrearTarjetaRangoVacia(), 1, 1);

            root.Controls.Add(cards);

            TableLayoutPanel bloqueInferior = new TableLayoutPanel();
            bloqueInferior.Dock = DockStyle.Top;
            bloqueInferior.AutoSize = true;
            bloqueInferior.ColumnCount = 2;
            bloqueInferior.RowCount = 1;
            bloqueInferior.Margin = new Padding(0, 0, 0, 0);

            bloqueInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            bloqueInferior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            Panel panelDecision = CrearPanelCard(Color.FromArgb(255, 255, 255));
            panelDecision.Controls.Add(
                CrearTituloCard("Decisión comercial", Color.FromArgb(238, 30, 91))
            );

            panelDecision.Controls.Add(
                CrearLineaDato("Presupuesto cliente", FormatearValorVisual(oferta.PresupuestoClienteCLP))
            );

            panelDecision.Controls.Add(
                CrearLineaDato("Escenario recomendado", ObtenerNombreEscenarioVisible(oferta.EscenarioRecomendado))
            );

            panelDecision.Controls.Add(
                CrearLineaDato("Escenario activo", ObtenerNombreEscenarioVisible(escenarioRealActivo))
            );

            panelDecision.Controls.Add(
                CrearLineaDato("Precio activo sugerido", FormatearValorVisual(precioActivo))
            );

            panelDecision.Controls.Add(
                CrearLineaDato(
                    "Gestión global",
                    FormatearValorVisual(d.CostoGestionEstandarCLP) +
                    " / " +
                    d.HorasGestionEstandar.ToString("0.##") +
                    " h"
                )
            );

            if (d.GestionesCalculadas != null && d.GestionesCalculadas.Count > 0)
            {
                foreach (GestionProductivaCalculada gestion in d.GestionesCalculadas.Take(5))
                {
                    panelDecision.Controls.Add(
                        CrearLineaDato(
                            gestion.Area,
                            gestion.HorasEstandar.ToString("0.##") +
                            " h / " +
                            FormatearValorVisual(gestion.CostoEstandarCLP)
                        )
                    );
                }
            }

            panelDecision.Controls.Add(
                CrearTextoDiagnostico(oferta.Diagnostico)
            );

            Panel panelCapacidad = CrearPanelCard(Color.FromArgb(255, 255, 255));
            panelCapacidad.Controls.Add(
                CrearTituloCard("Capacidad según plazo cliente", Color.FromArgb(33, 150, 243))
            );

            panelCapacidad.Controls.Add(
                CrearLineaDato("Plazo cliente", capacidadActiva.PlazoClienteSemanas.ToString("0.##") + " semanas")
            );

            panelCapacidad.Controls.Add(
                CrearLineaDato("Días disponibles", capacidadActiva.DiasCalendarioDisponibles.ToString("0.##") + " días")
            );

            panelCapacidad.Controls.Add(
                CrearLineaDato("Factor presión activo", capacidadActiva.FactorPresionGlobal.ToString("0.##") + "x")
            );

            panelCapacidad.Controls.Add(
                CrearLineaDato("Personas equivalentes", capacidadActiva.PersonasEquivalentesGlobales.ToString())
            );

            panelCapacidad.Controls.Add(
                CrearTextoDiagnostico(capacidadActiva.Diagnostico)
            );

            bloqueInferior.Controls.Add(panelDecision, 0, 0);
            bloqueInferior.Controls.Add(panelCapacidad, 1, 0);

            root.Controls.Add(bloqueInferior);

            Panel panelEtapas = CrearPanelCard(Color.FromArgb(255, 255, 255));
            panelEtapas.Margin = new Padding(0, 12, 0, 0);
            panelEtapas.Controls.Add(
                CrearTituloCard("Capacidad por etapa en escenario activo", Color.FromArgb(83, 192, 166))
            );

            foreach (ResultadoCapacidadEtapa etapa in capacidadActiva.Etapas)
            {
                panelEtapas.Controls.Add(
                    CrearBloqueEtapaCapacidad(etapa)
                );
            }

            root.Controls.Add(panelEtapas);

            Panel panelLinealidad = CrearPanelLinealidadDesglose(d);
            panelLinealidad.Margin = new Padding(0, 12, 0, 0);
            root.Controls.Add(panelLinealidad);

            panelResumenVisualDesglose.Controls.Add(root);
            panelResumenVisualDesglose.AutoScrollPosition = Point.Empty;
        }

        private Panel CrearPanelLinealidadDesglose(DesgloseProductivoProyecto desglose)
        {
            Panel panel = CrearPanelCard(Color.FromArgb(255, 255, 255));
            panel.Controls.Add(
                CrearTituloCard("Linealidad y paralelismo productivo", Color.FromArgb(122, 92, 180))
            );

            if (desglose == null ||
                desglose.Requerimientos == null ||
                desglose.Requerimientos.Count == 0)
            {
                panel.Controls.Add(CrearTextoDiagnostico("Sin requerimientos internos para ordenar."));
                return panel;
            }

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Top;
            layout.AutoSize = true;
            layout.ColumnCount = 1;
            layout.RowCount = 1;
            layout.Margin = new Padding(0, 8, 0, 0);

            var etapas = desglose.Requerimientos
                .Where(r => r != null)
                .Select(r =>
                {
                    AsegurarPlanificacionRequerimiento(r);
                    return r;
                })
                .GroupBy(r => ObtenerNombreVisibleEtapa(r.EtapaSugerida))
                .OrderBy(g => ObtenerOrdenEtapaGeneral(g.Key))
                .ToList();

            foreach (var etapa in etapas)
            {
                Label lblEtapa = CrearLabelSimple(etapa.Key, 9.6f, FontStyle.Bold);
                lblEtapa.ForeColor = ObtenerColorBordeEtapa(etapa.Key);
                lblEtapa.Margin = new Padding(0, 8, 0, 2);
                layout.Controls.Add(lblEtapa);

                var bloques = etapa
                    .GroupBy(r => r.BloqueProductivo)
                    .OrderBy(g => g.Min(r => ObtenerOrdenTipoInternoDesglose(r.TipoInterno)))
                    .ThenBy(g => g.Key)
                    .ToList();

                int indiceBloque = 1;

                foreach (var bloque in bloques)
                {
                    string resumen = ConstruirResumenBloquePlanificacion(
                        indiceBloque,
                        bloque.Key,
                        bloque.ToList()
                    );

                    Label lblBloque = CrearLabelSimple(resumen, 8.8f, FontStyle.Regular);
                    lblBloque.ForeColor = Color.FromArgb(45, 45, 45);
                    lblBloque.BackColor = MezclarConBlanco(ObtenerColorBaseEtapa(etapa.Key), 0.86);
                    lblBloque.Padding = new Padding(8, 5, 8, 5);
                    lblBloque.Margin = new Padding(0, 2, 0, 4);
                    lblBloque.Dock = DockStyle.Top;
                    layout.Controls.Add(lblBloque);

                    indiceBloque++;
                }
            }

            panel.Controls.Add(layout);
            return panel;
        }

        private string ConstruirResumenBloquePlanificacion(
            int indiceBloque,
            string nombreBloque,
            List<RequerimientoProduccionInterna> requerimientos
        )
        {
            int total = requerimientos == null ? 0 : requerimientos.Count;
            int paralelos = requerimientos == null
                ? 0
                : requerimientos.Count(r => EsModoPlanificacionParalelo(r.ModoPlanificacion));
            int dependientes = requerimientos == null
                ? 0
                : requerimientos.Count(r => !string.IsNullOrWhiteSpace(r.DependeDe));

            string modo = paralelos > 0
                ? "mixto / paralelo"
                : "secuencial";

            string detalle = "Bloque " + indiceBloque + " · " +
                (string.IsNullOrWhiteSpace(nombreBloque) ? "Sin nombre" : nombreBloque) +
                " · " + modo +
                " · " + total.ToString() + " pegas";

            if (dependientes > 0)
            {
                detalle += " · " + dependientes.ToString() + " con dependencia explícita";
            }

            return detalle;
        }

        private Label CrearLabelSimple(string texto, float size, FontStyle estilo)
        {
            Label lbl = new Label();
            lbl.Text = texto;
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", size, estilo);
            lbl.ForeColor = Color.FromArgb(40, 40, 40);
            lbl.Margin = new Padding(0, 0, 0, 6);
            return lbl;
        }

        private Panel CrearPanelCard(Color backColor)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.BackColor = backColor;
            panel.Padding = new Padding(12);
            panel.Margin = new Padding(0, 0, 10, 0);
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
        }

        private Label CrearTituloCard(string texto, Color color)
        {
            Label lbl = new Label();
            lbl.Text = texto;
            lbl.Dock = DockStyle.Top;
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lbl.ForeColor = color;
            lbl.Margin = new Padding(0, 0, 0, 8);
            return lbl;
        }

        private Control CrearLineaDato(string nombre, string valor)
        {
            TableLayoutPanel fila = new TableLayoutPanel();
            fila.Dock = DockStyle.Top;
            fila.AutoSize = true;
            fila.ColumnCount = 2;
            fila.Margin = new Padding(0, 1, 0, 1);

            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

            Label lblNombre = new Label();
            lblNombre.Text = nombre;
            lblNombre.Dock = DockStyle.Fill;
            lblNombre.AutoSize = true;
            lblNombre.Font = new Font("Segoe UI", 9.2f, FontStyle.Regular);
            lblNombre.ForeColor = Color.FromArgb(95, 95, 95);

            Label lblValor = new Label();
            lblValor.Text = valor;
            lblValor.Dock = DockStyle.Fill;
            lblValor.AutoSize = true;
            lblValor.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            lblValor.ForeColor = Color.FromArgb(30, 30, 30);

            fila.Controls.Add(lblNombre, 0, 0);
            fila.Controls.Add(lblValor, 1, 0);

            return fila;
        }

        private Label CrearTextoDiagnostico(string texto)
        {
            Label lbl = new Label();
            lbl.Text = texto;
            lbl.Dock = DockStyle.Top;
            lbl.AutoSize = true;
            lbl.MaximumSize = new Size(520, 0);
            lbl.Font = new Font("Segoe UI", 9.0f, FontStyle.Italic);
            lbl.ForeColor = Color.FromArgb(70, 70, 70);
            lbl.Margin = new Padding(0, 8, 0, 0);
            return lbl;
        }

        private Panel CrearTarjetaRango(
    string titulo,
    string subtitulo,
    double minimo,
    double estandar,
    double holgado,
    double referencia,
    string unidad,
    string etiquetaReferencia,
    Color color
)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Padding = new Padding(0);
            card.Margin = new Padding(0, 0, 10, 0);
            card.BorderStyle = BorderStyle.FixedSingle;
            card.MinimumSize = new Size(260, 175);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 7;
            layout.BackColor = Color.White;
            layout.Padding = new Padding(12, 10, 12, 10);

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 5));       // franja
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // título
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // subtítulo
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));      // barra
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // métricas
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // referencia
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // leyenda

            Panel franja = new Panel();
            franja.Dock = DockStyle.Fill;
            franja.BackColor = color;
            franja.Margin = new Padding(0, 0, 0, 8);

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 11.0f, FontStyle.Bold);
            lblTitulo.ForeColor = color;
            lblTitulo.Margin = new Padding(0, 8, 0, 0);

            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = subtitulo;
            lblSubtitulo.Dock = DockStyle.Fill;
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = new Font("Segoe UI", 8.7f, FontStyle.Regular);
            lblSubtitulo.ForeColor = Color.FromArgb(105, 105, 105);
            lblSubtitulo.Margin = new Padding(0, 0, 0, 4);

            Panel barra = CrearBarraRangoVisual(minimo, estandar, holgado, referencia, color);
            barra.Dock = DockStyle.Fill;
            barra.Margin = new Padding(0, 4, 0, 2);

            TableLayoutPanel metricas = new TableLayoutPanel();
            metricas.Dock = DockStyle.Top;
            metricas.AutoSize = true;
            metricas.ColumnCount = 3;
            metricas.RowCount = 2;
            metricas.Margin = new Padding(0, 4, 0, 4);

            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            metricas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

            metricas.Controls.Add(CrearMiniMetrica("Mínimo", FormatearValorRango(minimo, unidad), Color.FromArgb(85, 85, 85)), 0, 0);
            metricas.Controls.Add(CrearMiniMetrica("Estándar", FormatearValorRango(estandar, unidad), Color.FromArgb(35, 35, 35)), 1, 0);
            metricas.Controls.Add(CrearMiniMetrica("Holgado", FormatearValorRango(holgado, unidad), Color.FromArgb(85, 85, 85)), 2, 0);

            Label lblReferencia = new Label();
            lblReferencia.Dock = DockStyle.Top;
            lblReferencia.AutoSize = true;
            lblReferencia.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            lblReferencia.ForeColor = color;
            lblReferencia.Margin = new Padding(0, 4, 0, 2);
            lblReferencia.Text =
                etiquetaReferencia + ": " +
                FormatearValorRango(referencia, unidad);

            if (referencia <= 0.0)
            {
                lblReferencia.ForeColor = Color.FromArgb(170, 70, 70);
                lblReferencia.Text = etiquetaReferencia + ": no informado";
            }

            Label leyenda = new Label();
            leyenda.Dock = DockStyle.Top;
            leyenda.AutoSize = true;
            leyenda.Font = new Font("Segoe UI", 7.8f, FontStyle.Regular);
            leyenda.ForeColor = Color.FromArgb(120, 120, 120);
            leyenda.Margin = new Padding(0, 2, 0, 0);
            leyenda.Text = "● mín.   ▲ estándar   ■ holgado   ◆ referencia";

            layout.Controls.Add(franja, 0, 0);
            layout.Controls.Add(lblTitulo, 0, 1);
            layout.Controls.Add(lblSubtitulo, 0, 2);
            layout.Controls.Add(barra, 0, 3);
            layout.Controls.Add(metricas, 0, 4);
            layout.Controls.Add(lblReferencia, 0, 5);
            layout.Controls.Add(leyenda, 0, 6);

            card.Controls.Add(layout);

            return card;
        }

        private Panel CrearMiniMetrica(string titulo, string valor, Color color)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoSize = true;
            panel.Padding = new Padding(2);
            panel.Margin = new Padding(0);

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Dock = DockStyle.Top;
            lblTitulo.AutoSize = true;
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            lblTitulo.Font = new Font("Segoe UI", 7.8f, FontStyle.Regular);
            lblTitulo.ForeColor = Color.FromArgb(110, 110, 110);

            Label lblValor = new Label();
            lblValor.Text = valor;
            lblValor.Dock = DockStyle.Top;
            lblValor.AutoSize = true;
            lblValor.TextAlign = ContentAlignment.MiddleCenter;
            lblValor.Font = new Font("Segoe UI", 8.7f, FontStyle.Bold);
            lblValor.ForeColor = color;

            panel.Controls.Add(lblValor);
            panel.Controls.Add(lblTitulo);

            return panel;
        }

        private Panel CrearBarraRangoVisual(
            double minimo,
            double estandar,
            double holgado,
            double referencia,
            Color colorReferencia
        )
        {
            Panel panel = new Panel();
            panel.Height = 46;
            panel.BackColor = Color.White;

            panel.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int left = 18;
                int right = panel.Width - 18;
                int y = 22;

                if (right <= left)
                {
                    return;
                }

                Pen lineaBase = new Pen(Color.FromArgb(210, 210, 210), 5);
                g.DrawLine(lineaBase, left, y, right, y);

                int xMin = left;
                int xEst = CalcularXBarra(estandar, minimo, holgado, left, right);
                int xHol = right;
                int xRef = CalcularXBarra(referencia, minimo, holgado, left, right);

                Pen lineaActiva = new Pen(colorReferencia, 5);
                g.DrawLine(lineaActiva, left, y, xRef, y);

                Brush brushMin = new SolidBrush(Color.FromArgb(80, 80, 80));
                Brush brushEst = new SolidBrush(Color.FromArgb(40, 40, 40));
                Brush brushHol = new SolidBrush(Color.FromArgb(80, 80, 80));
                Brush brushRef = new SolidBrush(colorReferencia);

                g.FillEllipse(brushMin, xMin - 5, y - 5, 10, 10);

                Point[] triangulo =
                {
            new Point(xEst, y - 8),
            new Point(xEst - 7, y + 6),
            new Point(xEst + 7, y + 6)
        };
                g.FillPolygon(brushEst, triangulo);

                g.FillRectangle(brushHol, xHol - 5, y - 5, 10, 10);

                Point[] diamante =
                {
            new Point(xRef, y - 9),
            new Point(xRef - 9, y),
            new Point(xRef, y + 9),
            new Point(xRef + 9, y)
        };
                g.FillPolygon(brushRef, diamante);
            };

            return panel;
        }

        private int CalcularXBarra(
            double valor,
            double minimo,
            double maximo,
            int left,
            int right
        )
        {
            if (maximo <= minimo)
            {
                return left;
            }

            if (valor <= minimo)
            {
                return left;
            }

            if (valor >= maximo)
            {
                return right;
            }

            double fraccion = (valor - minimo) / (maximo - minimo);

            return left + (int)Math.Round(fraccion * (right - left));
        }

        private string FormatearValorRango(double valor, string unidad)
        {
            if (unidad == "CLP")
            {
                return FormatearCLP(valor);
            }

            if (unidad == "MONEDA")
            {
                return FormatearValorVisual(valor);
            }

            return valor.ToString("0.##") + " " + unidad;
        }

        private Panel CrearBloqueEtapaCapacidad(ResultadoCapacidadEtapa etapa)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.BackColor = Color.FromArgb(250, 250, 250);
            panel.Padding = new Padding(10);
            panel.Margin = new Padding(0, 4, 0, 6);
            panel.BorderStyle = BorderStyle.FixedSingle;

            Color colorEtapa = ObtenerColorBaseEtapa(etapa.Etapa);

            Label titulo = CrearTituloCard(etapa.Etapa, colorEtapa);
            panel.Controls.Add(titulo);

            panel.Controls.Add(
                CrearLineaDato(
                    "Carga",
                    etapa.DiasPersonaEtapa.ToString("0.##") + " días-persona"
                )
            );

            panel.Controls.Add(
                CrearLineaDato(
                    "Semanas asignadas",
                    etapa.SemanasCalendarioAsignadas.ToString("0.##")
                )
            );

            panel.Controls.Add(
                CrearLineaDato(
                    "Personas equivalentes",
                    etapa.PersonasMinimasEtapa.ToString()
                )
            );

            panel.Controls.Add(
                CrearTextoDiagnostico(etapa.Diagnostico)
            );

            foreach (ResultadoCapacidadCargo cargo in etapa.Cargos)
            {
                if (cargo.PersonasExtra > 0 || cargo.FactorPresionCargo > 1.0)
                {
                    Label lblCargo = CrearLabelSimple(
                        "• " + cargo.Cargo +
                        " | " + cargo.DiasPersonaCargo.ToString("0.##") + " días" +
                        " | presión " + cargo.FactorPresionCargo.ToString("0.##") +
                        " | sugerido " + cargo.PersonasSugeridas.ToString() + " pers.",
                        8.8f,
                        FontStyle.Regular
                    );

                    lblCargo.ForeColor = Color.FromArgb(70, 70, 70);
                    lblCargo.Margin = new Padding(0, 4, 0, 0);
                    panel.Controls.Add(lblCargo);
                }
            }

            return panel;
        }

        private Control CrearPanelCapasProductivas(DesgloseProductivoProyecto d)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = false;
            panel.Height = 104;
            panel.WrapContents = false;
            panel.AutoScroll = true;
            panel.Margin = new Padding(0, 0, 0, 12);
            panel.BackColor = Color.FromArgb(248, 249, 251);

            panel.Controls.Add(CrearChipCapaProductiva("Producción", d.HorasProduccionDirecta, d.CostoProduccionDirectaCLP));
            panel.Controls.Add(CrearChipCapaProductiva("Revisión", d.HorasRevisionControl, d.CostoRevisionControlCLP));
            panel.Controls.Add(CrearChipCapaProductiva("Corrección", d.HorasCorreccionRetrabajo, d.CostoCorreccionRetrabajoCLP));
            panel.Controls.Add(CrearChipCapaProductiva("Dirección", d.HorasDireccion, d.CostoDireccionCLP));
            panel.Controls.Add(CrearChipCapaProductiva("Gestión", d.HorasGestionCoordinacion, d.CostoGestionCoordinacionCLP));

            return panel;
        }

        private Control CrearChipCapaProductiva(string titulo, double horas, double costo)
        {
            Panel chip = CrearPanelCard(Color.White);
            chip.Dock = DockStyle.None;
            chip.AutoSize = false;
            chip.Width = 190;
            chip.Height = 92;
            chip.MinimumSize = new Size(190, 92);
            chip.Margin = new Padding(0, 0, 8, 8);
            chip.Controls.Add(CrearTituloCard(titulo, Color.FromArgb(60, 90, 115)));
            chip.Controls.Add(CrearLineaDato("Horas", horas.ToString("0.##") + " h"));
            chip.Controls.Add(CrearLineaDato("Costo", FormatearValorVisual(costo)));
            return chip;
        }

        private void AppendTituloResumen(string texto)
        {
            rtbResumenDesgloseProductivo.AppendText(texto + "\n");
            rtbResumenDesgloseProductivo.AppendText(new string('=', 78) + "\n\n");
        }

        private void AppendSubtituloResumen(string texto)
        {
            rtbResumenDesgloseProductivo.AppendText(texto + "\n");
            rtbResumenDesgloseProductivo.AppendText(new string('-', 78) + "\n");
        }

        private void AppendBloqueRangoNumerico(
            string nombre,
            double minimo,
            double estandar,
            double holgado,
            double valorReferencia,
            string unidad,
            string etiquetaReferencia
        )
        {
            rtbResumenDesgloseProductivo.AppendText(
                nombre + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Mínimo    : " + minimo.ToString("0.##") + " " + unidad + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Estándar  : " + estandar.ToString("0.##") + " " + unidad + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Holgado   : " + holgado.ToString("0.##") + " " + unidad + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + AjustarTextoDerecha(etiquetaReferencia + " :", 12) +
                " " + valorReferencia.ToString("0.##") + " " + unidad + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + ConstruirBarraRango(minimo, estandar, holgado, valorReferencia) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + ConstruirLeyendaRango() + "\n\n"
            );
        }

        private ResultadoCapacidadProyecto ObtenerCapacidadPorEscenario(
    EscenarioPlanificacionDesglose escenario,
    ResultadoCapacidadProyecto minimo,
    ResultadoCapacidadProyecto estandar,
    ResultadoCapacidadProyecto holgado
)
        {
            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return minimo;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return holgado;
            }

            return estandar;
        }

        private double ObtenerPrecioEscenario(
            ResultadoEscenarioOfertaDesglose oferta,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (oferta == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return oferta.PrecioMinimoCLP;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return oferta.PrecioHolgadoCLP;
            }

            return oferta.PrecioEstandarCLP;
        }

        private double ObtenerDiasReqEscenario(
            RequerimientoProduccionInterna req,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (req == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return req.DiasPersonaMin;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return req.DiasPersonaHolgura;
            }

            return req.DiasPersonaStd;
        }

        private double ObtenerCostoReqEscenario(
            RequerimientoProduccionInterna req,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (req == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return req.CostoMinimoCLP;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return req.CostoHolguraCLP;
            }

            return req.CostoEstandarCLP;
        }


        private void AppendBloqueRangoMonetario(
            string nombre,
            double minimo,
            double estandar,
            double holgado,
            double valorReferencia,
            string etiquetaReferencia
        )
        {
            rtbResumenDesgloseProductivo.AppendText(
                nombre + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Mínimo    : " + FormatearCLP(minimo) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Estándar  : " + FormatearCLP(estandar) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  Holgado   : " + FormatearCLP(holgado) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + AjustarTextoDerecha(etiquetaReferencia + " :", 12) +
                " " + FormatearCLP(valorReferencia) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + ConstruirBarraRango(minimo, estandar, holgado, valorReferencia) + "\n"
            );

            rtbResumenDesgloseProductivo.AppendText(
                "  " + ConstruirLeyendaRango() + "\n\n"
            );
        }

        private string ConstruirBarraRango(
            double minimo,
            double estandar,
            double holgado,
            double referencia
        )
        {
            int ancho = 44;

            if (holgado <= minimo)
            {
                return "[sin rango suficiente]";
            }

            char[] barra = new string('─', ancho).ToCharArray();

            int posMin = 0;
            int posEst = CalcularPosicionEnBarra(estandar, minimo, holgado, ancho);
            int posHol = ancho - 1;
            int posRef = CalcularPosicionEnBarra(referencia, minimo, holgado, ancho);

            barra[posMin] = '●';
            barra[posEst] = '▲';
            barra[posHol] = '■';

            if (posRef >= 0 && posRef < ancho)
            {
                if (barra[posRef] == '─')
                {
                    barra[posRef] = '◆';
                }
                else if (barra[posRef] == '●' || barra[posRef] == '▲' || barra[posRef] == '■')
                {
                    barra[posRef] = '◆';
                }
            }

            return "[" + new string(barra) + "]";
        }

        private string ConstruirLeyendaRango()
        {
            return "● mín   ▲ est   ■ holg   ◆ cliente/activo";
        }

        private int CalcularPosicionEnBarra(
            double valor,
            double minimo,
            double maximo,
            int ancho
        )
        {
            if (ancho <= 1)
            {
                return 0;
            }

            if (maximo <= minimo)
            {
                return 0;
            }

            if (valor <= minimo)
            {
                return 0;
            }

            if (valor >= maximo)
            {
                return ancho - 1;
            }

            double fraccion = (valor - minimo) / (maximo - minimo);
            int posicion = (int)Math.Round(fraccion * (ancho - 1));

            if (posicion < 0)
            {
                posicion = 0;
            }

            if (posicion >= ancho)
            {
                posicion = ancho - 1;
            }

            return posicion;
        }

        private string FormatearCLP(double valor)
        {
            return FormatearValorVisual(valor);
        }

        private string AjustarTextoDerecha(string texto, int ancho)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                texto = "";
            }

            if (texto.Length >= ancho)
            {
                return texto;
            }

            return texto.PadRight(ancho, ' ');
        }

        private void RefrescarDespuesDeEditarDesgloseProductivo()
        {
            try
            {
                RefrescarPanelSiguientePasoDatos();
            }
            catch
            {
            }

            try
            {
                RefrescarResumen();
            }
            catch
            {
            }

            try
            {
                if (panelGantt != null)
                {
                    panelGantt.Invalidate();
                    panelGantt.Refresh();
                }
            }
            catch
            {
            }
        }

        private ModoDuracionDesglose ConvertirEscenarioAModoDuracion(
    EscenarioPlanificacionDesglose escenario
)
        {
            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return ModoDuracionDesglose.Minimo;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return ModoDuracionDesglose.Holgura;
            }

            return ModoDuracionDesglose.Estandar;
        }

        private double ObtenerDiasEscenario(
            DesgloseProductivoProyecto d,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (d == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return d.DiasPersonaMinimos;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return d.DiasPersonaHolgura;
            }

            return d.DiasPersonaEstandar;
        }

        private double ObtenerSemanasEscenario(
            DesgloseProductivoProyecto d,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (d == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return d.SemanasMinimas;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return d.SemanasHolgura;
            }

            return d.SemanasEstandar;
        }

        private double ObtenerCostoEscenario(
            DesgloseProductivoProyecto d,
            EscenarioPlanificacionDesglose escenario
        )
        {
            if (d == null)
            {
                return 0.0;
            }

            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return d.CostoMinimoCLP;
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return d.CostoHolguraCLP;
            }

            return d.CostoEstandarCLP;
        }

        private string ObtenerNombreEscenarioVisible(
    EscenarioPlanificacionDesglose escenario
)
        {
            if (escenario == EscenarioPlanificacionDesglose.Minimo)
            {
                return "Mínimo";
            }

            if (escenario == EscenarioPlanificacionDesglose.Estandar)
            {
                return "Estándar";
            }

            if (escenario == EscenarioPlanificacionDesglose.Holgado)
            {
                return "Holgado";
            }

            if (escenario == EscenarioPlanificacionDesglose.Recomendado)
            {
                return "Recomendado";
            }

            return "Sin escenario";
        }

        private double ParsearDoubleDesglose(object valor, double respaldo)
        {
            if (valor == null)
            {
                return respaldo;
            }

            string texto = valor.ToString();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return respaldo;
            }

            texto = texto
                .Trim()
                .Replace("$", "")
                .Replace("CLP", "")
                .Replace(".", "")
                .Replace(",", ".");

            double resultado;

            if (double.TryParse(
                texto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out resultado))
            {
                return resultado;
            }

            return respaldo;
        }
    }
}
