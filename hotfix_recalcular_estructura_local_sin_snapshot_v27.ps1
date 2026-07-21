$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalculoLocalSinSnapshotV27")) {
    Write-Host "El hotfix V27 ya esta aplicado." -ForegroundColor Yellow
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

    if ($fin -lt 0) { return $null }

    return @{
        Inicio = $inicio
        Fin = $fin
    }
}

$firma = "        private void RecalcularProyectoProductivoActual("
$rango = Obtener-RangoMetodo $contenido $firma

if ($null -eq $rango) {
    throw "No se encontro RecalcularProyectoProductivoActual."
}

$nuevoMetodo = @'
        // RecalculoLocalSinSnapshotV27
        private void RecalcularProyectoProductivoActual()
        {
            if (proyectoCotizacionActual == null)
            {
                lblEstadoProyecto.Text =
                    "No hay un proyecto cargado para recalcular.";
                return;
            }

            double diasHabilesSemana =
                cotizacion != null &&
                cotizacion.DiasHabilesEstudioPorSemana > 0.0
                    ? cotizacion.DiasHabilesEstudioPorSemana
                    : 5.0;

            DesgloseProductivoProyecto globalOriginal =
                ProyectoDesgloseGlobalService.Construir(
                    proyectoCotizacionActual,
                    cotizacion
                );

            if (globalOriginal?.Requerimientos == null ||
                globalOriginal.Requerimientos.Count == 0)
            {
                lblEstadoProyecto.Text =
                    "No se encontraron procesos locales para recalcular.";

                MessageBox.Show(
                    this,
                    "El proyecto no contiene requerimientos productivos locales.",
                    "Recalcular proyecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string jsonSeguridad =
                JsonSerializer.Serialize(
                    globalOriginal
                );

            DesgloseProductivoProyecto globalTrabajo =
                JsonSerializer.Deserialize<DesgloseProductivoProyecto>(
                    jsonSeguridad
                );

            if (globalTrabajo?.Requerimientos == null)
            {
                lblEstadoProyecto.Text =
                    "No se pudo crear una copia segura del desglose.";
                return;
            }

            int exitosos = 0;
            List<string> errores =
                new List<string>();

            Cursor cursorAnterior = Cursor;

            try
            {
                Cursor = Cursors.WaitCursor;
                Enabled = false;

                foreach (RequerimientoProduccionInterna req in
                    globalTrabajo.Requerimientos
                        .Where(r => r != null))
                {
                    try
                    {
                        // La cantidad proviene de la estructura local:
                        // item/subproducto/proceso. No se usa snapshot.
                        bool calculado =
                            RecalcularRequerimientoDesdeEcuacionV16(
                                req,
                                diasHabilesSemana
                            );

                        if (!calculado)
                        {
                            errores.Add(
                                (req.NombreRequerimiento ??
                                    "Proceso sin nombre") +
                                ": " +
                                (req.DiagnosticoParametros ??
                                    "no pudo calcularse.")
                            );
                            continue;
                        }

                        exitosos++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add(
                            (req.NombreRequerimiento ??
                                "Proceso sin nombre") +
                            ": " +
                            ex.Message
                        );
                    }
                }

                bool resultadoValido =
                    ValidarRecalculoLocalV27(
                        globalOriginal,
                        globalTrabajo,
                        out string motivoValidacion
                    );

                if (!resultadoValido)
                {
                    MessageBox.Show(
                        this,
                        "El recálculo fue descartado y el proyecto quedó intacto." +
                        Environment.NewLine +
                        Environment.NewLine +
                        motivoValidacion,
                        "Recalculo seguro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    lblEstadoProyecto.Text =
                        "Recalculo descartado sin modificar el proyecto.";

                    return;
                }

                ProyectoDesgloseGlobalService.Aplicar(
                    proyectoCotizacionActual,
                    globalTrabajo,
                    cotizacion
                );

                if (cotizacion != null)
                {
                    cotizacion.DesgloseProductivo =
                        globalTrabajo;
                }
            }
            finally
            {
                Enabled = true;
                Cursor = cursorAnterior;
            }

            RefrescarProyectoUI();

            lblEstadoProyecto.Text =
                "Recalculo local completo: " +
                exitosos +
                " requerimiento(s) actualizados desde ecuaciones vigentes.";

            if (errores.Count > 0)
            {
                MessageBox.Show(
                    this,
                    "El recálculo terminó con advertencias:" +
                    Environment.NewLine +
                    Environment.NewLine +
                    string.Join(
                        Environment.NewLine,
                        errores
                    ),
                    "Recalcular proyecto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
'@

$contenido =
    $contenido.Substring(0, $rango.Inicio) +
    $nuevoMetodo +
    $contenido.Substring($rango.Fin + 1)

$ancla = "        private void RecalcularProyectoProductivoActual("
$indice = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro punto de insercion V27."
}

$helper = @'
        private bool ValidarRecalculoLocalV27(
            DesgloseProductivoProyecto anterior,
            DesgloseProductivoProyecto nuevo,
            out string motivo)
        {
            motivo = "";

            List<RequerimientoProduccionInterna> antes =
                anterior?.Requerimientos?
                    .Where(r => r != null)
                    .ToList() ??
                new List<RequerimientoProduccionInterna>();

            List<RequerimientoProduccionInterna> despues =
                nuevo?.Requerimientos?
                    .Where(r => r != null)
                    .ToList() ??
                new List<RequerimientoProduccionInterna>();

            if (despues.Count == 0)
            {
                motivo =
                    "El motor devolvió cero requerimientos.";
                return false;
            }

            if (antes.Count > 0 &&
                despues.Count != antes.Count)
            {
                motivo =
                    "Cambió la cantidad de requerimientos (" +
                    antes.Count +
                    " -> " +
                    despues.Count +
                    ").";
                return false;
            }

            int cantidadesPositivasAntes =
                antes.Count(r => r.Cantidad > 0.0);

            int cantidadesPositivasDespues =
                despues.Count(r => r.Cantidad > 0.0);

            if (cantidadesPositivasDespues <
                cantidadesPositivasAntes)
            {
                motivo =
                    "Se perdieron cantidades productivas durante el cálculo.";
                return false;
            }

            double horasNuevas =
                despues.Sum(r =>
                    Math.Max(
                        0.0,
                        r.HorasEstandar
                    ));

            if (horasNuevas <= 0.0)
            {
                motivo =
                    "El total calculado de horas quedó en cero.";
                return false;
            }

            foreach (RequerimientoProduccionInterna req in despues)
            {
                if (double.IsNaN(req.HorasEstandar) ||
                    double.IsInfinity(req.HorasEstandar) ||
                    req.HorasEstandar < 0.0)
                {
                    motivo =
                        "Se detectaron horas inválidas en " +
                        (req.NombreRequerimiento ??
                            "un proceso") +
                        ".";
                    return false;
                }

                if (double.IsNaN(req.CostoEstandarCLP) ||
                    double.IsInfinity(req.CostoEstandarCLP) ||
                    req.CostoEstandarCLP < 0.0)
                {
                    motivo =
                        "Se detectaron costos inválidos en " +
                        (req.NombreRequerimiento ??
                            "un proceso") +
                        ".";
                    return false;
                }
            }

            return true;
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helper +
    $contenido.Substring($indice)

# Retirar helpers destructivos o dependientes de snapshot si siguen presentes.
foreach ($firmaVieja in @(
    "        private void AplicarCantidadActualAlBriefV24(",
    "        private Cotizacion ReconstruirSnapshotFaltanteV25(",
    "        private void AplicarCantidadActualAlBriefSeguroV26(",
    "        private bool ValidarDesgloseRecalculadoV26("
)) {
    $rangoViejo =
        Obtener-RangoMetodo $contenido $firmaVieja

    if ($null -ne $rangoViejo) {
        $contenido =
            $contenido.Substring(0, $rangoViejo.Inicio) +
            $contenido.Substring($rangoViejo.Fin + 1)
    }
}

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_recalculo_local_v27_" +
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
Write-Host "HOTFIX V27 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "Recalcular ya no necesita snapshot."
Write-Host "Usa la estructura local real del proyecto y sus IDs."
Write-Host "El resultado se valida antes de aplicarse."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
