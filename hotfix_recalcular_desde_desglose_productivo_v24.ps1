$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalculoDesdeDesgloseProductivoV24")) {
    Write-Host "El hotfix V24 ya esta aplicado." -ForegroundColor Yellow
    exit 0
}

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
        // RecalculoDesdeDesgloseProductivoV24
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

            object seleccionAnterior =
                nodoProyectoSeleccionado;

            ItemProyecto itemAnterior =
                itemProyectoSeleccionado;

            int recalculados = 0;
            List<string> errores =
                new List<string>();

            Cursor cursorAnterior = Cursor;

            try
            {
                Cursor = Cursors.WaitCursor;
                Enabled = false;

                foreach (ItemProyecto item in items)
                {
                    try
                    {
                        Cotizacion snapshot =
                            CargarSnapshotCotizacionItemProyecto(item);

                        if (snapshot == null)
                        {
                            errores.Add(
                                (item.Nombre ?? "Producto sin nombre") +
                                ": no se pudo cargar el snapshot."
                            );
                            continue;
                        }

                        AplicarCantidadActualAlBriefV24(
                            item,
                            snapshot
                        );

                        // Se descarta el desglose derivado anterior y se
                        // reconstruye con el mismo motor que funciona en
                        // la pestaña Desglose productivo.
                        snapshot.DesgloseProductivo =
                            DesgloseProductivoService.Generar(
                                snapshot
                            );

                        GuardarSnapshotCotizacionItemProyecto(
                            item,
                            snapshot
                        );

                        SincronizarResultadosLocalesDesdeSnapshotV18(
                            item,
                            snapshot
                        );

                        item.FechaEdicionSnapshot =
                            DateTime.Now;

                        recalculados++;
                    }
                    catch (Exception ex)
                    {
                        errores.Add(
                            (item.Nombre ?? "Producto sin nombre") +
                            ": " +
                            ex.Message
                        );
                    }
                }
            }
            finally
            {
                Enabled = true;
                Cursor = cursorAnterior;
            }

            itemProyectoSeleccionado =
                itemAnterior;

            nodoProyectoSeleccionado =
                seleccionAnterior;

            RefrescarProyectoUI();

            if (errores.Count == 0)
            {
                lblEstadoProyecto.Text =
                    "Recalculo completo desde Desglose productivo: " +
                    recalculados +
                    " producto(s).";
                return;
            }

            lblEstadoProyecto.Text =
                "Recalculo parcial: " +
                recalculados +
                " producto(s) actualizados y " +
                errores.Count +
                " con errores.";

            MessageBox.Show(
                this,
                "El proyecto se recalculo parcialmente." +
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
    throw "No se encontro el punto para insertar el helper V24."
}

$helper = @'
        private void AplicarCantidadActualAlBriefV24(
            ItemProyecto item,
            Cotizacion snapshot)
        {
            if (item == null ||
                snapshot?.BriefProducto?
                    .EntregablesSeleccionados == null)
            {
                return;
            }

            decimal cantidadProducto =
                Math.Max(
                    0m,
                    item.Cantidad
                );

            string unidadProducto =
                NormalizarUnidadCantidadProyectoV4(
                    ObtenerUnidadVisibleItem(item)
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

                bool seMideEnSegundos =
                    unidadProducto == "segundo" ||
                    unidadDuracion == "segundo";

                if (!seMideEnSegundos)
                {
                    continue;
                }

                // DesgloseProductivoService calcula:
                // DuracionPorUnidad * Cantidad.
                // Para que el total productivo sea exactamente la
                // cantidad actual del producto, se expresa como una sola
                // unidad de esa duración.
                entregable.Cantidad = 1;
                entregable.DuracionPorUnidad =
                    Convert.ToDouble(
                        cantidadProducto
                    );
                entregable.UnidadDuracion =
                    "segundos";

                if (entregable.SegundosAnimadosEfectivos.HasValue)
                {
                    entregable.SegundosAnimadosEfectivos =
                        Convert.ToDouble(
                            cantidadProducto
                        );
                }

                // Las horas automáticas deben volver a ser derivadas
                // desde rendimiento. Solo se conservan horas cuando el
                // entregable está declarado explícitamente como manual.
                if (!ModosCalculoProductivo.EsTiempoAsignado(
                        entregable.ModoCalculoProductivo))
                {
                    entregable.HorasAsignadasMin = 0.0;
                    entregable.HorasAsignadasStd = 0.0;
                    entregable.HorasAsignadasHolgura = 0.0;
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
    "$ruta.backup_desglose_v24_" +
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
Write-Host "HOTFIX V24 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "El boton Recalcular ahora regenera el snapshot"
Write-Host "con DesgloseProductivoService.Generar()."
Write-Host ""
Write-Host "Cantidad -> brief -> ecuaciones -> rendimientos"
Write-Host "-> horas -> costos -> tabla."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
