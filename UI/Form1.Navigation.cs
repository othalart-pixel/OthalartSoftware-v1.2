using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private EstadoNavegacionPrincipal ObtenerEstadoNavegacionActual()
        {
            if (estadoNavegacionActual != null)
            {
                return new EstadoNavegacionPrincipal
                {
                    Modo = estadoNavegacionActual.Modo,
                    Tab = estadoNavegacionActual.Tab
                };
            }

            if (tabs == null || tabs.SelectedTab == null)
            {
                return null;
            }

            return new EstadoNavegacionPrincipal
            {
                Modo = modoAplicacionActual,
                Tab = tabs.SelectedTab
            };
        }

        private void CompletarCambioNavegacion(
            EstadoNavegacionPrincipal origen,
            TabPage destino,
            ModoAplicacion modoDestino,
            bool registrarHistorial)
        {
            if (destino == null)
            {
                ActualizarBotonVolverNavegacion();
                return;
            }

            bool cambioReal = origen != null &&
                origen.Tab != null &&
                (origen.Tab != destino || origen.Modo != modoDestino);

            if (registrarHistorial && !navegandoDesdeHistorial && cambioReal)
            {
                EstadoNavegacionPrincipal ultimo = historialNavegacionPrincipal.Count == 0
                    ? null
                    : historialNavegacionPrincipal[historialNavegacionPrincipal.Count - 1];

                if (ultimo == null || ultimo.Tab != origen.Tab || ultimo.Modo != origen.Modo)
                {
                    historialNavegacionPrincipal.Add(origen);
                    if (historialNavegacionPrincipal.Count > 50)
                    {
                        historialNavegacionPrincipal.RemoveAt(0);
                    }
                }
            }

            estadoNavegacionActual = new EstadoNavegacionPrincipal
            {
                Modo = modoDestino,
                Tab = destino
            };
            ActualizarBotonVolverNavegacion();
        }

        private void ActualizarBotonVolverNavegacion()
        {
            if (btnVolverNavegacion == null)
            {
                return;
            }

            btnVolverNavegacion.Enabled = historialNavegacionPrincipal.Count > 0;
            btnVolverNavegacion.Text = "← Volver";
        }

        private void BtnVolverNavegacion_Click(object sender, EventArgs e)
        {
            while (historialNavegacionPrincipal.Count > 0)
            {
                int indice = historialNavegacionPrincipal.Count - 1;
                EstadoNavegacionPrincipal destino = historialNavegacionPrincipal[indice];
                historialNavegacionPrincipal.RemoveAt(indice);

                if (destino == null || destino.Tab == null)
                {
                    continue;
                }

                navegandoDesdeHistorial = true;
                try
                {
                    AplicarModoAplicacion(destino.Modo, false);
                    if (!tabs.TabPages.Contains(destino.Tab))
                    {
                        tabs.TabPages.Add(destino.Tab);
                    }

                    tabs.SelectedTab = destino.Tab;
                    CompletarCambioNavegacion(null, destino.Tab, destino.Modo, false);
                }
                finally
                {
                    navegandoDesdeHistorial = false;
                }

                ActualizarVisibilidadPanelDerecho();
                return;
            }

            ActualizarBotonVolverNavegacion();
        }

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

            bool esCatalogo = tabs.SelectedTab == tabCatalogoProductosServiciosPrincipal;
            bool esProyecto = tabs.SelectedTab == tabProyectoPrincipal;
            bool tabSinPanelDerecho = EsTabBibliotecaOEditor(tabs.SelectedTab);
            bool cerrarPanelLateral = esCatalogo
                ? !panelLateralCatalogoVisible
                : esProyecto
                    ? estadoPanelDerechoProyecto == EstadoPanelDerechoProyecto.Oculto
                    : !panelLateralCatalogoVisible;
            bool ocultarDerechaCompleta = tabSinPanelDerecho;

            if (panelContenidoLateralDerecho != null && panelContenidoLateralDerecho.Visible && cerrarPanelLateral)
            {
                GuardarAnchoPanelDerechoAbierto(false);
            }

            ActualizarBotonPanelLateralCatalogo(!tabSinPanelDerecho, !cerrarPanelLateral);
            ActualizarLayoutPanelDerechoCatalogo(esCatalogo, esProyecto);

            if (splitPrincipal.Panel1Collapsed)
            {
                splitPrincipal.Panel1Collapsed = false;
            }

            if (panelContenidoLateralDerecho != null)
            {
                panelContenidoLateralDerecho.Visible = !tabSinPanelDerecho && !cerrarPanelLateral;
            }

            if (btnPestanaPanelDerecho != null)
            {
                btnPestanaPanelDerecho.Visible = !tabSinPanelDerecho;
            }

            if (splitPrincipal.Panel2Collapsed != ocultarDerechaCompleta)
            {
                splitPrincipal.Panel2Collapsed = ocultarDerechaCompleta;
            }

            if (!ocultarDerechaCompleta)
            {
                AjustarAnchoPanelDerecho();
                if (panelGantt != null)
                {
                    panelGantt.Invalidate();
                }
            }
        }

        private void ActualizarLayoutPanelDerechoCatalogo(bool esCatalogo, bool esProyecto)
        {
            if (layoutPanelDerecho == null || layoutPanelDerecho.RowStyles.Count < 2)
            {
                return;
            }

            if (esCatalogo)
            {
                layoutPanelDerecho.RowStyles[0].SizeType = SizeType.Percent;
                layoutPanelDerecho.RowStyles[0].Height = 100;
                layoutPanelDerecho.RowStyles[1].SizeType = SizeType.Absolute;
                layoutPanelDerecho.RowStyles[1].Height = 44;
                if (panelResumenScroll != null)
                {
                    panelResumenScroll.Visible = false;
                }
            }
            else
            {
                layoutPanelDerecho.RowStyles[0].SizeType = SizeType.Percent;
                layoutPanelDerecho.RowStyles[0].Height = 42;
                layoutPanelDerecho.RowStyles[1].SizeType = SizeType.Percent;
                layoutPanelDerecho.RowStyles[1].Height = 58;
                if (panelResumenScroll != null)
                {
                    panelResumenScroll.Visible = true;
                }
            }
        }

        private void ActualizarBotonPanelLateralCatalogo(bool esCatalogo, bool visible)
        {
            if (btnAlternarPanelLateral != null)
            {
                btnAlternarPanelLateral.Visible = false;
            }

            if (btnPestanaPanelDerecho == null)
            {
                return;
            }

            btnPestanaPanelDerecho.Text = visible ? ">" : "<";
            btnPestanaPanelDerecho.BackColor = modoOscuroActivo
                ? System.Drawing.Color.FromArgb(45, 45, 48)
                : System.Drawing.Color.FromArgb(245, 247, 250);
            btnPestanaPanelDerecho.ForeColor = modoOscuroActivo
                ? System.Drawing.Color.FromArgb(230, 230, 230)
                : System.Drawing.Color.FromArgb(55, 65, 81);
            tooltipPanelDerecho.SetToolTip(
                btnPestanaPanelDerecho,
                visible ? "Ocultar panel lateral" : "Mostrar panel lateral"
            );
        }

        private void CambiarEstadoPanelDerechoProyecto(EstadoPanelDerechoProyecto estado)
        {
            estadoPanelDerechoProyecto = estado == EstadoPanelDerechoProyecto.Oculto
                ? EstadoPanelDerechoProyecto.Oculto
                : EstadoPanelDerechoProyecto.Normal;
            ActualizarVisibilidadPanelDerecho();
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

            EstadoNavegacionPrincipal origen = ObtenerEstadoNavegacionActual();
            string nombreTab = tabs.SelectedTab.Text;
            if (tabs.SelectedTab == tabInicioPrincipal)
            {
                RefrescarTabInicio();
            }

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

            CompletarCambioNavegacion(
                origen,
                tabs.SelectedTab,
                modoAplicacionActual,
                !navegandoDesdeHistorial);
        }
    }
}
