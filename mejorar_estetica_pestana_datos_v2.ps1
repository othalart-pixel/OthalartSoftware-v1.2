$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabDatos.cs"
if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabDatos.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido
$cambios = New-Object System.Collections.Generic.List[string]
$omitidos = New-Object System.Collections.Generic.List[string]

function Reemplazar-RegexOpcional {
    param(
        [string]$Nombre,
        [string]$Patron,
        [string]$Reemplazo
    )

    $regex = [regex]::new(
        $Patron,
        [System.Text.RegularExpressions.RegexOptions]::Singleline
    )

    if ($regex.IsMatch($script:contenido)) {
        $script:contenido = $regex.Replace($script:contenido, $Reemplazo, 1)
        $script:cambios.Add($Nombre)
    }
    else {
        $script:omitidos.Add($Nombre)
    }
}

# ---------------------------------------------------------
# 1. Contenedor principal adaptable.
# Acepta Absolute, Percent o variantes ya modificadas.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Ancho adaptable del contenido" `
    'contenedor\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.(?:Absolute|Percent),\s*(?:1060|100F?)\)\);(?:\s*contenedor\.MinimumSize\s*=\s*new Size\(860,\s*0\);)?' `
    'contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contenedor.MinimumSize = new Size(860, 0);'

# ---------------------------------------------------------
# 2. Cabecera.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Titulo principal" `
    'titulo\.Font\s*=\s*new Font\("Segoe UI",\s*(?:19|20\.5f?),\s*FontStyle\.Bold\);\s* titulo\.ForeColor\s*=\s*Color\.FromArgb\([^)]+\);\s* titulo\.AutoSize\s*=\s*true;\s* titulo\.Margin\s*=\s*new Padding\([^)]+\);' `
    'titulo.Font = new Font("Segoe UI", 20.5f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(24, 28, 32);
            titulo.AutoSize = true;
            titulo.Margin = new Padding(2, 0, 0, 5);'

Reemplazar-RegexOpcional `
    "Bajada principal" `
    'bajada\.Font\s*=\s*new Font\("Segoe UI",\s*(?:9\.5f|10f?),\s*FontStyle\.Regular\);\s* bajada\.ForeColor\s*=\s*Color\.FromArgb\([^)]+\);\s* bajada\.AutoSize\s*=\s*true;\s* bajada\.Margin\s*=\s*new Padding\([^)]+\);' `
    'bajada.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            bajada.ForeColor = Color.FromArgb(100, 106, 114);
            bajada.AutoSize = true;
            bajada.Margin = new Padding(2, 0, 0, 20);'

# ---------------------------------------------------------
# 3. Tarjetas.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Estilo de tarjetas" `
    'card\.BackColor\s*=\s*Color\.White;\s* card\.Padding\s*=\s*new Padding\([^)]+\);\s* card\.Margin\s*=\s*new Padding\([^)]+\);\s* card\.BorderStyle\s*=\s*BorderStyle\.(?:FixedSingle|None);(?:\s*card\.MinimumSize\s*=\s*new Size\([^)]+\);)?' `
    'card.BackColor = Color.White;
            card.Padding = new Padding(22, 18, 22, 20);
            card.Margin = new Padding(0, 0, 0, 16);
            card.BorderStyle = BorderStyle.None;
            card.MinimumSize = new Size(0, 72);'

Reemplazar-RegexOpcional `
    "Titulos de tarjetas" `
    'lblTitulo\.Font\s*=\s*new Font\("Segoe UI",\s*(?:12\.2f|12\.8f),\s*FontStyle\.Bold\);\s* lblTitulo\.ForeColor\s*=\s*Color\.FromArgb\([^)]+\);\s* lblTitulo\.Margin\s*=\s*new Padding\([^)]+\);' `
    'lblTitulo.Font = new Font("Segoe UI", 12.8f, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(28, 32, 36);
            lblTitulo.Margin = new Padding(0, 0, 0, 14);'

