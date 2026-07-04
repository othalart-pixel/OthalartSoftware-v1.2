using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private const string ColRangoEtapa = "ColRangoEtapa";
        private const string ColRangoSubproceso = "ColRangoSubproceso";
        private const string ColRangoMin = "ColRangoMin";
        private const string ColRangoRec = "ColRangoRec";
        private const string ColRangoMax = "ColRangoMax";
        private const string ColRangoLectura = "ColRangoLectura";

        private void ConstruirTabRangosSubEtapas(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.Padding = new Padding(20);
            root.BackColor = Color.White;

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Rangos recomendados de subprocesos";
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;
            titulo.Font = new Font("Segoe UI", 15, FontStyle.Bold);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Fill;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = false;

            btnGuardarRangosSubEtapas.Text = "Guardar rangos";
            btnGuardarRangosSubEtapas.Width = 130;
            btnGuardarRangosSubEtapas.Height = 30;
            btnGuardarRangosSubEtapas.Margin = new Padding(0, 4, 8, 0);
            btnGuardarRangosSubEtapas.Click -= BtnGuardarRangosSubEtapas_Click;
            btnGuardarRangosSubEtapas.Click += BtnGuardarRangosSubEtapas_Click;

            btnRestaurarRangosSubEtapas.Text = "Restaurar base";
            btnRestaurarRangosSubEtapas.Width = 115;
            btnRestaurarRangosSubEtapas.Height = 30;
            btnRestaurarRangosSubEtapas.Margin = new Padding(0, 4, 0, 0);
            btnRestaurarRangosSubEtapas.Click -= BtnRestaurarRangosSubEtapas_Click;
            btnRestaurarRangosSubEtapas.Click += BtnRestaurarRangosSubEtapas_Click;

            acciones.Controls.Add(btnGuardarRangosSubEtapas);
            acciones.Controls.Add(btnRestaurarRangosSubEtapas);

            ConfigurarGrillaRangosSubEtapas();

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(acciones, 0, 1);
            root.Controls.Add(dgvRangosSubEtapas, 0, 2);

            tab.Controls.Add(root);
            CargarRangosSubEtapasEnPantalla();
        }

        private void ConfigurarGrillaRangosSubEtapas()
        {
            dgvRangosSubEtapas.Dock = DockStyle.Fill;
            dgvRangosSubEtapas.AllowUserToAddRows = false;
            dgvRangosSubEtapas.AllowUserToDeleteRows = false;
            dgvRangosSubEtapas.AllowUserToResizeRows = false;
            dgvRangosSubEtapas.RowHeadersVisible = false;
            dgvRangosSubEtapas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvRangosSubEtapas.MultiSelect = false;
            dgvRangosSubEtapas.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvRangosSubEtapas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRangosSubEtapas.BackgroundColor = Color.White;
            dgvRangosSubEtapas.BorderStyle = BorderStyle.FixedSingle;
            dgvRangosSubEtapas.GridColor = Color.Gainsboro;

            dgvRangosSubEtapas.Columns.Clear();
            dgvRangosSubEtapas.Columns.Add(ColRangoEtapa, "Etapa");
            dgvRangosSubEtapas.Columns.Add(ColRangoSubproceso, "Subproceso");
            dgvRangosSubEtapas.Columns.Add(ColRangoMin, "Mínimo");
            dgvRangosSubEtapas.Columns.Add(ColRangoRec, "Recomendado");
            dgvRangosSubEtapas.Columns.Add(ColRangoMax, "Máximo");
            dgvRangosSubEtapas.Columns.Add(ColRangoLectura, "Lectura");

            dgvRangosSubEtapas.Columns[ColRangoEtapa].ReadOnly = true;
            dgvRangosSubEtapas.Columns[ColRangoSubproceso].ReadOnly = true;
            dgvRangosSubEtapas.Columns[ColRangoLectura].ReadOnly = true;

            dgvRangosSubEtapas.Columns[ColRangoEtapa].FillWeight = 14;
            dgvRangosSubEtapas.Columns[ColRangoSubproceso].FillWeight = 28;
            dgvRangosSubEtapas.Columns[ColRangoMin].FillWeight = 12;
            dgvRangosSubEtapas.Columns[ColRangoRec].FillWeight = 12;
            dgvRangosSubEtapas.Columns[ColRangoMax].FillWeight = 12;
            dgvRangosSubEtapas.Columns[ColRangoLectura].FillWeight = 22;

            foreach (DataGridViewColumn col in dgvRangosSubEtapas.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvRangosSubEtapas.CellEndEdit -= DgvRangosSubEtapas_CellEndEdit;
            dgvRangosSubEtapas.CellEndEdit += DgvRangosSubEtapas_CellEndEdit;

            dgvRangosSubEtapas.EditingControlShowing -= DgvRangosSubEtapas_EditingControlShowing;
            dgvRangosSubEtapas.EditingControlShowing += DgvRangosSubEtapas_EditingControlShowing;

            AplicarEstiloTablaCentrada(dgvRangosSubEtapas);
        }

        private void CargarRangosSubEtapasEnPantalla()
        {
            if (dgvRangosSubEtapas == null || dgvRangosSubEtapas.Columns.Count == 0)
            {
                return;
            }

            dgvRangosSubEtapas.Rows.Clear();

            foreach (SubEtapaProyecto sub in bibliotecaSubEtapas
                .OrderBy(s => ObtenerOrdenEtapaParaCargos(s.EtapaPadre))
                .ThenBy(s => s.Orden))
            {
                int idx = dgvRangosSubEtapas.Rows.Add();
                DataGridViewRow row = dgvRangosSubEtapas.Rows[idx];

                row.Cells[ColRangoEtapa].Value = sub.EtapaPadre;
                row.Cells[ColRangoSubproceso].Value = sub.Nombre;
                row.Cells[ColRangoMin].Value = FormatearPonderadorSubEtapa(sub.PorcentajeMinimoEtapa);
                row.Cells[ColRangoRec].Value = FormatearPonderadorSubEtapa(sub.PorcentajeRecomendadoEtapa);
                row.Cells[ColRangoMax].Value = FormatearPonderadorSubEtapa(sub.PorcentajeMaximoEtapa);
                row.Cells[ColRangoLectura].Value = ConstruirLecturaRango(sub);
                row.Tag = sub;

                row.Cells[ColRangoMin].Style.BackColor = Color.FromArgb(255, 255, 235);
                row.Cells[ColRangoRec].Style.BackColor = Color.FromArgb(255, 255, 235);
                row.Cells[ColRangoMax].Style.BackColor = Color.FromArgb(255, 255, 235);
            }
        }

        private string ConstruirLecturaRango(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return "";
            }

            return "Usualmente entre " +
                   FormatearPonderadorSubEtapa(sub.PorcentajeMinimoEtapa) +
                   " y " +
                   FormatearPonderadorSubEtapa(sub.PorcentajeMaximoEtapa) +
                   "; ideal " +
                   FormatearPonderadorSubEtapa(sub.PorcentajeRecomendadoEtapa);
        }

        private void DgvRangosSubEtapas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvRangosSubEtapas.Rows[e.RowIndex];
            SubEtapaProyecto sub = row.Tag as SubEtapaProyecto;

            if (sub == null)
            {
                return;
            }

            sub.PorcentajeMinimoEtapa = ParsearPonderadorSubEtapa(Convert.ToString(row.Cells[ColRangoMin].Value), sub.PorcentajeMinimoEtapa);
            sub.PorcentajeRecomendadoEtapa = ParsearPonderadorSubEtapa(Convert.ToString(row.Cells[ColRangoRec].Value), sub.PorcentajeRecomendadoEtapa);
            sub.PorcentajeMaximoEtapa = ParsearPonderadorSubEtapa(Convert.ToString(row.Cells[ColRangoMax].Value), sub.PorcentajeMaximoEtapa);

            OrdenarPonderadoresSubEtapa(sub);

            row.Cells[ColRangoMin].Value = FormatearPonderadorSubEtapa(sub.PorcentajeMinimoEtapa);
            row.Cells[ColRangoRec].Value = FormatearPonderadorSubEtapa(sub.PorcentajeRecomendadoEtapa);
            row.Cells[ColRangoMax].Value = FormatearPonderadorSubEtapa(sub.PorcentajeMaximoEtapa);
            row.Cells[ColRangoLectura].Value = ConstruirLecturaRango(sub);

            BibliotecaSubEtapasJsonService.GuardarSubEtapas(bibliotecaSubEtapas);
            RefrescarTablaSubEtapasSiExiste();
        }

        private void DgvRangosSubEtapas_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox tb = e.Control as TextBox;

            if (tb == null)
            {
                return;
            }

            tb.KeyPress -= DgvSubEtapasDecimal_KeyPress;

            if (dgvRangosSubEtapas.CurrentCell == null)
            {
                return;
            }

            string col = dgvRangosSubEtapas.Columns[dgvRangosSubEtapas.CurrentCell.ColumnIndex].Name;

            if (col == ColRangoMin || col == ColRangoRec || col == ColRangoMax)
            {
                tb.KeyPress += DgvSubEtapasDecimal_KeyPress;
            }
        }

        private void BtnGuardarRangosSubEtapas_Click(object sender, EventArgs e)
        {
            BibliotecaSubEtapasJsonService.GuardarSubEtapas(bibliotecaSubEtapas);
            CargarRangosSubEtapasEnPantalla();
            RefrescarTablaSubEtapasSiExiste();

            MessageBox.Show(
                "Rangos de subprocesos guardados.",
                "Rangos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void BtnRestaurarRangosSubEtapas_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                "¿Restaurar los rangos base de subprocesos?",
                "Restaurar rangos",
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
            CargarRangosSubEtapasEnPantalla();
            RefrescarTablaSubEtapasSiExiste();
        }
    }
}
