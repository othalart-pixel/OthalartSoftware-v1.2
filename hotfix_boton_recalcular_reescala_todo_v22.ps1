$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este hotfix desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalculoAutoritativoBotonV22")) {
    Write-Host "El hotfix V22 ya esta aplicado." -ForegroundColor Yellow
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
        // RecalculoAutoritativoBotonV22
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
                        decimal cantidadActual =
                            Math.Max(
                                0m,
                                item.Cantidad
                            );

                        // Esta llamada vuelve a ejecutar el flujo completo:
                        // cantidad actual -> ecuacion vigente -> capacidad
                        // -> horas -> costos -> proceso local -> tabla.
                        SincronizarCantidadProductoYDescendientesV4(
                            item,
                            cantidadActual
                        );

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
                    "Recalculo completo: " +
                    recalculados +
                    " producto(s) reconstruidos desde las " +
                    "ecuaciones y rendimientos vigentes.";
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

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_recalcular_v22_" +
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
Write-Host "HOTFIX V22 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "El boton Recalcular ahora reconstruye todos los productos"
Write-Host "desde sus cantidades actuales y ecuaciones vigentes."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
