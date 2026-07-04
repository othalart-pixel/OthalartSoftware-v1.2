using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {

        private GroupBox ConstruirGrupoEntregablesIndustria()
        {
            GroupBox grupo = new GroupBox();
            grupo.Text = "3. Piezas 2D solicitadas";
            grupo.Dock = DockStyle.Top;
            grupo.AutoSize = false;
            grupo.Height = 330;
            grupo.Padding = new Padding(12);
            grupo.Margin = new Padding(0, 8, 0, 0);
            grupo.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold);
            grupo.ForeColor = Color.FromArgb(35, 35, 35);

            ConfigurarGrillaEntregablesIndustria();

            TableLayoutPanel contenedor = new TableLayoutPanel();
            contenedor.Dock = DockStyle.Fill;
            contenedor.ColumnCount = 1;
            contenedor.RowCount = 2;
            contenedor.Margin = new Padding(0);
            contenedor.Padding = new Padding(0);
            contenedor.MinimumSize = new Size(0, 300);

            contenedor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contenedor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            FlowLayoutPanel panelBotones = CrearPanelBotonesEntregables();
            panelBotones.Margin = new Padding(0, 0, 0, 8);

            dgvEntregablesIndustria.Dock = DockStyle.Fill;
            dgvEntregablesIndustria.MinimumSize = new Size(0, 240);

            contenedor.Controls.Add(panelBotones, 0, 0);
            contenedor.Controls.Add(dgvEntregablesIndustria, 0, 1);

            grupo.Controls.Clear();
            grupo.Controls.Add(contenedor);

            return grupo;
        }

        private void ConfigurarGrillaEntregablesIndustria()
        {
            dgvEntregablesIndustria.Dock = DockStyle.Fill;
            dgvEntregablesIndustria.MinimumSize = new Size(0, 240);
            dgvEntregablesIndustria.AllowUserToAddRows = false;
            dgvEntregablesIndustria.AllowUserToDeleteRows = false;
            dgvEntregablesIndustria.AllowUserToResizeRows = false;
            dgvEntregablesIndustria.RowHeadersVisible = false;

            dgvEntregablesIndustria.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvEntregablesIndustria.MultiSelect = false;
            dgvEntregablesIndustria.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvEntregablesIndustria.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvEntregablesIndustria.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvEntregablesIndustria.ScrollBars = ScrollBars.Both;

            dgvEntregablesIndustria.BackgroundColor = Color.White;
            dgvEntregablesIndustria.BorderStyle = BorderStyle.FixedSingle;
            dgvEntregablesIndustria.GridColor = Color.Gainsboro;

            dgvEntregablesIndustria.EnableHeadersVisualStyles = false;
            dgvEntregablesIndustria.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvEntregablesIndustria.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvEntregablesIndustria.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            dgvEntregablesIndustria.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEntregablesIndustria.ColumnHeadersHeight = 30;

            dgvEntregablesIndustria.DefaultCellStyle.Font = new Font("Segoe UI", 9.0f, FontStyle.Regular);
            dgvEntregablesIndustria.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvEntregablesIndustria.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgvEntregablesIndustria.Columns.Clear();

            DataGridViewCheckBoxColumn colUsar = new DataGridViewCheckBoxColumn();
            colUsar.Name = "Usar";
            colUsar.HeaderText = "Usar";
            colUsar.Width = 52;
            colUsar.FlatStyle = FlatStyle.Standard;
            dgvEntregablesIndustria.Columns.Add(colUsar);

            dgvEntregablesIndustria.Columns.Add("Categoria", "Categoría");
            dgvEntregablesIndustria.Columns.Add("Producto2D", "Pieza 2D");
            dgvEntregablesIndustria.Columns.Add("Cantidad", "Cantidad");
            dgvEntregablesIndustria.Columns.Add("DuracionPorUnidad", "Duración");

            DataGridViewComboBoxColumn colUnidadDuracion = new DataGridViewComboBoxColumn();
            colUnidadDuracion.Name = "UnidadDuracion";
            colUnidadDuracion.HeaderText = "Unidad";
            colUnidadDuracion.FlatStyle = FlatStyle.Flat;
            colUnidadDuracion.Items.Add("no aplica");
            colUnidadDuracion.Items.Add("segundos");
            colUnidadDuracion.Items.Add("minutos");
            dgvEntregablesIndustria.Columns.Add(colUnidadDuracion);

            DataGridViewComboBoxColumn colUnidadCantidad = new DataGridViewComboBoxColumn();
            colUnidadCantidad.Name = "UnidadCantidad";
            colUnidadCantidad.HeaderText = "Unidad";
            colUnidadCantidad.FlatStyle = FlatStyle.Flat;
            colUnidadCantidad.Items.Add("piezas");
            colUnidadCantidad.Items.Add("personajes");
            colUnidadCantidad.Items.Add("fondos");
            colUnidadCantidad.Items.Add("props");
            colUnidadCantidad.Items.Add("assets");
            colUnidadCantidad.Items.Add("unidades");
            colUnidadCantidad.Items.Add("cinemáticas");
            dgvEntregablesIndustria.Columns.Add(colUnidadCantidad);

            dgvEntregablesIndustria.Columns.Add("Nota", "Nota");

            dgvEntregablesIndustria.Columns["Categoria"].ReadOnly = true;
            dgvEntregablesIndustria.Columns["Producto2D"].ReadOnly = true;
            dgvEntregablesIndustria.Columns["Nota"].ReadOnly = true;

            dgvEntregablesIndustria.Columns["Usar"].Width = 52;
            dgvEntregablesIndustria.Columns["Categoria"].Width = 130;
            dgvEntregablesIndustria.Columns["Producto2D"].Width = 300;
            dgvEntregablesIndustria.Columns["Cantidad"].Width = 80;
            dgvEntregablesIndustria.Columns["DuracionPorUnidad"].Width = 80;
            dgvEntregablesIndustria.Columns["UnidadDuracion"].Width = 105;
            dgvEntregablesIndustria.Columns["UnidadCantidad"].Width = 105;
            dgvEntregablesIndustria.Columns["Nota"].Width = 230;

            dgvEntregablesIndustria.DataError -= DgvEntregablesIndustria_DataError;
            dgvEntregablesIndustria.DataError += DgvEntregablesIndustria_DataError;

            dgvEntregablesIndustria.CellBeginEdit -= DgvEntregablesIndustria_CellBeginEdit;
            dgvEntregablesIndustria.CellBeginEdit += DgvEntregablesIndustria_CellBeginEdit;

            dgvEntregablesIndustria.CurrentCellDirtyStateChanged -= DgvEntregablesIndustria_CurrentCellDirtyStateChanged;
            dgvEntregablesIndustria.CurrentCellDirtyStateChanged += DgvEntregablesIndustria_CurrentCellDirtyStateChanged;

            dgvEntregablesIndustria.CellValueChanged -= DgvEntregablesIndustria_CellValueChanged;
            dgvEntregablesIndustria.CellValueChanged += DgvEntregablesIndustria_CellValueChanged;
        }

        private void DgvEntregablesIndustria_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Evita que WinForms muestre el popup feo cuando un ComboBoxCell
            // recibe un valor que no está en su lista.
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void DgvEntregablesIndustria_CellBeginEdit(
    object sender,
    DataGridViewCellCancelEventArgs e
)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvEntregablesIndustria.Rows[e.RowIndex];

            if (EsFilaCategoriaEntregables(row))
            {
                e.Cancel = true;
            }
        }

        private bool EsFilaCategoriaEntregables(DataGridViewRow row)
        {
            if (row == null || row.Tag == null)
            {
                return false;
            }

            return row.Tag.ToString() == "CATEGORIA";
        }

        private void DgvEntregablesIndustria_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvEntregablesIndustria == null || !dgvEntregablesIndustria.IsCurrentCellDirty)
            {
                return;
            }

            try
            {
                dgvEntregablesIndustria.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch
            {
            }
        }

        private void DgvEntregablesIndustria_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvEntregablesIndustria == null)
            {
                return;
            }

            string columna = dgvEntregablesIndustria.Columns[e.ColumnIndex].Name;

            if (columna != "Usar")
            {
                return;
            }

            if (actualizandoEntregablesPorLote)
            {
                return;
            }

            RefrescarBotonesCategoriaEntregables();
            AplicarDatosDesdePantalla();
            RefrescarResumen();
            RefrescarPanelSiguientePasoDatos();
        }

        private void CargarEntregablesIndustria(string industria)
        {
            if (dgvEntregablesIndustria == null)
            {
                return;
            }

            dgvEntregablesIndustria.Rows.Clear();
            indiceEntregablesPorCategoria.Clear();

            if (string.IsNullOrWhiteSpace(industria))
            {
                AgregarFilaCategoriaEntregables("Selecciona una industria para ver piezas disponibles");
                return;
            }

            List<Producto2DDefinicion> productos = BibliotecaEntregablesIndustria
                .ObtenerProductosPorIndustria(industria)
                .Where(p => p != null)
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nombre)
                .ToList();

            if (productos.Count == 0)
            {
                AgregarFilaCategoriaEntregables("No hay piezas definidas para esta industria");
                return;
            }

            string categoriaActual = "";

            foreach (Producto2DDefinicion producto in productos)
            {
                string categoria = string.IsNullOrWhiteSpace(producto.Categoria)
                    ? "Sin categoría"
                    : producto.Categoria.Trim();

                if (categoria != categoriaActual)
                {
                    categoriaActual = categoria;
                    AgregarFilaCategoriaEntregables(categoriaActual);
                }

                AgregarFilaProductoEntregable(producto);
            }

            ConstruirIndiceEntregablesPorCategoria();
            RefrescarBotonesCategoriaEntregables();
        }

        private void AgregarFilaCategoriaEntregables(string categoria)
        {
            int rowIndex = dgvEntregablesIndustria.Rows.Add();
            DataGridViewRow row = dgvEntregablesIndustria.Rows[rowIndex];

            row.Tag = "CATEGORIA";

            row.Cells["Usar"].Value = false;
            row.Cells["Categoria"].Value = categoria;
            row.Cells["Producto2D"].Value = "";
            row.Cells["Cantidad"].Value = "";
            row.Cells["DuracionPorUnidad"].Value = "";
            row.Cells["UnidadDuracion"].Value = "no aplica";
            row.Cells["UnidadCantidad"].Value = "unidades";
            row.Cells["Nota"].Value = "";

            row.ReadOnly = true;
            row.DefaultCellStyle.BackColor = Color.FromArgb(238, 238, 238);
            row.DefaultCellStyle.ForeColor = Color.FromArgb(35, 35, 35);
            row.DefaultCellStyle.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold);
            row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 220, 220);
            row.DefaultCellStyle.SelectionForeColor = Color.Black;

            row.Height = 30;
        }

        private void AgregarFilaProductoEntregable(Producto2DDefinicion producto)
        {
            int rowIndex = dgvEntregablesIndustria.Rows.Add();
            DataGridViewRow row = dgvEntregablesIndustria.Rows[rowIndex];

            row.Tag = producto;

            row.Cells["Usar"].Value = false;
            row.Cells["Categoria"].Value = producto.Categoria;
            row.Cells["Producto2D"].Value = producto.Nombre;
            row.Cells["Cantidad"].Value = ObtenerCantidadGlobalProductoActual();
            row.Cells["DuracionPorUnidad"].Value = producto.DuracionSugerida;
            row.Cells["UnidadDuracion"].Value = producto.UnidadDuracionSugerida;
            row.Cells["UnidadCantidad"].Value = producto.UnidadCantidadSugerida;
            row.Cells["Nota"].Value = producto.Nota;

            EstilizarFilaProductoEntregable(row, producto);
        }

        private void EstilizarFilaProductoEntregable(
            DataGridViewRow row,
            Producto2DDefinicion producto
        )
        {
            if (row == null || producto == null)
            {
                return;
            }

            row.DefaultCellStyle.BackColor = Color.White;
            row.DefaultCellStyle.ForeColor = Color.FromArgb(45, 45, 45);
            row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 240, 255);
            row.DefaultCellStyle.SelectionForeColor = Color.Black;

            string categoria = NormalizarTextoDatosVisual(producto.Categoria);

            if (categoria.Contains("video") || categoria.Contains("animacion"))
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(248, 253, 255);
            }
            else if (categoria.Contains("asset") || categoria.Contains("ilustracion") || categoria.Contains("diseño"))
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(250, 255, 250);
            }
            else if (categoria.Contains("adaptacion") || categoria.Contains("version"))
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 242);
            }
            else if (categoria.Contains("desarrollo") || categoria.Contains("preproduccion"))
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(252, 248, 255);
            }

            row.Height = 28;
        }

        private double InferirDuracionProducto2D(string producto)
        {
            string p = NormalizarTextoDatosVisual(producto);

            if (p.Contains("trailer"))
            {
                return 45.0;
            }

            if (p.Contains("cinematica") || p.Contains("cutscene"))
            {
                return 30.0;
            }

            if (p.Contains("loop") || p.Contains("idle") || p.Contains("walk") || p.Contains("run"))
            {
                return 2.0;
            }

            if (p.Contains("attack") || p.Contains("damage") || p.Contains("death"))
            {
                return 1.0;
            }

            return 0.0;
        }

        private string InferirUnidadDuracionProducto2D(string producto)
        {
            string p = NormalizarTextoDatosVisual(producto);

            bool esAssetEstatico =
                p.Contains("background") ||
                p.Contains("escenario") ||
                p.Contains("fondo") ||
                p.Contains("props") ||
                p.Contains("objetos") ||
                p.Contains("asset") ||
                p.Contains("spritesheet") ||
                (p.Contains("personaje") && p.Contains("diseño"));

            if (esAssetEstatico)
            {
                return "no aplica";
            }

            return "segundos";
        }


        private string InferirUnidadCantidadProducto2D(string producto)
        {
            string p = NormalizarTextoDatosVisual(producto);

            if (p.Contains("personaje"))
            {
                return "personajes";
            }

            if (p.Contains("background") || p.Contains("escenario") || p.Contains("fondo"))
            {
                return "fondos";
            }

            if (p.Contains("props") || p.Contains("objetos"))
            {
                return "assets";
            }

            if (p.Contains("ui") || p.Contains("fx") || p.Contains("pack") || p.Contains("asset") || p.Contains("spritesheet"))
            {
                return "assets";
            }

            if (p.Contains("cinematica") || p.Contains("cinemática"))
            {
                return "cinemáticas";
            }

            return "piezas";
        }

    }
}
