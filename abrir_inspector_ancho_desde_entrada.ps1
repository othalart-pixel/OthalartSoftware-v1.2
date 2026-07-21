$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

# ---------------------------------------------------------
# 1. Campo para controlar la apertura inicial.
# ---------------------------------------------------------
$anclaCampo = "        private bool ajustandoSplitterInspectorProyecto = false;"

if ($contenido.Contains($anclaCampo) -and
    -not $contenido.Contains("private bool anchoInicialInspectorProyectoAplicado")) {

    $contenido = $contenido.Replace(
        $anclaCampo,
        $anclaCampo + @'

        private bool anchoInicialInspectorProyectoAplicado = false;
'@
    )
}

# ---------------------------------------------------------
# 2. Forzar inspector visible desde la primera entrada.
# ---------------------------------------------------------
$contenido = [regex]::Replace(
    $contenido,
    'private bool inspectorProyectoVisible\s*=\s*(?:false|true);',
    'private bool inspectorProyectoVisible = true;',
    1
)

$contenido = [regex]::Replace(
    $contenido,
    'private const int AnchoInicialInspectorProyecto\s*=\s*\d+;',
    'private const int AnchoInicialInspectorProyecto = 520;',
    1
)

# ---------------------------------------------------------
# 3. Insertar helper definitivo antes del header.
# ---------------------------------------------------------
$firmaHelper = "        private void AplicarAnchoInicialInspectorCuandoEsteVisible()"
$anclaMetodo = "        private Control CrearHeaderProductosServicios()"

if (-not $contenido.Contains($firmaHelper)) {
    $indice = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)

    if ($indice -lt 0) {
        throw "No se encontro CrearHeaderProductosServicios(). No se modifico nada."
    }

    $helper = @'
        private void AplicarAnchoInicialInspectorCuandoEsteVisible()
        {
            if (anchoInicialInspectorProyectoAplicado ||
                splitProyectoEstructuraInspector == null ||
                splitProyectoEstructuraInspector.IsDisposed ||
                splitProyectoEstructuraInspector.Panel2Collapsed ||
                !splitProyectoEstructuraInspector.Visible)
            {
                return;
            }

            SplitContainer split = splitProyectoEstructuraInspector;
            int anchoDisponible = split.ClientSize.Width - split.SplitterWidth;

            if (anchoDisponible < 900)
            {
                return;
            }

            const int anchoInspectorDeseado = 520;
            const int anchoInspectorMinimo = 480;
            const int anchoEstructuraMinimo = 650;

            split.Panel1MinSize = Math.Min(
                anchoEstructuraMinimo,
                Math.Max(200, anchoDisponible - anchoInspectorMinimo)
            );

            split.Panel2MinSize = Math.Min(
                anchoInspectorMinimo,
                Math.Max(160, anchoDisponible - split.Panel1MinSize)
            );

            int distancia = anchoDisponible - anchoInspectorDeseado;
            distancia = Math.Max(
                split.Panel1MinSize,
                Math.Min(
                    distancia,
                    anchoDisponible - split.Panel2MinSize
                )
            );

            ajustandoSplitterInspectorProyecto = true;
            try
            {
                split.SplitterDistance = distancia;
                anchoInspectorProyecto = anchoInspectorDeseado;
                anchoInicialInspectorProyectoAplicado = true;
            }
            catch (InvalidOperationException)
            {
                // La pestaña todavía puede estar terminando de medir.
            }
            finally
            {
                ajustandoSplitterInspectorProyecto = false;
            }
        }

'@

    $contenido =
        $contenido.Substring(0, $indice) +
        $helper +
        $contenido.Substring($indice)
}

# ---------------------------------------------------------
# 4. Engancharlo al momento real en que el SplitContainer
# queda visible y medido.
# ---------------------------------------------------------
$anclaSplitVisible = "            split.Panel2Collapsed = !inspectorProyectoVisible;"

if ($contenido.Contains($anclaSplitVisible) -and
    -not $contenido.Contains("split.VisibleChanged += SplitProyecto_VisibleChanged;")) {

    $contenido = $contenido.Replace(
        $anclaSplitVisible,
        $anclaSplitVisible + @'

            split.VisibleChanged -= SplitProyecto_VisibleChanged;
            split.VisibleChanged += SplitProyecto_VisibleChanged;
            split.SizeChanged -= SplitProyecto_SizeChangedInicial;
            split.SizeChanged += SplitProyecto_SizeChangedInicial;
'@
    )
}

# ---------------------------------------------------------
# 5. Agregar manejadores de apertura inicial.
# ---------------------------------------------------------
$firmaEventos = "        private void SplitProyecto_VisibleChanged(object sender, EventArgs e)"

if (-not $contenido.Contains($firmaEventos)) {
    $indice = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)

    $eventos = @'
        private void SplitProyecto_VisibleChanged(object sender, EventArgs e)
        {
            if (sender is not SplitContainer split ||
                !split.Visible ||
                split.Panel2Collapsed ||
                anchoInicialInspectorProyectoAplicado)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                AplicarAnchoInicialInspectorCuandoEsteVisible();

                // Segundo intento después del primer ciclo de layout.
                BeginInvoke(new Action(AplicarAnchoInicialInspectorCuandoEsteVisible));
            }));
        }

        private void SplitProyecto_SizeChangedInicial(object sender, EventArgs e)
        {
            if (!anchoInicialInspectorProyectoAplicado &&
                inspectorProyectoVisible &&
                splitProyectoEstructuraInspector != null &&
                !splitProyectoEstructuraInspector.Panel2Collapsed)
            {
                AplicarAnchoInicialInspectorCuandoEsteVisible();
            }
        }

'@

    $contenido =
        $contenido.Substring(0, $indice) +
        $eventos +
        $contenido.Substring($indice)
}

# ---------------------------------------------------------
# 6. Cuando se vuelve a mostrar el inspector, aplicar ancho
# si estaba colapsado o demasiado pequeño.
# ---------------------------------------------------------
$contenido = [regex]::Replace(
    $contenido,
    'anchoInspectorProyecto\s*=\s*Math\.Max\(split\.Panel2MinSize,\s*split\.Width\s*-\s*split\.SplitterDistance\s*-\s*split\.SplitterWidth\);',
    'anchoInspectorProyecto = Math.Max(
                        480,
                        split.Width - split.SplitterDistance - split.SplitterWidth
                    );',
    1
)

# Si existe el metodo AlternarInspectorProyecto, reiniciar el flag al abrir.
$patronAlternar = '(inspectorProyectoVisible\s*=\s*!inspectorProyectoVisible;)'
if ([regex]::IsMatch($contenido, $patronAlternar) -and
    -not $contenido.Contains("anchoInicialInspectorProyectoAplicado = false; // reaplicar al abrir")) {

    $contenido = [regex]::Replace(
        $contenido,
        $patronAlternar,
        '$1
            if (inspectorProyectoVisible)
            {
                anchoInicialInspectorProyectoAplicado = false; // reaplicar al abrir
            }',
        1
    )
}

if ($contenido -eq $original) {
    Write-Host ""
    Write-Host "No se encontraron cambios pendientes o el arreglo ya estaba aplicado." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_inspector_visible_al_entrar_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Inspector configurado para abrir ancho desde la primera entrada." -ForegroundColor Green
Write-Host "Ancho inicial: 520 px | minimo: 480 px"
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
