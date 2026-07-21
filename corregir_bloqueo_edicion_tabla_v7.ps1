$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalcularProyectoDespuesDeEdicionV7(")) {
    Write-Host "La correccion de interaccion V7 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

$firma = "        private void AplicarEdicionTablaProductos(int rowIndex, int columnIndex)"
$inicio = $contenido.IndexOf($firma, [StringComparison]::Ordinal)

if ($inicio -lt 0) {
    throw "No se encontro AplicarEdicionTablaProductos."
}

$llave = $contenido.IndexOf("{", $inicio, [StringComparison]::Ordinal)
$profundidad = 0
$fin = -1

for ($i = $llave; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') {
        $profundidad++
    }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--
        if ($profundidad -eq 0) {
            $fin = $i
            break
        }
    }
}

if ($fin -lt 0) {
    throw "No se encontro el final de AplicarEdicionTablaProductos."
}

$nuevoMetodo = @'
        private void AplicarEdicionTablaProductos(int rowIndex, int columnIndex)
        {
            if (aplicandoEdicionTablaProyecto ||
                refrescandoProyectoUI ||
                rowIndex < 0 ||
                columnIndex < 0)
            {
                return;
            }

            DataGridViewRow row =
                dgvProductosProyecto.Rows[rowIndex];

            object nodo = row.Tag;
            if (nodo == null)
            {
                return;
            }

            string col =
                dgvProductosProyecto.Columns[columnIndex].Name;

            columnaProyectoSeleccionada = col;

            if (!EsColumnaEditableTablaProductosProyecto(col))
            {
                return;
            }

            string valor =
                Convert.ToString(
                    row.Cells[columnIndex].Value
                ) ?? "";

            bool cambio = false;

            aplicandoEdicionTablaProyecto = true;
            try
            {
                cambio =
                    AplicarCambioNodoTablaProductosProyecto(
                        nodo,
                        col,
                        valor.Trim()
                    );

                if (cambio)
                {
                    MarcarNodoProyectoModificado(
                        nodo,
                        col,
                        valor.Trim()
                    );

                    nodoProyectoSeleccionado = nodo;
                    itemProyectoSeleccionado =
                        nodo as ItemProyecto ??
                        ObtenerItemContenedor(nodo);
                }
            }
            finally
            {
                aplicandoEdicionTablaProyecto = false;
            }

            if (!cambio)
            {
                return;
            }

            // Nunca reconstruir el DataGridView dentro de CellEndEdit.
            // Primero se cierra completamente la edición actual.
            try
            {
                if (dgvProductosProyecto.IsCurrentCellInEditMode)
                {
                    dgvProductosProyecto.EndEdit();
                }
            }
            catch
            {
                // El control puede haber cerrado ya la celda.
            }

            RecalcularProyectoDespuesDeEdicionV7(
                nodo,
                col
            );
        }

        private void RecalcularProyectoDespuesDeEdicionV7(
            object nodo,
            string columna)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            Action recalcular = () =>
            {
                if (IsDisposed || Disposing)
                {
                    return;
                }

                aplicandoEdicionTablaProyecto = false;

                RecalcularProyectoProductivoActual();

                // Recuperar una selección válida después de reconstruir filas.
                nodoProyectoSeleccionado = nodo;
                itemProyectoSeleccionado =
                    nodo as ItemProyecto ??
                    ObtenerItemContenedor(nodo);

                MostrarEditorNodoProyecto(nodo);

                if (dgvProductosProyecto != null &&
                    !dgvProductosProyecto.IsDisposed)
                {
                    dgvProductosProyecto.ReadOnly = false;
                    dgvProductosProyecto.Enabled = true;
                    dgvProductosProyecto.TabStop = true;
                }
            };

            if (IsHandleCreated)
            {
                BeginInvoke(recalcular);
            }
            else
            {
                recalcular();
            }
        }
'@

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $nuevoMetodo +
    $contenido.Substring($fin + 1)

if ($contenidoNuevo -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_interaccion_v7_" +
    (Get-Date -Format 'yyyyMMdd_HHmmss')

Copy-Item $ruta $backup -Force

$utf8ConBom =
    New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenidoNuevo,
    $utf8ConBom
)

Write-Host ""
Write-Host "Correccion de interaccion V7 aplicada." -ForegroundColor Green
Write-Host "El recalculo ahora ocurre despues de cerrar la edicion de la celda."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
