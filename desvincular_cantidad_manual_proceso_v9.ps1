$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("ProcesoTieneCantidadManualV9(")) {
    Write-Host "La independencia manual V9 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param(
        [string]$Texto,
        [string]$Firma
    )

    $inicio = $Texto.IndexOf($Firma, [StringComparison]::Ordinal)
    if ($inicio -lt 0) {
        return $null
    }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
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
        Texto = $Texto.Substring(
            $inicio,
            $fin - $inicio + 1
        )
    }
}

# =========================================================
# 1. REEMPLAZAR LA ACTUALIZACION DE CANTIDAD DEL PROCESO
# La cantidad ingresada es el TOTAL de la fila, no el valor
# que debe copiarse en cada requerimiento interno.
# =========================================================
$firmaCantidadProceso =
    "        private void ActualizarCantidadSnapshotProcesoProyecto("

$rangoCantidad =
    Obtener-RangoMetodo $contenido $firmaCantidadProceso

if ($null -eq $rangoCantidad) {
    throw "No se encontro ActualizarCantidadSnapshotProcesoProyecto."
}

$nuevoMetodoCantidad = @'
        private void ActualizarCantidadSnapshotProcesoProyecto(
            ItemProyecto item,
            ProcesoProyecto proceso,
            decimal cantidad)
        {
            if (item == null ||
                proceso == null ||
                cantidad < 0m)
            {
                return;
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                return;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r =>
                        r != null &&
                        CoincideRequerimientoConProceso(
                            r,
                            proceso,
                            null
                        ))
                    .ToList();

            if (requerimientos.Count == 0)
            {
                return;
            }

            double totalAnterior =
                requerimientos.Sum(r =>
                    Math.Max(0.0, r.Cantidad));

            double cantidadTotalNueva =
                Convert.ToDouble(cantidad);

            double factorTotal =
                totalAnterior > 0.0
                    ? cantidadTotalNueva / totalAnterior
                    : 1.0;

            double acumulado = 0.0;

            for (int i = 0;
                i < requerimientos.Count;
                i++)
            {
                RequerimientoProduccionInterna req =
                    requerimientos[i];

                double cantidadAnteriorReq =
                    Math.Max(0.0, req.Cantidad);

                double cantidadNuevaReq;

                if (i == requerimientos.Count - 1)
                {
                    // El ultimo absorbe cualquier diferencia de redondeo
                    // para que la suma sea exactamente el valor ingresado.
                    cantidadNuevaReq =
                        Math.Max(
                            0.0,
                            cantidadTotalNueva - acumulado
                        );
                }
                else if (totalAnterior > 0.0)
                {
                    cantidadNuevaReq =
                        cantidadTotalNueva *
                        cantidadAnteriorReq /
                        totalAnterior;
                }
                else
                {
                    cantidadNuevaReq =
                        cantidadTotalNueva /
                        requerimientos.Count;
                }

                double factorReq =
                    cantidadAnteriorReq > 0.0
                        ? cantidadNuevaReq /
                          cantidadAnteriorReq
                        : factorTotal;

                req.Cantidad = cantidadNuevaReq;

                EscalarRequerimientoLinealV6(
                    req,
                    factorReq
                );

                // Esta cantidad fue cambiada expresamente por el usuario.
                // Desde ahora no debe volver a heredarse del producto padre.
                req.EditadoManualmente = true;
                req.TieneOverrideLocalCalculo = true;
                req.OrigenHoras =
                    "Cantidad del proceso editada manualmente";

                acumulado += cantidadNuevaReq;
            }

            EscalarResultadoProcesoLinealV6(
                proceso,
                Convert.ToDecimal(factorTotal)
            );

            GuardarSnapshotCotizacionItemProyecto(
                item,
                snapshot
            );
        }
'@

$contenido =
    $contenido.Substring(0, $rangoCantidad.Inicio) +
    $nuevoMetodoCantidad +
    $contenido.Substring($rangoCantidad.Fin + 1)

# =========================================================
# 2. HACER QUE LA SINCRONIZACION DEL PADRE RESPETE PROCESOS
# CUYA CANTIDAD YA FUE EDITADA MANUALMENTE.
# =========================================================
$firmaSincronizarProceso =
    "        private void SincronizarCantidadProcesoV4("

