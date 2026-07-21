$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("private void SincronizarCantidadProductoYDescendientesV4(")) {
    Write-Host "La sincronizacion V4 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

function Obtener-Metodo {
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
        Texto = $Texto.Substring($inicio, $fin - $inicio + 1)
    }
}

$firma = "        private bool AplicarCambioItemTablaProyecto("
$rango = Obtener-Metodo $contenido $firma

if ($null -eq $rango) {
    throw "No se encontro AplicarCambioItemTablaProyecto."
}

$metodo = $rango.Texto

# Reemplaza dentro del metodo, sin depender de saltos de linea exactos.
$patronAsignacion = '(?m)^(?<indent>\s*)item\.Cantidad\s*=\s*cantidad\s*;\s*$'
$matchAsignacion = [regex]::Match($metodo, $patronAsignacion)

if (-not $matchAsignacion.Success) {
    throw "No se encontro item.Cantidad = cantidad dentro de AplicarCambioItemTablaProyecto."
}

$indent = $matchAsignacion.Groups["indent"].Value

$reemplazoAsignacion = @"
${indent}item.Cantidad = cantidad;
${indent}SincronizarCantidadProductoYDescendientesV4(
${indent}    item,
${indent}    cantidad
${indent});
"@

$metodo = [regex]::Replace(
    $metodo,
    $patronAsignacion,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return $reemplazoAsignacion
    },
    1
)

# Evita ejecutar dos veces la actualizacion simple del snapshot.
$metodo = [regex]::Replace(
    $metodo,
    '(?m)^\s*ActualizarCantidadSnapshotItemProyecto\(\s*item\s*,\s*cantidad\s*\)\s*;\s*$',
    '',
    1
)

$contenido =
    $contenido.Substring(0, $rango.Inicio) +
    $metodo +
    $contenido.Substring($rango.Fin + 1)

# Inserta helpers antes del metodo de subproductos.
$ancla = "        private bool AplicarCambioSubproductoTablaProyecto("
$indiceAncla = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indiceAncla -lt 0) {
    throw "No se encontro AplicarCambioSubproductoTablaProyecto."
}

$helper = @'
        private void SincronizarCantidadProductoYDescendientesV4(
            ItemProyecto item,
            decimal cantidad)
        {
            if (item == null || cantidad < 0m)
            {
                return;
            }

            string unidadProducto =
                NormalizarUnidadCantidadProyectoV4(
                    ObtenerUnidadVisibleItem(item)
                );

            ActualizarCantidadSnapshotItemProyecto(
                item,
                cantidad
            );

            if (unidadProducto != "segundo")
            {
                return;
            }

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ??
                new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                if (NormalizarUnidadCantidadProyectoV4(
                        subproducto.Unidad) == "segundo")
                {
                    subproducto.Cantidad = cantidad;

                    ActualizarCantidadSnapshotSubproductoProyecto(
                        item,
                        subproducto,
                        cantidad
                    );
                }

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ??
                    new List<ProcesoProyecto>())
                {
                    SincronizarCantidadProcesoV4(
                        item,
                        proceso,
                        cantidad
                    );
                }

                foreach (InstanciaSubproducto instancia in
                    subproducto.Instancias ??
                    new List<InstanciaSubproducto>())
                {
                    if (instancia == null)
                    {
                        continue;
                    }

                    foreach (ProcesoProyecto proceso in
                        instancia.Procesos ??
                        new List<ProcesoProyecto>())
                    {
                        SincronizarCantidadProcesoV4(
                            item,
                            proceso,
                            cantidad
                        );
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ??
                new List<ProcesoProyecto>())
            {
                SincronizarCantidadProcesoV4(
                    item,
                    proceso,
                    cantidad
                );
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            lblEstadoProyecto.Text =
                "Cantidad sincronizada inicialmente en todos " +
                "los subproductos y procesos medidos en segundos.";
        }

        private void SincronizarCantidadProcesoV4(
            ItemProyecto item,
            ProcesoProyecto proceso,
            decimal cantidad)
        {
            if (proceso == null ||
                proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual ||
                NormalizarUnidadCantidadProyectoV4(
                    proceso.Unidad) != "segundo")
            {
                return;
            }

            proceso.Cantidad = cantidad;

            ActualizarCantidadSnapshotProcesoProyecto(
                item,
                proceso,
                cantidad
            );
        }

        private string NormalizarUnidadCantidadProyectoV4(
            string unidad)
        {
            string valor = (unidad ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");

            if (valor == "s" ||
                valor == "seg" ||
                valor == "segs" ||
                valor == "segundo" ||
                valor == "segundos")
            {
                return "segundo";
            }

            return valor;
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
    "$ruta.backup_sincronizacion_v4_" +
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
Write-Host "Sincronizacion V4 aplicada correctamente." -ForegroundColor Green
Write-Host "Producto, subproductos y procesos en segundos tomaran la misma cantidad inicial."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
