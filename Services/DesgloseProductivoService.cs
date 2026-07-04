using System.Diagnostics;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class DesgloseProductivoService
    {
        private const double DiasTrabajoSemana = 5.0;
        private const double HorasGestionPorDiaPersonaProductivo = 10.0 / 60.0;
        private const double TarifaHoraGestionCLP = ServicioCotizacion.CostoHoraGestionCLP;

        private sealed class CargoPipelinePonderado
        {
            public string TextoOriginal { get; set; } = "";
            public string Cargo { get; set; } = "";
            public double Ponderador { get; set; } = 1.0;
        }

        public static DesgloseProductivoProyecto Generar(Cotizacion cotizacion)
        {
            DesgloseProductivoProyecto desglose = new DesgloseProductivoProyecto();

            if (cotizacion == null ||
                cotizacion.BriefProducto == null ||
                cotizacion.BriefProducto.EntregablesSeleccionados == null ||
                cotizacion.BriefProducto.EntregablesSeleccionados.Count == 0)
            {
                desglose.Diagnostico =
                    "No hay entregables seleccionados para generar desglose productivo.";

                Debug.WriteLine("[DESGLOSE] No hay entregables seleccionados.");

                return desglose;
            }

            int totalEntregables = cotizacion.BriefProducto.EntregablesSeleccionados.Count;
            int totalConEcuacion = 0;
            int totalSinEcuacion = 0;
            int totalDesdePipelineJson = 0;
            int totalDesdeEcuacionJson = 0;
            int totalRequerimientosAntes = 0;
            int totalRequerimientosDespues = 0;

            Debug.WriteLine("========================================");
            Debug.WriteLine("[DESGLOSE] INICIO GENERACIÓN");
            Debug.WriteLine("[DESGLOSE] Entregables seleccionados: " + totalEntregables);
            Debug.WriteLine("========================================");

            foreach (EntregableBrief entregable in cotizacion.BriefProducto.EntregablesSeleccionados)
            {
                if (entregable == null)
                {
                    Debug.WriteLine("[DESGLOSE] Entregable null. Se omite.");
                    totalSinEcuacion++;
                    continue;
                }

                string nombreEntregable = ObtenerNombreEntregableDebug(entregable);

                if (DebeGenerarseDesdePipelineJson(entregable))
                {
                    totalDesdePipelineJson++;
                    totalConEcuacion++;
                    totalRequerimientosAntes = desglose.Requerimientos.Count;

                    desglose.Requerimientos.Add(
                        CrearRequerimientoFallbackDesdePipeline(entregable)
                    );

                    totalRequerimientosDespues = desglose.Requerimientos.Count;

                    Debug.WriteLine(
                        "[DESGLOSE] DESDE PIPELINE JSON -> " +
                        nombreEntregable +
                        " | Requerimientos agregados: " +
                        (totalRequerimientosDespues - totalRequerimientosAntes)
                    );

                    continue;
                }

                if (VieneDeTablaProductoEditable(entregable))
                {
                    totalSinEcuacion++;
                    desglose.Requerimientos.Add(
                        CrearRequerimientoSinDefinicion(
                            entregable,
                            "Subproducto sin definición productiva explícita"
                        )
                    );

                    Debug.WriteLine(
                        "[DESGLOSE] SUBPRODUCTO SIN DEFINICIÓN EXPLÍCITA -> " +
                        nombreEntregable
                    );

                    continue;
                }

                if (TryAgregarRequerimientoDesdeEcuacionJson(
                    desglose,
                    entregable,
                    out string ecuacionJsonUsada))
                {
                    totalDesdeEcuacionJson++;
                    totalConEcuacion++;

                    Debug.WriteLine(
                        "[DESGLOSE] DESDE ECUACION JSON -> " +
                        nombreEntregable +
                        " | " +
                        ecuacionJsonUsada
                    );

                    continue;
                }

                IEcuacionProductiva2D ecuacion =
                    BibliotecaEcuacionesProductivas2D.Buscar(entregable);

                if (ecuacion == null)
                {
                    totalSinEcuacion++;
                    desglose.Requerimientos.Add(
                        CrearRequerimientoSinDefinicion(entregable, "Ecuación productiva no definida")
                    );

                    Debug.WriteLine(
                        "[DESGLOSE] SIN ECUACIÓN -> " +
                        nombreEntregable
                    );

                    continue;
                }

                totalConEcuacion++;

                Debug.WriteLine(
                    "[DESGLOSE] CON ECUACIÓN -> " +
                    nombreEntregable +
                    " | " +
                    ecuacion.GetType().Name
                );

                totalRequerimientosAntes = desglose.Requerimientos.Count;

                ecuacion.Calcular(entregable, desglose);

                totalRequerimientosDespues = desglose.Requerimientos.Count;

                Debug.WriteLine(
                    "[DESGLOSE] Requerimientos agregados: " +
                    (totalRequerimientosDespues - totalRequerimientosAntes)
                );
            }

            AsegurarRequerimientosDesdePipelineSeleccionado(desglose, cotizacion);
            CompactarRequerimientosDuplicados(desglose);
            AplicarRendimientosProductivos(desglose, cotizacion);
            ValidarParametrosDesglose(desglose);
            RecalcularTotales(desglose, cotizacion);

            Debug.WriteLine("========================================");
            Debug.WriteLine("[DESGLOSE] FIN GENERACIÓN");
            Debug.WriteLine("[DESGLOSE] Total entregables: " + totalEntregables);
            Debug.WriteLine("[DESGLOSE] Con ecuación: " + totalConEcuacion);
            Debug.WriteLine("[DESGLOSE] Sin ecuación: " + totalSinEcuacion);
            Debug.WriteLine("[DESGLOSE] Requerimientos finales: " + desglose.Requerimientos.Count);
            Debug.WriteLine("========================================");

            desglose.Diagnostico =
                desglose.Diagnostico +
                " | Diagnóstico técnico: " +
                totalEntregables +
                " entregables seleccionados, " +
                totalConEcuacion +
                " con ecuación, " +
                totalDesdePipelineJson +
                " desde pipeline JSON, " +
                totalDesdeEcuacionJson +
                " desde ecuaciones JSON, " +
                totalSinEcuacion +
                " sin ecuación, " +
                desglose.Requerimientos.Count +
                " requerimientos generados.";

            return desglose;
        }

        private static bool DebeGenerarseDesdePipelineJson(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return false;
            }

            return
                !string.IsNullOrWhiteSpace(entregable.EcuacionProductiva) ||
                !string.IsNullOrWhiteSpace(entregable.VariablesEcuacion) ||
                !string.IsNullOrWhiteSpace(entregable.ImpactoEcuacion) ||
                !string.IsNullOrWhiteSpace(entregable.SubEtapaSugerida) ||
                !string.IsNullOrWhiteSpace(entregable.CargosSugeridos) ||
                !string.IsNullOrWhiteSpace(entregable.DependeDe);
        }

        private static bool VieneDeTablaProductoEditable(EntregableBrief entregable)
        {
            return entregable != null &&
                !string.IsNullOrWhiteSpace(entregable.ClaveSeleccion);
        }

        private static bool TryAgregarRequerimientoDesdeEcuacionJson(
            DesgloseProductivoProyecto desglose,
            EntregableBrief entregable,
            out string ecuacionUsada
        )
        {
            ecuacionUsada = "";

            if (desglose == null || entregable == null)
            {
                return false;
            }

            EcuacionProductivaDefinicion ecuacion =
                BibliotecaEcuacionesProductivasJsonService.BuscarMejorPara(
                    string.IsNullOrWhiteSpace(entregable.EtapaSugerida)
                        ? entregable.Categoria
                        : entregable.EtapaSugerida,
                    entregable.SubEtapaSugerida,
                    entregable.Nombre,
                    entregable.CargosSugeridos
                );

            if (ecuacion == null)
            {
                return false;
            }

            EntregableBrief enriquecido =
                CrearEntregableEnriquecidoConEcuacionJson(entregable, ecuacion);

            desglose.Requerimientos.Add(
                CrearRequerimientoFallbackDesdePipeline(enriquecido)
            );

            ecuacionUsada = FormatearEcuacionJson(ecuacion);
            return true;
        }

        private static EntregableBrief CrearEntregableEnriquecidoConEcuacionJson(
            EntregableBrief origen,
            EcuacionProductivaDefinicion ecuacion
        )
        {
            return new EntregableBrief
            {
                Categoria = origen.Categoria,
                Nombre = origen.Nombre,
                ClaveSeleccion = origen.ClaveSeleccion,
                Cantidad = origen.Cantidad,
                DuracionPorUnidad = origen.DuracionPorUnidad,
                UnidadDuracion = origen.UnidadDuracion,
                UnidadCantidad = origen.UnidadCantidad,
                Nota = origen.Nota,
                EtapaSugerida = string.IsNullOrWhiteSpace(origen.EtapaSugerida)
                    ? ecuacion.Etapa
                    : origen.EtapaSugerida,
                SubEtapaSugerida = string.IsNullOrWhiteSpace(origen.SubEtapaSugerida)
                    ? ecuacion.SubEtapa
                    : origen.SubEtapaSugerida,
                DependeDe = origen.DependeDe,
                CargosSugeridos = string.IsNullOrWhiteSpace(origen.CargosSugeridos)
                    ? ObtenerCargosPonderadosDesdeEcuacion(ecuacion)
                    : origen.CargosSugeridos,
                CargosParticipantesJson = string.IsNullOrWhiteSpace(origen.CargosParticipantesJson)
                    ? ecuacion.CargosParticipantesJson
                    : origen.CargosParticipantesJson,
                EcuacionProductiva = string.IsNullOrWhiteSpace(origen.EcuacionProductiva)
                    ? FormatearEcuacionJson(ecuacion)
                    : origen.EcuacionProductiva,
                VariablesEcuacion = string.IsNullOrWhiteSpace(origen.VariablesEcuacion)
                    ? ecuacion.Variables
                    : origen.VariablesEcuacion,
                ImpactoEcuacion = string.IsNullOrWhiteSpace(origen.ImpactoEcuacion)
                    ? ecuacion.Impacto
                    : origen.ImpactoEcuacion,
                PlanosEstimados = origen.PlanosEstimados,
                BackgroundsEstimados = origen.BackgroundsEstimados,
                PersonajesEstimados = origen.PersonajesEstimados,
                PropsEstimados = origen.PropsEstimados,
                SegundosAnimadosEfectivos = origen.SegundosAnimadosEfectivos,
                NivelCalidadEstimado = origen.NivelCalidadEstimado,
                RevisionesIncluidas = origen.RevisionesIncluidas,
                SupuestosProduccionEditadosManualmente =
                    origen.SupuestosProduccionEditadosManualmente
            };
        }

        private static string FormatearEcuacionJson(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(ecuacion.NombreVisible))
            {
                return ecuacion.Clave ?? "";
            }

            if (string.IsNullOrWhiteSpace(ecuacion.Clave))
            {
                return ecuacion.NombreVisible;
            }

            return ecuacion.Clave + " | " + ecuacion.NombreVisible;
        }

        private static string ObtenerCargosPonderadosDesdeEcuacion(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            List<EcuacionProductivaRuntimeService.CargoVector> vector =
                EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion);

            if (vector.Count == 0)
            {
                return ecuacion.CargosPermitidos ?? "";
            }

            return EcuacionProductivaRuntimeService.SerializarVectorCargos(vector);
        }

        private static void AsegurarRequerimientosDesdePipelineSeleccionado(
            DesgloseProductivoProyecto desglose,
            Cotizacion cotizacion
        )
        {
            if (desglose == null ||
                cotizacion == null ||
                cotizacion.BriefProducto == null ||
                cotizacion.BriefProducto.EntregablesSeleccionados == null)
            {
                return;
            }

            foreach (EntregableBrief entregable in cotizacion.BriefProducto.EntregablesSeleccionados)
            {
                if (entregable == null || string.IsNullOrWhiteSpace(entregable.Nombre))
                {
                    continue;
                }

                string claveEntregable = ConstruirClaveEntregableSeleccionado(entregable);
                bool yaExiste = desglose.Requerimientos.Any(r =>
                    r != null &&
                    (
                        ConstruirClaveRequerimientoOrigen(r) == claveEntregable ||
                        ConstruirClaveBaseEntregable(r) == ConstruirClaveBaseEntregable(entregable)
                    ));

                if (yaExiste)
                {
                    continue;
                }

                desglose.Requerimientos.Add(
                    CrearRequerimientoFallbackDesdePipeline(entregable)
                );
            }
        }

        private static string ConstruirClaveEntregableSeleccionado(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "";
            }

            return NormalizarCompactacion(entregable.Nombre) + "|" +
                NormalizarCompactacion(entregable.Categoria) + "|" +
                NormalizarCompactacion(entregable.EtapaSugerida) + "|" +
                NormalizarCompactacion(entregable.SubEtapaSugerida) + "|" +
                NormalizarCompactacion(entregable.EcuacionProductiva) + "|" +
                NormalizarCompactacion(entregable.CargosSugeridos);
        }

        private static string ConstruirClaveRequerimientoOrigen(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "";
            }

            return NormalizarCompactacion(req.EntregableCliente) + "|" +
                NormalizarCompactacion(req.CategoriaEntregable) + "|" +
                NormalizarCompactacion(req.EtapaSugerida) + "|" +
                NormalizarCompactacion(req.TipoInterno) + "|" +
                NormalizarCompactacion(req.EcuacionUsada) + "|" +
                NormalizarCompactacion(req.CargoSugerido);
        }

        private static string ConstruirClaveBaseEntregable(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "";
            }

            return NormalizarCompactacion(entregable.Nombre) + "|" +
                NormalizarCompactacion(entregable.Categoria) + "|" +
                NormalizarCompactacion(entregable.EtapaSugerida) + "|" +
                NormalizarCompactacion(entregable.SubEtapaSugerida);
        }

        private static string ConstruirClaveBaseEntregable(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return "";
            }

            return NormalizarCompactacion(req.EntregableCliente) + "|" +
                NormalizarCompactacion(req.CategoriaEntregable) + "|" +
                NormalizarCompactacion(req.EtapaSugerida) + "|" +
                NormalizarCompactacion(req.TipoInterno);
        }

        private static RequerimientoProduccionInterna CrearRequerimientoFallbackDesdePipeline(
            EntregableBrief entregable
        )
        {
            double cantidad = ObtenerCantidadProductivaFallback(entregable);
            string unidad = ObtenerUnidadProductivaFallback(entregable);
            string etapa = string.IsNullOrWhiteSpace(entregable.EtapaSugerida)
                ? entregable.Categoria
                : entregable.EtapaSugerida;
            string subEtapa = string.IsNullOrWhiteSpace(entregable.SubEtapaSugerida)
                ? entregable.Nombre
                : entregable.SubEtapaSugerida;

            string cargoVisible = string.IsNullOrWhiteSpace(entregable.CargosSugeridos)
                ? ""
                : entregable.CargosSugeridos;
            string cargosCalculo = ObtenerCargosPonderadosDesdeEntregable(entregable);
            if (string.IsNullOrWhiteSpace(cargosCalculo))
            {
                cargosCalculo = ObtenerCargosPonderadosDesdeEcuacionVinculada(entregable, etapa, subEtapa);
            }
            if (string.IsNullOrWhiteSpace(cargosCalculo))
            {
                cargosCalculo = entregable.CargosSugeridos ?? "";
            }
            if (string.IsNullOrWhiteSpace(cargoVisible))
            {
                cargoVisible = cargosCalculo;
            }
            string nivelCargo = "típico";
            double sueldoMensualCLP = 0.0;
            double tarifaDiaCLP = 0.0;
            List<string> cargosNoEncontrados = new List<string>();

            CalcularCostoCargosPipeline(
                string.IsNullOrWhiteSpace(cargosCalculo) ? cargoVisible : cargosCalculo,
                etapa,
                out sueldoMensualCLP,
                out tarifaDiaCLP,
                out nivelCargo,
                cargosNoEncontrados
            );

            string ecuacionUsada = string.IsNullOrWhiteSpace(entregable.EcuacionProductiva)
                ? "PIPELINE-JSON-FALLBACK | Requerimiento desde producto editable"
                : entregable.EcuacionProductiva;
            string notaEcuacion = CrearNotaEcuacionPipeline(entregable);

            RequerimientoProduccionInterna req = new RequerimientoProduccionInterna
            {
                EntregableCliente = entregable.Nombre,
                CategoriaEntregable = entregable.Categoria,
                EcuacionUsada = ecuacionUsada,
                TipoInterno = subEtapa,
                NombreRequerimiento = subEtapa,
                Cantidad = cantidad,
                Unidad = unidad,
                EtapaSugerida = etapa,
                Calidad = string.IsNullOrWhiteSpace(entregable.NivelCalidadEstimado)
                    ? "Estándar"
                    : entregable.NivelCalidadEstimado,
                BloqueProductivo = etapa,
                ModoPlanificacion = string.IsNullOrWhiteSpace(entregable.DependeDe)
                    ? "Paralelo en etapa"
                    : "Secuencial",
                DependeDe = entregable.DependeDe ?? "",
                CargoSugerido = cargoVisible,
                NivelCargoSugerido = nivelCargo,
                AreaCargoSugerida = etapa,
                SueldoMensualCargoCLP = sueldoMensualCLP,
                TarifaDiaCargoCLP = tarifaDiaCLP,
                RendimientoCantidad = 0.0,
                RendimientoPeriodo = "",
                RendimientoOrigen = "",
                DiasPersonaMin = 0.0,
                DiasPersonaStd = 0.0,
                DiasPersonaHolgura = 0.0,
                CostoMinimoCLP = 0.0,
                CostoEstandarCLP = 0.0,
                CostoHolguraCLP = 0.0,
                Nota = notaEcuacion
            };

            List<string> faltantes = new List<string>();

            if (string.IsNullOrWhiteSpace(entregable.EcuacionProductiva))
            {
                faltantes.Add("ecuación productiva no explícita en productos2d.json");
            }

            if (string.IsNullOrWhiteSpace(entregable.CargosSugeridos))
            {
                faltantes.Add("cargo no definido en productos2d.json");
            }

            foreach (string cargoNoEncontrado in cargosNoEncontrados)
            {
                faltantes.Add("cargo no encontrado en cargos.json: " + cargoNoEncontrado);
            }

            if (cantidad <= 0.0)
            {
                faltantes.Add("cantidad/duración productiva no definida");
            }

            AplicarDiagnosticoParametros(req, faltantes);

            return req;
        }

        private static string ObtenerCargosPonderadosDesdeEntregable(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "";
            }

            List<EcuacionProductivaRuntimeService.CargoVector> desdeJson =
                EcuacionProductivaRuntimeService.ParsearVectorCargosParticipantesJson(
                    entregable.CargosParticipantesJson
                );

            if (desdeJson.Count > 0)
            {
                return EcuacionProductivaRuntimeService.SerializarVectorCargos(desdeJson);
            }

            return "";
        }

        private static string ObtenerCargosPonderadosDesdeEcuacionVinculada(
            EntregableBrief entregable,
            string etapa,
            string subEtapa
        )
        {
            if (entregable == null)
            {
                return "";
            }

            EcuacionProductivaDefinicion ecuacion = null;
            string ecuacionTexto = entregable.EcuacionProductiva ?? "";
            string clave = ecuacionTexto.Split('|').FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(clave))
            {
                ecuacion = BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones()
                    .FirstOrDefault(e =>
                        e != null &&
                        string.Equals(e.Clave, clave.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (ecuacion == null)
            {
                ecuacion = BibliotecaEcuacionesProductivasJsonService.BuscarMejorPara(
                    etapa,
                    subEtapa,
                    entregable.Nombre,
                    entregable.CargosSugeridos
                );
            }

            return ObtenerCargosPonderadosDesdeEcuacion(ecuacion);
        }

        private static RequerimientoProduccionInterna CrearRequerimientoSinDefinicion(
            EntregableBrief entregable,
            string motivo
        )
        {
            string etapa = entregable == null || string.IsNullOrWhiteSpace(entregable.EtapaSugerida)
                ? (entregable == null ? "" : entregable.Categoria)
                : entregable.EtapaSugerida;

            string nombre = entregable == null ? "Entregable sin nombre" : entregable.Nombre;

            RequerimientoProduccionInterna req = new RequerimientoProduccionInterna
            {
                EntregableCliente = nombre,
                CategoriaEntregable = entregable == null ? "" : entregable.Categoria,
                EcuacionUsada = "No definida",
                TipoInterno = entregable == null ? "" : entregable.SubEtapaSugerida,
                NombreRequerimiento = string.IsNullOrWhiteSpace(entregable == null ? "" : entregable.SubEtapaSugerida)
                    ? nombre
                    : entregable.SubEtapaSugerida,
                Cantidad = entregable == null ? 0.0 : ObtenerCantidadProductivaFallback(entregable),
                Unidad = entregable == null ? "" : ObtenerUnidadProductivaFallback(entregable),
                EtapaSugerida = etapa,
                BloqueProductivo = etapa,
                ModoPlanificacion = "Sin definir",
                CargoSugerido = "",
                NivelCargoSugerido = "típico",
                Nota = motivo + ". Definir en Pipeline del producto o en la pestaña Ecuaciones."
            };

            AplicarDiagnosticoParametros(req, new List<string>
            {
                motivo + " para " + nombre + " (Productos/Pipeline/Ecuaciones)"
            });

            return req;
        }

        private static string CrearNotaEcuacionPipeline(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "Generado desde productos2d.json.";
            }

            List<string> partes = new List<string>
            {
                "Generado desde productos2d.json."
            };

            if (!string.IsNullOrWhiteSpace(entregable.VariablesEcuacion))
            {
                partes.Add("Variables: " + entregable.VariablesEcuacion);
            }

            if (!string.IsNullOrWhiteSpace(entregable.ImpactoEcuacion))
            {
                partes.Add("Impacto: " + entregable.ImpactoEcuacion);
            }

            return string.Join(" ", partes);
        }

        private static double ObtenerCantidadProductivaFallback(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return 1.0;
            }

            string unidadDuracion = NormalizarCompactacion(entregable.UnidadDuracion);

            if (entregable.DuracionPorUnidad > 0.0 &&
                !unidadDuracion.Contains("noaplica"))
            {
                return entregable.DuracionPorUnidad * Math.Max(1, entregable.Cantidad);
            }

            return Math.Max(1, entregable.Cantidad);
        }

        private static string ObtenerUnidadProductivaFallback(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "piezas";
            }

            string unidadDuracion = (entregable.UnidadDuracion ?? "").Trim();

            if (entregable.DuracionPorUnidad > 0.0 &&
                !NormalizarCompactacion(unidadDuracion).Contains("noaplica"))
            {
                return string.IsNullOrWhiteSpace(unidadDuracion)
                    ? "segundos"
                    : unidadDuracion;
            }

            return string.IsNullOrWhiteSpace(entregable.UnidadCantidad)
                ? "piezas"
                : entregable.UnidadCantidad;
        }

        private static string ObtenerPrimerCargoPipeline(string cargos)
        {
            return (cargos ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => ParsearCargoPipelinePonderado(c).Cargo)
                .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? "";
        }

        private static CargoPipelinePonderado ParsearCargoPipelinePonderado(string texto)
        {
            string valor = (texto ?? "").Trim();
            string cargo = valor;
            double ponderador = 1.0;
            int separador = valor.LastIndexOf('|');

            if (separador >= 0)
            {
                cargo = valor.Substring(0, separador).Trim();
                string ponderadorTexto = valor.Substring(separador + 1).Trim();
                if (!double.TryParse(
                        ponderadorTexto,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out ponderador) &&
                    !double.TryParse(
                        ponderadorTexto,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.CurrentCulture,
                        out ponderador))
                {
                    ponderador = 1.0;
                }
            }

            return new CargoPipelinePonderado
            {
                TextoOriginal = valor,
                Cargo = cargo,
                Ponderador = Math.Max(0.0, Math.Min(5.0, ponderador))
            };
        }

        private static void CalcularCostoCargosPipeline(
            string cargos,
            string etapa,
            out double sueldoMensualCLP,
            out double tarifaDiaCLP,
            out string nivelPrincipal,
            List<string> cargosNoEncontrados
        )
        {
            sueldoMensualCLP = 0.0;
            tarifaDiaCLP = 0.0;
            nivelPrincipal = "típico";

            foreach (CargoPipelinePonderado cargoVector in (cargos ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParsearCargoPipelinePonderado)
                .Where(c => !string.IsNullOrWhiteSpace(c.Cargo))
                .GroupBy(c => c.Cargo, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First()))
            {
                SepararCargoPipeline(cargoVector.Cargo, out string nombreCargo, out string nivelCargo);

                if (string.IsNullOrWhiteSpace(nombreCargo))
                {
                    continue;
                }

                string nivelBusqueda = string.IsNullOrWhiteSpace(nivelCargo) ? "típico" : nivelCargo;
                CategoriaTrabajador cargo = BibliotecaCargosJsonService.BuscarCargo(
                    nombreCargo,
                    etapa,
                    nivelBusqueda
                );

                if (cargo == null)
                {
                    cargosNoEncontrados.Add(cargoVector.TextoOriginal);
                    continue;
                }

                if (sueldoMensualCLP <= 0.0)
                {
                    nivelPrincipal = string.IsNullOrWhiteSpace(nivelCargo) ? cargo.Nivel : nivelCargo;
                }

                sueldoMensualCLP += cargo.SueldoMensualCLPTipico * cargoVector.Ponderador;
                tarifaDiaCLP += (cargo.SueldoMensualCLPTipico / 22.0) * cargoVector.Ponderador;
            }
        }

        private static void SepararCargoPipeline(
            string texto,
            out string nombre,
            out string nivel
        )
        {
            nombre = (texto ?? "").Trim();
            nivel = "";

            int idx = nombre.LastIndexOf(" (", StringComparison.Ordinal);

            if (idx <= 0 || !nombre.EndsWith(")", StringComparison.Ordinal))
            {
                return;
            }

            nivel = nombre.Substring(idx + 2, nombre.Length - idx - 3).Trim();
            nombre = nombre.Substring(0, idx).Trim();
        }

        private static void AplicarRendimientosProductivos(
            DesgloseProductivoProyecto desglose,
            Cotizacion cotizacion
        )
        {
            if (desglose == null ||
                desglose.Requerimientos == null ||
                desglose.Requerimientos.Count == 0)
            {
                return;
            }

            double diasHabilesSemana = cotizacion != null &&
                cotizacion.DiasHabilesEstudioPorSemana > 0.0
                    ? cotizacion.DiasHabilesEstudioPorSemana
                    : DiasTrabajoSemana;

            foreach (RequerimientoProduccionInterna req in desglose.Requerimientos)
            {
                if (req == null)
                {
                    continue;
                }

                if (req.RendimientoCantidad <= 0.0)
                {
                    RendimientoProductivo rendimiento =
                        BibliotecaRendimientosProductivosJsonService.BuscarMejorPara(req);

                    if (rendimiento != null)
                    {
                        req.RendimientoCantidad = rendimiento.CantidadPorPeriodo;
                        req.RendimientoPeriodo = rendimiento.Periodo;
                        req.RendimientoOrigen = rendimiento.Proceso;
                    }
                }

                BibliotecaRendimientosProductivosJsonService.AplicarRendimiento(
                    req,
                    diasHabilesSemana
                );
            }
        }

        public static void ValidarParametrosDesglose(DesgloseProductivoProyecto desglose)
        {
            if (desglose == null || desglose.Requerimientos == null)
            {
                return;
            }

            List<string> resumen = new List<string>();

            foreach (RequerimientoProduccionInterna req in desglose.Requerimientos)
            {
                if (req == null)
                {
                    continue;
                }

                List<string> faltantes = ObtenerFaltantesRequerimiento(req);
                AplicarDiagnosticoParametros(req, faltantes);

                if (faltantes.Count > 0)
                {
                    resumen.Add(
                        (string.IsNullOrWhiteSpace(req.EntregableCliente) ? "Entregable sin nombre" : req.EntregableCliente) +
                        " / " +
                        (string.IsNullOrWhiteSpace(req.NombreRequerimiento) ? "requerimiento sin nombre" : req.NombreRequerimiento) +
                        ": " +
                        string.Join("; ", faltantes)
                    );
                }
            }

            if (resumen.Count > 0)
            {
                desglose.Diagnostico =
                    "Faltan parámetros por definir. " +
                    string.Join(" | ", resumen.Take(8)) +
                    (resumen.Count > 8 ? " | +" + (resumen.Count - 8) + " más." : "");
            }
        }

        private static List<string> ObtenerFaltantesRequerimiento(RequerimientoProduccionInterna req)
        {
            List<string> faltantes = new List<string>();

            string ecuacion = NormalizarCompactacion(req.EcuacionUsada);
            if (string.IsNullOrWhiteSpace(req.EcuacionUsada) ||
                ecuacion.Contains("nodefinida") ||
                ecuacion.Contains("sinecuacion"))
            {
                faltantes.Add("ecuación no definida");
            }

            if (req.Cantidad <= 0.0)
            {
                faltantes.Add("cantidad/duración no definida");
            }

            if (string.IsNullOrWhiteSpace(req.Unidad))
            {
                faltantes.Add("unidad no definida");
            }

            if (string.IsNullOrWhiteSpace(req.CargoSugerido))
            {
                faltantes.Add("cargo no definido");
            }
            else if (req.SueldoMensualCargoCLP <= 0.0 || req.TarifaDiaCargoCLP <= 0.0)
            {
                faltantes.Add("cargo no encontrado o sin sueldo en cargos.json: " + req.CargoSugerido);
            }

            CategoriaTrabajador cargoValidacion =
                ResolverCargoProductivoReferencia(req) ??
                BibliotecaCargosJsonService.BuscarCargo(
                    req.CargoSugerido,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    req.NivelCargoSugerido
                );

            bool requiereRendimiento = cargoValidacion == null
                ? ClasificacionCargosService.RequiereRendimientoProductivo(
                    req.CargoSugerido,
                    req.EntregableCliente,
                    req.NombreRequerimiento,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    req.EcuacionUsada + " " + req.Unidad
                )
                : ClasificacionCargosService.RequiereRendimientoProductivo(
                    cargoValidacion,
                    req.EntregableCliente,
                    req.NombreRequerimiento,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    req.EcuacionUsada + " " + req.Unidad
                );

            if (requiereRendimiento && req.RendimientoCantidad <= 0.0)
            {
                faltantes.Add("rendimiento/capacidad no definido en rendimientos_productivos.json");
            }

            if (requiereRendimiento && req.DiasPersonaStd <= 0.0)
            {
                faltantes.Add("días-persona no calculados");
            }

            return faltantes;
        }

        private static CategoriaTrabajador ResolverCargoProductivoReferencia(
            RequerimientoProduccionInterna req
        )
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CargoSugerido))
            {
                return null;
            }

            foreach (CargoPipelinePonderado cargoVector in (req.CargoSugerido ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParsearCargoPipelinePonderado)
                .Where(c => !string.IsNullOrWhiteSpace(c.Cargo)))
            {
                SepararCargoPipeline(cargoVector.Cargo, out string nombreCargo, out string nivelCargo);
                if (string.IsNullOrWhiteSpace(nombreCargo))
                {
                    continue;
                }

                CategoriaTrabajador cargo = BibliotecaCargosJsonService.BuscarCargo(
                    nombreCargo,
                    req.EtapaSugerida + " " + req.BloqueProductivo,
                    string.IsNullOrWhiteSpace(nivelCargo) ? req.NivelCargoSugerido : nivelCargo
                );

                if (cargo != null && ClasificacionCargosService.EsCargoProductivo(cargo))
                {
                    return cargo;
                }
            }

            return null;
        }

        private static void AplicarDiagnosticoParametros(
            RequerimientoProduccionInterna req,
            List<string> faltantes
        )
        {
            if (req == null)
            {
                return;
            }

            faltantes = faltantes == null
                ? new List<string>()
                : faltantes
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .Select(f => f.Trim())
                    .Distinct()
                    .ToList();

            req.ParametrosCompletos = faltantes.Count == 0;
            req.DiagnosticoParametros = faltantes.Count == 0
                ? ""
                : "Faltan parámetros por definir: " + string.Join("; ", faltantes);

            if (!req.ParametrosCompletos)
            {
                req.CostoMinimoCLP = 0.0;
                req.CostoEstandarCLP = 0.0;
                req.CostoHolguraCLP = 0.0;
            }
        }

        private static void CompactarRequerimientosDuplicados(DesgloseProductivoProyecto desglose)
        {
            if (desglose == null ||
                desglose.Requerimientos == null ||
                desglose.Requerimientos.Count <= 1)
            {
                return;
            }

            desglose.Requerimientos = desglose.Requerimientos
                .Where(r => r != null)
                .GroupBy(r => ConstruirClaveCompactacion(r))
                .Select(g => CompactarGrupoRequerimientos(g.ToList()))
                .ToList();
        }

        private static string ConstruirClaveCompactacion(RequerimientoProduccionInterna req)
        {
            return NormalizarCompactacion(req.EntregableCliente) + "|" +
                NormalizarCompactacion(req.CategoriaEntregable) + "|" +
                NormalizarCompactacion(req.EcuacionUsada) + "|" +
                NormalizarCompactacion(req.TipoInterno) + "|" +
                NormalizarCompactacion(req.NombreRequerimiento) + "|" +
                req.Cantidad.ToString("0.####") + "|" +
                NormalizarCompactacion(req.Unidad) + "|" +
                NormalizarCompactacion(req.EtapaSugerida) + "|" +
                NormalizarCompactacion(req.Calidad) + "|" +
                NormalizarCompactacion(req.BloqueProductivo) + "|" +
                NormalizarCompactacion(req.ModoPlanificacion) + "|" +
                NormalizarCompactacion(req.DependeDe);
        }

        private static RequerimientoProduccionInterna CompactarGrupoRequerimientos(
            List<RequerimientoProduccionInterna> grupo
        )
        {
            RequerimientoProduccionInterna principal = grupo[0];

            if (grupo.Count == 1)
            {
                return principal;
            }

            principal.CargoSugerido = string.Join(
                "; ",
                grupo
                    .Select(r => FormatearCargoCompactado(r.CargoSugerido, r.NivelCargoSugerido))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
            );

            principal.Nota = string.Join(
                " ",
                grupo
                    .Select(r => r.Nota)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
            );

            return principal;
        }

        private static string FormatearCargoCompactado(string cargo, string nivel)
        {
            if (string.IsNullOrWhiteSpace(cargo))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(nivel) ||
                cargo.Contains("(" + nivel + ")"))
            {
                return cargo.Trim();
            }

            return cargo.Trim() + " (" + nivel.Trim() + ")";
        }

        private static string NormalizarCompactacion(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto.Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ñ", "n");
        }

        private static string ObtenerNombreEntregableDebug(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return "(null)";
            }

            string texto = "";

            try
            {
                object nombre = entregable.GetType()
                    .GetProperty("Nombre")
                    ?.GetValue(entregable, null);

                if (nombre != null)
                {
                    texto += nombre.ToString();
                }
            }
            catch
            {
            }

            try
            {
                object pieza = entregable.GetType()
                    .GetProperty("Pieza2D")
                    ?.GetValue(entregable, null);

                if (pieza != null)
                {
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        texto += " / ";
                    }

                    texto += pieza.ToString();
                }
            }
            catch
            {
            }

            try
            {
                object tipo = entregable.GetType()
                    .GetProperty("TipoInterno")
                    ?.GetValue(entregable, null);

                if (tipo != null)
                {
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        texto += " / ";
                    }

                    texto += tipo.ToString();
                }
            }
            catch
            {
            }

            try
            {
                object categoria = entregable.GetType()
                    .GetProperty("Categoria")
                    ?.GetValue(entregable, null);

                if (categoria != null)
                {
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        texto += " / ";
                    }

                    texto += categoria.ToString();
                }
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                texto = entregable.ToString();
            }

            return texto ?? "(sin nombre)";
        }

        private static void RecalcularTotales(
            DesgloseProductivoProyecto desglose,
            Cotizacion cotizacion
        )
        {
            if (desglose == null)
            {
                return;
            }

            desglose.DiasPersonaMinimos = 0.0;
            desglose.DiasPersonaEstandar = 0.0;
            desglose.DiasPersonaHolgura = 0.0;

            desglose.CostoMinimoCLP = 0.0;
            desglose.CostoEstandarCLP = 0.0;
            desglose.CostoHolguraCLP = 0.0;
            desglose.HorasGestionEstandar = 0.0;
            desglose.CostoGestionEstandarCLP = 0.0;
            desglose.GestionesCalculadas.Clear();

            foreach (RequerimientoProduccionInterna req in desglose.Requerimientos)
            {
                if (req == null || !req.ParametrosCompletos)
                {
                    continue;
                }

                desglose.DiasPersonaMinimos += req.DiasPersonaMin;
                desglose.DiasPersonaEstandar += req.DiasPersonaStd;
                desglose.DiasPersonaHolgura += req.DiasPersonaHolgura;

                desglose.CostoMinimoCLP += req.CostoMinimoCLP;
                desglose.CostoEstandarCLP += req.CostoEstandarCLP;
                desglose.CostoHolguraCLP += req.CostoHolguraCLP;
            }

            AplicarGestionProductivaDerivada(desglose);

            double capacidadBasePersonas = ObtenerPersonasPlanificadasDesdeManoObra(cotizacion);

            desglose.SemanasMinimas =
                desglose.DiasPersonaMinimos / (DiasTrabajoSemana * capacidadBasePersonas);

            desglose.SemanasEstandar =
                desglose.DiasPersonaEstandar / (DiasTrabajoSemana * capacidadBasePersonas);

            desglose.SemanasHolgura =
                desglose.DiasPersonaHolgura / (DiasTrabajoSemana * capacidadBasePersonas);

            double plazoCliente = cotizacion == null
                ? 0.0
                : cotizacion.PlazoClienteSemanas;

            if (desglose.Requerimientos != null &&
                desglose.Requerimientos.Any(r => r != null && !r.ParametrosCompletos))
            {
                ValidarParametrosDesglose(desglose);
                return;
            }

            if (plazoCliente <= 0.0)
            {
                desglose.Diagnostico =
                    "Sin fecha objetivo declarada. Se entrega desglose productivo sin comparación de plazo.";
                return;
            }

            if (desglose.SemanasMinimas <= 0.0)
            {
                desglose.Diagnostico =
                    "No hay ecuaciones productivas aplicables a los entregables seleccionados.";
                return;
            }

            if (plazoCliente < desglose.SemanasMinimas)
            {
                desglose.Diagnostico =
                    "INVIABLE: el plazo declarado está bajo el mínimo productivo estimado.";
                return;
            }

            if (plazoCliente < desglose.SemanasEstandar)
            {
                desglose.Diagnostico =
                    "PLAZO AGRESIVO: requiere reducir alcance, bajar calidad o aumentar equipo.";
                return;
            }

            if (plazoCliente < desglose.SemanasHolgura)
            {
                desglose.Diagnostico =
                    "PLAZO VIABLE AJUSTADO: entra en rango, pero con poca holgura.";
                return;
            }

            desglose.Diagnostico =
                "PLAZO VIABLE: el plazo declarado permite una planificación preliminar con holgura.";
        }

        public static void AplicarGestionProductivaDerivada(
            DesgloseProductivoProyecto desglose
        )
        {
            if (desglose == null)
            {
                return;
            }

            if (desglose.GestionesCalculadas == null)
            {
                desglose.GestionesCalculadas = new List<GestionProductivaCalculada>();
            }

            desglose.GestionesCalculadas.Clear();
            desglose.HorasGestionEstandar = 0.0;
            desglose.CostoGestionEstandarCLP = 0.0;

            if (desglose.Requerimientos == null || desglose.Requerimientos.Count == 0)
            {
                return;
            }

            List<DefinicionGestionProductiva> definiciones =
                CrearDefinicionesGestionProductivaDesdeBiblioteca();

            foreach (DefinicionGestionProductiva definicion in definiciones)
            {
                List<RequerimientoProduccionInterna> asociados =
                    desglose.Requerimientos
                        .Where(r => r != null && r.ParametrosCompletos && definicion.Aplica(r))
                        .ToList();

                if (asociados.Count == 0)
                {
                    continue;
                }

                double diasMin = asociados.Sum(r => r.DiasPersonaMin);
                double diasStd = asociados.Sum(r => r.DiasPersonaStd);
                double diasHolgura = asociados.Sum(r => r.DiasPersonaHolgura);

                if (diasMin <= 0.0 && diasStd <= 0.0 && diasHolgura <= 0.0)
                {
                    continue;
                }

                CategoriaTrabajador cargo = BibliotecaCargosJsonService.BuscarCargo(
                    definicion.Cargo,
                    definicion.EtapaReferencia,
                    definicion.NivelCargo
                );

                double tarifaHora = ObtenerTarifaHoraGestion(cargo);

                GestionProductivaCalculada gestion = new GestionProductivaCalculada
                {
                    Area = definicion.Area,
                    Cargo = cargo == null ? definicion.Cargo : cargo.Nombre,
                    NivelCargo = cargo == null ? definicion.NivelCargo : cargo.Nivel,
                    Descripcion = definicion.Descripcion,
                    DiasPersonaMinimosAsociados = diasMin,
                    DiasPersonaEstandarAsociados = diasStd,
                    DiasPersonaHolguraAsociados = diasHolgura,
                    HorasMinimas = diasMin * definicion.HorasPorDiaPersona,
                    HorasEstandar = diasStd * definicion.HorasPorDiaPersona,
                    HorasHolgura = diasHolgura * definicion.HorasPorDiaPersona,
                    TarifaHoraCLP = tarifaHora
                };

                gestion.CostoMinimoCLP = gestion.HorasMinimas * tarifaHora;
                gestion.CostoEstandarCLP = gestion.HorasEstandar * tarifaHora;
                gestion.CostoHolguraCLP = gestion.HorasHolgura * tarifaHora;

                desglose.GestionesCalculadas.Add(gestion);

                desglose.CostoMinimoCLP += gestion.CostoMinimoCLP;
                desglose.CostoEstandarCLP += gestion.CostoEstandarCLP;
                desglose.CostoHolguraCLP += gestion.CostoHolguraCLP;
                desglose.HorasGestionEstandar += gestion.HorasEstandar;
                desglose.CostoGestionEstandarCLP += gestion.CostoEstandarCLP;
            }
        }

        private static double ObtenerTarifaHoraGestion(CategoriaTrabajador cargo)
        {
            if (cargo == null || cargo.SueldoMensualCLPTipico <= 0.0)
            {
                return TarifaHoraGestionCLP;
            }

            return cargo.SueldoMensualCLPTipico / 22.0 / 8.0;
        }

        private static List<DefinicionGestionProductiva> CrearDefinicionesGestionProductivaDesdeBiblioteca()
        {
            return BibliotecaGestionesProductivasJsonService.CargarGestiones()
                .Where(g => g != null && g.Activo)
                .Select(g => new DefinicionGestionProductiva(
                    g.Area,
                    g.Cargo,
                    g.NivelCargo,
                    g.EtapaReferencia,
                    g.Descripcion,
                    g.MinutosPorDiaPersona / 60.0,
                    CrearPredicadoGestion(g.TokensAsociados)
                ))
                .ToList();
        }

        private static Func<RequerimientoProduccionInterna, bool> CrearPredicadoGestion(
            string tokensAsociados
        )
        {
            List<string> tokens = (tokensAsociados ?? "")
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (tokens.Any(t => t == "*"))
            {
                return r => true;
            }

            return r => ContieneGestion(r, tokens.ToArray());
        }

        private static bool ContieneGestion(
            RequerimientoProduccionInterna req,
            params string[] tokens
        )
        {
            if (req == null || tokens == null || tokens.Length == 0)
            {
                return false;
            }

            string texto = NormalizarCompactacion(
                req.TipoInterno + " " +
                req.NombreRequerimiento + " " +
                req.CargoSugerido + " " +
                req.EtapaSugerida + " " +
                req.BloqueProductivo
            );

            texto = texto.Replace(" ", "");

            return tokens.Any(t => texto.Contains(NormalizarCompactacion(t).Replace(" ", "")));
        }

        private class DefinicionGestionProductiva
        {
            public DefinicionGestionProductiva(
                string area,
                string cargo,
                string nivelCargo,
                string etapaReferencia,
                string descripcion,
                double horasPorDiaPersona,
                Func<RequerimientoProduccionInterna, bool> aplica
            )
            {
                Area = area;
                Cargo = cargo;
                NivelCargo = nivelCargo;
                EtapaReferencia = etapaReferencia;
                Descripcion = descripcion;
                HorasPorDiaPersona = horasPorDiaPersona <= 0.0
                    ? HorasGestionPorDiaPersonaProductivo
                    : horasPorDiaPersona;
                Aplica = aplica;
            }

            public string Area { get; }
            public string Cargo { get; }
            public string NivelCargo { get; }
            public string EtapaReferencia { get; }
            public string Descripcion { get; }
            public double HorasPorDiaPersona { get; }
            public Func<RequerimientoProduccionInterna, bool> Aplica { get; }
        }

        private static double ObtenerPersonasPlanificadasDesdeManoObra(Cotizacion cotizacion)
        {
            const double respaldo = 2.0;

            if (cotizacion == null ||
                cotizacion.PlanGeneralManoObra == null ||
                cotizacion.PlanGeneralManoObra.Count == 0)
            {
                return respaldo;
            }

            int meses = cotizacion.PlanGeneralManoObra
                .Where(c => c != null && c.PersonasPorBloque != null)
                .Select(c => c.PersonasPorBloque.Count)
                .DefaultIfEmpty(0)
                .Max();

            if (meses <= 0)
            {
                return respaldo;
            }

            double maximoPersonasMes = 0.0;

            for (int i = 0; i < meses; i++)
            {
                double personasMes = cotizacion.PlanGeneralManoObra
                    .Where(c => c != null && c.PersonasPorBloque != null && i < c.PersonasPorBloque.Count)
                    .Sum(c => Math.Max(0.0, c.PersonasPorBloque[i]));

                maximoPersonasMes = Math.Max(maximoPersonasMes, personasMes);
            }

            if (maximoPersonasMes <= 0.0)
            {
                return respaldo;
            }

            return Math.Max(1.0, maximoPersonasMes);
        }
    }
}
