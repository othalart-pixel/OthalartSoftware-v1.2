$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabDatos.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabDatos.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)

$inicioFirma = "        private Control CrearResumenProductosProyectoEnDatos()"
$finFirma = "        private Control CrearFilaProductoProyectoEnDatos(ItemProyecto item, ProyectoConsolidado consolidado)"

$inicio = $contenido.IndexOf($inicioFirma, [StringComparison]::Ordinal)
$fin = $contenido.IndexOf($finFirma, [StringComparison]::Ordinal)

if ($inicio -lt 0) {
    throw "No se encontro el metodo CrearResumenProductosProyectoEnDatos(). No se modifico nada."
}

if ($fin -le $inicio) {
    throw "No se encontro el final del metodo de resumen. No se modifico nada."
}

$metodoActual = $contenido.Substring($inicio, $fin - $inicio)

if ($metodoActual.Contains("TableLayoutPanel lista = new TableLayoutPanel();")) {
    Write-Host ""
    Write-Host "El contenedor del resumen ya usa el layout corregido." -ForegroundColor Yellow
    exit 0
}

$nuevoMetodo = @'
        private Control CrearResumenProductosProyectoEnDatos()
        {
            SincronizarProyectoProductivoActualDesdeCotizacion();

            // TableLayoutPanel en vez de FlowLayoutPanel:
            // ocupa todo el ancho disponible y evita que las filas
            // responsive colapsen a un rectangulo angosto.
            TableLayoutPanel lista = new TableLayoutPanel();
            lista.Dock = DockStyle.Top;
            lista.AutoSize = true;
            lista.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            lista.ColumnCount = 1;
            lista.RowCount = 0;
            lista.Margin = new Padding(0);
            lista.Padding = new Padding(0);
            lista.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            List<ItemProyecto> items = ObtenerItemsProyectoParaDatos();

            if (items.Count == 0)
            {
                Label vacio = new Label();
                vacio.Text = "No hay productos ni servicios agregados al proyecto.";
                vacio.Dock = DockStyle.Top;
                vacio.AutoSize = true;
                vacio.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
                vacio.ForeColor = Color.FromArgb(90, 90, 90);
                vacio.Margin = new Padding(0, 2, 0, 8);

                lista.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                lista.Controls.Add(vacio, 0, 0);
                lista.RowCount = 1;
                return lista;
            }

            ProyectoConsolidado consolidado = ObtenerConsolidadoProyectoActualParaDatos();

            foreach (ItemProyecto item in items)
            {
                Control fila = CrearFilaProductoProyectoEnDatos(item, consolidado);
                fila.Dock = DockStyle.Top;
                fila.Margin = new Padding(0, 0, 0, 9);

                int indiceFila = lista.RowCount;
                lista.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                lista.Controls.Add(fila, 0, indiceFila);
                lista.RowCount++;
            }

            return lista;
        }

'@

$backup = "$ruta.backup_resumen_productos_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$contenidoNuevo =
    $contenido.Substring(0, $inicio) +
    $nuevoMetodo +
    $contenido.Substring($fin)

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenidoNuevo, $utf8ConBom)

Write-Host ""
Write-Host "Resumen de productos restaurado y adaptado al ancho disponible." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
