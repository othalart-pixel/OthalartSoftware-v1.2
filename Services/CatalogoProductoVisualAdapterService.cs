using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class CatalogoProductoVisualAdapterService
    {
        private static readonly string[] OrdenEtapas =
        {
            "Desarrollo",
            "Preproduccion",
            "Produccion",
            "Postproduccion"
        };

        public static CatalogoProductoVisual Crear(Producto2DDefinicion producto)
        {
            CatalogoProductoVisual visual = new CatalogoProductoVisual
            {
                Producto = producto
            };

            if (producto == null)
            {
                return visual;
            }

            List<CatalogoEtapaVisual> etapas = CrearEtapasBase(producto);
            List<Subproducto2D> subproductos = (producto.Subproductos ?? new List<Subproducto2D>())
                .Where(s => s != null)
                .OrderBy(s => s.Orden <= 0 ? int.MaxValue : s.Orden)
                .ThenBy(s => s.Nombre)
                .ToList();

            foreach (Subproducto2D subproducto in subproductos)
            {
                CatalogoEtapaVisual etapa = ResolverEtapa(etapas, subproducto);
                etapa.Procesos.Add(CrearProceso(subproducto));
            }

            visual.Etapas = etapas
                .OrderBy(e => e.Orden <= 0 ? int.MaxValue : e.Orden)
                .ThenBy(e => e.Nombre)
                .ToList();

            return visual;
        }

        private static List<CatalogoEtapaVisual> CrearEtapasBase(Producto2DDefinicion producto)
        {
            List<CatalogoEtapaVisual> etapas = new List<CatalogoEtapaVisual>();

            foreach (ProductoEtapaDefinicion etapa in producto.Etapas ?? new List<ProductoEtapaDefinicion>())
            {
                string nombre = string.IsNullOrWhiteSpace(etapa.NombreVisible)
                    ? etapa.ClaveEtapa
                    : etapa.NombreVisible;

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    continue;
                }

                AgregarEtapaSiFalta(
                    etapas,
                    etapa.ClaveEtapa,
                    NormalizarNombreVisible(nombre),
                    etapa.Orden,
                    etapa.Activa
                );
            }

            for (int i = 0; i < OrdenEtapas.Length; i++)
            {
                AgregarEtapaSiFalta(etapas, OrdenEtapas[i], OrdenEtapas[i], (i + 1) * 10, true);
            }

            return etapas;
        }

        private static void AgregarEtapaSiFalta(
            List<CatalogoEtapaVisual> etapas,
            string clave,
            string nombre,
            int orden,
            bool activa
        )
        {
            string normalizada = Normalizar(nombre);
            if (etapas.Any(e => Normalizar(e.Nombre) == normalizada || Normalizar(e.Clave) == normalizada))
            {
                return;
            }

            etapas.Add(new CatalogoEtapaVisual
            {
                Clave = string.IsNullOrWhiteSpace(clave) ? nombre : clave,
                Nombre = nombre,
                Orden = orden,
                Activa = activa
            });
        }

        private static CatalogoEtapaVisual ResolverEtapa(
            List<CatalogoEtapaVisual> etapas,
            Subproducto2D subproducto
        )
        {
            string nombreEtapa = !string.IsNullOrWhiteSpace(subproducto.EtapaSugerida)
                ? subproducto.EtapaSugerida
                : subproducto.Categoria;

            if (string.IsNullOrWhiteSpace(nombreEtapa))
            {
                nombreEtapa = "Produccion";
            }

            string normalizada = Normalizar(nombreEtapa);
            CatalogoEtapaVisual etapa = etapas.FirstOrDefault(e =>
                Normalizar(e.Clave) == normalizada ||
                Normalizar(e.Nombre) == normalizada
            );

            if (etapa != null)
            {
                return etapa;
            }

            etapa = new CatalogoEtapaVisual
            {
                Clave = nombreEtapa,
                Nombre = NormalizarNombreVisible(nombreEtapa),
                Orden = 500,
                Activa = true
            };
            etapas.Add(etapa);
            return etapa;
        }

        private static CatalogoProcesoVisual CrearProceso(Subproducto2D subproducto)
        {
            List<string> diagnosticos = new List<string>();

            if (string.IsNullOrWhiteSpace(subproducto.CargosSugeridos))
            {
                diagnosticos.Add("Sin cargos asignados");
            }

            if (string.IsNullOrWhiteSpace(subproducto.EcuacionProductiva))
            {
                diagnosticos.Add("Sin ecuacion vinculada");
            }

            if (string.IsNullOrWhiteSpace(subproducto.SubEtapaSugerida))
            {
                diagnosticos.Add("Sin proceso definido");
            }

            return new CatalogoProcesoVisual
            {
                Subproducto = subproducto,
                Nombre = subproducto.Nombre ?? "",
                Proceso = subproducto.SubEtapaSugerida ?? "",
                Cargos = subproducto.CargosSugeridos ?? "",
                Ecuacion = subproducto.EcuacionProductiva ?? "",
                Diagnostico = string.Join(" | ", diagnosticos),
                TieneDiagnostico = diagnosticos.Count > 0
            };
        }

        private static string NormalizarNombreVisible(string nombre)
        {
            string normalizado = Normalizar(nombre);
            if (normalizado == "preproduccion")
            {
                return "Preproduccion";
            }

            if (normalizado == "produccion")
            {
                return "Produccion";
            }

            if (normalizado == "postproduccion")
            {
                return "Postproduccion";
            }

            if (normalizado == "desarrollo")
            {
                return "Desarrollo";
            }

            return nombre;
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
                .Replace("ú", "u");
        }
    }
}
