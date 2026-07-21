$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("private bool PropagarCantidadMaestraProductoProyecto(")) {
    Write-Host "La propagacion maestra ya parece estar aplicada." -ForegroundColor Yellow
    exit 0
}

# ---------------------------------------------------------
# 1. Localizar el metodo AplicarEdicionTablaProductos
# ---------------------------------------------------------
$firma = "private void AplicarEdicionTablaProductos("
$inicioMetodo = $contenido.IndexOf($firma, [StringComparison]::Ordinal)

if ($inicioMetodo -lt 0) {
    throw "No se encontro el metodo AplicarEdicionTablaProductos."
}

$llaveInicial = $contenido.IndexOf("{", $inicioMetodo, [StringComparison]::Ordinal)
if ($llaveInicial -lt 0) {
    throw "No se encontro la llave inicial de AplicarEdicionTablaProductos."
}

$profundidad = 0
$finMetodo = -1

for ($i = $llaveInicial; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') {
        $profundidad++
    }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--

        if ($profundidad -eq 0) {
            $finMetodo = $i
            break
        }
    }
}

if ($finMetodo -lt 0) {
    throw "No se encontro el final de AplicarEdicionTablaProductos."
}

$metodo = $contenido.Substring(
    $inicioMetodo,
    $finMetodo - $inicioMetodo + 1
)

# ---------------------------------------------------------
# 2. Insertar preparacion antes de activar aplicandoEdicion
# ---------------------------------------------------------
$patronPreparar = '(?m)^(?<indent>\s*)aplicandoEdicionTablaProyecto\s*=\s*true\s*;'

$coincidenciaPreparar = [regex]::Match($metodo, $patronPreparar)

if (-not $coincidenciaPreparar.Success) {
    throw "No se encontro aplicandoEdicionTablaProyecto = true dentro del metodo."
}

$indent = $coincidenciaPreparar.Groups["indent"].Value

$bloquePreparar = @"
${indent}decimal cantidadAnteriorMaestra = 0m;
${indent}decimal cantidadNuevaMaestra = 0m;
${indent}bool esCambioCantidadMaestra =
${indent}    nodo is ItemProyecto itemCantidadMaestra &&
${indent}    col == "Cantidad" &&
${indent}    TryParseDecimalProyecto(valor.Trim(), out cantidadNuevaMaestra) &&
${indent}    cantidadNuevaMaestra >= 0m &&
${indent}    EsUnidadEscalablePorCantidadMaestra(itemCantidadMaestra.Unidad);

${indent}if (esCambioCantidadMaestra)
${indent}{
${indent}    cantidadAnteriorMaestra = itemCantidadMaestra.Cantidad;

${indent}    if (cantidadAnteriorMaestra != cantidadNuevaMaestra &&
${indent}        !ConfirmarCambioCantidadMaestraProducto(
${indent}            itemCantidadMaestra,
${indent}            cantidadAnteriorMaestra,
${indent}            cantidadNuevaMaestra))
${indent}    {
${indent}        row.Cells[columnIndex].Value =
${indent}            cantidadAnteriorMaestra.ToString("0.##");
${indent}        return;
${indent}    }
${indent}}

${indent}aplicandoEdicionTablaProyecto = true;
"@

$metodo = [regex]::Replace(
    $metodo,
    $patronPreparar,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return $bloquePreparar
    },
    1
)

# ---------------------------------------------------------
# 3. Insertar propagacion antes de MarcarNodoProyectoModificado
# ---------------------------------------------------------
$patronCambio = '(?m)^(?<indent>\s*)MarcarNodoProyectoModificado\s*\(\s*nodo\s*,\s*col\s*,\s*valor\.Trim\(\)\s*\)\s*;'
$coincidenciaCambio = [regex]::Match($metodo, $patronCambio)

if (-not $coincidenciaCambio.Success) {
    throw "No se encontro MarcarNodoProyectoModificado dentro del metodo."
}

