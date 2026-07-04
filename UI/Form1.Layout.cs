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

            panelBarraSuperiorTema.Controls.Clear();
            panelBarraSuperiorTema.Controls.Add(btnAlternarModoOscuro);
            accionesProyecto.Controls.Add(btnGuardarProyectoRapido);
            accionesProyecto.Controls.Add(btnCargarProyectoRapido);
            panelBarraSuperiorTema.Controls.Add(accionesProyecto);

            RefrescarBotonModoOscuro();
        }

        private void BtnAlternarModoOscuro_Click(object sender, EventArgs e)
        {
            AlternarModoOscuro();
        }

        private void SplitPrincipal_Resize(object? sender, EventArgs e)
        {
            AjustarAnchoPanelDerecho();
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

            // El panel derecho gana protagonismo.
            // 0.37 = 37% del ancho total.
            int anchoDerechoDeseado = (int)(anchoTotal * 0.37);

            if (anchoDerechoDeseado < 540)
            {
                anchoDerechoDeseado = 540;
            }

            if (anchoDerechoDeseado > 820)
            {
                anchoDerechoDeseado = 820;
            }

            int splitterDistance = anchoTotal - anchoDerechoDeseado;

            int minimoIzquierdo = 650;
            int minimoDerecho = 420;

            if (anchoTotal < minimoIzquierdo + minimoDerecho + splitPrincipal.SplitterWidth)
            {
                minimoIzquierdo = 400;
                minimoDerecho = 300;
            }

            splitPrincipal.Panel1MinSize = minimoIzquierdo;
            splitPrincipal.Panel2MinSize = minimoDerecho;

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
                splitPrincipal.SplitterDistance = splitterDistance;
            }
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
        }

        private void ConstruirPanelDerecho()
        {
            splitPrincipal.Panel2.Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel();
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

            panelGantt.Dock = DockStyle.Fill;
            panelGantt.BackColor = Color.White;
            panelGantt.Margin = new Padding(0);
            panelGantt.Paint -= PanelGantt_Paint;
            panelGantt.Paint += PanelGantt_Paint;

            tabGantt.Controls.Add(panelGantt);
            tabsDerecha.TabPages.Add(tabGantt);

            Panel panelResumenMarco = new Panel();
            panelResumenMarco.Dock = DockStyle.Fill;
            panelResumenMarco.BackColor = Color.FromArgb(225, 225, 225);
            panelResumenMarco.Padding = new Padding(1);
            panelResumenMarco.Margin = new Padding(0);

            TableLayoutPanel layoutResumen = new TableLayoutPanel();
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
            panelCabeceraResumen.Padding = new Padding(0);
            panelCabeceraResumen.Margin = new Padding(0);

            Panel barraResumen = new Panel();
            barraResumen.Dock = DockStyle.Left;
            barraResumen.Width = 6;
            barraResumen.BackColor = Color.FromArgb(83, 192, 166);

            Label lblTituloResumen = new Label();
            lblTituloResumen.Text = "RESUMEN ACTUAL";
            lblTituloResumen.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTituloResumen.AutoSize = true;
            lblTituloResumen.Location = new Point(14, 12);

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

            splitPrincipal.Panel2.Controls.Add(layout);

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
