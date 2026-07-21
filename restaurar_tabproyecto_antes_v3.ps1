$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$carpeta = Split-Path $ruta

$backup = Get-ChildItem -Path $carpeta `
    -Filter "Form1.TabProyecto.cs.backup_cantidad_maestra_v3_*" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $backup) {
    throw "No se encontro el backup automatico anterior al parche V3."
}

$respaldoRoto =
    "$ruta.backup_estructura_rota_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

Copy-Item $ruta $respaldoRoto -Force
Copy-Item $backup.FullName $ruta -Force

Write-Host ""
Write-Host "TabProyecto restaurado al estado anterior al V3." -ForegroundColor Green
Write-Host "Origen: $($backup.FullName)"
Write-Host "Respaldo del archivo roto: $respaldoRoto"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
