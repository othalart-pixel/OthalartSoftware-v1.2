using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Reports
{
    /// <summary>
    /// Genera el informe desde el proyecto consolidado, no desde la cotización
    /// individual que se usaba antes de incorporar productos múltiples.
    /// </summary>
    public static class InformeProyectoBuilder
    {
        private sealed class Contexto
        {
            public ProyectoCotizacion Proyecto { get; set; }
            public List<FilaProductivaProyecto> Filas { get; set; }
            public List<PersonaEquipo> Personas { get; set; }
            public List<CategoriaTrabajador> Cargos { get; set; }
            public List<string> Diagnosticos { get; set; }
        }

        public static string GenerarHtml(
            ProyectoCotizacion proyecto,
            IEnumerable<PersonaEquipo> personal,
            IEnumerable<CategoriaTrabajador> cargos)
        {
            if (proyecto == null)
            {
                return "<html><body><h1>Informe del proyecto</h1><p>No hay un proyecto activo.</p></body></html>";
            }

            ProyectoProductivoExpandido expandido = ProyectoProductivoExpansionService.Expandir(proyecto);
            Contexto c = new Contexto
            {
                Proyecto = proyecto,
                Filas = expandido.Filas ?? new List<FilaProductivaProyecto>(),
                Personas = (personal ?? Enumerable.Empty<PersonaEquipo>()).Where(p => p != null).ToList(),
                Cargos = (cargos ?? Enumerable.Empty<CategoriaTrabajador>()).Where(x => x != null).ToList(),
                Diagnosticos = expandido.Diagnosticos ?? new List<string>()
            };

            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html><html lang='es'><head><meta charset='utf-8'>");
            html.AppendLine("<meta http-equiv='X-UA-Compatible' content='IE=edge'>");
            html.AppendLine("<title>Informe global del proyecto</title>" + Css() + "</head><body><div class='page'>");
            html.AppendLine("<header><div class='kicker'>Othalart · Informe productivo global</div>");
            html.AppendLine("<h1>" + H(Valor(proyecto.Nombre, "Proyecto sin nombre")) + "</h1>");
            html.AppendLine("<p>Cliente: <strong>" + H(Valor(proyecto.Cliente, "No informado")) +
                "</strong> · Actualizado: " + proyecto.FechaModificacion.ToString("dd-MM-yyyy HH:mm") + "</p>");
            if (!string.IsNullOrWhiteSpace(proyecto.Descripcion))
            {
                html.AppendLine("<p>" + H(proyecto.Descripcion) + "</p>");
            }
            html.AppendLine("</header><main>");

            ConstruirResumen(html, c);
            ConstruirTrabajadores(html, c);
            ConstruirCargos(html, c);
            ConstruirJerarquia(html, c);
            ConstruirTransversales(html, c);
            ConstruirValidacion(html, c);

            html.AppendLine("<footer>Este informe usa el snapshot y las modificaciones locales guardadas en este proyecto. " +
                "La inversión usa primero el costo calculado; si falta, se estima con la tarifa del trabajador o el sueldo típico del cargo.</footer>");
            html.AppendLine("</main></div></body></html>");
            return html.ToString();
        }

        private static void ConstruirResumen(StringBuilder html, Contexto c)
        {
            decimal horas = c.Filas.Sum(Horas);
            decimal costo = c.Filas.Sum(f => Costo(f, c));
            decimal directo = c.Filas.Where(f => !f.Transversal).Sum(f => Costo(f, c));
            decimal transversal = c.Filas.Where(f => f.Transversal).Sum(f => Costo(f, c));
            int items = c.Proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .Count(i => i != null && i.Activo);
            int subs = c.Proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>())
                .Count(s => s != null && s.Activo);
            int personas = c.Filas.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                .Select(f => f.PersonaId).Distinct(StringComparer.OrdinalIgnoreCase).Count();

            html.AppendLine("<section class='cards'>");
            html.AppendLine(Card("Productos y servicios", items.ToString("N0"), subs + " subproductos"));
            html.AppendLine(Card("Tiempo total", FH(horas), c.Filas.Count + " filas productivas"));
            html.AppendLine(Card("Inversión atribuida", Dinero(costo), "Costo productivo consolidado"));
            html.AppendLine(Card("Costo directo", Dinero(directo), "Productos, subproductos y procesos"));
            html.AppendLine(Card("Costo transversal", Dinero(transversal), "Dirección, supervisión y gestión"));
            html.AppendLine(Card("Trabajadores", personas.ToString("N0"), "Personas identificadas"));
            html.AppendLine("</section>");
        }

        private static void ConstruirTrabajadores(StringBuilder html, Contexto c)
        {
            List<IGrouping<string, FilaProductivaProyecto>> grupos = c.Filas
                .Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                .GroupBy(f => f.PersonaId, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => NombrePersona(g.Key, c))
                .ToList();

            html.AppendLine("<section><h2>Equipo y trabajadores</h2>");
            if (grupos.Count == 0)
            {
                html.AppendLine("<div class='notice warn'>Todavía no hay trabajadores asociados. " +
                    "Los cargos requeridos sí aparecen en la sección siguiente.</div></section>");
                return;
            }

            html.AppendLine("<table><thead><tr><th>Trabajador</th><th>Cargo(s)</th><th>Pago acordado</th>" +
                "<th>Costo hora</th><th>Horas</th><th>Inversión</th><th>Productos</th></tr></thead><tbody>");
            foreach (IGrouping<string, FilaProductivaProyecto> grupo in grupos)
            {
                PersonaEquipo persona = Persona(grupo.Key, c);
                string nombresCargos = Unir(grupo.Select(f => NombreCargo(f.CargoId, c)));
                string productos = Unir(grupo.Select(f => NombreItem(f.ItemId, c)));
                html.AppendLine("<tr><td><strong>" + H(NombrePersona(grupo.Key, c)) + "</strong></td>" +
                    "<td>" + H(Valor(nombresCargos, "Sin cargo")) + "</td>" +
                    "<td>" + H(Pago(persona)) + "</td>" +
                    "<td>" + (Tarifa(persona, null) <= 0m ? "No definido" : Dinero(Tarifa(persona, null)) + "/h") + "</td>" +
                    "<td>" + FH(grupo.Sum(Horas)) + "</td>" +
                    "<td><strong>" + Dinero(grupo.Sum(f => Costo(f, c))) + "</strong></td>" +
                    "<td>" + H(Valor(productos, "Trabajo transversal")) + "</td></tr>");
            }
            html.AppendLine("</tbody></table>");

            List<FilaProductivaProyecto> sinPersona = c.Filas
                .Where(f => string.IsNullOrWhiteSpace(f.PersonaId) && Horas(f) > 0m).ToList();
            if (sinPersona.Count > 0)
            {
                html.AppendLine("<div class='notice warn'><strong>Trabajo aún sin trabajador:</strong> " +
                    FH(sinPersona.Sum(Horas)) + " · " + Dinero(sinPersona.Sum(f => Costo(f, c))) + ".</div>");
            }
            html.AppendLine("</section>");
        }

        private static void ConstruirCargos(StringBuilder html, Contexto c)
        {
            List<IGrouping<string, FilaProductivaProyecto>> grupos = c.Filas
                .Where(f => !string.IsNullOrWhiteSpace(f.CargoId))
                .GroupBy(f => f.CargoId, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => NombreCargo(g.Key, c))
                .ToList();

            html.AppendLine("<section><h2>Inversión por cargo</h2>");
            if (grupos.Count == 0)
            {
                html.AppendLine("<div class='notice'>No hay cargos asociados a los procesos.</div></section>");
                return;
            }

            decimal total = grupos.Sum(g => g.Sum(f => Costo(f, c)));
            html.AppendLine("<table><thead><tr><th>Cargo</th><th>Sueldo mensual típico</th><th>Tarifa referencial</th>" +
                "<th>Trabajadores</th><th>Horas</th><th>Inversión</th><th>% del costo</th></tr></thead><tbody>");
            foreach (IGrouping<string, FilaProductivaProyecto> grupo in grupos)
            {
                CategoriaTrabajador cargo = Cargo(grupo.Key, c);
                decimal inversion = grupo.Sum(f => Costo(f, c));
                string trabajadores = Unir(grupo.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                    .Select(f => NombrePersona(f.PersonaId, c)));
                decimal tarifa = Tarifa(null, cargo);
                html.AppendLine("<tr><td><strong>" + H(NombreCargo(grupo.Key, c)) + "</strong></td>" +
                    "<td>" + (cargo == null ? "No definido" : Dinero((decimal)cargo.SueldoMensualCLPTipico)) + "</td>" +
                    "<td>" + (tarifa <= 0m ? "No definida" : Dinero(tarifa) + "/h") + "</td>" +
                    "<td>" + H(Valor(trabajadores, "Sin asignar")) + "</td>" +
                    "<td>" + FH(grupo.Sum(Horas)) + "</td><td><strong>" + Dinero(inversion) + "</strong></td>" +
                    "<td>" + (total <= 0m ? "0 %" : (inversion / total).ToString("P1")) + "</td></tr>");
            }
            html.AppendLine("</tbody></table></section>");
        }

        private static void ConstruirJerarquia(StringBuilder html, Contexto c)
        {
            html.AppendLine("<section><h2>Desglose de productos, subproductos y procesos</h2>");
            foreach (GrupoProyecto grupo in (c.Proyecto.Grupos ?? new List<GrupoProyecto>())
                .Where(g => g != null && g.Activo).OrderBy(g => g.Orden))
            {
                html.AppendLine("<div class='group'><h3>" + H(Valor(grupo.Nombre, "Grupo sin nombre")) + "</h3>");
                foreach (ItemProyecto item in (grupo.Items ?? new List<ItemProyecto>())
                    .Where(i => i != null && i.Activo).OrderBy(i => i.Orden))
                {
                    List<FilaProductivaProyecto> filasItem = c.Filas.Where(f =>
                        Igual(f.ItemId, item.Id) || Igual(f.ProductoProyectoId, item.Id)).ToList();
                    html.AppendLine("<article class='product'><div class='product-head'><div>" +
                        "<span class='pill product-pill'>" + H(item.Tipo.ToString()) + "</span>" +
                        "<h4>" + H(Valor(item.Nombre, "Producto sin nombre")) + "</h4>" +
                        "<div class='muted'>" + item.Cantidad.ToString("N2") + " " + H(item.Unidad) + "</div></div>" +
                        "<div class='totals'><strong>" + FH(filasItem.Sum(Horas)) + "</strong><br>" +
                        Dinero(filasItem.Sum(f => Costo(f, c))) + "</div></div>");

                    List<FilaProductivaProyecto> directas = filasItem
                        .Where(f => string.IsNullOrWhiteSpace(f.SubproductoProyectoId)).ToList();
                    if (directas.Count > 0)
                    {
                        html.AppendLine("<div class='sub'><div class='sub-title'>Procesos directos</div>");
                        TablaProcesos(html, directas, c);
                        html.AppendLine("</div>");
                    }

                    foreach (SubproductoProyecto sub in (item.Subproductos ?? new List<SubproductoProyecto>())
                        .Where(s => s != null && s.Activo).OrderBy(s => s.Orden))
                    {
                        List<FilaProductivaProyecto> filasSub = filasItem
                            .Where(f => Igual(f.SubproductoProyectoId, sub.Id)).ToList();
                        html.AppendLine("<div class='sub'><div class='sub-head'><div>" +
                            "<span class='pill sub-pill'>Subproducto</span> <strong>" +
                            H(Valor(sub.Nombre, "Sin nombre")) + "</strong><span class='muted'> · " +
                            sub.Cantidad.ToString("N2") + " " + H(sub.Unidad) + "</span></div><div>" +
                            FH(filasSub.Sum(Horas)) + " · " + Dinero(filasSub.Sum(f => Costo(f, c))) +
                            "</div></div>");
                        TablaProcesos(html, filasSub, c);
                        html.AppendLine("</div>");
                    }
                    html.AppendLine("</article>");
                }
                html.AppendLine("</div>");
            }
            html.AppendLine("</section>");
        }

        private static void ConstruirTransversales(StringBuilder html, Contexto c)
        {
            List<FilaProductivaProyecto> filas = c.Filas.Where(f => f.Transversal).ToList();
            if (filas.Count == 0) return;
            html.AppendLine("<section><h2>Dirección, supervisión y gestión transversal</h2>");
            TablaProcesos(html, filas, c);
            html.AppendLine("</section>");
        }

        private static void TablaProcesos(StringBuilder html, List<FilaProductivaProyecto> filas, Contexto c)
        {
            if (filas.Count == 0)
            {
                html.AppendLine("<div class='empty'>Sin procesos calculados.</div>");
                return;
            }

            html.AppendLine("<table class='process'><thead><tr><th>Proceso</th><th>Etapa</th><th>Cargo</th>" +
                "<th>Trabajador</th><th>Horas</th><th>Inversión</th><th>Origen</th></tr></thead><tbody>");
            foreach (IGrouping<string, FilaProductivaProyecto> proceso in filas
                .GroupBy(f => Valor(f.ProcesoProyectoId, f.ProcesoBibliotecaId), StringComparer.OrdinalIgnoreCase))
            {
                FilaProductivaProyecto primera = proceso.First();
                html.AppendLine("<tr><td><strong>" + H(NombreProceso(primera, c)) + "</strong></td>" +
                    "<td>" + H(Valor(primera.EtapaId, "Sin etapa")) + "</td>" +
                    "<td>" + H(Valor(Unir(proceso.Select(f => NombreCargo(f.CargoId, c))), "Sin cargo")) + "</td>" +
                    "<td>" + H(Valor(Unir(proceso.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                        .Select(f => NombrePersona(f.PersonaId, c))), "Sin asignar")) + "</td>" +
                    "<td>" + FH(proceso.Sum(Horas)) + "</td>" +
                    "<td>" + Dinero(proceso.Sum(f => Costo(f, c))) + "</td>" +
                    "<td>" + H(Valor(primera.OrigenCalculo, primera.MetodoCalculo.ToString())) + "</td></tr>");
            }
            html.AppendLine("</tbody></table>");
        }

        private static void ConstruirValidacion(StringBuilder html, Contexto c)
        {
            List<string> alertas = c.Diagnosticos.Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
            int sinCargo = c.Filas.Count(f => Horas(f) > 0m && string.IsNullOrWhiteSpace(f.CargoId));
            int sinPersona = c.Filas.Count(f => Horas(f) > 0m && string.IsNullOrWhiteSpace(f.PersonaId));
            int personasPerdidas = c.Filas.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId))
                .Select(f => f.PersonaId).Distinct(StringComparer.OrdinalIgnoreCase).Count(id => Persona(id, c) == null);
            int cargosPerdidos = c.Filas.Where(f => !string.IsNullOrWhiteSpace(f.CargoId))
                .Select(f => f.CargoId).Distinct(StringComparer.OrdinalIgnoreCase).Count(id => Cargo(id, c) == null);
            if (sinCargo > 0) alertas.Add(sinCargo + " filas con horas no tienen cargo asociado.");
            if (sinPersona > 0) alertas.Add(sinPersona + " filas con horas no tienen trabajador asignado.");
            if (personasPerdidas > 0) alertas.Add(personasPerdidas + " trabajadores no existen en la biblioteca de personal.");
            if (cargosPerdidos > 0) alertas.Add(cargosPerdidos + " cargos no coinciden con la biblioteca de cargos.");

            html.AppendLine("<section><h2>Validación del informe</h2>");
            if (alertas.Count == 0)
            {
                html.AppendLine("<div class='notice ok'>Todas las referencias productivas están completas.</div>");
            }
            else
            {
                html.AppendLine("<div class='notice warn'><ul>");
                foreach (string alerta in alertas.Distinct()) html.AppendLine("<li>" + H(alerta) + "</li>");
                html.AppendLine("</ul></div>");
            }
            html.AppendLine("</section>");
        }

        private static decimal Horas(FilaProductivaProyecto f)
        {
            if (f == null) return 0m;
            return f.HorasAsignadas > 0m ? f.HorasAsignadas : f.HorasCalculadas;
        }

        private static decimal Costo(FilaProductivaProyecto f, Contexto c)
        {
            if (f == null) return 0m;
            if (f.Costo > 0m) return f.Costo;
            return Math.Round(Horas(f) * Tarifa(Persona(f.PersonaId, c), Cargo(f.CargoId, c)), 0);
        }

        private static decimal Tarifa(PersonaEquipo persona, CategoriaTrabajador cargo)
        {
            if (persona != null)
            {
                if (persona.CostoHora > 0m) return persona.CostoHora;
                if (persona.PagoInterno > 0m)
                {
                    decimal semana = persona.HorasTrabajoSemana > 0m ? persona.HorasTrabajoSemana : 42m;
                    string periodo = Normalizar(persona.PeriodoPago);
                    decimal horas = periodo.Contains("seman") ? semana :
                        periodo.Contains("quinc") ? semana * 2m :
                        periodo.Contains("dia") ? Math.Max(1m, semana / 5m) : semana * 4m;
                    return horas <= 0m ? 0m : persona.PagoInterno / horas;
                }
            }
            return cargo == null || cargo.SueldoMensualCLPTipico <= 0
                ? 0m : (decimal)cargo.SueldoMensualCLPTipico / 22m / 8m;
        }

        private static PersonaEquipo Persona(string id, Contexto c)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return c.Personas.FirstOrDefault(p => Igual(p.Id, id) || Igual(p.Nombre, id));
        }

        private static CategoriaTrabajador Cargo(string id, Contexto c)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (int.TryParse(id, out int numero))
            {
                CategoriaTrabajador porId = c.Cargos.FirstOrDefault(x => x.Id == numero);
                if (porId != null) return porId;
            }
            string buscado = NormalizarCargo(id);
            return c.Cargos.FirstOrDefault(x =>
                NormalizarCargo(x.Nombre) == buscado || NormalizarCargo(x.NombreCompleto) == buscado);
        }

        private static string NombrePersona(string id, Contexto c)
        {
            PersonaEquipo p = Persona(id, c);
            return p == null ? Valor(id, "Sin trabajador") : Valor(p.Nombre, p.Id);
        }

        private static string NombreCargo(string id, Contexto c)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            CategoriaTrabajador cargo = Cargo(id, c);
            return cargo == null ? id : cargo.NombreCompleto;
        }

        private static string NombreItem(string id, Contexto c)
        {
            if (string.IsNullOrWhiteSpace(id)) return "";
            ItemProyecto item = c.Proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .FirstOrDefault(i => Igual(i.Id, id));
            return item == null ? id : Valor(item.Nombre, id);
        }

        private static string NombreProceso(FilaProductivaProyecto fila, Contexto c)
        {
            IEnumerable<ProcesoProyecto> procesos = c.Proyecto.Grupos
                .SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => (i.Procesos ?? new List<ProcesoProyecto>())
                    .Concat((i.Subproductos ?? new List<SubproductoProyecto>())
                        .SelectMany(s => (s.Procesos ?? new List<ProcesoProyecto>())
                            .Concat((s.Instancias ?? new List<InstanciaSubproducto>())
                                .SelectMany(x => x.Procesos ?? new List<ProcesoProyecto>())))));
            ProcesoProyecto proceso = procesos.FirstOrDefault(p => Igual(p.Id, fila.ProcesoProyectoId));
            if (proceso != null) return Valor(proceso.Nombre, proceso.Id);
            ProcesoTransversalProyecto transversal = (c.Proyecto.ProcesosTransversales ??
                new List<ProcesoTransversalProyecto>()).FirstOrDefault(p => Igual(p.Id, fila.ProcesoProyectoId));
            return transversal == null
                ? Valor(fila.ProcesoBibliotecaId, Valor(fila.ProcesoProyectoId, "Proceso"))
                : Valor(transversal.Nombre, transversal.Id);
        }

        private static string Pago(PersonaEquipo persona)
        {
            return persona == null || persona.PagoInterno <= 0m
                ? "No definido"
                : Dinero(persona.PagoInterno) + " / " + Valor(persona.PeriodoPago, "Mensual");
        }

        private static string Unir(IEnumerable<string> valores)
        {
            return string.Join(", ", (valores ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x));
        }

        private static string NormalizarCargo(string valor)
        {
            return Normalizar(valor).Replace("cargo ", "").Replace(" tipico", "")
                .Replace(" tipica", "").Trim();
        }

        private static string Normalizar(string valor)
        {
            string texto = (valor ?? "").Trim().ToLowerInvariant().Replace("_", " ")
                .Normalize(NormalizationForm.FormD);
            StringBuilder limpio = new StringBuilder();
            foreach (char x in texto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark &&
                    (char.IsLetterOrDigit(x) || char.IsWhiteSpace(x))) limpio.Append(x);
            }
            return string.Join(" ", limpio.ToString().Split(new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries));
        }

        private static bool Igual(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string Valor(string valor, string respaldo)
        {
            return string.IsNullOrWhiteSpace(valor) ? respaldo : valor.Trim();
        }

        private static string FH(decimal valor) { return valor.ToString("N2") + " h"; }
        private static string Dinero(decimal valor) { return valor.ToString("N0") + " CLP"; }

        private static string Card(string titulo, string valor, string detalle)
        {
            return "<div class='card'><div class='card-label'>" + H(titulo) + "</div><div class='big'>" +
                H(valor) + "</div><div class='muted'>" + H(detalle) + "</div></div>";
        }

        private static string H(string texto)
        {
            return (texto ?? "").Replace("&", "&amp;").Replace("<", "&lt;")
                .Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");
        }

        private static string Css()
        {
            return @"<style>
body{margin:0;padding:28px;background:#f4f6f8;color:#1c2630;font-family:'Segoe UI',Arial,sans-serif}
.page{max-width:1280px;margin:auto;background:#fff;border:1px solid #dfe5e8;box-shadow:0 10px 32px rgba(25,40,50,.09)}
header{padding:30px 36px;background:#173d3b;color:#fff}header h1{font-size:30px;margin:8px 0}header p{color:#d7e7e3;margin:8px 0}
.kicker{color:#74d7bd;font-size:12px;font-weight:800;letter-spacing:.1em;text-transform:uppercase}
main{padding:28px 34px 40px}section{margin:0 0 30px}h2{font-size:21px;margin:0 0 13px;border-bottom:2px solid #d9eee8;padding-bottom:8px}
h3{font-size:18px;margin:0 0 12px}h4{display:inline;font-size:18px;margin-left:8px}
.cards{display:flex;flex-wrap:wrap;gap:12px}.card{width:28%;min-width:210px;border:1px solid #dfe5e8;border-radius:10px;padding:15px;background:#fbfcfc}
.card-label{font-size:12px;font-weight:800;color:#58706e;text-transform:uppercase}.big{font-size:23px;font-weight:800;margin:6px 0}
.muted{color:#68777e;font-size:12px}.group{margin:18px 0 26px}.product{border:1px solid #cfdadd;border-radius:10px;margin:12px 0;overflow:hidden}
.product-head,.sub-head{display:flex;justify-content:space-between;align-items:center}.product-head{background:#eef7f5;padding:15px 17px;border-bottom:1px solid #cfdadd}
.totals{text-align:right}.sub{padding:13px 16px;border-top:1px solid #e4e9eb}.sub-title{font-weight:700;color:#35645d;margin-bottom:8px}
.pill{display:inline-block;border-radius:12px;padding:3px 8px;font-size:11px;font-weight:800}.product-pill{background:#155b52;color:#fff}.sub-pill{background:#d8eee8;color:#245e54}
table{width:100%;border-collapse:collapse;margin-top:8px}th{text-align:left;background:#edf1f3;color:#40535b;font-size:12px;padding:9px;border:1px solid #d9e0e3}
td{font-size:12px;padding:9px;border:1px solid #dfe5e8;vertical-align:top}.process th,.process td{padding:7px}.process tr:nth-child(even){background:#fafcfc}
.notice{padding:13px 15px;border-radius:8px;background:#eef2f4;color:#43545c}.notice.warn{background:#fff7df;color:#735713}.notice.ok{background:#e7f6ef;color:#216346}
.empty{color:#7a858b;font-size:12px;padding:8px}footer{border-top:1px solid #dfe5e8;padding-top:16px;color:#738087;font-size:11px}
@media print{body{padding:0;background:#fff}.page{border:0;box-shadow:none}.product{break-inside:avoid}}
</style>";
        }
    }
}
