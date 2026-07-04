using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class BibliotecaSubEtapasService
    {
        public static List<SubEtapaProyecto> CrearBibliotecaBase()
        {
            List<SubEtapaProyecto> lista = new List<SubEtapaProyecto>();

            int id = 1;

            // =========================
            // DESARROLLO
            // =========================

            lista.Add(Crear(id++, "Desarrollo", "Brief cliente", true,
                "Requerimiento comercial / creativo",
                "Brief validado",
                "Productor ejecutivo; Director/a creativo; Project manager",
                0.10, 0.18, 0.28, 1,
                "Brief; Alcance; RequerimientoCliente",
                "",
                "brief; cliente; requerimiento; pedido; datos cliente; entregable cliente"));

            lista.Add(Crear(id++, "Desarrollo", "Pitch / propuesta creativa", true,
                "Brief validado",
                "Propuesta creativa",
                "Director/a creativo; Guionista; Art director",
                0.15, 0.25, 0.35, 2,
                "Propuesta; ConceptoCreativo; Pitch",
                "Brief",
                "pitch; propuesta; concepto; idea; direccion creativa"));

            lista.Add(Crear(id++, "Desarrollo", "Alcance del proyecto", true,
                "Brief + propuesta creativa",
                "Alcance, entregables y restricciones",
                "Productor ejecutivo; Project manager; Director/a creativo",
                0.10, 0.18, 0.30, 3,
                "Alcance; Restricciones; Entregables",
                "Brief; Propuesta",
                "alcance; entregables; restricciones; presupuesto; plazo; cliente"));

            lista.Add(Crear(id++, "Desarrollo", "Guion base", false,
                "Propuesta creativa",
                "Guion base / estructura narrativa",
                "Guionista; Director/a creativo",
                0.20, 0.30, 0.45, 4,
                "Guion; IdeaBase; Narrativa",
                "Propuesta; Brief",
                "guion; idea base; narrativa; script; historia; estructura"));

            lista.Add(Crear(id++, "Desarrollo", "Definición de estilo", true,
                "Propuesta creativa",
                "Dirección visual base",
                "Art director; Concept artist; Director/a creativo",
                0.15, 0.25, 0.40, 5,
                "Estilo; DireccionVisual; LookAndFeel",
                "Brief; Propuesta",
                "estilo; visual; look; direccion visual; arte; referencia; concept"));

            // =========================
            // PREPRODUCCIÓN
            // =========================

            lista.Add(Crear(id++, "Preproduccion", "Script final", true,
                "Guion base / feedback cliente",
                "Script aprobado",
                "Guionista; Director/a creativo; Project manager",
                0.08, 0.14, 0.22, 1,
                "Script; GuionFinal",
                "Guion; FeedbackCliente",
                "script; guion final; guion aprobado; dialogo; narrativa"));

            lista.Add(Crear(id++, "Preproduccion", "Concept art", true,
                "Definición de estilo",
                "Concept art aprobado",
                "Concept artist; Art director",
                0.10, 0.18, 0.28, 2,
                "ConceptArt; EstiloVisual; ReferenciaVisual",
                "Estilo; DireccionVisual",
                "concept art; concepto visual; props; prop; objeto; estilo; personaje; fondo; background"));

            lista.Add(Crear(id++, "Preproduccion", "Diseño de personajes", true,
                "Concept art / estilo aprobado",
                "Personajes aprobados",
                "Character designer; Art director",
                0.12, 0.20, 0.32, 3,
                "Personaje; CharacterDesign; ModeloPersonaje",
                "ConceptArt; Estilo",
                "personaje; personajes; character; diseño de personaje; modelo personaje"));

            lista.Add(Crear(id++, "Preproduccion", "Diseño de fondos", true,
                "Concept art / estilo aprobado",
                "Fondos o layouts visuales base",
                "Background artist; Art director",
                0.10, 0.18, 0.30, 4,
                "Fondo; BackgroundDesign; Escenario; Ambiente",
                "ConceptArt; Estilo",
                "fondo; fondos; background; backgrounds; escenario; ambiente; diseño de fondos"));

            lista.Add(Crear(id++, "Preproduccion", "Storyboard", true,
                "Script final",
                "Storyboard aprobado",
                "Storyboard artist; Director/a creativo",
                0.18, 0.28, 0.40, 5,
                "Plano; Storyboard; SecuenciaVisual",
                "Guion; Script",
                "plano; planos; storyboard; viñeta; secuencia; escena"));

            lista.Add(Crear(id++, "Preproduccion", "Voces / diálogos", false,
                "Script final",
                "Audio preliminar / voces guía",
                "Director/a creativo; Sound designer; Productor",
                0.05, 0.10, 0.18, 6,
                "Voz; Dialogo; AudioGuia",
                "Script",
                "voz; voces; dialogo; dialogos; audio guia; locucion"));

            lista.Add(Crear(id++, "Preproduccion", "Animatic", true,
                "Storyboard + audio preliminar",
                "Animatic aprobado",
                "Editor; Storyboard artist; Director/a creativo",
                0.12, 0.20, 0.30, 7,
                "Animatic; Previsualizacion; Timing",
                "Storyboard; AudioGuia",
                "animatic; previsualizacion; timing; video guia"));

            lista.Add(Crear(id++, "Preproduccion", "Plan de producción", true,
                "Animatic + diseños aprobados",
                "Plan de producción",
                "Project manager; Productor; Director/a de animación",
                0.05, 0.10, 0.16, 8,
                "PlanProduccion; Planificacion; CalendarioProduccion",
                "Animatic; Personaje; Fondo; Alcance",
                "plan de produccion; planificacion; calendario; gantt; recursos"));

            lista.Add(Crear(id++, "Preproduccion", "Preparación de assets", true,
                "Diseños aprobados",
                "Assets listos para producción",
                "Diseñador de personajes; Director de arte; Productor / Project manager",
                0.08, 0.14, 0.22, 9,
                "AssetsPreparados; ModeloPersonaje; AssetProduccion",
                "ConceptArt; Personaje; Estilo",
                "preparacion de assets; preparación de assets; assets; personaje base; rig; modelo base"));

            // =========================
            // PRODUCCIÓN
            // =========================

            lista.Add(Crear(id++, "Produccion", "Layout", true,
                "Animatic + diseños aprobados",
                "Layout de escenas",
                "Layout artist; Director/a de animación",
                0.08, 0.14, 0.22, 1,
                "Layout; Escena; PlanoProduccion",
                "Storyboard; Animatic; Personaje; Fondo",
                "layout; escena; escenas; plano produccion; blocking"));

            lista.Add(Crear(id++, "Produccion", "Animación", true,
                "Layout + animatic",
                "Animación base",
                "Animador/a 2D; Director/a de animación; Cleanup artist",
                0.45, 0.58, 0.72, 2,
                "Animacion; SegundosAnimados; Loop; Movimiento",
                "Layout; Animatic; Personaje",
                "animacion; animación; segundos animados; loop; movimiento; personaje animado"));

            lista.Add(Crear(id++, "Produccion", "Rough animation", true,
                "Layout + animatic",
                "Animación rough / bloqueo de movimiento",
                "Rough animator; Supervisor de animación; Director/a de animación",
                0.20, 0.32, 0.48, 3,
                "RoughAnimation; AnimacionBase; Movimiento",
                "Layout; Animatic; Personaje",
                "rough; rough animation; animacion rough; animación rough; bloqueo; movimiento base"));

            lista.Add(Crear(id++, "Produccion", "Clean up", true,
                "Rough animation aprobada",
                "Animación limpia",
                "Clean up artist; Artista clean up; Supervisor de animación",
                0.16, 0.26, 0.38, 4,
                "CleanUp; LineaFinal; AnimacionLimpia",
                "RoughAnimation; Animacion",
                "clean up; cleanup; linea limpia; línea limpia; animacion limpia"));

            lista.Add(Crear(id++, "Produccion", "Color", true,
                "Clean up aprobado",
                "Color aplicado",
                "Colorista; Artista de color; Asistente de color",
                0.10, 0.18, 0.28, 5,
                "Color; ColorFinal; Pintura",
                "CleanUp; Animacion",
                "color; colorista; pintura; paint; coloreado"));

            lista.Add(Crear(id++, "Produccion", "Backgrounds", true,
                "Diseño de fondos + layout",
                "Fondos finales",
                "Background artist; Art director",
                0.12, 0.20, 0.32, 6,
                "Background; FondoFinal; EscenarioFinal",
                "Diseño de fondos; Layout",
                "background; backgrounds; fondo; fondos; escenario; escenarios"));

            lista.Add(Crear(id++, "Produccion", "FX animation", false,
                "Animación base / requerimientos de escena",
                "FX animados",
                "FX animator; Compositor",
                0.05, 0.10, 0.20, 7,
                "FX; Efecto; EfectoAnimado",
                "Animacion; Layout",
                "fx; efecto; efectos; particulas; magia; impacto"));

            lista.Add(Crear(id++, "Produccion", "Compositing", true,
                "Animación + fondos + FX",
                "Escenas compuestas",
                "Compositor; Editor; Director/a de animación",
                0.10, 0.18, 0.28, 8,
                "Composicion; Compositing; Integracion; EscenaCompuesta",
                "Animacion; Background; FX",
                "composicion; composición; compositing; integracion; integración; escena compuesta"));

            lista.Add(Crear(id++, "Produccion", "Revisión interna de escenas", true,
                "Escenas compuestas",
                "Correcciones internas",
                "Director/a creativo; Director/a de animación; Project manager",
                0.05, 0.10, 0.18, 9,
                "RevisionInterna; CorreccionInterna; ControlCalidad",
                "Composicion; Animacion",
                "revision; revisión; correccion; corrección; control calidad; qa"));

            // =========================
            // POSTPRODUCCIÓN
            // =========================

            lista.Add(Crear(id++, "Postproduccion", "Edición final", true,
                "Escenas compuestas",
                "Corte final",
                "Editor; Director/a creativo",
                0.18, 0.28, 0.42, 1,
                "EdicionFinal; CorteFinal",
                "Composicion; EscenaCompuesta",
                "edicion; edición; corte final; montaje"));

            lista.Add(Crear(id++, "Postproduccion", "Sound design", false,
                "Corte final / animatic",
                "Diseño sonoro",
                "Sound designer; Editor",
                0.12, 0.20, 0.32, 2,
                "SoundDesign; Sonido; AudioFinal",
                "CorteFinal",
                "sound; sonido; audio; sound design; diseño sonoro"));

            lista.Add(Crear(id++, "Postproduccion", "Música / mezcla", false,
                "Corte final / diseño sonoro",
                "Mezcla final",
                "Sound designer; Músico; Editor",
                0.08, 0.16, 0.28, 3,
                "Musica; Mezcla; AudioMaster",
                "SoundDesign; CorteFinal",
                "musica; música; mezcla; audio master; banda sonora"));

            lista.Add(Crear(id++, "Postproduccion", "Rendering", true,
                "Corte final + mezcla",
                "Render final",
                "Compositor; Editor",
                0.08, 0.14, 0.24, 4,
                "Render; Exportacion; MasterVisual",
                "CorteFinal; Composicion",
                "render; rendering; export; exportacion; exportación; master"));

            lista.Add(Crear(id++, "Postproduccion", "Correcciones finales", true,
                "Render / revisión cliente",
                "Versión final corregida",
                "Editor; Compositor; Project manager",
                0.12, 0.20, 0.32, 5,
                "CorreccionFinal; RevisionCliente; AjusteFinal",
                "Render; EdicionFinal",
                "correcciones finales; revision cliente; revisión cliente; ajuste final"));

            lista.Add(Crear(id++, "Postproduccion", "Entrega final", true,
                "Versión final corregida",
                "Master / archivos finales",
                "Project manager; Editor; Productor",
                0.05, 0.08, 0.15, 6,
                "EntregaFinal; Master; ArchivoFinal",
                "Render; CorreccionFinal",
                "entrega; entrega final; master; archivo final; archivos finales"));

            return lista;
        }

        public static void SincronizarDesdeDesgloseProductivo(
            Cotizacion cotizacion,
            List<SubEtapaProyecto> bibliotecaSubEtapas,
            bool limpiarOpcionales
        )
        {
            if (cotizacion == null)
            {
                return;
            }

            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                return;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                return;
            }

            List<SubEtapaProyecto> subEtapas = ObtenerTodasLasSubEtapas(bibliotecaSubEtapas);

            if (subEtapas.Count == 0)
            {
                return;
            }

            if (limpiarOpcionales)
            {
                foreach (SubEtapaProyecto sub in subEtapas)
                {
                    if (sub == null)
                    {
                        continue;
                    }

                    if (!sub.Requerida)
                    {
                        sub.Activa = false;
                    }
                }
            }

            foreach (RequerimientoProduccionInterna req in cotizacion.DesgloseProductivo.Requerimientos)
            {
                ActivarSubEtapasRelacionadas(subEtapas, req);
            }

            ActivarBaseMinima(subEtapas);

            BibliotecaDependenciasSubEtapasService.AplicarPropuestaMinimaOrdenTemporal(
                subEtapas,
                new List<ResolucionDependenciaSubEtapa>()
            );
        }

        public static List<SubEtapaProyecto> ObtenerSubEtapasRelacionadas(
            List<SubEtapaProyecto> subEtapas,
            RequerimientoProduccionInterna req
        )
        {
            List<SubEtapaProyecto> resultado = new List<SubEtapaProyecto>();

            if (subEtapas == null || req == null)
            {
                return resultado;
            }

            foreach (SubEtapaProyecto sub in subEtapas)
            {
                if (sub == null)
                {
                    continue;
                }

                if (SubEtapaConversaConRequerimiento(sub, req))
                {
                    resultado.Add(sub);
                }
            }

            return resultado;
        }

        private static void ActivarSubEtapasRelacionadas(
            List<SubEtapaProyecto> subEtapas,
            RequerimientoProduccionInterna req
        )
        {
            foreach (SubEtapaProyecto sub in ObtenerSubEtapasRelacionadas(subEtapas, req))
            {
                sub.Activa = true;

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

        private static bool SubEtapaConversaConRequerimiento(
            SubEtapaProyecto sub,
            RequerimientoProduccionInterna req
        )
        {
            string textoReq = Normalizar(
                req.TipoInterno + ";" +
                req.NombreRequerimiento + ";" +
                req.Unidad + ";" +
                req.EtapaSugerida
            );

            string produce = sub.TiposInternosQueProduce;
            string consume = sub.TiposInternosQueConsume;
            string claves = sub.PalabrasClaveActivacion;

            if (CoincidePorTokens(textoReq, produce))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, claves))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, consume))
            {
                string etapaReq = Normalizar(req.EtapaSugerida);
                string etapaSub = Normalizar(sub.EtapaPadre);

                if (etapaReq == etapaSub)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CoincidePorTokens(string textoNormalizado, string tokensSeparados)
        {
            if (string.IsNullOrWhiteSpace(textoNormalizado) ||
                string.IsNullOrWhiteSpace(tokensSeparados))
            {
                return false;
            }

            string[] tokens = tokensSeparados.Split(';');

            foreach (string token in tokens)
            {
                string t = Normalizar(token);

                if (string.IsNullOrWhiteSpace(t))
                {
                    continue;
                }

                if (textoNormalizado.Contains(t) || t.Contains(textoNormalizado))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ActivarBaseMinima(List<SubEtapaProyecto> subEtapas)
        {
            Activar(subEtapas, "Desarrollo", "Brief cliente");
            Activar(subEtapas, "Desarrollo", "Alcance del proyecto");
            Activar(subEtapas, "Preproduccion", "Plan de producción");
            Activar(subEtapas, "Postproduccion", "Entrega final");
        }

        private static void Activar(
            List<SubEtapaProyecto> subEtapas,
            string etapaPadre,
            string nombre
        )
        {
            SubEtapaProyecto sub = Buscar(subEtapas, etapaPadre, nombre);

            if (sub == null)
            {
                return;
            }

            sub.Activa = true;

            if (sub.InicioSemana < 1)
            {
                sub.InicioSemana = 1;
            }

            if (sub.DuracionSemanas < 1)
            {
                sub.DuracionSemanas = 1;
            }
        }

        private static SubEtapaProyecto Buscar(
            List<SubEtapaProyecto> subEtapas,
            string etapaPadre,
            string nombre
        )
        {
            string etapa = Normalizar(etapaPadre);
            string nom = Normalizar(nombre);

            return subEtapas.FirstOrDefault(s =>
                s != null &&
                Normalizar(s.EtapaPadre) == etapa &&
                CoincideNombre(Normalizar(s.Nombre), nom)
            );
        }

        private static List<SubEtapaProyecto> ObtenerTodasLasSubEtapas(
            List<SubEtapaProyecto> bibliotecaSubEtapas
        )
        {
            if (bibliotecaSubEtapas == null)
            {
                return new List<SubEtapaProyecto>();
            }

            return bibliotecaSubEtapas
                .Where(s => s != null)
                .ToList();
        }

        private static SubEtapaProyecto Crear(
            int id,
            string etapaPadre,
            string nombre,
            bool requerida,
            string requiere,
            string entrega,
            string cargosSugeridos,
            double minimo,
            double recomendado,
            double maximo,
            int orden,
            string produce,
            string consume,
            string palabrasClave
        )
        {
            return new SubEtapaProyecto
            {
                Id = id,
                EtapaPadre = etapaPadre,
                Nombre = nombre,
                Activa = true,
                Requerida = requerida,
                Requiere = requiere,
                Entrega = entrega,
                CargosSugeridos = cargosSugeridos,
                PorcentajeMinimoEtapa = minimo,
                PorcentajeRecomendadoEtapa = recomendado,
                PorcentajeMaximoEtapa = maximo,
                Orden = orden,
                Editable = true,

                TiposInternosQueProduce = produce,
                TiposInternosQueConsume = consume,
                PalabrasClaveActivacion = palabrasClave
            };
        }

        private static bool CoincideNombre(string nombreReal, string nombreBuscado)
        {
            if (string.IsNullOrWhiteSpace(nombreReal) ||
                string.IsNullOrWhiteSpace(nombreBuscado))
            {
                return false;
            }

            return nombreReal.Contains(nombreBuscado) ||
                   nombreBuscado.Contains(nombreReal);
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
                .Replace("ñ", "n")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("/", "")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}
