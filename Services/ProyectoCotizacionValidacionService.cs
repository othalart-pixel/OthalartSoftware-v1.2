using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoCotizacionValidacionService
    {
        public sealed class DiagnosticoProyecto
        {
            public List<string> Errores { get; set; } = new List<string>();
            public List<string> Advertencias { get; set; } = new List<string>();
            public bool PuedeGuardar => Errores.Count == 0;
        }

        public static DiagnosticoProyecto Validar(ProyectoCotizacion proyecto)
        {
            DiagnosticoProyecto d = new DiagnosticoProyecto();
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (proyecto == null)
            {
                d.Errores.Add("Proyecto nulo.");
                return d;
            }

            ValidarId("proyecto", proyecto.Id, ids, d);

            if (proyecto.Grupos == null || proyecto.Grupos.Count == 0)
            {
                d.Advertencias.Add("Proyecto sin grupos.");
                return d;
            }

            foreach (GrupoProyecto grupo in proyecto.Grupos.Where(g => g != null))
            {
                ValidarGrupo(grupo, ids, d);
            }

            foreach (ProcesoTransversalProyecto transversal in proyecto.ProcesosTransversales ?? new List<ProcesoTransversalProyecto>())
            {
                ValidarTransversal(transversal, ids, d);
            }

            return d;
        }

        private static void ValidarGrupo(GrupoProyecto grupo, HashSet<string> ids, DiagnosticoProyecto d)
        {
            ValidarId("grupo", grupo.Id, ids, d);
            if (string.IsNullOrWhiteSpace(grupo.Nombre))
            {
                d.Advertencias.Add(grupo.Id + ": grupo sin nombre.");
            }

            foreach (ItemProyecto item in grupo.Items ?? new List<ItemProyecto>())
            {
                ValidarItem(grupo, item, ids, d);
            }
        }

        private static void ValidarItem(GrupoProyecto grupo, ItemProyecto item, HashSet<string> ids, DiagnosticoProyecto d)
        {
            if (item == null)
            {
                return;
            }

            ValidarId("item", item.Id, ids, d);
            if (item.Tipo == TipoItemProyecto.Producto && string.IsNullOrWhiteSpace(item.BibliotecaId))
            {
                d.Advertencias.Add(item.Id + ": producto sin biblioteca.");
            }

            if (item.Cantidad <= 0)
            {
                d.Errores.Add(item.Id + ": cantidad menor o igual a cero.");
            }

            if (item.Snapshot == null || string.IsNullOrWhiteSpace(item.Snapshot.NombreBiblioteca))
            {
                d.Advertencias.Add(item.Id + ": snapshot mínimo inexistente o incompleto.");
            }

            HashSet<string> subproductos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SubproductoProyecto sub in item.Subproductos ?? new List<SubproductoProyecto>())
            {
                ValidarSubproducto(item, sub, subproductos, ids, d);
            }

            foreach (ProcesoProyecto proceso in item.Procesos ?? new List<ProcesoProyecto>())
            {
                ValidarProceso(proceso, ids, d);
            }
        }

        private static void ValidarSubproducto(
            ItemProyecto item,
            SubproductoProyecto sub,
            HashSet<string> subproductos,
            HashSet<string> ids,
            DiagnosticoProyecto d
        )
        {
            if (sub == null)
            {
                return;
            }

            ValidarId("subproducto", sub.Id, ids, d);
            if (!string.Equals(sub.ProductoProyectoId, item.Id, StringComparison.OrdinalIgnoreCase))
            {
                d.Errores.Add(sub.Id + ": subproducto no referencia a su producto.");
            }

            string claveSub = (sub.SubproductoBibliotecaId + "|" + sub.Nombre).Trim();
            if (!subproductos.Add(claveSub))
            {
                d.Advertencias.Add(sub.Id + ": subproducto duplicado dentro del producto.");
            }

            if (sub.Cantidad <= 0)
            {
                d.Errores.Add(sub.Id + ": cantidad menor o igual a cero.");
            }

            if (string.IsNullOrWhiteSpace(sub.Unidad))
            {
                d.Errores.Add(sub.Id + ": unidad faltante.");
            }

            if (sub.ModoCantidad == ModoCantidadSubproducto.InstanciasIndividuales)
            {
                if (sub.Instancias == null || sub.Instancias.Count == 0)
                {
                    d.Errores.Add(sub.Id + ": modo individual sin instancias.");
                }
                else
                {
                    decimal total = sub.Instancias.Sum(i => i == null ? 0m : i.CantidadEquivalente);
                    if (Math.Abs(total - sub.Cantidad) > 0.0001m)
                    {
                        d.Advertencias.Add(sub.Id + ": cantidad incoherente con instancias.");
                    }
                }
            }

            HashSet<string> instancias = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (InstanciaSubproducto instancia in sub.Instancias ?? new List<InstanciaSubproducto>())
            {
                ValidarInstancia(sub, instancia, instancias, ids, d);
            }

            foreach (ProcesoProyecto proceso in sub.Procesos ?? new List<ProcesoProyecto>())
            {
                ValidarProceso(proceso, ids, d);
            }
        }

        private static void ValidarInstancia(
            SubproductoProyecto sub,
            InstanciaSubproducto instancia,
            HashSet<string> instancias,
            HashSet<string> ids,
            DiagnosticoProyecto d
        )
        {
            if (instancia == null)
            {
                return;
            }

            ValidarId("instancia", instancia.Id, ids, d);
            if (!string.Equals(instancia.SubproductoProyectoId, sub.Id, StringComparison.OrdinalIgnoreCase))
            {
                d.Errores.Add(instancia.Id + ": instancia no referencia a su subproducto.");
            }

            if (!instancias.Add(instancia.Nombre ?? ""))
            {
                d.Advertencias.Add(instancia.Id + ": instancia duplicada por nombre.");
            }

            foreach (ProcesoProyecto proceso in instancia.Procesos ?? new List<ProcesoProyecto>())
            {
                ValidarProceso(proceso, ids, d);
            }

            foreach (AsignacionProductiva asignacion in instancia.Asignaciones ?? new List<AsignacionProductiva>())
            {
                ValidarAsignacion(asignacion, ids, d);
            }
        }

        private static void ValidarProceso(ProcesoProyecto proceso, HashSet<string> ids, DiagnosticoProyecto d)
        {
            if (proceso == null)
            {
                return;
            }

            ValidarId("proceso", proceso.Id, ids, d);
            if (proceso.TipoProceso == TipoProcesoProductivo.NoClasificado)
            {
                d.Errores.Add(proceso.Id + ": proceso sin tipo.");
            }

            if (string.IsNullOrWhiteSpace(proceso.ProcesoBibliotecaId))
            {
                d.Advertencias.Add(proceso.Id + ": proceso sin referencia de biblioteca/fórmula.");
            }

            if ((proceso.TipoProceso == TipoProcesoProductivo.RevisionControl ||
                 proceso.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo) &&
                (proceso.Dependencias == null || proceso.Dependencias.Count == 0))
            {
                d.Advertencias.Add(proceso.Id + ": revisión/corrección sin proceso origen.");
            }

            foreach (AsignacionProductiva asignacion in proceso.Asignaciones ?? new List<AsignacionProductiva>())
            {
                ValidarAsignacion(asignacion, ids, d);
            }
        }

        private static void ValidarAsignacion(AsignacionProductiva asignacion, HashSet<string> ids, DiagnosticoProyecto d)
        {
            if (asignacion == null)
            {
                return;
            }

            ValidarId("asignación", asignacion.Id, ids, d);
            if (string.IsNullOrWhiteSpace(asignacion.CargoId))
            {
                d.Advertencias.Add(asignacion.Id + ": asignación sin cargo.");
            }

            if (asignacion.HorasCalculadas < 0 || asignacion.HorasAsignadas < 0)
            {
                d.Errores.Add(asignacion.Id + ": horas negativas.");
            }

            if (asignacion.DedicacionPorcentaje < 0 || asignacion.DedicacionPorcentaje > 100)
            {
                d.Errores.Add(asignacion.Id + ": dedicación fuera de rango 0-100.");
            }
        }

        private static void ValidarTransversal(ProcesoTransversalProyecto transversal, HashSet<string> ids, DiagnosticoProyecto d)
        {
            if (transversal == null)
            {
                return;
            }

            ValidarId("proceso transversal", transversal.Id, ids, d);
            if (transversal.EtapasCubiertas == null || transversal.EtapasCubiertas.Count == 0)
            {
                d.Advertencias.Add(transversal.Id + ": proceso transversal sin etapas cubiertas.");
            }

            if (transversal.HorasCalculadas < 0 || transversal.HorasAsignadas < 0)
            {
                d.Errores.Add(transversal.Id + ": horas negativas.");
            }
        }

        private static void ValidarId(string tipo, string id, HashSet<string> ids, DiagnosticoProyecto d)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                d.Errores.Add(tipo + " sin ID.");
                return;
            }

            if (!ids.Add(id))
            {
                d.Errores.Add("ID duplicado: " + id + ".");
            }
        }
    }
}