$indentCambio = $coincidenciaCambio.Groups["indent"].Value

$bloqueCambio = @"
${indentCambio}if (esCambioCantidadMaestra &&
${indentCambio}    nodo is ItemProyecto itemMaestro &&
${indentCambio}    cantidadAnteriorMaestra != cantidadNuevaMaestra)
${indentCambio}{
${indentCambio}    PropagarCantidadMaestraProductoProyecto(
${indentCambio}        itemMaestro,
${indentCambio}        cantidadAnteriorMaestra,
${indentCambio}        cantidadNuevaMaestra
${indentCambio}    );
${indentCambio}}

${indentCambio}MarcarNodoProyectoModificado(nodo, col, valor.Trim());
"@

$metodo = [regex]::Replace(
    $metodo,
    $patronCambio,
    [System.Text.RegularExpressions.MatchEvaluator]{
        param($m)
        return $bloqueCambio
    },
    1
)

$contenido =
    $contenido.Substring(0, $inicioMetodo) +
    $metodo +
    $contenido.Substring($finMetodo + 1)

# ---------------------------------------------------------
# 4. Insertar helpers antes de EsColumnaEditable...
# ---------------------------------------------------------
$ancla = "        private bool EsColumnaEditableTablaProductosProyecto(string columna)"
$indiceAncla = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indiceAncla -lt 0) {
    throw "No se encontro EsColumnaEditableTablaProductosProyecto para insertar helpers."
}

