using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class EcuacionProductivaRuntimeService
    {
        public sealed class CargoVector
        {
            public string Cargo { get; set; } = "";
            public double Dedicacion { get; set; } = 1.0;
        }

        public sealed class ResultadoCargo
        {
            public string CargoSolicitado { get; set; } = "";
            public string CargoResuelto { get; set; } = "";
            public bool CargoExiste { get; set; }
            public bool RendimientoExiste { get; set; }
            public bool RequiereRendimientoProductivo { get; set; }
            public double Dedicacion { get; set; }
            public double TarifaDiaCLP { get; set; }
            public double TarifaDiaPonderadaCLP { get; set; }
            public double HorasPorDia { get; set; } = 8.0;
            public double TarifaHoraCLP { get; set; }
            public double TarifaHoraPonderadaCLP { get; set; }
            public double CapacidadPorPeriodo { get; set; }
            public string Periodo { get; set; } = "";
            public double DiasPeriodo { get; set; }
            public double CantidadPrueba { get; set; }
            public string UnidadPrueba { get; set; } = "";
            public double HorasTecnicas { get; set; }
            public double DiasTecnicos { get; set; }
            public double CostoCLP { get; set; }
            public string Diagnostico { get; set; } = "";
        }

        public sealed class ResultadoPrueba
        {
            public string Clave { get; set; } = "";
            public string Nombre { get; set; } = "";
            public string FormulaMadre { get; set; } = "";
            public string UnidadPrueba { get; set; } = "";
            public double CantidadPrueba { get; set; }
            public double DiasTecnicos { get; set; }
            public double CostoCLP { get; set; }
            public List<ResultadoCargo> Cargos { get; set; } = new List<ResultadoCargo>();
            public List<string> Errores { get; set; } = new List<string>();
            public List<string> Advertencias { get; set; } = new List<string>();
        }

        public sealed class DiagnosticoBiblioteca
        {
            public List<string> Errores { get; set; } = new List<string>();
            public List<string> Advertencias { get; set; } = new List<string>();
            public List<string> Ok { get; set; } = new List<string>();

            public bool PuedeGuardar
            {
                get { return Errores.Count == 0; }
            }
        }

        public static List<CargoVector> ParsearVectorCargos(string texto)
        {
            return (texto ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParsearCargoVector)
                .Where(c => !string.IsNullOrWhiteSpace(c.Cargo))
                .GroupBy(c => Normalizar(c.Cargo))
                .Select(g => g.First())
                .ToList();
        }

        public static string SerializarVectorCargos(IEnumerable<CargoVector> cargos)
        {
            return string.Join(";", (cargos ?? Enumerable.Empty<CargoVector>())
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.Cargo))
                .Select(c => c.Cargo.Trim() + "|" + c.Dedicacion.ToString("0.####", CultureInfo.InvariantCulture)));
        }

        public static List<CargoVector> ObtenerVectorCargos(EcuacionProductivaDefinicion ecuacion)
        {
            if (ecuacion == null)
            {
                return new List<CargoVector>();
            }

            List<CargoVector> desdeJson = ParsearVectorCargosParticipantesJson(ecuacion.CargosParticipantesJson);
            if (desdeJson.Count > 0)
            {
                return desdeJson;
            }

            return ParsearVectorCargos(ecuacion.CargosPermitidos);
        }

        public static List<CargoVector> ParsearVectorCargosParticipantesJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CargoVector>();
            }

            try
            {
                List<CargoParticipanteFormula> participantes =
                    JsonSerializer.Deserialize<List<CargoParticipanteFormula>>(json) ??
                    new List<CargoParticipanteFormula>();

                return participantes
                    .Where(p => p != null && p.Activo && !string.IsNullOrWhiteSpace(p.Cargo))
                    .Select(p => new CargoVector
                    {
                        Cargo = p.Cargo.Trim(),
                        Dedicacion = Math.Max(0.0, Math.Min(5.0, p.DedicacionPorcentaje / 100.0))
                    })
                    .GroupBy(c => Normalizar(c.Cargo))
                    .Select(g => g.First())
                    .ToList();
            }
            catch
            {
                return new List<CargoVector>();
            }
        }

        public static ResultadoPrueba ProbarEcuacion(
            EcuacionProductivaDefinicion ecuacion,
            IEnumerable<EcuacionProductivaDefinicion> biblioteca,
            double diasHabilesSemana
        )
        {
            ResultadoPrueba resultado = new ResultadoPrueba();
            if (ecuacion == null)
            {
                resultado.Errores.Add("No hay ecuacion seleccionada.");
                return resultado;
            }

            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;

            DeterminarEntradaPrueba(efectiva, out double cantidad, out string unidad);
            resultado.CantidadPrueba = cantidad;
            resultado.UnidadPrueba = unidad;

            List<CargoVector> vector = ObtenerVectorCargos(efectiva);
            if (vector.Count == 0)
            {
                resultado.Errores.Add("La ecuacion no tiene vector de cargos.");
                return resultado;
            }

            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();
            foreach (CargoVector cargoVector in vector)
            {
                ResultadoCargo item = EvaluarCargo(efectiva, cargoVector, cantidad, unidad, diasHabilesSemana, cargos);
                resultado.Cargos.Add(item);
                if (!item.CargoExiste)
                {
                    resultado.Errores.Add(efectiva.Clave + ": cargo no encontrado: " + item.CargoSolicitado);
                }
                else if (!item.RendimientoExiste)
                {
                    if (item.RequiereRendimientoProductivo)
                    {
                        resultado.Advertencias.Add(efectiva.Clave + ": sin rendimiento para " + item.CargoResuelto + " / " + unidad + ".");
                    }
                }
            }

            double diasBaseProductiva = resultado.Cargos
                .Where(c => c.CargoExiste && c.RendimientoExiste && c.RequiereRendimientoProductivo)
                .Select(c => c.DiasTecnicos)
                .DefaultIfEmpty(0.0)
                .Max();

            foreach (ResultadoCargo item in resultado.Cargos.Where(c => c.CargoExiste && !c.RequiereRendimientoProductivo))
            {
                item.RendimientoExiste = true;
                item.DiasTecnicos = diasBaseProductiva * item.Dedicacion;
                item.HorasTecnicas = item.DiasTecnicos * item.HorasPorDia;
                item.CostoCLP = item.HorasTecnicas * item.TarifaHoraCLP;
                item.Diagnostico = diasBaseProductiva <= 0.0
                    ? "Cargo de gestion/apoyo. Espera un cargo productivo para estimar dedicacion."
                    : "Cargo de gestion/apoyo. Costo estimado por dedicacion sobre el cuello de botella productivo.";
            }

            resultado.DiasTecnicos = resultado.Cargos
                .Where(c => c.CargoExiste && c.RendimientoExiste && c.RequiereRendimientoProductivo)
                .Select(c => c.DiasTecnicos)
                .DefaultIfEmpty(0.0)
                .Max();
            resultado.CostoCLP = resultado.Cargos.Sum(c => c.CostoCLP);

            if (resultado.DiasTecnicos <= 0.0)
            {
                resultado.Advertencias.Add("La prueba no pudo calcular dias tecnicos porque faltan rendimientos/capacidades.");
            }

            return resultado;
        }

        public static ResultadoPrueba EvaluarRequerimiento(
            EcuacionProductivaDefinicion ecuacion,
            IEnumerable<EcuacionProductivaDefinicion> biblioteca,
            RequerimientoProduccionInterna requerimiento,
            double diasHabilesSemana
        )
        {
            ResultadoPrueba resultado = new ResultadoPrueba();
            if (requerimiento == null)
            {
                resultado.Errores.Add("No hay requerimiento productivo para evaluar.");
                return resultado;
            }

            if (ecuacion == null)
            {
                resultado.Errores.Add("La fila no tiene una ecuacion productiva enlazada.");
                resultado.Clave = requerimiento.EcuacionUsada ?? "";
                resultado.Nombre = requerimiento.NombreRequerimiento ?? "";
                resultado.CantidadPrueba = requerimiento.Cantidad;
                resultado.UnidadPrueba = requerimiento.Unidad ?? "";
                return resultado;
            }

            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;
            resultado.CantidadPrueba = requerimiento.Cantidad;
            resultado.UnidadPrueba = requerimiento.Unidad ?? "";

            if (resultado.CantidadPrueba <= 0.0)
            {
                resultado.Errores.Add("Cantidad o duracion no definida en la fila.");
                return resultado;
            }

            if (string.IsNullOrWhiteSpace(resultado.UnidadPrueba))
            {
                resultado.Errores.Add("Unidad no definida en la fila.");
                return resultado;
            }

            List<CargoVector> vector = ObtenerVectorCargos(efectiva);
            if (vector.Count == 0 && !string.IsNullOrWhiteSpace(requerimiento.CargoSugerido))
            {
                vector = ParsearVectorCargos(requerimiento.CargoSugerido);
                resultado.Advertencias.Add(
                    "La ecuacion no trae cargos propios; se usaron los cargos guardados en la fila del desglose."
                );
            }

            if (vector.Count == 0)
            {
                resultado.Errores.Add("No hay cargos participantes definidos para esta ecuacion.");
                return resultado;
            }

            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();
            foreach (CargoVector cargoVector in vector)
            {
                ResultadoCargo item = EvaluarCargo(
                    efectiva,
                    cargoVector,
                    resultado.CantidadPrueba,
                    resultado.UnidadPrueba,
                    diasHabilesSemana,
                    cargos
                );

                resultado.Cargos.Add(item);
                if (!item.CargoExiste)
                {
                    resultado.Errores.Add(efectiva.Clave + ": cargo no encontrado: " + item.CargoSolicitado);
                }
                else if (!item.RendimientoExiste && item.RequiereRendimientoProductivo)
                {
                    resultado.Advertencias.Add(
                        efectiva.Clave + ": sin rendimiento compatible para " +
                        item.CargoResuelto + " / " + resultado.UnidadPrueba + "."
                    );
                }
            }

            double diasBaseProductiva = resultado.Cargos
                .Where(c => c.CargoExiste && c.RendimientoExiste && c.RequiereRendimientoProductivo)
                .Select(c => c.DiasTecnicos)
                .DefaultIfEmpty(0.0)
                .Max();

            foreach (ResultadoCargo item in resultado.Cargos.Where(c => c.CargoExiste && !c.RequiereRendimientoProductivo))
            {
                item.RendimientoExiste = true;
                item.DiasTecnicos = diasBaseProductiva * item.Dedicacion;
                item.HorasTecnicas = item.DiasTecnicos * item.HorasPorDia;
                item.CostoCLP = item.HorasTecnicas * item.TarifaHoraCLP;
                item.Diagnostico = diasBaseProductiva <= 0.0
                    ? "Cargo de gestion/apoyo. Espera un cargo productivo para estimar dedicacion."
                    : "Cargo de gestion/apoyo. Dedicacion calculada sobre el cuello de botella productivo.";
            }

            resultado.DiasTecnicos = resultado.Cargos
                .Where(c => c.CargoExiste && c.RendimientoExiste && c.RequiereRendimientoProductivo)
                .Select(c => c.DiasTecnicos)
                .DefaultIfEmpty(0.0)
                .Max();
            resultado.CostoCLP = resultado.Cargos.Sum(c => c.CostoCLP);

            if (resultado.DiasTecnicos <= 0.0)
            {
                resultado.Advertencias.Add("No se calcularon dias tecnicos desde la ecuacion; revisa rendimiento/capacidad/cargos.");
            }

            return resultado;
        }

        public static DiagnosticoBiblioteca ValidarBiblioteca(
            IEnumerable<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            DiagnosticoBiblioteca diagnostico = new DiagnosticoBiblioteca();
            List<EcuacionProductivaDefinicion> lista = (ecuaciones ?? Enumerable.Empty<EcuacionProductivaDefinicion>())
                .Where(e => e != null)
                .ToList();

            Dictionary<string, int> claves = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> bases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();

            foreach (EcuacionProductivaDefinicion e in lista)
            {
                string clave = (e.Clave ?? "").Trim();
                bool esBase = EsBase(e);

                if (string.IsNullOrWhiteSpace(clave))
                {
                    diagnostico.Errores.Add("Hay una ecuacion con clave vacia.");
                    continue;
                }

                if (!claves.ContainsKey(clave))
                {
                    claves[clave] = 0;
                }
                claves[clave]++;

                if (esBase)
                {
                    bases.Add(clave);
                }

                if (string.IsNullOrWhiteSpace(e.NombreVisible))
                {
                    diagnostico.Errores.Add(clave + ": nombre visible vacio.");
                }

                if (e.SchemaVersion < 2)
                {
                    diagnostico.Advertencias.Add(clave + ": esquema anterior; se migrara a proceso productivo versionado al guardar.");
                }

                if (string.IsNullOrWhiteSpace(e.IdProceso))
                {
                    diagnostico.Errores.Add(clave + ": proceso sin Id estable.");
                }

                if (e.TipoProceso == TipoProcesoProductivo.NoClasificado)
                {
                    diagnostico.Errores.Add(clave + ": proceso sin TipoProceso.");
                }

                if (e.MetodoCalculo == MetodoCalculoProceso.NoDefinido)
                {
                    diagnostico.Advertencias.Add(clave + ": proceso sin MetodoCalculo.");
                }

                if (e.AlcanceTemporal == AlcanceTemporalProceso.NoDefinido)
                {
                    diagnostico.Advertencias.Add(clave + ": proceso sin AlcanceTemporal.");
                }

                if (string.IsNullOrWhiteSpace(e.FormulaId))
                {
                    diagnostico.Advertencias.Add(clave + ": proceso sin FormulaId explicito.");
                }

                if ((e.TipoProceso == TipoProcesoProductivo.RevisionControl ||
                     e.TipoProceso == TipoProcesoProductivo.CorreccionRetrabajo) &&
                    string.IsNullOrWhiteSpace(e.DependenciasJson))
                {
                    diagnostico.Advertencias.Add(clave + ": revision/correccion sin proceso origen o dependencia explicita.");
                }

                if ((e.TipoProceso == TipoProcesoProductivo.Direccion ||
                     e.TipoProceso == TipoProcesoProductivo.GestionCoordinacion ||
                     e.TipoProceso == TipoProcesoProductivo.Supervision) &&
                    e.AlcanceTemporal == AlcanceTemporalProceso.Item)
                {
                    diagnostico.Advertencias.Add(clave + ": proceso transversal con alcance Item; revisar para evitar duplicar horas por item.");
                }

                if ((e.AlcanceTemporal == AlcanceTemporalProceso.MultiplesEtapas ||
                     e.AlcanceTemporal == AlcanceTemporalProceso.ProyectoCompleto) &&
                    string.IsNullOrWhiteSpace(e.EtapasCubiertasJson))
                {
                    diagnostico.Advertencias.Add(clave + ": proceso transversal sin etapas cubiertas.");
                }

                if (!esBase && string.IsNullOrWhiteSpace(e.EcuacionBase))
                {
                    diagnostico.Errores.Add(clave + ": proceso sin formula madre.");
                }

                if (esBase && !string.IsNullOrWhiteSpace(e.EcuacionBase))
                {
                    diagnostico.Advertencias.Add(clave + ": una formula madre no deberia depender de otra formula madre.");
                }

                if (e.Activa && !esBase && ObtenerVectorCargos(e).Count == 0)
                {
                    diagnostico.Advertencias.Add(clave + ": proceso activo sin vector de cargos.");
                }

                if (e.Activa && string.IsNullOrWhiteSpace(e.Variables))
                {
                    diagnostico.Advertencias.Add(clave + ": variables de entrada no definidas.");
                }

                if (e.Activa && string.IsNullOrWhiteSpace(e.FormulaReferencia))
                {
                    diagnostico.Advertencias.Add(clave + ": formula/calculo visible no definido.");
                }

                foreach (CargoVector cargo in ObtenerVectorCargos(e))
                {
                    if (cargo.Dedicacion < 0.0 || cargo.Dedicacion > 1.0)
                    {
                        diagnostico.Errores.Add(clave + ": dedicacion fuera de rango 0-100% para " + cargo.Cargo + ".");
                    }

                    CategoriaTrabajador cargoBiblioteca = BuscarCargo(cargos, cargo.Cargo);
                    if (cargoBiblioteca == null)
                    {
                        diagnostico.Advertencias.Add(clave + ": cargo no encontrado en cargos.json: " + cargo.Cargo);
                    }
                    else if (cargoBiblioteca.SueldoMensualCLPTipico <= 0.0)
                    {
                        diagnostico.Advertencias.Add(clave + ": cargo sin tarifa: " + cargo.Cargo);
                    }
                }
            }

            foreach (KeyValuePair<string, int> duplicada in claves.Where(c => c.Value > 1))
            {
                diagnostico.Errores.Add("Clave duplicada: " + duplicada.Key + ".");
            }

            foreach (EcuacionProductivaDefinicion e in lista.Where(e => !EsBase(e)))
            {
                if (!string.IsNullOrWhiteSpace(e.EcuacionBase) && !bases.Contains(e.EcuacionBase))
                {
                    diagnostico.Errores.Add(e.Clave + ": formula madre no existe o no es Base: " + e.EcuacionBase);
                }
            }

            foreach (string ciclo in DetectarCiclosDependenciasProceso(lista))
            {
                diagnostico.Errores.Add("Dependencia circular entre procesos: " + ciclo + ".");
            }

            if (diagnostico.Errores.Count == 0)
            {
                diagnostico.Ok.Add("Claves y herencias criticas consistentes.");
            }

            return diagnostico;
        }

        private static ResultadoCargo EvaluarCargo(
            EcuacionProductivaDefinicion ecuacion,
            CargoVector cargoVector,
            double cantidad,
            string unidad,
            double diasHabilesSemana,
            List<CategoriaTrabajador> cargos
        )
        {
            ResultadoCargo resultado = new ResultadoCargo
            {
                CargoSolicitado = cargoVector.Cargo,
                Dedicacion = cargoVector.Dedicacion,
                CantidadPrueba = cantidad,
                UnidadPrueba = unidad
            };

            CategoriaTrabajador cargo = BuscarCargo(cargos, cargoVector.Cargo);
            if (cargo == null)
            {
                resultado.Diagnostico = "Cargo no encontrado en biblioteca de cargos.";
                return resultado;
            }

            resultado.CargoExiste = true;
            resultado.CargoResuelto = cargo.NombreCompleto;
            resultado.TarifaDiaCLP = cargo.SueldoMensualCLPTipico / 22.0;
            resultado.TarifaDiaPonderadaCLP = resultado.TarifaDiaCLP * cargoVector.Dedicacion;
            resultado.HorasPorDia = 8.0;
            resultado.TarifaHoraCLP = resultado.TarifaDiaCLP / resultado.HorasPorDia;
            resultado.TarifaHoraPonderadaCLP = resultado.TarifaHoraCLP * cargoVector.Dedicacion;
            resultado.RequiereRendimientoProductivo = ClasificacionCargosService.RequiereRendimientoProductivo(
                cargo,
                ecuacion.NombreVisible,
                string.IsNullOrWhiteSpace(ecuacion.SubEtapa) ? ecuacion.NombreVisible : ecuacion.SubEtapa,
                ecuacion.Etapa,
                ecuacion.Variables + ";" + ecuacion.Tokens
            );

            if (!resultado.RequiereRendimientoProductivo)
            {
                resultado.RendimientoExiste = true;
                resultado.Diagnostico = "Cargo de gestion/apoyo: no requiere rendimiento productivo.";
                return resultado;
            }

            RequerimientoProduccionInterna req = new RequerimientoProduccionInterna
            {
                EntregableCliente = ecuacion.NombreVisible,
                EcuacionUsada = ecuacion.Clave + " | " + ecuacion.NombreVisible,
                TipoInterno = string.IsNullOrWhiteSpace(ecuacion.SubEtapa) ? ecuacion.NombreVisible : ecuacion.SubEtapa,
                NombreRequerimiento = string.IsNullOrWhiteSpace(ecuacion.SubEtapa) ? ecuacion.NombreVisible : ecuacion.SubEtapa,
                Cantidad = cantidad,
                Unidad = unidad,
                EtapaSugerida = ecuacion.Etapa,
                BloqueProductivo = ecuacion.Etapa,
                CargoSugerido = cargo.NombreCompleto,
                NivelCargoSugerido = cargo.Nivel,
                SueldoMensualCargoCLP = cargo.SueldoMensualCLPTipico,
                TarifaDiaCargoCLP = resultado.TarifaDiaPonderadaCLP
            };

            RendimientoProductivo rendimiento = BibliotecaRendimientosProductivosJsonService.BuscarMejorPara(req);
            if (rendimiento == null)
            {
                resultado.Diagnostico = "No hay rendimiento compatible para cargo/unidad/proceso.";
                return resultado;
            }

            resultado.RendimientoExiste = true;
            resultado.CapacidadPorPeriodo = rendimiento.CantidadPorPeriodo;
            resultado.Periodo = rendimiento.Periodo;
            resultado.DiasPeriodo = BibliotecaRendimientosProductivosJsonService.ObtenerDiasPeriodo(
                rendimiento.Periodo,
                diasHabilesSemana > 0.0 ? diasHabilesSemana : 5.0
            );

            if (resultado.CapacidadPorPeriodo <= 0.0 || resultado.DiasPeriodo <= 0.0)
            {
                resultado.Diagnostico = "Rendimiento encontrado, pero capacidad/periodo no permite calcular.";
                return resultado;
            }

            resultado.DiasTecnicos = (cantidad / resultado.CapacidadPorPeriodo) * resultado.DiasPeriodo;
            resultado.HorasTecnicas = resultado.DiasTecnicos * resultado.HorasPorDia;
            resultado.CostoCLP = resultado.HorasTecnicas * resultado.TarifaHoraPonderadaCLP;
            resultado.Diagnostico = "OK";
            return resultado;
        }

        private static EcuacionProductivaDefinicion ResolverHerenciaSimple(
            EcuacionProductivaDefinicion ecuacion,
            IEnumerable<EcuacionProductivaDefinicion> biblioteca
        )
        {
            EcuacionProductivaDefinicion copia = new EcuacionProductivaDefinicion
            {
                Activa = ecuacion.Activa,
                SchemaVersion = ecuacion.SchemaVersion,
                Clave = ecuacion.Clave,
                IdProceso = ecuacion.IdProceso,
                NombreVisible = ecuacion.NombreVisible,
                TipoProceso = ecuacion.TipoProceso,
                MetodoCalculo = ecuacion.MetodoCalculo,
                AlcanceTemporal = ecuacion.AlcanceTemporal,
                EtapaId = ecuacion.EtapaId,
                SubEtapaId = ecuacion.SubEtapaId,
                FormulaId = ecuacion.FormulaId,
                DependenciasJson = ecuacion.DependenciasJson,
                PuedeEjecutarseEnParalelo = ecuacion.PuedeEjecutarseEnParalelo,
                ReglaActivacionJson = ecuacion.ReglaActivacionJson,
                EtapasCubiertasJson = ecuacion.EtapasCubiertasJson,
                WarningsMigracionJson = ecuacion.WarningsMigracionJson,
                TipoEcuacion = ecuacion.TipoEcuacion,
                EcuacionBase = ecuacion.EcuacionBase,
                Etapa = ecuacion.Etapa,
                SubEtapa = ecuacion.SubEtapa,
                Tokens = ecuacion.Tokens,
                Variables = ecuacion.Variables,
                CargosPermitidos = ecuacion.CargosPermitidos,
                CargosParticipantesJson = ecuacion.CargosParticipantesJson,
                Impacto = ecuacion.Impacto,
                FormulaReferencia = ecuacion.FormulaReferencia,
                Numerador = ecuacion.Numerador,
                Denominador = ecuacion.Denominador,
                Nota = ecuacion.Nota
            };

            if (string.IsNullOrWhiteSpace(copia.EcuacionBase))
            {
                return copia;
            }

            EcuacionProductivaDefinicion madre = (biblioteca ?? Enumerable.Empty<EcuacionProductivaDefinicion>())
                .FirstOrDefault(e => e != null && string.Equals(e.Clave, copia.EcuacionBase, StringComparison.OrdinalIgnoreCase));

            if (madre == null)
            {
                return copia;
            }

            if (string.IsNullOrWhiteSpace(copia.Variables)) copia.Variables = madre.Variables;
            if (string.IsNullOrWhiteSpace(copia.CargosPermitidos)) copia.CargosPermitidos = madre.CargosPermitidos;
            if (string.IsNullOrWhiteSpace(copia.CargosParticipantesJson)) copia.CargosParticipantesJson = madre.CargosParticipantesJson;
            if (string.IsNullOrWhiteSpace(copia.FormulaReferencia)) copia.FormulaReferencia = madre.FormulaReferencia;
            if (string.IsNullOrWhiteSpace(copia.Numerador)) copia.Numerador = madre.Numerador;
            if (string.IsNullOrWhiteSpace(copia.Denominador)) copia.Denominador = madre.Denominador;
            if (string.IsNullOrWhiteSpace(copia.Impacto)) copia.Impacto = madre.Impacto;
            return copia;
        }

        private static void DeterminarEntradaPrueba(
            EcuacionProductivaDefinicion ecuacion,
            out double cantidad,
            out string unidad
        )
        {
            string variables = Normalizar(ecuacion == null ? "" : ecuacion.Variables + ";" + ecuacion.Numerador);
            if (variables.Contains("segundos") || variables.Contains("duracion"))
            {
                cantidad = 30.0;
                unidad = "segundos";
                return;
            }

            if (variables.Contains("minutos"))
            {
                cantidad = 10.0;
                unidad = "minutos";
                return;
            }

            if (variables.Contains("personaje"))
            {
                cantidad = 1.0;
                unidad = "personajes";
                return;
            }

            cantidad = 1.0;
            unidad = "piezas";
        }

        private static CargoVector ParsearCargoVector(string texto)
        {
            string valor = (texto ?? "").Trim();
            string cargo = valor;
            double dedicacion = 1.0;
            int separador = valor.LastIndexOf('|');
            if (separador >= 0)
            {
                cargo = valor.Substring(0, separador).Trim();
                string dedicacionTexto = valor.Substring(separador + 1).Trim();
                if (!double.TryParse(dedicacionTexto, NumberStyles.Float, CultureInfo.InvariantCulture, out dedicacion) &&
                    !double.TryParse(dedicacionTexto, NumberStyles.Float, CultureInfo.CurrentCulture, out dedicacion))
                {
                    dedicacion = 1.0;
                }
            }

            return new CargoVector
            {
                Cargo = cargo,
                Dedicacion = Math.Max(0.0, Math.Min(5.0, dedicacion))
            };
        }

        private static CategoriaTrabajador BuscarCargo(List<CategoriaTrabajador> cargos, string cargoTexto)
        {
            string buscado = Normalizar(ParsearCargoVector(cargoTexto).Cargo);
            if (string.IsNullOrWhiteSpace(buscado))
            {
                return null;
            }

            return cargos.FirstOrDefault(c =>
                c != null &&
                (
                    Normalizar(c.NombreCompleto) == buscado ||
                    Normalizar(c.Nombre) == buscado ||
                    buscado.Contains(Normalizar(c.Nombre)) ||
                    Normalizar(c.NombreCompleto).Contains(buscado)
                ));
        }

        private static bool EsBase(EcuacionProductivaDefinicion ecuacion)
        {
            return string.Equals(ecuacion == null ? "" : ecuacion.TipoEcuacion, "Base", StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> DetectarCiclosDependenciasProceso(
            List<EcuacionProductivaDefinicion> ecuaciones
        )
        {
            Dictionary<string, List<string>> grafo = (ecuaciones ?? new List<EcuacionProductivaDefinicion>())
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.IdProceso))
                .GroupBy(e => e.IdProceso, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => LeerDependenciasProceso(g.First().DependenciasJson),
                    StringComparer.OrdinalIgnoreCase
                );

            List<string> ciclos = new List<string>();
            HashSet<string> visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> pila = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> ruta = new List<string>();

            foreach (string nodo in grafo.Keys)
            {
                VisitarDependenciaProceso(nodo, grafo, visitados, pila, ruta, ciclos);
            }

            return ciclos.Distinct().ToList();
        }

        private static void VisitarDependenciaProceso(
            string nodo,
            Dictionary<string, List<string>> grafo,
            HashSet<string> visitados,
            HashSet<string> pila,
            List<string> ruta,
            List<string> ciclos
        )
        {
            if (pila.Contains(nodo))
            {
                int inicio = ruta.FindIndex(r => string.Equals(r, nodo, StringComparison.OrdinalIgnoreCase));
                ciclos.Add(string.Join(" -> ", ruta.Skip(Math.Max(0, inicio)).Concat(new[] { nodo })));
                return;
            }

            if (visitados.Contains(nodo))
            {
                return;
            }

            visitados.Add(nodo);
            pila.Add(nodo);
            ruta.Add(nodo);

            if (grafo.TryGetValue(nodo, out List<string> dependencias))
            {
                foreach (string dependencia in dependencias.Where(d => grafo.ContainsKey(d)))
                {
                    VisitarDependenciaProceso(dependencia, grafo, visitados, pila, ruta, ciclos);
                }
            }

            pila.Remove(nodo);
            if (ruta.Count > 0)
            {
                ruta.RemoveAt(ruta.Count - 1);
            }
        }

        private static List<string> LeerDependenciasProceso(string json)
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
                return new List<string>();
            }
        }

        private static string Normalizar(string texto)
        {
            string descompuesto = (texto ?? "").Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                .Trim()
                .ToLowerInvariant()
                .Replace("Ã¡", "a")
                .Replace("Ã©", "e")
                .Replace("Ã­", "i")
                .Replace("Ã³", "o")
                .Replace("Ãº", "u")
                .Replace("Ã¼", "u")
                .Replace("Ã±", "n")
                .Replace(" ", "")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}
