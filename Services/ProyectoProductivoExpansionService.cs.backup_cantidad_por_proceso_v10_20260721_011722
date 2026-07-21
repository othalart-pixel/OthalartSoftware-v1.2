using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoProductivoExpansionService
    {
        public static ProyectoProductivoExpandido Expandir(ProyectoCotizacion proyecto)
        {
            ProyectoProductivoExpandido expandido = new ProyectoProductivoExpandido
            {
                ProyectoId = proyecto == null ? "" : proyecto.Id
            };

            if (proyecto == null)
            {
                expandido.Diagnosticos.Add("No hay proyecto para expandir.");
                return expandido;
            }

            foreach (GrupoProyecto grupo in proyecto.Grupos ?? new List<GrupoProyecto>())
            {
                if (grupo == null || !grupo.Activo)
                {
                    continue;
                }

                foreach (ItemProyecto item in grupo.Items ?? new List<ItemProyecto>())
                {
                    if (item == null || !item.Activo)
                    {
                        continue;
                    }

                    ExpandirItem(proyecto, grupo, item, expandido);
                }
            }

            ExpandirTransversales(proyecto, expandido);
            return expandido;
        }

        private static void ExpandirItem(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            ProyectoProductivoExpandido expandido
        )
        {
            if (CotizacionItemProyectoAdapterService.ItemTieneSnapshot(item))
            {
                ExpandirCotizacionSnapshotItem(proyecto, grupo, item, expandido);
                return;
            }

            foreach (ProcesoProyecto proceso in item.Procesos ?? new List<ProcesoProyecto>())
            {
                if (proceso != null && proceso.Activo)
                {
                    expandido.Filas.AddRange(CrearFilasProceso(
                        proyecto,
                        grupo,
                        item,
                        null,
                        null,
                        proceso,
                        item.Cantidad,
                        item.Unidad,
                        false
                    ));
                }
            }

            foreach (SubproductoProyecto sub in item.Subproductos ?? new List<SubproductoProyecto>())
            {
                if (sub == null || !sub.Activo)
                {
                    continue;
                }

                if (sub.ModoCantidad == ModoCantidadSubproducto.InstanciasIndividuales &&
                    sub.Instancias != null &&
                    sub.Instancias.Count > 0)
                {
                    foreach (InstanciaSubproducto instancia in sub.Instancias.Where(i => i != null))
                    {
                        ExpandirInstancia(proyecto, grupo, item, sub, instancia, expandido);
                    }
                }
                else
                {
                    foreach (ProcesoProyecto proceso in sub.Procesos ?? new List<ProcesoProyecto>())
                    {
                        if (proceso != null && proceso.Activo)
                        {
                            expandido.Filas.AddRange(CrearFilasProceso(
                                proyecto,
                                grupo,
                                item,
                                sub,
                                null,
                                proceso,
                                sub.Cantidad,
                                sub.Unidad,
                                false
                            ));
                        }
                    }
                }
            }
        }

        private static void ExpandirCotizacionSnapshotItem(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            ProyectoProductivoExpandido expandido
        )
        {
            Cotizacion cotizacionItem = CotizacionItemProyectoAdapterService.CrearCotizacionDesdeItem(item, null);
            List<RequerimientoProduccionInterna> requerimientos =
                cotizacionItem?.DesgloseProductivo?.Requerimientos ?? new List<RequerimientoProduccionInterna>();

            if (requerimientos.Count == 0)
            {
                expandido.Diagnosticos.Add("El item '" + item.Nombre + "' tiene snapshot, pero no tiene desglose productivo.");
                return;
            }

            foreach (RequerimientoProduccionInterna req in requerimientos.Where(r => r != null))
            {
                decimal horas = Convert.ToDecimal(req.HorasEstandar > 0.0
                    ? req.HorasEstandar
                    : req.DiasPersonaStd * 8.0);
                decimal costo = Convert.ToDecimal(req.CostoEstandarCLP);
                string procesoId = string.IsNullOrWhiteSpace(req.ProcesoId)
                    ? CrearIdSnapshot(item.Id, req.NombreRequerimiento, req.TipoInterno)
                    : req.ProcesoId;

                expandido.Filas.Add(new FilaProductivaProyecto
                {
                    ProyectoId = proyecto.Id,
                    GrupoId = grupo == null ? "" : grupo.Id,
                    ItemId = item.Id,
                    ProductoProyectoId = item.Tipo == TipoItemProyecto.Producto ? item.Id : "",
                    SubproductoProyectoId = ResolverSubproductoSnapshot(item, req),
                    InstanciaId = req.InstanciaId,
                    ProcesoProyectoId = procesoId,
                    ProcesoBibliotecaId = req.EcuacionUsada,
                    TipoProceso = req.TipoProceso == TipoProcesoProductivo.NoClasificado
                        ? TipoProcesoProductivo.ProduccionDirecta
                        : req.TipoProceso,
                    MetodoCalculo = req.MetodoCalculo,
                    AlcanceTemporal = req.AlcanceTemporal,
                    EtapaId = req.EtapaSugerida,
                    CargoId = string.IsNullOrWhiteSpace(req.CargoId) ? req.CargoSugerido : req.CargoId,
                    PersonaId = req.PersonaId,
                    Cantidad = Convert.ToDecimal(req.Cantidad),
                    Unidad = req.Unidad ?? "",
                    Capacidad = Convert.ToDecimal(req.RendimientoCantidadOverride > 0.0
                        ? req.RendimientoCantidadOverride
                        : req.RendimientoCantidad),
                    Periodo = string.IsNullOrWhiteSpace(req.RendimientoPeriodoOverride)
                        ? req.RendimientoPeriodo ?? ""
                        : req.RendimientoPeriodoOverride,
                    HorasCalculadas = horas,
                    HorasAsignadas = horas,
                    Costo = costo,
                    Dependencias = ParsearDependenciasSnapshot(req),
                    Paralelo = req.PuedeEjecutarseEnParalelo,
                    Transversal = EsTipoTransversal(req.TipoProceso),
                    TieneOverrideLocalCalculo = req.TieneOverrideLocalCalculo,
                    OrigenCalculo = req.TieneOverrideLocalCalculo
                        ? "Override local"
                        : string.IsNullOrWhiteSpace(req.RendimientoOrigen) ? "Plantilla" : req.RendimientoOrigen,
                    Diagnostico = string.IsNullOrWhiteSpace(req.DiagnosticoParametros)
                        ? "Fila expandida desde la interfaz actual del item."
                        : req.DiagnosticoParametros
                });
            }
        }

        private static string ResolverSubproductoSnapshot(ItemProyecto item, RequerimientoProduccionInterna req)
        {
            if (item == null || item.Subproductos == null || req == null)
            {
                return "";
            }

            SubproductoProyecto sub = item.Subproductos.FirstOrDefault(s =>
                s != null &&
                (
                    string.Equals(s.Nombre, req.EntregableCliente, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Nombre, req.NombreRequerimiento, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.SubproductoBibliotecaId, req.TipoInterno, StringComparison.OrdinalIgnoreCase)
                ));

            return sub == null ? "" : sub.Id;
        }

        private static List<string> ParsearDependenciasSnapshot(RequerimientoProduccionInterna req)
        {
            string texto = string.IsNullOrWhiteSpace(req.DependenciasProcesoJson)
                ? req.DependeDe
                : req.DependenciasProcesoJson;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return new List<string>();
            }

            return texto
                .Split(new[] { ';', ',', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim().Trim('"', '[', ']'))
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string CrearIdSnapshot(string itemId, string nombre, string tipo)
        {
            string baseId = (itemId + "_" + nombre + "_" + tipo).ToLowerInvariant();
            char[] chars = baseId.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
            return new string(chars).Trim('_');
        }

        private static bool EsTipoTransversal(TipoProcesoProductivo tipo)
        {
            return tipo == TipoProcesoProductivo.Supervision ||
                tipo == TipoProcesoProductivo.Direccion ||
                tipo == TipoProcesoProductivo.GestionCoordinacion;
        }

        private static void ExpandirInstancia(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            SubproductoProyecto sub,
            InstanciaSubproducto instancia,
            ProyectoProductivoExpandido expandido
        )
        {
            foreach (ProcesoProyecto proceso in instancia.Procesos ?? new List<ProcesoProyecto>())
            {
                if (proceso != null && proceso.Activo)
                {
                    expandido.Filas.AddRange(CrearFilasProceso(
                        proyecto,
                        grupo,
                        item,
                        sub,
                        instancia,
                        proceso,
                        instancia.CantidadEquivalente,
                        sub.Unidad,
                        false
                    ));
                }
            }

            foreach (AsignacionProductiva asignacion in instancia.Asignaciones ?? new List<AsignacionProductiva>())
            {
                expandido.Filas.Add(CrearFilaAsignacionDirecta(
                    proyecto,
                    grupo,
                    item,
                    sub,
                    instancia,
                    asignacion
                ));
            }
        }

        private static IEnumerable<FilaProductivaProyecto> CrearFilasProceso(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            SubproductoProyecto sub,
            InstanciaSubproducto instancia,
            ProcesoProyecto proceso,
            decimal cantidad,
            string unidad,
            bool transversal
        )
        {
            decimal cantidadProceso = proceso.Cantidad > 0m ? proceso.Cantidad : cantidad;
            string unidadProceso = string.IsNullOrWhiteSpace(proceso.Unidad) ? unidad : proceso.Unidad;
            List<AsignacionProductiva> asignaciones = proceso.Asignaciones ?? new List<AsignacionProductiva>();
            if (asignaciones.Count == 0)
            {
                yield return CrearFilaBase(
                    proyecto,
                    grupo,
                    item,
                    sub,
                    instancia,
                    proceso,
                    null,
                    cantidadProceso,
                    unidadProceso,
                    transversal,
                    "Proceso sin asignaciones productivas."
                );
                yield break;
            }

            foreach (AsignacionProductiva asignacion in asignaciones)
            {
                yield return CrearFilaBase(
                    proyecto,
                    grupo,
                    item,
                    sub,
                    instancia,
                    proceso,
                    asignacion,
                    cantidadProceso,
                    unidadProceso,
                    transversal,
                    ""
                );
            }
        }

        private static FilaProductivaProyecto CrearFilaBase(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            SubproductoProyecto sub,
            InstanciaSubproducto instancia,
            ProcesoProyecto proceso,
            AsignacionProductiva asignacion,
            decimal cantidad,
            string unidad,
            bool transversal,
            string diagnostico
        )
        {
            decimal horasCalculadas = asignacion == null
                ? proceso.Resultado?.HorasCalculadas ?? 0m
                : asignacion.HorasCalculadas;
            decimal horasAsignadas = asignacion == null
                ? proceso.Resultado?.HorasAsignadas ?? horasCalculadas
                : (asignacion.HorasAsignadas > 0m ? asignacion.HorasAsignadas : asignacion.HorasCalculadas);
            decimal costo = asignacion == null
                ? proceso.Resultado?.CostoCalculado ?? 0m
                : asignacion.CostoCalculado;

            return new FilaProductivaProyecto
            {
                ProyectoId = proyecto.Id,
                GrupoId = grupo == null ? "" : grupo.Id,
                ItemId = item == null ? "" : item.Id,
                ProductoProyectoId = item != null && item.Tipo == TipoItemProyecto.Producto ? item.Id : "",
                SubproductoProyectoId = sub == null ? "" : sub.Id,
                InstanciaId = instancia == null ? "" : instancia.Id,
                ProcesoProyectoId = proceso.Id,
                ProcesoBibliotecaId = proceso.ProcesoBibliotecaId,
                TipoProceso = proceso.TipoProceso,
                MetodoCalculo = proceso.MetodoCalculo,
                AlcanceTemporal = proceso.AlcanceTemporal,
                EtapaId = proceso.EtapaId,
                CargoId = asignacion == null ? "" : asignacion.CargoId,
                PersonaId = asignacion == null ? "" : asignacion.PersonaId,
                Cantidad = cantidad,
                Unidad = unidad ?? "",
                Capacidad = proceso.Capacidad,
                Periodo = proceso.Periodo ?? "",
                HorasCalculadas = horasCalculadas,
                HorasAsignadas = horasAsignadas,
                Costo = costo,
                Dependencias = proceso.Dependencias == null
                    ? new List<string>()
                    : new List<string>(proceso.Dependencias),
                Paralelo = proceso.Paralelo,
                Transversal = transversal,
                TieneOverrideLocalCalculo = false,
                OrigenCalculo = "Proceso local",
                Diagnostico = string.IsNullOrWhiteSpace(diagnostico)
                    ? proceso.Resultado?.Diagnostico ?? ""
                    : diagnostico
            };
        }

        private static FilaProductivaProyecto CrearFilaAsignacionDirecta(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            SubproductoProyecto sub,
            InstanciaSubproducto instancia,
            AsignacionProductiva asignacion
        )
        {
            return new FilaProductivaProyecto
            {
                ProyectoId = proyecto.Id,
                GrupoId = grupo.Id,
                ItemId = item.Id,
                ProductoProyectoId = item.Id,
                SubproductoProyectoId = sub.Id,
                InstanciaId = instancia.Id,
                ProcesoProyectoId = asignacion.ProcesoProyectoId,
                CargoId = asignacion.CargoId,
                PersonaId = asignacion.PersonaId,
                Cantidad = instancia.CantidadEquivalente,
                Unidad = sub.Unidad,
                Capacidad = 0m,
                Periodo = "",
                HorasCalculadas = asignacion.HorasCalculadas,
                HorasAsignadas = asignacion.HorasAsignadas > 0m ? asignacion.HorasAsignadas : asignacion.HorasCalculadas,
                Costo = asignacion.CostoCalculado,
                TieneOverrideLocalCalculo = false,
                OrigenCalculo = "Asignacion directa",
                Diagnostico = "Asignación directa de instancia."
            };
        }

        private static void ExpandirTransversales(
            ProyectoCotizacion proyecto,
            ProyectoProductivoExpandido expandido
        )
        {
            foreach (ProcesoTransversalProyecto proceso in proyecto.ProcesosTransversales ?? new List<ProcesoTransversalProyecto>())
            {
                if (proceso == null)
                {
                    continue;
                }

                decimal horasCalculadas = Convert.ToDecimal(proceso.HorasCalculadas);
                decimal horasAsignadas = Convert.ToDecimal(proceso.HorasAsignadas > 0.0 ? proceso.HorasAsignadas : proceso.HorasCalculadas);
                if (horasCalculadas <= 0m && proceso.SemanasActivas > 0.0 && proceso.HorasPorSemana > 0.0)
                {
                    horasCalculadas = Convert.ToDecimal(proceso.SemanasActivas * proceso.HorasPorSemana * (proceso.PorcentajeDedicacion / 100.0));
                    horasAsignadas = horasCalculadas;
                }

                expandido.Filas.Add(new FilaProductivaProyecto
                {
                    ProyectoId = proyecto.Id,
                    GrupoId = "grp_direccion_gestion",
                    ProcesoProyectoId = proceso.Id,
                    ProcesoBibliotecaId = proceso.ProcesoBibliotecaId,
                    TipoProceso = proceso.TipoProceso,
                    MetodoCalculo = MetodoCalculoProceso.PorDuracionProyecto,
                    AlcanceTemporal = proceso.AlcanceTemporal,
                    EtapaId = string.Join(";", proceso.EtapasCubiertas ?? new List<string>()),
                    CargoId = proceso.CargoId,
                    PersonaId = proceso.PersonaId,
                    Cantidad = Convert.ToDecimal(proceso.SemanasActivas),
                    Unidad = "semanas",
                    Capacidad = Convert.ToDecimal(proceso.HorasPorSemana),
                    Periodo = "semana",
                    HorasCalculadas = horasCalculadas,
                    HorasAsignadas = horasAsignadas,
                    Costo = 0m,
                    Paralelo = true,
                    Transversal = true,
                    TieneOverrideLocalCalculo = false,
                    OrigenCalculo = "Transversal",
                    Diagnostico = "Proceso transversal expandido una sola vez."
                });
            }
        }
    }
}
