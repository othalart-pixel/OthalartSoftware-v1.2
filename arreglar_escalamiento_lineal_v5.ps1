$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("SincronizarRequerimientosSnapshotConCantidadMaestraV5(")) {
    Write-Host "La sincronizacion V5 ya parece estar aplicada." -ForegroundColor Yellow
    exit 0
}

# ---------------------------------------------------------
# 1. Encontrar el helper V4 ya instalado.
# ---------------------------------------------------------
$firma = "        private void SincronizarCantidadProductoYDescendientesV4("
$inicio = $contenido.IndexOf($firma, [StringComparison]::Ordinal)

if ($inicio -lt 0) {
    throw "No se encontro SincronizarCantidadProductoYDescendientesV4. Ejecuta primero la V4 que ya te funciono."
}

$llaveInicial = $contenido.IndexOf("{", $inicio, [StringComparison]::Ordinal)

if ($llaveInicial -lt 0) {
    throw "No se encontro el inicio del helper V4."
}

$profundidad = 0
$fin = -1

for ($i = $llaveInicial; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') {
        $profundidad++
    }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--

        if ($profundidad -eq 0) {
            $fin = $i
            break
        }
    }
}

if ($fin -lt 0) {
    throw "No se encontro el final del helper V4."
}

$metodo = $contenido.Substring(
    $inicio,
    $fin - $inicio + 1
)

# ---------------------------------------------------------
# 2. Insertar sincronizacion directa del snapshot antes de
#    guardar la fecha de edicion.
# ---------------------------------------------------------
$patronFecha = '(?m)^(?<indent>\s*)item\.FechaEdicionSnapshot\s*=\s*DateTime\.Now\s*;'

$matchFecha = [regex]::Match($metodo, $patronFecha)

if (-not $matchFecha.Success) {
    throw "No se encontro item.FechaEdicionSnapshot dentro del helper V4."
}

$indent = $matchFecha.Groups["indent"].Value

$bloque = @"
${indent}SincronizarRequerimientosSnapshotConCantidadMaestraV5(
${indent}    item,
${indent}    cantidad
${indent});

${indent}item.FechaEdicionSnapshot = DateTime.Now;
"@

$metodo = [regex]::Replace(
    $metodo,
    $patronFecha,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return $bloque
    },
    1
)

$contenido =
    $contenido.Substring(0, $inicio) +
    $metodo +
    $contenido.Substring($fin + 1)

# ---------------------------------------------------------
# 3. Insertar helper V5 antes del helper de proceso V4.
# ---------------------------------------------------------
$ancla = "        private void SincronizarCantidadProcesoV4("
$indiceAncla = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indiceAncla -lt 0) {
    throw "No se encontro SincronizarCantidadProcesoV4 para insertar V5."
}

$helper = @'
        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5(
            ItemProyecto item,
            decimal cantidad)
        {
            if (item == null || cantidad < 0m)
            {
                return;
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                ActualizarCantidadSnapshotItemProyecto(
                    item,
                    cantidad
                );
                return;
            }

            foreach (RequerimientoProduccionInterna requerimiento in
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r => r != null))
            {
                bool esManual =
                    ModosCalculoProductivo.EsTiempoAsignado(
                        requerimiento.ModoCalculoProductivo
                    );

                bool usaSegundos =
                    NormalizarUnidadCantidadProyectoV4(
                        requerimiento.Unidad
                    ) == "segundo";

                if (esManual || !usaSegundos)
                {
                    continue;
                }

                requerimiento.Cantidad =
                    Convert.ToDouble(cantidad);
            }

            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
        }

'@

$contenido =
    $contenido.Substring(0, $indiceAncla) +
    $helper +
    $contenido.Substring($indiceAncla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_escalamiento_v5_" +
    (Get-Date -Format 'yyyyMMdd_HHmmss')

Copy-Item $ruta $backup -Force

$utf8ConBom =
    New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Escalamiento V5 aplicado." -ForegroundColor Green
Write-Host "Los requerimientos calculados en segundos ahora toman la cantidad maestra antes del recálculo."
Write-Host "Las horas manuales se conservan."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
