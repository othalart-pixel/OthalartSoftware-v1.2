$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("LimpiarOverridesHorasAutomaticasV20(")) {
    Write-Host "La correccion V20 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# 1. Limpiar residuos manuales antes de sincronizar cantidad.
$ancla = @'
            string unidadProducto =
                NormalizarUnidadCantidadProyectoV4(
                    ObtenerUnidadVisibleItem(item)
                );
'@

$reemplazo = @'
            // Al cambiar la cantidad maestra, los procesos calculados por
            // capacidad deben volver a depender de sus ecuaciones.
            // Se eliminan únicamente overrides de horas automáticas.
            LimpiarOverridesHorasAutomaticasV20(item);

            string unidadProducto =
                NormalizarUnidadCantidadProyectoV4(
                    ObtenerUnidadVisibleItem(item)
                );
'@

if (-not $contenido.Contains($ancla)) {
    throw "No se encontro el inicio estable de SincronizarCantidadProductoYDescendientesV4."
}

$contenido = $contenido.Replace($ancla, $reemplazo)

# 2. V19 no debe excluir asignaciones antiguamente marcadas Manual
# cuando el PROCESO completo es automático.
$bloqueFiltro = @'
            List<AsignacionProductiva> automaticas =
                (proceso.Asignaciones ??
                    new List<AsignacionProductiva>())
                .Where(a =>
                    a != null &&
                    a.OrigenHoras !=
                        OrigenHorasProductivas.Manual)
                .ToList();

            if (automaticas.Count == 0)
            {
                return;
            }
'@

$bloqueFiltroNuevo = @'
            // La naturaleza manual se decide por el método del proceso,
            // no por residuos antiguos guardados en cada asignación.
            if (proceso.MetodoCalculo ==
                MetodoCalculoProceso.Manual)
            {
                return;
            }

            List<AsignacionProductiva> automaticas =
                (proceso.Asignaciones ??
                    new List<AsignacionProductiva>())
                .Where(a => a != null)
                .ToList();

            if (automaticas.Count == 0)
            {
                return;
            }
'@

if (-not $contenido.Contains($bloqueFiltro)) {
    throw "No se encontro el filtro de asignaciones de V19."
}

$contenido = $contenido.Replace(
    $bloqueFiltro,
    $bloqueFiltroNuevo
)

# 3. Quitar cualquier referencia restante al enum inexistente.
$contenido = $contenido.Replace(
@'
                asignacion.OrigenHoras =
                    OrigenHorasProductivas.Calculada;
'@,
@'
                // Se conserva el valor del enum existente; horas y costo
                // ya fueron reemplazados por el cálculo automático.
'@
)

# 4. Insertar helper antes de SincronizarCantidadProductoYDescendientesV4.
$anclaHelper =
    "        private void SincronizarCantidadProductoYDescendientesV4("

$indice = $contenido.IndexOf(
    $anclaHelper,
    [StringComparison]::Ordinal
)

if ($indice -lt 0) {
    throw "No se encontro SincronizarCantidadProductoYDescendientesV4."
}

$helper = @'
        private void LimpiarOverridesHorasAutomaticasV20(
            ItemProyecto item)
        {
            if (item == null)
            {
                return;
            }

            List<ProcesoProyecto> procesos =
                ObtenerProcesosItemProyecto(item)
                    .Where(p => p != null)
                    .ToList();

            HashSet<string> rutasProcesosManuales =
                new HashSet<string>(
                    procesos
                        .Where(p =>
                            p.MetodoCalculo ==
                                MetodoCalculoProceso.Manual)
                        .Select(p =>
                            "proceso/" + (p.Id ?? "")),
                    StringComparer.OrdinalIgnoreCase
                );

            // Las horas del item y subproductos son agregados derivados.
            // Los overrides de horas solo se conservan cuando pertenecen
            // exactamente a un proceso cuya ecuación es manual.
            if (item.Overrides != null)
            {
                item.Overrides.RemoveAll(o =>
                    o != null &&
                    string.Equals(
                        o.Campo,
                        "Horas",
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !rutasProcesosManuales.Contains(
                        o.RutaElemento ?? ""
                    ));
            }

            foreach (ProcesoProyecto proceso in procesos)
            {
                if (proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual)
                {
                    continue;
                }

                proceso.Resultado =
                    proceso.Resultado ??
                    new ResultadoProcesoProyecto();

                proceso.Resultado.HorasAsignadas = 0m;

                foreach (AsignacionProductiva asignacion in
                    (proceso.Asignaciones ??
                        new List<AsignacionProductiva>())
                    .Where(a => a != null))
                {
                    // Borra la imposición manual anterior. V19 reemplazará
                    // HorasCalculadas y CostoCalculado con el runtime.
                    asignacion.HorasAsignadas = 0m;
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
    "$ruta.backup_reconectar_horas_v20_" +
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
Write-Host "Correccion V20 aplicada." -ForegroundColor Green
Write-Host "Se limpiaron overrides de horas en procesos automaticos."
Write-Host "Las ecuaciones manuales se mantienen protegidas."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
