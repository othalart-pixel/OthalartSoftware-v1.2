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

$viejoPreparar = @'
            string valor = Convert.ToString(row.Cells[columnIndex].Value) ?? "";
            aplicandoEdicionTablaProyecto = true;
            try
            {
                bool cambio = AplicarCambioNodoTablaProductosProyecto(nodo, col, valor.Trim());
'@

$nuevoPreparar = @'
            string valor = Convert.ToString(row.Cells[columnIndex].Value) ?? "";

            decimal cantidadAnteriorMaestra = 0m;
            decimal cantidadNuevaMaestra = 0m;
            bool esCambioCantidadMaestra =
                nodo is ItemProyecto itemCantidadMaestra &&
                col == "Cantidad" &&
                TryParseDecimalProyecto(valor.Trim(), out cantidadNuevaMaestra) &&
                cantidadNuevaMaestra >= 0m &&
                EsUnidadEscalablePorCantidadMaestra(itemCantidadMaestra.Unidad);

            if (esCambioCantidadMaestra)
            {
                cantidadAnteriorMaestra = itemCantidadMaestra.Cantidad;

                if (cantidadAnteriorMaestra != cantidadNuevaMaestra &&
                    !ConfirmarCambioCantidadMaestraProducto(
                        itemCantidadMaestra,
                        cantidadAnteriorMaestra,
                        cantidadNuevaMaestra))
                {
                    row.Cells[columnIndex].Value =
                        cantidadAnteriorMaestra.ToString("0.##");
                    return;
                }
            }

            aplicandoEdicionTablaProyecto = true;
            try
            {
                bool cambio = AplicarCambioNodoTablaProductosProyecto(nodo, col, valor.Trim());
'@

if (-not $contenido.Contains($viejoPreparar)) {
    throw "No se encontro el inicio esperado de AplicarEdicionTablaProductos."
}

$contenido = $contenido.Replace($viejoPreparar, $nuevoPreparar)

$viejoCambio = @'
                if (cambio)
                {
                    MarcarNodoProyectoModificado(nodo, col, valor.Trim());
                    nodoProyectoSeleccionado = nodo;
                    itemProyectoSeleccionado = nodo as ItemProyecto ?? ObtenerItemContenedor(nodo);
                    RecalcularProyectoProductivoActual();
                }
'@

$nuevoCambio = @'
                if (cambio)
                {
                    if (esCambioCantidadMaestra &&
                        nodo is ItemProyecto itemMaestro &&
                        cantidadAnteriorMaestra != cantidadNuevaMaestra)
                    {
                        PropagarCantidadMaestraProductoProyecto(
                            itemMaestro,
                            cantidadAnteriorMaestra,
                            cantidadNuevaMaestra
                        );
                    }

                    MarcarNodoProyectoModificado(nodo, col, valor.Trim());
                    nodoProyectoSeleccionado = nodo;
                    itemProyectoSeleccionado =
                        nodo as ItemProyecto ?? ObtenerItemContenedor(nodo);
                    RecalcularProyectoProductivoActual();
                }
'@

if (-not $contenido.Contains($viejoCambio)) {
    throw "No se encontro el bloque de recálculo esperado."
}

$contenido = $contenido.Replace($viejoCambio, $nuevoCambio)

$ancla = "        private bool EsColumnaEditableTablaProductosProyecto(string columna)"

if (-not $contenido.Contains($ancla)) {
    throw "No se encontro el punto para insertar los helpers."
}

$helpers = @'
        private bool ConfirmarCambioCantidadMaestraProducto(
            ItemProyecto item,
            decimal cantidadAnterior,
            decimal cantidadNueva)
        {
            if (item == null)
            {
                return false;
            }

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

            DialogResult respuesta = MessageBox.Show(
                this,
                "Cambiar la cantidad maestra de " +
                cantidadAnterior.ToString("0.##") + " a " +
                cantidadNueva.ToString("0.##") + " " +
                (item.Unidad ?? "") + " actualizará:" +
                Environment.NewLine +
                Environment.NewLine +
                "• " + subproductosCompatibles +
                " subproductos con unidad compatible" +
                Environment.NewLine +
                "• " + procesosCompatibles +
                " procesos calculados" +
                Environment.NewLine +
                "• los requerimientos productivos del snapshot" +
                Environment.NewLine +
                Environment.NewLine +
                "Las horas declaradas manualmente se conservarán.",
                "Escalar producto completo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question
            );

            return respuesta == DialogResult.OK;
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
                        instancia.Procesos ?? new List<ProcesoProyecto>())
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

                    // La cantidad cambia por herencia del producto maestro.
                    // No convertimos el proceso en una edición manual de horas.
                    modificados++;
                }

                GuardarSnapshotCotizacionItemProyecto(item, snapshot);
            }

            item.FechaEdicionSnapshot = DateTime.Now;

            lblEstadoProyecto.Text =
                "Cantidad maestra actualizada: " +
                modificados +
                " elementos compatibles escalados. " +
                "Recalculando horas y costos...";

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

            // Se limpian solo resultados calculados. Las horas asignadas
            // manualmente permanecen intactas.
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
                if (asignacion == null)
                {
                    continue;
                }

                if (asignacion.OrigenHoras ==
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
            if (requerimiento == null)
            {
                return false;
            }

            return ModosCalculoProductivo.EsTiempoAsignado(
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
            string normalizada =
                NormalizarUnidadCantidadMaestra(unidad);

            return normalizada == "segundo";
        }

        private string NormalizarUnidadCantidadMaestra(
            string unidad)
        {
            string limpia = (unidad ?? "")
                .Trim()
                .ToLowerInvariant();

            limpia = limpia
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

$contenido = $contenido.Replace($ancla, $helpers + $ancla)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_cantidad_maestra_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "Cantidad maestra del producto conectada." -ForegroundColor Green
Write-Host "Escala unidades en segundos, respeta horas manuales y recalcula."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
