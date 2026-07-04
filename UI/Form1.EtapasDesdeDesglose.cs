using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private const double DiasHabilesPorMesPlanificacionInterna = 20.0;
        private const double DuracionMinimaEtapaMesesInterna = 0.05;

        private readonly string[] ordenEtapasProductivasInternas =
        {
            "Desarrollo",
            "Preproduccion",
            "Produccion",
            "Postproduccion"
        };

        /// <summary>
        /// Nueva regla:
        /// La pestaña Etapas deja de ser fuente de verdad.
        /// Las etapas se recalculan internamente desde el Desglose Productivo,
        /// para alimentar resumen, resultados e idealmente el Gantt.
        /// </summary>
        private void SincronizarEtapasInternasDesdeDesgloseProductivo()
        {
            if (cotizacion == null)
            {
                return;
            }

            AsegurarEtapasBaseInternas();

            List<object> requerimientos = ObtenerRequerimientosProductivosComoObjetos();

            if (requerimientos.Count == 0)
            {
                DesactivarEtapasInternasSinDesglose();
                return;
            }

            var resumenPorEtapa = ordenEtapasProductivasInternas
                .ToDictionary(
                    etapa => etapa,
                    etapa => new ResumenEtapaInterna
                    {
                        Etapa = etapa
                    }
                );

            foreach (object req in requerimientos)
            {
                if (req == null)
                {
                    continue;
                }

                string etapa = LeerStringPropiedad(
                    req,
                    "EtapaSugerida",
                    "Etapa",
                    "EtapaProductiva",
                    "Categoria",
                    "Bloque"
                );

                etapa = NormalizarEtapaProductivaInterna(etapa);

                if (!resumenPorEtapa.ContainsKey(etapa))
                {
                    resumenPorEtapa[etapa] = new ResumenEtapaInterna
                    {
                        Etapa = etapa
                    };
                }

                double diasMin = LeerDoublePropiedad(
                    req,
                    "DiasPersonaMin",
                    "DiasMin",
                    "DiasMinimo",
                    "DiasMinimos",
                    "DiasEstimadosMin"
                );

                double diasStd = LeerDoublePropiedad(
                    req,
                    "DiasPersonaStd",
                    "DiasPersonaEstandar",
                    "DiasStd",
                    "DiasEstandar",
                    "DiasEstimados",
                    "DiasEstimadosStd"
                );

                double diasHolgura = LeerDoublePropiedad(
                    req,
                    "DiasPersonaHolgura",
                    "DiasHolgura",
                    "DiasMax",
                    "DiasMaximo",
                    "DiasMaximos",
                    "DiasEstimadosHolgura"
                );

                double costoMin = LeerDoublePropiedad(
                    req,
                    "CostoMinimoCLP",
                    "CostoMinCLP",
                    "CostoMinimo",
                    "CostoMin"
                );

                double costoStd = LeerDoublePropiedad(
                    req,
                    "CostoEstandarCLP",
                    "CostoStdCLP",
                    "CostoEstandar",
                    "CostoStd",
                    "CostoTotalCLP",
                    "CostoTotal"
                );

                double costoHolgura = LeerDoublePropiedad(
                    req,
                    "CostoHolguraCLP",
                    "CostoMaximoCLP",
                    "CostoMaxCLP",
                    "CostoHolgura",
                    "CostoMaximo",
                    "CostoMax"
                );

                if (diasStd <= 0.0)
                {
                    diasStd = diasMin > 0.0 ? diasMin : diasHolgura;
                }

                if (diasMin <= 0.0)
                {
                    diasMin = diasStd;
                }

                if (diasHolgura <= 0.0)
                {
                    diasHolgura = diasStd;
                }

                if (costoStd <= 0.0)
                {
                    costoStd = costoMin > 0.0 ? costoMin : costoHolgura;
                }

                if (costoMin <= 0.0)
                {
                    costoMin = costoStd;
                }

                if (costoHolgura <= 0.0)
                {
                    costoHolgura = costoStd;
                }

                string cargoSugerido = LeerStringPropiedad(
                    req,
                    "CargoSugerido",
                    "Cargo",
                    "CargoAsociado",
                    "CargoRequerido"
                );

                resumenPorEtapa[etapa].DiasMin += diasMin;
                resumenPorEtapa[etapa].DiasStd += diasStd;
                resumenPorEtapa[etapa].DiasHolgura += diasHolgura;
                resumenPorEtapa[etapa].AgregarDiasPorCargo(cargoSugerido, diasStd);

                resumenPorEtapa[etapa].CostoMin += costoMin;
                resumenPorEtapa[etapa].CostoStd += costoStd;
                resumenPorEtapa[etapa].CostoHolgura += costoHolgura;
            }

            double inicioActualMes = 0.0;

            foreach (string etapaNombre in ordenEtapasProductivasInternas)
            {
                ResumenEtapaInterna resumen = resumenPorEtapa[etapaNombre];

                EtapaProyecto etapa = ObtenerOCrearEtapaInterna(etapaNombre);

                if (resumen.DiasStd <= 0.0 && resumen.CostoStd <= 0.0)
                {
                    etapa.Seleccionada = false;
                    etapa.DuracionMeses = 0.0;
                    etapa.InicioMes = 0.0;
                    etapa.FinMes = 0.0;
                    etapa.CostoTotal = 0.0;

                    if (etapa.Plan != null)
                    {
                        etapa.Plan.Clear();
                    }

                    continue;
                }

                double duracionDiasCalendario =
                    CalcularDiasCalendarioEtapaSegunManoObra(etapaNombre, resumen);

                double duracionMeses = duracionDiasCalendario / DiasHabilesPorMesPlanificacionInterna;

                if (duracionMeses < DuracionMinimaEtapaMesesInterna)
                {
                    duracionMeses = DuracionMinimaEtapaMesesInterna;
                }

                etapa.Seleccionada = true;
                etapa.InicioMes = inicioActualMes;
                etapa.DuracionMeses = duracionMeses;
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                etapa.CostoTotal = resumen.CostoStd;

                inicioActualMes = etapa.FinMes;
            }

            ValidarOrdenInicioEtapas();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            RefrescarGanttGrandeEtapas();
        }

        private void AsegurarEtapasBaseInternas()
        {
            if (cotizacion.Etapas == null)
            {
                cotizacion.Etapas = new List<EtapaProyecto>();
            }

            foreach (string etapa in ordenEtapasProductivasInternas)
            {
                ObtenerOCrearEtapaInterna(etapa);
            }
        }

        private EtapaProyecto ObtenerOCrearEtapaInterna(string nombreEtapa)
        {
            string normalizada = NormalizarEtapaProductivaInterna(nombreEtapa);

            EtapaProyecto etapa = cotizacion.Etapas.FirstOrDefault(e =>
                e != null &&
                NormalizarEtapaProductivaInterna(e.Nombre) == normalizada
            );

            if (etapa != null)
            {
                etapa.Nombre = normalizada;
                return etapa;
            }

            etapa = new EtapaProyecto
            {
                Nombre = normalizada,
                Seleccionada = false,
                InicioMes = 0.0,
                DuracionMeses = 0.0,
                FinMes = 0.0,
                CostoTotal = 0.0
            };

            cotizacion.Etapas.Add(etapa);

            return etapa;
        }

        private void DesactivarEtapasInternasSinDesglose()
        {
            AsegurarEtapasBaseInternas();

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                etapa.Seleccionada = false;
                etapa.InicioMes = 0.0;
                etapa.DuracionMeses = 0.0;
                etapa.FinMes = 0.0;
                etapa.CostoTotal = 0.0;

                if (etapa.Plan != null)
                {
                    etapa.Plan.Clear();
                }
            }

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            RefrescarGanttGrandeEtapas();
        }

        private List<object> ObtenerRequerimientosProductivosComoObjetos()
        {
            List<object> lista = new List<object>();

            if (cotizacion == null || cotizacion.DesgloseProductivo == null)
            {
                return lista;
            }

            object desglose = cotizacion.DesgloseProductivo;

            string[] posiblesPropiedades =
            {
                "Requerimientos",
                "RequerimientosInternos",
                "RequerimientosProduccion",
                "RequerimientosProductivos",
                "Items",
                "Lineas",
                "Filas"
            };

            Type tipo = desglose.GetType();

            foreach (string nombrePropiedad in posiblesPropiedades)
            {
                PropertyInfo prop = tipo.GetProperty(nombrePropiedad);

                if (prop == null)
                {
                    continue;
                }

                object valor = prop.GetValue(desglose, null);

                if (valor == null || valor is string)
                {
                    continue;
                }

                IEnumerable enumerable = valor as IEnumerable;

                if (enumerable == null)
                {
                    continue;
                }

                foreach (object item in enumerable)
                {
                    if (item != null)
                    {
                        lista.Add(item);
                    }
                }

                if (lista.Count > 0)
                {
                    return lista;
                }
            }

            return lista;
        }

        private string NormalizarEtapaProductivaInterna(string etapa)
        {
            string n = NormalizarTextoInterno(etapa);

            if (n.Contains("desarrollo"))
            {
                return "Desarrollo";
            }

            if (n.Contains("preproduccion") || n.Contains("preproduccion"))
            {
                return "Preproduccion";
            }

            /*
             * Postproducción antes que Producción:
             * "postproduccion" contiene "produccion".
             */
            if (n.Contains("postproduccion"))
            {
                return "Postproduccion";
            }

            if (n.Contains("produccion"))
            {
                return "Produccion";
            }

            return "Produccion";
        }

        private string LeerStringPropiedad(object obj, params string[] nombres)
        {
            if (obj == null)
            {
                return "";
            }

            Type tipo = obj.GetType();

            foreach (string nombre in nombres)
            {
                PropertyInfo prop = tipo.GetProperty(nombre);

                if (prop == null)
                {
                    continue;
                }

                object valor = prop.GetValue(obj, null);

                if (valor == null)
                {
                    continue;
                }

                return valor.ToString() ?? "";
            }

            return "";
        }

        private double LeerDoublePropiedad(object obj, params string[] nombres)
        {
            if (obj == null)
            {
                return 0.0;
            }

            Type tipo = obj.GetType();

            foreach (string nombre in nombres)
            {
                PropertyInfo prop = tipo.GetProperty(nombre);

                if (prop == null)
                {
                    continue;
                }

                object valor = prop.GetValue(obj, null);

                double convertido = ConvertirObjetoADoubleInterno(valor);

                if (convertido > 0.0)
                {
                    return convertido;
                }
            }

            return 0.0;
        }

        private double ConvertirObjetoADoubleInterno(object valor)
        {
            if (valor == null)
            {
                return 0.0;
            }

            if (valor is double)
            {
                return (double)valor;
            }

            if (valor is float)
            {
                return Convert.ToDouble(valor);
            }

            if (valor is decimal)
            {
                return Convert.ToDouble(valor);
            }

            if (valor is int)
            {
                return Convert.ToDouble(valor);
            }

            if (valor is long)
            {
                return Convert.ToDouble(valor);
            }

            string texto = valor.ToString();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return 0.0;
            }

            texto = texto
                .Trim()
                .Replace("$", "")
                .Replace(".", "")
                .Replace(",", ".");

            double resultado;

            if (double.TryParse(
                texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out resultado))
            {
                return resultado;
            }

            return 0.0;
        }

        private string NormalizarTextoInterno(string texto)
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
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "");
        }

        private double CalcularDiasCalendarioEtapaSegunManoObra(
            string etapa,
            ResumenEtapaInterna resumen
        )
        {
            if (resumen == null || resumen.DiasStd <= 0.0)
            {
                return 0.0;
            }

            if (resumen.DiasStdPorCargo == null || resumen.DiasStdPorCargo.Count == 0)
            {
                return resumen.DiasStd;
            }

            double cuelloBotellaDias = 0.0;

            foreach (KeyValuePair<string, double> cargaCargo in resumen.DiasStdPorCargo)
            {
                if (cargaCargo.Value <= 0.0)
                {
                    continue;
                }

                double personas = ObtenerPersonasDisponiblesManoObra(etapa, cargaCargo.Key);
                double diasCalendarioCargo = cargaCargo.Value / Math.Max(1.0, personas);

                cuelloBotellaDias = Math.Max(cuelloBotellaDias, diasCalendarioCargo);
            }

            if (cuelloBotellaDias <= 0.0)
            {
                return resumen.DiasStd;
            }

            return cuelloBotellaDias;
        }

        private double CalcularDiasCalendarioRequerimientoSegunManoObra(
            string etapa,
            string cargo,
            double diasPersona
        )
        {
            if (diasPersona <= 0.0)
            {
                return 0.0;
            }

            double personas = ObtenerPersonasDisponiblesManoObra(etapa, cargo);
            return diasPersona / Math.Max(1.0, personas);
        }

        private double ObtenerPersonasDisponiblesManoObra(string etapa, string cargo)
        {
            double personas = 0.0;

            personas += ObtenerPersonasDesdePlanGeneral(cargo);
            personas += ObtenerPersonasDesdePlanEtapa(etapa, cargo);

            if (personas <= 0.0)
            {
                return 1.0;
            }

            return personas;
        }

        private double ObtenerPersonasDesdePlanGeneral(string cargo)
        {
            if (cotizacion == null || cotizacion.PlanGeneralManoObra == null)
            {
                return 0.0;
            }

            return cotizacion.PlanGeneralManoObra
                .Where(plan => CargoPlanCoincideConTexto(plan, cargo))
                .Select(ObtenerMaximoPersonasPlan)
                .DefaultIfEmpty(0.0)
                .Sum();
        }

        private double ObtenerPersonasDesdePlanEtapa(string etapa, string cargo)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return 0.0;
            }

            string etapaNormalizada = NormalizarEtapaProductivaInterna(etapa);

            return cotizacion.Etapas
                .Where(e =>
                    e != null &&
                    e.Plan != null &&
                    NormalizarEtapaProductivaInterna(e.Nombre) == etapaNormalizada)
                .SelectMany(e => e.Plan)
                .Where(plan => CargoPlanCoincideConTexto(plan, cargo))
                .Select(ObtenerMaximoPersonasPlan)
                .DefaultIfEmpty(0.0)
                .Sum();
        }

        private double ObtenerMaximoPersonasPlan(CargoPlanMensual plan)
        {
            if (plan == null || plan.PersonasPorBloque == null || plan.PersonasPorBloque.Count == 0)
            {
                return 0.0;
            }

            return plan.PersonasPorBloque
                .Select(p => Math.Max(0.0, p))
                .DefaultIfEmpty(0.0)
                .Max();
        }

        private bool CargoPlanCoincideConTexto(CargoPlanMensual plan, string cargoTexto)
        {
            if (plan == null || plan.Categoria == null)
            {
                return false;
            }

            List<string> tokensCargo = SepararCargosInternos(cargoTexto)
                .Select(NormalizarTextoInterno)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (tokensCargo.Count == 0)
            {
                return false;
            }

            string nombre = NormalizarTextoInterno(plan.Categoria.Nombre);
            string nombreCompleto = NormalizarTextoInterno(plan.Categoria.NombreCompleto);

            foreach (string token in tokensCargo)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (nombre == token ||
                    nombreCompleto == token ||
                    nombre.Contains(token) ||
                    nombreCompleto.Contains(token) ||
                    token.Contains(nombre))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> SepararCargosInternos(string cargos)
        {
            return (cargos ?? "")
                .Replace(" y ", ";")
                .Split(new[] { ';', '|', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();
        }

        private class ResumenEtapaInterna
        {
            public string Etapa { get; set; } = "";

            public double DiasMin { get; set; }
            public double DiasStd { get; set; }
            public double DiasHolgura { get; set; }

            public double CostoMin { get; set; }
            public double CostoStd { get; set; }
            public double CostoHolgura { get; set; }

            public Dictionary<string, double> DiasStdPorCargo { get; set; } =
                new Dictionary<string, double>();

            public void AgregarDiasPorCargo(string cargo, double diasStd)
            {
                if (diasStd <= 0.0)
                {
                    return;
                }

                string clave = string.IsNullOrWhiteSpace(cargo)
                    ? "Cargo no definido"
                    : cargo.Trim();

                if (!DiasStdPorCargo.ContainsKey(clave))
                {
                    DiasStdPorCargo[clave] = 0.0;
                }

                DiasStdPorCargo[clave] += diasStd;
            }
        }
    }
}
