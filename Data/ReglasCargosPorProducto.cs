using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Data
{
    public static class ReglasCargosPorProducto
    {
        public static List<string> ObtenerNombresCargosSugeridos(string productoVendible)
        {
            string p = Normalizar(productoVendible);

            List<string> cargos = new List<string>();

            if (EsProyectoCompleto(p))
            {
                Agregar(cargos, "Productor / Project manager");
                Agregar(cargos, "Director creativo");
                Agregar(cargos, "Director de arte");
            }

            // =====================================================
            // AUDIOVISUAL 2D COMPLETO
            // =====================================================

            if (p.Contains("comercial") ||
                p.Contains("redes") ||
                p.Contains("reel") ||
                p.Contains("short") ||
                p.Contains("cinematica") ||
                p.Contains("cutscene") ||
                p.Contains("trailer") ||
                p.Contains("teaser") ||
                p.Contains("videoclip") ||
                p.Contains("cortometraje") ||
                p.Contains("piloto") ||
                p.Contains("capitulo") ||
                p.Contains("videoexplicativo") ||
                p.Contains("videoeducativo") ||
                p.Contains("corporativo") ||
                p.Contains("documental"))
            {
                Agregar(cargos, "Guionista");
                Agregar(cargos, "Storyboard artist");
                Agregar(cargos, "Animatic editor");
                Agregar(cargos, "Layout artist");
                Agregar(cargos, "Diseñador de personajes");
                Agregar(cargos, "Diseñador de fondos");
                Agregar(cargos, "Diseñador de props");
                Agregar(cargos, "Background painter");
                Agregar(cargos, "Rough animator");
                Agregar(cargos, "Key animator");
                Agregar(cargos, "Inbetween artist");
                Agregar(cargos, "Clean up artist");
                Agregar(cargos, "Colorista");
                Agregar(cargos, "Compositor");
                Agregar(cargos, "Editor");
            }

            // =====================================================
            // MOTION GRAPHICS
            // =====================================================

            if (p.Contains("motiongraphics"))
            {
                Agregar(cargos, "Diseñador gráfico");
                Agregar(cargos, "Motion graphics artist");
                Agregar(cargos, "Compositor");
                Agregar(cargos, "Editor");
                Agregar(cargos, "Sound designer");
            }

            // =====================================================
            // VIDEOJUEGOS / ASSETS / UI
            // =====================================================

            if (p.Contains("videojuego") ||
                p.Contains("spritesheet") ||
                p.Contains("sprite") ||
                p.Contains("ui") ||
                p.Contains("fx2d") ||
                p.Contains("asset") ||
                p.Contains("personaje2danimado") ||
                p.Contains("enemigo") ||
                p.Contains("npc"))
            {
                Agregar(cargos, "Productor / Project manager");
                Agregar(cargos, "Director de arte");
                Agregar(cargos, "Game artist 2D");
                Agregar(cargos, "Concept artist");
                Agregar(cargos, "Diseñador de personajes");
                Agregar(cargos, "Diseñador de props");
                Agregar(cargos, "Pixel artist");
                Agregar(cargos, "Sprite artist");
                Agregar(cargos, "Animador de sprites");
                Agregar(cargos, "UI artist 2D");
                Agregar(cargos, "FX animator 2D");
                Agregar(cargos, "Technical artist 2D");
            }

            // =====================================================
            // ILUSTRACIÓN / DISEÑO VISUAL / CARTAS / PACKS
            // =====================================================
            // No usamos "Ilustrador de cartas".
            // Si el producto es cartas, piezas impresas o assets estáticos,
            // usamos cargos genéricos: ilustrador, director de arte,
            // diseñador gráfico, layout gráfico, etc.

            if (p.Contains("pack") ||
                p.Contains("cartas") ||
                p.Contains("card") ||
                p.Contains("ilustracion") ||
                p.Contains("disenovisual") ||
                p.Contains("concept") ||
                p.Contains("personajes") ||
                p.Contains("fondos") ||
                p.Contains("props") ||
                p.Contains("background") ||
                p.Contains("escenarios"))
            {
                Agregar(cargos, "Director de arte");
                Agregar(cargos, "Concept artist");
                Agregar(cargos, "Ilustrador");
                Agregar(cargos, "Diseñador gráfico");
                Agregar(cargos, "Diseñador editorial / layout gráfico");
                Agregar(cargos, "Diseñador de personajes");
                Agregar(cargos, "Diseñador de fondos");
                Agregar(cargos, "Diseñador de props");
            }

            // =====================================================
            // SERVICIOS PARCIALES
            // =====================================================

            if (p.Contains("guion"))
            {
                Agregar(cargos, "Guionista");
            }

            if (p.Contains("storyboard"))
            {
                Agregar(cargos, "Storyboard artist");
            }

            if (p.Contains("animatic"))
            {
                Agregar(cargos, "Animatic editor");
            }

            if (p.Contains("layout"))
            {
                Agregar(cargos, "Layout artist");
            }

            if (p.Contains("rough"))
            {
                Agregar(cargos, "Rough animator");
            }

            if (p.Contains("keyanimation"))
            {
                Agregar(cargos, "Key animator");
            }

            if (p.Contains("inbetween"))
            {
                Agregar(cargos, "Inbetween artist");
            }

            if (p.Contains("cleanup"))
            {
                Agregar(cargos, "Clean up artist");
            }

            if (p.Contains("color"))
            {
                Agregar(cargos, "Colorista");
            }

            if (p.Contains("composicion") || p.Contains("compositing"))
            {
                Agregar(cargos, "Compositor");
            }

            if (p.Contains("edicion"))
            {
                Agregar(cargos, "Editor");
            }

            if (p.Contains("postproduccion"))
            {
                Agregar(cargos, "Compositor");
                Agregar(cargos, "Editor");
                Agregar(cargos, "Sound designer");
                Agregar(cargos, "Corrección de color");
            }

            if (p.Contains("pitch"))
            {
                Agregar(cargos, "Director creativo");
                Agregar(cargos, "Director de arte");
                Agregar(cargos, "Guionista");
                Agregar(cargos, "Concept artist");
                Agregar(cargos, "Ilustrador");
                Agregar(cargos, "Diseñador gráfico");
            }

            if (cargos.Count == 0)
            {
                Agregar(cargos, "Productor / Project manager");
                Agregar(cargos, "Director creativo");
                Agregar(cargos, "Director de arte");
                Agregar(cargos, "Otro cargo general");
            }

            return cargos
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public static List<CategoriaTrabajador> ObtenerCargosSugeridos(
            string productoVendible,
            string nivelPreferido
        )
        {
            List<string> nombres = ObtenerNombresCargosSugeridos(productoVendible);
            List<CategoriaTrabajador> biblioteca = Cargos.CrearBibliotecaCompleta();

            string nivel = Normalizar(nivelPreferido);

            List<CategoriaTrabajador> resultado = new List<CategoriaTrabajador>();

            foreach (string nombre in nombres)
            {
                string nombreNormalizado = Normalizar(nombre);

                List<CategoriaTrabajador> candidatos = biblioteca
                    .Where(c =>
                        c != null &&
                        Normalizar(c.Nombre) == nombreNormalizado)
                    .ToList();

                if (candidatos.Count == 0)
                {
                    continue;
                }

                CategoriaTrabajador seleccionado = null;

                if (!string.IsNullOrWhiteSpace(nivel))
                {
                    seleccionado = candidatos.FirstOrDefault(c =>
                        Normalizar(c.Nivel).Contains(nivel)
                    );
                }

                if (seleccionado == null)
                {
                    seleccionado = candidatos.FirstOrDefault(c =>
                        Normalizar(c.Nivel).Contains("tipico")
                    );
                }

                if (seleccionado == null)
                {
                    seleccionado = candidatos.First();
                }

                resultado.Add(seleccionado);
            }

            return resultado;
        }

        private static bool EsProyectoCompleto(string p)
        {
            return p.Contains("comercial") ||
                   p.Contains("redes") ||
                   p.Contains("reel") ||
                   p.Contains("short") ||
                   p.Contains("cinematica") ||
                   p.Contains("cutscene") ||
                   p.Contains("trailer") ||
                   p.Contains("teaser") ||
                   p.Contains("videoclip") ||
                   p.Contains("cortometraje") ||
                   p.Contains("piloto") ||
                   p.Contains("capitulo") ||
                   p.Contains("video") ||
                   p.Contains("motiongraphics");
        }

        private static void Agregar(List<string> lista, string cargo)
        {
            if (lista == null || string.IsNullOrWhiteSpace(cargo))
            {
                return;
            }

            if (!lista.Contains(cargo))
            {
                lista.Add(cargo);
            }
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
                .Replace("/", "")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");
        }
    }
}