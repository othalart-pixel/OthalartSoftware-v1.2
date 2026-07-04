using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaPerfilesTipoProducto
    {
        public static List<PerfilTipoProducto> CrearBase()
        {
            return new List<PerfilTipoProducto>
            {
                new PerfilTipoProducto
                {
                    TipoProducto = "Comercial animado",
                    UnidadDuracionSugerida = "segundos",
                    UsoPrincipalSugerido = "Publicidad digital",
                    FormatoEntregaSugerido = "MP4",
                    RequierePersonajes = true,
                    RequiereFondos = true,
                    RequiereAnimacionPersonajes = true,
                    RequiereMotionGraphics = true,
                    RequiereEdicion = true,
                    RequiereAudio = true,
                    RequiereExportFinal = true,
                    Nota = "Producto audiovisual corto orientado a mensaje, marca y conversión."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Reel / pieza para redes",
                    UnidadDuracionSugerida = "segundos",
                    UsoPrincipalSugerido = "Redes sociales",
                    FormatoEntregaSugerido = "MP4",
                    RequiereMotionGraphics = true,
                    RequiereEdicion = true,
                    RequiereAudio = true,
                    RequiereExportFinal = true,
                    Nota = "Pieza breve, rápida y optimizada para publicación digital."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Capítulo de serie",
                    UnidadDuracionSugerida = "minutos",
                    UsoPrincipalSugerido = "Streaming",
                    FormatoEntregaSugerido = "MP4",
                    RequierePersonajes = true,
                    RequiereFondos = true,
                    RequiereProps = true,
                    RequiereAnimacionPersonajes = true,
                    RequiereEdicion = true,
                    RequiereAudio = true,
                    RequiereExportFinal = true,
                    Nota = "Producto narrativo largo con pipeline completo."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Assets para videojuego",
                    UnidadDuracionSugerida = "assets",
                    UsoPrincipalSugerido = "Videojuego",
                    FormatoEntregaSugerido = "Paquete de assets",
                    RequierePersonajes = true,
                    RequiereFondos = true,
                    RequiereProps = true,
                    RequiereAnimacionPersonajes = false,
                    RequiereEdicion = false,
                    RequiereAudio = false,
                    RequiereExportFinal = true,
                    Nota = "Entrega orientada a piezas reutilizables, no necesariamente a video final."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Movimiento de personajes",
                    UnidadDuracionSugerida = "loops",
                    UsoPrincipalSugerido = "Videojuego",
                    FormatoEntregaSugerido = "Spritesheet",
                    RequierePersonajes = true,
                    RequiereAnimacionPersonajes = true,
                    RequiereEdicion = false,
                    RequiereAudio = false,
                    RequiereExportFinal = true,
                    Nota = "Servicio centrado en animar personajes ya definidos o parcialmente definidos."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Backgrounds / escenarios",
                    UnidadDuracionSugerida = "assets",
                    UsoPrincipalSugerido = "Videojuego",
                    FormatoEntregaSugerido = "Paquete de assets",
                    RequiereFondos = true,
                    RequiereEdicion = false,
                    RequiereAudio = false,
                    RequiereExportFinal = true,
                    Nota = "Servicio centrado en diseño o producción de fondos."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Animación técnica / documental",
                    UnidadDuracionSugerida = "minutos",
                    UsoPrincipalSugerido = "Documental",
                    FormatoEntregaSugerido = "MP4",
                    RequiereMotionGraphics = true,
                    RequiereEdicion = true,
                    RequiereAudio = true,
                    RequiereExportFinal = true,
                    Nota = "Suele requerir investigación, explicación visual y claridad técnica."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Solo postproducción",
                    UnidadDuracionSugerida = "minutos",
                    UsoPrincipalSugerido = "Web",
                    FormatoEntregaSugerido = "MP4",
                    RequiereEdicion = true,
                    RequiereAudio = true,
                    RequiereExportFinal = true,
                    Nota = "Asume que el material visual principal ya existe o lo entrega el cliente."
                },

                new PerfilTipoProducto
                {
                    TipoProducto = "Solo animación",
                    UnidadDuracionSugerida = "segundos",
                    UsoPrincipalSugerido = "Web",
                    FormatoEntregaSugerido = "MP4",
                    RequiereAnimacionPersonajes = true,
                    RequiereEdicion = false,
                    RequiereAudio = false,
                    RequiereExportFinal = true,
                    Nota = "Asume que diseño, estilo y/o storyboard pueden venir desde el cliente."
                }
            };
        }

        public static PerfilTipoProducto ObtenerPerfil(string tipoProducto)
        {
            string normalizado = Normalizar(tipoProducto);

            return CrearBase()
                .FirstOrDefault(p => Normalizar(p.TipoProducto) == normalizado);
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
                .Replace("ñ", "n");
        }
    }
}