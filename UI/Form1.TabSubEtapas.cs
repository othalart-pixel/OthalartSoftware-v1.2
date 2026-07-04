using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;
using System.Collections.Generic;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private const string ColSubVistaNombre = "ColSubVistaNombre";
        private const string ColSubVistaDuracion = "ColSubVistaDuracion";
        private const string ColSubVistaInicio = "ColSubVistaInicio";
        private const string ColSubVistaFin = "ColSubVistaFin";
        private const string ColSubVistaEstado = "ColSubVistaEstado";
        private const string ColSubVistaRango = "ColSubVistaRango";

        private bool subEtapasBasePreparadas = false;
        private List<ResolucionDependenciaSubEtapa> resolucionesDependenciasSubEtapas =
        new List<ResolucionDependenciaSubEtapa>();

        private void ConstruirTabSubEtapas(TabPage tab)
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                bibliotecaSubEtapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();
            }

            PrepararSubEtapasComoRecomendadasIniciales();

            tab.Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 1;
            layout.RowCount = 2;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 2;
            panelSuperior.RowCount = 2;

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            Label titulo = new Label();
            titulo.Text = "Estado de etapas, subprocesos y semanas";
            titulo.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            Label ayuda = new Label();
            ayuda.Text = "Click en una etapa para expandir/contraer. Click en estado para activar/desactivar. Al activar una etapa, todos sus subprocesos quedan recomendados como opcionales.";
            ayuda.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            ayuda.ForeColor = Color.FromArgb(90, 90, 90);
            ayuda.Dock = DockStyle.Fill;
            ayuda.TextAlign = ContentAlignment.MiddleLeft;

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Fill;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = false;
            acciones.AutoSize = true;
            acciones.Margin = new Padding(0);

            btnAplicarSubEtapas.Text = "Actualizar";
            btnAplicarSubEtapas.Width = 135;
            btnAplicarSubEtapas.Height = 30;
            btnAplicarSubEtapas.Margin = new Padding(0, 4, 8, 0);
            btnAplicarSubEtapas.Click -= BtnAplicarSubEtapas_Click;
            btnAplicarSubEtapas.Click += BtnAplicarSubEtapas_Click;

            btnRestaurarSubEtapas.Text = "Restaurar plan";
            btnRestaurarSubEtapas.Width = 115;
            btnRestaurarSubEtapas.Height = 30;
            btnRestaurarSubEtapas.Margin = new Padding(0, 4, 0, 0);
            btnRestaurarSubEtapas.Click -= BtnRestaurarSubEtapas_Click;
            btnRestaurarSubEtapas.Click += BtnRestaurarSubEtapas_Click;

            acciones.Controls.Add(btnAplicarSubEtapas);
            acciones.Controls.Add(btnRestaurarSubEtapas);

            panelSuperior.Controls.Add(titulo, 0, 0);
            panelSuperior.SetColumnSpan(titulo, 2);
            panelSuperior.Controls.Add(ayuda, 0, 1);
            panelSuperior.Controls.Add(acciones, 1, 1);

            dgvSubEtapas.Dock = DockStyle.Fill;
            dgvSubEtapas.AllowUserToAddRows = false;
            dgvSubEtapas.AllowUserToDeleteRows = false;
            dgvSubEtapas.RowHeadersVisible = false;
            dgvSubEtapas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSubEtapas.ScrollBars = ScrollBars.Both;
            dgvSubEtapas.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dgvSubEtapas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvSubEtapas.MultiSelect = false;
            dgvSubEtapas.AllowUserToResizeRows = false;

            ReconstruirColumnasVistaSubEtapas();

            DesconectarEventosSubEtapas();
            ConectarEventosSubEtapas();

            layout.Controls.Add(panelSuperior, 0, 0);
            layout.Controls.Add(dgvSubEtapas, 0, 1);

            tab.Controls.Add(layout);
            InstalarGanttGrandeEnTabPrincipal(tab);
            RefrescarTablaSubEtapas();
            ActivarActualizacionGanttEnVivo();
        }


        private void SincronizarFilaEtapasConModeloParaGantt(int rowIndex)
        {
            if (dgvEtapas == null || rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[rowIndex];

            if (row == null || row.IsNewRow)
            {
                return;
            }

            if (row.Tag is SubEtapaProyecto sub)
            {
                SincronizarSubEtapaDesdeFilaGantt(row, sub);
                return;
            }

            if (row.Tag is EtapaProyecto etapa)
            {
                SincronizarEtapaDesdeFilaGantt(row, etapa);
                return;
            }
        }

        private void SincronizarSubEtapaDesdeFilaGantt(DataGridViewRow row, SubEtapaProyecto sub)
        {
            if (row == null || sub == null)
            {
                return;
            }

            double inicioAnterior = sub.InicioSemana;
            double duracionAnterior = sub.DuracionSemanas;

            if (inicioAnterior < 0.0)
            {
                inicioAnterior = 0.0;
            }

            if (duracionAnterior <= 0.0)
            {
                duracionAnterior = 0.1;
            }

            if (row.Cells[ColEtapaInicio].Value != null)
            {
                sub.InicioSemana = ParsearSemanaDesdeCelda(
                    row.Cells[ColEtapaInicio].Value,
                    inicioAnterior
                );
            }

            if (row.Cells[ColEtapaDuracion].Value != null)
            {
                sub.DuracionSemanas = ParsearSemanaDesdeCelda(
                    row.Cells[ColEtapaDuracion].Value,
                    duracionAnterior
                );
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

        private void SincronizarEtapaDesdeFilaGantt(DataGridViewRow row, EtapaProyecto etapa)
        {
            if (row == null || etapa == null)
            {
                return;
            }

            if (row.Cells[ColEtapaInicio].Value != null)
            {
                etapa.InicioMes = ParsearMesesDesdeTextoEtapa(
                    Convert.ToString(row.Cells[ColEtapaInicio].Value),
                    etapa.InicioMes
                );
            }

            if (row.Cells[ColEtapaDuracion].Value != null)
            {
                etapa.DuracionMeses = ParsearMesesDesdeTextoEtapa(
                    Convert.ToString(row.Cells[ColEtapaDuracion].Value),
                    etapa.DuracionMeses
                );
            }

            if (row.Cells[ColEtapaFin].Value != null)
            {
                etapa.FinMes = ParsearMesesDesdeTextoEtapa(
                    Convert.ToString(row.Cells[ColEtapaFin].Value),
                    etapa.FinMes
                );
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            if (etapa.DuracionMeses < 0.0)
            {
                etapa.DuracionMeses = 0.0;
            }

            if (etapa.FinMes <= etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }
        }


        private void PrepararSubEtapasComoRecomendadasIniciales()
        {
            if (subEtapasBasePreparadas)
            {
                return;
            }

            if (bibliotecaSubEtapas == null)
            {
                return;
            }

            foreach (SubEtapaProyecto sub in bibliotecaSubEtapas)
            {
                sub.Activa = false;
                sub.Requerida = false;

                if (sub.InicioSemana < 0.0)
                {
                    sub.InicioSemana = 0.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }
            }

            if (cotizacion != null && cotizacion.Etapas != null)
            {
                foreach (EtapaProyecto etapa in cotizacion.Etapas)
                {
                    DesactivarEtapaSinSubEtapas(etapa);
                }
            }

            subEtapasBasePreparadas = true;
        }

        private void ConectarEventosSubEtapas()
        {
            dgvSubEtapas.CellClick -= DgvSubEtapas_CellClick;
            dgvSubEtapas.CellEndEdit -= DgvSubEtapas_CellEndEdit;
            dgvSubEtapas.CellValueChanged -= DgvSubEtapas_CellValueChanged;
            dgvSubEtapas.CurrentCellDirtyStateChanged -= DgvSubEtapas_CurrentCellDirtyStateChanged;
            dgvSubEtapas.EditingControlShowing -= DgvSubEtapas_EditingControlShowing;

            dgvSubEtapas.CellClick += DgvSubEtapas_CellClick;
            dgvSubEtapas.CellEndEdit += DgvSubEtapas_CellEndEdit;

            /*
             * Ojo: dejamos CellValueChanged conectado solo para checkbox/combos si algún día se usan,
             * pero NO recalcularemos duración/inicio/fin ahí.
             */
            dgvSubEtapas.CellValueChanged += DgvSubEtapas_CellValueChanged;

            dgvSubEtapas.CurrentCellDirtyStateChanged += DgvSubEtapas_CurrentCellDirtyStateChanged;
            dgvSubEtapas.EditingControlShowing += DgvSubEtapas_EditingControlShowing;
        }

        private void DesconectarEventosSubEtapas()
        {
            dgvSubEtapas.CellClick -= DgvSubEtapas_CellClick;
            dgvSubEtapas.CellEndEdit -= DgvSubEtapas_CellEndEdit;
            dgvSubEtapas.CellValueChanged -= DgvSubEtapas_CellValueChanged;
            dgvSubEtapas.CurrentCellDirtyStateChanged -= DgvSubEtapas_CurrentCellDirtyStateChanged;
            dgvSubEtapas.EditingControlShowing -= DgvSubEtapas_EditingControlShowing;
        }

        private void DgvSubEtapas_EditingControlShowing(
    object sender,
    DataGridViewEditingControlShowingEventArgs e
)
        {
            TextBox tb = e.Control as TextBox;

            if (tb == null)
            {
                return;
            }

            tb.KeyPress -= DgvSubEtapasDecimal_KeyPress;

            if (dgvSubEtapas == null || dgvSubEtapas.CurrentCell == null)
            {
                return;
            }

            string columna = dgvSubEtapas.Columns[dgvSubEtapas.CurrentCell.ColumnIndex].Name;

            if (columna == ColSubVistaDuracion ||
                columna == ColSubVistaInicio ||
                columna == ColSubVistaFin)
            {
                tb.KeyPress += DgvSubEtapasDecimal_KeyPress;
            }
        }

        private void DgvSubEtapasDecimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == ',' || e.KeyChar == '.')
            {
                TextBox tb = sender as TextBox;

                if (tb != null &&
                    !tb.Text.Contains(",") &&
                    !tb.Text.Contains("."))
                {
                    return;
                }
            }

            if (e.KeyChar == '%')
            {
                TextBox tb = sender as TextBox;

                if (tb != null && !tb.Text.Contains("%"))
                {
                    return;
                }
            }

            e.Handled = true;
        }

        private void ReconstruirColumnasVistaSubEtapas()
        {
            cargandoTabla = true;

            dgvSubEtapas.Columns.Clear();

            dgvSubEtapas.Columns.Add(ColSubVistaNombre, "Etapa / subproceso");
            dgvSubEtapas.Columns.Add(ColSubVistaDuracion, "Duración");
            dgvSubEtapas.Columns.Add(ColSubVistaInicio, "Inicio");
            dgvSubEtapas.Columns.Add(ColSubVistaFin, "Fin");
            dgvSubEtapas.Columns.Add(ColSubVistaEstado, "Estado");
            dgvSubEtapas.Columns.Add(ColSubVistaRango, "Recomendación");

            dgvSubEtapas.Columns[ColSubVistaNombre].FillWeight = 32;
            dgvSubEtapas.Columns[ColSubVistaDuracion].FillWeight = 11;
            dgvSubEtapas.Columns[ColSubVistaInicio].FillWeight = 9;
            dgvSubEtapas.Columns[ColSubVistaFin].FillWeight = 9;
            dgvSubEtapas.Columns[ColSubVistaEstado].FillWeight = 18;
            dgvSubEtapas.Columns[ColSubVistaRango].FillWeight = 21;

            dgvSubEtapas.Columns[ColSubVistaNombre].ReadOnly = true;
            dgvSubEtapas.Columns[ColSubVistaEstado].ReadOnly = true;
            dgvSubEtapas.Columns[ColSubVistaRango].ReadOnly = true;

            foreach (DataGridViewColumn columna in dgvSubEtapas.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvSubEtapas.EnableHeadersVisualStyles = false;
            dgvSubEtapas.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSubEtapas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvSubEtapas.ColumnHeadersHeight = 32;

            dgvSubEtapas.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvSubEtapas.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvSubEtapas.RowTemplate.Height = 28;

            AplicarEstiloTablaCentrada(dgvSubEtapas);

            cargandoTabla = false;
        }

        private void RefrescarTablaSubEtapas()
        {
            if (dgvSubEtapas == null || dgvSubEtapas.Columns.Count == 0)
            {
                return;
            }

            DesconectarEventosSubEtapas();

            cargandoTabla = true;
            dgvSubEtapas.SuspendLayout();

            try
            {
                try
                {
                    if (dgvSubEtapas.IsCurrentCellInEditMode)
                    {
                        dgvSubEtapas.EndEdit();
                    }
                }
                catch
                {
                }

                try
                {
                    dgvSubEtapas.ClearSelection();
                    dgvSubEtapas.CurrentCell = null;
                }
                catch
                {
                }

                NormalizarTodasLasEtapasPorSubEtapas();

                BibliotecaDependenciasSubEtapasService.AplicarPropuestaMinimaOrdenTemporal(
                    bibliotecaSubEtapas,
                    resolucionesDependenciasSubEtapas
                );

                NormalizarTodasLasEtapasPorSubEtapas();
                ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();

                dgvSubEtapas.Rows.Clear();

                dgvSubEtapas.Rows.Clear();

                string[] etapasOrdenadas = new string[]
                {
                    "Desarrollo",
                    "Preproduccion",
                    "Produccion",
                    "Postproduccion"
                };

                foreach (string nombreEtapa in etapasOrdenadas)
                {
                    EtapaProyecto etapa = ObtenerEtapaCotizacionPorNombre(nombreEtapa);

                    if (etapa == null)
                    {
                        continue;
                    }

                    AgregarFilaEtapaVistaSubEtapas(etapa);

                    if (EtapaEstaExpandidaEnVistaSubEtapas(etapa))
                    {
                        AgregarFilasSubEtapasVista(etapa);
                    }
                }
            }
            finally
            {
                dgvSubEtapas.ResumeLayout();

                cargandoTabla = false;

                ConectarEventosSubEtapas();
            }
        }

        private void ProgramarRefrescoSubEtapas()
        {
            if (refrescoEtapasPendiente)
            {
                return;
            }

            refrescoEtapasPendiente = true;

            if (IsDisposed || !IsHandleCreated)
            {
                refrescoEtapasPendiente = false;
                return;
            }

            BeginInvoke(new Action(() =>
            {
                refrescoEtapasPendiente = false;

                NormalizarTodasLasEtapasPorSubEtapas();
                ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();

                RefrescarDespuesDeEditarEtapas();
            }));
        }

        private void AgregarFilaEtapaVistaSubEtapas(EtapaProyecto etapa)
        {
            int rowIndex = dgvSubEtapas.Rows.Add();
            DataGridViewRow row = dgvSubEtapas.Rows[rowIndex];

            bool expandida = EtapaEstaExpandidaEnVistaSubEtapas(etapa);
            string flecha = expandida ? "▾ " : "▸ ";

            row.Cells[ColSubVistaNombre].Value = flecha + etapa.Nombre;

            row.Cells[ColSubVistaDuracion].Value = etapa.Seleccionada
                ? FormatearDuracionEtapaVista(etapa.DuracionMeses)
                : "";

            row.Cells[ColSubVistaInicio].Value = etapa.Seleccionada
                ? FormatearPosicionEtapaVista(etapa.InicioMes)
                : "";

            row.Cells[ColSubVistaFin].Value = etapa.Seleccionada
                ? FormatearPosicionEtapaVista(etapa.FinMes)
                : "";

            row.Cells[ColSubVistaEstado].Value = etapa.Seleccionada
                ? "Activa"
                : "No corre";
            row.Cells[ColSubVistaRango].Value = "";

            row.Tag = etapa;

            AplicarEstiloFilaEtapaVistaSubEtapas(row, etapa);

            row.Cells[ColSubVistaNombre].ReadOnly = true;
            row.Cells[ColSubVistaDuracion].ReadOnly = false;
            row.Cells[ColSubVistaInicio].ReadOnly = false;
            row.Cells[ColSubVistaFin].ReadOnly = false;
            row.Cells[ColSubVistaEstado].ReadOnly = true;
            row.Cells[ColSubVistaRango].ReadOnly = true;
        }

        private string FormatearDuracionEtapaVista(double duracionMeses)
        {
            if (duracionMeses <= 0.0)
            {
                return "";
            }

            double semanas = duracionMeses * GanttGrandeSemanasPorMes;

            return semanas.ToString("0.##") + " sem";
        }

        private string FormatearPosicionEtapaVista(double posicionMeses)
        {
            if (posicionMeses < 0.0)
            {
                posicionMeses = 0.0;
            }

            return (posicionMeses * GanttGrandeSemanasPorMes).ToString("0.##") + " sem";
        }

        private void AgregarFilasSubEtapasVista(EtapaProyecto etapa)
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                return;
            }

            if (etapa.Seleccionada)
            {
                ExpandirEtapaParaContenerSubEtapas(etapa);
            }

            var subEtapas = bibliotecaSubEtapas
                .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .OrderBy(s => s.Orden)
                .ToList();

            foreach (SubEtapaProyecto sub in subEtapas)
            {
                AsegurarSemanasValidasSubEtapaDecimal(sub);

                int rowIndex = dgvSubEtapas.Rows.Add();
                DataGridViewRow row = dgvSubEtapas.Rows[rowIndex];

                string marca;

                if (!sub.Activa)
                {
                    marca = "□";
                }
                else
                {
                    marca = sub.Requerida ? "●" : "○";
                }

                row.Cells[ColSubVistaNombre].Value = "    ↳ " + marca + " " + sub.Nombre;

                row.Cells[ColSubVistaDuracion].Value = sub.Activa
                    ? sub.DuracionSemanas.ToString("0.##")
                    : "";

                row.Cells[ColSubVistaInicio].Value = sub.Activa
                    ? sub.InicioSemana.ToString("0.##")
                    : "";

                row.Cells[ColSubVistaFin].Value = sub.Activa
                    ? sub.FinSemana.ToString("0.##")
                    : "";

                row.Cells[ColSubVistaEstado].Value = ObtenerTextoEstadoSubEtapaVista(sub, etapa);
                row.Cells[ColSubVistaRango].Value = ObtenerTextoRangoSubEtapa(etapa, sub);

                row.Tag = sub;

                AplicarEstiloFilaSubEtapaVista(row, etapa, sub);

                row.Cells[ColSubVistaNombre].ReadOnly = true;
                row.Cells[ColSubVistaDuracion].ReadOnly = !sub.Activa;
                row.Cells[ColSubVistaInicio].ReadOnly = !sub.Activa;
                row.Cells[ColSubVistaFin].ReadOnly = !sub.Activa;
                row.Cells[ColSubVistaEstado].ReadOnly = true;
                row.Cells[ColSubVistaRango].ReadOnly = true;
            }
        }

        private void AplicarEstiloFilaEtapaVistaSubEtapas(DataGridViewRow row, EtapaProyecto etapa)
        {
            Color fondo = etapa.Seleccionada
                ? ObtenerColorFilaEtapa(etapa.Nombre)
                : Color.White;

            Color texto = etapa.Seleccionada
                ? Color.Black
                : Color.FromArgb(125, 125, 125);

            row.DefaultCellStyle.BackColor = fondo;
            row.DefaultCellStyle.ForeColor = texto;
            row.DefaultCellStyle.SelectionBackColor = fondo;
            row.DefaultCellStyle.SelectionForeColor = texto;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells[ColSubVistaNombre].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells[ColSubVistaEstado].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            row.Cells[ColSubVistaEstado].Style.ForeColor = etapa.Seleccionada
                ? Color.FromArgb(25, 100, 60)
                : Color.FromArgb(130, 130, 130);

            row.Cells[ColSubVistaNombre].ToolTipText = "Click para expandir o contraer subprocesos.";

            row.Cells[ColSubVistaEstado].ToolTipText = etapa.Seleccionada
                ? "Click para desactivar esta etapa y todos sus subprocesos."
                : "Click para activar esta etapa con todos sus subprocesos recomendados como opcionales.";
        }

        private void AplicarEstiloFilaSubEtapaVista(DataGridViewRow row, EtapaProyecto etapa, SubEtapaProyecto sub)
        {
            Color baseColor = ObtenerColorFilaEtapa(etapa.Nombre);

            Color fondo = sub.Activa
                ? MezclarConBlanco(baseColor, 0.45)
                : Color.FromArgb(247, 247, 247);

            Color texto = sub.Activa
                ? Color.FromArgb(40, 40, 40)
                : Color.FromArgb(140, 140, 140);

            row.DefaultCellStyle.BackColor = fondo;
            row.DefaultCellStyle.SelectionBackColor = fondo;
            row.DefaultCellStyle.ForeColor = texto;
            row.DefaultCellStyle.SelectionForeColor = texto;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells[ColSubVistaNombre].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            row.Cells[ColSubVistaNombre].Style.Font = sub.Activa
                ? new Font("Segoe UI", 9, FontStyle.Bold)
                : new Font("Segoe UI", 9, FontStyle.Italic);

            row.Cells[ColSubVistaEstado].Style.Font = sub.Activa
                ? (sub.Requerida
                    ? new Font("Segoe UI", 9, FontStyle.Bold)
                    : new Font("Segoe UI", 9, FontStyle.Italic))
                : new Font("Segoe UI", 9, FontStyle.Italic);

            row.Cells[ColSubVistaEstado].Style.ForeColor = sub.Activa
                ? Color.FromArgb(45, 45, 45)
                : Color.FromArgb(145, 145, 145);

            row.Cells[ColSubVistaNombre].ToolTipText = "Click para incluir o excluir este subproceso.";

            var alertas = BibliotecaDependenciasSubEtapasService.ValidarDependenciasDeSubEtapa(
                bibliotecaSubEtapas,
                sub,
                resolucionesDependenciasSubEtapas
            );

            if (alertas != null && alertas.Count > 0)
            {
                row.Cells[ColSubVistaEstado].ToolTipText =
                    "Click para alternar requerido/opcional.\n\nDependencias faltantes:\n- " +
                    string.Join("\n- ", alertas.Select(a => a.Mensaje));
            }
            else
            {
                row.Cells[ColSubVistaEstado].ToolTipText =
                    "Click para alternar requerido/opcional.";
            }
        }

        private void DgvSubEtapas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvSubEtapas == null || cargandoTabla)
            {
                return;
            }

            if (dgvSubEtapas.IsCurrentCellDirty)
            {
                try
                {
                    dgvSubEtapas.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
                catch
                {
                }
            }
        }

        private void DgvSubEtapas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvSubEtapas.Rows[e.RowIndex];
            string columna = dgvSubEtapas.Columns[e.ColumnIndex].Name;

            if (row.Tag is EtapaProyecto etapa)
            {
                if (columna == ColSubVistaNombre)
                {
                    AlternarExpansionEtapaDesdeVistaSubEtapas(etapa);
                    return;
                }

                if (columna == ColSubVistaEstado)
                {
                    ToggleEstadoEtapaDesdeVistaSubEtapas(etapa);
                    return;
                }

                return;
            }

            if (row.Tag is SubEtapaProyecto sub)
            {
                if (columna == ColSubVistaNombre)
                {
                    sub.Activa = !sub.Activa;

                    if (sub.Activa)
                    {
                        sub.Requerida = false;
                    }

                    MantenerEtapaPadreExpandida(sub);
                    ActualizarEstadoEtapaPadreDeSubEtapa(sub);
                    ProgramarRefrescoSubEtapas();
                    return;
                }

                if (columna == ColSubVistaEstado)
                {
                    if (!sub.Activa)
                    {
                        sub.Activa = true;
                        sub.Requerida = true;
                    }
                    else
                    {
                        sub.Requerida = !sub.Requerida;
                    }

                    MantenerEtapaPadreExpandida(sub);
                    ActualizarEstadoEtapaPadreDeSubEtapa(sub);
                    ProgramarRefrescoSubEtapas();
                    return;
                }
            }
        }

        private void DgvSubEtapas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvSubEtapas.Rows[e.RowIndex];

            if (row.Tag is EtapaProyecto)
            {
                AplicarCambiosFilaEtapaDesdeVistaSubEtapas(e.RowIndex);
                return;
            }

            if (row.Tag is SubEtapaProyecto)
            {
                AplicarCambiosFilaSubEtapaDesdeVistaSubEtapas(e.RowIndex);
                return;
            }
        }

        private void DgvSubEtapas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            /*
             * Intencionalmente NO aplicamos duración/inicio/fin aquí.
             *
             * Si recalculamos en CellValueChanged, al escribir 1,5 pasa:
             * escribe 1 -> refresca tabla -> nunca alcanza a escribir ,5.
             *
             * Los cambios reales se aplican en CellEndEdit.
             */
        }

        private void AplicarCambiosFilaEtapaDesdeVistaSubEtapas(int rowIndex)
        {
            if (cargandoTabla || rowIndex < 0 || rowIndex >= dgvSubEtapas.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvSubEtapas.Rows[rowIndex];
            EtapaProyecto etapa = row.Tag as EtapaProyecto;

            if (etapa == null)
            {
                return;
            }

            if (!TieneSubEtapasActivas(etapa))
            {
                DesactivarEtapaSinSubEtapas(etapa);
                ProgramarRefrescoSubEtapas();
                return;
            }

            string textoDuracion = LeerCeldaVistaSubEtapas(row, ColSubVistaDuracion);
            string textoInicio = LeerCeldaVistaSubEtapas(row, ColSubVistaInicio);
            string textoFin = LeerCeldaVistaSubEtapas(row, ColSubVistaFin);

            bool hayDuracion = !string.IsNullOrWhiteSpace(textoDuracion);
            bool hayInicio = !string.IsNullOrWhiteSpace(textoInicio);
            bool hayFin = !string.IsNullOrWhiteSpace(textoFin);

            if (!hayDuracion && !hayInicio && !hayFin)
            {
                DesactivarEtapaYSubEtapas(etapa);
                ProgramarRefrescoSubEtapas();
                return;
            }

            double duracion = etapa.DuracionMeses;
            double inicio = etapa.InicioMes;
            double fin = etapa.FinMes;

            if (hayDuracion)
            {
                duracion = ParsearMesesDesdeTextoEtapa(textoDuracion, duracion);
            }

            if (hayInicio)
            {
                inicio = ParsearMesesDesdeTextoEtapa(textoInicio, inicio);
            }

            if (hayFin)
            {
                fin = ParsearMesesDesdeTextoEtapa(textoFin, fin);
            }

            if (duracion < 0.0)
            {
                duracion = 0.0;
            }

            if (inicio < 0.0)
            {
                inicio = 0.0;
            }

            if (fin < inicio)
            {
                fin = inicio;
            }

            /*
             * IMPORTANTE:
             * Duración, inicio y fin son campos independientes.
             * No hacemos:
             *   duracion = fin - inicio;
             *   fin = inicio + duracion;
             */

            etapa.Seleccionada = true;
            etapa.DuracionMeses = duracion;
            etapa.InicioMes = inicio;
            etapa.FinMes = fin;

            etapaExpandidaEnTabla = etapa;

            ExpandirEtapaParaContenerSubEtapas(etapa);
            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();

            ProgramarRefrescoSubEtapas();
        }

        private void AplicarCambiosFilaSubEtapaDesdeVistaSubEtapas(int rowIndex)
        {
            if (cargandoTabla || rowIndex < 0 || rowIndex >= dgvSubEtapas.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvSubEtapas.Rows[rowIndex];
            SubEtapaProyecto sub = row.Tag as SubEtapaProyecto;

            if (sub == null)
            {
                return;
            }

            EtapaProyecto etapaPadre = ObtenerEtapaCotizacionPorNombre(sub.EtapaPadre);

            if (etapaPadre == null)
            {
                return;
            }

            string textoDuracion = LeerCeldaVistaSubEtapas(row, ColSubVistaDuracion);
            string textoInicio = LeerCeldaVistaSubEtapas(row, ColSubVistaInicio);
            string textoFin = LeerCeldaVistaSubEtapas(row, ColSubVistaFin);

            double duracionAnterior = sub.DuracionSemanas <= 0.0 ? 0.1 : sub.DuracionSemanas;
            double inicioAnterior = sub.InicioSemana < 1.0 ? 1.0 : sub.InicioSemana;
            double finAnterior = sub.FinSemana <= inicioAnterior ? inicioAnterior + duracionAnterior : sub.FinSemana;

            double duracion = string.IsNullOrWhiteSpace(textoDuracion)
                ? duracionAnterior
                : ParsearDecimalSemanaSubVista(textoDuracion, duracionAnterior);

            double inicio = string.IsNullOrWhiteSpace(textoInicio)
                ? inicioAnterior
                : ParsearDecimalSemanaSubVista(textoInicio, inicioAnterior);

            double fin = string.IsNullOrWhiteSpace(textoFin)
                ? finAnterior
                : ParsearDecimalSemanaSubVista(textoFin, finAnterior);

            if (duracion <= 0.0)
            {
                duracion = 0.1;
            }

            if (inicio < 1.0)
            {
                inicio = 1.0;
            }

            string columnaEditada = "";

            if (dgvSubEtapas.CurrentCell != null)
            {
                columnaEditada = dgvSubEtapas.Columns[dgvSubEtapas.CurrentCell.ColumnIndex].Name;
            }

            /*
             * FinSemana es fin exclusivo:
             * Inicio 1 + Duración 1 = Fin 2.
             * Por eso NO usamos +1 ni -1.
             */
            if (columnaEditada == ColSubVistaFin)
            {
                if (fin <= inicio)
                {
                    fin = inicio + 0.1;
                }

                duracion = fin - inicio;
            }
            else if (columnaEditada == ColSubVistaInicio)
            {
                if (fin > inicio)
                {
                    duracion = fin - inicio;
                }
                else
                {
                    fin = inicio + duracion;
                }
            }
            else
            {
                fin = inicio + duracion;
            }

            if (duracion <= 0.0)
            {
                duracion = 0.1;
                fin = inicio + duracion;
            }

            sub.Activa = true;
            sub.InicioSemana = inicio;
            sub.DuracionSemanas = duracion;

            ActualizarEstadoEtapaSegunSubEtapas(etapaPadre);

            if (etapaPadre.Seleccionada)
            {
                ExpandirEtapaParaContenerSubEtapas(etapaPadre);
            }

            etapaExpandidaEnTabla = etapaPadre;

            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();

            BibliotecaSubEtapasJsonService.GuardarSubEtapas(bibliotecaSubEtapas);
            ProgramarRefrescoSubEtapas();
        }

        private void OrdenarPonderadoresSubEtapa(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            if (sub.PorcentajeMinimoEtapa < 0.0)
            {
                sub.PorcentajeMinimoEtapa = 0.0;
            }

            if (sub.PorcentajeRecomendadoEtapa < sub.PorcentajeMinimoEtapa)
            {
                sub.PorcentajeRecomendadoEtapa = sub.PorcentajeMinimoEtapa;
            }

            if (sub.PorcentajeMaximoEtapa < sub.PorcentajeRecomendadoEtapa)
            {
                sub.PorcentajeMaximoEtapa = sub.PorcentajeRecomendadoEtapa;
            }
        }

        private string FormatearPonderadorSubEtapa(double valor)
        {
            return (valor * 100.0).ToString("0.##") + "%";
        }

        private string ObtenerTextoRangoSubEtapa(EtapaProyecto etapa, SubEtapaProyecto sub)
        {
            if (etapa == null || sub == null || !sub.Activa)
            {
                return "";
            }

            double semanasEtapa = ObtenerSemanasReferenciaEtapa(etapa);

            if (semanasEtapa <= 0.0)
            {
                return "Sin etapa activa";
            }

            double proporcionActual = sub.DuracionSemanas / semanasEtapa;

            if (proporcionActual < sub.PorcentajeMinimoEtapa)
            {
                return "Bajo rango";
            }

            if (proporcionActual > sub.PorcentajeMaximoEtapa)
            {
                return "Sobre rango";
            }

            double toleranciaRec = Math.Max(0.015, (sub.PorcentajeMaximoEtapa - sub.PorcentajeMinimoEtapa) * 0.18);

            if (Math.Abs(proporcionActual - sub.PorcentajeRecomendadoEtapa) <= toleranciaRec)
            {
                return "En punto recomendado";
            }

            if (proporcionActual < sub.PorcentajeRecomendadoEtapa)
            {
                return "En rango bajo";
            }

            return "En rango alto";
        }

        private double ObtenerSemanasReferenciaEtapa(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return 0.0;
            }

            double semanas = etapa.DuracionMeses * 4.345;

            if (semanas > 0.0)
            {
                return semanas;
            }

            if (bibliotecaSubEtapas == null)
            {
                return 0.0;
            }

            return bibliotecaSubEtapas
                .Where(s =>
                    s != null &&
                    s.Activa &&
                    NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .Sum(s => Math.Max(0.0, s.DuracionSemanas));
        }

        private double ParsearPonderadorSubEtapa(string texto, double valorDefecto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return valorDefecto;
            }

            string limpio = texto
                .Trim()
                .Replace("%", "")
                .Replace(",", ".");

            double valor;

            if (!double.TryParse(
                limpio,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out valor))
            {
                return valorDefecto;
            }

            if (valor < 0.0)
            {
                return 0.0;
            }

            if (valor > 1.0)
            {
                valor = valor / 100.0;
            }

            return valor;
        }

        private double ParsearDecimalSemanaSubVista(string texto, double valorDefecto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return valorDefecto;
            }

            string limpio = texto
                .Trim()
                .ToLowerInvariant()
                .Replace("semanas", "")
                .Replace("semana", "")
                .Replace("sems", "")
                .Replace("sem", "")
                .Replace("s", "")
                .Replace(" ", "")
                .Replace(",", ".");

            double valor;

            if (!double.TryParse(
                limpio,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out valor))
            {
                return valorDefecto;
            }

            return valor;
        }

        private double ParsearMesesDesdeTextoEtapa(string texto, double valorDefecto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return valorDefecto;
            }

            string limpio = texto.Trim().ToLowerInvariant();
            bool estaEnSemanas =
                limpio.Contains("semana") ||
                limpio.Contains("semanas") ||
                limpio.Contains("sem");
            bool estaEnMeses =
                limpio.Contains("mes") ||
                limpio.Contains("meses");

            limpio = limpio
                .Replace("semanas", "")
                .Replace("semana", "")
                .Replace("sems", "")
                .Replace("sem", "")
                .Replace("meses", "")
                .Replace("mes", "")
                .Trim();

            double valor = ConvertirDouble(limpio);

            if (valor < 0.0)
            {
                return valorDefecto;
            }

            if (estaEnSemanas)
            {
                return valor / GanttGrandeSemanasPorMes;
            }

            if (estaEnMeses)
            {
                return valor;
            }

            return valor / GanttGrandeSemanasPorMes;
        }

        private void AsegurarSemanasValidasSubEtapaDecimal(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            if (sub.InicioSemana < 1.0)
            {
                sub.InicioSemana = 1.0;
            }

            if (sub.DuracionSemanas <= 0.0)
            {
                sub.DuracionSemanas = 0.1;
            }
        }

        private void AlternarExpansionEtapaDesdeVistaSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (etapaExpandidaEnTabla != null &&
                NormalizarNombreEtapa(etapaExpandidaEnTabla.Nombre) == NormalizarNombreEtapa(etapa.Nombre))
            {
                etapaExpandidaEnTabla = null;
            }
            else
            {
                etapaExpandidaEnTabla = etapa;
            }

            ProgramarRefrescoSubEtapas();
        }

        private void ToggleEstadoEtapaDesdeVistaSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (etapa.Seleccionada)
            {
                DesactivarEtapaYSubEtapas(etapa);
                etapaExpandidaEnTabla = etapa;

                ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();
                ProgramarRefrescoSubEtapas();
                return;
            }

            ActivarEtapaConSubEtapasRecomendadas(etapa);
            etapaExpandidaEnTabla = etapa;

            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();
            ProgramarRefrescoSubEtapas();
        }

        private void ActivarEtapaConSubEtapasRecomendadas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (bibliotecaSubEtapas != null)
            {
                var subEtapasDeLaEtapa = bibliotecaSubEtapas
                    .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .OrderBy(s => s.Orden)
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
                {
                    sub.Activa = true;
                    sub.Requerida = false;

                    if (sub.InicioSemana < 1)
                    {
                        sub.InicioSemana = 1;
                    }

                    if (sub.DuracionSemanas < 1)
                    {
                        sub.DuracionSemanas = 1;
                    }
                }
            }

            etapa.Seleccionada = TieneSubEtapasActivas(etapa);

            if (!etapa.Seleccionada)
            {
                DesactivarEtapaSinSubEtapas(etapa);
                return;
            }

            if (etapa.DuracionMeses < 0.0)
            {
                etapa.DuracionMeses = 0.0;
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            if (etapa.FinMes < etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes;
            }

            /*
             * Si es una etapa recién activada y está completamente vacía,
             * le damos un rango mínimo visible.
             * Pero NO recalculamos duración.
             */
            if (etapa.FinMes == etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes + 1.0;
            }

            ExpandirEtapaParaContenerSubEtapas(etapa);
        }

        private void ActualizarEstadoEtapaPadreDeSubEtapa(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            EtapaProyecto etapaPadre = ObtenerEtapaCotizacionPorNombre(sub.EtapaPadre);

            if (etapaPadre == null)
            {
                return;
            }

            ActualizarEstadoEtapaSegunSubEtapas(etapaPadre);

            if (etapaPadre.Seleccionada)
            {
                ExpandirEtapaParaContenerSubEtapas(etapaPadre);
            }

            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();
        }

        private void ActualizarEstadoEtapaSegunSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (!TieneSubEtapasActivas(etapa))
            {
                DesactivarEtapaSinSubEtapas(etapa);
                return;
            }

            etapa.Seleccionada = true;

            if (etapa.DuracionMeses < 0.0)
            {
                etapa.DuracionMeses = 0.0;
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            if (etapa.FinMes < etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes;
            }

            /*
             * NO:
             * etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
             */
        }

        private bool TieneSubEtapasActivas(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return false;
            }

            return bibliotecaSubEtapas.Any(s =>
                NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre) &&
                s.Activa
            );
        }

        private void DesactivarEtapaSinSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            etapa.Seleccionada = false;
            etapa.DuracionMeses = 0.0;
            etapa.InicioMes = 0.0;
            etapa.FinMes = 0.0;

            if (etapa.Plan != null)
            {
                etapa.Plan.Clear();
            }
        }

        private void DesactivarEtapaYSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (bibliotecaSubEtapas != null)
            {
                var subEtapasDeLaEtapa = bibliotecaSubEtapas
                    .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
                {
                    sub.Activa = false;
                    sub.Requerida = false;
                }
            }

            etapa.Seleccionada = false;
            etapa.DuracionMeses = 0.0;
            etapa.InicioMes = 0.0;
            etapa.FinMes = 0.0;

            if (etapa.Plan != null)
            {
                etapa.Plan.Clear();
            }
        }

        private void ExpandirEtapaParaContenerSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return;
            }

            List<SubEtapaProyecto> subEtapasDeLaEtapa = bibliotecaSubEtapas
                .Where(s =>
                    s != null &&
                    NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .Where(s => s.Activa)
                .ToList();

            if (subEtapasDeLaEtapa.Count == 0)
            {
                DesactivarEtapaSinSubEtapas(etapa);
                return;
            }

            double menorInicioSemana = double.MaxValue;
            double mayorFinSemana = 1.0;

            foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
            {
                if (sub.InicioSemana < 1.0)
                {
                    sub.InicioSemana = 1.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }

                if (sub.InicioSemana < menorInicioSemana)
                {
                    menorInicioSemana = sub.InicioSemana;
                }

                if (sub.FinSemana > mayorFinSemana)
                {
                    mayorFinSemana = sub.FinSemana;
                }
            }

            if (menorInicioSemana == double.MaxValue)
            {
                menorInicioSemana = 1.0;
            }

            /*
             * SubEtapaProyecto ahora trabaja con semanas decimales.
             * La etapa debe contener el rango real de sus subetapas,
             * no inflarse por redondeos tempranos.
             */
            double inicioMesNecesario =
                (menorInicioSemana - 1.0) / 4.0;

            double finMesNecesario =
                mayorFinSemana / 4.0;

            double duracionMesesNecesaria =
                finMesNecesario - inicioMesNecesario;

            if (duracionMesesNecesaria <= 0.0)
            {
                duracionMesesNecesaria = 0.25;
            }

            etapa.Seleccionada = true;
            etapa.InicioMes = inicioMesNecesario;
            etapa.DuracionMeses = duracionMesesNecesaria;
            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

            if (etapa.FinMes <= etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes + 0.25;
                etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;
            }
        }
        private void NormalizarTodasLasEtapasPorSubEtapas()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas
                .OrderBy(e => ObtenerOrdenEtapaPorNombre(e.Nombre)))
            {
                if (etapa == null)
                {
                    continue;
                }

                if (!TieneSubEtapasActivas(etapa))
                {
                    DesactivarEtapaSinSubEtapas(etapa);
                    continue;
                }

                etapa.Seleccionada = true;

                if (etapa.DuracionMeses < 0.0)
                {
                    etapa.DuracionMeses = 0.0;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                if (etapa.FinMes < etapa.InicioMes)
                {
                    etapa.FinMes = etapa.InicioMes;
                }

                /*
                 * NO:
                 * etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                 */

                ExpandirEtapaParaContenerSubEtapas(etapa);
            }

            ValidarPrecedenciaTemporalEtapasDesdeSubEtapas();
        }

        private void ValidarPrecedenciaTemporalEtapasDesdeSubEtapas()
{
    ValidarPrecedenciaTemporalEtapasSinInflarDuracion();
}

        private string LeerCeldaVistaSubEtapas(DataGridViewRow row, string columna)
        {
            if (row == null || !dgvSubEtapas.Columns.Contains(columna))
            {
                return "";
            }

            object valor = row.Cells[columna].Value;

            if (valor == null)
            {
                return "";
            }

            return valor.ToString() ?? "";
        }

        private bool EtapaEstaExpandidaEnVistaSubEtapas(EtapaProyecto etapa)
        {
            if (etapa == null || etapaExpandidaEnTabla == null)
            {
                return false;
            }

            return NormalizarNombreEtapa(etapaExpandidaEnTabla.Nombre) ==
                   NormalizarNombreEtapa(etapa.Nombre);
        }

        private void MantenerEtapaPadreExpandida(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            EtapaProyecto etapaPadre = ObtenerEtapaCotizacionPorNombre(sub.EtapaPadre);

            if (etapaPadre != null)
            {
                etapaExpandidaEnTabla = etapaPadre;
            }
        }

        private string ObtenerTextoEstadoSubEtapaVista(SubEtapaProyecto sub, EtapaProyecto etapaPadre)
        {
            if (sub == null)
            {
                return "";
            }

            if (!sub.Activa)
            {
                if (etapaPadre != null && !etapaPadre.Seleccionada)
                {
                    return "Recomendado / pendiente";
                }

                return "Subproceso excluido";
            }

            string textoBase = sub.Requerida
                ? "Objetivo requerido"
                : "Objetivo opcional";

            if (bibliotecaSubEtapas == null)
            {
                return textoBase;
            }

            var alertas = BibliotecaDependenciasSubEtapasService.ValidarDependenciasDeSubEtapa(
                bibliotecaSubEtapas,
                sub,
                resolucionesDependenciasSubEtapas
            );

            if (alertas == null || alertas.Count == 0)
            {
                return textoBase;
            }

            bool tieneBloqueante = alertas.Any(a => a.Bloqueante);
            bool tieneRiesgoAlto = alertas.Any(a =>
                (a.Severidad ?? "").ToLowerInvariant().Contains("riesgo")
            );

            if (tieneBloqueante)
            {
                return textoBase + " ⛔ Insumo faltante";
            }

            if (tieneRiesgoAlto)
            {
                return textoBase + " ⚠ Falta insumo crítico";
            }

            return textoBase + " ⚠ Falta insumo";
        }

        private int ObtenerSemanasDisponiblesParaEtapa(EtapaProyecto etapa)
        {
            if (etapa == null || !etapa.Seleccionada || etapa.DuracionMeses <= 0.0)
            {
                return 1;
            }

            int semanas = (int)Math.Ceiling(etapa.DuracionMeses * 4.0);

            if (semanas < 1)
            {
                semanas = 1;
            }

            return semanas;
        }

        private void AsegurarSemanasValidasSubEtapa(SubEtapaProyecto sub, int semanasEtapa)
        {
            if (sub == null)
            {
                return;
            }

            if (sub.InicioSemana < 1)
            {
                sub.InicioSemana = 1;
            }

            if (sub.DuracionSemanas < 1)
            {
                sub.DuracionSemanas = 1;
            }
        }

        private int ParsearSemanaSubVista(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 1;
            }

            string limpio = texto
                .Trim()
                .ToUpperInvariant()
                .Replace("SEMANAS", "")
                .Replace("SEMANA", "")
                .Replace("SEM", "")
                .Replace("S", "")
                .Trim();

            int valor;

            if (!int.TryParse(limpio, out valor))
            {
                return 1;
            }

            return valor < 1 ? 1 : valor;
        }

        private int ParsearDuracionSemanasSubVista(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 1;
            }

            string limpio = texto
                .Trim()
                .ToLowerInvariant()
                .Replace("semanas", "")
                .Replace("semana", "")
                .Replace("sems", "")
                .Replace("sem", "")
                .Trim();

            int valor;

            if (!int.TryParse(limpio, out valor))
            {
                return 1;
            }

            return valor < 1 ? 1 : valor;
        }

        private void BtnAplicarSubEtapas_Click(object sender, EventArgs e)
        {
            BibliotecaSubEtapasJsonService.GuardarSubEtapas(bibliotecaSubEtapas);
            ProgramarRefrescoSubEtapas();

            MessageBox.Show(
                "Biblioteca de subprocesos guardada.",
                "Subprocesos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void BtnRestaurarSubEtapas_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                "¿Restaurar la biblioteca base de subetapas? Se perderán cambios manuales.",
                "Restaurar subetapas",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            BibliotecaSubEtapasJsonService.RegenerarSubEtapasDesdeBase();
            bibliotecaSubEtapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();
            subEtapasBasePreparadas = false;
            PrepararSubEtapasComoRecomendadasIniciales();
            ProgramarRefrescoSubEtapas();
        }

        private void CmbFiltroEtapaSubEtapas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProgramarRefrescoSubEtapas();
        }

        private void DgvSubEtapas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ya no es necesario el doble click.
        }

        private string ObtenerCargosDisponiblesParaSubEtapa(SubEtapaProyecto sub)
        {
            EtapaProyecto etapa = cotizacion.Etapas
                .FirstOrDefault(e => NormalizarNombreEtapa(e.Nombre) == NormalizarNombreEtapa(sub.EtapaPadre));

            if (etapa == null || etapa.Biblioteca == null)
            {
                return "";
            }

            string sugeridos = sub.CargosSugeridos.ToLowerInvariant();

            var coincidencias = etapa.Biblioteca
                .Where(c => sugeridos.Contains((c.Nombre ?? "").ToLowerInvariant()) ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("director") && sugeridos.Contains("director") ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("project") && sugeridos.Contains("project") ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("productor") && sugeridos.Contains("productor") ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("animador") && sugeridos.Contains("animador") ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("editor") && sugeridos.Contains("editor") ||
                            (c.Nombre ?? "").ToLowerInvariant().Contains("compositor") && sugeridos.Contains("compositor"))
                .Select(c => c.Nombre)
                .Distinct()
                .ToList();

            if (coincidencias.Count == 0)
            {
                return "Sin coincidencias exactas";
            }

            return string.Join("; ", coincidencias);
        }

        private EtapaProyecto ObtenerEtapaCotizacionPorNombre(string nombreEtapa)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return null;
            }

            string nombreNormalizado = NormalizarNombreEtapa(nombreEtapa);

            return cotizacion.Etapas.FirstOrDefault(e =>
                NormalizarNombreEtapa(e.Nombre) == nombreNormalizado
            );
        }

        private string NormalizarNombreEtapa(string nombre)
        {
            EtapaDefinicion definicion = ObtenerDefinicionEtapa(nombre);

            if (definicion != null)
            {
                return NormalizarNombreEtapaColor(definicion.Clave);
            }

            return (nombre ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("ó", "o")
                .Replace("í", "i")
                .Replace("é", "e")
                .Replace("á", "a")
                .Replace("ú", "u")
                .Replace(" ", "");
        }

        private void ActivarActualizacionGanttEnVivo()
        {
            if (dgvEtapas == null)
            {
                return;
            }

            /*
             * IMPORTANTE:
             * No sincronizar el Gantt mientras el usuario escribe.
             *
             * Antes se usaba CellValueChanged y EditingControlShowing/TextChanged.
             * Eso destruía la edición de decimales:
             *
             * Usuario escribe: 1
             * Se dispara sync.
             * La tabla refresca.
             * Nunca alcanza a escribir: ,5
             *
             * Ahora el Gantt se actualiza:
             * - al terminar edición;
             * - al hacer click;
             * - al cambiar checkbox.
             */

            dgvEtapas.CellEndEdit -= DgvEtapas_Gantt_CellEndEdit;
            dgvEtapas.CellEndEdit += DgvEtapas_Gantt_CellEndEdit;

            dgvEtapas.CellValueChanged -= DgvEtapas_Gantt_CellValueChanged;

            dgvEtapas.CellClick -= DgvEtapas_Gantt_CellClick;
            dgvEtapas.CellClick += DgvEtapas_Gantt_CellClick;

            dgvEtapas.CurrentCellDirtyStateChanged -= DgvEtapas_Gantt_CurrentCellDirtyStateChanged;
            dgvEtapas.CurrentCellDirtyStateChanged += DgvEtapas_Gantt_CurrentCellDirtyStateChanged;

            dgvEtapas.EditingControlShowing -= DgvEtapas_Gantt_EditingControlShowing;
        }

        private void DgvEtapas_Gantt_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            SincronizarFilaEtapasConModeloParaGantt(e.RowIndex, null);
            RefrescarGanttGrandeEtapas();
        }

        private void DgvEtapas_Gantt_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            /*
             * Intencionalmente vacío.
             *
             * No sincronizamos aquí porque CellValueChanged puede ocurrir
             * mientras el usuario todavía está editando la celda.
             *
             * La sincronización real ocurre en CellEndEdit.
             */
        }

        private void DgvEtapas_Gantt_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            /*
             * El click puede activar/desactivar subetapas o expandir/contraer.
             * Por eso refrescamos altiro y también dejamos una pasada diferida,
             * para tomar el resultado después de que los otros handlers terminen.
             */

            SincronizarTodasLasFilasEtapasConModeloParaGantt();
            RefrescarGanttGrandeEtapas();

            RefrescarGanttGrandeEtapasDiferido();
        }

        private void DgvEtapas_Gantt_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvEtapas == null)
            {
                return;
            }

            if (dgvEtapas.IsCurrentCellDirty)
            {
                dgvEtapas.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvEtapas_Gantt_EditingControlShowing(
            object sender,
            DataGridViewEditingControlShowingEventArgs e
        )
        {
            if (dgvEtapas == null || dgvEtapas.CurrentCell == null)
            {
                return;
            }

            if (!(e.Control is TextBox textBox))
            {
                return;
            }

            textBox.TextChanged -= TextBoxEtapas_Gantt_TextChanged;

            string columna = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;

            if (columna == ColEtapaInicio ||
                columna == ColEtapaDuracion ||
                columna == ColEtapaFin)
            {
                textBox.TextChanged += TextBoxEtapas_Gantt_TextChanged;
            }
        }

        private void TextBoxEtapas_Gantt_TextChanged(object sender, EventArgs e)
        {
            if (cargandoTabla || dgvEtapas == null || dgvEtapas.CurrentCell == null)
            {
                return;
            }

            if (!(sender is TextBox textBox))
            {
                return;
            }

            int rowIndex = dgvEtapas.CurrentCell.RowIndex;

            if (rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                return;
            }

            /*
             * Este es el punto clave:
             * mientras escribes S2, S3, 2 sem, etc.,
             * todavía no se ha guardado en la celda.
             * Por eso usamos textBox.Text como valor temporal.
             */

            SincronizarFilaEtapasConModeloParaGantt(rowIndex, textBox.Text);
            RefrescarGanttGrandeEtapas();
        }

        private void RefrescarGanttGrandeEtapasDiferido()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                SincronizarTodasLasFilasEtapasConModeloParaGantt();
                RefrescarGanttGrandeEtapas();
            }));
        }

        private void SincronizarTodasLasFilasEtapasConModeloParaGantt()
        {
            if (dgvEtapas == null)
            {
                return;
            }

            for (int i = 0; i < dgvEtapas.Rows.Count; i++)
            {
                SincronizarFilaEtapasConModeloParaGantt(i, null);
            }
        }

        private void SincronizarFilaEtapasConModeloParaGantt(int rowIndex, string valorTemporalEdicion)
        {
            if (dgvEtapas == null || rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[rowIndex];

            if (row == null || row.IsNewRow)
            {
                return;
            }

            if (row.Tag is SubEtapaProyecto sub)
            {
                SincronizarSubEtapaDesdeFilaParaGantt(row, sub, valorTemporalEdicion);
                return;
            }

            if (row.Tag is EtapaProyecto etapa)
            {
                SincronizarEtapaDesdeFilaParaGantt(row, etapa, valorTemporalEdicion);
                return;
            }
        }

        private void SincronizarSubEtapaDesdeFilaParaGantt(
    DataGridViewRow row,
    SubEtapaProyecto sub,
    string valorTemporalEdicion
)
        {
            if (row == null || sub == null)
            {
                return;
            }

            string columnaActual = "";

            if (dgvEtapas != null && dgvEtapas.CurrentCell != null)
            {
                columnaActual = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;
            }

            double inicioAnterior = sub.InicioSemana <= 0.0 ? 1.0 : sub.InicioSemana;
            double duracionAnterior = sub.DuracionSemanas <= 0.0 ? 0.1 : sub.DuracionSemanas;

            object valorInicio = row.Cells[ColEtapaInicio].Value;
            object valorDuracion = row.Cells[ColEtapaDuracion].Value;

            if (!string.IsNullOrWhiteSpace(valorTemporalEdicion))
            {
                if (columnaActual == ColEtapaInicio)
                {
                    valorInicio = valorTemporalEdicion;
                }
                else if (columnaActual == ColEtapaDuracion)
                {
                    valorDuracion = valorTemporalEdicion;
                }
            }

            sub.InicioSemana = ParsearSemanaDesdeCelda(valorInicio, inicioAnterior);
            sub.DuracionSemanas = ParsearSemanaDesdeCelda(valorDuracion, duracionAnterior);

            if (sub.InicioSemana < 1.0)
            {
                sub.InicioSemana = 1.0;
            }

            if (sub.DuracionSemanas <= 0.0)
            {
                sub.DuracionSemanas = 0.1;
            }
        }
        private void SincronizarEtapaDesdeFilaParaGantt(
            DataGridViewRow row,
            EtapaProyecto etapa,
            string valorTemporalEdicion
        )
        {
            if (row == null || etapa == null)
            {
                return;
            }

            string columnaActual = "";

            if (dgvEtapas != null && dgvEtapas.CurrentCell != null)
            {
                columnaActual = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;
            }

            object valorInicio = row.Cells[ColEtapaInicio].Value;
            object valorDuracion = row.Cells[ColEtapaDuracion].Value;
            object valorFin = row.Cells[ColEtapaFin].Value;

            if (!string.IsNullOrWhiteSpace(valorTemporalEdicion))
            {
                if (columnaActual == ColEtapaInicio)
                {
                    valorInicio = valorTemporalEdicion;
                }
                else if (columnaActual == ColEtapaDuracion)
                {
                    valorDuracion = valorTemporalEdicion;
                }
                else if (columnaActual == ColEtapaFin)
                {
                    valorFin = valorTemporalEdicion;
                }
            }

            etapa.InicioMes = ParsearMesesDesdeTextoEtapa(
                Convert.ToString(valorInicio),
                etapa.InicioMes
            );
            etapa.DuracionMeses = ParsearMesesDesdeTextoEtapa(
                Convert.ToString(valorDuracion),
                etapa.DuracionMeses
            );
            etapa.FinMes = ParsearMesesDesdeTextoEtapa(
                Convert.ToString(valorFin),
                etapa.FinMes
            );

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            if (etapa.DuracionMeses < 0.0)
            {
                etapa.DuracionMeses = 0.0;
            }

            if (etapa.FinMes <= etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }
        }

        private int ObtenerOrdenEtapaPorNombre(string nombreEtapa)
        {
            return ObtenerOrdenEtapaGeneral(nombreEtapa);
        }
    }
}
