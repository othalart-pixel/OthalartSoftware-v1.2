$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabDatos.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabDatos.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)

$inicioFirma = "        private Control CrearFilaProductoProyectoEnDatos(ItemProyecto item, ProyectoConsolidado consolidado)"
$finFirma = "        private void SincronizarProyectoProductivoActualDesdeCotizacion()"

$inicio = $contenido.IndexOf($inicioFirma, [StringComparison]::Ordinal)
$fin = $contenido.IndexOf($finFirma, [StringComparison]::Ordinal)

if ($inicio -lt 0 -or $fin -le $inicio) {
    throw "No se encontro el metodo CrearFilaProductoProyectoEnDatos. No se modifico nada."
}

$metodoOriginal = $contenido.Substring($inicio, $fin - $inicio)
$metodo = $metodoOriginal

# Reservar una columna mas ancha: el bloque comienza mas a la izquierda.
$metodo = [regex]::Replace(
    $metodo,
    'layout\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.Absolute,\s*\d+\)\);',
    'layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));',
    1
)

# Panel horizontal, alineado a la izquierda dentro de esa columna.
$metodo = [regex]::Replace(
    $metodo,
    'acciones\.Width\s*=\s*\d+;',
    'acciones.Width = 270;',
    1
)

$metodo = [regex]::Replace(
    $metodo,
    'acciones\.MinimumSize\s*=\s*new Size\(\d+,\s*38\);',
    'acciones.MinimumSize = new Size(270, 38);',
    1
)

$metodo = $metodo.Replace(
    'acciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;',
    'acciones.Anchor = AnchorStyles.Top | AnchorStyles.Left;'
)

# Restaurar textos y anchos normales.
$metodo = [regex]::Replace(
    $metodo,
    'Button editar = CrearBotonAccionAlcanceDatos\("Editar",\s*\d+\);',
    'Button editar = CrearBotonAccionAlcanceDatos("Editar", 78);',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'editar\.Width\s*=\s*\d+;',
    'editar.Width = 78;',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'editar\.MinimumSize\s*=\s*new Size\(\d+,\s*32\);',
    'editar.MinimumSize = new Size(78, 32);',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'editar\.Margin\s*=\s*new Padding\([^)]+\);',
    'editar.Margin = new Padding(0, 0, 7, 5);',
    1
)

$metodo = [regex]::Replace(
    $metodo,
    'Button pipeline = CrearBotonAccionAlcanceDatos\("Pipeline",\s*\d+\);',
    'Button pipeline = CrearBotonAccionAlcanceDatos("Pipeline", 88);',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'pipeline\.Width\s*=\s*\d+;',
    'pipeline.Width = 88;',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'pipeline\.MinimumSize\s*=\s*new Size\(\d+,\s*32\);',
    'pipeline.MinimumSize = new Size(88, 32);',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'pipeline\.Margin\s*=\s*new Padding\([^)]+\);',
    'pipeline.Margin = new Padding(0, 0, 7, 5);',
    1
)

$metodo = [regex]::Replace(
    $metodo,
    'Button quitar = CrearBotonAccionAlcanceDatos\("Quitar",\s*\d+\);',
    'Button quitar = CrearBotonAccionAlcanceDatos("Quitar", 78);',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'quitar\.Width\s*=\s*\d+;',
    'quitar.Width = 78;',
    1
)
$metodo = [regex]::Replace(
    $metodo,
    'quitar\.MinimumSize\s*=\s*new Size\(\d+,\s*32\);',
    'quitar.MinimumSize = new Size(78, 32);',
    1
)

# El helper general agrega padding; aqui lo quitamos para que el texto no se corte.
foreach ($nombre in @("editar", "pipeline", "quitar")) {
    $marcador = "$nombre.Cursor = Cursors.Hand;"
    if ($metodo.Contains($marcador) -and -not $metodo.Contains("$nombre.Padding = new Padding(0);")) {
        $metodo = $metodo.Replace(
            $marcador,
            "$nombre.Padding = new Padding(0);`r`n            $marcador"
        )
    }
}

if ($metodo -eq $metodoOriginal) {
    Write-Host ""
    Write-Host "No se encontraron cambios pendientes o el ajuste ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_mover_botones_izquierda_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $metodo +
    $contenido.Substring($fin)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Botones restaurados y movidos hacia la izquierda." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
