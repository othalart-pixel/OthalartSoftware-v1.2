using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Reports
{
    public static class ExcelInternoExporter
    {
        public static bool GuardarExcelInterno(Cotizacion cotizacion)
        {
            if (cotizacion == null)
            {
                MessageBox.Show(
                    "No hay cotización cargada para exportar.",
                    "Guardar Excel interno",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return false;
            }

            string nombreSugerido = ConstruirNombreArchivo(cotizacion);

            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar Excel interno";
            dialogo.Filter = "Excel XML (*.xml)|*.xml";
            dialogo.FileName = nombreSugerido;
            dialogo.DefaultExt = "xml";
            dialogo.AddExtension = true;
            dialogo.OverwritePrompt = true;

            DialogResult resultado = dialogo.ShowDialog();

            if (resultado != DialogResult.OK)
            {
                return false;
            }

            try
            {
                string xml = GenerarExcelXml(cotizacion);

                UTF8Encoding utf8ConBom = new UTF8Encoding(true);
                File.WriteAllText(dialogo.FileName, xml, utf8ConBom);

                MessageBox.Show(
                    "Excel interno guardado correctamente.",
                    "Guardar Excel interno",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo guardar el Excel interno.\n\n" + ex.Message,
                    "Error al guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
        }

        private static string GenerarExcelXml(Cotizacion cotizacion)
        {
            StringBuilder xml = new StringBuilder();

            EscribirInicioLibro(xml);
            EscribirHojaResumen(xml, cotizacion);
            EscribirHojaEtapas(xml, cotizacion);
            EscribirHojaManoObra(xml, cotizacion);
            EscribirHojaCostosExtra(xml, cotizacion);
            EscribirHojaCostos(xml, cotizacion);
            EscribirHojaRentabilidad(xml, cotizacion);
            EscribirHojaTiposCambio(xml, cotizacion);
            EscribirFinLibro(xml);

            return xml.ToString();
        }

        private static void EscribirInicioLibro(StringBuilder xml)
        {
            xml.AppendLine("<?xml version=\"1.0\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            xml.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            xml.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");

            xml.AppendLine("<Styles>");

            xml.AppendLine("<Style ss:ID=\"Titulo\">");
            xml.AppendLine("<Font ss:Bold=\"1\" ss:Size=\"16\" ss:Color=\"#FFFFFF\"/>");
            xml.AppendLine("<Interior ss:Color=\"#EE1E5B\" ss:Pattern=\"Solid\"/>");
            xml.AppendLine("<Alignment ss:Horizontal=\"Center\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Subtitulo\">");
            xml.AppendLine("<Font ss:Bold=\"1\" ss:Color=\"#000000\"/>");
            xml.AppendLine("<Interior ss:Color=\"#FBF283\" ss:Pattern=\"Solid\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Header\">");
            xml.AppendLine("<Font ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/>");
            xml.AppendLine("<Interior ss:Color=\"#53C0A6\" ss:Pattern=\"Solid\"/>");
            xml.AppendLine("<Borders>");
            xml.AppendLine("<Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            xml.AppendLine("<Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            xml.AppendLine("<Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            xml.AppendLine("<Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/>");
            xml.AppendLine("</Borders>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Label\">");
            xml.AppendLine("<Font ss:Bold=\"1\"/>");
            xml.AppendLine("<Interior ss:Color=\"#EEEEEE\" ss:Pattern=\"Solid\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Number\">");
            xml.AppendLine("<NumberFormat ss:Format=\"#,##0.00\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Integer\">");
            xml.AppendLine("<NumberFormat ss:Format=\"#,##0\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Percent\">");
            xml.AppendLine("<NumberFormat ss:Format=\"0.00\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("<Style ss:ID=\"Total\">");
            xml.AppendLine("<Font ss:Bold=\"1\"/>");
            xml.AppendLine("<Interior ss:Color=\"#FBF283\" ss:Pattern=\"Solid\"/>");
            xml.AppendLine("<NumberFormat ss:Format=\"#,##0.00\"/>");
            xml.AppendLine("</Style>");

            xml.AppendLine("</Styles>");
        }

        private static void EscribirFinLibro(StringBuilder xml)
        {
            xml.AppendLine("</Workbook>");
        }

        private static void AbrirHoja(StringBuilder xml, string nombreHoja)
        {
            xml.AppendLine("<Worksheet ss:Name=\"" + TextoSeguroXml(nombreHoja) + "\">");
            xml.AppendLine("<Table>");
        }

        private static void CerrarHoja(StringBuilder xml)
        {
            xml.AppendLine("</Table>");
            xml.AppendLine("</Worksheet>");
        }

        private static void EscribirColumna(StringBuilder xml, double ancho)
        {
            xml.AppendLine("<Column ss:Width=\"" + ancho.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\"/>");
        }

        private static void EscribirCeldaTexto(StringBuilder xml, string texto, string estilo = "")
        {
            xml.Append("<Cell");

            if (!string.IsNullOrWhiteSpace(estilo))
            {
                xml.Append(" ss:StyleID=\"" + estilo + "\"");
            }

            xml.Append("><Data ss:Type=\"String\">");
            xml.Append(TextoSeguroXml(texto));
            xml.AppendLine("</Data></Cell>");
        }

        private static void EscribirCeldaNumero(StringBuilder xml, double valor, string estilo = "Number")
        {
            xml.Append("<Cell");

            if (!string.IsNullOrWhiteSpace(estilo))
            {
                xml.Append(" ss:StyleID=\"" + estilo + "\"");
            }

            xml.Append("><Data ss:Type=\"Number\">");
            xml.Append(NumeroXml(valor));
            xml.AppendLine("</Data></Cell>");
        }

        private static void EscribirFilaTitulo(StringBuilder xml, string titulo, int columnas)
        {
            xml.AppendLine("<Row>");
            xml.AppendLine("<Cell ss:MergeAcross=\"" + (columnas - 1) + "\" ss:StyleID=\"Titulo\">");
            xml.AppendLine("<Data ss:Type=\"String\">" + TextoSeguroXml(titulo) + "</Data>");
            xml.AppendLine("</Cell>");
            xml.AppendLine("</Row>");
        }

        private static void EscribirFilaSubtitulo(StringBuilder xml, string titulo, int columnas)
        {
            xml.AppendLine("<Row>");
            xml.AppendLine("<Cell ss:MergeAcross=\"" + (columnas - 1) + "\" ss:StyleID=\"Subtitulo\">");
            xml.AppendLine("<Data ss:Type=\"String\">" + TextoSeguroXml(titulo) + "</Data>");
            xml.AppendLine("</Cell>");
            xml.AppendLine("</Row>");
        }

        private static void EscribirFilaDatoTexto(StringBuilder xml, string campo, string valor)
        {
            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, campo, "Label");
            EscribirCeldaTexto(xml, valor);
            xml.AppendLine("</Row>");
        }

        private static void EscribirFilaDatoNumero(StringBuilder xml, string campo, double valor)
        {
            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, campo, "Label");
            EscribirCeldaNumero(xml, valor);
            xml.AppendLine("</Row>");
        }

        private static void EscribirHojaResumen(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Resumen");

            EscribirColumna(xml, 230);
            EscribirColumna(xml, 220);

            EscribirFilaTitulo(xml, "COTIZACION OTHALART", 2);
            EscribirFilaSubtitulo(xml, "Resumen general del proyecto", 2);

            EscribirFilaDatoTexto(xml, "Cliente", ValorTexto(cotizacion.NombreCliente));
            EscribirFilaDatoTexto(xml, "Empresa / marca", ValorTexto(cotizacion.Empresa));
            EscribirFilaDatoTexto(xml, "Email", ValorTexto(cotizacion.Email));
            EscribirFilaDatoTexto(xml, "Proyecto", ValorTexto(cotizacion.NombreProyecto));
            EscribirFilaDatoTexto(xml, "Descripcion", ValorTexto(cotizacion.Descripcion));
            EscribirFilaDatoTexto(xml, "Moneda cliente", ValorTexto(cotizacion.Moneda));
            EscribirFilaDatoTexto(xml, "Moneda visualizacion", ValorTexto(cotizacion.MonedaVisualizacion));
            EscribirFilaDatoNumero(xml, "Duracion total meses", DuracionProyecto(cotizacion));

            xml.AppendLine("<Row></Row>");

            EscribirFilaSubtitulo(xml, "Resumen economico", 2);
            EscribirFilaDatoNumero(xml, "Costo total", cotizacion.CostoTotal);
            EscribirFilaDatoNumero(xml, "Precio recomendado", cotizacion.PrecioRecomendado);
            EscribirFilaDatoNumero(xml, "Precio acordado / evaluado", cotizacion.PrecioVentaEvaluado);
            EscribirFilaDatoNumero(xml, "Utilidad estimada", cotizacion.UtilidadEvaluada);
            EscribirFilaDatoNumero(xml, "Margen evaluado (%)", cotizacion.MargenEvaluado * 100.0);
            EscribirFilaDatoNumero(xml, "Markup sobre costo (%)", cotizacion.MarkupEvaluado * 100.0);

            CerrarHoja(xml);
        }

        private static void EscribirHojaEtapas(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Etapas");

            EscribirColumna(xml, 160);
            EscribirColumna(xml, 90);
            EscribirColumna(xml, 120);
            EscribirColumna(xml, 100);
            EscribirColumna(xml, 100);
            EscribirColumna(xml, 120);
            EscribirColumna(xml, 150);
            EscribirColumna(xml, 90);

            EscribirFilaTitulo(xml, "ETAPAS DEL PROYECTO", 8);

            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, "Etapa", "Header");
            EscribirCeldaTexto(xml, "Incluida", "Header");
            EscribirCeldaTexto(xml, "Duracion meses", "Header");
            EscribirCeldaTexto(xml, "Inicio mes", "Header");
            EscribirCeldaTexto(xml, "Fin mes", "Header");
            EscribirCeldaTexto(xml, "Persona-mes", "Header");
            EscribirCeldaTexto(xml, "Costo total", "Header");
            EscribirCeldaTexto(xml, "% costo MO", "Header");
            xml.AppendLine("</Row>");

            double totalMO = cotizacion.Etapas
                .Where(e => e.Seleccionada)
                .SelectMany(e => e.Plan)
                .Sum(c => c.CostoTotal);

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                double personaMes = etapa.Plan.Sum(c => c.PersonaMesTotal);
                double costo = etapa.Plan.Sum(c => c.CostoTotal);
                double porcentaje = totalMO > 0.0 ? costo / totalMO * 100.0 : 0.0;

                xml.AppendLine("<Row>");
                EscribirCeldaTexto(xml, etapa.Nombre);
                EscribirCeldaTexto(xml, etapa.Seleccionada ? "Si" : "No");
                EscribirCeldaNumero(xml, etapa.DuracionMeses);
                EscribirCeldaNumero(xml, etapa.InicioMes);
                EscribirCeldaNumero(xml, etapa.FinMes);
                EscribirCeldaNumero(xml, personaMes);
                EscribirCeldaNumero(xml, costo);
                EscribirCeldaNumero(xml, porcentaje);
                xml.AppendLine("</Row>");
            }

            CerrarHoja(xml);
        }

        private static void EscribirHojaManoObra(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Mano de obra");

            EscribirColumna(xml, 150);
            EscribirColumna(xml, 260);
            EscribirColumna(xml, 150);
            EscribirColumna(xml, 150);
            EscribirColumna(xml, 130);
            EscribirColumna(xml, 120);
            EscribirColumna(xml, 150);

            EscribirFilaTitulo(xml, "DETALLE DE MANO DE OBRA", 7);

            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, "Etapa", "Header");
            EscribirCeldaTexto(xml, "Cargo", "Header");
            EscribirCeldaTexto(xml, "Persona", "Header");
            EscribirCeldaTexto(xml, "Valor mensual CLP", "Header");
            EscribirCeldaTexto(xml, "Persona-mes", "Header");
            EscribirCeldaTexto(xml, "Costo CLP", "Header");
            EscribirCeldaTexto(xml, "Costo visual", "Header");
            xml.AppendLine("</Row>");

            foreach (EtapaProyecto etapa in cotizacion.Etapas.Where(e => e.Seleccionada))
            {
                foreach (CargoPlanMensual cargo in etapa.Plan)
                {
                    string nombreCargo = "Cargo por definir";

                    if (cargo.Categoria != null)
                    {
                        nombreCargo = cargo.Categoria.NombreCompleto;
                    }

                    xml.AppendLine("<Row>");
                    EscribirCeldaTexto(xml, etapa.Nombre);
                    EscribirCeldaTexto(xml, nombreCargo);
                    EscribirCeldaTexto(xml, ValorTexto(cargo.NombrePersona));
                    EscribirCeldaNumero(xml, cargo.SueldoMensualCLPEditable);
                    EscribirCeldaNumero(xml, cargo.PersonaMesTotal);
                    EscribirCeldaNumero(xml, cargo.CostoTotal);
                    EscribirCeldaTexto(xml, FormatearCLPYVisual(cotizacion, cargo.CostoTotal));
                    xml.AppendLine("</Row>");
                }
            }

            CerrarHoja(xml);
        }

        private static void EscribirHojaCostosExtra(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Costos extra");

            EscribirColumna(xml, 170);
            EscribirColumna(xml, 260);
            EscribirColumna(xml, 120);
            EscribirColumna(xml, 140);
            EscribirColumna(xml, 140);
            EscribirColumna(xml, 120);
            EscribirColumna(xml, 150);

            EscribirFilaTitulo(xml, "COSTOS EXTRA DEL PROYECTO", 7);

            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, "Categoria", "Header");
            EscribirCeldaTexto(xml, "Descripcion", "Header");
            EscribirCeldaTexto(xml, "Moneda ingreso", "Header");
            EscribirCeldaTexto(xml, "Monto ingreso", "Header");
            EscribirCeldaTexto(xml, "Monto CLP", "Header");
            EscribirCeldaTexto(xml, "Frecuencia", "Header");
            EscribirCeldaTexto(xml, "Total calculado CLP", "Header");
            xml.AppendLine("</Row>");

            if (cotizacion.CostosExtra != null)
            {
                foreach (CostoExtra costo in cotizacion.CostosExtra)
                {
                    xml.AppendLine("<Row>");
                    EscribirCeldaTexto(xml, ValorTexto(costo.Categoria));
                    EscribirCeldaTexto(xml, ValorTexto(costo.Descripcion));
                    EscribirCeldaTexto(xml, ValorTexto(costo.MonedaIngreso));
                    EscribirCeldaNumero(xml, costo.MontoIngreso);
                    EscribirCeldaNumero(xml, costo.Monto);
                    EscribirCeldaTexto(xml, ValorTexto(costo.Periodicidad));
                    EscribirCeldaNumero(xml, costo.MontoCalculado);
                    xml.AppendLine("</Row>");
                }
            }

            CerrarHoja(xml);
        }

        private static void EscribirHojaCostos(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Costos");

            EscribirColumna(xml, 280);
            EscribirColumna(xml, 180);

            EscribirFilaTitulo(xml, "RESUMEN DE COSTOS", 2);

            EscribirFilaDatoNumero(xml, "Mano de obra total etapas", cotizacion.CostoManoObraEtapas);
            EscribirFilaDatoNumero(xml, "Persona-mes total", cotizacion.PersonaMesTotal);
            EscribirFilaDatoNumero(xml, "Produccion interna automatica", cotizacion.CostoProduccionInterna);
            EscribirFilaDatoNumero(xml, "Administracion automatica", cotizacion.CostoAdministrativo);
            EscribirFilaDatoNumero(xml, "Servicios tercerizados", cotizacion.CostoTercerizados);
            EscribirFilaDatoNumero(xml, "Otros costos", cotizacion.OtrosCostos);
            EscribirFilaDatoNumero(xml, "Costo base", cotizacion.CostoBase);
            EscribirFilaDatoNumero(xml, "Colchon seguridad (%)", cotizacion.TasaImprevistos * 100.0);
            EscribirFilaDatoNumero(xml, "Monto colchon seguridad", cotizacion.Imprevistos);

            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, "Costo total", "Label");
            EscribirCeldaNumero(xml, cotizacion.CostoTotal, "Total");
            xml.AppendLine("</Row>");

            CerrarHoja(xml);
        }

        private static void EscribirHojaRentabilidad(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Rentabilidad");

            EscribirColumna(xml, 280);
            EscribirColumna(xml, 180);

            EscribirFilaTitulo(xml, "PRECIO Y RENTABILIDAD", 2);

            EscribirFilaDatoNumero(xml, "Margen objetivo (%)", cotizacion.MargenObjetivo * 100.0);
            EscribirFilaDatoNumero(xml, "Precio recomendado", cotizacion.PrecioRecomendado);
            EscribirFilaDatoNumero(xml, "Precio acordado / evaluado", cotizacion.PrecioVentaEvaluado);
            EscribirFilaDatoNumero(xml, "Utilidad estimada", cotizacion.UtilidadEvaluada);
            EscribirFilaDatoNumero(xml, "Margen evaluado (%)", cotizacion.MargenEvaluado * 100.0);
            EscribirFilaDatoNumero(xml, "Markup sobre costo (%)", cotizacion.MarkupEvaluado * 100.0);

            CerrarHoja(xml);
        }

        private static void EscribirHojaTiposCambio(StringBuilder xml, Cotizacion cotizacion)
        {
            AbrirHoja(xml, "Tipos cambio");

            EscribirColumna(xml, 90);
            EscribirColumna(xml, 190);
            EscribirColumna(xml, 140);
            EscribirColumna(xml, 150);
            EscribirColumna(xml, 160);

            EscribirFilaTitulo(xml, "TIPOS DE CAMBIO", 5);

            xml.AppendLine("<Row>");
            EscribirCeldaTexto(xml, "Codigo", "Header");
            EscribirCeldaTexto(xml, "Nombre", "Header");
            EscribirCeldaTexto(xml, "Valor en CLP", "Header");
            EscribirCeldaTexto(xml, "Fuente", "Header");
            EscribirCeldaTexto(xml, "Fecha actualizacion", "Header");
            xml.AppendLine("</Row>");

            if (cotizacion.TiposCambio != null)
            {
                foreach (TipoCambio tipo in cotizacion.TiposCambio)
                {
                    xml.AppendLine("<Row>");
                    EscribirCeldaTexto(xml, tipo.Codigo);
                    EscribirCeldaTexto(xml, tipo.Nombre);
                    EscribirCeldaNumero(xml, tipo.ValorEnCLP);
                    EscribirCeldaTexto(xml, tipo.Fuente);
                    EscribirCeldaTexto(xml, tipo.FechaActualizacion.ToString("dd-MM-yyyy HH:mm"));
                    xml.AppendLine("</Row>");
                }
            }

            CerrarHoja(xml);
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
                return "Excel_Othalart_" + proyecto + "_" + cliente + "_" + fecha + ".xml";
            }

            return "Excel_Othalart_" + proyecto + "_" + fecha + ".xml";
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

        private static string TextoSeguroXml(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "No informado";
            }

            string limpio = texto;

            limpio = limpio.Replace("&", "&amp;");
            limpio = limpio.Replace("<", "&lt;");
            limpio = limpio.Replace(">", "&gt;");
            limpio = limpio.Replace("\"", "&quot;");
            limpio = limpio.Replace("'", "&apos;");

            return limpio;
        }

        private static string NumeroXml(double valor)
        {
            return valor.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ValorTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "No informado";
            }

            return texto;
        }

        private static double DuracionProyecto(Cotizacion cotizacion)
        {
            if (cotizacion.Etapas == null)
            {
                return 0.0;
            }

            var etapas = cotizacion.Etapas.Where(e => e.Seleccionada).ToList();

            if (etapas.Count == 0)
            {
                return 0.0;
            }

            return etapas.Max(e => e.FinMes);
        }

        private static TipoCambio ObtenerTipoCambio(Cotizacion cotizacion, string codigo)
        {
            if (cotizacion.TiposCambio == null)
            {
                return null;
            }

            return cotizacion.TiposCambio.FirstOrDefault(t => t.Codigo == codigo);
        }

        private static double ConvertirDesdeCLP(Cotizacion cotizacion, double valorCLP, string monedaDestino)
        {
            TipoCambio tipo = ObtenerTipoCambio(cotizacion, monedaDestino);

            if (tipo == null || tipo.ValorEnCLP <= 0.0)
            {
                return valorCLP;
            }

            return valorCLP / tipo.ValorEnCLP;
        }

        private static string FormatearMiles(double valor)
        {
            return Math.Round(valor, 0).ToString("N0");
        }

        private static string FormatearValorVisual(Cotizacion cotizacion, double valorCLP)
        {
            string moneda = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion;

            double valor = ConvertirDesdeCLP(cotizacion, valorCLP, moneda);

            if (moneda == "CLP")
            {
                return "$ " + FormatearMiles(valor) + " CLP";
            }

            if (moneda == "JPY")
            {
                return "JPY " + valor.ToString("N0");
            }

            if (moneda == "UF")
            {
                return valor.ToString("0.00") + " UF";
            }

            return moneda + " " + valor.ToString("N2");
        }

        private static string FormatearCLPYVisual(Cotizacion cotizacion, double valorCLP)
        {
            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion;

            string clp = "$ " + FormatearMiles(valorCLP) + " CLP";

            if (monedaVisual == "CLP")
            {
                return clp;
            }

            return clp + " | " + FormatearValorVisual(cotizacion, valorCLP);
        }
    }
}