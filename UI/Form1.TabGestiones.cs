using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private DataGridView dgvGestionesProductivas = new DataGridView();
        private Label lblEstadoGestionesProductivas = new Label();

        private void ConstruirTabGestionesProductivas(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 5;
            root.Padding = new Padding(22, 18, 22, 22);
            root.BackColor = Color.White;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titulo = new Label();
            titulo.Text = "Gestiones productivas";
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.AutoSize = true;

            Label ayuda = new Label();
            ayuda.Text = "Define que direcciones y coordinaciones se calculan desde el desglose. Cada gestion cobra minutos por dia-persona de las pegas asociadas por tokens.";
            ayuda.Font = new Font("Segoe UI", 9.5f);
            ayuda.ForeColor = Color.FromArgb(90, 90, 90);
            ayuda.AutoSize = true;
            ayuda.MaximumSize = new Size(1150, 0);
            ayuda.Margin = new Padding(0, 0, 0, 10);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.Margin = new Padding(0, 6, 0, 14);
            acciones.Padding = new Padding(0, 2, 0, 2);

            Button btnAgregar = CrearBotonGestion("Agregar");
            btnAgregar.Click += (s, e) => AgregarGestionProductiva();

            Button btnQuitar = CrearBotonGestion("Quitar");
            btnQuitar.Click += (s, e) => QuitarGestionProductiva();

            Button btnGuardar = CrearBotonGestion("Guardar biblioteca");
            btnGuardar.Width = 150;
            btnGuardar.Click += (s, e) => GuardarBibliotecaGestionesProductivas();

            Button btnRestaurar = CrearBotonGestion("Restaurar base");
            btnRestaurar.Width = 130;
            btnRestaurar.Click += (s, e) => RestaurarBibliotecaGestionesProductivas();

            acciones.Controls.Add(btnAgregar);
            acciones.Controls.Add(btnQuitar);
            acciones.Controls.Add(btnGuardar);
            acciones.Controls.Add(btnRestaurar);

            ConfigurarGrillaGestionesProductivas();

            lblEstadoGestionesProductivas.AutoSize = true;
            lblEstadoGestionesProductivas.ForeColor = Color.FromArgb(90, 90, 90);
            lblEstadoGestionesProductivas.Margin = new Padding(0, 8, 0, 0);

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(ayuda, 0, 1);
            root.Controls.Add(acciones, 0, 2);
            root.Controls.Add(dgvGestionesProductivas, 0, 3);
            root.Controls.Add(lblEstadoGestionesProductivas, 0, 4);

            tab.Controls.Add(root);

            tab.Enter -= TabGestionesProductivas_Enter;
            tab.Enter += TabGestionesProductivas_Enter;
        }

        private Button CrearBotonGestion(string texto)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 110;
            boton.Height = 30;
            boton.Margin = new Padding(0, 0, 8, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.BackColor = Color.FromArgb(245, 245, 245);
            boton.ForeColor = Color.Black;
            boton.UseVisualStyleBackColor = false;
            return boton;
        }

        private void ConfigurarGrillaGestionesProductivas()
        {
            dgvGestionesProductivas.Dock = DockStyle.Fill;
            dgvGestionesProductivas.AllowUserToAddRows = false;
            dgvGestionesProductivas.AllowUserToDeleteRows = false;
            dgvGestionesProductivas.RowHeadersVisible = false;
            dgvGestionesProductivas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGestionesProductivas.MultiSelect = false;
            dgvGestionesProductivas.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvGestionesProductivas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvGestionesProductivas.BackgroundColor = Color.White;
            dgvGestionesProductivas.BorderStyle = BorderStyle.FixedSingle;
            dgvGestionesProductivas.GridColor = Color.Gainsboro;
            dgvGestionesProductivas.EnableHeadersVisualStyles = false;
            dgvGestionesProductivas.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvGestionesProductivas.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvGestionesProductivas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            dgvGestionesProductivas.Columns.Clear();
            dgvGestionesProductivas.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Activo", HeaderText = "Activo", Width = 60 });
            dgvGestionesProductivas.Columns.Add("Area", "Area / direccion");
            dgvGestionesProductivas.Columns.Add("Cargo", "Cargo");
            dgvGestionesProductivas.Columns.Add("NivelCargo", "Nivel");
            dgvGestionesProductivas.Columns.Add("EtapaReferencia", "Etapa ref.");
            dgvGestionesProductivas.Columns.Add("TokensAsociados", "Procesos asociados");
            dgvGestionesProductivas.Columns.Add("MinutosPorDiaPersona", "Min/dia-persona");
            dgvGestionesProductivas.Columns.Add("Descripcion", "Descripcion");

            SetColGestion("Area", 190);
            SetColGestion("Cargo", 230);
            SetColGestion("NivelCargo", 90);
            SetColGestion("EtapaReferencia", 110);
            SetColGestion("TokensAsociados", 430);
            SetColGestion("MinutosPorDiaPersona", 120);
            SetColGestion("Descripcion", 360);
        }

        private void SetColGestion(string nombre, int ancho)
        {
            if (dgvGestionesProductivas.Columns.Contains(nombre))
            {
                dgvGestionesProductivas.Columns[nombre].Width = ancho;
            }
        }

        private void TabGestionesProductivas_Enter(object sender, EventArgs e)
        {
            CargarBibliotecaGestionesProductivasEnPantalla();
        }

        private void CargarBibliotecaGestionesProductivasEnPantalla()
        {
            dgvGestionesProductivas.Rows.Clear();

            foreach (GestionProductivaRegla g in BibliotecaGestionesProductivasJsonService.CargarGestiones())
            {
                int rowIndex = dgvGestionesProductivas.Rows.Add();
                DataGridViewRow row = dgvGestionesProductivas.Rows[rowIndex];
                row.Tag = g;
                row.Cells["Activo"].Value = g.Activo;
                row.Cells["Area"].Value = g.Area;
                row.Cells["Cargo"].Value = g.Cargo;
                row.Cells["NivelCargo"].Value = g.NivelCargo;
                row.Cells["EtapaReferencia"].Value = g.EtapaReferencia;
                row.Cells["TokensAsociados"].Value = g.TokensAsociados;
                row.Cells["MinutosPorDiaPersona"].Value = g.MinutosPorDiaPersona.ToString("0.##");
                row.Cells["Descripcion"].Value = g.Descripcion;
            }

            lblEstadoGestionesProductivas.Text =
                "Biblioteca: " + BibliotecaGestionesProductivasJsonService.ObtenerRutaGestiones();
        }

        private void GuardarBibliotecaGestionesProductivas()
        {
            dgvGestionesProductivas.EndEdit();

            List<GestionProductivaRegla> lista = new List<GestionProductivaRegla>();
            int id = 1;

            foreach (DataGridViewRow row in dgvGestionesProductivas.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                lista.Add(new GestionProductivaRegla
                {
                    Id = id++,
                    Activo = Convert.ToBoolean(row.Cells["Activo"].Value ?? false),
                    Area = Convert.ToString(row.Cells["Area"].Value) ?? "",
                    Cargo = Convert.ToString(row.Cells["Cargo"].Value) ?? "",
                    NivelCargo = Convert.ToString(row.Cells["NivelCargo"].Value) ?? "típico",
                    EtapaReferencia = Convert.ToString(row.Cells["EtapaReferencia"].Value) ?? "General",
                    TokensAsociados = Convert.ToString(row.Cells["TokensAsociados"].Value) ?? "",
                    MinutosPorDiaPersona = ParsearDoubleDesglose(row.Cells["MinutosPorDiaPersona"].Value, 10.0),
                    Descripcion = Convert.ToString(row.Cells["Descripcion"].Value) ?? ""
                });
            }

            BibliotecaGestionesProductivasJsonService.GuardarGestiones(lista);

            if (cotizacion != null)
            {
                cotizacion.DesgloseProductivo = null;
            }

            lblEstadoGestionesProductivas.Text =
                "Gestiones guardadas. El desglose se recalculara con esta biblioteca.";
        }

        private void AgregarGestionProductiva()
        {
            int rowIndex = dgvGestionesProductivas.Rows.Add();
            DataGridViewRow row = dgvGestionesProductivas.Rows[rowIndex];
            row.Cells["Activo"].Value = true;
            row.Cells["Area"].Value = "Nueva gestion";
            row.Cells["Cargo"].Value = "Productor / Project manager";
            row.Cells["NivelCargo"].Value = "típico";
            row.Cells["EtapaReferencia"].Value = "General";
            row.Cells["TokensAsociados"].Value = "*";
            row.Cells["MinutosPorDiaPersona"].Value = "10";
            row.Cells["Descripcion"].Value = "";
        }

        private void QuitarGestionProductiva()
        {
            if (dgvGestionesProductivas.CurrentRow != null)
            {
                dgvGestionesProductivas.Rows.Remove(dgvGestionesProductivas.CurrentRow);
            }
        }

        private void RestaurarBibliotecaGestionesProductivas()
        {
            BibliotecaGestionesProductivasJsonService.RegenerarDesdeBase();
            CargarBibliotecaGestionesProductivasEnPantalla();
            lblEstadoGestionesProductivas.Text = "Biblioteca base restaurada.";
        }
    }
}
