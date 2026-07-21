using System;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoOverridesService
    {
        public static OverrideProductivo RegistrarOverrideProducto(
            ItemProyecto item,
            string campo,
            object valor,
            AlcanceModificacion alcance,
            string motivo = "",
            object valorPlantilla = null
        )
        {
            if (item == null || string.IsNullOrWhiteSpace(campo))
            {
                return null;
            }

            item.Overrides ??= new System.Collections.Generic.List<OverrideProductivo>();
            OverrideProductivo ov = CrearOverride(
                campo,
                valor,
                alcance,
                EstadoDesdeAlcance(alcance),
                "producto:" + item.Id,
                item.BibliotecaId,
                motivo,
                valorPlantilla
            );

            ReemplazarOverride(item.Overrides, ov);
            return ov;
        }

        public static OverrideProductivo RegistrarOverrideSubproducto(
            SubproductoProyecto subproducto,
            string campo,
            object valor,
            AlcanceModificacion alcance,
            string motivo = "",
            object valorPlantilla = null
        )
        {
            if (subproducto == null || string.IsNullOrWhiteSpace(campo))
            {
                return null;
            }

            subproducto.Overrides ??= new System.Collections.Generic.List<OverrideProductivo>();
            OverrideProductivo ov = CrearOverride(
                campo,
                valor,
                alcance,
                EstadoDesdeAlcance(alcance),
                "subproducto:" + subproducto.Id,
                subproducto.SubproductoBibliotecaId,
                motivo,
                valorPlantilla
            );

            ReemplazarOverride(subproducto.Overrides, ov);
            return ov;
        }

        public static OverrideProductivo RegistrarOverrideInstancia(
            InstanciaSubproducto instancia,
            string campo,
            object valor,
            AlcanceModificacion alcance,
            string motivo = "",
            object valorPlantilla = null
        )
        {
            if (instancia == null || string.IsNullOrWhiteSpace(campo))
            {
                return null;
            }

            instancia.Overrides ??= new System.Collections.Generic.List<OverrideProductivo>();
            OverrideProductivo ov = CrearOverride(
                campo,
                valor,
                alcance,
                EstadoDesdeAlcance(alcance),
                "instancia:" + instancia.Id,
                instancia.SubproductoProyectoId,
                motivo,
                valorPlantilla
            );

            ReemplazarOverride(instancia.Overrides, ov);
            return ov;
        }

        public static bool TieneOverrides(ItemProyecto item)
        {
            return item != null && item.Overrides != null && item.Overrides.Any();
        }

        public static bool TieneOverrides(SubproductoProyecto subproducto)
        {
            return subproducto != null && subproducto.Overrides != null && subproducto.Overrides.Any();
        }

        private static OverrideProductivo CrearOverride(
            string campo,
            object valor,
            AlcanceModificacion alcance,
            EstadoPersonalizacionProyecto estado,
            string ruta,
            string bibliotecaId,
            string motivo,
            object valorPlantilla
        )
        {
            return new OverrideProductivo
            {
                Id = "ovr_" + Guid.NewGuid().ToString("N"),
                Campo = campo,
                ValorJson = JsonSerializer.Serialize(valor),
                ValorPlantillaJson = valorPlantilla == null ? "" : JsonSerializer.Serialize(valorPlantilla),
                Alcance = alcance,
                Estado = estado,
                RutaElemento = ruta,
                BibliotecaId = bibliotecaId ?? "",
                Motivo = motivo ?? "",
                Fecha = DateTime.Now
            };
        }

        private static EstadoPersonalizacionProyecto EstadoDesdeAlcance(AlcanceModificacion alcance)
        {
            switch (alcance)
            {
                case AlcanceModificacion.Instancia:
                    return EstadoPersonalizacionProyecto.PersonalizadoInstancia;
                case AlcanceModificacion.PlantillaGlobal:
                    return EstadoPersonalizacionProyecto.PlantillaGlobalModificada;
                default:
                    return EstadoPersonalizacionProyecto.PersonalizadoProyecto;
            }
        }

        private static void ReemplazarOverride(
            System.Collections.Generic.List<OverrideProductivo> overrides,
            OverrideProductivo nuevo
        )
        {
            overrides.RemoveAll(o =>
                o != null &&
                string.Equals(o.Campo, nuevo.Campo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(o.RutaElemento, nuevo.RutaElemento, StringComparison.OrdinalIgnoreCase));
            overrides.Add(nuevo);
        }
    }
}
