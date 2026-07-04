using System;
using System.Collections.Generic;
using System.Linq;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public interface IEcuacionProductiva2D
    {
        string NombreEcuacion { get; }

        bool AplicaA(EntregableBrief entregable);

        void Calcular(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose
        );
    }

    public abstract class EcuacionProductiva2DBase : IEcuacionProductiva2D
    {
        protected const double DiasTrabajoMes = 22.0;

        private sealed class CargoPipelinePonderado
        {
            public string TextoOriginal { get; set; } = "";
            public string Cargo { get; set; } = "";
            public double Ponderador { get; set; } = 1.0;
        }

        public abstract string NombreEcuacion { get; }

        public abstract bool AplicaA(EntregableBrief entregable);

        public abstract void Calcular(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose
        );

        protected static string Normalizar(string texto)
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
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }

        protected static bool NombreContiene(EntregableBrief entregable, params string[] patrones)
        {
            string nombre = Normalizar(entregable?.Nombre ?? "");

            foreach (string patron in patrones)
            {
                if (nombre.Contains(Normalizar(patron)))
                {
                    return true;
                }
            }

            return false;
        }

        protected static int CantidadSegura(EntregableBrief entregable)
        {
            if (entregable == null || entregable.Cantidad < 1)
            {
                return 1;
            }

            return entregable.Cantidad;
        }

        protected static double ConvertirDuracionASegundos(EntregableBrief entregable)
        {
            if (entregable == null || entregable.DuracionPorUnidad <= 0.0)
            {
                return 0.0;
            }

            string unidad = Normalizar(entregable.UnidadDuracion);

            if (unidad.Contains("minuto"))
            {
                return entregable.DuracionPorUnidad * 60.0;
            }

            if (unidad.Contains("segundo"))
            {
                return entregable.DuracionPorUnidad;
            }

            return 0.0;
        }

        protected static double ObtenerFactorCalidad(string calidad)
        {
            string c = Normalizar(calidad);

            if (c.Contains("baja") || c.Contains("rough") || c.Contains("boceto"))
            {
                return 0.75;
            }

            if (c.Contains("alta") || c.Contains("pulido"))
            {
                return 1.35;
            }

            if (c.Contains("premium"))
            {
                return 1.70;
            }

            return 1.0;
        }

        protected static string CalidadSegura(EntregableBrief entregable)
        {
            if (entregable == null || string.IsNullOrWhiteSpace(entregable.NivelCalidadEstimado))
            {
                return "Estándar";
            }

            return entregable.NivelCalidadEstimado;
        }

        protected static int RevisionesSeguras(EntregableBrief entregable)
        {
            if (entregable == null || entregable.RevisionesIncluidas <= 0)
            {
                return 2;
            }

            return entregable.RevisionesIncluidas;
        }

        protected void AgregarRequerimiento(
            DesgloseProductivoProyecto desglose,
            EntregableBrief entregable,
            string tipoInterno,
            string nombreRequerimiento,
            double cantidad,
            string unidad,
            string etapa,
            string calidad,
            double diasMin,
            double diasStd,
            double diasHolgura,
            string cargoSugerido,
            string nivelCargoSugerido,
            string nota
        )
        {
            if (desglose == null || entregable == null)
            {
                return;
            }

            if (EsRequerimientoDeGestionGlobal(tipoInterno, nombreRequerimiento, cargoSugerido))
            {
                return;
            }

            List<string> cargosPipeline = SepararCargosPipeline(entregable.CargosSugeridos);

            if (cargosPipeline.Count > 0)
            {
                SepararNombreNivelCargoPipeline(
                    cargosPipeline[0],
                    out string nombreCargoPrincipal,
                    out string nivelCargoPrincipal
                );

                string cargosVisibles = string.Join("; ", cargosPipeline);

                AgregarRequerimientoConCargo(
                    desglose,
                    entregable,
                    tipoInterno,
                    nombreRequerimiento,
                    cantidad,
                    unidad,
                    etapa,
                    calidad,
                    diasMin,
                    diasStd,
                    diasHolgura,
                    cargosVisibles,
                    string.IsNullOrWhiteSpace(nivelCargoPrincipal)
                        ? nivelCargoSugerido
                        : nivelCargoPrincipal,
                    nota,
                    nombreCargoPrincipal
                );

                return;
            }

            AgregarRequerimientoConCargo(
                desglose,
                entregable,
                tipoInterno,
                nombreRequerimiento,
                cantidad,
                unidad,
                etapa,
                calidad,
                diasMin,
                diasStd,
                diasHolgura,
                cargoSugerido,
                nivelCargoSugerido,
                nota,
                cargoSugerido
            );
        }

        private static bool EsRequerimientoDeGestionGlobal(
            string tipoInterno,
            string nombreRequerimiento,
            string cargoSugerido
        )
        {
            string tipo = Normalizar(tipoInterno);
            string nombre = Normalizar(nombreRequerimiento);
            string cargo = Normalizar(cargoSugerido);

            if (tipo.Contains("direccion") || cargo.Contains("director"))
            {
                return true;
            }

            if (nombre.Contains("coordinacion") ||
                nombre.Contains("seguimiento") ||
                cargo.Contains("projectmanager") ||
                cargo.Contains("productor"))
            {
                return true;
            }

            return false;
        }

        private void AgregarRequerimientoConCargo(
            DesgloseProductivoProyecto desglose,
            EntregableBrief entregable,
            string tipoInterno,
            string nombreRequerimiento,
            double cantidad,
            string unidad,
            string etapa,
            string calidad,
            double diasMin,
            double diasStd,
            double diasHolgura,
            string cargoSugerido,
            string nivelCargoSugerido,
            string nota,
            string cargoParaCosto
        )
        {
            string etapaFinal = string.IsNullOrWhiteSpace(entregable.EtapaSugerida)
                ? etapa
                : entregable.EtapaSugerida;

            List<string> cargosParaCosto = SepararCargosPipeline(cargoSugerido);
            if (cargosParaCosto.Count == 0 && !string.IsNullOrWhiteSpace(cargoParaCosto))
            {
                cargosParaCosto.Add(cargoParaCosto);
            }

            double sueldoMensualCLP = 0.0;
            double tarifaDiaCLP = 0.0;
            List<string> cargosNoEncontrados = new List<string>();

            CalcularCostoCargosRequerimiento(
                cargosParaCosto,
                etapaFinal,
                nivelCargoSugerido,
                out sueldoMensualCLP,
                out tarifaDiaCLP,
                cargosNoEncontrados
            );

            string notaFinal = nota;

            if (cargosNoEncontrados.Count > 0)
            {
                notaFinal += " Cargos no encontrados en biblioteca: " +
                    string.Join("; ", cargosNoEncontrados) +
                    ". El costo solo considera los cargos encontrados.";
            }

            RequerimientoProduccionInterna requerimiento = new RequerimientoProduccionInterna
            {
                EntregableCliente = entregable.Nombre,
                CategoriaEntregable = entregable.Categoria,
                EcuacionUsada = NombreEcuacion,

                TipoInterno = tipoInterno,
                NombreRequerimiento = nombreRequerimiento,
                Cantidad = cantidad,
                Unidad = unidad,
                EtapaSugerida = etapaFinal,
                Calidad = calidad,
                BloqueProductivo = etapaFinal,
                DependeDe = entregable.DependeDe ?? "",

                CargoSugerido = cargoSugerido,
                NivelCargoSugerido = nivelCargoSugerido,
                AreaCargoSugerida = etapaFinal,
                SueldoMensualCargoCLP = sueldoMensualCLP,
                TarifaDiaCargoCLP = tarifaDiaCLP,

                DiasPersonaMin = diasMin,
                DiasPersonaStd = diasStd,
                DiasPersonaHolgura = diasHolgura,

                CostoMinimoCLP = diasMin * tarifaDiaCLP,
                CostoEstandarCLP = diasStd * tarifaDiaCLP,
                CostoHolguraCLP = diasHolgura * tarifaDiaCLP,

                Nota = notaFinal
            };

            desglose.Requerimientos.Add(requerimiento);
        }

        private static List<string> SepararCargosPipeline(string texto)
        {
            return (texto ?? "")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();
        }

        private static CargoPipelinePonderado ParsearCargoPipelinePonderado(string texto)
        {
            string valor = (texto ?? "").Trim();
            string cargo = valor;
            double ponderador = 1.0;
            int separador = valor.LastIndexOf('|');

            if (separador >= 0)
            {
                cargo = valor.Substring(0, separador).Trim();
                string ponderadorTexto = valor.Substring(separador + 1).Trim();
                if (!double.TryParse(
                        ponderadorTexto,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out ponderador) &&
                    !double.TryParse(
                        ponderadorTexto,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.CurrentCulture,
                        out ponderador))
                {
                    ponderador = 1.0;
                }
            }

            return new CargoPipelinePonderado
            {
                TextoOriginal = valor,
                Cargo = cargo,
                Ponderador = Math.Max(0.0, Math.Min(5.0, ponderador))
            };
        }

        private static void CalcularCostoCargosRequerimiento(
            List<string> cargos,
            string etapa,
            string nivelPreferido,
            out double sueldoMensualCLP,
            out double tarifaDiaCLP,
            List<string> cargosNoEncontrados
        )
        {
            sueldoMensualCLP = 0.0;
            tarifaDiaCLP = 0.0;

            foreach (string cargoTexto in cargos ?? new List<string>())
            {
                CargoPipelinePonderado cargoVector = ParsearCargoPipelinePonderado(cargoTexto);
                SepararNombreNivelCargoPipeline(
                    cargoVector.Cargo,
                    out string nombreCargo,
                    out string nivelCargo
                );

                if (string.IsNullOrWhiteSpace(nombreCargo))
                {
                    continue;
                }

                string nivelBusqueda = string.IsNullOrWhiteSpace(nivelCargo)
                    ? nivelPreferido
                    : nivelCargo;

                if (string.IsNullOrWhiteSpace(nivelBusqueda))
                {
                    nivelBusqueda = "típico";
                }

                CategoriaTrabajador cargo = BibliotecaCargosProductivos2D.BuscarCargo(
                    nombreCargo,
                    etapa,
                    nivelBusqueda
                );

                if (cargo == null)
                {
                    cargosNoEncontrados.Add(cargoVector.TextoOriginal);
                    continue;
                }

                sueldoMensualCLP += cargo.SueldoMensualCLPTipico * cargoVector.Ponderador;
                tarifaDiaCLP += (cargo.SueldoMensualCLPTipico / DiasTrabajoMes) * cargoVector.Ponderador;
            }
        }

        private static void SepararNombreNivelCargoPipeline(
            string texto,
            out string nombre,
            out string nivel
        )
        {
            nombre = (texto ?? "").Trim();
            nivel = "";

            int idx = nombre.LastIndexOf(" (", StringComparison.Ordinal);

            if (idx <= 0 || !nombre.EndsWith(")", StringComparison.Ordinal))
            {
                return;
            }

            nivel = nombre.Substring(idx + 2, nombre.Length - idx - 3).Trim();
            nombre = nombre.Substring(0, idx).Trim();
        }

        protected void GenerarPiezaAudiovisual(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double duracionDefaultSegundos,
            double segundosPorPlano,
            double backgroundsPorPlano,
            double personajesBase,
            double propsPorPlano,
            double diasAnimacionPorSegundoMin,
            double diasAnimacionPorSegundoStd,
            double diasAnimacionPorSegundoHolgura,
            string notaBase
        )
        {
            int cantidad = CantidadSegura(entregable);

            double duracionSegundos = ConvertirDuracionASegundos(entregable);

            if (duracionSegundos <= 0.0)
            {
                duracionSegundos = duracionDefaultSegundos;
            }

            double segundosTotales = duracionSegundos * cantidad;

            int planos = entregable.PlanosEstimados
                ?? Math.Max(1, (int)Math.Ceiling(segundosTotales / segundosPorPlano));

            int backgrounds = entregable.BackgroundsEstimados
                ?? Math.Max(0, (int)Math.Ceiling(planos * backgroundsPorPlano));

            int personajes = entregable.PersonajesEstimados
                ?? Math.Max(0, (int)Math.Ceiling(personajesBase * cantidad));

            int props = entregable.PropsEstimados
                ?? Math.Max(0, (int)Math.Ceiling(planos * propsPorPlano));

            double segundosAnimados = entregable.SegundosAnimadosEfectivos
                ?? segundosTotales;

            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);
            int revisiones = RevisionesSeguras(entregable);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Desarrollo",
                "Guion / idea base",
                1,
                "piezas",
                "Desarrollo",
                calidad,
                0.5 * factor,
                1.0 * factor,
                2.0 * factor,
                "Guionista",
                "típico",
                "Base narrativa mínima para ordenar la pieza audiovisual."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Plano",
                "Planos estimados",
                planos,
                "planos",
                "Preproduccion",
                calidad,
                planos * 0.15 * factor,
                planos * 0.30 * factor,
                planos * 0.50 * factor,
                "Storyboard artist",
                "típico",
                "Planos estimados desde duración y segundos promedio por plano."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animatic",
                "Animatic / previsualización",
                segundosTotales,
                "segundos",
                "Preproduccion",
                calidad,
                segundosTotales * 0.03 * factor,
                segundosTotales * 0.06 * factor,
                segundosTotales * 0.10 * factor,
                "Animatic editor",
                "típico",
                "Previsualización temporal de la pieza."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Background",
                "Backgrounds / fondos",
                backgrounds,
                "fondos",
                "Produccion",
                calidad,
                backgrounds * 1.5 * factor,
                backgrounds * 2.0 * factor,
                backgrounds * 3.0 * factor,
                "Artista de fondos",
                "típico",
                "Fondos estimados desde cantidad de planos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Personaje",
                "Personajes principales/secundarios",
                personajes,
                "personajes",
                "Preproduccion",
                calidad,
                personajes * 2.0 * factor,
                personajes * 4.0 * factor,
                personajes * 6.0 * factor,
                "Diseñador de personajes",
                "típico",
                "Personajes estimados preliminarmente. Puede reemplazarse por cantidad real."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Prop",
                "Props / objetos relevantes",
                props,
                "props",
                "Preproduccion",
                calidad,
                props * 0.25 * factor,
                props * 0.50 * factor,
                props * 1.0 * factor,
                "Diseñador de props",
                "típico",
                "Props estimados desde cantidad de planos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animación",
                "Segundos animados efectivos",
                segundosAnimados,
                "segundos",
                "Produccion",
                calidad,
                segundosAnimados * diasAnimacionPorSegundoMin * factor,
                segundosAnimados * diasAnimacionPorSegundoStd * factor,
                segundosAnimados * diasAnimacionPorSegundoHolgura * factor,
                "Animador 2D",
                "típico",
                "Carga principal de animación 2D."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Composición",
                "Composición / integración",
                segundosAnimados,
                "segundos",
                "Postproduccion",
                calidad,
                segundosAnimados * 0.05 * factor,
                segundosAnimados * 0.10 * factor,
                segundosAnimados * 0.18 * factor,
                "Compositor",
                "típico",
                "Composición estimada por segundo animado efectivo."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Revisión",
                "Rondas de revisión",
                revisiones,
                "rondas",
                "Postproduccion",
                calidad,
                revisiones * 0.25 * factor,
                revisiones * 0.50 * factor,
                revisiones * 1.0 * factor,
                "Editor",
                "típico",
                "Rondas de revisión incluidas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Exportación",
                "Entrega final / export",
                1,
                "entrega",
                "Postproduccion",
                calidad,
                0.25 * factor,
                0.50 * factor,
                1.0 * factor,
                "Render / export manager",
                "típico",
                notaBase
            );
        }
    }

    public class EcuacionCinematica2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-CIN-2D-001 | Ecuación productiva de cinemática 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "cinematica", "cinemática", "cutscene");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            GenerarPiezaAudiovisual(
                entregable,
                desglose,
                duracionDefaultSegundos: 30.0,
                segundosPorPlano: 5.0,
                backgroundsPorPlano: 0.70,
                personajesBase: 2.0,
                propsPorPlano: 1.0,
                diasAnimacionPorSegundoMin: 0.25,
                diasAnimacionPorSegundoStd: 0.50,
                diasAnimacionPorSegundoHolgura: 0.80,
                notaBase: "Cinemática 2D estimada por desglose de planos, fondos, personajes, props y segundos animados."
            );
        }
    }

    public class EcuacionTrailerTeaser2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-TRL-2D-001 | Ecuación productiva de trailer/teaser 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "trailer", "teaser");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            GenerarPiezaAudiovisual(
                entregable,
                desglose,
                duracionDefaultSegundos: 30.0,
                segundosPorPlano: 4.0,
                backgroundsPorPlano: 0.80,
                personajesBase: 2.0,
                propsPorPlano: 1.20,
                diasAnimacionPorSegundoMin: 0.30,
                diasAnimacionPorSegundoStd: 0.55,
                diasAnimacionPorSegundoHolgura: 0.90,
                notaBase: "Trailer/teaser 2D con más cortes y mayor carga de edición que una cinemática simple."
            );
        }
    }

    public class EcuacionLoopAnimado2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-LOOP-2D-001 | Ecuación productiva de loop animado 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "loop", "idle", "walk", "run", "ciclo");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int cantidad = CantidadSegura(entregable);
            double duracion = ConvertirDuracionASegundos(entregable);

            if (duracion <= 0.0)
            {
                duracion = 2.0;
            }

            double segundos = duracion * cantidad;
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animación",
                "Loop animado 2D",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.20 * factor,
                segundos * 0.40 * factor,
                segundos * 0.70 * factor,
                "Animador 2D",
                "típico",
                "Loop animado calculado por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Exportación",
                "Export de loop",
                cantidad,
                "piezas",
                "Postproduccion",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.25 * factor,
                cantidad * 0.50 * factor,
                "Render / export manager",
                "típico",
                "Preparación/exportación de ciclos animados."
            );
        }
    }


    public class EcuacionComic2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-COMIC-2D-001 | Ecuación productiva de comic";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(
                entregable,
                "comic",
                "cómic",
                "historieta",
                "manga",
                "pagina comic",
                "página comic",
                "viñeta",
                "vineta"
            );
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int paginas = CantidadSegura(entregable);

            int vinetas = entregable.PlanosEstimados
                ?? Math.Max(1, paginas * 5);

            int personajes = entregable.PersonajesEstimados
                ?? Math.Max(1, (int)Math.Ceiling(paginas * 1.25));

            int fondos = entregable.BackgroundsEstimados
                ?? Math.Max(0, (int)Math.Ceiling(vinetas * 0.45));

            int props = entregable.PropsEstimados
                ?? Math.Max(0, (int)Math.Ceiling(vinetas * 0.60));

            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Narrativa",
                "Guion / estructura narrativa comic",
                paginas,
                "páginas",
                "Desarrollo",
                calidad,
                paginas * 0.15 * factor,
                paginas * 0.30 * factor,
                paginas * 0.55 * factor,
                "Guionista",
                "típico",
                "Estructura narrativa calculada por páginas de comic."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección visual",
                "Dirección visual / referencias comic",
                paginas,
                "páginas",
                "Desarrollo",
                calidad,
                Math.Max(0.25, paginas * 0.08) * factor,
                Math.Max(0.50, paginas * 0.15) * factor,
                Math.Max(1.00, paginas * 0.30) * factor,
                "Director de arte",
                "típico",
                "Definición de estilo, referencias y criterio visual del comic."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Layout",
                "Layout de página / viñetas",
                vinetas,
                "viñetas",
                "Preproduccion",
                calidad,
                vinetas * 0.05 * factor,
                vinetas * 0.10 * factor,
                vinetas * 0.18 * factor,
                "Storyboard artist",
                "típico",
                "Layout calculado por viñetas. PlanosEstimados funciona como viñetas estimadas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Diseño",
                "Diseño / preparación de personajes comic",
                personajes,
                "personajes",
                "Preproduccion",
                calidad,
                personajes * 0.25 * factor,
                personajes * 0.60 * factor,
                personajes * 1.10 * factor,
                "Director de arte",
                "típico",
                "Preparación visual de personajes recurrentes o relevantes."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Diseño",
                "Fondos / ambientes comic",
                fondos,
                "fondos",
                "Produccion",
                calidad,
                fondos * 0.12 * factor,
                fondos * 0.28 * factor,
                fondos * 0.55 * factor,
                "Artista de fondos",
                "típico",
                "Fondos estimados desde cantidad de viñetas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Diseño",
                "Props / elementos comic",
                props,
                "props",
                "Produccion",
                calidad,
                props * 0.04 * factor,
                props * 0.10 * factor,
                props * 0.20 * factor,
                "Director de arte",
                "típico",
                "Props estimados desde cantidad de viñetas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dibujo",
                "Boceto comic",
                vinetas,
                "viñetas",
                "Produccion",
                calidad,
                vinetas * 0.08 * factor,
                vinetas * 0.16 * factor,
                vinetas * 0.30 * factor,
                "Storyboard artist",
                "típico",
                "Boceto de páginas y viñetas antes de línea final."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Clean up",
                "Línea final comic",
                vinetas,
                "viñetas",
                "Produccion",
                calidad,
                vinetas * 0.07 * factor,
                vinetas * 0.14 * factor,
                vinetas * 0.26 * factor,
                "Clean up artist",
                "típico",
                "Línea final calculada por viñetas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Color",
                "Color comic",
                vinetas,
                "viñetas",
                "Produccion",
                calidad,
                vinetas * 0.06 * factor,
                vinetas * 0.12 * factor,
                vinetas * 0.22 * factor,
                "Colorista",
                "típico",
                "Color calculado por viñetas."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Rotulación",
                "Rotulación / textos comic",
                paginas,
                "páginas",
                "Postproduccion",
                calidad,
                paginas * 0.05 * factor,
                paginas * 0.10 * factor,
                paginas * 0.20 * factor,
                "Compositor",
                "típico",
                "Integración de globos, textos, onomatopeyas y ajustes finales."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Exportación",
                "Export comic",
                paginas,
                "páginas",
                "Postproduccion",
                calidad,
                paginas * 0.03 * factor,
                paginas * 0.07 * factor,
                paginas * 0.12 * factor,
                "Render / export manager",
                "típico",
                "Exportación de páginas finales para entrega."
            );
        }
    }

    public class EcuacionAnimacionPersonaje2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-ANI-PER-2D-001 | Ecuación productiva de animación de personaje 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "animacion de personaje", "animación de personaje", "animacion personaje", "animación personaje");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int cantidad = CantidadSegura(entregable);
            double duracion = ConvertirDuracionASegundos(entregable);

            if (duracion <= 0.0)
            {
                duracion = 2.0;
            }

            double segundos = duracion * cantidad;
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animación",
                "Animación de personaje 2D",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.20 * factor,
                segundos * 0.45 * factor,
                segundos * 0.75 * factor,
                "Animador 2D",
                "típico",
                "Animación de personaje calculada por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Composición",
                "Revisión/export de animación de personaje",
                cantidad,
                "piezas",
                "Postproduccion",
                calidad,
                cantidad * 0.20 * factor,
                cantidad * 0.50 * factor,
                cantidad * 1.0 * factor,
                "Compositor",
                "típico",
                "Cierre técnico de animación de personaje."
            );
        }
    }

    public class EcuacionBackground2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-BG-2D-001 | Ecuación productiva de background/escenario 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "background", "fondo", "escenario");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int cantidad = CantidadSegura(entregable);
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Background",
                "Backgrounds / fondos",
                cantidad,
                "fondos",
                "Produccion",
                calidad,
                cantidad * 1.5 * factor,
                cantidad * 2.0 * factor,
                cantidad * 3.0 * factor,
                "Artista de fondos",
                "típico",
                "Background solicitado como entregable directo."
            );
        }
    }

    public class EcuacionPersonaje2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-PER-2D-001 | Ecuación productiva de personaje 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "personaje");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int cantidad = CantidadSegura(entregable);
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Personaje",
                "Diseño / preparación de personaje",
                cantidad,
                "personajes",
                "Preproduccion",
                calidad,
                cantidad * 2.0 * factor,
                cantidad * 4.0 * factor,
                cantidad * 6.0 * factor,
                "Diseñador de personajes",
                "típico",
                "Personaje solicitado como entregable directo o asset interno."
            );
        }
    }

    public class EcuacionProps2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion => "EC-PROP-2D-001 | Ecuación productiva de props/objetos 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            return NombreContiene(entregable, "prop", "props", "objeto", "objetos");
        }

        public override void Calcular(EntregableBrief entregable, DesgloseProductivoProyecto desglose)
        {
            int cantidad = CantidadSegura(entregable);
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);

            AgregarRequerimiento(
                desglose,
                entregable,
                "Prop",
                "Props / objetos 2D",
                cantidad,
                "props",
                "Preproduccion",
                calidad,
                cantidad * 0.25 * factor,
                cantidad * 0.50 * factor,
                cantidad * 1.0 * factor,
                "Diseñador de props",
                "típico",
                "Prop u objeto solicitado como entregable directo."
            );
        }
    }

    public class EcuacionSubproductoAudiovisual2D : EcuacionProductiva2DBase
    {
        public override string NombreEcuacion =>
            "EC-SUB-2D-002 | Ecuación productiva multi-cargo para subpiezas audiovisuales 2D";

        public override bool AplicaA(EntregableBrief entregable)
        {
            string nombre = Normalizar(entregable?.Nombre ?? "");

            return
                nombre.Contains("guion") ||
                nombre.Contains("direccionvisual") ||
                nombre.Contains("storyboard") ||
                nombre.Contains("animatic") ||
                nombre.Contains("rough") ||
                nombre.Contains("animacion2dfinal") ||
                nombre.Contains("animacionfinal") ||
                nombre.Contains("cleanup") ||
                nombre.Contains("clean") ||
                nombre.Contains("color") ||
                nombre.Contains("composicion") ||
                nombre.Contains("compositing") ||
                nombre.Contains("musica") ||
                nombre.Contains("ambiente") ||
                nombre.Contains("locucion") ||
                nombre.Contains("dialogo") ||
                nombre.Contains("dialogos") ||
                nombre.Contains("audio") ||
                nombre.Contains("sonido") ||
                nombre.Contains("sonoro") ||
                nombre.Contains("sincronizacion") ||
                nombre.Contains("mezcla") ||
                nombre.Contains("edicion") ||
                nombre.Contains("export") ||
                nombre.Contains("entrega");
        }

        public override void Calcular(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose
        )
        {
            if (entregable == null || desglose == null)
            {
                return;
            }

            string nombre = Normalizar(entregable.Nombre ?? "");
            string calidad = CalidadSegura(entregable);
            double factor = ObtenerFactorCalidad(calidad);
            int cantidad = CantidadSegura(entregable);

            double segundos = ConvertirDuracionASegundos(entregable);

            if (segundos <= 0.0 && EsPiezaTemporal(nombre))
            {
                segundos = 30.0 * cantidad;
            }
            else
            {
                segundos = segundos * cantidad;
            }

            double minutos = segundos / 60.0;

            if (minutos <= 0.0)
            {
                minutos = 0.5;
            }

            if (nombre.Contains("guion"))
            {
                CalcularGuion(entregable, desglose, cantidad, calidad, factor);
                return;
            }

            if (nombre.Contains("direccionvisual"))
            {
                CalcularDireccionVisual(entregable, desglose, cantidad, calidad, factor);
                return;
            }

            if (nombre.Contains("storyboard"))
            {
                CalcularStoryboard(entregable, desglose, cantidad, calidad, factor);
                return;
            }

            if (nombre.Contains("animatic"))
            {
                CalcularAnimatic(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("rough"))
            {
                CalcularRoughAnimation(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("animacion2dfinal") ||
                nombre.Contains("animacionfinal"))
            {
                CalcularAnimacionFinal2D(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("cleanup") || nombre.Contains("clean"))
            {
                CalcularCleanUp(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("color"))
            {
                CalcularColor(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("composicion") ||
                nombre.Contains("compositing"))
            {
                CalcularComposicion(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (EsAudio(nombre))
            {
                CalcularAudio(entregable, desglose, segundos, minutos, cantidad, calidad, factor);
                return;
            }

            if (nombre.Contains("edicion"))
            {
                CalcularEdicion(entregable, desglose, segundos, minutos, calidad, factor);
                return;
            }

            if (nombre.Contains("export") ||
                nombre.Contains("entrega"))
            {
                CalcularEntregaFinal(entregable, desglose, cantidad, calidad, factor);
                return;
            }
        }

        // =========================================================
        // DESARROLLO
        // =========================================================

        private void CalcularGuion(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            int cantidad,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Guion",
                "Guion / estructura narrativa",
                cantidad,
                "piezas",
                "Desarrollo",
                calidad,
                cantidad * 0.50 * factor,
                cantidad * 1.00 * factor,
                cantidad * 2.00 * factor,
                "Guionista",
                "típico",
                "Escritura o ajuste de base narrativa para el proyecto."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección narrativa",
                "Revisión narrativa / estructura",
                cantidad,
                "piezas",
                "Desarrollo",
                calidad,
                cantidad * 0.15 * factor,
                cantidad * 0.35 * factor,
                cantidad * 0.75 * factor,
                "Director narrativo",
                "típico",
                "Revisión de estructura, claridad narrativa e intención."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de desarrollo narrativo",
                cantidad,
                "piezas",
                "Desarrollo",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.20 * factor,
                cantidad * 0.40 * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación, control de versión y validación de entregables narrativos."
            );
        }

        private void CalcularDireccionVisual(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            int cantidad,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección visual",
                "Dirección visual / referencias de estilo",
                cantidad,
                "piezas",
                "Desarrollo",
                calidad,
                cantidad * 0.50 * factor,
                cantidad * 1.00 * factor,
                cantidad * 2.00 * factor,
                "Director de arte",
                "típico",
                "Definición visual inicial para orientar el proyecto."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Visual development",
                "Exploración visual / moodboard",
                cantidad,
                "piezas",
                "Preproduccion",
                calidad,
                cantidad * 0.40 * factor,
                cantidad * 0.80 * factor,
                cantidad * 1.50 * factor,
                "Visual development artist",
                "típico",
                "Exploración de estilo, lenguaje gráfico, referencias y tono visual."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de dirección visual",
                cantidad,
                "piezas",
                "Desarrollo",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.20 * factor,
                cantidad * 0.40 * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación de referencias, feedback y validación de dirección visual."
            );
        }

        // =========================================================
        // PREPRODUCCIÓN
        // =========================================================

        private void CalcularStoryboard(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            int cantidad,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Storyboard",
                "Storyboard / secuencia visual",
                cantidad,
                "piezas",
                "Preproduccion",
                calidad,
                cantidad * 0.75 * factor,
                cantidad * 1.50 * factor,
                cantidad * 3.00 * factor,
                "Storyboard artist",
                "típico",
                "Storyboard estimado como pieza narrativa. Los planos pueden ajustarse después."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección",
                "Revisión de storyboard",
                cantidad,
                "rondas",
                "Preproduccion",
                calidad,
                cantidad * 0.15 * factor,
                cantidad * 0.35 * factor,
                cantidad * 0.75 * factor,
                "Director/a de animación",
                "típico",
                "Revisión de continuidad, lectura narrativa, intención de acting y ritmo visual."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección visual",
                "Revisión de composición y claridad visual",
                cantidad,
                "rondas",
                "Preproduccion",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.25 * factor,
                cantidad * 0.50 * factor,
                "Director de arte",
                "típico",
                "Revisión de composición, claridad visual y coherencia gráfica."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación y control de storyboard",
                cantidad,
                "piezas",
                "Preproduccion",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.20 * factor,
                cantidad * 0.40 * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación, feedback, control de versión y validación de storyboard."
            );
        }

        private void CalcularAnimatic(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Storyboard",
                "Preparación visual para animatic",
                segundos,
                "segundos",
                "Preproduccion",
                calidad,
                Math.Max(0.25, minutos * 0.40) * factor,
                Math.Max(0.50, minutos * 0.80) * factor,
                Math.Max(1.00, minutos * 1.40) * factor,
                "Storyboard artist",
                "típico",
                "Preparación o ajuste de viñetas/base visual para construir el animatic."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animatic",
                "Edición y timing de animatic",
                segundos,
                "segundos",
                "Preproduccion",
                calidad,
                Math.Max(0.25, minutos * 0.60) * factor,
                Math.Max(0.75, minutos * 1.20) * factor,
                Math.Max(1.50, minutos * 2.00) * factor,
                "Animatic editor",
                "típico",
                "Montaje temporal, ritmo, cortes, duración de planos y previsualización narrativa."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección",
                "Revisión de ritmo y dirección",
                1,
                "rondas",
                "Preproduccion",
                calidad,
                0.15 * factor,
                0.30 * factor,
                0.60 * factor,
                "Director/a de animación",
                "típico",
                "Revisión de ritmo, intención narrativa, continuidad y claridad de actuación."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección visual",
                "Revisión de claridad visual",
                1,
                "rondas",
                "Preproduccion",
                calidad,
                0.10 * factor,
                0.25 * factor,
                0.50 * factor,
                "Director de arte",
                "típico",
                "Revisión de composición, lectura visual y coherencia del lenguaje gráfico."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Audio guía",
                "Audio guía / sincronización básica",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                Math.Max(0.10, minutos * 0.25) * factor,
                Math.Max(0.25, minutos * 0.50) * factor,
                Math.Max(0.50, minutos * 1.00) * factor,
                "Diseñador sonoro",
                "típico",
                "Audio guía, referencia sonora o sincronización básica para validar timing."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación y revisión de entrega animatic",
                1,
                "piezas",
                "Preproduccion",
                calidad,
                0.10 * factor,
                0.20 * factor,
                0.40 * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación, control de versión y validación de entrega del animatic."
            );
        }

        // =========================================================
        // PRODUCCIÓN
        // =========================================================

        private void CalcularRoughAnimation(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Rough animation",
                "Rough animation / acting base",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.12 * factor,
                segundos * 0.24 * factor,
                segundos * 0.45 * factor,
                "Animador 2D",
                "típico",
                "Animación rough calculada por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección",
                "Dirección de animación / revisión acting",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.20, minutos * 0.40) * factor,
                Math.Max(0.40, minutos * 0.80) * factor,
                Math.Max(0.80, minutos * 1.50) * factor,
                "Director/a de animación",
                "típico",
                "Revisión de acting, poses clave, ritmo, intención y continuidad."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Supervisión",
                "Supervisión de animación",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.10, minutos * 0.25) * factor,
                Math.Max(0.25, minutos * 0.50) * factor,
                Math.Max(0.50, minutos * 1.00) * factor,
                "Supervisor de animación",
                "típico",
                "Control técnico y artístico de consistencia de animación."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de producción de rough",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.20, minutos * 0.40) * factor,
                Math.Max(0.40, minutos * 0.80) * factor,
                "Productor / Project manager",
                "típico",
                "Seguimiento, control de versiones, feedback y coordinación de entrega."
            );
        }

        private void CalcularAnimacionFinal2D(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Animación final",
                "Key animation / poses finales",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.06 * factor,
                segundos * 0.12 * factor,
                segundos * 0.22 * factor,
                "Key animator",
                "típico",
                "Refinamiento de poses clave y estructura de animación final."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Animación final",
                "Inbetween / asistencia de animación",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.04 * factor,
                segundos * 0.08 * factor,
                segundos * 0.16 * factor,
                "Inbetween artist",
                "típico",
                "Intermedios, asistencia y continuidad de movimiento."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Dirección",
                "Dirección / revisión de animación final",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.15, minutos * 0.30) * factor,
                Math.Max(0.30, minutos * 0.60) * factor,
                Math.Max(0.60, minutos * 1.20) * factor,
                "Director/a de animación",
                "típico",
                "Revisión de consistencia, timing, acting y calidad final de movimiento."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación animación final",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.20, minutos * 0.40) * factor,
                Math.Max(0.40, minutos * 0.80) * factor,
                "Productor / Project manager",
                "típico",
                "Seguimiento de avance, feedback y control de entrega de animación final."
            );
        }

        private void CalcularCleanUp(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Clean up",
                "Clean up / línea final",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.08 * factor,
                segundos * 0.16 * factor,
                segundos * 0.25 * factor,
                "Clean up artist",
                "típico",
                "Clean up calculado por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Supervisión",
                "Revisión de clean up",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.20, minutos * 0.40) * factor,
                Math.Max(0.40, minutos * 0.80) * factor,
                "Supervisor de arte",
                "típico",
                "Revisión de consistencia de línea, modelo y limpieza visual."
            );
        }

        private void CalcularColor(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Color",
                "Color / pintura base",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                segundos * 0.04 * factor,
                segundos * 0.08 * factor,
                segundos * 0.14 * factor,
                "Colorista",
                "típico",
                "Color calculado por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Supervisión",
                "Revisión de color",
                segundos,
                "segundos",
                "Produccion",
                calidad,
                Math.Max(0.05, minutos * 0.15) * factor,
                Math.Max(0.15, minutos * 0.30) * factor,
                Math.Max(0.30, minutos * 0.60) * factor,
                "Director de arte",
                "típico",
                "Revisión de paleta, consistencia visual y aplicación de estilo."
            );
        }

        // =========================================================
        // POSTPRODUCCIÓN
        // =========================================================

        private void CalcularComposicion(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Composición",
                "Composición / integración",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                segundos * 0.04 * factor,
                segundos * 0.08 * factor,
                segundos * 0.15 * factor,
                "Compositor",
                "típico",
                "Composición calculada por segundos efectivos."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Edición",
                "Revisión de integración y corte",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.20, minutos * 0.40) * factor,
                Math.Max(0.40, minutos * 0.80) * factor,
                "Editor",
                "típico",
                "Revisión de integración, continuidad y corte final."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de postproducción",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                Math.Max(0.10, minutos * 0.15) * factor,
                Math.Max(0.20, minutos * 0.30) * factor,
                Math.Max(0.40, minutos * 0.60) * factor,
                "Productor / Project manager",
                "típico",
                "Seguimiento de postproducción, versiones, feedback y validación."
            );
        }

        private void CalcularAudio(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            int cantidad,
            string calidad,
            double factor
        )
        {
            double cantidadBase = segundos > 0.0 ? segundos : cantidad;
            string unidadBase = segundos > 0.0 ? "segundos" : "piezas";

            AgregarRequerimiento(
                desglose,
                entregable,
                "Sonido",
                "Diseño sonoro / audio",
                cantidadBase,
                unidadBase,
                "Postproduccion",
                calidad,
                Math.Max(0.25, minutos * 0.40) * factor,
                Math.Max(0.50, minutos * 0.80) * factor,
                Math.Max(1.00, minutos * 1.50) * factor,
                "Diseñador sonoro",
                "típico",
                "Diseño sonoro, ambiente, sincronización o tratamiento de audio."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Audio",
                "Mezcla / ajuste de niveles",
                cantidadBase,
                unidadBase,
                "Postproduccion",
                calidad,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.25, minutos * 0.40) * factor,
                Math.Max(0.50, minutos * 0.80) * factor,
                "Mezclador de audio",
                "típico",
                "Mezcla básica, balance de niveles y preparación de audio para entrega."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de audio",
                cantidadBase,
                unidadBase,
                "Postproduccion",
                calidad,
                Math.Max(0.05, minutos * 0.10) * factor,
                Math.Max(0.10, minutos * 0.20) * factor,
                Math.Max(0.25, minutos * 0.40) * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación de referencias, feedback y validación de audio."
            );
        }

        private void CalcularEdicion(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            double segundos,
            double minutos,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Edición",
                "Edición final",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                segundos * 0.03 * factor,
                segundos * 0.06 * factor,
                segundos * 0.12 * factor,
                "Editor",
                "típico",
                "Edición calculada por duración final."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Postproducción",
                "Revisión de edición",
                segundos,
                "segundos",
                "Postproduccion",
                calidad,
                Math.Max(0.10, minutos * 0.15) * factor,
                Math.Max(0.20, minutos * 0.30) * factor,
                Math.Max(0.40, minutos * 0.60) * factor,
                "Postproductor",
                "típico",
                "Revisión de continuidad, ritmo, corte y preparación para entrega."
            );
        }

        private void CalcularEntregaFinal(
            EntregableBrief entregable,
            DesgloseProductivoProyecto desglose,
            int cantidad,
            string calidad,
            double factor
        )
        {
            AgregarRequerimiento(
                desglose,
                entregable,
                "Exportación",
                "Entrega final / export",
                cantidad,
                "piezas",
                "Postproduccion",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.25 * factor,
                cantidad * 0.50 * factor,
                "Render / export manager",
                "típico",
                "Preparación de entrega final, exportación, revisión técnica y empaquetado."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Control de calidad",
                "Control técnico de entrega",
                cantidad,
                "piezas",
                "Postproduccion",
                calidad,
                cantidad * 0.10 * factor,
                cantidad * 0.20 * factor,
                cantidad * 0.40 * factor,
                "Control de calidad técnico",
                "típico",
                "Revisión de archivos, nombres, formato, resolución, peso y consistencia técnica."
            );

            AgregarRequerimiento(
                desglose,
                entregable,
                "Producción",
                "Coordinación de cierre y entrega",
                cantidad,
                "piezas",
                "Postproduccion",
                calidad,
                cantidad * 0.05 * factor,
                cantidad * 0.15 * factor,
                cantidad * 0.30 * factor,
                "Productor / Project manager",
                "típico",
                "Coordinación final con cliente, control de versión y cierre de entrega."
            );
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private bool EsAudio(string nombreNormalizado)
        {
            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                return false;
            }

            return
                nombreNormalizado.Contains("musica") ||
                nombreNormalizado.Contains("ambiente") ||
                nombreNormalizado.Contains("locucion") ||
                nombreNormalizado.Contains("dialogo") ||
                nombreNormalizado.Contains("dialogos") ||
                nombreNormalizado.Contains("audio") ||
                nombreNormalizado.Contains("sonido") ||
                nombreNormalizado.Contains("sonoro") ||
                nombreNormalizado.Contains("sincronizacion") ||
                nombreNormalizado.Contains("mezcla");
        }

        private bool EsPiezaTemporal(string nombreNormalizado)
        {
            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                return false;
            }

            return
                nombreNormalizado.Contains("animatic") ||
                nombreNormalizado.Contains("rough") ||
                nombreNormalizado.Contains("animacion") ||
                nombreNormalizado.Contains("cleanup") ||
                nombreNormalizado.Contains("clean") ||
                nombreNormalizado.Contains("color") ||
                nombreNormalizado.Contains("composicion") ||
                nombreNormalizado.Contains("compositing") ||
                nombreNormalizado.Contains("musica") ||
                nombreNormalizado.Contains("ambiente") ||
                nombreNormalizado.Contains("locucion") ||
                nombreNormalizado.Contains("dialogo") ||
                nombreNormalizado.Contains("audio") ||
                nombreNormalizado.Contains("sonido") ||
                nombreNormalizado.Contains("sonoro") ||
                nombreNormalizado.Contains("sincronizacion") ||
                nombreNormalizado.Contains("mezcla") ||
                nombreNormalizado.Contains("edicion");
        }
    }

    public static class BibliotecaEcuacionesProductivas2D
    {
        private static readonly List<IEcuacionProductiva2D> Ecuaciones =
    new List<IEcuacionProductiva2D>
    {
        new EcuacionTrailerTeaser2D(),
        new EcuacionCinematica2D(),
        new EcuacionLoopAnimado2D(),
        new EcuacionAnimacionPersonaje2D(),

        // Esta es la clave: reconoce subpiezas como Guion, Storyboard,
        // Animatic, Rough, Clean up, Color, Audio, Edición, Entrega, etc.
        new EcuacionSubproductoAudiovisual2D(),

        new EcuacionBackground2D(),
        new EcuacionPersonaje2D(),
        new EcuacionProps2D()
    };

        public static IEcuacionProductiva2D Buscar(EntregableBrief entregable)
        {
            if (entregable == null)
            {
                return null;
            }

            return Ecuaciones.FirstOrDefault(e => e.AplicaA(entregable));
        }

        public static List<IEcuacionProductiva2D> ObtenerTodas()
        {
            return Ecuaciones.ToList();
        }
    }

    public static class BibliotecaCargosProductivos2D
    {
        public static CategoriaTrabajador BuscarCargo(
            string nombreCargo,
            string etapa,
            string nivel
        )
        {
            List<CategoriaTrabajador> biblioteca = ObtenerBibliotecaPorEtapa(etapa);

            CategoriaTrabajador cargo = BuscarEnLista(biblioteca, nombreCargo, nivel);

            if (cargo != null)
            {
                return cargo;
            }

            return BuscarEnLista(ObtenerTodos(), nombreCargo, nivel);
        }

        private static List<CategoriaTrabajador> ObtenerBibliotecaPorEtapa(string etapa)
        {
            string e = Normalizar(etapa);

            if (e.Contains("desarrollo"))
            {
                return Cargos.CrearBibliotecaDesarrollo();
            }

            if (e.Contains("preproduccion"))
            {
                return Cargos.CrearBibliotecaPreproduccion();
            }

            if (e.Contains("produccion") && !e.Contains("post"))
            {
                return Cargos.CrearBibliotecaProduccion();
            }

            if (e.Contains("postproduccion") || e.Contains("post"))
            {
                return Cargos.CrearBibliotecaPostproduccion();
            }

            return ObtenerTodos();
        }

        private static List<CategoriaTrabajador> ObtenerTodos()
        {
            List<CategoriaTrabajador> todos = new List<CategoriaTrabajador>();

            todos.AddRange(Cargos.CrearBibliotecaDesarrollo());
            todos.AddRange(Cargos.CrearBibliotecaPreproduccion());
            todos.AddRange(Cargos.CrearBibliotecaProduccion());
            todos.AddRange(Cargos.CrearBibliotecaPostproduccion());

            return todos;
        }

        private static CategoriaTrabajador BuscarEnLista(
            List<CategoriaTrabajador> cargos,
            string nombreCargo,
            string nivel
        )
        {
            if (cargos == null || cargos.Count == 0)
            {
                return null;
            }

            string nombreNormalizado = Normalizar(nombreCargo);
            string nivelNormalizado = Normalizar(nivel);

            CategoriaTrabajador exactoNivel = cargos.FirstOrDefault(c =>
                Normalizar(c.Nombre) == nombreNormalizado &&
                Normalizar(c.Nivel) == nivelNormalizado
            );

            if (exactoNivel != null)
            {
                return exactoNivel;
            }

            CategoriaTrabajador contieneNivel = cargos.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(nombreNormalizado) &&
                Normalizar(c.Nivel) == nivelNormalizado
            );

            if (contieneNivel != null)
            {
                return contieneNivel;
            }

            CategoriaTrabajador exactoTipico = cargos.FirstOrDefault(c =>
                Normalizar(c.Nombre) == nombreNormalizado &&
                Normalizar(c.Nivel).Contains("tipico")
            );

            if (exactoTipico != null)
            {
                return exactoTipico;
            }

            CategoriaTrabajador contieneTipico = cargos.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(nombreNormalizado) &&
                Normalizar(c.Nivel).Contains("tipico")
            );

            if (contieneTipico != null)
            {
                return contieneTipico;
            }

            return cargos.FirstOrDefault(c =>
                Normalizar(c.Nombre).Contains(nombreNormalizado)
            );
        }

        private static string Normalizar(string texto)
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
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }
    }
}
