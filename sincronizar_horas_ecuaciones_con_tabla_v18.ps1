$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este archivo desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("SincronizarResultadosLocalesDesdeSnapshotV18(")) {
    Write-Host "La correccion V18 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param(
        [string]$Texto,
        [string]$Firma
    )

    $inicio = $Texto.IndexOf($Firma, [StringComparison]::Ordinal)
    if ($inicio -lt 0) {
        return $null
    }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
    if ($llave -lt 0) {
        return $null
    }

    $profundidad = 0
    $fin = -1

    for ($i = $llave; $i -lt $Texto.Length; $i++) {
        if ($Texto[$i] -eq '{') {
            $profundidad++
        }
        elseif ($Texto[$i] -eq '}') {
            $profundidad--

            if ($profundidad -eq 0) {
                $fin = $i
                break
            }
        }
    }

    if ($fin -lt 0) {
        return $null
    }

    return @{
        Inicio = $inicio
        Fin = $fin
        Texto = $Texto.Substring(
            $inicio,
            $fin - $inicio + 1
        )
    }
}

# ---------------------------------------------------------
# 1. Conectar sincronizacion local en el cambio global.
# ---------------------------------------------------------
$firmaGlobal =
    "        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5("

$rangoGlobal =
    Obtener-RangoMetodo $contenido $firmaGlobal

if ($null -eq $rangoGlobal) {
    throw "No se encontro SincronizarRequerimientosSnapshotConCantidadMaestraV5."
}

$metodoGlobal = $rangoGlobal.Texto

$patronGuardarGlobal = @'
            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
'@

$reemplazoGuardarGlobal = @'
            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );

            // El snapshot ya fue recalculado. Ahora se actualiza el
            // modelo local que alimenta la tabla de Productos y servicios.
            SincronizarResultadosLocalesDesdeSnapshotV18(
                item,
                snapshot
            );
'@

if (-not $metodoGlobal.Contains($patronGuardarGlobal)) {
    throw "No se encontro el guardado del snapshot en el cambio global."
}

$metodoGlobal = $metodoGlobal.Replace(
    $patronGuardarGlobal,
    $reemplazoGuardarGlobal
)

$contenido =
    $contenido.Substring(0, $rangoGlobal.Inicio) +
    $metodoGlobal +
    $contenido.Substring($rangoGlobal.Fin + 1)

# ---------------------------------------------------------
# 2. Conectar sincronizacion local en el cambio individual.
# ---------------------------------------------------------
$firmaIndividual =
    "        private void ActualizarCantidadSnapshotProcesoProyecto("

$rangoIndividual =
    Obtener-RangoMetodo $contenido $firmaIndividual

if ($null -eq $rangoIndividual) {
    throw "No se encontro ActualizarCantidadSnapshotProcesoProyecto."
}

$metodoIndividual = $rangoIndividual.Texto

$patronGuardarIndividual = @'
            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
'@

$reemplazoGuardarIndividual = @'
            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );

            SincronizarResultadosLocalesDesdeSnapshotV18(
                item,
                snapshot
            );
'@

if (-not $metodoIndividual.Contains($patronGuardarIndividual)) {
    throw "No se encontro el guardado del snapshot en el cambio individual."
}

$metodoIndividual = $metodoIndividual.Replace(
    $patronGuardarIndividual,
    $reemplazoGuardarIndividual
)

$contenido =
    $contenido.Substring(0, $rangoIndividual.Inicio) +
    $metodoIndividual +
    $contenido.Substring($rangoIndividual.Fin + 1)

# ---------------------------------------------------------
# 3. Insertar helper antes de SincronizarCantidadProcesoV4.
# ---------------------------------------------------------
$ancla =
    "        private void SincronizarCantidadProcesoV4("

$indice =
    $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro SincronizarCantidadProcesoV4 para insertar V18."
}

$helper = @'
        private void SincronizarResultadosLocalesDesdeSnapshotV18(
            ItemProyecto item,
            Cotizacion snapshot)
        {
            if (item == null ||
                snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                return;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r => r != null)
                    .ToList();

            foreach (ProcesoProyecto proceso in
                ObtenerProcesosItemProyecto(item)
                    .Where(p => p != null))
            {
                List<RequerimientoProduccionInterna> relacionados =
                    requerimientos
                        .Where(r =>
                            CoincideRequerimientoConProceso(
                                r,
                                proceso,
                                null
                            ))
                        .ToList();

                if (relacionados.Count == 0)
                {
                    continue;
                }

                // Una fila de proceso puede tener varios cargos, pero las
                // horas del trabajo corresponden al cuello de botella,
                // no a la suma de las horas de todos los cargos.
                RequerimientoProduccionInterna principal =
                    relacionados
                        .OrderByDescending(r =>
                            r.HorasEstandar)
                        .First();

                decimal horas =
                    Convert.ToDecimal(
                        Math.Max(
                            0.0,
                            principal.HorasEstandar
                        )
                    );

                decimal costo =
                    Convert.ToDecimal(
                        Math.Max(
                            0.0,
                            principal.CostoEstandarCLP
                        )
                    );

                proceso.Resultado =
                    proceso.Resultado ??
                    new ResultadoProcesoProyecto();

                proceso.Resultado.HorasCalculadas =
                    horas;

                proceso.Resultado.CostoCalculado =
                    costo;

                proceso.Resultado.DuracionSemanas =
                    snapshot.DiasHabilesEstudioPorSemana > 0.0
                        ? horas /
                          Convert.ToDecimal(
                              snapshot.DiasHabilesEstudioPorSemana *
                              CalculoProductivoResolverService.HorasDiaEstandar
                          )
                        : 0m;

                // Solo los procesos calculados automáticamente deben volver
                // a tomar sus horas desde la ecuación y el rendimiento.
                if (proceso.MetodoCalculo !=
                    MetodoCalculoProceso.Manual)
                {
                    proceso.Resultado.HorasAsignadas = 0m;
                }

                proceso.Capacidad =
                    Convert.ToDecimal(
                        Math.Max(
                            0.0,
                            principal.RendimientoCantidad
                        )
                    );

                proceso.Periodo =
                    principal.RendimientoPeriodo ?? "";

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
    "$ruta.backup_sincronizar_horas_runtime_v18_" +
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
Write-Host "Correccion V18 aplicada." -ForegroundColor Green
Write-Host "Las horas calculadas por ecuaciones ahora se copian al modelo local de la tabla."
Write-Host "Las horas manuales siguen protegidas."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
