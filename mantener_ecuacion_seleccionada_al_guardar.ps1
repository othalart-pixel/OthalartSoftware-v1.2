$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabEcuaciones.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabEcuaciones.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$viejoClick = '            btnGuardar.Click += (s, e) => GuardarBibliotecaEcuacionesProductivas();'

$nuevoClick = @'
            btnGuardar.Click += (s, e) =>
            {
                string idSeleccionado = ObtenerIdentidadEcuacionSeleccionada();
                int pestañaInterna = tabsEcuacionesProductivasInternas == null
                    ? 0
                    : tabsEcuacionesProductivasInternas.SelectedIndex;

                int primeraFilaVisible = -1;
                if (dgvEcuacionesProductivas != null &&
                    dgvEcuacionesProductivas.Rows.Count > 0)
                {
                    try
                    {
                        primeraFilaVisible =
                            dgvEcuacionesProductivas.FirstDisplayedScrollingRowIndex;
                    }
                    catch
                    {
                        primeraFilaVisible = -1;
                    }
                }

                GuardarBibliotecaEcuacionesProductivas();

                RestaurarContextoEcuacionDespuesDeGuardar(
                    idSeleccionado,
                    pestañaInterna,
                    primeraFilaVisible
                );
            };
'@

if (-not $contenido.Contains($viejoClick)) {
    throw "No se encontro el evento actual de Guardar biblioteca. No se modifico nada."
}

$contenido = $contenido.Replace($viejoClick, $nuevoClick)

$ancla = "        private void SetColEcuacion(string nombre, int ancho)"

if (-not $contenido.Contains($ancla)) {
    throw "No se encontro el punto para insertar los helpers."
}

$helpers = @'
        private string ObtenerIdentidadEcuacionSeleccionada()
        {
            DataGridViewRow fila = dgvEcuacionesProductivas?.CurrentRow;

            if (fila == null || fila.IsNewRow)
            {
                return "";
            }

            string idProceso = ObtenerValorFilaEcuacion(fila, "IdProceso");
            if (!string.IsNullOrWhiteSpace(idProceso))
            {
                return "ID:" + idProceso.Trim();
            }

            string clave = ObtenerValorFilaEcuacion(fila, "Clave");
            return string.IsNullOrWhiteSpace(clave)
                ? ""
                : "CLAVE:" + clave.Trim();
        }

        private void RestaurarContextoEcuacionDespuesDeGuardar(
            string identidad,
            int pestañaInterna,
            int primeraFilaVisible)
        {
            if (dgvEcuacionesProductivas == null ||
                dgvEcuacionesProductivas.IsDisposed)
            {
                return;
            }

            DataGridViewRow filaObjetivo = null;

            if (!string.IsNullOrWhiteSpace(identidad))
            {
                bool buscarPorId = identidad.StartsWith(
                    "ID:",
                    StringComparison.OrdinalIgnoreCase
                );

                string valor = identidad.Substring(
                    buscarPorId ? 3 : 6
                );

                string columna = buscarPorId
                    ? "IdProceso"
                    : "Clave";

                filaObjetivo = dgvEcuacionesProductivas.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(f =>
                        f != null &&
                        !f.IsNewRow &&
                        string.Equals(
                            ObtenerValorFilaEcuacion(f, columna)?.Trim(),
                            valor.Trim(),
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
            }

            if (filaObjetivo == null &&
                dgvEcuacionesProductivas.Rows.Count > 0)
            {
                filaObjetivo = dgvEcuacionesProductivas.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(f => f != null && !f.IsNewRow);
            }

            if (filaObjetivo != null)
            {
                dgvEcuacionesProductivas.ClearSelection();
                filaObjetivo.Selected = true;

                if (filaObjetivo.Cells.Count > 0)
                {
                    dgvEcuacionesProductivas.CurrentCell =
                        filaObjetivo.Cells
                            .Cast<DataGridViewCell>()
                            .FirstOrDefault(c => c.Visible) ??
                        filaObjetivo.Cells[0];
                }

                CargarEditorEcuacionDesdeFilaSeleccionada();
            }

            if (tabsEcuacionesProductivasInternas != null &&
                tabsEcuacionesProductivasInternas.TabPages.Count > 0)
            {
                tabsEcuacionesProductivasInternas.SelectedIndex =
                    Math.Max(
                        0,
                        Math.Min(
                            pestañaInterna,
                            tabsEcuacionesProductivasInternas.TabPages.Count - 1
                        )
                    );
            }

            if (primeraFilaVisible >= 0 &&
                primeraFilaVisible < dgvEcuacionesProductivas.Rows.Count)
            {
                try
                {
                    dgvEcuacionesProductivas.FirstDisplayedScrollingRowIndex =
                        primeraFilaVisible;
                }
                catch
                {
                    // La grilla puede reajustarse por filtros o filas ocultas.
                }
            }

            dgvEcuacionesProductivas.Focus();
            lblEstadoEcuacionesProductivas.Text =
                "Biblioteca guardada. Se mantiene la ecuacion seleccionada.";
        }

'@

$contenido = $contenido.Replace($ancla, $helpers + $ancla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_mantener_seleccion_guardar_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Guardar biblioteca ahora conserva el contexto de trabajo." -ForegroundColor Green
Write-Host "Mantiene ecuacion, pestaña interna y posicion de la lista."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
