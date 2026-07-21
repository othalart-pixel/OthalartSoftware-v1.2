$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido
$cambios = 0

# Eliminar llamadas prematuras a BeginInvoke agregadas dentro de ConstruirTabProyecto.
$patrones = @(
    '(?ms)^\s{12}if \(inspectorProyectoVisible\)\s*\{\s*BeginInvoke\(new Action\(\(\) => ForzarAnchoInicialInspectorProyecto\(split\)\)\);\s*\}\s*',
    '(?ms)^\s{12}if \(inspectorProyectoVisible\)\s*\{\s*BeginInvoke\(new Action\(\(\) =>\s*\{.*?\}\)\);\s*\}\s*'
)

foreach ($patron in $patrones) {
    $nuevo = [regex]::Replace($contenido, $patron, "", 1)
    if ($nuevo -ne $contenido) {
        $contenido = $nuevo
        $cambios++
    }
}

# Agregar HandleCreated si aun no existe.
$ancla = "            split.Panel2Collapsed = !inspectorProyectoVisible;"

if ($contenido.Contains($ancla) -and
    -not $contenido.Contains("split.HandleCreated += SplitProyecto_HandleCreated;")) {

    $inyeccion = @'
            split.Panel2Collapsed = !inspectorProyectoVisible;

            split.HandleCreated -= SplitProyecto_HandleCreated;
            split.HandleCreated += SplitProyecto_HandleCreated;
'@

    $contenido = $contenido.Replace($ancla, $inyeccion)
    $cambios++
}

# Insertar manejador seguro antes de VisibleChanged o del header.
$firma = "        private void SplitProyecto_HandleCreated(object sender, EventArgs e)"

if (-not $contenido.Contains($firma)) {
    $anclaMetodo = "        private void SplitProyecto_VisibleChanged(object sender, EventArgs e)"
    $indice = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)

    if ($indice -lt 0) {
        $anclaMetodo = "        private Control CrearHeaderProductosServicios()"
        $indice = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)
    }

    if ($indice -lt 0) {
        throw "No se encontro un lugar seguro para insertar el manejador. No se modifico el archivo."
    }

    $manejador = @'
        private void SplitProyecto_HandleCreated(object sender, EventArgs e)
        {
            if (sender is not SplitContainer split ||
                split.IsDisposed ||
                split.Panel2Collapsed ||
                !split.IsHandleCreated)
            {
                return;
            }

            split.BeginInvoke(new Action(() =>
            {
                if (!split.IsDisposed && split.IsHandleCreated)
                {
                    AplicarAnchoInicialInspectorCuandoEsteVisible();
                }
            }));
        }

'@

    $contenido =
        $contenido.Substring(0, $indice) +
        $manejador +
        $contenido.Substring($indice)

    $cambios++
}

# Proteger el VisibleChanged para que no invoque sin Handle.
$viejoVisible = @'
            if (sender is not SplitContainer split ||
                !split.Visible ||
                split.Panel2Collapsed ||
                anchoInicialInspectorProyectoAplicado)
'@

$nuevoVisible = @'
            if (sender is not SplitContainer split ||
                !split.IsHandleCreated ||
                !split.Visible ||
                split.Panel2Collapsed ||
                anchoInicialInspectorProyectoAplicado)
'@

if ($contenido.Contains($viejoVisible)) {
    $contenido = $contenido.Replace($viejoVisible, $nuevoVisible)
    $cambios++
}

if ($contenido -eq $original -or $cambios -eq 0) {
    Write-Host ""
    Write-Host "No se encontro la llamada prematura o el arreglo ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_fix_begininvoke_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "BeginInvoke prematuro corregido." -ForegroundColor Green
Write-Host "El inspector ahora se ajustara despues de HandleCreated."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
