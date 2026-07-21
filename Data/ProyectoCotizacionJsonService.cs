using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart.Data
{
    public static class ProyectoCotizacionJsonService
    {
        public const int SchemaVersionActual = 1;

        public static ProyectoCotizacion CrearProyectoVacio(string nombre, string cliente)
        {
            DateTime ahora = DateTime.Now;
            ProyectoCotizacion proyecto = new ProyectoCotizacion
            {
                SchemaVersion = SchemaVersionActual,
                Id = CrearId("prj", string.IsNullOrWhiteSpace(nombre) ? "proyecto" : nombre),
                Nombre = nombre ?? "",
                Cliente = cliente ?? "",
                FechaCreacion = ahora,
                FechaModificacion = ahora
            };

            proyecto.Grupos.Add(new GrupoProyecto
            {
                Id = "grp_direccion_gestion",
                Nombre = "Dirección, supervisión y gestión",
                Tipo = TipoGrupoProyecto.DireccionGestion,
                Orden = 0
            });
            proyecto.Grupos.Add(new GrupoProyecto
            {
                Id = "grp_preproduccion",
                Nombre = "Preproducción",
                Tipo = TipoGrupoProyecto.Preproduccion,
                Orden = 1
            });
            proyecto.Grupos.Add(new GrupoProyecto
            {
                Id = "grp_produccion",
                Nombre = "Producción",
                Tipo = TipoGrupoProyecto.Produccion,
                Orden = 2
            });
            proyecto.Grupos.Add(new GrupoProyecto
            {
                Id = "grp_postproduccion",
                Nombre = "Postproducción",
                Tipo = TipoGrupoProyecto.Postproduccion,
                Orden = 3
            });

            Normalizar(proyecto);
            return proyecto;
        }

        public static ProyectoCotizacion Cargar(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
            {
                ProyectoCotizacion nuevo = CrearProyectoVacio("Proyecto sin nombre", "");
                nuevo.Warnings.Add("No se encontró archivo de proyecto; se creó un proyecto vacío.");
                return nuevo;
            }

            string json = File.ReadAllText(ruta);
            ProyectoCotizacion proyecto = JsonSerializer.Deserialize<ProyectoCotizacion>(
                json,
                CrearOpcionesJson()
            ) ?? CrearProyectoVacio("Proyecto sin nombre", "");

            Migrar(proyecto);
            Normalizar(proyecto);
            return proyecto;
        }

        public static void Guardar(ProyectoCotizacion proyecto, string ruta)
        {
            if (proyecto == null)
            {
                throw new InvalidOperationException("No hay proyecto para guardar.");
            }

            if (string.IsNullOrWhiteSpace(ruta))
            {
                throw new InvalidOperationException("Ruta de guardado vacía.");
            }

            Migrar(proyecto);
            Normalizar(proyecto);

            var diagnostico = ProyectoCotizacionValidacionService.Validar(proyecto);
            if (diagnostico.Errores.Count > 0)
            {
                throw new InvalidOperationException(
                    "El proyecto tiene errores y no se guardó: " +
                    string.Join(" | ", diagnostico.Errores)
                );
            }

            string carpeta = Path.GetDirectoryName(ruta) ?? "";
            if (!string.IsNullOrWhiteSpace(carpeta) && !Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            if (File.Exists(ruta))
            {
                string backup = Path.Combine(
                    carpeta,
                    Path.GetFileNameWithoutExtension(ruta) +
                    ".backup_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                    Path.GetExtension(ruta)
                );
                File.Copy(ruta, backup, false);
            }

            proyecto.FechaModificacion = DateTime.Now;
            string temporal = ruta + ".tmp";
            File.WriteAllText(temporal, JsonSerializer.Serialize(proyecto, CrearOpcionesJson()));

            if (File.Exists(ruta))
            {
                File.Replace(temporal, ruta, null);
            }
            else
            {
                File.Move(temporal, ruta);
            }
        }

        public static void Migrar(ProyectoCotizacion proyecto)
        {
            if (proyecto == null)
            {
                return;
            }

            if (proyecto.SchemaVersion <= 0)
            {
                proyecto.SchemaVersion = 1;
                proyecto.Warnings.Add("Proyecto migrado desde esquema sin versión explícita.");
            }

            if (proyecto.Metadata == null)
            {
                proyecto.Metadata = new MetadataProyecto();
            }

            if (proyecto.Grupos == null)
            {
                proyecto.Grupos = new System.Collections.Generic.List<GrupoProyecto>();
            }

            if (proyecto.ProcesosTransversales == null)
            {
                proyecto.ProcesosTransversales = new System.Collections.Generic.List<ProcesoTransversalProyecto>();
            }

            proyecto.SchemaVersion = SchemaVersionActual;
        }

        public static void Normalizar(ProyectoCotizacion proyecto)
        {
            if (proyecto == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(proyecto.Id))
            {
                proyecto.Id = CrearId("prj", proyecto.Nombre);
            }

            if (string.IsNullOrWhiteSpace(proyecto.MonedaId))
            {
                proyecto.MonedaId = "CLP";
            }

            for (int i = 0; i < proyecto.Grupos.Count; i++)
            {
                GrupoProyecto grupo = proyecto.Grupos[i];
                if (grupo == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(grupo.Id))
                {
                    grupo.Id = CrearId("grp", grupo.Nombre + "_" + i);
                }

                grupo.Orden = grupo.Orden == 0 ? i : grupo.Orden;
                if (grupo.Items == null)
                {
                    grupo.Items = new System.Collections.Generic.List<ItemProyecto>();
                }

                NormalizarItems(grupo);
            }
        }

        private static void NormalizarItems(GrupoProyecto grupo)
        {
            for (int i = 0; i < grupo.Items.Count; i++)
            {
                ItemProyecto item = grupo.Items[i];
                if (item == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    item.Id = CrearId(item.Tipo == TipoItemProyecto.Servicio ? "srv" : "prod", item.Nombre + "_" + i);
                }

                item.Orden = item.Orden == 0 ? i : item.Orden;
                item.Snapshot ??= new SnapshotItem();
                item.Parametros ??= new System.Collections.Generic.Dictionary<string, object>();
                item.Subproductos ??= new System.Collections.Generic.List<SubproductoProyecto>();
                item.Procesos ??= new System.Collections.Generic.List<ProcesoProyecto>();

                for (int s = 0; s < item.Subproductos.Count; s++)
                {
                    NormalizarSubproducto(item, item.Subproductos[s], s);
                }
            }
        }

        private static void NormalizarSubproducto(ItemProyecto item, SubproductoProyecto sub, int indice)
        {
            if (sub == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sub.Id))
            {
                sub.Id = CrearId("sub", sub.Nombre + "_" + indice);
            }

            sub.ProductoProyectoId = string.IsNullOrWhiteSpace(sub.ProductoProyectoId)
                ? item.Id
                : sub.ProductoProyectoId;
            sub.Orden = sub.Orden == 0 ? indice : sub.Orden;
            sub.Parametros ??= new System.Collections.Generic.Dictionary<string, object>();
            sub.Instancias ??= new System.Collections.Generic.List<InstanciaSubproducto>();
            sub.Procesos ??= new System.Collections.Generic.List<ProcesoProyecto>();
            sub.Overrides ??= new System.Collections.Generic.List<OverrideProductivo>();

            for (int i = 0; i < sub.Instancias.Count; i++)
            {
                InstanciaSubproducto instancia = sub.Instancias[i];
                if (instancia == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(instancia.Id))
                {
                    instancia.Id = CrearId("ins", instancia.Nombre + "_" + i);
                }

                instancia.SubproductoProyectoId = string.IsNullOrWhiteSpace(instancia.SubproductoProyectoId)
                    ? sub.Id
                    : instancia.SubproductoProyectoId;
                instancia.Orden = instancia.Orden == 0 ? i : instancia.Orden;
                instancia.Parametros ??= new System.Collections.Generic.Dictionary<string, object>();
                instancia.Procesos ??= new System.Collections.Generic.List<ProcesoProyecto>();
                instancia.Asignaciones ??= new System.Collections.Generic.List<AsignacionProductiva>();
                instancia.Overrides ??= new System.Collections.Generic.List<OverrideProductivo>();
            }

            foreach (ProcesoProyecto proceso in sub.Procesos)
            {
                NormalizarProceso(proceso, sub.Id);
            }
        }

        private static void NormalizarProceso(ProcesoProyecto proceso, string ownerId)
        {
            if (proceso == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(proceso.Id))
            {
                proceso.Id = CrearId("proc", proceso.Nombre + "_" + ownerId);
            }

            proceso.Dependencias ??= new System.Collections.Generic.List<string>();
            proceso.Asignaciones ??= new System.Collections.Generic.List<AsignacionProductiva>();
            proceso.Resultado ??= new ResultadoProcesoProyecto();
        }

        private static string CrearId(string prefijo, string texto)
        {
            string limpio = (texto ?? "nuevo").Trim().ToLowerInvariant();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                limpio = limpio.Replace(c, '_');
            }

            limpio = limpio
                .Replace(" ", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace("__", "_");

            if (string.IsNullOrWhiteSpace(limpio))
            {
                limpio = Guid.NewGuid().ToString("N").Substring(0, 8);
            }

            return prefijo + "_" + limpio;
        }

        private static JsonSerializerOptions CrearOpcionesJson()
        {
            JsonSerializerOptions opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            opciones.Converters.Add(new JsonStringEnumConverter());
            return opciones;
        }
    }
}
