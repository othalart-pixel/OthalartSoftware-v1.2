using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Cotizador_animacion_Othalart.Data;
using Cotizador_animacion_Othalart.Models;

namespace Cotizador_animacion_Othalart
{
    public partial class Form1
    {
        private void ConstruirTabPersonalEmpresa(TabPage tab)
        {
            tab.Controls.Clear();
            tab.BackColor = Color.FromArgb(245, 246, 248);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.Padding = new Padding(28, 24, 28, 20);
            root.BackColor = Color.FromArgb(245, 246, 248);
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titulo = new Label();
            titulo.Text = "Personal / equipo interno";
            titulo.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(18, 22, 28);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 4);

            Label ayuda = new Label();
            ayuda.Text = "Administra personas de la empresa y los cargos que pueden tomar. Esta biblioteca se guarda en JSON y no modifica todavia los calculos de mano de obra.";
            ayuda.Font = new Font("Segoe UI", 10f);
            ayuda.ForeColor = Color.FromArgb(85, 92, 105);
            ayuda.AutoSize = true;
            ayuda.MaximumSize = new Size(1100, 0);
            ayuda.Margin = new Padding(0, 0, 0, 18);

            TableLayoutPanel encabezado = new TableLayoutPanel();
            encabezado.Dock = DockStyle.Top;
            encabezado.AutoSize = true;
            encabezado.ColumnCount = 1;
            encabezado.RowCount = 2;
            encabezado.Controls.Add(titulo, 0, 0);
            encabezado.Controls.Add(ayuda, 0, 1);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;
            split.BackColor = Color.FromArgb(226, 230, 236);
            split.SizeChanged += (s, e) => AjustarSplitterPersonalEmpresa(split);

            split.Panel1.Controls.Add(CrearPanelListadoPersonal());
            split.Panel2.Controls.Add(CrearPanelEditorPersonal());

            lblRutaPersonalEmpresa.Text =
                "Biblioteca: " + BibliotecaPersonalEmpresaJsonService.ObtenerRutaPersonal();
            lblRutaPersonalEmpresa.Dock = DockStyle.Fill;
            lblRutaPersonalEmpresa.AutoSize = true;
            lblRutaPersonalEmpresa.Font = new Font("Segoe UI", 8.8f);
            lblRutaPersonalEmpresa.ForeColor = Color.FromArgb(95, 101, 112);
            lblRutaPersonalEmpresa.Margin = new Padding(0, 12, 0, 0);

            root.Controls.Add(encabezado, 0, 0);
            root.Controls.Add(split, 0, 1);
            root.Controls.Add(lblRutaPersonalEmpresa, 0, 2);

            tab.Controls.Add(root);
            CargarCargosEnEditorPersonal();
            CargarTablaPersonalEmpresa();
        }

        private void AjustarSplitterPersonalEmpresa(SplitContainer split)
        {
            if (split == null || split.Width <= 0)
            {
                return;
            }

            int panel1Min = Math.Min(420, Math.Max(0, split.Width / 3));
            int panel2Min = Math.Min(360, Math.Max(0, split.Width / 4));
            int maxDistancia = split.Width - panel2Min;
            int distancia = Math.Min(640, Math.Max(panel1Min, maxDistancia / 2));

            if (maxDistancia <= panel1Min)
            {
                split.Panel1MinSize = 0;
                split.Panel2MinSize = 0;
                distancia = Math.Max(1, split.Width / 2);
            }
            else
            {
                split.Panel1MinSize = panel1Min;
                split.Panel2MinSize = panel2Min;
                distancia = Math.Max(panel1Min, Math.Min(distancia, maxDistancia));
            }

            try
            {
                if (split.SplitterDistance != distancia)
                {
                    split.SplitterDistance = distancia;
                }
            }
            catch
            {
                // WinForms puede reportar ancho transitorio durante el arranque; se reintenta en el siguiente SizeChanged.
            }
        }

