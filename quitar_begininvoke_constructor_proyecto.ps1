$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)

$firmaInicio = "        private void ConstruirTabProyecto(TabPage tab)"
$firmaFin = "        private Control CrearHeaderProductosServicios()"

$inicio = $contenido.IndexOf($firmaInicio, [StringComparison]::Ordinal)
$fin = $contenido.IndexOf($firmaFin, [StringComparison]::Ordinal)

if ($inicio -lt 0 -or $fin -le $inicio) {
    throw "No se pudo aislar ConstruirTabProyecto(). No se modifico nada."
}

$metodo = $contenido.Substring($inicio, $fin - $inicio)
$metodoOriginal = $metodo

# Eliminar bloques if que contienen BeginInvoke dentro de ConstruirTabProyecto.
$patronesBloque = @(
    '(?ms)^\s{12}if\s*\([^)]*inspectorProyectoVisible[^)]*\)\s*\{\s*BeginInvoke\s*\(.*?\);\s*\}\s*',
    '(?ms)^\s{12}if\s*\([^)]*\)\s*\{\s*BeginInvoke\s*\(.*?\);\s*\}\s*'
)

foreach ($patron in $patronesBloque) {
    $metodo = [regex]::Replace($metodo, $patron, "", 10)
}

# Eliminar cualquier BeginInvoke residual dentro del metodo, incluyendo lambdas multilínea.
$metodo = [regex]::Replace(
    $metodo,
    '(?ms)^\s*BeginInvoke\s*\(\s*new Action\s*\(\s*\(\)\s*=>\s*\{.*?\}\s*\)\s*\)\s*;\s*',
    "",
    10
)

$metodo = [regex]::Replace(
    $metodo,
    '(?ms)^\s*BeginInvoke\s*\(.*?\)\s*;\s*',
    "",
    10
)

if ($metodo.Contains("BeginInvoke")) {
    Write-Host ""
    Write-Host "Todavia hay BeginInvoke dentro de ConstruirTabProyecto. No se escribio el archivo." -ForegroundColor Red
    Write-Host ""
    Write-Host "Fragmentos encontrados:"
    ($metodo -split "`r?`n") |
        Select-String "BeginInvoke" |
        ForEach-Object { Write-Host $_.Line }
    exit 1
}

if ($metodo -eq $metodoOriginal) {
    Write-Host ""
    Write-Host "No se encontro ningun BeginInvoke dentro de ConstruirTabProyecto." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_quitar_begininvoke_constructor_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $metodo +
    $contenido.Substring($fin)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Se eliminaron todos los BeginInvoke de ConstruirTabProyecto." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila primero Debug: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
Write-Host 'Luego Release: dotnet build "OthalartSoftware v1.0.csproj" -c Release'
