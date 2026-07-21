$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta el hotfix desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("EcuacionAutoritativaV21")) {
    Write-Host "El hotfix V21 ya esta aplicado." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param([string]$Texto, [string]$Firma)

    $inicio = $Texto.IndexOf($Firma, [StringComparison]::Ordinal)
    if ($inicio -lt 0) { return $null }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
    if ($llave -lt 0) { return $null }

    $profundidad = 0
    $fin = -1

    for ($i = $llave; $i -lt $Texto.Length; $i++) {
        if ($Texto[$i] -eq '{') { $profundidad++ }
        elseif ($Texto[$i] -eq '}') {
            $profundidad--
            if ($profundidad -eq 0) {
                $fin = $i
                break
            }
        }
    }

    if ($fin -lt 0) { return $null }

    return @{
        Inicio = $inicio
        Fin = $fin
    }
}

function Reemplazar-Metodo {
    param([string]$Texto, [string]$Firma, [string]$NuevoMetodo)

    $rango = Obtener-RangoMetodo $Texto $Firma
    if ($null -eq $rango) {
        throw "No se encontro el metodo: $Firma"
    }

    return $Texto.Substring(0, $rango.Inicio) +
        $NuevoMetodo +
        $Texto.Substring($rango.Fin + 1)
}

$nuevoRecalculo = @'
        // EcuacionAutoritativaV21
        private bool RecalcularRequerimientoDesdeEcuacionV16(
            RequerimientoProduccionInterna requerimiento,
            double diasHabilesSemana)
        {
            if (requerimiento == null)
            {
                return false;
            }

            List<EcuacionProductivaDefinicion> biblioteca =
                BibliotecaEcuacionesProductivasJsonService
                    .CargarEcuaciones();

            EcuacionProductivaDefinicion ecuacion =
                ResolverEcuacionVigenteV21(
                    requerimiento,
                    biblioteca
                );

            if (ecuacion == null)
            {
                return CalculoProductivoResolverService.Aplicar(
                    requerimiento,
                    diasHabilesSemana
                );
            }

            EcuacionProductivaRuntimeService.ResultadoPrueba resultado =
                EcuacionProductivaRuntimeService.EvaluarRequerimiento(
                    ecuacion,
                    biblioteca,
                    requerimiento,
                    diasHabilesSemana
                );

            bool esManual =
                ecuacion.MetodoCalculo ==
                    MetodoCalculoProceso.Manual;

            if (esManual)
            {
                requerimiento.CostoEstandarCLP =
                    resultado.CostoCLP;

                requerimiento.CostoMinimoCLP =
                    resultado.CostoCLP;

                requerimiento.CostoHolguraCLP =
                    resultado.CostoCLP;

                requerimiento.DiasPersonaStd =
                    resultado.DiasTecnicos;

                requerimiento.DiasPersonaMin =
                    resultado.DiasTecnicos;

                requerimiento.DiasPersonaHolgura =
                    resultado.DiasTecnicos;

                requerimiento.OrigenHoras =
                    "Horas declaradas manualmente por la ecuacion vigente";

                requerimiento.DiagnosticoParametros =
                    string.Join(
                        " | ",
                        resultado.Advertencias
                            .Concat(resultado.Errores)
                    );

                return resultado.Errores.Count == 0;
            }

            List<EcuacionProductivaRuntimeService.ResultadoCargo>
                cargosProductivos =
                    resultado.Cargos
                        .Where(c =>
                            c != null &&
                            c.CargoExiste &&
                            c.RendimientoExiste &&
                            c.RequiereRendimientoProductivo)
                        .ToList();

            if (cargosProductivos.Count == 0 ||
                resultado.DiasTecnicos <= 0.0)
            {
                requerimiento.DiagnosticoParametros =
                    "La ecuacion vigente no pudo calcular horas. " +
                    string.Join(
                        " | ",
                        resultado.Advertencias
                            .Concat(resultado.Errores)
                    );

                return false;
            }

            double horasEstandar =
                resultado.DiasTecnicos *
                CalculoProductivoResolverService.HorasDiaEstandar;

            requerimiento.DiasPersonaStd =
                resultado.DiasTecnicos;

            requerimiento.DiasPersonaMin =
                resultado.DiasTecnicos;

            requerimiento.DiasPersonaHolgura =
                resultado.DiasTecnicos;

            requerimiento.HorasEstandar =
                horasEstandar;

            requerimiento.HorasMinimas =
                horasEstandar;

            requerimiento.HorasHolgura =
                horasEstandar;

            requerimiento.CostoEstandarCLP =
                resultado.CostoCLP;

            requerimiento.CostoMinimoCLP =
                resultado.CostoCLP;

            requerimiento.CostoHolguraCLP =
                resultado.CostoCLP;

            EcuacionProductivaRuntimeService.ResultadoCargo
                cuelloBotella =
                    cargosProductivos
                        .OrderByDescending(c =>
                            c.DiasTecnicos)
                        .First();

            requerimiento.RendimientoCantidad =
                cuelloBotella.CapacidadPorPeriodo;

            requerimiento.RendimientoPeriodo =
                cuelloBotella.Periodo;

            requerimiento.RendimientoOrigen =
                "Ecuacion vigente " +
                ecuacion.Clave +
                " / " +
                cuelloBotella.CargoResuelto;

            requerimiento.OrigenHoras =
                "Cantidad actual / capacidad de la ecuacion vigente";

            requerimiento.TieneOverrideLocalCalculo =
                false;

            requerimiento.EditadoManualmente =
                false;

            requerimiento.ParametrosCompletos =
                true;

            requerimiento.DiagnosticoParametros =
                "Cantidad=" +
                requerimiento.Cantidad.ToString("0.####") +
                " " +
                requerimiento.Unidad +
                " | Ecuacion=" +
                ecuacion.Clave +
                " | Capacidad=" +
                cuelloBotella.CapacidadPorPeriodo
                    .ToString("0.####") +
                " / " +
                cuelloBotella.Periodo +
                " | Horas=" +
                horasEstandar.ToString("0.####") +
                ".";

            return true;
        }
