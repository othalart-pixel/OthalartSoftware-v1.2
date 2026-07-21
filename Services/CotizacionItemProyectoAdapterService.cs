using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class CotizacionItemProyectoAdapterService
    {
        public static void CapturarCotizacionEnItem(ItemProyecto item, Cotizacion cotizacion)
        {
            if (item == null || cotizacion == null)
            {
                return;
            }

            ProyectoCotizacion proyectoTemporal = cotizacion.ProyectoProductivo;
            cotizacion.ProyectoProductivo = null;

            try
            {
                item.CotizacionSnapshotJson = JsonSerializer.Serialize(cotizacion, CrearOpciones());
            }
            finally
            {
                cotizacion.ProyectoProductivo = proyectoTemporal;
            }
            item.FechaEdicionSnapshot = DateTime.Now;

            if (string.IsNullOrWhiteSpace(item.Nombre) && !string.IsNullOrWhiteSpace(cotizacion.NombreProyecto))
            {
                item.Nombre = cotizacion.NombreProyecto;
            }

            if (item.Snapshot == null)
            {
                item.Snapshot = new SnapshotItem();
            }

            item.Snapshot.NombreBiblioteca = string.IsNullOrWhiteSpace(item.Snapshot.NombreBiblioteca)
                ? item.Nombre
                : item.Snapshot.NombreBiblioteca;
            item.Snapshot.JsonMinimo = JsonSerializer.Serialize(new
            {
                cotizacion.NombreProyecto,
                cotizacion.Descripcion,
                Entregables = cotizacion.BriefProducto?.EntregablesSeleccionados?.Count ?? 0,
                Requerimientos = cotizacion.DesgloseProductivo?.Requerimientos?.Count ?? 0,
                cotizacion.CostoTotal,
                cotizacion.PrecioRecomendado
            }, CrearOpciones());
            item.Snapshot.FechaSnapshot = DateTime.Now;
        }

        public static Cotizacion CrearCotizacionDesdeItem(ItemProyecto item, Cotizacion fallback)
        {
            if (item == null)
            {
                return fallback ?? new Cotizacion();
            }

            if (!string.IsNullOrWhiteSpace(item.CotizacionSnapshotJson))
            {
                try
                {
                    Cotizacion cargada = JsonSerializer.Deserialize<Cotizacion>(
                        item.CotizacionSnapshotJson,
                        CrearOpciones()
                    );

                    if (cargada != null)
                    {
                        return cargada;
                    }
                }
                catch
                {
                }
            }

            Cotizacion nueva = new Cotizacion();
            nueva.NombreProyecto = item.Nombre;
            nueva.Descripcion = item.Descripcion;
            nueva.Moneda = fallback == null ? "CLP" : fallback.Moneda;
            nueva.MonedaVisualizacion = fallback == null ? "CLP" : fallback.MonedaVisualizacion;
            nueva.MonedaPrecioCliente = fallback == null ? "CLP" : fallback.MonedaPrecioCliente;
            nueva.MargenObjetivo = fallback == null ? 0.0 : fallback.MargenObjetivo;
            nueva.DiasHabilesEstudioPorSemana = fallback == null ? 5.0 : fallback.DiasHabilesEstudioPorSemana;
            return nueva;
        }

        public static bool ItemTieneSnapshot(ItemProyecto item)
        {
            return item != null && !string.IsNullOrWhiteSpace(item.CotizacionSnapshotJson);
        }

        private static JsonSerializerOptions CrearOpciones()
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
