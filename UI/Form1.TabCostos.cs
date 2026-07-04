using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabCostos(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 1;
            layout.RowCount = 4;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 12;
            panelSuperior.RowCount = 4;

            panelSuperior.ColumnStyles.Clear();

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

            Label titulo = new Label();
            titulo.Text = "Costos extra del proyecto";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Dock = DockStyle.Fill;

            panelSuperior.Controls.Add(titulo, 0, 0);
            panelSuperior.SetColumnSpan(titulo, 10);

            Label lblCategoria = new Label();
            lblCategoria.Text = "Categoría:";
            lblCategoria.TextAlign = ContentAlignment.MiddleLeft;
            lblCategoria.Dock = DockStyle.Fill;

            cmbCategoriaCostoExtra.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoriaCostoExtra.Dock = DockStyle.Fill;

            cmbCategoriaCostoExtra.Items.Clear();
            cmbCategoriaCostoExtra.Items.Add("Servicios externos");
            cmbCategoriaCostoExtra.Items.Add("Inversión inicial");
            cmbCategoriaCostoExtra.Items.Add("Insumos");
            cmbCategoriaCostoExtra.Items.Add("Licencias / software");
            cmbCategoriaCostoExtra.Items.Add("Compra de assets");
            cmbCategoriaCostoExtra.Items.Add("Equipamiento");
            cmbCategoriaCostoExtra.Items.Add("Arriendo / infraestructura");
            cmbCategoriaCostoExtra.Items.Add("Transporte / reuniones");
            cmbCategoriaCostoExtra.Items.Add("Correcciones extraordinarias");
            cmbCategoriaCostoExtra.Items.Add("Otros");

            if (cmbCategoriaCostoExtra.Items.Count > 0)
            {
                cmbCategoriaCostoExtra.SelectedIndex = 0;
            }

            Label lblDescripcion = new Label();
            lblDescripcion.Text = "Descripción:";
            lblDescripcion.TextAlign = ContentAlignment.MiddleLeft;
            lblDescripcion.Dock = DockStyle.Fill;

            txtDescripcionCostoExtra.Dock = DockStyle.Fill;

            Label lblMonto = new Label();
            lblMonto.Text = "Monto:";
            lblMonto.TextAlign = ContentAlignment.MiddleLeft;
            lblMonto.Dock = DockStyle.Fill;

            txtMontoCostoExtra.Dock = DockStyle.Fill;
            txtMontoCostoExtra.TextChanged += TextBoxMoneda_TextChanged;

            Label lblPeriodicidad = new Label();
            lblPeriodicidad.Text = "Frecuencia:";
            lblPeriodicidad.TextAlign = ContentAlignment.MiddleLeft;
            lblPeriodicidad.Dock = DockStyle.Fill;

            cmbPeriodicidadCostoExtra.Dock = DockStyle.Fill;
            cmbPeriodicidadCostoExtra.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriodicidadCostoExtra.Items.Clear();
            cmbPeriodicidadCostoExtra.Items.Add("Una sola vez");
            cmbPeriodicidadCostoExtra.Items.Add("Mensual");
            cmbPeriodicidadCostoExtra.Items.Add("Anual");
            cmbPeriodicidadCostoExtra.SelectedIndex = 0;

            chkCostoVisibleCliente.Text = "Visible cliente";
            chkCostoVisibleCliente.Checked = true;
            chkCostoVisibleCliente.AutoSize = true;
            chkCostoVisibleCliente.Dock = DockStyle.Fill;

            btnAgregarCostoExtra.Text = "Agregar costo";
            btnAgregarCostoExtra.Width = 130;
            btnAgregarCostoExtra.Height = 32;
            btnAgregarCostoExtra.Click += BtnAgregarCostoExtra_Click;

            btnEliminarCostoExtra.Text = "Eliminar costo";
            btnEliminarCostoExtra.Width = 130;
            btnEliminarCostoExtra.Height = 32;
            btnEliminarCostoExtra.Click += BtnEliminarCostoExtra_Click;

            Button btnRecomendarCostos = new Button();
            btnRecomendarCostos.Text = "Recomendar costos";
            btnRecomendarCostos.Width = 155;
            btnRecomendarCostos.Height = 32;
            btnRecomendarCostos.Click += BtnRecomendarCostos_Click;

            FlowLayoutPanel panelBotonesCostos = new FlowLayoutPanel();
            panelBotonesCostos.Dock = DockStyle.Fill;
            panelBotonesCostos.FlowDirection = FlowDirection.LeftToRight;
            panelBotonesCostos.Controls.Add(btnAgregarCostoExtra);
            panelBotonesCostos.Controls.Add(btnEliminarCostoExtra);
            panelBotonesCostos.Controls.Add(btnRecomendarCostos);

            panelSuperior.Controls.Add(lblCategoria, 0, 1);
            panelSuperior.Controls.Add(cmbCategoriaCostoExtra, 1, 1);
            panelSuperior.Controls.Add(lblDescripcion, 2, 1);
            panelSuperior.Controls.Add(txtDescripcionCostoExtra, 3, 1);
            panelSuperior.Controls.Add(lblMonto, 4, 1);
            panelSuperior.Controls.Add(txtMontoCostoExtra, 5, 1);
            panelSuperior.Controls.Add(lblPeriodicidad, 6, 1);
            panelSuperior.Controls.Add(cmbPeriodicidadCostoExtra, 7, 1);
            panelSuperior.Controls.Add(chkCostoVisibleCliente, 8, 1);
            panelSuperior.Controls.Add(panelBotonesCostos, 0, 2);
            panelSuperior.SetColumnSpan(panelBotonesCostos, 10);

            Label ayuda = new Label();
            ayuda.Text = "Agregue aquí costos no incluidos en mano de obra: inversiones, licencias, insumos, servicios externos, compras de assets, arriendos, transporte u otros.";
            ayuda.ForeColor = Color.DimGray;
            ayuda.Dock = DockStyle.Fill;
            ayuda.TextAlign = ContentAlignment.MiddleLeft;

            panelSuperior.Controls.Add(ayuda, 0, 3);
            panelSuperior.SetColumnSpan(ayuda, 10);

            dgvCostosExtra.Dock = DockStyle.Fill;
            dgvCostosExtra.AllowUserToAddRows = false;
            dgvCostosExtra.AllowUserToDeleteRows = false;
            dgvCostosExtra.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCostosExtra.RowHeadersVisible = false;
            dgvCostosExtra.EditMode = DataGridViewEditMode.EditOnEnter;

            dgvCostosExtra.Columns.Clear();

            dgvCostosExtra.Columns.Add("Categoria", "Categoría");
            dgvCostosExtra.Columns.Add("Descripcion", "Descripción");
            dgvCostosExtra.Columns.Add("Monto", "Monto base");
            dgvCostosExtra.Columns.Add("Periodicidad", "Frecuencia");
            dgvCostosExtra.Columns.Add("MontoCalculado", "Monto calculado");

            DataGridViewCheckBoxColumn colCliente = new DataGridViewCheckBoxColumn();
            colCliente.Name = "VisibleCliente";
            colCliente.HeaderText = "Cliente";
            dgvCostosExtra.Columns.Add(colCliente);

            dgvCostosExtra.Columns["Categoria"].FillWeight = 18;
            dgvCostosExtra.Columns["Descripcion"].FillWeight = 32;
            dgvCostosExtra.Columns["Monto"].FillWeight = 14;
            dgvCostosExtra.Columns["Periodicidad"].FillWeight = 14;
            dgvCostosExtra.Columns["MontoCalculado"].FillWeight = 14;
            dgvCostosExtra.Columns["VisibleCliente"].FillWeight = 8;

            dgvCostosExtra.Columns["MontoCalculado"].ReadOnly = true;

            dgvCostosExtra.CellValueChanged += DgvCostosExtra_CellValueChanged;
            dgvCostosExtra.CurrentCellDirtyStateChanged += DgvCostosExtra_CurrentCellDirtyStateChanged;
            dgvCostosExtra.CellContentClick += DgvCostosExtra_CellContentClick;

            lblTotalCostosExtra.Dock = DockStyle.Fill;
            lblTotalCostosExtra.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblTotalCostosExtra.TextAlign = ContentAlignment.MiddleLeft;

            TableLayoutPanel panelImprevistos = new TableLayoutPanel();
            panelImprevistos.Dock = DockStyle.Fill;
            panelImprevistos.ColumnCount = 4;
            panelImprevistos.RowCount = 1;

            panelImprevistos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            panelImprevistos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            panelImprevistos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panelImprevistos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label lblImprevistos = new Label();
            lblImprevistos.Text = "Colchón seguridad (%):";
            lblImprevistos.Dock = DockStyle.Fill;
            lblImprevistos.TextAlign = ContentAlignment.MiddleLeft;

            txtTasaImprevistos.Dock = DockStyle.Fill;
            txtTasaImprevistos.Leave += CampoCostos_Leave;

            Button btnAplicarCostos = new Button();
            btnAplicarCostos.Text = "Aplicar costos";
            btnAplicarCostos.Width = 130;
            btnAplicarCostos.Height = 30;
            btnAplicarCostos.Click += BtnAplicarCostos_Click;

            panelImprevistos.Controls.Add(lblImprevistos, 0, 0);
            panelImprevistos.Controls.Add(txtTasaImprevistos, 1, 0);
            panelImprevistos.Controls.Add(btnAplicarCostos, 2, 0);

            layout.Controls.Add(panelSuperior, 0, 0);
            layout.Controls.Add(dgvCostosExtra, 0, 1);
            layout.Controls.Add(lblTotalCostosExtra, 0, 2);
            layout.Controls.Add(panelImprevistos, 0, 3);

            tab.Controls.Add(layout);
        }

        private void BtnAgregarCostoExtra_Click(object sender, EventArgs e)
        {
            string categoria = cmbCategoriaCostoExtra.SelectedItem?.ToString() ?? "Otros";
            string descripcion = txtDescripcionCostoExtra.Text.Trim();
            double montoIngreso = ConvertirDouble(txtMontoCostoExtra.Text);
            double monto = ConvertirHaciaCLP(montoIngreso, cotizacion.MonedaPrecioCliente);
            string periodicidad = cmbPeriodicidadCostoExtra.SelectedItem?.ToString() ?? "Una sola vez";

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                MessageBox.Show(
                    "Ingrese una descripción para el costo extra.",
                    "Falta descripción",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            if (monto <= 0.0)
            {
                MessageBox.Show(
                    "Ingrese un monto mayor a cero.",
                    "Monto inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            CostoExtra costo = new CostoExtra
            {
                Categoria = categoria,
                Descripcion = descripcion,
                Monto = monto,
                Periodicidad = periodicidad,
                IncluirEnCliente = chkCostoVisibleCliente.Checked
            };

            costo.MontoCalculado = CalcularMontoCostoExtra(costo);

            cotizacion.CostosExtra.Add(costo);

            txtDescripcionCostoExtra.Text = "";
            txtMontoCostoExtra.Text = "";
            cmbPeriodicidadCostoExtra.SelectedIndex = 0;
            chkCostoVisibleCliente.Checked = true;

            RefrescarCalculosYVista();
        }

        private void BtnRecomendarCostos_Click(object sender, EventArgs e)
        {
            int personas = ObtenerPersonasSugeridasParaCostos();

            int agregados = 0;

            agregados += AgregarCostoRecomendadoSiFalta(
                "Adobe Creative Cloud equipo",
                "Licencias / software",
                $"Adobe Creative Cloud equipo ({personas} personas)",
                Math.Max(90000.0, personas * 45000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Toon Boom / software de animacion",
                "Licencias / software",
                $"Toon Boom / software de animacion ({personas} personas)",
                Math.Max(70000.0, personas * 35000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Microsoft 365 equipo",
                "Licencias / software",
                $"Microsoft 365 equipo ({personas} personas)",
                Math.Max(16000.0, personas * 8000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Almacenamiento y transferencia cloud",
                "Arriendo / infraestructura",
                $"Almacenamiento y transferencia cloud ({personas} personas)",
                Math.Max(15000.0, personas * 8000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Arriendo oficina / cowork",
                "Arriendo / infraestructura",
                $"Arriendo oficina / cowork ({personas} personas)",
                Math.Max(180000.0, personas * 120000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Luz, internet y servicios basicos",
                "Arriendo / infraestructura",
                $"Luz, internet y servicios basicos ({personas} personas)",
                Math.Max(80000.0, 60000.0 + personas * 10000.0),
                "Mensual",
                false
            );

            agregados += AgregarCostoRecomendadoSiFalta(
                "Gastos operacionales generales",
                "Otros",
                $"Gastos operacionales generales ({personas} personas)",
                Math.Max(50000.0, personas * 25000.0),
                "Mensual",
                false
            );

            if (agregados == 0)
            {
                MessageBox.Show(
                    "Los costos recomendados ya estaban actualizados para este proyecto.",
                    "Costos recomendados",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            RefrescarCalculosYVista();

            MessageBox.Show(
                "Se agregaron o actualizaron " + agregados.ToString() +
                " costo(s) recomendado(s) para un equipo estimado de " +
                personas.ToString() + " persona(s).",
                "Costos recomendados",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private int AgregarCostoRecomendadoSiFalta(
            string claveDescripcion,
            string categoria,
            string descripcion,
            double montoMensualCLP,
            string periodicidad,
            bool visibleCliente
        )
        {
            CostoExtra existente = cotizacion.CostosExtra.FirstOrDefault(c =>
                (c.Descripcion ?? "").StartsWith(claveDescripcion, StringComparison.OrdinalIgnoreCase)
            );

            if (existente != null)
            {
                bool cambio =
                    !string.Equals(existente.Categoria, categoria, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existente.Descripcion, descripcion, StringComparison.OrdinalIgnoreCase) ||
                    Math.Abs(existente.Monto - montoMensualCLP) > 0.1 ||
                    !string.Equals(existente.Periodicidad, periodicidad, StringComparison.OrdinalIgnoreCase) ||
                    existente.IncluirEnCliente != visibleCliente;

                existente.Categoria = categoria;
                existente.Descripcion = descripcion;
                existente.MonedaIngreso = "CLP";
                existente.MontoIngreso = montoMensualCLP;
                existente.Monto = montoMensualCLP;
                existente.Periodicidad = periodicidad;
                existente.IncluirEnCliente = visibleCliente;
                existente.MontoCalculado = CalcularMontoCostoExtra(existente);

                return cambio ? 1 : 0;
            }

            CostoExtra costo = new CostoExtra
            {
                Categoria = categoria,
                Descripcion = descripcion,
                MonedaIngreso = "CLP",
                MontoIngreso = montoMensualCLP,
                Monto = montoMensualCLP,
                Periodicidad = periodicidad,
                IncluirEnCliente = visibleCliente
            };

            costo.MontoCalculado = CalcularMontoCostoExtra(costo);
            cotizacion.CostosExtra.Add(costo);

            return 1;
        }

        private int ObtenerPersonasSugeridasParaCostos()
        {
            int personas = 1;

            try
            {
                ResultadoCapacidadProyecto capacidad =
                    PlanCapacidadDesdeDesgloseService.Calcular(
                        cotizacion,
                        ModoDuracionDesglose.Estandar
                    );

                personas = Math.Max(personas, capacidad.PersonasEquivalentesGlobales);

                if (capacidad.Etapas != null && capacidad.Etapas.Count > 0)
                {
                    personas = Math.Max(
                        personas,
                        capacidad.Etapas.Max(e => e.PersonasMinimasEtapa)
                    );

                    int maxCargo = capacidad.Etapas
                        .SelectMany(e => e.Cargos ?? Enumerable.Empty<ResultadoCapacidadCargo>())
                        .Select(c => c.PersonasSugeridas)
                        .DefaultIfEmpty(1)
                        .Max();

                    personas = Math.Max(personas, maxCargo);
                }
            }
            catch
            {
                personas = 1;
            }

            if (cotizacion.PlanGeneralManoObra != null && cotizacion.PlanGeneralManoObra.Count > 0)
            {
                double maxPersonasMes = cotizacion.PlanGeneralManoObra
                    .SelectMany(c => c.PersonasPorBloque ?? new System.Collections.Generic.List<double>())
                    .DefaultIfEmpty(0.0)
                    .Max();

                personas = Math.Max(personas, (int)Math.Ceiling(maxPersonasMes));
            }

            bool hayDesglose =
                cotizacion.DesgloseProductivo != null &&
                cotizacion.DesgloseProductivo.Requerimientos != null &&
                cotizacion.DesgloseProductivo.Requerimientos.Count > 0;

            if (hayDesglose)
            {
                personas = Math.Max(personas, 2);
            }

            return Math.Max(1, personas);
        }

        private void BtnEliminarCostoExtra_Click(object sender, EventArgs e)
        {
            if (dgvCostosExtra.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un costo extra para eliminar.");
                return;
            }

            CostoExtra costo = dgvCostosExtra.CurrentRow.Tag as CostoExtra;

            if (costo == null)
            {
                MessageBox.Show("La fila seleccionada no contiene un costo válido.");
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "¿Eliminar el costo seleccionado?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            cotizacion.CostosExtra.Remove(costo);
            RefrescarCalculosYVista();
        }

        private void DgvCostosExtra_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvCostosExtra.IsCurrentCellDirty)
            {
                dgvCostosExtra.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvCostosExtra_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvCostosExtra.Columns[e.ColumnIndex].Name == "VisibleCliente")
            {
                dgvCostosExtra.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvCostosExtra_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvCostosExtra.Rows[e.RowIndex];

            CostoExtra costo = row.Tag as CostoExtra;

            if (costo == null)
            {
                return;
            }

            costo.Categoria = row.Cells["Categoria"].Value?.ToString() ?? "";
            costo.Descripcion = row.Cells["Descripcion"].Value?.ToString() ?? "";
            costo.Monto = ConvertirHaciaCLP(
                ConvertirDouble(row.Cells["Monto"].Value),
                cotizacion.MonedaVisualizacion
            );
            costo.Periodicidad = row.Cells["Periodicidad"].Value?.ToString() ?? "Una sola vez";
            costo.IncluirEnCliente = ConvertirBool(row.Cells["VisibleCliente"].Value);

            costo.MontoCalculado = CalcularMontoCostoExtra(costo);

            RefrescarCalculosYVista();
        }

        private double CalcularMontoCostoExtra(CostoExtra costo)
        {
            double meses = DuracionVisibleProyecto();

            if (meses <= 0.0)
            {
                meses = 1.0;
            }

            if (costo.Periodicidad == "Mensual")
            {
                return costo.Monto * Math.Ceiling(meses);
            }

            if (costo.Periodicidad == "Anual")
            {
                return (costo.Monto / 12.0) * meses;
            }

            return costo.Monto;
        }

        private void RecalcularCostosExtra()
        {
            foreach (CostoExtra costo in cotizacion.CostosExtra)
            {
                costo.MontoCalculado = CalcularMontoCostoExtra(costo);
            }
        }

        private void CargarTablaCostosExtra()
        {
            cargandoTabla = true;

            RecalcularCostosExtra();

            dgvCostosExtra.Rows.Clear();

            foreach (CostoExtra costo in cotizacion.CostosExtra)
            {
                int rowIndex = dgvCostosExtra.Rows.Add();
                DataGridViewRow row = dgvCostosExtra.Rows[rowIndex];

                row.Cells["Categoria"].Value = costo.Categoria;
                row.Cells["Descripcion"].Value = costo.Descripcion;
                row.Cells["Monto"].Value = FormatearValorVisual(costo.Monto);
                row.Cells["MontoCalculado"].Value = FormatearValorVisual(costo.MontoCalculado);
                row.Cells["Periodicidad"].Value = costo.Periodicidad;
                row.Cells["VisibleCliente"].Value = costo.IncluirEnCliente;

                row.Tag = costo;
            }

            double total = cotizacion.CostosExtra.Sum(c => c.MontoCalculado);

            lblTotalCostosExtra.Text =
                $"Total costos extra: {FormatearValorVisual(total)}";

            cargandoTabla = false;
        }

        private void CargarCostosEnPantalla()
        {
            cargandoCostos = true;

            txtTasaImprevistos.Text = (cotizacion.TasaImprevistos * 100.0).ToString("0.##");

            txtMargenObjetivo.Text = (cotizacion.MargenObjetivo * 100.0).ToString("0.##");
            txtPrecioEvaluado.Text = cotizacion.PrecioVentaEvaluado > 0.0
                ? FormatearMiles(cotizacion.PrecioVentaEvaluado)
                : "";

            if (cmbPaisImpuestoVenta.Items.Count > 0)
            {
                string pais = string.IsNullOrWhiteSpace(cotizacion.PaisImpuestoVenta)
                    ? "Chile"
                    : cotizacion.PaisImpuestoVenta;

                cmbPaisImpuestoVenta.SelectedItem = cmbPaisImpuestoVenta.Items.Contains(pais)
                    ? pais
                    : "Manual";
            }

            txtTasaImpuestoVenta.Text = (cotizacion.TasaImpuestoVenta * 100.0).ToString("0.##");

            CargarTablaCostosExtra();

            cargandoCostos = false;
        }

        private void AplicarCostosDesdePantalla()
        {
            if (cargandoCostos)
            {
                return;
            }

            cotizacion.TasaImprevistos = ConvertirTasa(txtTasaImprevistos.Text);

            RefrescarCalculosYVista();
        }

        private void BtnAplicarCostos_Click(object sender, EventArgs e)
        {
            AplicarCostosDesdePantalla();
        }

        private void CampoCostos_Leave(object sender, EventArgs e)
        {
            AplicarCostosDesdePantalla();
        }
    }
}
