$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "Services\ProyectoProductivoExpansionService.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro Services\ProyectoProductivoExpansionService.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("CantidadPortadaUnaVezPorProcesoV10")) {
    Write-Host "La correccion V10 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# =========================================================
# 1. SNAPSHOTS:
# Solo la primera fila de cada proceso porta la cantidad.
# Las filas adicionales por cargo mantienen horas y costos,
# pero Cantidad = 0 para no duplicar el trabajo.
# =========================================================
$patronLista = @'
            List<RequerimientoProduccionInterna> requerimientos =
                cotizacionItem?.DesgloseProductivo?.Requerimientos ?? new List<RequerimientoProduccionInterna>();
'@

$reemplazoLista = @'
            List<RequerimientoProduccionInterna> requerimientos =
                cotizacionItem?.DesgloseProductivo?.Requerimientos ?? new List<RequerimientoProduccionInterna>();

            // CantidadPortadaUnaVezPorProcesoV10:
            // un proceso puede generar varias filas por sus cargos,
            // pero la cantidad del trabajo no se suma por cargo.
            HashSet<string> clavesCantidadRegistradas =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );
'@

if (-not $contenido.Contains($patronLista)) {
    throw "No se encontro la declaracion de requerimientos de snapshot."
}

$contenido = $contenido.Replace(
    $patronLista,
    $reemplazoLista
)

$patronProcesoId = @'
                string procesoId = string.IsNullOrWhiteSpace(req.ProcesoId)
                    ? CrearIdSnapshot(item.Id, req.NombreRequerimiento, req.TipoInterno)
                    : req.ProcesoId;
'@

$reemplazoProcesoId = @'
                string procesoId = string.IsNullOrWhiteSpace(req.ProcesoId)
                    ? CrearIdSnapshot(item.Id, req.NombreRequerimiento, req.TipoInterno)
                    : req.ProcesoId;

                string subproductoId =
                    ResolverSubproductoSnapshot(item, req);

                string claveCantidad =
                    (item.Id ?? "") + "|" +
                    (subproductoId ?? "") + "|" +
                    (req.InstanciaId ?? "") + "|" +
                    (procesoId ?? "") + "|" +
                    (req.Unidad ?? "");

                bool portarCantidad =
                    clavesCantidadRegistradas.Add(
                        claveCantidad
                    );
'@

if (-not $contenido.Contains($patronProcesoId)) {
    throw "No se encontro el bloque procesoId del snapshot."
}

$contenido = $contenido.Replace(
    $patronProcesoId,
    $reemplazoProcesoId
)

$contenido = $contenido.Replace(
    "SubproductoProyectoId = ResolverSubproductoSnapshot(item, req),",
    "SubproductoProyectoId = subproductoId,"
)

$contenido = $contenido.Replace(
    "Cantidad = Convert.ToDecimal(req.Cantidad),",
    "Cantidad = portarCantidad ? Convert.ToDecimal(req.Cantidad) : 0m,"
)

# =========================================================
# 2. PROCESOS NORMALES:
# CrearFilasProceso genera una fila por asignacion/cargo.
# Solo la primera debe portar la cantidad del proceso.
# =========================================================
$patronForeach = @'
            foreach (AsignacionProductiva asignacion in asignaciones)
            {
                yield return CrearFilaBase(
                    proyecto,
                    grupo,
                    item,
                    sub,
                    instancia,
                    proceso,
                    asignacion,
                    cantidadProceso,
                    unidadProceso,
                    transversal,
                    ""
                );
            }
'@

$reemplazoForeach = @'
            bool cantidadYaPortada = false;

            foreach (AsignacionProductiva asignacion in asignaciones)
            {
                decimal cantidadFila =
                    cantidadYaPortada
                        ? 0m
                        : cantidadProceso;

                cantidadYaPortada = true;

                yield return CrearFilaBase(
                    proyecto,
                    grupo,
                    item,
                    sub,
                    instancia,
                    proceso,
                    asignacion,
                    cantidadFila,
                    unidadProceso,
                    transversal,
                    ""
                );
            }
'@

if (-not $contenido.Contains($patronForeach)) {
    throw "No se encontro el foreach de asignaciones en CrearFilasProceso."
}

$contenido = $contenido.Replace(
    $patronForeach,
    $reemplazoForeach
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_cantidad_por_proceso_v10_" +
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
Write-Host "Correccion V10 aplicada." -ForegroundColor Green
Write-Host "La cantidad del trabajo ahora se registra una sola vez por proceso."
Write-Host "Los cargos siguen sumando horas y costos de forma independiente."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
