$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalcularCostosRequerimientoDesdeTarifaV12(")) {
    Write-Host "La correccion V12 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# =========================================================
# 1. Reemplazar el bloque V11 que dejaba costos en cero.
# =========================================================
$bloqueV11 = @'
            // Los costos son resultados derivados, no cantidades escalables.
            // Se limpian para que el motor los recalcule usando las horas
            // actuales y las tarifas vigentes de cada cargo.
            requerimiento.CostoEstandarCLP = 0.0;
            requerimiento.CostoMinimoCLP = 0.0;
            requerimiento.CostoHolguraCLP = 0.0;
            requerimiento.TieneOverrideLocalCalculo = false;
'@

$bloqueV12 = @'
            // Los costos son resultados derivados. Se reconstruyen
            // inmediatamente desde las horas actuales y la tarifa
            // vigente del cargo asociado al requerimiento.
            RecalcularCostosRequerimientoDesdeTarifaV12(
                requerimiento
            );

            requerimiento.TieneOverrideLocalCalculo = false;
'@

if (-not $contenido.Contains($bloqueV11)) {
    throw "No se encontro el bloque de costos en cero generado por V11."
}

$contenido = $contenido.Replace(
    $bloqueV11,
    $bloqueV12
)

# =========================================================
# 2. Insertar helper antes del normalizador V4.
# =========================================================
$ancla =
    "        private string NormalizarUnidadCantidadProyectoV4("

$indice =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indice -lt 0) {
    throw "No se encontro NormalizarUnidadCantidadProyectoV4."
}

$helper = @'
        private void RecalcularCostosRequerimientoDesdeTarifaV12(
            RequerimientoProduccionInterna requerimiento)
        {
            if (requerimiento == null)
            {
                return;
            }

            double tarifaHora = 0.0;

            if (requerimiento.TarifaDiaCargoCLP > 0.0)
            {
                tarifaHora =
                    requerimiento.TarifaDiaCargoCLP / 8.0;
            }
            else if (requerimiento.SueldoMensualCargoCLP > 0.0)
            {
                // Convención usada por la biblioteca:
                // 22 días laborales por mes y 8 horas por día.
                tarifaHora =
                    requerimiento.SueldoMensualCargoCLP /
                    22.0 /
                    8.0;
            }

            if (tarifaHora <= 0.0)
            {
                requerimiento.CostoEstandarCLP = 0.0;
                requerimiento.CostoMinimoCLP = 0.0;
                requerimiento.CostoHolguraCLP = 0.0;

                requerimiento.DiagnosticoParametros =
                    string.IsNullOrWhiteSpace(
                        requerimiento.DiagnosticoParametros)
                        ? "No existe tarifa diaria ni sueldo mensual para calcular el costo."
                        : requerimiento.DiagnosticoParametros +
                          " | No existe tarifa para recalcular costo.";

                return;
            }

            requerimiento.CostoEstandarCLP =
                Math.Round(
                    requerimiento.HorasEstandar *
                    tarifaHora,
                    2
                );

            requerimiento.CostoMinimoCLP =
                Math.Round(
                    requerimiento.HorasMinimas *
                    tarifaHora,
                    2
                );

            requerimiento.CostoHolguraCLP =
                Math.Round(
                    requerimiento.HorasHolgura *
                    tarifaHora,
                    2
                );
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helper +
    $contenido.Substring($indice)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_costos_tarifa_v12_" +
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
Write-Host "Correccion V12 aplicada." -ForegroundColor Green
Write-Host "Los costos ahora se calculan desde horas y tarifa del cargo."
Write-Host "Tarifa/hora = tarifa/dia / 8."
Write-Host "Fallback = sueldo mensual / 22 / 8."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
