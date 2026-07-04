using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;
using Cotizador_animacion_Othalart.Services;
using System.Collections.Generic;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private const string ColEtapaUsar = "ColUsar";
        private const string ColEtapaNombre = "ColEtapa";
        private const string ColEtapaDuracion = "ColDuracion";
        private const string ColEtapaInicio = "ColInicio";
        private const string ColEtapaFin = "ColFin";
        private const string ColEtapaEstado = "ColEstado";
        private const string ColEtapaCosto = "ColCosto";


        private const double SemanasPorMesPlanificacion = 4.0;

        private double ConvertirSemanaInicioAMesProporcional(double semana)
        {
            if (semana < 1.0)
            {
                semana = 1.0;
            }

            // S1 corresponde al mes 0.
            // S5 corresponde al mes 1.
            // S9 corresponde al mes 2.
            return (semana - 1.0) / SemanasPorMesPlanificacion;
        }

        private void NormalizarInicioEtapaDesdeSubEtapasActivas(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return;
            }

            var subEtapasActivas = bibliotecaSubEtapas
                .Where(s =>
                    s != null &&
                    s.Activa &&
                    NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .ToList();

            if (subEtapasActivas.Count == 0)
            {
                etapa.Seleccionada = false;
                etapa.InicioMes = 0.0;
                etapa.DuracionMeses = 0.0;
                etapa.FinMes = 0.0;
                return;
            }

            etapa.Seleccionada = true;

            double semanaInicioMenor = subEtapasActivas
                .Select(s => s.InicioSemana < 1.0 ? 1.0 : s.InicioSemana)
                .Min();

            double nuevoInicioMes = ConvertirSemanaInicioAMesProporcional(semanaInicioMenor);

            double finAnterior = etapa.FinMes;

            if (finAnterior <= 0.0)
            {
                finAnterior = etapa.InicioMes + etapa.DuracionMeses;
            }

            if (finAnterior <= nuevoInicioMes)
            {
                double duracionBase = etapa.DuracionMeses;

                if (duracionBase <= 0.0)
                {
                    duracionBase = 1.0;
                }

                finAnterior = nuevoInicioMes + duracionBase;
            }

            etapa.InicioMes = nuevoInicioMes;
            etapa.FinMes = finAnterior;
            etapa.DuracionMeses = Math.Max(0.0, etapa.FinMes - etapa.InicioMes);
        }

        private void NormalizarIniciosTodasLasEtapasDesdeSubEtapasActivas()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas)
            {
                NormalizarInicioEtapaDesdeSubEtapasActivas(etapa);
            }
        }

        private bool refrescoEtapasPendiente = false;
        private readonly HashSet<string> etapasExpandidasEnTabla = new HashSet<string>();

        private void ConstruirTabEtapas(TabPage tab)
        {
            tab.Controls.Clear();

            DesconectarEventosEtapas();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            dgvEtapas.Dock = DockStyle.Fill;
            dgvEtapas.AllowUserToAddRows = false;
            dgvEtapas.AllowUserToDeleteRows = false;
            dgvEtapas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEtapas.RowHeadersVisible = false;
            dgvEtapas.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dgvEtapas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvEtapas.MultiSelect = false;
            dgvEtapas.AllowUserToResizeRows = false;

            ReconstruirColumnasEtapas();

            ConectarEventosEtapas();

            layout.Controls.Add(ConstruirEditorBibliotecaEtapas(), 0, 0);
            layout.Controls.Add(dgvEtapas, 0, 1);

            tab.Controls.Add(layout);
            CargarBibliotecaEtapasEnPantalla();
        }

        private Control ConstruirEditorBibliotecaEtapas()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(8, 8, 8, 6);

            Label titulo = new Label();
            titulo.Text = "Biblioteca editable de etapas";
            titulo.Dock = DockStyle.Top;
            titulo.Height = 26;
            titulo.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.Dock = DockStyle.Bottom;
            acciones.Height = 38;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = false;
            acciones.Padding = new Padding(0, 5, 0, 0);

            btnGuardarBibliotecaEtapas.Text = "Guardar etapas";
            btnGuardarBibliotecaEtapas.Width = 150;
            btnGuardarBibliotecaEtapas.Height = 28;
            btnGuardarBibliotecaEtapas.Click -= BtnGuardarBibliotecaEtapas_Click;
            btnGuardarBibliotecaEtapas.Click += BtnGuardarBibliotecaEtapas_Click;

            btnRestaurarBibliotecaEtapas.Text = "Restaurar base";
            btnRestaurarBibliotecaEtapas.Width = 130;
            btnRestaurarBibliotecaEtapas.Height = 28;
            btnRestaurarBibliotecaEtapas.Click -= BtnRestaurarBibliotecaEtapas_Click;
            btnRestaurarBibliotecaEtapas.Click += BtnRestaurarBibliotecaEtapas_Click;

            acciones.Controls.Add(btnGuardarBibliotecaEtapas);
            acciones.Controls.Add(btnRestaurarBibliotecaEtapas);

            ConfigurarGrillaBibliotecaEtapas();

            panel.Controls.Add(dgvBibliotecaEtapas);
            panel.Controls.Add(acciones);
            panel.Controls.Add(titulo);

            return panel;
        }

        private void ConfigurarGrillaBibliotecaEtapas()
        {
            dgvBibliotecaEtapas.Dock = DockStyle.Fill;
            dgvBibliotecaEtapas.AllowUserToAddRows = false;
            dgvBibliotecaEtapas.AllowUserToDeleteRows = false;
            dgvBibliotecaEtapas.RowHeadersVisible = false;
            dgvBibliotecaEtapas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvBibliotecaEtapas.MultiSelect = false;
            dgvBibliotecaEtapas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBibliotecaEtapas.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dgvBibliotecaEtapas.Columns.Clear();

            dgvBibliotecaEtapas.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Activa",
                HeaderText = "Activa",
                FillWeight = 55
            });

            dgvBibliotecaEtapas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Orden",
                HeaderText = "Orden",
                FillWeight = 60
            });

            dgvBibliotecaEtapas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Clave",
                HeaderText = "Clave interna",
                ReadOnly = true,
                FillWeight = 110
            });

            dgvBibliotecaEtapas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "Nombre visible",
                FillWeight = 130
            });

            dgvBibliotecaEtapas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Color",
                HeaderText = "Color",
                FillWeight = 80
            });

            dgvBibliotecaEtapas.CellFormatting -= DgvBibliotecaEtapas_CellFormatting;
            dgvBibliotecaEtapas.CellFormatting += DgvBibliotecaEtapas_CellFormatting;
        }

        private void CargarBibliotecaEtapasEnPantalla()
        {
            if (bibliotecaEtapas == null || bibliotecaEtapas.Count == 0)
            {
                bibliotecaEtapas = BibliotecaEtapasJsonService.CargarEtapas();
            }

            dgvBibliotecaEtapas.Rows.Clear();

            foreach (EtapaDefinicion etapa in bibliotecaEtapas.OrderBy(e => e.Orden))
            {
                int index = dgvBibliotecaEtapas.Rows.Add();
                DataGridViewRow row = dgvBibliotecaEtapas.Rows[index];

                row.Tag = etapa;
                row.Cells["Activa"].Value = etapa.Activa;
                row.Cells["Orden"].Value = etapa.Orden;
                row.Cells["Clave"].Value = etapa.Clave;
                row.Cells["Nombre"].Value = etapa.Nombre;
                row.Cells["Color"].Value = ConvertirColorArgbAHex(etapa.ColorArgb);
            }
        }

        private void DgvBibliotecaEtapas_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvBibliotecaEtapas.Columns[e.ColumnIndex].Name != "Color")
            {
                return;
            }

            Color color = ParsearColorEtapa(Convert.ToString(e.Value), Color.Gray);
            e.CellStyle.BackColor = color;
            e.CellStyle.ForeColor = ObtenerColorTextoLegible(color);
            e.CellStyle.SelectionBackColor = color;
            e.CellStyle.SelectionForeColor = ObtenerColorTextoLegible(color);
        }

        private void BtnGuardarBibliotecaEtapas_Click(object sender, EventArgs e)
        {
            dgvBibliotecaEtapas.EndEdit();

            List<EtapaDefinicion> etapas = LeerBibliotecaEtapasDesdePantalla();

            if (etapas.Count == 0)
            {
                MessageBox.Show(
                    "Debe existir al menos una etapa activa.",
                    "Etapas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            List<EtapaDefinicion> etapasAnteriores = bibliotecaEtapas ?? new List<EtapaDefinicion>();

            bibliotecaEtapas = etapas;
            BibliotecaEtapasJsonService.GuardarEtapas(bibliotecaEtapas);
            AplicarBibliotecaEtapasACotizacion(etapasAnteriores);
            CargarBibliotecaEtapasEnPantalla();
            RefrescarDespuesDeCambiarBibliotecaEtapas();
        }

        private void BtnRestaurarBibliotecaEtapas_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show(
                "Esto restaurará Desarrollo, Preproducción, Producción y Postproducción en etapas.json.",
                "Restaurar etapas",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            BibliotecaEtapasJsonService.RegenerarEtapasBase();
            bibliotecaEtapas = BibliotecaEtapasJsonService.CargarEtapas();
            AplicarBibliotecaEtapasACotizacion();
            CargarBibliotecaEtapasEnPantalla();
            RefrescarDespuesDeCambiarBibliotecaEtapas();
        }

        private List<EtapaDefinicion> LeerBibliotecaEtapasDesdePantalla()
        {
            List<EtapaDefinicion> etapas = new List<EtapaDefinicion>();

            foreach (DataGridViewRow row in dgvBibliotecaEtapas.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string clave = Convert.ToString(row.Cells["Clave"].Value) ?? "";
                string nombre = Convert.ToString(row.Cells["Nombre"].Value) ?? "";
                string colorTexto = Convert.ToString(row.Cells["Color"].Value) ?? "";
                int orden = ParsearEntero(row.Cells["Orden"].Value, etapas.Count * 10 + 10);
                bool activa = Convert.ToBoolean(row.Cells["Activa"].Value ?? false);

                if (string.IsNullOrWhiteSpace(clave))
                {
                    continue;
                }

                etapas.Add(new EtapaDefinicion
                {
                    Clave = clave.Trim(),
                    Nombre = string.IsNullOrWhiteSpace(nombre) ? clave.Trim() : nombre.Trim(),
                    Orden = orden,
                    Activa = activa,
                    ColorArgb = ParsearColorEtapa(colorTexto, Color.Gray).ToArgb()
                });
            }

            return etapas;
        }

        private void AplicarBibliotecaEtapasACotizacion(
            List<EtapaDefinicion> etapasAnteriores = null
        )
        {
            if (cotizacion == null)
            {
                return;
            }

            List<EtapaProyecto> existentes = cotizacion.Etapas ?? new List<EtapaProyecto>();
            List<EtapaProyecto> nuevasEtapas = new List<EtapaProyecto>();

            foreach (EtapaDefinicion definicion in bibliotecaEtapas
                .Where(e => e != null && e.Activa)
                .OrderBy(e => e.Orden))
            {
                string clave = NormalizarNombreEtapaColor(definicion.Clave);
                EtapaProyecto existente = existentes.FirstOrDefault(e =>
                    ObtenerClaveEtapaConfigurada(e.Nombre) == clave ||
                    ObtenerClaveEtapaConfiguradaDesdeBibliotecaAnterior(
                        e.Nombre,
                        etapasAnteriores
                    ) == clave
                );

                EtapaProyecto etapa = existente ??
                    BibliotecasEtapas.CrearEtapasBase(
                        new List<EtapaDefinicion> { definicion }
                    ).FirstOrDefault();

                if (etapa == null)
                {
                    continue;
                }

                etapa.Nombre = definicion.Nombre;
                nuevasEtapas.Add(etapa);
            }

            cotizacion.Etapas = nuevasEtapas;
            SincronizarCargosGeneralesEnTodasLasEtapas();
        }

        private string ObtenerClaveEtapaConfiguradaDesdeBibliotecaAnterior(
            string nombreEtapa,
            List<EtapaDefinicion> etapasAnteriores
        )
        {
            if (etapasAnteriores == null || etapasAnteriores.Count == 0)
            {
                return "";
            }

            string nombre = NormalizarNombreEtapaColor(nombreEtapa);

            EtapaDefinicion definicion = etapasAnteriores.FirstOrDefault(e =>
                e != null &&
                (
                    NormalizarNombreEtapaColor(e.Clave) == nombre ||
                    NormalizarNombreEtapaColor(e.Nombre) == nombre
                )
            );

            return definicion == null
                ? ""
                : NormalizarNombreEtapaColor(definicion.Clave);
        }

        private void RefrescarDespuesDeCambiarBibliotecaEtapas()
        {
            CargarTablaEtapas();
            RefrescarTablaSubEtapasSiExiste();
            CargarComboEtapasBibliotecaCargos();
            CargarTablaBibliotecaCargos();
            RefrescarResumen();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }
        }

        private int ParsearEntero(object valor, int fallback)
        {
            int resultado;

            if (int.TryParse(Convert.ToString(valor), out resultado))
            {
                return resultado;
            }

            return fallback;
        }

        private Color ParsearColorEtapa(string texto, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return fallback;
            }

            string limpio = texto.Trim();

            try
            {
                if (!limpio.StartsWith("#"))
                {
                    limpio = "#" + limpio;
                }

                return ColorTranslator.FromHtml(limpio);
            }
            catch
            {
                int argb;

                if (int.TryParse(
                    texto,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out argb))
                {
                    return Color.FromArgb(argb);
                }

                return fallback;
            }
        }

        private string ConvertirColorArgbAHex(int argb)
        {
            Color color = Color.FromArgb(argb);
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private Color ObtenerColorTextoLegible(Color fondo)
        {
            double luminancia = (0.299 * fondo.R + 0.587 * fondo.G + 0.114 * fondo.B) / 255.0;
            return luminancia > 0.55 ? Color.Black : Color.White;
        }

        private void AplicarCambiosFilaSubEtapa(int rowIndex)
        {
            if (cargandoTabla)
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[rowIndex];
            SubEtapaProyecto sub = row.Tag as SubEtapaProyecto;

            if (sub == null)
            {
                return;
            }

            string textoDuracion = LeerCeldaEtapa(row, ColEtapaDuracion);
            string textoInicio = LeerCeldaEtapa(row, ColEtapaInicio);
            string textoFin = LeerCeldaEtapa(row, ColEtapaFin);

            string columnaEditada = "";

            if (dgvEtapas.CurrentCell != null)
            {
                columnaEditada = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;
            }

            double inicioAnterior = sub.InicioSemana <= 0.0 ? 1.0 : sub.InicioSemana;
            double duracionAnterior = sub.DuracionSemanas <= 0.0 ? 0.1 : sub.DuracionSemanas;
            double finAnterior = sub.FinSemana;

            double nuevoInicio = ParsearSemana(textoInicio);
            double nuevaDuracion = ParsearDuracionSemanas(textoDuracion);
            double nuevoFin = ParsearSemana(textoFin);

            if (nuevoInicio < 1.0)
            {
                nuevoInicio = inicioAnterior;
            }

            if (nuevaDuracion <= 0.0)
            {
                nuevaDuracion = duracionAnterior;
            }

            if (nuevoFin < 1.0)
            {
                nuevoFin = finAnterior;
            }

            if (columnaEditada == ColEtapaDuracion)
            {
                sub.InicioSemana = inicioAnterior;
                sub.DuracionSemanas = nuevaDuracion;
            }
            else if (columnaEditada == ColEtapaInicio)
            {
                sub.InicioSemana = nuevoInicio;
                sub.DuracionSemanas = duracionAnterior;
            }
            else if (columnaEditada == ColEtapaFin)
            {
                if (nuevoFin <= inicioAnterior)
                {
                    nuevoFin = inicioAnterior + 0.1;
                }

                sub.InicioSemana = inicioAnterior;
                sub.DuracionSemanas = nuevoFin - inicioAnterior;
            }
            else
            {
                sub.InicioSemana = nuevoInicio;
                sub.DuracionSemanas = nuevaDuracion;
            }

            if (sub.InicioSemana < 1.0)
            {
                sub.InicioSemana = 1.0;
            }

            if (sub.DuracionSemanas <= 0.0)
            {
                sub.DuracionSemanas = 0.1;
            }

            sub.Activa = true;

            EtapaProyecto etapaPadre = ObtenerEtapaPorNombreSubEtapa(sub.EtapaPadre);

            if (etapaPadre != null)
            {
                etapasExpandidasEnTabla.Add(NormalizarNombreEtapa(etapaPadre.Nombre));
                etapaPadre.Seleccionada = true;
                ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(etapaPadre);
                ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();
            }

            RefrescarDespuesDeEditarEtapas();
        }

        private double ParsearSemana(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 1.0;
            }

            string limpio = texto
                .Trim()
                .ToUpperInvariant()
                .Replace("SEMANAS", "")
                .Replace("SEMANA", "")
                .Replace("SEM", "")
                .Replace("S", "")
                .Replace(" ", "")
                .Replace(",", ".");

            double valor;

            if (!double.TryParse(
                limpio,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out valor))
            {
                return 1.0;
            }

            if (valor < 1.0)
            {
                valor = 1.0;
            }

            return valor;
        }


        private void ValidarPrecedenciaTemporalEtapasSinInflarDuracion()
        {
            ValidarOrdenInicioEtapas();
        }

        private double ParsearDuracionSemanas(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return 1.0;
            }

            string limpio = texto
                .Trim()
                .ToLowerInvariant()
                .Replace("semanas", "")
                .Replace("semana", "")
                .Replace("sems", "")
                .Replace("sem", "")
                .Replace("s", "")
                .Replace(" ", "")
                .Replace(",", ".");

            double valor;

            if (!double.TryParse(
                limpio,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out valor))
            {
                return 1.0;
            }

            if (valor <= 0.0)
            {
                valor = 0.1;
            }

            return valor;
        }

        private void ConectarEventosEtapas()
        {
            dgvEtapas.CellValueChanged -= DgvEtapas_CellValueChanged;
            dgvEtapas.CellEndEdit -= DgvEtapas_CellEndEdit;
            dgvEtapas.CellClick -= DgvEtapas_CellClick;
            dgvEtapas.SelectionChanged -= DgvEtapas_SelectionChanged;
            dgvEtapas.CurrentCellDirtyStateChanged -= DgvEtapas_CurrentCellDirtyStateChanged;
            dgvEtapas.EditingControlShowing -= DgvEtapas_EditingControlShowing;

            dgvEtapas.CellValueChanged += DgvEtapas_CellValueChanged;
            dgvEtapas.CellEndEdit += DgvEtapas_CellEndEdit;
            dgvEtapas.CellClick += DgvEtapas_CellClick;
            dgvEtapas.SelectionChanged += DgvEtapas_SelectionChanged;
            dgvEtapas.CurrentCellDirtyStateChanged += DgvEtapas_CurrentCellDirtyStateChanged;
            dgvEtapas.EditingControlShowing += DgvEtapas_EditingControlShowing;
        }
        private void DgvEtapas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (cargandoTabla)
            {
                return;
            }

            if (dgvEtapas == null || dgvEtapas.CurrentCell == null)
            {
                return;
            }

            string columna = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;

            if (columna == ColEtapaUsar && dgvEtapas.IsCurrentCellDirty)
            {
                dgvEtapas.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
        private void DesconectarEventosEtapas()
        {
            dgvEtapas.CellValueChanged -= DgvEtapas_CellValueChanged;
            dgvEtapas.CellEndEdit -= DgvEtapas_CellEndEdit;
            dgvEtapas.CellClick -= DgvEtapas_CellClick;
            dgvEtapas.SelectionChanged -= DgvEtapas_SelectionChanged;
            dgvEtapas.CurrentCellDirtyStateChanged -= DgvEtapas_CurrentCellDirtyStateChanged;
            dgvEtapas.EditingControlShowing -= DgvEtapas_EditingControlShowing;
        }

        private void DgvEtapas_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox tb = e.Control as TextBox;

            if (tb == null)
            {
                return;
            }

            tb.KeyPress -= DgvEtapasDecimal_KeyPress;

            if (dgvEtapas == null || dgvEtapas.CurrentCell == null)
            {
                return;
            }

            string columna = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;

            if (columna == ColEtapaDuracion ||
                columna == ColEtapaInicio ||
                columna == ColEtapaFin)
            {
                tb.KeyPress += DgvEtapasDecimal_KeyPress;
            }
        }

        private void DgvEtapasDecimal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == ',' || e.KeyChar == '.')
            {
                TextBox tb = sender as TextBox;

                if (tb != null &&
                    !tb.Text.Contains(",") &&
                    !tb.Text.Contains("."))
                {
                    return;
                }
            }

            e.Handled = true;
        }

        private bool EtapaEstaExpandidaEnTabla(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return false;
            }

            string clave = NormalizarNombreEtapa(etapa.Nombre);

            return etapasExpandidasEnTabla.Contains(clave);
        }

        private string ObtenerTextoVisibleEtapaEnTabla(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return "";
            }

            string flecha = EtapaEstaExpandidaEnTabla(etapa) ? "▾ " : "▸ ";
            return flecha + etapa.Nombre;
        }

        private int ObtenerOrdenEtapa(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return 99;
            }

            return ObtenerOrdenEtapaGeneral(etapa.Nombre);
        }

        private void ReconstruirColumnasEtapas()
        {
            cargandoTabla = true;

            dgvEtapas.Columns.Clear();

            DataGridViewCheckBoxColumn colUsar = new DataGridViewCheckBoxColumn();
            colUsar.Name = ColEtapaUsar;
            colUsar.HeaderText = "Usar";
            colUsar.TrueValue = true;
            colUsar.FalseValue = false;
            colUsar.IndeterminateValue = false;
            colUsar.Width = 44;
            colUsar.FillWeight = 7;
            colUsar.ReadOnly = true;
            dgvEtapas.Columns.Add(colUsar);

            dgvEtapas.Columns.Add(ColEtapaNombre, "Etapa");
            dgvEtapas.Columns.Add(ColEtapaDuracion, "Duración meses");
            dgvEtapas.Columns.Add(ColEtapaInicio, "Inicio");
            dgvEtapas.Columns.Add(ColEtapaFin, "Fin");
            dgvEtapas.Columns.Add(ColEtapaEstado, "Estado");
            dgvEtapas.Columns.Add(ColEtapaCosto, "Costo");

            dgvEtapas.Columns[ColEtapaUsar].ReadOnly = true;
            dgvEtapas.Columns[ColEtapaNombre].ReadOnly = true;
            dgvEtapas.Columns[ColEtapaDuracion].ReadOnly = false;
            dgvEtapas.Columns[ColEtapaInicio].ReadOnly = false;
            dgvEtapas.Columns[ColEtapaFin].ReadOnly = false;
            dgvEtapas.Columns[ColEtapaEstado].ReadOnly = true;
            dgvEtapas.Columns[ColEtapaCosto].ReadOnly = true;

            dgvEtapas.Columns[ColEtapaUsar].FillWeight = 7;
            dgvEtapas.Columns[ColEtapaNombre].FillWeight = 23;
            dgvEtapas.Columns[ColEtapaDuracion].FillWeight = 17;
            dgvEtapas.Columns[ColEtapaInicio].FillWeight = 13;
            dgvEtapas.Columns[ColEtapaFin].FillWeight = 13;
            dgvEtapas.Columns[ColEtapaEstado].FillWeight = 15;
            dgvEtapas.Columns[ColEtapaCosto].FillWeight = 15;

            foreach (DataGridViewColumn columna in dgvEtapas.Columns)
            {
                columna.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvEtapas.EnableHeadersVisualStyles = false;
            dgvEtapas.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEtapas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvEtapas.ColumnHeadersHeight = 32;

            dgvEtapas.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvEtapas.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvEtapas.RowTemplate.Height = 30;

            dgvEtapas.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvEtapas.MultiSelect = false;
            dgvEtapas.RowHeadersVisible = false;
            dgvEtapas.AllowUserToResizeRows = false;

            cargandoTabla = false;
        }

        private bool ColumnasEtapasValidas()
        {
            if (dgvEtapas == null || dgvEtapas.Columns == null)
            {
                return false;
            }

            return dgvEtapas.Columns.Contains(ColEtapaUsar) &&
                   dgvEtapas.Columns.Contains(ColEtapaNombre) &&
                   dgvEtapas.Columns.Contains(ColEtapaDuracion) &&
                   dgvEtapas.Columns.Contains(ColEtapaInicio) &&
                   dgvEtapas.Columns.Contains(ColEtapaFin) &&
                   dgvEtapas.Columns.Contains(ColEtapaEstado) &&
                   dgvEtapas.Columns.Contains(ColEtapaCosto);
        }

        private void AsegurarColumnasEtapas()
        {
            if (ColumnasEtapasValidas())
            {
                return;
            }

            ReconstruirColumnasEtapas();
        }

        private void CargarTablaEtapas()
        {
            if (dgvEtapas == null || dgvEtapas.Columns.Count == 0)
            {
                return;
            }

            DesconectarEventosEtapas();

            cargandoTabla = true;

            dgvEtapas.Rows.Clear();

            foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapa(e)))
            {
            
                bool tieneSubEtapasActivas = TieneSubEtapasActivasDesdeTablaPrincipal(etapa);

                if (tieneSubEtapasActivas)
                {
                    etapa.Seleccionada = true;

                    if (etapa.InicioMes < 0.0)
                    {
                        etapa.InicioMes = 0.0;
                    }

                    if (etapa.DuracionMeses <= 0.0)
                    {
                        etapa.DuracionMeses = 1.0;
                    }

                    if (etapa.FinMes <= etapa.InicioMes)
                    {
                        etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                    }

                   
                }
                else
                {
                    etapa.Seleccionada = false;
                    etapa.DuracionMeses = 0.0;
                    etapa.InicioMes = 0.0;
                    etapa.FinMes = 0.0;
                }

                int rowIndex = dgvEtapas.Rows.Add();
                DataGridViewRow row = dgvEtapas.Rows[rowIndex];

                row.Tag = etapa;

                // IMPORTANTE:
                // El checkbox se pinta desde el modelo real, no desde el click visual.
                row.Cells[ColEtapaUsar].Value = etapa.Seleccionada;
                row.Cells[ColEtapaUsar].ReadOnly = true;

                row.Cells[ColEtapaNombre].Value = ObtenerTextoVisibleEtapaEnTabla(etapa);

                row.Cells[ColEtapaDuracion].Value = etapa.Seleccionada
                    ? etapa.DuracionMeses.ToString("0.##")
                    : "";

                row.Cells[ColEtapaInicio].Value = etapa.Seleccionada
                    ? etapa.InicioMes.ToString("0.##")
                    : "";

                row.Cells[ColEtapaFin].Value = etapa.Seleccionada
                    ? etapa.FinMes.ToString("0.##")
                    : "";

                row.Cells[ColEtapaEstado].Value = etapa.Seleccionada
                    ? "Activa"
                    : "Inactiva";

                row.Cells[ColEtapaCosto].Value = etapa.Seleccionada
                    ? FormatearValorVisual(etapa.CostoTotal)
                    : "";

                AplicarEstiloFilaEtapa(row, etapa);

                // Doble seguridad para que el checkbox quede como corresponde,
                // incluso en la primera fila.
                row.Cells[ColEtapaUsar].Value = etapa.Seleccionada;

                if (EtapaEstaExpandidaEnTabla(etapa))
                {
                    AgregarFilasSubEtapasEnTabla(etapa);
                }
            }

            cargandoTabla = false;

            ConectarEventosEtapas();
        }

        private void AgregarFilasSubEtapasEnTabla(EtapaProyecto etapa)
        {
            if (bibliotecaSubEtapas == null || bibliotecaSubEtapas.Count == 0)
            {
                return;
            }

            var subEtapas = bibliotecaSubEtapas
                .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .OrderBy(s => s.Orden)
                .ToList();

            if (subEtapas.Count == 0)
            {
                return;
            }

            foreach (SubEtapaProyecto sub in subEtapas)
            {
                int rowIndex = dgvEtapas.Rows.Add();
                DataGridViewRow row = dgvEtapas.Rows[rowIndex];

                string marca;

                if (!sub.Activa)
                {
                    marca = "□";
                }
                else
                {
                    marca = sub.Requerida ? "●" : "○";
                }

                row.Cells[ColEtapaUsar] = new DataGridViewTextBoxCell();
                row.Cells[ColEtapaUsar].Value = "";

                row.Cells[ColEtapaNombre].Value = $"    ↳ {marca} {sub.Nombre}";

                /*
                 * En la tabla principal NO mostramos duración/inicio/fin de subetapas.
                 * Esa edición fina queda para la pestaña Subetapas/Gantt.
                 */
                row.Cells[ColEtapaDuracion].Value =
    sub.Activa ? sub.DuracionSemanas.ToString("0.##") : "";

                row.Cells[ColEtapaInicio].Value =
                    sub.Activa ? sub.InicioSemana.ToString("0.##") : "";

                row.Cells[ColEtapaFin].Value =
                    sub.Activa ? sub.FinSemana.ToString("0.##") : "";

                row.Cells[ColEtapaEstado].Value = ObtenerTextoEstadoSubEtapa(sub);
                row.Cells[ColEtapaCosto].Value = "";

                row.Tag = sub;

                AplicarEstiloFilaSubEtapaEnTabla(row, etapa, sub);

                row.Cells[ColEtapaUsar].ReadOnly = true;
                row.Cells[ColEtapaNombre].ReadOnly = true;
                row.Cells[ColEtapaDuracion].ReadOnly = !sub.Activa;
                row.Cells[ColEtapaInicio].ReadOnly = !sub.Activa;
                row.Cells[ColEtapaFin].ReadOnly = !sub.Activa;
                row.Cells[ColEtapaEstado].ReadOnly = true;
                row.Cells[ColEtapaCosto].ReadOnly = true;
            }
        }

        private string ObtenerTextoEstadoSubEtapa(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return "";
            }

            if (!sub.Activa)
            {
                return "Subproceso excluido";
            }

            return sub.Requerida
                ? "Objetivo requerido"
                : "Objetivo opcional";
        }

        private void AplicarEstiloFilaSubEtapaEnTabla(
            DataGridViewRow row,
            EtapaProyecto etapa,
            SubEtapaProyecto sub
        )
        {
            Color baseColor = ObtenerColorFilaEtapa(etapa.Nombre);

            Color fondo = sub.Activa
                ? MezclarConBlanco(baseColor, 0.35)
                : Color.FromArgb(245, 245, 245);

            Color texto = sub.Activa
                ? Color.FromArgb(40, 40, 40)
                : Color.FromArgb(135, 135, 135);

            row.DefaultCellStyle.BackColor = fondo;
            row.DefaultCellStyle.SelectionBackColor = fondo;
            row.DefaultCellStyle.ForeColor = texto;
            row.DefaultCellStyle.SelectionForeColor = texto;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells[ColEtapaNombre].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            row.Cells[ColEtapaNombre].Style.Font = sub.Activa
                ? new Font("Segoe UI", 9, FontStyle.Bold)
                : new Font("Segoe UI", 9, FontStyle.Italic);

            row.Cells[ColEtapaDuracion].Style.BackColor = fondo;
            row.Cells[ColEtapaInicio].Style.BackColor = fondo;
            row.Cells[ColEtapaFin].Style.BackColor = fondo;
            row.Cells[ColEtapaCosto].Style.BackColor = fondo;

            row.Cells[ColEtapaEstado].Style.Font = sub.Activa
                ? (sub.Requerida
                    ? new Font("Segoe UI", 9, FontStyle.Bold)
                    : new Font("Segoe UI", 9, FontStyle.Italic))
                : new Font("Segoe UI", 9, FontStyle.Italic);

            row.Cells[ColEtapaEstado].Style.ForeColor = sub.Activa
                ? Color.FromArgb(40, 40, 40)
                : Color.FromArgb(135, 135, 135);

            row.Cells[ColEtapaNombre].ToolTipText =
                "Click para incluir o excluir este subproceso.";

            row.Cells[ColEtapaEstado].ToolTipText = etapa.Seleccionada
                ? "Click para desactivar esta etapa y todos sus subprocesos."
                : "Click para activar esta etapa con todos sus subprocesos como opcionales recomendados.";

            row.ReadOnly = false;

            // La columna Usar queda reservada solo para la etapa/conjunto.
            // Las subetapas se manejan como antes: nombre = incluir/excluir,
            // estado = opcional/requerido.
            row.Cells[ColEtapaUsar].ReadOnly = true;
            row.Cells[ColEtapaNombre].ReadOnly = true;
            row.Cells[ColEtapaDuracion].ReadOnly = true;
            row.Cells[ColEtapaInicio].ReadOnly = true;
            row.Cells[ColEtapaFin].ReadOnly = true;
            row.Cells[ColEtapaEstado].ReadOnly = true;
            row.Cells[ColEtapaCosto].ReadOnly = true;

            row.Cells[ColEtapaUsar].Style.BackColor = fondo;
            row.Cells[ColEtapaUsar].Style.SelectionBackColor = fondo;
            row.Cells[ColEtapaUsar].ToolTipText =
                "El selector Usar aplica solo a la etapa completa.";

            row.Cells[ColEtapaNombre].ToolTipText = sub.Activa
                ? "Click para excluir este subproceso."
                : "Click para incluir este subproceso como opcional.";

            row.Cells[ColEtapaEstado].ToolTipText = sub.Activa
                ? "Click para alternar entre objetivo requerido y objetivo opcional."
                : "Click para activar este subproceso como requerido.";
        }

        private void AplicarEstiloFilaEtapa(DataGridViewRow row, EtapaProyecto etapa)
        {
            if (row == null || etapa == null)
            {
                return;
            }

            Color colorFondo = etapa.Seleccionada
                ? ObtenerColorFilaEtapa(etapa.Nombre)
                : Color.White;

            Color colorTexto = etapa.Seleccionada
                ? Color.Black
                : Color.FromArgb(120, 120, 120);

            row.DefaultCellStyle.BackColor = colorFondo;
            row.DefaultCellStyle.ForeColor = colorTexto;
            row.DefaultCellStyle.SelectionBackColor = colorFondo;
            row.DefaultCellStyle.SelectionForeColor = colorTexto;
            row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            row.Cells[ColEtapaNombre].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            row.Cells[ColEtapaUsar].ToolTipText = etapa.Seleccionada
                ? "Click para quitar esta etapa del proyecto."
                : "Click para incluir esta etapa en el proyecto.";

            row.Cells[ColEtapaNombre].ToolTipText =
                "Click para expandir o contraer sus subprocesos sin activar la etapa.";

            row.Cells[ColEtapaDuracion].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells[ColEtapaInicio].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells[ColEtapaFin].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            row.Cells[ColEtapaEstado].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            row.Cells[ColEtapaEstado].Style.ForeColor = etapa.Seleccionada
                ? Color.FromArgb(30, 90, 60)
                : Color.FromArgb(130, 130, 130);

            row.Cells[ColEtapaEstado].ToolTipText =
    "Estado informativo. Para incluir o quitar la etapa, use la columna Usar.";

            row.Cells[ColEtapaCosto].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }


        
        private void DgvEtapas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[e.RowIndex];
            string columna = dgvEtapas.Columns[e.ColumnIndex].Name;

            if (row.Tag is EtapaProyecto etapa)
            {
                if (columna == ColEtapaUsar)
                {
                    AlternarActivacionEtapaDesdeTablaPrincipal(etapa);
                    return;
                }

                if (columna == ColEtapaNombre)
                {
                    AlternarExpansionEtapaEnTablaPrincipal(etapa);
                    return;
                }

                return;
            }

            if (row.Tag is SubEtapaProyecto sub)
            {
                if (columna == ColEtapaUsar)
                {
                    return;
                }

                /*
                 * Click en nombre:
                 * - activo -> excluido
                 * - excluido -> activo opcional
                 */
                if (columna == ColEtapaNombre)
                {
                    sub.Activa = !sub.Activa;

                    if (sub.Activa)
                    {
                        sub.Requerida = false;
                    }
                    else
                    {
                        sub.Requerida = false;
                    }

                    MantenerEtapaPadreExpandidaDesdeTablaPrincipal(sub);
                    ActualizarEstadoEtapaPadreDesdeTablaPrincipal(sub);

                    RefrescarDespuesDeEditarEtapas();
                    return;
                }

                /*
                 * Click en estado:
                 * - si estaba excluido -> activo requerido
                 * - si estaba activo opcional -> activo requerido
                 * - si estaba activo requerido -> activo opcional
                 */
                if (columna == ColEtapaEstado)
                {
                    if (!sub.Activa)
                    {
                        sub.Activa = true;
                        sub.Requerida = true;
                    }
                    else
                    {
                        sub.Requerida = !sub.Requerida;
                    }

                    MantenerEtapaPadreExpandidaDesdeTablaPrincipal(sub);
                    ActualizarEstadoEtapaPadreDesdeTablaPrincipal(sub);

                    RefrescarDespuesDeEditarEtapas();
                    return;
                }

                /*
                 * Duración, inicio y fin de subetapas no se editan acá.
                 */
                return;
            }
        }

        private void AlternarExpansionEtapaEnTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            string clave = NormalizarNombreEtapa(etapa.Nombre);

            if (etapasExpandidasEnTabla.Contains(clave))
            {
                etapasExpandidasEnTabla.Remove(clave);
            }
            else
            {
                etapasExpandidasEnTabla.Add(clave);
            }

            RefrescarDespuesDeEditarEtapas();
        }

        private void ManejarClickUsarEtapaDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            /*
             * Regla correcta:
             *
             * U existe si y solo si existe al menos un subconjunto u_i activo.
             *
             * Por eso:
             * - Si la etapa está inactiva, el click en Usar activa la etapa
             *   y propone todas sus subetapas como opcionales.
             *
             * - Si la etapa ya está activa, el click en Usar NO la destruye
             *   si todavía tiene subetapas activas.
             *
             * - La etapa solo se cancela realmente cuando todas sus subetapas
             *   quedan inactivas.
             */

            if (!etapa.Seleccionada)
            {
                ActivarEtapaConTodasSusSubEtapasOpcionalesDesdeTablaPrincipal(etapa);
                etapaExpandidaEnTabla = etapa;

                NormalizarTodasLasEtapasPorSubEtapasDesdeTablaPrincipal();
                ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();

                RefrescarDespuesDeEditarEtapas();
                return;
            }

            if (TieneSubEtapasActivasDesdeTablaPrincipal(etapa))
            {
                // No se permite apagar el conjunto desde el checkbox general
                // mientras existan subconjuntos activos.
                etapa.Seleccionada = true;
                etapaExpandidaEnTabla = etapa;

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

                ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();
                RefrescarDespuesDeEditarEtapas();
                return;
            }

            // Solo si ya no quedan subetapas activas, la etapa puede apagarse.
            DesactivarSoloEtapaDesdeTablaPrincipal(etapa);
            etapaExpandidaEnTabla = etapa;

            NormalizarTodasLasEtapasPorSubEtapasDesdeTablaPrincipal();
            ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();

            RefrescarDespuesDeEditarEtapas();
        }
        private void AlternarActivacionEtapaDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            /*
             * Regla UX:
             *
             * - Usar apagado -> prende el conjunto completo.
             * - Usar prendido -> apaga todos los subconjuntos.
             * - La etapa queda activa/inactiva como consecuencia de sus subetapas.
             * - La etapa permanece expandida para que el usuario vea qué pasó.
             */

            if (TieneSubEtapasActivasDesdeTablaPrincipal(etapa))
            {
                DesactivarEtapaYSubEtapasDesdeTablaPrincipal(etapa);
            }
            else
            {
                ActivarEtapaConTodasSusSubEtapasOpcionalesDesdeTablaPrincipal(etapa);
            }

            etapasExpandidasEnTabla.Add(NormalizarNombreEtapa(etapa.Nombre));
            NormalizarTodasLasEtapasPorSubEtapasDesdeTablaPrincipal();
            ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();

            RefrescarDespuesDeEditarEtapas();
        }

        private void ActivarEtapaConTodasSusSubEtapasOpcionalesDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (bibliotecaSubEtapas != null)
            {
                var subEtapasDeLaEtapa = bibliotecaSubEtapas
                    .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .OrderBy(s => s.Orden)
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
                {
                    sub.Activa = true;
                    sub.Requerida = false;

                    if (sub.InicioSemana < 0.0)
                    {
                        sub.InicioSemana = 0.0;
                    }

                    if (sub.DuracionSemanas <= 0.0)
                    {
                        sub.DuracionSemanas = 0.1;
                    }
                }
            }

            etapa.Seleccionada = TieneSubEtapasActivasDesdeTablaPrincipal(etapa);

            if (!etapa.Seleccionada)
            {
                DesactivarSoloEtapaDesdeTablaPrincipal(etapa);
                return;
            }

            if (etapa.DuracionMeses <= 0.0)
            {
                etapa.DuracionMeses = 1.0;
            }

            if (etapa.InicioMes < 0.0)
            {
                etapa.InicioMes = 0.0;
            }

            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

            ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(etapa);
        }

        private void DesactivarEtapaYSubEtapasDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (bibliotecaSubEtapas != null)
            {
                var subEtapasDeLaEtapa = bibliotecaSubEtapas
                    .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                    .ToList();

                foreach (SubEtapaProyecto sub in subEtapasDeLaEtapa)
                {
                    sub.Activa = false;
                    sub.Requerida = false;
                }
            }

            DesactivarSoloEtapaDesdeTablaPrincipal(etapa);
        }

        private void DesactivarSoloEtapaDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            etapa.Seleccionada = false;
            etapa.DuracionMeses = 0.0;
            etapa.InicioMes = 0.0;
            etapa.FinMes = 0.0;

            if (etapa.Plan != null)
            {
                etapa.Plan.Clear();
            }
        }

        private void MantenerEtapaPadreExpandidaDesdeTablaPrincipal(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            string clave = NormalizarNombreEtapa(sub.EtapaPadre);

            if (!string.IsNullOrWhiteSpace(clave))
            {
                etapasExpandidasEnTabla.Add(clave);
            }
        }

        private void ActualizarEstadoEtapaPadreDesdeTablaPrincipal(SubEtapaProyecto sub)
        {
            if (sub == null)
            {
                return;
            }

            EtapaProyecto etapaPadre = ObtenerEtapaPorNombreSubEtapaDesdeTablaPrincipal(sub.EtapaPadre);

            if (etapaPadre == null)
            {
                return;
            }

            if (!TieneSubEtapasActivasDesdeTablaPrincipal(etapaPadre))
            {
                DesactivarSoloEtapaDesdeTablaPrincipal(etapaPadre);
                return;
            }

            etapaPadre.Seleccionada = true;

            if (etapaPadre.InicioMes < 0.0)
            {
                etapaPadre.InicioMes = 0.0;
            }

            if (etapaPadre.DuracionMeses <= 0.0)
            {
                etapaPadre.DuracionMeses = 1.0;
            }

            if (etapaPadre.FinMes < etapaPadre.InicioMes)
            {
                etapaPadre.FinMes = etapaPadre.InicioMes + etapaPadre.DuracionMeses;
            }

            ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(etapaPadre);
            ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();
        }

        private bool TieneSubEtapasActivasDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return false;
            }

            return bibliotecaSubEtapas.Any(s =>
                NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre) &&
                s.Activa
            );
        }

        private EtapaProyecto ObtenerEtapaPorNombreSubEtapaDesdeTablaPrincipal(string nombreEtapa)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return null;
            }

            string nombreNormalizado = NormalizarNombreEtapa(nombreEtapa);

            return cotizacion.Etapas.FirstOrDefault(e =>
                NormalizarNombreEtapa(e.Nombre) == nombreNormalizado
            );
        }

        private void ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(EtapaProyecto etapa)
        {
            if (etapa == null || bibliotecaSubEtapas == null)
            {
                return;
            }

            var subEtapasActivas = bibliotecaSubEtapas
                .Where(s => NormalizarNombreEtapa(s.EtapaPadre) == NormalizarNombreEtapa(etapa.Nombre))
                .Where(s => s.Activa)
                .ToList();

            if (subEtapasActivas.Count == 0)
            {
                DesactivarSoloEtapaDesdeTablaPrincipal(etapa);
                return;
            }

            double menorInicioSemana = double.MaxValue;
            double mayorFinSemana = 0.0;

            foreach (SubEtapaProyecto sub in subEtapasActivas)
            {
                if (sub.InicioSemana < 0.0)
                {
                    sub.InicioSemana = 0.0;
                }

                if (sub.DuracionSemanas <= 0.0)
                {
                    sub.DuracionSemanas = 0.1;
                }

                if (sub.InicioSemana < menorInicioSemana)
                {
                    menorInicioSemana = sub.InicioSemana;
                }

                if (sub.FinSemana > mayorFinSemana)
                {
                    mayorFinSemana = sub.FinSemana;
                }
            }

            if (menorInicioSemana == double.MaxValue)
            {
                menorInicioSemana = 0.0;
            }

            double inicioMesNecesario =
                menorInicioSemana / SemanasPorMesPlanificacion;

            double finMesNecesario =
                mayorFinSemana / SemanasPorMesPlanificacion;

            if (finMesNecesario <= inicioMesNecesario)
            {
                finMesNecesario = inicioMesNecesario + 0.025;
            }

            etapa.Seleccionada = true;
            etapa.InicioMes = inicioMesNecesario;
            etapa.FinMes = finMesNecesario;
            etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;

            if (etapa.DuracionMeses <= 0.0)
            {
                etapa.DuracionMeses = 0.025;
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }
        }

        private void NormalizarTodasLasEtapasPorSubEtapasDesdeTablaPrincipal()
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return;
            }

            foreach (EtapaProyecto etapa in cotizacion.Etapas.OrderBy(e => ObtenerOrdenEtapaDesdeTablaPrincipal(e.Nombre)))
            {
                if (etapa == null)
                {
                    continue;
                }

                if (!TieneSubEtapasActivasDesdeTablaPrincipal(etapa))
                {
                    DesactivarSoloEtapaDesdeTablaPrincipal(etapa);
                    continue;
                }

                etapa.Seleccionada = true;

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                }

                if (etapa.FinMes < etapa.InicioMes)
                {
                    etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                }

                ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(etapa);
            }

            ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal();
        }

        private void ValidarPrecedenciaTemporalEtapasDesdeTablaPrincipal()
        {
            ValidarPrecedenciaTemporalEtapasSinInflarDuracion();
        }

        private int ObtenerOrdenEtapaDesdeTablaPrincipal(string nombreEtapa)
        {
            return ObtenerOrdenEtapaGeneral(nombreEtapa);
        }

        private void ManejarClickSubEtapaEnTabla(SubEtapaProyecto sub, string columna)
        {
            if (sub == null)
            {
                return;
            }

            if (columna == ColEtapaNombre)
            {
                sub.Activa = !sub.Activa;

                if (sub.Activa && !sub.Requerida)
                {
                    sub.Requerida = true;
                }
            }
            else if (columna == ColEtapaEstado)
            {
                if (!sub.Activa)
                {
                    sub.Activa = true;
                    sub.Requerida = true;
                }
                else
                {
                    sub.Requerida = !sub.Requerida;
                }
            }
            else
            {
                return;
            }

            EtapaProyecto etapaPadre = ObtenerEtapaPorNombreSubEtapa(sub.EtapaPadre);

            if (etapaPadre != null)
            {
                etapasExpandidasEnTabla.Add(NormalizarNombreEtapa(etapaPadre.Nombre));
            }

            RefrescarDespuesDeEditarEtapas();
        }

        private EtapaProyecto ObtenerEtapaPorNombreSubEtapa(string nombreEtapa)
        {
            if (cotizacion == null || cotizacion.Etapas == null)
            {
                return null;
            }

            string nombreNormalizado = NormalizarNombreEtapa(nombreEtapa);

            return cotizacion.Etapas.FirstOrDefault(e =>
                NormalizarNombreEtapa(e.Nombre) == nombreNormalizado
            );
        }

        private void ActivarYExpandirEtapa(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (!etapa.Seleccionada)
            {
                etapa.Seleccionada = true;

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }

            if (etapaExpandidaEnTabla != null &&
                NormalizarNombreEtapa(etapaExpandidaEnTabla.Nombre) == NormalizarNombreEtapa(etapa.Nombre))
            {
                etapaExpandidaEnTabla = null;
            }
            else
            {
                etapasExpandidasEnTabla.Add(NormalizarNombreEtapa(etapa.Nombre));
            }

            ValidarOrdenInicioEtapas();

            RefrescarDespuesDeEditarEtapas();
        }

        private void ToggleEstadoEtapa(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            if (etapa.Seleccionada)
            {
                DesactivarEtapa(etapa);
            }
            else
            {
                etapa.Seleccionada = true;

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }

            ValidarOrdenInicioEtapas();
            ProgramarRefrescoDespuesDeEditarEtapas();
        }

        private void DgvEtapas_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = dgvEtapas.Columns[e.ColumnIndex].Name;

            if (columna == ColEtapaUsar)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[e.RowIndex];

            if (row.Tag is SubEtapaProyecto)
            {
                if (columna == ColEtapaDuracion ||
                    columna == ColEtapaInicio ||
                    columna == ColEtapaFin)
                {
                    AplicarCambiosFilaSubEtapa(e.RowIndex);
                }

                return;
            }

            AplicarCambiosFilaEtapa(e.RowIndex);
        }

        private void DgvEtapas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (cargandoTabla || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columna = dgvEtapas.Columns[e.ColumnIndex].Name;

            if (columna == ColEtapaUsar)
            {
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[e.RowIndex];

            /*
             * Las subetapas en esta tabla solo se activan/desactivan
             * o cambian opcional/requerido por click.
             */
            if (row.Tag is SubEtapaProyecto)
            {
                return;
            }

            AplicarCambiosFilaEtapa(e.RowIndex);
        }

        private void AplicarCambiosFilaEtapa(int rowIndex)
        {
            if (cargandoTabla)
            {
                return;
            }

            if (rowIndex < 0 || rowIndex >= dgvEtapas.Rows.Count)
            {
                return;
            }

            if (!ColumnasEtapasValidas())
            {
                AsegurarColumnasEtapas();
                return;
            }

            DataGridViewRow row = dgvEtapas.Rows[rowIndex];
            EtapaProyecto etapa = row.Tag as EtapaProyecto;

            if (etapa == null)
            {
                return;
            }

            string textoDuracion = LeerCeldaEtapa(row, ColEtapaDuracion);
            string textoInicio = LeerCeldaEtapa(row, ColEtapaInicio);
            string textoFin = LeerCeldaEtapa(row, ColEtapaFin);

            bool hayDuracion = !string.IsNullOrWhiteSpace(textoDuracion);
            bool hayInicio = !string.IsNullOrWhiteSpace(textoInicio);
            bool hayFin = !string.IsNullOrWhiteSpace(textoFin);

            if (!hayDuracion && !hayInicio && !hayFin)
            {
                DesactivarEtapa(etapa);
                ProgramarRefrescoDespuesDeEditarEtapas();
                return;
            }

            string columnaEditada = "";

            if (dgvEtapas.CurrentCell != null)
            {
                columnaEditada = dgvEtapas.Columns[dgvEtapas.CurrentCell.ColumnIndex].Name;
            }

            double inicioAnterior = etapa.InicioMes;
            double duracionAnterior = etapa.DuracionMeses;
            double finAnterior = etapa.FinMes;

            if (inicioAnterior < 0.0)
            {
                inicioAnterior = 0.0;
            }

            if (duracionAnterior <= 0.0)
            {
                duracionAnterior = 1.0;
            }

            if (finAnterior <= inicioAnterior)
            {
                finAnterior = inicioAnterior + duracionAnterior;
            }

            double nuevoInicio = hayInicio ? ConvertirDouble(textoInicio) : inicioAnterior;
            double nuevaDuracion = hayDuracion ? ConvertirDouble(textoDuracion) : duracionAnterior;
            double nuevoFin = hayFin ? ConvertirDouble(textoFin) : finAnterior;

            if (nuevoInicio < 0.0)
            {
                nuevoInicio = 0.0;
            }

            if (nuevaDuracion <= 0.0)
            {
                nuevaDuracion = 1.0;
            }

            if (nuevoFin < nuevoInicio)
            {
                nuevoFin = nuevoInicio;
            }

            etapa.Seleccionada = true;

            if (columnaEditada == ColEtapaDuracion)
            {
                /*
                 * Editar duración:
                 * - mantiene inicio
                 * - cambia duración
                 * - recalcula fin de ESA etapa
                 */
                etapa.InicioMes = inicioAnterior;
                etapa.DuracionMeses = nuevaDuracion;
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
            }
            else if (columnaEditada == ColEtapaInicio)
            {
                /*
                 * Editar inicio tiene dos comportamientos:
                 *
                 * 1) Si el inicio SUBE:
                 *    - se posterga la etapa completa.
                 *    - mantiene duración.
                 *    - mueve fin.
                 *
                 * 2) Si el inicio BAJA:
                 *    - la etapa empieza antes.
                 *    - mantiene fin.
                 *    - aumenta duración.
                 */

                double delta = nuevoInicio - inicioAnterior;

                if (nuevoInicio > inicioAnterior)
                {
                    /*
                     * Inicio sube:
                     * se mueve todo el bloque.
                     */
                    etapa.InicioMes = nuevoInicio;
                    etapa.DuracionMeses = duracionAnterior;
                    etapa.FinMes = finAnterior + delta;
                }
                else if (nuevoInicio < inicioAnterior)
                {
                    /*
                     * Inicio baja:
                     * se mantiene el fin anterior.
                     * aumenta la duración.
                     */
                    etapa.InicioMes = nuevoInicio;
                    etapa.FinMes = finAnterior;
                    etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;

                    if (etapa.DuracionMeses <= 0.0)
                    {
                        etapa.DuracionMeses = 1.0;
                        etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                    }
                }
                else
                {
                    /*
                     * Inicio igual:
                     * no cambia nada relevante.
                     */
                    etapa.InicioMes = inicioAnterior;
                    etapa.DuracionMeses = duracionAnterior;
                    etapa.FinMes = finAnterior;
                }

                /*
                 * Al mover inicio de etapa, se mueven sus subetapas
                 * con respecto al nuevo inicio.
                 */
                double deltaSemanas = delta * SemanasPorMesPlanificacion;

                if (Math.Abs(deltaSemanas) > 0.0001 && bibliotecaSubEtapas != null)
                {
                    foreach (SubEtapaProyecto sub in bibliotecaSubEtapas)
                    {
                        if (sub == null)
                        {
                            continue;
                        }

                        if (NormalizarNombreEtapa(sub.EtapaPadre) != NormalizarNombreEtapa(etapa.Nombre))
                        {
                            continue;
                        }

                        sub.InicioSemana += deltaSemanas;

                        if (sub.InicioSemana < 1.0)
                        {
                            sub.InicioSemana = 1.0;
                        }
                    }
                }

                if (etapa.FinMes <= etapa.InicioMes)
                {
                    etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                }
            }
            else if (columnaEditada == ColEtapaFin)
            {
                /*
                 * Editar fin:
                 * - mantiene inicio
                 * - cambia fin
                 * - recalcula duración de ESA etapa
                 */
                if (nuevoFin <= inicioAnterior)
                {
                    nuevoFin = inicioAnterior + 1.0;
                }

                etapa.InicioMes = inicioAnterior;
                etapa.FinMes = nuevoFin;
                etapa.DuracionMeses = etapa.FinMes - etapa.InicioMes;

                if (etapa.DuracionMeses <= 0.0)
                {
                    etapa.DuracionMeses = 1.0;
                    etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                }
            }
            else
            {
                etapa.InicioMes = nuevoInicio;
                etapa.DuracionMeses = nuevaDuracion;
                etapa.FinMes = nuevoFin;

                if (etapa.FinMes <= etapa.InicioMes)
                {
                    etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                }
            }

            if (TieneSubEtapasActivasDesdeTablaPrincipal(etapa))
            {
                ExpandirEtapaParaContenerSubEtapasDesdeTablaPrincipal(etapa);
            }

            ValidarOrdenInicioEtapas();

            ProgramarRefrescoDespuesDeEditarEtapas();
        }

        private string LeerCeldaEtapa(DataGridViewRow row, string nombreColumna)
        {
            if (row == null)
            {
                return "";
            }

            if (!ColumnasEtapasValidas())
            {
                return "";
            }

            if (!dgvEtapas.Columns.Contains(nombreColumna))
            {
                return "";
            }

            object valor = row.Cells[nombreColumna].Value;

            if (valor == null)
            {
                return "";
            }

            return valor.ToString() ?? "";
        }

        private void DesactivarEtapa(EtapaProyecto etapa)
        {
            if (etapa == null)
            {
                return;
            }

            etapa.Seleccionada = false;
            etapa.DuracionMeses = 0.0;
            etapa.InicioMes = 0.0;
            etapa.FinMes = 0.0;
            etapa.Plan.Clear();
        }

        private void ProgramarRefrescoDespuesDeEditarEtapas()
        {
            if (refrescoEtapasPendiente)
            {
                return;
            }

            refrescoEtapasPendiente = true;

            BeginInvoke(new Action(() =>
            {
                refrescoEtapasPendiente = false;
                RefrescarDespuesDeEditarEtapas();
            }));
        }

        private void RefrescarDespuesDeEditarEtapas()
        {
            ServicioCotizacion.RecalcularCotizacion(cotizacion);

            if (dgvEtapas != null && dgvEtapas.Parent != null)
            {
                CargarTablaEtapas();
            }

            RefrescarTablaSubEtapas();

            CargarCombos();
            CargarTablaManoObra();
            CargarCostosEnPantalla();
            RefrescarResumen();
            RefrescarResultadosDetalle();
            ActualizarBloqueoPestanas();

            if (panelGantt != null)
            {
                panelGantt.Invalidate();
            }

            RefrescarGanttGrandeEtapas();
        }

        private void ValidarOrdenInicioEtapas()
        {
            EtapaProyecto etapaAnterior = null;

            foreach (EtapaProyecto etapa in cotizacion.Etapas
                .Where(e => e.Seleccionada)
                .OrderBy(e => ObtenerOrdenEtapa(e)))
            {
                if (etapa == null)
                {
                    continue;
                }

                if (etapa.InicioMes < 0.0)
                {
                    etapa.InicioMes = 0.0;
                }

                double duracionPropia = etapa.DuracionMeses;

                if (duracionPropia <= 0.0)
                {
                    duracionPropia = 1.0;
                }

                /*
                 * Primero aseguramos la ecuación interna:
                 * Fin = Inicio + Duración.
                 *
                 * Ojo: la duración ya viene corregida antes si el usuario editó Fin.
                 */
                etapa.DuracionMeses = duracionPropia;
                etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;

                if (etapaAnterior != null)
                {
                    /*
                     * Regla 1:
                     * Una etapa posterior no puede iniciar antes que la anterior.
                     *
                     * Si pasa, se mueve completa.
                     */
                    if (etapa.InicioMes < etapaAnterior.InicioMes)
                    {
                        etapa.InicioMes = etapaAnterior.InicioMes;
                        etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                    }

                    /*
                     * Regla 2:
                     * Una etapa posterior no puede terminar antes que la anterior.
                     *
                     * Si pasa, se mueve completa hacia adelante,
                     * manteniendo su duración propia.
                     */
                    if (etapa.FinMes < etapaAnterior.FinMes)
                    {
                        etapa.FinMes = etapaAnterior.FinMes;
                        etapa.InicioMes = etapa.FinMes - etapa.DuracionMeses;

                        /*
                         * Si por conservar duración quedó iniciando antes que la etapa anterior,
                         * entonces manda la regla de inicio y el fin se recalcula desde ahí.
                         */
                        if (etapa.InicioMes < etapaAnterior.InicioMes)
                        {
                            etapa.InicioMes = etapaAnterior.InicioMes;
                            etapa.FinMes = etapa.InicioMes + etapa.DuracionMeses;
                        }
                    }
                }

                etapaAnterior = etapa;
            }
        }

        private bool ExisteEtapaValidaParaCotizar()
        {
            return cotizacion.Etapas.Any(etapa =>
                etapa.Seleccionada &&
                etapa.DuracionMeses >= DuracionMinimaEtapaMesesInterna
            );
        }

        private bool EtapaListaParaManoObra(EtapaProyecto etapa)
        {
            return etapa.Seleccionada && etapa.DuracionMeses >= DuracionMinimaEtapaMesesInterna;
        }
    }
}
