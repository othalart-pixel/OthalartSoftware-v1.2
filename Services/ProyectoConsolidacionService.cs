using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoConsolidacionService
    {
        public static ProyectoConsolidado Consolidar(ProyectoCotizacion proyecto)
        {
            ProyectoProductivoExpandido expandido = ProyectoProductivoExpansionService.Expandir(proyecto);
            return Consolidar(proyecto, expandido);
        }

        public static ProyectoConsolidado Consolidar(
            ProyectoCotizacion proyecto,
            ProyectoProductivoExpandido expandido
        )
        {
            ProyectoConsolidado c = new ProyectoConsolidado
            {
                ProyectoId = proyecto == null ? "" : proyecto.Id
            };

            if (proyecto == null)
            {
                c.Diagnosticos.Add("No hay proyecto para consolidar.");
                return c;
            }

            List<FilaProductivaProyecto> filas = expandido?.Filas ?? new List<FilaProductivaProyecto>();
            c.TotalProductos = proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .Count(i => i != null && i.Activo && i.Tipo == TipoItemProyecto.Producto);
            c.TotalSubproductos = proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>())
                .Count(s => s != null && s.Activo);
            c.TotalInstancias = proyecto.Grupos.SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>())
                .SelectMany(s => s.Instancias ?? new List<InstanciaSubproducto>())
                .Count();

            foreach (FilaProductivaProyecto fila in filas)
            {
                decimal horas = ObtenerHoras(fila);
                switch (fila.TipoProceso)
                {
                    case TipoProcesoProductivo.RevisionControl:
                        c.HorasRevision += horas;
                        c.CostoDirecto += fila.Costo;
                        break;
                    case TipoProcesoProductivo.CorreccionRetrabajo:
                        c.HorasCorreccion += horas;
                        c.CostoDirecto += fila.Costo;
                        break;
                    case TipoProcesoProductivo.Supervision:
                        c.HorasSupervision += horas;
                        c.CostoTransversal += fila.Costo;
                        break;
                    case TipoProcesoProductivo.Direccion:
                        c.HorasDireccion += horas;
                        c.CostoTransversal += fila.Costo;
                        break;
                    case TipoProcesoProductivo.GestionCoordinacion:
                        c.HorasGestion += horas;
                        c.CostoTransversal += fila.Costo;
                        break;
                    default:
                        c.HorasProductivas += horas;
                        c.CostoDirecto += fila.Costo;
                        break;
                }
            }

            c.CostoTotal = c.CostoDirecto + c.CostoTransversal;
            c.Precio = c.CostoTotal;
            c.Margen = c.Precio <= 0m ? 0m : (c.Precio - c.CostoTotal) / c.Precio;
            c.DuracionSemanas = CalcularDuracionSemanas(proyecto, filas);

            c.PorGrupo = Agrupar(filas, f => f.GrupoId, id => ObtenerNombreGrupo(proyecto, id));
            c.PorProducto = Agrupar(filas.Where(f => !string.IsNullOrWhiteSpace(f.ProductoProyectoId)), f => f.ProductoProyectoId, id => ObtenerNombreItem(proyecto, id));
            c.PorSubproducto = Agrupar(filas.Where(f => !string.IsNullOrWhiteSpace(f.SubproductoProyectoId)), f => f.SubproductoProyectoId, id => ObtenerNombreSubproducto(proyecto, id));
            c.PorCargo = Agrupar(filas.Where(f => !string.IsNullOrWhiteSpace(f.CargoId)), f => f.CargoId, id => id);
            c.PorPersona = Agrupar(filas.Where(f => !string.IsNullOrWhiteSpace(f.PersonaId)), f => f.PersonaId, id => id);

            if (expandido != null)
            {
                c.Diagnosticos.AddRange(expandido.Diagnosticos);
            }

            return c;
        }

        private static decimal ObtenerHoras(FilaProductivaProyecto fila)
        {
            if (fila == null)
            {
                return 0m;
            }

            return fila.HorasAsignadas > 0m ? fila.HorasAsignadas : fila.HorasCalculadas;
        }

        private static decimal CalcularDuracionSemanas(
            ProyectoCotizacion proyecto,
            List<FilaProductivaProyecto> filas
        )
        {
            decimal maxTransversal = filas
                .Where(f => f.Transversal && f.Unidad == "semanas")
                .Select(f => f.Cantidad)
                .DefaultIfEmpty(0m)
                .Max();

            decimal rutaCritica = CalcularRutaCriticaSemanas(filas);

            return Math.Max(maxTransversal, rutaCritica);
        }

        private static decimal CalcularRutaCriticaSemanas(List<FilaProductivaProyecto> filas)
        {
            Dictionary<string, decimal> duraciones = filas
                .Where(f => !f.Transversal && !string.IsNullOrWhiteSpace(f.ProcesoProyectoId))
                .GroupBy(f => f.ProcesoProyectoId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(ObtenerHoras) / 40m,
                    StringComparer.OrdinalIgnoreCase
                );

            Dictionary<string, List<string>> dependencias = filas
                .Where(f => !f.Transversal && !string.IsNullOrWhiteSpace(f.ProcesoProyectoId))
                .GroupBy(f => f.ProcesoProyectoId)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(f => f.Dependencias ?? new List<string>())
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase
                );

            Dictionary<string, decimal> memo = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> visitando = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            decimal max = 0m;
            foreach (string procesoId in duraciones.Keys)
            {
                max = Math.Max(max, CalcularRutaProceso(procesoId, duraciones, dependencias, memo, visitando));
            }

            return max;
        }

        private static decimal CalcularRutaProceso(
            string procesoId,
            Dictionary<string, decimal> duraciones,
            Dictionary<string, List<string>> dependencias,
            Dictionary<string, decimal> memo,
            HashSet<string> visitando
        )
        {
            if (memo.TryGetValue(procesoId, out decimal valor))
            {
                return valor;
            }

            if (visitando.Contains(procesoId))
            {
                return duraciones.TryGetValue(procesoId, out decimal circular) ? circular : 0m;
            }

            visitando.Add(procesoId);
            decimal maxDependencias = 0m;
            if (dependencias.TryGetValue(procesoId, out List<string> deps))
            {
                foreach (string dependencia in deps.Where(d => duraciones.ContainsKey(d)))
                {
                    maxDependencias = Math.Max(
                        maxDependencias,
                        CalcularRutaProceso(dependencia, duraciones, dependencias, memo, visitando)
                    );
                }
            }

            visitando.Remove(procesoId);
            decimal total = maxDependencias + (duraciones.TryGetValue(procesoId, out decimal propia) ? propia : 0m);
            memo[procesoId] = total;
            return total;
        }

        private static List<ResumenConsolidado> Agrupar(
            IEnumerable<FilaProductivaProyecto> filas,
            Func<FilaProductivaProyecto, string> clave,
            Func<string, string> nombre
        )
        {
            return (filas ?? Enumerable.Empty<FilaProductivaProyecto>())
                .GroupBy(clave)
                .Select(g => new ResumenConsolidado
                {
                    Id = g.Key,
                    Nombre = nombre(g.Key),
                    Cantidad = g.Sum(f => f.Cantidad),
                    Horas = g.Sum(ObtenerHoras),
                    Costo = g.Sum(f => f.Costo),
                    Precio = g.Sum(f => f.Costo),
                    Margen = 0m
                })
                .OrderBy(r => r.Nombre)
                .ToList();
        }

        private static string ObtenerNombreGrupo(ProyectoCotizacion proyecto, string id)
        {
            return proyecto.Grupos.FirstOrDefault(g => g.Id == id)?.Nombre ?? id;
        }

        private static string ObtenerNombreItem(ProyectoCotizacion proyecto, string id)
        {
            return proyecto.Grupos
                .SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .FirstOrDefault(i => i.Id == id)?.Nombre ?? id;
        }

        private static string ObtenerNombreSubproducto(ProyectoCotizacion proyecto, string id)
        {
            return proyecto.Grupos
                .SelectMany(g => g.Items ?? new List<ItemProyecto>())
                .SelectMany(i => i.Subproductos ?? new List<SubproductoProyecto>())
                .FirstOrDefault(s => s.Id == id)?.Nombre ?? id;
        }
    }
}
