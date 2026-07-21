$ErrorActionPreference = "Stop"

$rutaActual = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $rutaActual)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la raiz del proyecto."
}

$carpeta = Split-Path $rutaActual

$backup = Get-ChildItem -Path $carpeta `
    -Filter "Form1.TabProyecto.cs.backup_cantidad_maestra_regex_*" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $backup) {
    throw "No se encontro el backup previo al parche de cantidad maestra."
}

$backupSeguridad = "$rutaActual.backup_archivo_roto_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

Copy-Item $rutaActual $backupSeguridad -Force
Copy-Item $backup.FullName $rutaActual -Force

Write-Host ""
Write-Host "Form1.TabProyecto.cs restaurado al estado anterior al parche." -ForegroundColor Green
Write-Host "Origen: $($backup.FullName)"
Write-Host "Respaldo del archivo actual: $backupSeguridad"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
