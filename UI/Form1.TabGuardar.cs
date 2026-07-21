using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Reports;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private bool cargandoProyectoDesdeArchivo = false;

        private void ConstruirTabGuardar(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Top;
            layout.Padding = new Padding(25);
            layout.ColumnCount = 1;
            layout.RowCount = 6;
            layout.AutoSize = true;

            Label titulo = new Label();
            titulo.Text = "Guardar cotización";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.AutoSize = true;

            Label descripcion = new Label();
            descripcion.Text =
                "Desde aquí puedes guardar el proyecto completo o exportar la cotización.\n\n" +
                "Proyecto editable: archivo para continuar, modificar y revisar después.\n" +
                "Excel interno: archivo para análisis del equipo, con etapas, mano de obra y costos.\n" +
                "Informe cliente: versión visual limpia para enviar o revisar con el cliente.";

            descripcion.Font = new Font("Segoe UI", 10);
            descripcion.AutoSize = true;
            descripcion.Padding = new Padding(0, 10, 0, 15);

            btnGuardarExcelInterno.Text = "Exportar Excel";
            btnGuardarExcelInterno.Width = 190;
            btnGuardarExcelInterno.Height = 36;
            btnGuardarExcelInterno.Click += BtnGuardarExcelInterno_Click;

            btnGuardarInformeHtml.Text = "Exportar informe PDF";
            btnGuardarInformeHtml.Width = 190;
            btnGuardarInformeHtml.Height = 36;
            btnGuardarInformeHtml.Click += BtnGuardarInformeHtml_Click;

            lblGuardarEstado.Text = "";
            lblGuardarEstado.AutoSize = true;
            lblGuardarEstado.Font = new Font("Segoe UI", 10);
            lblGuardarEstado.Padding = new Padding(0, 15, 0, 0);

            layout.Controls.Add(titulo, 0, 0);
            layout.Controls.Add(descripcion, 0, 1);
            layout.Controls.Add(btnGuardarExcelInterno, 0, 2);
            layout.Controls.Add(btnGuardarInformeHtml, 0, 3);
            layout.Controls.Add(lblGuardarEstado, 0, 4);

            tab.Controls.Add(layout);
        }

        private void BtnGuardarProyecto_Click(object sender, EventArgs e)
        {
            try
            {
                AplicarDatosDesdePantalla();
                GuardarDesgloseProductivoDesdePantalla();
                GuardarAsignacionesManoObraDesdeTabla(false);
                ServicioCotizacion.RecalcularCotizacion(cotizacion);

                ProyectoOthalartGuardado proyecto = CrearSnapshotProyecto();
                string rutaGuardado = ResolverRutaGuardadoProyectoActual();
                if (string.IsNullOrWhiteSpace(rutaGuardado))
                {
                    lblGuardarEstado.Text = "Guardado de proyecto cancelado.";
                    lblGuardarEstado.ForeColor = Color.DimGray;
                    return;
                }

                ProyectoOthalartArchivoService.Guardar(rutaGuardado, proyecto);

                cotizacion.NombreArchivo = rutaGuardado;
                if (cotizacion.ProyectoProductivo != null)
                {
                    cotizacion.ProyectoProductivo.FechaModificacion = DateTime.Now;
                }

                ProyectosRecientesService.Registrar(
                    rutaGuardado,
                    cotizacion.NombreProyecto,
                    cotizacion.NombreCliente
                );
                RefrescarTabInicio();

                int piezasGuardadas = proyecto.Piezas2DDatosGuardadas == null
                    ? 0
                    : proyecto.Piezas2DDatosGuardadas.Count;
                int piezasSeleccionadas = proyecto.Piezas2DDatosGuardadas == null
                    ? 0
                    : proyecto.Piezas2DDatosGuardadas.Count(p => p != null && p.Usar);

                lblGuardarEstado.Text =
                    "Proyecto guardado con sus bibliotecas en:\n" +
                    Path.GetDirectoryName(rutaGuardado) +
                    "\nPiezas: " +
                    piezasSeleccionadas.ToString("0") + "/" +
                    piezasGuardadas.ToString("0") + ".";
                lblGuardarEstado.ForeColor = Color.DarkGreen;
                MarcarProyectoGuardado();
            }
            catch (Exception ex)
            {
                lblGuardarEstado.Text = "No se pudo guardar el proyecto.";
                lblGuardarEstado.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    "No se pudo guardar el proyecto editable.\n\n" + ex.Message,
                    "Error al guardar proyecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string ResolverRutaGuardadoProyectoActual()
        {
            string rutaActual = cotizacion == null ? "" : cotizacion.NombreArchivo ?? "";
            if (!string.IsNullOrWhiteSpace(rutaActual))
            {
                return rutaActual;
            }

            return CrearRutaAutomaticaProyectoNuevo();
        }

        private string CrearRutaAutomaticaProyectoNuevo()
        {
            string nombre = cotizacion == null
                ? "Proyecto Othalart"
                : cotizacion.NombreProyecto;
            return ProyectoOthalartArchivoService.CrearRutaProyectoNuevo(nombre);
        }

        private void BtnCargarProyecto_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialogo = new OpenFileDialog())
                {
                    dialogo.Title = "Cargar proyecto Othalart";
                    dialogo.Filter = "Proyecto Othalart (*.othalart.json;*.json)|*.othalart.json;*.json";
                    dialogo.InitialDirectory =
                        ProyectoOthalartArchivoService.ObtenerCarpetaProyectosPrograma();

                    if (dialogo.ShowDialog(this) != DialogResult.OK)
                    {
                        lblGuardarEstado.Text = "Carga de proyecto cancelada.";
                        lblGuardarEstado.ForeColor = Color.DimGray;
                        return;
                    }

                    ProyectoOthalartGuardado proyecto =
                        ProyectoOthalartArchivoService.Cargar(dialogo.FileName);

                    if (proyecto == null || proyecto.Cotizacion == null)
                    {
                        throw new InvalidOperationException("El archivo no contiene un proyecto válido.");
                    }

                    cargandoProyectoDesdeArchivo = true;

                    try
                    {
                        AplicarSnapshotProyecto(proyecto, dialogo.FileName);
                    }
                    finally
                    {
                        cargandoProyectoDesdeArchivo = false;
                    }

                    ProyectosRecientesService.Registrar(
                        dialogo.FileName,
                        proyecto.Cotizacion.NombreProyecto,
                        proyecto.Cotizacion.NombreCliente
                    );
                    RefrescarTabInicio();

                    int piezasCargadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count;
                    int piezasSeleccionadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count(p => p != null && p.Usar);

                    lblGuardarEstado.Text =
                        $"Proyecto editable cargado correctamente. Piezas: {piezasSeleccionadas}/{piezasCargadas}.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
                    MarcarProyectoGuardado();
                    AplicarModoAplicacion(ModoAplicacion.Proyecto);
                    RefrescarDatosDesdeProyectoProductivo();
                }
            }
            catch (Exception ex)
            {
                lblGuardarEstado.Text = "No se pudo cargar el proyecto.";
                lblGuardarEstado.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    "No se pudo cargar el proyecto editable.\n\n" + ex.Message,
                    "Error al cargar proyecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private ProyectoOthalartGuardado CrearSnapshotProyecto()
        {
            CapturarItemProyectoActivoAntesDeGuardar();

            if (cotizacion != null)
            {
                AplicarConsolidadoProyectoAlSnapshot();
                cotizacion.ProyectoProductivo = proyectoCotizacionActual;
            }

            List<CategoriaTrabajador> cargosSnapshot =
                ObtenerBibliotecaCargosCompletaParaProyecto();
            List<PersonaEquipo> personalSnapshot =
                BibliotecaPersonalEmpresaJsonService.CargarPersonal();
            bibliotecaCargosProyectoInforme = cargosSnapshot;
            bibliotecaPersonalProyectoInforme = personalSnapshot;

            return new ProyectoOthalartGuardado
            {
                FechaGuardado = DateTime.Now,
                Cotizacion = cotizacion ?? new Cotizacion(),
                ProyectoProductivo = proyectoCotizacionActual,
                TipoSolicitudDatosGuardado = ObtenerTipoSolicitudDatosGuardado(),
                ProductoServicioDatosGuardado = ObtenerProductoServicioDatosGuardado(),
                Piezas2DDatosGuardadas = ObtenerPiezas2DDatosGuardadas(),
                BibliotecaEtapas = bibliotecaEtapas ?? BibliotecaEtapasJsonService.CargarEtapas(),
                BibliotecaSubEtapas = bibliotecaSubEtapas ?? BibliotecaSubEtapasJsonService.CargarSubEtapas(),
                BibliotecaCargos = cargosSnapshot,
                BibliotecaPersonal = personalSnapshot,
                BibliotecaProductos2D = BibliotecaProductos2D.CrearBase(),
                BibliotecaRendimientosProductivos =
                    BibliotecaRendimientosProductivosJsonService.CargarRendimientos(),
                BibliotecaEcuacionesProductivas =
                    BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones(),
                BibliotecaGestionesProductivas =
                    BibliotecaGestionesProductivasJsonService.CargarGestiones(),
                ResolucionesDependenciasSubEtapas =
                    resolucionesDependenciasSubEtapas ??
                    new List<ResolucionDependenciaSubEtapa>(),
                ModoOscuroActivo = modoOscuroActivo
            };
        }

        private void CapturarItemProyectoActivoAntesDeGuardar()
        {
            if (itemProyectoEnEdicionActual == null || cotizacion == null)
            {
                return;
            }

            try
            {
                AplicarDatosDesdePantalla();
                GuardarDesgloseProductivoDesdePantalla();
                GuardarAsignacionesManoObraDesdeTabla(false);
                ServicioCotizacion.RecalcularCotizacion(cotizacion);
                SincronizarItemProyectoDesdeCotizacion(itemProyectoEnEdicionActual, cotizacion);
                CotizacionItemProyectoAdapterService.CapturarCotizacionEnItem(itemProyectoEnEdicionActual, cotizacion);
                ProyectoCotizacionJsonService.Normalizar(proyectoCotizacionActual);

                if (cotizacionRaizAntesEdicionItem != null)
                {
                    cotizacion = cotizacionRaizAntesEdicionItem;
                    cotizacion.ProyectoProductivo = proyectoCotizacionActual;
                }
            }
            catch
            {
                // Si algo falla, el guardado principal mostrará el error en su flujo normal.
                throw;
            }
        }

        private void AplicarConsolidadoProyectoAlSnapshot()
        {
            if (cotizacion == null || proyectoCotizacionActual == null)
            {
                return;
            }

            ProyectoProductivoExpandido expandido = ProyectoProductivoExpansionService.Expandir(proyectoCotizacionActual);
            ProyectoConsolidado consolidado = ProyectoConsolidacionService.Consolidar(proyectoCotizacionActual, expandido);

            cotizacion.HorasProyectoProductivo = Convert.ToDouble(
                consolidado.HorasProductivas +
                consolidado.HorasRevision +
                consolidado.HorasCorreccion +
                consolidado.HorasSupervision +
                consolidado.HorasDireccion +
                consolidado.HorasGestion
            );
            cotizacion.CostoProyectoProductivoCLP = Convert.ToDouble(consolidado.CostoTotal);
            ServicioCotizacion.RecalcularCotizacion(cotizacion);
        }

        private void AplicarSnapshotProyecto(
            ProyectoOthalartGuardado proyecto,
            string rutaArchivo
        )
        {
            cotizacion = proyecto.Cotizacion ?? new Cotizacion();
            cotizacion.NombreArchivo = rutaArchivo;
            proyectoCotizacionActual =
                proyecto.ProyectoProductivo ??
                cotizacion.ProyectoProductivo ??
                ProyectoCotizacionPruebaService.CrearPilotoAnimado();
            cotizacion.ProyectoProductivo = proyectoCotizacionActual;
            ProyectoCotizacionJsonService.Normalizar(proyectoCotizacionActual);

            bibliotecaEtapas = proyecto.BibliotecaEtapas ?? BibliotecaEtapasJsonService.CargarEtapas();
            bibliotecaSubEtapas = proyecto.BibliotecaSubEtapas ?? BibliotecaSubEtapasJsonService.CargarSubEtapas();
            List<CategoriaTrabajador> cargosProyecto = ObtenerBibliotecaCargosValidaParaCargar(
                proyecto.BibliotecaCargos
            );
            bibliotecaCargosProyectoInforme = cargosProyecto;
            bibliotecaPersonalProyectoInforme =
                proyecto.BibliotecaPersonal != null && proyecto.BibliotecaPersonal.Count > 0
                    ? proyecto.BibliotecaPersonal
                    : BibliotecaPersonalEmpresaJsonService.CargarPersonal();

            bibliotecaCargosGenerales = cargosProyecto
                .Where(c => c != null && NormalizarNombreEtapa(c.Bloque).Contains("general"))
                .ToList();

            resolucionesDependenciasSubEtapas =
                proyecto.ResolucionesDependenciasSubEtapas ??
                new List<ResolucionDependenciaSubEtapa>();

            // Las bibliotecas incluidas en el archivo se usan como contexto del proyecto.
            // No se escriben automáticamente sobre los JSON maestros al cargar.

            modoOscuroActivo = proyecto.ModoOscuroActivo;

            RestaurarRespaldoDatosProyectoEnBrief(proyecto);

            ServicioCotizacion.RecalcularCotizacion(cotizacion);
            RefrescarTodo();
            RefrescarProyectoUI();
            RestaurarRespaldoDatosProyectoEnBrief(proyecto);
            RestaurarPiezas2DGuardadasEnPantalla(proyecto);
            TemaVisualService.AplicarTema(this, modoOscuroActivo);
            RefrescarBotonModoOscuro();
        }

        private void CargarProyectoRecienteDesdeInicio(string rutaArchivo)
        {
            try
            {
                ProyectoOthalartGuardado proyecto =
                    ProyectoOthalartArchivoService.Cargar(rutaArchivo);

                if (proyecto == null || proyecto.Cotizacion == null)
                {
                    throw new InvalidOperationException("El archivo no contiene un proyecto válido.");
                }

                cargandoProyectoDesdeArchivo = true;
                try
                {
                    AplicarSnapshotProyecto(proyecto, rutaArchivo);
                }
                finally
                {
                    cargandoProyectoDesdeArchivo = false;
                }

                ProyectosRecientesService.Registrar(
                    rutaArchivo,
                    proyecto.Cotizacion.NombreProyecto,
                    proyecto.Cotizacion.NombreCliente
                );
                RefrescarTabInicio();
                MarcarProyectoGuardado();
                AplicarModoAplicacion(ModoAplicacion.Proyecto);
                RefrescarDatosDesdeProyectoProductivo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el proyecto reciente.\n\n" + ex.Message,
                    "Proyecto reciente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private string ObtenerTipoSolicitudDatosGuardado()
        {
            return cmbTipoProductoServicio == null
                ? ""
                : cmbTipoProductoServicio.SelectedItem?.ToString() ?? "";
        }

        private string ObtenerProductoServicioDatosGuardado()
        {
            return cmbProductoServicio == null
                ? ""
                : cmbProductoServicio.SelectedItem?.ToString() ?? "";
        }

        private List<Pieza2DSeleccionGuardada> ObtenerPiezas2DDatosGuardadas()
        {
            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            if (brief.Piezas2DSeleccionGuardadas == null)
            {
                return new List<Pieza2DSeleccionGuardada>();
            }

            return brief.Piezas2DSeleccionGuardadas
                .Where(p => p != null)
                .Select(p => new Pieza2DSeleccionGuardada
                {
                    ClaveSeleccion = p.ClaveSeleccion,
                    Usar = p.Usar,
                    Categoria = p.Categoria,
                    Nombre = p.Nombre,
                    Cantidad = p.Cantidad,
                    DuracionPorUnidad = p.DuracionPorUnidad,
                    UnidadDuracion = p.UnidadDuracion,
                    UnidadCantidad = p.UnidadCantidad,
                    EtapaSugerida = p.EtapaSugerida,
                    SubEtapaSugerida = p.SubEtapaSugerida,
                    DependeDe = p.DependeDe,
                    CargosSugeridos = p.CargosSugeridos,
                    EquationKey = p.EquationKey,
                    EcuacionProductiva = p.EcuacionProductiva,
                    VariablesEcuacion = p.VariablesEcuacion,
                    ImpactoEcuacion = p.ImpactoEcuacion,
                    ModoCalculoProductivo = p.ModoCalculoProductivo,
                    HorasAsignadasMin = p.HorasAsignadasMin,
                    HorasAsignadasStd = p.HorasAsignadasStd,
                    HorasAsignadasHolgura = p.HorasAsignadasHolgura,
                    Nota = p.Nota
                })
                .ToList();
        }

        private void RestaurarRespaldoDatosProyectoEnBrief(ProyectoOthalartGuardado proyecto)
        {
            if (proyecto == null || cotizacion == null)
            {
                return;
            }

            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            if (!string.IsNullOrWhiteSpace(proyecto.TipoSolicitudDatosGuardado))
            {
                brief.TipoSolicitudProducto = proyecto.TipoSolicitudDatosGuardado;
            }

            if (!string.IsNullOrWhiteSpace(proyecto.ProductoServicioDatosGuardado))
            {
                brief.ProductoServicioSeleccionado = proyecto.ProductoServicioDatosGuardado;
            }

            if (proyecto.Piezas2DDatosGuardadas != null &&
                proyecto.Piezas2DDatosGuardadas.Count > 0)
            {
                brief.Piezas2DSeleccionGuardadas = proyecto.Piezas2DDatosGuardadas;
                brief.ClavesEntregablesSeleccionados = proyecto.Piezas2DDatosGuardadas
                    .Where(p => p != null && p.Usar)
                    .Select(p => p.ClaveSeleccion)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .ToList();
                brief.SeleccionarTodosEntregablesActivo =
                    proyecto.Piezas2DDatosGuardadas.All(p => p != null && p.Usar);
            }
        }

        private void RestaurarPiezas2DGuardadasEnPantalla(ProyectoOthalartGuardado proyecto)
        {
            if (proyecto == null ||
                proyecto.Piezas2DDatosGuardadas == null ||
                proyecto.Piezas2DDatosGuardadas.Count == 0)
            {
                return;
            }

            if (dgvEntregablesIndustria == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(proyecto.TipoSolicitudDatosGuardado))
            {
                SeleccionarComboSiExiste(cmbTipoProductoServicio, proyecto.TipoSolicitudDatosGuardado);
                CargarProductosServiciosSegunTipo();
            }

            if (!string.IsNullOrWhiteSpace(proyecto.ProductoServicioDatosGuardado))
            {
                SeleccionarComboSiExiste(cmbProductoServicio, proyecto.ProductoServicioDatosGuardado);
                RefrescarOpcionesSegunProducto();
            }

            if (dgvEntregablesIndustria.Rows.Count == 0)
            {
                RefrescarOpcionesSegunProducto();
            }

            MarcarEntregablesSeleccionadosEnPantalla(ObtenerBriefProductoSeguro());
        }

        private List<CategoriaTrabajador> ObtenerBibliotecaCargosCompletaParaProyecto()
        {
            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();

            if (BibliotecaCargosPareceCompleta(cargos))
            {
                return cargos;
            }

            return CargosSeed.CrearBibliotecaCompleta();
        }

        private List<CategoriaTrabajador> ObtenerBibliotecaCargosValidaParaCargar(
            List<CategoriaTrabajador> cargosProyecto
        )
        {
            if (BibliotecaCargosPareceCompleta(cargosProyecto))
            {
                return cargosProyecto;
            }

            List<CategoriaTrabajador> cargosActuales = Cargos.CrearBibliotecaCompleta();

            if (BibliotecaCargosPareceCompleta(cargosActuales))
            {
                return cargosActuales;
            }

            return CargosSeed.CrearBibliotecaCompleta();
        }

        private bool BibliotecaCargosPareceCompleta(List<CategoriaTrabajador> cargos)
        {
            if (cargos == null || cargos.Count == 0)
            {
                return false;
            }

            HashSet<string> bloques = cargos
                .Where(c => c != null)
                .Select(c => NormalizarNombreEtapa(c.Bloque))
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToHashSet();

            return
                bloques.Contains("general") &&
                bloques.Contains("desarrollo") &&
                bloques.Contains("preproduccion") &&
                bloques.Contains("produccion") &&
                bloques.Contains("postproduccion");
        }

        private string CrearNombreArchivoProyecto()
        {
            string nombre = cotizacion == null
                ? ""
                : cotizacion.NombreProyecto;

            if (string.IsNullOrWhiteSpace(nombre))
            {
                nombre = "Proyecto Othalart";
            }

            foreach (char invalido in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(invalido, '-');
            }

            return nombre.Trim() + ".othalart.json";
        }

        private void BtnGuardarInformeHtml_Click(object sender, EventArgs e)
        {
            try
            {
                AplicarDatosDesdePantalla();
                GuardarDesgloseProductivoDesdePantalla();
                GuardarAsignacionesManoObraDesdeTabla(false);
                AplicarConsolidadoProyectoAlSnapshot();
                ServicioCotizacion.RecalcularCotizacion(cotizacion);

                bool guardado = InformeClienteExporter.GuardarPdfProyecto(
                    proyectoCotizacionActual,
                    bibliotecaPersonalProyectoInforme ??
                        BibliotecaPersonalEmpresaJsonService.CargarPersonal(),
                    bibliotecaCargosProyectoInforme ??
                        BibliotecaCargosJsonService.CargarCargos());

                if (guardado)
                {
                    lblGuardarEstado.Text = "Informe PDF guardado correctamente.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblGuardarEstado.Text = "Exportación de PDF cancelada.";
                    lblGuardarEstado.ForeColor = Color.DimGray;
                }
            }
            catch (Exception ex)
            {
                lblGuardarEstado.Text = "No se pudo exportar el informe PDF.";
                lblGuardarEstado.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    "No se pudo exportar el informe PDF.\n\n" + ex.Message,
                    "Error al exportar PDF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnGuardarExcelInterno_Click(object sender, EventArgs e)
        {
            try
            {
                AplicarDatosDesdePantalla();
                GuardarDesgloseProductivoDesdePantalla();
                GuardarAsignacionesManoObraDesdeTabla(false);
                AplicarConsolidadoProyectoAlSnapshot();
                ServicioCotizacion.RecalcularCotizacion(cotizacion);

                bool guardado = ExcelInternoExporter.GuardarExcelInterno(
                    cotizacion,
                    proyectoCotizacionActual,
                    bibliotecaPersonalProyectoInforme ??
                        BibliotecaPersonalEmpresaJsonService.CargarPersonal(),
                    bibliotecaCargosProyectoInforme ??
                        BibliotecaCargosJsonService.CargarCargos());

                if (guardado)
                {
                    lblGuardarEstado.Text = "Excel interno guardado correctamente.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblGuardarEstado.Text = "Guardado de Excel cancelado.";
                    lblGuardarEstado.ForeColor = Color.DimGray;
                }
            }
            catch (Exception ex)
            {
                lblGuardarEstado.Text = "No se pudo guardar el Excel interno.";
                lblGuardarEstado.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    "No se pudo guardar el Excel interno.\n\n" + ex.Message,
                    "Error al guardar Excel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
