$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este hotfix desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("DiagnosticoRecalculoHorasV23")) {
    Write-Host "El diagnostico V23 ya esta aplicado." -ForegroundColor Yellow
    exit 0
}

# Agregar helpers antes del metodo RecalcularProyectoProductivoActual.
$ancla = "        private void RecalcularProyectoProductivoActual("
$indice = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro RecalcularProyectoProductivoActual."
}

$helpers = @'
        private string RutaDiagnosticoRecalculoHorasV23()
        {
            return System.IO.Path.Combine(
                AppContext.BaseDirectory,
                "diagnostico-recalculo-horas.log"
            );
        }

        private void LimpiarDiagnosticoRecalculoHorasV23()
        {
            try
            {
                System.IO.File.WriteAllText(
                    RutaDiagnosticoRecalculoHorasV23(),
                    "=== DIAGNOSTICO RECALCULO HORAS V23 ===" +
                    Environment.NewLine +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    Environment.NewLine +
                    Environment.NewLine
                );
            }
            catch
            {
            }
        }

        private void EscribirDiagnosticoRecalculoHorasV23(
            string texto)
        {
            try
            {
                System.IO.File.AppendAllText(
                    RutaDiagnosticoRecalculoHorasV23(),
                    (texto ?? "") +
                    Environment.NewLine
                );
            }
            catch
            {
            }
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helpers +
    $contenido.Substring($indice)

# Insertar inicio del log dentro de RecalcularProyectoProductivoActual.
$patronInicio = @'
        private void RecalcularProyectoProductivoActual()
        {
'@

$reemplazoInicio = @'
        // DiagnosticoRecalculoHorasV23
        private void RecalcularProyectoProductivoActual()
        {
            LimpiarDiagnosticoRecalculoHorasV23();

            EscribirDiagnosticoRecalculoHorasV23(
                "Inicio del recálculo autoritativo."
            );
'@

if (-not $contenido.Contains($patronInicio)) {
    throw "No se encontro el inicio del metodo RecalcularProyectoProductivoActual."
}

$contenido = $contenido.Replace(
    $patronInicio,
    $reemplazoInicio
)

# Registrar antes y despues de cada item.
$patronItem = @'
                        SincronizarCantidadProductoYDescendientesV4(
                            item,
                            cantidadActual
                        );

                        recalculados++;
'@

$reemplazoItem = @'
                        EscribirDiagnosticoRecalculoHorasV23(
                            "ITEM: " +
                            (item.Nombre ?? "(sin nombre)") +
                            " | Cantidad actual: " +
                            cantidadActual.ToString("0.####") +
                            " | Unidad: " +
                            (item.Unidad ?? "")
                        );

                        SincronizarCantidadProductoYDescendientesV4(
                            item,
                            cantidadActual
                        );

                        Cotizacion snapshotDiagnostico =
                            CargarSnapshotCotizacionItemProyecto(item);

                        List<RequerimientoProduccionInterna>
                            requerimientosDiagnostico =
                                snapshotDiagnostico?
                                    .DesgloseProductivo?
                                    .Requerimientos?
                                    .Where(r => r != null)
                                    .ToList() ??
                                new List<RequerimientoProduccionInterna>();

                        foreach (
                            RequerimientoProduccionInterna reqDiag
                            in requerimientosDiagnostico)
                        {
                            EscribirDiagnosticoRecalculoHorasV23(
                                "  REQ: " +
                                (reqDiag.NombreRequerimiento ?? "(sin nombre)") +
                                " | ProcesoId: " +
                                (reqDiag.ProcesoId ?? "") +
                                " | Cantidad: " +
                                reqDiag.Cantidad.ToString("0.####") +
                                " " +
                                (reqDiag.Unidad ?? "") +
                                " | Ecuacion: " +
                                (reqDiag.EcuacionUsada ?? "") +
                                " | Modo: " +
                                (reqDiag.ModoCalculoProductivo ?? "") +
                                " | Capacidad: " +
                                reqDiag.RendimientoCantidad.ToString("0.####") +
                                " / " +
                                (reqDiag.RendimientoPeriodo ?? "") +
                                " | Horas: " +
                                reqDiag.HorasEstandar.ToString("0.####") +
                                " | Costo: " +
                                reqDiag.CostoEstandarCLP.ToString("0.##") +
                                " | Override: " +
                                reqDiag.TieneOverrideLocalCalculo +
                                " | Editado: " +
                                reqDiag.EditadoManualmente +
                                " | Origen: " +
                                (reqDiag.OrigenHoras ?? "") +
                                " | Diagnostico: " +
                                (reqDiag.DiagnosticoParametros ?? "")
                            );
                        }

                        foreach (
                            ProcesoProyecto procDiag
                            in ObtenerProcesosItemProyecto(item)
                                .Where(p => p != null))
                        {
                            EscribirDiagnosticoRecalculoHorasV23(
                                "  PROC LOCAL: " +
                                (procDiag.Nombre ?? "(sin nombre)") +
                                " | Id: " +
                                (procDiag.Id ?? "") +
                                " | Cantidad: " +
                                procDiag.Cantidad.ToString("0.####") +
                                " " +
                                (procDiag.Unidad ?? "") +
                                " | Metodo local: " +
                                procDiag.MetodoCalculo +
                                " | Capacidad: " +
                                procDiag.Capacidad.ToString("0.####") +
                                " / " +
                                (procDiag.Periodo ?? "") +
                                " | Horas resultado: " +
                                (procDiag.Resultado?
                                    .HorasCalculadas ?? 0m)
                                    .ToString("0.####") +
                                " | Horas asignadas: " +
                                (procDiag.Resultado?
                                    .HorasAsignadas ?? 0m)
                                    .ToString("0.####") +
                                " | Costo: " +
                                (procDiag.Resultado?
                                    .CostoCalculado ?? 0m)
                                    .ToString("0.##")
                            );

                            foreach (
                                AsignacionProductiva asigDiag
                                in (procDiag.Asignaciones ??
                                    new List<AsignacionProductiva>())
                                    .Where(a => a != null))
                            {
                                EscribirDiagnosticoRecalculoHorasV23(
                                    "    ASIG: " +
                                    (asigDiag.CargoId ?? "") +
                                    " | Origen: " +
                                    asigDiag.OrigenHoras +
                                    " | Horas calc: " +
                                    asigDiag.HorasCalculadas
                                        .ToString("0.####") +
                                    " | Horas asignadas: " +
                                    asigDiag.HorasAsignadas
                                        .ToString("0.####") +
                                    " | Costo: " +
                                    asigDiag.CostoCalculado
                                        .ToString("0.##")
                                );
                            }
                        }

                        EscribirDiagnosticoRecalculoHorasV23(
                            "--------------------------------------------------"
                        );

                        recalculados++;
'@

if (-not $contenido.Contains($patronItem)) {
    throw "No se encontro la llamada de recálculo V22 para instrumentarla."
}

$contenido = $contenido.Replace(
    $patronItem,
    $reemplazoItem
)

# Agregar ruta del log al final exitoso.
$patronExito = @'
                lblEstadoProyecto.Text =
                    "Recalculo completo: " +
                    recalculados +
                    " producto(s) reconstruidos desde las " +
                    "ecuaciones y rendimientos vigentes.";
                return;
'@

$reemplazoExito = @'
                lblEstadoProyecto.Text =
                    "Recalculo completo: " +
                    recalculados +
                    " producto(s). Log: diagnostico-recalculo-horas.log";

                EscribirDiagnosticoRecalculoHorasV23(
                    "FIN CORRECTO. Productos recalculados: " +
                    recalculados
                );

                MessageBox.Show(
                    this,
                    "Se genero el diagnostico en:" +
                    Environment.NewLine +
                    RutaDiagnosticoRecalculoHorasV23(),
                    "Diagnostico de recálculo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
'@

if (-not $contenido.Contains($patronExito)) {
    throw "No se encontro el bloque de final exitoso de V22."
}

$contenido = $contenido.Replace(
    $patronExito,
    $reemplazoExito
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_diagnostico_v23_" +
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
Write-Host "DIAGNOSTICO V23 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "Al pulsar Recalcular se generara:"
Write-Host "bin\Debug\net8.0-windows\diagnostico-recalculo-horas.log"
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
