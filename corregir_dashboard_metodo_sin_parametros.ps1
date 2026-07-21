$ErrorActionPreference = "Stop"

$raiz = Get-Location

$archivo = Get-ChildItem -Path $raiz -Recurse -Filter "*.cs" |
    Where-Object {
        $texto = [System.IO.File]::ReadAllText($_.FullName)
        $texto.Contains("private Control CrearPanelInicioProyecto()") -and
        $texto.Contains("RedisenarInicioDashboard(tab);")
    } |
    Select-Object -First 1

if ($null -eq $archivo) {
    throw "No se encontro el archivo con CrearPanelInicioProyecto() y la llamada incorrecta."
}

$ruta = $archivo.FullName
$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$firma = "        private Control CrearPanelInicioProyecto()"
$inicioMetodo = $contenido.IndexOf($firma, [StringComparison]::Ordinal)

if ($inicioMetodo -lt 0) {
    throw "No se encontro CrearPanelInicioProyecto()."
}

$llaveInicial = $contenido.IndexOf("{", $inicioMetodo, [StringComparison]::Ordinal)
if ($llaveInicial -lt 0) {
    throw "No se encontro el inicio del metodo."
}

$profundidad = 0
$finMetodo = -1

for ($i = $llaveInicial; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') {
        $profundidad++
    }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--

        if ($profundidad -eq 0) {
            $finMetodo = $i
            break
        }
    }
}

if ($finMetodo -lt 0) {
    throw "No se encontro el final del metodo."
}

$metodo = $contenido.Substring(
    $inicioMetodo,
    $finMetodo - $inicioMetodo + 1
)

# Quitar la llamada inválida.
$metodo = $metodo.Replace(
    "            RedisenarInicioDashboard(tab);",
    ""
)

# Buscar el último return de un control o variable dentro del método.
$coincidencias = [regex]::Matches(
    $metodo,
    '(?m)^(?<indent>\s*)return\s+(?<expr>[A-Za-z_][A-Za-z0-9_\.]*)\s*;'
)

if ($coincidencias.Count -eq 0) {
    throw "No se encontro un return simple dentro de CrearPanelInicioProyecto()."
}

$ultimoReturn = $coincidencias[$coincidencias.Count - 1]
$indentacion = $ultimoReturn.Groups["indent"].Value
$expresion = $ultimoReturn.Groups["expr"].Value

$nuevoReturn = @"
${indentacion}RedisenarInicioDashboard($expresion);
${indentacion}return $expresion;
"@

$metodo = $metodo.Remove(
    $ultimoReturn.Index,
    $ultimoReturn.Length
).Insert(
    $ultimoReturn.Index,
    $nuevoReturn
)

$contenidoNuevo =
    $contenido.Substring(0, $inicioMetodo) +
    $metodo +
    $contenido.Substring($finMetodo + 1)

if ($contenidoNuevo.Contains("RedisenarInicioDashboard(tab);")) {
    throw "La llamada incorrecta todavía existe. No se escribio el archivo."
}

if ($contenidoNuevo -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_fix_dashboard_return_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText(
    $ruta,
    $contenidoNuevo,
    $utf8ConBom
)

Write-Host ""
Write-Host "Dashboard conectado al control retornado correctamente." -ForegroundColor Green
Write-Host "Control detectado: $expresion"
Write-Host "Archivo: $ruta"
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
