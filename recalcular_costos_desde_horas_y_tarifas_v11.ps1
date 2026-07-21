$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("CostosDerivadosDesdeTarifasV11")) {
    Write-Host "La correccion V11 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# Marca para detectar aplicación.
$contenido = $contenido.Replace(
    "private void EscalarResultadoProcesoLinealV6(",
    "// CostosDerivadosDesdeTarifasV11" + [Environment]::NewLine +
    "        private void EscalarResultadoProcesoLinealV6("
)

# El costo del proceso no se escala: se invalida para recálculo.
$patronCostoProceso = @'
            proceso.Resultado.CostoCalculado =
                Math.Round(
                    proceso.Resultado.CostoCalculado *
                    factor,
                    2
                );
'@

$reemplazoCostoProceso = @'
            // El costo nunca se hereda ni se escala directamente.
            // Debe reconstruirse desde horas actuales y tarifa/hora.
            proceso.Resultado.CostoCalculado = 0m;
'@

if (-not $contenido.Contains($patronCostoProceso)) {
    throw "No se encontro el escalamiento de CostoCalculado del proceso."
}

$contenido = $contenido.Replace(
    $patronCostoProceso,
    $reemplazoCostoProceso
)

# El costo de cada asignación tampoco se escala.
$patronCostoAsignacion = @'
                asignacion.CostoCalculado =
                    Math.Round(
                        asignacion.CostoCalculado *
                        factor,
                        2
                    );
'@

$reemplazoCostoAsignacion = @'
                // Se invalida para que el motor lo derive nuevamente
                // desde HorasCalculadas y la tarifa vigente del cargo.
                asignacion.CostoCalculado = 0m;
'@

if (-not $contenido.Contains($patronCostoAsignacion)) {
    throw "No se encontro el escalamiento de costo de asignaciones."
}

$contenido = $contenido.Replace(
    $patronCostoAsignacion,
    $reemplazoCostoAsignacion
)

# Los costos del requerimiento no se multiplican por el factor.
$patronCostosReq = @'
            requerimiento.CostoEstandarCLP *= factor;
            requerimiento.CostoMinimoCLP *= factor;
            requerimiento.CostoHolguraCLP *= factor;
'@

$reemplazoCostosReq = @'
            // Los costos son resultados derivados, no cantidades escalables.
            // Se limpian para que el motor los recalcule usando las horas
            // actuales y las tarifas vigentes de cada cargo.
            requerimiento.CostoEstandarCLP = 0.0;
            requerimiento.CostoMinimoCLP = 0.0;
            requerimiento.CostoHolguraCLP = 0.0;
            requerimiento.TieneOverrideLocalCalculo = false;
'@

if (-not $contenido.Contains($patronCostosReq)) {
    throw "No se encontro el bloque de costos de requerimiento."
}

$contenido = $contenido.Replace(
    $patronCostosReq,
    $reemplazoCostosReq
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_costos_desde_tarifas_v11_" +
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
Write-Host "Correccion V11 aplicada." -ForegroundColor Green
Write-Host "Los costos historicos ya no se multiplican."
Write-Host "El motor debera reconstruirlos desde horas y tarifas vigentes."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
