using System.Collections.Generic;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaEntregablesIndustria
    {
        public static List<Producto2DDefinicion> ObtenerProductosPorIndustria(string industria)
        {
            string i = Normalizar(industria);

            if (i.Contains("videojuego"))
            {
                return CrearVideojuegos();
            }

            if (i.Contains("publicidad") || i.Contains("marketing") || i.Contains("redes"))
            {
                return CrearPublicidad();
            }

            if (
                i.Contains("cine") ||
                i.Contains("serie") ||
                i.Contains("entretenimiento") ||
                i.Contains("streaming") ||
                i.Contains("television") ||
                i.Contains("televisión")
            )
            {
                return CrearAudiovisualNarrativo();
            }

            if (
                i.Contains("educacion") ||
                i.Contains("educación") ||
                i.Contains("e-learning") ||
                i.Contains("corporativo") ||
                i.Contains("documental") ||
                i.Contains("tecnico") ||
                i.Contains("técnico")
            )
            {
                return CrearEducacionCorporativo();
            }

            if (i.Contains("musica") || i.Contains("música") || i.Contains("videoclip"))
            {
                return CrearMusicaVideoclip();
            }

            return CrearGeneral();
        }

        // Compatibilidad con código antiguo.
        // Si en otra parte todavía llamas ObtenerEntregablesPorIndustria(),
        // esto evita que se rompa mientras migras todo a Producto2DDefinicion.
        public static List<string> ObtenerEntregablesPorIndustria(string industria)
        {
            List<string> nombres = new List<string>();

            foreach (Producto2DDefinicion producto in ObtenerProductosPorIndustria(industria))
            {
                nombres.Add(producto.Nombre);
            }

            return nombres;
        }

        private static List<Producto2DDefinicion> CrearVideojuegos()
        {
            return new List<Producto2DDefinicion>
            {
                // =========================
                // DESARROLLO / PREPRODUCCION
                // =========================

                P("Desarrollo / preproducción", "Guion / idea base", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),
                P("Desarrollo / preproducción", "Diseño de personaje 2D", "personajes", "no aplica", 0),
                P("Desarrollo / preproducción", "Diseño de escenario / fondo 2D", "fondos", "no aplica", 0),

                // =========================
                // ASSETS 2D
                // =========================

                P("Assets 2D", "Personaje 2D listo para animar", "personajes", "no aplica", 0),
                P("Assets 2D", "Background / escenario 2D", "fondos", "no aplica", 0),
                P("Assets 2D", "Props / objetos 2D", "assets", "no aplica", 0),
                P("Assets 2D", "UI / elementos visuales 2D", "assets", "no aplica", 0),

                // =========================
                // ANIMACION 2D
                // =========================

                P("Animación 2D", "Animación de personaje 2D", "personajes", "segundos", 2),
                P("Animación 2D", "Loop animado 2D", "loops", "segundos", 2),
                P("Animación 2D", "FX 2D", "assets", "segundos", 1),
                P("Animación 2D", "Spritesheet / export para juego", "piezas", "no aplica", 0),

                // =========================
                // PIEZAS AUDIOVISUALES
                // =========================

                P("Piezas audiovisuales", "Cinemática 2D", "cinemáticas", "segundos", 30),
                P("Piezas audiovisuales", "Trailer / teaser 2D", "piezas", "segundos", 30)
            };
        }

        private static List<Producto2DDefinicion> CrearPublicidad()
        {
            return new List<Producto2DDefinicion>
            {
                // =========================
                // DESARROLLO / PREPRODUCCION
                // =========================

                P("Desarrollo / preproducción", "Brief creativo", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Guion / mensaje comercial", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),

                // =========================
                // DISEÑO VISUAL
                // =========================

                P("Diseño visual", "Diseño gráfico / dirección visual", "piezas", "no aplica", 0),
                P("Diseño visual", "Diseño de personaje 2D", "personajes", "no aplica", 0),
                P("Diseño visual", "Fondos / escenarios 2D", "fondos", "no aplica", 0),

                // =========================
                // ANIMACION 2D
                // =========================

                P("Animación 2D", "Comercial animado 2D", "piezas", "segundos", 30),
                P("Animación 2D", "Reel / pieza corta 2D", "piezas", "segundos", 15),
                P("Animación 2D", "Motion graphics 2D", "piezas", "segundos", 30),
                P("Animación 2D", "Animación de personaje 2D", "personajes", "segundos", 2),

                // =========================
                // POSTPRODUCCION / ENTREGA
                // =========================

                P("Postproducción / entrega", "Adaptaciones por formato", "piezas", "no aplica", 0),
                P("Postproducción / entrega", "Edición final", "piezas", "segundos", 30),
                P("Postproducción / entrega", "Subtítulos / gráfica animada", "piezas", "segundos", 30)
            };
        }

        private static List<Producto2DDefinicion> CrearAudiovisualNarrativo()
        {
            return new List<Producto2DDefinicion>
            {
                // =========================
                // DESARROLLO / PREPRODUCCION
                // =========================

                P("Desarrollo / preproducción", "Guion / escena", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),
                P("Desarrollo / preproducción", "Diseño de personajes 2D", "personajes", "no aplica", 0),
                P("Desarrollo / preproducción", "Diseño de fondos 2D", "fondos", "no aplica", 0),

                // =========================
                // PRODUCCION 2D
                // =========================

                P("Producción 2D", "Animación 2D", "segundos", "segundos", 30),
                P("Producción 2D", "Clean up", "segundos", "segundos", 30),
                P("Producción 2D", "Color", "segundos", "segundos", 30),
                P("Producción 2D", "Composición", "segundos", "segundos", 30),

                // =========================
                // PIEZAS AUDIOVISUALES
                // =========================

                P("Piezas audiovisuales", "Cinemática 2D", "cinemáticas", "segundos", 30),
                P("Piezas audiovisuales", "Trailer / teaser 2D", "piezas", "segundos", 30),
                P("Piezas audiovisuales", "Capítulo 2D", "capítulos", "minutos", 5),

                // =========================
                // POSTPRODUCCION / ENTREGA
                // =========================

                P("Postproducción / entrega", "Edición final", "piezas", "segundos", 30),
                P("Postproducción / entrega", "Entrega final", "piezas", "no aplica", 0)
            };
        }

        private static List<Producto2DDefinicion> CrearEducacionCorporativo()
        {
            return new List<Producto2DDefinicion>
            {
                P("Desarrollo / preproducción", "Brief técnico / levantamiento", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Guion explicativo", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),

                P("Diseño visual", "Dirección visual / estilo gráfico", "piezas", "no aplica", 0),
                P("Diseño visual", "Infografía / elementos gráficos 2D", "assets", "no aplica", 0),
                P("Diseño visual", "Diseño de personaje 2D", "personajes", "no aplica", 0),

                P("Animación 2D", "Video explicativo 2D", "piezas", "segundos", 60),
                P("Animación 2D", "Animación técnica 2D", "piezas", "segundos", 60),
                P("Animación 2D", "Motion graphics 2D", "piezas", "segundos", 30),

                P("Postproducción / entrega", "Locución / audio", "piezas", "segundos", 60),
                P("Postproducción / entrega", "Subtítulos / gráfica animada", "piezas", "segundos", 60),
                P("Postproducción / entrega", "Edición final", "piezas", "segundos", 60)
            };
        }

        private static List<Producto2DDefinicion> CrearMusicaVideoclip()
        {
            return new List<Producto2DDefinicion>
            {
                P("Desarrollo / preproducción", "Concepto visual", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),

                P("Diseño visual", "Dirección visual", "piezas", "no aplica", 0),
                P("Diseño visual", "Background / escenario 2D", "fondos", "no aplica", 0),
                P("Diseño visual", "Diseño de personaje 2D", "personajes", "no aplica", 0),

                P("Animación 2D", "Videoclip 2D", "piezas", "segundos", 60),
                P("Animación 2D", "Loop visual 2D", "loops", "segundos", 5),
                P("Animación 2D", "FX 2D", "assets", "segundos", 1),

                P("Postproducción / entrega", "Composición / postproducción", "piezas", "segundos", 60),
                P("Postproducción / entrega", "Edición final", "piezas", "segundos", 60),
                P("Postproducción / entrega", "Trailer / teaser 2D", "piezas", "segundos", 30)
            };
        }

        private static List<Producto2DDefinicion> CrearGeneral()
        {
            return new List<Producto2DDefinicion>
            {
                P("Desarrollo / preproducción", "Guion / idea base", "piezas", "no aplica", 0),
                P("Desarrollo / preproducción", "Storyboard", "escenas", "no aplica", 0),
                P("Desarrollo / preproducción", "Animatic", "piezas", "segundos", 30),
                

                P("Diseño visual", "Diseño de personaje 2D", "personajes", "no aplica", 0),
                P("Diseño visual", "Background / escenario 2D", "fondos", "no aplica", 0),
                P("Diseño visual", "Props / objetos 2D", "assets", "no aplica", 0),
                
                P("Narrativa visual", "Comic", "páginas", "no aplica", 0),

                P("Animación 2D", "Animación de personaje 2D", "personajes", "segundos", 2),
                P("Animación 2D", "Motion graphics 2D", "piezas", "segundos", 30),

                P("Piezas audiovisuales", "Cinemática 2D", "cinemáticas", "segundos", 30),
                P("Piezas audiovisuales", "Trailer / teaser 2D", "piezas", "segundos", 30),

                P("Postproducción / entrega", "Edición final", "piezas", "segundos", 30)
            };
        }

        private static Producto2DDefinicion P(
            string categoria,
            string nombre,
            string unidadCantidad,
            string unidadDuracion,
            double duracion
        )
        {
            return new Producto2DDefinicion
            {
                Categoria = categoria,
                Nombre = nombre,
                UnidadCantidadSugerida = unidadCantidad,
                UnidadDuracionSugerida = unidadDuracion,
                DuracionSugerida = duracion
            };
        }

        private static string Normalizar(string texto)
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