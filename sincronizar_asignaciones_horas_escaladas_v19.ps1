$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("SincronizarAsignacionesCalculadasV19(")) {
    Write-Host "La correccion V19 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# ---------------------------------------------------------
# 1. Reemplazar el bloque antiguo de asignaciones dentro V18
# ---------------------------------------------------------
$bloqueAntiguo = @'
                foreach (AsignacionProductiva asignacion in
                    (proceso.Asignaciones ??
                        new List<AsignacionProductiva>())
                    .Where(a => a != null))
                {
                    if (asignacion.OrigenHoras ==
                        OrigenHorasProductivas.Manual)
                    {
                        continue;
                    }

                    // Las asignaciones automáticas no pueden conservar
                    // resultados antiguos después de cambiar la cantidad.
                    asignacion.HorasAsignadas = 0m;
                }
'@

$bloqueNuevo = @'
                SincronizarAsignacionesCalculadasV19(
                    proceso,
                    horas,
                    costo
                );
'@

if (-not $contenido.Contains($bloqueAntiguo)) {
    throw "No se encontro el bloque de asignaciones antiguas dentro de V18."
}

$contenido = $contenido.Replace(
    $bloqueAntiguo,
    $bloqueNuevo
)

# ---------------------------------------------------------
# 2. Insertar helper antes de SincronizarCantidadProcesoV4
# ---------------------------------------------------------
$ancla =
    "        private void SincronizarCantidadProcesoV4("

$indice =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indice -lt 0) {
    throw "No se encontro SincronizarCantidadProcesoV4."
}

$helper = @'
        private void SincronizarAsignacionesCalculadasV19(
            ProcesoProyecto proceso,
            decimal horasTotales,
            decimal costoTotal)
        {
            if (proceso == null)
            {
                return;
            }

            List<AsignacionProductiva> automaticas =
                (proceso.Asignaciones ??
                    new List<AsignacionProductiva>())
                .Where(a =>
                    a != null &&
                    a.OrigenHoras !=
                        OrigenHorasProductivas.Manual)
                .ToList();

            if (automaticas.Count == 0)
            {
                return;
            }

            decimal sumaHorasAnteriores =
                automaticas.Sum(a =>
                    Math.Max(
                        0m,
                        a.HorasCalculadas
                    ));

            decimal sumaCostosAnteriores =
                automaticas.Sum(a =>
                    Math.Max(
                        0m,
                        a.CostoCalculado
                    ));

            decimal horasRestantes =
                Math.Max(0m, horasTotales);

            decimal costoRestante =
                Math.Max(0m, costoTotal);

            for (int i = 0;
                i < automaticas.Count;
                i++)
            {
                AsignacionProductiva asignacion =
                    automaticas[i];

                decimal proporcionHoras;

                if (sumaHorasAnteriores > 0m)
                {
                    proporcionHoras =
                        Math.Max(
                            0m,
                            asignacion.HorasCalculadas
                        ) /
                        sumaHorasAnteriores;
                }
                else
                {
                    proporcionHoras =
                        1m / automaticas.Count;
                }

                decimal proporcionCosto;

                if (sumaCostosAnteriores > 0m)
                {
                    proporcionCosto =
                        Math.Max(
                            0m,
                            asignacion.CostoCalculado
                        ) /
                        sumaCostosAnteriores;
                }
                else
                {
                    proporcionCosto =
                        proporcionHoras;
                }

                decimal horasNuevas =
                    i == automaticas.Count - 1
                        ? horasRestantes
                        : Math.Round(
                            horasTotales *
                            proporcionHoras,
                            6
                        );

                decimal costoNuevo =
                    i == automaticas.Count - 1
                        ? costoRestante
                        : Math.Round(
                            costoTotal *
                            proporcionCosto,
                            2
                        );

                horasRestantes -= horasNuevas;
                costoRestante -= costoNuevo;

                asignacion.HorasAsignadas = 0m;
                asignacion.HorasCalculadas =
                    Math.Max(0m, horasNuevas);

                asignacion.CostoCalculado =
                    Math.Max(0m, costoNuevo);

                asignacion.OrigenHoras =
                    OrigenHorasProductivas.Calculada;
            }
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
    "$ruta.backup_asignaciones_runtime_v19_" +
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
Write-Host "Correccion V19 aplicada." -ForegroundColor Green
Write-Host "Las horas y costos recalculados ahora se copian tambien a las asignaciones que lee la tabla."
Write-Host "Las asignaciones manuales se mantienen protegidas."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
