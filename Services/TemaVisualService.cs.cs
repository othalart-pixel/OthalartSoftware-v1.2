using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart.Services
{
    public static class TemaVisualService
    {
        public static readonly Color FondoOscuro = Color.FromArgb(30, 30, 30);
        public static readonly Color PanelOscuro = Color.FromArgb(37, 37, 38);
        public static readonly Color PanelElevadoOscuro = Color.FromArgb(45, 45, 48);
        public static readonly Color CampoOscuro = Color.FromArgb(30, 30, 30);
        public static readonly Color BordeOscuro = Color.FromArgb(55, 55, 60);
        public static readonly Color BordeSuaveOscuro = Color.FromArgb(48, 48, 52);
        public static readonly Color VinoOscuro = Color.FromArgb(74, 38, 75);
        public static readonly Color VinoClaroOscuro = Color.FromArgb(104, 66, 122);
        public static readonly Color TextoOscuro = Color.FromArgb(220, 220, 220);
        public static readonly Color TextoSecundarioOscuro = Color.FromArgb(160, 160, 160);

        public static readonly Color FondoClaro = Color.White;
        public static readonly Color PanelClaro = Color.FromArgb(246, 246, 246);
        public static readonly Color CampoClaro = Color.White;
        public static readonly Color BordeClaro = Color.Gainsboro;
        public static readonly Color TextoClaro = Color.Black;
        public static readonly Color TextoSecundarioClaro = Color.DimGray;

        public static void AplicarTema(Control raiz, bool modoOscuro)
        {
            if (raiz == null)
            {
                return;
            }

            AplicarTemaRecursivo(raiz, modoOscuro);
        }

        private static void AplicarTemaRecursivo(Control control, bool modoOscuro)
        {
            if (control == null)
            {
                return;
            }

            AplicarTemaAControl(control, modoOscuro);

            foreach (Control hijo in control.Controls)
            {
                AplicarTemaRecursivo(hijo, modoOscuro);
            }
        }

        private static void AplicarTemaAControl(Control control, bool modoOscuro)
        {
            Color fondo = modoOscuro ? FondoOscuro : FondoClaro;
            Color panel = modoOscuro ? PanelOscuro : FondoClaro;
            Color campo = modoOscuro ? CampoOscuro : CampoClaro;
            Color texto = modoOscuro ? TextoOscuro : TextoClaro;
            Color textoSecundario = modoOscuro ? TextoSecundarioOscuro : TextoSecundarioClaro;
            Color borde = modoOscuro ? BordeOscuro : BordeClaro;

            if (control is Form)
            {
                control.BackColor = fondo;
                control.ForeColor = texto;
                return;
            }

            if (control is TabControl)
            {
                control.BackColor = fondo;
                control.ForeColor = texto;
                return;
            }

            if (control is TabPage)
            {
                control.BackColor = fondo;
                control.ForeColor = texto;
                return;
            }

            if (control is GroupBox)
            {
                control.BackColor = modoOscuro ? PanelOscuro : fondo;
                control.ForeColor = texto;
                return;
            }

            if (control is Panel ||
                control is TableLayoutPanel ||
                control is FlowLayoutPanel)
            {
                control.BackColor = panel;
                control.ForeColor = texto;
                return;
            }

            if (control is Label lbl)
            {
                lbl.BackColor = Color.Transparent;

                if (EsColorDeAdvertencia(lbl.ForeColor))
                {
                    // Mantiene rojos de obligatorio/error.
                    return;
                }

                if (EsColorSecundario(lbl.ForeColor))
                {
                    lbl.ForeColor = textoSecundario;
                }
                else
                {
                    lbl.ForeColor = texto;
                }

                return;
            }

            if (control is TextBox tb)
            {
                tb.BackColor = campo;
                tb.ForeColor = texto;
                tb.BorderStyle = modoOscuro ? BorderStyle.None : BorderStyle.FixedSingle;
                return;
            }

            if (control is RichTextBox rtb)
            {
                rtb.BackColor = campo;
                rtb.ForeColor = texto;
                rtb.BorderStyle = modoOscuro ? BorderStyle.None : BorderStyle.FixedSingle;
                return;
            }

            if (control is ComboBox combo)
            {
                combo.BackColor = campo;
                combo.ForeColor = texto;
                combo.FlatStyle = FlatStyle.Flat;
                return;
            }

            if (control is DateTimePicker dtp)
            {
                dtp.BackColor = campo;
                dtp.ForeColor = texto;
                dtp.CalendarMonthBackground = campo;
                dtp.CalendarForeColor = texto;
                dtp.CalendarTitleBackColor = modoOscuro ? PanelOscuro : PanelClaro;
                dtp.CalendarTitleForeColor = texto;
                return;
            }

            if (control is Button btn)
            {
                AplicarTemaBoton(btn, modoOscuro);
                return;
            }

            if (control is CheckBox chk)
            {
                chk.BackColor = Color.Transparent;
                chk.ForeColor = texto;
                return;
            }

            if (control is DataGridView dgv)
            {
                AplicarTemaDataGridView(dgv, modoOscuro);
                return;
            }

            control.BackColor = fondo;
            control.ForeColor = texto;
        }

        private static void AplicarTemaBoton(Button btn, bool modoOscuro)
        {
            if (btn == null)
            {
                return;
            }

            /*
             * Si el botón tiene Tag y pertenece a un selector especial
             * de moneda/presupuesto/destino/formato, Form1 puede volver
             * a pintarlo después con sus colores propios.
             *
             * Este método solo deja una base sobria.
             */

            btn.UseVisualStyleBackColor = false;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;

            if (modoOscuro)
            {
                btn.BackColor = VinoOscuro;
                btn.ForeColor = TextoOscuro;
                btn.FlatAppearance.BorderColor = VinoClaroOscuro;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
                btn.FlatAppearance.BorderColor = Color.Silver;
            }
        }

        public static void AplicarTemaDataGridView(DataGridView dgv, bool modoOscuro)
        {
            if (dgv == null)
            {
                return;
            }

            dgv.EnableHeadersVisualStyles = false;
            dgv.BorderStyle = modoOscuro ? BorderStyle.None : BorderStyle.FixedSingle;

            if (modoOscuro)
            {
                dgv.BackgroundColor = PanelOscuro;
                dgv.GridColor = BordeOscuro;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = VinoOscuro;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextoOscuro;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = VinoOscuro;
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextoOscuro;

                dgv.RowHeadersDefaultCellStyle.BackColor = PanelElevadoOscuro;
                dgv.RowHeadersDefaultCellStyle.ForeColor = TextoOscuro;

                dgv.DefaultCellStyle.BackColor = PanelElevadoOscuro;
                dgv.DefaultCellStyle.ForeColor = TextoOscuro;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 153, 255);
                dgv.DefaultCellStyle.SelectionForeColor = TextoOscuro;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 42);
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextoOscuro;
            }
            else
            {
                dgv.BackgroundColor = Color.White;
                dgv.GridColor = Color.Gainsboro;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 235, 235);
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

                dgv.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
                dgv.RowHeadersDefaultCellStyle.ForeColor = Color.Black;

                dgv.DefaultCellStyle.BackColor = Color.White;
                dgv.DefaultCellStyle.ForeColor = Color.Black;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black;
            }
        }

        public static Color FondoBotonInactivo(bool modoOscuro)
        {
            return modoOscuro ? PanelOscuro : Color.White;
        }

        public static Color TextoBotonInactivo(bool modoOscuro)
        {
            return modoOscuro ? TextoOscuro : Color.FromArgb(45, 45, 45);
        }

        public static Color BordeBotonInactivo(bool modoOscuro)
        {
            return modoOscuro ? BordeOscuro : Color.Silver;
        }

        private static bool EsColorDeAdvertencia(Color color)
        {
            return color == Color.Firebrick ||
                   color == Color.Red ||
                   color == Color.DarkRed ||
                   color == Color.OrangeRed ||
                   color == Color.DarkOrange;
        }

        private static bool EsColorSecundario(Color color)
        {
            return color == Color.DimGray ||
                   color == Color.Gray ||
                   color == Color.DarkGray ||
                   color == Color.FromArgb(120, 120, 120);
        }
    }
}
