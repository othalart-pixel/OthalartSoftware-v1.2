using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class AuditoriaVinculosJsonService
    {
        public sealed class HallazgoVinculoJson
        {
            public string Severidad { get; set; } = "";
            public string Biblioteca { get; set; } = "";
            public string Producto { get; set; } = "";
            public string Pieza { get; set; } = "";
            public string Campo { get; set; } = "";
            public string Mensaje { get; set; } = "";
            public string DondeEditar { get; set; } = "";
        }

        public sealed class ResultadoAuditoria
        {
            public List<HallazgoVinculoJson> Hallazgos { get; set; } = new List<HallazgoVinculoJson>();

            public int Errores
            {
                get { return Hallazgos.Count(h => h.Severidad == "Error"); }
            }

            public int Advertencias
            {
                get { return Hallazgos.Count(h => h.Severidad == "Advertencia"); }
            }

            public string Resumen
            {
                get
                {
                    return Errores == 0 && Advertencias == 0
                        ? "Bibliotecas vinculadas sin hallazgos."
                        : Errores + " errores / " + Advertencias + " advertencias";
                }
            }
        }

        public static ResultadoAuditoria Auditar()
        {
            ResultadoAuditoria resultado = new ResultadoAuditoria();

            List<Producto2DDefinicion> productos = BibliotecaProductos2DJsonService.CargarProductos();
            List<EcuacionProductivaDefinicion> ecuaciones = BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones();
            List<CategoriaTrabajador> cargos = BibliotecaCargosJsonService.CargarCargos();
            List<RendimientoProductivo> rendimientos = BibliotecaRendimientosProductivosJsonService.CargarRendimientos();
            List<SubEtapaProyecto> subetapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();

            Dictionary<string, EcuacionProductivaDefinicion> ecuacionesPorClave = ecuaciones
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Clave))
                .GroupBy(e => Normalizar(e.Clave))
                .ToDictionary(g => g.Key, g => g.First());

            HashSet<string> nombresCargos = cargos
                .Where(c => c != null)
                .SelectMany(c => new[] { c.Nombre, c.NombreCompleto })
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(Normalizar)
                .ToHashSet();

            HashSet<string> nombresSubetapas = subetapas
                .Where(s => s != null)
                .Select(s => Normalizar(s.Nombre))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToHashSet();

            foreach (var duplicado in productos
                .Where(p => p != null)
                .GroupBy(p => Normalizar(p.Nombre))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1))
            {
                Agregar(resultado, "Error", "productos2d.json", duplicado.First().Nombre, "", "Nombre",
                    "Producto duplicado. Esto hace ambiguo el selector de productos.",
                    "Productos");
            }

            foreach (Producto2DDefinicion producto in productos.Where(p => p != null))
            {
                AuditarProducto(resultado, producto, ecuacionesPorClave, nombresCargos, rendimientos, nombresSubetapas);
            }

            AuditarEcuaciones(resultado, ecuaciones, nombresCargos, rendimientos);

            return resultado;
        }

        public static int CompletarRendimientosFaltantes()
        {
            List<Producto2DDefinicion> productos = BibliotecaProductos2DJsonService.CargarProductos();
            List<EcuacionProductivaDefinicion> ecuaciones = BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones();
            List<RendimientoProductivo> rendimientos = BibliotecaRendimientosProductivosJsonService.CargarRendimientos();

            Dictionary<string, EcuacionProductivaDefinicion> ecuacionesPorClave = ecuaciones
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Clave))
                .GroupBy(e => Normalizar(e.Clave))
                .ToDictionary(g => g.Key, g => g.First());

            int siguienteId = rendimientos
                .Where(r => r != null)
                .Select(r => r.Id)
                .DefaultIfEmpty(0)
                .Max() + 1;

            int agregados = 0;

            foreach (Producto2DDefinicion producto in productos.Where(p => p != null))
            {
                foreach (Subproducto2D sub in producto.Subproductos.Where(s => s != null))
                {
                    if (UsaTiempoAsignado(sub))
                    {
                        continue;
                    }

                    EcuacionProductivaDefinicion ecuacion = null;
                    if (!string.IsNullOrWhiteSpace(sub.EcuacionProductiva))
                    {
                        ecuacionesPorClave.TryGetValue(Normalizar(ExtraerClaveEcuacion(sub.EcuacionProductiva)), out ecuacion);
                    }

                    List<string> cargos = ecuacion == null
                        ? SepararCargos(sub.CargosSugeridos)
                        : EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion).Select(c => c.Cargo).ToList();

                    foreach (string cargo in cargos)
                    {
                        if (!CargoRequiereRendimiento(cargo, sub.Nombre, sub.SubEtapaSugerida, sub.EtapaSugerida, sub.VariablesEcuacion))
                        {
                            continue;
                        }

                        if (TieneRendimientoCompatible(rendimientos, cargo, sub))
                        {
                            continue;
                        }

                        RendimientoProductivo nuevo = CrearRendimientoSugerido(siguienteId++, cargo, sub, ecuacion);
                        rendimientos.Add(nuevo);
                        agregados++;
                    }
                }
            }

            foreach (EcuacionProductivaDefinicion ecuacion in ecuaciones.Where(e => e != null && e.Activa))
            {
                foreach (var cargo in EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion))
                {
                    if (!CargoParticipanteRequiereRendimiento(cargo, ecuacion))
                    {
                        continue;
                    }

                    if (TieneRendimientoCompatible(rendimientos, cargo.Cargo, ecuacion.SubEtapa, ecuacion.Etapa, ecuacion.Variables))
                    {
                        continue;
                    }

                    RendimientoProductivo nuevo = CrearRendimientoSugerido(siguienteId++, cargo.Cargo, ecuacion);
                    rendimientos.Add(nuevo);
                    agregados++;
                }
            }

            if (agregados > 0)
            {
                BibliotecaRendimientosProductivosJsonService.GuardarRendimientos(rendimientos);
            }

            return agregados;
        }

        public static int EliminarRendimientosNoProductivosSugeridos()
        {
            List<RendimientoProductivo> rendimientos = BibliotecaRendimientosProductivosJsonService.CargarRendimientos();
            int antes = rendimientos.Count;

            rendimientos = rendimientos
                .Where(r => r == null ||
                    !EsRendimientoSugeridoPorAuditoria(r) ||
                    CargoRequiereRendimiento(r.Cargo, r.Proceso, r.TipoInterno, r.Etapa, r.Unidad))
                .ToList();

            int eliminados = antes - rendimientos.Count;
            if (eliminados > 0)
            {
                BibliotecaRendimientosProductivosJsonService.GuardarRendimientos(rendimientos);
            }

            return eliminados;
        }

        public static string ConstruirReporteTexto(ResultadoAuditoria auditoria)
        {
            if (auditoria == null)
            {
                return "No hay auditoria disponible.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Auditoria de vinculos JSON");
            sb.AppendLine(auditoria.Resumen);
            sb.AppendLine();

            foreach (IGrouping<string, HallazgoVinculoJson> grupo in auditoria.Hallazgos.GroupBy(h => h.Severidad))
            {
                sb.AppendLine(grupo.Key);
                foreach (HallazgoVinculoJson h in grupo)
                {
                    sb.Append("- ");
                    sb.Append(h.Biblioteca);
                    if (!string.IsNullOrWhiteSpace(h.Producto))
                    {
                        sb.Append(" | ");
                        sb.Append(h.Producto);
                    }
                    if (!string.IsNullOrWhiteSpace(h.Pieza))
                    {
                        sb.Append(" | ");
                        sb.Append(h.Pieza);
                    }
                    sb.Append(": ");
                    sb.AppendLine(h.Mensaje);
                    if (!string.IsNullOrWhiteSpace(h.DondeEditar))
                    {
                        sb.AppendLine("  Editar en: " + h.DondeEditar);
                    }
                }
                sb.AppendLine();
            }

            if (auditoria.Hallazgos.Count == 0)
            {
                sb.AppendLine("Todo conversa: productos, ecuaciones, cargos, rendimientos y subetapas.");
            }

            return sb.ToString();
        }

        private static void AuditarProducto(
            ResultadoAuditoria resultado,
            Producto2DDefinicion producto,
            Dictionary<string, EcuacionProductivaDefinicion> ecuacionesPorClave,
            HashSet<string> nombresCargos,
            List<RendimientoProductivo> rendimientos,
            HashSet<string> nombresSubetapas
        )
        {
            string productoNombre = producto.Nombre ?? "";
            HashSet<string> piezasProducto = producto.Subproductos
                .Where(s => s != null)
                .Select(s => Normalizar(s.Nombre))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToHashSet();

            HashSet<string> etapasProducto = producto.Etapas
                .Where(e => e != null && e.Activa)
                .SelectMany(e => new[] { e.ClaveEtapa, e.NombreVisible })
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(Normalizar)
                .ToHashSet();

            foreach (var duplicado in producto.Subproductos
                .Where(s => s != null)
                .GroupBy(s => Normalizar(s.Nombre))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1))
            {
                Agregar(resultado, "Advertencia", "productos2d.json", productoNombre, duplicado.First().Nombre, "Pieza",
                    "Pieza/subproducto duplicado dentro del producto. Puede duplicar desglose y Gantt.",
                    "Productos / Pipeline");
            }

            foreach (Subproducto2D sub in producto.Subproductos.Where(s => s != null))
            {
                AuditarSubproducto(resultado, productoNombre, sub, etapasProducto, piezasProducto, ecuacionesPorClave, nombresCargos, rendimientos, nombresSubetapas);
            }
        }

        private static void AuditarSubproducto(
            ResultadoAuditoria resultado,
            string producto,
            Subproducto2D sub,
            HashSet<string> etapasProducto,
            HashSet<string> piezasProducto,
            Dictionary<string, EcuacionProductivaDefinicion> ecuacionesPorClave,
            HashSet<string> nombresCargos,
            List<RendimientoProductivo> rendimientos,
            HashSet<string> nombresSubetapas
        )
        {
            string pieza = sub.Nombre ?? "";

            if (string.IsNullOrWhiteSpace(pieza))
            {
                Agregar(resultado, "Error", "productos2d.json", producto, "", "Pieza",
                    "Hay una pieza sin nombre.",
                    "Productos / Pipeline");
            }

            if (!string.IsNullOrWhiteSpace(sub.EtapaSugerida) &&
                etapasProducto.Count > 0 &&
                !etapasProducto.Contains(Normalizar(sub.EtapaSugerida)))
            {
                Agregar(resultado, "Advertencia", "productos2d.json", producto, pieza, "Etapa productiva",
                    "La etapa de la pieza no aparece activa en el pipeline del producto.",
                    "Productos / Pipeline");
            }

            foreach (string dependencia in SepararLista(sub.DependeDe))
            {
                if (!piezasProducto.Contains(Normalizar(dependencia)))
                {
                    Agregar(resultado, "Error", "productos2d.json", producto, pieza, "Depende de",
                        "Dependencia explicita no encontrada en el mismo producto: " + dependencia,
                        "Productos / Pipeline");
                }
            }

            if (!string.IsNullOrWhiteSpace(sub.SubEtapaSugerida) &&
                !nombresSubetapas.Contains(Normalizar(sub.SubEtapaSugerida)))
            {
                Agregar(resultado, "Advertencia", "productos2d.json + subetapas.json", producto, pieza, "Subetapa / trabajo",
                    "La subetapa sugerida no existe en subetapas.json: " + sub.SubEtapaSugerida,
                    "Subetapas");
            }

            EcuacionProductivaDefinicion ecuacion = null;
            if (string.IsNullOrWhiteSpace(sub.EcuacionProductiva))
            {
                Agregar(resultado, "Error", "productos2d.json", producto, pieza, "Ecuacion productiva",
                    "La pieza no tiene ecuacion productiva explicita.",
                    "Productos / Pipeline o Ecuaciones");
            }
            else
            {
                string clave = ExtraerClaveEcuacion(sub.EcuacionProductiva);
                if (!ecuacionesPorClave.TryGetValue(Normalizar(clave), out ecuacion))
                {
                    Agregar(resultado, "Error", "productos2d.json + ecuaciones_productivas.json", producto, pieza, "Ecuacion productiva",
                        "La ecuacion indicada no existe en ecuaciones_productivas.json: " + sub.EcuacionProductiva,
                        "Ecuaciones");
                }
            }

            List<EcuacionProductivaRuntimeService.CargoVector> cargosExplicitos = SepararCargos(sub.CargosSugeridos)
                .Select(c => new EcuacionProductivaRuntimeService.CargoVector
                {
                    Cargo = c,
                    Dedicacion = 1.0
                })
                .ToList();

            List<EcuacionProductivaRuntimeService.CargoVector> cargosEcuacion = ecuacion == null
                ? new List<EcuacionProductivaRuntimeService.CargoVector>()
                : EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion);

            List<EcuacionProductivaRuntimeService.CargoVector> cargosParaValidar =
                cargosEcuacion.Count > 0 ? cargosEcuacion : cargosExplicitos;

            if (cargosParaValidar.Count == 0)
            {
                Agregar(resultado, "Advertencia", "productos2d.json + ecuaciones_productivas.json", producto, pieza, "Cargos",
                    "No hay cargos participantes definidos para calcular costo/mano de obra.",
                    "Ecuaciones o Productos / Pipeline");
            }

            foreach (EcuacionProductivaRuntimeService.CargoVector cargo in cargosParaValidar)
            {
                if (!CargoExiste(cargo.Cargo, nombresCargos))
                {
                    Agregar(resultado, "Error", "cargos.json", producto, pieza, "Cargo",
                        "Cargo no encontrado: " + cargo.Cargo,
                        "Cargos");
                    continue;
                }

                if (!CargoParticipanteRequiereRendimiento(cargo, pieza, sub.SubEtapaSugerida, sub.EtapaSugerida, sub.VariablesEcuacion))
                {
                    continue;
                }

                if (!TieneRendimientoCompatible(rendimientos, cargo.Cargo, sub))
                {
                    Agregar(resultado, "Advertencia", "rendimientos_productivos.json", producto, pieza, "Rendimiento",
                        "No se encontro rendimiento activo compatible para cargo/proceso: " + cargo.Cargo,
                        "Rendimientos");
                }
            }
        }

        private static void AuditarEcuaciones(
            ResultadoAuditoria resultado,
            List<EcuacionProductivaDefinicion> ecuaciones,
            HashSet<string> nombresCargos,
            List<RendimientoProductivo> rendimientos
        )
        {
            HashSet<string> claves = ecuaciones
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Clave))
                .Select(e => Normalizar(e.Clave))
                .ToHashSet();

            foreach (var duplicado in ecuaciones
                .Where(e => e != null)
                .GroupBy(e => Normalizar(e.Clave))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1))
            {
                Agregar(resultado, "Error", "ecuaciones_productivas.json", "", duplicado.First().NombreVisible, "Clave",
                    "Clave de ecuacion duplicada: " + duplicado.First().Clave,
                    "Ecuaciones");
            }

            foreach (EcuacionProductivaDefinicion ecuacion in ecuaciones.Where(e => e != null && e.Activa))
            {
                bool esMadre = Normalizar(ecuacion.TipoEcuacion) == "base" ||
                    Normalizar(ecuacion.TipoEcuacion) == "madre";

                if (!esMadre && string.IsNullOrWhiteSpace(ecuacion.EcuacionBase))
                {
                    Agregar(resultado, "Error", "ecuaciones_productivas.json", "", ecuacion.NombreVisible, "Formula madre",
                        "Proceso activo sin formula madre.",
                        "Ecuaciones");
                }

                if (!string.IsNullOrWhiteSpace(ecuacion.EcuacionBase) &&
                    !claves.Contains(Normalizar(ecuacion.EcuacionBase)))
                {
                    Agregar(resultado, "Error", "ecuaciones_productivas.json", "", ecuacion.NombreVisible, "Formula madre",
                        "Formula madre no encontrada: " + ecuacion.EcuacionBase,
                        "Ecuaciones");
                }

                if (string.IsNullOrWhiteSpace(ecuacion.Variables))
                {
                    Agregar(resultado, "Advertencia", "ecuaciones_productivas.json", "", ecuacion.NombreVisible, "Variables",
                        "Ecuacion activa sin variables de entrada.",
                        "Ecuaciones");
                }

                foreach (var cargo in EcuacionProductivaRuntimeService.ObtenerVectorCargos(ecuacion))
                {
                    if (!CargoExiste(cargo.Cargo, nombresCargos))
                    {
                        Agregar(resultado, "Error", "ecuaciones_productivas.json + cargos.json", "", ecuacion.NombreVisible, "Cargo",
                            "Cargo participante no encontrado: " + cargo.Cargo,
                            "Ecuaciones / Cargos");
                    }
                    else if (CargoParticipanteRequiereRendimiento(cargo, ecuacion) &&
                        !TieneRendimientoCompatible(rendimientos, cargo.Cargo, ecuacion.SubEtapa, ecuacion.Etapa, ecuacion.Variables))
                    {
                        Agregar(resultado, "Advertencia", "ecuaciones_productivas.json + rendimientos_productivos.json", "", ecuacion.NombreVisible, "Rendimiento",
                            "Cargo participante sin rendimiento activo compatible: " + cargo.Cargo,
                            "Rendimientos");
                    }
                }
            }
        }

        private static bool CargoRequiereRendimiento(string cargo, string pieza, string proceso, string etapa, string variables)
        {
            CategoriaTrabajador cargoBiblioteca = BibliotecaCargosJsonService.BuscarCargo(cargo, etapa, "");
            if (cargoBiblioteca != null)
            {
                return ClasificacionCargosService.RequiereRendimientoProductivo(
                    cargoBiblioteca,
                    pieza,
                    proceso,
                    etapa,
                    variables
                );
            }

            return ClasificacionCargosService.RequiereRendimientoProductivo(
                cargo,
                pieza,
                proceso,
                etapa,
                variables
            );
        }

        private static bool UsaTiempoAsignado(Subproducto2D sub)
        {
            return sub != null &&
                ModosCalculoProductivo.EsTiempoAsignado(sub.ModoCalculoProductivo);
        }

        private static bool CargoParticipanteRequiereRendimiento(
            EcuacionProductivaRuntimeService.CargoVector cargo,
            EcuacionProductivaDefinicion ecuacion
        )
        {
            if (cargo == null || ecuacion == null || string.IsNullOrWhiteSpace(cargo.Cargo))
            {
                return false;
            }

            // Una dedicacion menor a 100% representa apoyo/gestion sobre el tiempo base
            // de otro cargo productivo. No debe crear rendimientos falsos para ese cargo.
            if (cargo.Dedicacion > 0.0 && cargo.Dedicacion < 0.999)
            {
                return false;
            }

            return CargoRequiereRendimiento(
                cargo.Cargo,
                ecuacion.NombreVisible,
                ecuacion.SubEtapa,
                ecuacion.Etapa,
                ecuacion.Variables + ";" + ecuacion.Tokens
            );
        }

        private static bool CargoParticipanteRequiereRendimiento(
            EcuacionProductivaRuntimeService.CargoVector cargo,
            string pieza,
            string proceso,
            string etapa,
            string variables
        )
        {
            if (cargo == null || string.IsNullOrWhiteSpace(cargo.Cargo))
            {
                return false;
            }

            if (cargo.Dedicacion > 0.0 && cargo.Dedicacion < 0.999)
            {
                return false;
            }

            return CargoRequiereRendimiento(cargo.Cargo, pieza, proceso, etapa, variables);
        }

        private static bool EsRendimientoSugeridoPorAuditoria(RendimientoProductivo rendimiento)
        {
            string nota = Normalizar(rendimiento.Nota);
            return nota.Contains("validacion json") ||
                nota.Contains("auditoria") ||
                nota.Contains("sugerida por validacion");
        }

        private static bool TieneRendimientoCompatible(List<RendimientoProductivo> rendimientos, string cargo, Subproducto2D sub)
        {
            return TieneRendimientoCompatible(rendimientos, cargo, sub.SubEtapaSugerida, sub.EtapaSugerida, sub.VariablesEcuacion + ";" + sub.Nombre);
        }

        private static RendimientoProductivo CrearRendimientoSugerido(
            int id,
            string cargo,
            Subproducto2D sub,
            EcuacionProductivaDefinicion ecuacion
        )
        {
            string proceso = !string.IsNullOrWhiteSpace(sub.SubEtapaSugerida)
                ? sub.SubEtapaSugerida
                : sub.Nombre;
            string unidad = InferirUnidad((sub.VariablesEcuacion ?? "") + ";" + (ecuacion == null ? "" : ecuacion.Variables) + ";" + sub.Nombre);
            double cantidad = CapacidadBasePara(unidad, proceso);

            return CrearRendimientoSugerido(
                id,
                cargo,
                sub.EtapaSugerida,
                proceso,
                proceso,
                unidad,
                cantidad
            );
        }

        private static RendimientoProductivo CrearRendimientoSugerido(
            int id,
            string cargo,
            EcuacionProductivaDefinicion ecuacion
        )
        {
            string proceso = !string.IsNullOrWhiteSpace(ecuacion.SubEtapa)
                ? ecuacion.SubEtapa
                : ecuacion.NombreVisible;
            string unidad = InferirUnidad((ecuacion.Variables ?? "") + ";" + (ecuacion.Tokens ?? "") + ";" + ecuacion.NombreVisible);
            double cantidad = CapacidadBasePara(unidad, proceso);

            return CrearRendimientoSugerido(
                id,
                cargo,
                ecuacion.Etapa,
                proceso,
                proceso,
                unidad,
                cantidad
            );
        }

        private static RendimientoProductivo CrearRendimientoSugerido(
            int id,
            string cargo,
            string etapa,
            string tipo,
            string proceso,
            string unidad,
            double cantidad
        )
        {
            return new RendimientoProductivo
            {
                Id = id,
                Activo = true,
                Etapa = string.IsNullOrWhiteSpace(etapa) ? "General" : etapa,
                TipoInterno = string.IsNullOrWhiteSpace(tipo) ? proceso : tipo,
                Proceso = proceso,
                Unidad = unidad,
                Cargo = LimpiarCargoParaRendimiento(cargo),
                NivelCargo = "tipico",
                CantidadMinimaPorPeriodo = cantidad * 0.80,
                CantidadPorPeriodo = cantidad,
                CantidadMaximaPorPeriodo = cantidad * 1.25,
                Periodo = "semana",
                Nota = "Base sugerida por Validacion JSON. Revisar capacidad real del estudio."
            };
        }

        private static string InferirUnidad(string texto)
        {
            string t = Normalizar(texto);

            if (t.Contains("segundo") || t.Contains("duracion") || t.Contains("animatic") || t.Contains("audio"))
            {
                return "segundos";
            }

            if (t.Contains("personaje"))
            {
                return "personajes";
            }

            if (t.Contains("fondo") || t.Contains("background"))
            {
                return "fondos";
            }

            if (t.Contains("pagina") || t.Contains("vineta") || t.Contains("comic"))
            {
                return "paginas";
            }

            return "piezas";
        }

        private static double CapacidadBasePara(string unidad, string proceso)
        {
            string u = Normalizar(unidad);
            string p = Normalizar(proceso);

            if (u.Contains("segundo"))
            {
                if (p.Contains("rough")) return 8.0;
                if (p.Contains("clean")) return 10.0;
                if (p.Contains("color")) return 15.0;
                return 10.0;
            }

            if (u.Contains("personaje") || u.Contains("fondo"))
            {
                return 2.0;
            }

            if (u.Contains("pagina"))
            {
                return 5.0;
            }

            return 5.0;
        }

        private static bool TieneRendimientoCompatible(
            List<RendimientoProductivo> rendimientos,
            string cargo,
            string proceso,
            string etapa,
            string tokens
        )
        {
            string cargoNorm = NormalizarCargo(cargo);
            string procesoNorm = Normalizar(proceso);
            string etapaNorm = Normalizar(etapa);
            string tokensNorm = Normalizar(tokens);

            return rendimientos.Any(r =>
                r != null &&
                r.Activo &&
                r.CantidadPorPeriodo > 0.0 &&
                CargoCompatible(cargoNorm, r.Cargo) &&
                ProcesoCompatible(procesoNorm, tokensNorm, r) &&
                (string.IsNullOrWhiteSpace(etapaNorm) || string.IsNullOrWhiteSpace(r.Etapa) || Normalizar(r.Etapa) == etapaNorm));
        }

        private static bool ProcesoCompatible(string procesoNorm, string tokensNorm, RendimientoProductivo rendimiento)
        {
            if (string.IsNullOrWhiteSpace(procesoNorm))
            {
                return true;
            }

            string procesoRendimiento = Normalizar(rendimiento.Proceso);
            string tipoRendimiento = Normalizar(rendimiento.TipoInterno);

            return (!string.IsNullOrWhiteSpace(procesoRendimiento) &&
                    (procesoRendimiento.Contains(procesoNorm) || procesoNorm.Contains(procesoRendimiento))) ||
                   (!string.IsNullOrWhiteSpace(tipoRendimiento) && tokensNorm.Contains(tipoRendimiento));
        }

        private static bool CargoExiste(string cargo, HashSet<string> nombresCargos)
        {
            string cargoNorm = NormalizarCargo(cargo);
            return nombresCargos.Any(c => c == cargoNorm || c.Contains(cargoNorm) || cargoNorm.Contains(c));
        }

        private static bool CargoCompatible(string cargoNorm, string cargoRendimiento)
        {
            string rendimientoNorm = NormalizarCargo(cargoRendimiento);
            if (string.IsNullOrWhiteSpace(cargoNorm) || string.IsNullOrWhiteSpace(rendimientoNorm))
            {
                return false;
            }

            return rendimientoNorm == cargoNorm ||
                rendimientoNorm.Contains(cargoNorm) ||
                cargoNorm.Contains(rendimientoNorm);
        }

        private static string ExtraerClaveEcuacion(string valor)
        {
            string texto = valor ?? "";
            int separador = texto.IndexOf('|');
            return separador >= 0 ? texto.Substring(0, separador).Trim() : texto.Trim();
        }

        private static string LimpiarCargoParaRendimiento(string cargo)
        {
            string texto = (cargo ?? "").Trim();
            int separador = texto.IndexOf('|');
            if (separador >= 0)
            {
                texto = texto.Substring(0, separador).Trim();
            }

            int parentesis = texto.IndexOf('(');
            if (parentesis >= 0)
            {
                texto = texto.Substring(0, parentesis).Trim();
            }

            return texto;
        }

        private static List<string> SepararCargos(string texto)
        {
            return SepararLista(texto)
                .Select(v =>
                {
                    int separador = v.IndexOf('|');
                    return separador >= 0 ? v.Substring(0, separador).Trim() : v.Trim();
                })
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> SepararLista(string texto)
        {
            return (texto ?? "")
                .Replace("\r", ";")
                .Replace("\n", ";")
                .Replace(" y ", ";")
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
        }

        private static void Agregar(
            ResultadoAuditoria resultado,
            string severidad,
            string biblioteca,
            string producto,
            string pieza,
            string campo,
            string mensaje,
            string dondeEditar
        )
        {
            resultado.Hallazgos.Add(new HallazgoVinculoJson
            {
                Severidad = severidad,
                Biblioteca = biblioteca,
                Producto = producto,
                Pieza = pieza,
                Campo = campo,
                Mensaje = mensaje,
                DondeEditar = dondeEditar
            });
        }

        private static string NormalizarCargo(string texto)
        {
            string normalizado = Normalizar(texto);
            int parentesis = normalizado.IndexOf('(');
            if (parentesis >= 0)
            {
                normalizado = normalizado.Substring(0, parentesis).Trim();
            }
            return normalizado;
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
                .Replace("ñ", "n");
        }
    }
}
