$ErrorActionPreference = "Stop"

$rutaActual = Join-Path (Get-Location) "UI\Form1.TabInicio.cs"

if (-not (Test-Path $rutaActual)) {
    throw "No se encontro UI\Form1.TabInicio.cs. Ejecuta este script desde la raiz del proyecto."
}

$backup = Get-ChildItem -Path (Split-Path $rutaActual) `
    -Filter "Form1.TabInicio.cs.backup_dashboard_inicio_*" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $backup) {
    throw "No se encontro el backup previo al dashboard: Form1.TabInicio.cs.backup_dashboard_inicio_*"
}

$backupSeguridad = "$rutaActual.backup_antes_de_restaurar_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $rutaActual $backupSeguridad -Force
Copy-Item $backup.FullName $rutaActual -Force

Write-Host ""
Write-Host "Pantalla de inicio restaurada al estado estable anterior." -ForegroundColor Green
Write-Host "Origen: $($backup.FullName)"
Write-Host "Respaldo del archivo roto: $backupSeguridad"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
