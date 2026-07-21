$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalculoOficialPorRendimientoV15")) {
    Write-Host "La correccion V15 ya esta aplicada." -ForegroundColor Yellow
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

# =========================================================
# 1. SINCRONIZACION GLOBAL DEL PRODUCTO
# =========================================================
$firmaGlobal =
    "        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5("

$rangoGlobal =
    Obtener-RangoMetodo $contenido $firmaGlobal

if ($null -eq $rangoGlobal) {
    throw "No se encontro SincronizarRequerimientosSnapshotConCantidadMaestraV5."
}

$nuevoGlobal = @'
        // RecalculoOficialPorRendimientoV15
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

            double diasHabilesSemana =
                snapshot.DiasHabilesEstudioPorSemana > 0.0
                    ? snapshot.DiasHabilesEstudioPorSemana
                    : 5.0;

            foreach (RequerimientoProduccionInterna requerimiento in
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r => r != null))
            {
                bool usaSegundos =
                    NormalizarUnidadCantidadProyectoV4(
                        requerimiento.Unidad
                    ) == "segundo";

                if (!usaSegundos)
                {
                    continue;
                }

                // La cantidad cambia. Horas, días y costos NO se escalan
                // manualmente: se vuelven a resolver desde la ecuación,
                // el rendimiento y la tarifa oficial del requerimiento.
                requerimiento.Cantidad =
                    Convert.ToDouble(cantidad);

                if (!ModosCalculoProductivo.EsTiempoAsignado(
                        requerimiento.ModoCalculoProductivo))
                {
                    requerimiento.EditadoManualmente = false;
                    requerimiento.TieneOverrideLocalCalculo = false;
                }

                CalculoProductivoResolverService.Aplicar(
                    requerimiento,
                    diasHabilesSemana
                );
            }

            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoGlobal.Inicio) +
    $nuevoGlobal +
    $contenido.Substring($rangoGlobal.Fin + 1)

# =========================================================
# 2. CAMBIO INDIVIDUAL DE CANTIDAD DE UN PROCESO
# =========================================================
$firmaProceso =
    "        private void ActualizarCantidadSnapshotProcesoProyecto("

$rangoProceso =
    Obtener-RangoMetodo $contenido $firmaProceso

if ($null -eq $rangoProceso) {
    throw "No se encontro ActualizarCantidadSnapshotProcesoProyecto."
}

$nuevoProceso = @'
        private void ActualizarCantidadSnapshotProcesoProyecto(
            ItemProyecto item,
            ProcesoProyecto proceso,
            decimal cantidad)
        {
            if (item == null ||
                proceso == null ||
                cantidad < 0m)
            {
                return;
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                return;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r =>
                        r != null &&
                        CoincideRequerimientoConProceso(
                            r,
                            proceso,
                            null
                        ))
                    .ToList();

            if (requerimientos.Count == 0)
            {
                return;
            }

            double diasHabilesSemana =
                snapshot.DiasHabilesEstudioPorSemana > 0.0
                    ? snapshot.DiasHabilesEstudioPorSemana
                    : 5.0;

            foreach (RequerimientoProduccionInterna req in
                requerimientos)
            {
                // Cada cargo participa sobre la MISMA cantidad del proceso.
                // No se divide la cantidad entre cargos.
                req.Cantidad =
                    Convert.ToDouble(cantidad);

                // El cambio manual es sobre la cantidad, no sobre las horas.
                // Las horas continúan derivándose del rendimiento.
                if (!ModosCalculoProductivo.EsTiempoAsignado(
                        req.ModoCalculoProductivo))
                {
                    req.EditadoManualmente = false;
                    req.TieneOverrideLocalCalculo = false;
                }

                CalculoProductivoResolverService.Aplicar(
                    req,
                    diasHabilesSemana
                );
            }

            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoProceso.Inicio) +
    $nuevoProceso +
    $contenido.Substring($rangoProceso.Fin + 1)

# =========================================================
# 3. EVITAR EL ESCALAMIENTO MANUAL DEL MODELO LOCAL
# SincronizarCantidadProcesoV4 solo cambia la cantidad y
# delega el cálculo al snapshot/resolver oficial.
# =========================================================
$firmaSyncProceso =
    "        private void SincronizarCantidadProcesoV4("

$rangoSyncProceso =
    Obtener-RangoMetodo $contenido $firmaSyncProceso

if ($null -eq $rangoSyncProceso) {
    throw "No se encontro SincronizarCantidadProcesoV4."
}

$nuevoSyncProceso = @'
        private void SincronizarCantidadProcesoV4(
            ItemProyecto item,
            ProcesoProyecto proceso,
            decimal cantidad)
        {
            if (proceso == null ||
                proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual ||
                NormalizarUnidadCantidadProyectoV4(
                    proceso.Unidad) != "segundo")
            {
                return;
            }

            proceso.Cantidad = cantidad;

            ActualizarCantidadSnapshotProcesoProyecto(
                item,
                proceso,
                cantidad
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoSyncProceso.Inicio) +
    $nuevoSyncProceso +
    $contenido.Substring($rangoSyncProceso.Fin + 1)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_resolver_oficial_v15_" +
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
Write-Host "Correccion V15 aplicada." -ForegroundColor Green
Write-Host "Cantidad -> rendimiento -> horas -> costo vuelve a usar el resolver oficial."
Write-Host "Se elimino el escalamiento manual del flujo activo."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
