using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private ComboBox cmbMetodoCalculoSimpleEcuacion = new ComboBox();
        private ComboBox cmbTipoTrabajoSimpleEcuacion = new ComboBox();
        private ComboBox cmbDependenciaSimpleEcuacion = new ComboBox();
        private NumericUpDown nudValorSimpleEcuacion = new NumericUpDown();
        private Label lblValorSimpleEcuacion = new Label();
        private Label lblExplicacionSimpleEcuacion = new Label();
        private RichTextBox rtbVistaPreviaSimpleEcuacion = new RichTextBox();
        private Panel pnlAvanzadoEcuacion = new Panel();
        private Button btnAlternarAvanzadoEcuacion = new Button();
        private bool sincronizandoEditorSimpleEcuacion = false;

        private sealed class OpcionMetodoSimpleEcuacion
        {
            public MetodoCalculoProceso Metodo { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return Nombre;
            }
        }

        private sealed class OpcionDependenciaSimpleEcuacion
        {
            public string Id { get; set; } = "";
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(Id)
                    ? "Sin dependencia"
                    : (string.IsNullOrWhiteSpace(Nombre) ? Id : Nombre);
            }
        }

        private sealed class OpcionTipoTrabajoSimpleEcuacion
        {
            public TipoProcesoProductivo Tipo { get; set; }
            public string Nombre { get; set; } = "";

            public override string ToString()
            {
                return Nombre;
            }
        }

        private Control CrearPanelEditorSimpleEcuacion()
        {
            TableLayoutPanel tarjeta = new TableLayoutPanel();
            tarjeta.Dock = DockStyle.Top;
            tarjeta.AutoSize = true;
            tarjeta.ColumnCount = 1;
            tarjeta.RowCount = 4;
            tarjeta.Padding = new Padding(14, 10, 14, 12);
            tarjeta.Margin = new Padding(0, 0, 0, 8);
            tarjeta.BackColor = Color.White;
            tarjeta.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            Label titulo = new Label();
            titulo.Text = "¿Cómo se calculan las horas de este trabajo?";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(28, 38, 55);
            titulo.Margin = new Padding(0, 0, 0, 6);

            TableLayoutPanel formulario = new TableLayoutPanel();
            formulario.Dock = DockStyle.Top;
            formulario.AutoSize = true;
            formulario.ColumnCount = 2;
            formulario.RowCount = 4;
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            ConfigurarMetodosSimplesEcuacion();
            ConfigurarTiposTrabajoSimplesEcuacion();
            cmbTipoTrabajoSimpleEcuacion.Dock = DockStyle.Top;
            cmbTipoTrabajoSimpleEcuacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoTrabajoSimpleEcuacion.Margin = new Padding(0, 0, 0, 8);
            cmbTipoTrabajoSimpleEcuacion.SelectedIndexChanged -= EditorSimpleEcuacion_Changed;
            cmbTipoTrabajoSimpleEcuacion.SelectedIndexChanged += EditorSimpleEcuacion_Changed;

            cmbMetodoCalculoSimpleEcuacion.Dock = DockStyle.Top;
            cmbMetodoCalculoSimpleEcuacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetodoCalculoSimpleEcuacion.Margin = new Padding(0, 0, 0, 8);
            cmbMetodoCalculoSimpleEcuacion.SelectedIndexChanged -= EditorSimpleEcuacion_Changed;
            cmbMetodoCalculoSimpleEcuacion.SelectedIndexChanged += EditorSimpleEcuacion_Changed;

            cmbDependenciaSimpleEcuacion.Dock = DockStyle.Top;
            cmbDependenciaSimpleEcuacion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDependenciaSimpleEcuacion.Margin = new Padding(0, 0, 0, 8);
            cmbDependenciaSimpleEcuacion.SelectedIndexChanged -= EditorSimpleEcuacion_Changed;
            cmbDependenciaSimpleEcuacion.SelectedIndexChanged += EditorSimpleEcuacion_Changed;

            nudValorSimpleEcuacion.DecimalPlaces = 2;
            nudValorSimpleEcuacion.Minimum = 0;
            nudValorSimpleEcuacion.Maximum = 100000;
            nudValorSimpleEcuacion.Increment = 0.25M;
            nudValorSimpleEcuacion.Width = 150;
            nudValorSimpleEcuacion.ThousandsSeparator = true;
            nudValorSimpleEcuacion.ValueChanged -= EditorSimpleEcuacion_Changed;
            nudValorSimpleEcuacion.ValueChanged += EditorSimpleEcuacion_Changed;

            formulario.Controls.Add(CrearEtiquetaSimpleEcuacion("Tipo de trabajo"), 0, 0);
            formulario.Controls.Add(cmbTipoTrabajoSimpleEcuacion, 1, 0);
            formulario.Controls.Add(CrearEtiquetaSimpleEcuacion("Tipo de cálculo"), 0, 1);
            formulario.Controls.Add(cmbMetodoCalculoSimpleEcuacion, 1, 1);
            formulario.Controls.Add(CrearEtiquetaSimpleEcuacion("Trabajo relacionado"), 0, 2);
            formulario.Controls.Add(cmbDependenciaSimpleEcuacion, 1, 2);
            lblValorSimpleEcuacion = CrearEtiquetaSimpleEcuacion("Valor");
            formulario.Controls.Add(lblValorSimpleEcuacion, 0, 3);
            formulario.Controls.Add(nudValorSimpleEcuacion, 1, 3);

            lblExplicacionSimpleEcuacion.AutoSize = true;
            lblExplicacionSimpleEcuacion.MaximumSize = new Size(1000, 0);
            lblExplicacionSimpleEcuacion.Padding = new Padding(10, 7, 10, 7);
            lblExplicacionSimpleEcuacion.Margin = new Padding(0, 7, 0, 0);
            lblExplicacionSimpleEcuacion.BackColor = Color.FromArgb(235, 247, 244);
            lblExplicacionSimpleEcuacion.ForeColor = Color.FromArgb(24, 82, 72);
            lblExplicacionSimpleEcuacion.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            tarjeta.Controls.Add(titulo, 0, 0);
            tarjeta.Controls.Add(formulario, 0, 1);
            tarjeta.Controls.Add(lblExplicacionSimpleEcuacion, 0, 2);
            tarjeta.Controls.Add(CrearAyudaSimpleEcuacion(), 0, 3);
            return tarjeta;
        }

        private Label CrearEtiquetaSimpleEcuacion(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                Margin = new Padding(0, 5, 12, 8)
            };
        }

        private Label CrearAyudaSimpleEcuacion()
        {
            return new Label
            {
                Text = "Elige una regla y completa solo los datos visibles. La fórmula técnica se genera automáticamente.",
                AutoSize = true,
                MaximumSize = new Size(1000, 0),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(95, 100, 110),
                Margin = new Padding(0, 6, 0, 0)
            };
        }

        private Control CrearTarjetaCargosEcuacion()
        {
            TableLayoutPanel tarjeta = new TableLayoutPanel();
            tarjeta.Dock = DockStyle.Top;
            tarjeta.AutoSize = true;
            tarjeta.ColumnCount = 1;
            tarjeta.RowCount = 2;
            tarjeta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tarjeta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tarjeta.Padding = new Padding(14, 10, 14, 12);
            tarjeta.Margin = new Padding(0, 0, 0, 8);
            tarjeta.BackColor = Color.White;
            tarjeta.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            Label ayuda = new Label();
            ayuda.Text = "¿Quién realiza este trabajo?";
            ayuda.AutoSize = true;
            ayuda.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            ayuda.ForeColor = Color.FromArgb(28, 38, 55);
            ayuda.Margin = new Padding(0, 0, 0, 8);

            tarjeta.Controls.Add(ayuda, 0, 0);
            tarjeta.Controls.Add(CrearPanelCargosParticipantesEcuacion(), 0, 1);
            return tarjeta;
        }

        private Control CrearPanelVistaPreviaSimpleEcuacion()
        {
            TableLayoutPanel tarjeta = new TableLayoutPanel();
            tarjeta.Dock = DockStyle.Top;
            tarjeta.AutoSize = true;
            tarjeta.ColumnCount = 1;
            tarjeta.RowCount = 2;
            tarjeta.Padding = new Padding(18, 14, 18, 16);
            tarjeta.Margin = new Padding(0, 0, 0, 12);
            tarjeta.BackColor = Color.FromArgb(245, 249, 255);
            tarjeta.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            Label titulo = new Label();
            titulo.Text = "Ejemplo inmediato";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(30, 80, 150);
            titulo.Margin = new Padding(0, 0, 0, 8);

            rtbVistaPreviaSimpleEcuacion.Dock = DockStyle.Top;
            rtbVistaPreviaSimpleEcuacion.Height = 105;
            rtbVistaPreviaSimpleEcuacion.ReadOnly = true;
            rtbVistaPreviaSimpleEcuacion.BorderStyle = BorderStyle.None;
            rtbVistaPreviaSimpleEcuacion.BackColor = Color.FromArgb(245, 249, 255);
            rtbVistaPreviaSimpleEcuacion.Font = new Font("Segoe UI", 10.5f);
            rtbVistaPreviaSimpleEcuacion.ScrollBars = RichTextBoxScrollBars.None;

            tarjeta.Controls.Add(titulo, 0, 0);
            tarjeta.Controls.Add(rtbVistaPreviaSimpleEcuacion, 0, 1);
            return tarjeta;
        }

        private Control CrearPanelAvanzadoEcuacion()
        {
            TableLayoutPanel contenedor = new TableLayoutPanel();
            contenedor.Dock = DockStyle.Top;
            contenedor.AutoSize = true;
            contenedor.ColumnCount = 1;
            contenedor.RowCount = 2;
            contenedor.Margin = new Padding(0, 0, 0, 12);

            btnAlternarAvanzadoEcuacion = CrearBotonEcuacion("▸ Opciones avanzadas");
            btnAlternarAvanzadoEcuacion.Width = 180;
            btnAlternarAvanzadoEcuacion.Margin = new Padding(0, 0, 0, 8);
            btnAlternarAvanzadoEcuacion.Click += (s, e) =>
            {
                pnlAvanzadoEcuacion.Visible = !pnlAvanzadoEcuacion.Visible;
                btnAlternarAvanzadoEcuacion.Text = pnlAvanzadoEcuacion.Visible
                    ? "▾ Ocultar opciones avanzadas"
                    : "▸ Opciones avanzadas";
                btnAlternarAvanzadoEcuacion.Width = pnlAvanzadoEcuacion.Visible ? 230 : 180;
            };

            TableLayoutPanel contenido = new TableLayoutPanel();
            contenido.Dock = DockStyle.Top;
            contenido.AutoSize = true;
            contenido.ColumnCount = 1;
            contenido.RowCount = 3;
            contenido.Controls.Add(CrearGrupoIdentificacionEcuacion(), 0, 0);
            contenido.Controls.Add(CrearGrupoTecnicoAvanzadoEcuacion(), 0, 1);
            contenido.Controls.Add(CrearPanelRenderEcuacionProductiva(), 0, 2);

            pnlAvanzadoEcuacion = new Panel();
            pnlAvanzadoEcuacion.Dock = DockStyle.Top;
            pnlAvanzadoEcuacion.AutoSize = true;
            pnlAvanzadoEcuacion.Visible = false;
            pnlAvanzadoEcuacion.Controls.Add(contenido);

            contenedor.Controls.Add(btnAlternarAvanzadoEcuacion, 0, 0);
            contenedor.Controls.Add(pnlAvanzadoEcuacion, 0, 1);
            return contenedor;
        }

        private Control CrearGrupoTecnicoAvanzadoEcuacion()
        {
            TableLayoutPanel tabla = new TableLayoutPanel();
            tabla.Dock = DockStyle.Top;
            tabla.AutoSize = true;
            tabla.ColumnCount = 2;
            tabla.RowCount = 5;
            tabla.Padding = new Padding(12, 10, 12, 12);
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AgregarCampoEditorEcuacion(tabla, 0, "Fórmula madre", CrearPanelFormulaMadreEditor());
            tabla.Controls.Add(CrearEtiquetaEditorEcuacion("Variables de entrada"), 0, 1);
            tabla.Controls.Add(CrearPanelListaEditableEcuacion(lstEcuacionVariables, txtNuevaVariableEcuacion, "Agregar variable"), 1, 1);
            AgregarCampoEditorEcuacion(tabla, 2, "Fórmula técnica", txtEcuacionFormula);
            AgregarCampoEditorEcuacion(tabla, 3, "Impacto", txtEcuacionImpacto);
            AgregarCampoEditorEcuacion(tabla, 4, "Nota", txtEcuacionNota);
            return CrearGrupoEditorEcuacion("Configuración técnica", tabla);
        }

        private void ConfigurarMetodosSimplesEcuacion()
        {
            if (cmbMetodoCalculoSimpleEcuacion.Items.Count > 0)
            {
                return;
            }

            cmbMetodoCalculoSimpleEcuacion.Items.AddRange(new object[]
            {
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorCantidad, Nombre = "Horas por cantidad" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorCapacidad, Nombre = "Cantidad según capacidad del cargo" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorPorcentajeProduccion, Nombre = "Porcentaje de otro trabajo" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorDuracionEntregable, Nombre = "Según duración del entregable" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorDuracionEtapa, Nombre = "Horas por semana de la etapa" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorDuracionProyecto, Nombre = "Horas por semana del proyecto" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorEvento, Nombre = "Horas por evento" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.PorRevision, Nombre = "Horas por revisión" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.Fijo, Nombre = "Horas fijas" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.Manual, Nombre = "Manual / fórmula avanzada" },
                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.NoDefinido, Nombre = "Sin definir" }
            });
        }

        private void ConfigurarTiposTrabajoSimplesEcuacion()
        {
            if (cmbTipoTrabajoSimpleEcuacion.Items.Count > 0)
            {
                return;
            }

            cmbTipoTrabajoSimpleEcuacion.Items.AddRange(new object[]
            {
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.ProduccionDirecta, Nombre = "Producción principal" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.RevisionControl, Nombre = "Revisión y control" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.CorreccionRetrabajo, Nombre = "Corrección o retrabajo" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.Supervision, Nombre = "Supervisión" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.Direccion, Nombre = "Dirección" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.GestionCoordinacion, Nombre = "Gestión y coordinación" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.EntregaSoporte, Nombre = "Entrega o soporte" },
                new OpcionTipoTrabajoSimpleEcuacion { Tipo = TipoProcesoProductivo.NoClasificado, Nombre = "Sin clasificar" }
            });
        }

        private void RefrescarDependenciasSimplesEcuacion(string seleccion)
        {
            cmbDependenciaSimpleEcuacion.Items.Clear();
            cmbDependenciaSimpleEcuacion.Items.Add(new OpcionDependenciaSimpleEcuacion());

            foreach (DataGridViewRow fila in dgvEcuacionesProductivas.Rows)
            {
                if (fila == null || fila.IsNewRow || fila == dgvEcuacionesProductivas.CurrentRow)
                {
                    continue;
                }

                string id = ObtenerValorFilaEcuacion(fila, "IdProceso");
                if (string.IsNullOrWhiteSpace(id))
                {
                    id = ObtenerValorFilaEcuacion(fila, "Clave");
                }

                if (string.IsNullOrWhiteSpace(id) ||
                    cmbDependenciaSimpleEcuacion.Items.Cast<OpcionDependenciaSimpleEcuacion>()
                        .Any(o => string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                cmbDependenciaSimpleEcuacion.Items.Add(new OpcionDependenciaSimpleEcuacion
                {
                    Id = id,
                    Nombre = ObtenerValorFilaEcuacion(fila, "NombreVisible") + " (" + id + ")"
                });
            }

            OpcionDependenciaSimpleEcuacion elegida =
                cmbDependenciaSimpleEcuacion.Items.Cast<OpcionDependenciaSimpleEcuacion>()
                    .FirstOrDefault(o => string.Equals(o.Id, seleccion ?? "", StringComparison.OrdinalIgnoreCase));

            if (elegida == null && !string.IsNullOrWhiteSpace(seleccion))
            {
                elegida = new OpcionDependenciaSimpleEcuacion
                {
                    Id = seleccion,
                    Nombre = seleccion + " (no encontrado)"
                };
                cmbDependenciaSimpleEcuacion.Items.Add(elegida);
            }

            cmbDependenciaSimpleEcuacion.SelectedItem = elegida ??
                cmbDependenciaSimpleEcuacion.Items.Cast<OpcionDependenciaSimpleEcuacion>().First();
        }

        private void CargarEditorSimpleEcuacionDesdeFila(DataGridViewRow fila)
        {
            if (fila == null || fila.IsNewRow)
            {
                return;
            }

            sincronizandoEditorSimpleEcuacion = true;
            try
            {
                MetodoCalculoProceso metodo = MetodoCalculoProceso.NoDefinido;
                Enum.TryParse(ObtenerValorFilaEcuacion(fila, "MetodoCalculo"), true, out metodo);
                SeleccionarMetodoSimpleEcuacion(metodo);
                TipoProcesoProductivo tipo = TipoProcesoProductivo.NoClasificado;
                Enum.TryParse(ObtenerValorFilaEcuacion(fila, "TipoProceso"), true, out tipo);
                SeleccionarTipoTrabajoSimpleEcuacion(tipo);

                string dependencia = LeerPrimeraDependenciaSimpleEcuacion(
                    ObtenerValorFilaEcuacion(fila, "DependenciasJson"));
                RefrescarDependenciasSimplesEcuacion(dependencia);

                decimal valor = (decimal)Math.Max(0.0, ExtraerValorSimpleEcuacion(
                    metodo,
                    ObtenerValorFilaEcuacion(fila, "FormulaReferencia")));
                nudValorSimpleEcuacion.Value = Math.Min(nudValorSimpleEcuacion.Maximum, valor);
                ActualizarCamposVisiblesEditorSimpleEcuacion();
            }
            finally
            {
                sincronizandoEditorSimpleEcuacion = false;
            }

            ActualizarVistaPreviaSimpleEcuacion();
        }

        private void SeleccionarMetodoSimpleEcuacion(MetodoCalculoProceso metodo)
        {
            OpcionMetodoSimpleEcuacion opcion = cmbMetodoCalculoSimpleEcuacion.Items
                .Cast<OpcionMetodoSimpleEcuacion>()
                .FirstOrDefault(o => o.Metodo == metodo);
            cmbMetodoCalculoSimpleEcuacion.SelectedItem = opcion ??
                cmbMetodoCalculoSimpleEcuacion.Items.Cast<OpcionMetodoSimpleEcuacion>().Last();
        }

        private MetodoCalculoProceso ObtenerMetodoSimpleEcuacion()
        {
            return cmbMetodoCalculoSimpleEcuacion.SelectedItem is OpcionMetodoSimpleEcuacion opcion
                ? opcion.Metodo
                : MetodoCalculoProceso.NoDefinido;
        }

        private void SeleccionarTipoTrabajoSimpleEcuacion(TipoProcesoProductivo tipo)
        {
            OpcionTipoTrabajoSimpleEcuacion opcion = cmbTipoTrabajoSimpleEcuacion.Items
                .Cast<OpcionTipoTrabajoSimpleEcuacion>()
                .FirstOrDefault(o => o.Tipo == tipo);
            cmbTipoTrabajoSimpleEcuacion.SelectedItem = opcion ??
                cmbTipoTrabajoSimpleEcuacion.Items.Cast<OpcionTipoTrabajoSimpleEcuacion>().Last();
        }

        private TipoProcesoProductivo ObtenerTipoTrabajoSimpleEcuacion()
        {
            return cmbTipoTrabajoSimpleEcuacion.SelectedItem is OpcionTipoTrabajoSimpleEcuacion opcion
                ? opcion.Tipo
                : TipoProcesoProductivo.NoClasificado;
        }

        private string ObtenerDependenciaSimpleEcuacion()
        {
            return cmbDependenciaSimpleEcuacion.SelectedItem is OpcionDependenciaSimpleEcuacion opcion
                ? opcion.Id ?? ""
                : "";
        }

        private void EditorSimpleEcuacion_Changed(object sender, EventArgs e)
        {
            if (sincronizandoEditorSimpleEcuacion || cargandoEditorEcuaciones)
            {
                return;
            }

            ActualizarCamposVisiblesEditorSimpleEcuacion();
            SincronizarFormulaDesdeEditorSimpleEcuacion();
            ActualizarVistaPreviaSimpleEcuacion();
            ActualizarRenderEcuacionProductiva();
        }

        private void ActualizarCamposVisiblesEditorSimpleEcuacion()
        {
            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            bool usaDependencia = metodo == MetodoCalculoProceso.PorPorcentajeProduccion ||
                                  metodo == MetodoCalculoProceso.PorRevision;
            cmbDependenciaSimpleEcuacion.Enabled = usaDependencia;

            switch (metodo)
            {
                case MetodoCalculoProceso.PorPorcentajeProduccion:
                    lblValorSimpleEcuacion.Text = "Porcentaje";
                    nudValorSimpleEcuacion.Enabled = true;
                    break;
                case MetodoCalculoProceso.PorDuracionEtapa:
                case MetodoCalculoProceso.PorDuracionProyecto:
                    lblValorSimpleEcuacion.Text = "Horas por semana";
                    nudValorSimpleEcuacion.Enabled = true;
                    break;
                case MetodoCalculoProceso.Fijo:
                    lblValorSimpleEcuacion.Text = "Horas totales";
                    nudValorSimpleEcuacion.Enabled = true;
                    break;
                case MetodoCalculoProceso.PorEvento:
                case MetodoCalculoProceso.PorRevision:
                case MetodoCalculoProceso.PorCantidad:
                case MetodoCalculoProceso.PorDuracionEntregable:
                    lblValorSimpleEcuacion.Text = "Horas por unidad";
                    nudValorSimpleEcuacion.Enabled = true;
                    break;
                case MetodoCalculoProceso.PorCapacidad:
                    lblValorSimpleEcuacion.Text = "Capacidad";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
                default:
                    lblValorSimpleEcuacion.Text = "Valor";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
            }

            if (metodo == MetodoCalculoProceso.Manual && !pnlAvanzadoEcuacion.Visible)
            {
                pnlAvanzadoEcuacion.Visible = true;
                btnAlternarAvanzadoEcuacion.Text = "▾ Ocultar opciones avanzadas";
                btnAlternarAvanzadoEcuacion.Width = 230;
            }
        }

        private void SincronizarFormulaDesdeEditorSimpleEcuacion()
        {
            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            double valor = (double)nudValorSimpleEcuacion.Value;
            string destino = CrearNombreVariableFormulaSimple(txtEcuacionNombre.Text);

            switch (metodo)
            {
                case MetodoCalculoProceso.PorPorcentajeProduccion:
                    txtEcuacionFormula.Text = destino + " = horas_produccion * " +
                        (valor / 100.0).ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.PorDuracionEtapa:
                case MetodoCalculoProceso.PorDuracionProyecto:
                    txtEcuacionFormula.Text = destino + " = semanas_activas * " +
                        valor.ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.Fijo:
                    txtEcuacionFormula.Text = destino + " = " +
                        valor.ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.PorEvento:
                    txtEcuacionFormula.Text = destino + " = eventos * " +
                        valor.ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.PorRevision:
                    txtEcuacionFormula.Text = destino + " = revisiones * " +
                        valor.ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.PorCantidad:
                case MetodoCalculoProceso.PorDuracionEntregable:
                    txtEcuacionFormula.Text = destino + " = cantidad * " +
                        valor.ToString("0.####", CultureInfo.InvariantCulture);
                    break;
                case MetodoCalculoProceso.PorCapacidad:
                    txtEcuacionFormula.Text = destino + " = cantidad / capacidad_por_periodo[cargo]";
                    break;
                case MetodoCalculoProceso.Manual:
                case MetodoCalculoProceso.NoDefinido:
                    break;
            }
        }

        private string CrearNombreVariableFormulaSimple(string nombre)
        {
            string normalizado = NormalizarTextoDatosVisual(nombre ?? "")
                .Replace(" ", "_")
                .Replace("-", "_");
            return string.IsNullOrWhiteSpace(normalizado) ? "horas_proceso" : "horas_" + normalizado;
        }

        private void AplicarEditorSimpleAFila(DataGridViewRow fila)
        {
            if (fila == null || fila.IsNewRow)
            {
                return;
            }

            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            fila.Cells["MetodoCalculo"].Value = metodo.ToString();
            fila.Cells["TipoProceso"].Value = ObtenerTipoTrabajoSimpleEcuacion().ToString();
            string dependencia = ObtenerDependenciaSimpleEcuacion();
            fila.Cells["DependenciasJson"].Value =
                ReglaCalculoProcesoService.CrearDependenciasJson(new[] { dependencia });
        }

        private string LeerPrimeraDependenciaSimpleEcuacion(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "";
            }

            return ReglaCalculoProcesoService.LeerDependenciasJson(json).FirstOrDefault() ?? "";
        }

        private double ExtraerValorSimpleEcuacion(MetodoCalculoProceso metodo, string formula)
        {
            string texto = formula ?? "";
            IEnumerable<double> numeros = System.Text.RegularExpressions.Regex
                .Matches(texto, @"(?<![\w])\d+(?:[\.,]\d+)?")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(m =>
                {
                    double.TryParse(m.Value.Replace(',', '.'), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out double numero);
                    return numero;
                });

            double valor = numeros.LastOrDefault();
            if (metodo == MetodoCalculoProceso.PorPorcentajeProduccion && valor <= 1.0)
            {
                valor *= 100.0;
            }

            if (valor <= 0.0)
            {
                return metodo == MetodoCalculoProceso.PorPorcentajeProduccion ? 10.0 :
                    metodo == MetodoCalculoProceso.PorDuracionProyecto ? 4.0 : 1.0;
            }

            return valor;
        }

        private void ActualizarVistaPreviaSimpleEcuacion()
        {
            if (rtbVistaPreviaSimpleEcuacion == null)
            {
                return;
            }

            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            double valor = (double)nudValorSimpleEcuacion.Value;
            string dependencia = ObtenerDependenciaSimpleEcuacion();
            string nombreDependencia = cmbDependenciaSimpleEcuacion.SelectedItem?.ToString() ?? "otro trabajo";
            string regla;
            string ejemplo;

            switch (metodo)
            {
                case MetodoCalculoProceso.PorPorcentajeProduccion:
                    regla = valor.ToString("0.##") + " % de " +
                        (string.IsNullOrWhiteSpace(dependencia) ? "otro trabajo" : nombreDependencia);
                    ejemplo = "Si el trabajo relacionado dura 10 h: 10 × " +
                        valor.ToString("0.##") + " % = " + (10.0 * valor / 100.0).ToString("0.##") + " h";
                    break;
                case MetodoCalculoProceso.PorDuracionEtapa:
                case MetodoCalculoProceso.PorDuracionProyecto:
                    regla = valor.ToString("0.##") + " horas por cada semana activa";
                    ejemplo = "Para 4 semanas: 4 × " + valor.ToString("0.##") + " = " +
                        (4.0 * valor).ToString("0.##") + " h";
                    break;
                case MetodoCalculoProceso.Fijo:
                    regla = valor.ToString("0.##") + " horas fijas";
                    ejemplo = "El trabajo siempre considera " + valor.ToString("0.##") + " h";
                    break;
                case MetodoCalculoProceso.PorCapacidad:
                    regla = "Cantidad dividida por la capacidad del cargo";
                    ejemplo = "La capacidad se obtiene desde Rendimientos y el cargo seleccionado";
                    break;
                case MetodoCalculoProceso.Manual:
                    regla = "Fórmula configurada manualmente";
                    ejemplo = string.IsNullOrWhiteSpace(txtEcuacionFormula.Text)
                        ? "Abre Opciones avanzadas para escribir la fórmula"
                        : txtEcuacionFormula.Text;
                    break;
                case MetodoCalculoProceso.NoDefinido:
                    regla = "Todavía no se ha definido una regla";
                    ejemplo = "Selecciona un tipo de cálculo para ver un ejemplo";
                    break;
                default:
                    regla = valor.ToString("0.##") + " horas por unidad";
                    ejemplo = "Para 10 unidades: 10 × " + valor.ToString("0.##") + " = " +
                        (10.0 * valor).ToString("0.##") + " h";
                    break;
            }

            lblExplicacionSimpleEcuacion.Text = "Regla: " + regla;
            int cargos = dgvCargosParticipantesEcuacion == null
                ? 0
                : dgvCargosParticipantesEcuacion.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);

            rtbVistaPreviaSimpleEcuacion.Clear();
            rtbVistaPreviaSimpleEcuacion.SelectionFont = new Font("Segoe UI", 11f, FontStyle.Bold);
            rtbVistaPreviaSimpleEcuacion.SelectionColor = Color.FromArgb(25, 70, 130);
            rtbVistaPreviaSimpleEcuacion.AppendText(regla + Environment.NewLine);
            rtbVistaPreviaSimpleEcuacion.SelectionFont = new Font("Segoe UI", 10f, FontStyle.Regular);
            rtbVistaPreviaSimpleEcuacion.SelectionColor = Color.FromArgb(55, 65, 81);
            rtbVistaPreviaSimpleEcuacion.AppendText(ejemplo + Environment.NewLine);
            List<string> avisos = new List<string>();
            if (metodo == MetodoCalculoProceso.PorPorcentajeProduccion &&
                string.IsNullOrWhiteSpace(dependencia))
            {
                avisos.Add("⚠ Selecciona el trabajo del que depende.");
            }
            if (cargos == 0)
            {
                avisos.Add("⚠ Falta agregar al menos un cargo.");
            }

            rtbVistaPreviaSimpleEcuacion.AppendText(avisos.Count > 0
                ? string.Join("  ", avisos)
                : cargos + (cargos == 1 ? " cargo asociado." : " cargos asociados."));
        }
    }
}
