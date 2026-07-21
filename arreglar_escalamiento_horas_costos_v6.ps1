$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("EscalarResultadoProcesoLinealV6(")) {
    Write-Host "El escalamiento lineal V6 ya esta aplicado." -ForegroundColor Yellow
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
        Texto = $Texto.Substring($inicio, $fin - $inicio + 1)
    }
}

# =========================================================
# 1. REEMPLAZAR SINCRONIZACION DEL SNAPSHOT
# =========================================================
$firmaSnapshot =
    "        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5("

$rangoSnapshot = Obtener-RangoMetodo $contenido $firmaSnapshot

if ($null -eq $rangoSnapshot) {
    throw "No se encontro SincronizarRequerimientosSnapshotConCantidadMaestraV5."
}

$nuevoSnapshot = @'
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

                double cantidadAnterior =
                    requerimiento.Cantidad;

                double factor =
                    cantidadAnterior > 0.0
                        ? Convert.ToDouble(cantidad) /
                          cantidadAnterior
                        : 1.0;

                requerimiento.Cantidad =
                    Convert.ToDouble(cantidad);

                EscalarRequerimientoLinealV6(
                    requerimiento,
                    factor
                );

                // La cantidad es heredada del producto, no una edición
                // manual de horas ni de rendimiento.
                requerimiento.EditadoManualmente = false;
                requerimiento.TieneOverrideLocalCalculo = false;
            }

            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoSnapshot.Inicio) +
    $nuevoSnapshot +
    $contenido.Substring($rangoSnapshot.Fin + 1)

# =========================================================
# 2. REEMPLAZAR SINCRONIZACION DEL PROCESO LOCAL
# =========================================================
$firmaProceso =
    "        private void SincronizarCantidadProcesoV4("

$rangoProceso = Obtener-RangoMetodo $contenido $firmaProceso

if ($null -eq $rangoProceso) {
    throw "No se encontro SincronizarCantidadProcesoV4."
}

$nuevoProceso = @'
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

            decimal cantidadAnterior =
                proceso.Cantidad;

            decimal factor =
                cantidadAnterior > 0m
                    ? cantidad / cantidadAnterior
                    : 1m;

            proceso.Cantidad = cantidad;

            EscalarResultadoProcesoLinealV6(
                proceso,
                factor
            );

            ActualizarCantidadSnapshotProcesoProyecto(
                item,
                proceso,
                cantidad
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoProceso.Inicio) +
    $nuevoProceso +
    $contenido.Substring($rangoProceso.Fin + 1)

# =========================================================
# 3. INSERTAR HELPERS ANTES DEL NORMALIZADOR V4
# =========================================================
$ancla =
    "        private string NormalizarUnidadCantidadProyectoV4("

$indiceAncla =
    $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indiceAncla -lt 0) {
    throw "No se encontro NormalizarUnidadCantidadProyectoV4."
}

$helpers = @'
        private void EscalarResultadoProcesoLinealV6(
            ProcesoProyecto proceso,
            decimal factor)
        {
            if (proceso == null ||
                factor < 0m ||
                proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual)
            {
                return;
            }

            proceso.Resultado =
                proceso.Resultado ??
                new ResultadoProcesoProyecto();

            proceso.Resultado.HorasCalculadas =
                Math.Round(
                    proceso.Resultado.HorasCalculadas *
                    factor,
                    6
                );

            proceso.Resultado.CostoCalculado =
                Math.Round(
                    proceso.Resultado.CostoCalculado *
                    factor,
                    2
                );

            proceso.Resultado.DuracionSemanas =
                Math.Round(
                    proceso.Resultado.DuracionSemanas *
                    factor,
                    6
                );

            // HorasAsignadas representan una decisión manual.
            // No se modifican.
            foreach (AsignacionProductiva asignacion in
                proceso.Asignaciones ??
                new List<AsignacionProductiva>())
            {
                if (asignacion == null ||
                    asignacion.OrigenHoras ==
                        OrigenHorasProductivas.Manual)
                {
                    continue;
                }

                asignacion.HorasCalculadas =
                    Math.Round(
                        asignacion.HorasCalculadas *
                        factor,
                        6
                    );

                asignacion.CostoCalculado =
                    Math.Round(
                        asignacion.CostoCalculado *
                        factor,
                        2
                    );
            }
        }

        private void EscalarRequerimientoLinealV6(
            RequerimientoProduccionInterna requerimiento,
            double factor)
        {
            if (requerimiento == null ||
                factor < 0.0 ||
                ModosCalculoProductivo.EsTiempoAsignado(
                    requerimiento.ModoCalculoProductivo
                ))
            {
                return;
            }

            requerimiento.HorasEstandar *= factor;
            requerimiento.HorasMinimas *= factor;
            requerimiento.HorasHolgura *= factor;

            requerimiento.DiasPersonaStd *= factor;
            requerimiento.DiasPersonaMin *= factor;
            requerimiento.DiasPersonaHolgura *= factor;

            requerimiento.CostoEstandarCLP *= factor;
            requerimiento.CostoMinimoCLP *= factor;
            requerimiento.CostoHolguraCLP *= factor;

            requerimiento.OrigenHoras =
                "Escalado linealmente por cantidad del producto";
        }

'@

$contenido =
    $contenido.Substring(0, $indiceAncla) +
    $helpers +
    $contenido.Substring($indiceAncla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_escalamiento_lineal_v6_" +
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
Write-Host "Escalamiento lineal V6 aplicado correctamente." -ForegroundColor Green
Write-Host "Horas y costos calculados ahora escalan con nuevaCantidad / cantidadAnterior."
Write-Host "Las horas manuales se conservan."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
