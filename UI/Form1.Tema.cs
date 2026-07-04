using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private readonly System.Windows.Forms.Timer timerOcultarScrollbarsOscuras =
            new System.Windows.Forms.Timer();
        private readonly HashSet<Control> controlesScrollbarsOscuras = new HashSet<Control>();
        private readonly HashSet<Panel> panelesAutoScrollOscuro = new HashSet<Panel>();

        private void AlternarModoOscuro()
        {
            modoOscuroActivo = !modoOscuroActivo;
            AplicarModoOscuroActual();
        }

        private void AplicarModoOscuroActual()
        {
            TemaVisualService.AplicarTema(this, modoOscuroActivo);

            ReaplicarEstilosEspecialesDespuesDeTema();

            RefrescarResumen();

            if (panelSiguientePasoDatos != null)
            {
                RefrescarPanelSiguientePasoDatos();
            }

            RefrescarBotonModoOscuro();
            ConfigurarScrollbarsAutoOcultas();
        }

        private void ReaplicarEstilosEspecialesDespuesDeTema()
        {
            RefrescarBotonModoOscuro();

            try
            {
                MarcarBotonMonedaCliente(monedaClienteSeleccionadaRapida);
            }
            catch
            {
            }

            try
            {
                MarcarBotonPresupuestoRapido(presupuestoClienteModo);
            }
            catch
            {
            }

            try
            {
                MarcarBotonesDestinoUso();
            }
            catch
            {
            }

            try
            {
                MarcarBotonesFormatoEntrega();
            }
            catch
            {
            }

            try
            {
                RefrescarBotonesCategoriaEntregables();
            }
            catch
            {
            }

            AplicarContrasteOscuroEspecial();
        }

        private void AplicarContrasteOscuroEspecial()
        {
            if (!modoOscuroActivo)
            {
                return;
            }

            if (panelGantt != null)
            {
                panelGantt.BackColor = TemaVisualService.PanelOscuro;
                panelGantt.Invalidate();
            }

            if (panelResumenScroll != null)
            {
                panelResumenScroll.BackColor = TemaVisualService.PanelElevadoOscuro;
            }

            if (rtbResumen != null)
            {
                rtbResumen.BackColor = TemaVisualService.PanelElevadoOscuro;
                rtbResumen.ForeColor = TemaVisualService.TextoOscuro;
            }

            AplicarContrasteOscuroRecursivo(this);
            OcultarScrollbarsOscuras();
        }

        private void AplicarContrasteOscuroRecursivo(Control control)
        {
            if (control == null)
            {
                return;
            }

            if (control is TabControl tab)
            {
                tab.BackColor = TemaVisualService.FondoOscuro;
                tab.ForeColor = TemaVisualService.TextoOscuro;
            }

            if (control is Label lbl && lbl.Text == "RESUMEN ACTUAL")
            {
                lbl.ForeColor = TemaVisualService.TextoOscuro;
            }

            foreach (Control hijo in control.Controls)
            {
                AplicarContrasteOscuroRecursivo(hijo);
            }
        }

        private void RefrescarBotonModoOscuro()
        {
            if (btnAlternarModoOscuro == null)
            {
                return;
            }

            btnAlternarModoOscuro.Text = modoOscuroActivo ? "Modo claro" : "Modo oscuro";
            btnAlternarModoOscuro.UseVisualStyleBackColor = false;
            btnAlternarModoOscuro.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAlternarModoOscuro.FlatAppearance.BorderSize = 1;

            if (modoOscuroActivo)
            {
                btnAlternarModoOscuro.BackColor = Color.FromArgb(45, 45, 48);
                btnAlternarModoOscuro.ForeColor = Color.FromArgb(220, 220, 220);
                btnAlternarModoOscuro.FlatAppearance.BorderColor = Color.FromArgb(104, 66, 122);
            }
            else
            {
                btnAlternarModoOscuro.BackColor = Color.FromArgb(30, 30, 30);
                btnAlternarModoOscuro.ForeColor = Color.FromArgb(220, 220, 220);
                btnAlternarModoOscuro.FlatAppearance.BorderColor = Color.FromArgb(63, 63, 70);
            }
        }

        private void ConfigurarScrollbarsAutoOcultas()
        {
            if (!modoOscuroActivo)
            {
                MostrarScrollbarsOscuras();
            }

            timerOcultarScrollbarsOscuras.Stop();
            timerOcultarScrollbarsOscuras.Interval = 1200;
            timerOcultarScrollbarsOscuras.Tick -= TimerOcultarScrollbarsOscuras_Tick;
            timerOcultarScrollbarsOscuras.Tick += TimerOcultarScrollbarsOscuras_Tick;

            controlesScrollbarsOscuras.Clear();

            if (!modoOscuroActivo)
            {
                panelesAutoScrollOscuro.Clear();
            }

            RegistrarControlScrollbarAutoOculta(this);

            if (modoOscuroActivo)
            {
                OcultarScrollbarsOscuras();
            }
            else
            {
                MostrarScrollbarsOscuras();
            }
        }

        private void RegistrarControlScrollbarAutoOculta(Control control)
        {
            if (control == null)
            {
                return;
            }

            bool registrar = false;

            if (control is DataGridView || control is RichTextBox)
            {
                registrar = true;
            }

            Panel panel = control as Panel;

            if (panel != null && (panel.AutoScroll || panelesAutoScrollOscuro.Contains(panel)))
            {
                panelesAutoScrollOscuro.Add(panel);
                registrar = true;
            }

            control.MouseEnter -= ControlScrollbarAutoOculta_Interactuar;
            control.MouseMove -= ControlScrollbarAutoOculta_Interactuar;
            control.MouseWheel -= ControlScrollbarAutoOculta_Interactuar;

            control.MouseEnter += ControlScrollbarAutoOculta_Interactuar;
            control.MouseMove += ControlScrollbarAutoOculta_Interactuar;
            control.MouseWheel += ControlScrollbarAutoOculta_Interactuar;

            if (registrar)
            {
                controlesScrollbarsOscuras.Add(control);

                control.MouseLeave -= ControlScrollbarAutoOculta_MouseLeave;

                control.MouseLeave += ControlScrollbarAutoOculta_MouseLeave;
            }

            foreach (Control hijo in control.Controls)
            {
                RegistrarControlScrollbarAutoOculta(hijo);
            }
        }

        private void ControlScrollbarAutoOculta_Interactuar(object sender, System.EventArgs e)
        {
            if (!modoOscuroActivo)
            {
                return;
            }

            MostrarScrollbarsOscuras();
            timerOcultarScrollbarsOscuras.Stop();
            timerOcultarScrollbarsOscuras.Start();
        }

        private void ControlScrollbarAutoOculta_MouseLeave(object sender, System.EventArgs e)
        {
            if (!modoOscuroActivo)
            {
                return;
            }

            timerOcultarScrollbarsOscuras.Stop();
            timerOcultarScrollbarsOscuras.Start();
        }

        private void TimerOcultarScrollbarsOscuras_Tick(object sender, System.EventArgs e)
        {
            timerOcultarScrollbarsOscuras.Stop();
            OcultarScrollbarsOscuras();
        }

        private void MostrarScrollbarsOscuras()
        {
            foreach (Control control in controlesScrollbarsOscuras)
            {
                if (control is DataGridView dgv)
                {
                    dgv.ScrollBars = ScrollBars.Both;
                }
                else if (control is RichTextBox rtb)
                {
                    rtb.ScrollBars = RichTextBoxScrollBars.Vertical;
                }
                else if (control is Panel panel && panelesAutoScrollOscuro.Contains(panel))
                {
                    panel.AutoScroll = true;
                }
            }
        }

        private void OcultarScrollbarsOscuras()
        {
            if (!modoOscuroActivo)
            {
                return;
            }

            foreach (Control control in controlesScrollbarsOscuras)
            {
                if (control is DataGridView dgv)
                {
                    dgv.ScrollBars = ScrollBars.None;
                }
                else if (control is RichTextBox rtb)
                {
                    rtb.ScrollBars = RichTextBoxScrollBars.None;
                }
                else if (control is Panel panel && panelesAutoScrollOscuro.Contains(panel))
                {
                    panel.AutoScroll = false;
                }
            }
        }
    }
}
