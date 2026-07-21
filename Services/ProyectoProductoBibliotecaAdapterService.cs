using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class ProyectoProductoBibliotecaAdapterService
    {
        public sealed class OpcionesAgregarProducto
        {
            public decimal Cantidad { get; set; } = 1m;
            public string Unidad { get; set; } = "";
            public ModoCantidadSubproducto ModoCantidad { get; set; } = ModoCantidadSubproducto.Homogeneo;
            public string Complejidad { get; set; } = "Media";
            public string Acabado { get; set; } = "Estándar";
            public int CiclosRevision { get; set; } = 1;
            public decimal TasaRetrabajo { get; set; } = 10m;
            public decimal Duracion { get; set; } = 0m;
            public List<Subproducto2D> SubproductosSeleccionados { get; set; } = new List<Subproducto2D>();
        }

        public static ProductoProyecto CrearProductoDesdeBiblioteca(
            Producto2DDefinicion producto,
            OpcionesAgregarProducto opciones,
            int orden
        )
        {
            if (producto == null)
            {
                throw new ArgumentNullException(nameof(producto));
            }

            opciones ??= new OpcionesAgregarProducto();
            List<Subproducto2D> subproductos = opciones.SubproductosSeleccionados == null || opciones.SubproductosSeleccionados.Count == 0
                ? producto.Subproductos.Where(s => s != null && s.RequeridoPorDefecto).ToList()
                : opciones.SubproductosSeleccionados.Where(s => s != null).ToList();

            string productoId = CrearId("prod", producto.Nombre + "_" + orden);
            ProductoProyecto item = new ProductoProyecto
            {
                Id = productoId,
                BibliotecaId = CrearId("bib", producto.Nombre),
                Nombre = producto.Nombre,
                Descripcion = producto.Nota,
                Cantidad = opciones.Cantidad <= 0m ? 1m : opciones.Cantidad,
                Unidad = string.IsNullOrWhiteSpace(opciones.Unidad)
                    ? producto.UnidadCantidadSugerida
                    : opciones.Unidad,
                Orden = orden,
                Snapshot = new SnapshotItem
                {
                    NombreBiblioteca = producto.Nombre,
                    CategoriaBiblioteca = producto.Categoria,
                    UnidadBase = producto.UnidadCantidadSugerida,
                    JsonMinimo = JsonSerializer.Serialize(new
                    {
                        producto.Nombre,
                        producto.Industria,
                        producto.Categoria,
                        producto.UnidadCantidadSugerida,
                        producto.UnidadDuracionSugerida,
                        producto.DuracionSugerida,
                        Subproductos = producto.Subproductos.Select(s => s.Nombre).ToList()
                    }),
                    FechaSnapshot = DateTime.Now
                }
            };

            item.Parametros["Complejidad"] = opciones.Complejidad;
            item.Parametros["Acabado"] = opciones.Acabado;
            item.Parametros["CiclosRevision"] = opciones.CiclosRevision;
            item.Parametros["TasaRetrabajo"] = opciones.TasaRetrabajo;
            item.Parametros["Duracion"] = opciones.Duracion;
            item.Parametros["PersonalizadoProyecto"] = true;

            ProyectoOverridesService.RegistrarOverrideProducto(
                item,
                "Cantidad",
                item.Cantidad,
                AlcanceModificacion.ProductoProyecto,
                "Cantidad definida al agregar desde biblioteca"
            );
            ProyectoOverridesService.RegistrarOverrideProducto(
                item,
                "Unidad",
                item.Unidad,
                AlcanceModificacion.ProductoProyecto,
                "Unidad definida al agregar desde biblioteca",
                producto.UnidadCantidadSugerida
            );
            ProyectoOverridesService.RegistrarOverrideProducto(
                item,
                "Parametros",
                item.Parametros,
                AlcanceModificacion.ProductoProyecto,
                "Parametros recomendados aplicados al proyecto"
            );

            List<EcuacionProductivaDefinicion> ecuaciones = BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones();
            int ordenSub = 0;
            foreach (Subproducto2D sub in subproductos.OrderBy(s => s.Orden))
            {
                item.Subproductos.Add(CrearSubproducto(productoId, sub, opciones, ordenSub++, ecuaciones));
            }

            return item;
        }

        private static SubproductoProyecto CrearSubproducto(
            string productoId,
            Subproducto2D sub,
            OpcionesAgregarProducto opciones,
            int orden,
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            SubproductoProyecto proyecto = new SubproductoProyecto
            {
                Id = CrearId("sub", productoId + "_" + sub.Nombre + "_" + orden),
                ProductoProyectoId = productoId,
                SubproductoBibliotecaId = CrearId("subbib", sub.Nombre),
                Nombre = sub.Nombre,
                Cantidad = opciones.Cantidad <= 0m ? 1m : opciones.Cantidad,
                Unidad = string.IsNullOrWhiteSpace(opciones.Unidad) ? "unidad" : opciones.Unidad,
                ModoCantidad = opciones.ModoCantidad,
                Orden = orden,
                Notas = sub.Nota
            };

            proyecto.Parametros["Categoria"] = sub.Categoria;
            proyecto.Parametros["EtapaSugerida"] = sub.EtapaSugerida;
            proyecto.Parametros["SubEtapaSugerida"] = sub.SubEtapaSugerida;
            proyecto.Parametros["EquationKey"] = string.IsNullOrWhiteSpace(sub.EquationKey)
                ? ExtraerClaveEcuacion(sub.EcuacionProductiva)
                : sub.EquationKey;
            proyecto.Parametros["EcuacionProductiva"] = sub.EcuacionProductiva;
            proyecto.Parametros["VariablesEcuacion"] = sub.VariablesEcuacion;
            proyecto.Parametros["CargosSugeridos"] = sub.CargosSugeridos;
            proyecto.Parametros["Complejidad"] = opciones.Complejidad;
            proyecto.Parametros["Acabado"] = opciones.Acabado;
            proyecto.Parametros["CiclosRevision"] = opciones.CiclosRevision;
            proyecto.Parametros["TasaRetrabajo"] = opciones.TasaRetrabajo;
            proyecto.Parametros["Duracion"] = opciones.Duracion;

            MaterializarProcesosSubproducto(proyecto, sub, opciones, ecuaciones);

            return proyecto;
        }

        private static void MaterializarProcesosSubproducto(
            SubproductoProyecto subproyecto,
            Subproducto2D sub,
            OpcionesAgregarProducto opciones,
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            EcuacionProductivaDefinicion ecuacionBase = ResolverEcuacionSubproducto(sub, ecuaciones);
            if (ecuacionBase == null)
            {
                return;
            }

            List<EcuacionProductivaDefinicion> procesos = new List<EcuacionProductivaDefinicion> { ecuacionBase };
            procesos.AddRange((ecuaciones ?? new List<EcuacionProductivaDefinicion>())
                .Where(e => e != null && e.Activa && DependeDeProceso(e, ecuacionBase))
                .OrderBy(e => e.TipoProceso == TipoProcesoProductivo.RevisionControl ? 10 :
                    e.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo ? 20 : 30));

            HashSet<string> agregados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int ordenProceso = 0;
            foreach (EcuacionProductivaDefinicion ecuacion in procesos)
            {
                string claveProceso = string.IsNullOrWhiteSpace(ecuacion.IdProceso)
                    ? CrearId("proc", ecuacion.Clave)
                    : ecuacion.IdProceso;
                if (!agregados.Add(claveProceso))
                {
                    continue;
                }

                ProcesoProyecto proceso = CrearProcesoDesdeEcuacion(
                    subproyecto,
                    sub,
                    opciones,
                    ecuacion,
                    ecuaciones,
                    ordenProceso++);
                subproyecto.Procesos.Add(proceso);
            }
        }

        private static ProcesoProyecto CrearProcesoDesdeEcuacion(
            SubproductoProyecto subproyecto,
            Subproducto2D sub,
            OpcionesAgregarProducto opciones,
            EcuacionProductivaDefinicion ecuacion,
            List<EcuacionProductivaDefinicion> ecuaciones,
            int ordenProceso
        )
        {
            string procesoId = CrearId("proc", subproyecto.Id + "_" + (string.IsNullOrWhiteSpace(ecuacion.IdProceso) ? ecuacion.Clave : ecuacion.IdProceso));
            RequerimientoProduccionInterna req = CrearRequerimientoParaEvaluacion(subproyecto, sub, opciones, ecuacion, procesoId);
            EcuacionProductivaRuntimeService.ResultadoPrueba resultado =
                EcuacionProductivaRuntimeService.EvaluarRequerimiento(ecuacion, ecuaciones, req, 5.0);

            ProcesoProyecto proceso = new ProcesoProyecto
            {
                Id = procesoId,
                ProcesoBibliotecaId = string.IsNullOrWhiteSpace(ecuacion.IdProceso) ? ecuacion.Clave : ecuacion.IdProceso,
                Nombre = string.IsNullOrWhiteSpace(ecuacion.NombreVisible) ? sub.Nombre : ecuacion.NombreVisible,
                TipoProceso = ecuacion.TipoProceso,
                MetodoCalculo = ecuacion.MetodoCalculo,
                AlcanceTemporal = ecuacion.AlcanceTemporal,
                EtapaId = string.IsNullOrWhiteSpace(ecuacion.EtapaId) ? sub.EtapaSugerida : ecuacion.EtapaId,
                SubetapaId = string.IsNullOrWhiteSpace(ecuacion.SubEtapaId) ? sub.SubEtapaSugerida : ecuacion.SubEtapaId,
                Cantidad = Convert.ToDecimal(req.Cantidad),
                Unidad = req.Unidad ?? "",
                Capacidad = Convert.ToDecimal(
                    req.RendimientoCantidadOverride > 0.0
                        ? req.RendimientoCantidadOverride
                        : req.RendimientoCantidad),
                Periodo = string.IsNullOrWhiteSpace(req.RendimientoPeriodoOverride)
                    ? req.RendimientoPeriodo ?? ""
                    : req.RendimientoPeriodoOverride,
                Paralelo = ecuacion.PuedeEjecutarseEnParalelo,
                Dependencias = ResolverDependenciasProceso(ecuacion, subproyecto),
                Activo = true
            };

            proceso.Resultado.HorasCalculadas = Convert.ToDecimal(resultado.Cargos.Sum(c => c.HorasTecnicas));
            proceso.Resultado.HorasAsignadas = proceso.Resultado.HorasCalculadas;
            proceso.Resultado.CostoCalculado = Convert.ToDecimal(resultado.CostoCLP);
            proceso.Resultado.Diagnostico = ConstruirDiagnosticoResultado(resultado);
            proceso.Resultado.Warnings.AddRange(resultado.Advertencias);
            proceso.Resultado.Warnings.AddRange(resultado.Errores);

            int ordenAsignacion = 0;
            foreach (EcuacionProductivaRuntimeService.ResultadoCargo cargo in resultado.Cargos)
            {
                proceso.Asignaciones.Add(new AsignacionProductiva
                {
                    Id = CrearId("asg", procesoId + "_" + cargo.CargoResuelto + "_" + ordenAsignacion++),
                    ProcesoProyectoId = procesoId,
                    CargoId = string.IsNullOrWhiteSpace(cargo.CargoResuelto) ? cargo.CargoSolicitado : cargo.CargoResuelto,
                    HorasCalculadas = Convert.ToDecimal(cargo.HorasTecnicas),
                    HorasAsignadas = Convert.ToDecimal(cargo.HorasTecnicas),
                    OrigenHoras = OrigenHorasProductivas.Calculado,
                    DedicacionPorcentaje = Convert.ToDecimal(cargo.Dedicacion * 100.0),
                    CostoCalculado = Convert.ToDecimal(cargo.CostoCLP),
                    Notas = cargo.Diagnostico
                });
            }

            if (proceso.Asignaciones.Count == 0)
            {
                proceso.Resultado.Diagnostico = string.IsNullOrWhiteSpace(proceso.Resultado.Diagnostico)
                    ? "Proceso creado desde biblioteca, pero no tiene cargos/asignaciones calculables."
                    : proceso.Resultado.Diagnostico;
            }

            return proceso;
        }

        private static RequerimientoProduccionInterna CrearRequerimientoParaEvaluacion(
            SubproductoProyecto subproyecto,
            Subproducto2D sub,
            OpcionesAgregarProducto opciones,
            EcuacionProductivaDefinicion ecuacion,
            string procesoId
        )
        {
            return new RequerimientoProduccionInterna
            {
                EntregableCliente = sub.Nombre,
                CategoriaEntregable = sub.Categoria,
                EcuacionUsada = ecuacion.Clave + " | " + ecuacion.NombreVisible,
                TipoInterno = sub.Nombre,
                NombreRequerimiento = sub.Nombre,
                ProcesoId = procesoId,
                TipoProceso = ecuacion.TipoProceso,
                MetodoCalculo = ecuacion.MetodoCalculo,
                AlcanceTemporal = ecuacion.AlcanceTemporal,
                DependenciasProcesoJson = ecuacion.DependenciasJson,
                PuedeEjecutarseEnParalelo = ecuacion.PuedeEjecutarseEnParalelo,
                Cantidad = Convert.ToDouble(subproyecto.Cantidad <= 0m ? 1m : subproyecto.Cantidad),
                Unidad = string.IsNullOrWhiteSpace(subproyecto.Unidad) ? "unidad" : subproyecto.Unidad,
                EtapaSugerida = string.IsNullOrWhiteSpace(ecuacion.Etapa) ? sub.EtapaSugerida : ecuacion.Etapa,
                Calidad = opciones.Acabado,
                BloqueProductivo = sub.EtapaSugerida,
                DependeDe = sub.DependeDe,
                CargoSugerido = string.IsNullOrWhiteSpace(ecuacion.CargosPermitidos)
                    ? sub.CargosSugeridos
                    : ecuacion.CargosPermitidos,
                ModoCalculoProductivo = sub.ModoCalculoProductivo,
                HorasMinimas = sub.HorasAsignadasMin,
                HorasEstandar = sub.HorasAsignadasStd,
                HorasHolgura = sub.HorasAsignadasHolgura
            };
        }

        private static EcuacionProductivaDefinicion ResolverEcuacionSubproducto(
            Subproducto2D sub,
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            string clave = string.IsNullOrWhiteSpace(sub.EquationKey)
                ? ExtraerClaveEcuacion(sub.EcuacionProductiva)
                : sub.EquationKey;
            EcuacionProductivaDefinicion porClave = (ecuaciones ?? new List<EcuacionProductivaDefinicion>())
                .FirstOrDefault(e => e != null &&
                    (string.Equals(e.Clave, clave, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(e.IdProceso, clave, StringComparison.OrdinalIgnoreCase)));
            if (porClave != null)
            {
                return porClave;
            }

            return BibliotecaEcuacionesProductivasJsonService.BuscarMejorPara(
                sub.EtapaSugerida,
                sub.SubEtapaSugerida,
                sub.Nombre,
                sub.CargosSugeridos);
        }

        private static bool DependeDeProceso(EcuacionProductivaDefinicion candidata, EcuacionProductivaDefinicion baseProceso)
        {
            if (candidata == null || baseProceso == null)
            {
                return false;
            }

            string idBase = string.IsNullOrWhiteSpace(baseProceso.IdProceso) ? baseProceso.Clave : baseProceso.IdProceso;
            return LeerListaJson(candidata.DependenciasJson)
                .Any(d => string.Equals(d, idBase, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(d, baseProceso.Clave, StringComparison.OrdinalIgnoreCase)) ||
                LeerReglaActivacion(candidata.ReglaActivacionJson)?.ProcesoOrigenId.Equals(idBase, StringComparison.OrdinalIgnoreCase) == true;
        }

        private static List<string> ResolverDependenciasProceso(EcuacionProductivaDefinicion ecuacion, SubproductoProyecto subproyecto)
        {
            List<string> dependencias = LeerListaJson(ecuacion.DependenciasJson);
            if (dependencias.Count == 0)
            {
                return dependencias;
            }

            return dependencias
                .Select(dep =>
                {
                    ProcesoProyecto local = subproyecto.Procesos.FirstOrDefault(p =>
                        string.Equals(p.ProcesoBibliotecaId, dep, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Id, dep, StringComparison.OrdinalIgnoreCase));
                    return local == null ? dep : local.Id;
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> LeerListaJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return json.Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim().Trim('"', '[', ']'))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
            }
        }

        private static ReglaActivacionProceso LeerReglaActivacion(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ReglaActivacionProceso>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string ExtraerClaveEcuacion(string texto)
        {
            string valor = (texto ?? "").Trim();
            int pipe = valor.IndexOf('|');
            if (pipe >= 0)
            {
                valor = valor.Substring(0, pipe);
            }

            return valor.Trim();
        }

        private static string ConstruirDiagnosticoResultado(EcuacionProductivaRuntimeService.ResultadoPrueba resultado)
        {
            if (resultado == null)
            {
                return "No se pudo evaluar el proceso.";
            }

            if (resultado.Errores.Count > 0)
            {
                return string.Join("; ", resultado.Errores);
            }

            if (resultado.Advertencias.Count > 0)
            {
                return string.Join("; ", resultado.Advertencias);
            }

            return "OK";
        }

        private static string CrearId(string prefijo, string texto)
        {
            string limpio = new string((texto ?? "")
                .Trim()
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray());

            while (limpio.Contains("__"))
            {
                limpio = limpio.Replace("__", "_");
            }

            limpio = limpio.Trim('_');
            if (string.IsNullOrWhiteSpace(limpio))
            {
                limpio = Guid.NewGuid().ToString("N");
            }

            return prefijo + "_" + limpio;
        }
    }
}
