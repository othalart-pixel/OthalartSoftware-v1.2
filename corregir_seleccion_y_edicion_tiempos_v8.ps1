$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("RecalcularProyectoDespuesDeEdicionV8(")) {
    Write-Host "La correccion de seleccion V8 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

$firma = "        private void RecalcularProyectoDespuesDeEdicionV7("
$inicio = $contenido.IndexOf($firma, [StringComparison]::Ordinal)

if ($inicio -lt 0) {
    throw "No se encontro RecalcularProyectoDespuesDeEdicionV7. Ejecuta este parche sobre la version que ya tiene V7."
}

$llave = $contenido.IndexOf("{", $inicio, [StringComparison]::Ordinal)
if ($llave -lt 0) {
    throw "No se encontro el inicio del metodo V7."
}

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
    throw "No se encontro el final del metodo V7."
}

$nuevoMetodo = @'
        private void RecalcularProyectoDespuesDeEdicionV8(
            object nodoEditado,
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

                // Guardar la selección que exista al momento real del
                // recálculo. Puede ser distinta del nodo recién editado
                // si el usuario ya hizo clic en otra fila.
                object seleccionActual =
                    nodoProyectoSeleccionado;

                RecalcularProyectoProductivoActual();

                // No volver a imponer el nodo antiguo ni reconstruir
                // su inspector. Si el usuario seleccionó otro elemento,
                // esa selección debe prevalecer.
                if (seleccionActual != null &&
                    !ReferenceEquals(
                        seleccionActual,
                        nodoEditado))
                {
                    nodoProyectoSeleccionado =
                        seleccionActual;

                    itemProyectoSeleccionado =
                        seleccionActual as ItemProyecto ??
                        ObtenerItemContenedor(
                            seleccionActual
                        );

                    MostrarEditorNodoProyecto(
                        seleccionActual
                    );
                }

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

# Cambiar la llamada V7 por V8.
$contenidoNuevo = $contenidoNuevo.Replace(
    "RecalcularProyectoDespuesDeEdicionV7(",
    "RecalcularProyectoDespuesDeEdicionV8("
)

if ($contenidoNuevo -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_seleccion_v8_" +
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
Write-Host "Correccion de seleccion V8 aplicada." -ForegroundColor Green
Write-Host "El recálculo ya no obligará a volver al nodo editado anteriormente."
Write-Host "Ahora podras seleccionar otros procesos y modificar sus horas."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
