using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Reports
{
    public static class InformeClienteExporter
    {
        public static bool GuardarPdfProyecto(
            ProyectoCotizacion proyecto,
            IEnumerable<PersonaEquipo> personal,
            IEnumerable<CategoriaTrabajador> cargos)
        {
            if (proyecto == null)
            {
                MessageBox.Show("No hay un proyecto cargado para exportar.", "Exportar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            using SaveFileDialog dialogo = new SaveFileDialog
            {
                Title = "Exportar informe del proyecto a PDF",
                Filter = "Documento PDF (*.pdf)|*.pdf",
                FileName = ConstruirNombreArchivoPdf(proyecto),
                DefaultExt = "pdf",
                AddExtension = true,
                OverwritePrompt = true
            };
            if (dialogo.ShowDialog() != DialogResult.OK) return false;

            try
            {
                GenerarPdfDesdeHtml(
                    InformeProyectoBuilder.GenerarHtml(proyecto, personal, cargos),
                    dialogo.FileName);

                MessageBox.Show("Informe PDF guardado correctamente.", "Exportar PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo generar el PDF.\n\n" + ex.Message,
                    "Error al exportar PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static void GenerarPdfDesdeHtml(string html, string rutaPdf)
        {
            string edge = BuscarMicrosoftEdge();
            if (string.IsNullOrWhiteSpace(edge))
                throw new FileNotFoundException("No se encontró Microsoft Edge para convertir el informe a PDF.");
            if (string.IsNullOrWhiteSpace(rutaPdf))
                throw new ArgumentException("La ruta del PDF está vacía.", nameof(rutaPdf));

            string temporal = Path.Combine(Path.GetTempPath(),
                "OthalartPdf_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporal);
            string htmlTemporal = Path.Combine(temporal, "informe.html");
            string perfilTemporal = Path.Combine(temporal, "edge-profile");
            try
            {
                File.WriteAllText(htmlTemporal, html ?? "", new UTF8Encoding(true));
                ProcessStartInfo inicio = new ProcessStartInfo
                {
                    FileName = edge,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                inicio.ArgumentList.Add("--headless");
                inicio.ArgumentList.Add("--disable-gpu");
                inicio.ArgumentList.Add("--no-pdf-header-footer");
                inicio.ArgumentList.Add("--user-data-dir=" + perfilTemporal);
                inicio.ArgumentList.Add("--print-to-pdf=" + Path.GetFullPath(rutaPdf));
                inicio.ArgumentList.Add(new Uri(htmlTemporal).AbsoluteUri);

                using Process proceso = Process.Start(inicio);
                if (proceso == null || !proceso.WaitForExit(60000))
                {
                    try { proceso?.Kill(true); } catch { }
                    throw new TimeoutException("La generación del PDF superó el tiempo de espera.");
                }
                if (proceso.ExitCode != 0 || !File.Exists(rutaPdf) || new FileInfo(rutaPdf).Length == 0)
                    throw new InvalidOperationException("No se generó un archivo PDF válido (código " + proceso.ExitCode + ").");
            }
            finally
            {
                try { Directory.Delete(temporal, true); } catch { }
            }
        }

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

        private static string ConstruirNombreArchivoPdf(ProyectoCotizacion proyecto)
        {
            string nombre = LimpiarNombreArchivo(proyecto?.Nombre);
            string cliente = LimpiarNombreArchivo(proyecto?.Cliente);
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "proyecto";
            string sufijo = string.IsNullOrWhiteSpace(cliente) ? "" : "_" + cliente;
            return "Informe_Othalart_" + nombre + sufijo + "_" +
                DateTime.Now.ToString("yyyyMMdd") + ".pdf";
        }

        private static string BuscarMicrosoftEdge()
        {
            string[] candidatos =
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Edge", "Application", "msedge.exe")
            };
            return candidatos.FirstOrDefault(File.Exists) ?? "";
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
