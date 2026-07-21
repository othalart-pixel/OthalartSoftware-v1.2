$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este hotfix desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$patron = @'
                asignacion.OrigenHoras =
                    OrigenHorasProductivas.Calculada;
'@

if (-not $contenido.Contains($patron)) {
    throw "No se encontro la asignacion invalida OrigenHorasProductivas.Calculada."
}

$reemplazo = @'
                // Se conserva el origen existente. La asignacion ya fue
                // filtrada como automatica antes de entrar a este bloque.
'@

$contenido = $contenido.Replace(
    $patron,
    $reemplazo
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_fix_enum_v19_" +
    (Get-Date -Format "yyyyMMdd_HHmmss")

Copy-Item $ruta $backup -Force

$utf8ConBom =
    New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "HOTFIX APLICADO CORRECTAMENTE" -ForegroundColor Green
Write-Host "Se elimino la referencia inexistente OrigenHorasProductivas.Calculada."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
