using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;


namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private bool aplicarDatosProgramadoDesdeMouse = false;

        private void CargarDatosEnPantalla()
        {
            if (cotizacion == null)
            {
                return;
            }

            txtNombreCliente.Text = cotizacion.NombreCliente;
            txtEmpresa.Text = cotizacion.Empresa;
            txtEmail.Text = cotizacion.Email;
            txtNombreProyecto.Text = cotizacion.NombreProyecto;
            txtDescripcion.Text = cotizacion.Descripcion;
            txtNotas.Text = cotizacion.Notas;

            string monedaCliente = string.IsNullOrWhiteSpace(cotizacion.Moneda)
                ? "CLP"
                : cotizacion.Moneda;

            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            monedaClienteSeleccionadaRapida = string.IsNullOrWhiteSpace(
                brief.MonedaClienteSeleccionadaRapida
            )
                ? monedaCliente
                : brief.MonedaClienteSeleccionadaRapida.Trim().ToUpperInvariant();

            presupuestoClienteModo = string.IsNullOrWhiteSpace(brief.PresupuestoClienteModo)
                ? (cotizacion.PresupuestoCliente > 0 ? "Manual" : "Sin informar")
                : brief.PresupuestoClienteModo;

            presupuestoClienteRapidoCLP = brief.PresupuestoClienteRapidoCLP;

            SeleccionarComboSiExiste(cmbMoneda, monedaCliente);
            MarcarBotonMonedaCliente(monedaClienteSeleccionadaRapida);

            if (cotizacion.PresupuestoCliente > 0)
            {
                txtPresupuestoCliente.Text =
                    FormatearNumeroPresupuestoEnMonedaActual((double)cotizacion.PresupuestoCliente);
            }
            else
            {
                txtPresupuestoCliente.Text = "";
            }

            txtPresupuestoCliente.Visible = presupuestoClienteModo == "Manual";
            MarcarBotonPresupuestoRapido(presupuestoClienteModo);

            CargarFechasClienteEnPantalla();

            SeleccionarComboSiExiste(cmbIndustriaCliente, brief.IndustriaCliente);

            RefrescarOpcionesSegunIndustria();

            string tipoSolicitud = string.IsNullOrWhiteSpace(brief.TipoSolicitudProducto)
                ? "Producto final"
                : brief.TipoSolicitudProducto;

            SeleccionarComboSiExiste(cmbTipoProductoServicio, tipoSolicitud);
            CargarProductosServiciosSegunTipo();

            string productoSeleccionado = string.IsNullOrWhiteSpace(brief.ProductoServicioSeleccionado)
                ? brief.TipoProducto
                : brief.ProductoServicioSeleccionado;

            SeleccionarComboSiExiste(cmbProductoServicio, productoSeleccionado);
            RefrescarOpcionesSegunProducto();
            CargarDuracionProductoEnPantalla(brief);
            CargarCantidadGlobalProductoEnPantalla(brief);

            SeleccionarComboSiExiste(cmbDestinoUso, brief.DestinoUso);
            SeleccionarComboSiExiste(cmbEstiloVisual, brief.EstiloVisual);
            SeleccionarComboSiExiste(cmbNivelAcabado, brief.NivelAcabado);
            SeleccionarComboSiExiste(cmbFormatoEntregaBrief, brief.FormatoEntrega);
            SeleccionarComboSiExiste(cmbRelacionAspecto, brief.RelacionAspecto);
            SeleccionarComboSiExiste(cmbResolucionEntrega, brief.ResolucionEntrega);

            txtReferenciasVisuales.Text = brief.ReferenciasVisuales;
            txtNotasBrief.Text = brief.NotasBrief;

            CargarDestinosUsoEnPantalla(brief);
            MarcarEntregablesSeleccionadosEnPantalla(brief);
            CargarInsumosClienteEnPantalla(brief);

            RefrescarPlazoClienteCalculado();
            RefrescarPanelSiguientePasoDatos();
        }

        private void InvalidarDesgloseProductivoPorCambioDeBrief()
        {
            if (cotizacion == null)
            {
                return;
            }

            /*
             * Cuando cambia el brief, el desglose anterior ya no es confiable.
             * Si no lo limpiamos, la pestaña Desglose productivo puede seguir mostrando
             * requerimientos antiguos aunque el brief ya tenga más entregables.
             */
            cotizacion.DesgloseProductivo = null;
            cotizacion.EvaluacionPlazo = null;

            cotizacion.DuracionMinimaTecnicaSemanas = 0.0;
            cotizacion.DuracionEstandarTecnicaSemanas = 0.0;
            cotizacion.DuracionHolguraTecnicaSemanas = 0.0;
            cotizacion.DiagnosticoPlazo = "";
        }
            
        private void AplicarDatosDesdePantalla()
        {
            if (cotizacion == null)
            {
                return;
            }

            if (cargandoProyectoDesdeArchivo)
            {
                return;
            }

            cotizacion.NombreCliente = txtNombreCliente.Text.Trim();
            cotizacion.Empresa = txtEmpresa.Text.Trim();
            cotizacion.Email = txtEmail.Text.Trim();
            cotizacion.NombreProyecto = txtNombreProyecto.Text.Trim();
            cotizacion.Descripcion = txtDescripcion.Text.Trim();
            cotizacion.Notas = txtNotas.Text.Trim();

            ConfirmarEdicionEntregablesIndustria();

            string monedaCliente = string.IsNullOrWhiteSpace(monedaClienteSeleccionadaRapida)
                ? (cmbMoneda.SelectedItem?.ToString() ?? "CLP")
                : monedaClienteSeleccionadaRapida.Trim().ToUpperInvariant();

            cotizacion.Moneda = monedaCliente;
            cotizacion.MonedaPrecioCliente = monedaCliente;

            AplicarPresupuestoClienteDesdeSeleccion();
            cotizacion.MonedaPresupuestoCliente = monedaCliente;

            cotizacion.FechaInicioCliente = dtpFechaInicioCliente.Checked
    ? dtpFechaInicioCliente.Value.Date
    : null;

            cotizacion.FechaEntregaCliente = dtpFechaEntregaCliente.Checked
                ? dtpFechaEntregaCliente.Value.Date
                : null;

            cotizacion.DiasHabilesEstudioPorSemana =
                Convert.ToDouble(nudDiasHabilesEstudioSemana.Value);

            if (string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion))
            {
                cotizacion.MonedaVisualizacion = monedaCliente;
            }

            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            brief.TipoSolicitudProducto = cmbTipoProductoServicio.SelectedItem?.ToString() ?? "";
            brief.ProductoServicioSeleccionado = cmbProductoServicio.SelectedItem?.ToString() ?? "";
            brief.TipoProducto = brief.ProductoServicioSeleccionado;
            brief.TipoPiezaPrincipal = brief.ProductoServicioSeleccionado;
            brief.DuracionProductoValor = ObtenerDuracionProductoValor();
            brief.DuracionProductoUnidad = ObtenerDuracionProductoUnidad();
            brief.DuracionSolicitudValor = brief.DuracionProductoValor;
            brief.DuracionSolicitudUnidad = brief.DuracionProductoUnidad;
            brief.CantidadGlobalProducto = nudCantidadPiezas == null
                ? Math.Max(1, brief.CantidadGlobalProducto)
                : (int)nudCantidadPiezas.Value;
            brief.PresupuestoClienteModo = presupuestoClienteModo;
            brief.PresupuestoClienteRapidoCLP = presupuestoClienteRapidoCLP;
            brief.MonedaClienteSeleccionadaRapida = monedaClienteSeleccionadaRapida;

            brief.IndustriaCliente = cmbIndustriaCliente.SelectedItem?.ToString() ?? "";
            GuardarDestinosUsoDesdePantalla(brief);
            brief.UsoPrincipal = brief.DestinoUso;
            brief.PlataformaDestino = brief.DestinoUso;

            brief.EstiloVisual = cmbEstiloVisual.SelectedItem?.ToString() ?? "";
            brief.NivelAcabado = cmbNivelAcabado.SelectedItem?.ToString() ?? "";
            brief.ReferenciasVisuales = txtReferenciasVisuales.Text.Trim();

            brief.FormatoEntrega = cmbFormatoEntregaBrief.SelectedItem?.ToString() ?? "";
            brief.RelacionAspecto = cmbRelacionAspecto.SelectedItem?.ToString() ?? "";
            brief.ResolucionEntrega = cmbResolucionEntrega.SelectedItem?.ToString() ?? "";

            brief.NotasBrief = txtNotasBrief.Text.Trim();

            GuardarEntregablesSeleccionadosDesdePantalla(brief);
            GuardarInsumosClienteDesdePantalla(brief);
            ActualizarFlagsCompatibilidadDesdeEntregables(brief);
            ActualizarResumenCompatibilidadBrief(brief);

            InvalidarDesgloseProductivoPorCambioDeBrief();

            ExpansorBriefProduccionService.ExpandirBrief(brief);

            ActivadorSubEtapasDesdeBriefService.ActivarSubEtapasInternas(
                brief,
                bibliotecaSubEtapas
            );

            cotizacion.EvaluacionPlazo =
    EvaluadorPlazoProyectoService.Evaluar(cotizacion);

            RefrescarCalculosYVista();
            RefrescarResumen();
            RefrescarPanelSiguientePasoDatos();

            RefrescarTablaSubEtapasSiExiste();
            RefrescarVistaCargosSiExiste();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }
        }

        private BriefProductoProyecto ObtenerBriefProductoSeguro()
        {
            if (cotizacion.BriefProducto == null)
            {
                cotizacion.BriefProducto = new BriefProductoProyecto();
            }

            if (cotizacion.BriefProducto.EntregablesSeleccionados == null)
            {
                cotizacion.BriefProducto.EntregablesSeleccionados =
                    new System.Collections.Generic.List<EntregableBrief>();
            }

            if (cotizacion.BriefProducto.InsumosClienteSeleccionados == null)
            {
                cotizacion.BriefProducto.InsumosClienteSeleccionados =
                    new System.Collections.Generic.List<string>();
            }

            if (cotizacion.BriefProducto.ClavesEntregablesSeleccionados == null)
            {
                cotizacion.BriefProducto.ClavesEntregablesSeleccionados =
                    new System.Collections.Generic.List<string>();
            }

            if (cotizacion.BriefProducto.Piezas2DSeleccionGuardadas == null)
            {
                cotizacion.BriefProducto.Piezas2DSeleccionGuardadas =
                    new System.Collections.Generic.List<Pieza2DSeleccionGuardada>();
            }

            if (cotizacion.BriefProducto.DestinosUsoSeleccionados == null)
            {
                cotizacion.BriefProducto.DestinosUsoSeleccionados =
                    new System.Collections.Generic.List<string>();
            }

            if (string.IsNullOrWhiteSpace(cotizacion.BriefProducto.DuracionProductoUnidad))
            {
                cotizacion.BriefProducto.DuracionProductoUnidad = "segundos";
            }

            if (string.IsNullOrWhiteSpace(cotizacion.BriefProducto.DuracionSolicitudUnidad))
            {
                cotizacion.BriefProducto.DuracionSolicitudUnidad =
                    cotizacion.BriefProducto.DuracionProductoUnidad;
            }

            if (string.IsNullOrWhiteSpace(cotizacion.BriefProducto.PresupuestoClienteModo))
            {
                cotizacion.BriefProducto.PresupuestoClienteModo = "Sin informar";
            }

            if (string.IsNullOrWhiteSpace(cotizacion.BriefProducto.MonedaClienteSeleccionadaRapida))
            {
                cotizacion.BriefProducto.MonedaClienteSeleccionadaRapida = "CLP";
            }

            if (cotizacion.BriefProducto.CantidadPiezas < 1)
            {
                cotizacion.BriefProducto.CantidadPiezas = 1;
            }

            if (cotizacion.BriefProducto.CantidadGlobalProducto < 1)
            {
                cotizacion.BriefProducto.CantidadGlobalProducto =
                    Math.Max(1, cotizacion.BriefProducto.CantidadPiezas);
            }

            return cotizacion.BriefProducto;
        }

        private void CargarCantidadGlobalProductoEnPantalla(BriefProductoProyecto brief)
        {
            if (nudCantidadPiezas == null || brief == null)
            {
                return;
            }

            int cantidad = brief.CantidadGlobalProducto > 0
                ? brief.CantidadGlobalProducto
                : Math.Max(1, brief.CantidadPiezas);

            if (cantidad < nudCantidadPiezas.Minimum)
            {
                cantidad = (int)nudCantidadPiezas.Minimum;
            }

            if (cantidad > nudCantidadPiezas.Maximum)
            {
                cantidad = (int)nudCantidadPiezas.Maximum;
            }

            bloqueandoEventosCantidadGlobalProducto = true;
            nudCantidadPiezas.Value = cantidad;
            bloqueandoEventosCantidadGlobalProducto = false;
        }

        private void CargarFechasClienteEnPantalla()
        {
            if (dtpFechaInicioCliente != null)
            {
                dtpFechaInicioCliente.ValueChanged -= CalendarioCliente_ValueChanged;

                if (cotizacion.FechaInicioCliente.HasValue)
                {
                    dtpFechaInicioCliente.Value = cotizacion.FechaInicioCliente.Value.Date;
                    dtpFechaInicioCliente.Checked = true;
                }
                else
                {
                    dtpFechaInicioCliente.Checked = false;
                }

                dtpFechaInicioCliente.ValueChanged += CalendarioCliente_ValueChanged;
            }

            if (dtpFechaEntregaCliente != null)
            {
                dtpFechaEntregaCliente.ValueChanged -= CalendarioCliente_ValueChanged;

                if (cotizacion.FechaEntregaCliente.HasValue)
                {
                    dtpFechaEntregaCliente.Value = cotizacion.FechaEntregaCliente.Value.Date;
                    dtpFechaEntregaCliente.Checked = true;
                }
                else
                {
                    dtpFechaEntregaCliente.Checked = false;
                }

                dtpFechaEntregaCliente.ValueChanged += CalendarioCliente_ValueChanged;
            }

            if (nudDiasHabilesEstudioSemana != null)
            {
                decimal diasHabiles = Convert.ToDecimal(
                    cotizacion.DiasHabilesEstudioPorSemana <= 0.0
                        ? 5.0
                        : cotizacion.DiasHabilesEstudioPorSemana
                );

                if (diasHabiles < nudDiasHabilesEstudioSemana.Minimum)
                {
                    diasHabiles = nudDiasHabilesEstudioSemana.Minimum;
                }

                if (diasHabiles > nudDiasHabilesEstudioSemana.Maximum)
                {
                    diasHabiles = nudDiasHabilesEstudioSemana.Maximum;
                }

                nudDiasHabilesEstudioSemana.Value = diasHabiles;
            }
        }

        private void CargarDuracionProductoEnPantalla(BriefProductoProyecto brief)
        {
            bloqueandoEventosDuracionProducto = true;

            bool tieneEstadoProductoGuardado =
                !string.IsNullOrWhiteSpace(brief.ProductoServicioSeleccionado) ||
                !string.IsNullOrWhiteSpace(brief.TipoProducto) ||
                brief.DuracionSolicitudValor > 0.0 ||
                brief.DuracionProductoValor > 0.0;

            if (tieneEstadoProductoGuardado)
            {
                double duracionVisible = brief.DuracionSolicitudValor > 0.0
                    ? brief.DuracionSolicitudValor
                    : brief.DuracionProductoValor;

                txtDuracionProductoValor.Text =
                    duracionVisible.ToString("0.##", CultureInfo.InvariantCulture);
            }

            string unidad = string.IsNullOrWhiteSpace(brief.DuracionSolicitudUnidad)
                ? brief.DuracionProductoUnidad
                : brief.DuracionSolicitudUnidad;

            SeleccionarComboSiExiste(cmbDuracionProductoUnidad, unidad);

            bloqueandoEventosDuracionProducto = false;

            ActualizarDuracionEnFilasDependientesDelTiempo();
        }

        private void GuardarDestinosUsoDesdePantalla(BriefProductoProyecto brief)
        {
            brief.DestinosUsoSeleccionados.Clear();

            if (destinosUsoSeleccionados != null && destinosUsoSeleccionados.Count > 0)
            {
                brief.DestinosUsoSeleccionados.AddRange(
                    destinosUsoSeleccionados
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .Select(d => d.Trim())
                        .Distinct()
                );
            }

            if (brief.DestinosUsoSeleccionados.Count > 0)
            {
                brief.DestinoUso = string.Join("; ", brief.DestinosUsoSeleccionados);
                return;
            }

            brief.DestinoUso = cmbDestinoUso.SelectedItem?.ToString() ?? "";
        }

        private void CargarDestinosUsoEnPantalla(BriefProductoProyecto brief)
        {
            destinosUsoSeleccionados.Clear();

            if (brief.DestinosUsoSeleccionados != null)
            {
                destinosUsoSeleccionados.AddRange(
                    brief.DestinosUsoSeleccionados
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .Select(d => d.Trim())
                        .Distinct()
                );
            }

            if (destinosUsoSeleccionados.Count == 0 &&
                !string.IsNullOrWhiteSpace(brief.DestinoUso))
            {
                destinosUsoSeleccionados.AddRange(
                    brief.DestinoUso
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                );
            }

            if (destinosUsoSeleccionados.Count == 0)
            {
                destinosUsoSeleccionados.Add("General");
            }

            MarcarBotonesDestinoUso();
        }

        private void GuardarEntregablesSeleccionadosDesdePantalla(BriefProductoProyecto brief)
        {
            brief.EntregablesSeleccionados.Clear();
            brief.ClavesEntregablesSeleccionados.Clear();
            brief.Piezas2DSeleccionGuardadas.Clear();
            brief.SeleccionarTodosEntregablesActivo = false;

            if (dgvEntregablesIndustria == null || dgvEntregablesIndustria.Rows == null)
            {
                return;
            }

            if (dgvEntregablesIndustria.IsCurrentCellDirty)
            {
                dgvEntregablesIndustria.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

            dgvEntregablesIndustria.EndEdit();

            int totalFilasProducto = 0;
            int totalFilasUsadas = 0;
            HashSet<string> nombresSeleccionados = ObtenerNombresEntregablesMarcados();

            foreach (DataGridViewRow row in dgvEntregablesIndustria.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                if (EsFilaCategoriaEntregables(row))
                {
                    continue;
                }

                totalFilasProducto++;

                bool usar = false;
                object valorUsar = row.Cells["Usar"].Value;

                if (valorUsar is bool valorBool)
                {
                    usar = valorBool;
                }
                else if (valorUsar != null)
                {
                    bool.TryParse(valorUsar.ToString(), out usar);
                }

                string categoria = row.Cells["Categoria"].Value?.ToString() ?? "";
                string nombre = row.Cells["Producto2D"].Value?.ToString() ?? "";
                string unidadCantidad = row.Cells["UnidadCantidad"].Value?.ToString() ?? "unidades";
                string unidadDuracion = row.Cells["UnidadDuracion"].Value?.ToString() ?? "segundos";
                string nota = row.Cells["Nota"].Value?.ToString() ?? "";
                Subproducto2D subproducto = EnriquecerSubproductoDesdeBibliotecas(
                    row.Tag as Subproducto2D,
                    categoria,
                    nombre
                );
                row.Tag = subproducto;
                string etapaSugerida = subproducto == null ? "" : subproducto.EtapaSugerida;
                string subEtapaSugerida = subproducto == null ? "" : subproducto.SubEtapaSugerida;

                int cantidad = 1;

                int.TryParse(row.Cells["Cantidad"].Value?.ToString() ?? "1", out cantidad);

                if (cantidad < 1)
                {
                    cantidad = 1;
                }

                double duracionPorUnidad = ParsearDoubleFlexible(
                    row.Cells["DuracionPorUnidad"].Value?.ToString() ?? "0"
                );

                if (duracionPorUnidad < 0.0)
                {
                    duracionPorUnidad = 0.0;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    continue;
                }

                string claveSeleccion = CrearClaveSeleccionEntregable(
                    categoria,
                    nombre,
                    etapaSugerida,
                    subEtapaSugerida
                );

                string cargosSugeridos = subproducto == null ? "" : subproducto.CargosSugeridos;
                string ecuacionProductiva = subproducto == null ? "" : subproducto.EcuacionProductiva;
                string variablesEcuacion = subproducto == null ? "" : subproducto.VariablesEcuacion;
                string impactoEcuacion = subproducto == null ? "" : subproducto.ImpactoEcuacion;

                string dependeDeOriginal = subproducto == null ? "" : subproducto.DependeDe;
                string dependeDeExplicito = FiltrarDependenciasPorTablaFinal(
                    dependeDeOriginal,
                    nombresSeleccionados
                );

                brief.Piezas2DSeleccionGuardadas.Add(new Pieza2DSeleccionGuardada
                {
                    ClaveSeleccion = claveSeleccion,
                    Usar = usar,
                    Categoria = categoria,
                    Nombre = nombre,
                    Cantidad = cantidad,
                    DuracionPorUnidad = duracionPorUnidad,
                    UnidadDuracion = unidadDuracion,
                    UnidadCantidad = unidadCantidad,
                    EtapaSugerida = etapaSugerida,
                    SubEtapaSugerida = subEtapaSugerida,
                    DependeDe = dependeDeExplicito,
                    CargosSugeridos = cargosSugeridos,
                    EcuacionProductiva = ecuacionProductiva,
                    VariablesEcuacion = variablesEcuacion,
                    ImpactoEcuacion = impactoEcuacion,
                    Nota = nota
                });

                if (!usar)
                {
                    continue;
                }

                totalFilasUsadas++;

                if (!brief.ClavesEntregablesSeleccionados.Contains(claveSeleccion))
                {
                    brief.ClavesEntregablesSeleccionados.Add(claveSeleccion);
                }

                brief.EntregablesSeleccionados.Add(new EntregableBrief
                {
                    Categoria = categoria,
                    Nombre = nombre,
                    ClaveSeleccion = claveSeleccion,
                    Cantidad = cantidad,
                    DuracionPorUnidad = duracionPorUnidad,
                    UnidadDuracion = unidadDuracion,
                    UnidadCantidad = unidadCantidad,
                    EtapaSugerida = etapaSugerida,
                    SubEtapaSugerida = subEtapaSugerida,
                    DependeDe = dependeDeExplicito,
                    CargosSugeridos = cargosSugeridos,
                    EcuacionProductiva = ecuacionProductiva,
                    VariablesEcuacion = variablesEcuacion,
                    ImpactoEcuacion = impactoEcuacion,
                    Nota = nota
                });
            }

            brief.SeleccionarTodosEntregablesActivo =
                totalFilasProducto > 0 && totalFilasProducto == totalFilasUsadas;
        }

        private HashSet<string> ObtenerNombresEntregablesMarcados()
        {
            HashSet<string> nombres = new HashSet<string>();

            if (dgvEntregablesIndustria == null || dgvEntregablesIndustria.Rows == null)
            {
                return nombres;
            }

            foreach (DataGridViewRow row in dgvEntregablesIndustria.Rows)
            {
                if (row == null || row.IsNewRow || EsFilaCategoriaEntregables(row))
                {
                    continue;
                }

                bool usar = false;
                object valorUsar = row.Cells["Usar"].Value;

                if (valorUsar is bool valorBool)
                {
                    usar = valorBool;
                }
                else if (valorUsar != null)
                {
                    bool.TryParse(valorUsar.ToString(), out usar);
                }

                if (!usar)
                {
                    continue;
                }

                string nombre = row.Cells["Producto2D"].Value?.ToString() ?? "";
                string clave = NormalizarTextoDatosVisual(nombre);

                if (!string.IsNullOrWhiteSpace(clave))
                {
                    nombres.Add(clave);
                }
            }

            return nombres;
        }

        private string FiltrarDependenciasPorTablaFinal(
            string dependeDe,
            HashSet<string> nombresSeleccionados
        )
        {
            if (string.IsNullOrWhiteSpace(dependeDe) ||
                nombresSeleccionados == null ||
                nombresSeleccionados.Count == 0)
            {
                return "";
            }

            List<string> dependenciasValidas = new List<string>();

            foreach (string dependencia in SepararDependenciasPipeline(dependeDe))
            {
                string clave = NormalizarTextoDatosVisual(dependencia);

                if (!string.IsNullOrWhiteSpace(clave) &&
                    nombresSeleccionados.Contains(clave))
                {
                    dependenciasValidas.Add(dependencia.Trim());
                }
            }

            return string.Join("; ", dependenciasValidas.Distinct());
        }

        private void MarcarEntregablesSeleccionadosEnPantalla(BriefProductoProyecto brief)
        {
            if (dgvEntregablesIndustria == null || dgvEntregablesIndustria.Rows == null)
            {
                return;
            }

            bool estadoAnteriorLote = actualizandoEntregablesPorLote;
            actualizandoEntregablesPorLote = true;

            try
            {
                foreach (DataGridViewRow row in dgvEntregablesIndustria.Rows)
                {
                    if (row == null || row.IsNewRow)
                    {
                        continue;
                    }

                    if (EsFilaCategoriaEntregables(row))
                    {
                        continue;
                    }

                    string categoria = row.Cells["Categoria"].Value?.ToString() ?? "";
                    string nombre = row.Cells["Producto2D"].Value?.ToString() ?? "";
                    Subproducto2D subproducto = row.Tag as Subproducto2D;
                    string claveFila = CrearClaveSeleccionEntregable(
                        categoria,
                        nombre,
                        subproducto == null ? "" : subproducto.EtapaSugerida,
                        subproducto == null ? "" : subproducto.SubEtapaSugerida
                    );

                    Pieza2DSeleccionGuardada piezaGuardada = null;

                    if (brief.Piezas2DSeleccionGuardadas != null)
                    {
                        piezaGuardada = brief.Piezas2DSeleccionGuardadas
                            .FirstOrDefault(p =>
                                !string.IsNullOrWhiteSpace(p.ClaveSeleccion) &&
                                NormalizarTextoDatosVisual(p.ClaveSeleccion) ==
                                NormalizarTextoDatosVisual(claveFila)
                            );

                        if (piezaGuardada == null)
                        {
                            piezaGuardada = brief.Piezas2DSeleccionGuardadas
                                .FirstOrDefault(p =>
                                    NormalizarTextoDatosVisual(p.Nombre) ==
                                    NormalizarTextoDatosVisual(nombre)
                                );
                        }
                    }

                    if (piezaGuardada != null)
                    {
                        AsegurarValorEnComboGrid(5, piezaGuardada.UnidadDuracion);
                        AsegurarValorEnComboGrid(6, piezaGuardada.UnidadCantidad);

                        row.Cells["Usar"].Value = piezaGuardada.Usar;
                        row.Cells["Cantidad"].Value = piezaGuardada.Cantidad < 1
                            ? 1
                            : piezaGuardada.Cantidad;
                        row.Cells["DuracionPorUnidad"].Value = piezaGuardada.DuracionPorUnidad;
                        row.Cells["UnidadDuracion"].Value =
                            string.IsNullOrWhiteSpace(piezaGuardada.UnidadDuracion)
                                ? "segundos"
                                : piezaGuardada.UnidadDuracion;
                        row.Cells["UnidadCantidad"].Value =
                            string.IsNullOrWhiteSpace(piezaGuardada.UnidadCantidad)
                                ? "unidades"
                                : piezaGuardada.UnidadCantidad;
                        row.Cells["Nota"].Value = piezaGuardada.Nota;
                        continue;
                    }

                    bool marcadoPorClave =
                        brief.ClavesEntregablesSeleccionados != null &&
                        brief.ClavesEntregablesSeleccionados.Any(c =>
                            NormalizarTextoDatosVisual(c) ==
                            NormalizarTextoDatosVisual(claveFila)
                        );

                    EntregableBrief existente = brief.EntregablesSeleccionados
                        .FirstOrDefault(e =>
                            !string.IsNullOrWhiteSpace(e.ClaveSeleccion) &&
                            NormalizarTextoDatosVisual(e.ClaveSeleccion) ==
                            NormalizarTextoDatosVisual(claveFila)
                        );

                    if (existente == null)
                    {
                        existente = brief.EntregablesSeleccionados
                            .FirstOrDefault(e =>
                                NormalizarTextoDatosVisual(e.Nombre) ==
                                NormalizarTextoDatosVisual(nombre)
                            );
                    }

                    if (!brief.SeleccionarTodosEntregablesActivo &&
                        !marcadoPorClave &&
                        existente == null)
                    {
                        row.Cells["Usar"].Value = false;
                        continue;
                    }

                    row.Cells["Usar"].Value = true;

                    if (existente == null)
                    {
                        continue;
                    }

                    row.Cells["Cantidad"].Value = existente.Cantidad;
                    row.Cells["DuracionPorUnidad"].Value = existente.DuracionPorUnidad;
                    row.Cells["UnidadDuracion"].Value = string.IsNullOrWhiteSpace(existente.UnidadDuracion)
                        ? "segundos"
                        : existente.UnidadDuracion;

                    row.Cells["UnidadCantidad"].Value = string.IsNullOrWhiteSpace(existente.UnidadCantidad)
                        ? "unidades"
                        : existente.UnidadCantidad;

                    row.Cells["Nota"].Value = existente.Nota;
                }
            }
            finally
            {
                actualizandoEntregablesPorLote = estadoAnteriorLote;
            }

            RefrescarBotonesCategoriaEntregables();
        }

        private string CrearClaveSeleccionEntregable(
            string categoria,
            string nombre,
            string etapa,
            string subEtapa
        )
        {
            return
                NormalizarTextoDatosVisual(categoria) + "|" +
                NormalizarTextoDatosVisual(nombre) + "|" +
                NormalizarTextoDatosVisual(etapa) + "|" +
                NormalizarTextoDatosVisual(subEtapa);
        }

        private void GuardarInsumosClienteDesdePantalla(BriefProductoProyecto brief)
        {
            brief.InsumosClienteSeleccionados.Clear();

            GuardarInsumoSiCorresponde(brief, chkClienteEntregaGuion, "Guion");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaEstilo, "Estilo / referencias");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaStoryboard, "Storyboard");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaAnimatic, "Animatic");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaPersonajes, "Personajes");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaFondos, "Fondos");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaProps, "Props");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaAnimacion, "Animación");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaAudio, "Audio");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaAssetsEditables, "Assets editables");
            GuardarInsumoSiCorresponde(brief, chkClienteEntregaMaterialGrabado, "Material grabado");

            brief.ClienteEntregaGuion = chkClienteEntregaGuion.Checked;
            brief.ClienteEntregaEstilo = chkClienteEntregaEstilo.Checked;
            brief.ClienteEntregaStoryboard = chkClienteEntregaStoryboard.Checked;
            brief.ClienteEntregaAnimatic = chkClienteEntregaAnimatic.Checked;
            brief.ClienteEntregaPersonajes = chkClienteEntregaPersonajes.Checked;
            brief.ClienteEntregaFondos = chkClienteEntregaFondos.Checked;
            brief.ClienteEntregaProps = chkClienteEntregaProps.Checked;
            brief.ClienteEntregaAnimacion = chkClienteEntregaAnimacion.Checked;
            brief.ClienteEntregaAudio = chkClienteEntregaAudio.Checked;
            brief.ClienteEntregaAssetsEditables = chkClienteEntregaAssetsEditables.Checked;
            brief.ClienteEntregaMaterialGrabado = chkClienteEntregaMaterialGrabado.Checked;
        }

        private void GuardarInsumoSiCorresponde(
            BriefProductoProyecto brief,
            CheckBox check,
            string nombre
        )
        {
            if (check != null && check.Checked)
            {
                brief.InsumosClienteSeleccionados.Add(nombre);
            }
        }

        private void CargarInsumosClienteEnPantalla(BriefProductoProyecto brief)
        {
            chkClienteEntregaGuion.Checked =
                brief.ClienteEntregaGuion ||
                brief.InsumosClienteSeleccionados.Contains("Guion");

            chkClienteEntregaEstilo.Checked =
                brief.ClienteEntregaEstilo ||
                brief.InsumosClienteSeleccionados.Contains("Estilo / referencias");

            chkClienteEntregaStoryboard.Checked =
                brief.ClienteEntregaStoryboard ||
                brief.InsumosClienteSeleccionados.Contains("Storyboard");

            chkClienteEntregaAnimatic.Checked =
                brief.ClienteEntregaAnimatic ||
                brief.InsumosClienteSeleccionados.Contains("Animatic");

            chkClienteEntregaPersonajes.Checked =
                brief.ClienteEntregaPersonajes ||
                brief.InsumosClienteSeleccionados.Contains("Personajes");

            chkClienteEntregaFondos.Checked =
                brief.ClienteEntregaFondos ||
                brief.InsumosClienteSeleccionados.Contains("Fondos");

            chkClienteEntregaProps.Checked =
                brief.ClienteEntregaProps ||
                brief.InsumosClienteSeleccionados.Contains("Props");

            chkClienteEntregaAnimacion.Checked =
                brief.ClienteEntregaAnimacion ||
                brief.InsumosClienteSeleccionados.Contains("Animación");

            chkClienteEntregaAudio.Checked =
                brief.ClienteEntregaAudio ||
                brief.InsumosClienteSeleccionados.Contains("Audio");

            chkClienteEntregaAssetsEditables.Checked =
                brief.ClienteEntregaAssetsEditables ||
                brief.InsumosClienteSeleccionados.Contains("Assets editables");

            chkClienteEntregaMaterialGrabado.Checked =
                brief.ClienteEntregaMaterialGrabado ||
                brief.InsumosClienteSeleccionados.Contains("Material grabado");
        }

        private void ActualizarResumenCompatibilidadBrief(BriefProductoProyecto brief)
        {
            if (brief.EntregablesSeleccionados == null || brief.EntregablesSeleccionados.Count == 0)
            {
                brief.TipoPiezaPrincipal = "";
                brief.TipoProducto = "";
                brief.CantidadPiezas = 1;
                brief.DuracionProductoValor = 0.0;
                brief.DuracionProductoUnidad = "segundos";
                return;
            }

            if (brief.EntregablesSeleccionados.Count == 1)
            {
                EntregableBrief unico = brief.EntregablesSeleccionados[0];

                brief.TipoPiezaPrincipal = unico.Nombre;
                brief.TipoProducto = unico.Nombre;
                brief.CantidadPiezas = unico.Cantidad;
                brief.DuracionProductoValor = unico.DuracionPorUnidad;
                brief.DuracionProductoUnidad = unico.UnidadDuracion;
                return;
            }

            brief.TipoPiezaPrincipal = "Pedido mixto 2D";
            brief.TipoProducto = "Pedido mixto 2D";
            brief.CantidadPiezas = brief.EntregablesSeleccionados.Sum(e => e.Cantidad);
            brief.DuracionProductoValor = brief.EntregablesSeleccionados.Sum(e =>
                e.Cantidad * e.DuracionPorUnidad
            );
            brief.DuracionProductoUnidad = "segundos equivalentes";
        }

        private void ActualizarFlagsCompatibilidadDesdeEntregables(BriefProductoProyecto brief)
        {
            string texto = string.Join(
                " ",
                brief.EntregablesSeleccionados.Select(e => e.Nombre)
            ).ToLowerInvariant();

            brief.RequierePersonajes =
                texto.Contains("personaje") ||
                texto.Contains("idle") ||
                texto.Contains("walk") ||
                texto.Contains("run") ||
                texto.Contains("attack") ||
                texto.Contains("damage") ||
                texto.Contains("death");

            brief.RequiereFondos =
                texto.Contains("background") ||
                texto.Contains("fondo") ||
                texto.Contains("escenario");

            brief.RequiereProps =
                texto.Contains("props") ||
                texto.Contains("objetos");

            brief.RequiereAnimacionPersonajes =
                texto.Contains("loop") ||
                texto.Contains("walk") ||
                texto.Contains("run") ||
                texto.Contains("attack") ||
                texto.Contains("animacion") ||
                texto.Contains("animación") ||
                texto.Contains("idle");

            brief.RequiereMotionGraphics =
                texto.Contains("motion") ||
                texto.Contains("ui animada") ||
                texto.Contains("subtítulos") ||
                texto.Contains("subtitulos");

            brief.RequiereEdicion =
                texto.Contains("trailer") ||
                texto.Contains("teaser") ||
                texto.Contains("cinemática") ||
                texto.Contains("cinematica") ||
                texto.Contains("cutscene") ||
                texto.Contains("edición") ||
                texto.Contains("edicion");

            brief.RequiereAudio =
                texto.Contains("audio") ||
                texto.Contains("locución") ||
                texto.Contains("locucion");

            brief.RequiereExportFinal = true;
        }

        private void BtnAplicarDatos_Click(object sender, EventArgs e)
        {
            if (aplicarDatosProgramadoDesdeMouse)
            {
                return;
            }

            EjecutarAplicarDatosYActualizar();
        }

        private void BtnAplicarDatos_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (aplicarDatosProgramadoDesdeMouse)
            {
                return;
            }

            aplicarDatosProgramadoDesdeMouse = true;

            BeginInvoke(new Action(() =>
            {
                try
                {
                    EjecutarAplicarDatosYActualizar();
                }
                finally
                {
                    aplicarDatosProgramadoDesdeMouse = false;
                }
            }));
        }

        private void EjecutarAplicarDatosYActualizar()
        {
            PrepararControlesDatosParaAplicar();
            AplicarDatosDesdePantalla();

            /*
             * Si ya existe la pestaña de desglose/productivo abierta o cargada,
             * regeneramos inmediatamente para que no quede con datos antiguos.
             */
            GenerarDesgloseProductivoDesdeEcuaciones();
            CargarDesgloseProductivoEnPantalla();
            RefrescarResumenDesgloseProductivo();
            RefrescarDespuesDeEditarDesgloseProductivo();
            AplicarEstandarDesgloseYPropuestaAEtapasDesdeDatos();
            RellenarManoObraDesdeDesgloseProductivo(false);

            RefrescarResumen();
            RefrescarPanelSiguientePasoDatos();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }
        }

        private void PrepararControlesDatosParaAplicar()
        {
            try
            {
                ValidateChildren();
            }
            catch
            {
            }

            try
            {
                if (dgvEntregablesIndustria != null)
                {
                    if (dgvEntregablesIndustria.IsCurrentCellDirty)
                    {
                        dgvEntregablesIndustria.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    }

                    dgvEntregablesIndustria.EndEdit(DataGridViewDataErrorContexts.Commit);
                    dgvEntregablesIndustria.EndEdit();
                }
            }
            catch
            {
            }

            try
            {
                if (dgvEntregablesIndustria != null)
                {
                    BindingContext[dgvEntregablesIndustria.DataSource]?.EndCurrentEdit();
                }
            }
            catch
            {
            }
        }

        private void BtnAplicarSugerenciaBrief_Click(object sender, EventArgs e)
        {
            RefrescarOpcionesSegunIndustria();
            AplicarDatosDesdePantalla();
        }

        private void RefrescarPanelSiguientePasoDatos()
        {
            if (panelSiguientePasoDatos != null)
            {
                panelSiguientePasoDatos.Visible = true;
            }

            if (lblSiguientePasoDatos == null)
            {
                return;
            }

            string monedaVisual = ObtenerMonedaInternaParaInforme();
            BriefProductoProyecto brief = ObtenerBriefProductoSeguro();

            EvaluacionPlazoProyecto evaluacionPlazo =
    EvaluadorPlazoProyectoService.Evaluar(cotizacion);

            string textoPlazo = "";

            if (evaluacionPlazo.SemanasCliente > 0.0)
            {
                textoPlazo =
                    $"Plazo cliente: {evaluacionPlazo.SemanasCliente:N1} semanas | " +
                    $"Mínimo técnico: {evaluacionPlazo.SemanasMinimasEstimadas:N1} | " +
                    $"Estándar: {evaluacionPlazo.SemanasEstandarEstimadas:N1} | " +
                    $"Holgura: {evaluacionPlazo.SemanasConHolguraEstimadas:N1}\n" +
                    $"Diagnóstico plazo: {evaluacionPlazo.DiagnosticoPlazo}";
            }
            else
            {
                textoPlazo = "Plazo cliente: no informado.";
            }

            string industria = string.IsNullOrWhiteSpace(brief.IndustriaCliente)
                ? "sin industria definida"
                : brief.IndustriaCliente;

            string destino = string.IsNullOrWhiteSpace(brief.DestinoUso)
                ? "sin destino definido"
                : brief.DestinoUso;

            string pieza = string.IsNullOrWhiteSpace(brief.TipoPiezaPrincipal)
                ? "sin productos 2D definidos"
                : brief.TipoPiezaPrincipal;

            int cantidadEntregables = brief.EntregablesSeleccionados == null
                ? 0
                : brief.EntregablesSeleccionados.Count;

            int cantidadTotal = brief.EntregablesSeleccionados == null
                ? 0
                : brief.EntregablesSeleccionados.Sum(e => e.Cantidad);

            decimal presupuestoCliente = ParsearDecimalFlexible(txtPresupuestoCliente.Text);
            string monedaCliente = cmbMoneda.SelectedItem?.ToString() ?? ObtenerMonedaInternaParaInforme();

            string textoPresupuesto = presupuestoCliente > 0
                ? $"Presupuesto cliente: {FormatearMontoDatos(presupuestoCliente)} {monedaCliente}"
                : "Presupuesto cliente: no informado";

            string textoDistribucion = ConstruirTextoDistribucionPresupuestoDatos(
                presupuestoCliente,
                monedaCliente,
                brief
            );

            lblSiguientePasoDatos.Text =
    $"Moneda para informe interno: {monedaVisual}\n" +
    $"Industria: {industria} | Destino: {destino}\n" +
    $"Productos: {pieza} | Tipos seleccionados: {cantidadEntregables} | Cantidad total: {cantidadTotal}\n" +
    $"{textoPlazo}\n" +
    "Siguiente paso: avanzar a Etapas para transformar el brief en subprocesos recomendados.";
        }

        private string ConstruirTextoDistribucionPresupuestoDatos(
    decimal presupuesto,
    string moneda,
    BriefProductoProyecto brief
)
        {
            if (presupuesto <= 0m)
            {
                return "Distribución sugerida: ingresa un presupuesto para estimar cuánto podría asignarse por etapa.";
            }

            decimal porcentajeDesarrollo = 0.15m;
            decimal porcentajePreproduccion = 0.25m;
            decimal porcentajeProduccion = 0.45m;
            decimal porcentajePostproduccion = 0.15m;

            string textoEntregables = "";

            if (brief != null && brief.EntregablesSeleccionados != null)
            {
                textoEntregables = string.Join(
                    " ",
                    brief.EntregablesSeleccionados.Select(e => e.Nombre)
                ).ToLowerInvariant();
            }

            bool tieneAnimacionPesada =
                textoEntregables.Contains("animación") ||
                textoEntregables.Contains("animacion") ||
                textoEntregables.Contains("cinemática") ||
                textoEntregables.Contains("cinematica") ||
                textoEntregables.Contains("trailer") ||
                textoEntregables.Contains("teaser") ||
                textoEntregables.Contains("cutscene");

            bool tieneAssetsEstaticos =
                textoEntregables.Contains("personaje") ||
                textoEntregables.Contains("fondo") ||
                textoEntregables.Contains("background") ||
                textoEntregables.Contains("props") ||
                textoEntregables.Contains("asset");

            if (tieneAnimacionPesada)
            {
                porcentajeDesarrollo = 0.10m;
                porcentajePreproduccion = 0.20m;
                porcentajeProduccion = 0.50m;
                porcentajePostproduccion = 0.20m;
            }
            else if (tieneAssetsEstaticos)
            {
                porcentajeDesarrollo = 0.15m;
                porcentajePreproduccion = 0.30m;
                porcentajeProduccion = 0.40m;
                porcentajePostproduccion = 0.15m;
            }

            decimal montoDesarrollo = presupuesto * porcentajeDesarrollo;
            decimal montoPreproduccion = presupuesto * porcentajePreproduccion;
            decimal montoProduccion = presupuesto * porcentajeProduccion;
            decimal montoPostproduccion = presupuesto * porcentajePostproduccion;

            string alerta = ConstruirAlertaPresupuestoDatos(presupuesto, moneda, brief);

            return
                "Distribución sugerida del presupuesto:\n" +
                $"- Desarrollo: {FormatearMontoDatos(montoDesarrollo)} {moneda} ({porcentajeDesarrollo:P0})\n" +
                $"- Preproducción: {FormatearMontoDatos(montoPreproduccion)} {moneda} ({porcentajePreproduccion:P0})\n" +
                $"- Producción: {FormatearMontoDatos(montoProduccion)} {moneda} ({porcentajeProduccion:P0})\n" +
                $"- Postproducción: {FormatearMontoDatos(montoPostproduccion)} {moneda} ({porcentajePostproduccion:P0})" +
                alerta;
        }


        private string ConstruirAlertaPresupuestoDatos(
    decimal presupuesto,
    string moneda,
    BriefProductoProyecto brief
)
        {
            if (presupuesto <= 0m || brief == null || brief.EntregablesSeleccionados == null)
            {
                return "";
            }

            int cantidadTotal = brief.EntregablesSeleccionados.Sum(e => e.Cantidad);

            if (cantidadTotal <= 0)
            {
                return "";
            }

            decimal presupuestoUnitario = presupuesto / cantidadTotal;

            if (moneda == "CLP")
            {
                if (presupuestoUnitario < 10000m)
                {
                    return "\nALERTA: el presupuesto unitario es menor a $10.000 CLP. Comercialmente no recomendable.";
                }

                if (presupuestoUnitario < 50000m)
                {
                    return "\nAdvertencia: el presupuesto unitario es bajo. Revisar alcance antes de aceptar.";
                }
            }

            if (moneda == "USD")
            {
                if (presupuestoUnitario < 15m)
                {
                    return "\nALERTA: el presupuesto unitario es extremadamente bajo para producción 2D.";
                }

                if (presupuestoUnitario < 60m)
                {
                    return "\nAdvertencia: el presupuesto unitario es bajo. Revisar alcance antes de aceptar.";
                }
            }

            return "";
        }

        private string FormatearMontoDatos(decimal monto)
        {
            return monto.ToString("N0");
        }

        private string ObtenerMonedaInternaParaInforme()
        {
            if (!string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion))
            {
                return cotizacion.MonedaVisualizacion;
            }

            if (!string.IsNullOrWhiteSpace(cotizacion.Moneda))
            {
                return cotizacion.Moneda;
            }

            return "CLP";
        }

        private void BtnDatosIrMoneda_Click(object sender, EventArgs e)
        {
            AplicarDatosDesdePantalla();
            IrATabPorNombre("Moneda");
        }

        private void BtnDatosIrEtapas_Click(object sender, EventArgs e)
        {
            AplicarDatosDesdePantalla();
            IrATabPorNombre("Etapas");
        }

        private void IrATabPorNombre(string nombreTab)
        {
            if (tabs == null)
            {
                return;
            }

            for (int i = 0; i < tabs.TabPages.Count; i++)
            {
                if (tabs.TabPages[i].Text == nombreTab)
                {
                    tabs.SelectedIndex = i;
                    return;
                }
            }
        }

        // =========================================================
        // COMPATIBILIDAD TEMPORAL - MIGRACIÓN INDUSTRIA -> PRODUCTO
        // =========================================================
        // Estos wrappers evitan errores en otros archivos parciales
        // que todavía llaman nombres antiguos.

        private ComboBox cmbIndustriaCliente
        {
            get
            {
                return cmbProductoServicio;
            }
        }

        private void RefrescarOpcionesSegunIndustria()
        {
            RefrescarOpcionesSegunProducto();
        }

        private void CmbIndustriaCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefrescarOpcionesSegunProducto();
        }

        private void DatosConfigurarComboIndustrias()
        {
            DatosConfigurarComboProductosServicios();
        }

        private void CampoDatos_Leave(object sender, EventArgs e)
        {
            // Intencionalmente vacío.
        }

        private void CmbMoneda_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Intencionalmente vacío.
        }

        private void SeleccionarComboSiExiste(ComboBox combo, string valor)
        {
            if (combo == null || combo.Items == null || combo.Items.Count == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(valor) && combo.Items.Contains(valor))
            {
                combo.SelectedItem = valor;
                return;
            }

            if (combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private decimal ParsearDecimalFlexible(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 0m;
            }

            string limpio = texto
                .Trim()
                .Replace("$", "")
                .Replace("CLP", "")
                .Replace("USD", "")
                .Replace("EUR", "")
                .Replace("JPY", "")
                .Replace("KRW", "")
                .Replace("UF", "")
                .Trim();

            decimal valor;

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.CurrentCulture, out valor))
            {
                return valor < 0m ? 0m : valor;
            }

            limpio = limpio.Replace(".", "").Replace(",", ".");

            if (decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
            {
                return valor < 0m ? 0m : valor;
            }

            return 0m;
        }

        private double ParsearDoubleFlexible(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 0.0;
            }

            string limpio = texto.Trim();

            double valor;

            if (double.TryParse(limpio, NumberStyles.Any, CultureInfo.CurrentCulture, out valor))
            {
                return valor < 0.0 ? 0.0 : valor;
            }

            limpio = limpio.Replace(",", ".");

            if (double.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out valor))
            {
                return valor < 0.0 ? 0.0 : valor;
            }

            return 0.0;
        }

        private string NormalizarTextoDatos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            return texto
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(" ", "");
        }
    }
}
