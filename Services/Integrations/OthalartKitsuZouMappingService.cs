using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Models.Integrations;

namespace Cotizador_animacion_Othalart.Services.Integrations
{
    public static class OthalartKitsuZouMappingService
    {
        private const double DefaultHoursPerDay = 8.0;

        public static OthalartUpstreamDraft CrearBorrador(Cotizacion cotizacion)
        {
            OthalartUpstreamDraft draft = new OthalartUpstreamDraft();

            if (cotizacion == null)
            {
                draft.Warnings.Add(Warn("Cotizacion", "No hay cotizacion para mapear."));
                return draft;
            }

            draft.Project = CrearProyecto(cotizacion);
            AgregarEntidades(draft, cotizacion);
            AgregarTareas(draft, cotizacion);
            AgregarAsignaciones(draft, cotizacion);

            return draft;
        }

        private static UpstreamProjectDraft CrearProyecto(Cotizacion cotizacion)
        {
            string producto = cotizacion.BriefProducto?.ProductoServicioSeleccionado ?? "";
            string solicitud = cotizacion.BriefProducto?.TipoSolicitudProducto ?? "";

            return new UpstreamProjectDraft
            {
                LocalId = CrearId("project", cotizacion.NombreProyecto, producto),
                Name = TextoONoDefinido(cotizacion.NombreProyecto, "Proyecto Othalart"),
                Client = cotizacion.NombreCliente ?? "",
                Company = cotizacion.Empresa ?? "",
                Description = cotizacion.Descripcion ?? "",
                Currency = TextoONoDefinido(cotizacion.MonedaVisualizacion, "CLP"),
                ProductName = producto,
                RequestType = solicitud
            };
        }

        private static void AgregarEntidades(OthalartUpstreamDraft draft, Cotizacion cotizacion)
        {
            List<EntregableBrief> entregables =
                cotizacion.BriefProducto?.EntregablesSeleccionados ?? new List<EntregableBrief>();

            foreach (EntregableBrief entregable in entregables)
            {
                if (entregable == null)
                {
                    continue;
                }

                string entityId = CrearEntityId(entregable);
                if (draft.Entities.Any(e => e.LocalId == entityId))
                {
                    continue;
                }

                draft.Entities.Add(new UpstreamEntityDraft
                {
                    LocalId = entityId,
                    Name = TextoONoDefinido(entregable.Nombre, "Sin nombre"),
                    EntityType = InferirEntityType(entregable),
                    ProductName = cotizacion.BriefProducto?.ProductoServicioSeleccionado ?? "",
                    Category = entregable.Categoria ?? "",
                    Quantity = Math.Max(1, entregable.Cantidad),
                    DurationPerUnit = Math.Max(0.0, entregable.DuracionPorUnidad),
                    DurationUnit = entregable.UnidadDuracion ?? "",
                    QuantityUnit = entregable.UnidadCantidad ?? ""
                });
            }

            if (draft.Entities.Count == 0)
            {
                draft.Warnings.Add(Warn("Entities", "No hay entregables seleccionados para exportar como entidades."));
            }
        }

        private static void AgregarTareas(OthalartUpstreamDraft draft, Cotizacion cotizacion)
        {
            List<RequerimientoProduccionInterna> requerimientos =
                cotizacion.DesgloseProductivo?.Requerimientos ?? new List<RequerimientoProduccionInterna>();

            foreach (RequerimientoProduccionInterna req in requerimientos)
            {
                if (req == null)
                {
                    continue;
                }

                string entityId = CrearId("entity", req.EntregableCliente, req.CategoriaEntregable);
                if (!draft.Entities.Any(e => e.LocalId == entityId))
                {
                    draft.Entities.Add(new UpstreamEntityDraft
                    {
                        LocalId = entityId,
                        Name = TextoONoDefinido(req.EntregableCliente, "Entidad desde desglose"),
                        EntityType = "Asset",
                        ProductName = cotizacion.BriefProducto?.ProductoServicioSeleccionado ?? "",
                        Category = req.CategoriaEntregable ?? "",
                        Quantity = Math.Max(1, (int)Math.Ceiling(req.Cantidad)),
                        QuantityUnit = req.Unidad ?? ""
                    });
                }

                string taskId = CrearTaskId(req);
                draft.Tasks.Add(new UpstreamTaskDraft
                {
                    LocalId = taskId,
                    EntityLocalId = entityId,
                    Name = TextoONoDefinido(req.NombreRequerimiento, req.TipoInterno),
                    TaskType = TextoONoDefinido(req.TipoInterno, req.NombreRequerimiento),
                    Department = TextoONoDefinido(req.EtapaSugerida, req.BloqueProductivo),
                    Status = req.ParametrosCompletos ? "todo" : "blocked",
                    DependsOnTaskLocalId = CrearDependenciaTaskId(req, requerimientos),
                    EquationKey = req.EcuacionUsada ?? "",
                    RequiredRole = req.CargoSugerido ?? "",
                    EstimatedPersonDays = Math.Max(0.0, req.DiasPersonaStd),
                    EstimatedHours = Math.Max(0.0, req.DiasPersonaStd * DefaultHoursPerDay),
                    EstimatedCostCLP = Math.Max(0.0, req.CostoEstandarCLP)
                });

                if (!req.ParametrosCompletos)
                {
                    draft.Warnings.Add(Warn(taskId, TextoONoDefinido(req.DiagnosticoParametros, "Parametros incompletos.")));
                }
            }

            if (draft.Tasks.Count == 0)
            {
                draft.Warnings.Add(Warn("Tasks", "No hay requerimientos de desglose para mapear como tareas."));
            }
        }

