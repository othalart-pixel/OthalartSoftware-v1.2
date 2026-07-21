$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

function Obtener-RangoMetodo {
    param(
        [string]$Texto,
        [string]$Firma
    )

    $inicio = $Texto.IndexOf(
        $Firma,
        [StringComparison]::Ordinal
    )

    if ($inicio -lt 0) {
        return $null
    }

    $llave = $Texto.IndexOf(
        "{",
        $inicio,
        [StringComparison]::Ordinal
    )

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
    }
}

$firma =
    "        private void RecalcularProyectoProductivoActual("

$rango =
    Obtener-RangoMetodo $contenido $firma

if ($null -eq $rango) {
    throw "No se encontro RecalcularProyectoProductivoActual."
}

$nuevoMetodo = @'
        // RecalculoTransaccionalSeguroV26
        private void RecalcularProyectoProductivoActual()
        {
            if (proyectoCotizacionActual == null)
            {
                lblEstadoProyecto.Text =
                    "No hay un proyecto cargado para recalcular.";
                return;
            }

            List<ItemProyecto> items =
                (proyectoCotizacionActual.Grupos ??
                    new List<GrupoProyecto>())
                .Where(g => g != null && g.Activo)
                .SelectMany(g =>
                    g.Items ??
                    new List<ItemProyecto>())
                .Where(i => i != null && i.Activo)
                .ToList();

            if (items.Count == 0)
            {
                lblEstadoProyecto.Text =
                    "El proyecto no contiene productos activos.";
                return;
            }

            List<string> errores =
                new List<string>();

            int recalculados = 0;

            Cursor cursorAnterior = Cursor;

            try
            {
                Cursor = Cursors.WaitCursor;
                Enabled = false;

                foreach (ItemProyecto item in items)
                {
                    Cotizacion snapshotOriginal =
                        CargarSnapshotCotizacionItemProyecto(item);

                    if (snapshotOriginal == null)
                    {
                        errores.Add(
                            (item.Nombre ?? "Producto sin nombre") +
                            ": no tiene snapshot válido. " +
                            "Se conserva sin cambios."
                        );
                        continue;
                    }

                    string jsonOriginal =
                        JsonSerializer.Serialize(
                            snapshotOriginal
                        );

                    Cotizacion copiaTrabajo =
                        JsonSerializer.Deserialize<Cotizacion>(
                            jsonOriginal
                        );

                    if (copiaTrabajo == null)
                    {
                        errores.Add(
                            (item.Nombre ?? "Producto sin nombre") +
                            ": no se pudo crear copia de trabajo."
                        );
                        continue;
                    }

                    int procesosAntes =
                        ObtenerProcesosItemProyecto(item)
                            .Count(p => p != null);

                    int requerimientosAntes =
                        snapshotOriginal
                            .DesgloseProductivo?
                            .Requerimientos?
                            .Count(r => r != null) ?? 0;

                    double horasAntes =
                        snapshotOriginal
                            .DesgloseProductivo?
                            .Requerimientos?
                            .Where(r => r != null)
                            .Sum(r => Math.Max(
                                0.0,
                                r.HorasEstandar
                            )) ?? 0.0;

                    AplicarCantidadActualAlBriefSeguroV26(
                        item,
                        copiaTrabajo
                    );

                    DesgloseProductivoProyecto nuevoDesglose =
                        DesgloseProductivoService.Generar(
                            copiaTrabajo
                        );

                    if (!ValidarDesgloseRecalculadoV26(
                            nuevoDesglose,
                            requerimientosAntes,
                            procesosAntes,
                            horasAntes,
                            out string motivo))
                    {
                        errores.Add(
                            (item.Nombre ?? "Producto sin nombre") +
                            ": recálculo descartado. " +
                            motivo
                        );
                        continue;
                    }

                    copiaTrabajo.DesgloseProductivo =
                        nuevoDesglose;

                    GuardarSnapshotCotizacionItemProyecto(
                        item,
                        copiaTrabajo
                    );

                    SincronizarResultadosLocalesDesdeSnapshotV18(
                        item,
                        copiaTrabajo
                    );

                    item.FechaEdicionSnapshot =
                        DateTime.Now;

                    recalculados++;
                }
            }
            catch (Exception ex)
            {
                errores.Add(
                    "Error general: " +
                    ex.Message
                );
            }
            finally
            {
                Enabled = true;
                Cursor = cursorAnterior;
            }

            RefrescarProyectoUI();

            if (errores.Count == 0)
            {
                lblEstadoProyecto.Text =
                    "Recalculo seguro completado: " +
                    recalculados +
                    " producto(s).";

                return;
            }

            lblEstadoProyecto.Text =
                "Recalculo seguro: " +
                recalculados +
                " producto(s) actualizados; " +
                errores.Count +
                " descartados sin modificar.";

            MessageBox.Show(
                this,
                string.Join(
                    Environment.NewLine +
                    Environment.NewLine,
                    errores
                ),
                "Recalculo seguro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rango.Inicio) +
    $nuevoMetodo +
    $contenido.Substring($rango.Fin + 1)

$ancla =
    "        private void RecalcularProyectoProductivoActual("

$indice =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indice -lt 0) {
    throw "No se encontro punto de insercion V26."
}

$helpers = @'
        private void AplicarCantidadActualAlBriefSeguroV26(
            ItemProyecto item,
            Cotizacion snapshot)
        {
            if (item == null ||
                snapshot?.BriefProducto?
                    .EntregablesSeleccionados == null)
            {
                return;
            }

            double cantidadActual =
                Convert.ToDouble(
                    Math.Max(
                        0m,
                        item.Cantidad
                    )
                );

            foreach (EntregableBrief entregable in
                snapshot.BriefProducto
                    .EntregablesSeleccionados
                    .Where(e => e != null))
            {
                string unidadDuracion =
                    NormalizarUnidadCantidadProyectoV4(
                        entregable.UnidadDuracion
                    );

                if (unidadDuracion != "segundo")
                {
                    continue;
                }

                entregable.Cantidad = 1;
                entregable.DuracionPorUnidad =
                    cantidadActual;
                entregable.UnidadDuracion =
                    "segundos";

                if (!ModosCalculoProductivo.EsTiempoAsignado(
                        entregable.ModoCalculoProductivo))
                {
                    entregable.HorasAsignadasMin = 0.0;
                    entregable.HorasAsignadasStd = 0.0;
                    entregable.HorasAsignadasHolgura = 0.0;
                }
            }
        }

        private bool ValidarDesgloseRecalculadoV26(
            DesgloseProductivoProyecto desglose,
            int requerimientosAntes,
            int procesosAntes,
            double horasAntes,
            out string motivo)
        {
            motivo = "";

            if (desglose?.Requerimientos == null)
            {
                motivo =
                    "el motor devolvió un desglose nulo.";

                return false;
            }

            List<RequerimientoProduccionInterna> reqs =
                desglose.Requerimientos
                    .Where(r => r != null)
                    .ToList();

            if (reqs.Count == 0)
            {
                motivo =
                    "el motor devolvió cero requerimientos.";

                return false;
            }

            if (requerimientosAntes > 0 &&
                reqs.Count <
                    Math.Max(
                        1,
                        requerimientosAntes / 2
                    ))
            {
                motivo =
                    "desapareció más de la mitad de los requerimientos.";

                return false;
            }

            int conCantidadCero =
                reqs.Count(r =>
                    r.Cantidad <= 0.0 &&
                    !ModosCalculoProductivo.EsTiempoAsignado(
                        r.ModoCalculoProductivo
                    ));

            if (conCantidadCero >
                Math.Max(
                    1,
                    reqs.Count / 3
                ))
            {
                motivo =
                    "demasiados procesos quedaron con cantidad cero.";

                return false;
            }

            double horasNuevas =
                reqs.Sum(r =>
                    Math.Max(
                        0.0,
                        r.HorasEstandar
                    ));

            if (horasNuevas <= 0.0)
            {
                motivo =
                    "el total de horas quedó en cero.";

                return false;
            }

            if (horasAntes > 0.0 &&
                horasNuevas > horasAntes * 10.0)
            {
                motivo =
                    "las horas aumentaron más de 10 veces (" +
                    horasAntes.ToString("0.##") +
                    " -> " +
                    horasNuevas.ToString("0.##") +
                    ").";

                return false;
            }

            return true;
        }

'@

$contenido =
    $contenido.Substring(0, $indice) +
    $helpers +
    $contenido.Substring($indice)

# Quitar helpers destructivos V24/V25 si todavía permanecen.
foreach ($firmaVieja in @(
    "        private void AplicarCantidadActualAlBriefV24(",
    "        private Cotizacion ReconstruirSnapshotFaltanteV25("
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
    "$ruta.backup_recalculo_seguro_v26_" +
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
Write-Host "HOTFIX V26 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "El boton Recalcular ahora trabaja sobre una copia."
Write-Host "Los resultados invalidos se descartan sin modificar el producto."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
