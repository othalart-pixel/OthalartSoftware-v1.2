$ErrorActionPreference = "Stop"

$raiz = Get-Location

$archivo = Get-ChildItem -Path $raiz -Recurse -Filter "*.cs" |
    Where-Object {
        $texto = [System.IO.File]::ReadAllText($_.FullName)
        $texto.Contains("RedisenarInicioDashboard(tab);") -and
        $texto.Contains("Crear nuevo proyecto")
    } |
    Select-Object -First 1

if ($null -eq $archivo) {
    throw "No se encontro el archivo con RedisenarInicioDashboard(tab)."
}

$ruta = $archivo.FullName
$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$posLlamada = $contenido.IndexOf(
    "RedisenarInicioDashboard(tab);",
    [StringComparison]::Ordinal
)

if ($posLlamada -lt 0) {
    throw "No se encontro la llamada incorrecta."
}

$inicioMetodo = $contenido.LastIndexOf(
    "        private ",
    $posLlamada,
    [StringComparison]::Ordinal
)

if ($inicioMetodo -lt 0) {
    throw "No se pudo encontrar la firma del metodo que contiene la llamada."
}

$finFirma = $contenido.IndexOf(
    "{",
    $inicioMetodo,
    [StringComparison]::Ordinal
)

if ($finFirma -lt 0 -or $finFirma -gt $posLlamada) {
    throw "No se pudo leer la firma del metodo."
}

$firma = $contenido.Substring(
    $inicioMetodo,
    $finFirma - $inicioMetodo
)

$coincidencia = [regex]::Match(
    $firma,
    '\(\s*(?:TabPage|Control)\s+([A-Za-z_][A-Za-z0-9_]*)'
)

if (-not $coincidencia.Success) {
    throw "No se pudo detectar el nombre del parametro TabPage/Control en: $firma"
}

$nombreParametro = $coincidencia.Groups[1].Value

$contenido = $contenido.Replace(
    "RedisenarInicioDashboard(tab);",
    "RedisenarInicioDashboard($nombreParametro);"
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_fix_parametro_dashboard_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Parametro del dashboard corregido." -ForegroundColor Green
Write-Host "Parametro detectado: $nombreParametro"
Write-Host "Archivo: $ruta"
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
