using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabInicio(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.Padding = new Padding(28, 24, 28, 28);
            root.BackColor = Color.White;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Inicio";
            titulo.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titulo.AutoSize = true;

            Label ayuda = new Label();
            ayuda.Text = "Crea o carga proyectos, entra al flujo de cotización y edita bibliotecas del programa desde un solo lugar.";
            ayuda.Font = new Font("Segoe UI", 10f);
            ayuda.ForeColor = Color.FromArgb(85, 85, 85);
            ayuda.AutoSize = true;
            ayuda.Margin = new Padding(0, 0, 0, 18);

            TableLayoutPanel secciones = new TableLayoutPanel();
            secciones.Dock = DockStyle.Top;
            secciones.AutoSize = true;
            secciones.ColumnCount = 2;
            secciones.RowCount = 1;
            secciones.Margin = new Padding(0);
            secciones.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            secciones.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 840));

            secciones.Controls.Add(CrearPanelInicioProyecto(), 0, 0);
            secciones.Controls.Add(CrearPanelInicioBibliotecas(), 1, 0);

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(ayuda, 0, 1);
            root.Controls.Add(secciones, 0, 2);

            tab.Controls.Add(root);
        }

        private Control CrearPanelInicioProyecto()
        {
            FlowLayoutPanel panel = CrearGrupoInicio("Proyecto", 286);

            Button nuevo = CrearBotonInicio("Nuevo proyecto");
            nuevo.Click += (s, e) => NuevoProyectoDesdeInicio();

            Button guardar = CrearBotonInicio("Guardar proyecto");
            guardar.Click += BtnGuardarProyecto_Click;

            Button cargar = CrearBotonInicio("Cargar proyecto");
            cargar.Click += BtnCargarProyecto_Click;

            panel.Controls.Add(nuevo);
            panel.Controls.Add(guardar);
            panel.Controls.Add(cargar);
            return panel;
        }

        private Control CrearPanelInicioBibliotecas()
        {
            TableLayoutPanel panel = CrearGrupoInicioGrid("Editar bibliotecas", 816);
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Top;
            grid.AutoSize = true;
            grid.ColumnCount = 3;
            grid.RowCount = 4;
            grid.Margin = new Padding(0);
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            AgregarBotonAdminInicio(grid, "Productos", tabProductosPrincipal, 0, 0);
            AgregarBotonAdminInicio(grid, "Moneda", tabMonedaPrincipal, 1, 0);
            AgregarBotonAdminInicio(grid, "Rendimientos", tabRendimientosPrincipal, 2, 0);
            AgregarBotonAdminInicio(grid, "Ecuaciones", tabEcuacionesPrincipal, 0, 1);
            AgregarBotonAdminInicio(grid, "Gestiones", tabGestionesPrincipal, 1, 1);
            AgregarBotonAdminInicio(grid, "Rangos", tabRangosPrincipal, 2, 1);
            AgregarBotonAdminInicio(grid, "Cargos", tabCargosPrincipal, 0, 2);
            AgregarBotonAdminInicio(grid, "Mano de obra", tabManoObraPrincipal, 1, 2);
            AgregarBotonAdminInicio(grid, "Costos", tabCostosPrincipal, 2, 2);
            AgregarBotonAdminInicio(grid, "Personal", tabPersonalPrincipal, 0, 3);

            panel.Controls.Add(grid, 0, 1);
            return panel;
        }

        private FlowLayoutPanel CrearGrupoInicio(string titulo, int ancho)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.FlowDirection = FlowDirection.TopDown;
            panel.WrapContents = false;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Width = ancho;
            panel.Padding = new Padding(14);
            panel.Margin = new Padding(0, 0, 12, 0);
            panel.BackColor = Color.FromArgb(248, 248, 248);
            panel.BorderStyle = BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = titulo;
            label.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            label.Width = ancho - 30;
            label.Height = 24;
            label.Margin = new Padding(0, 0, 0, 10);
            panel.Controls.Add(label);

            return panel;
        }

        private TableLayoutPanel CrearGrupoInicioGrid(string titulo, int ancho)
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.ColumnCount = 1;
            panel.RowCount = 2;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Width = ancho;
            panel.Padding = new Padding(14);
            panel.Margin = new Padding(0, 0, 12, 0);
            panel.BackColor = Color.FromArgb(248, 248, 248);
            panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label label = new Label();
            label.Text = titulo;
            label.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(0, 0, 0, 10);
            panel.Controls.Add(label, 0, 0);

            panel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(
                    e.Graphics,
                    panel.ClientRectangle,
                    Color.FromArgb(110, 110, 110),
                    ButtonBorderStyle.Solid
                );
            };

            return panel;
        }

        private Button CrearBotonInicio(string texto)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 248;
            boton.Height = 34;
            boton.Margin = new Padding(0, 0, 8, 7);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            boton.BackColor = Color.White;
            boton.ForeColor = Color.Black;
            boton.UseVisualStyleBackColor = false;
            boton.Cursor = Cursors.Hand;
            return boton;
        }

        private void AgregarBotonAdminInicio(FlowLayoutPanel panel, string texto, TabPage tab)
        {
            Button boton = CrearBotonInicio(texto);
            boton.Click += (s, e) => AbrirTabPrincipal(tab, true);
            panel.Controls.Add(boton);
        }

        private void AgregarBotonAdminInicio(
            TableLayoutPanel panel,
            string texto,
            TabPage tab,
            int columna,
            int fila
        )
        {
            Button boton = CrearBotonInicio(texto);
            boton.Dock = DockStyle.Fill;
            boton.Margin = new Padding(0, 0, 8, 7);
            boton.Click += (s, e) => AbrirTabPrincipal(tab, true);
            panel.Controls.Add(boton, columna, fila);
        }

        private void AbrirTabPrincipal(TabPage tab, bool administrativa)
        {
            if (tabs == null || tab == null)
            {
                return;
            }

            if (administrativa &&
                tabAdministrativaActiva != null &&
                tabAdministrativaActiva != tab &&
                !EsTabPrincipalFijo(tabAdministrativaActiva) &&
                tabs.TabPages.Contains(tabAdministrativaActiva))
            {
                tabs.TabPages.Remove(tabAdministrativaActiva);
            }

            if (!tabs.TabPages.Contains(tab))
            {
                tabs.TabPages.Add(tab);
            }

            if (administrativa && !EsTabPrincipalFijo(tab))
            {
                tabAdministrativaActiva = tab;
            }

            tabs.SelectedTab = tab;
        }

        private bool EsTabPrincipalFijo(TabPage tab)
        {
            return tab == tabInicioPrincipal ||
                tab == tabDatosPrincipal ||
                tab == tabDesgloseProductivoPrincipal ||
                tab == tabSubEtapasPrincipal ||
                tab == tabResultadosPrincipal ||
                tab == tabInformePrincipal ||
                tab == tabGuardarPrincipal;
        }

        private void VolverAInicio()
        {
            if (tabs != null && tabInicioPrincipal != null)
            {
                tabs.SelectedTab = tabInicioPrincipal;
            }
        }

        private void NuevoProyectoDesdeInicio()
        {
            cotizacion = null;
            InicializarCotizacion();
            RefrescarTodo();
            AbrirTabPrincipal(tabDatosPrincipal, false);
        }
    }
}
