$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("TarifaEfectivaPreservadaV13")) {
    Write-Host "La correccion V13 ya esta aplicada." -ForegroundColor Yellow
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
# 1. REEMPLAZAR ESCALAMIENTO DE RESULTADOS DEL PROCESO
# =========================================================
$firmaProceso =
    "        private void EscalarResultadoProcesoLinealV6("

$rangoProceso =
    Obtener-RangoMetodo $contenido $firmaProceso

if ($null -eq $rangoProceso) {
    throw "No se encontro EscalarResultadoProcesoLinealV6."
}

$nuevoProceso = @'
        // TarifaEfectivaPreservadaV13
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

            decimal horasAnteriores =
                proceso.Resultado.HorasCalculadas;

            decimal costoAnterior =
                proceso.Resultado.CostoCalculado;

            decimal tarifaHoraEfectiva =
                horasAnteriores > 0m &&
                costoAnterior > 0m
                    ? costoAnterior / horasAnteriores
                    : 0m;

            proceso.Resultado.HorasCalculadas =
                Math.Round(
                    horasAnteriores * factor,
                    6
                );

            proceso.Resultado.CostoCalculado =
                tarifaHoraEfectiva > 0m
                    ? Math.Round(
                        proceso.Resultado.HorasCalculadas *
                        tarifaHoraEfectiva,
                        2
                    )
                    : costoAnterior;

            proceso.Resultado.DuracionSemanas =
                Math.Round(
                    proceso.Resultado.DuracionSemanas *
                    factor,
                    6
                );

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

                decimal horasAsignacionAnteriores =
                    asignacion.HorasCalculadas;

                decimal costoAsignacionAnterior =
                    asignacion.CostoCalculado;

                decimal tarifaAsignacion =
                    horasAsignacionAnteriores > 0m &&
                    costoAsignacionAnterior > 0m
                        ? costoAsignacionAnterior /
                          horasAsignacionAnteriores
                        : 0m;

                asignacion.HorasCalculadas =
                    Math.Round(
                        horasAsignacionAnteriores *
                        factor,
                        6
                    );

                asignacion.CostoCalculado =
                    tarifaAsignacion > 0m
                        ? Math.Round(
                            asignacion.HorasCalculadas *
                            tarifaAsignacion,
                            2
                        )
                        : costoAsignacionAnterior;
            }
        }
'@

$contenido =
    $contenido.Substring(0, $rangoProceso.Inicio) +
    $nuevoProceso +
    $contenido.Substring($rangoProceso.Fin + 1)

# =========================================================
# 2. REEMPLAZAR ESCALAMIENTO DE REQUERIMIENTOS
# =========================================================
$firmaReq =
    "        private void EscalarRequerimientoLinealV6("

$rangoReq =
    Obtener-RangoMetodo $contenido $firmaReq

if ($null -eq $rangoReq) {
    throw "No se encontro EscalarRequerimientoLinealV6."
}

$nuevoReq = @'
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

            double horasStdAnteriores =
                requerimiento.HorasEstandar;

            double horasMinAnteriores =
                requerimiento.HorasMinimas;

            double horasHolguraAnteriores =
                requerimiento.HorasHolgura;

            double costoStdAnterior =
                requerimiento.CostoEstandarCLP;

            double costoMinAnterior =
                requerimiento.CostoMinimoCLP;

            double costoHolguraAnterior =
                requerimiento.CostoHolguraCLP;

            double tarifaHoraStd =
                ObtenerTarifaHoraEfectivaV13(
                    requerimiento,
                    horasStdAnteriores,
                    costoStdAnterior
                );

            double tarifaHoraMin =
                horasMinAnteriores > 0.0 &&
                costoMinAnterior > 0.0
                    ? costoMinAnterior /
                      horasMinAnteriores
                    : tarifaHoraStd;

            double tarifaHoraHolgura =
                horasHolguraAnteriores > 0.0 &&
                costoHolguraAnterior > 0.0
                    ? costoHolguraAnterior /
                      horasHolguraAnteriores
                    : tarifaHoraStd;

            requerimiento.HorasEstandar =
                horasStdAnteriores * factor;

            requerimiento.HorasMinimas =
                horasMinAnteriores * factor;

            requerimiento.HorasHolgura =
                horasHolguraAnteriores * factor;

            requerimiento.DiasPersonaStd *= factor;
            requerimiento.DiasPersonaMin *= factor;
            requerimiento.DiasPersonaHolgura *= factor;

            requerimiento.CostoEstandarCLP =
                tarifaHoraStd > 0.0
                    ? Math.Round(
                        requerimiento.HorasEstandar *
                        tarifaHoraStd,
                        2
                    )
                    : costoStdAnterior;

            requerimiento.CostoMinimoCLP =
                tarifaHoraMin > 0.0
                    ? Math.Round(
                        requerimiento.HorasMinimas *
                        tarifaHoraMin,
                        2
                    )
                    : costoMinAnterior;

            requerimiento.CostoHolguraCLP =
                tarifaHoraHolgura > 0.0
                    ? Math.Round(
                        requerimiento.HorasHolgura *
                        tarifaHoraHolgura,
                        2
                    )
                    : costoHolguraAnterior;

            requerimiento.OrigenHoras =
                "Escalado linealmente conservando tarifa efectiva";
        }

        private double ObtenerTarifaHoraEfectivaV13(
            RequerimientoProduccionInterna requerimiento,
            double horasAnteriores,
            double costoAnterior)
        {
            if (requerimiento == null)
            {
                return 0.0;
            }

            if (requerimiento.TarifaDiaCargoCLP > 0.0)
            {
                return
                    requerimiento.TarifaDiaCargoCLP / 8.0;
            }

            if (requerimiento.SueldoMensualCargoCLP > 0.0)
            {
                return
                    requerimiento.SueldoMensualCargoCLP /
                    22.0 /
                    8.0;
            }

            if (horasAnteriores > 0.0 &&
                costoAnterior > 0.0)
            {
                return
                    costoAnterior / horasAnteriores;
            }

            return 0.0;
        }
'@

$contenido =
    $contenido.Substring(0, $rangoReq.Inicio) +
    $nuevoReq +
    $contenido.Substring($rangoReq.Fin + 1)

# =========================================================
# 3. QUITAR HELPER V12 PARA EVITAR SEGUNDO RECALCULO
# =========================================================
$firmaV12 =
    "        private void RecalcularCostosRequerimientoDesdeTarifaV12("

$rangoV12 =
    Obtener-RangoMetodo $contenido $firmaV12

if ($null -ne $rangoV12) {
    $contenido =
        $contenido.Substring(0, $rangoV12.Inicio) +
        $contenido.Substring($rangoV12.Fin + 1)
}

# Quitar llamada V12 si sigue dentro de algún bloque.
$contenido = [regex]::Replace(
    $contenido,
    '(?ms)\s*RecalcularCostosRequerimientoDesdeTarifaV12\s*\(\s*requerimiento\s*\)\s*;',
    '',
    1
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_tarifa_efectiva_v13_" +
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
Write-Host "Correccion V13 aplicada." -ForegroundColor Green
Write-Host "Las horas escalan y los costos se reconstruyen conservando la tarifa efectiva original."
Write-Host "Prioridad de tarifa: tarifa/dia, sueldo mensual, costo/hora historico."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
