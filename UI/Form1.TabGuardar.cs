using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
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

            btnGuardarExcelInterno.Text = "Guardar Excel interno";
            btnGuardarExcelInterno.Width = 190;
            btnGuardarExcelInterno.Height = 36;
            btnGuardarExcelInterno.Click += BtnGuardarExcelInterno_Click;

            btnGuardarInformeHtml.Text = "Guardar informe cliente";
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

                using (SaveFileDialog dialogo = new SaveFileDialog())
                {
                    dialogo.Title = "Guardar proyecto Othalart";
                    dialogo.Filter = "Proyecto Othalart (*.othalart.json)|*.othalart.json|JSON (*.json)|*.json";
                    dialogo.FileName = CrearNombreArchivoProyecto();
                    dialogo.AddExtension = true;
                    dialogo.DefaultExt = "othalart.json";

                    if (dialogo.ShowDialog(this) != DialogResult.OK)
                    {
                        lblGuardarEstado.Text = "Guardado de proyecto cancelado.";
                        lblGuardarEstado.ForeColor = Color.DimGray;
                        return;
                    }

                    string json = JsonSerializer.Serialize(proyecto, CrearOpcionesJsonProyecto());
                    File.WriteAllText(dialogo.FileName, json);

                    cotizacion.NombreArchivo = dialogo.FileName;
                    int piezasGuardadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count;
                    int piezasSeleccionadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count(p => p != null && p.Usar);

                    lblGuardarEstado.Text =
                        $"Proyecto editable guardado correctamente. Piezas: {piezasSeleccionadas}/{piezasGuardadas}.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
                }
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

        private void BtnCargarProyecto_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialogo = new OpenFileDialog())
                {
                    dialogo.Title = "Cargar proyecto Othalart";
                    dialogo.Filter = "Proyecto Othalart (*.othalart.json;*.json)|*.othalart.json;*.json";

                    if (dialogo.ShowDialog(this) != DialogResult.OK)
                    {
                        lblGuardarEstado.Text = "Carga de proyecto cancelada.";
                        lblGuardarEstado.ForeColor = Color.DimGray;
                        return;
                    }

                    string json = File.ReadAllText(dialogo.FileName);
                    ProyectoOthalartGuardado proyecto =
                        JsonSerializer.Deserialize<ProyectoOthalartGuardado>(
                            json,
                            CrearOpcionesJsonProyecto()
                        );

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

                    int piezasCargadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count;
                    int piezasSeleccionadas = proyecto.Piezas2DDatosGuardadas == null
                        ? 0
                        : proyecto.Piezas2DDatosGuardadas.Count(p => p != null && p.Usar);

                    lblGuardarEstado.Text =
                        $"Proyecto editable cargado correctamente. Piezas: {piezasSeleccionadas}/{piezasCargadas}.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
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
            return new ProyectoOthalartGuardado
            {
                FechaGuardado = DateTime.Now,
                Cotizacion = cotizacion ?? new Cotizacion(),
                TipoSolicitudDatosGuardado = ObtenerTipoSolicitudDatosGuardado(),
                ProductoServicioDatosGuardado = ObtenerProductoServicioDatosGuardado(),
                Piezas2DDatosGuardadas = ObtenerPiezas2DDatosGuardadas(),
                BibliotecaEtapas = bibliotecaEtapas ?? BibliotecaEtapasJsonService.CargarEtapas(),
                BibliotecaSubEtapas = bibliotecaSubEtapas ?? BibliotecaSubEtapasJsonService.CargarSubEtapas(),
                BibliotecaCargos = ObtenerBibliotecaCargosCompletaParaProyecto(),
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

        private void AplicarSnapshotProyecto(
            ProyectoOthalartGuardado proyecto,
            string rutaArchivo
        )
        {
            cotizacion = proyecto.Cotizacion ?? new Cotizacion();
            cotizacion.NombreArchivo = rutaArchivo;

            bibliotecaEtapas = proyecto.BibliotecaEtapas ?? BibliotecaEtapasJsonService.CargarEtapas();
            bibliotecaSubEtapas = proyecto.BibliotecaSubEtapas ?? BibliotecaSubEtapasJsonService.CargarSubEtapas();
            List<CategoriaTrabajador> cargosProyecto = ObtenerBibliotecaCargosValidaParaCargar(
                proyecto.BibliotecaCargos
            );

            bibliotecaCargosGenerales = cargosProyecto
                .Where(c => c != null && NormalizarNombreEtapa(c.Bloque).Contains("general"))
                .ToList();

            resolucionesDependenciasSubEtapas =
                proyecto.ResolucionesDependenciasSubEtapas ??
                new List<ResolucionDependenciaSubEtapa>();

            BibliotecaEtapasJsonService.GuardarEtapas(bibliotecaEtapas);
            BibliotecaSubEtapasJsonService.GuardarSubEtapas(bibliotecaSubEtapas);
            Cargos.GuardarBibliotecaCompleta(cargosProyecto);

            if (proyecto.BibliotecaProductos2D != null &&
                proyecto.BibliotecaProductos2D.Count > 0)
            {
                BibliotecaProductos2DJsonService.GuardarProductos(proyecto.BibliotecaProductos2D);
            }

            if (proyecto.BibliotecaRendimientosProductivos != null &&
                proyecto.BibliotecaRendimientosProductivos.Count > 0)
            {
                BibliotecaRendimientosProductivosJsonService.GuardarRendimientos(
                    proyecto.BibliotecaRendimientosProductivos
                );
            }

            if (proyecto.BibliotecaEcuacionesProductivas != null &&
                proyecto.BibliotecaEcuacionesProductivas.Count > 0)
            {
                BibliotecaEcuacionesProductivasJsonService.GuardarEcuaciones(
                    proyecto.BibliotecaEcuacionesProductivas
                );
            }

            if (proyecto.BibliotecaGestionesProductivas != null &&
                proyecto.BibliotecaGestionesProductivas.Count > 0)
            {
                BibliotecaGestionesProductivasJsonService.GuardarGestiones(
                    proyecto.BibliotecaGestionesProductivas
                );
            }

            modoOscuroActivo = proyecto.ModoOscuroActivo;

            RestaurarRespaldoDatosProyectoEnBrief(proyecto);

            ServicioCotizacion.RecalcularCotizacion(cotizacion);
            RefrescarTodo();
            RestaurarRespaldoDatosProyectoEnBrief(proyecto);
            RestaurarPiezas2DGuardadasEnPantalla(proyecto);
            TemaVisualService.AplicarTema(this, modoOscuroActivo);
            RefrescarBotonModoOscuro();
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
                    EcuacionProductiva = p.EcuacionProductiva,
                    VariablesEcuacion = p.VariablesEcuacion,
                    ImpactoEcuacion = p.ImpactoEcuacion,
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

        private JsonSerializerOptions CrearOpcionesJsonProyecto()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        private void BtnGuardarInformeHtml_Click(object sender, EventArgs e)
        {
            try
            {
                AplicarDatosDesdePantalla();
                ServicioCotizacion.RecalcularCotizacion(cotizacion);

                bool guardado = InformeClienteExporter.GuardarHtmlCliente(cotizacion);

                if (guardado)
                {
                    lblGuardarEstado.Text = "Informe cliente guardado correctamente.";
                    lblGuardarEstado.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblGuardarEstado.Text = "Guardado de informe cancelado.";
                    lblGuardarEstado.ForeColor = Color.DimGray;
                }
            }
            catch (Exception ex)
            {
                lblGuardarEstado.Text = "No se pudo guardar el informe cliente.";
                lblGuardarEstado.ForeColor = Color.DarkRed;

                MessageBox.Show(
                    "No se pudo guardar el informe cliente.\n\n" + ex.Message,
                    "Error al guardar informe",
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
                ServicioCotizacion.RecalcularCotizacion(cotizacion);

                bool guardado = ExcelInternoExporter.GuardarExcelInterno(cotizacion);

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
