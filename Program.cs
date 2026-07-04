using System;
using System.IO;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                RegistrarTrazaInicio("Main: inicio");
                ApplicationConfiguration.Initialize();
                RegistrarTrazaInicio("Main: ApplicationConfiguration.Initialize OK");

                Application.ThreadException += (sender, args) =>
                {
                    RegistrarErrorInicio(args.Exception);
                    MessageBox.Show(
                        "La app encontró un error.\n\nSe guardó el detalle en startup-error.log.",
                        "Cotizador Othalart",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                };

                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    RegistrarErrorInicio(args.ExceptionObject as Exception);
                };

                RegistrarTrazaInicio("Main: construyendo Form1");
                using Form1 formulario = new Form1();
                RegistrarTrazaInicio("Main: Form1 construido");
                Application.Run(formulario);
                RegistrarTrazaInicio("Main: Application.Run finalizado");
            }
            catch (Exception ex)
            {
                RegistrarErrorInicio(ex);

                MessageBox.Show(
                    "No se pudo iniciar la app.\n\nSe guardó el detalle en startup-error.log.",
                    "Cotizador Othalart",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private static void RegistrarErrorInicio(Exception ex)
        {
            try
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
                string texto =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    Environment.NewLine +
                    (ex == null ? "Error desconocido" : ex.ToString()) +
                    Environment.NewLine +
                    new string('-', 80) +
                    Environment.NewLine;

                File.AppendAllText(ruta, texto);
            }
            catch
            {
                // No bloquear el arranque si el log no se puede escribir.
            }
        }

        internal static void RegistrarTrazaInicio(string texto)
        {
            try
            {
                string ruta = Path.Combine(AppContext.BaseDirectory, "startup-trace.log");
                File.AppendAllText(
                    ruta,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                    " | " +
                    texto +
                    Environment.NewLine
                );
            }
            catch
            {
                // La traza no debe bloquear el arranque.
            }
        }
    }
}
