using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class Cargos
    {
        // =========================================================
        // API PÚBLICA
        // =========================================================

        public static List<CategoriaTrabajador> CrearBibliotecaGenerales()
        {
            return FiltrarPorBloque("General");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaDesarrollo()
        {
            return FiltrarPorBloque("Desarrollo");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaPreproduccion()
        {
            return FiltrarPorBloque("Preproduccion");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaProduccion()
        {
            return FiltrarPorBloque("Produccion");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaPostproduccion()
        {
            return FiltrarPorBloque("Postproduccion");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaVideojuegosAssets()
        {
            return FiltrarPorBloque("VideojuegosAssets");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaTecnicaInterna()
        {
            return FiltrarPorBloque("TecnicaInterna");
        }

        public static List<CategoriaTrabajador> CrearBibliotecaCompleta()
        {
            return BibliotecaCargosJsonService.CargarCargos();
        }

        public static List<CategoriaTrabajador> CrearBibliotecaPorBloque(string bloque)
        {
            string b = Normalizar(bloque);

            if (b == "general" || b == "generales")
            {
                return CrearBibliotecaGenerales();
            }

            if (b == "desarrollo")
            {
                return CrearBibliotecaDesarrollo();
            }

            if (b == "preproduccion" || b == "preproducción")
            {
                return CrearBibliotecaPreproduccion();
            }

            if (b == "produccion" || b == "producción")
            {
                return CrearBibliotecaProduccion();
            }

            if (b == "postproduccion" || b == "postproducción")
            {
                return CrearBibliotecaPostproduccion();
            }

            if (b == "videojuegosassets" || b == "videojuegos" || b == "assets")
            {
                return CrearBibliotecaVideojuegosAssets();
            }

            if (b == "tecnicainterna" || b == "técnicainterna" || b == "tecnica interna" || b == "técnica interna")
            {
                return CrearBibliotecaTecnicaInterna();
            }

            return CrearBibliotecaCompleta();
        }

        public static void GuardarBibliotecaCompleta(List<CategoriaTrabajador> cargos)
        {
            BibliotecaCargosJsonService.GuardarCargos(cargos);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private static List<CategoriaTrabajador> FiltrarPorBloque(string bloque)
        {
            string b = Normalizar(bloque);

            return CrearBibliotecaCompleta()
                .Where(c => Normalizar(ObtenerBloqueCargo(c)) == b)
                .ToList();
        }

        private static string ObtenerBloqueCargo(CategoriaTrabajador cargo)
        {
            if (cargo == null)
            {
                return "";
            }

            /*
             * Ajusta si tu modelo usa otra propiedad.
             */
            return cargo.Bloque ?? "";
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
                .Replace("ñ", "n")
                .Replace(" ", "");
        }
    }
}