'@

$contenido = Reemplazar-Metodo `
    $contenido `
    "        private bool RecalcularRequerimientoDesdeEcuacionV16(" `
    $nuevoRecalculo

$nuevoSyncCantidad = @'
        private void SincronizarCantidadProcesoV4(
            ItemProyecto item,
            ProcesoProyecto proceso,
            decimal cantidad)
        {
            if (proceso == null ||
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

$contenido = Reemplazar-Metodo `
    $contenido `
    "        private void SincronizarCantidadProcesoV4(" `
    $nuevoSyncCantidad

$nuevoSyncLocal = @'
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

            List<EcuacionProductivaDefinicion> biblioteca =
                BibliotecaEcuacionesProductivasJsonService
                    .CargarEcuaciones();

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

                RequerimientoProduccionInterna principal =
                    relacionados
                        .OrderByDescending(r =>
                            r.HorasEstandar)
                        .First();

                EcuacionProductivaDefinicion ecuacion =
                    ResolverEcuacionVigenteV21(
                        principal,
                        biblioteca
                    );

                bool esManualVigente =
                    ecuacion != null &&
                    ecuacion.MetodoCalculo ==
                        MetodoCalculoProceso.Manual;

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

                if (!esManualVigente)
                {
                    proceso.Resultado.HorasAsignadas = 0m;

                    SincronizarAsignacionesAutomaticasV21(
                        proceso,
                        horas,
                        costo
                    );
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
            }
        }
'@

