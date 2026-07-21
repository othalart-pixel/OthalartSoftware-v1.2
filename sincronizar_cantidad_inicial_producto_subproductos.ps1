$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("SincronizarCantidadInicialProductoProyecto(")) {
    Write-Host "La sincronizacion inicial ya parece estar aplicada." -ForegroundColor Yellow
    exit 0
}

# Reemplaza solamente las dos lineas estables del bloque Cantidad.
$viejo = @'
                item.Cantidad = cantidad;
                ActualizarCantidadSnapshotItemProyecto(item, cantidad);
                return true;
'@

$nuevo = @'
                item.Cantidad = cantidad;
                SincronizarCantidadInicialProductoProyecto(
                    item,
                    cantidad
                );
                return true;
'@

if (-not $contenido.Contains($viejo)) {
    throw "No se encontro el bloque estable de cantidad del ItemProyecto. No se modifico nada."
}

$contenido = $contenido.Replace($viejo, $nuevo)

# Insertar helper justo antes del metodo siguiente, sin alterar estructuras.
$ancla = "        private bool AplicarCambioSubproductoTablaProyecto("
$indice = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro AplicarCambioSubproductoTablaProyecto para insertar el helper."
}

$helper = @'
        private void SincronizarCantidadInicialProductoProyecto(
            ItemProyecto item,
            decimal cantidad)
        {
            if (item == null || cantidad < 0m)
            {
                return;
            }

            string unidadMaestra =
                NormalizarUnidadSincronizacionProyecto(
                    ObtenerUnidadVisibleItem(item)
                );

            ActualizarCantidadSnapshotItemProyecto(
                item,
                cantidad
            );

            if (unidadMaestra != "segundo")
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

                if (NormalizarUnidadSincronizacionProyecto(
                        subproducto.Unidad) == unidadMaestra)
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
                    SincronizarCantidadProcesoProyecto(
                        item,
                        proceso,
                        unidadMaestra,
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

                    if (instancia.CantidadEquivalente >= 0m)
                    {
                        instancia.CantidadEquivalente = cantidad;
                    }

                    foreach (ProcesoProyecto proceso in
                        instancia.Procesos ??
                        new List<ProcesoProyecto>())
                    {
                        SincronizarCantidadProcesoProyecto(
                            item,
                            proceso,
                            unidadMaestra,
                            cantidad
                        );
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ??
                new List<ProcesoProyecto>())
            {
                SincronizarCantidadProcesoProyecto(
                    item,
                    proceso,
                    unidadMaestra,
                    cantidad
                );
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            lblEstadoProyecto.Text =
                "Cantidad inicial sincronizada en producto, " +
                "subproductos y procesos en segundos.";
        }

        private void SincronizarCantidadProcesoProyecto(
            ItemProyecto item,
            ProcesoProyecto proceso,
            string unidadMaestra,
            decimal cantidad)
        {
            if (proceso == null ||
                proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual ||
                NormalizarUnidadSincronizacionProyecto(
                    proceso.Unidad) != unidadMaestra)
            {
                return;
            }

            proceso.Cantidad = cantidad;

            ActualizarCantidadSnapshotProcesoProyecto(
                item,
                proceso,
                cantidad
            );

            if (proceso.Resultado != null)
            {
                proceso.Resultado.HorasCalculadas = 0m;
                proceso.Resultado.CostoCalculado = 0m;
                proceso.Resultado.DuracionSemanas = 0m;
            }

            foreach (AsignacionProductiva asignacion in
                proceso.Asignaciones ??
                new List<AsignacionProductiva>())
            {
                if (asignacion == null ||
                    asignacion.OrigenHoras !=
                        OrigenHorasProductivas.Calculado)
                {
                    continue;
                }

                asignacion.HorasCalculadas = 0m;
                asignacion.CostoCalculado = 0m;
            }
        }

        private string NormalizarUnidadSincronizacionProyecto(
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
    $contenido.Substring(0, $indice) +
    $helper +
    $contenido.Substring($indice)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_sincronizacion_inicial_" +
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
Write-Host "Sincronizacion inicial aplicada correctamente." -ForegroundColor Green
Write-Host "Producto, subproductos y procesos en segundos tomaran la misma cantidad."
Write-Host "Las horas manuales se conservan."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
