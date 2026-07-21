$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)

$firmaInicio = "        private void ConstruirTabProyecto(TabPage tab)"
$firmaFin = "        private Control CrearHeaderProductosServicios()"

$inicioMetodo = $contenido.IndexOf($firmaInicio, [StringComparison]::Ordinal)
$finMetodo = $contenido.IndexOf($firmaFin, [StringComparison]::Ordinal)

if ($inicioMetodo -lt 0 -or $finMetodo -le $inicioMetodo) {
    throw "No se pudo aislar ConstruirTabProyecto(). No se modifico nada."
}

$metodo = $contenido.Substring($inicioMetodo, $finMetodo - $inicioMetodo)
$marcador = "split.BeginInvoke(new Action(() =>"
$pos = $metodo.IndexOf($marcador, [StringComparison]::Ordinal)

if ($pos -lt 0) {
    Write-Host ""
    Write-Host "No se encontro split.BeginInvoke dentro de ConstruirTabProyecto." -ForegroundColor Yellow
    exit 0
}

# Buscar el inicio real de la linea para eliminar tambien la indentacion.
$inicioBloque = $metodo.LastIndexOf("`n", $pos)
if ($inicioBloque -lt 0) {
    $inicioBloque = 0
}
else {
    $inicioBloque++
}

# Buscar la llave de apertura de la lambda.
$llaveInicial = $metodo.IndexOf("{", $pos, [StringComparison]::Ordinal)
if ($llaveInicial -lt 0) {
    throw "Se encontro BeginInvoke, pero no su bloque de apertura. No se modifico nada."
}

# Recorrer el texto contando llaves hasta cerrar la lambda.
$profundidad = 0
$llaveFinal = -1
$enCadena = $false
$escape = $false

for ($i = $llaveInicial; $i -lt $metodo.Length; $i++) {
    $c = $metodo[$i]

    if ($enCadena) {
        if ($escape) {
            $escape = $false
            continue
        }

        if ($c -eq '\') {
            $escape = $true
            continue
        }

        if ($c -eq '"') {
            $enCadena = $false
        }

        continue
    }

    if ($c -eq '"') {
        $enCadena = $true
        continue
    }

    if ($c -eq '{') {
        $profundidad++
    }
    elseif ($c -eq '}') {
        $profundidad--

        if ($profundidad -eq 0) {
            $llaveFinal = $i
            break
        }
    }
}

if ($llaveFinal -lt 0) {
    throw "No se pudo encontrar el cierre del bloque BeginInvoke. No se modifico nada."
}

# Despues de la llave deben venir los cierres de Action y BeginInvoke:
#   }));
# Permitimos espacios y saltos de linea.
$finBloque = $llaveFinal + 1

while ($finBloque -lt $metodo.Length -and
       [char]::IsWhiteSpace($metodo[$finBloque])) {
    $finBloque++
}

$esperado = "));"
if ($finBloque + $esperado.Length -gt $metodo.Length -or
    $metodo.Substring($finBloque, $esperado.Length) -ne $esperado) {

    # Algunas variantes terminan como:
    #   }));
    # donde hay un parentesis adicional por new Action(...).
    $esperado = ")));"
}

if ($finBloque + $esperado.Length -gt $metodo.Length -or
    $metodo.Substring($finBloque, $esperado.Length) -ne $esperado) {
    throw "Se encontro el cierre de la lambda, pero no el final esperado de BeginInvoke. No se modifico nada."
}

$finBloque += $esperado.Length

# Consumir el salto de linea posterior.
while ($finBloque -lt $metodo.Length -and
       ($metodo[$finBloque] -eq "`r" -or $metodo[$finBloque] -eq "`n")) {
    $finBloque++
}

$metodoNuevo =
    $metodo.Substring(0, $inicioBloque) +
    $metodo.Substring($finBloque)

if ($metodoNuevo.Contains($marcador)) {
    throw "Quedo otro split.BeginInvoke dentro de ConstruirTabProyecto. No se escribio el archivo."
}

$backup = "$ruta.backup_quitar_begininvoke_exacto_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicioMetodo) +
    $metodoNuevo +
    $contenido.Substring($finMetodo)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Se elimino exactamente el split.BeginInvoke del constructor." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila Debug: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
Write-Host 'Compila Release: dotnet build "OthalartSoftware v1.0.csproj" -c Release'
