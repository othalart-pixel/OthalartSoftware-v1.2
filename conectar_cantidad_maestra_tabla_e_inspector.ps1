$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("private bool AplicarCantidadMaestraItemProyecto(")) {
    Write-Host "La cantidad maestra ya parece estar conectada." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param(
        [string]$Texto,
        [int]$PosicionInterna
    )

    $inicio = $Texto.LastIndexOf("        private ", $PosicionInterna, [StringComparison]::Ordinal)
    if ($inicio -lt 0) {
        $inicio = $Texto.LastIndexOf("        protected ", $PosicionInterna, [StringComparison]::Ordinal)
    }

    if ($inicio -lt 0) {
        return $null
    }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
    if ($llave -lt 0 -or $llave -gt $PosicionInterna) {
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

function Reemplazar-AsignacionCantidadItem {
    param(
        [string]$Metodo,
        [string]$Descripcion
    )

    $patrones = @(
        '(?m)^(?<indent>\s*)item\.Cantidad\s*=\s*(?<valor>[^;]+);',
        '(?m)^(?<indent>\s*)itemProyecto\.Cantidad\s*=\s*(?<valor>[^;]+);',
        '(?m)^(?<indent>\s*)producto\.Cantidad\s*=\s*(?<valor>[^;]+);'
    )

    foreach ($patron in $patrones) {
        $match = [regex]::Match($Metodo, $patron)
        if ($match.Success) {
            $variable = if ($match.Value -match 'itemProyecto\.Cantidad') {
                "itemProyecto"
            }
            elseif ($match.Value -match 'producto\.Cantidad') {
                "producto"
            }
            else {
                "item"
            }

            $indent = $match.Groups["indent"].Value
            $valor = $match.Groups["valor"].Value.Trim()

            $reemplazo = @"
${indent}AplicarCantidadMaestraItemProyecto(
${indent}    $variable,
${indent}    Convert.ToDecimal($valor)
${indent});
"@

            return [regex]::Replace(
                $Metodo,
                $patron,
                [System.Text.RegularExpressions.MatchEvaluator]{
                    param($m)
                    return $reemplazo
                },
                1
            )
        }
    }

    throw "No se encontro una asignacion de Cantidad del ItemProyecto en $Descripcion."
}

# =========================================================
# 1. CONECTAR LA EDICION DESDE LA TABLA
# =========================================================
$posTabla = $contenido.IndexOf(
    "private bool AplicarCambioNodoTablaProductosProyecto",
    [StringComparison]::Ordinal
)

if ($posTabla -lt 0) {
    $posTabla = $contenido.IndexOf(
        "private void AplicarCambioNodoTablaProductosProyecto",
        [StringComparison]::Ordinal
    )
}

if ($posTabla -lt 0) {
    throw "No se encontro AplicarCambioNodoTablaProductosProyecto."
}

$rangoTabla = Obtener-RangoMetodo $contenido $posTabla
if ($null -eq $rangoTabla) {
    throw "No se pudo leer AplicarCambioNodoTablaProductosProyecto."
}

$metodoTablaNuevo = Reemplazar-AsignacionCantidadItem `
    $rangoTabla.Texto `
    "AplicarCambioNodoTablaProductosProyecto"

$contenido =
    $contenido.Substring(0, $rangoTabla.Inicio) +
    $metodoTablaNuevo +
    $contenido.Substring($rangoTabla.Fin + 1)

# =========================================================
# 2. CONECTAR EL BOTON APLICAR DEL INSPECTOR
# Busca el metodo real que construye el campo Cantidad.
# =========================================================
$matchesCantidad = [regex]::Matches(
    $contenido,
    '"Cantidad:"|"Cantidad"'
)

$inspectorConectado = $false

foreach ($matchCantidad in $matchesCantidad) {
    $rango = Obtener-RangoMetodo $contenido $matchCantidad.Index

    if ($null -eq $rango) {
        continue
    }

    $metodo = $rango.Texto

    if ($metodo.Contains("NumericUpDown") -and
        $metodo.Contains("Aplicar") -and
        ($metodo.Contains("ItemProyecto") -or
         $metodo.Contains("item.Cantidad") -or
         $metodo.Contains("itemProyecto.Cantidad"))) {

        try {
            $metodoNuevo = Reemplazar-AsignacionCantidadItem `
                $metodo `
                "el editor de Cantidad del inspector"

            $contenido =
                $contenido.Substring(0, $rango.Inicio) +
                $metodoNuevo +
                $contenido.Substring($rango.Fin + 1)

            $inspectorConectado = $true
            break
        }
        catch {
            continue
        }
    }
}

if (-not $inspectorConectado) {
    throw "No se encontro el handler real del boton Aplicar para Cantidad en el inspector."
}

# =========================================================
# 3. INSERTAR LA FUNCION CENTRAL
# =========================================================
$ancla = "        private bool EsColumnaEditableTablaProductosProyecto(string columna)"
$indiceAncla = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)

if ($indiceAncla -lt 0) {
    $ancla = "        private void ConfigurarColumnasEditablesTablaProductosProyecto()"
    $indiceAncla = $contenido.IndexOf($ancla, [StringComparison]::Ordinal)
}

if ($indiceAncla -lt 0) {
    throw "No se encontro un punto seguro para insertar la funcion central."
}

$helpers = @'
        private bool AplicarCantidadMaestraItemProyecto(
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
                item.Cantidad = cantidadNueva;
                return true;
            }

            bool unidadEscalable =
                EsUnidadEscalableCantidadMaestraProyecto(
                    ObtenerUnidadVisibleItem(item)
                );

            if (!unidadEscalable || cantidadAnterior <= 0m)
            {
                item.Cantidad = cantidadNueva;
                return true;
            }

            decimal factor = cantidadNueva / cantidadAnterior;

            int subproductos = (item.Subproductos ??
                new List<SubproductoProyecto>())
                .Count(s =>
                    s != null &&
                    EsUnidadCompatibleCantidadMaestraProyecto(
                        ObtenerUnidadVisibleItem(item),
                        s.Unidad
                    ));

            int procesos = ObtenerProcesosCantidadMaestraProyecto(item)
                .Count(p =>
                    p != null &&
                    p.MetodoCalculo != MetodoCalculoProceso.Manual &&
                    EsUnidadCompatibleCantidadMaestraProyecto(
                        ObtenerUnidadVisibleItem(item),
                        p.Unidad
                    ));

            DialogResult confirmar = MessageBox.Show(
                this,
                "Cambiar la cantidad del producto de " +
                cantidadAnterior.ToString("0.##") +
                " a " +
                cantidadNueva.ToString("0.##") +
                " " +
                ObtenerUnidadVisibleItem(item) +
                " escalará:" +
                Environment.NewLine +
                Environment.NewLine +
                "• " + subproductos + " subproductos" +
                Environment.NewLine +
                "• " + procesos + " procesos calculados" +
                Environment.NewLine +
                "• los requerimientos productivos compatibles" +
                Environment.NewLine +
                Environment.NewLine +
                "Las horas declaradas manualmente se conservarán.",
                "Actualizar cantidad maestra",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            if (confirmar != DialogResult.OK)
            {
                return false;
            }

            item.Cantidad = cantidadNueva;

            foreach (SubproductoProyecto subproducto in
                item.Subproductos ?? new List<SubproductoProyecto>())
            {
                if (subproducto == null)
                {
                    continue;
                }

                if (EsUnidadCompatibleCantidadMaestraProyecto(
                    ObtenerUnidadVisibleItem(item),
                    subproducto.Unidad))
                {
                    subproducto.Cantidad =
                        Math.Round(subproducto.Cantidad * factor, 4);
                }

                foreach (ProcesoProyecto proceso in
                    subproducto.Procesos ?? new List<ProcesoProyecto>())
                {
                    EscalarProcesoCantidadMaestraProyecto(
                        proceso,
                        ObtenerUnidadVisibleItem(item),
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

                    foreach (ProcesoProyecto proceso in
                        instancia.Procesos ??
                        new List<ProcesoProyecto>())
                    {
                        EscalarProcesoCantidadMaestraProyecto(
                            proceso,
                            ObtenerUnidadVisibleItem(item),
                            factor
                        );
                    }
                }
            }

            foreach (ProcesoProyecto proceso in
                item.Procesos ?? new List<ProcesoProyecto>())
            {
                EscalarProcesoCantidadMaestraProyecto(
                    proceso,
                    ObtenerUnidadVisibleItem(item),
                    factor
                );
            }

            Cotizacion snapshot =
                CargarSnapshotCotizacionItemProyecto(item);

            if (snapshot?.DesgloseProductivo?.Requerimientos != null)
            {
                foreach (RequerimientoProduccionInterna req in
                    snapshot.DesgloseProductivo.Requerimientos
                        .Where(r => r != null))
                {
                    if (!EsUnidadCompatibleCantidadMaestraProyecto(
                            ObtenerUnidadVisibleItem(item),
                            req.Unidad) ||
                        ModosCalculoProductivo.EsTiempoAsignado(
                            req.ModoCalculoProductivo))
                    {
                        continue;
                    }

                    req.Cantidad = Math.Round(
                        req.Cantidad * Convert.ToDouble(factor),
                        6
                    );

                    req.EditadoManualmente = false;
                }

                GuardarSnapshotCotizacionItemProyecto(item, snapshot);
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            MarcarProyectoConCambiosPendientes();
            RecalcularProyectoProductivoActual();
            RefrescarProyectoUI();

            return true;
        }

        private void EscalarProcesoCantidadMaestraProyecto(
            ProcesoProyecto proceso,
            string unidadMaestra,
            decimal factor)
        {
            if (proceso == null ||
                proceso.MetodoCalculo == MetodoCalculoProceso.Manual ||
                !EsUnidadCompatibleCantidadMaestraProyecto(
                    unidadMaestra,
                    proceso.Unidad))
            {
                return;
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
        }

        private IEnumerable<ProcesoProyecto>
            ObtenerProcesosCantidadMaestraProyecto(
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

        private bool EsUnidadEscalableCantidadMaestraProyecto(
            string unidad)
        {
            return NormalizarUnidadCantidadMaestraProyecto(unidad) ==
                "segundo";
        }

        private bool EsUnidadCompatibleCantidadMaestraProyecto(
            string unidadMaestra,
            string unidadHija)
        {
            string maestra =
                NormalizarUnidadCantidadMaestraProyecto(unidadMaestra);
            string hija =
                NormalizarUnidadCantidadMaestraProyecto(unidadHija);

            return !string.IsNullOrWhiteSpace(maestra) &&
                string.Equals(
                    maestra,
                    hija,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private string NormalizarUnidadCantidadMaestraProyecto(
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

            return valor.EndsWith("s") && valor.Length > 1
                ? valor.Substring(0, valor.Length - 1)
                : valor;
        }

'@

$contenido =
    $contenido.Substring(0, $indiceAncla) +
    $helpers +
    $contenido.Substring($indiceAncla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_cantidad_maestra_tabla_inspector_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup -Force

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Cantidad maestra conectada en tabla e inspector." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
