using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public class DialogoAlcanceModificacion : Form
    {
        public AlcanceModificacion AlcanceSeleccionado { get; private set; } = AlcanceModificacion.Cancelado;

        public DialogoAlcanceModificacion(IEnumerable<AlcanceModificacion> opciones)
        {
            Text = "Alcance del cambio";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.White;
            Padding = new Padding(18);
            KeyPreview = true;

            TableLayoutPanel root = new TableLayoutPanel();
            root.AutoSize = true;
            root.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 2;

            Label pregunta = new Label();
            pregunta.Text = "¿Dónde quieres aplicar estos cambios?";
            pregunta.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            pregunta.AutoSize = true;
            pregunta.Margin = new Padding(0, 0, 0, 12);
            root.Controls.Add(pregunta, 0, 0);

            FlowLayoutPanel botones = new FlowLayoutPanel();
            botones.FlowDirection = FlowDirection.TopDown;
            botones.WrapContents = false;
            botones.AutoSize = true;
            botones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            botones.Margin = new Padding(0);

            foreach (AlcanceModificacion opcion in opciones)
            {
                botones.Controls.Add(CrearBotonOpcion(opcion));
            }

            if (!ContieneCancelado(opciones))
            {
                botones.Controls.Add(CrearBotonOpcion(AlcanceModificacion.Cancelado));
            }

            root.Controls.Add(botones, 0, 1);
            Controls.Add(root);

            CancelButton = null;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    AlcanceSeleccionado = AlcanceModificacion.Cancelado;
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
            FormClosing += (s, e) =>
            {
                if (DialogResult != DialogResult.OK)
                {
                    AlcanceSeleccionado = AlcanceModificacion.Cancelado;
                }
            };
        }

        public static AlcanceModificacion Preguntar(
            IWin32Window owner,
            IEnumerable<AlcanceModificacion> opciones
        )
        {
            using (DialogoAlcanceModificacion dialogo = new DialogoAlcanceModificacion(opciones))
            {
                dialogo.ShowDialog(owner);
                return dialogo.AlcanceSeleccionado;
            }
        }

        private Control CrearBotonOpcion(AlcanceModificacion alcance)
        {
            Button boton = new Button();
            boton.Width = 430;
            boton.Height = 58;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = alcance == AlcanceModificacion.Instancia ||
                alcance == AlcanceModificacion.ProductoProyecto
                    ? Color.FromArgb(232, 244, 255)
                    : Color.White;
            boton.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            boton.Margin = new Padding(0, 0, 0, 8);
            boton.Text = ObtenerTitulo(alcance) + Environment.NewLine + ObtenerDescripcion(alcance);
            boton.Click += (s, e) =>
            {
                if (alcance == AlcanceModificacion.PlantillaGlobal &&
                    MessageBox.Show(
                        this,
                        "Esto puede afectar proyectos futuros. ¿Confirmas actualizar la plantilla global?",
                        "Confirmar cambio global",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning
                    ) != DialogResult.OK)
                {
                    return;
                }

                AlcanceSeleccionado = alcance;
                DialogResult = alcance == AlcanceModificacion.Cancelado
                    ? DialogResult.Cancel
                    : DialogResult.OK;
                Close();
            };
            return boton;
        }

        private static bool ContieneCancelado(IEnumerable<AlcanceModificacion> opciones)
        {
            foreach (AlcanceModificacion opcion in opciones)
            {
                if (opcion == AlcanceModificacion.Cancelado)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ObtenerTitulo(AlcanceModificacion alcance)
        {
            switch (alcance)
            {
                case AlcanceModificacion.Instancia:
                    return "Solo esta instancia";
                case AlcanceModificacion.SubproductoProyecto:
                    return "Este subproducto del proyecto";
                case AlcanceModificacion.ProductoProyecto:
                    return "Este producto del proyecto";
                case AlcanceModificacion.PlantillaGlobal:
                    return "Plantilla global";
                case AlcanceModificacion.NuevaPlantilla:
                    return "Guardar como nueva plantilla";
                default:
                    return "Cancelar";
            }
        }

        private static string ObtenerDescripcion(AlcanceModificacion alcance)
        {
            switch (alcance)
            {
                case AlcanceModificacion.Instancia:
                    return "Modifica solo esta aparicion concreta.";
                case AlcanceModificacion.SubproductoProyecto:
                    return "Modifica el subproducto dentro de esta cotizacion.";
                case AlcanceModificacion.ProductoProyecto:
                    return "Modifica solamente esta cotizacion. La biblioteca global no cambia.";
                case AlcanceModificacion.PlantillaGlobal:
                    return "Actualiza la configuracion predeterminada para proyectos futuros.";
                case AlcanceModificacion.NuevaPlantilla:
                    return "Crea una variante reutilizable sin modificar la plantilla original.";
                default:
                    return "Volver al editor sin aplicar cambios.";
            }
        }
    }
}
