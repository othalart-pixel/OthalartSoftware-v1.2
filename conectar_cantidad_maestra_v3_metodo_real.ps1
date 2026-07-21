$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("private bool AplicarCantidadMaestraProductoV3(")) {
    Write-Host "La cantidad maestra V3 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# =========================================================
# 1. REEMPLAZAR EL BLOQUE REAL DE CANTIDAD DEL ITEM
# Este metodo es usado tanto por la tabla como por el inspector.
# =========================================================
$patron = '(?ms)' +
    'if\s*\(\s*columna\s*==\s*"Cantidad"\s*\)\s*' +
    '\{\s*' +
    'if\s*\(\s*!TryParseDecimalProyecto\(\s*valor\s*,\s*out\s+decimal\s+cantidad\s*\)\s*\|\|\s*cantidad\s*<\s*0m\s*\)\s*' +
    '\{.*?return\s+false\s*;\s*\}\s*' +
    'item\.Cantidad\s*=\s*cantidad\s*;\s*' +
    'ActualizarCantidadSnapshotItemProyecto\(\s*item\s*,\s*cantidad\s*\)\s*;\s*' +
    'return\s+true\s*;\s*' +
    '\}'

$match = [regex]::Match($contenido, $patron)

if (-not $match.Success) {
    throw "No se encontro el bloque real de Cantidad dentro de AplicarCambioItemTablaProyecto."
}

$reemplazo = @'
if (columna == "Cantidad")
            {
                if (!TryParseDecimalProyecto(valor, out decimal cantidad) ||
                    cantidad < 0m)
                {
                    MessageBox.Show(
                        this,
                        "La cantidad debe ser un numero mayor o igual a 0.",
                        "Productos y servicios",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                return AplicarCantidadMaestraProductoV3(
                    item,
                    cantidad
                );
            }
'@

$contenido = [regex]::Replace(
    $contenido,
    $patron,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return $reemplazo
    },
    1
)

# =========================================================
# 2. INSERTAR IMPLEMENTACION CENTRAL
# =========================================================
$ancla = "        private bool AplicarCambioSubproductoTablaProyecto("
$indice = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indice -lt 0) {
    throw "No se encontro AplicarCambioSubproductoTablaProyecto para insertar la funcion."
}

$helper = @'
        private bool AplicarCantidadMaestraProductoV3(
            ItemProyecto item,
            decimal cantidadNueva)
        {
            if (item == null || cantidadNueva < 0m)
            {
                return false;
            }

            decimal cantidadAnterior = item.Cantidad;

            if (cantidadAnterior == cantidadNueva)
            {
                return true;
            }

            string unidadMaestra =
                ObtenerUnidadVisibleItem(item);

            if (!EsUnidadSegundosCantidadMaestraV3(unidadMaestra) ||
                cantidadAnterior <= 0m)
            {
                item.Cantidad = cantidadNueva;
                ActualizarCantidadSnapshotItemProyecto(
                    item,
                    cantidadNueva
                );
                return true;
            }

            decimal factor =
                cantidadNueva / cantidadAnterior;

            int subproductosCompatibles =
                (item.Subproductos ??
                    new List<SubproductoProyecto>())
                .Count(s =>
                    s != null &&
                    UnidadesCompatiblesCantidadMaestraV3(
                        unidadMaestra,
                        s.Unidad
                    ));

            int procesosCompatibles =
                ObtenerProcesosCantidadMaestraV3(item)
                .Count(p =>
                    p != null &&
                    p.MetodoCalculo !=
                        MetodoCalculoProceso.Manual &&
                    UnidadesCompatiblesCantidadMaestraV3(
                        unidadMaestra,
                        p.Unidad
                    ));

            DialogResult confirmacion = MessageBox.Show(
                this,
                "Cambiar la cantidad del producto de " +
                cantidadAnterior.ToString("0.##") +
                " a " +
                cantidadNueva.ToString("0.##") +
                " " +
                unidadMaestra +
                " actualizara:" +
                Environment.NewLine +
                Environment.NewLine +
                "- " +
                subproductosCompatibles +
                " subproductos en segundos" +
                Environment.NewLine +
                "- " +
                procesosCompatibles +
                " procesos calculados" +
                Environment.NewLine +
                "- los requerimientos compatibles del snapshot" +
                Environment.NewLine +
                Environment.NewLine +
                "Las horas declaradas manualmente se conservaran.",
                "Escalar producto completo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (confirmacion != DialogResult.OK)
            {
                return false;
            }

            item.Cantidad = cantidadNueva;

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ??
                new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                if (UnidadesCompatiblesCantidadMaestraV3(
                    unidadMaestra,
                    subproducto.Unidad))
                {
                    subproducto.Cantidad = Math.Round(
                        subproducto.Cantidad * factor,
                        4
                    );
                }

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ??
                    new List<ProcesoProyecto>())
                {
                    EscalarProcesoCantidadMaestraV3(
                        proceso,
                        unidadMaestra,
                        factor
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

                    if (instancia.CantidadEquivalente > 0m)
                    {
                        instancia.CantidadEquivalente =
                            Math.Round(
                                instancia.CantidadEquivalente *
                                factor,
                                4
                            );
                    }

                    foreach (ProcesoProyecto proceso in
                        instancia.Procesos ??
                        new List<ProcesoProyecto>())
                    {
                        EscalarProcesoCantidadMaestraV3(
                            proceso,
                            unidadMaestra,
                            factor
                        );
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ??
                new List<ProcesoProyecto>())
            {
                EscalarProcesoCantidadMaestraV3(
                    proceso,
                    unidadMaestra,
                    factor
                );
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos != null)
            {
                foreach (RequerimientoProduccionInterna requerimiento in
                    snapshot.DesgloseProductivo.Requerimientos
                        .Where(r => r != null))
                {
                    bool esManual =
                        ModosCalculoProductivo.EsTiempoAsignado(
                            requerimiento.ModoCalculoProductivo
                        );

                    if (esManual ||
                        !UnidadesCompatiblesCantidadMaestraV3(
                            unidadMaestra,
                            requerimiento.Unidad))
                    {
                        continue;
                    }

                    requerimiento.Cantidad = Math.Round(
                        requerimiento.Cantidad *
                        Convert.ToDouble(factor),
                        6
                    );
                }

                GuardarSnapshotCotizacionItemProyecto(
                    item,
                    snapshot
                );
            }
            else
            {
                ActualizarCantidadSnapshotItemProyecto(
                    item,
                    cantidadNueva
                );
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            lblEstadoProyecto.Text =
                "Cantidad maestra actualizada. " +
                "Recalculando horas y costos...";

            return true;
        }

        private void EscalarProcesoCantidadMaestraV3(
            ProcesoProyecto proceso,
            string unidadMaestra,
            decimal factor)
        {
            if (proceso == null ||
                proceso.MetodoCalculo ==
                    MetodoCalculoProceso.Manual ||
                !UnidadesCompatiblesCantidadMaestraV3(
                    unidadMaestra,
                    proceso.Unidad))
            {
                return;
            }

            proceso.Cantidad = Math.Round(
                proceso.Cantidad * factor,
                4
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

        private IEnumerable<ProcesoProyecto>
            ObtenerProcesosCantidadMaestraV3(
                ItemProyecto item)
        {
            if (item == null)
            {
                yield break;
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ??
                new List<ProcesoProyecto>())
            {
                if (proceso != null)
                {
                    yield return proceso;
                }
            }

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ??
                new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ??
                    new List<ProcesoProyecto>())
                {
                    if (proceso != null)
                    {
                        yield return proceso;
                    }
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
                        if (proceso != null)
                        {
                            yield return proceso;
                        }
                    }
                }
            }
        }

        private bool EsUnidadSegundosCantidadMaestraV3(
            string unidad)
        {
            return NormalizarUnidadCantidadMaestraV3(
                unidad
            ) == "segundo";
        }

        private bool UnidadesCompatiblesCantidadMaestraV3(
            string unidadA,
            string unidadB)
        {
            string a =
                NormalizarUnidadCantidadMaestraV3(unidadA);
            string b =
                NormalizarUnidadCantidadMaestraV3(unidadB);

            return !string.IsNullOrWhiteSpace(a) &&
                string.Equals(
                    a,
                    b,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private string NormalizarUnidadCantidadMaestraV3(
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

            return valor.EndsWith("s") &&
                valor.Length > 1
                ? valor.Substring(0, valor.Length - 1)
                : valor;
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
    "$ruta.backup_cantidad_maestra_v3_" +
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
Write-Host "Cantidad maestra V3 aplicada correctamente." -ForegroundColor Green
Write-Host "Se conecto en AplicarCambioItemTablaProyecto."
Write-Host "Esto cubre tabla e inspector."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
