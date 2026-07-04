using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConfigurarFeedbackResumen()
        {
            if (feedbackResumenConfigurado)
            {
                return;
            }

            feedbackResumenConfigurado = true;

            timerResumenFeedback.Interval = 90;
            timerResumenFeedback.Tick += TimerResumenFeedback_Tick;
        }

        private void MarcarResumenActualizado()
        {
            if (panelResumenScroll == null || rtbResumen == null)
            {
                return;
            }

            rtbResumen.Dock = DockStyle.Fill;
            rtbResumen.ScrollBars = RichTextBoxScrollBars.Vertical;

            pasosFeedbackResumen = 0;

            if (!timerResumenFeedback.Enabled)
            {
                timerResumenFeedback.Start();
            }
        }

        private void TimerResumenFeedback_Tick(object sender, EventArgs e)
        {
            pasosFeedbackResumen++;

            if (pasosFeedbackResumen == 1)
            {
                if (modoOscuroActivo)
                {
                    panelResumenScroll.BackColor = Color.FromArgb(45, 45, 48);
                    rtbResumen.BackColor = Color.FromArgb(45, 45, 48);
                }
                else
                {
                    panelResumenScroll.BackColor = Color.FromArgb(240, 250, 247);
                    rtbResumen.BackColor = Color.FromArgb(240, 250, 247);
                }
                return;
            }

            if (pasosFeedbackResumen == 2)
            {
                if (modoOscuroActivo)
                {
                    panelResumenScroll.BackColor = Color.FromArgb(42, 42, 44);
                    rtbResumen.BackColor = Color.FromArgb(42, 42, 44);
                }
                else
                {
                    panelResumenScroll.BackColor = Color.FromArgb(247, 252, 250);
                    rtbResumen.BackColor = Color.FromArgb(247, 252, 250);
                }
                return;
            }

            if (modoOscuroActivo)
            {
                panelResumenScroll.BackColor = Color.FromArgb(37, 37, 38);
                rtbResumen.BackColor = Color.FromArgb(37, 37, 38);
            }
            else
            {
                panelResumenScroll.BackColor = Color.White;
                rtbResumen.BackColor = Color.White;
            }

            timerResumenFeedback.Stop();
        }
    }
}
