using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoDesgloseGlobalService
    {
        public static DesgloseProductivoProyecto Construir(
            ProyectoCotizacion proyecto,
            Cotizacion contexto)
        {
            DesgloseProductivoProyecto global = new DesgloseProductivoProyecto();
            if (proyecto == null)
            {
                global.Diagnostico = "No hay un proyecto global para desglosar.";
                return global;
            }

            ProyectoProductivoExpandido expandido =
                ProyectoProductivoExpansionService.Expandir(proyecto);

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

                    Cotizacion cotizacionItem =
                        CotizacionItemProyectoAdapterService.CrearCotizacionDesdeItem(
                            item,
                            contexto);
                    List<RequerimientoProduccionInterna> requerimientosSnapshot =
                        cotizacionItem?.DesgloseProductivo?.Requerimientos;

                    if (requerimientosSnapshot != null &&
                        requerimientosSnapshot.Count > 0)
                    {
                        HashSet<string> identidadesAgregadas =
                            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (RequerimientoProduccionInterna req in requerimientosSnapshot)
                        {
                            if (req == null)
                            {
                                continue;
                            }

                            CompletarIdentidad(req, proyecto, grupo, item);
                            if (!identidadesAgregadas.Add(
                                CrearIdentidadRequerimiento(req)))
                            {
                                continue;
                            }

                            global.Requerimientos.Add(req);
                        }
                        continue;
                    }

                    foreach (FilaProductivaProyecto fila in
                        (expandido.Filas ?? new List<FilaProductivaProyecto>())
                        .Where(f => f != null && f.ItemId == item.Id))
                    {
                        global.Requerimientos.Add(
                            CrearRequerimientoDesdeFila(
                                proyecto,
                                grupo,
                                item,
                                fila));
                    }
                }
            }

            RecalcularTotales(global, contexto);
            global.Diagnostico =
                global.Requerimientos.Count == 0
                    ? "El proyecto todavía no tiene procesos productivos."
                    : global.Requerimientos.Count +
                      " requerimientos de todos los productos del proyecto.";
            return global;
        }

        public static void Aplicar(
            ProyectoCotizacion proyecto,
            DesgloseProductivoProyecto global,
            Cotizacion contexto)
        {
            if (proyecto == null || global?.Requerimientos == null)
            {
                return;
            }

            foreach (GrupoProyecto grupo in proyecto.Grupos ?? new List<GrupoProyecto>())
            {
                foreach (ItemProyecto item in grupo?.Items ?? new List<ItemProyecto>())
                {
                    List<RequerimientoProduccionInterna> requerimientos =
                        global.Requerimientos
                        .Where(r => r != null &&
                            string.Equals(
                                r.ItemId,
                                item.Id,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (requerimientos.Count == 0)
                    {
                        continue;
                    }

                    if (CotizacionItemProyectoAdapterService.ItemTieneSnapshot(item))
                    {
                        Cotizacion cotizacionItem =
                            CotizacionItemProyectoAdapterService.CrearCotizacionDesdeItem(
                                item,
                                contexto);
                        cotizacionItem.DesgloseProductivo =
                            new DesgloseProductivoProyecto
                            {
                                Requerimientos = requerimientos
                            };
                        RecalcularTotales(
                            cotizacionItem.DesgloseProductivo,
                            cotizacionItem);
                        CotizacionItemProyectoAdapterService.CapturarCotizacionEnItem(
                            item,
                            cotizacionItem);
                    }

                    AplicarAProcesosLocales(item, requerimientos);
                }
            }

            proyecto.FechaModificacion = DateTime.Now;
            RecalcularTotales(global, contexto);
        }

        private static RequerimientoProduccionInterna CrearRequerimientoDesdeFila(
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item,
            FilaProductivaProyecto fila)
        {
            SubproductoProyecto sub = (item.Subproductos ??
                new List<SubproductoProyecto>()).FirstOrDefault(s =>
                    s != null && s.Id == fila.SubproductoProyectoId);
            ProcesoProyecto proceso = BuscarProceso(item, fila.ProcesoProyectoId);
            decimal horas = fila.HorasAsignadas > 0m
                ? fila.HorasAsignadas
                : fila.HorasCalculadas;

            return new RequerimientoProduccionInterna
            {
                ProyectoId = proyecto.Id,
                GrupoId = grupo.Id,
                ItemId = item.Id,
                SubproductoProyectoId = fila.SubproductoProyectoId,
                InstanciaId = fila.InstanciaId,
                ProcesoId = fila.ProcesoProyectoId,
                EntregableCliente = sub == null ? item.Nombre : sub.Nombre,
                NombreRequerimiento = proceso?.Nombre ?? fila.ProcesoProyectoId,
                TipoInterno = proceso?.ProcesoBibliotecaId ?? fila.ProcesoBibliotecaId,
                EcuacionUsada = fila.ProcesoBibliotecaId,
                TipoProceso = fila.TipoProceso,
                MetodoCalculo = fila.MetodoCalculo,
                AlcanceTemporal = fila.AlcanceTemporal,
                Cantidad = Convert.ToDouble(fila.Cantidad),
                Unidad = fila.Unidad ?? "",
                EtapaSugerida = fila.EtapaId ?? "",
                BloqueProductivo = fila.EtapaId ?? "",
                ModoPlanificacion = fila.Paralelo ? "Paralelo" : "Secuencial",
                DependeDe = fila.Dependencias == null
                    ? ""
                    : string.Join("; ", fila.Dependencias),
                CargoId = fila.CargoId ?? "",
                CargoSugerido = fila.CargoId ?? "",
                PersonaId = fila.PersonaId ?? "",
                RendimientoCantidad = Convert.ToDouble(fila.Capacidad),
                RendimientoPeriodo = fila.Periodo ?? "",
                RendimientoOrigen = fila.OrigenCalculo ?? "",
                ModoCalculoProductivo =
                    fila.MetodoCalculo == MetodoCalculoProceso.Manual ||
                    fila.MetodoCalculo == MetodoCalculoProceso.Fijo
                        ? ModosCalculoProductivo.TiempoAsignado
                        : ModosCalculoProductivo.Rendimiento,
                HorasMinimas = Convert.ToDouble(horas),
                HorasEstandar = Convert.ToDouble(horas),
                HorasHolgura = Convert.ToDouble(horas),
                DiasPersonaMin = Convert.ToDouble(horas / 8m),
                DiasPersonaStd = Convert.ToDouble(horas / 8m),
                DiasPersonaHolgura = Convert.ToDouble(horas / 8m),
                CostoMinimoCLP = Convert.ToDouble(fila.Costo),
                CostoEstandarCLP = Convert.ToDouble(fila.Costo),
                CostoHolguraCLP = Convert.ToDouble(fila.Costo),
                TieneOverrideLocalCalculo = fila.TieneOverrideLocalCalculo,
                OrigenHoras = fila.OrigenCalculo ?? "",
                DiagnosticoParametros = fila.Diagnostico ?? ""
            };
        }

        private static void CompletarIdentidad(
            RequerimientoProduccionInterna req,
            ProyectoCotizacion proyecto,
            GrupoProyecto grupo,
            ItemProyecto item)
        {
            req.ProyectoId = proyecto.Id;
            req.GrupoId = grupo.Id;
            req.ItemId = item.Id;

            if (string.IsNullOrWhiteSpace(req.SubproductoProyectoId))
            {
                SubproductoProyecto sub = (item.Subproductos ??
                    new List<SubproductoProyecto>()).FirstOrDefault(s =>
                        s != null &&
                        (TextoRelacionado(s.Nombre, req.EntregableCliente) ||
                         TextoRelacionado(s.Nombre, req.NombreRequerimiento) ||
                         TextoRelacionado(s.SubproductoBibliotecaId, req.TipoInterno)));
                req.SubproductoProyectoId = sub?.Id ?? "";
            }
        }

        private static string CrearIdentidadRequerimiento(
            RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "";
            }

            string proceso = string.IsNullOrWhiteSpace(req.ProcesoId)
                ? (req.TipoInterno ?? "") + "|" + (req.NombreRequerimiento ?? "")
                : req.ProcesoId;
            string cargo = string.IsNullOrWhiteSpace(req.CargoId)
                ? req.CargoSugerido ?? ""
                : req.CargoId;

            return string.Join(
                "|",
                req.ItemId ?? "",
                req.SubproductoProyectoId ?? "",
                req.InstanciaId ?? "",
                proceso,
                cargo,
                req.PersonaId ?? "",
                req.Cantidad.ToString("R",
                    System.Globalization.CultureInfo.InvariantCulture),
                req.HorasEstandar.ToString("R",
                    System.Globalization.CultureInfo.InvariantCulture),
                req.CostoEstandarCLP.ToString("R",
                    System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        private static void AplicarAProcesosLocales(
            ItemProyecto item,
            List<RequerimientoProduccionInterna> requerimientos)
        {
            foreach (IGrouping<string, RequerimientoProduccionInterna> grupo in
                requerimientos
                .Where(r => !string.IsNullOrWhiteSpace(r.ProcesoId))
                .GroupBy(r => r.ProcesoId, StringComparer.OrdinalIgnoreCase))
            {
                ProcesoProyecto proceso = BuscarProceso(item, grupo.Key);
                if (proceso == null)
                {
                    continue;
                }

                List<RequerimientoProduccionInterna> filas = grupo.ToList();
                RequerimientoProduccionInterna principal = filas[0];
                proceso.Cantidad = Convert.ToDecimal(principal.Cantidad);
                proceso.Unidad = principal.Unidad ?? proceso.Unidad;
                proceso.Capacidad = Convert.ToDecimal(principal.RendimientoCantidad);
                proceso.Periodo = principal.RendimientoPeriodo ?? "";
                proceso.EtapaId = principal.EtapaSugerida ?? "";
                proceso.Dependencias = (principal.DependeDe ?? "")
                    .Split(new[] { ';', ',', '|' },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Trim())
                    .Where(v => v.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                proceso.Resultado ??= new ResultadoProcesoProyecto();
                proceso.Resultado.HorasCalculadas =
                    Convert.ToDecimal(filas.Sum(r => r.HorasEstandar));
                proceso.Resultado.HorasAsignadas =
                    proceso.Resultado.HorasCalculadas;
                proceso.Resultado.CostoCalculado =
                    Convert.ToDecimal(filas.Sum(r => r.CostoEstandarCLP));

                foreach (RequerimientoProduccionInterna req in filas)
                {
                    AsignacionProductiva asignacion =
                        (proceso.Asignaciones ??
                            new List<AsignacionProductiva>()).FirstOrDefault(a =>
                                a != null &&
                                (string.Equals(a.CargoId, req.CargoId,
                                     StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(a.CargoId, req.CargoSugerido,
                                     StringComparison.OrdinalIgnoreCase)));
                    if (asignacion == null)
                    {
                        continue;
                    }

                    asignacion.CargoId = string.IsNullOrWhiteSpace(req.CargoId)
                        ? req.CargoSugerido
                        : req.CargoId;
                    asignacion.PersonaId = req.PersonaId ?? "";
                    asignacion.HorasCalculadas =
                        Convert.ToDecimal(req.HorasEstandar);
                    asignacion.HorasAsignadas = asignacion.HorasCalculadas;
                    asignacion.CostoCalculado =
                        Convert.ToDecimal(req.CostoEstandarCLP);
                    asignacion.OrigenHoras = OrigenHorasProductivas.Manual;
                }
            }
        }

        private static ProcesoProyecto BuscarProceso(
            ItemProyecto item,
            string procesoId)
        {
            IEnumerable<ProcesoProyecto> procesos =
                (item.Procesos ?? new List<ProcesoProyecto>())
                .Concat((item.Subproductos ?? new List<SubproductoProyecto>())
                    .Where(s => s != null)
                    .SelectMany(s => s.Procesos ??
                        new List<ProcesoProyecto>()))
                .Concat((item.Subproductos ?? new List<SubproductoProyecto>())
                    .Where(s => s != null)
                    .SelectMany(s => s.Instancias ??
                        new List<InstanciaSubproducto>())
                    .Where(i => i != null)
                    .SelectMany(i => i.Procesos ??
                        new List<ProcesoProyecto>()));
            return procesos.FirstOrDefault(p => p != null &&
                string.Equals(
                    p.Id,
                    procesoId,
                    StringComparison.OrdinalIgnoreCase));
        }

        private static bool TextoRelacionado(string a, string b)
        {
            string na = (a ?? "").Trim().ToLowerInvariant();
            string nb = (b ?? "").Trim().ToLowerInvariant();
            return na.Length > 0 && nb.Length > 0 &&
                (na == nb || na.Contains(nb) || nb.Contains(na));
        }

        private static void RecalcularTotales(
            DesgloseProductivoProyecto desglose,
            Cotizacion contexto)
        {
            List<RequerimientoProduccionInterna> filas =
                desglose?.Requerimientos ??
                new List<RequerimientoProduccionInterna>();
            double diasSemana = contexto != null &&
                contexto.DiasHabilesEstudioPorSemana > 0
                    ? contexto.DiasHabilesEstudioPorSemana
                    : 5.0;
            desglose.DiasPersonaMinimos = filas.Sum(r => r.DiasPersonaMin);
            desglose.DiasPersonaEstandar = filas.Sum(r => r.DiasPersonaStd);
            desglose.DiasPersonaHolgura = filas.Sum(r => r.DiasPersonaHolgura);
            desglose.SemanasMinimas =
                desglose.DiasPersonaMinimos / diasSemana;
            desglose.SemanasEstandar =
                desglose.DiasPersonaEstandar / diasSemana;
            desglose.SemanasHolgura =
                desglose.DiasPersonaHolgura / diasSemana;
            desglose.CostoMinimoCLP = filas.Sum(r => r.CostoMinimoCLP);
            desglose.CostoEstandarCLP = filas.Sum(r => r.CostoEstandarCLP);
            desglose.CostoHolguraCLP = filas.Sum(r => r.CostoHolguraCLP);
        }
    }
}
