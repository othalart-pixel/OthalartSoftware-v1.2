using Cotizador_animacion_Othalart.Reports;

namespace OthalartSoftware.Tests;

public class InformePdfExporterTests
{
    [Fact]
    public void GenerarPdfDesdeHtml_crea_un_pdf_valido_incluso_con_espacios_en_la_ruta()
    {
        string carpeta = Path.Combine(Path.GetTempPath(), "Othalart PDF Tests",
            Guid.NewGuid().ToString("N"));
        string ruta = Path.Combine(carpeta, "Informe prueba.pdf");
        Directory.CreateDirectory(carpeta);
        try
        {
            InformeClienteExporter.GenerarPdfDesdeHtml(
                "<html><body><h1>Informe Othalart</h1><p>Prueba PDF</p></body></html>",
                ruta);

            Assert.True(File.Exists(ruta));
            Assert.True(new FileInfo(ruta).Length > 100);
            Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(
                File.ReadAllBytes(ruta).Take(4).ToArray()));
        }
        finally
        {
            if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
        }
    }
}
