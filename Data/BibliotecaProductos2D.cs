using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaProductos2D
    {
        // =========================================================
        // API PÚBLICA
        // =========================================================

        public static List<Producto2DDefinicion> CrearBase()
        {
            return BibliotecaProductos2DJsonService.CargarProductos();
        }

        public static List<Producto2DDefinicion> CrearBaseDesdeCodigo()
        {
            return new List<Producto2DDefinicion>
            {
                CrearComic(),
                CrearAnimacionPersonaje(),
                CrearLoopAnimado(),
                CrearSpritesheetJuego(),
                CrearFx2D(),
                CrearDisenoPersonaje(),
                CrearBackground(),
                CrearProps(),
                CrearPackAssets(),
                CrearRiggingCutOut(),
                CrearCleanUpColor(),
                CrearComercialAnimado(),
                CrearCinematica(),
                CrearTrailer(),
                CrearMotionGraphics(),
                CrearVideoExplicativo(),
                CrearStoryboard(),
                CrearAnimatic(),
                CrearSonidoAnimacion(),
                CrearLogoAnimado()
            };
        }

        public static List<string> ObtenerProductos()
        {
            return CrearBase()
                .Select(p => p.Nombre)
                .ToList();
        }

        public static Producto2DDefinicion ObtenerProducto(string nombre)
        {
            string n = Normalizar(nombre);

            return CrearBase()
                .FirstOrDefault(p => Normalizar(p.Nombre) == n);
        }

        public static List<Subproducto2D> ObtenerSubproductos(string nombreProducto)
        {
            Producto2DDefinicion producto = ObtenerProducto(nombreProducto);

            if (producto == null)
            {
                return new List<Subproducto2D>();
            }

            return producto.Subproductos ?? new List<Subproducto2D>();
        }

        public static List<string> ObtenerDestinosUsoGenerales()
        {
            return new List<string>
            {
                "Videojuego",
                "Steam",
                "Mobile",
                "PC",
                "Consola",
                "Web",
                "Redes sociales",
                "Pitch",
                "Trailer",
                "Cinemática interna",
                "Cutscene",
                "UI animada",
                "Assets in-game",
                "Publicidad",
                "Serie / piloto",
                "Videoclip",
                "Presentación comercial",
                "Campaña digital"
            };
        }

        // =========================================================
        // PRODUCTOS 2D
        // =========================================================

        private static Producto2DDefinicion CrearComic()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Comic",
                Industria = "General",
                Categoria = "Narrativa visual",
                UnidadCantidadSugerida = "páginas",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
        {
            SubD("Brief de comic", "Definición de alcance", true, true),
            SubD("Guion / estructura narrativa", "Guion", true, true),
            SubD("Referencias visuales comic", "Investigación de referencias", true, true),

            SubPre("Diseño / preparación visual comic", "Preparación de assets", true, true),
            SubPre("Layout de página / viñetas", "Storyboard / layout", true, false),

            SubPro("Boceto comic", "Boceto", true, false),
            SubPro("Línea final comic", "Clean up", true, false),
            SubPro("Color comic", "Color", true, false),

            SubPost("Rotulación / textos comic", "Rotulación", true, false),
            SubPost("Export comic", "Entrega final", true, false)
        }
            };
        }


        private static Producto2DDefinicion CrearAnimacionPersonaje()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Animación de personaje 2D",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "personajes",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 2.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de movimiento", "Definición de alcance", true, true),
                    SubD("Referencias de movimiento", "Investigación de referencias", true, true),

                    SubPre("Preparación de personaje base", "Preparación de assets", true, true),

                    SubPro("Rough animation", "Rough animation", true, false),
                    SubPro("Animación 2D final", "Animación final", true, false),
                    SubPro("Clean up", "Clean up", true, false),
                    SubPro("Color", "Color", true, false),

                    SubPost("Export final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearLoopAnimado()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Loop animado 2D",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 2.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de loop", "Definición de alcance", true, true),
                    SubD("Referencias de movimiento", "Investigación de referencias", true, true),

                    SubPre("Diseño / preparación visual", "Preparación de assets", true, true),

                    SubPro("Rough animation loop", "Rough animation", true, false),
                    SubPro("Animación loop final", "Animación final", true, false),
                    SubPro("Clean up", "Clean up", true, false),
                    SubPro("Color", "Color", true, false),

                    SubPost("Export loop seamless", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearSpritesheetJuego()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Spritesheet / export para juego",
                Industria = "Videojuegos",
                Categoria = "Assets",
                UnidadCantidadSugerida = "sets",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Lista de animaciones requeridas", "Definición de alcance", true, true),
                    SubD("Requisitos técnicos del juego", "Definición técnica", true, true),

                    SubPre("Preparación de personaje para juego", "Preparación de assets", true, true),

                    SubPro("Rough animation idle", "Rough animation", true, false),
                    SubPro("Idle animation final", "Animación final", true, false),

                    SubPro("Rough animation walk / run", "Rough animation", true, false),
                    SubPro("Walk / run cycle final", "Animación final", true, false),

                    SubPro("Rough animation acción / ataque", "Rough animation", false, false),
                    SubPro("Acción / ataque / interacción final", "Animación final", false, false),

                    SubPro("Clean up de animaciones", "Clean up", true, false),
                    SubPro("Color de animaciones", "Color", true, false),

                    SubPost("Export PNG sequence", "Entrega final", false, false),
                    SubPost("Export spritesheet", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearFx2D()
        {
            return new Producto2DDefinicion
            {
                Nombre = "FX 2D",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "assets",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 1.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief FX", "Definición de alcance", true, true),
                    SubD("Referencias FX", "Investigación de referencias", true, true),

                    SubPre("Diseño FX", "Diseño visual", true, false),

                    SubPro("Rough animation FX", "Rough animation", true, false),
                    SubPro("Animación FX final", "Animación final", true, false),
                    SubPro("Clean up / color FX", "Color", true, false),
                    SubPro("Diseño sonoro FX", "Sonido", false, true),

                    SubPost("Export FX", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearDisenoPersonaje()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Diseño de personaje 2D",
                Industria = "General",
                Categoria = "Diseño",
                UnidadCantidadSugerida = "personajes",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de personaje", "Definición de alcance", true, true),
                    SubD("Referencias visuales", "Investigación de referencias", true, true),

                    SubPre("Bocetos de personaje", "Diseño visual", true, false),
                    SubPre("Diseño final de personaje", "Diseño visual", true, false),
                    SubPre("Expresiones / poses base", "Diseño visual", false, false),

                    SubPost("Archivo editable / export", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearBackground()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Background / escenario 2D",
                Industria = "General",
                Categoria = "Diseño",
                UnidadCantidadSugerida = "fondos",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de escenario", "Definición de alcance", true, true),
                    SubD("Referencias visuales", "Investigación de referencias", true, true),

                    SubPre("Boceto de fondo", "Diseño visual", true, false),

                    SubPro("Fondo final", "Arte final", true, false),

                    SubPost("Separación por capas", "Preparación de entrega", false, false),
                    SubPost("Archivo editable / export", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearProps()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Props / objetos 2D",
                Industria = "General",
                Categoria = "Diseño",
                UnidadCantidadSugerida = "assets",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de objetos", "Definición de alcance", true, true),
                    SubD("Referencias visuales", "Investigación de referencias", true, true),

                    SubPre("Bocetos de props", "Diseño visual", true, false),

                    SubPro("Props finales", "Arte final", true, false),

                    SubPost("Export de assets", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearPackAssets()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Pack de assets 2D",
                Industria = "General",
                Categoria = "Assets",
                UnidadCantidadSugerida = "assets",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Lista de assets", "Definición de alcance", true, true),
                    SubD("Dirección visual", "Definición de estilo", true, true),

                    SubPre("Diseño de assets", "Diseño visual", true, false),

                    SubPro("Producción de assets", "Arte final", true, false),

                    SubPost("Export de assets", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearRiggingCutOut()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Rigging cut-out 2D",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "personajes",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Revisión de personaje", "Definición de alcance", true, true),

                    SubPre("Separación por capas", "Preparación de assets", true, true),
                    SubPre("Preparación de pivotes", "Preparación de assets", true, false),

                    SubPro("Rigging cut-out", "Rigging", true, false),
                    SubPro("Prueba de movimiento", "Animación de prueba", true, false),

                    SubPost("Archivo editable / export", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearCleanUpColor()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Clean up / color",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 2.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Revisión de animación rough", "Definición de alcance", true, true),

                    SubPro("Clean up", "Clean up", true, false),
                    SubPro("Color", "Color", true, false),
                    SubPro("Control de consistencia", "Control de calidad", true, false),

                    SubPost("Export final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearComercialAnimado()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Comercial animado 2D",
                Industria = "Publicidad / marketing",
                Categoria = "Audiovisual",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief comercial", "Definición de alcance", true, true),
                    SubD("Guion / mensaje", "Guion", true, true),
                    SubD("Dirección visual", "Definición de estilo", true, true),

                    SubPre("Storyboard", "Storyboard", true, true),
                    SubPre("Animatic", "Animatic", true, true),
                    SubPre("Diseño de personajes", "Diseño visual", false, true),
                    SubPre("Diseño de fondos", "Diseño visual", false, true),
                    SubPre("Diseño de props", "Diseño visual", false, true),

                    SubPro("Rough animation", "Rough animation", true, false),
                    SubPro("Animación 2D final", "Animación final", true, false),
                    SubPro("Clean up", "Clean up", false, false),
                    SubPro("Color", "Color", false, false),
                    SubPro("Composición", "Compositing", true, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Edición final", "Edición", true, false),
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Entrega final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearCinematica()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Cinemática 2D",
                Industria = "General",
                Categoria = "Audiovisual",
                UnidadCantidadSugerida = "cinemáticas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Guion / escena", "Guion", true, true),
                    SubD("Dirección visual", "Definición de estilo", true, true),

                    SubPre("Storyboard", "Storyboard", true, true),
                    SubPre("Animatic", "Animatic", true, true),
                    SubPre("Diseño de personajes", "Diseño visual", true, true),
                    SubPre("Diseño de fondos", "Diseño visual", true, true),
                    SubPre("Diseño de props", "Diseño visual", false, true),

                    SubPro("Rough animation", "Rough animation", true, false),
                    SubPro("Animación 2D final", "Animación final", true, false),
                    SubPro("Clean up", "Clean up", true, false),
                    SubPro("Color", "Color", true, false),
                    SubPro("Composición", "Compositing", true, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución / diálogos", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Edición final", "Edición", true, false),
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Entrega final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearTrailer()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Trailer / teaser 2D",
                Industria = "General",
                Categoria = "Audiovisual",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Guion / concepto", "Guion", true, true),
                    SubD("Dirección visual", "Definición de estilo", false, true),

                    SubPre("Storyboard", "Storyboard", true, true),
                    SubPre("Animatic", "Animatic", true, true),

                    SubPro("Rough animation", "Rough animation", true, false),
                    SubPro("Animación 2D final", "Animación final", true, false),
                    SubPro("Clean up", "Clean up", false, false),
                    SubPro("Color", "Color", false, false),
                    SubPro("Composición", "Compositing", true, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Edición final", "Edición", true, false),
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Entrega final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearMotionGraphics()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Motion graphics 2D",
                Industria = "General",
                Categoria = "Audiovisual",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Guion / mensaje", "Guion", true, true),
                    SubD("Dirección visual", "Definición de estilo", true, true),

                    SubPre("Diseño de piezas gráficas", "Diseño visual", true, true),
                    SubPre("Storyboard simple", "Storyboard", false, true),
                    SubPre("Animatic simple", "Animatic", false, true),

                    SubPro("Rough motion / animación base", "Rough animation", true, false),
                    SubPro("Animación gráfica final", "Animación final", true, false),
                    SubPro("Composición", "Compositing", true, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Edición final", "Edición", true, false),
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Entrega final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearVideoExplicativo()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Video explicativo animado",
                Industria = "General",
                Categoria = "Audiovisual",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 60.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief explicativo", "Definición de alcance", true, true),
                    SubD("Guion explicativo", "Guion", true, true),
                    SubD("Dirección visual", "Definición de estilo", true, true),

                    SubPre("Storyboard", "Storyboard", true, true),
                    SubPre("Animatic", "Animatic", true, true),
                    SubPre("Diseño de elementos gráficos", "Diseño visual", true, true),

                    SubPro("Rough animation", "Rough animation", true, false),
                    SubPro("Animación 2D final", "Animación final", true, false),
                    SubPro("Composición", "Compositing", true, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución", "Sonido", true, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Edición final", "Edición", true, false),
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Entrega final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearStoryboard()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Storyboard",
                Industria = "General",
                Categoria = "Preproducción",

                /*
                 * Simple beta:
                 * No pedimos planos como input obligatorio.
                 * Se ingresa como pieza/entregable y la complejidad se ajusta después.
                 */
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "no aplica",
                DuracionSugerida = 0.0,

                Subproductos = new List<Subproducto2D>
                {
                    SubD("Guion / idea base", "Guion", true, true),

                    SubPre("Storyboard", "Storyboard", true, false),
                    SubPre("Revisión de storyboard", "Storyboard", true, false),

                    SubPost("Entrega storyboard", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearAnimatic()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Animatic",
                Industria = "General",
                Categoria = "Preproducción",

                /*
                 * Regla simple:
                 * El cliente/usuario NO ingresa planos.
                 * El animatic se define como una pieza con duración final.
                 * El costeo se calcula después por segundos/minutos.
                 */
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,

                Subproductos = new List<Subproducto2D>
                {
                    SubD("Guion / idea base", "Guion", true, true),

                    /*
                     * El storyboard puede venir del cliente.
                     * Si no viene, Othalart lo produce.
                     */
                    SubPre("Storyboard base para animatic", "Storyboard", true, true),

                    /*
                     * El animatic como tal se produce por duración final.
                     * No se piden planos en esta versión simple.
                     */
                    SubPre("Edición animatic", "Animatic", true, false),

                    /*
                     * Audio guía y sincronización son producción sonora,
                     * aunque estén al servicio del animatic.
                     */
                    SubPro("Audio guía", "Sonido guía", false, true),
                    SubPro("Sincronización audio guía", "Sonido guía", false, false),

                    SubPost("Entrega animatic", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearSonidoAnimacion()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Sonido / audio para animación",
                Industria = "General",
                Categoria = "Audio",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 30.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de sonido", "Definición de alcance", true, true),
                    SubD("Referencias sonoras", "Investigación de referencias", true, true),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Locución / diálogos", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", true, false),
                    SubPro("Sincronización de audio", "Sonido", true, false),

                    // Cierre/post.
                    SubPost("Mezcla final", "Sonido", true, false),
                    SubPost("Export audio final", "Entrega final", true, false)
                }
            };
        }

        private static Producto2DDefinicion CrearLogoAnimado()
        {
            return new Producto2DDefinicion
            {
                Nombre = "Logo animado",
                Industria = "General",
                Categoria = "Animación",
                UnidadCantidadSugerida = "piezas",
                UnidadDuracionSugerida = "segundos",
                DuracionSugerida = 3.0,
                Subproductos = new List<Subproducto2D>
                {
                    SubD("Brief de logo animado", "Definición de alcance", true, true),
                    SubD("Referencias de movimiento", "Investigación de referencias", true, true),

                    SubPre("Preparación de logo / capas", "Preparación de assets", true, true),

                    SubPro("Rough animation de logo", "Rough animation", true, false),
                    SubPro("Animación final de logo", "Animación final", true, false),
                    SubPro("Composición", "Compositing", false, false),

                    // Producción sonora.
                    SubPro("Música / ambiente", "Sonido", false, true),
                    SubPro("Diseño sonoro base", "Sonido", false, true),
                    SubPro("Sincronización de audio", "Sonido", false, false),

                    // Cierre/post.
                    SubPost("Mezcla final", "Sonido", false, false),
                    SubPost("Export final", "Entrega final", true, false)
                }
            };
        }

        // =========================================================
        // HELPERS DE CATEGORÍA
        // =========================================================

        private static Subproducto2D SubD(
            string nombre,
            string subEtapa,
            bool requerido,
            bool puedeEntregarCliente
        )
        {
            return Sub(nombre, "Desarrollo", subEtapa, requerido, puedeEntregarCliente);
        }

        private static Subproducto2D SubPre(
            string nombre,
            string subEtapa,
            bool requerido,
            bool puedeEntregarCliente
        )
        {
            return Sub(nombre, "Preproduccion", subEtapa, requerido, puedeEntregarCliente);
        }

        private static Subproducto2D SubPro(
            string nombre,
            string subEtapa,
            bool requerido,
            bool puedeEntregarCliente
        )
        {
            return Sub(nombre, "Produccion", subEtapa, requerido, puedeEntregarCliente);
        }

        private static Subproducto2D SubPost(
            string nombre,
            string subEtapa,
            bool requerido,
            bool puedeEntregarCliente
        )
        {
            return Sub(nombre, "Postproduccion", subEtapa, requerido, puedeEntregarCliente);
        }

        private static Subproducto2D Sub(
            string nombre,
            string etapa,
            string subEtapa,
            bool requerido,
            bool puedeEntregarCliente
        )
        {
            return new Subproducto2D
            {
                Nombre = nombre,
                EtapaSugerida = etapa,
                SubEtapaSugerida = subEtapa,
                RequeridoPorDefecto = requerido,
                PuedeEntregarCliente = puedeEntregarCliente
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
