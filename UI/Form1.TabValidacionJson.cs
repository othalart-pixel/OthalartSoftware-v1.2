using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabValidacionJson(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 4;
            root.Padding = new Padding(26, 22, 26, 22);
            root.BackColor = Color.White;
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Validacion de vinculos JSON";
            titulo.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 4);

            Label ayuda = new Label();
            ayuda.Text = "Revisa que productos2d.json converse con ecuaciones, cargos, rendimientos, subetapas y dependencias explicitas.";
            ayuda.Font = new Font("Segoe UI", 10f);
            ayuda.ForeColor = Color.FromArgb(80, 80, 80);
            ayuda.AutoSize = true;
            ayuda.Margin = new Padding(0, 0, 0, 14);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Top;
            acciones.AutoSize = true;
            acciones.WrapContents = false;
            acciones.Margin = new Padding(0, 0, 0, 14);

            Button btnActualizar = CrearBotonValidacionJson("Actualizar auditoria", 170);
            btnActualizar.Click += (s, e) => RefrescarValidacionJson();

            Button btnCompletar = CrearBotonValidacionJson("Completar productivos", 190);
            btnCompletar.Click += (s, e) => CompletarRendimientosDesdeValidacionJson();

            Button btnIr = CrearBotonValidacionJson("Abrir editor sugerido", 170);
            btnIr.Click += (s, e) => AbrirEditorDesdeHallazgoValidacionJson();

            acciones.Controls.Add(btnActualizar);
            acciones.Controls.Add(btnCompletar);
            acciones.Controls.Add(btnIr);

            dgvValidacionJson.Dock = DockStyle.Fill;
            dgvValidacionJson.AllowUserToAddRows = false;
            dgvValidacionJson.AllowUserToDeleteRows = false;
            dgvValidacionJson.ReadOnly = true;
            dgvValidacionJson.AutoGenerateColumns = false;
            dgvValidacionJson.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvValidacionJson.MultiSelect = false;
            dgvValidacionJson.RowHeadersVisible = false;
            dgvValidacionJson.BackgroundColor = Color.White;
            dgvValidacionJson.BorderStyle = BorderStyle.FixedSingle;
            dgvValidacionJson.CellDoubleClick -= DgvValidacionJson_CellDoubleClick;
            dgvValidacionJson.CellDoubleClick += DgvValidacionJson_CellDoubleClick;
            ConfigurarColumnasValidacionJson();

            rtbValidacionJsonResumen.Dock = DockStyle.Fill;
            rtbValidacionJsonResumen.ReadOnly = true;
            rtbValidacionJsonResumen.BorderStyle = BorderStyle.FixedSingle;
            rtbValidacionJsonResumen.BackColor = Color.FromArgb(250, 250, 250);
            rtbValidacionJsonResumen.Font = new Font("Consolas", 9.5f);
            rtbValidacionJsonResumen.WordWrap = true;
            rtbValidacionJsonResumen.ScrollBars = RichTextBoxScrollBars.Vertical;

            root.Controls.Add(titulo, 0, 0);
            root.Controls.Add(ayuda, 0, 1);
            root.Controls.Add(acciones, 0, 2);

            TableLayoutPanel contenido = new TableLayoutPanel();
            contenido.Dock = DockStyle.Fill;
            contenido.ColumnCount = 1;
            contenido.RowCount = 2;
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            contenido.Controls.Add(dgvValidacionJson, 0, 0);
            contenido.Controls.Add(rtbValidacionJsonResumen, 0, 1);
            root.SetRow(contenido, 3);
            root.Controls.Add(contenido, 0, 3);

            tab.Controls.Add(root);
            tab.Enter -= TabValidacionJson_Enter;
            tab.Enter += TabValidacionJson_Enter;
        }

        private Button CrearBotonValidacionJson(string texto, int ancho)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = ancho;
            boton.Height = 32;
            boton.Margin = new Padding(0, 0, 10, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = Color.White;
            boton.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
            return boton;
        }

        private void ConfigurarColumnasValidacionJson()
        {
            if (dgvValidacionJson.Columns.Count > 0)
            {
                return;
            }

            AgregarColumnaValidacionJson("Severidad", "Severidad", 95);
            AgregarColumnaValidacionJson("Biblioteca", "Biblioteca", 190);
            AgregarColumnaValidacionJson("Producto", "Producto", 180);
            AgregarColumnaValidacionJson("Pieza", "Pieza / subproducto", 220);
            AgregarColumnaValidacionJson("Campo", "Campo", 145);
            AgregarColumnaValidacionJson("Mensaje", "Mensaje", 420);
            AgregarColumnaValidacionJson("DondeEditar", "Editar en", 150);
        }

        private void AgregarColumnaValidacionJson(string propiedad, string titulo, int ancho)
        {
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = propiedad;
            col.Name = propiedad;
            col.HeaderText = titulo;
            col.Width = ancho;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            dgvValidacionJson.Columns.Add(col);
        }

        private void TabValidacionJson_Enter(object sender, EventArgs e)
        {
            RefrescarValidacionJson();
        }

        private void RefrescarValidacionJson()
        {
            AuditoriaVinculosJsonService.ResultadoAuditoria auditoria =
                AuditoriaVinculosJsonService.Auditar();

            dgvValidacionJson.DataSource = null;
            dgvValidacionJson.DataSource = auditoria.Hallazgos
                .OrderBy(h => h.Severidad == "Error" ? 0 : 1)
                .ThenBy(h => h.Biblioteca)
                .ThenBy(h => h.Producto)
                .ThenBy(h => h.Pieza)
                .ToList();

            foreach (DataGridViewRow row in dgvValidacionJson.Rows)
            {
                string severidad = Convert.ToString(row.Cells["Severidad"].Value) ?? "";
                if (severidad == "Error")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(120, 25, 25);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 247, 220);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(100, 70, 0);
                }
            }

            rtbValidacionJsonResumen.Clear();
            rtbValidacionJsonResumen.Text = AuditoriaVinculosJsonService.ConstruirReporteTexto(auditoria);
        }

        private void DgvValidacionJson_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            AbrirEditorDesdeHallazgoValidacionJson();
        }

        private void CompletarRendimientosDesdeValidacionJson()
        {
            DialogResult respuesta = MessageBox.Show(
                "Esto agregara rendimientos sugeridos para los cargos/procesos faltantes. Quedaran guardados en rendimientos_productivos.json y marcados para revisar capacidad real. ¿Continuar?",
                "Completar rendimientos productivos",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            int limpiados = AuditoriaVinculosJsonService.EliminarRendimientosNoProductivosSugeridos();
            int agregados = AuditoriaVinculosJsonService.CompletarRendimientosFaltantes();
            RefrescarValidacionJson();

            MessageBox.Show(
                agregados == 0 && limpiados == 0
                    ? "No habia rendimientos productivos faltantes ni sugerencias no productivas que limpiar."
                    : "Se agregaron " + agregados + " rendimientos productivos y se limpiaron " + limpiados + " rendimientos sugeridos no productivos.",
                "Completar rendimientos productivos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void AbrirEditorDesdeHallazgoValidacionJson()
        {
            if (dgvValidacionJson.CurrentRow == null)
            {
                return;
            }

            string destino = Convert.ToString(dgvValidacionJson.CurrentRow.Cells["DondeEditar"].Value) ?? "";

            if (destino.Contains("Productos"))
            {
                AbrirTabPrincipal(tabProductosPrincipal, true);
            }
            else if (destino.Contains("Ecuaciones"))
            {
                AbrirTabPrincipal(tabEcuacionesPrincipal, true);
            }
            else if (destino.Contains("Cargos"))
            {
                AbrirTabPrincipal(tabCargosPrincipal, true);
            }
            else if (destino.Contains("Rendimientos"))
            {
                AbrirTabPrincipal(tabRendimientosPrincipal, true);
            }
            else if (destino.Contains("Subetapas"))
            {
                AbrirTabPrincipal(tabSubEtapasPrincipal, true);
            }
            else
            {
                MessageBox.Show(
                    "Este hallazgo no tiene un editor directo sugerido.",
                    "Validacion JSON",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
    }
}