        private Control CrearPanelListadoPersonal()
        {
            Panel card = CrearCardPersonal();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            TableLayoutPanel acciones = new TableLayoutPanel();
            acciones.Dock = DockStyle.Top;
            acciones.AutoSize = true;
            acciones.ColumnCount = 1;
            acciones.RowCount = 2;
            acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            acciones.Margin = new Padding(0, 0, 0, 12);

            FlowLayoutPanel accionesPrincipales = new FlowLayoutPanel();
            accionesPrincipales.Dock = DockStyle.Top;
            accionesPrincipales.AutoSize = true;
            accionesPrincipales.FlowDirection = FlowDirection.LeftToRight;
            accionesPrincipales.Margin = new Padding(0, 0, 0, 8);

            FlowLayoutPanel accionesSecundarias = new FlowLayoutPanel();
            accionesSecundarias.Dock = DockStyle.Top;
            accionesSecundarias.AutoSize = true;
            accionesSecundarias.FlowDirection = FlowDirection.LeftToRight;
            accionesSecundarias.Margin = new Padding(0, 0, 0, 0);

            Button btnNuevo = CrearBotonPersonal("Nuevo");
            btnNuevo.Click += (s, e) => NuevoPersonalSeguro();

            Button btnGuardar = CrearBotonPersonal("Guardar persona");
            btnGuardar.Width = 180;
            btnGuardar.BackColor = Color.FromArgb(213, 244, 233);
            btnGuardar.FlatAppearance.BorderColor = Color.FromArgb(41, 171, 135);
            btnGuardar.Click += (s, e) => GuardarPersonaDesdeEditor();

            Button btnDuplicar = CrearBotonPersonal("Duplicar persona");
            btnDuplicar.Width = 150;
            btnDuplicar.Click += (s, e) => DuplicarPersonaSeleccionada();

            Button btnDesactivar = CrearBotonPersonal("Eliminar / desactivar");
            btnDesactivar.Width = 160;
            btnDesactivar.Click += (s, e) => DesactivarPersonaSeleccionada();

            Button btnRecargar = CrearBotonPersonal("Recargar");
            btnRecargar.Click += (s, e) =>
            {
                CargarCargosEnEditorPersonal();
                CargarTablaPersonalEmpresa();
            };

            accionesPrincipales.Controls.Add(btnGuardar);
            accionesSecundarias.Controls.Add(btnNuevo);
            accionesSecundarias.Controls.Add(btnDuplicar);
            accionesSecundarias.Controls.Add(btnDesactivar);
            accionesSecundarias.Controls.Add(btnRecargar);
            acciones.Controls.Add(accionesPrincipales, 0, 0);
            acciones.Controls.Add(accionesSecundarias, 0, 1);

            dgvPersonalEmpresa = new DataGridView();
            dgvPersonalEmpresa.Dock = DockStyle.Fill;
            dgvPersonalEmpresa.AllowUserToAddRows = false;
            dgvPersonalEmpresa.AllowUserToDeleteRows = false;
            dgvPersonalEmpresa.RowHeadersVisible = false;
            dgvPersonalEmpresa.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPersonalEmpresa.MultiSelect = false;
            dgvPersonalEmpresa.ReadOnly = true;
            dgvPersonalEmpresa.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvPersonalEmpresa.BackgroundColor = Color.White;
            dgvPersonalEmpresa.BorderStyle = BorderStyle.None;
            dgvPersonalEmpresa.GridColor = Color.FromArgb(226, 230, 236);
            AplicarEstiloGrillaPersonal(dgvPersonalEmpresa);

            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Id",
                Width = 120,
                Visible = false
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "Nombre",
                Width = 190
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CargoPrincipal",
                HeaderText = "Cargo principal",
                Width = 190
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CargosPosibles",
                HeaderText = "Cargos posibles",
                Width = 260
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TrabajosPosibles",
                HeaderText = "Trabajos",
                Width = 240
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PagoInterno",
                HeaderText = "Pago",
                Width = 120
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CostoHora",
                HeaderText = "Costo/h",
                Width = 95
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "HorasMaximas",
                HeaderText = "Horas tanda",
                Width = 95
            });
            dgvPersonalEmpresa.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Activo",
                HeaderText = "Activo",
                Width = 70
            });

            dgvPersonalEmpresa.SelectionChanged += (s, e) => CargarPersonaSeleccionadaEnEditor();
            dgvPersonalEmpresa.CellMouseDown += DgvPersonalEmpresa_CellMouseDown;
            dgvPersonalEmpresa.ContextMenuStrip = CrearMenuContextualPersonalEmpresa();

            Panel bordeGrilla = new Panel();
            bordeGrilla.Dock = DockStyle.Fill;
            bordeGrilla.Padding = new Padding(1);
            bordeGrilla.BackColor = Color.FromArgb(218, 223, 230);
            bordeGrilla.Controls.Add(dgvPersonalEmpresa);

            layout.Controls.Add(acciones, 0, 0);
            layout.Controls.Add(bordeGrilla, 0, 1);
            card.Controls.Add(layout);
            return card;
        }

        private Control CrearPanelEditorPersonal()
        {
            Panel card = CrearCardPersonal();

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 13;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label titulo = new Label();
            titulo.Text = "Ficha de persona";
            titulo.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(0, 0, 0, 12);
            layout.Controls.Add(titulo, 0, 0);
            layout.SetColumnSpan(titulo, 2);

            txtPersonalId.ReadOnly = true;
            txtPersonalNombre.MaxLength = 120;
            txtPersonalNotas.Multiline = true;
            txtPersonalNotas.Height = 76;
            txtPersonalNotas.ScrollBars = ScrollBars.Vertical;

            ConfigurarNumericPersonal(nudPersonalPagoInterno, 0, 100000000, 0);
            ConfigurarNumericPersonal(nudPersonalHorasSemana, 1, 120, 42);
            ConfigurarNumericPersonal(nudPersonalHorasMaximas, 1, 200, 42);

            cmbPersonalPeriodoPago.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPersonalPeriodoPago.Items.Clear();
            cmbPersonalPeriodoPago.Items.Add("Mensual");
            cmbPersonalPeriodoPago.Items.Add("Quincenal");
            cmbPersonalPeriodoPago.Items.Add("Semanal");
            cmbPersonalPeriodoPago.Items.Add("Diario");
            cmbPersonalPeriodoPago.SelectedIndex = 0;

            nudPersonalPagoInterno.ValueChanged -= PersonalPagoBase_Changed;
            nudPersonalPagoInterno.ValueChanged += PersonalPagoBase_Changed;
            nudPersonalHorasSemana.ValueChanged -= PersonalPagoBase_Changed;
            nudPersonalHorasSemana.ValueChanged += PersonalPagoBase_Changed;
            cmbPersonalPeriodoPago.SelectedIndexChanged -= PersonalPagoBase_Changed;
            cmbPersonalPeriodoPago.SelectedIndexChanged += PersonalPagoBase_Changed;

            lblPersonalCostoHoraCalculado.Dock = DockStyle.Fill;
            lblPersonalCostoHoraCalculado.TextAlign = ContentAlignment.MiddleLeft;
            lblPersonalCostoHoraCalculado.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPersonalCostoHoraCalculado.ForeColor = Color.FromArgb(38, 128, 96);

            chkPersonalActivo.Text = "Activo";
            chkPersonalActivo.Checked = true;
            chkPersonalActivo.AutoSize = true;

            clbPersonalCargosPosibles.CheckOnClick = true;
            clbPersonalCargosPosibles.Height = 92;
            clbPersonalCargosPosibles.BorderStyle = BorderStyle.FixedSingle;
            clbPersonalTrabajosPosibles.CheckOnClick = true;
            clbPersonalTrabajosPosibles.Height = 120;
            clbPersonalTrabajosPosibles.BorderStyle = BorderStyle.FixedSingle;

            AgregarFilaEditorPersonal(layout, 1, "Id interno", txtPersonalId);
            AgregarFilaEditorPersonal(layout, 2, "Nombre visible", txtPersonalNombre);
            AgregarFilaEditorPersonal(layout, 3, "Cargo principal", cmbPersonalCargoPrincipal);
            AgregarFilaEditorPersonal(layout, 4, "Cargos que puede tomar", clbPersonalCargosPosibles);
            AgregarFilaEditorPersonal(layout, 5, "Trabajos que sabe hacer", clbPersonalTrabajosPosibles);
            AgregarFilaEditorPersonal(layout, 6, "Pago interno", nudPersonalPagoInterno);
            AgregarFilaEditorPersonal(layout, 7, "Periodo de pago", cmbPersonalPeriodoPago);
            AgregarFilaEditorPersonal(layout, 8, "Horas por semana", nudPersonalHorasSemana);
            AgregarFilaEditorPersonal(layout, 9, "Costo hora calculado", lblPersonalCostoHoraCalculado);
            AgregarFilaEditorPersonal(layout, 10, "Horas max. por tanda", nudPersonalHorasMaximas);
            AgregarFilaEditorPersonal(layout, 11, "Estado", chkPersonalActivo);
            AgregarFilaEditorPersonal(layout, 12, "Notas", txtPersonalNotas);

            card.Controls.Add(layout);
            return card;
        }

        private Panel CrearCardPersonal()
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(16);
            card.Margin = new Padding(0);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            return card;
        }

        private Button CrearBotonPersonal(string texto)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Width = 130;
            boton.Height = 32;
            boton.Margin = new Padding(0, 0, 8, 0);
            boton.FlatStyle = FlatStyle.Flat;
            boton.BackColor = Color.White;
            boton.ForeColor = Color.FromArgb(25, 25, 25);
            boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            boton.FlatAppearance.BorderColor = Color.FromArgb(150, 160, 170);
            boton.FlatAppearance.BorderSize = 1;
            return boton;
        }

        private void AgregarFilaEditorPersonal(
            TableLayoutPanel layout,
            int fila,
            string etiqueta,
            Control control
        )
        {
            Label label = new Label();
            label.Text = etiqueta;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            label.Margin = new Padding(0, 0, 8, 8);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 0, 0, 8);

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(label, 0, fila);
            layout.Controls.Add(control, 1, fila);
        }

        private void ConfigurarNumericPersonal(
            NumericUpDown control,
            decimal minimo,
            decimal maximo,
            decimal valor
        )
        {
            control.Minimum = minimo;
            control.Maximum = maximo;
            control.DecimalPlaces = 2;
            control.ThousandsSeparator = true;
            control.Value = Math.Min(Math.Max(valor, minimo), maximo);
        }

        private void AplicarEstiloGrillaPersonal(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 30;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 171, 135);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(238, 241, 245);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(22, 24, 29);
        }

        private void CargarCargosEnEditorPersonal()
        {
            List<string> cargos = BibliotecaCargosJsonService.CargarCargos()
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.NombreCompleto))
                .Select(c => c.NombreCompleto.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            cmbPersonalCargoPrincipal.Items.Clear();
            cmbPersonalCargoPrincipal.Items.Add("");
            clbPersonalCargosPosibles.Items.Clear();
            clbPersonalTrabajosPosibles.Items.Clear();

            foreach (string cargo in cargos)
            {
                cmbPersonalCargoPrincipal.Items.Add(cargo);
                clbPersonalCargosPosibles.Items.Add(cargo, false);
            }

            foreach (string trabajo in ObtenerTrabajosProductivosParaPersonal())
            {
                clbPersonalTrabajosPosibles.Items.Add(trabajo, false);
            }
        }

        private List<string> ObtenerTrabajosProductivosParaPersonal()
        {
            try
            {
                return BibliotecaEcuacionesProductivasJsonService.CargarEcuaciones()
                    .Where(e => e != null && e.Activa && !string.IsNullOrWhiteSpace(e.NombreVisible))
                    .Select(e => e.NombreVisible.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(t => t)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void CargarTablaPersonalEmpresa()
        {
            cargandoPersonalEmpresa = true;
            dgvPersonalEmpresa.Rows.Clear();

            foreach (PersonaEquipo persona in BibliotecaPersonalEmpresaJsonService.CargarPersonal()
                .OrderByDescending(p => p.Activo)
                .ThenBy(p => p.Nombre))
            {
                int rowIndex = dgvPersonalEmpresa.Rows.Add();
                DataGridViewRow row = dgvPersonalEmpresa.Rows[rowIndex];
                row.Cells["Id"].Value = persona.Id;
                row.Cells["Nombre"].Value = persona.Nombre;
                row.Cells["CargoPrincipal"].Value = persona.CargoPrincipal;
                row.Cells["CargosPosibles"].Value = string.Join("; ", persona.CargosPosibles ?? new List<string>());
                row.Cells["TrabajosPosibles"].Value = string.Join("; ", persona.TrabajosPosibles ?? new List<string>());
                row.Cells["PagoInterno"].Value = FormatearPagoPersonal(persona.PagoInterno, persona.PeriodoPago);
                row.Cells["CostoHora"].Value = persona.CostoHora <= 0 ? "" : FormatearMiles((double)persona.CostoHora) + " CLP/h";
                row.Cells["HorasMaximas"].Value = persona.HorasMaximasPorTanda;
                row.Cells["Activo"].Value = persona.Activo;
            }

            cargandoPersonalEmpresa = false;

            if (dgvPersonalEmpresa.Rows.Count == 0)
            {
                LimpiarEditorPersonal(true);
            }
            else
            {
                dgvPersonalEmpresa.ClearSelection();
                dgvPersonalEmpresa.Rows[0].Selected = true;
                dgvPersonalEmpresa.CurrentCell = dgvPersonalEmpresa.Rows[0].Cells["Nombre"];
                CargarPersonaSeleccionadaEnEditor();
            }
        }

        private void CargarPersonaSeleccionadaEnEditor()
        {
            if (cargandoPersonalEmpresa)
            {
                return;
            }

            string id = ObtenerIdPersonaSeleccionadaListado();
            PersonaEquipo persona = BibliotecaPersonalEmpresaJsonService.CargarPersonal()
                .FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

            if (persona == null)
            {
                return;
            }

            txtPersonalId.Text = persona.Id;
            txtPersonalNombre.Text = persona.Nombre;
            cmbPersonalCargoPrincipal.Text = persona.CargoPrincipal;
            nudPersonalPagoInterno.Value = LimitarNumericPersonal(nudPersonalPagoInterno, persona.PagoInterno);
            cmbPersonalPeriodoPago.Text = string.IsNullOrWhiteSpace(persona.PeriodoPago) ? "Mensual" : persona.PeriodoPago;
            nudPersonalHorasSemana.Value = LimitarNumericPersonal(nudPersonalHorasSemana, persona.HorasTrabajoSemana <= 0 ? 42m : persona.HorasTrabajoSemana);
            nudPersonalHorasMaximas.Value = LimitarNumericPersonal(nudPersonalHorasMaximas, persona.HorasMaximasPorTanda <= 0 ? 42m : persona.HorasMaximasPorTanda);
            chkPersonalActivo.Checked = persona.Activo;
            txtPersonalNotas.Text = persona.Notas;
            MarcarCargosPosiblesPersonal(persona.CargosPosibles ?? new List<string>());
            MarcarTrabajosPosiblesPersonal(persona.TrabajosPosibles ?? new List<string>());
            ActualizarCostoHoraCalculadoPersonal();
        }

        private decimal LimitarNumericPersonal(NumericUpDown control, decimal valor)
        {
            if (valor < control.Minimum)
            {
                return control.Minimum;
            }

            if (valor > control.Maximum)
            {
                return control.Maximum;
            }

            return valor;
        }

        private void MarcarCargosPosiblesPersonal(List<string> cargos)
        {
            HashSet<string> seleccionados = new HashSet<string>(
                cargos ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase
            );

            for (int i = 0; i < clbPersonalCargosPosibles.Items.Count; i++)
            {
                string cargo = Convert.ToString(clbPersonalCargosPosibles.Items[i]) ?? "";
                clbPersonalCargosPosibles.SetItemChecked(i, seleccionados.Contains(cargo));
            }
        }

        private void MarcarTrabajosPosiblesPersonal(List<string> trabajos)
        {
            HashSet<string> seleccionados = new HashSet<string>(
                trabajos ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase
            );

            for (int i = 0; i < clbPersonalTrabajosPosibles.Items.Count; i++)
            {
                string trabajo = Convert.ToString(clbPersonalTrabajosPosibles.Items[i]) ?? "";
                clbPersonalTrabajosPosibles.SetItemChecked(i, seleccionados.Contains(trabajo));
            }
        }

        private void LimpiarEditorPersonal(bool crearId)
        {
            List<PersonaEquipo> personas = BibliotecaPersonalEmpresaJsonService.CargarPersonal();

            txtPersonalId.Text = crearId
                ? BibliotecaPersonalEmpresaJsonService.CrearIdUnico(personas)
                : "";
            txtPersonalNombre.Text = "";
            cmbPersonalCargoPrincipal.Text = "";
            nudPersonalPagoInterno.Value = 0;
            cmbPersonalPeriodoPago.Text = "Mensual";
            nudPersonalHorasSemana.Value = 42;
            nudPersonalHorasMaximas.Value = 42;
            chkPersonalActivo.Checked = true;
            txtPersonalNotas.Text = "";
            MarcarCargosPosiblesPersonal(new List<string>());
            MarcarTrabajosPosiblesPersonal(new List<string>());
            ActualizarCostoHoraCalculadoPersonal();
        }

        private void NuevoPersonalSeguro()
        {
            if (EditorPersonalTieneDatos())
            {
                DialogResult respuesta = MessageBox.Show(
                    "Crear una persona nueva limpia la ficha actual. Si no guardaste los cambios, se perderan.\n\nQuieres continuar?",
                    "Nueva persona",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (respuesta != DialogResult.Yes)
                {
                    return;
                }
            }

            LimpiarEditorPersonal(true);
        }

        private bool EditorPersonalTieneDatos()
        {
            if (!string.IsNullOrWhiteSpace(txtPersonalNombre.Text))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(cmbPersonalCargoPrincipal.Text))
            {
                return true;
            }

            if (nudPersonalPagoInterno.Value > 0 || nudPersonalHorasSemana.Value != 42 || nudPersonalHorasMaximas.Value != 42)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(txtPersonalNotas.Text))
            {
                return true;
            }

            return clbPersonalCargosPosibles.CheckedItems.Count > 0 ||
                   clbPersonalTrabajosPosibles.CheckedItems.Count > 0;
        }

        private void DuplicarPersonaSeleccionada()
        {
            string id = ObtenerIdPersonaSeleccionadaListado();

            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            List<PersonaEquipo> personas = BibliotecaPersonalEmpresaJsonService.CargarPersonal();
            PersonaEquipo origen = personas
                .FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

            if (origen == null)
            {
                MessageBox.Show(
                    "Selecciona una persona guardada para duplicarla.",
                    "Duplicar persona",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            PersonaEquipo copia = new PersonaEquipo
            {
                Id = BibliotecaPersonalEmpresaJsonService.CrearIdUnico(personas),
                Nombre = CrearNombreCopiaPersonal(origen.Nombre, personas),
                CargoPrincipal = origen.CargoPrincipal,
                CargosPosibles = new List<string>(origen.CargosPosibles ?? new List<string>()),
                TrabajosPosibles = new List<string>(origen.TrabajosPosibles ?? new List<string>()),
                PagoInterno = origen.PagoInterno,
                PeriodoPago = origen.PeriodoPago,
                HorasTrabajoSemana = origen.HorasTrabajoSemana <= 0 ? 42m : origen.HorasTrabajoSemana,
                CostoHora = origen.CostoHora,
                TarifaHora = origen.TarifaHora,
                HorasMaximasPorTanda = origen.HorasMaximasPorTanda <= 0 ? 42m : origen.HorasMaximasPorTanda,
                Activo = origen.Activo,
                Notas = origen.Notas
            };

            personas.Add(copia);
            BibliotecaPersonalEmpresaJsonService.GuardarPersonal(personas);
            CargarTablaPersonalEmpresa();
            SeleccionarPersonaPorId(copia.Id);
        }

        private string CrearNombreCopiaPersonal(string nombreBase, List<PersonaEquipo> personas)
        {
            string baseLimpia = string.IsNullOrWhiteSpace(nombreBase)
                ? "Persona"
                : nombreBase.Trim();

            string candidato = baseLimpia + " copia";

            if (!personas.Any(p => string.Equals(p.Nombre, candidato, StringComparison.OrdinalIgnoreCase)))
            {
                return candidato;
            }

            for (int i = 2; i < 1000; i++)
            {
                candidato = baseLimpia + " copia (" + i.ToString() + ")";

                if (!personas.Any(p => string.Equals(p.Nombre, candidato, StringComparison.OrdinalIgnoreCase)))
                {
                    return candidato;
                }
            }

            return baseLimpia + " copia " + DateTime.Now.ToString("HHmmss");
        }

        private void GuardarPersonaDesdeEditor()
        {
            string nombre = txtPersonalNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show(
                    "Ingresa un nombre visible para la persona.",
                    "Personal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            List<PersonaEquipo> personas = BibliotecaPersonalEmpresaJsonService.CargarPersonal();
            string id = txtPersonalId.Text.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                id = BibliotecaPersonalEmpresaJsonService.CrearIdUnico(personas);
                txtPersonalId.Text = id;
            }

            PersonaEquipo persona = personas
                .FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

            if (persona == null)
            {
                persona = new PersonaEquipo();
                persona.Id = id;
                personas.Add(persona);
            }

            persona.Nombre = nombre;
            persona.CargoPrincipal = cmbPersonalCargoPrincipal.Text.Trim();
            persona.CargosPosibles = ObtenerCargosMarcadosPersonal();
            persona.TrabajosPosibles = ObtenerTrabajosMarcadosPersonal();
            persona.PagoInterno = nudPersonalPagoInterno.Value;
            persona.PeriodoPago = string.IsNullOrWhiteSpace(cmbPersonalPeriodoPago.Text)
                ? "Mensual"
                : cmbPersonalPeriodoPago.Text.Trim();
            persona.HorasTrabajoSemana = nudPersonalHorasSemana.Value <= 0 ? 42m : nudPersonalHorasSemana.Value;
            persona.CostoHora = CalcularCostoHoraPersonal(persona.PagoInterno, persona.PeriodoPago, persona.HorasTrabajoSemana);
            persona.TarifaHora = persona.CostoHora;
            persona.HorasMaximasPorTanda = nudPersonalHorasMaximas.Value <= 0 ? 42m : nudPersonalHorasMaximas.Value;
            persona.Activo = chkPersonalActivo.Checked;
            persona.Notas = txtPersonalNotas.Text.Trim();

            BibliotecaPersonalEmpresaJsonService.GuardarPersonal(personas);
            CargarTablaPersonalEmpresa();
            SeleccionarPersonaPorId(id);
        }

        private List<string> ObtenerCargosMarcadosPersonal()
        {
            List<string> cargos = new List<string>();

            foreach (object item in clbPersonalCargosPosibles.CheckedItems)
            {
                string cargo = Convert.ToString(item) ?? "";

                if (!string.IsNullOrWhiteSpace(cargo))
                {
                    cargos.Add(cargo.Trim());
                }
            }

            return cargos
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();
        }

        private List<string> ObtenerTrabajosMarcadosPersonal()
        {
            List<string> trabajos = new List<string>();

            foreach (object item in clbPersonalTrabajosPosibles.CheckedItems)
            {
                string trabajo = Convert.ToString(item) ?? "";

                if (!string.IsNullOrWhiteSpace(trabajo))
                {
                    trabajos.Add(trabajo.Trim());
                }
            }

            return trabajos
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t)
                .ToList();
        }

        private void PersonalPagoBase_Changed(object sender, EventArgs e)
        {
            ActualizarCostoHoraCalculadoPersonal();
        }

        private void ActualizarCostoHoraCalculadoPersonal()
        {
            decimal costoHora = CalcularCostoHoraPersonal(
                nudPersonalPagoInterno.Value,
                cmbPersonalPeriodoPago.Text,
                nudPersonalHorasSemana.Value
            );

            lblPersonalCostoHoraCalculado.Text = costoHora <= 0
                ? "No definido"
                : FormatearMiles((double)costoHora) + " CLP/h";
        }

        private decimal CalcularCostoHoraPersonal(decimal pago, string periodo, decimal horasSemana)
        {
            decimal horasPeriodo = ObtenerHorasPeriodoPersonal(periodo, horasSemana);
            return pago <= 0 || horasPeriodo <= 0
                ? 0
                : Math.Round(pago / horasPeriodo, 2);
        }

        private decimal ObtenerHorasPeriodoPersonal(string periodo, decimal horasSemana)
        {
            horasSemana = horasSemana <= 0 ? 42m : horasSemana;
            string valor = (periodo ?? "").Trim().ToLowerInvariant();

            if (valor == "semanal")
            {
                return horasSemana;
            }

            if (valor == "quincenal")
            {
                return horasSemana * 2m;
            }

            if (valor == "diario")
            {
                return Math.Max(1m, horasSemana / 5m);
            }

            return horasSemana * 4m;
        }

        private string FormatearPagoPersonal(decimal pago, string periodo)
        {
            return pago <= 0
                ? ""
                : FormatearMiles((double)pago) + " CLP / " + (string.IsNullOrWhiteSpace(periodo) ? "Mensual" : periodo);
        }

        private void SeleccionarPersonaPorId(string id)
        {
            foreach (DataGridViewRow row in dgvPersonalEmpresa.Rows)
            {
                if (string.Equals(Convert.ToString(row.Cells["Id"].Value), id, StringComparison.OrdinalIgnoreCase))
                {
                    dgvPersonalEmpresa.ClearSelection();
                    row.Selected = true;
                    dgvPersonalEmpresa.CurrentCell = row.Cells["Nombre"];
                    CargarPersonaSeleccionadaEnEditor();
                    return;
                }
            }
        }

        private void DesactivarPersonaSeleccionada()
        {
            string id = ObtenerIdPersonaSeleccionadaListado();

            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            List<PersonaEquipo> personas = BibliotecaPersonalEmpresaJsonService.CargarPersonal();
            PersonaEquipo persona = personas
                .FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

            if (persona == null)
            {
                return;
            }

            DialogResult respuesta = MessageBox.Show(
                "Quieres desactivar esta persona? Se conserva en el JSON para historial.",
                "Personal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (respuesta != DialogResult.Yes)
            {
                return;
            }

            persona.Activo = false;
            BibliotecaPersonalEmpresaJsonService.GuardarPersonal(personas);
            CargarTablaPersonalEmpresa();
            SeleccionarPersonaPorId(id);
        }

        private ContextMenuStrip CrearMenuContextualPersonalEmpresa()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem duplicar = new ToolStripMenuItem("Duplicar persona");
            duplicar.Click += (s, e) => DuplicarPersonaSeleccionada();

            ToolStripMenuItem desactivar = new ToolStripMenuItem("Eliminar / desactivar");
            desactivar.Click += (s, e) => DesactivarPersonaSeleccionada();

            menu.Items.Add(duplicar);
            menu.Items.Add(desactivar);

            menu.Opening += (s, e) =>
            {
                bool haySeleccion = !string.IsNullOrWhiteSpace(ObtenerIdPersonaSeleccionadaListado());
                duplicar.Enabled = haySeleccion;
                desactivar.Enabled = haySeleccion;
            };

            return menu;
        }

        private void DgvPersonalEmpresa_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0)
            {
                return;
            }

            dgvPersonalEmpresa.ClearSelection();
            dgvPersonalEmpresa.Rows[e.RowIndex].Selected = true;

            int columna = e.ColumnIndex >= 0
                ? e.ColumnIndex
                : dgvPersonalEmpresa.Columns["Nombre"].Index;

            dgvPersonalEmpresa.CurrentCell = dgvPersonalEmpresa.Rows[e.RowIndex].Cells[columna];
            CargarPersonaSeleccionadaEnEditor();
        }

        private string ObtenerIdPersonaSeleccionadaListado()
        {
            if (dgvPersonalEmpresa == null)
            {
                return "";
            }

            DataGridViewRow row = null;

            if (dgvPersonalEmpresa.CurrentRow != null && dgvPersonalEmpresa.CurrentRow.Index >= 0)
            {
                row = dgvPersonalEmpresa.CurrentRow;
            }
            else if (dgvPersonalEmpresa.SelectedRows.Count > 0)
            {
                row = dgvPersonalEmpresa.SelectedRows[0];
            }

            if (row == null || !dgvPersonalEmpresa.Columns.Contains("Id"))
            {
                return "";
            }

            return Convert.ToString(row.Cells["Id"].Value) ?? "";
        }
    }
}