# ---------------------------------------------------------
# 4. Formularios flexibles.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Formularios adaptables" `
    'formulario\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.Absolute,\s*anchoEtiqueta\)\);\s* formulario\.ColumnStyles\.Add\(new ColumnStyle\(SizeType\.(?:Absolute|Percent),\s*(?:anchoControl|100F?)\)\);(?:\s*formulario\.MinimumSize\s*=\s*new Size\([^)]+\);)?' `
    'formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, anchoEtiqueta));
            formulario.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            formulario.MinimumSize = new Size(anchoEtiqueta + Math.Min(anchoControl, 520), 0);'

# ---------------------------------------------------------
# 5. Boton aplicar.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Boton Aplicar datos" `
    'btnAplicarDatos\.(?:Width\s*=\s*250|AutoSize\s*=\s*true);.*?btnAplicarDatos\.Font\s*=\s*new Font\("Segoe UI",\s*10,\s*FontStyle\.Bold\);\s* btnAplicarDatos\.Margin\s*=\s*new Padding\([^)]+\);' `
    'btnAplicarDatos.AutoSize = true;
            btnAplicarDatos.MinimumSize = new Size(230, 40);
            btnAplicarDatos.Height = 40;
            btnAplicarDatos.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAplicarDatos.Margin = new Padding(0, 2, 0, 9);
            btnAplicarDatos.Padding = new Padding(18, 0, 18, 0);
            btnAplicarDatos.Cursor = Cursors.Hand;'

# ---------------------------------------------------------
# 6. Botones del alcance.
# ---------------------------------------------------------
Reemplazar-RegexOpcional `
    "Botones del alcance" `
    'boton\.Width\s*=\s*ancho;\s* boton\.Height\s*=\s*34;\s* boton\.Font\s*=\s*new Font\("Segoe UI",\s*9\.2f,\s*FontStyle\.Bold\);\s* boton\.FlatStyle\s*=\s*FlatStyle\.Flat;\s* boton\.BackColor\s*=\s*Color\.White;\s* boton\.Margin\s*=\s*new Padding\([^)]+\);\s* boton\.UseVisualStyleBackColor\s*=\s*false;' `
    'boton.AutoSize = true;
            boton.MinimumSize = new Size(ancho, 34);
            boton.Height = 34;
            boton.Font = new Font("Segoe UI", 9.2f, FontStyle.Bold);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = Color.FromArgb(184, 190, 196);
            boton.BackColor = Color.White;
            boton.Margin = new Padding(0, 0, 8, 5);
            boton.Padding = new Padding(12, 0, 12, 0);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;'

# ---------------------------------------------------------
# 7. Reemplazo completo y seguro de la fila del producto.
# ---------------------------------------------------------
$inicio = $contenido.IndexOf(
    "        private Control CrearFilaProductoProyectoEnDatos(ItemProyecto item, ProyectoConsolidado consolidado)",
    [StringComparison]::Ordinal
)
$fin = $contenido.IndexOf(
    "        private void SincronizarProyectoProductivoActualDesdeCotizacion()",
    [StringComparison]::Ordinal
)

