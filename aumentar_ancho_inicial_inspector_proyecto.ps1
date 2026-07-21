$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

# 1) Aumentar el ancho inicial del inspector.
$contenido = [regex]::Replace(
    $contenido,
    'private const int AnchoInicialInspectorProyecto\s*=\s*\d+;',
    'private const int AnchoInicialInspectorProyecto = 520;',
    1
)

# 2) Evitar que un ancho antiguo demasiado pequeno se conserve.
$contenido = [regex]::Replace(
    $contenido,
    'private int anchoInspectorProyecto\s*=\s*AnchoInicialInspectorProyecto;',
    'private int anchoInspectorProyecto = AnchoInicialInspectorProyecto;',
    1
)

# 3) Subir los minimos reales del panel derecho.
$contenido = [regex]::Replace(
    $contenido,
    'const int minPanel2Deseado\s*=\s*\d+;',
    'const int minPanel2Deseado = 460;',
    1
)

# 4) Antes de calcular el inspector, corregir valores absurdamente pequenos.
$patron = 'int inspector = Math\.Min\(Math\.Max\(minPanel2Real,\s*anchoInspectorProyecto\),\s*inspectorMaximo\);'
$reemplazo = @'
if (anchoInspectorProyecto < 420)
                {
                    anchoInspectorProyecto = AnchoInicialInspectorProyecto;
                }

                int inspector = Math.Min(
                    Math.Max(minPanel2Real, anchoInspectorProyecto),
                    inspectorMaximo
                );
'@

$contenido = [regex]::Replace(
    $contenido,
    $patron,
    $reemplazo,
    1
)

# 5) Cuando se construye la pestana y el inspector esta visible,
# aplicar el ancho una vez que WinForms termina de medir.
$ancla = 'split.Panel2Collapsed = !inspectorProyectoVisible;'
$inyeccion = @'
split.Panel2Collapsed = !inspectorProyectoVisible;

            if (inspectorProyectoVisible)
            {
                BeginInvoke(new Action(() =>
                {
                    if (split.IsDisposed || split.Panel2Collapsed)
                    {
                        return;
                    }

                    int anchoDisponible = split.ClientSize.Width - split.SplitterWidth;
                    if (anchoDisponible <= 0)
                    {
                        return;
                    }

                    int anchoDeseado = Math.Max(460, anchoInspectorProyecto);
                    anchoDeseado = Math.Min(anchoDeseado, Math.Max(460, anchoDisponible - 620));

                    if (anchoDeseado > 0)
                    {
                        split.SplitterDistance = Math.Max(
                            split.Panel1MinSize,
                            anchoDisponible - anchoDeseado
                        );
                    }
                }));
            }
'@

if ($contenido.Contains($ancla) -and -not $contenido.Contains('int anchoDeseado = Math.Max(460, anchoInspectorProyecto);')) {
    $contenido = $contenido.Replace($ancla, $inyeccion)
}

if ($contenido -eq $original) {
    Write-Host ""
    Write-Host "No se encontraron cambios pendientes o el ajuste ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_inspector_ancho_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Ancho inicial del inspector aumentado correctamente." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
