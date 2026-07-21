using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

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
            titulo.Text = "Othalart";
            titulo.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titulo.AutoSize = true;

            Label ayuda = new Label();
            ayuda.Text = "Elige si vas a cotizar un proyecto real o configurar las bibliotecas maestras del sistema.";
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

            Button nuevo = CrearBotonInicio("Crear nuevo proyecto");
            nuevo.Click += (s, e) => NuevoProyectoDesdeInicio();

            Button cargar = CrearBotonInicio("Abrir proyecto");
            cargar.Click += BtnCargarProyecto_Click;

            Button revisar = CrearBotonInicio("Revisar productos y servicios");
            revisar.Click += (s, e) => AbrirCatalogoProductosServicios();

            panel.Controls.Add(nuevo);
            panel.Controls.Add(cargar);
            panel.Controls.Add(revisar);
            panel.Controls.Add(CrearListaProyectosRecientesInicio());
            return panel;
        }

        private Control CrearListaProyectosRecientesInicio()
        {
            FlowLayoutPanel contenedor = new FlowLayoutPanel();
            contenedor.FlowDirection = FlowDirection.TopDown;
            contenedor.WrapContents = false;
            contenedor.AutoSize = true;
            contenedor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            contenedor.Width = 248;
            contenedor.Margin = new Padding(0, 10, 0, 0);

            Label titulo = new Label();
            titulo.Text = "Proyectos guardados";
            titulo.Width = 248;
            titulo.Height = 22;
            titulo.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(45, 45, 45);
            titulo.Margin = new Padding(0, 0, 0, 4);
            contenedor.Controls.Add(titulo);

            List<ProyectoReciente> recientes = CargarProyectosGuardadosInicio();
            if (recientes.Count == 0)
            {
                contenedor.Controls.Add(CrearEtiquetaInicio("No hay proyectos guardados todavía. Cuando guardes uno, aparecerá aquí."));
                return contenedor;
            }

            foreach (ProyectoReciente proyecto in recientes)
            {
                contenedor.Controls.Add(CrearBotonProyectoRecienteInicio(proyecto));
            }

            return contenedor;
        }

        private List<ProyectoReciente> CargarProyectosGuardadosInicio()
        {
            List<ProyectoReciente> proyectos = ProyectosRecientesService.Cargar();
            HashSet<string> rutasConocidas = new HashSet<string>(
                proyectos.Select(p => Path.GetFullPath(p.Ruta)),
                StringComparer.OrdinalIgnoreCase);

            foreach (string ruta in ProyectoOthalartArchivoService.ListarRutasProyectosGuardados())
            {
                string rutaCompleta = Path.GetFullPath(ruta);
                if (!rutasConocidas.Add(rutaCompleta))
                {
                    continue;
                }

                try
                {
                    ProyectoOthalartGuardado guardado =
                        ProyectoOthalartArchivoService.Cargar(rutaCompleta);
                    proyectos.Add(new ProyectoReciente
                    {
                        Ruta = rutaCompleta,
                        Nombre = guardado.Cotizacion?.NombreProyecto ?? "",
                        Cliente = guardado.Cotizacion?.NombreCliente ?? "",
                        Fecha = File.GetLastWriteTime(rutaCompleta)
                    });
                }
                catch
                {
                    // Un archivo dañado no debe impedir mostrar los demás proyectos.
                }
            }

            return proyectos
                .OrderByDescending(p => p.Fecha)
                .Take(12)
                .ToList();
        }

        private void RefrescarTabInicio()
        {
            if (tabInicioPrincipal == null || tabInicioPrincipal.IsDisposed)
            {
                return;
            }

            ConstruirTabInicio(tabInicioPrincipal);
        }

        private Control CrearBotonProyectoRecienteInicio(ProyectoReciente proyecto)
        {
            Button boton = CrearBotonInicio("");
            boton.Width = 248;
            boton.Height = 52;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Font = new Font("Segoe UI", 8.8f, FontStyle.Bold);
            boton.Text =
                (string.IsNullOrWhiteSpace(proyecto.Nombre) ? Path.GetFileNameWithoutExtension(proyecto.Ruta) : proyecto.Nombre) +
                Environment.NewLine +
                (string.IsNullOrWhiteSpace(proyecto.Cliente) ? proyecto.Fecha.ToString("dd-MM-yyyy HH:mm") : proyecto.Cliente);
            boton.Tag = proyecto.Ruta;
            boton.Click += (s, e) => CargarProyectoRecienteDesdeInicio(Convert.ToString(((Control)s).Tag) ?? "");
            return boton;
        }

        private Control CrearPanelInicioBibliotecas()
        {
            TableLayoutPanel panel = CrearGrupoInicioGrid("Configuracion de bibliotecas", 816);
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

            Button abrirModo = CrearBotonInicio("Configurar bibliotecas");
            abrirModo.Width = 260;
            abrirModo.Click += (s, e) => AplicarModoAplicacion(ModoAplicacion.Configuracion);

            panel.Controls.Add(grid, 0, 1);
            panel.Controls.Add(abrirModo, 0, 2);
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
            panel.RowCount = 3;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Width = ancho;
            panel.Padding = new Padding(14);
            panel.Margin = new Padding(0, 0, 12, 0);
            panel.BackColor = Color.FromArgb(248, 248, 248);
            panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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

        private Label CrearEtiquetaInicio(string texto)
        {
            Label label = new Label();
            label.Text = texto;
            label.Width = 248;
            label.Height = 44;
            label.Font = new Font("Segoe UI", 8.8f);
            label.ForeColor = Color.FromArgb(90, 90, 90);
            label.Margin = new Padding(0, 8, 8, 0);
            return label;
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

            EstadoNavegacionPrincipal origen = ObtenerEstadoNavegacionActual();
            navegandoDesdeHistorial = true;
            if (administrativa)
            {
                AplicarModoAplicacion(ModoAplicacion.Configuracion, false);
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
            navegandoDesdeHistorial = false;
            CompletarCambioNavegacion(
                origen,
                tab,
                administrativa ? ModoAplicacion.Configuracion : modoAplicacionActual,
                true);
        }

        private bool EsTabPrincipalFijo(TabPage tab)
        {
            return tab == tabInicioPrincipal ||
                tab == tabDatosPrincipal ||
                tab == tabProyectoPrincipal ||
                tab == tabDesgloseProductivoPrincipal ||
                tab == tabSubEtapasPrincipal ||
                tab == tabResultadosPrincipal ||
                tab == tabInformePrincipal ||
                tab == tabGuardarPrincipal;
        }

        private void VolverAInicio()
        {
            AplicarModoAplicacion(ModoAplicacion.Inicio);
        }

        private void NuevoProyectoDesdeInicio()
        {
            using (FormNuevoProyecto dialogo = new FormNuevoProyecto())
            {
                if (dialogo.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                cargandoProyectoDesdeArchivo = true;
                try
                {
                    ReiniciarEstadoLocalParaNuevoProyecto();

                    cotizacion = null;
                    InicializarCotizacion();

                    cotizacion.NombreProyecto = dialogo.NombreProyecto;
                    cotizacion.NombreCliente = dialogo.Cliente;
                    cotizacion.Moneda = dialogo.Moneda;
                    cotizacion.MonedaVisualizacion = dialogo.Moneda;
                    cotizacion.MonedaPrecioCliente = dialogo.Moneda;
                    cotizacion.FechaInicioCliente = dialogo.FechaInicio;
                    cotizacion.FechaEntregaCliente = dialogo.FechaObjetivo;
                    cotizacion.Descripcion = dialogo.Descripcion;
                    cotizacion.NombreArchivo = "";

                    proyectoCotizacionActual =
                        ProyectoCotizacionJsonService.CrearProyectoVacio(
                            dialogo.NombreProyecto,
                            dialogo.Cliente
                        );
                    proyectoCotizacionActual.Descripcion = dialogo.Descripcion;
                    proyectoCotizacionActual.Metadata.Valores["TipoProyecto"] =
                        dialogo.TipoProyecto;
                    cotizacion.ProyectoProductivo = proyectoCotizacionActual;

                    RefrescarTodo();
                    RefrescarProyectoUI();
                }
                finally
                {
                    cargandoProyectoDesdeArchivo = false;
                }

                MarcarProyectoConCambiosPendientes();
                AplicarModoAplicacion(ModoAplicacion.Proyecto);
            }
        }

        private void ReiniciarEstadoLocalParaNuevoProyecto()
        {
            itemProyectoSeleccionado = null;
            nodoProyectoSeleccionado = null;
            procesoProyectoSeleccionado = null;
            itemProyectoEnEdicionActual = null;
            cotizacionRaizAntesEdicionItem = null;
            expandidoProyectoPresentacion = null;
            nodosProyectoPresentacion = new List<ProjectStructureNode>();
            nodosProyectoExpandidos.Clear();
            inspectorProyectoVisible = false;
            estructuraProyectoExpandida = false;
            columnaProyectoSeleccionada = "Elemento";

            resolucionesDependenciasSubEtapas =
                new List<ResolucionDependenciaSubEtapa>();
            bibliotecaCargosGenerales = new List<CategoriaTrabajador>();
            bibliotecaCargosProyectoInforme = null;
            bibliotecaPersonalProyectoInforme = null;
            subEtapasBasePreparadas = false;
            refrescoEtapasPendiente = false;
            etapaExpandidaEnTabla = null;
            etapaSeleccionadaParaSubGantt = null;
            etapasExpandidasEnTabla.Clear();

            producto2DSeleccionado = null;
            indiceArrastreProducto2D = -1;
            reiniciarBriefEnProximoCambioProducto = false;
            bloqueandoEventosDuracionProducto = false;
            bloqueandoEventosCantidadGlobalProducto = false;
            actualizandoEntregablesPorLote = false;
            presupuestoClienteModo = "Sin informar";
            presupuestoClienteRapidoCLP = 0.0;
            monedaClienteSeleccionadaRapida = "CLP";

            historialNavegacionPrincipal.Clear();
            estadoNavegacionActual = null;
            navegandoDesdeHistorial = false;
            proyectoTieneCambiosPendientes = false;
        }

        private void AbrirCatalogoProductosServicios()
        {
            if (tabs == null || tabCatalogoProductosServiciosPrincipal == null)
            {
                return;
            }

            if (tabCatalogoProductosServiciosPrincipal.Controls.Count == 0)
            {
                ConstruirTabCatalogoProductosServicios(tabCatalogoProductosServiciosPrincipal);
            }

            if (!tabs.TabPages.Contains(tabCatalogoProductosServiciosPrincipal))
            {
                tabs.TabPages.Add(tabCatalogoProductosServiciosPrincipal);
            }

            tabs.SelectedTab = tabCatalogoProductosServiciosPrincipal;
        }
    }
}
