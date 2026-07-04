using System.Collections.Generic;

namespace Cotizador_animacion_Othalart.Data
{
    public static class BibliotecaIndustrias
    {
        public static List<string> ObtenerIndustrias()
        {
            return new List<string>
            {
                "Videojuegos",
                "Publicidad / marketing",
                "Redes sociales",
                "Educación / e-learning",
                "Corporativo",
                "Música / videoclip",
                "Cine / serie / entretenimiento",
                "Documental / técnico",
                "Streaming / televisión",
                "Editorial / cómic / narrativa visual",
                "Otro"
            };
        }

        public static List<string> ObtenerDestinosPorIndustria(string industria)
        {
            string i = Normalizar(industria);

            if (i.Contains("videojuego"))
            {
                return new List<string>
                {
                    "Celular / mobile",
                    "PC",
                    "Consola",
                    "Web",
                    "Steam",
                    "Trailer de juego",
                    "Cinemática interna",
                    "Cutscene",
                    "UI animada",
                    "Assets in-game"
                };
            }

            if (i.Contains("publicidad") || i.Contains("marketing") || i.Contains("redes"))
            {
                return new List<string>
                {
                    "Instagram",
                    "TikTok",
                    "YouTube",
                    "Meta Ads",
                    "LinkedIn",
                    "Pantalla exterior",
                    "Sitio web",
                    "Presentación comercial"
                };
            }

            return new List<string>
            {
                "Web",
                "Redes sociales",
                "Pantalla",
                "Presentación",
                "Otro"
            };
        }

        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto.Trim().ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");
        }
    }
}