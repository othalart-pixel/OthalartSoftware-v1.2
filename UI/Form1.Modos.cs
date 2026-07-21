using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void AplicarModoAplicacion(ModoAplicacion modo, bool registrarHistorial = true)
        {
            EstadoNavegacionPrincipal origen = ObtenerEstadoNavegacionActual();
            modoAplicacionActual = modo;

            if (tabs == null)
            {
                return;
            }

            if (modo == ModoAplicacion.Inicio)
            {
                RefrescarTabInicio();
            }

            TabPage seleccionDeseada = modo == ModoAplicacion.Inicio
                ? tabInicioPrincipal
                : modo == ModoAplicacion.Proyecto
                    ? tabDatosPrincipal
                    : tabProductosPrincipal;

            List<TabPage> visibles = ObtenerTabsParaModo(modo)
                .Where(t => t != null)
                .ToList();

            cambiandoTabInternamente = true;
            tabs.TabPages.Clear();
            foreach (TabPage tab in visibles)
            {
                tabs.TabPages.Add(tab);
            }

            if (seleccionDeseada != null && tabs.TabPages.Contains(seleccionDeseada))
            {
                tabs.SelectedTab = seleccionDeseada;
            }
            else if (tabs.TabPages.Count > 0)
            {
                tabs.SelectedIndex = 0;
            }
            cambiandoTabInternamente = false;

            ActualizarEncabezadoModoAplicacion();
            ActualizarVisibilidadPanelDerecho();
            tabs.Invalidate();
            CompletarCambioNavegacion(origen, tabs.SelectedTab, modo, registrarHistorial);
        }

        private IEnumerable<TabPage> ObtenerTabsParaModo(ModoAplicacion modo)
        {
            if (modo == ModoAplicacion.Inicio)
            {
                yield return tabInicioPrincipal;
                yield break;
            }

            if (modo == ModoAplicacion.Proyecto)
            {
                yield return tabInicioPrincipal;
                yield return tabDatosPrincipal;
                yield return tabProyectoPrincipal;
                yield return tabDesgloseProductivoPrincipal;
                yield return tabManoObraPrincipal;
                yield return tabSubEtapasPrincipal;
                yield return tabResultadosPrincipal;
                yield return tabInformePrincipal;
                yield return tabValidacionJsonPrincipal;
                yield return tabGuardarPrincipal;
                yield break;
            }

            yield return tabInicioPrincipal;

            if (proyectoCotizacionActual != null)
            {
                yield return tabProyectoPrincipal;
                yield return tabDatosPrincipal;
                yield return tabDesgloseProductivoPrincipal;
                yield return tabManoObraPrincipal;
                yield return tabResultadosPrincipal;
                yield return tabInformePrincipal;
                yield return tabGuardarPrincipal;
            }

            yield return tabProductosPrincipal;
            yield return tabEcuacionesPrincipal;
            yield return tabGestionesPrincipal;
            yield return tabSubEtapasPrincipal;
            yield return tabRangosPrincipal;
            yield return tabCargosPrincipal;
            yield return tabPersonalPrincipal;
            yield return tabRendimientosPrincipal;
            yield return tabMonedaPrincipal;
            yield return tabValidacionJsonPrincipal;
        }

        private void ActualizarEncabezadoModoAplicacion()
        {
            if (lblModoAplicacion == null)
            {
                return;
            }

            string nombreProyecto = cotizacion == null || string.IsNullOrWhiteSpace(cotizacion.NombreProyecto)
                ? "Sin proyecto"
                : cotizacion.NombreProyecto.Trim();

            switch (modoAplicacionActual)
            {
                case ModoAplicacion.Proyecto:
                    lblModoAplicacion.Text = "Othalart | Proyecto: " + nombreProyecto;
                    break;
                case ModoAplicacion.Configuracion:
                    lblModoAplicacion.Text = "Othalart | Configuracion de bibliotecas";
                    break;
                default:
                    lblModoAplicacion.Text = "Othalart | Inicio";
                    break;
            }

            if (lblEstadoGuardadoGlobal != null)
            {
                lblEstadoGuardadoGlobal.Text = proyectoTieneCambiosPendientes
                    ? "Cambios pendientes"
                    : "Guardado";
            }
        }

        private void MarcarProyectoConCambiosPendientes()
        {
            proyectoTieneCambiosPendientes = true;
            ActualizarEncabezadoModoAplicacion();
        }

        private void MarcarProyectoGuardado()
        {
            proyectoTieneCambiosPendientes = false;
            ActualizarEncabezadoModoAplicacion();
        }
    }
}
