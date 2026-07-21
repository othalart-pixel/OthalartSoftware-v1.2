$ErrorActionPreference = "Stop"

$raiz = Get-Location

$archivo = Get-ChildItem -Path $raiz -Recurse -Filter "*.cs" |
    Where-Object {
        $texto = [System.IO.File]::ReadAllText($_.FullName)
        $texto.Contains("Crear nuevo proyecto") -and
        ($texto.Contains("Configuración de bibliotecas") -or
         $texto.Contains("Configuracion de bibliotecas"))
    } |
    Select-Object -First 1

if ($null -eq $archivo) {
    throw "No se encontro el archivo que construye la pantalla de inicio."
}

$ruta = $archivo.FullName
$contenido = [System.IO.File]::ReadAllText($ruta)

if ($contenido.Contains("private void RedisenarInicioDashboard(Control tab)")) {
    Write-Host "El rediseño ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$posTexto = $contenido.IndexOf("Crear nuevo proyecto", [StringComparison]::Ordinal)
$inicioMetodo = $contenido.LastIndexOf("        private ", $posTexto, [StringComparison]::Ordinal)
$llaveInicial = $contenido.IndexOf("{", $inicioMetodo, [StringComparison]::Ordinal)

if ($posTexto -lt 0 -or $inicioMetodo -lt 0 -or $llaveInicial -lt 0) {
    throw "No se pudo localizar el metodo de inicio."
}

$profundidad = 0
$finMetodo = -1
for ($i = $llaveInicial; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') { $profundidad++ }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--
        if ($profundidad -eq 0) {
            $finMetodo = $i
            break
        }
    }
}

if ($finMetodo -lt 0) {
    throw "No se pudo localizar el final del metodo de inicio."
}

$llamada = "`r`n            RedisenarInicioDashboard(tab);`r`n"
$contenido = $contenido.Substring(0, $finMetodo) + $llamada + $contenido.Substring($finMetodo)

