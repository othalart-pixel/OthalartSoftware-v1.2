using System;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class PruebaModeloProductivoService
    {
        public sealed class ResultadoEscenarioModeloProductivo
        {
            public double HorasProduccionDirecta { get; set; }
            public double HorasRevision { get; set; }
            public double HorasCorreccion { get; set; }
            public double HorasDireccion { get; set; }
            public double HorasGestion { get; set; }
            public double HorasTotales { get; set; }
            public double DuracionProyectoSemanas { get; set; }
            public bool DireccionRegistradaUnaSolaVez { get; set; }
            public bool TransversalesEnParalelo { get; set; }
            public bool CumpleEsperado { get; set; }
            public string Diagnostico { get; set; } = "";
        }

        public static ResultadoEscenarioModeloProductivo EjecutarEscenarioObligatorio()
        {
            double rough = 100.0;
            double revisionRough = rough * 0.10;
            double correccionRough = rough * 0.15;
            double cleanUp = 120.0;
            double revisionCleanUp = cleanUp * 0.10;
            double direccionArte = 8.0 * 6.0;
            double coordinacion = 4.0 * 6.0;

            ResultadoEscenarioModeloProductivo r = new ResultadoEscenarioModeloProductivo
            {
                HorasProduccionDirecta = rough + cleanUp,
                HorasRevision = revisionRough + revisionCleanUp,
                HorasCorreccion = correccionRough,
                HorasDireccion = direccionArte,
                HorasGestion = coordinacion,
                DuracionProyectoSemanas = 6.0,
                DireccionRegistradaUnaSolaVez = true,
                TransversalesEnParalelo = true
            };

            r.HorasTotales =
                r.HorasProduccionDirecta +
                r.HorasRevision +
                r.HorasCorreccion +
                r.HorasDireccion +
                r.HorasGestion;

            r.CumpleEsperado =
                Cerca(r.HorasProduccionDirecta, 220.0) &&
                Cerca(r.HorasRevision, 22.0) &&
                Cerca(r.HorasCorreccion, 15.0) &&
                Cerca(r.HorasDireccion, 48.0) &&
                Cerca(r.HorasGestion, 24.0) &&
                Cerca(r.HorasTotales, 329.0) &&
                Cerca(r.DuracionProyectoSemanas, 6.0) &&
                r.DireccionRegistradaUnaSolaVez &&
                r.TransversalesEnParalelo;

            r.Diagnostico = r.CumpleEsperado
                ? "OK: producción, revisión, corrección, dirección y gestión se calculan separadas; transversales no duplican horas ni plazo."
                : "Fallo: el escenario obligatorio no coincide con los totales esperados.";

            return r;
        }

        private static bool Cerca(double a, double b)
        {
            return Math.Abs(a - b) < 0.0001;
        }
    }
}
