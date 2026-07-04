using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private bool easterEggGanttColoresActivado = false;

        private System.Collections.Generic.HashSet<Keys> teclasPresionadasEasterEgg =
            new System.Collections.Generic.HashSet<Keys>();

        private bool monedaOcultaActivadaEasterEgg = false;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool ctrlPresionado = (keyData & Keys.Control) == Keys.Control;
            Keys tecla = keyData & Keys.KeyCode;

            /*
             * Ctrl + W:
             * alterna modo oscuro.
             *
             * Lo dejamos acá y NO en Form1_KeyDown para evitar duplicados
             * con otros partials de Form1.
             */
            if (ctrlPresionado && tecla == Keys.W)
            {
                AlternarModoOscuro();
                return true;
            }

            if (!ctrlPresionado)
            {
                teclasPresionadasEasterEgg.Clear();
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (tecla == Keys.A || tecla == Keys.S || tecla == Keys.D)
            {
                teclasPresionadasEasterEgg.Add(tecla);

                bool combinacionMonedaOculta =
                    teclasPresionadasEasterEgg.Contains(Keys.A) &&
                    teclasPresionadasEasterEgg.Contains(Keys.S) &&
                    teclasPresionadasEasterEgg.Contains(Keys.D);

                bool combinacionGanttColores =
                    teclasPresionadasEasterEgg.Contains(Keys.A) &&
                    teclasPresionadasEasterEgg.Contains(Keys.D) &&
                    !teclasPresionadasEasterEgg.Contains(Keys.S);

                if (combinacionMonedaOculta)
                {
                    teclasPresionadasEasterEgg.Clear();
                    AlternarUnidadesOcultas();
                    return true;
                }

                if (combinacionGanttColores)
                {
                    teclasPresionadasEasterEgg.Clear();
                    AlternarColoresSecretosGantt();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            /*
             * Este método queda por compatibilidad si Form1.cs todavía conecta:
             *
             * KeyDown -= Form1_KeyDown;
             * KeyDown += Form1_KeyDown;
             *
             * La detección real de atajos se hace en ProcessCmdKey().
             * No pongas Ctrl+W aquí para evitar duplicar lógica.
             */
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                teclasPresionadasEasterEgg.Clear();
            }
        }

        private void AlternarUnidadesOcultas()
        {
            if (!monedaOcultaActivadaEasterEgg)
            {
                ActivarUnidadesOcultas();
                return;
            }

            DesactivarUnidadesOcultas();
        }

        private void ActivarUnidadesOcultas()
        {
            monedaOcultaActivadaEasterEgg = true;

            InvocarMetodoPrivado(
                "AsegurarTipoCambio",
                "GRM",
                "Gramo secreto",
                1000.0,
                "Easter egg interno",
                true
            );

            InvocarMetodoPrivado(
                "AsegurarTipoCambio",
                "PKM",
                "Sobre cartas Pokémon",
                6000.0,
                "Easter egg interno",
                true
            );

            InvocarMetodoPrivado("CargarCombosMonedas");
            InvocarMetodoPrivado("CargarTablaTiposCambio");

            CambiarTextoEstadoTiposCambio(
                "Modo oculto activado: GRM y PKM agregados a la biblioteca. 1 GRM = 1000 CLP | 1 PKM = 6000 CLP."
            );

            InvocarMetodoPrivado("RefrescarTodo");

            MessageBox.Show(
                "Modo oculto activado.\n\n" +
                "Se agregaron unidades secretas a la biblioteca:\n\n" +
                "1 GRM = 1000 CLP\n" +
                "1 PKM = 6000 CLP\n\n" +
                "PKM representa un sobre de cartas Pokémon.",
                "Easter egg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void DesactivarUnidadesOcultas()
        {
            monedaOcultaActivadaEasterEgg = false;

            QuitarTipoCambioOculto("GRM");
            QuitarTipoCambioOculto("PKM");

            ForzarMonedasOcultasAClpSiEstanSeleccionadas();

            InvocarMetodoPrivado("CargarCombosMonedas");
            InvocarMetodoPrivado("CargarTablaTiposCambio");

            ComboBox cmbMonedaVisualizacion =
                ObtenerCampoPrivadoComo<ComboBox>("cmbMonedaVisualizacion");

            if (cmbMonedaVisualizacion != null)
            {
                cmbMonedaVisualizacion.SelectedItem = "CLP";
                cmbMonedaVisualizacion.Text = "CLP";
            }

            CambiarTextoEstadoTiposCambio(
                "Modo oculto desactivado: GRM y PKM fueron removidos de la biblioteca."
            );

            InvocarMetodoPrivado("RefrescarTodo");

            MessageBox.Show(
                "Modo oculto desactivado.\n\n" +
                "Se quitaron GRM y PKM de la biblioteca.",
                "Easter egg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void QuitarTipoCambioOculto(string codigo)
        {
            object cotizacion = ObtenerCampoPrivado("cotizacion");

            if (cotizacion == null)
            {
                return;
            }

            PropertyInfo propiedadTiposCambio =
                cotizacion.GetType().GetProperty("TiposCambio");

            if (propiedadTiposCambio == null)
            {
                return;
            }

            object listaObj = propiedadTiposCambio.GetValue(cotizacion);

            if (listaObj == null)
            {
                return;
            }

            System.Collections.IList lista = listaObj as System.Collections.IList;

            if (lista == null)
            {
                return;
            }

            object encontrado = null;

            foreach (object item in lista)
            {
                if (item == null)
                {
                    continue;
                }

                PropertyInfo propiedadCodigo =
                    item.GetType().GetProperty("Codigo");

                if (propiedadCodigo == null)
                {
                    continue;
                }

                object valorCodigo = propiedadCodigo.GetValue(item);

                if (valorCodigo == null)
                {
                    continue;
                }

                if (valorCodigo.ToString().Trim().Equals(
                    codigo,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    encontrado = item;
                    break;
                }
            }

            if (encontrado != null)
            {
                lista.Remove(encontrado);
            }
        }

        private void ForzarMonedasOcultasAClpSiEstanSeleccionadas()
        {
            object cotizacion = ObtenerCampoPrivado("cotizacion");

            if (cotizacion == null)
            {
                return;
            }

            CambiarPropiedadMonedaOcultaAClp(cotizacion, "MonedaVisualizacion");
            CambiarPropiedadMonedaOcultaAClp(cotizacion, "Moneda");
            CambiarPropiedadMonedaOcultaAClp(cotizacion, "MonedaPrecioCliente");
            CambiarPropiedadMonedaOcultaAClp(cotizacion, "MonedaPresupuestoCliente");
        }

        private void CambiarPropiedadMonedaOcultaAClp(
            object objeto,
            string nombrePropiedad
        )
        {
            if (objeto == null)
            {
                return;
            }

            PropertyInfo propiedad = objeto.GetType().GetProperty(nombrePropiedad);

            if (propiedad == null || !propiedad.CanRead || !propiedad.CanWrite)
            {
                return;
            }

            object valor = propiedad.GetValue(objeto);

            if (valor == null)
            {
                return;
            }

            string moneda = valor.ToString().Trim().ToUpperInvariant();

            if (moneda == "GRM" || moneda == "PKM")
            {
                propiedad.SetValue(objeto, "CLP");
            }
        }

        private void AlternarColoresSecretosGantt()
        {
            easterEggGanttColoresActivado = !easterEggGanttColoresActivado;

            Panel panelGantt = ObtenerCampoPrivadoComo<Panel>("panelGantt");

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            string mensaje = easterEggGanttColoresActivado
                ? "Modo visual secreto activado: Gantt causal."
                : "Modo visual secreto desactivado: Gantt normal.";

            CambiarTextoEstadoTiposCambio(mensaje);

            MessageBox.Show(
                mensaje,
                "Easter egg Gantt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void AplicarColoresSecretosGantt(
            string nombreEtapa,
            ref Color colorBarra,
            ref Color colorBorde,
            ref Color colorFila
        )
        {
            if (!easterEggGanttColoresActivado)
            {
                return;
            }

            string etapa = NormalizarNombreEtapaEasterEgg(nombreEtapa);

            switch (etapa)
            {
                case "desarrollo":
                    colorBarra = Color.FromArgb(238, 30, 91);
                    colorBorde = Color.FromArgb(150, 10, 55);
                    colorFila = Color.FromArgb(255, 205, 220);
                    break;

                case "preproduccion":
                    colorBarra = Color.FromArgb(251, 242, 131);
                    colorBorde = Color.FromArgb(180, 150, 35);
                    colorFila = Color.FromArgb(255, 248, 190);
                    break;

                case "produccion":
                    colorBarra = Color.FromArgb(83, 192, 166);
                    colorBorde = Color.FromArgb(35, 120, 105);
                    colorFila = Color.FromArgb(200, 245, 235);
                    break;

                case "postproduccion":
                    colorBarra = Color.FromArgb(120, 80, 210);
                    colorBorde = Color.FromArgb(70, 40, 140);
                    colorFila = Color.FromArgb(220, 210, 250);
                    break;

                default:
                    colorBarra = Color.FromArgb(40, 40, 40);
                    colorBorde = Color.Black;
                    colorFila = Color.FromArgb(230, 230, 230);
                    break;
            }
        }

        private string NormalizarNombreEtapaEasterEgg(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return string.Empty;
            }

            return nombre
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace(" ", "");
        }

        private void CambiarTextoEstadoTiposCambio(string texto)
        {
            Label lblEstadoTiposCambio =
                ObtenerCampoPrivadoComo<Label>("lblEstadoTiposCambio");

            if (lblEstadoTiposCambio != null)
            {
                lblEstadoTiposCambio.Text = texto;
            }
        }

        private object ObtenerCampoPrivado(string nombreCampo)
        {
            FieldInfo campo = GetType().GetField(
                nombreCampo,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public
            );

            if (campo == null)
            {
                return null;
            }

            return campo.GetValue(this);
        }

        private T ObtenerCampoPrivadoComo<T>(string nombreCampo) where T : class
        {
            object valor = ObtenerCampoPrivado(nombreCampo);

            return valor as T;
        }

        private object InvocarMetodoPrivado(
            string nombreMetodo,
            params object[] parametros
        )
        {
            MethodInfo[] metodos = GetType()
                .GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.Public
                )
                .Where(m =>
                    m.Name == nombreMetodo &&
                    m.GetParameters().Length == parametros.Length
                )
                .ToArray();

            foreach (MethodInfo metodo in metodos)
            {
                ParameterInfo[] parametrosMetodo = metodo.GetParameters();

                bool compatible = true;

                for (int i = 0; i < parametrosMetodo.Length; i++)
                {
                    object valor = parametros[i];
                    Type tipoEsperado = parametrosMetodo[i].ParameterType;

                    if (valor == null)
                    {
                        if (tipoEsperado.IsValueType &&
                            Nullable.GetUnderlyingType(tipoEsperado) == null)
                        {
                            compatible = false;
                            break;
                        }

                        continue;
                    }

                    Type tipoValor = valor.GetType();

                    if (tipoEsperado.IsAssignableFrom(tipoValor))
                    {
                        continue;
                    }

                    Type tipoNullable = Nullable.GetUnderlyingType(tipoEsperado);

                    if (tipoNullable != null &&
                        tipoNullable.IsAssignableFrom(tipoValor))
                    {
                        continue;
                    }

                    compatible = false;
                    break;
                }

                if (!compatible)
                {
                    continue;
                }

                return metodo.Invoke(this, parametros);
            }

            MessageBox.Show(
                "No se encontró una versión compatible del método interno:\n\n" +
                nombreMetodo +
                "\n\nRevisa los parámetros usados por el easter egg.",
                "Error interno easter egg",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return null;
        }
    }
}