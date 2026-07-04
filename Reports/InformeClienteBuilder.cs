using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Reports
{
    public static class InformeClienteBuilder
    {
        public static string GenerarHtml(Cotizacion cotizacion)
        {
            if (cotizacion == null)
            {
                return GenerarHtmlVacio();
            }

            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='es'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Informe cliente Othalart</title>");
            html.AppendLine(ConstruirCss());
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='page'>");

            html.AppendLine(ConstruirHero(cotizacion));
            html.AppendLine("<div class='content'>");

            html.AppendLine(ConstruirResumenEjecutivo(cotizacion));
            html.AppendLine(ConstruirDatosClienteProyecto(cotizacion));
            html.AppendLine(ConstruirEntregablesAcordados(cotizacion));
            html.AppendLine(ConstruirEvaluacionComercial(cotizacion));
            html.AppendLine(ConstruirDistribucionCostos(cotizacion));
            html.AppendLine(ConstruirHerramientasSoftware(cotizacion));
            html.AppendLine(ConstruirEquipoNecesario(cotizacion));
            html.AppendLine(ConstruirPlanificacion(cotizacion));
            html.AppendLine(ConstruirDesgloseProductivo(cotizacion));
            html.AppendLine(ConstruirSupuestos(cotizacion));

            html.AppendLine("<div class='footer'>");
            html.AppendLine("Informe generado por Cotizador Othalart. Documento preliminar sujeto a validación de alcance, estilo, complejidad y disponibilidad de equipo.");
            html.AppendLine("</div>");

            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private static string ConstruirHero(Cotizacion c)
        {
            string cliente = Texto(c, "Cliente", "NombreCliente", "ClienteNombre");
            string proyecto = Texto(c, "Proyecto", "NombreProyecto", "TituloProyecto");
            string empresa = Texto(c, "Empresa", "EmpresaMarca", "Marca", "EmpresaCliente");

            if (string.IsNullOrWhiteSpace(cliente))
            {
                cliente = "Cliente no informado";
            }

            if (string.IsNullOrWhiteSpace(proyecto))
            {
                proyecto = "Proyecto de animación 2D";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<div class='hero'>");
            sb.AppendLine("<div class='kicker'>Othalart / Informe preliminar cliente</div>");
            sb.AppendLine("<h1>" + H(proyecto) + "</h1>");
            sb.AppendLine("<p>");
            sb.AppendLine("Informe preliminar de alcance, producción, planificación, costos y propuesta comercial para ");
            sb.AppendLine("<strong>" + H(cliente) + "</strong>");
            if (!string.IsNullOrWhiteSpace(empresa))
            {
                sb.AppendLine(" / <strong>" + H(empresa) + "</strong>");
            }
            sb.AppendLine(".</p>");
            sb.AppendLine("</div>");

            return sb.ToString();
        }

        private static string ConstruirResumenEjecutivo(Cotizacion c)
        {
            double costoTotal = Numero(c, "CostoTotal", "CostoTotalCLP", "CostoInternoTotalCLP");
            double precio = Numero(c, "PrecioVentaEvaluado", "PrecioEvaluadoCLP", "PrecioVentaCLP", "PrecioFinalCLP");
            double utilidad = Numero(c, "UtilidadEvaluada", "UtilidadCLP");
            double margen = Numero(c, "MargenEvaluado", "Margen", "MargenFinal");
            double plazo = Numero(c, "DuracionPlanificadaSemanas", "PlazoClienteSemanas", "DuracionTotalSemanas");
            double presupuesto = Numero(c, "PresupuestoClienteCLP", "PresupuestoCliente", "PrecioClienteCLP");

            object desglose = Valor(c, "DesgloseProductivo");
            if (desglose != null)
            {
                if (plazo <= 0.0)
                {
                    plazo = Numero(desglose, "SemanasEstandar", "SemanasHolgura", "SemanasMinimas");
                }

                if (costoTotal <= 0.0)
                {
                    costoTotal = Numero(desglose, "CostoEstandarCLP", "CostoHolguraCLP", "CostoMinimoCLP");
                }
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<div class='grid cards-top'>");
            sb.AppendLine(Card("Costo interno estimado", FormatoCLP(costoTotal), "Base productiva preliminar."));
            sb.AppendLine(Card("Precio sugerido", FormatoCLP(precio), "Precio evaluado para cliente."));
            sb.AppendLine(Card("Plazo técnico", plazo > 0 ? plazo.ToString("0.##") + " semanas" : "No informado", "Planificación preliminar."));
            sb.AppendLine(Card("Presupuesto cliente", presupuesto > 0 ? FormatoCLP(presupuesto) : "No informado", "Referencia declarada por cliente."));
            sb.AppendLine(Card("Utilidad estimada", FormatoCLP(utilidad), "Antes de ajustes finales."));
            sb.AppendLine(Card("Margen evaluado", FormatoPorcentaje(margen), "Margen sobre precio evaluado."));
            sb.AppendLine("</div>");

            string diagnostico = Texto(c, "DiagnosticoPlazo", "DiagnosticoComercial", "Diagnostico");
            if (string.IsNullOrWhiteSpace(diagnostico) && desglose != null)
            {
                diagnostico = Texto(desglose, "Diagnostico", "DiagnosticoProductivo");
            }

            if (!string.IsNullOrWhiteSpace(diagnostico))
            {
                sb.AppendLine("<div class='callout'>");
                sb.AppendLine("<strong>Lectura preliminar:</strong> " + H(diagnostico));
                sb.AppendLine("</div>");
            }

            return sb.ToString();
        }

        private static string ConstruirDatosClienteProyecto(Cotizacion c)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>1. Datos del cliente y proyecto</h2>");
            sb.AppendLine("<table class='table'>");

            FilaDato(sb, "Cliente", Texto(c, "Cliente", "NombreCliente", "ClienteNombre"));
            FilaDato(sb, "Empresa / marca", Texto(c, "Empresa", "EmpresaMarca", "Marca", "EmpresaCliente"));
            FilaDato(sb, "Email", Texto(c, "Email", "Correo", "EmailCliente"));
            FilaDato(sb, "Proyecto", Texto(c, "Proyecto", "NombreProyecto", "TituloProyecto"));
            FilaDato(sb, "Industria", Texto(c, "Industria", "IndustriaCliente"));
            FilaDato(sb, "Destino / uso", Texto(c, "Destino", "DestinoUso", "UsoPrincipal"));
            FilaDato(sb, "Formato entrega", Texto(c, "FormatoEntrega", "Formato"));
            FilaDato(sb, "Resolución", Texto(c, "Resolucion"));
            FilaDato(sb, "Relación aspecto", Texto(c, "RelacionAspecto", "AspectRatio"));
            FilaDato(sb, "Descripción", Texto(c, "Descripcion", "DescripcionProyecto"));
            FilaDato(sb, "Notas internas", Texto(c, "NotasInternas", "Notas"));

            sb.AppendLine("</table>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirEntregablesAcordados(Cotizacion c)
        {
            List<object> requerimientos = ObtenerRequerimientosDesglose(c);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>2. Entregables y alcance acordado</h2>");

            if (requerimientos.Count == 0)
            {
                sb.AppendLine("<p class='muted'>No hay entregables productivos generados todavía.</p>");
                sb.AppendLine("</section>");
                return sb.ToString();
            }

            var grupos = requerimientos
                .GroupBy(r => Texto(r, "EntregableCliente", "Entregable", "ProductoCliente"))
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var grupo in grupos)
            {
                string nombreGrupo = string.IsNullOrWhiteSpace(grupo.Key)
                    ? "Entregable sin nombre"
                    : grupo.Key;

                sb.AppendLine("<div class='subcard'>");
                sb.AppendLine("<h3>" + H(nombreGrupo) + "</h3>");
                sb.AppendLine("<table class='table'>");
                sb.AppendLine("<tr><th>Requerimiento</th><th>Cantidad</th><th>Unidad</th><th>Etapa</th><th>Cargo sugerido</th></tr>");

                foreach (object req in grupo)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>" + H(Texto(req, "NombreRequerimiento", "Requerimiento", "TipoInterno")) + "</td>");
                    sb.AppendLine("<td>" + Numero(req, "Cantidad").ToString("0.##") + "</td>");
                    sb.AppendLine("<td>" + H(Texto(req, "Unidad")) + "</td>");
                    sb.AppendLine("<td>" + BadgeEtapa(Texto(req, "EtapaSugerida", "Etapa")) + "</td>");
                    sb.AppendLine("<td>" + H(Texto(req, "CargoSugerido", "Cargo")) + "</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</table>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirEvaluacionComercial(Cotizacion c)
        {
            double costo = Numero(c, "CostoTotal", "CostoTotalCLP", "CostoInternoTotalCLP");
            double utilidad = Numero(c, "UtilidadEvaluada", "UtilidadCLP");
            double precio = Numero(c, "PrecioVentaEvaluado", "PrecioEvaluadoCLP", "PrecioVentaCLP", "PrecioFinalCLP");
            double margen = Numero(c, "MargenEvaluado", "Margen", "MargenFinal");
            double markup = Numero(c, "MarkupEvaluado", "Markup");
            double presupuesto = Numero(c, "PresupuestoClienteCLP", "PresupuestoCliente", "PrecioClienteCLP");

            object desglose = Valor(c, "DesgloseProductivo");
            if (desglose != null && costo <= 0.0)
            {
                costo = Numero(desglose, "CostoEstandarCLP", "CostoMinimoCLP", "CostoHolguraCLP");
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>3. Evaluación comercial</h2>");
            sb.AppendLine("<div class='grid two'>");

            sb.AppendLine("<div class='card'>");
            sb.AppendLine("<h3>Precio y margen</h3>");
            sb.AppendLine("<table class='table compact'>");
            FilaDato(sb, "Costo interno estimado", FormatoCLP(costo));
            FilaDato(sb, "Utilidad estimada", FormatoCLP(utilidad));
            FilaDato(sb, "Precio sugerido", FormatoCLP(precio));
            FilaDato(sb, "Margen evaluado", FormatoPorcentaje(margen));
            FilaDato(sb, "Markup evaluado", FormatoPorcentaje(markup));
            FilaDato(sb, "Presupuesto cliente", presupuesto > 0 ? FormatoCLP(presupuesto) : "No informado");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='card'>");
            sb.AppendLine("<h3>Interpretación</h3>");
            sb.AppendLine("<p class='muted'>La propuesta comercial se calcula desde la carga productiva, costos internos, costos adicionales, margen base y presupuesto declarado por el cliente cuando existe.</p>");

            if (presupuesto > 0.0 && precio > 0.0)
            {
                if (precio <= presupuesto)
                {
                    sb.AppendLine("<div class='status ok'>El precio sugerido cabe dentro del presupuesto declarado.</div>");
                }
                else
                {
                    sb.AppendLine("<div class='status warn'>El precio sugerido supera el presupuesto declarado. Se recomienda ajustar alcance, calidad, plazo o margen.</div>");
                }
            }
            else
            {
                sb.AppendLine("<div class='status neutral'>No existe presupuesto cliente suficiente para comparar la propuesta.</div>");
            }

            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirDistribucionCostos(Cotizacion c)
        {
            StringBuilder sb = new StringBuilder();

            double manoObra = Numero(c, "CostoManoObraCLP", "CostoLaboralCLP", "CostoProduccionCLP");
            double costosExtra = Numero(c, "CostoExtraCLP", "CostosExtraCLP", "TotalCostosExtraCLP");
            double admin = Numero(c, "CostoAdministrativoCLP", "CostosAdministrativosCLP");
            double total = Numero(c, "CostoTotal", "CostoTotalCLP", "CostoInternoTotalCLP");

            object desglose = Valor(c, "DesgloseProductivo");
            if (desglose != null)
            {
                if (total <= 0.0)
                {
                    total = Numero(desglose, "CostoEstandarCLP", "CostoMinimoCLP", "CostoHolguraCLP");
                }
            }

            if (manoObra <= 0.0)
            {
                manoObra = SumarCostosManoObraDesdeDesglose(c);
            }

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>4. Distribución de costos</h2>");

            sb.AppendLine("<div class='grid three'>");
            sb.AppendLine(Card("Mano de obra / producción", FormatoCLP(manoObra), PorcentajeSobreTotal(manoObra, total)));
            sb.AppendLine(Card("Costos extra / herramientas", FormatoCLP(costosExtra), PorcentajeSobreTotal(costosExtra, total)));
            sb.AppendLine(Card("Administración / gestión", FormatoCLP(admin), PorcentajeSobreTotal(admin, total)));
            sb.AppendLine("</div>");

            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Concepto</th><th>Monto estimado</th><th>Peso relativo</th></tr>");
            FilaCosto(sb, "Mano de obra / producción", manoObra, total);
            FilaCosto(sb, "Costos extra / herramientas", costosExtra, total);
            FilaCosto(sb, "Administración / gestión", admin, total);
            FilaCosto(sb, "Total costo interno", total, total);
            sb.AppendLine("</table>");

            sb.AppendLine(ConstruirTablaCostosExtra(c));

            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirHerramientasSoftware(Cotizacion c)
        {
            List<object> costos = ObtenerListaCostos(c);

            List<object> software = costos
                .Where(x =>
                {
                    string texto = (
                        Texto(x, "Nombre", "Concepto", "Descripcion", "Categoria") + " " +
                        Texto(x, "Tipo", "Frecuencia")
                    ).ToLowerInvariant();

                    return texto.Contains("adobe") ||
                           texto.Contains("software") ||
                           texto.Contains("licencia") ||
                           texto.Contains("photoshop") ||
                           texto.Contains("after") ||
                           texto.Contains("premiere") ||
                           texto.Contains("toon") ||
                           texto.Contains("tvpaint") ||
                           texto.Contains("clip");
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>5. Herramientas, licencias y software considerados</h2>");

            if (software.Count == 0)
            {
                sb.AppendLine("<p class='muted'>No se detectaron costos de software/licencias individualizados en la cotización actual. Si Adobe u otras herramientas están en costos extra, aparecerán aquí automáticamente.</p>");
                sb.AppendLine("</section>");
                return sb.ToString();
            }

            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Herramienta / costo</th><th>Tipo</th><th>Monto</th><th>Nota</th></tr>");

            foreach (object item in software)
            {
                double monto = Numero(item, "MontoCLP", "CostoCLP", "ValorCLP", "TotalCLP", "CostoTotalCLP");

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + H(Texto(item, "Nombre", "Concepto", "Descripcion")) + "</td>");
                sb.AppendLine("<td>" + H(Texto(item, "Tipo", "Frecuencia", "Categoria")) + "</td>");
                sb.AppendLine("<td>" + FormatoCLP(monto) + "</td>");
                sb.AppendLine("<td>" + H(Texto(item, "Nota", "Observacion", "Comentario")) + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirEquipoNecesario(Cotizacion c)
        {
            List<object> requerimientos = ObtenerRequerimientosDesglose(c);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>6. Equipo y cargos necesarios</h2>");

            if (requerimientos.Count == 0)
            {
                sb.AppendLine("<p class='muted'>No hay desglose suficiente para estimar cargos requeridos.</p>");
                sb.AppendLine("</section>");
                return sb.ToString();
            }

            var cargos = requerimientos
                .GroupBy(r => Texto(r, "CargoSugerido", "Cargo"))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .OrderBy(g => g.Key)
                .ToList();

            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Cargo requerido</th><th>Días-persona estándar</th><th>Etapas asociadas</th><th>Requerimientos</th></tr>");

            foreach (var grupo in cargos)
            {
                double dias = grupo.Sum(r => Numero(r, "DiasPersonaStd", "DiasPersonaEstandar", "DiasStd"));
                string etapas = string.Join(", ",
                    grupo
                        .Select(r => Texto(r, "EtapaSugerida", "Etapa"))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                );

                string reqs = string.Join("; ",
                    grupo
                        .Select(r => Texto(r, "NombreRequerimiento", "Requerimiento", "TipoInterno"))
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct()
                );

                sb.AppendLine("<tr>");
                sb.AppendLine("<td><strong>" + H(grupo.Key) + "</strong></td>");
                sb.AppendLine("<td>" + dias.ToString("0.##") + " días</td>");
                sb.AppendLine("<td>" + H(etapas) + "</td>");
                sb.AppendLine("<td>" + H(reqs) + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            int personasEquivalentes = CalcularPersonasEquivalentes(c);
            if (personasEquivalentes > 0)
            {
                sb.AppendLine("<div class='callout small'>");
                sb.AppendLine("<strong>Capacidad sugerida:</strong> " + personasEquivalentes.ToString() + " persona(s) equivalente(s), según plazo y carga productiva.");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirPlanificacion(Cotizacion c)
        {
            List<object> etapas = ComoLista(Valor(c, "Etapas")).ToList();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>7. Planificación por etapas</h2>");

            if (etapas.Count == 0)
            {
                sb.AppendLine("<p class='muted'>No hay etapas configuradas.</p>");
                sb.AppendLine("</section>");
                return sb.ToString();
            }

            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Etapa</th><th>Estado</th><th>Inicio</th><th>Fin</th><th>Duración</th><th>Costo</th></tr>");

            foreach (object etapa in etapas)
            {
                bool seleccionada = Bool(etapa, "Seleccionada", "Activa");
                if (!seleccionada)
                {
                    continue;
                }

                string nombre = Texto(etapa, "Nombre", "Etapa");
                double inicio = Numero(etapa, "InicioMes");
                double fin = Numero(etapa, "FinMes");
                double duracion = Numero(etapa, "DuracionMeses");
                double costo = Numero(etapa, "CostoTotal", "CostoCLP");

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + BadgeEtapa(nombre) + "</td>");
                sb.AppendLine("<td>Activa</td>");
                sb.AppendLine("<td>Mes " + inicio.ToString("0.##") + "</td>");
                sb.AppendLine("<td>Mes " + fin.ToString("0.##") + "</td>");
                sb.AppendLine("<td>" + duracion.ToString("0.##") + " meses</td>");
                sb.AppendLine("<td>" + FormatoCLP(costo) + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirDesgloseProductivo(Cotizacion c)
        {
            List<object> requerimientos = ObtenerRequerimientosDesglose(c);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>8. Desglose productivo interno</h2>");

            if (requerimientos.Count == 0)
            {
                sb.AppendLine("<p class='muted'>No hay desglose productivo generado.</p>");
                sb.AppendLine("</section>");
                return sb.ToString();
            }

            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Entregable</th><th>Tipo interno</th><th>Requerimiento</th><th>Cantidad</th><th>Etapa</th><th>Días std.</th><th>Costo std.</th></tr>");

            foreach (object req in requerimientos)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + H(Texto(req, "EntregableCliente", "Entregable")) + "</td>");
                sb.AppendLine("<td>" + H(Texto(req, "TipoInterno")) + "</td>");
                sb.AppendLine("<td>" + H(Texto(req, "NombreRequerimiento", "Requerimiento")) + "</td>");
                sb.AppendLine("<td>" + Numero(req, "Cantidad").ToString("0.##") + " " + H(Texto(req, "Unidad")) + "</td>");
                sb.AppendLine("<td>" + BadgeEtapa(Texto(req, "EtapaSugerida", "Etapa")) + "</td>");
                sb.AppendLine("<td>" + Numero(req, "DiasPersonaStd", "DiasPersonaEstandar", "DiasStd").ToString("0.##") + "</td>");
                sb.AppendLine("<td>" + FormatoCLP(Numero(req, "CostoEstandarCLP", "CostoStdCLP")) + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirSupuestos(Cotizacion c)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<section>");
            sb.AppendLine("<h2>9. Supuestos y condiciones preliminares</h2>");
            sb.AppendLine("<ul class='list'>");
            sb.AppendLine("<li>La estimación es preliminar y depende de la validación de estilo, referencias, complejidad, fluidez y alcance final.</li>");
            sb.AppendLine("<li>La cantidad de fondos, personajes, props, planos y segundos animados puede modificar significativamente plazo y precio.</li>");
            sb.AppendLine("<li>Los costos de herramientas, licencias, software y servicios externos se consideran según lo ingresado en la cotización.</li>");
            sb.AppendLine("<li>El precio final puede ajustarse según urgencia, revisiones, cambios de alcance y disponibilidad de equipo.</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</section>");

            return sb.ToString();
        }

        private static string ConstruirTablaCostosExtra(Cotizacion c)
        {
            List<object> costos = ObtenerListaCostos(c);

            if (costos.Count == 0)
            {
                return "<p class='muted'>No hay costos extra individualizados.</p>";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h3>Detalle de costos extra</h3>");
            sb.AppendLine("<table class='table'>");
            sb.AppendLine("<tr><th>Concepto</th><th>Categoría</th><th>Monto</th><th>Nota</th></tr>");

            foreach (object item in costos)
            {
                double monto = Numero(item, "MontoCLP", "CostoCLP", "ValorCLP", "TotalCLP", "CostoTotalCLP");

                sb.AppendLine("<tr>");
                sb.AppendLine("<td>" + H(Texto(item, "Nombre", "Concepto", "Descripcion")) + "</td>");
                sb.AppendLine("<td>" + H(Texto(item, "Categoria", "Tipo", "Frecuencia")) + "</td>");
                sb.AppendLine("<td>" + FormatoCLP(monto) + "</td>");
                sb.AppendLine("<td>" + H(Texto(item, "Nota", "Observacion", "Comentario")) + "</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            return sb.ToString();
        }

        private static List<object> ObtenerRequerimientosDesglose(Cotizacion c)
        {
            object desglose = Valor(c, "DesgloseProductivo");

            if (desglose == null)
            {
                return new List<object>();
            }

            return ComoLista(Valor(desglose, "Requerimientos")).ToList();
        }

        private static List<object> ObtenerListaCostos(Cotizacion c)
        {
            List<object> costos = new List<object>();

            costos.AddRange(ComoLista(Valor(c, "CostosExtra")));
            costos.AddRange(ComoLista(Valor(c, "CostosAdicionales")));
            costos.AddRange(ComoLista(Valor(c, "ItemsCosto")));
            costos.AddRange(ComoLista(Valor(c, "Costos")));

            return costos;
        }

        private static double SumarCostosManoObraDesdeDesglose(Cotizacion c)
        {
            List<object> requerimientos = ObtenerRequerimientosDesglose(c);

            return requerimientos.Sum(r =>
                Numero(r, "CostoEstandarCLP", "CostoStdCLP", "CostoCLP")
            );
        }

        private static int CalcularPersonasEquivalentes(Cotizacion c)
        {
            object capacidad = Valor(c, "ResultadoCapacidad", "CapacidadProyecto", "Capacidad");

            if (capacidad != null)
            {
                int personas = (int)Math.Round(Numero(capacidad, "PersonasEquivalentesGlobales", "PersonasEquivalentes"));

                if (personas > 0)
                {
                    return personas;
                }
            }

            List<object> requerimientos = ObtenerRequerimientosDesglose(c);

            double dias = requerimientos.Sum(r =>
                Numero(r, "DiasPersonaStd", "DiasPersonaEstandar", "DiasStd")
            );

            if (dias <= 0.0)
            {
                return 0;
            }

            return Math.Max(1, (int)Math.Ceiling(dias / 20.0));
        }

        private static void FilaDato(StringBuilder sb, string nombre, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                valor = "No informado";
            }

            sb.AppendLine("<tr><td class='label'>" + H(nombre) + "</td><td>" + H(valor) + "</td></tr>");
        }

        private static void FilaCosto(StringBuilder sb, string nombre, double monto, double total)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>" + H(nombre) + "</td>");
            sb.AppendLine("<td>" + FormatoCLP(monto) + "</td>");
            sb.AppendLine("<td>" + PorcentajeSobreTotal(monto, total) + "</td>");
            sb.AppendLine("</tr>");
        }

        private static string Card(string titulo, string valor, string subtitulo)
        {
            return
                "<div class='card'>" +
                "<h3>" + H(titulo) + "</h3>" +
                "<div class='big'>" + H(valor) + "</div>" +
                "<div class='muted'>" + H(subtitulo) + "</div>" +
                "</div>";
        }

        private static string BadgeEtapa(string etapa)
        {
            string e = Normalizar(etapa);
            string clase = "neutral";

            if (e.Contains("desarrollo"))
            {
                clase = "dev";
            }
            else if (e.Contains("preproduccion"))
            {
                clase = "pre";
            }
            else if (e.Contains("postproduccion"))
            {
                clase = "post";
            }
            else if (e.Contains("produccion"))
            {
                clase = "prod";
            }

            if (string.IsNullOrWhiteSpace(etapa))
            {
                etapa = "Sin etapa";
            }

            return "<span class='badge " + clase + "'>" + H(etapa) + "</span>";
        }

        private static string FormatoCLP(double valor)
        {
            if (valor <= 0.0)
            {
                return "CLP 0";
            }

            return "CLP " + valor.ToString("N0", new CultureInfo("es-CL"));
        }

        private static string FormatoPorcentaje(double valor)
        {
            if (valor == 0.0)
            {
                return "0%";
            }

            if (Math.Abs(valor) <= 1.0)
            {
                return (valor * 100.0).ToString("0.##") + "%";
            }

            return valor.ToString("0.##") + "%";
        }

        private static string PorcentajeSobreTotal(double valor, double total)
        {
            if (total <= 0.0 || valor <= 0.0)
            {
                return "0%";
            }

            return ((valor / total) * 100.0).ToString("0.##") + "%";
        }

        private static string Texto(object obj, params string[] nombres)
        {
            object valor = Valor(obj, nombres);

            if (valor == null)
            {
                return "";
            }

            return valor.ToString() ?? "";
        }

        private static double Numero(object obj, params string[] nombres)
        {
            object valor = Valor(obj, nombres);

            if (valor == null)
            {
                return 0.0;
            }

            try
            {
                if (valor is int)
                {
                    return Convert.ToDouble((int)valor);
                }

                if (valor is double)
                {
                    return (double)valor;
                }

                if (valor is decimal)
                {
                    return Convert.ToDouble((decimal)valor);
                }

                if (valor is float)
                {
                    return Convert.ToDouble((float)valor);
                }

                string texto = valor.ToString();

                if (string.IsNullOrWhiteSpace(texto))
                {
                    return 0.0;
                }

                texto = texto.Replace(".", "").Replace(",", ".");

                double resultado;

                if (double.TryParse(
                    texto,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out resultado))
                {
                    return resultado;
                }
            }
            catch
            {
                return 0.0;
            }

            return 0.0;
        }

        private static bool Bool(object obj, params string[] nombres)
        {
            object valor = Valor(obj, nombres);

            if (valor == null)
            {
                return false;
            }

            if (valor is bool)
            {
                return (bool)valor;
            }

            string texto = valor.ToString();

            return texto == "true" ||
                   texto == "True" ||
                   texto == "1" ||
                   texto == "si" ||
                   texto == "sí" ||
                   texto == "Si" ||
                   texto == "Sí";
        }

        private static object Valor(object obj, params string[] nombres)
        {
            if (obj == null || nombres == null)
            {
                return null;
            }

            Type tipo = obj.GetType();

            foreach (string nombre in nombres)
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    continue;
                }

                PropertyInfo prop = tipo.GetProperty(
                    nombre,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase
                );

                if (prop != null)
                {
                    try
                    {
                        return prop.GetValue(obj, null);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<object> ComoLista(object valor)
        {
            if (valor == null)
            {
                yield break;
            }

            if (valor is string)
            {
                yield break;
            }

            IEnumerable enumerable = valor as IEnumerable;

            if (enumerable == null)
            {
                yield break;
            }

            foreach (object item in enumerable)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        private static string Normalizar(string texto)
        {
            return (texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n");
        }

        private static string H(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return "";
            }

            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        private static string ConstruirCss()
        {
            return @"
<style>
:root {
    --teal: #53c0a6;
    --red: #ee1e5b;
    --gold: #f4b400;
    --blue: #2196f3;
    --green: #2e7d32;
    --ink: #202124;
    --muted: #68707c;
    --bg: #f6f7fb;
    --line: #e0e4ea;
}
body {
    margin: 0;
    padding: 34px;
    font-family: Segoe UI, Arial, sans-serif;
    background: var(--bg);
    color: var(--ink);
}
.page {
    max-width: 1180px;
    margin: 0 auto;
    background: #fff;
    border: 1px solid var(--line);
    border-radius: 18px;
    overflow: hidden;
    box-shadow: 0 10px 32px rgba(0,0,0,0.08);
}
.hero {
    padding: 34px 38px;
    background: linear-gradient(135deg, #101820, #26313f);
    color: white;
}
.kicker {
    color: var(--teal);
    font-weight: 800;
    letter-spacing: .08em;
    text-transform: uppercase;
    font-size: 12px;
}
h1 {
    margin: 8px 0 8px 0;
    font-size: 32px;
}
h2 {
    font-size: 21px;
    margin: 0 0 13px 0;
}
h3 {
    font-size: 15px;
    margin: 0 0 8px 0;
}
.hero p {
    max-width: 860px;
    color: #d8dde5;
    line-height: 1.55;
}
.content {
    padding: 30px 38px 42px 38px;
}
section {
    margin-top: 30px;
}
.grid {
    display: grid;
    gap: 14px;
}
.cards-top {
    grid-template-columns: repeat(3, 1fr);
}
.two {
    grid-template-columns: 1fr 1fr;
}
.three {
    grid-template-columns: repeat(3, 1fr);
}
.card, .subcard {
    border: 1px solid var(--line);
    border-radius: 14px;
    padding: 18px;
    background: #fff;
}
.big {
    font-size: 23px;
    font-weight: 850;
    color: var(--ink);
}
.muted {
    color: var(--muted);
    font-size: 13px;
    line-height: 1.45;
}
.callout {
    margin-top: 18px;
    border-left: 5px solid var(--teal);
    background: #effaf7;
    padding: 16px 18px;
    border-radius: 10px;
    color: #245b50;
}
.callout.small {
    font-size: 13px;
}
.status {
    margin-top: 14px;
    padding: 12px;
    border-radius: 10px;
    font-weight: 700;
}
.status.ok {
    background: #e7f5ea;
    color: #2e7d32;
}
.status.warn {
    background: #fff1f1;
    color: #b3261e;
}
.status.neutral {
    background: #f2f4f7;
    color: #53606f;
}
.table {
    width: 100%;
    border-collapse: collapse;
    border: 1px solid var(--line);
    border-radius: 12px;
    overflow: hidden;
    margin-top: 8px;
}
.table th {
    text-align: left;
    background: #f1f3f6;
    padding: 10px;
    font-size: 13px;
    border-bottom: 1px solid var(--line);
}
.table td {
    padding: 10px;
    border-top: 1px solid var(--line);
    font-size: 13px;
    vertical-align: top;
}
.table.compact td {
    padding: 8px 10px;
}
.label {
    width: 220px;
    color: var(--muted);
    font-weight: 700;
}
.badge {
    display: inline-block;
    padding: 5px 9px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 800;
}
.dev { background: #e7f5ea; color: #2e7d32; }
.pre { background: #fff4cc; color: #8a6700; }
.prod { background: #ffe0df; color: #b3261e; }
.post { background: #dff0ff; color: #0b65b1; }
.neutral { background: #eceff3; color: #4f5965; }
.list {
    color: var(--muted);
    line-height: 1.6;
}
.footer {
    margin-top: 34px;
    padding-top: 16px;
    border-top: 1px solid var(--line);
    color: var(--muted);
    font-size: 12px;
}
@media print {
    body {
        background: white;
        padding: 0;
    }
    .page {
        box-shadow: none;
        border: none;
        border-radius: 0;
    }
}
</style>";
        }

        private static string GenerarHtmlVacio()
        {
            return "<html><body><h1>Informe cliente Othalart</h1><p>No hay cotización activa.</p></body></html>";
        }
    }
}