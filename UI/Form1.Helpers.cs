using System;
using System.Linq;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private bool ConvertirBool(object valor)
        {
            if (valor == null)
            {
                return false;
            }

            if (valor is bool)
            {
                return (bool)valor;
            }

            bool resultado;

            if (bool.TryParse(valor.ToString(), out resultado))
            {
                return resultado;
            }

            return false;
        }

        private string FormatearMiles(double valor)
        {
            return valor.ToString("N0", new System.Globalization.CultureInfo("es-CL"));
        }

        private string FormatearMilesDesdeTexto(string texto)
        {
            double valor = ConvertirDouble(texto);

            if (valor <= 0.0)
            {
                return "";
            }

            return FormatearMiles(valor);
        }

        private void TextBoxMoneda_TextChanged(object sender, EventArgs e)
        {
            if (formateandoNumero)
            {
                return;
            }

            TextBox textBox = sender as TextBox;

            if (textBox == null)
            {
                return;
            }

            string textoOriginal = textBox.Text;

            if (string.IsNullOrWhiteSpace(textoOriginal))
            {
                return;
            }

            int posicionDesdeFinal = textoOriginal.Length - textBox.SelectionStart;

            string textoLimpio = textoOriginal
                .Replace(".", "")
                .Replace(",", "")
                .Replace("$", "")
                .Replace("CLP", "")
                .Replace("USD", "")
                .Replace(" ", "")
                .Trim();

            if (string.IsNullOrWhiteSpace(textoLimpio))
            {
                return;
            }

            double valor;

            if (!double.TryParse(textoLimpio, out valor))
            {
                return;
            }

            string textoFormateado = FormatearMiles(valor);

            formateandoNumero = true;

            textBox.Text = textoFormateado;

            int nuevaPosicion = textBox.Text.Length - posicionDesdeFinal;

            if (nuevaPosicion < 0)
            {
                nuevaPosicion = 0;
            }

            if (nuevaPosicion > textBox.Text.Length)
            {
                nuevaPosicion = textBox.Text.Length;
            }

            textBox.SelectionStart = nuevaPosicion;
            textBox.SelectionLength = 0;

            formateandoNumero = false;
        }

        private double ConvertirDouble(object valor)
        {
            if (valor == null)
            {
                return 0.0;
            }

            string texto = valor.ToString() ?? "0";

            texto = texto
                .Replace("$", "")
                .Replace("CLP", "")
                .Replace("USD", "")
                .Replace(" ", "")
                .Trim();

            int ultimaComa = texto.LastIndexOf(",");
            int ultimoPunto = texto.LastIndexOf(".");

            if (ultimaComa >= 0 && ultimoPunto >= 0)
            {
                if (ultimaComa > ultimoPunto)
                {
                    texto = texto.Replace(".", "");
                    texto = texto.Replace(",", ".");
                }
                else
                {
                    texto = texto.Replace(",", "");
                }
            }
            else if (ultimaComa >= 0)
            {
                texto = texto.Replace(",", ".");
            }
            else
            {
                int cantidadPuntos = texto.Count(c => c == '.');

                if (cantidadPuntos > 1)
                {
                    texto = texto.Replace(".", "");
                }
            }

            double resultado;

            if (double.TryParse(
                texto,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out resultado
            ))
            {
                return resultado;
            }

            return 0.0;
        }

        private double ConvertirTasa(string texto)
        {
            double valor = ConvertirDouble(texto);

            if (valor > 1.0)
            {
                return valor / 100.0;
            }

            return valor;
        }

        private string FormatearMonedaVisual(double valorCLP)
        {
            string moneda = cotizacion.MonedaVisualizacion;

            double valorConvertido = ConvertirDesdeCLP(valorCLP, moneda);

            if (moneda == "CLP")
            {
                return "$ " + FormatearMiles(valorConvertido);
            }

            if (moneda == "UF")
            {
                return valorConvertido.ToString("0.00") + " UF";
            }

            if (moneda == "JPY")
            {
                return "JPY " + valorConvertido.ToString("N0");
            }

            return moneda + " " + valorConvertido.ToString("N2");
        }

        private string ValorTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "No informado";
            }

            return texto.Trim();
        }

        private string EscaparHtml(string texto)
        {
            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}