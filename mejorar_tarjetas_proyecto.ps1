$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontró UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raíz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("private void EstilizarTarjetasProyecto(Control raiz)")) {
    Write-Host ""
    Write-Host "La mejora visual de tarjetas ya parece estar aplicada." -ForegroundColor Yellow
    exit 0
}

# 1) Enganchar el estilizado al construir la pestaña.
$ancla = "            split.Panel2Collapsed = !inspectorProyectoVisible;"

if (-not $contenido.Contains($ancla)) {
    throw "No se encontró el punto de anclaje esperado en ConstruirTabProyecto."
}

$reemplazo = @'
            split.Panel2Collapsed = !inspectorProyectoVisible;

            tab.Enter -= TabProyecto_Enter_EstilizarTarjetas;
            tab.Enter += TabProyecto_Enter_EstilizarTarjetas;
            EstilizarTarjetasProyecto(tab);
'@

$contenido = $contenido.Replace($ancla, $reemplazo)

# 2) Insertar helpers antes del header de productos y servicios.
$anclaHelpers = "        private Control CrearHeaderProductosServicios()"
$indice = $contenido.IndexOf($anclaHelpers, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontró el punto para insertar helpers visuales."
}

$helpers = @'
        private void TabProyecto_Enter_EstilizarTarjetas(object sender, EventArgs e)
        {
            if (sender is Control raiz && !raiz.IsDisposed)
            {
                EstilizarTarjetasProyecto(raiz);
            }
        }

        private void EstilizarTarjetasProyecto(Control raiz)
        {
            if (raiz == null || raiz.IsDisposed)
            {
                return;
            }

            foreach (Control control in ObtenerControlesRecursivos(raiz))
            {
                if (control is Panel panel)
                {
                    bool tieneBotonOcultar = panel.Controls.OfType<Button>()
                        .Any(b => string.Equals(b.Text, "Ocultar", StringComparison.OrdinalIgnoreCase));

                    bool tieneBotonContraer = panel.Controls.OfType<Button>()
                        .Any(b => string.Equals(b.Text, "Contraer", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(b.Text, "Expandir", StringComparison.OrdinalIgnoreCase));

                    bool tieneMarcadorModificado = panel.Controls.OfType<Label>()
                        .Any(l => l.Text != null && l.Text.IndexOf("MODIFICADO", StringComparison.OrdinalIgnoreCase) >= 0);

                    bool pareceTarjetaPrincipal =
                        (tieneBotonContraer || tieneMarcadorModificado) &&
                        panel.Width >= 320;

                    bool pareceSubtarjeta =
                        tieneBotonOcultar &&
                        panel.Width >= 250;

                    if (pareceTarjetaPrincipal)
                    {
                        panel.BackColor = Color.White;
                        panel.Padding = new Padding(14, 12, 14, 12);
                        panel.Margin = new Padding(0, 0, 0, 12);
                        panel.BorderStyle = BorderStyle.FixedSingle;
                    }
                    else if (pareceSubtarjeta)
                    {
                        panel.BackColor = Color.FromArgb(252, 253, 255);
                        panel.Padding = new Padding(10, 8, 10, 8);
                        panel.Margin = new Padding(0, 0, 0, 8);
                        panel.BorderStyle = BorderStyle.FixedSingle;
                    }
                }

                if (control is Label label)
                {
                    string texto = (label.Text ?? string.Empty).Trim();

                    if (texto.StartsWith("v ", StringComparison.OrdinalIgnoreCase) ||
                        texto.StartsWith("▾ ", StringComparison.OrdinalIgnoreCase) ||
                        texto.StartsWith("▼ ", StringComparison.OrdinalIgnoreCase))
                    {
                        label.Font = new Font("Segoe UI", 10.6f, FontStyle.Bold);
                        label.ForeColor = Color.FromArgb(28, 32, 36);
                        label.AutoSize = true;
                    }
                    else if (texto.IndexOf("Plantilla global", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        label.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                        label.ForeColor = Color.FromArgb(110, 116, 124);
                        label.AutoSize = true;
                    }
                    else if (texto.IndexOf("segundos", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             texto.IndexOf("procesos", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             texto.IndexOf("Costo:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             texto.IndexOf("Tiempo:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        label.Font = new Font("Segoe UI", 9.2f, FontStyle.Regular);
                        label.ForeColor = Color.FromArgb(76, 84, 92);
                        label.AutoSize = true;
                    }
                    else if (texto.IndexOf("MODIFICADO", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        label.Font = new Font("Segoe UI", 9.3f, FontStyle.Bold);
                        label.ForeColor = Color.FromArgb(186, 104, 0);
                        label.AutoSize = true;
                    }
                    else if (texto.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                    {
                        label.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                        label.ForeColor = Color.FromArgb(92, 98, 106);
                        label.AutoSize = true;
                    }
                }

                if (control is Button boton)
                {
                    string texto = (boton.Text ?? string.Empty).Trim();

                    boton.FlatStyle = FlatStyle.Flat;
                    boton.FlatAppearance.BorderColor = Color.FromArgb(187, 194, 201);
                    boton.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 246, 250);
                    boton.BackColor = Color.White;
                    boton.ForeColor = Color.FromArgb(34, 38, 42);
                    boton.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    boton.Height = Math.Max(boton.Height, 30);
                    boton.Padding = new Padding(10, 0, 10, 0);
                    boton.UseVisualStyleBackColor = false;
                    boton.Cursor = Cursors.Hand;

                    if (string.Equals(texto, "Editar", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(texto, "Aplicar", StringComparison.OrdinalIgnoreCase))
                    {
                        boton.BackColor = Color.FromArgb(240, 247, 255);
                        boton.FlatAppearance.BorderColor = Color.FromArgb(145, 176, 214);
                    }

                    if (string.Equals(texto, "Ocultar", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(texto, "Contraer", StringComparison.OrdinalIgnoreCase))
                    {
                        boton.BackColor = Color.FromArgb(250, 250, 250);
                    }
                }
            }
        }

        private IEnumerable<Control> ObtenerControlesRecursivos(Control raiz)
        {
            foreach (Control hijo in raiz.Controls)
            {
                yield return hijo;

                foreach (Control nieto in ObtenerControlesRecursivos(hijo))
                {
                    yield return nieto;
                }
            }
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helpers +
    $contenido.Substring($indice)

$backup = "$ruta.backup_mejora_tarjetas_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Mejora visual de tarjetas aplicada correctamente." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
