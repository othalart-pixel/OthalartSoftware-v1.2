using System;
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
        private void ConstruirPreviewSubEtapasEnTabEtapas(TabPage tab)
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                bibliotecaSubEtapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();
            }

            panelPreviewSubEtapas.Controls.Clear();

            panelPreviewSubEtapas.Dock = DockStyle.Bottom;
            panelPreviewSubEtapas.Height = 190;
            panelPreviewSubEtapas.Padding = new Padding(14, 10, 14, 10);
            panelPreviewSubEtapas.BackColor = Color.FromArgb(245, 245, 245);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 2;
            layout.Margin = new Padding(0);

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblPreviewSubEtapasTitulo.Text = "Sub-Gantt de objetivos";
            lblPreviewSubEtapasTitulo.Dock = DockStyle.Fill;
            lblPreviewSubEtapasTitulo.TextAlign = ContentAlignment.MiddleLeft;
            lblPreviewSubEtapasTitulo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPreviewSubEtapasTitulo.ForeColor = Color.FromArgb(35, 35, 35);

            btnIrSubEtapasDesdeEtapas.Text = "Ver detalle";
            btnIrSubEtapasDesdeEtapas.Dock = DockStyle.Right;
            btnIrSubEtapasDesdeEtapas.Width = 180;
            btnIrSubEtapasDesdeEtapas.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnIrSubEtapasDesdeEtapas.Click -= BtnIrSubEtapasDesdeEtapas_Click;
            btnIrSubEtapasDesdeEtapas.Click += BtnIrSubEtapasDesdeEtapas_Click;

            dgvPreviewSubEtapas.Dock = DockStyle.Fill;
            dgvPreviewSubEtapas.AllowUserToAddRows = false;
            dgvPreviewSubEtapas.AllowUserToDeleteRows = false;
            dgvPreviewSubEtapas.RowHeadersVisible = false;
            dgvPreviewSubEtapas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPreviewSubEtapas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPreviewSubEtapas.MultiSelect = false;
            dgvPreviewSubEtapas.ReadOnly = true;
            dgvPreviewSubEtapas.BackgroundColor = Color.White;
            dgvPreviewSubEtapas.BorderStyle = BorderStyle.FixedSingle;

            dgvPreviewSubEtapas.Columns.Clear();
            dgvPreviewSubEtapas.Columns.Add("Objetivo", "Objetivo");
            dgvPreviewSubEtapas.Columns.Add("Inicio", "Inicio");
            dgvPreviewSubEtapas.Columns.Add("Fin", "Fin");
            dgvPreviewSubEtapas.Columns.Add("Duracion", "Duración");

            dgvPreviewSubEtapas.Columns["Objetivo"].FillWeight = 48;
            dgvPreviewSubEtapas.Columns["Inicio"].FillWeight = 16;
            dgvPreviewSubEtapas.Columns["Fin"].FillWeight = 16;
            dgvPreviewSubEtapas.Columns["Duracion"].FillWeight = 20;

            AplicarEstiloTablaCentrada(dgvPreviewSubEtapas);

            panelPreviewSubGantt.Dock = DockStyle.Fill;
            panelPreviewSubGantt.BackColor = Color.White;
            panelPreviewSubGantt.BorderStyle = BorderStyle.FixedSingle;
            panelPreviewSubGantt.Paint -= PanelPreviewSubGantt_Paint;
            panelPreviewSubGantt.Paint += PanelPreviewSubGantt_Paint;

            FlowLayoutPanel panelBoton = new FlowLayoutPanel();
            panelBoton.Dock = DockStyle.Fill;
            panelBoton.FlowDirection = FlowDirection.RightToLeft;
            panelBoton.Controls.Add(btnIrSubEtapasDesdeEtapas);

            layout.Controls.Add(lblPreviewSubEtapasTitulo, 0, 0);
            layout.Controls.Add(panelBoton, 1, 0);
            layout.Controls.Add(dgvPreviewSubEtapas, 0, 1);
            layout.Controls.Add(panelPreviewSubGantt, 1, 1);

            panelPreviewSubEtapas.Controls.Add(layout);

            tab.Controls.Add(panelPreviewSubEtapas);
            panelPreviewSubEtapas.BringToFront();

            ActualizarPreviewSubEtapasDesdeSeleccion();
        }

        private void BtnIrSubEtapasDesdeEtapas_Click(object? sender, EventArgs e)
        {
            IrATabPorNombre("Subetapas");
        }

        private void ActualizarPreviewSubEtapasDesdeSeleccion()
        {
            if (dgvEtapas == null || dgvEtapas.CurrentRow == null)
            {
                MostrarSubGanttEtapa(null);
                return;
            }

            EtapaProyecto etapa = dgvEtapas.CurrentRow.Tag as EtapaProyecto;
            MostrarSubGanttEtapa(etapa);
        }

        private void ActualizarPreviewSubEtapasDesdeFila(int rowIndex)
        {
            if (dgvEtapas == null || rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                MostrarSubGanttEtapa(null);
                return;
            }

            EtapaProyecto etapa = dgvEtapas.Rows[rowIndex].Tag as EtapaProyecto;
            MostrarSubGanttEtapa(etapa);
        }

        private void MostrarSubGanttEtapa(EtapaProyecto? etapa)
        {
            etapaSeleccionadaParaSubGantt = etapa;

            if (dgvPreviewSubEtapas == null || dgvPreviewSubEtapas.Columns.Count == 0)
            {
                return;
            }

            dgvPreviewSubEtapas.Rows.Clear();

            if (etapa == null)
            {
                lblPreviewSubEtapasTitulo.Text = "Sub-Gantt de objetivos";
                panelPreviewSubGantt.Invalidate();
                return;
            }

            lblPreviewSubEtapasTitulo.Text = "Sub-Gantt de objetivos: " + etapa.Nombre;

            if (!etapa.Seleccionada || etapa.DuracionMeses <= 0.0)
            {
                panelPreviewSubGantt.Invalidate();
                return;
            }

            var subEtapas = bibliotecaSubEtapas
                .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .Where(s => s.Activa)
                .OrderBy(s => s.Orden)
                .ToList();

            if (subEtapas.Count == 0)
            {
                panelPreviewSubGantt.Invalidate();
                return;
            }

            double inicioActual = etapa.InicioMes;
            double finEtapa = etapa.FinMes;
            double duracionEtapa = etapa.DuracionMeses;

            double sumaRecomendada = subEtapas.Sum(s => s.PorcentajeRecomendadoEtapa);

            if (sumaRecomendada <= 0.0)
            {
                sumaRecomendada = subEtapas.Count;
            }

            foreach (SubEtapaProyecto sub in subEtapas)
            {
                double peso = sub.PorcentajeRecomendadoEtapa > 0.0
                    ? sub.PorcentajeRecomendadoEtapa / sumaRecomendada
                    : 1.0 / subEtapas.Count;

                double duracionSub = duracionEtapa * peso;

                if (duracionSub < 0.1)
                {
                    duracionSub = 0.1;
                }

                double inicioSub = inicioActual;
                double finSub = inicioSub + duracionSub;

                if (finSub > finEtapa)
                {
                    finSub = finEtapa;
                    duracionSub = finSub - inicioSub;
                }

                int rowIndex = dgvPreviewSubEtapas.Rows.Add();
                DataGridViewRow row = dgvPreviewSubEtapas.Rows[rowIndex];

                row.Cells["Objetivo"].Value = sub.Requerida
                    ? "● " + sub.Nombre
                    : "○ " + sub.Nombre;

                row.Cells["Inicio"].Value = inicioSub.ToString("0.##");
                row.Cells["Fin"].Value = finSub.ToString("0.##");
                row.Cells["Duracion"].Value = duracionSub.ToString("0.##");

                row.Tag = new SubGanttPreviewItem
                {
                    SubEtapa = sub,
                    Inicio = inicioSub,
                    Fin = finSub,
                    Duracion = duracionSub
                };

                row.DefaultCellStyle.BackColor = ObtenerColorFilaEtapa(etapa.Nombre);
                row.DefaultCellStyle.SelectionBackColor = ObtenerColorFilaEtapa(etapa.Nombre);
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
                row.Cells["Objetivo"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                inicioActual = finSub;
            }

            panelPreviewSubGantt.Invalidate();
        }

        private void PanelPreviewSubGantt_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            using Font font = new Font("Segoe UI", 9);
            using Font titleFont = new Font("Segoe UI", 9, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.Black);

            if (etapaSeleccionadaParaSubGantt == null ||
                !etapaSeleccionadaParaSubGantt.Seleccionada ||
                etapaSeleccionadaParaSubGantt.DuracionMeses <= 0.0)
            {
                g.DrawString("Selecciona una etapa activa para ver sus objetivos.", font, textBrush, 12, 12);
                return;
            }

            EtapaProyecto etapa = etapaSeleccionadaParaSubGantt;

            var items = dgvPreviewSubEtapas.Rows
                .Cast<DataGridViewRow>()
                .Select(r => r.Tag as SubGanttPreviewItem)
                .Where(i => i != null)
                .ToList();

            if (items.Count == 0)
            {
                g.DrawString("Sin objetivos activos para esta etapa.", font, textBrush, 12, 12);
                return;
            }

            int marginLeft = 145;
            int marginRight = 20;
            int marginTop = 30;
            int rowHeight = 27;
            int barHeight = 18;

            double inicioEtapa = etapa.InicioMes;
            double finEtapa = etapa.FinMes;
            double duracionEtapa = Math.Max(0.01, finEtapa - inicioEtapa);

            int usableWidth = panelPreviewSubGantt.Width - marginLeft - marginRight;

            if (usableWidth <= 20)
            {
                return;
            }

            g.DrawString("Objetivos internos", titleFont, textBrush, 12, 8);

            Color colorBase = ObtenerColorBarraEtapa(etapa.Nombre);
            Color colorBorde = ObtenerColorBordeEtapa(etapa.Nombre);
            Color colorFondo = MezclarConBlanco(colorBase, 0.80);

            using Brush fondoBrush = new SolidBrush(colorFondo);
            using Brush barraBrush = new SolidBrush(colorBase);
            using Pen bordePen = new Pen(colorBorde, 1.0f);

            for (int i = 0; i < items.Count; i++)
            {
                SubGanttPreviewItem item = items[i];
                SubEtapaProyecto sub = item.SubEtapa;

                int y = marginTop + i * rowHeight;

                string nombreCorto = sub.Nombre;

                if (nombreCorto.Length > 22)
                {
                    nombreCorto = nombreCorto.Substring(0, 22) + "...";
                }

                using Brush labelBrush = new SolidBrush(colorBorde);
                g.DrawString(nombreCorto, font, labelBrush, 10, y + 1);

                int x = marginLeft + (int)(((item.Inicio - inicioEtapa) / duracionEtapa) * usableWidth);
                int width = (int)((item.Duracion / duracionEtapa) * usableWidth);

                if (width < 4)
                {
                    width = 4;
                }

                Rectangle fondo = new Rectangle(marginLeft, y, usableWidth, barHeight);
                Rectangle barra = new Rectangle(x, y, width, barHeight);

                g.FillRectangle(fondoBrush, fondo);
                g.FillRectangle(barraBrush, barra);
                g.DrawRectangle(bordePen, barra);

                string texto = item.Inicio.ToString("0.##") + " - " + item.Fin.ToString("0.##");
                g.DrawString(texto, font, Brushes.Black, x + 4, y + 1);
            }
        }

        private void DgvEtapas_SelectionChanged(object? sender, EventArgs e)
        {
            if (cargandoTabla)
            {
                return;
            }

            ActualizarPreviewSubEtapasDesdeSeleccion();
        }

        private class SubGanttPreviewItem
        {
            public SubEtapaProyecto SubEtapa { get; set; } = new SubEtapaProyecto();
            public double Inicio { get; set; }
            public double Fin { get; set; }
            public double Duracion { get; set; }
        }
    }
}
