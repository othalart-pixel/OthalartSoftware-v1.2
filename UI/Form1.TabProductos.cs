using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private DataGridView dgvProductos2D = new DataGridView();
        private DataGridView dgvProductos2DSubproductos = new DataGridView();
        private Panel panelProductos2DSubproductos = new Panel();
        private Label lblEstadoProductos2D = new Label();
        private List<Producto2DDefinicion> productos2DEditables =
            new List<Producto2DDefinicion>();
        private Producto2DDefinicion producto2DSeleccionado;
        private bool cargandoProductos2D = false;
        private int indiceArrastreProducto2D = -1;

        private void ConstruirTabProductos2D(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 7;
            root.Padding = new Padding(22, 18, 22, 22);
            root.BackColor = Color.White;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titulo = new Label();
            titulo.Text = "Productos y recetas JSON";
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.AutoSize = true;

            Label ayuda = new Label();
            ayuda.Text = "Edita la biblioteca maestra de productos. Cada producto guarda sus piezas, etapa productiva, subetapa/trabajo, dependencias y cargos sugeridos en productos2d.json.";
            ayuda.Font = new Font("Segoe UI", 9.5f);
            ayuda.ForeColor = Color.FromArgb(90, 90, 90);
            ayuda.AutoSize = true;
            ayuda.MaximumSize = new Size(1180, 0);
            ayuda.Margin = new Padding(0, 0, 0, 10);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.Margin = new Padding(0, 6, 0, 12);
            acciones.Padding = new Padding(0, 2, 0, 2);

            Button btnAgregar = CrearBotonProducto2D("Agregar producto", 140);
            btnAgregar.Click += (s, e) => AgregarProducto2D();

            Button btnDuplicar = CrearBotonProducto2D("Duplicar", 100);
            btnDuplicar.Click += (s, e) => DuplicarProducto2DSeleccionado();

            Button btnQuitar = CrearBotonProducto2D("Quitar", 90);
            btnQuitar.Click += (s, e) => QuitarProducto2DSeleccionado();

            Button btnSubir = CrearBotonProducto2D("Subir", 80);
            btnSubir.Click += (s, e) => MoverProducto2DSeleccionado(-1);

            Button btnBajar = CrearBotonProducto2D("Bajar", 80);
            btnBajar.Click += (s, e) => MoverProducto2DSeleccionado(1);

            Button btnGuardar = CrearBotonProducto2D("Guardar JSON", 125);
            btnGuardar.Click += (s, e) => GuardarBibliotecaProductos2D();

            Button btnRecargar = CrearBotonProducto2D("Recargar", 100);
            btnRecargar.Click += (s, e) => CargarBibliotecaProductos2DEnPantalla();

            Button btnRestaurar = CrearBotonProducto2D("Restaurar base", 130);
            btnRestaurar.Click += (s, e) => RestaurarBibliotecaProductos2D();

            acciones.Controls.Add(btnAgregar);
            acciones.Controls.Add(btnDuplicar);
            acciones.Controls.Add(btnQuitar);
            acciones.Controls.Add(btnSubir);
            acciones.Controls.Add(btnBajar);
            acciones.Controls.Add(btnGuardar);
            acciones.Controls.Add(btnRecargar);
            acciones.Controls.Add(btnRestaurar);

            ConfigurarGrillaProductos2D();

            Label lblSubproductos = new Label();
            lblSubproductos.Text = "Receta del producto seleccionado";
            lblSubproductos.AutoSize = true;
            lblSubproductos.Font = new Font("Segoe UI", 11.0f, FontStyle.Bold);
            lblSubproductos.Margin = new Padding(0, 12, 0, 6);

            panelProductos2DSubproductos.Dock = DockStyle.Fill;
            panelProductos2DSubproductos.BackColor = Color.White;

            lblEstadoProductos2D.AutoSize = true;
            lblEstadoProductos2D.ForeColor = Color.FromArgb(90, 90, 90);
            lblEstadoProductos2D.Margin = new Padding(0, 8, 0, 0);

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(ayuda, 0, 1);
            root.Controls.Add(acciones, 0, 2);
            root.Controls.Add(dgvProductos2D, 0, 3);
            root.Controls.Add(lblSubproductos, 0, 4);
            root.Controls.Add(panelProductos2DSubproductos, 0, 5);
            root.Controls.Add(lblEstadoProductos2D, 0, 6);

            tab.Controls.Add(root);

            tab.Enter -= TabProductos2D_Enter;
            tab.Enter += TabProductos2D_Enter;

            CargarBibliotecaProductos2DEnPantalla();
        }

        private Button CrearBotonProducto2D(string texto, int ancho)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = ancho;
            boton.Height = 30;
            boton.Margin = new Padding(0, 0, 8, 6);
            boton.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            return boton;
        }

        private void ConfigurarGrillaProductos2D()
        {
            dgvProductos2D.Dock = DockStyle.Fill;
            dgvProductos2D.AllowUserToAddRows = false;
            dgvProductos2D.AllowUserToDeleteRows = false;
            dgvProductos2D.RowHeadersVisible = false;
            dgvProductos2D.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProductos2D.MultiSelect = false;
            dgvProductos2D.EditMode = DataGridViewEditMode.EditProgrammatically;
            dgvProductos2D.AllowDrop = true;
            dgvProductos2D.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvProductos2D.BackgroundColor = Color.White;
            dgvProductos2D.BorderStyle = BorderStyle.FixedSingle;
            dgvProductos2D.GridColor = Color.Gainsboro;
            dgvProductos2D.EnableHeadersVisualStyles = false;
            dgvProductos2D.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvProductos2D.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvProductos2D.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgvProductos2D.Columns.Clear();
            dgvProductos2D.Columns.Add("Nombre", "Producto");
            dgvProductos2D.Columns.Add("Industria", "Industria");
            dgvProductos2D.Columns.Add("Categoria", "Categoria");
            dgvProductos2D.Columns.Add("UnidadCantidadSugerida", "Unidad cantidad");
            dgvProductos2D.Columns.Add("UnidadDuracionSugerida", "Unidad duracion");
            dgvProductos2D.Columns.Add("DuracionSugerida", "Duracion base");
            dgvProductos2D.Columns.Add("Nota", "Nota");

            AjustarColumnaProducto2D("Nombre", 250);
            AjustarColumnaProducto2D("Industria", 150);
            AjustarColumnaProducto2D("Categoria", 145);
            AjustarColumnaProducto2D("UnidadCantidadSugerida", 125);
            AjustarColumnaProducto2D("UnidadDuracionSugerida", 125);
            AjustarColumnaProducto2D("DuracionSugerida", 105);
            AjustarColumnaProducto2D("Nota", 320);

            dgvProductos2D.SelectionChanged -= DgvProductos2D_SelectionChanged;
            dgvProductos2D.SelectionChanged += DgvProductos2D_SelectionChanged;
            dgvProductos2D.CellEndEdit -= DgvProductos2D_CellEndEdit;
            dgvProductos2D.CellEndEdit += DgvProductos2D_CellEndEdit;
            dgvProductos2D.CellDoubleClick -= DgvProductos2D_CellDoubleClick;
            dgvProductos2D.CellDoubleClick += DgvProductos2D_CellDoubleClick;
            dgvProductos2D.MouseDown -= DgvProductos2D_MouseDown;
            dgvProductos2D.MouseDown += DgvProductos2D_MouseDown;
            dgvProductos2D.MouseMove -= DgvProductos2D_MouseMove;
            dgvProductos2D.MouseMove += DgvProductos2D_MouseMove;
            dgvProductos2D.DragOver -= DgvProductos2D_DragOver;
            dgvProductos2D.DragOver += DgvProductos2D_DragOver;
            dgvProductos2D.DragDrop -= DgvProductos2D_DragDrop;
            dgvProductos2D.DragDrop += DgvProductos2D_DragDrop;
        }

        private void DgvProductos2D_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            dgvProductos2D.CurrentCell = dgvProductos2D.Rows[e.RowIndex].Cells[e.ColumnIndex];
            dgvProductos2D.BeginEdit(true);
        }

        private void AjustarColumnaProducto2D(string nombre, int ancho)
        {
            if (dgvProductos2D.Columns.Contains(nombre))
            {
                dgvProductos2D.Columns[nombre].Width = ancho;
            }
        }

        private void TabProductos2D_Enter(object sender, EventArgs e)
        {
            if (productos2DEditables.Count == 0)
            {
                CargarBibliotecaProductos2DEnPantalla();
            }
        }

        private void AbrirTabProductos2D(string nombreProducto)
        {
            CargarBibliotecaProductos2DEnPantalla();
            AbrirTabPrincipal(tabProductosPrincipal, true);

            SeleccionarProducto2DEnGrilla(nombreProducto);
        }

        private void SeleccionarProducto2DEnGrilla(string nombreProducto)
        {
            if (string.IsNullOrWhiteSpace(nombreProducto))
            {
                return;
            }

            foreach (DataGridViewRow row in dgvProductos2D.Rows)
            {
                Producto2DDefinicion producto = row.Tag as Producto2DDefinicion;

                if (producto == null)
                {
                    continue;
                }

                if (NormalizarTextoDatosVisual(producto.Nombre) !=
                    NormalizarTextoDatosVisual(nombreProducto))
                {
                    continue;
                }

                dgvProductos2D.ClearSelection();
                row.Selected = true;
                dgvProductos2D.CurrentCell = row.Cells["Nombre"];
                SeleccionarProducto2DDesdeFila(row);
                break;
            }
        }

        private void CargarBibliotecaProductos2DEnPantalla()
        {
            cargandoProductos2D = true;
            dgvProductos2D.Rows.Clear();
            panelProductos2DSubproductos.Controls.Clear();
            producto2DSeleccionado = null;

            productos2DEditables = BibliotecaProductos2DJsonService
                .CargarProductos()
                .Select(ClonarProducto2D)
                .ToList();

            foreach (Producto2DDefinicion producto in productos2DEditables)
            {
                AgregarFilaProducto2D(producto);
            }

            cargandoProductos2D = false;

            if (dgvProductos2D.Rows.Count > 0)
            {
                dgvProductos2D.Rows[0].Selected = true;
                dgvProductos2D.CurrentCell = dgvProductos2D.Rows[0].Cells["Nombre"];
                SeleccionarProducto2DDesdeFila(dgvProductos2D.Rows[0]);
            }

            lblEstadoProductos2D.Text =
                "Biblioteca: " + BibliotecaProductos2DJsonService.ObtenerRutaProductos();
        }

        private void AgregarFilaProducto2D(Producto2DDefinicion producto)
        {
            int rowIndex = dgvProductos2D.Rows.Add();
            DataGridViewRow row = dgvProductos2D.Rows[rowIndex];
            row.Tag = producto;
            PoblarFilaProducto2D(row, producto);
        }

        private void PoblarFilaProducto2D(DataGridViewRow row, Producto2DDefinicion producto)
        {
            row.Cells["Nombre"].Value = producto.Nombre;
            row.Cells["Industria"].Value = producto.Industria;
            row.Cells["Categoria"].Value = producto.Categoria;
            row.Cells["UnidadCantidadSugerida"].Value = producto.UnidadCantidadSugerida;
            row.Cells["UnidadDuracionSugerida"].Value = producto.UnidadDuracionSugerida;
            row.Cells["DuracionSugerida"].Value = producto.DuracionSugerida.ToString("0.##");
            row.Cells["Nota"].Value = producto.Nota;
        }

        private void DgvProductos2D_SelectionChanged(object sender, EventArgs e)
        {
            if (cargandoProductos2D || dgvProductos2D.CurrentRow == null)
            {
                return;
            }

            SincronizarProducto2DSeleccionadoDesdeEditor();
            SeleccionarProducto2DDesdeFila(dgvProductos2D.CurrentRow);
        }

        private void DgvProductos2D_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvProductos2D.Rows.Count)
            {
                return;
            }

            SincronizarProducto2DDesdeFila(dgvProductos2D.Rows[e.RowIndex]);
        }

        private void DgvProductos2D_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = dgvProductos2D.HitTest(e.X, e.Y);
            indiceArrastreProducto2D = hit.RowIndex >= 0 ? hit.RowIndex : -1;
        }

        private void DgvProductos2D_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || indiceArrastreProducto2D < 0 ||
                indiceArrastreProducto2D >= dgvProductos2D.Rows.Count)
            {
                return;
            }

            dgvProductos2D.DoDragDrop(
                dgvProductos2D.Rows[indiceArrastreProducto2D],
                DragDropEffects.Move
            );
        }

        private void DgvProductos2D_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void DgvProductos2D_DragDrop(object sender, DragEventArgs e)
        {
            Point punto = dgvProductos2D.PointToClient(new Point(e.X, e.Y));
            DataGridView.HitTestInfo hit = dgvProductos2D.HitTest(punto.X, punto.Y);
            int destino = hit.RowIndex >= 0 ? hit.RowIndex : dgvProductos2D.Rows.Count - 1;

            MoverProducto2D(indiceArrastreProducto2D, destino);
            indiceArrastreProducto2D = -1;
        }

        private void SeleccionarProducto2DDesdeFila(DataGridViewRow row)
        {
            Producto2DDefinicion producto = row?.Tag as Producto2DDefinicion;

            if (producto == null)
            {
                return;
            }

            SincronizarProducto2DDesdeFila(row);
            producto2DSeleccionado = producto;
            CargarSubproductosProducto2D(producto);
        }

        private void CargarSubproductosProducto2D(Producto2DDefinicion producto)
        {
            panelProductos2DSubproductos.Controls.Clear();
            dgvProductos2DSubproductos = CrearGrillaSubproductosProducto(producto);
            panelProductos2DSubproductos.Controls.Add(
                CrearPanelEditorPipelineGrid(dgvProductos2DSubproductos, "subproducto")
            );
        }

        private void SincronizarProducto2DSeleccionadoDesdeEditor()
        {
            if (producto2DSeleccionado == null || dgvProductos2DSubproductos == null)
            {
                return;
            }

            dgvProductos2DSubproductos.EndEdit();
            producto2DSeleccionado.Subproductos =
                LeerSubproductosProductoDesdeGrilla(dgvProductos2DSubproductos);
            producto2DSeleccionado.Etapas =
                ObtenerEtapasSecuencialesProducto(producto2DSeleccionado);
        }

        private void SincronizarProducto2DDesdeFila(DataGridViewRow row)
        {
            Producto2DDefinicion producto = row?.Tag as Producto2DDefinicion;

            if (producto == null)
            {
                return;
            }

            producto.Nombre = Convert.ToString(row.Cells["Nombre"].Value)?.Trim() ?? "";
            producto.Industria = Convert.ToString(row.Cells["Industria"].Value)?.Trim() ?? "";
            producto.Categoria = Convert.ToString(row.Cells["Categoria"].Value)?.Trim() ?? "";
            producto.UnidadCantidadSugerida =
                Convert.ToString(row.Cells["UnidadCantidadSugerida"].Value)?.Trim() ?? "piezas";
            producto.UnidadDuracionSugerida =
                Convert.ToString(row.Cells["UnidadDuracionSugerida"].Value)?.Trim() ?? "segundos";
            producto.DuracionSugerida =
                ParsearDoubleProducto2D(row.Cells["DuracionSugerida"].Value, producto.DuracionSugerida);
            producto.Nota = Convert.ToString(row.Cells["Nota"].Value)?.Trim() ?? "";

            if (producto.Subproductos == null)
            {
                producto.Subproductos = new List<Subproducto2D>();
            }
        }

        private double ParsearDoubleProducto2D(object valor, double fallback)
        {
            double resultado;
            string texto = Convert.ToString(valor)?.Trim() ?? "";

            if (double.TryParse(texto, out resultado))
            {
                return resultado;
            }

            if (double.TryParse(texto.Replace(".", ","), out resultado))
            {
                return resultado;
            }

            if (double.TryParse(texto.Replace(",", "."), out resultado))
            {
                return resultado;
            }

            return fallback;
        }

        private void AgregarProducto2D()
        {
            SincronizarProducto2DSeleccionadoDesdeEditor();

            Producto2DDefinicion producto = new Producto2DDefinicion
            {
                Nombre = CrearNombreProducto2DDisponible("Nuevo producto 2D"),
                Industria = "General",
                Categoria = "Producto final",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 0.0,
                Nota = "Producto agregado desde la biblioteca editable.",
                Subproductos = new List<Subproducto2D>()
            };

            producto.Etapas = ObtenerEtapasSecuencialesProducto(producto);
            productos2DEditables.Add(producto);
            AgregarFilaProducto2D(producto);

            int rowIndex = dgvProductos2D.Rows.Count - 1;
            dgvProductos2D.CurrentCell = dgvProductos2D.Rows[rowIndex].Cells["Nombre"];
            dgvProductos2D.Rows[rowIndex].Selected = true;
            SeleccionarProducto2DDesdeFila(dgvProductos2D.Rows[rowIndex]);
        }

        private void DuplicarProducto2DSeleccionado()
        {
            if (dgvProductos2D.CurrentRow == null)
            {
                return;
            }

            SincronizarProducto2DSeleccionadoDesdeEditor();
            Producto2DDefinicion origen = dgvProductos2D.CurrentRow.Tag as Producto2DDefinicion;

            if (origen == null)
            {
                return;
            }

            Producto2DDefinicion copia = ClonarProducto2D(origen);
            copia.Nombre = CrearNombreProducto2DDisponible(origen.Nombre + " copia");
            int indiceOrigen = dgvProductos2D.CurrentRow.Index;
            int indiceCopia = Math.Min(indiceOrigen + 1, dgvProductos2D.Rows.Count);
            productos2DEditables.Insert(indiceCopia, copia);
            dgvProductos2D.Rows.Insert(indiceCopia, 1);
            DataGridViewRow filaCopia = dgvProductos2D.Rows[indiceCopia];
            filaCopia.Tag = copia;
            PoblarFilaProducto2D(filaCopia, copia);

            dgvProductos2D.CurrentCell = filaCopia.Cells["Nombre"];
            filaCopia.Selected = true;
            SeleccionarProducto2DDesdeFila(filaCopia);
        }

        private void MoverProducto2DSeleccionado(int direccion)
        {
            if (dgvProductos2D.CurrentRow == null)
            {
                return;
            }

            int origen = dgvProductos2D.CurrentRow.Index;
            MoverProducto2D(origen, origen + direccion);
        }

        private void MoverProducto2D(int origen, int destino)
        {
            if (origen < 0 || origen >= dgvProductos2D.Rows.Count ||
                destino < 0 || destino >= dgvProductos2D.Rows.Count ||
                origen == destino)
            {
                return;
            }

            dgvProductos2D.EndEdit();
            SincronizarProducto2DSeleccionadoDesdeEditor();
            SincronizarProducto2DDesdeFila(dgvProductos2D.Rows[origen]);

            Producto2DDefinicion producto = dgvProductos2D.Rows[origen].Tag as Producto2DDefinicion;

            if (producto == null)
            {
                return;
            }

            cargandoProductos2D = true;
            productos2DEditables.Remove(producto);
            productos2DEditables.Insert(destino, producto);

            dgvProductos2D.Rows.RemoveAt(origen);
            dgvProductos2D.Rows.Insert(destino, 1);
            DataGridViewRow nuevaFila = dgvProductos2D.Rows[destino];
            nuevaFila.Tag = producto;
            PoblarFilaProducto2D(nuevaFila, producto);

            dgvProductos2D.ClearSelection();
            nuevaFila.Selected = true;
            dgvProductos2D.CurrentCell = nuevaFila.Cells["Nombre"];
            cargandoProductos2D = false;

            SeleccionarProducto2DDesdeFila(nuevaFila);
            lblEstadoProductos2D.Text =
                "Orden actualizado. Presiona Guardar JSON para usar este orden en Datos.";
        }

        private void QuitarProducto2DSeleccionado()
        {
            if (dgvProductos2D.CurrentRow == null)
            {
                return;
            }

            Producto2DDefinicion producto = dgvProductos2D.CurrentRow.Tag as Producto2DDefinicion;

            if (producto != null)
            {
                productos2DEditables.Remove(producto);
            }

            dgvProductos2D.Rows.Remove(dgvProductos2D.CurrentRow);
            producto2DSeleccionado = null;
            panelProductos2DSubproductos.Controls.Clear();

            if (dgvProductos2D.Rows.Count > 0)
            {
                dgvProductos2D.CurrentCell = dgvProductos2D.Rows[0].Cells["Nombre"];
                SeleccionarProducto2DDesdeFila(dgvProductos2D.Rows[0]);
            }
        }

        private void GuardarBibliotecaProductos2D()
        {
            dgvProductos2D.EndEdit();
            SincronizarProducto2DSeleccionadoDesdeEditor();

            List<Producto2DDefinicion> productos = new List<Producto2DDefinicion>();

            foreach (DataGridViewRow row in dgvProductos2D.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                SincronizarProducto2DDesdeFila(row);
                Producto2DDefinicion producto = row.Tag as Producto2DDefinicion;

                if (producto == null || string.IsNullOrWhiteSpace(producto.Nombre))
                {
                    continue;
                }

                producto.Subproductos = producto.Subproductos ?? new List<Subproducto2D>();
                producto.Etapas = ObtenerEtapasSecuencialesProducto(producto);
                productos.Add(producto);
            }

            List<Producto2DDefinicion> productosUnicos = new List<Producto2DDefinicion>();
            HashSet<string> productosVistos = new HashSet<string>();

            foreach (Producto2DDefinicion producto in productos)
            {
                string clave = NormalizarTextoDatosVisual(producto.Nombre);

                if (string.IsNullOrWhiteSpace(clave) || !productosVistos.Add(clave))
                {
                    continue;
                }

                productosUnicos.Add(producto);
            }

            productos = productosUnicos;

            BibliotecaProductos2DJsonService.GuardarProductos(productos);
            productos2DEditables = productos.Select(ClonarProducto2D).ToList();

            string productoActual = producto2DSeleccionado != null &&
                !string.IsNullOrWhiteSpace(producto2DSeleccionado.Nombre)
                    ? producto2DSeleccionado.Nombre
                    : cmbProductoServicio.SelectedItem?.ToString() ?? "";
            CargarProductosServiciosSegunTipo();
            SeleccionarComboSiExiste(cmbProductoServicio, productoActual);
            RefrescarOpcionesSegunProducto();

            if (cotizacion != null)
            {
                cotizacion.DesgloseProductivo = null;
            }

            lblEstadoProductos2D.Text =
                "Productos guardados. Datos, desglose y Gantt usaran productos2d.json.";
        }

        private void RestaurarBibliotecaProductos2D()
        {
            DialogResult respuesta = MessageBox.Show(
                "Esto reemplaza productos2d.json por la biblioteca base del codigo. ¿Continuar?",
                "Restaurar productos",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            BibliotecaProductos2DJsonService.RegenerarProductosBase();
            CargarBibliotecaProductos2DEnPantalla();
            CargarProductosServiciosSegunTipo();
            RefrescarOpcionesSegunProducto();
            lblEstadoProductos2D.Text = "Biblioteca base de productos restaurada.";
        }

        private string CrearNombreProducto2DDisponible(string baseNombre)
        {
            string nombre = string.IsNullOrWhiteSpace(baseNombre)
                ? "Nuevo producto 2D"
                : baseNombre.Trim();

            HashSet<string> existentes = productos2DEditables
                .Where(p => p != null)
                .Select(p => NormalizarTextoDatosVisual(p.Nombre))
                .ToHashSet();

            if (!existentes.Contains(NormalizarTextoDatosVisual(nombre)))
            {
                return nombre;
            }

            int contador = 2;

            while (existentes.Contains(NormalizarTextoDatosVisual(nombre + " " + contador)))
            {
                contador++;
            }

            return nombre + " " + contador;
        }

        private Producto2DDefinicion ClonarProducto2D(Producto2DDefinicion origen)
        {
            if (origen == null)
            {
                return new Producto2DDefinicion();
            }

            return new Producto2DDefinicion
            {
                Nombre = origen.Nombre,
                Industria = origen.Industria,
                Categoria = origen.Categoria,
                UnidadCantidadSugerida = origen.UnidadCantidadSugerida,
                UnidadDuracionSugerida = origen.UnidadDuracionSugerida,
                DuracionSugerida = origen.DuracionSugerida,
                Nota = origen.Nota,
                Etapas = (origen.Etapas ?? new List<ProductoEtapaDefinicion>())
                    .Select(e => new ProductoEtapaDefinicion
                    {
                        ClaveEtapa = e.ClaveEtapa,
                        NombreVisible = e.NombreVisible,
                        Orden = e.Orden,
                        Activa = e.Activa,
                        DependeDe = e.DependeDe,
                        Nota = e.Nota
                    })
                    .ToList(),
                Subproductos = (origen.Subproductos ?? new List<Subproducto2D>())
                    .Select(s => new Subproducto2D
                    {
                        Nombre = s.Nombre,
                        Categoria = s.Categoria,
                        Orden = s.Orden,
                        RequeridoPorDefecto = s.RequeridoPorDefecto,
                        PuedeEntregarCliente = s.PuedeEntregarCliente,
                        EtapaSugerida = s.EtapaSugerida,
                        SubEtapaSugerida = s.SubEtapaSugerida,
                        DependeDe = s.DependeDe,
                        CargosSugeridos = s.CargosSugeridos,
                        EcuacionProductiva = s.EcuacionProductiva,
                        VariablesEcuacion = s.VariablesEcuacion,
                        ImpactoEcuacion = s.ImpactoEcuacion,
                        Resolucion = s.Resolucion,
                        Nota = s.Nota
                    })
                    .ToList()
            };
        }
    }
}
