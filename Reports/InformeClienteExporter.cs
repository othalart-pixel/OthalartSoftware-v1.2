using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Reports
{
    public static class InformeClienteExporter
    {
        public static bool GuardarHtmlCliente(Cotizacion cotizacion)
        {
            if (cotizacion == null)
            {
                MessageBox.Show(
                    "No hay cotización cargada para exportar.",
                    "Guardar informe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return false;
            }

            string nombreSugerido = ConstruirNombreArchivo(cotizacion);

            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar informe cliente";
            dialogo.Filter = "Informe HTML (*.html)|*.html|Página web (*.htm)|*.htm";
            dialogo.FileName = nombreSugerido;
            dialogo.DefaultExt = "html";
            dialogo.AddExtension = true;
            dialogo.OverwritePrompt = true;

            DialogResult resultado = dialogo.ShowDialog();

            if (resultado != DialogResult.OK)
            {
                return false;
            }

            try
            {
                string html = InformeClienteBuilder.GenerarHtml(cotizacion);

                UTF8Encoding utf8ConBom = new UTF8Encoding(true);
                File.WriteAllText(dialogo.FileName, html, utf8ConBom);

                MessageBox.Show(
                    "Informe cliente guardado correctamente.",
                    "Guardar informe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo guardar el informe cliente.\n\n" + ex.Message,
                    "Error al guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

        private static string ConstruirNombreArchivo(Cotizacion cotizacion)
        {
            string proyecto = LimpiarNombreArchivo(cotizacion.NombreProyecto);
            string cliente = LimpiarNombreArchivo(cotizacion.NombreCliente);
            string fecha = DateTime.Now.ToString("yyyyMMdd");

            if (string.IsNullOrWhiteSpace(proyecto))
            {
                proyecto = "cotizacion";
            }

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                return "Informe_Othalart_" + proyecto + "_" + cliente + "_" + fecha + ".html";
            }

            return "Informe_Othalart_" + proyecto + "_" + fecha + ".html";
        }

        private static string LimpiarNombreArchivo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            string limpio = texto.Trim();

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                limpio = limpio.Replace(c.ToString(), "");
            }

            limpio = limpio.Replace(" ", "_");
            limpio = limpio.Replace("__", "_");

            return limpio;
        }
    }
}