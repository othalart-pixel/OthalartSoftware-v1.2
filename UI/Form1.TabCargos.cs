using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private const string NombreOpcionTodosLosCargos = "Todos los cargos";

        private const string NombreOpcionChecklistCargos = "Checklist del proyecto";

        private bool cargandoChecklistCargos = false;

        private Dictionary<string, CargoChecklistSeleccion> checklistCargosProyecto =
            new Dictionary<string, CargoChecklistSeleccion>();

        private List<CargoPlanMensual> planGeneralProyecto = new List<CargoPlanMensual>();
        private class CargoChecklistSeleccion
        {
            public string Etapa { get; set; } = "";
            public string CargoBase { get; set; } = "";
            public string NivelSeleccionado { get; set; } = "";
            public bool Usar { get; set; } = false;
        }

        private class CargoChecklistRow
        {
            public string Etapa { get; set; } = "";
            public string CargoBase { get; set; } = "";
            public string RecomendadoPor { get; set; } = "";

            public List<CategoriaTrabajador> OpcionesNivel { get; set; } =
                new List<CategoriaTrabajador>();
        }

        private void ConstruirTabCargos(TabPage tab)
        {
            tab.Enter -= TabCargos_Enter;
            tab.Enter += TabCargos_Enter;

            tab.Controls.Clear();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(15);
            layout.ColumnCount = 1;
            layout.RowCount = 2;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 95));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 6;
            panelSuperior.RowCount = 2;

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            Label titulo = new Label();
            titulo.Text = "Biblioteca de cargos y valores estimados";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            panelSuperior.Controls.Add(titulo, 0, 0);
            panelSuperior.SetColumnSpan(titulo, 6);

            Label lblEtapa = new Label();
            lblEtapa.Text = "Biblioteca:";
            lblEtapa.Dock = DockStyle.Fill;
            lblEtapa.TextAlign = ContentAlignment.MiddleLeft;

            cmbEtapaBibliotecaCargos.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEtapaBibliotecaCargos.Dock = DockStyle.Fill;
            cmbEtapaBibliotecaCargos.SelectedIndexChanged -= CmbEtapaBibliotecaCargos_SelectedIndexChanged;
            cmbEtapaBibliotecaCargos.SelectedIndexChanged += CmbEtapaBibliotecaCargos_SelectedIndexChanged;

            btnAgregarBibliotecaCargo.Text = "Agregar cargo";
            btnAgregarBibliotecaCargo.Width = 130;
            btnAgregarBibliotecaCargo.Height = 30;
            btnAgregarBibliotecaCargo.Click -= BtnAgregarBibliotecaCargo_Click;
            btnAgregarBibliotecaCargo.Click += BtnAgregarBibliotecaCargo_Click;

            btnEliminarBibliotecaCargo.Text = "Eliminar cargo";
            btnEliminarBibliotecaCargo.Width = 130;
            btnEliminarBibliotecaCargo.Height = 30;
            btnEliminarBibliotecaCargo.Click -= BtnEliminarBibliotecaCargo_Click;
            btnEliminarBibliotecaCargo.Click += BtnEliminarBibliotecaCargo_Click;

            btnAplicarBibliotecaCargos.Text = "Aplicar cambios";
            btnAplicarBibliotecaCargos.Width = 130;
            btnAplicarBibliotecaCargos.Height = 30;
            btnAplicarBibliotecaCargos.Click -= BtnAplicarBibliotecaCargos_Click;
            btnAplicarBibliotecaCargos.Click += BtnAplicarBibliotecaCargos_Click;

            panelSuperior.Controls.Add(lblEtapa, 0, 1);
            panelSuperior.Controls.Add(cmbEtapaBibliotecaCargos, 1, 1);
            panelSuperior.Controls.Add(btnAgregarBibliotecaCargo, 2, 1);
            panelSuperior.Controls.Add(btnEliminarBibliotecaCargo, 3, 1);
            panelSuperior.Controls.Add(btnAplicarBibliotecaCargos, 4, 1);

            dgvBibliotecaCargos.Dock = DockStyle.Fill;
            dgvBibliotecaCargos.AllowUserToAddRows = false;
            dgvBibliotecaCargos.AllowUserToDeleteRows = false;
            dgvBibliotecaCargos.RowHeadersVisible = false;
            dgvBibliotecaCargos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvBibliotecaCargos.ScrollBars = ScrollBars.Both;
            dgvBibliotecaCargos.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvBibliotecaCargos.CellValueChanged -= DgvBibliotecaCargos_CellValueChanged;

            dgvBibliotecaCargos.Columns.Clear();

            dgvBibliotecaCargos.Columns.Add("Id", "ID");
            dgvBibliotecaCargos.Columns.Add("Nombre", "Nombre");
            dgvBibliotecaCargos.Columns.Add("Nivel", "Nivel");
            dgvBibliotecaCargos.Columns.Add("Alcance", "Alcance");

            DataGridViewComboBoxColumn colTipoCargo = new DataGridViewComboBoxColumn();
            colTipoCargo.Name = "TipoCargo";
            colTipoCargo.HeaderText = "Tipo";
            colTipoCargo.Items.AddRange(
                ClasificacionCargosService.TipoProductivo,
                ClasificacionCargosService.TipoGestion,
                ClasificacionCargosService.TipoApoyo
            );
            dgvBibliotecaCargos.Columns.Add(colTipoCargo);

            dgvBibliotecaCargos.Columns.Add("CLPMin", "CLP mínimo");
            dgvBibliotecaCargos.Columns.Add("CLPTipico", "CLP típico");
            dgvBibliotecaCargos.Columns.Add("CLPMax", "CLP máximo");

            dgvBibliotecaCargos.Columns.Add("VisualMin", "Visual mínimo");
            dgvBibliotecaCargos.Columns.Add("VisualTipico", "Visual típico");
            dgvBibliotecaCargos.Columns.Add("VisualMax", "Visual máximo");

            dgvBibliotecaCargos.Columns["Id"].ReadOnly = true;
            dgvBibliotecaCargos.Columns["Alcance"].ReadOnly = true;

            dgvBibliotecaCargos.Columns["VisualMin"].ReadOnly = true;
            dgvBibliotecaCargos.Columns["VisualTipico"].ReadOnly = true;
            dgvBibliotecaCargos.Columns["VisualMax"].ReadOnly = true;

            dgvBibliotecaCargos.Columns["Id"].Width = 55;
            dgvBibliotecaCargos.Columns["Nombre"].Width = 230;
            dgvBibliotecaCargos.Columns["Nivel"].Width = 120;
            dgvBibliotecaCargos.Columns["Alcance"].Width = 130;
            dgvBibliotecaCargos.Columns["TipoCargo"].Width = 120;

            dgvBibliotecaCargos.Columns["CLPMin"].Width = 120;
            dgvBibliotecaCargos.Columns["CLPTipico"].Width = 120;
            dgvBibliotecaCargos.Columns["CLPMax"].Width = 120;

            dgvBibliotecaCargos.Columns["VisualMin"].Width = 120;
            dgvBibliotecaCargos.Columns["VisualTipico"].Width = 120;
            dgvBibliotecaCargos.Columns["VisualMax"].Width = 120;

            AplicarEstiloTablaBibliotecaCargos(dgvBibliotecaCargos);
            DesactivarOrdenamientoColumnasBibliotecaCargos();

            dgvBibliotecaCargos.CellValueChanged -= DgvBibliotecaCargos_CellValueChanged;
            dgvBibliotecaCargos.CellValueChanged += DgvBibliotecaCargos_CellValueChanged;

            dgvBibliotecaCargos.CellClick -= DgvBibliotecaCargos_CellClick;
            dgvBibliotecaCargos.CellClick += DgvBibliotecaCargos_CellClick;

            dgvBibliotecaCargos.CurrentCellDirtyStateChanged -= DgvBibliotecaCargos_CurrentCellDirtyStateChanged;
            dgvBibliotecaCargos.CurrentCellDirtyStateChanged += DgvBibliotecaCargos_CurrentCellDirtyStateChanged;

            layout.Controls.Add(panelSuperior, 0, 0);
            layout.Controls.Add(dgvBibliotecaCargos, 0, 1);

            tab.Controls.Add(layout);

            CargarBibliotecaCargosDesdeJsonEnMemoria();

            CargarComboEtapasBibliotecaCargos();
            CargarTablaBibliotecaCargos();
            ActivarActualizacionGanttEnVivo();
        }

        private void TabCargos_Enter(object sender, EventArgs e)
        {
            CargarBibliotecaCargosDesdeJsonEnMemoria();

            /*
             * Al entrar a Cargos, primero actualizamos Subetapas,
             * porque el checklist depende de los subprocesos activos.
             */
            RefrescarTablaSubEtapasSiExiste();

            /*
             * Luego regeneramos la vista de cargos.
             * Si está en Checklist del proyecto, recalcula recomendados y otros cargos.
             */
            RefrescarVistaCargosSiExiste();
        }

        private void DgvBibliotecaCargos_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvBibliotecaCargos == null || dgvBibliotecaCargos.CurrentCell == null)
            {
                return;
            }

            if (dgvBibliotecaCargos.IsCurrentCellDirty)
            {
                dgvBibliotecaCargos.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvBibliotecaCargos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || cargandoChecklistCargos || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (!EstaEnModoChecklistCargos())
            {
                return;
            }

            string columna = dgvBibliotecaCargos.Columns[e.ColumnIndex].Name;
            DataGridViewRow row = dgvBibliotecaCargos.Rows[e.RowIndex];

            if (row == null || row.IsNewRow)
            {
                return;
            }

            CargoChecklistRow info = row.Tag as CargoChecklistRow;

            if (info == null)
            {
                return;
            }

            if (columna == "Id")
            {
                bool valorActual = false;

                object valorCelda = row.Cells["Id"].Value;

                if (valorCelda is bool valorBool)
                {
                    valorActual = valorBool;
                }

                row.Cells["Id"].Value = !valorActual;

                AplicarCambiosChecklistCargosProyecto();

                // Esto es lo nuevo:
                // al clickear, se rellena altiro Mano de obra.
                SincronizarChecklistCargosConManoDeObra();

                CargarTablaChecklistCargosProyecto();

                return;
            }

            if (columna == "Nivel")
            {
                dgvBibliotecaCargos.BeginEdit(true);
                return;
            }
        }


        private void AplicarEstiloTablaBibliotecaCargos(DataGridView dgv)
        {
            if (dgv == null)
            {
                return;
            }

            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersHeight = 32;

            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.BackgroundColor = Color.FromArgb(170, 170, 170);
            dgv.BorderStyle = BorderStyle.FixedSingle;
        }


        private void DesactivarOrdenamientoColumnasBibliotecaCargos()
        {
            if (dgvBibliotecaCargos == null || dgvBibliotecaCargos.Columns == null)
            {
                return;
            }

            foreach (DataGridViewColumn columna in dgvBibliotecaCargos.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        private void CargarComboEtapasBibliotecaCargos()
        {
            if (cmbEtapaBibliotecaCargos == null)
            {
                return;
            }

            string nombreAnterior = "";

            if (cmbEtapaBibliotecaCargos.SelectedItem is SelectorBibliotecaCargos selectorAnterior)
            {
                nombreAnterior = selectorAnterior.Nombre;
            }

            cmbEtapaBibliotecaCargos.SelectedIndexChanged -= CmbEtapaBibliotecaCargos_SelectedIndexChanged;

            cmbEtapaBibliotecaCargos.Items.Clear();

            cmbEtapaBibliotecaCargos.Items.Add(new SelectorBibliotecaCargos
            {
                Nombre = NombreOpcionChecklistCargos,
                EsGeneral = false,
                Etapa = null
            });

            cmbEtapaBibliotecaCargos.Items.Add(new SelectorBibliotecaCargos
            {
                Nombre = NombreOpcionTodosLosCargos,
                EsGeneral = false,
                Etapa = null
            });

            cmbEtapaBibliotecaCargos.Items.Add(new SelectorBibliotecaCargos
            {
                Nombre = NombreOpcionCargosGenerales,
                EsGeneral = true,
                Etapa = null
            });

            foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapaParaCargos(e.Nombre)))
            {
                cmbEtapaBibliotecaCargos.Items.Add(new SelectorBibliotecaCargos
                {
                    Nombre = etapa.Nombre,
                    EsGeneral = false,
                    Etapa = etapa
                });
            }

            cmbEtapaBibliotecaCargos.DisplayMember = "Nombre";

            int indice = 0;

            for (int i = 0; i < cmbEtapaBibliotecaCargos.Items.Count; i++)
            {
                SelectorBibliotecaCargos selector =
                    cmbEtapaBibliotecaCargos.Items[i] as SelectorBibliotecaCargos;

                if (selector != null && selector.Nombre == nombreAnterior)
                {
                    indice = i;
                    break;
                }
            }

            if (cmbEtapaBibliotecaCargos.Items.Count > 0)
            {
                cmbEtapaBibliotecaCargos.SelectedIndex = indice;
            }

            cmbEtapaBibliotecaCargos.SelectedIndexChanged += CmbEtapaBibliotecaCargos_SelectedIndexChanged;
        }

        private bool EstaVisualizandoCLP()
        {
            return string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion) ||
                   cotizacion.MonedaVisualizacion == "CLP";
        }

        private void CargarBibliotecaCargosDesdeJsonEnMemoria()
        {
            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();

            if (cargos == null)
            {
                cargos = new List<CategoriaTrabajador>();
            }

            bibliotecaCargosGenerales = cargos
                .Where(c => c != null && NormalizarNombreEtapa(c.Bloque).Contains("general"))
                .OrderBy(c => c.Id)
                .ToList();

            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                string bloqueEtapa = ObtenerBloqueCargoDesdeNombreEtapa(etapa.Nombre);

                etapa.Biblioteca = cargos
                    .Where(c => c != null)
                    .Where(c => NormalizarNombreEtapa(c.Bloque) == NormalizarNombreEtapa(bloqueEtapa))
                    .OrderBy(c => c.Id)
                    .ToList();
            }
        }

        private void GuardarBibliotecaCargosActualEnJson()
        {
            List<CategoriaTrabajador> cargos = new List<CategoriaTrabajador>();

            if (bibliotecaCargosGenerales != null)
            {
                foreach (CategoriaTrabajador cargo in bibliotecaCargosGenerales)
                {
                    if (cargo == null)
                    {
                        continue;
                    }

                    cargo.Bloque = "General";
                    cargos.Add(cargo);
                }
            }

            if (cotizacion != null && cotizacion.Etapas != null)
            {
                foreach (EtapaProyecto etapa in cotizacion.Etapas)
                {
                    if (etapa == null || etapa.Biblioteca == null)
                    {
                        continue;
                    }

                    string bloqueEtapa = ObtenerBloqueCargoDesdeNombreEtapa(etapa.Nombre);

                    foreach (CategoriaTrabajador cargo in etapa.Biblioteca)
                    {
                        if (cargo == null)
                        {
                            continue;
                        }

                        if (EsCargoGeneral(cargo))
                        {
                            continue;
                        }

                        if (CargoEsDuplicadoDeGeneral(cargo))
                        {
                            continue;
                        }

                        cargo.Bloque = bloqueEtapa;
                        cargos.Add(cargo);
                    }
                }
            }

            cargos = cargos
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Id)
                .ToList();

            Cargos.GuardarBibliotecaCompleta(cargos);
        }

        private string ObtenerBloqueCargoDesdeNombreEtapa(string nombreEtapa)
        {
            string n = NormalizarNombreEtapa(nombreEtapa);

            if (n.Contains("desarrollo"))
            {
                return "Desarrollo";
            }

            if (n.Contains("preproduccion") || n.Contains("pre"))
            {
                return "Preproduccion";
            }

            if (n.Contains("postproduccion") || n.Contains("post"))
            {
                return "Postproduccion";
            }

            if (n.Contains("videojuego") ||
                n.Contains("asset") ||
                n.Contains("sprite") ||
                n.Contains("interactivo"))
            {
                return "VideojuegosAssets";
            }

            if (n.Contains("tecnica") || n.Contains("interno"))
            {
                return "TecnicaInterna";
            }

            if (n.Contains("produccion"))
            {
                return "Produccion";
            }

            return nombreEtapa ?? "";
        }

        private void CargarTablaBibliotecaCargos()
        {
            if (dgvBibliotecaCargos == null || dgvBibliotecaCargos.Columns.Count == 0)
            {
                return;
            }

            cargandoTabla = true;

            try
            {
                /*
                 * IMPORTANTE:
                 * Los cargos generales NO se sincronizan dentro de cada etapa.
                 * Viven solamente en:
                 *
                 * - bibliotecaCargosGenerales
                 * - cotizacion.PlanGeneralManoObra
                 *
                 * Si los copiamos a cada etapa, después aparecen repetidos como:
                 * Desarrollo / Productor
                 * Preproduccion / Productor
                 * Produccion / Productor
                 * Postproduccion / Productor
                 *
                 * Eso es justamente lo que queremos evitar.
                 */
                CargarBibliotecaCargosDesdeJsonEnMemoria();

                dgvBibliotecaCargos.Rows.Clear();
                PrepararColumnasBibliotecaCargosNormal();

                string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                    ? "CLP"
                    : cotizacion.MonedaVisualizacion;

                bool mostrarVisual = !EstaVisualizandoCLP();

                if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
                {
                    dgvBibliotecaCargos.Columns["VisualMin"].Visible = mostrarVisual;
                    dgvBibliotecaCargos.Columns["VisualTipico"].Visible = mostrarVisual;
                    dgvBibliotecaCargos.Columns["VisualMax"].Visible = mostrarVisual;

                    dgvBibliotecaCargos.Columns["VisualMin"].HeaderText = monedaVisual + " mínimo";
                    dgvBibliotecaCargos.Columns["VisualTipico"].HeaderText = monedaVisual + " típico";
                    dgvBibliotecaCargos.Columns["VisualMax"].HeaderText = monedaVisual + " máximo";
                }

                SelectorBibliotecaCargos selector =
                    cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

                if (selector == null)
                {
                    DesactivarOrdenamientoColumnasBibliotecaCargos();
                    return;
                }

                if (selector.Nombre == NombreOpcionChecklistCargos)
                {
                    cargandoTabla = false;
                    CargarTablaChecklistCargosProyecto();
                    return;
                }

                bool vistaTodos = selector.Nombre == NombreOpcionTodosLosCargos;
                bool vistaGeneral = selector.EsGeneral;

                if (vistaTodos)
                {
                    /*
                     * Vista global editable:
                     * Primero muestra los cargos generales una sola vez.
                     */
                    if (bibliotecaCargosGenerales != null)
                    {
                        foreach (CategoriaTrabajador cargo in bibliotecaCargosGenerales.OrderBy(c => c.Id))
                        {
                            AgregarFilaCargoBiblioteca(cargo, "General / proyecto completo", true);
                        }
                    }

                    /*
                     * Luego muestra cargos propios de cada etapa.
                     * Se excluyen cargos generales y duplicados de generales.
                     */
                    foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapaParaCargos(e.Nombre)))
                    {
                        if (etapa == null || etapa.Biblioteca == null)
                        {
                            continue;
                        }

                        foreach (CategoriaTrabajador cargo in etapa.Biblioteca.OrderBy(c => c.Id))
                        {
                            if (cargo == null)
                            {
                                continue;
                            }

                            if (EsCargoGeneral(cargo))
                            {
                                continue;
                            }

                            if (CargoEsDuplicadoDeGeneral(cargo))
                            {
                                continue;
                            }

                            AgregarFilaCargoBiblioteca(cargo, etapa.Nombre, true);
                        }
                    }

                    ConfigurarEdicionVistaBibliotecaCargos(true);
                    return;
                }

                if (vistaGeneral)
                {
                    /*
                     * Vista editable de cargos generales.
                     * Aquí se editan cargos transversales:
                     * productor, director, coordinador, project manager, etc.
                     */
                    if (bibliotecaCargosGenerales != null)
                    {
                        foreach (CategoriaTrabajador cargo in bibliotecaCargosGenerales.OrderBy(c => c.Id))
                        {
                            AgregarFilaCargoBiblioteca(cargo, "General / proyecto completo", false);
                        }
                    }

                    ConfigurarEdicionVistaBibliotecaCargos(true);
                    return;
                }

                if (selector.Etapa != null)
                {
                    /*
                     * Vista editable de una etapa específica.
                     * Aquí NO mostramos cargos generales.
                     * La etapa solo debe contener cargos propios de producción de esa etapa.
                     */
                    if (selector.Etapa.Biblioteca != null)
                    {
                        foreach (CategoriaTrabajador cargo in selector.Etapa.Biblioteca.OrderBy(c => c.Id))
                        {
                            if (cargo == null)
                            {
                                continue;
                            }

                            if (EsCargoGeneral(cargo))
                            {
                                continue;
                            }

                            if (CargoEsDuplicadoDeGeneral(cargo))
                            {
                                continue;
                            }

                            AgregarFilaCargoBiblioteca(cargo, selector.Etapa.Nombre, false);
                        }
                    }

                    ConfigurarEdicionVistaBibliotecaCargos(true);

                    if (dgvBibliotecaCargos.Columns.Contains("Id"))
                    {
                        dgvBibliotecaCargos.Columns["Id"].ReadOnly = false;
                    }

                    return;
                }
            }
            finally
            {
                cargandoTabla = false;
                DesactivarOrdenamientoColumnasBibliotecaCargos();
            }
        }

        private void PrepararColumnasBibliotecaCargosNormal()
        {
            if (dgvBibliotecaCargos == null || dgvBibliotecaCargos.Columns.Count == 0)
            {
                return;
            }

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            bool monedaEsCLP = monedaVisual == "CLP";

            SelectorBibliotecaCargos selector =
                cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

            bool vistaTodos = selector != null &&
                              selector.Nombre == NombreOpcionTodosLosCargos;

            bool vistaChecklist = selector != null &&
                                  selector.Nombre == NombreOpcionChecklistCargos;

            bool vistaEditable = !vistaChecklist;

            if (vistaChecklist)
            {
                return;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Id"))
            {
                dgvBibliotecaCargos.Columns["Id"].HeaderText = "ID";
                dgvBibliotecaCargos.Columns["Id"].Width = 55;
                dgvBibliotecaCargos.Columns["Id"].ReadOnly = true;
                dgvBibliotecaCargos.Columns["Id"].Visible = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Nombre"))
            {
                dgvBibliotecaCargos.Columns["Nombre"].HeaderText = "Nombre";
                dgvBibliotecaCargos.Columns["Nombre"].Width = 230;
                dgvBibliotecaCargos.Columns["Nombre"].ReadOnly = !vistaEditable;
                dgvBibliotecaCargos.Columns["Nombre"].Visible = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Nivel"))
            {
                dgvBibliotecaCargos.Columns["Nivel"].HeaderText = "Nivel";
                dgvBibliotecaCargos.Columns["Nivel"].Width = 120;
                dgvBibliotecaCargos.Columns["Nivel"].ReadOnly = !vistaEditable;
                dgvBibliotecaCargos.Columns["Nivel"].Visible = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Alcance"))
            {
                dgvBibliotecaCargos.Columns["Alcance"].HeaderText = "Alcance";
                dgvBibliotecaCargos.Columns["Alcance"].Width = 130;
                dgvBibliotecaCargos.Columns["Alcance"].ReadOnly = true;
                dgvBibliotecaCargos.Columns["Alcance"].Visible = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("TipoCargo"))
            {
                dgvBibliotecaCargos.Columns["TipoCargo"].HeaderText = "Tipo";
                dgvBibliotecaCargos.Columns["TipoCargo"].Width = 120;
                dgvBibliotecaCargos.Columns["TipoCargo"].ReadOnly = !vistaEditable;
                dgvBibliotecaCargos.Columns["TipoCargo"].Visible = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("CLPMin"))
            {
                dgvBibliotecaCargos.Columns["CLPMin"].HeaderText = "CLP mínimo";
                dgvBibliotecaCargos.Columns["CLPTipico"].HeaderText = "CLP típico";
                dgvBibliotecaCargos.Columns["CLPMax"].HeaderText = "CLP máximo";

                dgvBibliotecaCargos.Columns["CLPMin"].ReadOnly = !vistaEditable;
                dgvBibliotecaCargos.Columns["CLPTipico"].ReadOnly = !vistaEditable;
                dgvBibliotecaCargos.Columns["CLPMax"].ReadOnly = !vistaEditable;

                // Si estoy revisando todos los cargos y la moneda visual no es CLP,
                // escondemos CLP para no ensuciar la lectura.
                bool mostrarCLP = !vistaTodos || monedaEsCLP;

                dgvBibliotecaCargos.Columns["CLPMin"].Visible = mostrarCLP;
                dgvBibliotecaCargos.Columns["CLPTipico"].Visible = mostrarCLP;
                dgvBibliotecaCargos.Columns["CLPMax"].Visible = mostrarCLP;
            }

            if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
            {
                bool mostrarVisual = vistaTodos && !monedaEsCLP;

                dgvBibliotecaCargos.Columns["VisualMin"].Visible = mostrarVisual;
                dgvBibliotecaCargos.Columns["VisualTipico"].Visible = mostrarVisual;
                dgvBibliotecaCargos.Columns["VisualMax"].Visible = mostrarVisual;

                dgvBibliotecaCargos.Columns["VisualMin"].HeaderText = monedaVisual + " mínimo";
                dgvBibliotecaCargos.Columns["VisualTipico"].HeaderText = monedaVisual + " típico";
                dgvBibliotecaCargos.Columns["VisualMax"].HeaderText = monedaVisual + " máximo";

                dgvBibliotecaCargos.Columns["VisualMin"].ReadOnly = true;
                dgvBibliotecaCargos.Columns["VisualTipico"].ReadOnly = true;
                dgvBibliotecaCargos.Columns["VisualMax"].ReadOnly = true;
            }

            foreach (DataGridViewColumn columna in dgvBibliotecaCargos.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (btnAgregarBibliotecaCargo != null)
            {
                btnAgregarBibliotecaCargo.Text = "Agregar cargo";
            }

            if (btnEliminarBibliotecaCargo != null)
            {
                btnEliminarBibliotecaCargo.Text = "Eliminar cargo";
            }

            if (btnAplicarBibliotecaCargos != null)
            {
                btnAplicarBibliotecaCargos.Text = "Aplicar cambios";
            }
        }

        private bool CargoEsDuplicadoDeGeneral(CategoriaTrabajador cargo)
        {
            if (cargo == null || bibliotecaCargosGenerales == null)
            {
                return false;
            }

            string nombreCargo = NormalizarNombreCargoParaComparar(cargo.Nombre);

            if (string.IsNullOrWhiteSpace(nombreCargo))
            {
                return false;
            }

            return bibliotecaCargosGenerales.Any(general =>
                general != null &&
                NormalizarNombreCargoParaComparar(general.Nombre) == nombreCargo
            );
        }

        private string NormalizarNombreCargoParaComparar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "";
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
                .Replace("/", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("  ", " ");
        }

        private void AgregarFilaCargoBiblioteca(
            CategoriaTrabajador cargo,
            string alcance,
            bool soloLectura
        )
        {
            if (cargo == null)
            {
                return;
            }

            int rowIndex = dgvBibliotecaCargos.Rows.Add();
            DataGridViewRow row = dgvBibliotecaCargos.Rows[rowIndex];

            row.Cells["Id"].Value = cargo.Id;
            row.Cells["Nombre"].Value = cargo.Nombre;
            row.Cells["Nivel"].Value = cargo.Nivel;
            row.Cells["Alcance"].Value = alcance;
            row.Cells["TipoCargo"].Value = ClasificacionCargosService.ObtenerTipoCargo(cargo);

            row.Cells["CLPMin"].Value = FormatearMiles(cargo.SueldoMensualCLPMin);
            row.Cells["CLPTipico"].Value = FormatearMiles(cargo.SueldoMensualCLPTipico);
            row.Cells["CLPMax"].Value = FormatearMiles(cargo.SueldoMensualCLPMax);

            row.Cells["VisualMin"].Value = FormatearValorCargoVisual(cargo.SueldoMensualCLPMin);
            row.Cells["VisualTipico"].Value = FormatearValorCargoVisual(cargo.SueldoMensualCLPTipico);
            row.Cells["VisualMax"].Value = FormatearValorCargoVisual(cargo.SueldoMensualCLPMax);

            row.Tag = cargo;

            AplicarEstiloFilaCargoPorAlcance(row, cargo, alcance, soloLectura);

            row.ReadOnly = false;

            row.Cells["Id"].ReadOnly = true;
            row.Cells["Alcance"].ReadOnly = true;
            row.Cells["TipoCargo"].ReadOnly = soloLectura;
            row.Cells["VisualMin"].ReadOnly = true;
            row.Cells["VisualTipico"].ReadOnly = true;
            row.Cells["VisualMax"].ReadOnly = true;
        }

        private void AplicarEstiloFilaCargoPorAlcance(
            DataGridViewRow row,
            CategoriaTrabajador cargo,
            string alcance,
            bool soloLectura
        )
        {
            if (row == null || cargo == null)
            {
                return;
            }

            Color fondo = ObtenerColorFondoCargoPorAlcance(alcance, soloLectura);
            Color seleccion = ObtenerColorSeleccionCargoPorAlcance(alcance);

            Color texto = soloLectura
                ? Color.FromArgb(80, 80, 80)
                : Color.Black;

            row.DefaultCellStyle.BackColor = fondo;
            row.DefaultCellStyle.ForeColor = texto;
            row.DefaultCellStyle.SelectionBackColor = seleccion;
            row.DefaultCellStyle.SelectionForeColor = Color.Black;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells["Nombre"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells["Alcance"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells["Alcance"].Style.ForeColor = ObtenerColorTextoAlcanceCargo(alcance);

            if (NormalizarNombreEtapa(alcance).Contains("general"))
            {
                row.Cells["Nombre"].ToolTipText =
                    "Cargo general: disponible para todas las etapas.";
            }
            else
            {
                row.Cells["Nombre"].ToolTipText =
                    "Cargo específico de " + alcance + ".";
            }

            row.Cells["Id"].ReadOnly = true;
            row.Cells["Alcance"].ReadOnly = true;

            if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
            {
                row.Cells["VisualMin"].ReadOnly = true;
                row.Cells["VisualTipico"].ReadOnly = true;
                row.Cells["VisualMax"].ReadOnly = true;
            }
        }

        private Color ObtenerColorFondoCargoPorAlcance(string alcance, bool soloLectura)
        {
            string normalizado = NormalizarNombreEtapa(alcance);

            if (normalizado.Contains("general"))
            {
                return Color.White;
            }

            Color baseFila = ObtenerColorFilaEtapa(alcance);

            if (soloLectura)
            {
                return MezclarConBlanco(baseFila, 0.35);
            }

            return baseFila;
        }

        private Color ObtenerColorSeleccionCargoPorAlcance(string alcance)
        {
            string normalizado = NormalizarNombreEtapa(alcance);

            if (normalizado.Contains("general"))
            {
                return Color.FromArgb(230, 230, 230);
            }

            return ObtenerColorFilaEtapa(alcance);
        }

        private Color ObtenerColorTextoAlcanceCargo(string alcance)
        {
            string normalizado = NormalizarNombreEtapa(alcance);

            if (normalizado.Contains("general"))
            {
                return Color.FromArgb(60, 60, 60);
            }

            return ObtenerColorBordeEtapa(alcance);
        }

        private void ConfigurarEdicionVistaBibliotecaCargos(bool editable)
        {
            bool modoChecklist = EstaEnModoChecklistCargos();

            if (dgvBibliotecaCargos != null)
            {
                dgvBibliotecaCargos.ReadOnly = !editable;

                if (dgvBibliotecaCargos.Columns.Contains("Id"))
                {
                    // En modo normal, Id es ID y debe ser solo lectura.
                    // En modo checklist, Id se reutiliza como columna "Usar",
                    // por lo tanto debe poder clickearse.
                    dgvBibliotecaCargos.Columns["Id"].ReadOnly = !modoChecklist;
                }

                if (dgvBibliotecaCargos.Columns.Contains("Nombre"))
                {
                    dgvBibliotecaCargos.Columns["Nombre"].ReadOnly = modoChecklist;
                }

                if (dgvBibliotecaCargos.Columns.Contains("Nivel"))
                {
                    dgvBibliotecaCargos.Columns["Nivel"].ReadOnly = false;
                }

                if (dgvBibliotecaCargos.Columns.Contains("Alcance"))
                {
                    dgvBibliotecaCargos.Columns["Alcance"].ReadOnly = true;
                }

                if (dgvBibliotecaCargos.Columns.Contains("TipoCargo"))
                {
                    dgvBibliotecaCargos.Columns["TipoCargo"].ReadOnly = !editable || modoChecklist;
                }

                bool bloquearValores = !editable || modoChecklist;

                if (dgvBibliotecaCargos.Columns.Contains("CLPMin"))
                {
                    dgvBibliotecaCargos.Columns["CLPMin"].ReadOnly = bloquearValores;
                }

                if (dgvBibliotecaCargos.Columns.Contains("CLPTipico"))
                {
                    dgvBibliotecaCargos.Columns["CLPTipico"].ReadOnly = bloquearValores;
                }

                if (dgvBibliotecaCargos.Columns.Contains("CLPMax"))
                {
                    dgvBibliotecaCargos.Columns["CLPMax"].ReadOnly = bloquearValores;
                }

                if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
                {
                    dgvBibliotecaCargos.Columns["VisualMin"].ReadOnly = true;
                    dgvBibliotecaCargos.Columns["VisualTipico"].ReadOnly = true;
                    dgvBibliotecaCargos.Columns["VisualMax"].ReadOnly = true;
                }
            }

            if (btnAgregarBibliotecaCargo != null)
            {
                btnAgregarBibliotecaCargo.Enabled = editable && !modoChecklist;
            }

            if (btnEliminarBibliotecaCargo != null)
            {
                btnEliminarBibliotecaCargo.Enabled = editable && !modoChecklist;
            }

            if (btnAplicarBibliotecaCargos != null)
            {
                btnAplicarBibliotecaCargos.Enabled = editable;

                btnAplicarBibliotecaCargos.Text = modoChecklist
                    ? "Aplicar a Mano"
                    : "Aplicar cambios";
            }
        }

        private string FormatearValorCargoVisual(double valorCLP)
        {
            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion.Trim().ToUpperInvariant();

            if (monedaVisual == "CLP")
            {
                return "CLP " + FormatearNumeroCargoVisual(valorCLP, 0);
            }

            double valorConvertido = ConvertirCLPAMonedaCargoVisual(valorCLP, monedaVisual);

            if (Math.Abs(valorConvertido) > 0.0 && Math.Abs(valorConvertido) < 0.01)
            {
                return valorConvertido.ToString("0.##E+0", CultureInfo.InvariantCulture) + " " + monedaVisual;
            }

            return monedaVisual + " " + FormatearNumeroCargoVisual(valorConvertido, 2);
        }

        private double ConvertirCLPAMonedaCargoVisual(double valorCLP, string monedaDestino)
        {
            if (cotizacion == null || cotizacion.TiposCambio == null)
            {
                return valorCLP;
            }

            monedaDestino = string.IsNullOrWhiteSpace(monedaDestino)
                ? "CLP"
                : monedaDestino.Trim().ToUpperInvariant();

            if (monedaDestino == "CLP")
            {
                return valorCLP;
            }

            TipoCambio tipo = cotizacion.TiposCambio
                .FirstOrDefault(t => t.Codigo == monedaDestino);

            if (tipo == null || tipo.ValorEnCLP <= 0.0)
            {
                return valorCLP;
            }

            return valorCLP / tipo.ValorEnCLP;
        }

        private string FormatearNumeroCargoVisual(double valor, int decimales)
        {
            CultureInfo cultura = new CultureInfo("es-CL");

            if (decimales <= 0)
            {
                return valor.ToString("#,##0", cultura);
            }

            return valor.ToString("#,##0." + new string('0', decimales), cultura);
        }

        private void CmbEtapaBibliotecaCargos_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTablaBibliotecaCargos();
        }

        private void BtnAgregarBibliotecaCargo_Click(object sender, EventArgs e)
        {
            SelectorBibliotecaCargos selector =
                cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

            if (selector == null)
            {
                MessageBox.Show("Seleccione una biblioteca.");
                return;
            }

            if (selector.Nombre == NombreOpcionTodosLosCargos)
            {
                MessageBox.Show(
                    "Para agregar un cargo nuevo, selecciona primero una biblioteca específica: General, Desarrollo, Preproducción, Producción o Postproducción.\n\nLa vista 'Todos los cargos' permite editar valores existentes, pero no sabe en qué bloque crear un cargo nuevo.",
                    "Agregar cargo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            CategoriaTrabajador nuevoCargo;

            if (selector.EsGeneral)
            {
                nuevoCargo = new CategoriaTrabajador
                {
                    Id = ObtenerNuevoIdCargoGeneral(),
                    Nombre = "Nuevo cargo general",
                    Nivel = "general",
                    Bloque = "General",
                    TipoCargo = ClasificacionCargosService.TipoGestion,

                    SueldoMensualCLPMin = 0,
                    SueldoMensualCLPTipico = 0,
                    SueldoMensualCLPMax = 0,

                    SueldoMensualUSDMin = 0,
                    SueldoMensualUSDTipico = 0,
                    SueldoMensualUSDMax = 0
                };

                bibliotecaCargosGenerales.Add(nuevoCargo);
                SincronizarCargosGeneralesEnTodasLasEtapas();
            }
            else
            {
                EtapaProyecto etapa = selector.Etapa;

                if (etapa == null)
                {
                    MessageBox.Show("Seleccione una etapa.");
                    return;
                }

                nuevoCargo = new CategoriaTrabajador
                {
                    Id = ObtenerNuevoIdCargo(etapa),
                    Nombre = "Nuevo cargo",
                    Nivel = "personalizado",
                    Bloque = ObtenerBloqueCargoDesdeNombreEtapa(etapa.Nombre),
                    TipoCargo = ClasificacionCargosService.TipoProductivo,

                    SueldoMensualCLPMin = 0,
                    SueldoMensualCLPTipico = 0,
                    SueldoMensualCLPMax = 0,

                    SueldoMensualUSDMin = 0,
                    SueldoMensualUSDTipico = 0,
                    SueldoMensualUSDMax = 0
                };

                etapa.Biblioteca.Add(nuevoCargo);
            }

            GuardarBibliotecaCargosActualEnJson();

            CargarTablaBibliotecaCargos();
            CargarCombos();
            RefrescarTablaSubEtapasSiExiste();
        }

        private void BtnEliminarBibliotecaCargo_Click(object sender, EventArgs e)
        {
            SelectorBibliotecaCargos selector =
                cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

            if (selector == null || dgvBibliotecaCargos.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una biblioteca y un cargo.");
                return;
            }

            if (selector.Nombre == NombreOpcionTodosLosCargos)
            {
                MessageBox.Show(
                    "Para eliminar un cargo, selecciona primero su biblioteca específica en el combo.\n\nEjemplo: General, Desarrollo, Preproducción, Producción o Postproducción.",
                    "Eliminar cargo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            CategoriaTrabajador cargo = dgvBibliotecaCargos.CurrentRow.Tag as CategoriaTrabajador;

            if (cargo == null)
            {
                return;
            }

            if (!selector.EsGeneral && EsCargoGeneral(cargo))
            {
                MessageBox.Show(
                    "Este cargo es general y se usa en todas las etapas.\n\nPara eliminarlo, entra a 'General / todas las etapas'.",
                    "Cargo general",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (selector.EsGeneral)
            {
                bool estaUsado = cotizacion.Etapas.Any(etapa =>
                    etapa.Plan.Any(plan =>
                        plan.Categoria != null &&
                        plan.Categoria.Id == cargo.Id
                    )
                );

                if (estaUsado)
                {
                    MessageBox.Show(
                        "Este cargo general está usado en mano de obra. Primero elimínelo desde Mano de obra en todas las etapas donde aparezca.",
                        "Cargo general en uso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    return;
                }

                DialogResult respuestaGeneral = MessageBox.Show(
                    "¿Eliminar este cargo general de todas las etapas?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (respuestaGeneral != DialogResult.Yes)
                {
                    return;
                }

                bibliotecaCargosGenerales.Remove(cargo);
                QuitarCargoGeneralDeTodasLasEtapas(cargo.Id);
            }
            else
            {
                EtapaProyecto etapa = selector.Etapa;

                if (etapa == null)
                {
                    return;
                }

                bool estaUsado = etapa.Plan.Any(plan =>
                    plan.Categoria != null &&
                    plan.Categoria.Id == cargo.Id
                );

                if (estaUsado)
                {
                    MessageBox.Show(
                        "Este cargo ya está usado en la mano de obra de esta etapa. Primero elimínelo desde Mano de obra.",
                        "Cargo en uso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    return;
                }

                DialogResult respuesta = MessageBox.Show(
                    "¿Eliminar este cargo de la biblioteca?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (respuesta != DialogResult.Yes)
                {
                    return;
                }

                etapa.Biblioteca.Remove(cargo);
            }

            CargarTablaBibliotecaCargos();
            CargarCombos();
            RefrescarTablaSubEtapasSiExiste();
        }

        private void BtnAplicarBibliotecaCargos_Click(object sender, EventArgs e)
        {
            if (EstaEnModoChecklistCargos())
            {
                AplicarCambiosChecklistCargosProyecto();
                SincronizarChecklistCargosConManoDeObra();

                MessageBox.Show(
                    "Checklist sincronizado con Mano de obra.\n\nLos cargos seleccionados quedaron listos para agendar personas por mes.",
                    "Checklist del proyecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            AplicarCambiosTablaBibliotecaCargos();
            GuardarBibliotecaCargosActualEnJson();

            CargarBibliotecaCargosDesdeJsonEnMemoria();
            CargarTablaBibliotecaCargos();
            CargarCombos();
            RefrescarCalculosYVista();

            MessageBox.Show(
                "Biblioteca de cargos guardada correctamente en cargos.json.",
                "Cargos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void DgvBibliotecaCargos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || cargandoChecklistCargos || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (EstaEnModoChecklistCargos())
            {
                AplicarCambiosChecklistCargosProyecto();

                // Sincroniza inmediatamente con Mano de obra.
                SincronizarChecklistCargosConManoDeObra();

                string columna = dgvBibliotecaCargos.Columns[e.ColumnIndex].Name;

                // Si cambia nivel o usar, refresca precios/colores del checklist.
                if (columna == "Nivel" || columna == "Id")
                {
                    CargarTablaChecklistCargosProyecto();
                }

                return;
            }

            AplicarCambiosTablaBibliotecaCargos();
            CargarCombos();
        }

        private int ObtenerOrdenEtapaParaCargos(string nombreEtapa)
        {
            string nombre = NormalizarNombreEtapa(nombreEtapa);

            if (nombre.Contains("general")) return -1;

            return ObtenerOrdenEtapaGeneral(nombreEtapa);
        }

        private int ObtenerNuevoIdCargo(EtapaProyecto etapa)
        {
            int baseId = 900;

            if (etapa.Nombre.Contains("Desarrollo"))
            {
                baseId = 100;
            }
            else if (etapa.Nombre.Contains("Preproduccion") || etapa.Nombre.Contains("Preproducción"))
            {
                baseId = 200;
            }
            else if (etapa.Nombre.Contains("Produccion") || etapa.Nombre.Contains("Producción"))
            {
                baseId = 300;
            }
            else if (etapa.Nombre.Contains("Postproduccion") || etapa.Nombre.Contains("Postproducción"))
            {
                baseId = 400;
            }

            int maxId = etapa.Biblioteca
                .Select(c => c.Id)
                .DefaultIfEmpty(baseId)
                .Max();

            return maxId + 1;
        }

        private void AplicarCambiosTablaBibliotecaCargos()
        {
            if (cargandoTabla)
            {
                return;
            }

            SelectorBibliotecaCargos selector =
                cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

            if (selector == null)
            {
                return;
            }

            if (selector.Nombre == NombreOpcionTodosLosCargos)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvBibliotecaCargos.Rows)
            {
                CategoriaTrabajador cargo = row.Tag as CategoriaTrabajador;

                if (cargo == null)
                {
                    continue;
                }

                if (!selector.EsGeneral && EsCargoGeneral(cargo))
                {
                    continue;
                }

                cargo.Nombre = row.Cells["Nombre"].Value?.ToString()?.Trim() ?? "";
                cargo.Nivel = row.Cells["Nivel"].Value?.ToString()?.Trim() ?? "";
                string tipoCargo = row.Cells["TipoCargo"].Value?.ToString()?.Trim() ?? "";
                cargo.TipoCargo = ClasificacionCargosService.NormalizarTipoCargo(tipoCargo);

                cargo.SueldoMensualCLPMin = ConvertirDouble(row.Cells["CLPMin"].Value);
                cargo.SueldoMensualCLPTipico = ConvertirDouble(row.Cells["CLPTipico"].Value);
                cargo.SueldoMensualCLPMax = ConvertirDouble(row.Cells["CLPMax"].Value);

                if (string.IsNullOrWhiteSpace(cargo.Nombre))
                {
                    cargo.Nombre = "Cargo sin nombre";
                }

                if (string.IsNullOrWhiteSpace(cargo.Nivel))
                {
                    cargo.Nivel = EsCargoGeneral(cargo)
                        ? "general"
                        : "personalizado";
                }

                if (string.IsNullOrWhiteSpace(cargo.TipoCargo))
                {
                    cargo.TipoCargo = ClasificacionCargosService.InferirTipoCargo(cargo.NombreCompleto);
                }

                if (EsCargoGeneral(cargo))
                {
                    CategoriaTrabajador cargoGeneral = ObtenerCargoGeneralPorId(cargo.Id);

                    if (cargoGeneral != null && !object.ReferenceEquals(cargoGeneral, cargo))
                    {
                        CopiarValoresCargo(cargoGeneral, cargo);
                    }
                }
            }

            SincronizarCargosGeneralesEnTodasLasEtapas();
            GuardarBibliotecaCargosActualEnJson();
            RefrescarTablaSubEtapasSiExiste();
        }

        private bool EstaEnModoChecklistCargos()
        {
            SelectorBibliotecaCargos selector =
                cmbEtapaBibliotecaCargos.SelectedItem as SelectorBibliotecaCargos;

            return selector != null &&
                   selector.Nombre == NombreOpcionChecklistCargos;
        }

        private void CargarTablaChecklistCargosProyecto()
        {
            if (dgvBibliotecaCargos == null || dgvBibliotecaCargos.Columns.Count == 0)
            {
                return;
            }

            cargandoTabla = true;
            cargandoChecklistCargos = true;

            try
            {
                InicializarCargosGenerales();
                SincronizarCargosGeneralesEnTodasLasEtapas();

                dgvBibliotecaCargos.Rows.Clear();

                PrepararColumnasParaChecklistCargos();

                CargarChecklistCargosRecomendadosPorSubEtapas();

                ConfigurarEdicionVistaBibliotecaCargos(true);

                if (btnAgregarBibliotecaCargo != null)
                {
                    btnAgregarBibliotecaCargo.Enabled = false;
                }

                if (btnEliminarBibliotecaCargo != null)
                {
                    btnEliminarBibliotecaCargo.Enabled = false;
                }

                if (btnAplicarBibliotecaCargos != null)
                {
                    btnAplicarBibliotecaCargos.Enabled = true;
                    btnAplicarBibliotecaCargos.Text = "Aplicar a Mano de obra";
                }
            }
            finally
            {
                cargandoChecklistCargos = false;
                cargandoTabla = false;
            }
        }

        private void CargarChecklistCargosRecomendadosPorSubEtapas()
        {
            if (bibliotecaSubEtapas == null)
            {
                return;
            }

            List<RecomendacionCargoSubEtapa> recomendaciones =
                BibliotecaRecomendacionesCargoSubEtapaService
                    .ObtenerRecomendacionesParaSubEtapasActivas(bibliotecaSubEtapas);

            List<string> clavesRecomendadas = new List<string>();

            if (recomendaciones != null && recomendaciones.Count > 0)
            {
                AgregarSeparadorChecklistCargos("Recomendado:");

                var grupos = recomendaciones
                    .GroupBy(r =>
                        NormalizarNombreEtapa(r.EsGeneral ? "General" : r.Etapa) +
                        "|" +
                        NormalizarNombreCargoParaComparar(r.Cargo))
                    .OrderBy(g => ObtenerOrdenEtapaParaCargos(g.First().EsGeneral ? "General" : g.First().Etapa))
                    .ThenBy(g => ObtenerOrdenCargoBaseChecklist(g.First().Cargo))
                    .ThenBy(g => g.First().Cargo)
                    .ToList();

                foreach (var grupo in grupos)
                {
                    RecomendacionCargoSubEtapa recomendacionBase = grupo.First();

                    string etapaChecklist = recomendacionBase.EsGeneral
                        ? "General"
                        : recomendacionBase.Etapa;

                    string cargoBuscado = recomendacionBase.Cargo;

                    List<CategoriaTrabajador> opciones =
                        ObtenerOpcionesCargoParaRecomendacion(etapaChecklist, cargoBuscado);

                    if (opciones == null || opciones.Count == 0)
                    {
                        continue;
                    }

                    string recomendadoPor = string.Join(
                        ", ",
                        grupo
                            .Select(r => r.SubEtapa)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct()
                    );

                    string cargoBase = opciones.First().Nombre;

                    clavesRecomendadas.Add(
                        ObtenerClaveChecklistCargo(etapaChecklist, cargoBase)
                    );

                    AgregarFilaChecklistCargo(
                        etapaChecklist,
                        cargoBase,
                        opciones,
                        recomendadoPor
                    );
                }
            }

            AgregarSeparadorChecklistCargos("Otros cargos para las etapas:");

            AgregarOtrosCargosDisponiblesParaEtapasActivas(clavesRecomendadas);
        }

        private void AgregarOtrosCargosDisponiblesParaEtapasActivas(List<string> clavesRecomendadas)
        {
            if (clavesRecomendadas == null)
            {
                clavesRecomendadas = new List<string>();
            }

            HashSet<string> clavesYaMostradas = new HashSet<string>(
                clavesRecomendadas
            );

            /*
             * Generales: también pueden ser útiles aunque no hayan salido como recomendados.
             */
            if (bibliotecaCargosGenerales != null)
            {
                var gruposGenerales = bibliotecaCargosGenerales
                    .Where(c => c != null)
                    .GroupBy(c => NormalizarNombreCargoParaComparar(c.Nombre))
                    .OrderBy(g => ObtenerOrdenCargoBaseChecklist(g.First().Nombre))
                    .ThenBy(g => g.First().Nombre)
                    .ToList();

                foreach (var grupo in gruposGenerales)
                {
                    List<CategoriaTrabajador> opciones = grupo
                        .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                        .ToList();

                    if (opciones.Count == 0)
                    {
                        continue;
                    }

                    string cargoBase = opciones.First().Nombre;
                    string clave = ObtenerClaveChecklistCargo("General", cargoBase);

                    if (clavesYaMostradas.Contains(clave))
                    {
                        continue;
                    }

                    clavesYaMostradas.Add(clave);

                    AgregarFilaChecklistCargo(
                        "General",
                        cargoBase,
                        opciones,
                        "Cargo general disponible"
                    );
                }
            }

            List<EtapaProyecto> etapasActivas = ObtenerEtapasActivasParaChecklistCargos();

            foreach (EtapaProyecto etapa in etapasActivas.OrderBy(e => ObtenerOrdenEtapaParaCargos(e.Nombre)))
            {
                if (etapa == null || etapa.Biblioteca == null)
                {
                    continue;
                }

                var gruposEtapa = etapa.Biblioteca
                    .Where(c => c != null)
                    .Where(c => !EsCargoGeneral(c))
                    .Where(c => !CargoEsDuplicadoDeGeneral(c))
                    .GroupBy(c => NormalizarNombreCargoParaComparar(c.Nombre))
                    .OrderBy(g => ObtenerOrdenCargoBaseChecklist(g.First().Nombre))
                    .ThenBy(g => g.First().Nombre)
                    .ToList();

                foreach (var grupo in gruposEtapa)
                {
                    List<CategoriaTrabajador> opciones = grupo
                        .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                        .ToList();

                    if (opciones.Count == 0)
                    {
                        continue;
                    }

                    string cargoBase = opciones.First().Nombre;
                    string clave = ObtenerClaveChecklistCargo(etapa.Nombre, cargoBase);

                    if (clavesYaMostradas.Contains(clave))
                    {
                        continue;
                    }

                    clavesYaMostradas.Add(clave);

                    AgregarFilaChecklistCargo(
                        etapa.Nombre,
                        cargoBase,
                        opciones,
                        "Disponible en " + etapa.Nombre
                    );
                }
            }
        }

        private List<EtapaProyecto> ObtenerEtapasActivasParaChecklistCargos()
        {
            List<EtapaProyecto> resultado = new List<EtapaProyecto>();

            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return resultado;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                if (etapa == null)
                {
                    continue;
                }

                string nombreEtapa = NormalizarNombreEtapa(etapa.Nombre);

                bool etapaSeleccionada = etapa.Seleccionada;

                bool etapaTieneDuracion =
                    etapa.DuracionMeses > 0.0 ||
                    etapa.FinMes > etapa.InicioMes;

                bool tieneSubEtapasActivasEnBiblioteca = false;

                if (bibliotecaSubEtapas != null)
                {
                    tieneSubEtapasActivasEnBiblioteca = bibliotecaSubEtapas.Any(sub =>
                        sub != null &&
                        sub.Activa &&
                        NormalizarNombreEtapa(sub.EtapaPadre) == nombreEtapa
                    );
                }

                bool tieneSubEtapasActivasDesdeTabla = false;

                try
                {
                    tieneSubEtapasActivasDesdeTabla =
                        TieneSubEtapasActivasDesdeTablaPrincipal(etapa);
                }
                catch
                {
                    tieneSubEtapasActivasDesdeTabla = false;
                }

                /*
                 * Para cargos, una etapa debe considerarse relevante si:
                 * 1. Está seleccionada en Etapas.
                 * 2. Tiene duración real.
                 * 3. Tiene subetapas activas.
                 *
                 * No exigimos que tenga subetapas visibles/expandidas.
                 */
                if (etapaSeleccionada ||
                    etapaTieneDuracion ||
                    tieneSubEtapasActivasEnBiblioteca ||
                    tieneSubEtapasActivasDesdeTabla)
                {
                    resultado.Add(etapa);
                }
            }

            return resultado
                .OrderBy(e => ObtenerOrdenEtapaParaCargos(e.Nombre))
                .ToList();
        }

        private List<CategoriaTrabajador> ObtenerOpcionesCargoParaRecomendacion(
    string etapa,
    string cargoBuscado
)
        {
            List<CategoriaTrabajador> opciones = new List<CategoriaTrabajador>();

            string cargoNormalizado = NormalizarNombreCargoParaComparar(cargoBuscado);

            if (string.IsNullOrWhiteSpace(cargoNormalizado))
            {
                return opciones;
            }

            bool esGeneral = NormalizarNombreEtapa(etapa).Contains("general");

            if (esGeneral)
            {
                if (bibliotecaCargosGenerales == null)
                {
                    return opciones;
                }

                opciones = bibliotecaCargosGenerales
                    .Where(c => c != null)
                    .Where(c =>
                        NormalizarNombreCargoParaComparar(c.Nombre).Contains(cargoNormalizado) ||
                        cargoNormalizado.Contains(NormalizarNombreCargoParaComparar(c.Nombre)))
                    .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                    .ToList();

                return opciones;
            }

            EtapaProyecto etapaProyecto = ObtenerEtapaBibliotecaPorNombreChecklist(etapa);

            if (etapaProyecto == null || etapaProyecto.Biblioteca == null)
            {
                return opciones;
            }

            opciones = etapaProyecto.Biblioteca
                .Where(c => c != null)
                .Where(c => !EsCargoGeneral(c))
                .Where(c => !CargoEsDuplicadoDeGeneral(c))
                .Where(c =>
                    NormalizarNombreCargoParaComparar(c.Nombre).Contains(cargoNormalizado) ||
                    cargoNormalizado.Contains(NormalizarNombreCargoParaComparar(c.Nombre)))
                .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                .ToList();

            return opciones;
        }

        private void PrepararColumnasParaChecklistCargos()
        {
            if (dgvBibliotecaCargos == null)
            {
                return;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Id"))
            {
                dgvBibliotecaCargos.Columns["Id"].HeaderText = "Usar";
                dgvBibliotecaCargos.Columns["Id"].Width = 55;
                dgvBibliotecaCargos.Columns["Id"].ReadOnly = false;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Nombre"))
            {
                dgvBibliotecaCargos.Columns["Nombre"].HeaderText = "Cargo";
                dgvBibliotecaCargos.Columns["Nombre"].Width = 240;
                dgvBibliotecaCargos.Columns["Nombre"].ReadOnly = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Nivel"))
            {
                dgvBibliotecaCargos.Columns["Nivel"].HeaderText = "Nivel sugerido";
                dgvBibliotecaCargos.Columns["Nivel"].Width = 140;
                dgvBibliotecaCargos.Columns["Nivel"].ReadOnly = false;
            }

            if (dgvBibliotecaCargos.Columns.Contains("Alcance"))
            {
                dgvBibliotecaCargos.Columns["Alcance"].HeaderText = "Recomendado por";
                dgvBibliotecaCargos.Columns["Alcance"].Width = 230;
                dgvBibliotecaCargos.Columns["Alcance"].ReadOnly = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("CLPMin"))
            {
                dgvBibliotecaCargos.Columns["CLPMin"].ReadOnly = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("CLPTipico"))
            {
                dgvBibliotecaCargos.Columns["CLPTipico"].ReadOnly = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("CLPMax"))
            {
                dgvBibliotecaCargos.Columns["CLPMax"].ReadOnly = true;
            }

            if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
            {
                dgvBibliotecaCargos.Columns["VisualMin"].Visible = false;
                dgvBibliotecaCargos.Columns["VisualTipico"].Visible = false;
                dgvBibliotecaCargos.Columns["VisualMax"].Visible = false;
            }

            foreach (DataGridViewColumn columna in dgvBibliotecaCargos.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void AgregarChecklistCargosGenerales()
        {
            if (bibliotecaCargosGenerales == null)
            {
                return;
            }

            var grupos = bibliotecaCargosGenerales
                .Where(c => c != null)
                .GroupBy(c => NormalizarNombreCargoParaComparar(c.Nombre))
                .OrderBy(g => ObtenerOrdenCargoBaseChecklist(g.First().Nombre))
                .ThenBy(g => g.First().Nombre)
                .ToList();

            foreach (var grupo in grupos)
            {
                List<CategoriaTrabajador> opciones = grupo
                    .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                    .ToList();

                if (opciones.Count == 0)
                {
                    continue;
                }

                AgregarFilaChecklistCargo("General", opciones.First().Nombre, opciones);
            }
        }

        private void AgregarChecklistCargosDeEtapa(EtapaProyecto etapa)
        {
            if (etapa == null || etapa.Biblioteca == null)
            {
                return;
            }

            var grupos = etapa.Biblioteca
                .Where(c => c != null)
                .Where(c => !EsCargoGeneral(c))
                .Where(c => !CargoEsDuplicadoDeGeneral(c))
                .GroupBy(c => NormalizarNombreCargoParaComparar(c.Nombre))
                .OrderBy(g => ObtenerOrdenCargoBaseChecklist(g.First().Nombre))
                .ThenBy(g => g.First().Nombre)
                .ToList();

            foreach (var grupo in grupos)
            {
                List<CategoriaTrabajador> opciones = grupo
                    .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                    .ToList();

                if (opciones.Count == 0)
                {
                    continue;
                }

                AgregarFilaChecklistCargo(etapa.Nombre, opciones.First().Nombre, opciones);
            }
        }

        private void AgregarSeparadorChecklistCargos(string texto)
        {
            if (dgvBibliotecaCargos == null)
            {
                return;
            }

            int rowIndex = dgvBibliotecaCargos.Rows.Add();
            DataGridViewRow row = dgvBibliotecaCargos.Rows[rowIndex];

            row.Cells["Id"].Value = "";
            row.Cells["Nombre"].Value = texto;
            row.Cells["Nivel"].Value = "";
            row.Cells["Alcance"].Value = "";
            row.Cells["CLPMin"].Value = "";
            row.Cells["CLPTipico"].Value = "";
            row.Cells["CLPMax"].Value = "";

            if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
            {
                row.Cells["VisualMin"].Value = "";
                row.Cells["VisualTipico"].Value = "";
                row.Cells["VisualMax"].Value = "";
            }

            row.Tag = null;
            row.ReadOnly = true;

            row.DefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 235, 235);
            row.DefaultCellStyle.SelectionForeColor = Color.Black;
            row.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            row.Cells["Nombre"].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;

            row.Cells["Id"].ReadOnly = true;
            row.Cells["Nombre"].ReadOnly = true;
            row.Cells["Nivel"].ReadOnly = true;
            row.Cells["Alcance"].ReadOnly = true;
            row.Cells["CLPMin"].ReadOnly = true;
            row.Cells["CLPTipico"].ReadOnly = true;
            row.Cells["CLPMax"].ReadOnly = true;

            if (dgvBibliotecaCargos.Columns.Contains("VisualMin"))
            {
                row.Cells["VisualMin"].ReadOnly = true;
                row.Cells["VisualTipico"].ReadOnly = true;
                row.Cells["VisualMax"].ReadOnly = true;
            }
        }

        private void AgregarFilaChecklistCargo(
    string etapa,
    string cargoBase,
    List<CategoriaTrabajador> opcionesNivel,
    string recomendadoPor = ""
)
        {
            if (opcionesNivel == null || opcionesNivel.Count == 0)
            {
                return;
            }

            string clave = ObtenerClaveChecklistCargo(etapa, cargoBase);

            CargoChecklistSeleccion seleccion;

            if (!checklistCargosProyecto.TryGetValue(clave, out seleccion))
            {
                seleccion = new CargoChecklistSeleccion
                {
                    Etapa = etapa,
                    CargoBase = cargoBase,
                    NivelSeleccionado = ObtenerNivelInicialChecklist(opcionesNivel),
                    Usar = false
                };

                checklistCargosProyecto[clave] = seleccion;
            }

            CategoriaTrabajador cargoNivel = ObtenerCargoPorNivelChecklist(
                opcionesNivel,
                seleccion.NivelSeleccionado
            );

            if (cargoNivel == null)
            {
                cargoNivel = opcionesNivel.First();
                seleccion.NivelSeleccionado = cargoNivel.Nivel;
            }

            int rowIndex = dgvBibliotecaCargos.Rows.Add();
            DataGridViewRow row = dgvBibliotecaCargos.Rows[rowIndex];

            DataGridViewCheckBoxCell celdaUsar = new DataGridViewCheckBoxCell();
            celdaUsar.Value = seleccion.Usar;
            row.Cells["Id"] = celdaUsar;

            row.Cells["Nombre"].Value = cargoBase;
            row.Cells["Alcance"].Value = string.IsNullOrWhiteSpace(recomendadoPor)
    ? etapa
    : recomendadoPor;

            DataGridViewComboBoxCell celdaNivel = new DataGridViewComboBoxCell();

            foreach (CategoriaTrabajador opcion in opcionesNivel)
            {
                if (!celdaNivel.Items.Contains(opcion.Nivel))
                {
                    celdaNivel.Items.Add(opcion.Nivel);
                }
            }

            celdaNivel.Value = seleccion.NivelSeleccionado;
            row.Cells["Nivel"] = celdaNivel;

            row.Cells["CLPMin"].Value = FormatearMiles(cargoNivel.SueldoMensualCLPMin);
            row.Cells["CLPTipico"].Value = FormatearMiles(cargoNivel.SueldoMensualCLPTipico);
            row.Cells["CLPMax"].Value = FormatearMiles(cargoNivel.SueldoMensualCLPMax);

            row.Tag = new CargoChecklistRow
            {
                Etapa = etapa,
                CargoBase = cargoBase,
                RecomendadoPor = recomendadoPor,
                OpcionesNivel = opcionesNivel
            };

            AplicarEstiloFilaChecklistCargo(row, etapa, seleccion.Usar);

            row.Cells["Id"].ReadOnly = false;
            row.Cells["Nombre"].ReadOnly = true;
            row.Cells["Nivel"].ReadOnly = false;
            row.Cells["Alcance"].ReadOnly = true;
            row.Cells["CLPMin"].ReadOnly = true;
            row.Cells["CLPTipico"].ReadOnly = true;
            row.Cells["CLPMax"].ReadOnly = true;
        }

        private void AplicarEstiloFilaChecklistCargo(
            DataGridViewRow row,
            string etapa,
            bool usar
        )
        {
            if (row == null)
            {
                return;
            }

            bool esGeneral = NormalizarNombreEtapa(etapa).Contains("general");

            Color fondo = esGeneral
                ? Color.White
                : ObtenerColorFilaEtapa(etapa);

            if (!usar)
            {
                fondo = MezclarConBlanco(fondo, 0.45);
            }

            row.DefaultCellStyle.BackColor = fondo;
            row.DefaultCellStyle.SelectionBackColor = esGeneral
                ? Color.FromArgb(230, 230, 230)
                : ObtenerColorFilaEtapa(etapa);

            row.DefaultCellStyle.ForeColor = usar
                ? Color.Black
                : Color.FromArgb(95, 95, 95);

            row.DefaultCellStyle.SelectionForeColor = Color.Black;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells["Nombre"].Style.Font = usar
                ? new Font("Segoe UI", 10, FontStyle.Bold)
                : new Font("Segoe UI", 10, FontStyle.Regular);

            row.Cells["Alcance"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells["Alcance"].Style.ForeColor = esGeneral
                ? Color.FromArgb(60, 60, 60)
                : ObtenerColorBordeEtapa(etapa);

            row.Cells["Id"].ToolTipText =
                "Marcar si este cargo se usará en el proyecto.";

            row.Cells["Nivel"].ToolTipText =
                "Nivel sugerido. Luego se agenda cantidad de personas por mes en Mano de obra.";
        }

        private void AplicarCambiosChecklistCargosProyecto()
        {
            if (dgvBibliotecaCargos == null)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvBibliotecaCargos.Rows)
            {
                if (row == null || row.IsNewRow)
                {
                    continue;
                }

                CargoChecklistRow info = row.Tag as CargoChecklistRow;

                if (info == null)
                {
                    continue;
                }

                string clave = ObtenerClaveChecklistCargo(info.Etapa, info.CargoBase);

                CargoChecklistSeleccion seleccion;

                if (!checklistCargosProyecto.TryGetValue(clave, out seleccion))
                {
                    seleccion = new CargoChecklistSeleccion
                    {
                        Etapa = info.Etapa,
                        CargoBase = info.CargoBase,
                        NivelSeleccionado = ObtenerNivelInicialChecklist(info.OpcionesNivel),
                        Usar = false
                    };

                    checklistCargosProyecto[clave] = seleccion;
                }

                object valorUsar = row.Cells["Id"].Value;

                if (valorUsar is bool usarBool)
                {
                    seleccion.Usar = usarBool;
                }
                else
                {
                    seleccion.Usar = false;
                }

                object valorNivel = row.Cells["Nivel"].Value;

                if (valorNivel != null)
                {
                    seleccion.NivelSeleccionado = valorNivel.ToString() ?? seleccion.NivelSeleccionado;
                }
            }
        }

        private void MaterializarChecklistCargosEnManoDeObra()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (var par in checklistCargosProyecto)
            {
                CargoChecklistSeleccion seleccion = par.Value;

                if (seleccion == null)
                {
                    continue;
                }

                CategoriaTrabajador cargoSeleccionado = ObtenerCategoriaDesdeSeleccionChecklist(seleccion);

                if (cargoSeleccionado == null)
                {
                    continue;
                }

                bool esGeneral = NormalizarNombreEtapa(seleccion.Etapa).Contains("general");

                if (esGeneral)
                {
                    foreach (EtapaProyecto etapa in cotizacion.Etapas)
                    {
                        if (etapa == null)
                        {
                            continue;
                        }

                        bool etapaDebeRecibirCargo =
                            etapa.Seleccionada ||
                            TieneSubEtapasActivasDesdeTablaPrincipal(etapa);

                        if (!etapaDebeRecibirCargo)
                        {
                            // Si la etapa no está activa, no metemos cargos generales ahí.
                            QuitarCargoChecklistDePlanEtapa(etapa, cargoSeleccionado);
                            continue;
                        }

                        if (seleccion.Usar)
                        {
                            AgregarCargoChecklistAPlanEtapa(etapa, cargoSeleccionado);
                        }
                        else
                        {
                            QuitarCargoChecklistDePlanEtapa(etapa, cargoSeleccionado);
                        }
                    }
                }
                else
                {
                    EtapaProyecto etapa = ObtenerEtapaBibliotecaPorNombreChecklist(seleccion.Etapa);

                    if (etapa == null)
                    {
                        continue;
                    }

                    if (seleccion.Usar)
                    {
                        AgregarCargoChecklistAPlanEtapa(etapa, cargoSeleccionado);
                    }
                    else
                    {
                        QuitarCargoChecklistDePlanEtapa(etapa, cargoSeleccionado);
                    }
                }
            }
        }

        private void AgregarCargoChecklistAPlanGeneralProyecto(CategoriaTrabajador categoria)
        {
            if (categoria == null)
            {
                return;
            }

            if (planGeneralProyecto == null)
            {
                planGeneralProyecto = new List<CargoPlanMensual>();
            }

            bool yaExiste = planGeneralProyecto.Any(p =>
                p != null &&
                p.Categoria != null &&
                NormalizarNombreCargoParaComparar(p.Categoria.Nombre) ==
                NormalizarNombreCargoParaComparar(categoria.Nombre) &&
                NormalizarTextoChecklist(p.Categoria.Nivel) ==
                NormalizarTextoChecklist(categoria.Nivel)
            );

            if (yaExiste)
            {
                return;
            }

            CargoPlanMensual nuevo = new CargoPlanMensual
            {
                Categoria = categoria,
                NombrePersona = "",
                PersonasPorBloque = new List<double>(),
                CostoTotal = 0.0
            };

            double duracionProyecto = ObtenerDuracionTotalProyectoParaPlanGeneral();

            PlanEtapasService.AsegurarCantidadBloques(nuevo, duracionProyecto);

            for (int i = 0; i < nuevo.PersonasPorBloque.Count; i++)
            {
                nuevo.PersonasPorBloque[i] = 0.0;
            }

            planGeneralProyecto.Add(nuevo);
        }

        private void QuitarCargoChecklistDePlanGeneralProyecto(CategoriaTrabajador categoria)
        {
            if (categoria == null || planGeneralProyecto == null)
            {
                return;
            }

            planGeneralProyecto.RemoveAll(p =>
                p != null &&
                p.Categoria != null &&
                NormalizarNombreCargoParaComparar(p.Categoria.Nombre) ==
                NormalizarNombreCargoParaComparar(categoria.Nombre) &&
                NormalizarTextoChecklist(p.Categoria.Nivel) ==
                NormalizarTextoChecklist(categoria.Nivel)
            );
        }

        private double ObtenerDuracionTotalProyectoParaPlanGeneral()
        {
            if (cotizacion == null || cotizacion.Etapas == null || cotizacion.Etapas.Count == 0)
            {
                return 1.0;
            }

            double finMaximo = cotizacion.Etapas
                .Where(e => e != null && e.Seleccionada)
                .Select(e => e.FinMes)
                .DefaultIfEmpty(1.0)
                .Max();

            if (finMaximo <= 0.0)
            {
                finMaximo = cotizacion.Etapas
                    .Where(e => e != null)
                    .Select(e => e.DuracionMeses)
                    .DefaultIfEmpty(1.0)
                    .Max();
            }

            if (finMaximo <= 0.0)
            {
                finMaximo = 1.0;
            }

            return finMaximo;
        }

        private void SincronizarChecklistCargosConManoDeObra()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            MaterializarChecklistCargosEnManoDeObra();

            CargarCombos();
            CargarTablaManoObra();
            
           
        }

        private CategoriaTrabajador ObtenerCategoriaDesdeSeleccionChecklist(
            CargoChecklistSeleccion seleccion
        )
        {
            if (seleccion == null)
            {
                return null;
            }

            bool esGeneral = NormalizarNombreEtapa(seleccion.Etapa).Contains("general");

            if (esGeneral)
            {
                return bibliotecaCargosGenerales
                    .Where(c => c != null)
                    .Where(c =>
                        NormalizarNombreCargoParaComparar(c.Nombre) ==
                        NormalizarNombreCargoParaComparar(seleccion.CargoBase)
                    )
                    .OrderBy(c =>
                        NormalizarTextoChecklist(c.Nivel) ==
                        NormalizarTextoChecklist(seleccion.NivelSeleccionado)
                            ? 0
                            : 1
                    )
                    .ThenBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                    .FirstOrDefault();
            }

            EtapaProyecto etapa = ObtenerEtapaBibliotecaPorNombreChecklist(seleccion.Etapa);

            if (etapa == null || etapa.Biblioteca == null)
            {
                return null;
            }

            return etapa.Biblioteca
                .Where(c => c != null)
                .Where(c =>
                    NormalizarNombreCargoParaComparar(c.Nombre) ==
                    NormalizarNombreCargoParaComparar(seleccion.CargoBase)
                )
                .OrderBy(c =>
                    NormalizarTextoChecklist(c.Nivel) ==
                    NormalizarTextoChecklist(seleccion.NivelSeleccionado)
                        ? 0
                        : 1
                )
                .ThenBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                .FirstOrDefault();
        }

        private void AgregarCargoChecklistAPlanEtapa(
            EtapaProyecto etapa,
            CategoriaTrabajador categoria
        )
        {
            if (etapa == null || categoria == null)
            {
                return;
            }

            if (etapa.Plan == null)
            {
                etapa.Plan = new List<CargoPlanMensual>();
            }

            bool yaExiste = etapa.Plan.Any(p =>
                p != null &&
                p.Categoria != null &&
                NormalizarNombreCargoParaComparar(p.Categoria.Nombre) ==
                NormalizarNombreCargoParaComparar(categoria.Nombre) &&
                NormalizarTextoChecklist(p.Categoria.Nivel) ==
                NormalizarTextoChecklist(categoria.Nivel)
            );

            if (yaExiste)
            {
                return;
            }

            CargoPlanMensual nuevo = new CargoPlanMensual
            {
                Categoria = categoria,
                NombrePersona = "",
                PersonasPorBloque = new List<double>(),
                CostoTotal = 0.0
            };

            double duracion = etapa.DuracionMeses;

            if (duracion <= 0.0)
            {
                duracion = 1.0;
            }

            PlanEtapasService.AsegurarCantidadBloques(nuevo, duracion);

            for (int i = 0; i < nuevo.PersonasPorBloque.Count; i++)
            {
                nuevo.PersonasPorBloque[i] = 0.0;
            }

            etapa.Plan.Add(nuevo);
        }

        private void QuitarCargoChecklistDePlanEtapa(
            EtapaProyecto etapa,
            CategoriaTrabajador categoria
        )
        {
            if (etapa == null || categoria == null || etapa.Plan == null)
            {
                return;
            }

            etapa.Plan.RemoveAll(p =>
                p != null &&
                p.Categoria != null &&
                NormalizarNombreCargoParaComparar(p.Categoria.Nombre) ==
                NormalizarNombreCargoParaComparar(categoria.Nombre) &&
                NormalizarTextoChecklist(p.Categoria.Nivel) ==
                NormalizarTextoChecklist(categoria.Nivel)
            );
        }

        private EtapaProyecto ObtenerEtapaBibliotecaPorNombreChecklist(string nombre)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return null;
            }

            string normalizado = NormalizarNombreEtapa(nombre);

            return cotizacion.Etapas.FirstOrDefault(e =>
                e != null &&
                NormalizarNombreEtapa(e.Nombre) == normalizado
            );
        }

        private string ObtenerNivelInicialChecklist(List<CategoriaTrabajador> opciones)
        {
            if (opciones == null || opciones.Count == 0)
            {
                return "";
            }

            CategoriaTrabajador tipico = opciones.FirstOrDefault(c =>
                NormalizarTextoChecklist(c.Nivel).Contains("tipico")
            );

            if (tipico != null)
            {
                return tipico.Nivel;
            }

            CategoriaTrabajador general = opciones.FirstOrDefault(c =>
                NormalizarTextoChecklist(c.Nivel).Contains("general")
            );

            if (general != null)
            {
                return general.Nivel;
            }

            return opciones
                .OrderBy(c => ObtenerOrdenNivelChecklist(c.Nivel))
                .First()
                .Nivel;
        }

        private CategoriaTrabajador ObtenerCargoPorNivelChecklist(
            List<CategoriaTrabajador> opciones,
            string nivel
        )
        {
            if (opciones == null || opciones.Count == 0)
            {
                return null;
            }

            string nivelNormalizado = NormalizarTextoChecklist(nivel);

            CategoriaTrabajador encontrado = opciones.FirstOrDefault(c =>
                NormalizarTextoChecklist(c.Nivel) == nivelNormalizado
            );

            if (encontrado != null)
            {
                return encontrado;
            }

            return opciones.First();
        }

        private int ObtenerOrdenNivelChecklist(string nivel)
        {
            string n = NormalizarTextoChecklist(nivel);

            if (n.Contains("general")) return 0;
            if (n.Contains("junior")) return 1;
            if (n.Contains("tipico")) return 2;
            if (n.Contains("senior")) return 3;
            if (n.Contains("personalizado")) return 99;

            return 50;
        }

        private int ObtenerOrdenCargoBaseChecklist(string nombreCargo)
        {
            string nombre = NormalizarTextoChecklist(nombreCargo);

            if (nombre.Contains("productor") || nombre.Contains("projectmanager")) return 0;
            if (nombre.Contains("directorcreativo")) return 1;
            if (nombre.Contains("directoraanimacion") || nombre.Contains("directordeanimacion")) return 2;
            if (nombre.Contains("coordinador")) return 3;

            if (nombre.Contains("guionista")) return 10;
            if (nombre.Contains("investigador")) return 11;
            if (nombre.Contains("conceptual")) return 12;
            if (nombre.Contains("directordearte")) return 13;

            if (nombre.Contains("storyboard")) return 20;
            if (nombre.Contains("personajes")) return 21;
            if (nombre.Contains("fondos")) return 22;
            if (nombre.Contains("props")) return 23;
            if (nombre.Contains("animatic")) return 24;

            if (nombre.Contains("animador")) return 30;
            if (nombre.Contains("cleanup")) return 31;
            if (nombre.Contains("colorista")) return 32;
            if (nombre.Contains("artistadefondos")) return 33;
            if (nombre.Contains("asistente")) return 34;

            if (nombre.Contains("editor")) return 40;
            if (nombre.Contains("compositor")) return 41;
            if (nombre.Contains("motion")) return 42;
            if (nombre.Contains("sound")) return 43;
            if (nombre.Contains("mezclador")) return 44;
            if (nombre.Contains("correccion")) return 45;
            if (nombre.Contains("render")) return 46;

            if (nombre.Contains("otro")) return 99;

            return 60;
        }

        private string ObtenerClaveChecklistCargo(string etapa, string cargoBase)
        {
            return NormalizarNombreEtapa(etapa) + "|" + NormalizarNombreCargoParaComparar(cargoBase);
        }

        private string NormalizarTextoChecklist(string texto)
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
