using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class CalculoProductivoResolverService
    {
        public const double HorasDiaEstandar = 8.0;

        public static bool Aplicar(
            RequerimientoProduccionInterna req,
            double diasHabilesSemana
        )
        {
            if (req == null)
            {
                return false;
            }

            req.ModoCalculoProductivo =
                ModosCalculoProductivo.Normalizar(req.ModoCalculoProductivo);

            if (ModosCalculoProductivo.EsTiempoAsignado(req.ModoCalculoProductivo))
            {
                return AplicarTiempoAsignado(req);
            }

            return AplicarRendimiento(req, diasHabilesSemana);
        }

        public static bool AplicarTiempoAsignado(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return false;
            }

            req.ModoCalculoProductivo = ModosCalculoProductivo.TiempoAsignado;
            req.HorasMinimas = Math.Max(0.0, req.HorasMinimas);
            req.HorasEstandar = Math.Max(0.0, req.HorasEstandar);
            req.HorasHolgura = Math.Max(0.0, req.HorasHolgura);

            if (req.HorasEstandar <= 0.0)
            {
                SincronizarHorasDesdeDias(req);
            }

            if (req.HorasEstandar <= 0.0)
            {
                return false;
            }

            if (req.HorasMinimas <= 0.0)
            {
                req.HorasMinimas = req.HorasEstandar;
            }

            if (req.HorasHolgura <= 0.0)
            {
                req.HorasHolgura = req.HorasEstandar;
            }

            req.DiasPersonaMin = req.HorasMinimas / HorasDiaEstandar;
            req.DiasPersonaStd = req.HorasEstandar / HorasDiaEstandar;
            req.DiasPersonaHolgura = req.HorasHolgura / HorasDiaEstandar;

            req.RendimientoCantidad = 0.0;
            req.RendimientoPeriodo = "";
            req.RendimientoOrigen = "Tiempo asignado";
            req.OrigenHoras = "Tiempo asignado";

            RecalcularCostosDesdeDias(req);
            return true;
        }

        public static void SincronizarHorasDesdeDias(RequerimientoProduccionInterna req)
        {
            if (req == null)
            {
                return;
            }

            req.HorasMinimas = Math.Max(0.0, req.DiasPersonaMin) * HorasDiaEstandar;
            req.HorasEstandar = Math.Max(0.0, req.DiasPersonaStd) * HorasDiaEstandar;
            req.HorasHolgura = Math.Max(0.0, req.DiasPersonaHolgura) * HorasDiaEstandar;
        }

        private static bool AplicarRendimiento(
            RequerimientoProduccionInterna req,
            double diasHabilesSemana
        )
        {
            bool aplicado =
                BibliotecaRendimientosProductivosJsonService.AplicarRendimiento(
                    req,
                    diasHabilesSemana
                );

            if (!aplicado)
            {
                return false;
            }

            SincronizarHorasDesdeDias(req);
            req.OrigenHoras = string.IsNullOrWhiteSpace(req.RendimientoOrigen)
                ? "Rendimiento"
                : "Rendimiento: " + req.RendimientoOrigen;

            return true;
        }

        private static void RecalcularCostosDesdeDias(RequerimientoProduccionInterna req)
        {
            if (req == null || req.TarifaDiaCargoCLP <= 0.0)
            {
                return;
            }

            req.CostoMinimoCLP = req.DiasPersonaMin * req.TarifaDiaCargoCLP;
            req.CostoEstandarCLP = req.DiasPersonaStd * req.TarifaDiaCargoCLP;
            req.CostoHolguraCLP = req.DiasPersonaHolgura * req.TarifaDiaCargoCLP;
        }
    }
}
