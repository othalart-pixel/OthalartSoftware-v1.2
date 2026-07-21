using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirInterfaz()
        {
            Text = "Cotizador Othalart";

            Width = 1500;
            Height = 850;
            MinimumSize = new Size(1200, 720);

            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            KeyPreview = true;

            KeyDown -= Form1_KeyDown;
            KeyUp -= Form1_KeyUp;

            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;

            Controls.Clear();

            ConstruirBarraSuperiorTema();

            splitPrincipal.Dock = DockStyle.Fill;
            splitPrincipal.Orientation = Orientation.Vertical;
            splitPrincipal.BorderStyle = BorderStyle.FixedSingle;
            splitPrincipal.SplitterWidth = 6;
            splitPrincipal.FixedPanel = FixedPanel.None;
            splitPrincipal.IsSplitterFixed = false;

            // Mínimos seguros durante construcción.
            splitPrincipal.Panel1MinSize = 100;
            splitPrincipal.Panel2MinSize = 100;

            splitPrincipal.Resize -= SplitPrincipal_Resize;
            splitPrincipal.Resize += SplitPrincipal_Resize;
            splitPrincipal.SplitterMoved -= SplitPrincipal_SplitterMoved;
            splitPrincipal.SplitterMoved += SplitPrincipal_SplitterMoved;
            splitPrincipal.SplitterMoving -= SplitPrincipal_SplitterMoving;
            splitPrincipal.SplitterMoving += SplitPrincipal_SplitterMoving;

            Controls.Add(splitPrincipal);
            Controls.Add(panelBarraSuperiorTema);

            ConstruirPanelIzquierdo();
            ConstruirPanelDerecho();
            ActualizarVisibilidadPanelDerecho();
        }

        private void ConstruirBarraSuperiorTema()
        {
            panelBarraSuperiorTema.Dock = DockStyle.Top;
            panelBarraSuperiorTema.Height = 46;
            panelBarraSuperiorTema.Padding = new Padding(12, 7, 12, 7);
            panelBarraSuperiorTema.BackColor = Color.FromArgb(246, 247, 249);

            FlowLayoutPanel accionesProyecto = new FlowLayoutPanel();
            accionesProyecto.Dock = DockStyle.Left;
            accionesProyecto.AutoSize = true;
            accionesProyecto.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            accionesProyecto.FlowDirection = FlowDirection.LeftToRight;
            accionesProyecto.WrapContents = false;
            accionesProyecto.Margin = new Padding(0);
            accionesProyecto.Padding = new Padding(0);

            btnVolverNavegacion.Dock = DockStyle.None;
            btnVolverNavegacion.Width = 104;
            btnVolverNavegacion.Height = 30;
            btnVolverNavegacion.Text = "← Volver";
            btnVolverNavegacion.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnVolverNavegacion.FlatStyle = FlatStyle.Flat;
            btnVolverNavegacion.FlatAppearance.BorderSize = 1;
            btnVolverNavegacion.FlatAppearance.BorderColor = Color.FromArgb(185, 190, 198);
            btnVolverNavegacion.BackColor = Color.White;
            btnVolverNavegacion.ForeColor = Color.FromArgb(25, 25, 25);
            btnVolverNavegacion.Cursor = Cursors.Hand;
            btnVolverNavegacion.UseVisualStyleBackColor = false;
            btnVolverNavegacion.Margin = new Padding(0, 0, 8, 0);
            btnVolverNavegacion.Enabled = false;
            btnVolverNavegacion.Click -= BtnVolverNavegacion_Click;
            btnVolverNavegacion.Click += BtnVolverNavegacion_Click;

            btnGuardarProyectoRapido.Dock = DockStyle.None;
            btnGuardarProyectoRapido.Width = 126;
            btnGuardarProyectoRapido.Height = 30;
            btnGuardarProyectoRapido.Text = "Guardar";
            btnGuardarProyectoRapido.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnGuardarProyectoRapido.FlatStyle = FlatStyle.Flat;
            btnGuardarProyectoRapido.FlatAppearance.BorderSize = 1;
            btnGuardarProyectoRapido.FlatAppearance.BorderColor = Color.FromArgb(185, 190, 198);
            btnGuardarProyectoRapido.BackColor = Color.White;
            btnGuardarProyectoRapido.ForeColor = Color.FromArgb(25, 25, 25);
            btnGuardarProyectoRapido.Cursor = Cursors.Hand;
            btnGuardarProyectoRapido.UseVisualStyleBackColor = false;
            btnGuardarProyectoRapido.Margin = new Padding(0, 0, 8, 0);
            btnGuardarProyectoRapido.Click -= BtnGuardarProyecto_Click;
            btnGuardarProyectoRapido.Click += BtnGuardarProyecto_Click;

            btnCargarProyectoRapido.Dock = DockStyle.None;
            btnCargarProyectoRapido.Width = 126;
            btnCargarProyectoRapido.Height = 30;
            btnCargarProyectoRapido.Text = "Cargar";
            btnCargarProyectoRapido.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            btnCargarProyectoRapido.FlatStyle = FlatStyle.Flat;
            btnCargarProyectoRapido.FlatAppearance.BorderSize = 1;
            btnCargarProyectoRapido.FlatAppearance.BorderColor = Color.FromArgb(185, 190, 198);
            btnCargarProyectoRapido.BackColor = Color.White;
            btnCargarProyectoRapido.ForeColor = Color.FromArgb(25, 25, 25);
            btnCargarProyectoRapido.Cursor = Cursors.Hand;
            btnCargarProyectoRapido.UseVisualStyleBackColor = false;
            btnCargarProyectoRapido.Margin = new Padding(0);
            btnCargarProyectoRapido.Click -= BtnCargarProyecto_Click;
            btnCargarProyectoRapido.Click += BtnCargarProyecto_Click;

            btnAlternarModoOscuro.Dock = DockStyle.Right;
            btnAlternarModoOscuro.Width = 130;
            btnAlternarModoOscuro.Height = 26;
            btnAlternarModoOscuro.Text = modoOscuroActivo ? "Modo claro" : "Modo oscuro";
            btnAlternarModoOscuro.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            btnAlternarModoOscuro.FlatStyle = FlatStyle.Flat;
            btnAlternarModoOscuro.FlatAppearance.BorderSize = 1;
            btnAlternarModoOscuro.Cursor = Cursors.Hand;
            btnAlternarModoOscuro.UseVisualStyleBackColor = false;

            btnAlternarModoOscuro.Click -= BtnAlternarModoOscuro_Click;
            btnAlternarModoOscuro.Click += BtnAlternarModoOscuro_Click;

            btnAlternarPanelLateral.Dock = DockStyle.Right;
            btnAlternarPanelLateral.Width = 156;
            btnAlternarPanelLateral.Height = 26;
            btnAlternarPanelLateral.Font = new Font("Segoe UI", 8.4f, FontStyle.Bold);
            btnAlternarPanelLateral.FlatStyle = FlatStyle.Flat;
            btnAlternarPanelLateral.FlatAppearance.BorderSize = 1;
            btnAlternarPanelLateral.Cursor = Cursors.Hand;
            btnAlternarPanelLateral.UseVisualStyleBackColor = false;
            btnAlternarPanelLateral.Margin = new Padding(0, 0, 8, 0);
            btnAlternarPanelLateral.Click -= BtnAlternarPanelLateral_Click;
            btnAlternarPanelLateral.Click += BtnAlternarPanelLateral_Click;

            lblModoAplicacion.AutoSize = true;
            lblModoAplicacion.Font = new Font("Segoe UI", 10.2f, FontStyle.Bold);
            lblModoAplicacion.ForeColor = Color.FromArgb(35, 35, 35);
            lblModoAplicacion.Margin = new Padding(18, 5, 18, 0);

            lblEstadoGuardadoGlobal.AutoSize = true;
            lblEstadoGuardadoGlobal.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblEstadoGuardadoGlobal.ForeColor = Color.FromArgb(85, 85, 85);
            lblEstadoGuardadoGlobal.Margin = new Padding(0, 7, 0, 0);

            panelBarraSuperiorTema.Controls.Clear();
            panelBarraSuperiorTema.Controls.Add(btnAlternarModoOscuro);
            accionesProyecto.Controls.Add(btnVolverNavegacion);
            accionesProyecto.Controls.Add(btnGuardarProyectoRapido);
            accionesProyecto.Controls.Add(btnCargarProyectoRapido);
            panelBarraSuperiorTema.Controls.Add(accionesProyecto);
            panelBarraSuperiorTema.Controls.Add(lblEstadoGuardadoGlobal);
            panelBarraSuperiorTema.Controls.Add(lblModoAplicacion);

            RefrescarBotonModoOscuro();
        }

        private void BtnAlternarModoOscuro_Click(object sender, EventArgs e)
        {
            AlternarModoOscuro();
        }

        private void BtnAlternarPanelLateral_Click(object sender, EventArgs e)
        {
            if (tabs != null && tabs.SelectedTab == tabProyectoPrincipal)
            {
                if (estadoPanelDerechoProyecto != EstadoPanelDerechoProyecto.Oculto)
                {
                    GuardarAnchoPanelDerechoAbierto(false);
                }
                estadoPanelDerechoProyecto = estadoPanelDerechoProyecto == EstadoPanelDerechoProyecto.Oculto
                    ? EstadoPanelDerechoProyecto.Normal
                    : EstadoPanelDerechoProyecto.Oculto;
            }
            else
            {
                if (panelLateralCatalogoVisible)
                {
                    GuardarAnchoPanelDerechoAbierto(false);
                }
                panelLateralCatalogoVisible = !panelLateralCatalogoVisible;
            }
            ActualizarVisibilidadPanelDerecho();
        }

        private void BtnPestanaPanelDerecho_Click(object? sender, EventArgs e)
        {
            bool esProyecto = tabs != null && tabs.SelectedTab == tabProyectoPrincipal;
            if (esProyecto)
            {
                if (estadoPanelDerechoProyecto != EstadoPanelDerechoProyecto.Oculto)
                {
                    GuardarAnchoPanelDerechoAbierto(false);
                }
                estadoPanelDerechoProyecto = estadoPanelDerechoProyecto == EstadoPanelDerechoProyecto.Oculto
                    ? EstadoPanelDerechoProyecto.Normal
                    : EstadoPanelDerechoProyecto.Oculto;
            }
            else
            {
                if (panelLateralCatalogoVisible)
                {
                    GuardarAnchoPanelDerechoAbierto(false);
                }
                panelLateralCatalogoVisible = !panelLateralCatalogoVisible;
            }

            ActualizarVisibilidadPanelDerecho();
        }

        private void GuardarAnchoPanelDerechoAbierto(bool definidoPorUsuario)
        {
            if (splitPrincipal == null || splitPrincipal.Panel2Collapsed)
            {
                return;
            }

            if (!definidoPorUsuario && !anchoPanelDerechoDefinidoPorUsuario)
            {
                return;
            }

            int anchoActual = splitPrincipal.Panel2.Width;
            if (anchoActual <= AnchoPestanaPanelDerecho + splitPrincipal.SplitterWidth)
            {
                return;
            }

            ultimoAnchoPanelDerechoAbierto = Math.Max(
                AnchoMinimoPanelDerechoAbierto,
                anchoActual
            );
            if (definidoPorUsuario)
            {
                anchoPanelDerechoDefinidoPorUsuario = true;
            }
        }

        private void BtnPestanaPanelDerecho_MouseEnter(object? sender, EventArgs e)
        {
            if (btnPestanaPanelDerecho == null)
            {
                return;
            }

            btnPestanaPanelDerecho.BackColor = modoOscuroActivo
                ? Color.FromArgb(62, 62, 66)
                : Color.FromArgb(226, 232, 240);
        }

        private void BtnPestanaPanelDerecho_MouseLeave(object? sender, EventArgs e)
        {
            ActualizarBotonPanelLateralCatalogo(
                tabs != null && tabs.SelectedTab != null && !EsTabBibliotecaOEditor(tabs.SelectedTab),
                panelContenidoLateralDerecho != null && panelContenidoLateralDerecho.Visible
            );
        }

        private void SplitPrincipal_Resize(object? sender, EventArgs e)
        {
            if (moviendoSplitterPanelDerecho)
            {
                return;
            }

            AjustarAnchoPanelDerecho();
        }

        private void SplitPrincipal_SplitterMoving(object? sender, SplitterCancelEventArgs e)
        {
            moviendoSplitterPanelDerecho = true;
        }

        private void SplitPrincipal_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            bool fueMovimientoUsuario = moviendoSplitterPanelDerecho && !ajustandoSplitterPanelDerechoProgramaticamente;
            moviendoSplitterPanelDerecho = false;
            if (splitPrincipal == null || splitPrincipal.Panel2Collapsed)
            {
                return;
            }

            bool panelAbierto = panelContenidoLateralDerecho != null && panelContenidoLateralDerecho.Visible;
            if (panelAbierto && fueMovimientoUsuario)
            {
                GuardarAnchoPanelDerechoAbierto(true);
            }
        }

        private void AjustarAnchoPanelDerecho()
        {
            if (splitPrincipal == null)
            {
                return;
            }

            if (splitPrincipal.Panel2Collapsed)
            {
                return;
            }

            int anchoTotal = splitPrincipal.ClientSize.Width;

            if (anchoTotal <= 900)
            {
                return;
            }

            bool catalogoActivo = tabs != null && tabs.SelectedTab == tabCatalogoProductosServiciosPrincipal;
            bool panelAbierto = panelContenidoLateralDerecho != null && panelContenidoLateralDerecho.Visible;

            int minimoIzquierdo = catalogoActivo ? 860 : 650;
            int minimoDerecho = panelAbierto ? AnchoMinimoPanelDerechoAbierto : AnchoPestanaPanelDerecho;

            if (anchoTotal < minimoIzquierdo + minimoDerecho + splitPrincipal.SplitterWidth)
            {
                minimoIzquierdo = 400;
                minimoDerecho = panelAbierto ? 300 : AnchoPestanaPanelDerecho;
            }

            splitPrincipal.Panel1MinSize = minimoIzquierdo;
            splitPrincipal.Panel2MinSize = minimoDerecho;

            int anchoDerechoDeseado = panelAbierto
                ? ObtenerAnchoPanelDerechoAbiertoDeseado(anchoTotal, minimoDerecho)
                : AnchoPestanaPanelDerecho;
            int anchoDerechoMaximo = Math.Max(minimoDerecho, anchoTotal - minimoIzquierdo);
            if (anchoDerechoDeseado > anchoDerechoMaximo)
            {
                anchoDerechoDeseado = anchoDerechoMaximo;
            }

            int splitterDistance = anchoTotal - anchoDerechoDeseado;
            int maxSplitterDistance = anchoTotal - minimoDerecho;

            if (splitterDistance < minimoIzquierdo)
            {
                splitterDistance = minimoIzquierdo;
            }

            if (splitterDistance > maxSplitterDistance)
            {
                splitterDistance = maxSplitterDistance;
            }

            if (splitterDistance <= 0)
            {
                return;
            }

            if (splitterDistance >= anchoTotal)
            {
                return;
            }

            if (splitPrincipal.SplitterDistance != splitterDistance)
            {
                ajustandoSplitterPanelDerechoProgramaticamente = true;
                try
                {
                    splitPrincipal.SplitterDistance = splitterDistance;
                }
                finally
                {
                    ajustandoSplitterPanelDerechoProgramaticamente = false;
                }
            }
        }

        private int ObtenerAnchoPanelDerechoAbiertoDeseado(int anchoTotal, int minimoDerecho)
        {
            int anchoDeseado = anchoPanelDerechoDefinidoPorUsuario
                ? ultimoAnchoPanelDerechoAbierto
                : AnchoInicialPanelDerechoAbierto;

            if (anchoDeseado < minimoDerecho)
            {
                anchoDeseado = minimoDerecho;
            }

            return anchoDeseado;
        }

        private void ConstruirPanelIzquierdo()
        {
            splitPrincipal.Panel1.Controls.Clear();

            tabs.TabPages.Clear();
            tabs.Dock = DockStyle.Fill;

            tabs.DrawItem -= Tabs_DrawItem;
            tabs.Selecting -= Tabs_Selecting;
            tabs.SelectedIndexChanged -= Tabs_SelectedIndexChanged;

            tabInicioPrincipal = new TabPage("Inicio");
            tabCatalogoProductosServiciosPrincipal = new TabPage("Catalogo");
            tabProyectoPrincipal = new TabPage("Productos y servicios");
            tabDatosPrincipal = new TabPage("Datos");
            tabProductosPrincipal = new TabPage("Productos");
            tabMonedaPrincipal = new TabPage("Moneda");
            tabDesgloseProductivoPrincipal = new TabPage("Desglose productivo");
            tabRendimientosPrincipal = new TabPage("Rendim.");
            tabEcuacionesPrincipal = new TabPage("Ecuaciones");
            tabGestionesPrincipal = new TabPage("Gestiones");
            tabValidacionJsonPrincipal = new TabPage("Validacion JSON");
            tabPersonalPrincipal = new TabPage("Personal");
            //TabPage tabEtapas = new TabPage("Etapas");
            tabSubEtapasPrincipal = new TabPage("Subetapas");
            tabRangosPrincipal = new TabPage("Rangos");
            tabCargosPrincipal = new TabPage("Cargos");
            tabManoObraPrincipal = new TabPage("Mano de obra");
            tabCostosPrincipal = new TabPage("Costos");
            tabResultadosPrincipal = new TabPage("Resultados");
            tabInformePrincipal = new TabPage("Informe");
            tabGuardarPrincipal = new TabPage("Exportar");

            ConstruirTabInicio(tabInicioPrincipal);
            ConstruirTabProyecto(tabProyectoPrincipal);
            ConstruirTabDatos(tabDatosPrincipal);
            ConstruirTabProductos2D(tabProductosPrincipal);
            ConstruirTabMoneda(tabMonedaPrincipal);
            ConstruirTabDesgloseProductivo(tabDesgloseProductivoPrincipal);
            ConstruirTabRendimientosProductivos(tabRendimientosPrincipal);
            ConstruirTabEcuacionesProductivas(tabEcuacionesPrincipal);
            ConstruirTabGestionesProductivas(tabGestionesPrincipal);
            ConstruirTabValidacionJson(tabValidacionJsonPrincipal);
            ConstruirTabPersonalEmpresa(tabPersonalPrincipal);
            //ConstruirTabEtapas(tabEtapas);
            ConstruirTabSubEtapas(tabSubEtapasPrincipal);
            ConstruirTabRangosSubEtapas(tabRangosPrincipal);
            ConstruirTabCargos(tabCargosPrincipal);
            ConstruirTabManoObra(tabManoObraPrincipal);
            ConstruirTabCostos(tabCostosPrincipal);
            ConstruirTabResultados(tabResultadosPrincipal);
            ConstruirTabInforme(tabInformePrincipal);
            ConstruirTabGuardar(tabGuardarPrincipal);

            tabs.TabPages.Add(tabInicioPrincipal);
            tabs.TabPages.Add(tabProyectoPrincipal);
            tabs.TabPages.Add(tabDatosPrincipal);
            tabs.TabPages.Add(tabDesgloseProductivoPrincipal);
            tabs.TabPages.Add(tabValidacionJsonPrincipal);
            tabs.TabPages.Add(tabPersonalPrincipal);
            tabs.TabPages.Add(tabManoObraPrincipal);
            tabs.TabPages.Add(tabSubEtapasPrincipal);
            tabs.TabPages.Add(tabResultadosPrincipal);
            tabs.TabPages.Add(tabInformePrincipal);
            tabs.TabPages.Add(tabGuardarPrincipal);
            //tabs.TabPages.Add(tabEtapas);

            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += Tabs_DrawItem;
            tabs.Selecting += Tabs_Selecting;
            tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            splitPrincipal.Panel1.Controls.Add(tabs);
            AplicarModoAplicacion(ModoAplicacion.Inicio);
        }

        private void ConstruirPanelDerecho()
        {
            splitPrincipal.Panel2.Controls.Clear();

            TableLayoutPanel contenedorPanelDerecho = new TableLayoutPanel();
            contenedorPanelDerecho.Dock = DockStyle.Fill;
            contenedorPanelDerecho.RowCount = 1;
            contenedorPanelDerecho.ColumnCount = 2;
            contenedorPanelDerecho.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            contenedorPanelDerecho.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoPestanaPanelDerecho));
            contenedorPanelDerecho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contenedorPanelDerecho.Margin = new Padding(0);
            contenedorPanelDerecho.Padding = new Padding(0);

            btnPestanaPanelDerecho.Dock = DockStyle.Fill;
            btnPestanaPanelDerecho.Margin = new Padding(0);
            btnPestanaPanelDerecho.Padding = new Padding(0);
            btnPestanaPanelDerecho.FlatStyle = FlatStyle.Flat;
            btnPestanaPanelDerecho.FlatAppearance.BorderSize = 0;
            btnPestanaPanelDerecho.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnPestanaPanelDerecho.Cursor = Cursors.Hand;
            btnPestanaPanelDerecho.TabStop = false;
            btnPestanaPanelDerecho.UseVisualStyleBackColor = false;
            btnPestanaPanelDerecho.Click -= BtnPestanaPanelDerecho_Click;
            btnPestanaPanelDerecho.Click += BtnPestanaPanelDerecho_Click;
            btnPestanaPanelDerecho.MouseEnter -= BtnPestanaPanelDerecho_MouseEnter;
            btnPestanaPanelDerecho.MouseEnter += BtnPestanaPanelDerecho_MouseEnter;
            btnPestanaPanelDerecho.MouseLeave -= BtnPestanaPanelDerecho_MouseLeave;
            btnPestanaPanelDerecho.MouseLeave += BtnPestanaPanelDerecho_MouseLeave;

            panelContenidoLateralDerecho.Dock = DockStyle.Fill;
            panelContenidoLateralDerecho.Margin = new Padding(0);
            panelContenidoLateralDerecho.Padding = new Padding(0);
            panelContenidoLateralDerecho.BackColor = Color.White;

            TableLayoutPanel layout = layoutPanelDerecho;
            layout.Controls.Clear();
            layout.RowStyles.Clear();
            layout.ColumnStyles.Clear();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.ColumnCount = 1;
            layout.Padding = new Padding(0);
            layout.Margin = new Padding(0);

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));

            TabControl tabsDerecha = new TabControl();
            tabsDerecha.Dock = DockStyle.Fill;
            tabsDerecha.Margin = new Padding(0);

            TabPage tabGantt = new TabPage("Gantt");
            tabGantt.Padding = new Padding(0);
            tabGantt.BackColor = Color.White;

            TableLayoutPanel layoutGantt = new TableLayoutPanel();
            layoutGantt.Dock = DockStyle.Fill;
            layoutGantt.RowCount = 2;
            layoutGantt.ColumnCount = 1;
            layoutGantt.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layoutGantt.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layoutGantt.Margin = new Padding(0);
            layoutGantt.Padding = new Padding(0);

            Panel headerGantt = new Panel();
            headerGantt.Dock = DockStyle.Fill;
            headerGantt.BackColor = Color.FromArgb(248, 248, 248);
            headerGantt.Padding = new Padding(10, 0, 10, 0);

            Label tituloGantt = new Label();
            tituloGantt.Text = "Gantt de etapas";
            tituloGantt.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            tituloGantt.Dock = DockStyle.Fill;
            tituloGantt.TextAlign = ContentAlignment.MiddleLeft;
            tituloGantt.AutoEllipsis = true;

            headerGantt.Controls.Add(tituloGantt);

            panelGantt.Dock = DockStyle.Fill;
            panelGantt.BackColor = Color.White;
            panelGantt.Margin = new Padding(0);
            panelGantt.Paint -= PanelGantt_Paint;
            panelGantt.Paint += PanelGantt_Paint;

            layoutGantt.Controls.Add(headerGantt, 0, 0);
            layoutGantt.Controls.Add(panelGantt, 0, 1);
            tabGantt.Controls.Add(layoutGantt);
            tabsDerecha.TabPages.Add(tabGantt);

            Panel panelResumenMarco = new Panel();
            panelResumenMarco.Dock = DockStyle.Fill;
            panelResumenMarco.BackColor = Color.FromArgb(225, 225, 225);
            panelResumenMarco.Padding = new Padding(1);
            panelResumenMarco.Margin = new Padding(0);

            TableLayoutPanel layoutResumen = layoutResumenDerecho;
            layoutResumen.Controls.Clear();
            layoutResumen.RowStyles.Clear();
            layoutResumen.ColumnStyles.Clear();
            layoutResumen.Dock = DockStyle.Fill;
            layoutResumen.RowCount = 2;
            layoutResumen.ColumnCount = 1;
            layoutResumen.BackColor = Color.White;
            layoutResumen.Margin = new Padding(0);
            layoutResumen.Padding = new Padding(0);

            layoutResumen.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layoutResumen.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Panel panelCabeceraResumen = new Panel();
            panelCabeceraResumen.Dock = DockStyle.Fill;
            panelCabeceraResumen.BackColor = Color.FromArgb(248, 248, 248);
            panelCabeceraResumen.Padding = new Padding(0, 0, 10, 0);
            panelCabeceraResumen.Margin = new Padding(0);

            Panel barraResumen = new Panel();
            barraResumen.Dock = DockStyle.Left;
            barraResumen.Width = 6;
            barraResumen.BackColor = Color.FromArgb(83, 192, 166);

            Label lblTituloResumen = new Label();
            lblTituloResumen.Text = "RESUMEN ACTUAL";
            lblTituloResumen.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTituloResumen.Dock = DockStyle.Fill;
            lblTituloResumen.TextAlign = ContentAlignment.MiddleLeft;
            lblTituloResumen.Padding = new Padding(14, 0, 0, 0);
            lblTituloResumen.AutoEllipsis = true;

            panelCabeceraResumen.Controls.Add(lblTituloResumen);
            panelCabeceraResumen.Controls.Add(barraResumen);

            panelResumenScroll.Dock = DockStyle.Fill;
            panelResumenScroll.AutoScroll = false;
            panelResumenScroll.BackColor = Color.White;
            panelResumenScroll.Padding = new Padding(10);
            panelResumenScroll.Margin = new Padding(0);

            rtbResumen.Dock = DockStyle.Fill;
            rtbResumen.ReadOnly = true;
            rtbResumen.BorderStyle = BorderStyle.None;
            rtbResumen.BackColor = Color.White;
            rtbResumen.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbResumen.TabStop = false;
            rtbResumen.DetectUrls = false;
            rtbResumen.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            rtbResumen.Margin = new Padding(0);
            rtbResumen.Multiline = true;
            rtbResumen.WordWrap = true;
            rtbResumen.ShortcutsEnabled = false;
            rtbResumen.Cursor = Cursors.Default;
            rtbResumen.HideSelection = true;

            panelResumenScroll.Controls.Clear();
            panelResumenScroll.Controls.Add(rtbResumen);

            layoutResumen.Controls.Add(panelCabeceraResumen, 0, 0);
            layoutResumen.Controls.Add(panelResumenScroll, 0, 1);

            panelResumenMarco.Controls.Add(layoutResumen);

            layout.Controls.Add(tabsDerecha, 0, 0);
            layout.Controls.Add(panelResumenMarco, 0, 1);

            panelContenidoLateralDerecho.Controls.Clear();
            panelContenidoLateralDerecho.Controls.Add(layout);
            contenedorPanelDerecho.Controls.Add(btnPestanaPanelDerecho, 0, 0);
            contenedorPanelDerecho.Controls.Add(panelContenidoLateralDerecho, 1, 0);

            splitPrincipal.Panel2.Controls.Add(contenedorPanelDerecho);

            ConfigurarFeedbackResumen();
        }

        private void AgregarFilaTexto(TableLayoutPanel layout, string etiqueta, Control control, int fila)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(0, 8, 10, 8);

            control.Width = 320;
            control.Margin = new Padding(0, 5, 0, 5);

            layout.Controls.Add(lbl, 0, fila);
            layout.Controls.Add(control, 1, fila);
        }

        private void AgregarFilaCosto(TableLayoutPanel layout, string etiqueta, TextBox textBox, int fila)
        {
            Label lbl = new Label();
            lbl.Text = etiqueta;
            lbl.AutoSize = true;
            lbl.Margin = new Padding(0, 8, 10, 8);

            textBox.Width = 180;
            textBox.Margin = new Padding(0, 5, 0, 5);

            layout.Controls.Add(lbl, 0, fila);
            layout.Controls.Add(textBox, 1, fila);
        }
    }
}
