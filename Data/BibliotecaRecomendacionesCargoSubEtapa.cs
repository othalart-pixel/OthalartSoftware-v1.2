using System.Collections.Generic;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Datos
{
    public static class BibliotecaRecomendacionesCargoSubEtapa
    {
        public static List<RecomendacionCargoSubEtapa> CrearBase()
        {
            return new List<RecomendacionCargoSubEtapa>
            {
                /*
                 * GENERALES / TRANSVERSALES
                 */

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "General",
                    SubEtapa = "Todo el proyecto",
                    Cargo = "Productor",
                    NivelSugerido = "típico",
                    EsGeneral = true,
                    Motivo = "Coordina alcance, cliente, tiempos y recursos."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "General",
                    SubEtapa = "Todo el proyecto",
                    Cargo = "Director creativo",
                    NivelSugerido = "típico",
                    EsGeneral = true,
                    Motivo = "Mantiene coherencia creativa transversal."
                },

                /*
                 * DESARROLLO
                 */

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Desarrollo",
                    SubEtapa = "Guion base",
                    Cargo = "Guionista",
                    NivelSugerido = "típico",
                    Motivo = "Desarrolla estructura narrativa, guion o base conceptual."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Desarrollo",
                    SubEtapa = "Definición de estilo",
                    Cargo = "Director de arte",
                    NivelSugerido = "típico",
                    Motivo = "Define línea visual, tono, referencias y criterios estéticos."
                },

                /*
                 * PREPRODUCCIÓN
                 */

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Preproducción",
                    SubEtapa = "Storyboard",
                    Cargo = "Storyboard artist",
                    NivelSugerido = "típico",
                    Motivo = "Convierte guion o idea en estructura visual."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Preproducción",
                    SubEtapa = "Animatic",
                    Cargo = "Editor",
                    NivelSugerido = "típico",
                    Motivo = "Arma ritmo, tiempos y revisión audiovisual previa."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Preproducción",
                    SubEtapa = "Diseño de personajes",
                    Cargo = "Diseñador de personajes",
                    NivelSugerido = "típico",
                    Motivo = "Diseña personajes, expresiones, proporciones y referencias."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Preproducción",
                    SubEtapa = "Diseño de fondos",
                    Cargo = "Artista de fondos",
                    NivelSugerido = "típico",
                    Motivo = "Diseña ambientes y fondos base."
                },

                /*
                 * PRODUCCIÓN
                 */

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Producción",
                    SubEtapa = "Animación",
                    Cargo = "Animador 2D",
                    NivelSugerido = "típico",
                    Motivo = "Ejecuta animación principal."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Producción",
                    SubEtapa = "Clean up",
                    Cargo = "Clean up artist",
                    NivelSugerido = "típico",
                    Motivo = "Limpia línea, consistencia y preparación para color."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Producción",
                    SubEtapa = "Color",
                    Cargo = "Colorista",
                    NivelSugerido = "típico",
                    Motivo = "Aplica color, paleta y acabado visual."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Producción",
                    SubEtapa = "Backgrounds",
                    Cargo = "Artista de fondos",
                    NivelSugerido = "típico",
                    Motivo = "Produce fondos finales o assets visuales."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Producción",
                    SubEtapa = "Compositing",
                    Cargo = "Compositor",
                    NivelSugerido = "típico",
                    Motivo = "Integra capas, efectos y salida visual."
                },

                /*
                 * POSTPRODUCCIÓN
                 */

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Postproducción",
                    SubEtapa = "Edición final",
                    Cargo = "Editor",
                    NivelSugerido = "típico",
                    Motivo = "Cierra versión audiovisual final."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Postproducción",
                    SubEtapa = "Correcciones finales",
                    Cargo = "Editor",
                    NivelSugerido = "típico",
                    Motivo = "Aplica ajustes finales de montaje o entrega."
                },

                new RecomendacionCargoSubEtapa
                {
                    Etapa = "Postproducción",
                    SubEtapa = "Entrega final",
                    Cargo = "Render / export",
                    NivelSugerido = "típico",
                    Motivo = "Prepara masters, formatos y entregables."
                }
            };
        }
    }
}