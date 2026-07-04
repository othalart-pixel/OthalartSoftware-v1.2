using System;
using System.Linq;
using System.Threading.Tasks;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void InicializarCotizacion()
        {
            cotizacion = new Cotizacion();
            cotizacion.BriefProducto = new BriefProductoProyecto();

            cotizacion.Moneda = "CLP";
            cotizacion.MonedaVisualizacion = "CLP";
            cotizacion.MonedaPrecioCliente = "CLP";

            cotizacion.MargenObjetivo = 0.30;
            cotizacion.TasaImprevistos = 0.10;

            bibliotecaEtapas = BibliotecaEtapasJsonService.CargarEtapas();
            cotizacion.Etapas = BibliotecasEtapas.CrearEtapasBase(bibliotecaEtapas);

            bibliotecaSubEtapas = BibliotecaSubEtapasJsonService.CargarSubEtapas();

            InicializarTiposCambio();

            InicializarCargosGenerales();
            SincronizarCargosGeneralesEnTodasLasEtapas();
        }

        private async Task ActualizarTiposCambioAlIniciar()
        {
            if (lblEstadoTiposCambio != null)
            {
                lblEstadoTiposCambio.Text = "Actualizando indicadores automáticamente...";
            }

            bool actualizado = false;

            try
            {
                actualizado = await ActualizarIndicadoresMindicadorConTimeout();
            }
            catch
            {
                actualizado = false;
            }

            CargarCombosMonedas();
            CargarTablaTiposCambio();
            RefrescarCalculosYVista();

            if (lblEstadoTiposCambio != null)
            {
                lblEstadoTiposCambio.Text = actualizado
                    ? "Indicadores actualizados automáticamente al iniciar."
                    : "Sin respuesta online en 3 segundos. Usando valores actuales de la biblioteca.";
            }
        }

        private void RefrescarTodo()
        {
            InicializarTiposCambio();
            RecalcularCostosExtra();

            ServicioCotizacion.RecalcularCotizacion(cotizacion);
            SincronizarEtapasInternasDesdeDesgloseProductivo();

            CargarCombosMonedas();
            CargarDatosEnPantalla();
            CargarTablaTiposCambio();

            CargarTablaEtapas();
            CargarCombos();
            CargarTablaManoObra();

            CargarCostosEnPantalla();

            /*
             * Importante:
             * Cargos depende de Subetapas, porque el checklist se alimenta
             * de los subprocesos activos.
             *
             * Por eso Subetapas debe refrescarse antes que Cargos.
             */
            RefrescarTablaSubEtapasSiExiste();
            RefrescarVistaCargosSiExiste();

            RefrescarResumen();
            RefrescarResultadosDetalle();

            ActualizarBloqueoPestanas();

            if (webInformeCliente != null)
            {
                webInformeCliente.DocumentText = GenerarHtmlInformeCliente();
            }

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            InicializarMonedaVisualDependienteDeTabla();
            RefrescarTextoMonedaVisual();

            RefrescarGanttGrandeEtapas();

            if (modoOscuroActivo)
            {
                AplicarModoOscuroActual();
            }
        }

        private void RefrescarCalculosYVista()
        {
            InicializarTiposCambio();
            RecalcularCostosExtra();

            ServicioCotizacion.RecalcularCotizacion(cotizacion);

            CargarCombosMonedas();
            CargarTablaTiposCambio();

            CargarTablaEtapas();
            CargarCombos();
            CargarTablaManoObra();

            CargarCostosEnPantalla();

            /*
             * Importante:
             * Cargos depende de Subetapas, porque el checklist se alimenta
             * de los subprocesos activos.
             *
             * Por eso Subetapas debe refrescarse antes que Cargos.
             */
            RefrescarTablaSubEtapasSiExiste();
            RefrescarVistaCargosSiExiste();

            RefrescarResumen();
            RefrescarResultadosDetalle();

            ActualizarBloqueoPestanas();

            if (webInformeCliente != null)
            {
                webInformeCliente.DocumentText = GenerarHtmlInformeCliente();
            }

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            RefrescarGanttGrandeEtapas();

            if (modoOscuroActivo)
            {
                AplicarModoOscuroActual();
            }
        }

        private void RefrescarTablaSubEtapasSiExiste()
        {
            if (dgvSubEtapas == null)
            {
                return;
            }

            if (dgvSubEtapas.Columns.Count == 0)
            {
                return;
            }

            RefrescarTablaSubEtapas();
        }

        private void RefrescarVistaCargosSiExiste()
        {
            if (dgvBibliotecaCargos == null)
            {
                return;
            }

            if (cmbEtapaBibliotecaCargos == null)
            {
                return;
            }

            if (dgvBibliotecaCargos.Columns.Count == 0)
            {
                return;
            }

            /*
             * La pestaña Cargos depende de:
             * - etapas activas
             * - subetapas activas
             * - recomendaciones por subetapa
             *
             * Por eso debe refrescarse después de Subetapas.
             */
            CargarComboEtapasBibliotecaCargos();

            if (EstaEnModoChecklistCargos())
            {
                CargarTablaChecklistCargosProyecto();
            }
            else
            {
                CargarTablaBibliotecaCargos();
            }
        }

        private double DuracionVisibleProyecto()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return 0.0;
            }

            return cotizacion.Etapas
                .Where(etapa => etapa.Seleccionada)
                .Select(etapa => etapa.FinMes)
                .DefaultIfEmpty(0.0)
                .Max();
        }
    }
}
