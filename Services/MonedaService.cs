using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart.Services
{
    public static class MonedaService
    {
        private const string FuenteBaseInterna = "Base interna";
        private const string FuenteManual = "Manual";
        private const string FuenteManualInterno = "Manual interno";
        private const string FuenteMindicador = "mindicador.cl";
        private const string FuenteDivisasGlobales = "open.er-api.com";

        public static void InicializarTiposCambio(Cotizacion cotizacion)
        {
            if (cotizacion == null)
            {
                return;
            }

            if (cotizacion.TiposCambio == null)
            {
                cotizacion.TiposCambio = new List<TipoCambio>();
            }

            /*
             * Base contable interna: CLP.
             *
             * Todo costo debe calcularse en CLP primero.
             * Luego se convierte a moneda visual si corresponde.
             */

            AgregarSiNoExiste(cotizacion, "CLP", "Peso chileno", 1.0, FuenteBaseInterna);

            /*
             * Indicadores actualizables desde mindicador.cl.
             * Los valores manuales son fallback inicial.
             */

            AgregarSiNoExiste(cotizacion, "USD", "Dólar observado", 900.0, FuenteManual);
            AgregarSiNoExiste(cotizacion, "EUR", "Euro", 1000.0, FuenteManual);
            AgregarSiNoExiste(cotizacion, "UF", "Unidad de Fomento", 40000.0, FuenteManual);
            AgregarSiNoExiste(cotizacion, "UTM", "Unidad Tributaria Mensual", 70000.0, FuenteManual);

            /*
             * Monedas internacionales.
             * Se actualizan desde una fuente global porque mindicador.cl no las entrega.
             */
            AgregarSiNoExiste(cotizacion, "JPY", "Yen japonés", 6.0, FuenteManual);
            AgregarSiNoExiste(cotizacion, "KRW", "Won surcoreano", 0.7, FuenteManual);

            /*
             * GBP interno.
             *
             * IMPORTANTE:
             * En este software GBP NO significa libra esterlina.
             * Es una unidad/conversión interna manual.
             * No se actualiza online.
             * No debe aparecer como moneda cliente normal.
             */

            AgregarSiNoExiste(cotizacion, "GBP", "GBP interno", 1.0, FuenteManualInterno);

            NormalizarMonedasInternas(cotizacion);
        }

        private static void NormalizarMonedasInternas(Cotizacion cotizacion)
        {
            if (cotizacion == null || cotizacion.TiposCambio == null)
            {
                return;
            }

            TipoCambio gbp = cotizacion.TiposCambio.FirstOrDefault(t =>
                t != null &&
                NormalizarCodigoMoneda(t.Codigo) == "GBP"
            );

            if (gbp != null)
            {
                /*
                 * Limpieza histórica:
                 * Si antes GBP quedó como libra esterlina,
                 * lo convertimos a unidad interna manual.
                 */

                gbp.Codigo = "GBP";
                gbp.Nombre = "GBP interno";
                gbp.Fuente = FuenteManualInterno;

                if (gbp.ValorEnCLP <= 0.0)
                {
                    gbp.ValorEnCLP = 1.0;
                }
            }

            /*
             * Tu clase Cotizacion NO tiene MonedaCliente ni MonedaIngreso.
             * Por eso solo se corrige MonedaVisualizacion.
             *
             * Además, GBP no debe ser moneda visual normal.
             */

            if (NormalizarCodigoMoneda(cotizacion.MonedaVisualizacion) == "GBP")
            {
                cotizacion.MonedaVisualizacion = "CLP";
            }
        }

        private static void AgregarSiNoExiste(
            Cotizacion cotizacion,
            string codigo,
            string nombre,
            double valorEnCLP,
            string fuente
        )
        {
            if (cotizacion == null)
            {
                return;
            }

            if (cotizacion.TiposCambio == null)
            {
                cotizacion.TiposCambio = new List<TipoCambio>();
            }

            codigo = NormalizarCodigoMoneda(codigo);

            bool existe = cotizacion.TiposCambio.Any(t =>
                t != null &&
                NormalizarCodigoMoneda(t.Codigo) == codigo
            );

            if (existe)
            {
                return;
            }

            cotizacion.TiposCambio.Add(new TipoCambio
            {
                Codigo = codigo,
                Nombre = nombre,
                ValorEnCLP = valorEnCLP,
                Fuente = fuente,
                FechaActualizacion = DateTime.Now
            });
        }

        public static TipoCambio ObtenerTipoCambio(Cotizacion cotizacion, string codigo)
        {
            InicializarTiposCambio(cotizacion);

            codigo = NormalizarCodigoMoneda(codigo);

            TipoCambio tipo = cotizacion.TiposCambio
                .FirstOrDefault(t =>
                    t != null &&
                    NormalizarCodigoMoneda(t.Codigo) == codigo
                );

            if (tipo == null)
            {
                tipo = cotizacion.TiposCambio.First(t => t.Codigo == "CLP");
            }

            return tipo;
        }

        public static double ConvertirHaciaCLP(
            Cotizacion cotizacion,
            double valor,
            string monedaOrigen
        )
        {
            TipoCambio tipo = ObtenerTipoCambio(cotizacion, monedaOrigen);

            if (tipo == null || tipo.ValorEnCLP <= 0.0)
            {
                return valor;
            }

            return valor * tipo.ValorEnCLP;
        }

        public static double ConvertirDesdeCLP(
            Cotizacion cotizacion,
            double valorCLP,
            string monedaDestino
        )
        {
            TipoCambio tipo = ObtenerTipoCambio(cotizacion, monedaDestino);

            if (tipo == null || tipo.ValorEnCLP <= 0.0)
            {
                return valorCLP;
            }

            return valorCLP / tipo.ValorEnCLP;
        }

        public static string FormatearMoneda(Cotizacion cotizacion, double valorCLP)
        {
            if (cotizacion == null)
            {
                return "$ " + valorCLP.ToString("N0", new CultureInfo("es-CL"));
            }

            InicializarTiposCambio(cotizacion);

            string moneda = NormalizarCodigoMoneda(cotizacion.MonedaVisualizacion);

            /*
             * GBP interno no se usa como moneda visual normal.
             */
            if (moneda == "GBP")
            {
                moneda = "CLP";
            }

            double valorConvertido = ConvertirDesdeCLP(cotizacion, valorCLP, moneda);

            if (moneda == "CLP")
            {
                return "$ " + valorConvertido.ToString("N0", new CultureInfo("es-CL"));
            }

            if (moneda == "UF")
            {
                return valorConvertido.ToString("0.00", new CultureInfo("es-CL")) + " UF";
            }

            if (moneda == "UTM")
            {
                return valorConvertido.ToString("0.00", new CultureInfo("es-CL")) + " UTM";
            }

            if (moneda == "JPY")
            {
                return "JPY " + valorConvertido.ToString("N0", new CultureInfo("es-CL"));
            }

            return moneda + " " + valorConvertido.ToString("N2", new CultureInfo("es-CL"));
        }

        public static string FormatearConversionInterna(
            Cotizacion cotizacion,
            double valorCLP,
            string codigoInterno
        )
        {
            InicializarTiposCambio(cotizacion);

            string codigo = NormalizarCodigoMoneda(codigoInterno);

            double valorConvertido = ConvertirDesdeCLP(cotizacion, valorCLP, codigo);

            if (codigo == "GBP")
            {
                return "GBP interno " + valorConvertido.ToString("N2", new CultureInfo("es-CL"));
            }

            if (codigo == "CLP")
            {
                return "$ " + valorConvertido.ToString("N0", new CultureInfo("es-CL"));
            }

            if (codigo == "UF")
            {
                return valorConvertido.ToString("0.00", new CultureInfo("es-CL")) + " UF";
            }

            if (codigo == "UTM")
            {
                return valorConvertido.ToString("0.00", new CultureInfo("es-CL")) + " UTM";
            }

            if (codigo == "JPY")
            {
                return "JPY " + valorConvertido.ToString("N0", new CultureInfo("es-CL"));
            }

            return codigo + " " + valorConvertido.ToString("N2", new CultureInfo("es-CL"));
        }

        public static async Task<bool> ActualizarDesdeMindicadorAsync(Cotizacion cotizacion)
        {
            InicializarTiposCambio(cotizacion);

            bool actualizoAlgo = false;

            try
            {
                actualizoAlgo = await ActualizarIndicadoresChilenosAsync(cotizacion);
            }
            catch
            {
                /*
                 * Si falla internet, DNS, JSON o API:
                 * se conservan los valores manuales/locales.
                 */
            }

            try
            {
                actualizoAlgo = await ActualizarDivisasGlobalesAsync(cotizacion) || actualizoAlgo;
            }
            catch
            {
                /*
                 * JPY/KRW son convenientes, pero no deben impedir
                 * que el resto de la actualización quede usable.
                 */
            }

            return actualizoAlgo;
        }

        private static async Task<bool> ActualizarIndicadoresChilenosAsync(Cotizacion cotizacion)
        {
            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                using HttpClient client = CrearClienteIndicadores(TimeSpan.FromSeconds(4));

                string json = await ObtenerJsonAsync(client, "https://mindicador.cl/api", cts.Token);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                int actualizados = 0;

                if (ActualizarIndicador(root, cotizacion, "dolar", "USD", "Dólar observado", FuenteMindicador))
                {
                    actualizados++;
                }

                if (ActualizarIndicador(root, cotizacion, "euro", "EUR", "Euro", FuenteMindicador))
                {
                    actualizados++;
                }

                if (ActualizarIndicador(root, cotizacion, "uf", "UF", "Unidad de Fomento", FuenteMindicador))
                {
                    actualizados++;
                }

                if (ActualizarIndicador(root, cotizacion, "utm", "UTM", "Unidad Tributaria Mensual", FuenteMindicador))
                {
                    actualizados++;
                }

                /*
                 * No se actualiza GBP.
                 * GBP es interno manual.
                 */

                return actualizados > 0;
            }
            catch
            {
                return await ActualizarIndicadoresChilenosPorSerieAsync(cotizacion);
            }
        }

        private static async Task<bool> ActualizarIndicadoresChilenosPorSerieAsync(Cotizacion cotizacion)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            using HttpClient client = CrearClienteIndicadores(TimeSpan.FromSeconds(6));

            int actualizados = 0;

            if (await ActualizarIndicadorSerieMindicadorAsync(client, cotizacion, "dolar", "USD", "Dólar observado", cts.Token))
            {
                actualizados++;
            }

            if (await ActualizarIndicadorSerieMindicadorAsync(client, cotizacion, "euro", "EUR", "Euro", cts.Token))
            {
                actualizados++;
            }

            if (await ActualizarIndicadorSerieMindicadorAsync(client, cotizacion, "uf", "UF", "Unidad de Fomento", cts.Token))
            {
                actualizados++;
            }

            if (await ActualizarIndicadorSerieMindicadorAsync(client, cotizacion, "utm", "UTM", "Unidad Tributaria Mensual", cts.Token))
            {
                actualizados++;
            }

            return actualizados > 0;
        }

        private static async Task<bool> ActualizarIndicadorSerieMindicadorAsync(
            HttpClient client,
            Cotizacion cotizacion,
            string indicador,
            string codigo,
            string nombre,
            CancellationToken cancellationToken
        )
        {
            try
            {
                string json = await ObtenerJsonAsync(client, "https://mindicador.cl/api/" + indicador, cancellationToken);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("serie", out JsonElement serie) || serie.GetArrayLength() == 0)
                {
                    return false;
                }

                JsonElement primerValor = serie[0];

                if (!primerValor.TryGetProperty("valor", out JsonElement valorElemento))
                {
                    return false;
                }

                double valor = valorElemento.GetDouble();

                if (valor <= 0.0)
                {
                    return false;
                }

                TipoCambio tipo = ObtenerTipoCambio(cotizacion, codigo);

                tipo.Codigo = NormalizarCodigoMoneda(codigo);
                tipo.Nombre = nombre;
                tipo.ValorEnCLP = valor;
                tipo.Fuente = FuenteMindicador;
                tipo.FechaActualizacion = DateTime.Now;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> ActualizarDivisasGlobalesAsync(Cotizacion cotizacion)
        {
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            using HttpClient client = CrearClienteIndicadores(TimeSpan.FromSeconds(4));

            string json = await ObtenerJsonAsync(client, "https://open.er-api.com/v6/latest/CLP", cts.Token);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("rates", out JsonElement rates))
            {
                return false;
            }

            int actualizados = 0;

            if (ActualizarDivisaDesdeBaseClp(rates, cotizacion, "USD", "Dólar estadounidense", true))
            {
                actualizados++;
            }

            if (ActualizarDivisaDesdeBaseClp(rates, cotizacion, "EUR", "Euro", true))
            {
                actualizados++;
            }

            if (ActualizarDivisaDesdeBaseClp(rates, cotizacion, "JPY", "Yen japonés", false))
            {
                actualizados++;
            }

            if (ActualizarDivisaDesdeBaseClp(rates, cotizacion, "KRW", "Won surcoreano", false))
            {
                actualizados++;
            }

            return actualizados > 0;
        }

        private static HttpClient CrearClienteIndicadores(TimeSpan timeout)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            HttpClient client = new HttpClient(handler);
            client.Timeout = timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OthalartCotizador/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            return client;
        }

        private static async Task<string> ObtenerJsonAsync(
            HttpClient client,
            string url,
            CancellationToken cancellationToken
        )
        {
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private static bool ActualizarDivisaDesdeBaseClp(
            JsonElement rates,
            Cotizacion cotizacion,
            string codigo,
            string nombre,
            bool soloSiNoVieneDeMindicador
        )
        {
            if (!rates.TryGetProperty(codigo, out JsonElement tasaElemento))
            {
                return false;
            }

            double tasaDesdeClp = tasaElemento.GetDouble();

            if (tasaDesdeClp <= 0.0)
            {
                return false;
            }

            double valorEnClp = 1.0 / tasaDesdeClp;

            if (valorEnClp <= 0.0 || double.IsInfinity(valorEnClp) || double.IsNaN(valorEnClp))
            {
                return false;
            }

            TipoCambio tipo = ObtenerTipoCambio(cotizacion, codigo);

            if (soloSiNoVieneDeMindicador &&
                !string.IsNullOrWhiteSpace(tipo.Fuente) &&
                tipo.Fuente.Contains(FuenteMindicador, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            tipo.Codigo = NormalizarCodigoMoneda(codigo);
            tipo.Nombre = nombre;
            tipo.ValorEnCLP = valorEnClp;
            tipo.Fuente = FuenteDivisasGlobales;
            tipo.FechaActualizacion = DateTime.Now;

            return true;
        }

        private static bool ActualizarIndicador(
            JsonElement root,
            Cotizacion cotizacion,
            string nombreJson,
            string codigo,
            string nombre,
            string fuente
        )
        {
            if (!root.TryGetProperty(nombreJson, out JsonElement indicador))
            {
                return false;
            }

            if (!indicador.TryGetProperty("valor", out JsonElement valorElemento))
            {
                return false;
            }

            double valor = valorElemento.GetDouble();

            if (valor <= 0.0)
            {
                return false;
            }

            TipoCambio tipo = ObtenerTipoCambio(cotizacion, codigo);

            tipo.Codigo = NormalizarCodigoMoneda(codigo);
            tipo.Nombre = nombre;
            tipo.ValorEnCLP = valor;
            tipo.Fuente = fuente;
            tipo.FechaActualizacion = DateTime.Now;

            return true;
        }

        public static List<string> ObtenerCodigosMonedaDisponibles(Cotizacion cotizacion)
        {
            InicializarTiposCambio(cotizacion);

            /*
             * Monedas visibles para cliente / cotización.
             * GBP queda fuera porque es interno manual.
             */

            return cotizacion.TiposCambio
                .Where(t => t != null)
                .Select(t => NormalizarCodigoMoneda(t.Codigo))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Where(c => c != "GBP")
                .Distinct()
                .OrderBy(c => ObtenerOrdenMoneda(c))
                .ToList();
        }

        public static List<string> ObtenerCodigosConversionInterna(Cotizacion cotizacion)
        {
            InicializarTiposCambio(cotizacion);

            /*
             * Conversión interna.
             * Aquí sí puede aparecer GBP.
             */

            return cotizacion.TiposCambio
                .Where(t => t != null)
                .Select(t => NormalizarCodigoMoneda(t.Codigo))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => ObtenerOrdenMoneda(c))
                .ToList();
        }

        private static int ObtenerOrdenMoneda(string codigo)
        {
            codigo = NormalizarCodigoMoneda(codigo);

            if (codigo == "CLP") return 0;
            if (codigo == "USD") return 1;
            if (codigo == "EUR") return 2;
            if (codigo == "UF") return 3;
            if (codigo == "UTM") return 4;
            if (codigo == "JPY") return 5;
            if (codigo == "GBP") return 90;

            return 99;
        }

        private static string NormalizarCodigoMoneda(string codigo)
        {
            return (codigo ?? "")
                .Trim()
                .ToUpperInvariant();
        }
    }
}
