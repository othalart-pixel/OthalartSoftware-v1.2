using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ActivadorSubEtapasDesdeBriefService
    {
        public static void ActivarSubEtapasInternas(
            BriefProductoProyecto brief,
            List<SubEtapaProyecto> subEtapas
        )
        {
            if (brief == null || subEtapas == null)
            {
                return;
            }

            if (brief.SubproductosCalculados == null)
            {
                return;
            }

            foreach (SubproductoBrief subproducto in brief.SubproductosCalculados)
            {
                if (subproducto.Resolucion != "Interno")
                {
                    continue;
                }

                string etapa = Normalizar(subproducto.EtapaSugerida);
                string subEtapa = Normalizar(subproducto.SubEtapaSugerida);

                SubEtapaProyecto encontrada = subEtapas.FirstOrDefault(s =>
                    Normalizar(s.EtapaPadre) == etapa &&
                    Normalizar(s.Nombre) == subEtapa
                );

                if (encontrada == null)
                {
                    continue;
                }

                encontrada.Activa = true;
                encontrada.Requerida = subproducto.Requerido;
            }
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