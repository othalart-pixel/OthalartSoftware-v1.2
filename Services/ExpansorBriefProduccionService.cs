using System.Collections.Generic;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ExpansorBriefProduccionService
    {
        public static void ExpandirBrief(BriefProductoProyecto brief)
        {
            if (brief == null)
            {
                return;
            }

            if (brief.EntregablesSeleccionados == null)
            {
                return;
            }

            if (brief.SubproductosCalculados == null)
            {
                brief.SubproductosCalculados = new List<SubproductoBrief>();
            }

            brief.SubproductosCalculados.Clear();

            foreach (EntregableBrief producto in brief.EntregablesSeleccionados)
            {
                if (producto == null || string.IsNullOrWhiteSpace(producto.Nombre))
                {
                    continue;
                }

                List<Subproducto2D> subproductos =
                    BibliotecaProductos2D.ObtenerSubproductos(producto.Nombre);

                foreach (Subproducto2D subproducto in subproductos)
                {
                    brief.SubproductosCalculados.Add(new SubproductoBrief
                    {
                        ProductoOrigen = producto.Nombre,
                        Nombre = subproducto.Nombre,
                        EtapaSugerida = subproducto.EtapaSugerida,
                        SubEtapaSugerida = subproducto.SubEtapaSugerida,
                        EcuacionProductiva = subproducto.EcuacionProductiva,
                        VariablesEcuacion = subproducto.VariablesEcuacion,
                        ImpactoEcuacion = subproducto.ImpactoEcuacion,
                        Requerido = subproducto.RequeridoPorDefecto,
                        PuedeEntregarCliente = subproducto.PuedeEntregarCliente,
                        Resolucion = ResolverSubproducto(brief, subproducto),
                        Nota = subproducto.Nota
                    });
                }
            }
        }

        private static string ResolverSubproducto(
            BriefProductoProyecto brief,
            Subproducto2D subproducto
        )
        {
            if (subproducto == null)
            {
                return "NoAplica";
            }

            if (!subproducto.RequeridoPorDefecto)
            {
                return "NoAplica";
            }

            if (ClienteEntregaSubproducto(brief, subproducto))
            {
                return "Cliente";
            }

            return "Interno";
        }

        private static bool ClienteEntregaSubproducto(
            BriefProductoProyecto brief,
            Subproducto2D subproducto
        )
        {
            string n = Normalizar(subproducto.Nombre);

            if (n.Contains("guion") && brief.ClienteEntregaGuion) return true;
            if ((n.Contains("referencia") || n.Contains("estilo") || n.Contains("direccionvisual")) && brief.ClienteEntregaEstilo) return true;
            if (n.Contains("storyboard") && brief.ClienteEntregaStoryboard) return true;
            if (n.Contains("animatic") && brief.ClienteEntregaAnimatic) return true;
            if (n.Contains("personaje") && brief.ClienteEntregaPersonajes) return true;
            if ((n.Contains("fondo") || n.Contains("background") || n.Contains("escenario")) && brief.ClienteEntregaFondos) return true;
            if ((n.Contains("prop") || n.Contains("objeto") || n.Contains("asset")) && brief.ClienteEntregaProps) return true;
            if (n.Contains("animacion") && brief.ClienteEntregaAnimacion) return true;
            if ((n.Contains("audio") || n.Contains("musica") || n.Contains("locucion")) && brief.ClienteEntregaAudio) return true;
            if ((n.Contains("editable") || n.Contains("archivo")) && brief.ClienteEntregaAssetsEditables) return true;
            if (n.Contains("material") && brief.ClienteEntregaMaterialGrabado) return true;

            return false;
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
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }
    }
}