$helpers = @'
        private bool ConfirmarCambioCantidadMaestraProducto(
            ItemProyecto item,
            decimal cantidadAnterior,
            decimal cantidadNueva)
        {
            int subproductosCompatibles =
                (item.Subproductos ?? new List<SubproductoProyecto>())
                .Count(s =>
                    s != null &&
                    EsUnidadCompatibleCantidadMaestra(item.Unidad, s.Unidad));

            int procesosCompatibles =
                ObtenerProcesosDescendientesCantidadMaestra(item)
                .Count(p =>
                    p != null &&
                    p.MetodoCalculo != MetodoCalculoProceso.Manual &&
                    EsUnidadCompatibleCantidadMaestra(item.Unidad, p.Unidad));

            return MessageBox.Show(
                this,
                "Cambiar la cantidad maestra de " +
                cantidadAnterior.ToString("0.##") + " a " +
                cantidadNueva.ToString("0.##") + " " +
                (item.Unidad ?? "") + " actualizará:" +
                Environment.NewLine + Environment.NewLine +
                "• " + subproductosCompatibles +
                " subproductos con unidad compatible" +
                Environment.NewLine +
                "• " + procesosCompatibles +
                " procesos calculados" +
                Environment.NewLine +
                "• los requerimientos productivos del snapshot" +
                Environment.NewLine + Environment.NewLine +
                "Las horas declaradas manualmente se conservarán.",
                "Escalar producto completo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            ) == DialogResult.OK;
        }

        private bool PropagarCantidadMaestraProductoProyecto(
            ItemProyecto item,
            decimal cantidadAnterior,
            decimal cantidadNueva)
        {
            if (item == null ||
                cantidadAnterior <= 0m ||
                cantidadNueva < 0m ||
                !EsUnidadEscalablePorCantidadMaestra(item.Unidad))
            {
                return false;
            }

            decimal factor = cantidadNueva / cantidadAnterior;
            int modificados = 0;

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ?? new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                if (EsUnidadCompatibleCantidadMaestra(
                    item.Unidad,
                    subproducto.Unidad))
                {
                    subproducto.Cantidad =
                        Math.Round(subproducto.Cantidad * factor, 4);
                    modificados++;
                }

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ?? new List<ProcesoProyecto>())
                {
                    if (EscalarProcesoPorCantidadMaestra(
                        proceso,
                        item.Unidad,
                        factor))
                    {
                        modificados++;
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
                        if (EscalarProcesoPorCantidadMaestra(
                            proceso,
                            item.Unidad,
                            factor))
                        {
                            modificados++;
                        }
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ?? new List<ProcesoProyecto>())
            {
                if (EscalarProcesoPorCantidadMaestra(
                    proceso,
                    item.Unidad,
                    factor))
                {
                    modificados++;
                }
            }

            Cotizacion snapshot = CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos != null)
            {
                foreach (RequerimientoProduccionInterna requerimiento in
                    snapshot.DesgloseProductivo.Requerimientos
                        .Where(r => r != null))
                {
                    if (!EsUnidadCompatibleCantidadMaestra(
                            item.Unidad,
                            requerimiento.Unidad) ||
                        EsRequerimientoHorasDeclaradasManualmente(
                            requerimiento))
                    {
                        continue;
                    }

                    requerimiento.Cantidad =
                        Math.Round(
                            requerimiento.Cantidad *
                            Convert.ToDouble(factor),
                            6
                        );

                    modificados++;
                }

                GuardarSnapshotCotizacionItemProyecto(item, snapshot);
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            lblEstadoProyecto.Text =
                "Cantidad maestra actualizada: " +
                modificados +
                " elementos compatibles escalados.";

            return true;
        }

        private bool EscalarProcesoPorCantidadMaestra(
            ProcesoProyecto proceso,
            string unidadMaestra,
            decimal factor)
        {
            if (proceso == null ||
                proceso.MetodoCalculo == MetodoCalculoProceso.Manual ||
                !EsUnidadCompatibleCantidadMaestra(
                    unidadMaestra,
                    proceso.Unidad))
            {
                return false;
            }

            proceso.Cantidad =
                Math.Round(proceso.Cantidad * factor, 4);

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
                if (asignacion != null &&
                    asignacion.OrigenHoras ==
                    OrigenHorasProductivas.Calculado)
                {
                    asignacion.HorasCalculadas = 0m;
                    asignacion.CostoCalculado = 0m;
                }
            }

            return true;
        }

        private IEnumerable<ProcesoProyecto>
            ObtenerProcesosDescendientesCantidadMaestra(
                ItemProyecto item)
        {
            if (item == null)
            {
                yield break;
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ?? new List<ProcesoProyecto>())
            {
                if (proceso != null)
                {
                    yield return proceso;
                }
            }

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ?? new List<SubproductoProyecto>())
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

        private bool EsRequerimientoHorasDeclaradasManualmente(
            RequerimientoProduccionInterna requerimiento)
        {
            return requerimiento != null &&
                ModosCalculoProductivo.EsTiempoAsignado(
                    requerimiento.ModoCalculoProductivo
                );
        }

        private bool EsUnidadCompatibleCantidadMaestra(
            string unidadMaestra,
            string unidadHija)
        {
            string maestra =
                NormalizarUnidadCantidadMaestra(unidadMaestra);
            string hija =
                NormalizarUnidadCantidadMaestra(unidadHija);

            return !string.IsNullOrWhiteSpace(maestra) &&
                string.Equals(
                    maestra,
                    hija,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private bool EsUnidadEscalablePorCantidadMaestra(
            string unidad)
        {
            return NormalizarUnidadCantidadMaestra(unidad) ==
                "segundo";
        }

        private string NormalizarUnidadCantidadMaestra(
            string unidad)
        {
            string limpia = (unidad ?? "")
                .Trim()
                .ToLowerInvariant()
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");

            if (limpia == "s" ||
                limpia == "seg" ||
                limpia == "segs" ||
                limpia == "segundo" ||
                limpia == "segundos")
            {
                return "segundo";
            }

            return limpia.EndsWith("s") && limpia.Length > 1
                ? limpia.Substring(0, limpia.Length - 1)
                : limpia;
        }

'@

$contenido =
    $contenido.Substring(0, $indiceAncla) +
    $helpers +
    $contenido.Substring($indiceAncla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_cantidad_maestra_regex_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Cantidad maestra conectada mediante deteccion robusta." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