if ($inicio -ge 0 -and $fin -gt $inicio) {
    $metodoActual = $contenido.Substring($inicio, $fin - $inicio)

    if ($metodoActual.Contains("TableLayoutPanel layout = new TableLayoutPanel();") -and
        $metodoActual.Contains("FlowLayoutPanel acciones = new FlowLayoutPanel();")) {
        $omitidos.Add("Fila responsive del producto (ya estaba aplicada)")
    }
    else {
        $nuevoMetodo = @'
        private Control CrearFilaProductoProyectoEnDatos(ItemProyecto item, ProyectoConsolidado consolidado)
        {
            Panel fila = new Panel();
            fila.Dock = DockStyle.Top;
            fila.AutoSize = true;
            fila.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fila.MinimumSize = new Size(0, 72);
            fila.Margin = new Padding(0, 0, 0, 9);
            fila.Padding = new Padding(14, 10, 14, 10);
            fila.BackColor = Color.FromArgb(252, 253, 253);
            fila.BorderStyle = BorderStyle.FixedSingle;

            ResumenConsolidado resumen = consolidado == null
                ? null
                : consolidado.PorProducto.FirstOrDefault(p =>
                    string.Equals(p.Id, item.Id, StringComparison.OrdinalIgnoreCase));

            int subproductos = item.Subproductos == null
                ? 0
                : item.Subproductos.Count(s => s != null && s.Activo);
            decimal horas = resumen == null ? 0m : resumen.Horas;
            decimal costo = resumen == null ? 0m : resumen.Costo;
            string cantidad = FormatearCantidadProyectoDatos(item.Cantidad, item.Unidad);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Top;
            layout.AutoSize = true;
            layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.Margin = new Padding(0);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            TableLayoutPanel bloqueTexto = new TableLayoutPanel();
            bloqueTexto.Dock = DockStyle.Fill;
            bloqueTexto.AutoSize = true;
            bloqueTexto.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bloqueTexto.ColumnCount = 1;
            bloqueTexto.RowCount = 2;
            bloqueTexto.Margin = new Padding(0, 0, 16, 0);
            bloqueTexto.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bloqueTexto.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titulo = new Label();
            titulo.Text = string.IsNullOrWhiteSpace(item.Nombre)
                ? "Producto o servicio sin nombre"
                : item.Nombre;
            titulo.Dock = DockStyle.Top;
            titulo.AutoSize = true;
            titulo.MaximumSize = new Size(620, 0);
            titulo.Font = new Font("Segoe UI", 10.7f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(27, 31, 35);
            titulo.Margin = new Padding(0, 0, 0, 4);

            Label detalle = new Label();
            detalle.Text = cantidad + "  ·  " +
                subproductos.ToString("0") + " subproductos  ·  " +
                horas.ToString("0.##") + " h  ·  " +
                FormatearValorVisual((double)costo);
            detalle.Dock = DockStyle.Top;
            detalle.AutoSize = true;
            detalle.MaximumSize = new Size(650, 0);
            detalle.Font = new Font("Segoe UI", 9f);
            detalle.ForeColor = Color.FromArgb(95, 100, 108);
            detalle.Margin = new Padding(0);

            FlowLayoutPanel acciones = new FlowLayoutPanel();
            acciones.AutoSize = true;
            acciones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            acciones.FlowDirection = FlowDirection.LeftToRight;
            acciones.WrapContents = true;
            acciones.Margin = new Padding(0, 4, 0, 0);
            acciones.Padding = new Padding(0);

            Button editar = CrearBotonAccionAlcanceDatos("Editar", 78);
            editar.AutoSize = true;
            editar.MinimumSize = new Size(78, 32);
            editar.Margin = new Padding(0, 0, 7, 5);
            editar.Cursor = Cursors.Hand;
            editar.Click += (s, e) => EditarProductoProyectoDesdeDatos(item);

            Button pipeline = CrearBotonAccionAlcanceDatos("Pipeline", 88);
            pipeline.AutoSize = true;
            pipeline.MinimumSize = new Size(88, 32);
            pipeline.Margin = new Padding(0, 0, 7, 5);
            pipeline.Cursor = Cursors.Hand;
            pipeline.Click += (s, e) => AbrirPipelineProductoProyectoDesdeDatos(item);

            Button quitar = CrearBotonAccionAlcanceDatos("Quitar", 78);
            quitar.AutoSize = true;
            quitar.MinimumSize = new Size(78, 32);
            quitar.Margin = new Padding(0, 0, 0, 5);
            quitar.Cursor = Cursors.Hand;
            quitar.Click += (s, e) => QuitarItemProyecto(item);

            bloqueTexto.Controls.Add(titulo, 0, 0);
            bloqueTexto.Controls.Add(detalle, 0, 1);

            acciones.Controls.Add(editar);
            acciones.Controls.Add(pipeline);
            acciones.Controls.Add(quitar);

            layout.Controls.Add(bloqueTexto, 0, 0);
            layout.Controls.Add(acciones, 1, 0);

            fila.Controls.Add(layout);
            return fila;
        }

'@

        $contenido =
            $contenido.Substring(0, $inicio) +
            $nuevoMetodo +
            $contenido.Substring($fin)

        $cambios.Add("Fila responsive del producto")
    }
}
else {
    $omitidos.Add("Fila responsive del producto (metodo no encontrado)")
}

if ($contenido -eq $original) {
    Write-Host ""
    Write-Host "No fue necesario modificar el archivo." -ForegroundColor Yellow
    Write-Host "Los cambios pueden estar ya aplicados o el archivo difiere demasiado."
    Write-Host ""
    Write-Host "Elementos omitidos:"
    foreach ($item in $omitidos) {
        Write-Host " - $item"
    }
    exit 0
}

$backup = "$ruta.backup_estetica_datos_v2_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Mejora de la pestana Datos aplicada correctamente." -ForegroundColor Green
Write-Host "Backup: $backup"
Write-Host ""
Write-Host "Cambios aplicados:"
foreach ($item in $cambios) {
    Write-Host " + $item"
}

if ($omitidos.Count -gt 0) {
    Write-Host ""
    Write-Host "Elementos omitidos de forma segura:"
    foreach ($item in $omitidos) {
        Write-Host " - $item"
    }
}

Write-Host ""
Write-Host 'Ahora compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