        private static void AgregarAsignaciones(OthalartUpstreamDraft draft, Cotizacion cotizacion)
        {
            List<AsignacionManoObraProyecto> asignaciones =
                cotizacion.AsignacionesManoObra ?? new List<AsignacionManoObraProyecto>();

            foreach (AsignacionManoObraProyecto asignacion in asignaciones)
            {
                if (asignacion == null)
                {
                    continue;
                }

                string taskId = BuscarTaskIdParaAsignacion(draft.Tasks, asignacion);
                if (string.IsNullOrWhiteSpace(taskId))
                {
                    draft.Warnings.Add(Warn(
                        asignacion.ClaveLabor,
                        "Asignacion sin tarea equivalente en el borrador upstream."
                    ));
                    continue;
                }

                draft.Assignments.Add(new UpstreamAssignmentDraft
                {
                    LocalId = CrearId("assignment", taskId, asignacion.PersonaId, asignacion.CargoRequerido),
                    TaskLocalId = taskId,
                    PersonLocalId = asignacion.PersonaId ?? "",
                    PersonName = asignacion.PersonaNombre ?? "",
                    RequiredRole = asignacion.CargoRequerido ?? "",
                    Hours = Math.Max(0.0, asignacion.HorasAsignadas),
                    IsGenericResource = EsRecursoGenerico(asignacion)
                });
            }
        }

        private static string BuscarTaskIdParaAsignacion(
            List<UpstreamTaskDraft> tasks,
            AsignacionManoObraProyecto asignacion)
        {
            string pieza = NormalizarToken(asignacion.PiezaSubproducto);
            string labor = NormalizarToken(asignacion.SubEtapaLabor);
            string cargo = NormalizarToken(asignacion.CargoRequerido);

            UpstreamTaskDraft match = tasks.FirstOrDefault(t =>
                NormalizarToken(t.EntityLocalId).Contains(pieza) &&
                (NormalizarToken(t.Name).Contains(labor) || NormalizarToken(t.TaskType).Contains(labor)) &&
                (string.IsNullOrWhiteSpace(cargo) || NormalizarToken(t.RequiredRole).Contains(cargo))
            );

            return match?.LocalId ?? "";
        }

        private static string CrearDependenciaTaskId(
            RequerimientoProduccionInterna req,
            List<RequerimientoProduccionInterna> todos)
        {
            if (string.IsNullOrWhiteSpace(req.DependeDe))
            {
                return "";
            }

            RequerimientoProduccionInterna dependencia = todos.FirstOrDefault(t =>
                string.Equals(t.EntregableCliente, req.EntregableCliente, StringComparison.OrdinalIgnoreCase) &&
                (
                    string.Equals(t.NombreRequerimiento, req.DependeDe, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t.TipoInterno, req.DependeDe, StringComparison.OrdinalIgnoreCase)
                )
            );

            return dependencia == null ? "" : CrearTaskId(dependencia);
        }

        private static string CrearEntityId(EntregableBrief entregable)
        {
            return CrearId("entity", entregable.Nombre, entregable.Categoria);
        }

        private static string CrearTaskId(RequerimientoProduccionInterna req)
        {
            return CrearId("task", req.EntregableCliente, req.NombreRequerimiento, req.TipoInterno, req.CargoSugerido);
        }

        private static string CrearId(string prefijo, params string[] partes)
        {
            string cuerpo = string.Join("_", partes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(NormalizarToken));
            if (string.IsNullOrWhiteSpace(cuerpo))
            {
                cuerpo = "sin_definir";
            }

            return prefijo + "_" + cuerpo;
        }

        private static string NormalizarToken(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in texto.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (char.IsWhiteSpace(c) || c == '-' || c == '/' || c == '_')
                {
                    sb.Append('_');
                }
            }

            string normalizado = sb.ToString();
            while (normalizado.Contains("__"))
            {
                normalizado = normalizado.Replace("__", "_");
            }

            return normalizado.Trim('_');
        }

        private static string TextoONoDefinido(string valor, string respaldo)
        {
            if (!string.IsNullOrWhiteSpace(valor))
            {
                return valor.Trim();
            }

            return string.IsNullOrWhiteSpace(respaldo) ? "No definido" : respaldo.Trim();
        }

        private static string InferirEntityType(EntregableBrief entregable)
        {
            string texto = (entregable.Categoria + " " + entregable.UnidadCantidad + " " + entregable.Nombre)
                .ToLowerInvariant();

            if (texto.Contains("escena") || texto.Contains("shot") || texto.Contains("plano"))
            {
                return "Shot";
            }

            return "Asset";
        }

        private static bool EsRecursoGenerico(AsignacionManoObraProyecto asignacion)
        {
            return string.IsNullOrWhiteSpace(asignacion.PersonaId) ||
                   (asignacion.TipoAsignacion ?? "").ToLowerInvariant().Contains("gener");
        }

        private static OthalartUpstreamMappingWarning Warn(string scope, string message)
        {
            return new OthalartUpstreamMappingWarning
            {
                Severity = "Warning",
                Scope = scope ?? "",
                Message = message ?? ""
            };
        }
    }
}
