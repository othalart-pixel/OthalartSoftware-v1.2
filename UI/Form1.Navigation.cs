using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private bool EsTabBloqueado(TabPage tab)
        {
            if (tab == null)
            {
                return false;
            }

            bool requiereEtapa =
                tab.Text == "Mano de obra" ||
                tab.Text == "Costos" ||
                tab.Text == "Resultados" ||
                tab.Text == "Informe" ||
                tab.Text == "Guardar";

            return requiereEtapa && !ExisteEtapaValidaParaCotizar();
        }

        private void Tabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (EsTabBloqueado(e.TabPage))
            {
                e.Cancel = true;

                MessageBox.Show(
                    "Antes de continuar debe existir al menos una etapa activa calculada desde el desglose.",
                    "Faltan etapas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                tabs.SelectedIndex = 1;
            }
        }

        private void Tabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabs.TabPages[e.Index];
            Rectangle rect = e.Bounds;

            bool bloqueado = EsTabBloqueado(page);
            bool seleccionado = tabs.SelectedIndex == e.Index;

            Color fondo;
            Color texto;

            if (modoOscuroActivo)
            {
                fondo = bloqueado
                    ? Color.FromArgb(45, 45, 48)
                    : seleccionado
                        ? Color.FromArgb(74, 38, 75)
                        : Color.FromArgb(30, 30, 30);

                texto = bloqueado
                    ? Color.FromArgb(130, 130, 130)
                    : Color.FromArgb(220, 220, 220);
            }
            else
            {
                fondo = bloqueado
                    ? Color.FromArgb(220, 220, 220)
                    : seleccionado
                        ? Color.White
                        : SystemColors.Control;

                texto = bloqueado
                    ? Color.Gray
                    : Color.Black;
            }

            using (Brush brush = new SolidBrush(fondo))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                Font,
                rect,
                texto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private int CantidadMesesProyectoVisible()
        {
            double duracionVisible = DuracionVisibleProyecto();

            if (duracionVisible <= 0.0)
            {
                return 1;
            }

            int meses = (int)Math.Ceiling(duracionVisible);

            if (meses < 1)
            {
                meses = 1;
            }

            if (meses > MaxMesesTabla)
            {
                meses = MaxMesesTabla;
            }

            return meses;
        }

        private void AjustarColumnasMesesManoObra()
        {
            int mesesVisibles = CantidadMesesProyectoVisible();

            for (int i = 1; i <= MaxMesesTabla; i++)
            {
                string nombreColumna = "M" + i;

                if (dgvManoObra.Columns.Contains(nombreColumna))
                {
                    dgvManoObra.Columns[nombreColumna].Visible = i <= mesesVisibles;
                }
            }
        }

        private void ActualizarBloqueoPestanas()
        {
            tabs.Invalidate();
        }

        private bool EsTabBibliotecaOEditor(TabPage tab)
        {
            if (tab == null)
            {
                return false;
            }

            return tab == tabProductosPrincipal ||
                   tab == tabMonedaPrincipal ||
                   tab == tabRendimientosPrincipal ||
                   tab == tabEcuacionesPrincipal ||
                   tab == tabGestionesPrincipal ||
                   tab == tabValidacionJsonPrincipal ||
                   tab == tabSubEtapasPrincipal ||
                   tab == tabRangosPrincipal ||
                   tab == tabCargosPrincipal ||
                   tab == tabCostosPrincipal;
        }

        private void ActualizarVisibilidadPanelDerecho()
        {
            if (splitPrincipal == null || tabs == null || tabs.SelectedTab == null)
            {
                return;
            }

            bool ocultarDerecha = EsTabBibliotecaOEditor(tabs.SelectedTab);

            if (splitPrincipal.Panel2Collapsed != ocultarDerecha)
            {
                splitPrincipal.Panel2Collapsed = ocultarDerecha;
            }

            if (!ocultarDerecha)
            {
                AjustarAnchoPanelDerecho();
            }
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cambiandoTabInternamente)
            {
                return;
            }

            if (tabs.SelectedTab == null)
            {
                return;
            }

            string nombreTab = tabs.SelectedTab.Text;
            ActualizarVisibilidadPanelDerecho();

            bool requiereEtapa =
                nombreTab == "Mano de obra" ||
                nombreTab == "Costos" ||
                nombreTab == "Resultados";

            if (requiereEtapa && !ExisteEtapaValidaParaCotizar())
            {
                MessageBox.Show(
                    "Antes de continuar debe existir al menos una etapa activa calculada desde el desglose.",
                    "Faltan etapas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                cambiandoTabInternamente = true;
                tabs.SelectedIndex = 1;
                cambiandoTabInternamente = false;
            }
        }
    }
}
