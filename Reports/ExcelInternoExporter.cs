using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Reports
{
    public static class ExcelInternoExporter
    {
        public static bool GuardarExcelInterno(Cotizacion cotizacion)
        {
            return GuardarExcelInterno(cotizacion, cotizacion?.ProyectoProductivo, null, null);
        }

        public static bool GuardarExcelInterno(
            Cotizacion cotizacion,
            ProyectoCotizacion proyecto,
            IEnumerable<PersonaEquipo> personal,
            IEnumerable<CategoriaTrabajador> cargos)
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
            dialogo.Filter = "Libro de Excel (*.xlsx)|*.xlsx";
            dialogo.FileName = nombreSugerido;
            dialogo.DefaultExt = "xlsx";
            dialogo.AddExtension = true;
            dialogo.OverwritePrompt = true;

            DialogResult resultado = dialogo.ShowDialog();

            if (resultado != DialogResult.OK)
            {
                return false;
            }

            try
            {
                GuardarExcelXlsxEnRuta(
                    dialogo.FileName,
                    cotizacion,
                    proyecto,
                    personal,
                    cargos);

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

        public static void GuardarExcelXlsxEnRuta(
            string ruta,
            Cotizacion cotizacion,
            ProyectoCotizacion proyecto = null,
            IEnumerable<PersonaEquipo> personal = null,
            IEnumerable<CategoriaTrabajador> cargos = null)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new ArgumentException("La ruta de Excel está vacía.", nameof(ruta));
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            string xml = GenerarExcelXml(cotizacion, proyecto, personal, cargos);
            XDocument documento = XDocument.Parse(xml);
            XNamespace hojaNs = "urn:schemas-microsoft-com:office:spreadsheet";
            XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

            using XLWorkbook libro = new XLWorkbook();
            foreach (XElement hojaXml in documento.Root.Elements(hojaNs + "Worksheet"))
            {
                string nombre = (string)hojaXml.Attribute(ss + "Name") ?? "Hoja";
                IXLWorksheet hoja = libro.Worksheets.Add(NombreHojaSeguro(nombre));
                XElement tabla = hojaXml.Element(hojaNs + "Table");
                if (tabla == null) continue;

                int indiceColumna = 1;
                foreach (XElement columna in tabla.Elements(hojaNs + "Column"))
                {
                    double anchoXml = (double?)columna.Attribute(ss + "Width") ?? 80;
                    hoja.Column(indiceColumna++).Width = Math.Max(8, Math.Min(80, anchoXml / 7.0));
                }

                int fila = 1;
                foreach (XElement filaXml in tabla.Elements(hojaNs + "Row"))
                {
                    int columna = 1;
                    foreach (XElement celdaXml in filaXml.Elements(hojaNs + "Cell"))
                    {
                        int? indiceExplicito = (int?)celdaXml.Attribute(ss + "Index");
                        if (indiceExplicito.HasValue) columna = indiceExplicito.Value;
                        XElement dato = celdaXml.Element(hojaNs + "Data");
                        string tipo = (string)dato?.Attribute(ss + "Type") ?? "String";
                        string valor = dato?.Value ?? "";
                        IXLCell celda = hoja.Cell(fila, columna);
                        if (tipo == "Number" && double.TryParse(valor, NumberStyles.Any,
                            CultureInfo.InvariantCulture, out double numero))
                            celda.Value = numero;
                        else
                            celda.Value = valor;

                        string estilo = (string)celdaXml.Attribute(ss + "StyleID") ?? "";
                        AplicarEstiloXlsx(celda, estilo);
                        int combinar = (int?)celdaXml.Attribute(ss + "MergeAcross") ?? 0;
                        if (combinar > 0)
                        {
                            hoja.Range(fila, columna, fila, columna + combinar).Merge();
                            AplicarEstiloXlsx(hoja.Cell(fila, columna), estilo);
                        }
                        columna += combinar + 1;
                    }
                    fila++;
                }

                hoja.SheetView.FreezeRows(2);
                hoja.RangeUsed()?.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            }

            string carpeta = Path.GetDirectoryName(Path.GetFullPath(ruta)) ?? "";
            if (!string.IsNullOrWhiteSpace(carpeta)) Directory.CreateDirectory(carpeta);
            libro.SaveAs(ruta);
        }

        private static void AplicarEstiloXlsx(IXLCell celda, string estilo)
        {
            celda.Style.Font.FontName = "Segoe UI";
            if (estilo == "Titulo")
            {
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontSize = 16;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#EE1E5B");
                celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            else if (estilo == "Subtitulo" || estilo == "Total")
            {
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#FBF283");
            }
            else if (estilo == "Header")
            {
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#53C0A6");
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            else if (estilo == "Label")
            {
                celda.Style.Font.Bold = true;
                celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEEEEE");
            }

            if (estilo == "Number" || estilo == "Total") celda.Style.NumberFormat.Format = "#,##0.00";
            if (estilo == "Integer") celda.Style.NumberFormat.Format = "#,##0";
            if (estilo == "Percent") celda.Style.NumberFormat.Format = "0.00";
        }

        private static string NombreHojaSeguro(string nombre)
        {
            string limpio = string.IsNullOrWhiteSpace(nombre) ? "Hoja" : nombre.Trim();
            foreach (char invalido in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                limpio = limpio.Replace(invalido, '-');
            return limpio.Length <= 31 ? limpio : limpio.Substring(0, 31);
        }

        public static string GenerarExcelXml(
            Cotizacion cotizacion,
            ProyectoCotizacion proyecto = null,
            IEnumerable<PersonaEquipo> personal = null,
            IEnumerable<CategoriaTrabajador> cargos = null)
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
            if (proyecto != null)
            {
                List<PersonaEquipo> personas = (personal ?? Enumerable.Empty<PersonaEquipo>())
                    .Where(p => p != null).ToList();
                List<CategoriaTrabajador> bibliotecaCargos =
                    (cargos ?? Enumerable.Empty<CategoriaTrabajador>())
                    .Where(c => c != null).ToList();
                ProyectoProductivoExpandido expandido =
                    ProyectoProductivoExpansionService.Expandir(proyecto);
                EscribirHojaProyectoGlobal(xml, proyecto, expandido, personas, bibliotecaCargos);
                EscribirHojaDesgloseGlobal(xml, proyecto, expandido, personas, bibliotecaCargos);
                EscribirHojaTrabajadoresGlobal(xml, proyecto, expandido, personas, bibliotecaCargos);
                EscribirHojaCargosGlobal(xml, expandido, personas, bibliotecaCargos);
            }
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

        private static void EscribirHojaProyectoGlobal(
            StringBuilder xml,
            ProyectoCotizacion proyecto,
            ProyectoProductivoExpandido expandido,
            List<PersonaEquipo> personas,
            List<CategoriaTrabajador> cargos)
        {
            List<FilaProductivaProyecto> filas = expandido?.Filas ?? new List<FilaProductivaProyecto>();
            AbrirHoja(xml, "Proyecto global");
            EscribirColumna(xml, 240);
            EscribirColumna(xml, 190);
            EscribirFilaTitulo(xml, "RESUMEN PRODUCTIVO GLOBAL", 2);
            EscribirFilaDatoTexto(xml, "Proyecto", ValorTexto(proyecto.Nombre));
            EscribirFilaDatoTexto(xml, "Cliente", ValorTexto(proyecto.Cliente));
            EscribirFilaDatoNumero(xml, "Productos y servicios",
                proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>()).Count(i => i != null && i.Activo));
            EscribirFilaDatoNumero(xml, "Subproductos",
                proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                    .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>()).Count(s => s != null && s.Activo));
            EscribirFilaDatoNumero(xml, "Horas totales", filas.Sum(HorasFila));
            EscribirFilaDatoNumero(xml, "Costo directo CLP",
                filas.Where(f => !f.Transversal).Sum(f => CostoFila(f, personas, cargos)));
            EscribirFilaDatoNumero(xml, "Costo transversal CLP",
                filas.Where(f => f.Transversal).Sum(f => CostoFila(f, personas, cargos)));
            EscribirFilaDatoNumero(xml, "Inversión total CLP",
                filas.Sum(f => CostoFila(f, personas, cargos)));
            EscribirFilaDatoNumero(xml, "Trabajadores asignados",
                filas.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId)).Select(f => f.PersonaId)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count());
            CerrarHoja(xml);
        }

        private static void EscribirHojaDesgloseGlobal(
            StringBuilder xml,
            ProyectoCotizacion proyecto,
            ProyectoProductivoExpandido expandido,
            List<PersonaEquipo> personas,
            List<CategoriaTrabajador> cargos)
        {
            AbrirHoja(xml, "Desglose productivo");
            foreach (double ancho in new double[] { 130, 190, 170, 210, 105, 110, 80, 85, 85, 85, 210, 150, 85, 85, 110, 140, 260 })
                EscribirColumna(xml, ancho);
            EscribirFilaTitulo(xml, "DESGLOSE GLOBAL DEL PROYECTO", 17);
            xml.AppendLine("<Row>");
            foreach (string cabecera in new[] { "Grupo", "Producto / servicio", "Subproducto", "Proceso", "Tipo", "Etapa", "Cantidad", "Unidad", "Capacidad", "Periodo", "Cargo", "Trabajador", "Horas calculadas", "Horas asignadas", "Costo CLP", "Origen", "Diagnóstico" })
                EscribirCeldaTexto(xml, cabecera, "Header");
            xml.AppendLine("</Row>");

            foreach (FilaProductivaProyecto fila in expandido?.Filas ?? new List<FilaProductivaProyecto>())
            {
                xml.AppendLine("<Row>");
                EscribirCeldaTexto(xml, NombreGrupo(proyecto, fila.GrupoId));
                EscribirCeldaTexto(xml, NombreItem(proyecto, fila.ItemId));
                EscribirCeldaTexto(xml, NombreSubproducto(proyecto, fila.SubproductoProyectoId));
                EscribirCeldaTexto(xml, NombreProceso(proyecto, fila));
                EscribirCeldaTexto(xml, fila.TipoProceso.ToString());
                EscribirCeldaTexto(xml, ValorTexto(fila.EtapaId));
                EscribirCeldaNumero(xml, Convert.ToDouble(fila.Cantidad));
                EscribirCeldaTexto(xml, ValorTexto(fila.Unidad));
                EscribirCeldaNumero(xml, Convert.ToDouble(fila.Capacidad));
                EscribirCeldaTexto(xml, ValorTexto(fila.Periodo));
                EscribirCeldaTexto(xml, NombreCargo(fila.CargoId, cargos));
                EscribirCeldaTexto(xml, NombrePersona(fila.PersonaId, personas));
                EscribirCeldaNumero(xml, Convert.ToDouble(fila.HorasCalculadas));
                EscribirCeldaNumero(xml, Convert.ToDouble(fila.HorasAsignadas));
                EscribirCeldaNumero(xml, CostoFila(fila, personas, cargos));
                EscribirCeldaTexto(xml, ValorTexto(fila.OrigenCalculo));
                EscribirCeldaTexto(xml, ValorTexto(fila.Diagnostico));
                xml.AppendLine("</Row>");
            }
            CerrarHoja(xml);
        }

        private static void EscribirHojaTrabajadoresGlobal(
            StringBuilder xml,
            ProyectoCotizacion proyecto,
            ProyectoProductivoExpandido expandido,
            List<PersonaEquipo> personas,
            List<CategoriaTrabajador> cargos)
        {
            AbrirHoja(xml, "Trabajadores");
            foreach (double ancho in new double[] { 180, 220, 140, 100, 110, 100, 120, 280 })
                EscribirColumna(xml, ancho);
            EscribirFilaTitulo(xml, "TRABAJADORES E INVERSIÓN", 8);
            xml.AppendLine("<Row>");
            foreach (string cabecera in new[] { "Trabajador", "Cargo(s)", "Pago acordado CLP", "Periodo", "Costo hora CLP", "Horas", "Inversión CLP", "Productos" })
                EscribirCeldaTexto(xml, cabecera, "Header");
            xml.AppendLine("</Row>");

            List<FilaProductivaProyecto> filas = expandido?.Filas ?? new List<FilaProductivaProyecto>();
            foreach (IGrouping<string, FilaProductivaProyecto> grupo in filas
                .Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                .GroupBy(f => f.PersonaId, StringComparer.OrdinalIgnoreCase))
            {
                PersonaEquipo persona = BuscarPersona(grupo.Key, personas);
                xml.AppendLine("<Row>");
                EscribirCeldaTexto(xml, NombrePersona(grupo.Key, personas));
                EscribirCeldaTexto(xml, Unir(grupo.Select(f => NombreCargo(f.CargoId, cargos))));
                EscribirCeldaNumero(xml, Convert.ToDouble(persona?.PagoInterno ?? 0m));
                EscribirCeldaTexto(xml, ValorTexto(persona?.PeriodoPago));
                EscribirCeldaNumero(xml, Convert.ToDouble(TarifaHora(persona, null)));
                EscribirCeldaNumero(xml, grupo.Sum(HorasFila));
                EscribirCeldaNumero(xml, grupo.Sum(f => CostoFila(f, personas, cargos)));
                EscribirCeldaTexto(xml, Unir(grupo.Select(f => NombreItem(proyecto, f.ItemId))));
                xml.AppendLine("</Row>");
            }
            CerrarHoja(xml);
        }

        private static void EscribirHojaCargosGlobal(
            StringBuilder xml,
            ProyectoProductivoExpandido expandido,
            List<PersonaEquipo> personas,
            List<CategoriaTrabajador> cargos)
        {
            AbrirHoja(xml, "Cargos");
            foreach (double ancho in new double[] { 220, 125, 125, 125, 110, 260, 100, 130 })
                EscribirColumna(xml, ancho);
            EscribirFilaTitulo(xml, "CARGOS, SUELDOS E INVERSIÓN", 8);
            xml.AppendLine("<Row>");
            foreach (string cabecera in new[] { "Cargo", "Sueldo mínimo", "Sueldo típico", "Sueldo máximo", "Tarifa hora", "Trabajadores", "Horas", "Inversión CLP" })
                EscribirCeldaTexto(xml, cabecera, "Header");
            xml.AppendLine("</Row>");

            foreach (IGrouping<string, FilaProductivaProyecto> grupo in
                (expandido?.Filas ?? new List<FilaProductivaProyecto>())
                .Where(f => !string.IsNullOrWhiteSpace(f.CargoId))
                .GroupBy(f => f.CargoId, StringComparer.OrdinalIgnoreCase))
            {
                CategoriaTrabajador cargo = BuscarCargo(grupo.Key, cargos);
                xml.AppendLine("<Row>");
                EscribirCeldaTexto(xml, NombreCargo(grupo.Key, cargos));
                EscribirCeldaNumero(xml, cargo?.SueldoMensualCLPMin ?? 0);
                EscribirCeldaNumero(xml, cargo?.SueldoMensualCLPTipico ?? 0);
                EscribirCeldaNumero(xml, cargo?.SueldoMensualCLPMax ?? 0);
                EscribirCeldaNumero(xml, Convert.ToDouble(TarifaHora(null, cargo)));
                EscribirCeldaTexto(xml, Unir(grupo.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                    .Select(f => NombrePersona(f.PersonaId, personas))));
                EscribirCeldaNumero(xml, grupo.Sum(HorasFila));
                EscribirCeldaNumero(xml, grupo.Sum(f => CostoFila(f, personas, cargos)));
                xml.AppendLine("</Row>");
            }
            CerrarHoja(xml);
        }

        private static double HorasFila(FilaProductivaProyecto fila)
        {
            return Convert.ToDouble(fila.HorasAsignadas > 0m ? fila.HorasAsignadas : fila.HorasCalculadas);
        }

        private static double CostoFila(FilaProductivaProyecto fila, List<PersonaEquipo> personas, List<CategoriaTrabajador> cargos)
        {
            if (fila.Costo > 0m) return Convert.ToDouble(fila.Costo);
            return HorasFila(fila) * Convert.ToDouble(TarifaHora(
                BuscarPersona(fila.PersonaId, personas), BuscarCargo(fila.CargoId, cargos)));
        }

        private static decimal TarifaHora(PersonaEquipo persona, CategoriaTrabajador cargo)
        {
            if (persona != null)
            {
                if (persona.CostoHora > 0m) return persona.CostoHora;
                if (persona.PagoInterno > 0m)
                {
                    decimal semana = persona.HorasTrabajoSemana > 0m ? persona.HorasTrabajoSemana : 42m;
                    string periodo = Normalizar(persona.PeriodoPago);
                    decimal horas = periodo.Contains("seman") ? semana : periodo.Contains("quinc") ? semana * 2m :
                        periodo.Contains("dia") ? Math.Max(1m, semana / 5m) : semana * 4m;
                    return persona.PagoInterno / horas;
                }
            }
            return cargo == null || cargo.SueldoMensualCLPTipico <= 0 ? 0m :
                (decimal)cargo.SueldoMensualCLPTipico / 22m / 8m;
        }

        private static PersonaEquipo BuscarPersona(string id, List<PersonaEquipo> personas)
        {
            return string.IsNullOrWhiteSpace(id) ? null : personas.FirstOrDefault(p =>
                string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Nombre, id, StringComparison.OrdinalIgnoreCase));
        }

        private static CategoriaTrabajador BuscarCargo(string id, List<CategoriaTrabajador> cargos)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (int.TryParse(id, out int numero))
            {
                CategoriaTrabajador porId = cargos.FirstOrDefault(c => c.Id == numero);
                if (porId != null) return porId;
            }
            string buscado = NormalizarCargo(id);
            return cargos.FirstOrDefault(c => NormalizarCargo(c.Nombre) == buscado ||
                NormalizarCargo(c.NombreCompleto) == buscado);
        }

        private static string NombrePersona(string id, List<PersonaEquipo> personas)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Sin asignar";
            PersonaEquipo persona = BuscarPersona(id, personas);
            return persona == null ? id : ValorTexto(persona.Nombre);
        }

        private static string NombreCargo(string id, List<CategoriaTrabajador> cargos)
        {
            if (string.IsNullOrWhiteSpace(id)) return "Sin cargo";
            CategoriaTrabajador cargo = BuscarCargo(id, cargos);
            return cargo == null ? id : cargo.NombreCompleto;
        }

        private static string NombreGrupo(ProyectoCotizacion proyecto, string id) =>
            proyecto.Grupos.FirstOrDefault(g => string.Equals(g.Id, id, StringComparison.OrdinalIgnoreCase))?.Nombre ?? id;
        private static string NombreItem(ProyectoCotizacion proyecto, string id) =>
            proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .FirstOrDefault(i => string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase))?.Nombre ?? id;
        private static string NombreSubproducto(ProyectoCotizacion proyecto, string id) =>
            string.IsNullOrWhiteSpace(id) ? "Proceso directo" : proyecto.Grupos
                .SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>())
                .FirstOrDefault(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase))?.Nombre ?? id;

        private static string NombreProceso(ProyectoCotizacion proyecto, FilaProductivaProyecto fila)
        {
            IEnumerable<ProcesoProyecto> procesos = proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => (i.Procesos ?? new List<ProcesoProyecto>()).Concat(
                    (i.Subproductos ?? new List<SubproductoProyecto>()).SelectMany(s =>
                        (s.Procesos ?? new List<ProcesoProyecto>()).Concat(
                            (s.Instancias ?? new List<InstanciaSubproducto>()).SelectMany(x => x.Procesos ?? new List<ProcesoProyecto>())))));
            ProcesoProyecto proceso = procesos.FirstOrDefault(p => string.Equals(p.Id, fila.ProcesoProyectoId, StringComparison.OrdinalIgnoreCase));
            if (proceso != null) return ValorTexto(proceso.Nombre);
            ProcesoTransversalProyecto transversal = (proyecto.ProcesosTransversales ?? new List<ProcesoTransversalProyecto>())
                .FirstOrDefault(p => string.Equals(p.Id, fila.ProcesoProyectoId, StringComparison.OrdinalIgnoreCase));
            return transversal == null ? (string.IsNullOrWhiteSpace(fila.ProcesoBibliotecaId) ? fila.ProcesoProyectoId : fila.ProcesoBibliotecaId) : transversal.Nombre;
        }

        private static string Unir(IEnumerable<string> valores) => string.Join(", ", valores
            .Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(v => v));

        private static string NormalizarCargo(string valor) => Normalizar(valor)
            .Replace("cargo ", "").Replace(" tipico", "").Replace(" tipica", "").Trim();

        private static string Normalizar(string valor)
        {
            string texto = (valor ?? "").Trim().ToLowerInvariant().Replace("_", " ").Normalize(NormalizationForm.FormD);
            StringBuilder limpio = new StringBuilder();
            foreach (char c in texto)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))) limpio.Append(c);
            return string.Join(" ", limpio.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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
                return "Excel_Othalart_" + proyecto + "_" + cliente + "_" + fecha + ".xlsx";
            }

            return "Excel_Othalart_" + proyecto + "_" + fecha + ".xlsx";
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
