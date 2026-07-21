using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaEcuacionesProductivasJsonService
    {
        private const string NombreArchivo = "ecuaciones_productivas.json";

        public static string ObtenerRutaEcuaciones()
        {
            return Path.Combine(ObtenerCarpetaBiblioteca(), NombreArchivo);
        }

        public static List<EcuacionProductivaDefinicion> CargarEcuaciones()
        {
            try
            {
                AsegurarArchivo();
                string json = File.ReadAllText(ObtenerRutaEcuaciones());
                List<EcuacionProductivaDefinicion> ecuaciones =
                    JsonSerializer.Deserialize<List<EcuacionProductivaDefinicion>>(
                        json,
                        CrearOpcionesJson()
                    ) ?? CrearBase();

                if (ecuaciones.Count == 0)
                {
                    ecuaciones = CrearBase();
                    GuardarEcuaciones(ecuaciones);
                }

                bool requiereGuardar = AgregarEcuacionesBaseFaltantes(ecuaciones) ||
                    RequiereMigracionPersistente(ecuaciones);
                Normalizar(ecuaciones);
                ResolverHerencias(ecuaciones);
                if (requiereGuardar)
                {
                    GuardarEcuaciones(ecuaciones);
                }

                return ecuaciones;
            }
            catch
            {
                return CrearBase();
            }
        }

        public static void GuardarEcuaciones(List<EcuacionProductivaDefinicion> ecuaciones)
        {
            if (ecuaciones == null)
            {
                ecuaciones = CrearBase();
            }

            Normalizar(ecuaciones);

            string ruta = ObtenerRutaEcuaciones();
            string carpeta = Path.GetDirectoryName(ruta) ?? "";

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            File.WriteAllText(ruta, JsonSerializer.Serialize(ecuaciones, CrearOpcionesJson()));
        }

        public static void RegenerarDesdeBase()
        {
            GuardarEcuaciones(CrearBase());
        }

        public static EcuacionProductivaDefinicion BuscarMejorPara(
            string etapa,
            string subEtapa,
            string nombre,
            string cargos
        )
        {
            string etapaNorm = NormalizarTexto(etapa);
            string subEtapaNorm = NormalizarTexto(subEtapa);
            string nombreNorm = NormalizarTexto(nombre);
            string cargosNorm = NormalizarTexto(cargos);

            return CargarEcuaciones()
                .Where(e => e != null && e.Activa)
                .Select(e => new
                {
                    Ecuacion = e,
                    Puntaje = CalcularPuntaje(e, etapaNorm, subEtapaNorm, nombreNorm, cargosNorm)
                })
                .Where(x => x.Puntaje > 0)
                .OrderByDescending(x => x.Puntaje)
                .ThenBy(x => x.Ecuacion.NombreVisible)
                .Select(x => x.Ecuacion)
                .FirstOrDefault();
        }

        public static List<EcuacionProductivaDefinicion> CrearBase()
        {
            List<EcuacionProductivaDefinicion> baseSistema = new List<EcuacionProductivaDefinicion>
            {
                CrearBaseGenerica("CALCULO_POR_CAPACIDAD", "Calculo por capacidad productiva",
                    "cantidad;unidad;capacidad;periodo;cargo",
                    "Convierte cantidad productiva y capacidad esperada por cargo en tiempo tecnico y costo vectorial.",
                    ConstruirFormulaVectorial("cantidad")),
                CrearBaseGenerica("CALCULO_POR_DURACION", "Calculo por duracion",
                    "segundos;minutos;capacidad;periodo;cargo",
                    "Convierte duracion productiva y rendimiento temporal por cargo en tiempo tecnico y costo vectorial.",
                    ConstruirFormulaVectorial("segundos")),
                CrearBaseGenerica("CALCULO_GESTION", "Calculo de gestion",
                    "dias-productivos;minutos-gestion;cargo;procesos-asociados",
                    "Calcula direccion, coordinacion o supervision desde los dias productivos asociados y la tarifa del cargo.",
                    ConstruirFormulaGestion()),
                Crear("GUION", "Guion / escena", "CALCULO_POR_CAPACIDAD", "Desarrollo", "Guion / estructura narrativa",
                    "guion;escena;narrativa;estructura",
                    "cantidad;unidad;complejidad;revisiones;cargo",
                    "Guionista (típico); Director/a de animación (típico)",
                    "Calcula escritura y revisiones iniciales cuando el cliente no entrega guion.",
                    ConstruirFormulaVectorial("cantidad")),
                Crear("DIRECCION_VISUAL", "Direccion visual", "CALCULO_GESTION", "Desarrollo", "Direccion visual / referencias de estilo",
                    "direccion visual;referencias;estilo;arte",
                    "dias-productivos;minutos-gestion;procesos-asociados;cargo",
                    "Director de arte (típico); Director/a de animación (típico)",
                    "Alimenta direccion de arte, look inicial y referencias tecnicas.",
                    ConstruirFormulaGestion()),
                Crear("STORYBOARD", "Storyboard / secuencia visual", "CALCULO_POR_CAPACIDAD", "Preproduccion", "Storyboard",
                    "storyboard;secuencia;pagina;vineta;layout;dibujo base",
                    "cantidad;unidad;planos;calidad;rendimiento;cargo",
                    "Storyboard artist (típico); Director/a de animación (típico)",
                    "Calcula dias-persona y costo de secuencias, paginas o planos de storyboard.",
                    ConstruirFormulaVectorial("cantidad")),
                Crear("ANIMATIC", "Animatic / timing", "CALCULO_POR_DURACION", "Preproduccion", "Animatic",
                    "animatic;timing;edicion;previsualizacion",
                    "duracion;segundos;planos;revisiones;cargo",
                    "Editor (junior); Postproductor (junior)",
                    "Convierte duracion o planos en tiempo de edicion/timing.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("PREPARACION_ASSETS", "Preparacion de assets", "CALCULO_POR_CAPACIDAD", "Preproduccion", "Preparacion de assets",
                    "personaje;base;preparacion;assets;diseno de personajes;fondos;props",
                    "cantidad;unidad;calidad;cargo",
                    "Diseñador de personajes (típico); Director de arte (típico)",
                    "Calcula preparacion de personajes, fondos, props o assets antes de produccion.",
                    ConstruirFormulaVectorial("cantidad")),
                Crear("ROUGH", "Rough animation", "CALCULO_POR_DURACION", "Produccion", "Rough animation",
                    "rough;animacion base;acting;animacion 2d final",
                    "segundos;calidad;capacidad;personas;cargo",
                    "Animador 2D (típico); Supervisor de animación (típico)",
                    "Calcula animacion base y acting en segundos productivos.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("CLEAN_UP", "Clean up / linea final", "CALCULO_POR_DURACION", "Produccion", "Clean up",
                    "clean;cleanup;linea final;entintado;dibujo limpio",
                    "segundos;piezas;capacidad;periodo;cargo",
                    "Clean up artist (típico); Supervisor de arte (típico)",
                    "Calcula linea final o limpieza segun capacidad productiva.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("COLOR", "Color / pintura base", "CALCULO_POR_DURACION", "Produccion", "Color",
                    "color;pintura;shading;sombreado",
                    "segundos;piezas;capacidad;periodo;cargo",
                    "Colorista (típico); Supervisor de arte (típico)",
                    "Calcula color, pintura o sombreado segun cantidad y rendimiento.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("COMPOSICION", "Composicion / integracion", "CALCULO_POR_DURACION", "Produccion", "Composicion",
                    "composicion;integracion;comp;postproduccion",
                    "segundos;piezas;capacidad;periodo;cargo",
                    "Compositor (típico); Postproductor (típico)",
                    "Calcula integracion, composicion y armado final de piezas.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("AUDIO", "Audio / mezcla", "CALCULO_POR_DURACION", "Produccion", "Audio",
                    "audio;sonido;musica;locucion;dialogos;mezcla",
                    "segundos;minutos;piezas;capacidad;cargo",
                    "Editor de audio (típico); Músico / compositor musical (típico)",
                    "Calcula diseno sonoro, locucion, musica o mezcla si se incluye.",
                    ConstruirFormulaVectorial("segundos")),
                Crear("EXPORT", "Export / entrega final", "CALCULO_POR_CAPACIDAD", "Postproduccion", "Entrega final",
                    "export;render;entrega;control tecnico;calidad",
                    "piezas;formatos;revisiones;cargo",
                    "Render / export manager (típico); Control de calidad técnico (típico)",
                    "Calcula revision tecnica, render/export y armado de entregables.",
                    ConstruirFormulaVectorial("piezas"))
            };

            AgregarProcesosProductivosBase(baseSistema);
            return baseSistema;
        }

        private static void AgregarProcesosProductivosBase(List<EcuacionProductivaDefinicion> baseSistema)
        {
            if (baseSistema == null)
            {
                return;
            }

            baseSistema.Add(ConfigurarProceso(Crear("REVISION_ROUGH", "Revision de rough animation", "CALCULO_POR_DURACION", "Produccion", "Revision de rough animation",
                "revision;rough;control;supervision", "horas_produccion;porcentaje_revision;ciclos;cargo",
                "Supervisor de animacion (tipico)", "Revision y control de calidad posterior a rough animation.",
                "horas_revision = horas_produccion * 0.10"), TipoProcesoProductivo.RevisionControl, MetodoCalculoProceso.PorPorcentajeProduccion, AlcanceTemporalProceso.Item, "proc_rough", false));

            baseSistema.Add(ConfigurarProceso(Crear("CORRECCION_ROUGH", "Correcciones de rough animation", "CALCULO_POR_DURACION", "Produccion", "Correcciones de rough animation",
                "correccion;retrabajo;rough", "horas_produccion;tasa_retrabajo;cargo",
                "Animador 2D (tipico)", "Retrabajo esperado luego de revision de rough animation.",
                "horas_correccion = horas_produccion * 0.15"), TipoProcesoProductivo.CorreccionRetrabajo, MetodoCalculoProceso.PorPorcentajeProduccion, AlcanceTemporalProceso.Item, "proc_rough", false));

            baseSistema.Add(ConfigurarProceso(Crear("REVISION_CLEAN_UP", "Revision de clean up", "CALCULO_POR_DURACION", "Produccion", "Revision de clean up",
                "revision;clean up;cleanup;control;supervision", "horas_produccion;porcentaje_revision;ciclos;cargo",
                "Supervisor de animacion (tipico)", "Revision tecnica de clean up como proceso independiente.",
                "horas_revision = horas_produccion * 0.10"), TipoProcesoProductivo.RevisionControl, MetodoCalculoProceso.PorPorcentajeProduccion, AlcanceTemporalProceso.Item, "proc_clean_up", false));

            baseSistema.Add(ConfigurarProceso(Crear("DIRECCION_ARTE_PROYECTO", "Direccion de arte", "CALCULO_GESTION", "Direccion, supervision y gestion", "Direccion de arte",
                "direccion;arte;transversal;visual", "semanas_activas;horas_por_semana;dedicacion;cargo",
                "Director de arte (tipico)", "Direccion transversal por duracion, sin multiplicarse por item.",
                "horas_direccion = semanas_activas * 8"), TipoProcesoProductivo.Direccion, MetodoCalculoProceso.PorDuracionProyecto, AlcanceTemporalProceso.ProyectoCompleto, "", true));

            baseSistema.Add(ConfigurarProceso(Crear("COORDINACION_PRODUCCION_PROYECTO", "Coordinacion de produccion", "CALCULO_GESTION", "Direccion, supervision y gestion", "Coordinacion de produccion",
                "gestion;coordinacion;produccion;seguimiento;transversal", "semanas_activas;horas_por_semana;dedicacion;cargo",
                "Productor / coordinador (tipico)", "Coordinacion transversal por duracion, sin duplicarse por item.",
                "horas_gestion = semanas_activas * 4"), TipoProcesoProductivo.GestionCoordinacion, MetodoCalculoProceso.PorDuracionProyecto, AlcanceTemporalProceso.ProyectoCompleto, "", true));
        }

        private static EcuacionProductivaDefinicion ConfigurarProceso(
            EcuacionProductivaDefinicion ecuacion,
            TipoProcesoProductivo tipo,
            MetodoCalculoProceso metodo,
            AlcanceTemporalProceso alcance,
            string dependencia,
            bool paralelo
        )
        {
            ecuacion.SchemaVersion = 2;
            ecuacion.IdProceso = "proc_" + NormalizarId(ecuacion.Clave);
            ecuacion.TipoProceso = tipo;
            ecuacion.MetodoCalculo = metodo;
            ecuacion.AlcanceTemporal = alcance;
            ecuacion.FormulaId = ecuacion.EcuacionBase;
            ecuacion.EtapaId = NormalizarId(ecuacion.Etapa);
            ecuacion.SubEtapaId = NormalizarId(ecuacion.SubEtapa);
            ecuacion.PuedeEjecutarseEnParalelo = paralelo;
            ecuacion.ReglaActivacionJson = JsonSerializer.Serialize(new ReglaActivacionProceso
            {
                Tipo = string.IsNullOrWhiteSpace(dependencia)
                    ? TipoReglaActivacionProceso.Manual
                    : TipoReglaActivacionProceso.ProcesoExistente,
                ProcesoOrigenId = dependencia
            }, CrearOpcionesJson());
            ecuacion.DependenciasJson = string.IsNullOrWhiteSpace(dependencia)
                ? ""
                : JsonSerializer.Serialize(new List<string> { dependencia }, CrearOpcionesJson());
            ecuacion.EtapasCubiertasJson = alcance == AlcanceTemporalProceso.ProyectoCompleto ||
                alcance == AlcanceTemporalProceso.MultiplesEtapas
                    ? JsonSerializer.Serialize(new List<string> { "preproduccion", "produccion", "postproduccion" }, CrearOpcionesJson())
                    : "";
            ecuacion.WarningsMigracionJson = "[]";
            return ecuacion;
        }

        private static EcuacionProductivaDefinicion Crear(
            string clave,
            string nombre,
            string ecuacionBase,
            string etapa,
            string subEtapa,
            string tokens,
            string variables,
            string cargosPermitidos,
            string impacto,
            string formula
        )
        {
            return new EcuacionProductivaDefinicion
            {
                Clave = clave,
                NombreVisible = nombre,
                TipoEcuacion = "Variante",
                EcuacionBase = ecuacionBase,
                Etapa = etapa,
                SubEtapa = subEtapa,
                Tokens = tokens,
                Variables = variables,
                CargosPermitidos = cargosPermitidos,
                Impacto = impacto,
                FormulaReferencia = formula,
                Numerador = ExtraerNumeradorFormula(formula),
                Denominador = ExtraerDenominadorFormula(formula),
                Nota = "Base editable del sistema"
            };
        }

        private static EcuacionProductivaDefinicion CrearBaseGenerica(
            string clave,
            string nombre,
            string variables,
            string impacto,
            string formula
        )
        {
            return new EcuacionProductivaDefinicion
            {
                Clave = clave,
                NombreVisible = nombre,
                TipoEcuacion = "Base",
                EcuacionBase = "",
                Etapa = "General",
                SubEtapa = "",
                Tokens = "",
                Variables = variables,
                Impacto = impacto,
                FormulaReferencia = formula,
                Numerador = ExtraerNumeradorFormula(formula),
                Denominador = ExtraerDenominadorFormula(formula),
                Nota = "Ecuacion madre reutilizable"
            };
        }

        private static int CalcularPuntaje(
            EcuacionProductivaDefinicion ecuacion,
            string etapa,
            string subEtapa,
            string nombre,
            string cargos
        )
        {
            int puntaje = 0;
            string eEtapa = NormalizarTexto(ecuacion.Etapa);
            string eSubEtapa = NormalizarTexto(ecuacion.SubEtapa);

            if (!string.IsNullOrWhiteSpace(eEtapa) && eEtapa == etapa)
            {
                puntaje += 20;
            }

            if (!string.IsNullOrWhiteSpace(eSubEtapa) && eSubEtapa == subEtapa)
            {
                puntaje += 45;
            }

            foreach (string token in Separar(ecuacion.Tokens))
            {
                string t = NormalizarTexto(token);
                if (string.IsNullOrWhiteSpace(t))
                {
                    continue;
                }

                if (nombre.Contains(t)) puntaje += 12;
                if (subEtapa.Contains(t)) puntaje += 10;
                if (cargos.Contains(t)) puntaje += 4;
            }

            return puntaje;
        }

        private static void AsegurarArchivo()
        {
            if (!File.Exists(ObtenerRutaEcuaciones()))
            {
                GuardarEcuaciones(CrearBase());
            }
        }

        private static void Normalizar(List<EcuacionProductivaDefinicion> ecuaciones)
        {
            foreach (EcuacionProductivaDefinicion ecuacion in ecuaciones)
            {
                if (ecuacion == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Clave))
                {
                    ecuacion.Clave = NormalizarTexto(ecuacion.NombreVisible).ToUpperInvariant();
                }

                if (ecuacion.SchemaVersion <= 0)
                {
                    ecuacion.SchemaVersion = 1;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.NombreVisible))
                {
                    ecuacion.NombreVisible = ecuacion.Clave;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.EcuacionBase))
                {
                    ecuacion.EcuacionBase = InferirEcuacionBase(ecuacion);
                }

                if (string.IsNullOrWhiteSpace(ecuacion.TipoEcuacion))
                {
                    ecuacion.TipoEcuacion = string.IsNullOrWhiteSpace(ecuacion.EcuacionBase)
                        ? "Base"
                        : "Variante";
                }

                MigrarMetadatosProceso(ecuacion);

                if (string.IsNullOrWhiteSpace(ecuacion.CargosPermitidos))
                {
                    ecuacion.CargosPermitidos = InferirCargosPermitidos(ecuacion);
                }

                if (EsFormulaGestionAnterior(ecuacion.FormulaReferencia))
                {
                    ecuacion.Numerador = "dias_productivos * minutos_gestion / 60";
                    ecuacion.Denominador = "horas_dia[cargo]";
                    ecuacion.FormulaReferencia = ConstruirFormulaGestion();
                }
                else if (EsFormulaGestionVectorial(ecuacion.FormulaReferencia))
                {
                    ecuacion.Numerador = "dias_productivos * minutos_gestion / 60";
                    ecuacion.Denominador = "horas_dia[cargo]";
                }

                if (EsFormulaProductivaSimpleAnterior(ecuacion.FormulaReferencia))
                {
                    string numeradorLegacy = ExtraerNumeradorFormula(ecuacion.FormulaReferencia);
                    if (string.IsNullOrWhiteSpace(numeradorLegacy))
                    {
                        numeradorLegacy = InferirNumeradorVectorial(ecuacion);
                    }

                    ecuacion.Numerador = numeradorLegacy;
                    ecuacion.Denominador = "capacidad_por_periodo[cargo]";
                    ecuacion.FormulaReferencia = ConstruirFormulaVectorial(numeradorLegacy);
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Numerador) &&
                    string.IsNullOrWhiteSpace(ecuacion.Denominador) &&
                    !string.IsNullOrWhiteSpace(ecuacion.FormulaReferencia))
                {
                    ecuacion.Numerador = ExtraerNumeradorFormula(ecuacion.FormulaReferencia);
                    ecuacion.Denominador = ExtraerDenominadorFormula(ecuacion.FormulaReferencia);
                }

                if (string.IsNullOrWhiteSpace(ecuacion.FormulaReferencia) &&
                    (!string.IsNullOrWhiteSpace(ecuacion.Numerador) ||
                     !string.IsNullOrWhiteSpace(ecuacion.Denominador)))
                {
                    ecuacion.FormulaReferencia = ConstruirFormulaReferencia(
                        ecuacion.Numerador,
                        ecuacion.Denominador
                    );
                }
            }
        }

        private static bool RequiereMigracionPersistente(
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            if (ecuaciones == null)
            {
                return false;
            }

            return ecuaciones.Any(e =>
                e != null &&
                (EsFormulaProductivaSimpleAnterior(e.FormulaReferencia) ||
                 EsFormulaGestionAnterior(e.FormulaReferencia) ||
                 e.SchemaVersion < 2 ||
                 string.IsNullOrWhiteSpace(e.IdProceso) ||
                 e.TipoProceso == TipoProcesoProductivo.NoClasificado ||
                 e.MetodoCalculo == MetodoCalculoProceso.NoDefinido ||
                 e.AlcanceTemporal == AlcanceTemporalProceso.NoDefinido ||
                 string.IsNullOrWhiteSpace(e.FormulaId) ||
                 string.IsNullOrWhiteSpace(e.Numerador) ||
                 string.IsNullOrWhiteSpace(e.Denominador)));
        }

        private static void MigrarMetadatosProceso(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return;
            }

            List<string> warnings = LeerListaJson(ecuacion.WarningsMigracionJson);
            bool fueMigrada = ecuacion.SchemaVersion < 2 ||
                string.IsNullOrWhiteSpace(ecuacion.IdProceso) ||
                ecuacion.TipoProceso == TipoProcesoProductivo.NoClasificado ||
                ecuacion.MetodoCalculo == MetodoCalculoProceso.NoDefinido ||
                ecuacion.AlcanceTemporal == AlcanceTemporalProceso.NoDefinido;

            if (string.IsNullOrWhiteSpace(ecuacion.IdProceso))
            {
                ecuacion.IdProceso = "proc_" + NormalizarId(
                    string.IsNullOrWhiteSpace(ecuacion.Clave)
                        ? ecuacion.NombreVisible
                        : ecuacion.Clave
                );
            }

            if (string.IsNullOrWhiteSpace(ecuacion.FormulaId))
            {
                ecuacion.FormulaId = string.IsNullOrWhiteSpace(ecuacion.EcuacionBase)
                    ? ecuacion.Clave
                    : ecuacion.EcuacionBase;
            }

            if (string.IsNullOrWhiteSpace(ecuacion.EtapaId))
            {
                ecuacion.EtapaId = NormalizarId(ecuacion.Etapa);
            }

            if (string.IsNullOrWhiteSpace(ecuacion.SubEtapaId))
            {
                ecuacion.SubEtapaId = NormalizarId(ecuacion.SubEtapa);
            }

            if (ecuacion.TipoProceso == TipoProcesoProductivo.NoClasificado)
            {
                ecuacion.TipoProceso = InferirTipoProceso(ecuacion);
                AgregarWarningMigracion(
                    warnings,
                    "TipoProceso inferido por fallback desde clave/nombre. Confirmar clasificación."
                );
            }

            if (ecuacion.MetodoCalculo == MetodoCalculoProceso.NoDefinido)
            {
                ecuacion.MetodoCalculo = InferirMetodoCalculo(ecuacion);
                AgregarWarningMigracion(
                    warnings,
                    "MetodoCalculo inferido por fallback desde fórmula/variables. Confirmar método."
                );
            }

            if (ecuacion.AlcanceTemporal == AlcanceTemporalProceso.NoDefinido)
            {
                ecuacion.AlcanceTemporal = InferirAlcanceTemporal(ecuacion);
                AgregarWarningMigracion(
                    warnings,
                    "AlcanceTemporal inferido por fallback desde tipo de proceso. Confirmar alcance."
                );
            }

            if (string.IsNullOrWhiteSpace(ecuacion.ReglaActivacionJson))
            {
                ReglaActivacionProceso regla = new ReglaActivacionProceso
                {
                    Tipo = ecuacion.TipoProceso == TipoProcesoProductivo.RevisionControl ||
                        ecuacion.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo
                            ? TipoReglaActivacionProceso.ProcesoExistente
                            : TipoReglaActivacionProceso.Manual,
                    ProcesoOrigenId = InferirProcesoOrigenId(ecuacion)
                };
                ecuacion.ReglaActivacionJson = JsonSerializer.Serialize(regla, CrearOpcionesJson());
            }

            if (string.IsNullOrWhiteSpace(ecuacion.DependenciasJson) &&
                (ecuacion.TipoProceso == TipoProcesoProductivo.RevisionControl ||
                 ecuacion.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo))
            {
                string origen = InferirProcesoOrigenId(ecuacion);
                if (!string.IsNullOrWhiteSpace(origen))
                {
                    ecuacion.DependenciasJson = JsonSerializer.Serialize(
                        new List<string> { origen },
                        CrearOpcionesJson()
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(ecuacion.EtapasCubiertasJson) &&
                (ecuacion.AlcanceTemporal == AlcanceTemporalProceso.MultiplesEtapas ||
                 ecuacion.AlcanceTemporal == AlcanceTemporalProceso.ProyectoCompleto))
            {
                List<string> etapas = string.IsNullOrWhiteSpace(ecuacion.EtapaId)
                    ? new List<string>()
                    : new List<string> { ecuacion.EtapaId };
                ecuacion.EtapasCubiertasJson = JsonSerializer.Serialize(etapas, CrearOpcionesJson());
            }

            if (EsProcesoTransversal(ecuacion.TipoProceso))
            {
                ecuacion.PuedeEjecutarseEnParalelo = true;
            }

            if (fueMigrada)
            {
                ecuacion.SchemaVersion = 2;
            }

            ecuacion.WarningsMigracionJson = JsonSerializer.Serialize(
                warnings.Distinct().ToList(),
                CrearOpcionesJson()
            );
        }

        private static bool AgregarEcuacionesBaseFaltantes(
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            bool cambio = false;
            HashSet<string> claves = new HashSet<string>(
                ecuaciones
                    .Where(e => e != null)
                    .Select(e => NormalizarTexto(e.Clave))
                    .Where(c => !string.IsNullOrWhiteSpace(c))
            );

            foreach (EcuacionProductivaDefinicion baseDef in CrearBase()
                .Where(e => e != null))
            {
                string clave = NormalizarTexto(baseDef.Clave);

                if (claves.Contains(clave))
                {
                    continue;
                }

                ecuaciones.Insert(0, baseDef);
                claves.Add(clave);
                cambio = true;
            }

            return cambio;
        }

        private static string InferirEcuacionBase(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            string clave = NormalizarTexto(ecuacion.Clave);

            if (clave == "calculo_por_capacidad" ||
                clave == "calculo_por_duracion" ||
                clave == "calculo_gestion")
            {
                return "";
            }

            if (clave == "rough" ||
                clave == "clean_up" ||
                clave == "color" ||
                clave == "composicion" ||
                clave == "animatic" ||
                clave == "audio")
            {
                return "CALCULO_POR_DURACION";
            }

            if (clave == "direccion_visual")
            {
                return "CALCULO_GESTION";
            }

            if (clave == "guion" ||
                clave == "storyboard" ||
                clave == "preparacion_assets" ||
                clave == "export")
            {
                return "CALCULO_POR_CAPACIDAD";
            }

            string etapa = NormalizarTexto(ecuacion.Etapa);
            string variables = NormalizarTexto(ecuacion.Variables);

            if (variables.Contains("segundos") || variables.Contains("duracion"))
            {
                return "CALCULO_POR_DURACION";
            }

            if (etapa == "general" && string.IsNullOrWhiteSpace(ecuacion.SubEtapa))
            {
                return "";
            }

            return "CALCULO_POR_CAPACIDAD";
        }

        private static string InferirCargosPermitidos(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            string clave = NormalizarTexto(ecuacion.Clave);

            switch (clave)
            {
                case "guion":
                    return "Guionista (tipico); Director/a de animacion (tipico)";
                case "direccion_visual":
                    return "Director de arte (tipico); Director/a de animacion (tipico)";
                case "storyboard":
                    return "Storyboard artist (tipico); Director/a de animacion (tipico)";
                case "animatic":
                    return "Editor (junior); Postproductor (junior)";
                case "preparacion_assets":
                    return "Disenador de personajes (tipico); Director de arte (tipico)";
                case "rough":
                    return "Animador 2D (tipico); Supervisor de animacion (tipico)";
                case "clean_up":
                    return "Clean up artist (tipico); Supervisor de arte (tipico)";
                case "color":
                    return "Colorista (tipico); Supervisor de arte (tipico)";
                case "composicion":
                    return "Compositor (tipico); Postproductor (tipico)";
                case "audio":
                    return "Editor de audio (tipico); Musico / compositor musical (tipico)";
                case "export":
                    return "Render / export manager (tipico); Control de calidad tecnico (tipico)";
                default:
                    return "";
            }
        }

        private static void ResolverHerencias(List<EcuacionProductivaDefinicion> ecuaciones)
        {
            Dictionary<string, EcuacionProductivaDefinicion> porClave = ecuaciones
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Clave))
                .GroupBy(e => NormalizarTexto(e.Clave))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (EcuacionProductivaDefinicion ecuacion in ecuaciones)
            {
                if (ecuacion == null || string.IsNullOrWhiteSpace(ecuacion.EcuacionBase))
                {
                    continue;
                }

                string claveBase = NormalizarTexto(ecuacion.EcuacionBase);
                if (!porClave.TryGetValue(claveBase, out EcuacionProductivaDefinicion baseDef))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Variables))
                {
                    ecuacion.Variables = baseDef.Variables;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Impacto))
                {
                    ecuacion.Impacto = baseDef.Impacto;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.FormulaReferencia))
                {
                    ecuacion.FormulaReferencia = baseDef.FormulaReferencia;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Numerador))
                {
                    ecuacion.Numerador = baseDef.Numerador;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Denominador))
                {
                    ecuacion.Denominador = baseDef.Denominador;
                }

                if (string.IsNullOrWhiteSpace(ecuacion.CargosPermitidos))
                {
                    ecuacion.CargosPermitidos = baseDef.CargosPermitidos;
                }
            }
        }

        private static List<string> Separar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return new List<string>();
            }

            return texto.Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        private static string ExtraerNumeradorFormula(string formula)
        {
            SepararFormulaReferencia(formula, out string numerador, out _);
            return numerador;
        }

        private static string ExtraerDenominadorFormula(string formula)
        {
            SepararFormulaReferencia(formula, out _, out string denominador);
            return denominador;
        }

        private static bool EsFormulaProductivaSimpleAnterior(string formula)
        {
            string f = NormalizarTexto(formula);
            if (string.IsNullOrWhiteSpace(f))
            {
                return false;
            }

            if (!f.StartsWith("dias="))
            {
                return false;
            }

            return f.Contains("/capacidad_periodo") ||
                f.Contains("/capacidad_por_periodo") ||
                f.Contains("/capacidad_diaria") ||
                f.Contains("/rendimiento_por_dia");
        }

        private static bool EsFormulaGestionAnterior(string formula)
        {
            string f = NormalizarTexto(formula);
            if (string.IsNullOrWhiteSpace(f))
            {
                return false;
            }

            return f.Contains("dias_productivos*minutos_gestion/60") &&
                !f.Contains("tarifa_dia") &&
                !f.Contains("horas_dia");
        }

        private static bool EsFormulaGestionVectorial(string formula)
        {
            string f = NormalizarTexto(formula);
            if (string.IsNullOrWhiteSpace(f))
            {
                return false;
            }

            return f.Contains("horas[cargo]=") &&
                f.Contains("dias_productivos*minutos_gestion/60") &&
                f.Contains("horas_dia[cargo]") &&
                f.Contains("tarifa_dia[cargo]");
        }

        private static string InferirNumeradorVectorial(EcuacionProductivaDefinicion ecuacion)
        {
            string variables = NormalizarTexto(ecuacion == null ? "" : ecuacion.Variables);

            if (variables.Contains("segundos") || variables.Contains("duracion"))
            {
                return "segundos";
            }

            if (variables.Contains("piezas"))
            {
                return "piezas";
            }

            return "cantidad";
        }

        private static string ConstruirFormulaVectorial(string numerador)
        {
            if (string.IsNullOrWhiteSpace(numerador))
            {
                numerador = "cantidad";
            }

            return
                "tiempo[cargo] = " + numerador + " / capacidad_por_periodo[cargo]; " +
                "dias_tecnicos = max(tiempo[cargo]); " +
                "costo = dot(tiempo[cargo], tarifa_dia[cargo])";
        }

        private static string ConstruirFormulaGestion()
        {
            return
                "horas[cargo] = dias_productivos * minutos_gestion / 60; " +
                "dias_tecnicos = horas[cargo] / horas_dia[cargo]; " +
                "costo = dot(dias_tecnicos[cargo], tarifa_dia[cargo])";
        }

        private static void SepararFormulaReferencia(
            string formula,
            out string numerador,
            out string denominador
        )
        {
            numerador = "";
            denominador = "";

            string texto = (formula ?? "").Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return;
            }

            int igual = texto.IndexOf('=');
            if (igual >= 0 && igual + 1 < texto.Length)
            {
                texto = texto.Substring(igual + 1).Trim();
            }

            int largoOperador = 1;
            int division = texto.IndexOf('÷');
            if (division < 0)
            {
                division = texto.IndexOf(" / ", StringComparison.Ordinal);
                largoOperador = division >= 0 ? 3 : 1;
            }

            if (division < 0)
            {
                numerador = LimpiarFactorFormula(texto);
                return;
            }

            numerador = LimpiarFactorFormula(texto.Substring(0, division));
            string textoDenominador = texto.Substring(division + largoOperador);
            int finPrimerTermino = textoDenominador.IndexOf(';');
            if (finPrimerTermino >= 0)
            {
                textoDenominador = textoDenominador.Substring(0, finPrimerTermino);
            }

            denominador = LimpiarFactorFormula(textoDenominador);
        }

        private static string LimpiarFactorFormula(string texto)
        {
            return (texto ?? "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim()
                .Replace(" + ", ";");
        }

#pragma warning disable CS0162
        private static string ConstruirFormulaReferencia(string numerador, string denominador)
        {
            string arriba = string.IsNullOrWhiteSpace(numerador)
                ? "No definido"
                : numerador.Replace(";", " + ");

            return ConstruirFormulaVectorial(arriba);

            if (string.IsNullOrWhiteSpace(denominador))
            {
                return "dias = " + arriba;
            }

            return "dias = (" + arriba + ") ÷ (" + denominador.Replace(";", " + ") + ")";
        }

#pragma warning restore CS0162

        private static string ObtenerCarpetaBiblioteca()
        {
            string carpeta = Path.Combine(ObtenerRaizAplicacion(), "Bibliotecas", "default");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            return carpeta;
        }

        private static string ObtenerRaizAplicacion()
        {
            string raizConfigurada = Environment.GetEnvironmentVariable("OTHALART_PROJECT_ROOT") ?? "";
            if (!string.IsNullOrWhiteSpace(raizConfigurada) &&
                Directory.Exists(Path.Combine(raizConfigurada, "Bibliotecas")))
            {
                return raizConfigurada;
            }

            string directorioTrabajo = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(directorioTrabajo) &&
                Directory.Exists(Path.Combine(directorioTrabajo, "Bibliotecas")))
            {
                return directorioTrabajo;
            }

            DirectoryInfo actual = new DirectoryInfo(AppContext.BaseDirectory);

            while (actual != null)
            {
                bool tieneCsproj = actual.GetFiles("*.csproj").Length > 0;
                bool tieneCarpetaData = Directory.Exists(Path.Combine(actual.FullName, "Data"));

                if (tieneCsproj || tieneCarpetaData)
                {
                    return actual.FullName;
                }

                actual = actual.Parent;
            }

            return AppContext.BaseDirectory;
        }

        private static TipoProcesoProductivo InferirTipoProceso(EcuacionProductivaDefinicion ecuacion)
        {
            string texto = NormalizarTexto(
                (ecuacion == null ? "" : ecuacion.Clave + " " + ecuacion.NombreVisible + " " +
                 ecuacion.SubEtapa + " " + ecuacion.Tokens)
            );

            if (texto.Contains("correccion") || texto.Contains("retrabajo"))
            {
                return TipoProcesoProductivo.CorreccionRetrabajo;
            }

            if (texto.Contains("revision") || texto.Contains("aprobacion") || texto.Contains("control"))
            {
                return TipoProcesoProductivo.RevisionControl;
            }

            if (texto.Contains("supervision") || texto.Contains("supervisor"))
            {
                return TipoProcesoProductivo.Supervision;
            }

            if (texto.Contains("direccion") || texto.Contains("director"))
            {
                return TipoProcesoProductivo.Direccion;
            }

            if (texto.Contains("gestion") || texto.Contains("coordinacion") || texto.Contains("seguimiento"))
            {
                return TipoProcesoProductivo.GestionCoordinacion;
            }

            if (texto.Contains("export") || texto.Contains("entrega") || texto.Contains("render"))
            {
                return TipoProcesoProductivo.EntregaSoporte;
            }

            return TipoProcesoProductivo.ProduccionDirecta;
        }

        private static MetodoCalculoProceso InferirMetodoCalculo(EcuacionProductivaDefinicion ecuacion)
        {
            string texto = NormalizarTexto(
                (ecuacion == null ? "" : ecuacion.EcuacionBase + " " + ecuacion.FormulaReferencia + " " +
                 ecuacion.Variables + " " + ecuacion.Numerador + " " + ecuacion.Denominador)
            );
            TipoProcesoProductivo tipo = ecuacion == null
                ? TipoProcesoProductivo.NoClasificado
                : ecuacion.TipoProceso;

            if (tipo == TipoProcesoProductivo.RevisionControl)
            {
                return texto.Contains("porcentaje") || texto.Contains("%")
                    ? MetodoCalculoProceso.PorPorcentajeProduccion
                    : MetodoCalculoProceso.PorRevision;
            }

            if (tipo == TipoProcesoProductivo.CorreccionRetrabajo)
            {
                return MetodoCalculoProceso.PorPorcentajeProduccion;
            }

            if (tipo == TipoProcesoProductivo.Direccion ||
                tipo == TipoProcesoProductivo.Supervision ||
                tipo == TipoProcesoProductivo.GestionCoordinacion)
            {
                if (texto.Contains("proyecto"))
                {
                    return MetodoCalculoProceso.PorDuracionProyecto;
                }

                return MetodoCalculoProceso.PorDuracionEtapa;
            }

            if (texto.Contains("capacidad"))
            {
                return MetodoCalculoProceso.PorCapacidad;
            }

            if (texto.Contains("segundos") || texto.Contains("duracion"))
            {
                return MetodoCalculoProceso.PorDuracionEntregable;
            }

            return MetodoCalculoProceso.PorCantidad;
        }

        private static AlcanceTemporalProceso InferirAlcanceTemporal(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return AlcanceTemporalProceso.NoDefinido;
            }

            if (ecuacion.TipoProceso == TipoProcesoProductivo.Direccion ||
                ecuacion.TipoProceso == TipoProcesoProductivo.GestionCoordinacion)
            {
                return AlcanceTemporalProceso.MultiplesEtapas;
            }

            if (ecuacion.TipoProceso == TipoProcesoProductivo.Supervision)
            {
                return AlcanceTemporalProceso.Etapa;
            }

            if (ecuacion.TipoProceso == TipoProcesoProductivo.EntregaSoporte)
            {
                return AlcanceTemporalProceso.Etapa;
            }

            return AlcanceTemporalProceso.Item;
        }

        private static string InferirProcesoOrigenId(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return "";
            }

            string texto = NormalizarTexto(ecuacion.Clave + " " + ecuacion.NombreVisible + " " + ecuacion.SubEtapa);
            string[] prefijos = new[] { "revision de ", "revision ", "correcciones de ", "correccion de ", "correccion ", "retrabajo de " };
            foreach (string prefijo in prefijos)
            {
                int indice = texto.IndexOf(prefijo, StringComparison.Ordinal);
                if (indice >= 0)
                {
                    string origen = texto.Substring(indice + prefijo.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(origen))
                    {
                        return "proc_" + NormalizarId(origen);
                    }
                }
            }

            return "";
        }

        private static bool EsProcesoTransversal(TipoProcesoProductivo tipo)
        {
            return tipo == TipoProcesoProductivo.Supervision ||
                tipo == TipoProcesoProductivo.Direccion ||
                tipo == TipoProcesoProductivo.GestionCoordinacion;
        }

        private static List<string> LeerListaJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json, CrearOpcionesJson()) ??
                    new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static void AgregarWarningMigracion(List<string> warnings, string warning)
        {
            if (warnings == null || string.IsNullOrWhiteSpace(warning))
            {
                return;
            }

            if (!warnings.Contains(warning))
            {
                warnings.Add(warning);
            }
        }

        private static string NormalizarId(string texto)
        {
            string normalizado = NormalizarTexto(texto);
            if (string.IsNullOrWhiteSpace(normalizado))
            {
                return "";
            }

            char[] chars = normalizado
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray();

            string id = new string(chars);
            while (id.Contains("__"))
            {
                id = id.Replace("__", "_");
            }

            return id.Trim('_');
        }

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            JsonSerializerOptions opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            opciones.Converters.Add(new JsonStringEnumConverter());
            return opciones;
        }

        private static string NormalizarTexto(string texto)
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
    }
}
