using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class PlanCapacidadDesdeDesgloseService
    {
        public static ResultadoCapacidadProyecto Calcular(
            Cotizacion cotizacion,
            ModoDuracionDesglose modo
        )
        {
            ResultadoCapacidadProyecto resultado = new ResultadoCapacidadProyecto();

            if (cotizacion == null)
            {
                resultado.Diagnostico = "No hay cotización activa.";
                return resultado;
            }

            resultado.PlazoClienteSemanas = cotizacion.PlazoClienteSemanas;
            resultado.DiasCalendarioDisponibles = cotizacion.PlazoClienteSemanas * 5.0;

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                resultado.Diagnostico = "No hay desglose productivo para calcular capacidad.";
                return resultado;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                cotizacion.DesgloseProductivo.Requerimientos;

            resultado.DiasPersonaTotales = requerimientos.Sum(r => ObtenerDiasPorModo(r, modo));

            if (resultado.DiasPersonaTotales <= 0.0)
            {
                resultado.Diagnostico = "El desglose no tiene días-persona calculados.";
                return resultado;
            }

            if (resultado.DiasCalendarioDisponibles <= 0.0)
            {
                resultado.Diagnostico =
                    "No hay plazo cliente definido. Se calcula carga, pero no presión de equipo.";
            }
            else
            {
                resultado.FactorPresionGlobal =
                    resultado.DiasPersonaTotales / resultado.DiasCalendarioDisponibles;

                resultado.PersonasEquivalentesGlobales =
                    CalcularPersonasNecesarias(resultado.DiasPersonaTotales, resultado.DiasCalendarioDisponibles, true, 50);
            }

            var gruposEtapa = requerimientos
                .GroupBy(r => ObtenerNombreEtapaVisible(r.EtapaSugerida))
                .OrderBy(g => ObtenerOrdenEtapa(g.Key))
                .ToList();

            foreach (var grupoEtapa in gruposEtapa)
            {
                ResultadoCapacidadEtapa etapa = new ResultadoCapacidadEtapa();

                etapa.Etapa = grupoEtapa.Key;
                etapa.DiasPersonaEtapa = grupoEtapa.Sum(r => ObtenerDiasPorModo(r, modo));

                if (resultado.DiasPersonaTotales > 0.0)
                {
                    etapa.PesoEtapa = etapa.DiasPersonaEtapa / resultado.DiasPersonaTotales;
                }

                if (resultado.DiasCalendarioDisponibles > 0.0)
                {
                    etapa.DiasCalendarioAsignados =
                        resultado.DiasCalendarioDisponibles * etapa.PesoEtapa;

                    if (etapa.DiasCalendarioAsignados < 1.0)
                    {
                        etapa.DiasCalendarioAsignados = 1.0;
                    }

                    etapa.SemanasCalendarioAsignadas = etapa.DiasCalendarioAsignados / 5.0;

                    etapa.FactorPresionEtapa =
                        etapa.DiasPersonaEtapa / etapa.DiasCalendarioAsignados;

                    etapa.PersonasMinimasEtapa =
                        CalcularPersonasNecesarias(
                            etapa.DiasPersonaEtapa,
                            etapa.DiasCalendarioAsignados,
                            true,
                            50
                        );
                }
                else
                {
                    etapa.DiasCalendarioAsignados = etapa.DiasPersonaEtapa;
                    etapa.SemanasCalendarioAsignadas = etapa.DiasPersonaEtapa / 5.0;
                    etapa.FactorPresionEtapa = 1.0;
                    etapa.PersonasMinimasEtapa = 1;
                }

                etapa.Diagnostico = ConstruirDiagnosticoEtapa(etapa);

                etapa.Cargos = CalcularCargosEtapa(
                    grupoEtapa.ToList(),
                    etapa.DiasCalendarioAsignados,
                    modo
                );

                resultado.Etapas.Add(etapa);
            }

            resultado.Diagnostico = ConstruirDiagnosticoProyecto(resultado);

            return resultado;
        }

        private static List<ResultadoCapacidadCargo> CalcularCargosEtapa(
            List<RequerimientoProduccionInterna> requerimientos,
            double diasCalendarioEtapa,
            ModoDuracionDesglose modo
        )
        {
            List<ResultadoCapacidadCargo> cargos = new List<ResultadoCapacidadCargo>();

            if (requerimientos == null || requerimientos.Count == 0)
            {
                return cargos;
            }

            if (diasCalendarioEtapa <= 0.0)
            {
                diasCalendarioEtapa = 1.0;
            }

            var gruposCargo = requerimientos
                .GroupBy(r => NormalizarCargoVisible(r.CargoSugerido))
                .ToList();

            foreach (var grupo in gruposCargo)
            {
                string cargo = grupo.Key;

                double diasCargo = grupo.Sum(r => ObtenerDiasPorModo(r, modo));

                bool paralelizable = CargoPermiteParalelizacion(cargo);

                int maxPersonas = ObtenerMaximoPersonasRazonable(cargo);

                int personas = CalcularPersonasNecesarias(
                    diasCargo,
                    diasCalendarioEtapa,
                    paralelizable,
                    maxPersonas
                );

                ResultadoCapacidadCargo resultado = new ResultadoCapacidadCargo();

                resultado.Etapa = ObtenerNombreEtapaVisible(grupo.First().EtapaSugerida);
                resultado.Cargo = cargo;
                resultado.DiasPersonaCargo = diasCargo;
                resultado.FactorPresionCargo = diasCargo / diasCalendarioEtapa;
                resultado.Paralelizable = paralelizable;
                resultado.MaximoPersonasRazonable = maxPersonas;
                resultado.PersonasSugeridas = personas;
                resultado.PersonasExtra = personas - 1;
                resultado.PrioridadAumento = ObtenerPrioridadAumento(cargo);
                resultado.Nota = ConstruirNotaCargo(resultado);

                cargos.Add(resultado);
            }

            return cargos
                .OrderBy(c => c.PrioridadAumento)
                .ThenByDescending(c => c.PersonasExtra)
                .ThenByDescending(c => c.DiasPersonaCargo)
                .ToList();
        }

        private static double ObtenerDiasPorModo(
            RequerimientoProduccionInterna req,
            ModoDuracionDesglose modo
        )
        {
            if (req == null)
            {
                return 0.0;
            }

            if (modo == ModoDuracionDesglose.Minimo)
            {
                return req.DiasPersonaMin;
            }

            if (modo == ModoDuracionDesglose.Holgura)
            {
                return req.DiasPersonaHolgura;
            }

            return req.DiasPersonaStd;
        }

        private static int CalcularPersonasNecesarias(
            double diasPersona,
            double diasCalendarioDisponibles,
            bool paralelizable,
            int maxPersonas
        )
        {
            if (diasPersona <= 0.0)
            {
                return 1;
            }

            if (diasCalendarioDisponibles <= 0.0)
            {
                return 1;
            }

            if (!paralelizable)
            {
                return 1;
            }

            int personas = (int)Math.Ceiling(diasPersona / diasCalendarioDisponibles);

            if (personas < 1)
            {
                personas = 1;
            }

            if (personas > maxPersonas)
            {
                personas = maxPersonas;
            }

            return personas;
        }

        private static bool CargoPermiteParalelizacion(string cargo)
        {
            string c = Normalizar(cargo);

            if (c.Contains("animador") ||
                c.Contains("animadora") ||
                c.Contains("background") ||
                c.Contains("fondo") ||
                c.Contains("fondos") ||
                c.Contains("ilustrador") ||
                c.Contains("ilustradora") ||
                c.Contains("artistadefondos") ||
                c.Contains("disenadordepersonajes") ||
                c.Contains("characterdesigner") ||
                c.Contains("disenadordeprops") ||
                c.Contains("prop") ||
                c.Contains("cleanup") ||
                c.Contains("fx"))
            {
                return true;
            }

            return false;
        }

        private static int ObtenerMaximoPersonasRazonable(string cargo)
        {
            string c = Normalizar(cargo);

            if (c.Contains("animador") || c.Contains("animadora"))
            {
                return 12;
            }

            if (c.Contains("background") ||
                c.Contains("fondo") ||
                c.Contains("fondos") ||
                c.Contains("ilustrador") ||
                c.Contains("artistadefondos"))
            {
                return 8;
            }

            if (c.Contains("disenadordepersonajes") ||
                c.Contains("characterdesigner"))
            {
                return 4;
            }

            if (c.Contains("prop"))
            {
                return 4;
            }

            if (c.Contains("fx"))
            {
                return 4;
            }

            if (c.Contains("compositor"))
            {
                return 3;
            }

            if (c.Contains("storyboard"))
            {
                return 3;
            }

            return 1;
        }

        private static int ObtenerPrioridadAumento(string cargo)
        {
            string c = Normalizar(cargo);

            /*
             * Menor número = mayor prioridad para sumar gente.
             * Priorizamos donde agregar gente suele reducir plazo sin romper tanto el proceso.
             */

            if (c.Contains("animador") || c.Contains("animadora"))
            {
                return 10;
            }

            if (c.Contains("background") ||
                c.Contains("fondo") ||
                c.Contains("fondos") ||
                c.Contains("artistadefondos") ||
                c.Contains("ilustrador"))
            {
                return 20;
            }

            if (c.Contains("cleanup"))
            {
                return 30;
            }

            if (c.Contains("prop"))
            {
                return 40;
            }

            if (c.Contains("disenadordepersonajes") ||
                c.Contains("characterdesigner"))
            {
                return 50;
            }

            if (c.Contains("fx"))
            {
                return 60;
            }

            if (c.Contains("compositor"))
            {
                return 70;
            }

            if (c.Contains("storyboard"))
            {
                return 80;
            }

            return 999;
        }

        private static string ConstruirDiagnosticoProyecto(ResultadoCapacidadProyecto r)
        {
            if (r == null)
            {
                return "";
            }

            if (r.DiasCalendarioDisponibles <= 0.0)
            {
                return "Sin plazo cliente: se calcula carga productiva, pero no presión de equipo.";
            }

            if (r.FactorPresionGlobal <= 1.0)
            {
                return "PLAZO HOLGADO: el trabajo cabe con una persona equivalente por proceso.";
            }

            if (r.FactorPresionGlobal <= 1.5)
            {
                return "PLAZO AJUSTADO: conviene reforzar cargos paralelizables puntuales.";
            }

            if (r.FactorPresionGlobal <= 2.5)
            {
                return "PLAZO EXIGENTE: se requiere aumentar equipo en producción, especialmente animación y fondos.";
            }

            return "PLAZO CRÍTICO: probablemente requiere más equipo, reducir alcance o renegociar plazo.";
        }

        private static string ConstruirDiagnosticoEtapa(ResultadoCapacidadEtapa etapa)
        {
            if (etapa == null)
            {
                return "";
            }

            if (etapa.FactorPresionEtapa <= 1.0)
            {
                return "Una persona equivalente alcanza para esta etapa.";
            }

            if (etapa.FactorPresionEtapa <= 1.5)
            {
                return "Etapa ajustada; revisar si conviene reforzar algún cargo.";
            }

            if (etapa.FactorPresionEtapa <= 2.5)
            {
                return "Etapa exigente; requiere paralelizar trabajo.";
            }

            return "Etapa crítica; el plazo exige más equipo o reducir alcance.";
        }

        private static string ConstruirNotaCargo(ResultadoCapacidadCargo cargo)
        {
            if (cargo == null)
            {
                return "";
            }

            if (!cargo.Paralelizable && cargo.FactorPresionCargo > 1.0)
            {
                return "Cuello de botella no paralelizable: aumentar personas no reduce linealmente el plazo.";
            }

            if (cargo.PersonasExtra <= 0)
            {
                return "Una persona alcanza para el plazo asignado.";
            }

            return "Sumar " + cargo.PersonasExtra.ToString() +
                   " persona(s) adicional(es) en este cargo para cumplir plazo.";
        }

        private static string NormalizarCargoVisible(string cargo)
        {
            if (string.IsNullOrWhiteSpace(cargo))
            {
                return "Cargo no definido";
            }

            return cargo.Trim();
        }

        private static string ObtenerNombreEtapaVisible(string etapa)
        {
            string e = Normalizar(etapa);

            if (e == "desarrollo")
            {
                return "Desarrollo";
            }

            if (e == "preproduccion")
            {
                return "Preproducción";
            }

            if (e == "produccion")
            {
                return "Producción";
            }

            if (e == "postproduccion")
            {
                return "Postproducción";
            }

            return string.IsNullOrWhiteSpace(etapa) ? "Sin etapa" : etapa;
        }

        private static int ObtenerOrdenEtapa(string etapa)
        {
            string e = Normalizar(etapa);

            if (e == "desarrollo")
            {
                return 10;
            }

            if (e == "preproduccion")
            {
                return 20;
            }

            if (e == "produccion")
            {
                return 30;
            }

            if (e == "postproduccion")
            {
                return 40;
            }

            return 90;
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
                .Replace("ü", "u")
                .Replace("ñ", "n")
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace("/", "")
                .Replace(".", "")
                .Replace(",", "");
        }
    }
}