$helpers = @'

        private void RedisenarInicioDashboard(Control tab)
        {
            if (tab == null || tab.IsDisposed)
            {
                return;
            }

            List<Control> controles = ObtenerControlesInicioRecursivos(tab).ToList();

            Button btnCrear = BuscarBotonInicio(controles, "Crear nuevo proyecto");
            Button btnAbrir = BuscarBotonInicio(controles, "Abrir proyecto");
            Button btnRevisar = BuscarBotonInicio(controles, "Revisar productos y servicios");

            Dictionary<string, Button> botones = new Dictionary<string, Button>(
                StringComparer.OrdinalIgnoreCase
            );

            string[] nombres =
            {
                "Productos", "Moneda", "Rendimientos", "Ecuaciones",
                "Gestiones", "Rangos", "Cargos", "Mano de obra",
                "Costos", "Personal", "Configurar bibliotecas"
            };

            foreach (string nombre in nombres)
            {
                Button boton = BuscarBotonInicio(controles, nombre);
                if (boton != null)
                {
                    botones[nombre] = boton;
                }
            }

            Control proyectos = BuscarBloqueProyectosGuardados(tab);

            Panel dashboard = new Panel();
            dashboard.Dock = DockStyle.Fill;
            dashboard.AutoScroll = true;
            dashboard.BackColor = Color.FromArgb(246, 248, 251);
            dashboard.Padding = new Padding(28, 24, 28, 28);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Top;
            root.AutoSize = true;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.BackColor = Color.Transparent;

            TableLayoutPanel hero = new TableLayoutPanel();
            hero.Dock = DockStyle.Top;
            hero.AutoSize = true;
            hero.ColumnCount = 1;
            hero.RowCount = 3;
            hero.Margin = new Padding(0, 0, 0, 18);

            Label titulo = new Label();
            titulo.Text = "Othalart";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 24f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(24, 32, 44);

            Label subtitulo = new Label();
            subtitulo.Text = "Gestión productiva, costos y planificación para estudios creativos.";
            subtitulo.AutoSize = true;
            subtitulo.MaximumSize = new Size(1000, 0);
            subtitulo.Font = new Font("Segoe UI", 10.5f);
            subtitulo.ForeColor = Color.FromArgb(93, 103, 116);
            subtitulo.Margin = new Padding(0, 3, 0, 14);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.WrapContents = true;

            if (btnCrear != null)
            {
                PrepararBotonDashboard(btnCrear, "+ Crear nuevo proyecto", true, 200);
                acciones.Controls.Add(btnCrear);
            }

            if (btnAbrir != null)
            {
                PrepararBotonDashboard(btnAbrir, "Abrir proyecto existente", false, 210);
                acciones.Controls.Add(btnAbrir);
            }

            if (btnRevisar != null)
            {
                PrepararBotonDashboard(btnRevisar, "Revisar productos y servicios", false, 230);
                acciones.Controls.Add(btnRevisar);
            }

            hero.Controls.Add(titulo, 0, 0);
            hero.Controls.Add(subtitulo, 0, 1);
            hero.Controls.Add(acciones, 0, 2);

            TableLayoutPanel cuerpo = new TableLayoutPanel();
            cuerpo.Dock = DockStyle.Top;
            cuerpo.AutoSize = true;
            cuerpo.ColumnCount = 2;
            cuerpo.RowCount = 1;
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            cuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            cuerpo.Margin = new Padding(0, 0, 0, 18);

            cuerpo.Controls.Add(CrearTarjetaProyectosRecientes(proyectos), 0, 0);
            cuerpo.Controls.Add(CrearTarjetaBibliotecasDashboard(botones), 1, 0);

            Panel ayuda = new Panel();
            ayuda.Dock = DockStyle.Top;
            ayuda.Height = 70;
            ayuda.Padding = new Padding(16, 12, 16, 12);
            ayuda.BackColor = Color.FromArgb(236, 244, 250);
            ayuda.BorderStyle = BorderStyle.FixedSingle;

            Label txtAyuda = new Label();
            txtAyuda.Dock = DockStyle.Fill;
            txtAyuda.Text = "Abre un proyecto para activar el Gantt, el resumen comercial y el desglose productivo.";
            txtAyuda.Font = new Font("Segoe UI", 9.5f);
            txtAyuda.ForeColor = Color.FromArgb(53, 83, 111);
            txtAyuda.TextAlign = ContentAlignment.MiddleLeft;
            ayuda.Controls.Add(txtAyuda);

            root.Controls.Add(hero, 0, 0);
            root.Controls.Add(cuerpo, 0, 1);
            root.Controls.Add(ayuda, 0, 2);

            dashboard.Controls.Add(root);

            tab.SuspendLayout();
            try
            {
                tab.Controls.Clear();
                tab.BackColor = dashboard.BackColor;
                tab.Controls.Add(dashboard);
            }
            finally
            {
                tab.ResumeLayout(true);
            }
        }

        private Control CrearTarjetaProyectosRecientes(Control bloqueOriginal)
        {
            TableLayoutPanel tarjeta = CrearTarjetaDashboardBase(
                "Continuar trabajando",
                "Tus proyectos recientes y accesos rápidos."
            );

            Panel contenido = new Panel();
            contenido.Dock = DockStyle.Top;
            contenido.AutoSize = true;
            contenido.MinimumSize = new Size(0, 235);
            contenido.BackColor = Color.White;

            if (bloqueOriginal != null)
            {
                bloqueOriginal.Dock = DockStyle.Top;
                bloqueOriginal.AutoSize = true;
                bloqueOriginal.Margin = new Padding(0);
                bloqueOriginal.Padding = new Padding(0);
                bloqueOriginal.BackColor = Color.White;
                EstilizarControlesProyectosRecientes(bloqueOriginal);
                contenido.Controls.Add(bloqueOriginal);
            }
            else
            {
                contenido.Controls.Add(new Label
                {
                    Dock = DockStyle.Top,
                    Height = 90,
                    Text = "Todavía no hay proyectos recientes.\r\nCrea o abre uno para comenzar.",
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.FromArgb(105, 112, 122),
                    Font = new Font("Segoe UI", 9.5f)
                });
            }

            tarjeta.Controls.Add(contenido, 0, 2);
            return tarjeta;
        }

        private Control CrearTarjetaBibliotecasDashboard(
            Dictionary<string, Button> botones)
        {
            TableLayoutPanel tarjeta = CrearTarjetaDashboardBase(
                "Configurar el sistema",
                "Bibliotecas maestras organizadas por función."
            );

            TableLayoutPanel grupos = new TableLayoutPanel();
            grupos.Dock = DockStyle.Top;
            grupos.AutoSize = true;
            grupos.ColumnCount = 3;
            grupos.RowCount = 1;
            grupos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grupos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            grupos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            grupos.Controls.Add(CrearGrupoBibliotecaDashboard(
                "Producción",
                new[] { "Productos", "Ecuaciones", "Rendimientos", "Gestiones" },
                botones), 0, 0);

            grupos.Controls.Add(CrearGrupoBibliotecaDashboard(
                "Equipo y costos",
                new[] { "Cargos", "Personal", "Mano de obra", "Rangos", "Costos" },
                botones), 1, 0);

            grupos.Controls.Add(CrearGrupoBibliotecaDashboard(
                "Sistema",
                new[] { "Moneda", "Configurar bibliotecas" },
                botones), 2, 0);

            tarjeta.Controls.Add(grupos, 0, 2);
            return tarjeta;
        }

        private TableLayoutPanel CrearTarjetaDashboardBase(
            string titulo,
            string descripcion)
        {
            TableLayoutPanel tarjeta = new TableLayoutPanel();
            tarjeta.Dock = DockStyle.Fill;
            tarjeta.AutoSize = true;
            tarjeta.ColumnCount = 1;
            tarjeta.RowCount = 3;
            tarjeta.Padding = new Padding(18, 16, 18, 18);
            tarjeta.Margin = new Padding(0, 0, 14, 0);
            tarjeta.BackColor = Color.White;
            tarjeta.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tarjeta.MinimumSize = new Size(330, 310);

            tarjeta.Controls.Add(new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 36, 48)
            }, 0, 0);

            tarjeta.Controls.Add(new Label
            {
                Text = descripcion,
                AutoSize = true,
                MaximumSize = new Size(520, 0),
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = Color.FromArgb(105, 112, 122),
                Margin = new Padding(0, 3, 0, 12)
            }, 0, 1);

            return tarjeta;
        }

        private Control CrearGrupoBibliotecaDashboard(
            string titulo,
            IEnumerable<string> nombres,
            Dictionary<string, Button> botones)
        {
            TableLayoutPanel grupo = new TableLayoutPanel();
            grupo.Dock = DockStyle.Fill;
            grupo.AutoSize = true;
            grupo.ColumnCount = 1;
            grupo.Padding = new Padding(10);
            grupo.Margin = new Padding(0, 0, 8, 0);
            grupo.BackColor = Color.FromArgb(248, 250, 252);
            grupo.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            grupo.Controls.Add(new Label
            {
                Text = titulo,
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 56, 72),
                Margin = new Padding(0, 0, 0, 8)
            });

            foreach (string nombre in nombres)
            {
                if (!botones.TryGetValue(nombre, out Button boton))
                {
                    continue;
                }

                PrepararBotonDashboard(boton, nombre, false, 0);
                boton.Dock = DockStyle.Top;
                boton.TextAlign = ContentAlignment.MiddleLeft;
                boton.Padding = new Padding(12, 0, 8, 0);
                boton.Margin = new Padding(0, 0, 0, 6);
                grupo.Controls.Add(boton);
            }

            return grupo;
        }

        private void PrepararBotonDashboard(
            Button boton,
            string texto,
            bool principal,
            int ancho)
        {
            boton.Text = texto;
            boton.Height = 38;
            if (ancho > 0)
            {
                boton.Width = ancho;
            }

            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = principal
                ? Color.FromArgb(0, 124, 105)
                : Color.FromArgb(190, 198, 207);
            boton.FlatAppearance.MouseOverBackColor = principal
                ? Color.FromArgb(0, 111, 95)
                : Color.FromArgb(242, 246, 249);
            boton.BackColor = principal
                ? Color.FromArgb(0, 124, 105)
                : Color.White;
            boton.ForeColor = principal
                ? Color.White
                : Color.FromArgb(35, 43, 54);
            boton.Font = new Font("Segoe UI", 9.4f, FontStyle.Bold);
            boton.UseVisualStyleBackColor = false;
            boton.Cursor = Cursors.Hand;
            boton.Margin = new Padding(0, 0, 10, 8);
        }

        private Control BuscarBloqueProyectosGuardados(Control raiz)
        {
            Label etiqueta = ObtenerControlesInicioRecursivos(raiz)
                .OfType<Label>()
                .FirstOrDefault(l =>
                    string.Equals(
                        (l.Text ?? "").Trim(),
                        "Proyectos guardados",
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if (etiqueta == null)
            {
                return null;
            }

            Control candidato = etiqueta.Parent;
            while (candidato != null && candidato != raiz)
            {
                if (candidato.Width >= 160 && candidato.Height >= 100)
                {
                    return candidato;
                }
                candidato = candidato.Parent;
            }

            return etiqueta.Parent;
        }

        private void EstilizarControlesProyectosRecientes(Control raiz)
        {
            foreach (Control control in ObtenerControlesInicioRecursivos(raiz))
            {
                if (control is Label label)
                {
                    if (string.Equals(
                        (label.Text ?? "").Trim(),
                        "Proyectos guardados",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        label.Visible = false;
                    }
                    else
                    {
                        label.Font = new Font("Segoe UI", 9.3f);
                        label.ForeColor = Color.FromArgb(54, 63, 74);
                    }
                }
                else if (control is Button boton)
                {
                    boton.FlatStyle = FlatStyle.Flat;
                    boton.FlatAppearance.BorderColor = Color.FromArgb(205, 211, 218);
                    boton.BackColor = Color.White;
                    boton.ForeColor = Color.FromArgb(35, 43, 54);
                    boton.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
                    boton.Height = Math.Max(36, boton.Height);
                    boton.Cursor = Cursors.Hand;
                    boton.UseVisualStyleBackColor = false;
                }
            }
        }

        private Button BuscarBotonInicio(
            IEnumerable<Control> controles,
            string texto)
        {
            return controles
                .OfType<Button>()
                .FirstOrDefault(b =>
                    string.Equals(
                        (b.Text ?? "").Trim(),
                        texto,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }

        private IEnumerable<Control> ObtenerControlesInicioRecursivos(Control raiz)
        {
            foreach (Control hijo in raiz.Controls)
            {
                yield return hijo;

                foreach (Control nieto in ObtenerControlesInicioRecursivos(hijo))
                {
                    yield return nieto;
                }
            }
        }
'@

$indiceHelpers = $finMetodo + $llamada.Length + 1
$contenido = $contenido.Substring(0, $indiceHelpers) + $helpers + $contenido.Substring($indiceHelpers)

$backup = "$ruta.backup_dashboard_inicio_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Dashboard de inicio aplicado correctamente." -ForegroundColor Green
Write-Host "Archivo modificado: $ruta"
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
