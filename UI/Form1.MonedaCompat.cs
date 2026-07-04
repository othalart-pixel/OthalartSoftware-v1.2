using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void InicializarTiposCambio()
        {
            if (cotizacion.TiposCambio == null)
            {
                cotizacion.TiposCambio = new System.Collections.Generic.List<TipoCambio>();
            }

            AsegurarTipoCambio("CLP", "Peso chileno", 1.0, "Base interna", false);
            AsegurarTipoCambio("USD", "Dólar estadounidense", 950.0, "Respaldo manual", true);
            AsegurarTipoCambio("EUR", "Euro", 1050.0, "Respaldo manual", true);
            AsegurarTipoCambio("JPY", "Yen japonés", 6.5, "Respaldo manual", true);
            AsegurarTipoCambio("KRW", "Won surcoreano", 0.70, "Respaldo manual", true);
            AsegurarTipoCambio("UF", "Unidad de fomento", 40000.0, "Respaldo manual", true);

            if (string.IsNullOrWhiteSpace(cotizacion.Moneda))
            {
                cotizacion.Moneda = "CLP";
            }

            if (string.IsNullOrWhiteSpace(cotizacion.MonedaPrecioCliente))
            {
                cotizacion.MonedaPrecioCliente = cotizacion.Moneda;
            }

            if (string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion))
            {
                cotizacion.MonedaVisualizacion = "CLP";
            }
        }

        private void AsegurarTipoCambio(
            string codigo,
            string nombre,
            double valorEnCLP,
            string fuente,
            bool esEditable
        )
        {
            TipoCambio? tipo = cotizacion.TiposCambio
                .FirstOrDefault(t => t.Codigo == codigo);

            if (tipo == null)
            {
                cotizacion.TiposCambio.Add(new TipoCambio
                {
                    Codigo = codigo,
                    Nombre = nombre,
                    ValorEnCLP = valorEnCLP,
                    Fuente = fuente,
                    EsEditable = esEditable,
                    FechaActualizacion = DateTime.Now
                });

                return;
            }

            tipo.Nombre = nombre;
            tipo.EsEditable = esEditable;

            if (tipo.ValorEnCLP <= 0.0)
            {
                tipo.ValorEnCLP = valorEnCLP;
            }

            if (string.IsNullOrWhiteSpace(tipo.Fuente))
            {
                tipo.Fuente = fuente;
            }

            if (tipo.FechaActualizacion == DateTime.MinValue)
            {
                tipo.FechaActualizacion = DateTime.Now;
            }
        }

        private async Task<bool> ActualizarIndicadoresMindicadorConTimeout()
        {
            try
            {
                InicializarTiposCambio();

                using CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(3));

                using HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);

                await ActualizarIndicadorMindicador(client, "dolar", "USD", cts.Token);
                await ActualizarIndicadorMindicador(client, "euro", "EUR", cts.Token);
                await ActualizarIndicadorMindicador(client, "uf", "UF", cts.Token);

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task ActualizarIndicadorMindicador(
            HttpClient client,
            string indicador,
            string codigoMoneda,
            CancellationToken cancellationToken
        )
        {
            string url = "https://mindicador.cl/api/" + indicador;

            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            using JsonDocument doc = JsonDocument.Parse(json);

            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("serie", out JsonElement serie))
            {
                return;
            }

            if (serie.GetArrayLength() == 0)
            {
                return;
            }

            JsonElement primerValor = serie[0];

            if (!primerValor.TryGetProperty("valor", out JsonElement valorJson))
            {
                return;
            }

            double valor = valorJson.GetDouble();

            TipoCambio? tipo = cotizacion.TiposCambio
                .FirstOrDefault(t => t.Codigo == codigoMoneda);

            if (tipo == null)
            {
                return;
            }

            tipo.ValorEnCLP = valor;
            tipo.Fuente = "mindicador.cl";
            tipo.FechaActualizacion = DateTime.Now;
        }

        private void CargarCombosMonedas()
        {
            InicializarTiposCambio();

            if (cmbMonedaVisualizacion != null)
            {
                string monedaSeleccionada = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                    ? "CLP"
                    : cotizacion.MonedaVisualizacion;

                cmbMonedaVisualizacion.SelectedIndexChanged -= CmbMonedaVisualizacion_SelectedIndexChanged;

                cmbMonedaVisualizacion.Items.Clear();

                foreach (TipoCambio tipo in cotizacion.TiposCambio.OrderBy(t => OrdenMoneda(t.Codigo)))
                {
                    cmbMonedaVisualizacion.Items.Add(tipo.Codigo);
                }

                if (cmbMonedaVisualizacion.Items.Contains(monedaSeleccionada))
                {
                    cmbMonedaVisualizacion.SelectedItem = monedaSeleccionada;
                }
                else if (cmbMonedaVisualizacion.Items.Count > 0)
                {
                    cmbMonedaVisualizacion.SelectedIndex = 0;
                }

                cmbMonedaVisualizacion.SelectedIndexChanged += CmbMonedaVisualizacion_SelectedIndexChanged;
            }

            if (cmbMoneda != null)
            {
                string monedaCliente = string.IsNullOrWhiteSpace(cotizacion.Moneda)
                    ? "CLP"
                    : cotizacion.Moneda;

                cmbMoneda.SelectedIndexChanged -= CmbMoneda_SelectedIndexChanged;

                cmbMoneda.Items.Clear();

                foreach (TipoCambio tipo in cotizacion.TiposCambio.OrderBy(t => OrdenMoneda(t.Codigo)))
                {
                    cmbMoneda.Items.Add(tipo.Codigo);
                }

                if (cmbMoneda.Items.Contains(monedaCliente))
                {
                    cmbMoneda.SelectedItem = monedaCliente;
                }
                else if (cmbMoneda.Items.Count > 0)
                {
                    cmbMoneda.SelectedIndex = 0;
                }

                cmbMoneda.SelectedIndexChanged += CmbMoneda_SelectedIndexChanged;
            }

            if (lblMonedaClienteActual != null)
            {
                lblMonedaClienteActual.Text =
                    $"Moneda cliente/cotización: {cotizacion.Moneda}. " +
                    $"Moneda visual activa: {cotizacion.MonedaVisualizacion}. " +
                    "Base interna contable: CLP.";
            }
        }

        private int OrdenMoneda(string codigo)
        {
            if (codigo == "CLP") return 0;
            if (codigo == "USD") return 1;
            if (codigo == "EUR") return 2;
            if (codigo == "JPY") return 3;
            if (codigo == "KRW") return 4;
            if (codigo == "UF") return 5;
            if (codigo == "GRM") return 99;

            return 100;
        }
    }
}