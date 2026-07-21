using System;
using System.Drawing;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public class CatalogoProcesoControl : UserControl
    {
        private readonly CatalogoProcesoVisual proceso;
        private readonly Action<CatalogoProcesoVisual> editar;
        private readonly Action<CatalogoProcesoVisual> abrirPipeline;
        private readonly Action<CatalogoProcesoVisual> abrirEcuacion;
        private readonly ToolTip tooltip = new ToolTip();

        public CatalogoProcesoControl(
            CatalogoProcesoVisual proceso,
            bool modoOscuro,
            Action<CatalogoProcesoVisual> editar,
            Action<CatalogoProcesoVisual> abrirPipeline,
            Action<CatalogoProcesoVisual> abrirEcuacion
        )
        {
            this.proceso = proceso;
            this.editar = editar;
            this.abrirPipeline = abrirPipeline;
            this.abrirEcuacion = abrirEcuacion;

            Dock = DockStyle.Top;
            Height = 94;
            Margin = new Padding(0, 0, 0, 8);
            Padding = new Padding(10);
            BackColor = modoOscuro ? Color.FromArgb(45, 45, 48) : Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Cursor = Cursors.Hand;

            Construir(modoOscuro);
            DoubleClick += (s, e) => this.editar?.Invoke(this.proceso);
        }

        private void Construir(bool modoOscuro)
        {
            Color texto = modoOscuro ? Color.FromArgb(235, 235, 235) : Color.FromArgb(28, 32, 38);
            Color secundario = modoOscuro ? Color.FromArgb(190, 190, 190) : Color.FromArgb(88, 96, 110);
            Color alerta = modoOscuro ? Color.FromArgb(255, 205, 115) : Color.FromArgb(157, 95, 0);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            layout.Margin = new Padding(0);

            TableLayoutPanel contenido = new TableLayoutPanel();
            contenido.Dock = DockStyle.Fill;
            contenido.ColumnCount = 1;
            contenido.RowCount = 4;
            contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label nombre = CrearLabel(proceso.Nombre, 10.0f, FontStyle.Bold, texto);
            Label trabajo = CrearLabel(
                string.IsNullOrWhiteSpace(proceso.Proceso) ? "Sin proceso definido" : proceso.Proceso,
                8.8f,
                FontStyle.Regular,
                secundario
            );
            Label cargos = CrearLabel(
                string.IsNullOrWhiteSpace(proceso.Cargos) ? "Sin cargos asignados" : proceso.Cargos,
                8.8f,
                FontStyle.Regular,
                secundario
            );
            Label ecuacion = CrearLabel(
                string.IsNullOrWhiteSpace(proceso.Ecuacion) ? "Sin ecuacion vinculada" : proceso.Ecuacion,
                8.8f,
                proceso.TieneDiagnostico ? FontStyle.Bold : FontStyle.Regular,
                proceso.TieneDiagnostico ? alerta : secundario
            );

            contenido.Controls.Add(nombre, 0, 0);
            contenido.Controls.Add(trabajo, 0, 1);
            contenido.Controls.Add(cargos, 0, 2);
            contenido.Controls.Add(ecuacion, 0, 3);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Fill;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = true;
            acciones.Margin = new Padding(8, 0, 0, 0);

            acciones.Controls.Add(CrearBoton("Editar", () => editar?.Invoke(proceso), 66));
            acciones.Controls.Add(CrearBoton("Pipeline", () => abrirPipeline?.Invoke(proceso), 76));
            acciones.Controls.Add(CrearBoton("Ecuacion", () => abrirEcuacion?.Invoke(proceso), 76));

            layout.Controls.Add(contenido, 0, 0);
            layout.Controls.Add(acciones, 1, 0);
            Controls.Add(layout);

            tooltip.SetToolTip(this, CrearTooltipProceso());
            tooltip.SetToolTip(nombre, proceso.Nombre);
            tooltip.SetToolTip(trabajo, string.IsNullOrWhiteSpace(proceso.Proceso) ? "Sin proceso definido" : proceso.Proceso);
            tooltip.SetToolTip(cargos, string.IsNullOrWhiteSpace(proceso.Cargos) ? "Sin cargos asignados" : proceso.Cargos);
            tooltip.SetToolTip(ecuacion, string.IsNullOrWhiteSpace(proceso.Ecuacion) ? "Sin ecuacion vinculada" : proceso.Ecuacion);
        }

        private Label CrearLabel(string texto, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = texto;
            label.AutoEllipsis = true;
            label.Dock = DockStyle.Top;
            label.Height = 18;
            label.Font = new Font("Segoe UI", size, style);
            label.ForeColor = color;
            label.Margin = new Padding(0, 0, 0, 2);
            return label;
        }

        private Button CrearBoton(string texto, Action accion, int ancho)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = ancho;
            boton.Height = 28;
            boton.Margin = new Padding(0, 0, 6, 6);
            boton.FlatStyle = FlatStyle.Flat;
            boton.Font = new Font("Segoe UI", 8.4f, FontStyle.Bold);
            boton.Click += (s, e) => accion();
            return boton;
        }

        private string CrearTooltipProceso()
        {
            return proceso.Nombre + Environment.NewLine +
                "Proceso: " + (string.IsNullOrWhiteSpace(proceso.Proceso) ? "Sin proceso definido" : proceso.Proceso) + Environment.NewLine +
                "Cargos: " + (string.IsNullOrWhiteSpace(proceso.Cargos) ? "Sin cargos asignados" : proceso.Cargos) + Environment.NewLine +
                "Ecuacion: " + (string.IsNullOrWhiteSpace(proceso.Ecuacion) ? "Sin ecuacion vinculada" : proceso.Ecuacion) + Environment.NewLine +
                "Diagnostico: " + (string.IsNullOrWhiteSpace(proceso.Diagnostico) ? "OK" : proceso.Diagnostico);
        }
    }
}
