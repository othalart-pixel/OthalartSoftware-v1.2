using System;
using System.Drawing;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public class CatalogoEtapaControl : UserControl
    {
        private readonly CatalogoEtapaVisual etapa;
        private readonly bool modoOscuro;
        private readonly FlowLayoutPanel contenido = new FlowLayoutPanel();
        private readonly Label indicador = new Label();
        private readonly Label titulo = new Label();
        private bool expandida = true;

        public CatalogoEtapaControl(
            CatalogoEtapaVisual etapa,
            bool modoOscuro,
            Action<CatalogoProcesoVisual> editar,
            Action<CatalogoProcesoVisual> abrirPipeline,
            Action<CatalogoProcesoVisual> abrirEcuacion
        )
        {
            this.etapa = etapa;
            this.modoOscuro = modoOscuro;
            Dock = DockStyle.Top;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Width = 820;
            MinimumSize = new Size(420, 0);
            Margin = new Padding(0, 0, 0, 12);
            BackColor = modoOscuro ? Color.FromArgb(37, 37, 38) : Color.FromArgb(246, 247, 249);
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(0);

            Construir(editar, abrirPipeline, abrirEcuacion);
        }

        private void Construir(
            Action<CatalogoProcesoVisual> editar,
            Action<CatalogoProcesoVisual> abrirPipeline,
            Action<CatalogoProcesoVisual> abrirEcuacion
        )
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Top;
            root.AutoSize = true;
            root.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            root.ColumnCount = 1;
            root.RowCount = 2;
            root.Margin = new Padding(0);

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 38;
            header.BackColor = ObtenerColorEtapa(etapa.Nombre, modoOscuro);
            header.Cursor = Cursors.Hand;
            header.Click += (s, e) => Alternar();

            indicador.Text = "v";
            indicador.AutoSize = false;
            indicador.Width = 24;
            indicador.Height = 28;
            indicador.Location = new Point(10, 6);
            indicador.TextAlign = ContentAlignment.MiddleCenter;
            indicador.ForeColor = Color.White;
            indicador.Font = new Font("Segoe UI", 9.0f, FontStyle.Bold);
            indicador.Click += (s, e) => Alternar();

            titulo.Text = etapa.Nombre + " - " + etapa.Procesos.Count + " procesos";
            titulo.AutoEllipsis = true;
            titulo.Location = new Point(38, 8);
            titulo.Width = 620;
            titulo.Height = 22;
            titulo.ForeColor = Color.White;
            titulo.Font = new Font("Segoe UI", 10.0f, FontStyle.Bold);
            titulo.Click += (s, e) => Alternar();

            header.Controls.Add(indicador);
            header.Controls.Add(titulo);

            contenido.Dock = DockStyle.Top;
            contenido.AutoSize = true;
            contenido.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            contenido.FlowDirection = FlowDirection.TopDown;
            contenido.WrapContents = false;
            contenido.Padding = new Padding(10);
            contenido.BackColor = modoOscuro ? Color.FromArgb(37, 37, 38) : Color.FromArgb(250, 250, 251);

            if (etapa.Procesos.Count == 0)
            {
                Label vacio = new Label();
                vacio.Text = "No existen procesos configurados para esta etapa";
                vacio.AutoSize = true;
                vacio.Font = new Font("Segoe UI", 9.2f, FontStyle.Italic);
                vacio.ForeColor = modoOscuro ? Color.FromArgb(185, 185, 185) : Color.FromArgb(90, 90, 90);
                vacio.Margin = new Padding(0, 0, 0, 4);
                contenido.Controls.Add(vacio);
            }
            else
            {
                foreach (CatalogoProcesoVisual proceso in etapa.Procesos)
                {
                    CatalogoProcesoControl tarjeta = new CatalogoProcesoControl(
                        proceso,
                        modoOscuro,
                        editar,
                        abrirPipeline,
                        abrirEcuacion
                    );
                    tarjeta.Width = Math.Max(360, Width - 28);
                    contenido.Controls.Add(tarjeta);
                }
            }

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(contenido, 0, 1);
            Controls.Add(root);
            Resize += (s, e) => AjustarAnchos();
            AjustarAnchos();
        }

        private void Alternar()
        {
            expandida = !expandida;
            contenido.Visible = expandida;
            indicador.Text = expandida ? "v" : ">";
        }

        private static Color ObtenerColorEtapa(string etapa, bool modoOscuro)
        {
            string normalizada = (etapa ?? "").ToLowerInvariant();
            if (normalizada.Contains("desarrollo"))
            {
                return modoOscuro ? Color.FromArgb(40, 115, 72) : Color.FromArgb(34, 145, 85);
            }

            if (normalizada.Contains("pre"))
            {
                return modoOscuro ? Color.FromArgb(145, 104, 22) : Color.FromArgb(214, 142, 26);
            }

            if (normalizada.Contains("post"))
            {
                return modoOscuro ? Color.FromArgb(42, 102, 168) : Color.FromArgb(42, 128, 214);
            }

            return modoOscuro ? Color.FromArgb(145, 54, 54) : Color.FromArgb(212, 75, 75);
        }

        private void AjustarAnchos()
        {
            titulo.Width = Math.Max(160, ClientSize.Width - 52);
            foreach (Control control in contenido.Controls)
            {
                if (control is CatalogoProcesoControl)
                {
                    control.Width = Math.Max(360, ClientSize.Width - 28);
                }
            }
        }
    }
}