$contenido = Reemplazar-Metodo `
    $contenido `
    "        private void SincronizarResultadosLocalesDesdeSnapshotV18(" `
    $nuevoSyncLocal

$ancla =
    "        private void SincronizarCantidadProcesoV4("

$indice =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indice -lt 0) {
    throw "No se encontro el punto de insercion para V21."
}

$helpers = @'
        private EcuacionProductivaDefinicion
            ResolverEcuacionVigenteV21(
                RequerimientoProduccionInterna requerimiento,
                List<EcuacionProductivaDefinicion> biblioteca)
        {
            if (requerimiento == null)
            {
                return null;
            }

            biblioteca =
                biblioteca ??
                new List<EcuacionProductivaDefinicion>();

            string clave =
                ExtraerClaveEcuacionV16(
                    requerimiento.EcuacionUsada
                );

            EcuacionProductivaDefinicion ecuacion =
                biblioteca.FirstOrDefault(e =>
                    e != null &&
                    string.Equals(
                        e.Clave,
                        clave,
                        StringComparison.OrdinalIgnoreCase
                    ));

            if (ecuacion != null)
            {
                return ecuacion;
            }

            return BibliotecaEcuacionesProductivasJsonService
                .BuscarMejorPara(
                    requerimiento.EtapaSugerida,
                    requerimiento.TipoInterno,
                    requerimiento.NombreRequerimiento,
                    requerimiento.CargoSugerido
                );
        }

        private void SincronizarAsignacionesAutomaticasV21(
            ProcesoProyecto proceso,
            decimal horasTotales,
            decimal costoTotal)
        {
            if (proceso == null)
            {
                return;
            }

            List<AsignacionProductiva> asignaciones =
                (proceso.Asignaciones ??
                    new List<AsignacionProductiva>())
                .Where(a => a != null)
                .ToList();

            if (asignaciones.Count == 0)
            {
                return;
            }

            decimal horasBase =
                asignaciones.Sum(a =>
                    Math.Max(
                        0m,
                        a.HorasCalculadas
                    ));

            decimal costoBase =
                asignaciones.Sum(a =>
                    Math.Max(
                        0m,
                        a.CostoCalculado
                    ));

            decimal horasRestantes =
                Math.Max(0m, horasTotales);

            decimal costoRestante =
                Math.Max(0m, costoTotal);

            for (int i = 0;
                i < asignaciones.Count;
                i++)
            {
                AsignacionProductiva asignacion =
                    asignaciones[i];

                decimal proporcionHoras =
                    horasBase > 0m
                        ? Math.Max(
                            0m,
                            asignacion.HorasCalculadas
                          ) / horasBase
                        : 1m / asignaciones.Count;

                decimal proporcionCosto =
                    costoBase > 0m
                        ? Math.Max(
                            0m,
                            asignacion.CostoCalculado
                          ) / costoBase
                        : proporcionHoras;

                decimal horasNuevas =
                    i == asignaciones.Count - 1
                        ? horasRestantes
                        : Math.Round(
                            horasTotales *
                            proporcionHoras,
                            6
                        );

                decimal costoNuevo =
                    i == asignaciones.Count - 1
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
            }
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helpers +
    $contenido.Substring($indice)

$rangoV19 =
    Obtener-RangoMetodo `
        $contenido `
        "        private void SincronizarAsignacionesCalculadasV19("

if ($null -ne $rangoV19) {
    $contenido =
        $contenido.Substring(0, $rangoV19.Inicio) +
        $contenido.Substring($rangoV19.Fin + 1)
}

$contenido = [regex]::Replace(
    $contenido,
    '(?ms)\s*SincronizarAsignacionesCalculadasV19\s*\([^;]*?\)\s*;',
    ''
)

$contenido = $contenido.Replace(
    "OrigenHorasProductivas.Calculada",
    "OrigenHorasProductivas.Manual"
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_ecuacion_autoritativa_v21_" +
    (Get-Date -Format "yyyyMMdd_HHmmss")

Copy-Item $ruta $backup -Force

$utf8ConBom =
    New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "HOTFIX V21 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "La ecuacion vigente ahora manda sobre el metodo de calculo."
Write-Host "Los procesos por capacidad reciben la cantidad actual."
Write-Host "Solo las ecuaciones manuales conservan horas declaradas."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
