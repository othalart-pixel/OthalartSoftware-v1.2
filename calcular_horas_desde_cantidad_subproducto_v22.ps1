$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("CantidadJerarquicaSubproductoV22")) {
    Write-Host "La correccion V22 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param([string]$Texto, [string]$Firma)

    $inicio = $Texto.IndexOf($Firma, [StringComparison]::Ordinal)
    if ($inicio -lt 0) { return $null }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
    if ($llave -lt 0) { return $null }

    $profundidad = 0
    $fin = -1

    for ($i = $llave; $i -lt $Texto.Length; $i++) {
        if ($Texto[$i] -eq '{') { $profundidad++ }
        elseif ($Texto[$i] -eq '}') {
            $profundidad--
            if ($profundidad -eq 0) {
                $fin = $i
                break
            }
        }
    }

    if ($fin -lt 0) { return $null }

    return @{
        Inicio = $inicio
        Fin = $fin
    }
}

$firma = "        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5("
$rango = Obtener-RangoMetodo $contenido $firma

if ($null -eq $rango) {
    throw "No se encontro SincronizarRequerimientosSnapshotConCantidadMaestraV5."
}

$nuevoMetodo = @'
        // CantidadJerarquicaSubproductoV22
        private void SincronizarRequerimientosSnapshotConCantidadMaestraV5(
            ItemProyecto item,
            decimal cantidad)
        {
            if (item == null || cantidad < 0m)
            {
                return;
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos == null)
            {
                ActualizarCantidadSnapshotItemProyecto(item, cantidad);
                return;
            }

            List<RequerimientoProduccionInterna> requerimientos =
                snapshot.DesgloseProductivo.Requerimientos
                    .Where(r => r != null)
                    .ToList();

            double diasHabilesSemana =
                snapshot.DiasHabilesEstudioPorSemana > 0.0
                    ? snapshot.DiasHabilesEstudioPorSemana
                    : 5.0;

            HashSet<RequerimientoProduccionInterna> procesados =
                new HashSet<RequerimientoProduccionInterna>();

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ?? new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                decimal cantidadSubproducto =
                    subproducto.Cantidad >= 0m
                        ? subproducto.Cantidad
                        : cantidad;

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ?? new List<ProcesoProyecto>())
                {
                    RecalcularProcesoConCantidadJerarquicaV22(
                        proceso,
                        cantidadSubproducto,
                        requerimientos,
                        procesados,
                        diasHabilesSemana
                    );
                }

                foreach (InstanciaSubproducto instancia in
                    subproducto.Instancias ?? new List<InstanciaSubproducto>())
                {
                    if (instancia == null)
                    {
                        continue;
                    }

                    foreach (ProcesoProyecto proceso in
                        instancia.Procesos ?? new List<ProcesoProyecto>())
                    {
                        RecalcularProcesoConCantidadJerarquicaV22(
                            proceso,
                            cantidadSubproducto,
                            requerimientos,
                            procesados,
                            diasHabilesSemana
                        );
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ?? new List<ProcesoProyecto>())
            {
                RecalcularProcesoConCantidadJerarquicaV22(
                    proceso,
                    cantidad,
                    requerimientos,
                    procesados,
                    diasHabilesSemana
                );
            }

            foreach (RequerimientoProduccionInterna requerimiento in
                requerimientos.Where(r => !procesados.Contains(r)))
            {
                if (NormalizarUnidadCantidadProyectoV4(
                        requerimiento.Unidad) != "segundo")
                {
                    continue;
                }

                requerimiento.Cantidad =
                    Convert.ToDouble(cantidad);

                PrepararRequerimientoAutomaticoV22(requerimiento);

                RecalcularRequerimientoDesdeEcuacionV16(
                    requerimiento,
                    diasHabilesSemana
                );
            }

            GuardarSnapshotCotizacionItemProyecto(item, snapshot);

            SincronizarResultadosLocalesDesdeSnapshotV18(
                item,
                snapshot
            );
        }

        private void RecalcularProcesoConCantidadJerarquicaV22(
            ProcesoProyecto proceso,
            decimal cantidadReferencia,
            List<RequerimientoProduccionInterna> requerimientos,
            HashSet<RequerimientoProduccionInterna> procesados,
            double diasHabilesSemana)
        {
            if (proceso == null ||
                requerimientos == null ||
                procesados == null ||
                cantidadReferencia < 0m)
            {
                return;
            }

            if (NormalizarUnidadCantidadProyectoV4(
                    proceso.Unidad) == "segundo")
            {
                proceso.Cantidad = cantidadReferencia;
            }

            List<RequerimientoProduccionInterna> relacionados =
                requerimientos
                    .Where(r =>
                        r != null &&
                        CoincideRequerimientoConProceso(
                            r,
                            proceso,
                            null
                        ))
                    .ToList();

            foreach (RequerimientoProduccionInterna requerimiento in relacionados)
            {
                if (NormalizarUnidadCantidadProyectoV4(
                        requerimiento.Unidad) != "segundo")
                {
                    continue;
                }

                requerimiento.Cantidad =
                    Convert.ToDouble(cantidadReferencia);

                PrepararRequerimientoAutomaticoV22(requerimiento);

                RecalcularRequerimientoDesdeEcuacionV16(
                    requerimiento,
                    diasHabilesSemana
                );

                procesados.Add(requerimiento);
            }
        }

        private void PrepararRequerimientoAutomaticoV22(
            RequerimientoProduccionInterna requerimiento)
        {
            if (requerimiento == null)
            {
                return;
            }

            List<EcuacionProductivaDefinicion> biblioteca =
                BibliotecaEcuacionesProductivasJsonService
                    .CargarEcuaciones();

            EcuacionProductivaDefinicion ecuacion =
                ResolverEcuacionVigenteV21(
                    requerimiento,
                    biblioteca
                );

            bool esManual =
                ecuacion != null &&
                ecuacion.MetodoCalculo ==
                    MetodoCalculoProceso.Manual;

            if (esManual)
            {
                return;
            }

            requerimiento.EditadoManualmente = false;
            requerimiento.TieneOverrideLocalCalculo = false;
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
    "$ruta.backup_cantidad_subproducto_v22_" +
    (Get-Date -Format "yyyyMMdd_HHmmss")

Copy-Item $ruta $backup -Force

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Correccion V22 aplicada." -ForegroundColor Green
Write-Host "Cada proceso ahora toma la cantidad de su subproducto padre."
Write-Host "Los procesos directos del producto usan la cantidad global."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
