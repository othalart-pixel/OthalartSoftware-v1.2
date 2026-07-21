using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private class GanttHitBox
        {
            public Rectangle Rect { get; set; }
            public EtapaProyecto Etapa { get; set; }
            public SubEtapaProyecto SubEtapa { get; set; }
            public string TipoAccion { get; set; }
        }

        private readonly List<GanttHitBox> ganttHitBoxes = new List<GanttHitBox>();

        private void AgregarTituloResumen(string texto)
        {
            AgregarTextoResumen(texto + "\n", new Font("Segoe UI", 10, FontStyle.Bold), ColorResumenTitulo());
        }

        private void AgregarLineaResumen(string etiqueta, string valor, bool destacarValor)
        {
            AgregarTextoResumen(etiqueta, new Font("Segoe UI", 10, FontStyle.Regular), ColorResumenTexto());

            if (destacarValor)
            {
                AgregarTextoResumen(valor + "\n", new Font("Segoe UI", 10, FontStyle.Bold), ColorResumenDestacado());
            }
            else
            {
                AgregarTextoResumen(valor + "\n", new Font("Segoe UI", 10, FontStyle.Regular), ColorResumenSecundario());
            }
        }

        private void AgregarBloqueResumen(string valor, bool destacarValor)
        {
            if (destacarValor)
            {
                AgregarTextoResumen(valor + "\n", new Font("Segoe UI", 10, FontStyle.Bold), ColorResumenDestacado());
            }
            else
            {
                AgregarTextoResumen(valor + "\n", new Font("Segoe UI", 10, FontStyle.Regular), ColorResumenSecundario());
            }
        }

        private void AgregarSaltoResumen()
        {
            AgregarTextoResumen("\n", new Font("Segoe UI", 10, FontStyle.Regular), ColorResumenTexto());
        }

        private Color ColorResumenTitulo()
        {
            return modoOscuroActivo ? Color.FromArgb(230, 230, 230) : Color.Black;
        }

        private Color ColorResumenTexto()
        {
            return modoOscuroActivo ? Color.FromArgb(220, 220, 220) : Color.Black;
        }

        private Color ColorResumenDestacado()
        {
            return modoOscuroActivo ? Color.FromArgb(235, 235, 235) : Color.FromArgb(30, 30, 30);
        }

        private Color ColorResumenSecundario()
        {
            return modoOscuroActivo ? Color.FromArgb(160, 160, 160) : Color.FromArgb(135, 135, 135);
        }

        private void AgregarTextoResumen(string texto, Font fuente, Color color)
        {
            rtbResumen.SelectionStart = rtbResumen.TextLength;
            rtbResumen.SelectionLength = 0;
            rtbResumen.SelectionFont = fuente;
            rtbResumen.SelectionColor = color;
            rtbResumen.AppendText(texto);
        }

        private bool TieneContenido(string texto)
        {
            return !string.IsNullOrWhiteSpace(texto);
        }

        private string TextoOPlaceholder(string valor, string placeholder)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? placeholder
                : valor.Trim();
        }

        private void RefrescarResumen()
        {
            if (rtbResumen == null || cotizacion == null)
            {
                return;
            }

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            string monedaCliente = string.IsNullOrWhiteSpace(cotizacion.MonedaPrecioCliente)
                ? (string.IsNullOrWhiteSpace(cotizacion.Moneda) ? "CLP" : cotizacion.Moneda.Trim().ToUpperInvariant())
                : cotizacion.MonedaPrecioCliente.Trim().ToUpperInvariant();

            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            string cliente = TextoOPlaceholder(cotizacion.NombreCliente, "No informado");
            string empresa = TextoOPlaceholder(cotizacion.Empresa, "No informada");
            string email = TextoOPlaceholder(cotizacion.Email, "No informado");
            string proyecto = TextoOPlaceholder(cotizacion.NombreProyecto, "No informado");
            string descripcion = TextoOPlaceholder(cotizacion.Descripcion, "No informada");

            string industria = TextoOPlaceholder(brief.IndustriaCliente, "No informada");
            string destino = TextoOPlaceholder(brief.DestinoUso, "No informado");
            string estilo = TextoOPlaceholder(brief.EstiloVisual, "No informado");
            string acabado = TextoOPlaceholder(brief.NivelAcabado, "No informado");
            string formato = TextoOPlaceholder(brief.FormatoEntrega, "No informado");
            string aspecto = TextoOPlaceholder(brief.RelacionAspecto, "No informado");
            string resolucion = TextoOPlaceholder(brief.ResolucionEntrega, "No informada");

            string fechaInicio = FormatearFechaResumen(cotizacion.FechaInicioCliente);
            string fechaEntrega = FormatearFechaResumen(cotizacion.FechaEntregaCliente);
            string plazoCliente = ObtenerTextoPlazoClienteResumen();

            int cantidadEntregables = brief.EntregablesSeleccionados == null
                ? 0
                : brief.EntregablesSeleccionados.Count;

            int cantidadTotal = brief.EntregablesSeleccionados == null
                ? 0
                : brief.EntregablesSeleccionados.Sum(e => e.Cantidad);

            rtbResumen.SuspendLayout();
            rtbResumen.Clear();

            // =========================================================
            // RESUMEN COMERCIAL
            // =========================================================

            AgregarTituloResumen("RESUMEN COMERCIAL");
            AgregarLineaResumen("Proyecto: ", proyecto, TieneContenido(cotizacion.NombreProyecto));
            AgregarLineaResumen("Cliente: ", cliente, TieneContenido(cotizacion.NombreCliente));
            AgregarLineaResumen("Empresa / marca: ", empresa, TieneContenido(cotizacion.Empresa));
            AgregarLineaResumen("Email: ", email, TieneContenido(cotizacion.Email));
            AgregarLineaResumen("Moneda cotización: ", monedaCliente, true);
            AgregarLineaResumen(
                "Presupuesto cliente: ",
                FormatearPresupuestoClienteResumen(),
                cotizacion.PresupuestoCliente > 0
            );
            AgregarSaltoResumen();

            // =========================================================
            // ALCANCE PRODUCTIVO
            // =========================================================

            AgregarTituloResumen("ALCANCE PRODUCTIVO");
            AgregarLineaResumen("Industria / rubro: ", industria, TieneContenido(brief.IndustriaCliente));
            AgregarLineaResumen("Destino / uso: ", destino, TieneContenido(brief.DestinoUso));
            AgregarLineaResumen("Estilo visual: ", estilo, TieneContenido(brief.EstiloVisual));
            AgregarLineaResumen("Nivel de acabado: ", acabado, TieneContenido(brief.NivelAcabado));
            AgregarLineaResumen("Formato entrega: ", formato, TieneContenido(brief.FormatoEntrega));
            AgregarLineaResumen("Relación aspecto: ", aspecto, TieneContenido(brief.RelacionAspecto));
            AgregarLineaResumen("Resolución: ", resolucion, TieneContenido(brief.ResolucionEntrega));
            AgregarLineaResumen(
                "Entregables seleccionados: ",
                $"{cantidadEntregables} tipos / {cantidadTotal} unidades",
                cantidadEntregables > 0
            );

            string resumenEntregables = ObtenerResumenEntregablesSeleccionados(brief);

            if (!string.IsNullOrWhiteSpace(resumenEntregables))
            {
                AgregarLineaResumen("Detalle entregables: ", resumenEntregables, true);
            }

            AgregarSaltoResumen();

            // =========================================================
            // PLAZO CLIENTE
            // =========================================================

            AgregarTituloResumen("PLAZO CLIENTE");
            AgregarLineaResumen("Fecha inicio estimada: ", fechaInicio, cotizacion.FechaInicioCliente.HasValue);
            AgregarLineaResumen("Fecha entrega objetivo: ", fechaEntrega, cotizacion.FechaEntregaCliente.HasValue);
            AgregarLineaResumen("Plazo declarado: ", plazoCliente, cotizacion.FechaInicioCliente.HasValue && cotizacion.FechaEntregaCliente.HasValue);

            if (cotizacion.EvaluacionPlazo != null)
            {
                AgregarLineaResumen(
                    "Diagnóstico plazo: ",
                    TextoOPlaceholder(cotizacion.EvaluacionPlazo.DiagnosticoPlazo, "No evaluado"),
                    TieneContenido(cotizacion.EvaluacionPlazo.DiagnosticoPlazo)
                );

                AgregarLineaResumen(
                    "Mínimo técnico: ",
                    $"{cotizacion.EvaluacionPlazo.SemanasMinimasEstimadas:0.##} semanas",
                    cotizacion.EvaluacionPlazo.SemanasMinimasEstimadas > 0.0
                );

                AgregarLineaResumen(
                    "Estándar técnico: ",
                    $"{cotizacion.EvaluacionPlazo.SemanasEstandarEstimadas:0.##} semanas",
                    cotizacion.EvaluacionPlazo.SemanasEstandarEstimadas > 0.0
                );

                AgregarLineaResumen(
                    "Holgura técnica: ",
                    $"{cotizacion.EvaluacionPlazo.SemanasConHolguraEstimadas:0.##} semanas",
                    cotizacion.EvaluacionPlazo.SemanasConHolguraEstimadas > 0.0
                );
            }

            AgregarSaltoResumen();

            // =========================================================
            // PLANIFICACIÓN INTERNA
            // =========================================================

            AgregarTituloResumen("PLANIFICACIÓN INTERNA");
            AgregarLineaResumen("Duración planificada: ", $"{DuracionVisibleProyecto():0.##} meses", DuracionVisibleProyecto() > 0.0);
            AgregarLineaResumen("Persona-mes total: ", $"{cotizacion.PersonaMesTotal:0.##}", cotizacion.PersonaMesTotal > 0.0);
            AgregarLineaResumen("Persona-mes etapas: ", $"{cotizacion.PersonaMesEtapas:0.##}", cotizacion.PersonaMesEtapas > 0.0);
            AgregarLineaResumen("Persona-mes general: ", $"{cotizacion.PersonaMesGeneral:0.##}", cotizacion.PersonaMesGeneral > 0.0);
            AgregarSaltoResumen();

            // =========================================================
            // COSTOS
            // =========================================================

            AgregarTituloResumen($"COSTOS EN {monedaVisual}");
            AgregarLineaResumen("Mano de obra etapas: ", FormatearValorVisual(cotizacion.CostoManoObraEtapas), cotizacion.CostoManoObraEtapas > 0.0);
            AgregarLineaResumen("Mano de obra general: ", FormatearValorVisual(cotizacion.CostoManoObraGeneral), cotizacion.CostoManoObraGeneral > 0.0);
            AgregarLineaResumen("Mano de obra total: ", FormatearValorVisual(cotizacion.CostoManoObraTotal), cotizacion.CostoManoObraTotal > 0.0);
            AgregarLineaResumen("Producción interna: ", FormatearValorVisual(cotizacion.CostoProduccionInterna), cotizacion.CostoProduccionInterna > 0.0);
            AgregarLineaResumen("Administración: ", FormatearValorVisual(cotizacion.CostoAdministrativo), cotizacion.CostoAdministrativo > 0.0);
            AgregarLineaResumen("Servicios tercerizados: ", FormatearValorVisual(cotizacion.CostoTercerizados), cotizacion.CostoTercerizados > 0.0);
            AgregarLineaResumen("Otros costos: ", FormatearValorVisual(cotizacion.OtrosCostos), cotizacion.OtrosCostos > 0.0);
            AgregarLineaResumen("Costo base: ", FormatearValorVisual(cotizacion.CostoBase), cotizacion.CostoBase > 0.0);
            AgregarLineaResumen("Imprevistos: ", FormatearValorVisual(cotizacion.Imprevistos), cotizacion.Imprevistos > 0.0);
            AgregarLineaResumen("Costo total: ", FormatearValorVisual(cotizacion.CostoTotal), cotizacion.CostoTotal > 0.0);
            AgregarSaltoResumen();

            // =========================================================
            // PRECIO / RENTABILIDAD
            // =========================================================

            AgregarTituloResumen($"PRECIO / RENTABILIDAD EN {monedaVisual}");
            AgregarLineaResumen("Precio recomendado: ", FormatearValorVisual(cotizacion.PrecioRecomendado), cotizacion.PrecioRecomendado > 0.0);
            AgregarLineaResumen("Precio evaluado: ", FormatearValorVisual(cotizacion.PrecioVentaEvaluado), cotizacion.PrecioVentaEvaluado > 0.0);
            AgregarLineaResumen("Utilidad estimada: ", FormatearValorVisual(cotizacion.UtilidadEvaluada), cotizacion.UtilidadEvaluada != 0.0);
            AgregarLineaResumen("Margen evaluado: ", $"{cotizacion.MargenEvaluado * 100.0:0.##}%", cotizacion.MargenEvaluado != 0.0);
            AgregarLineaResumen("Markup evaluado: ", $"{cotizacion.MarkupEvaluado * 100.0:0.##}%", cotizacion.MarkupEvaluado != 0.0);

            AgregarSaltoResumen();

            // =========================================================
            // DESCRIPCIÓN
            // =========================================================

            AgregarTituloResumen("DESCRIPCIÓN");
            AgregarBloqueResumen(descripcion, TieneContenido(cotizacion.Descripcion));

            rtbResumen.SelectionStart = 0;
            rtbResumen.SelectionLength = 0;
            rtbResumen.ResumeLayout();

            MarcarResumenActualizado();
        }

        private string FormatearFechaResumen(DateTime? fecha)
        {
            if (!fecha.HasValue)
            {
                return "No informada";
            }

            return fecha.Value.ToString("dd-MM-yyyy");
        }

        private string ObtenerTextoPlazoClienteResumen()
        {
            if (!cotizacion.FechaInicioCliente.HasValue || !cotizacion.FechaEntregaCliente.HasValue)
            {
                return "No informado";
            }

            DateTime inicio = cotizacion.FechaInicioCliente.Value.Date;
            DateTime entrega = cotizacion.FechaEntregaCliente.Value.Date;

            double dias = (entrega - inicio).TotalDays;

            if (dias < 0.0)
            {
                return "Fecha inválida";
            }

            double semanas = dias / 7.0;
            double meses = dias / 30.0;

            return $"{dias:0} días / {semanas:0.#} semanas / {meses:0.##} meses aprox.";
        }

        private string FormatearPresupuestoClienteResumen()
        {
            if (cotizacion == null || cotizacion.PresupuestoCliente <= 0)
            {
                return "No informado";
            }

            string moneda = string.IsNullOrWhiteSpace(cotizacion.MonedaPresupuestoCliente)
                ? (string.IsNullOrWhiteSpace(cotizacion.MonedaPrecioCliente) ? "CLP" : cotizacion.MonedaPrecioCliente)
                : cotizacion.MonedaPresupuestoCliente;

            double valorVisual = ConvertirDesdeCLP((double)cotizacion.PresupuestoCliente, moneda);

            return FormatearValorVisual(valorVisual, moneda);
        }

        private string ObtenerResumenEntregablesSeleccionados(BriefProductoProyecto brief)
        {
            if (brief == null ||
                brief.EntregablesSeleccionados == null ||
                brief.EntregablesSeleccionados.Count == 0)
            {
                return "";
            }

            var principales = brief.EntregablesSeleccionados
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Nombre))
                .Take(4)
                .Select(e =>
                {
                    string unidad = string.IsNullOrWhiteSpace(e.UnidadCantidad)
                        ? "unid."
                        : e.UnidadCantidad;

                    return $"{e.Cantidad} {unidad} {e.Nombre}";
                })
                .ToList();

            if (principales.Count == 0)
            {
                return "";
            }

            string texto = string.Join("; ", principales);

            int restantes = brief.EntregablesSeleccionados.Count - principales.Count;

            if (restantes > 0)
            {
                texto += $"; +{restantes} más";
            }

            return texto;
        }


        private string TextoResumen(string texto, string respaldo)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return respaldo;
            }

            return texto.Trim();
        }

        private void PanelGraficoMargen_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Font font = new Font("Segoe UI", 9);
            Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            Font smallFont = new Font("Segoe UI", 8);

            Brush textBrush = Brushes.Black;
            Pen axisPen = new Pen(Color.FromArgb(80, 80, 80), 1);
            Pen gridPen = new Pen(Color.FromArgb(225, 225, 225), 1);
            Pen curvePen = new Pen(Color.FromArgb(83, 192, 166), 3);
            Pen selectedPen = new Pen(Color.FromArgb(238, 30, 91), 2);

            int left = 90;
            int right = 55;
            int top = 58;
            int bottom = 55;

            int width = panelGraficoMargen.Width - left - right;
            int height = panelGraficoMargen.Height - top - bottom;

            if (width <= 20 || height <= 20)
            {
                axisPen.Dispose();
                gridPen.Dispose();
                curvePen.Dispose();
                selectedPen.Dispose();
                return;
            }

            double costoTotal = cotizacion.CostoTotal;

            g.DrawString("Precio de venta según margen", titleFont, textBrush, 15, 18);

            if (costoTotal <= 0.0)
            {
                g.DrawString("Agregue costos para visualizar el análisis.", font, textBrush, 15, 48);

                axisPen.Dispose();
                gridPen.Dispose();
                curvePen.Dispose();
                selectedPen.Dispose();
                return;
            }

            double margenMin = -0.10;
            double margenMax = 0.40;

            double precioMin = CalcularPrecioPorMargen(costoTotal, margenMin);
            double precioMax = CalcularPrecioPorMargen(costoTotal, margenMax);

            double paddingPrecio = (precioMax - precioMin) * 0.08;
            precioMin -= paddingPrecio;
            precioMax += paddingPrecio;

            if (precioMax <= precioMin)
            {
                precioMax = precioMin + 1.0;
            }

            Rectangle area = new Rectangle(left, top, width, height);

            g.FillRectangle(Brushes.White, area);

            for (int i = 0; i <= 5; i++)
            {
                int y = top + (int)(height * i / 5.0);
                g.DrawLine(gridPen, left, y, left + width, y);

                double precio = precioMax - ((precioMax - precioMin) * i / 5.0);
                g.DrawString(precio.ToString("N0"), smallFont, textBrush, 10, y - 8);
            }

            for (int margenPorcentaje = -10; margenPorcentaje <= 40; margenPorcentaje += 10)
            {
                double margen = margenPorcentaje / 100.0;
                int x = left + (int)(((margen - margenMin) / (margenMax - margenMin)) * width);

                g.DrawLine(gridPen, x, top, x, top + height);
                g.DrawString(margenPorcentaje + "%", font, textBrush, x - 14, top + height + 8);
            }

            g.DrawLine(axisPen, left, top, left, top + height);
            g.DrawLine(axisPen, left, top + height, left + width, top + height);

            Point? puntoAnterior = null;

            for (int i = 0; i <= 160; i++)
            {
                double margen = margenMin + ((margenMax - margenMin) * i / 160.0);
                double precio = CalcularPrecioPorMargen(costoTotal, margen);

                int x = left + (int)(((margen - margenMin) / (margenMax - margenMin)) * width);
                int y = top + height - (int)(((precio - precioMin) / (precioMax - precioMin)) * height);

                Point punto = new Point(x, y);

                if (puntoAnterior.HasValue)
                {
                    g.DrawLine(curvePen, puntoAnterior.Value, punto);
                }

                puntoAnterior = punto;
            }

            double margenActual = cotizacion.MargenObjetivo;

            if (cotizacion.PrecioVentaEvaluado > 0.0)
            {
                margenActual =
                    (cotizacion.PrecioVentaEvaluado - cotizacion.CostoTotal) /
                    cotizacion.PrecioVentaEvaluado;
            }

            double margenGrafico = margenActual;

            if (margenGrafico < margenMin)
            {
                margenGrafico = margenMin;
            }

            if (margenGrafico > margenMax)
            {
                margenGrafico = margenMax;
            }

            int xSeleccionado = left + (int)(((margenGrafico - margenMin) / (margenMax - margenMin)) * width);

            double precioSeleccionado = CalcularPrecioPorMargen(costoTotal, margenGrafico);

            if (cotizacion.PrecioVentaEvaluado > 0.0)
            {
                precioSeleccionado = cotizacion.PrecioVentaEvaluado;
            }

            int ySeleccionado = top + height - (int)(((precioSeleccionado - precioMin) / (precioMax - precioMin)) * height);

            g.DrawLine(selectedPen, xSeleccionado, top, xSeleccionado, top + height);

            using (Brush puntoBrush = new SolidBrush(Color.FromArgb(238, 30, 91)))
            {
                g.FillEllipse(puntoBrush, xSeleccionado - 5, ySeleccionado - 5, 10, 10);
            }

            double utilidad = precioSeleccionado - costoTotal;

            string etiqueta =
                $"{margenActual * 100.0:0.##}% | " +
                $"{cotizacion.Moneda} {precioSeleccionado:N0} | " +
                $"Utilidad {utilidad:N0}";

            int etiquetaX = xSeleccionado + 10;

            if (etiquetaX > left + width - 260)
            {
                etiquetaX = xSeleccionado - 260;
            }

            using (Brush etiquetaBrush = new SolidBrush(Color.FromArgb(250, 250, 250)))
            {
                g.FillRectangle(etiquetaBrush, etiquetaX - 4, ySeleccionado - 28, 255, 24);
            }

            g.DrawRectangle(Pens.LightGray, etiquetaX - 4, ySeleccionado - 28, 255, 24);
            g.DrawString(etiqueta, font, textBrush, etiquetaX, ySeleccionado - 24);

            g.DrawString("Margen", font, textBrush, left + width - 55, top + height + 30);
            g.DrawString("Precio", font, textBrush, 15, top - 25);

            axisPen.Dispose();
            gridPen.Dispose();
            curvePen.Dispose();
            selectedPen.Dispose();
        }

        private void PanelGantt_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Color fondoGantt = modoOscuroActivo ? Color.FromArgb(37, 37, 38) : Color.White;
            Color textoGantt = modoOscuroActivo ? Color.FromArgb(220, 220, 220) : Color.Black;

            g.Clear(fondoGantt);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            ganttHitBoxes.Clear();

            if (panelGantt == null)
            {
                return;
            }

            Point scroll = panelGantt.AutoScrollPosition;
            g.TranslateTransform(scroll.X, scroll.Y);

            using Font font = new Font("Segoe UI", 9);
            using Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using Font smallFont = new Font("Segoe UI", 8, FontStyle.Bold);
            using Font subFont = new Font("Segoe UI", 8, FontStyle.Regular);
            using Font subFontBold = new Font("Segoe UI", 8, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(textoGantt);

            if (tabs != null &&
                tabs.SelectedTab == tabCatalogoProductosServiciosPrincipal &&
                productoCatalogoVistaPrevia != null)
            {
                DibujarVistaPreviaPipelineCatalogo(g, productoCatalogoVistaPrevia, textoGantt);
                return;
            }

            int marginLeft = 125;
            int marginTop = 45;
            int rowHeight = 42;
            int subRowHeight = 24;
            int barHeight = 24;

            g.DrawString("Gantt de etapas", titleFont, textBrush, 15, 15);

            int usableWidth = panelGantt.ClientSize.Width - marginLeft - 30;

            if (usableWidth <= 20)
            {
                return;
            }

            string[] ordenEtapas = new string[]
            {
        "Desarrollo",
        "Preproduccion",
        "Produccion",
        "Postproduccion"
            };

            double duracionTotal = CalcularDuracionTotalGanttVisual(ordenEtapas);

            if (duracionTotal <= 0.0)
            {
                duracionTotal = 4.0;
            }

            int yActual = marginTop;

            foreach (string nombreBase in ordenEtapas)
            {
                EtapaProyecto etapa = BuscarEtapaGantt(nombreBase);
                string nombreVisible = ObtenerNombreVisibleEtapaGantt(nombreBase, etapa);

                bool trabajada =
                    etapa != null &&
                    etapa.Seleccionada &&
                    etapa.DuracionMeses > 0.0;

                bool expandida =
                    etapa != null &&
                    etapaExpandidaEnTabla != null &&
                    NormalizarNombreEtapa(etapaExpandidaEnTabla.Nombre) == NormalizarNombreEtapa(etapa.Nombre);

                Color colorBarraBase = ObtenerColorBarraEtapa(nombreVisible);
                Color colorBordeBase = ObtenerColorBordeEtapa(nombreVisible);
                Color colorFilaBase = ObtenerColorFilaEtapa(nombreVisible);

                AplicarColoresSecretosGantt(
                    nombreVisible,
                    ref colorBarraBase,
                    ref colorBordeBase,
                    ref colorFilaBase
                );

                Color colorFilaTrabajada = MezclarConBlanco(colorFilaBase, 0.46);
                Color colorBarraTrabajada = MezclarConBlanco(colorBarraBase, 0.08);
                Color colorBordeTrabajada = MezclarConBlanco(colorBordeBase, 0.02);

                Color colorTextoEtapa = trabajada
                    ? (modoOscuroActivo ? Color.FromArgb(219, 222, 225) : Color.FromArgb(55, 55, 55))
                    : colorBordeBase;

                Rectangle hitEtapa = new Rectangle(
                    0,
                    yActual - 6,
                    panelGantt.ClientSize.Width,
                    rowHeight
                );

                ganttHitBoxes.Add(new GanttHitBox
                {
                    Rect = hitEtapa,
                    Etapa = etapa,
                    SubEtapa = null,
                    TipoAccion = "Etapa"
                });

                using Brush labelBrush = new SolidBrush(colorTextoEtapa);

                string indicador = expandida ? "▾ " : "▸ ";
                g.DrawString(indicador + nombreVisible, font, labelBrush, 10, yActual + 3);

                if (trabajada)
                {
                    Rectangle fondoFila = new Rectangle(
                        marginLeft,
                        yActual - 2,
                        usableWidth,
                        barHeight + 4
                    );

                    using Brush filaBrush = new SolidBrush(colorFilaTrabajada);
                    using Pen filaBorderPen = new Pen(colorBordeTrabajada, 1.1f);

                    g.FillRectangle(filaBrush, fondoFila);
                    g.DrawRectangle(filaBorderPen, fondoFila);

                    double inicio = etapa.InicioMes;
                    double duracion = etapa.DuracionMeses;

                    if (inicio < 0.0)
                    {
                        inicio = 0.0;
                    }

                    if (duracion <= 0.0)
                    {
                        duracion = 1.0;
                    }

                    int x = marginLeft + (int)((inicio / duracionTotal) * usableWidth);
                    int width = (int)((duracion / duracionTotal) * usableWidth);

                    if (width < 8)
                    {
                        width = 8;
                    }

                    if (x < marginLeft)
                    {
                        x = marginLeft;
                    }

                    if (x + width > marginLeft + usableWidth)
                    {
                        width = marginLeft + usableWidth - x;
                    }

                    Rectangle rectTrabajado = new Rectangle(
                        x,
                        yActual,
                        Math.Max(width, 8),
                        barHeight
                    );

                    using Brush barBrush = new SolidBrush(colorBarraTrabajada);
                    using Pen borderPen = new Pen(colorBordeTrabajada, 1.6f);
                    using Brush labelTextoBrush = new SolidBrush(Color.FromArgb(25, 25, 25));

                    g.FillRectangle(barBrush, rectTrabajado);
                    g.DrawRectangle(borderPen, rectTrabajado);

                    string label = $"{etapa.InicioMes:0.##} - {etapa.FinMes:0.##}";
                    g.DrawString(label, font, labelTextoBrush, x + 5, yActual + 3);
                }
                else
                {
                    Color colorFondoPendiente = modoOscuroActivo
                        ? Color.FromArgb(30, 30, 30)
                        : MezclarConBlanco(colorFilaBase, 0.96);

                    Rectangle fondoFila = new Rectangle(
                        marginLeft,
                        yActual - 2,
                        usableWidth,
                        barHeight + 4
                    );

                    using Brush fondoBrush = new SolidBrush(colorFondoPendiente);
                    using Pen fondoPen = new Pen(modoOscuroActivo ? Color.FromArgb(63, 63, 70) : colorFondoPendiente, 1.0f);
                    using Brush textoPendienteBrush = new SolidBrush(colorBordeBase);

                    g.FillRectangle(fondoBrush, fondoFila);
                    g.DrawRectangle(fondoPen, fondoFila);

                    g.DrawString(
                        "PENDIENTE / SIN DEFINIR",
                        smallFont,
                        textoPendienteBrush,
                        marginLeft + 8,
                        yActual + 5
                    );
                }

                yActual += rowHeight;

                if (expandida && etapa != null && etapa.Seleccionada)
                {
                    yActual = DibujarSubEtapasGantt(
                        g,
                        etapa,
                        marginLeft,
                        yActual,
                        usableWidth,
                        subRowHeight,
                        colorFilaBase,
                        colorBordeBase,
                        subFont,
                        subFontBold
                    );
                }
            }

            int altoContenido = yActual + 30;

            if (panelGantt.AutoScrollMinSize.Height != altoContenido)
            {
                panelGantt.AutoScrollMinSize = new Size(
                    Math.Max(panelGantt.ClientSize.Width, marginLeft + usableWidth + 30),
                    altoContenido
                );
            }
        }

        private void DibujarVistaPreviaPipelineCatalogo(
            Graphics g,
            Producto2DDefinicion producto,
            Color textoGantt
        )
        {
            CatalogoProductoPreview preview = previewCatalogoActual ??
                CatalogoProductoPreviewService.Calcular(producto, cotizacion);

            using Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold);
            using Font subtitleFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            using Font font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            using Font smallFont = new Font("Segoe UI", 8.0f, FontStyle.Regular);
            using Brush textBrush = new SolidBrush(textoGantt);
            using Brush secondaryBrush = new SolidBrush(modoOscuroActivo ? Color.FromArgb(190, 190, 190) : Color.FromArgb(92, 92, 92));

            int x = 15;
            int y = 15;
            int ancho = Math.Max(80, panelGantt.ClientSize.Width - 30);

            g.DrawString("Vista previa productiva", titleFont, textBrush, x, y);
            y += 24;
            g.DrawString(
                "Los plazos se calcularan al crear o cargar un proyecto.",
                subtitleFont,
                secondaryBrush,
                new RectangleF(x, y, ancho, 34)
            );
            y += 44;

            foreach (CatalogoEtapaPreview etapa in preview.Etapas)
            {
                Color color = ObtenerColorBarraEtapa(etapa.Etapa);
                Color borde = ObtenerColorBordeEtapa(etapa.Etapa);
                Color fondo = modoOscuroActivo
                    ? Color.FromArgb(45, 45, 48)
                    : MezclarConBlanco(color, 0.90);

                Rectangle rect = new Rectangle(x, y, ancho, 58);
                using Brush fondoBrush = new SolidBrush(fondo);
                using Pen pen = new Pen(borde, 1.2f);
                g.FillRectangle(fondoBrush, rect);
                g.DrawRectangle(pen, rect);

                using Brush colorBrush = new SolidBrush(color);
                g.FillRectangle(colorBrush, x, y, 5, 58);

                string texto = etapa.Etapa + " - " + etapa.CantidadProcesos + " procesos";
                g.DrawString(texto, font, textBrush, x + 14, y + 8);
                string detalle = etapa.CantidadProcesos == 0
                    ? "Sin procesos configurados"
                    : etapa.Horas.ToString("0.##") + " h | " +
                      MonedaService.FormatearMoneda(cotizacion, etapa.CostoTotalCLP) +
                      " | " + etapa.Estado;
                g.DrawString(detalle, smallFont, secondaryBrush, x + 14, y + 31);

                y += 66;
            }

            if (preview.Etapas.All(e => e.CantidadProcesos == 0))
            {
                g.DrawString(
                    "No existen procesos configurados para este producto.",
                    smallFont,
                    secondaryBrush,
                    new RectangleF(x, y, ancho, 40)
                );
                y += 44;
            }

            int altoContenido = y + 20;
            if (panelGantt.AutoScrollMinSize.Height != altoContenido)
            {
                panelGantt.AutoScrollMinSize = new Size(panelGantt.ClientSize.Width, altoContenido);
            }
        }

        private int DibujarSubEtapasGantt(
            Graphics g,
            EtapaProyecto etapa,
            int marginLeft,
            int yInicial,
            int usableWidth,
            int subRowHeight,
            Color colorFilaBase,
            Color colorBordeBase,
            Font subFont,
            Font subFontBold
        )
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                return yInicial;
            }

            var subEtapas = bibliotecaSubEtapas
                .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .OrderBy(s => s.Orden)
                .ToList();

            if (subEtapas.Count == 0)
            {
                return yInicial;
            }

            int y = yInicial;

            foreach (SubEtapaProyecto sub in subEtapas)
            {
                Color fondo = sub.Activa
                    ? MezclarConBlanco(colorFilaBase, 0.72)
                    : Color.FromArgb(248, 248, 248);

                Color texto = sub.Activa
                    ? Color.FromArgb(45, 45, 45)
                    : Color.FromArgb(145, 145, 145);

                Color estadoColor = sub.Activa
                    ? colorBordeBase
                    : Color.FromArgb(155, 155, 155);

                Rectangle rectFila = new Rectangle(
                    marginLeft,
                    y - 2,
                    usableWidth,
                    subRowHeight
                );

                Rectangle rectNombre = new Rectangle(
                    marginLeft,
                    y - 2,
                    usableWidth / 2,
                    subRowHeight
                );

                Rectangle rectEstado = new Rectangle(
                    marginLeft + usableWidth / 2,
                    y - 2,
                    usableWidth / 2,
                    subRowHeight
                );

                ganttHitBoxes.Add(new GanttHitBox
                {
                    Rect = rectNombre,
                    Etapa = etapa,
                    SubEtapa = sub,
                    TipoAccion = "SubEtapaNombre"
                });

                ganttHitBoxes.Add(new GanttHitBox
                {
                    Rect = rectEstado,
                    Etapa = etapa,
                    SubEtapa = sub,
                    TipoAccion = "SubEtapaEstado"
                });

                using Brush fondoBrush = new SolidBrush(fondo);
                using Brush textoBrush = new SolidBrush(texto);
                using Brush estadoBrush = new SolidBrush(estadoColor);

                g.FillRectangle(fondoBrush, rectFila);

                string marca;

                if (!sub.Activa)
                {
                    marca = "□";
                }
                else
                {
                    marca = sub.Requerida ? "●" : "○";
                }

                string estado;

                if (!sub.Activa)
                {
                    estado = "Subproceso excluido";
                }
                else
                {
                    estado = sub.Requerida
                        ? "Objetivo requerido"
                        : "Objetivo opcional";
                }

                g.DrawString(
                    $"↳ {marca} {sub.Nombre}",
                    sub.Activa ? subFontBold : subFont,
                    textoBrush,
                    marginLeft + 18,
                    y + 2
                );

                g.DrawString(
                    estado,
                    sub.Activa && sub.Requerida ? subFontBold : subFont,
                    estadoBrush,
                    marginLeft + usableWidth / 2 + 8,
                    y + 2
                );

                y += subRowHeight;
            }

            return y + 6;
        }

        private void ConectarEventosGanttInteractivo()
        {
            if (panelGantt == null)
            {
                return;
            }

            panelGantt.AutoScroll = true;
            panelGantt.MouseClick -= PanelGantt_MouseClick;
            panelGantt.MouseClick += PanelGantt_MouseClick;
            panelGantt.Cursor = Cursors.Hand;
        }

        private void PanelGantt_MouseClick(object sender, MouseEventArgs e)
        {
            if (ganttHitBoxes == null || ganttHitBoxes.Count == 0)
            {
                return;
            }

            Point puntoContenido = new Point(
                e.X - panelGantt.AutoScrollPosition.X,
                e.Y - panelGantt.AutoScrollPosition.Y
            );

            GanttHitBox hit = ganttHitBoxes
                .LastOrDefault(h => h.Rect.Contains(puntoContenido));

            if (hit == null)
            {
                return;
            }

            if (hit.TipoAccion == "Etapa" && hit.Etapa != null)
            {
                ActivarYExpandirEtapaDesdeGantt(hit.Etapa);
                return;
            }

            if (hit.SubEtapa != null)
            {
                ManejarClickSubEtapaDesdeGantt(hit.SubEtapa, hit.TipoAccion);
            }
        }

        private void ActivarYExpandirEtapaDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            /*
             * Lógica correcta:
             *
             * U existe si y solo si tiene al menos un u_i activo.
             *
             * Si la etapa está inactiva y se clickea desde la Gantt:
             * - se activa U;
             * - se activan TODAS sus subetapas como opcionales recomendadas.
             *
             * Si la etapa ya está activa:
             * - el click solo expande/contrae visualmente.
             */

            if (!etapa.Seleccionada)
            {
                ActivarEtapaConSubEtapasOpcionalesDesdeGantt(etapa);
            }

            if (etapaExpandidaEnTabla != null &&
                NormalizarNombreEtapa(etapaExpandidaEnTabla.Nombre) == NormalizarNombreEtapa(etapa.Nombre))
            {
                etapaExpandidaEnTabla = null;
            }
            else
            {
                etapaExpandidaEnTabla = etapa;
            }

            ValidarPrecedenciaTemporalEtapasDesdeGantt();
            RefrescarDespuesDeEditarEtapas();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
                panelGantt.Refresh();
            }
        }

        private void ManejarClickSubEtapaDesdeGantt(SubEtapaProyecto sub, string tipoAccion)
        {
            if (sub == null)
            {
                return;
            }

            if (tipoAccion == "SubEtapaNombre")
            {
                /*
                 * Click en el nombre:
                 * - incluye/excluye la subetapa.
                 * - si se incluye, entra como opcional recomendada.
                 */

                sub.Activa = !sub.Activa;

                if (sub.Activa)
                {
                    sub.Requerida = false;
                }
                else
                {
                    sub.Requerida = false;
                }
            }
            else if (tipoAccion == "SubEtapaEstado")
            {
                /*
                 * Click en estado:
                 * - si estaba excluida, se activa como requerida.
                 * - si estaba activa, alterna requerido/opcional.
                 */

                if (!sub.Activa)
                {
                    sub.Activa = true;
                    sub.Requerida = true;
                }
                else
                {
                    sub.Requerida = !sub.Requerida;
                }
            }

            EtapaProyecto etapaPadre = BuscarEtapaGantt(sub.EtapaPadre);

            if (etapaPadre != null)
            {
                ActualizarEtapaPadreDesdeGantt(etapaPadre);
                etapaExpandidaEnTabla = etapaPadre;
            }

            ValidarPrecedenciaTemporalEtapasDesdeGantt();
            RefrescarDespuesDeEditarEtapas();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
                panelGantt.Refresh();
            }
        }

        private void ActivarEtapaConSubEtapasOpcionalesDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (bibliotecaSubEtapas != null)
            {
                var subEtapasDeLaEtapa = bibliotecaSubEtapas
                    .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .OrderBy(s => s.Orden)
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
                {
                    sub.Activa = true;
                    sub.Requerida = false;

                    if (sub.InicioSemana < 1)
                    {
                        sub.InicioSemana = 1;
                    }

                    if (sub.DuracionSemanas < 1)
                    {
                        sub.DuracionSemanas = 1;
                    }
                }
            }

            etapa.Seleccionada = TieneSubEtapasActivasDesdeGantt(etapa);

            if (!etapa.Seleccionada)
            {
                DesactivarSoloEtapaDesdeGantt(etapa);
                return;
            }

            if (etapa.DuracionMeses <= 0.0)
            {
                etapa.DuracionMeses = 1.0;
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

            ExpandirEtapaParaContenerSubEtapasDesdeGantt(etapa);
        }

        private void ActualizarEtapaPadreDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (!TieneSubEtapasActivasDesdeGantt(etapa))
            {
                DesactivarSoloEtapaDesdeGantt(etapa);
                return;
            }

            etapa.Seleccionada = true;

            if (etapa.DuracionMeses <= 0.0)
            {
                etapa.DuracionMeses = 1.0;
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

            ExpandirEtapaParaContenerSubEtapasDesdeGantt(etapa);
        }

        private bool TieneSubEtapasActivasDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return false;
            }

            return bibliotecaSubEtapas.Any(s =>
                NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre) &&
                s.Activa
            );
        }

        private void DesactivarSoloEtapaDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            etapa.Seleccionada = false;
            etapa.DuracionMeses = 0.0;
            etapa.InicioMes = 0.0;
            etapa.FinMes = 0.0;

            if (etapa.Plan != null)
            {
                etapa.Plan.Clear();
            }
        }

        private void ExpandirEtapaParaContenerSubEtapasDesdeGantt(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return;
            }

            List<SubEtapaProyecto> subEtapasActivas = bibliotecaSubEtapas
                .Where(s =>
                    s != null &&
                    NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .Where(s => s.Activa)
                .ToList();

            if (subEtapasActivas.Count == 0)
            {
                DesactivarSoloEtapaDesdeGantt(etapa);
                return;
            }

            double menorInicioSemana = double.MaxValue;
            double mayorFinSemana = 1.0;

            foreach (SubEtapaProyecto sub in subEtapasActivas)
            {
                if (sub.InicioSemana < 1.0)
                {
                    sub.InicioSemana = 1.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }

                if (sub.InicioSemana < menorInicioSemana)
                {
                    menorInicioSemana = sub.InicioSemana;
                }

                if (sub.FinSemana > mayorFinSemana)
                {
                    mayorFinSemana = sub.FinSemana;
                }
            }

            if (menorInicioSemana == double.MaxValue)
            {
                menorInicioSemana = 1.0;
            }

            /*
             * Las subetapas ahora trabajan con semanas decimales.
             * La etapa debe contener el rango real de sus subetapas.
             * El Gantt redondea solo al dibujar.
             */
            double inicioMesNecesario =
                (menorInicioSemana - 1.0) / 4.0;

            double finMesNecesario =
                mayorFinSemana / 4.0;

            double duracionMesesNecesaria =
                finMesNecesario - inicioMesNecesario;

            if (duracionMesesNecesaria <= 0.0)
            {
                duracionMesesNecesaria = 0.25;
            }

            etapa.Seleccionada = true;
            etapa.InicioMes = inicioMesNecesario;
            etapa.DuracionMeses = duracionMesesNecesaria;
            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

            if (etapa.FinMes <= etapa.InicioMes)
            {
                etapa.FinMes = etapa.InicioMes + 0.25;
                etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;
            }
        }

        private void ValidarPrecedenciaTemporalEtapasDesdeGantt()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            /*
             * La precedencia solo se valida entre etapas activas.
             * Si Desarrollo no corre porque el cliente lo entrega,
             * Preproducción puede partir desde 0.
             */

            EtapaProyecto etapaActivaAnterior = null;

            foreach (EtapaProyecto etapa in cotizacion.Etapas
                .OrderBy(e => ObtenerOrdenEtapaParaGantt(e.Nombre)))
            {
                if (etapa == null)
                {
                    continue;
                }

                if (!etapa.Seleccionada)
                {
                    continue;
                }

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

                if (etapaActivaAnterior != null)
                {
                    if (etapa.InicioMes < etapaActivaAnterior.InicioMes)
                    {
                        etapa.InicioMes = etapaActivaAnterior.InicioMes;
                        etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                    }

                    if (etapa.FinMes < etapaActivaAnterior.FinMes)
                    {
                        etapa.FinMes = etapaActivaAnterior.FinMes;
                        etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;

                        if (etapa.DuracionMeses < 1.0)
                        {
                            etapa.DuracionMeses = 1.0;
                            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                        }
                    }
                }

                etapaActivaAnterior = etapa;
            }
        }

        private int ObtenerOrdenEtapaParaGantt(string nombreEtapa)
        {
            return ObtenerOrdenEtapaGeneral(nombreEtapa);
        }

        private EtapaProyecto BuscarEtapaGantt(string nombreBase)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return null;
            }

            string normalizadoBase = NormalizarNombreEtapaGantt(nombreBase);

            return cotizacion.Etapas.FirstOrDefault(etapa =>
                NormalizarNombreEtapaGantt(etapa.Nombre) == normalizadoBase
            );
        }

        private string ObtenerNombreVisibleEtapaGantt(string nombreBase, EtapaProyecto etapa)
        {
            if (etapa != null && !string.IsNullOrWhiteSpace(etapa.Nombre))
            {
                return etapa.Nombre;
            }

            switch (NormalizarNombreEtapaGantt(nombreBase))
            {
                case "desarrollo":
                    return "Desarrollo";

                case "preproduccion":
                    return "Preproduccion";

                case "produccion":
                    return "Produccion";

                case "postproduccion":
                    return "Postproduccion";

                default:
                    return nombreBase;
            }
        }

        private string NormalizarNombreEtapaGantt(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return string.Empty;
            }

            return nombre
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace(" ", "");
        }

        private double CalcularDuracionTotalGanttVisual(string[] ordenEtapas)
        {
            double mayorFin = 0.0;

            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return 4.0;
            }

            foreach (string nombreBase in ordenEtapas)
            {
                EtapaProyecto etapa = BuscarEtapaGantt(nombreBase);

                if (etapa == null)
                {
                    continue;
                }

                if (!etapa.Seleccionada)
                {
                    continue;
                }

                if (etapa.DuracionMeses <= 0.0)
                {
                    continue;
                }

                if (etapa.FinMes > mayorFin)
                {
                    mayorFin = etapa.FinMes;
                }
            }

            if (mayorFin <= 0.0)
            {
                return 4.0;
            }

            return mayorFin;
        }
    }
}
