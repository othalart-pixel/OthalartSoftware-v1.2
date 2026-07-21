$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("ReconstruirSnapshotFaltanteV25")) {
    Write-Host "El hotfix V25 ya esta aplicado." -ForegroundColor Yellow
    exit 0
}

$patron = @'
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
'@

$reemplazo = @'
                        Cotizacion snapshot =
                            CargarSnapshotCotizacionItemProyecto(item);

                        if (snapshot == null)
                        {
                            snapshot =
                                ReconstruirSnapshotFaltanteV25(
                                    item
                                );
                        }

                        if (snapshot == null)
                        {
                            errores.Add(
                                (item.Nombre ?? "Producto sin nombre") +
                                ": no se pudo cargar ni reconstruir el snapshot."
                            );
                            continue;
                        }
'@

if (-not $contenido.Contains($patron)) {
    throw "No se encontro el bloque de snapshot de V24."
}

$contenido = $contenido.Replace(
    $patron,
    $reemplazo
)

$ancla =
    "        private void AplicarCantidadActualAlBriefV24("

$indice =
    $contenido.IndexOf(
        $ancla,
        [StringComparison]::Ordinal
    )

if ($indice -lt 0) {
    throw "No se encontro AplicarCantidadActualAlBriefV24."
}

$helper = @'
        // ReconstruirSnapshotFaltanteV25
        private Cotizacion ReconstruirSnapshotFaltanteV25(
            ItemProyecto item)
        {
            if (item == null ||
                cotizacion == null)
            {
                return null;
            }

            try
            {
                JsonSerializerOptions opciones =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        IncludeFields = true
                    };

                string json =
                    JsonSerializer.Serialize(
                        cotizacion,
                        opciones
                    );

                Cotizacion copia =
                    JsonSerializer.Deserialize<Cotizacion>(
                        json,
                        opciones
                    );

                if (copia == null)
                {
                    return null;
                }

                copia.NombreProyecto =
                    string.IsNullOrWhiteSpace(item.Nombre)
                        ? copia.NombreProyecto
                        : item.Nombre;

                if (copia.BriefProducto == null)
                {
                    copia.BriefProducto =
                        new BriefProducto();
                }

                if (copia.BriefProducto
                        .EntregablesSeleccionados == null)
                {
                    copia.BriefProducto
                        .EntregablesSeleccionados =
                            new List<EntregableBrief>();
                }

                // Si la cotización global no contiene entregables,
                // se intenta reconstruir uno mínimo a partir del item.
                if (copia.BriefProducto
                        .EntregablesSeleccionados.Count == 0)
                {
                    copia.BriefProducto
                        .EntregablesSeleccionados.Add(
                            new EntregableBrief
                            {
                                Nombre =
                                    item.Nombre ??
                                    "Producto del proyecto",

                                Categoria =
                                    item.Tipo.ToString(),

                                Cantidad = 1,

                                DuracionPorUnidad =
                                    Convert.ToDouble(
                                        Math.Max(
                                            0m,
                                            item.Cantidad
                                        )
                                    ),

                                UnidadDuracion =
                                    NormalizarUnidadCantidadProyectoV4(
                                        ObtenerUnidadVisibleItem(item)
                                    ) == "segundo"
                                        ? "segundos"
                                        : ObtenerUnidadVisibleItem(item),

                                UnidadCantidad =
                                    ObtenerUnidadVisibleItem(item)
                            }
                        );
                }

                AplicarCantidadActualAlBriefV24(
                    item,
                    copia
                );

                copia.DesgloseProductivo =
                    DesgloseProductivoService.Generar(
                        copia
                    );

                GuardarSnapshotCotizacionItemProyecto(
                    item,
                    copia
                );

                return copia;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[V25] No se pudo reconstruir snapshot: " +
                    ex
                );

                return null;
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
    "$ruta.backup_snapshot_v25_" +
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
Write-Host "HOTFIX V25 APLICADO" -ForegroundColor Green
Write-Host ""
Write-Host "Si un producto no tiene snapshot, ahora se reconstruye"
Write-Host "desde la cotizacion global y se guarda uno nuevo."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
