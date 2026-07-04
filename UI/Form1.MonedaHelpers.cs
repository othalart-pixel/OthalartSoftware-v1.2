using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private double ConvertirDesdeCLP(double valorCLP, string? monedaDestino = null)
        {
            string moneda = NormalizarCodigoMoneda(monedaDestino ?? ObtenerMonedaVisualActual());

            if (moneda == "CLP")
            {
                return valorCLP;
            }

            double tipoCambio = ObtenerTipoCambioCLP(moneda);

            if (tipoCambio <= 0)
            {
                return valorCLP;
            }

            return valorCLP / tipoCambio;
        }

        private double ConvertirHaciaCLP(double valor, string? monedaOrigen = null)
        {
            string moneda = NormalizarCodigoMoneda(monedaOrigen ?? ObtenerMonedaVisualActual());

            if (moneda == "CLP")
            {
                return valor;
            }

            double tipoCambio = ObtenerTipoCambioCLP(moneda);

            if (tipoCambio <= 0)
            {
                return valor;
            }

            return valor * tipoCambio;
        }

        private string FormatearValorVisual(double valorCLP)
        {
            string moneda = ObtenerMonedaVisualActual();
            double valorVisual = ConvertirDesdeCLP(valorCLP, moneda);

            return FormatearValorVisual(valorVisual, moneda);
        }

        private string FormatearValorVisual(double valor, string? moneda)
        {
            string codigo = NormalizarCodigoMoneda(moneda ?? "CLP");
            CultureInfo cultura = new CultureInfo("es-CL");

            if (codigo == "CLP")
            {
                return "$" + Math.Round(valor, 0).ToString("N0", cultura) + " CLP";
            }

            if (codigo == "USD")
            {
                return "US$ " + valor.ToString("N2", cultura);
            }

            if (codigo == "EUR")
            {
                return "€ " + valor.ToString("N2", cultura);
            }

            if (codigo == "JPY" || codigo == "YEN")
            {
                return "¥ " + Math.Round(valor, 0).ToString("N0", cultura);
            }

            return valor.ToString("N2", cultura) + " " + codigo;
        }

        private void AplicarEstiloTablaCentrada(DataGridView tabla)
        {
            if (tabla == null)
            {
                return;
            }

            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeRows = false;
            tabla.RowHeadersVisible = false;
            tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            tabla.MultiSelect = false;
            tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            tabla.BackgroundColor = Color.White;
            tabla.BorderStyle = BorderStyle.None;
            tabla.GridColor = Color.Gainsboro;

            tabla.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font(tabla.Font, FontStyle.Bold);

            tabla.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            tabla.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP)
        {
            string codigoNormalizado = NormalizarCodigoMoneda(codigo);

            try
            {
                object? cotizacionActual = cotizacion;

                if (cotizacionActual == null)
                {
                    return;
                }

                PropertyInfo? propTiposCambio = cotizacionActual.GetType().GetProperty("TiposCambio");

                if (propTiposCambio == null)
                {
                    return;
                }

                object? listaObj = propTiposCambio.GetValue(cotizacionActual);

                if (listaObj == null)
                {
                    return;
                }

                IEnumerable? lista = listaObj as IEnumerable;

                if (lista == null)
                {
                    return;
                }

                object? existente = null;

                foreach (object item in lista)
                {
                    string codigoItem = ObtenerStringPropiedad(item, "Codigo", "Código", "Moneda", "CodigoMoneda", "Nombre");

                    if (NormalizarCodigoMoneda(codigoItem) == codigoNormalizado)
                    {
                        existente = item;
                        break;
                    }
                }

                if (existente != null)
                {
                    SetearStringSiExiste(existente, nombre, "Nombre", "Descripcion", "Descripción");
                    SetearDoubleSiExiste(existente, valorCLP, "ValorCLP", "Valor", "Tasa", "TasaCLP", "PesosPorUnidad", "ValorEnCLP", "TipoCambioCLP");
                    return;
                }

                Type? tipoElemento = ObtenerTipoElementoLista(listaObj);

                if (tipoElemento == null)
                {
                    return;
                }

                object? nuevo = Activator.CreateInstance(tipoElemento);

                if (nuevo == null)
                {
                    return;
                }

                SetearStringSiExiste(nuevo, codigoNormalizado, "Codigo", "Código", "Moneda", "CodigoMoneda");
                SetearStringSiExiste(nuevo, nombre, "Nombre", "Descripcion", "Descripción");
                SetearDoubleSiExiste(nuevo, valorCLP, "ValorCLP", "Valor", "Tasa", "TasaCLP", "PesosPorUnidad", "ValorEnCLP", "TipoCambioCLP");

                MethodInfo? metodoAdd = listaObj.GetType().GetMethod("Add");

                if (metodoAdd != null)
                {
                    metodoAdd.Invoke(listaObj, new object[] { nuevo });
                }
            }
            catch
            {
                // Intencionalmente silencioso:
                // este método asegura compatibilidad entre módulos viejos y nuevos de moneda.
            }
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, string fuente, DateTime fechaActualizacion)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, string fuente, string fechaActualizacion)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, DateTime fechaActualizacion, string fuente)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, string fuente, bool esEditable)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, bool esEditable, string fuente)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, DateTime fechaActualizacion, bool esEditable)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private void AsegurarTipoCambio(string codigo, string nombre, double valorCLP, bool esEditable, DateTime fechaActualizacion)
        {
            AsegurarTipoCambio(codigo, nombre, valorCLP);
        }

        private string ObtenerMonedaVisualActual()
        {
            try
            {
                if (cotizacion != null && !string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion))
                {
                    return NormalizarCodigoMoneda(cotizacion.MonedaVisualizacion);
                }
            }
            catch
            {
            }

            return "CLP";
        }

        private string NormalizarCodigoMoneda(string? moneda)
        {
            if (string.IsNullOrWhiteSpace(moneda))
            {
                return "CLP";
            }

            string codigo = moneda.Trim().ToUpperInvariant();

            if (codigo.Contains("CLP") || codigo.Contains("PESO"))
            {
                return "CLP";
            }

            if (codigo.Contains("USD") || codigo.Contains("DOLAR") || codigo.Contains("DÓLAR"))
            {
                return "USD";
            }

            if (codigo.Contains("EUR") || codigo.Contains("EURO"))
            {
                return "EUR";
            }

            if (codigo.Contains("JPY") || codigo.Contains("YEN") || codigo.Contains("¥"))
            {
                return "JPY";
            }

            return codigo;
        }

        private double ObtenerTipoCambioCLP(string moneda)
        {
            string codigo = NormalizarCodigoMoneda(moneda);

            if (codigo == "CLP")
            {
                return 1.0;
            }

            try
            {
                object? cotizacionActual = cotizacion;

                if (cotizacionActual != null)
                {
                    PropertyInfo? propTiposCambio = cotizacionActual.GetType().GetProperty("TiposCambio");
                    object? listaObj = propTiposCambio?.GetValue(cotizacionActual);
                    IEnumerable? lista = listaObj as IEnumerable;

                    if (lista != null)
                    {
                        foreach (object item in lista)
                        {
                            string codigoItem = ObtenerStringPropiedad(item, "Codigo", "Código", "Moneda", "CodigoMoneda", "Nombre");

                            if (NormalizarCodigoMoneda(codigoItem) == codigo)
                            {
                                double valor = ObtenerDoublePropiedad(item, "ValorCLP", "Valor", "Tasa", "TasaCLP", "PesosPorUnidad", "ValorEnCLP", "TipoCambioCLP");

                                if (valor > 0)
                                {
                                    if (codigo == "USD" && (valor < 100.0 || valor > 5000.0))
                                    {
                                        return 950.0;
                                    }

                                    if (codigo == "EUR" && (valor < 100.0 || valor > 6000.0))
                                    {
                                        return 1050.0;
                                    }

                                    if (codigo == "JPY" && (valor < 1.0 || valor > 100.0))
                                    {
                                        return 6.5;
                                    }

                                    return valor;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            if (codigo == "USD")
            {
                return 950.0;
            }

            if (codigo == "EUR")
            {
                return 1050.0;
            }

            if (codigo == "JPY")
            {
                return 6.5;
            }

            return 1.0;
        }

        private string ObtenerStringPropiedad(object objeto, params string[] nombres)
        {
            foreach (string nombre in nombres)
            {
                PropertyInfo? prop = objeto.GetType().GetProperty(nombre);

                if (prop == null)
                {
                    continue;
                }

                object? valor = prop.GetValue(objeto);

                if (valor != null)
                {
                    return Convert.ToString(valor) ?? "";
                }
            }

            return "";
        }

        private double ObtenerDoublePropiedad(object objeto, params string[] nombres)
        {
            foreach (string nombre in nombres)
            {
                PropertyInfo? prop = objeto.GetType().GetProperty(nombre);

                if (prop == null)
                {
                    continue;
                }

                object? valor = prop.GetValue(objeto);

                if (valor == null)
                {
                    continue;
                }

                if (double.TryParse(Convert.ToString(valor), NumberStyles.Any, CultureInfo.InvariantCulture, out double numeroInvariant))
                {
                    return numeroInvariant;
                }

                if (double.TryParse(Convert.ToString(valor), NumberStyles.Any, new CultureInfo("es-CL"), out double numeroCL))
                {
                    return numeroCL;
                }
            }

            return 0;
        }

        private void SetearStringSiExiste(object objeto, string valor, params string[] nombres)
        {
            foreach (string nombre in nombres)
            {
                PropertyInfo? prop = objeto.GetType().GetProperty(nombre);

                if (prop == null || !prop.CanWrite)
                {
                    continue;
                }

                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(objeto, valor);
                    return;
                }
            }
        }

        private void SetearDoubleSiExiste(object objeto, double valor, params string[] nombres)
        {
            foreach (string nombre in nombres)
            {
                PropertyInfo? prop = objeto.GetType().GetProperty(nombre);

                if (prop == null || !prop.CanWrite)
                {
                    continue;
                }

                if (prop.PropertyType == typeof(double))
                {
                    prop.SetValue(objeto, valor);
                    return;
                }

                if (prop.PropertyType == typeof(decimal))
                {
                    prop.SetValue(objeto, Convert.ToDecimal(valor));
                    return;
                }

                if (prop.PropertyType == typeof(float))
                {
                    prop.SetValue(objeto, Convert.ToSingle(valor));
                    return;
                }

                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(objeto, Convert.ToInt32(valor));
                    return;
                }
            }
        }

        private Type? ObtenerTipoElementoLista(object listaObj)
        {
            Type tipoLista = listaObj.GetType();

            if (tipoLista.IsGenericType)
            {
                Type[] argumentos = tipoLista.GetGenericArguments();

                if (argumentos.Length > 0)
                {
                    return argumentos[0];
                }
            }

            Type? tipoEnumerable = tipoLista
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (tipoEnumerable != null)
            {
                return tipoEnumerable.GetGenericArguments()[0];
            }

            return null;
        }
    }
}
