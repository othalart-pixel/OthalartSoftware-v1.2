$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalcularRequerimientoDesdeEcuacionV16(")) {
    Write-Host "La correccion V16 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# Reemplaza las llamadas genéricas del V15 por la evaluación real
# de la ecuación vinculada.
$patron = @'
                CalculoProductivoResolverService.Aplicar(
                    requerimiento,
                    diasHabilesSemana
                );
'@

$reemplazo = @'
                RecalcularRequerimientoDesdeEcuacionV16(
                    requerimiento,
                    diasHabilesSemana
                );
'@

if (-not $contenido.Contains($patron)) {
    throw "No se encontro la llamada global al resolver del V15."
}

$contenido = $contenido.Replace($patron, $reemplazo)

$patronReq = @'
                CalculoProductivoResolverService.Aplicar(
                    req,
                    diasHabilesSemana
                );
'@

$reemplazoReq = @'
                RecalcularRequerimientoDesdeEcuacionV16(
                    req,
                    diasHabilesSemana
                );
'@

if (-not $contenido.Contains($patronReq)) {
    throw "No se encontro la llamada individual al resolver del V15."
}

$contenido = $contenido.Replace($patronReq, $reemplazoReq)

$ancla = "        private void SincronizarCantidadProcesoV4("
$indice = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro SincronizarCantidadProcesoV4 para insertar el helper."
}

$helper = @'
        private bool RecalcularRequerimientoDesdeEcuacionV16(
            RequerimientoProduccionInterna requerimiento,
            double diasHabilesSemana)
        {
            if (requerimiento == null)
            {
                return false;
            }

            // Las horas declaradas manualmente siguen usando el resolver
            // oficial para no tocar la decisión del usuario.
            if (ModosCalculoProductivo.EsTiempoAsignado(
                    requerimiento.ModoCalculoProductivo))
            {
                return CalculoProductivoResolverService.Aplicar(
                    requerimiento,
                    diasHabilesSemana
                );
            }

            List<EcuacionProductivaDefinicion> biblioteca =
                BibliotecaEcuacionesProductivasJsonService
                    .CargarEcuaciones();

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

            if (ecuacion == null)
            {
                ecuacion =
                    BibliotecaEcuacionesProductivasJsonService
                        .BuscarMejorPara(
                            requerimiento.EtapaSugerida,
                            requerimiento.TipoInterno,
                            requerimiento.NombreRequerimiento,
                            requerimiento.CargoSugerido
                        );
            }

            if (ecuacion == null)
            {
                // Fallback seguro para requerimientos antiguos sin vínculo.
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
                    "La ecuacion vinculada no pudo calcular horas. " +
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

            requerimiento.HorasEstandar =
                horasEstandar;

            // Por ahora se conserva el estándar como referencia común.
            // Las bandas min/holgura pueden reconstruirse después desde
            // los valores mínimo/máximo de la biblioteca.
            requerimiento.DiasPersonaMin =
                resultado.DiasTecnicos;

            requerimiento.DiasPersonaHolgura =
                resultado.DiasTecnicos;

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
                "Ecuacion " +
                ecuacion.Clave +
                " / " +
                cuelloBotella.CargoResuelto;

            requerimiento.OrigenHoras =
                "Ecuacion productiva: cantidad / rendimiento";

            requerimiento.TieneOverrideLocalCalculo =
                false;

            requerimiento.EditadoManualmente =
                false;

            requerimiento.ParametrosCompletos =
                true;

            requerimiento.DiagnosticoParametros =
                "Calculado desde la ecuacion vinculada. " +
                "Cuello de botella: " +
                cuelloBotella.CargoResuelto +
                " | " +
                cuelloBotella.CapacidadPorPeriodo
                    .ToString("0.####") +
                " " +
                requerimiento.Unidad +
                " / " +
                cuelloBotella.Periodo +
                ".";

            return true;
        }

        private string ExtraerClaveEcuacionV16(
            string ecuacionUsada)
        {
            string texto =
                (ecuacionUsada ?? "").Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return "";
            }

            int separador =
                texto.IndexOf(
                    '|',
                    StringComparison.Ordinal
                );

            if (separador >= 0)
            {
                texto =
                    texto.Substring(0, separador);
            }

            return texto.Trim();
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
    "$ruta.backup_ecuaciones_runtime_v16_" +
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
Write-Host "Correccion V16 aplicada." -ForegroundColor Green
Write-Host "El proyecto ahora usa el mismo runtime que la prueba de Ecuaciones."
Write-Host "Cantidad -> rendimiento por cargo -> cuello de botella -> horas -> costo."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
