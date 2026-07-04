using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class PropuestaPlanificacionDesgloseService
    {
        public static PropuestaVinculacionDesglose Construir(
            Cotizacion cotizacion,
            List<SubEtapaProyecto> bibliotecaSubEtapas,
            ModoDuracionDesglose modo
        )
        {
            PropuestaVinculacionDesglose propuesta =
                new PropuestaVinculacionDesglose();

            if (cotizacion == null)
            {
                propuesta.Diagnostico = "No hay cotización activa.";
                return propuesta;
            }

            if (cotizacion.DesgloseProductivo == null ||
                cotizacion.DesgloseProductivo.Requerimientos == null ||
                cotizacion.DesgloseProductivo.Requerimientos.Count == 0)
            {
                propuesta.Diagnostico = "No hay desglose productivo para vincular.";
                return propuesta;
            }

            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                propuesta.Diagnostico = "No hay biblioteca de subetapas disponible.";
                return propuesta;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                cotizacion.DesgloseProductivo.Requerimientos;

            propuesta.DiasPersonaTotales =
                requerimientos.Sum(r => ObtenerDiasPorModo(r, modo));

            var gruposEtapa = requerimientos
                .GroupBy(r => ObtenerNombreEtapaVisible(r.EtapaSugerida))
                .OrderBy(g => ObtenerOrdenEtapa(g.Key))
                .ToList();

            foreach (var grupoEtapa in gruposEtapa)
            {
                PropuestaEtapaDesglose etapa =
                    new PropuestaEtapaDesglose();

                etapa.Etapa = grupoEtapa.Key;
                etapa.DiasPersona = grupoEtapa.Sum(r => ObtenerDiasPorModo(r, modo));
                etapa.SemanasSugeridas = CalcularSemanas(etapa.DiasPersona);

                List<SubEtapaProyecto> subEtapasEtapa = bibliotecaSubEtapas
                    .Where(s =>
                        s != null &&
                        Normalizar(s.EtapaPadre) == Normalizar(etapa.Etapa))
                    .OrderBy(s => s.Orden)
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasEtapa)
                {
                    List<RequerimientoProduccionInterna> vinculados = grupoEtapa
                        .Where(r => SubEtapaConversaConRequerimiento(sub, r))
                        .ToList();

                    if (vinculados.Count == 0)
                    {
                        continue;
                    }

                    PropuestaSubEtapaDesglose propuestaSub =
                        new PropuestaSubEtapaDesglose();

                    propuestaSub.EtapaPadre = etapa.Etapa;
                    propuestaSub.NombreSubEtapa = sub.Nombre;
                    propuestaSub.Estado = sub.Requerida ? "Requerida" : "Sugerida";
                    propuestaSub.DiasPersona = vinculados.Sum(r => ObtenerDiasPorModo(r, modo));
                    propuestaSub.SemanasSugeridas = CalcularSemanas(propuestaSub.DiasPersona);
                    propuestaSub.Justificacion = ConstruirJustificacion(vinculados);

                    foreach (RequerimientoProduccionInterna req in vinculados)
                    {
                        propuestaSub.Requerimientos.Add(
                            new PropuestaRequerimientoVinculado
                            {
                                EntregableCliente = req.EntregableCliente,
                                TipoInterno = req.TipoInterno,
                                NombreRequerimiento = req.NombreRequerimiento,
                                Cantidad = req.Cantidad,
                                Unidad = req.Unidad,
                                CargoSugerido = req.CargoSugerido,
                                DiasPersona = ObtenerDiasPorModo(req, modo)
                            }
                        );
                    }

                    etapa.SubEtapas.Add(propuestaSub);
                }

                propuesta.Etapas.Add(etapa);
            }

            propuesta.Diagnostico = ConstruirDiagnostico(propuesta);

            return propuesta;
        }

        private static bool SubEtapaConversaConRequerimiento(
            SubEtapaProyecto sub,
            RequerimientoProduccionInterna req
        )
        {
            if (sub == null || req == null)
            {
                return false;
            }

            string textoReq = Normalizar(
                req.TipoInterno + " " +
                req.NombreRequerimiento + " " +
                req.Unidad + " " +
                req.CargoSugerido
            );

            string produce = Normalizar(sub.TiposInternosQueProduce);
            string consume = Normalizar(sub.TiposInternosQueConsume);
            string claves = Normalizar(sub.PalabrasClaveActivacion);
            string nombreSub = Normalizar(sub.Nombre);
            string entrega = Normalizar(sub.Entrega);
            string requiere = Normalizar(sub.Requiere);

            if (CoincidePorTokens(textoReq, produce))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, consume))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, claves))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, nombreSub))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, entrega))
            {
                return true;
            }

            if (CoincidePorTokens(textoReq, requiere))
            {
                return true;
            }

            return false;
        }

        private static bool CoincidePorTokens(string textoReq, string textoSubEtapa)
        {
            if (string.IsNullOrWhiteSpace(textoReq) ||
                string.IsNullOrWhiteSpace(textoSubEtapa))
            {
                return false;
            }

            string[] tokens = textoSubEtapa
                .Split(new[] { ';', ',', '|', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Normalizar(t))
                .Where(t => t.Length >= 3)
                .Distinct()
                .ToArray();

            foreach (string token in tokens)
            {
                if (textoReq.Contains(token))
                {
                    return true;
                }
            }

            return false;
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

        private static double CalcularSemanas(double diasPersona)
        {
            if (diasPersona <= 0.0)
            {
                return 0.0;
            }

            return diasPersona / 5.0;
        }

        private static string ConstruirJustificacion(
            List<RequerimientoProduccionInterna> requerimientos
        )
        {
            if (requerimientos == null || requerimientos.Count == 0)
            {
                return "";
            }

            return string.Join(
                "; ",
                requerimientos
                    .Select(r =>
                        r.NombreRequerimiento +
                        " (" +
                        r.Cantidad.ToString("0.##") +
                        " " +
                        r.Unidad +
                        ")"
                    )
                    .Distinct()
            );
        }

        private static string ConstruirDiagnostico(
            PropuestaVinculacionDesglose propuesta
        )
        {
            if (propuesta == null || propuesta.Etapas == null)
            {
                return "";
            }

            int totalSubEtapas = propuesta.Etapas.Sum(e => e.SubEtapas.Count);

            if (totalSubEtapas == 0)
            {
                return "El desglose existe, pero no se encontraron subetapas vinculadas. Revisa tokens de subetapas.";
            }

            return "Se construyó una propuesta global de subetapas basada en los requerimientos internos del desglose productivo.";
        }

        private static string ObtenerNombreEtapaVisible(string etapa)
        {
            string e = Normalizar(etapa);

            if (e.Contains("desarrollo"))
            {
                return "Desarrollo";
            }

            if (e.Contains("preproduccion"))
            {
                return "Preproducción";
            }

            if (e.Contains("postproduccion"))
            {
                return "Postproducción";
            }

            if (e.Contains("produccion"))
            {
                return "Producción";
            }

            return string.IsNullOrWhiteSpace(etapa) ? "Sin etapa" : etapa;
        }

        private static int ObtenerOrdenEtapa(string etapa)
        {
            string e = Normalizar(etapa);

            if (e.Contains("desarrollo"))
            {
                return 10;
            }

            if (e.Contains("preproduccion"))
            {
                return 20;
            }

            if (e.Contains("postproduccion"))
            {
                return 40;
            }

            if (e.Contains("produccion"))
            {
                return 30;
            }

            return 99;
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
                .Replace("-", " ")
                .Replace("_", " ");
        }
    }
}