using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public class FormNuevoProyecto : Form
    {
        private readonly TextBox txtNombre = new TextBox();
        private readonly TextBox txtCliente = new TextBox();
        private readonly ComboBox cmbMoneda = new ComboBox();
        private readonly DateTimePicker dtpInicio = new DateTimePicker();
        private readonly DateTimePicker dtpObjetivo = new DateTimePicker();
        private readonly ComboBox cmbTipo = new ComboBox();
        private readonly TextBox txtDescripcion = new TextBox();

        public string NombreProyecto => txtNombre.Text.Trim();
        public string Cliente => txtCliente.Text.Trim();
        public string Moneda => Convert.ToString(cmbMoneda.SelectedItem) ?? "CLP";
        public DateTime? FechaInicio => dtpInicio.Checked ? dtpInicio.Value.Date : null;
        public DateTime? FechaObjetivo => dtpObjetivo.Checked ? dtpObjetivo.Value.Date : null;
        public string TipoProyecto => Convert.ToString(cmbTipo.SelectedItem) ?? "";
        public string Descripcion => txtDescripcion.Text.Trim();

        public FormNuevoProyecto()
        {
            Text = "Crear nuevo proyecto";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 520;
            Height = 430;
            BackColor = Color.White;
            Padding = new Padding(18);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 9;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Datos iniciales";
            titulo.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 14);
            root.Controls.Add(titulo, 0, 0);
            root.SetColumnSpan(titulo, 2);

            ConfigurarCombo(cmbMoneda, new[] { "CLP", "USD", "EUR" }, "CLP");
            ConfigurarCombo(cmbTipo, new[] { "Animacion", "Diseno", "Produccion audiovisual", "Servicio creativo", "Otro" }, "Animacion");

            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.ShowCheckBox = true;
            dtpInicio.Checked = true;
            dtpObjetivo.Format = DateTimePickerFormat.Short;
            dtpObjetivo.ShowCheckBox = true;
            dtpObjetivo.Checked = false;

            txtDescripcion.Multiline = true;
            txtDescripcion.Height = 72;

            AgregarFila(root, "Nombre", txtNombre, 1);
            AgregarFila(root, "Cliente", txtCliente, 2);
            AgregarFila(root, "Moneda", cmbMoneda, 3);
            AgregarFila(root, "Fecha inicio", dtpInicio, 4);
            AgregarFila(root, "Fecha objetivo", dtpObjetivo, 5);
            AgregarFila(root, "Tipo de proyecto", cmbTipo, 6);
            AgregarFila(root, "Descripcion", txtDescripcion, 7);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.FlowDirection = FlowDirection.RightToLeft;
            acciones.Dock = DockStyle.Fill;
            acciones.Margin = new Padding(0, 12, 0, 0);

            Button crear = CrearBoton("Crear proyecto");
            crear.Click += (s, e) => Confirmar();
            AcceptButton = crear;

            Button cancelar = CrearBoton("Cancelar");
            cancelar.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            CancelButton = cancelar;

            acciones.Controls.Add(crear);
            acciones.Controls.Add(cancelar);
            root.Controls.Add(acciones, 0, 8);
            root.SetColumnSpan(acciones, 2);

            Controls.Add(root);
        }

        private static void ConfigurarCombo(ComboBox combo, string[] items, string seleccionado)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            combo.Items.AddRange(items);
            combo.SelectedItem = seleccionado;
        }

        private static void AgregarFila(TableLayoutPanel root, string etiqueta, Control control, int fila)
        {
            Label label = new Label();
            label.Text = etiqueta + ":";
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            label.Margin = new Padding(0, 7, 10, 7);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 4, 0, 4);

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Controls.Add(label, 0, fila);
            root.Controls.Add(control, 1, fila);
        }

        private static Button CrearBoton(string texto)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 128;
            boton.Height = 32;
            boton.Margin = new Padding(8, 0, 0, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = Color.White;
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            return boton;
        }

        private void Confirmar()
        {
            if (string.IsNullOrWhiteSpace(NombreProyecto))
            {
                MessageBox.Show(this, "Ingresa un nombre para el proyecto.", "Crear proyecto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNombre.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Cliente))
            {
                MessageBox.Show(this, "Ingresa el cliente del proyecto.", "Crear proyecto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtCliente.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
