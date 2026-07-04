using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void InicializarCotizacion()
        {
            if (cotizacion == null)
            {
                cotizacion = new Cotizacion();
            }

            if (cotizacion.Etapas == null || cotizacion.Etapas.Count == 0)
            {
                bibliotecaEtapas = BibliotecaEtapasJsonService.CargarEtapas();
                cotizacion.Etapas = BibliotecasEtapas.CrearEtapasBase(bibliotecaEtapas);
            }
            else if (bibliotecaEtapas == null || bibliotecaEtapas.Count == 0)
            {
                bibliotecaEtapas = BibliotecaEtapasJsonService.CargarEtapas();
            }

            InicializarTiposCambio();

            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                bibliotecaSubEtapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();
            }

            InicializarCargosGenerales();
            SincronizarCargosGeneralesEnTodasLasEtapas();

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

        private void RefrescarTodo()
        {
            try
            {
                ServicioCotizacion.RecalcularCotizacion(cotizacion);
            }
            catch
            {
                // Evita que el formulario reviente si todavía falta algo por construir.
            }

            InvocarMetodoSiExiste("CargarDatosEnPantalla");
            InvocarMetodoSiExiste("CargarCombosMonedas");
            InvocarMetodoSiExiste("CargarTablaTiposCambio");

            InvocarMetodoSiExiste("CargarTablaEtapas");
            InvocarMetodoSiExiste("CargarCombos");
            InvocarMetodoSiExiste("CargarTablaManoObra");

            InvocarMetodoSiExiste("CargarCostosEnPantalla");
            InvocarMetodoSiExiste("CargarComboEtapasBibliotecaCargos");
            InvocarMetodoSiExiste("CargarTablaBibliotecaCargos");
            InvocarMetodoSiExiste("RefrescarTablaSubEtapasSiExiste");

            InvocarMetodoSiExiste("RefrescarResumen");
            InvocarMetodoSiExiste("RefrescarResultadosDetalle");
            InvocarMetodoSiExiste("ActualizarBloqueoPestanas");

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }
        }

        private async Task ActualizarTiposCambioAlIniciar()
        {
            bool actualizado = await ActualizarIndicadoresMindicadorConTimeout();

            InvocarMetodoSiExiste("CargarCombosMonedas");
            InvocarMetodoSiExiste("CargarTablaTiposCambio");

            try
            {
                ServicioCotizacion.RecalcularCotizacion(cotizacion);
            }
            catch
            {
                // No bloquear inicio si el cálculo todavía no está listo.
            }

            InvocarMetodoSiExiste("RefrescarResumen");
            InvocarMetodoSiExiste("RefrescarResultadosDetalle");

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            if (lblEstadoTiposCambio != null)
            {
                lblEstadoTiposCambio.Text = actualizado
                    ? "Biblioteca de indicadores actualizada online."
                    : "Sin respuesta online en 3 segundos. Se mantienen los valores actuales.";
            }
        }

        private void InvocarMetodoSiExiste(string nombreMetodo)
        {
            try
            {
                MethodInfo? metodo = GetType().GetMethod(
                    nombreMetodo,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                if (metodo == null)
                {
                    return;
                }

                if (metodo.GetParameters().Length > 0)
                {
                    return;
                }

                metodo.Invoke(this, null);
            }
            catch
            {
                // Método opcional: si falla, no debe romper la carga completa.
            }
        }
    }
}
