$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabDatos.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabDatos.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$inicioFirma = "        private Control CrearFilaProductoProyectoEnDatos(ItemProyecto item, ProyectoConsolidado consolidado)"
$finFirma = "        private void SincronizarProyectoProductivoActualDesdeCotizacion()"

$inicio = $contenido.IndexOf($inicioFirma, [StringComparison]::Ordinal)
$fin = $contenido.IndexOf($finFirma, [StringComparison]::Ordinal)

if ($inicio -lt 0 -or $fin -le $inicio) {
    throw "No se encontro el metodo CrearFilaProductoProyectoEnDatos. No se modifico nada."
}

$metodo = $contenido.Substring($inicio, $fin - $inicio)

$reemplazos = @{
    'layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270));' =
    'layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252));'

    'acciones.AutoSize = true;' =
    'acciones.AutoSize = false;'

    'acciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;' =
    'acciones.Width = 248;
            acciones.Height = 38;
            acciones.Dock = DockStyle.Fill;'

    'acciones.MinimumSize = new Size(260, 38);' =
    'acciones.MinimumSize = new Size(248, 38);'

    'Button editar = CrearBotonAccionAlcanceDatos("Editar", 78);' =
    'Button editar = CrearBotonAccionAlcanceDatos("Editar", 70);'

    'editar.MinimumSize = new Size(78, 32);' =
    'editar.AutoSize = false;
            editar.Width = 70;
            editar.MinimumSize = new Size(70, 32);'

    'editar.Margin = new Padding(0, 0, 7, 5);' =
    'editar.Margin = new Padding(0, 0, 5, 5);'

    'Button pipeline = CrearBotonAccionAlcanceDatos("Pipeline", 88);' =
    'Button pipeline = CrearBotonAccionAlcanceDatos("Pipeline", 82);'

    'pipeline.MinimumSize = new Size(88, 32);' =
    'pipeline.AutoSize = false;
            pipeline.Width = 82;
            pipeline.MinimumSize = new Size(82, 32);'

    'pipeline.Margin = new Padding(0, 0, 7, 5);' =
    'pipeline.Margin = new Padding(0, 0, 5, 5);'

    'Button quitar = CrearBotonAccionAlcanceDatos("Quitar", 78);' =
    'Button quitar = CrearBotonAccionAlcanceDatos("Quitar", 70);'

    'quitar.MinimumSize = new Size(78, 32);' =
    'quitar.AutoSize = false;
            quitar.Width = 70;
            quitar.MinimumSize = new Size(70, 32);'
}

foreach ($par in $reemplazos.GetEnumerator()) {
    if ($metodo.Contains($par.Key)) {
        $metodo = $metodo.Replace($par.Key, $par.Value)
    }
}

if ($metodo -eq $contenido.Substring($inicio, $fin - $inicio)) {
    Write-Host ""
    Write-Host "No se encontraron cambios pendientes o el ajuste ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_botones_compactos_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $metodo +
    $contenido.Substring($fin)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Botones del alcance compactados correctamente." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