$rangoSincronizar =
    Obtener-RangoMetodo $contenido $firmaSincronizarProceso

if ($null -eq $rangoSincronizar) {
    throw "No se encontro SincronizarCantidadProcesoV4."
}

$metodoSincronizar = $rangoSincronizar.Texto

$patronCondicion =
    '(?ms)if\s*\(\s*proceso\s*==\s*null\s*\|\|'

if (-not [regex]::IsMatch(
    $metodoSincronizar,
    $patronCondicion
)) {
    throw "No se encontro la condicion inicial de SincronizarCantidadProcesoV4."
}

$metodoSincronizar =
    [regex]::Replace(
        $metodoSincronizar,
        $patronCondicion,
        @'
if (proceso == null ||
                ProcesoTieneCantidadManualV9(
                    item,
                    proceso
                ) ||
'@,
        1
    )

$contenido =
    $contenido.Substring(0, $rangoSincronizar.Inicio) +
    $metodoSincronizar +
    $contenido.Substring($rangoSincronizar.Fin + 1)

# =========================================================
# 3. HACER QUE LA SINCRONIZACION GLOBAL DEL SNAPSHOT OMITA
# REQUERIMIENTOS CON OVERRIDE MANUAL.
# =========================================================
$firmaSnapshot =
    "        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5("

$rangoSnapshot =
    Obtener-RangoMetodo $contenido $firmaSnapshot

if ($null -eq $rangoSnapshot) {
    throw "No se encontro SincronizarRequerimientosSnapshotConCantidadMaestraV5."
}

$metodoSnapshot = $rangoSnapshot.Texto

$patronIf =
    'if\s*\(\s*esManual\s*\|\|\s*!usaSegundos\s*\)'

if (-not [regex]::IsMatch(
    $metodoSnapshot,
    $patronIf
)) {
    throw "No se encontro el filtro de requerimientos del snapshot."
}

$metodoSnapshot =
    [regex]::Replace(
        $metodoSnapshot,
        $patronIf,
        @'
if (esManual ||
                    !usaSegundos ||
                    (requerimiento.EditadoManualmente &&
                     requerimiento.TieneOverrideLocalCalculo))
'@,
        1
    )

$contenido =
    $contenido.Substring(0, $rangoSnapshot.Inicio) +
    $metodoSnapshot +
    $contenido.Substring($rangoSnapshot.Fin + 1)

# =========================================================
# 4. INSERTAR HELPER QUE REVISA EL OVERRIDE DEL PROCESO.
# =========================================================
$ancla =
    "        private void SincronizarCantidadProcesoV4("

$indiceAncla =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indiceAncla -lt 0) {
    throw "No se encontro el punto para insertar el helper V9."
}

$helper = @'
        private bool ProcesoTieneCantidadManualV9(
            ItemProyecto item,
            ProcesoProyecto proceso)
        {
            if (item == null || proceso == null)
            {
                return false;
            }

            string ruta =
                "proceso/" + (proceso.Id ?? "");

            bool tieneOverrideRegistrado =
                (item.Overrides ??
                    new List<OverrideProductivo>())
                .Any(o =>
                    o != null &&
                    string.Equals(
                        o.Campo,
                        "Cantidad",
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    string.Equals(
                        o.RutaElemento,
                        ruta,
                        StringComparison.OrdinalIgnoreCase
                    ));

            if (tieneOverrideRegistrado)
            {
                return true;
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                return false;
            }

            return snapshot.DesgloseProductivo.Requerimientos
                .Where(r =>
                    r != null &&
                    CoincideRequerimientoConProceso(
                        r,
                        proceso,
                        null
                    ))
                .Any(r =>
                    r.EditadoManualmente &&
                    r.TieneOverrideLocalCalculo);
        }

'@

$contenido =
    $contenido.Substring(0, $indiceAncla) +
    $helper +
    $contenido.Substring($indiceAncla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_independencia_cantidad_v9_" +
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
Write-Host "Independencia manual V9 aplicada." -ForegroundColor Green
Write-Host "La cantidad de una fila de proceso ahora es un total, no un valor por requerimiento."
Write-Host "Las ediciones manuales ya no seran pisadas por la cantidad del producto padre."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
