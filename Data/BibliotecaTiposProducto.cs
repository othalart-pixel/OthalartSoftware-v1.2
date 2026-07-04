using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaTiposProducto
    {
        // =========================================================
        // PRODUCTOS / SERVICIOS VENDIBLES 2D
        // =========================================================
        // Producto vendible = lo que el cliente compra.
        //
        // No mezclar aquí:
        // - formato de archivo,
        // - resolución,
        // - editable,
        // - paquete técnico,
        // - codec,
        // - especificaciones de exportación,
        // - tareas técnicas internas de cierre.
        //
        // Nota:
        // No usamos "Solo animación" porque es demasiado ambiguo.
        // Si se vende animación parcial, se especifica el proceso:
        // rough, key animation, inbetween, clean-up, color, composición, etc.

        public static List<string> CrearTiposBase()
        {
            return CrearProductosVendibles2D();
        }

        public static List<string> CrearProductosVendibles2D()
        {
            return new List<string>
            {
                // =================================================
                // PIEZAS AUDIOVISUALES COMPLETAS
                // =================================================

                "Comercial animado",
                "Pieza animada para redes",
                "Reel / short animado",
                "Cinemática / cutscene",
                "Trailer / teaser animado",
                "Videoclip animado",
                "Cortometraje animado",
                "Piloto de serie",
                "Capítulo de serie",
                "Video explicativo",
                "Video educativo",
                "Video corporativo animado",
                "Animación técnica / documental",
                "Motion graphics",

                // =================================================
                // VIDEOJUEGOS / INTERACTIVO
                // =================================================
                // El cliente normalmente no pide "walk cycle".
                // Pide personajes, acciones animadas, assets, fondos,
                // props, UI, FX, etc. El software traduce internamente
                // esas necesidades a ciclos, loops, frames y carga.

                "Pack de assets 2D para videojuego",
                "Animaciones de personaje para videojuego",
                "Personaje 2D animado",
                "Enemigo / criatura 2D animada",
                "NPC 2D animado",
                "Spritesheet / animación por frames",
                "UI animada",
                "FX 2D para videojuego",
                "Backgrounds / escenarios",
                "Props / objetos",
                "Diseño de personajes",
                "Pack de personajes",
                "Pack de fondos",
                "Pack de props",

                // =================================================
                // DESARROLLO Y PREPRODUCCIÓN
                // =================================================

                "Desarrollo creativo",
                "Guion",
                "Biblia visual",
                "Dirección de arte",
                "Diseño visual",
                "Diseño de personajes",
                "Diseño de fondos",
                "Diseño de props",
                "Storyboard",
                "Animatic",
                "Layout",

                // =================================================
                // SERVICIOS PARCIALES DE ANIMACIÓN
                // =================================================
                // Estos sí pueden venderse como servicios parciales,
                // siempre que el alcance esté bien definido.

                "Rough animation",
                "Key animation",
                "Inbetween / asistencia de animación",
                "Clean-up",
                "Color",
                "Animación cut-out",
                "Animación frame by frame",
                "Animación de acting",
                "Animación de FX",

                // =================================================
                // POSTPRODUCCIÓN
                // =================================================
                // Render/export NO se ofrece como producto comercial.
                // Es cierre técnico de entrega, no servicio vendible principal.

                "Composición",
                "Edición",
                "Corrección final",
                "Postproducción completa",

                // =================================================
                // APOYO ESTRATÉGICO
                // =================================================

                "Consultoría creativa",
                "Desarrollo de pitch",
                "Supervisión de producción",
                "Otro"
            };
        }

        // =========================================================
        // UNIDADES DE DURACIÓN / TIEMPO
        // =========================================================
        // Esto responde:
        // ¿Cómo se mide la duración o extensión temporal?
        //
        // Loop NO va como unidad.
        // "Loopable" es una especificación técnica.
        //
        // Ciclo de animación puede existir internamente, pero para cliente
        // suele ser más claro hablar de "acciones animadas".

        public static List<string> CrearUnidadesDuracion()
        {
            return new List<string>
            {
                "segundos",
                "minutos",
                "frames",
                "planos",
                "escenas",
                "capítulos",
                "acciones animadas",
                "no aplica"
            };
        }

        // =========================================================
        // UNIDADES DE CANTIDAD / ALCANCE
        // =========================================================
        // Esto responde:
        // ¿Cuántas unidades se piden?
        //
        // Para videojuegos:
        // Cliente dice "5 acciones animadas".
        // Internamente el estudio puede traducir:
        // idle, caminar, correr, ataque, daño, muerte, etc.

        public static List<string> CrearUnidadesCantidad()
        {
            return new List<string>
            {
                "piezas",
                "unidades",
                "versiones",
                "personajes",
                "fondos",
                "props",
                "assets",
                "planos",
                "escenas",
                "capítulos",
                "acciones animadas",
                "animaciones de personaje",
                "sprites",
                "spritesheets",
                "pantallas",
                "segundos",
                "minutos",
                "no aplica"
            };
        }

        // =========================================================
        // ACCIONES ANIMADAS PARA VIDEOJUEGO / INTERACTIVO
        // =========================================================
        // Esta lista es más interna/comercial asistida.
        // El cliente puede elegir "acciones animadas", y luego se detalla
        // cuáles acciones corresponden.
        //
        // Ojo:
        // "Caminar" puede traducirse internamente a walk cycle.
        // "Idle / espera" puede ser loopable.
        // "Loopable" sigue siendo especificación técnica, no unidad.

        public static List<string> CrearAccionesAnimadasVideojuego()
        {
            return new List<string>
            {
                "Idle / espera",
                "Caminar",
                "Correr",
                "Saltar",
                "Caer",
                "Ataque",
                "Ataque especial",
                "Daño / hit reaction",
                "Muerte",
                "Interacción",
                "Emote",
                "Transición",
                "Loop ambiental",
                "Otro"
            };
        }

        // =========================================================
        // USO / DESTINO DEL PRODUCTO
        // =========================================================
        // Esto responde:
        // ¿Dónde se va a usar?
        //
        // Influye en:
        // - resolución,
        // - relación de aspecto,
        // - compresión,
        // - codec,
        // - derechos de uso,
        // - cantidad de versiones,
        // - exigencia técnica.

        public static List<string> CrearUsosBase()
        {
            return new List<string>
            {
                "Redes sociales",
                "TikTok / Reels / Shorts",
                "YouTube",
                "Publicidad digital",
                "Campaña pagada",
                "TV",
                "Web",
                "Landing page",
                "Pitch / presentación",
                "Videojuego",
                "App mobile",
                "Educación",
                "E-learning",
                "Corporativo interno",
                "Evento / pantalla",
                "Festival",
                "Streaming",
                "Documental",
                "Museografía / instalación",
                "Otro"
            };
        }

        // =========================================================
        // FORMATOS FINALES DE ENTREGA
        // =========================================================
        // Formato = archivo final usable.
        //
        // No confundir con:
        // - editables,
        // - paquetes,
        // - especificaciones técnicas,
        // - cierre técnico interno.

        public static List<string> CrearFormatosEntrega()
        {
            return CrearFormatosEntregaFinal();
        }

        public static List<string> CrearFormatosEntregaFinal()
        {
            return new List<string>
            {
                // Video final
                "MP4 H.264",
                "MP4 H.265 / HEVC",
                "MOV ProRes 422",
                "MOV ProRes 4444 con alpha",
                "WebM",
                "GIF",

                // Secuencias
                "PNG sequence",
                "PNG sequence con alpha",
                "EXR sequence",
                "TIFF sequence",
                "JPEG sequence",

                // Web / interactivo
                "Lottie / JSON",
                "SVG animado",
                "HTML5",

                // Videojuego / assets
                "Spritesheet PNG",
                "Spritesheet PNG con alpha",
                "Atlas de sprites",
                "Tileset",
                "Assets separados en PNG",

                // Otros
                "Otro formato final"
            };
        }

        // =========================================================
        // EDITABLES / FUENTES
        // =========================================================
        // Editable = archivo fuente de trabajo.
        //
        // Comercialmente puede cobrarse aparte.
        // No todo proyecto incluye editables.

        public static List<string> CrearEditablesFuente()
        {
            return new List<string>
            {
                "No incluye editables",
                "After Effects project (.aep)",
                "Premiere project (.prproj)",
                "Photoshop document (.psd)",
                "Illustrator document (.ai)",
                "Toon Boom project",
                "TVPaint project",
                "Clip Studio file",
                "Animate / Flash project",
                "Spine project",
                "DragonBones project",
                "Archivos editables parciales",
                "Archivos editables completos",
                "Otro editable"
            };
        }

        // =========================================================
        // PAQUETES DE ENTREGA
        // =========================================================
        // Paquete = cómo se organiza la entrega.
        //
        // No es formato en sí mismo.
        // Ejemplo:
        // - Video final + versiones
        // - Paquete para implementación en juego
        // - Paquete completo con editables

        public static List<string> CrearPaquetesEntrega()
        {
            return new List<string>
            {
                "Video final solamente",
                "Video final + miniatura",
                "Video final + versiones por formato",
                "Video final + audio separado",
                "Video final + subtítulos",
                "Paquete de assets",
                "Paquete de sprites",
                "Paquete de fondos",
                "Paquete de personajes",
                "Paquete de props",
                "Paquete de capas separadas",
                "Paquete para edición posterior",
                "Paquete para implementación en juego",
                "Paquete completo final + editables",
                "Otro paquete"
            };
        }

        // =========================================================
        // RELACIÓN DE ASPECTO
        // =========================================================
        // Especificación técnica del entregable.

        public static List<string> CrearRelacionesAspecto()
        {
            return new List<string>
            {
                "16:9",
                "9:16",
                "1:1",
                "4:5",
                "5:4",
                "21:9",
                "2.39:1",
                "Personalizado"
            };
        }

        // =========================================================
        // RESOLUCIÓN
        // =========================================================
        // Especificación técnica del entregable.

        public static List<string> CrearResolucionesEntrega()
        {
            return new List<string>
            {
                "720p",
                "1080p",
                "1440p / 2K",
                "4K UHD",
                "DCI 2K",
                "DCI 4K",
                "Mobile / juego",
                "Asset variable",
                "Personalizada"
            };
        }

        // =========================================================
        // FPS
        // =========================================================
        // Especificación técnica.
        //
        // En animación 2D, FPS de archivo y cantidad de dibujos
        // por segundo no siempre son lo mismo. Esta lista define
        // FPS de entrega o reproducción, no necesariamente densidad
        // de animación.

        public static List<string> CrearFPS()
        {
            return new List<string>
            {
                "12 fps",
                "15 fps",
                "24 fps",
                "25 fps",
                "30 fps",
                "60 fps",
                "Variable",
                "Personalizado"
            };
        }

        // =========================================================
        // NIVEL DE ACABADO
        // =========================================================
        // Esto debería afectar:
        // - costo,
        // - plazo,
        // - cantidad de revisión,
        // - nivel de seniority,
        // - densidad visual.

        public static List<string> CrearNivelesAcabado()
        {
            return new List<string>
            {
                "Boceto / rough",
                "Base funcional",
                "Estándar",
                "Pulido",
                "Premium",
                "Broadcast / alta exigencia"
            };
        }

        // =========================================================
        // ESTILO VISUAL
        // =========================================================
        // Esto no es producto.
        // Es una condición estética/productiva.

        public static List<string> CrearEstilosVisuales()
        {
            return new List<string>
            {
                "Cartoon",
                "Anime",
                "Frame by frame",
                "Cut-out",
                "Motion graphics",
                "Vectorial",
                "Pixel art",
                "Painterly / ilustrado",
                "Cinemático",
                "Técnico / explicativo",
                "Minimalista",
                "Experimental",
                "Otro"
            };
        }

        // =========================================================
        // INSUMOS QUE PUEDE ENTREGAR EL CLIENTE
        // =========================================================
        // Esto responde:
        // ¿Qué trae el cliente?
        //
        // Si el insumo sirve, puede reducir trabajo interno.
        // Si no sirve, solo funciona como referencia.

        public static List<string> CrearInsumosCliente()
        {
            return new List<string>
            {
                "Brief creativo",
                "Guion",
                "Storyboard",
                "Animatic",
                "Diseño de personajes",
                "Diseño de fondos",
                "Props / objetos",
                "Paleta de color",
                "Guía de estilo",
                "Referencias visuales",
                "Voces / diálogos",
                "Música",
                "SFX",
                "Logo / marca",
                "Material grabado",
                "Assets editables",
                "Nada / requiere desarrollo completo"
            };
        }

        // =========================================================
        // SERVICIOS PRODUCTIVOS INTERNOS
        // =========================================================
        // Esto sirve para conectar con cargos, subetapas y costos.
        //
        // No necesariamente se muestra como producto vendible.
        // Render/export queda fuera como servicio productivo ofrecible;
        // si quieres contabilizarlo, va en tareas técnicas internas.

        public static List<string> CrearServiciosProductivos()
        {
            return new List<string>
            {
                "Dirección creativa",
                "Producción",
                "Guion",
                "Storyboard",
                "Animatic",
                "Dirección de arte",
                "Diseño de personajes",
                "Diseño de fondos",
                "Diseño de props",
                "Layout",
                "Rough animation",
                "Key animation",
                "Inbetween / asistencia de animación",
                "Clean-up",
                "Color",
                "Animación cut-out",
                "Animación frame by frame",
                "Animación de acting",
                "FX animation",
                "Compositing",
                "Edición",
                "Sound design",
                "Música",
                "Revisión / correcciones"
            };
        }

        // =========================================================
        // TAREAS TÉCNICAS INTERNAS
        // =========================================================
        // No son productos vendibles.
        // No deberían aparecer como "servicio ofrecido" al cliente.
        //
        // Sirven para costear tiempo interno de cierre, orden,
        // control de calidad y transferencia.

        public static List<string> CrearTareasTecnicasInternas()
        {
            return new List<string>
            {
                "Preparación de archivos",
                "Organización de capas",
                "Chequeo de versiones",
                "Exportación final",
                "Compresión final",
                "Control de calidad técnico",
                "Revisión de alpha / transparencia",
                "Revisión de audio",
                "Naming técnico",
                "Respaldo de entrega",
                "Subida / transferencia de archivos"
            };
        }

        // =========================================================
        // ESPECIFICACIONES TÉCNICAS
        // =========================================================
        // Esto sirve para condiciones del entregable.
        //
        // "Loopable" vive aquí:
        // no es producto ni unidad; es una propiedad técnica
        // de reproducción.

        public static List<string> CrearEspecificacionesTecnicas()
        {
            return new List<string>
            {
                "Resolución",
                "Relación de aspecto",
                "FPS",
                "Codec",
                "Alpha / transparencia",
                "Loopable",
                "Audio incluido",
                "Subtítulos",
                "Versiones por formato",
                "Entrega por capas",
                "Naming técnico",
                "Optimización para videojuego",
                "Optimización para web",
                "Peso máximo de archivo",
                "Compresión final",
                "Formato de exportación"
            };
        }
    }
}