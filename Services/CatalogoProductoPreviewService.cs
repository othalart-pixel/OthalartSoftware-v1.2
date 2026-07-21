using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class CatalogoProductoPreviewService
    {
        private static readonly string[] OrdenEtapas =
        {
            "Desarrollo",
            "Preproduccion",
            "Produccion",
            "Postproduccion"
        };

        public static CatalogoProductoPreview Calcular(
            Producto2DDefinicion producto,
            Cotizacion contexto,
            ProductQuoteConfiguration configuracion = null
        )
        {
            CatalogoProductoPreview preview = new CatalogoProductoPreview
            {
                Producto = producto
            };

            if (producto == null)
            {
                preview.Validaciones.Add("No hay producto seleccionado");
                return preview;
            }

            configuracion ??= ProductQuoteConfigurationService.Crear(producto);
            Cotizacion temporal = CrearCotizacionTemporal(producto, contexto, configuracion);
            DesgloseProductivoProyecto desglose = DesgloseProductivoService.Generar(temporal);
            temporal.DesgloseProductivo = desglose;
            ResultadoEscenarioOfertaDesglose oferta =
                EscenarioOfertaDesdeDesgloseService.Calcular(temporal);

            List<Subproducto2D> subproductos = (producto.Subproductos ?? new List<Subproducto2D>())
                .Where(s => s != null)
                .OrderBy(s => s.Orden <= 0 ? int.MaxValue : s.Orden)
                .ThenBy(s => s.Nombre)
                .ToList();

            foreach (Subproducto2D subproducto in subproductos)
            {
                RequerimientoProduccionInterna req =
                    BuscarRequerimientoParaSubproducto(desglose, subproducto);
                CatalogoProcesoPreview proceso =
                    CrearProcesoPreview(subproducto, req);
                preview.Procesos.Add(proceso);
            }

            preview.TotalHoras = preview.Procesos.Sum(p => p.Horas);
            preview.CostoDirectoCLP = preview.Procesos.Sum(p => p.CostoTotalCLP);
            preview.PrecioSugeridoCLP = oferta.PrecioEstandarCLP > 0.0
                ? oferta.PrecioEstandarCLP
                : 0.0;
            preview.MargenEstimado = oferta.MargenBase;
            preview.CantidadProcesos = preview.Procesos.Count;
            preview.CantidadCargos = preview.Procesos
                .SelectMany(p => SepararCargos(p.Cargos))
                .Select(Normalizar)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .Count();

            ProrratearPrecioProcesos(preview);
            preview.Etapas = ConstruirEtapas(preview);
            preview.Validaciones = preview.Procesos
                .SelectMany(p => p.DependenciasFaltantes.Select(f => p.SubproductoNombre + ": " + f))
                .Distinct()
                .ToList();

            foreach (string validacion in ProductQuoteConfigurationService.Validar(configuracion))
            {
                preview.Validaciones.Add(validacion);
            }

            if (!string.IsNullOrWhiteSpace(desglose.Diagnostico))
            {
                preview.Validaciones.Add(desglose.Diagnostico);
            }

            return preview;
        }

        private static Cotizacion CrearCotizacionTemporal(
            Producto2DDefinicion producto,
            Cotizacion contexto,
            ProductQuoteConfiguration configuracion
        )
        {
            Cotizacion temporal = new Cotizacion
            {
                NombreProyecto = "Preview catalogo - " + producto.Nombre,
                Moneda = contexto == null ? "CLP" : contexto.Moneda,
                MonedaBase = contexto == null ? "CLP" : contexto.MonedaBase,
                MonedaVisualizacion = contexto == null ? "CLP" : contexto.MonedaVisualizacion,
                MonedaPrecioCliente = contexto == null ? "CLP" : contexto.MonedaPrecioCliente,
                TiposCambio = contexto == null
                    ? new List<TipoCambio>()
                    : contexto.TiposCambio.Select(t => new TipoCambio
                    {
                        Codigo = t.Codigo,
                        Nombre = t.Nombre,
                        ValorEnCLP = t.ValorEnCLP,
                        Fuente = t.Fuente,
                        FechaActualizacion = t.FechaActualizacion
                    }).ToList(),
                DiasHabilesEstudioPorSemana = contexto == null ||
                    contexto.DiasHabilesEstudioPorSemana <= 0.0
                        ? 5.0
                        : contexto.DiasHabilesEstudioPorSemana
            };

            temporal.BriefProducto.ProductoServicioSeleccionado = producto.Nombre;
            temporal.BriefProducto.TipoProducto = producto.Nombre;
            temporal.BriefProducto.IndustriaCliente = producto.Industria;
            temporal.BriefProducto.DuracionProductoValor = EsUnidadDuracion(configuracion.CommercialUnit)
                ? (double)configuracion.Quantity
                : producto.DuracionSugerida;
            temporal.BriefProducto.DuracionProductoUnidad = producto.UnidadDuracionSugerida;
            temporal.BriefProducto.CantidadGlobalProducto = (int)Math.Max(1, Math.Round(configuracion.Quantity));

            foreach (Subproducto2D subproducto in producto.Subproductos ?? new List<Subproducto2D>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                temporal.BriefProducto.EntregablesSeleccionados.Add(
                    CrearEntregable(producto, subproducto, configuracion)
                );
            }

            return temporal;
        }

        private static EntregableBrief CrearEntregable(
            Producto2DDefinicion producto,
            Subproducto2D subproducto,
            ProductQuoteConfiguration configuracion
        )
        {
            bool usaDuracion = !string.Equals(
                producto.UnidadDuracionSugerida,
                "no aplica",
                StringComparison.OrdinalIgnoreCase
            );

            EntregableBrief entregable = new EntregableBrief
            {
                Categoria = string.IsNullOrWhiteSpace(subproducto.Categoria)
                    ? subproducto.EtapaSugerida
                    : subproducto.Categoria,
                Nombre = subproducto.Nombre ?? "",
                ClaveSeleccion =
                    (subproducto.EtapaSugerida ?? "") + "|" +
                    (subproducto.SubEtapaSugerida ?? "") + "|" +
                    (subproducto.Nombre ?? ""),
                Cantidad = 1,
                DuracionPorUnidad = usaDuracion ? producto.DuracionSugerida : 0.0,
                UnidadDuracion = usaDuracion ? producto.UnidadDuracionSugerida : "no aplica",
                UnidadCantidad = string.IsNullOrWhiteSpace(producto.UnidadCantidadSugerida)
                    ? "unidades"
                    : producto.UnidadCantidadSugerida,
                Nota = subproducto.Nota ?? "",
                EtapaSugerida = subproducto.EtapaSugerida ?? "",
                SubEtapaSugerida = subproducto.SubEtapaSugerida ?? "",
                DependeDe = subproducto.DependeDe ?? "",
                CargosSugeridos = subproducto.CargosSugeridos ?? "",
                EquationKey = subproducto.EquationKey ?? "",
                EcuacionProductiva = subproducto.EcuacionProductiva ?? "",
                VariablesEcuacion = subproducto.VariablesEcuacion ?? "",
                ImpactoEcuacion = subproducto.ImpactoEcuacion ?? "",
                ModoCalculoProductivo =
                    ModosCalculoProductivo.Normalizar(subproducto.ModoCalculoProductivo),
                HorasAsignadasMin = subproducto.HorasAsignadasMin,
                HorasAsignadasStd = subproducto.HorasAsignadasStd,
                HorasAsignadasHolgura = subproducto.HorasAsignadasHolgura
            };

            ProductQuoteConfigurationService.AplicarAEntregable(configuracion, entregable);
            return entregable;
        }

        private static RequerimientoProduccionInterna BuscarRequerimientoParaSubproducto(
            DesgloseProductivoProyecto desglose,
            Subproducto2D subproducto
        )
        {
            if (desglose == null || subproducto == null)
            {
                return null;
            }

            string nombre = Normalizar(subproducto.Nombre);
            string etapa = Normalizar(subproducto.EtapaSugerida);
            string proceso = Normalizar(subproducto.SubEtapaSugerida);

            return desglose.Requerimientos.FirstOrDefault(r =>
                r != null &&
                Normalizar(r.EntregableCliente) == nombre &&
                (
                    string.IsNullOrWhiteSpace(etapa) ||
                    Normalizar(r.EtapaSugerida) == etapa
                ) &&
                (
                    string.IsNullOrWhiteSpace(proceso) ||
                    Normalizar(r.TipoInterno) == proceso ||
                    Normalizar(r.NombreRequerimiento) == proceso
                )
            ) ?? desglose.Requerimientos.FirstOrDefault(r =>
                r != null && Normalizar(r.EntregableCliente) == nombre
            );
        }

        private static CatalogoProcesoPreview CrearProcesoPreview(
            Subproducto2D subproducto,
            RequerimientoProduccionInterna req
        )
        {
            CatalogoProcesoPreview proceso = new CatalogoProcesoPreview
            {
                Subproducto = subproducto,
                Requerimiento = req,
                Etapa = NormalizarEtapaVisible(
                    string.IsNullOrWhiteSpace(subproducto.EtapaSugerida)
                        ? subproducto.Categoria
                        : subproducto.EtapaSugerida
                ),
                Proceso = string.IsNullOrWhiteSpace(subproducto.SubEtapaSugerida)
                    ? subproducto.Nombre
                    : subproducto.SubEtapaSugerida,
                SubproductoNombre = subproducto.Nombre ?? "",
                Cargos = req == null || string.IsNullOrWhiteSpace(req.CargoSugerido)
                    ? subproducto.CargosSugeridos ?? ""
                    : req.CargoSugerido,
                Ecuacion = req == null || string.IsNullOrWhiteSpace(req.EcuacionUsada)
                    ? subproducto.EcuacionProductiva ?? ""
                    : req.EcuacionUsada
            };

            if (req != null)
            {
                proceso.Horas = req.HorasEstandar;
                proceso.CostoManoObraCLP = req.CostoEstandarCLP;
                proceso.CostoTotalCLP = req.CostoEstandarCLP;
                proceso.DetalleCalculo = CrearDetalle(req);
            }
            else
            {
                proceso.DependenciasFaltantes.Add("No se genero requerimiento de calculo");
            }

            AgregarFaltantes(proceso, req, subproducto);
            proceso.Estado = proceso.DependenciasFaltantes.Count == 0
                ? "OK"
                : proceso.Horas > 0.0 || proceso.CostoTotalCLP > 0.0
                    ? "Incompleto"
                    : "No calculable";

            return proceso;
        }

        private static void AgregarFaltantes(
            CatalogoProcesoPreview proceso,
            RequerimientoProduccionInterna req,
            Subproducto2D subproducto
        )
        {
            if (string.IsNullOrWhiteSpace(proceso.Cargos))
            {
                proceso.DependenciasFaltantes.Add("Falta cargo asociado");
            }

            if (string.IsNullOrWhiteSpace(proceso.Ecuacion))
            {
                proceso.DependenciasFaltantes.Add("Falta ecuacion");
            }

            if (string.IsNullOrWhiteSpace(proceso.Proceso))
            {
                proceso.DependenciasFaltantes.Add("Falta proceso");
            }

            if (req == null)
            {
                return;
            }

            if (req.TarifaDiaCargoCLP <= 0.0)
            {
                proceso.DependenciasFaltantes.Add("Falta valor hora");
            }

            if (!ModosCalculoProductivo.EsTiempoAsignado(req.ModoCalculoProductivo) &&
                req.RendimientoCantidad <= 0.0)
            {
                proceso.DependenciasFaltantes.Add("Falta rendimiento");
            }

            if (req.Cantidad <= 0.0)
            {
                proceso.DependenciasFaltantes.Add("Falta parametro de cantidad");
            }

            if (!req.ParametrosCompletos &&
                !string.IsNullOrWhiteSpace(req.DiagnosticoParametros))
            {
                foreach (string item in req.DiagnosticoParametros.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    proceso.DependenciasFaltantes.Add(item.Trim());
                }
            }

            proceso.DependenciasFaltantes = proceso.DependenciasFaltantes
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct()
                .ToList();
        }

        private static string CrearDetalle(RequerimientoProduccionInterna req)
        {
            double tarifaHora = req.TarifaDiaCargoCLP <= 0.0
                ? 0.0
                : req.TarifaDiaCargoCLP / CalculoProductivoResolverService.HorasDiaEstandar;

            return
                "Tiempo base: " + FormatearHoras(req.HorasEstandar) + "\n" +
                "Cantidad: " + req.Cantidad.ToString("0.##") + " " + req.Unidad + "\n" +
                "Rendimiento: " + (req.RendimientoCantidad > 0.0
                    ? req.RendimientoCantidad.ToString("0.##") + " / " + req.RendimientoPeriodo
                    : "No definido") + "\n" +
                "Tiempo calculado: " + FormatearHoras(req.HorasEstandar) + "\n" +
                "Cargo: " + (string.IsNullOrWhiteSpace(req.CargoSugerido) ? "No definido" : req.CargoSugerido) + "\n" +
                "Valor hora: " + (tarifaHora > 0.0 ? tarifaHora.ToString("N0") + " CLP" : "No definido") + "\n" +
                "Costo mano de obra: " + req.CostoEstandarCLP.ToString("N0") + " CLP\n" +
                "Costos adicionales: 0 CLP\n" +
                "Costo total: " + req.CostoEstandarCLP.ToString("N0") + " CLP\n" +
                "Ecuacion utilizada: " + (string.IsNullOrWhiteSpace(req.EcuacionUsada) ? "No definida" : req.EcuacionUsada);
        }

        private static void ProrratearPrecioProcesos(CatalogoProductoPreview preview)
        {
            if (preview == null || preview.PrecioSugeridoCLP <= 0.0)
            {
                return;
            }

            double costoTotal = preview.Procesos.Sum(p => p.CostoTotalCLP);
            foreach (CatalogoProcesoPreview proceso in preview.Procesos)
            {
                proceso.PrecioSugeridoCLP = costoTotal <= 0.0
                    ? 0.0
                    : preview.PrecioSugeridoCLP * (proceso.CostoTotalCLP / costoTotal);
            }
        }

        private static List<CatalogoEtapaPreview> ConstruirEtapas(CatalogoProductoPreview preview)
        {
            List<CatalogoEtapaPreview> etapas = new List<CatalogoEtapaPreview>();
            foreach (string etapaBase in OrdenEtapas)
            {
                List<CatalogoProcesoPreview> procesos = preview.Procesos
                    .Where(p => Normalizar(p.Etapa) == Normalizar(etapaBase))
                    .ToList();

                CatalogoEtapaPreview etapa = new CatalogoEtapaPreview
                {
                    Etapa = etapaBase,
                    CantidadProcesos = procesos.Count,
                    Horas = procesos.Sum(p => p.Horas),
                    CostoTotalCLP = procesos.Sum(p => p.CostoTotalCLP),
                    PrecioSugeridoCLP = procesos.Sum(p => p.PrecioSugeridoCLP),
                    Estado = procesos.Count == 0
                        ? "Sin procesos configurados"
                        : procesos.Any(p => p.Estado != "OK") ? "Incompleto" : "OK",
                    Cargos = procesos
                        .SelectMany(p => SepararCargos(p.Cargos))
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .ToList()
                };

                etapa.PorcentajeCosto = preview.CostoDirectoCLP <= 0.0
                    ? 0.0
                    : etapa.CostoTotalCLP / preview.CostoDirectoCLP;
                etapas.Add(etapa);
            }

            foreach (IGrouping<string, CatalogoProcesoPreview> grupo in preview.Procesos
                .Where(p => !OrdenEtapas.Any(e => Normalizar(e) == Normalizar(p.Etapa)))
                .GroupBy(p => p.Etapa))
            {
                List<CatalogoProcesoPreview> procesos = grupo.ToList();
                etapas.Add(new CatalogoEtapaPreview
                {
                    Etapa = grupo.Key,
                    CantidadProcesos = procesos.Count,
                    Horas = procesos.Sum(p => p.Horas),
                    CostoTotalCLP = procesos.Sum(p => p.CostoTotalCLP),
                    PrecioSugeridoCLP = procesos.Sum(p => p.PrecioSugeridoCLP),
                    PorcentajeCosto = preview.CostoDirectoCLP <= 0.0
                        ? 0.0
                        : procesos.Sum(p => p.CostoTotalCLP) / preview.CostoDirectoCLP,
                    Estado = procesos.Any(p => p.Estado != "OK") ? "Incompleto" : "OK",
                    Cargos = procesos.SelectMany(p => SepararCargos(p.Cargos)).Distinct().ToList()
                });
            }

            return etapas;
        }

        private static IEnumerable<string> SepararCargos(string cargos)
        {
            return (cargos ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim());
        }

        private static string FormatearHoras(double horas)
        {
            return horas <= 0.0 ? "No calculable" : horas.ToString("0.##") + " h";
        }

        private static string NormalizarEtapaVisible(string etapa)
        {
            string normalizada = Normalizar(etapa);
            if (normalizada == "preproduccion")
            {
                return "Preproduccion";
            }

            if (normalizada == "postproduccion")
            {
                return "Postproduccion";
            }

            if (normalizada == "desarrollo")
            {
                return "Desarrollo";
            }

            if (normalizada == "produccion" || string.IsNullOrWhiteSpace(normalizada))
            {
                return "Produccion";
            }

            return etapa;
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
                .Replace("ú", "u");
        }

        private static bool EsUnidadDuracion(string unidad)
        {
            string u = (unidad ?? "").Trim().ToLowerInvariant();
            return u.Contains("segundo") || u == "s" || u.Contains("minuto") || u.Contains("frame");
        }
    }
}
