using System;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Reports;
using Cotizador_animacion_Othalart.Services;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabInforme(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(15);
            layout.ColumnCount = 1;
            layout.RowCount = 2;

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            FlowLayoutPanel barra = new FlowLayoutPanel();
            barra.Dock = DockStyle.Fill;
            barra.FlowDirection = FlowDirection.LeftToRight;

            Label titulo = new Label();
            titulo.Text = "Informe global del proyecto";
            titulo.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 10, 25, 0);

            btnActualizarInforme.Text = "Actualizar informe";
            btnActualizarInforme.Width = 150;
            btnActualizarInforme.Height = 32;
            btnActualizarInforme.Click += BtnActualizarInforme_Click;

            barra.Controls.Add(titulo);
            barra.Controls.Add(btnActualizarInforme);

            webInformeCliente.Dock = DockStyle.Fill;
            webInformeCliente.ScriptErrorsSuppressed = true;

            layout.Controls.Add(barra, 0, 0);
            layout.Controls.Add(webInformeCliente, 0, 1);

            tab.Controls.Add(layout);
            tab.Enter += (s, e) => ActualizarInformeCliente();
        }

        private void BtnActualizarInforme_Click(object sender, EventArgs e)
        {
            ActualizarInformeCliente();
        }

        private void ActualizarInformeCliente()
        {
            AplicarDatosDesdePantalla();
            RecalcularCostosExtra();
            if (cotizacion != null)
            {
                ServicioCotizacion.RecalcularCotizacion(cotizacion);
            }

            string html = GenerarHtmlInformeCliente();
            webInformeCliente.DocumentText = html;
        }

        private string GenerarHtmlInformeCliente()
        {
            if (proyectoCotizacionActual != null)
            {
                if (cotizacion != null)
                {
                    cotizacion.ProyectoProductivo = proyectoCotizacionActual;
                }

                return InformeProyectoBuilder.GenerarHtml(
                    proyectoCotizacionActual,
                    bibliotecaPersonalProyectoInforme ??
                        BibliotecaPersonalEmpresaJsonService.CargarPersonal(),
                    bibliotecaCargosProyectoInforme ??
                        BibliotecaCargosJsonService.CargarCargos()
                );
            }

            return InformeClienteBuilder.GenerarHtml(cotizacion);
        }
    }
}
