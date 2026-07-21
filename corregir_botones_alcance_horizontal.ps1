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
    throw "No se encontro el metodo de la fila de producto. No se modifico nada."
}

$metodo = $contenido.Substring($inicio, $fin - $inicio)

# Mantener una columna fija suficiente para las tres acciones.
$metodoNuevo = $metodo.Replace(
    'layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));',
    'layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270));'
)

# Evitar que los botones salten a una columna vertical.
$metodoNuevo = $metodoNuevo.Replace(
    'acciones.WrapContents = true;',
    'acciones.WrapContents = false;
            acciones.MinimumSize = new Size(260, 38);
            acciones.Anchor = AnchorStyles.Top | AnchorStyles.Right;'
)

# Asegurar ancho total de la fila y altura útil.
$metodoNuevo = $metodoNuevo.Replace(
    'fila.MinimumSize = new Size(0, 72);',
    'fila.MinimumSize = new Size(760, 72);'
)

# Dar un poco más de aire inferior por seguridad con escalado alto.
$metodoNuevo = $metodoNuevo.Replace(
    'fila.Padding = new Padding(14, 10, 14, 10);',
    'fila.Padding = new Padding(14, 10, 14, 12);'
)

if ($metodoNuevo -eq $metodo) {
    Write-Host ""
    Write-Host "No se encontraron los bloques esperados o el arreglo ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_botones_alcance_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $metodoNuevo +
    $contenido.Substring($fin)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Botones del alcance alineados horizontalmente." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
