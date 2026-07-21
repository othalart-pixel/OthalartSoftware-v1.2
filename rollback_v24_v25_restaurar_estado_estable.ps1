$ErrorActionPreference = "Stop"

$raiz = Get-Location
$rutaActual = Join-Path $raiz "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $rutaActual)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$backups = Get-ChildItem `
    -Path (Join-Path $raiz "UI") `
    -Filter "Form1.TabProyecto.cs.backup_desglose_v24_*" `
    -File |
    Sort-Object LastWriteTime -Descending

if ($backups.Count -eq 0) {
    throw "No se encontro el backup anterior a V24: Form1.TabProyecto.cs.backup_desglose_v24_*"
}

$backupElegido = $backups[0]

$respaldoEstadoRoto =
    "$rutaActual.backup_estado_v24_v25_roto_" +
    (Get-Date -Format "yyyyMMdd_HHmmss")

Copy-Item $rutaActual $respaldoEstadoRoto -Force
Copy-Item $backupElegido.FullName $rutaActual -Force

Write-Host ""
Write-Host "ROLLBACK APLICADO CORRECTAMENTE" -ForegroundColor Green
Write-Host ""
Write-Host "Restaurado desde:"
Write-Host $backupElegido.FullName
Write-Host ""
Write-Host "El estado roto se respaldo en:"
Write-Host $respaldoEstadoRoto
Write-Host ""

$csproj = Join-Path $raiz "OthalartSoftware v1.0.csproj"

if (Test-Path $csproj) {
    Write-Host "Compilando..." -ForegroundColor Cyan
    dotnet build $csproj -c Debug --nologo

    if ($LASTEXITCODE -ne 0) {
        throw "El rollback se aplico, pero la compilacion fallo. Revisa los errores mostrados arriba."
    }

    Write-Host ""
    Write-Host "Compilacion correcta." -ForegroundColor Green
}
else {
    Write-Host "No se encontro OthalartSoftware v1.0.csproj; compila manualmente."
}

Write-Host ""
Write-Host "IMPORTANTE:"
Write-Host "Abre nuevamente el proyecto guardado en disco."
Write-Host "No guardes la sesion que mostraba 384,63 horas."
