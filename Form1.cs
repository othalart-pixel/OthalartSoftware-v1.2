using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Program.RegistrarTrazaInicio("Form1: constructor inicio");
            InitializeComponent();
            Program.RegistrarTrazaInicio("Form1: InitializeComponent OK");

            KeyPreview = true;
            KeyDown -= Form1_KeyDown;
            KeyDown += Form1_KeyDown;

            Program.RegistrarTrazaInicio("Form1: InicializarCotizacion inicio");
            InicializarCotizacion();
            Program.RegistrarTrazaInicio("Form1: InicializarCotizacion OK");
            Program.RegistrarTrazaInicio("Form1: ConstruirInterfaz inicio");
            ConstruirInterfaz();
            Program.RegistrarTrazaInicio("Form1: ConstruirInterfaz OK");

            Program.RegistrarTrazaInicio("Form1: ConectarEventosGanttInteractivo inicio");
            ConectarEventosGanttInteractivo();
            Program.RegistrarTrazaInicio("Form1: ConectarEventosGanttInteractivo OK");
            Program.RegistrarTrazaInicio("Form1: RefrescarTodo inicio");
            RefrescarTodo();
            Program.RegistrarTrazaInicio("Form1: RefrescarTodo OK");

            Shown += Form1_Shown;
            Program.RegistrarTrazaInicio("Form1: constructor fin");
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            Program.RegistrarTrazaInicio("Form1_Shown: inicio");
            MostrarVentanaPrincipalAlFrente();

            Program.RegistrarTrazaInicio("Form1_Shown: AjustarAnchoPanelDerecho inicio");
            AjustarAnchoPanelDerecho();
            BeginInvoke(new Action(AjustarAnchoPanelDerecho));
            ProgramarAjusteInicialPanelDerecho();
            Program.RegistrarTrazaInicio("Form1_Shown: ActualizarTiposCambioAlIniciar inicio");

            await ActualizarTiposCambioAlIniciar();
            Program.RegistrarTrazaInicio("Form1_Shown: ActualizarTiposCambioAlIniciar OK");

            AjustarAnchoPanelDerecho();
            BeginInvoke(new Action(AjustarAnchoPanelDerecho));
            ProgramarAjusteInicialPanelDerecho();

            if (modoOscuroActivo)
            {
                AplicarModoOscuroActual();
            }

            MostrarVentanaPrincipalAlFrente();
            Program.RegistrarTrazaInicio("Form1_Shown: fin");
        }

        private void ProgramarAjusteInicialPanelDerecho()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 150;
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                AjustarAnchoPanelDerecho();
            };
            timer.Start();
        }

        private void MostrarVentanaPrincipalAlFrente()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            Show();
            BringToFront();
            Activate();

            TopMost = true;
            BeginInvoke(new Action(() =>
            {
                TopMost = false;
                BringToFront();
                Activate();
            }));
        }
    }
}
