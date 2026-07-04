using System;
using System.Drawing;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabResultados(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 1;
            layout.RowCount = 4;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

            TableLayoutPanel panelSuperior = new TableLayoutPanel();
            panelSuperior.Dock = DockStyle.Fill;
            panelSuperior.ColumnCount = 8;
            panelSuperior.RowCount = 4;

            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            panelSuperior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panelSuperior.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            Label titulo = new Label();
            titulo.Text = "Análisis de precio y rentabilidad";
            titulo.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titulo.Dock = DockStyle.Fill;
            titulo.TextAlign = ContentAlignment.MiddleLeft;

            panelSuperior.Controls.Add(titulo, 0, 0);
            panelSuperior.SetColumnSpan(titulo, 8);

            rbModoMargen.Text = "Calcular por margen objetivo";
            rbModoMargen.Checked = true;
            rbModoMargen.AutoSize = true;
            rbModoMargen.CheckedChanged += ModoResultados_CheckedChanged;

            rbModoPrecio.Text = "Calcular desde precio cliente";
            rbModoPrecio.AutoSize = true;
            rbModoPrecio.CheckedChanged += ModoResultados_CheckedChanged;

            panelSuperior.Controls.Add(rbModoMargen, 0, 1);
            panelSuperior.SetColumnSpan(rbModoMargen, 2);
            panelSuperior.Controls.Add(rbModoPrecio, 2, 1);
            panelSuperior.SetColumnSpan(rbModoPrecio, 2);

            Label lblMargen = new Label();
            lblMargen.Text = "Margen objetivo (%):";
            lblMargen.Dock = DockStyle.Fill;
            lblMargen.TextAlign = ContentAlignment.MiddleLeft;

            txtMargenObjetivo.Dock = DockStyle.Fill;
            txtMargenObjetivo.Leave += CampoResultados_Leave;

            Label lblPrecio = new Label();
            lblPrecio.Text = "Precio cliente/acordado:";
            lblPrecio.Dock = DockStyle.Fill;
            lblPrecio.TextAlign = ContentAlignment.MiddleLeft;

            txtPrecioEvaluado.Dock = DockStyle.Fill;
            txtPrecioEvaluado.TextChanged += TextBoxMoneda_TextChanged;
            txtPrecioEvaluado.Leave += CampoResultados_Leave;

            Label lblPaisImpuesto = new Label();
            lblPaisImpuesto.Text = "País impuesto venta:";
            lblPaisImpuesto.Dock = DockStyle.Fill;
            lblPaisImpuesto.TextAlign = ContentAlignment.MiddleLeft;

            cmbPaisImpuestoVenta.Dock = DockStyle.Fill;
            cmbPaisImpuestoVenta.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaisImpuestoVenta.Items.Clear();
            cmbPaisImpuestoVenta.Items.AddRange(new object[]
            {
                "Sin impuesto",
                "Chile",
                "USA",
                "Mexico",
                "Espana",
                "Argentina",
                "Colombia",
                "Peru",
                "Uruguay",
                "Manual"
            });
            cmbPaisImpuestoVenta.SelectedIndexChanged += PaisImpuestoVenta_SelectedIndexChanged;

            Label lblTasaImpuesto = new Label();
            lblTasaImpuesto.Text = "Impuesto venta (%):";
            lblTasaImpuesto.Dock = DockStyle.Fill;
            lblTasaImpuesto.TextAlign = ContentAlignment.MiddleLeft;

            txtTasaImpuestoVenta.Dock = DockStyle.Fill;
            txtTasaImpuestoVenta.Leave += CampoResultados_Leave;

            Button btnAplicarResultados = new Button();
            btnAplicarResultados.Text = "Aplicar resultados";
            btnAplicarResultados.Width = 150;
            btnAplicarResultados.Height = 30;
            btnAplicarResultados.Click += BtnAplicarResultados_Click;

            panelSuperior.Controls.Add(lblMargen, 0, 2);
            panelSuperior.Controls.Add(txtMargenObjetivo, 1, 2);
            panelSuperior.Controls.Add(lblPrecio, 2, 2);
            panelSuperior.Controls.Add(txtPrecioEvaluado, 3, 2);
            panelSuperior.Controls.Add(btnAplicarResultados, 4, 2);
            panelSuperior.Controls.Add(lblPaisImpuesto, 0, 3);
            panelSuperior.Controls.Add(cmbPaisImpuestoVenta, 1, 3);
            panelSuperior.Controls.Add(lblTasaImpuesto, 2, 3);
            panelSuperior.Controls.Add(txtTasaImpuestoVenta, 3, 3);

            panelGraficoMargen.Dock = DockStyle.Fill;
            panelGraficoMargen.BackColor = Color.White;
            panelGraficoMargen.Paint += PanelGraficoMargen_Paint;

            dgvAnalisisMargen.Dock = DockStyle.Fill;
            dgvAnalisisMargen.AllowUserToAddRows = false;
            dgvAnalisisMargen.AllowUserToDeleteRows = false;
            dgvAnalisisMargen.ReadOnly = true;
            dgvAnalisisMargen.RowHeadersVisible = false;
            dgvAnalisisMargen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvAnalisisMargen.Columns.Clear();
            dgvAnalisisMargen.Columns.Add("Margen", "Margen");
            dgvAnalisisMargen.Columns.Add("PrecioVenta", "Precio venta");
            dgvAnalisisMargen.Columns.Add("Utilidad", "Utilidad");
            dgvAnalisisMargen.Columns.Add("Markup", "Markup");

            lblAnalisisMargen.Dock = DockStyle.Fill;
            lblAnalisisMargen.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblAnalisisMargen.Padding = new Padding(0, 10, 0, 0);

            layout.Controls.Add(panelSuperior, 0, 0);
            layout.Controls.Add(panelGraficoMargen, 0, 1);
            layout.Controls.Add(dgvAnalisisMargen, 0, 2);
            layout.Controls.Add(lblAnalisisMargen, 0, 3);

            tab.Controls.Add(layout);

            ActualizarModoResultados();
        }

        private void BtnAplicarResultados_Click(object sender, EventArgs e)
        {
            AplicarResultadosDesdePantalla();
        }

        private void CampoResultados_Leave(object sender, EventArgs e)
        {
            AplicarResultadosDesdePantalla();
        }

        private void ModoResultados_CheckedChanged(object sender, EventArgs e)
        {
            ActualizarModoResultados();
        }

        private void ActualizarModoResultados()
        {
            if (txtMargenObjetivo == null || txtPrecioEvaluado == null)
            {
                return;
            }

            txtMargenObjetivo.Enabled = rbModoMargen.Checked;
            txtPrecioEvaluado.Enabled = rbModoPrecio.Checked;
        }

        private void PaisImpuestoVenta_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cargandoCostos)
            {
                return;
            }

            string pais = Convert.ToString(cmbPaisImpuestoVenta.SelectedItem) ?? "Chile";
            cotizacion.PaisImpuestoVenta = pais;

            if (pais != "Manual")
            {
                cotizacion.TasaImpuestoVenta = ObtenerTasaImpuestoVentaPais(pais);
                txtTasaImpuestoVenta.Text = (cotizacion.TasaImpuestoVenta * 100.0).ToString("0.##");
            }

            AplicarResultadosDesdePantalla();
        }

        private void AplicarResultadosDesdePantalla()
        {
            if (cargandoCostos)
            {
                return;
            }

            ServicioCotizacion.RecalcularCotizacion(cotizacion);

            double costoTotal = cotizacion.CostoTotal;

            cotizacion.PaisImpuestoVenta =
                Convert.ToString(cmbPaisImpuestoVenta.SelectedItem) ?? cotizacion.PaisImpuestoVenta;
            cotizacion.TasaImpuestoVenta = ConvertirTasa(txtTasaImpuestoVenta.Text);

            if (rbModoMargen.Checked)
            {
                cotizacion.MargenObjetivo = ConvertirTasa(txtMargenObjetivo.Text);

                if (cotizacion.MargenObjetivo >= 0.95)
                {
                    cotizacion.MargenObjetivo = 0.95;
                }

                cotizacion.PrecioRecomendado = CalcularPrecioPorMargen(
                    costoTotal,
                    cotizacion.MargenObjetivo
                );

                cotizacion.PrecioVentaEvaluado = cotizacion.PrecioRecomendado;
            }
            else
            {
                double precioManual = ConvertirDouble(txtPrecioEvaluado.Text);

                if (precioManual <= 0.0)
                {
                    precioManual = cotizacion.PrecioRecomendado;
                }

                cotizacion.PrecioVentaEvaluado = precioManual;

                if (precioManual > 0.0)
                {
                    cotizacion.MargenObjetivo = (precioManual - costoTotal) / precioManual;
                }
                else
                {
                    cotizacion.MargenObjetivo = 0.0;
                }
            }

            ServicioCotizacion.RecalcularPrecioEvaluado(cotizacion);

            txtMargenObjetivo.Text = (cotizacion.MargenObjetivo * 100.0).ToString("0.##");
            txtPrecioEvaluado.Text = cotizacion.PrecioVentaEvaluado.ToString("0.##");
            txtTasaImpuestoVenta.Text = (cotizacion.TasaImpuestoVenta * 100.0).ToString("0.##");

            CargarTablaEtapas();
            CargarTablaManoObra();
            CargarCostosEnPantalla();
            RefrescarResumen();
            RefrescarResultadosDetalle();
            RefrescarAnalisisMargen();
            panelGantt.Invalidate();
        }

        private double CalcularPrecioPorMargen(double costoTotal, double margen)
        {
            if (margen >= 0.99)
            {
                margen = 0.99;
            }

            if (costoTotal <= 0.0)
            {
                return 0.0;
            }

            return costoTotal / (1.0 - margen);
        }

        private double ObtenerTasaImpuestoVentaPais(string pais)
        {
            switch ((pais ?? "").Trim())
            {
                case "Chile":
                    return 0.19;
                case "Mexico":
                    return 0.16;
                case "Espana":
                    return 0.21;
                case "Argentina":
                    return 0.21;
                case "Colombia":
                    return 0.19;
                case "Peru":
                    return 0.18;
                case "Uruguay":
                    return 0.22;
                case "USA":
                case "Sin impuesto":
                    return 0.0;
                default:
                    return cotizacion.TasaImpuestoVenta;
            }
        }

        private void CargarTablaAnalisisMargen()
        {
            if (dgvAnalisisMargen.Columns.Count == 0)
            {
                return;
            }

            dgvAnalisisMargen.Rows.Clear();

            double costoTotal = cotizacion.CostoTotal;

            for (int margenPorcentaje = 0; margenPorcentaje <= 30; margenPorcentaje += 5)
            {
                double margen = margenPorcentaje / 100.0;
                double precioVenta = CalcularPrecioPorMargen(costoTotal, margen);
                double utilidad = precioVenta - costoTotal;

                double markup = 0.0;

                if (costoTotal > 0.0)
                {
                    markup = utilidad / costoTotal;
                }

                int rowIndex = dgvAnalisisMargen.Rows.Add();
                DataGridViewRow row = dgvAnalisisMargen.Rows[rowIndex];

                row.Cells["Margen"].Value = margenPorcentaje + "%";
                row.Cells["PrecioVenta"].Value = FormatearValorVisual(precioVenta);
                row.Cells["Utilidad"].Value = FormatearValorVisual(utilidad);
                row.Cells["Markup"].Value = (markup * 100.0).ToString("0.00") + "%";

                if (Math.Abs(margen - cotizacion.MargenObjetivo) < 0.001)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(230, 250, 245);
                }
            }
        }

        private void RefrescarAnalisisMargen()
        {
            CargarTablaAnalisisMargen();

            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion;

            double costoTotal = cotizacion.CostoTotal;
            double precioEvaluado = cotizacion.PrecioVentaEvaluado;
            double utilidad = precioEvaluado - costoTotal;

            double margenReal = 0.0;
            double markup = 0.0;

            if (precioEvaluado > 0.0)
            {
                margenReal = utilidad / precioEvaluado;
            }

            if (costoTotal > 0.0)
            {
                markup = utilidad / costoTotal;
            }

            lblAnalisisMargen.Text =
                $"Moneda visual activa: {monedaVisual}    |    " +
                $"Costo total: {FormatearValorVisual(costoTotal)}    |    " +
                $"Precio neto: {FormatearValorVisual(precioEvaluado)}    |    " +
                $"Impuesto venta: {FormatearValorVisual(cotizacion.ImpuestoVenta)}    |    " +
                $"Total cliente: {FormatearValorVisual(cotizacion.PrecioVentaConImpuesto)}    |    " +
                $"Margen real: {margenReal * 100.0:0.00}%    |    " +
                $"Utilidad: {FormatearValorVisual(utilidad)}    |    " +
                $"Markup: {markup * 100.0:0.00}%";

            panelGraficoMargen.Invalidate();
        }

        private void RefrescarResultadosDetalle()
        {
            string monedaVisual = string.IsNullOrWhiteSpace(cotizacion.MonedaVisualizacion)
                ? "CLP"
                : cotizacion.MonedaVisualizacion;

            lblResultadosDetalle.Text =
                $"RESULTADOS ACTUALES EN {monedaVisual}\n\n" +

                $"Costo mano de obra: {FormatearValorVisual(cotizacion.CostoManoObraEtapas)}\n" +
                $"Producción interna: {FormatearValorVisual(cotizacion.CostoProduccionInterna)}\n" +
                $"Administración: {FormatearValorVisual(cotizacion.CostoAdministrativo)}\n" +
                $"Servicios tercerizados: {FormatearValorVisual(cotizacion.CostoTercerizados)}\n" +
                $"Otros costos: {FormatearValorVisual(cotizacion.OtrosCostos)}\n\n" +

                $"Costo base: {FormatearValorVisual(cotizacion.CostoBase)}\n" +
                $"Imprevistos: {FormatearValorVisual(cotizacion.Imprevistos)}\n" +
                $"Costo total: {FormatearValorVisual(cotizacion.CostoTotal)}\n\n" +

                $"Margen objetivo: {cotizacion.MargenObjetivo * 100.0:0.00}%\n" +
                $"Precio recomendado neto: {FormatearValorVisual(cotizacion.PrecioRecomendado)}\n" +
                $"Precio evaluado neto: {FormatearValorVisual(cotizacion.PrecioVentaEvaluado)}\n" +
                $"País impuesto venta: {cotizacion.PaisImpuestoVenta}\n" +
                $"Tasa impuesto venta: {cotizacion.TasaImpuestoVenta * 100.0:0.00}%\n" +
                $"Impuesto venta: {FormatearValorVisual(cotizacion.ImpuestoVenta)}\n" +
                $"Total cliente con impuesto: {FormatearValorVisual(cotizacion.PrecioVentaConImpuesto)}\n" +
                $"Utilidad estimada: {FormatearValorVisual(cotizacion.UtilidadEvaluada)}\n" +
                $"Margen evaluado: {cotizacion.MargenEvaluado * 100.0:0.00}%\n" +
                $"Markup: {cotizacion.MarkupEvaluado * 100.0:0.00}%";

            RefrescarAnalisisMargen();
        }
    }
}
