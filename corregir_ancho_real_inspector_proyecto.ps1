$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$inicioTexto = "            split.SizeChanged += (s, e) =>"
$finTexto = "            split.SplitterMoving += (s, e) =>"

$inicio = $contenido.IndexOf($inicioTexto, [StringComparison]::Ordinal)
$fin = $contenido.IndexOf($finTexto, [StringComparison]::Ordinal)

if ($inicio -lt 0 -or $fin -le $inicio) {
    throw "No se encontro el bloque SizeChanged del inspector. No se modifico nada."
}

$nuevoBloque = @'
            split.SizeChanged += (s, e) =>
            {
                if (split.Panel2Collapsed ||
                    ajustandoSplitterInspectorProyecto ||
                    moviendoSplitterInspectorProyecto)
                {
                    return;
                }

                int anchoDisponible = split.ClientSize.Width - split.SplitterWidth;
                if (anchoDisponible < 900)
                {
                    return;
                }

                // El inspector debe ser util desde que se abre.
                // Conserva el ancho ajustado manualmente, pero nunca permite
                // que arranque como una franja ilegible.
                const int anchoMinimoInspector = 480;
                const int anchoMinimoEstructura = 700;

                if (anchoInspectorProyecto < anchoMinimoInspector)
                {
                    anchoInspectorProyecto = 520;
                }

                int anchoMaximoInspector = Math.Max(
                    anchoMinimoInspector,
                    anchoDisponible - anchoMinimoEstructura
                );

                int anchoDeseadoInspector = Math.Min(
                    Math.Max(anchoMinimoInspector, anchoInspectorProyecto),
                    anchoMaximoInspector
                );

                int distanciaDeseada = anchoDisponible - anchoDeseadoInspector;
                distanciaDeseada = Math.Max(
                    anchoMinimoEstructura,
                    Math.Min(
                        distanciaDeseada,
                        anchoDisponible - anchoMinimoInspector
                    )
                );

                split.Panel1MinSize = Math.Min(
                    anchoMinimoEstructura,
                    Math.Max(200, anchoDisponible - anchoMinimoInspector)
                );
                split.Panel2MinSize = Math.Min(
                    anchoMinimoInspector,
                    Math.Max(160, anchoDisponible - split.Panel1MinSize)
                );

                if (split.SplitterDistance != distanciaDeseada)
                {
                    ajustandoSplitterInspectorProyecto = true;
                    try
                    {
                        split.SplitterDistance = distanciaDeseada;
                    }
                    catch (InvalidOperationException)
                    {
                        // WinForms puede medir el SplitContainer antes de estabilizarlo.
                    }
                    finally
                    {
                        ajustandoSplitterInspectorProyecto = false;
                    }
                }
            };

'@

$contenido =
    $contenido.Substring(0, $inicio) +
    $nuevoBloque +
    $contenido.Substring($fin)

# Asegurar ancho inicial coherente.
$contenido = [regex]::Replace(
    $contenido,
    'private const int AnchoInicialInspectorProyecto\s*=\s*\d+;',
    'private const int AnchoInicialInspectorProyecto = 520;',
    1
)

# Forzar una medicion posterior al mostrar/construir el panel.
$ancla = "            split.Panel2Collapsed = !inspectorProyectoVisible;"
if ($contenido.Contains($ancla) -and
    -not $contenido.Contains("ForzarAnchoInicialInspectorProyecto(split);")) {

    $contenido = $contenido.Replace(
        $ancla,
        $ancla + @'

            if (inspectorProyectoVisible)
            {
                BeginInvoke(new Action(() => ForzarAnchoInicialInspectorProyecto(split)));
            }
'@
    )
}

# Inyectar helper antes de CrearHeaderProductosServicios.
$firmaHelper = "        private void ForzarAnchoInicialInspectorProyecto(SplitContainer split)"
$anclaMetodo = "        private Control CrearHeaderProductosServicios()"

if (-not $contenido.Contains($firmaHelper)) {
    $indiceMetodo = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)
    if ($indiceMetodo -lt 0) {
        throw "No se encontro el lugar para insertar el helper. No se modifico nada."
    }

    $helper = @'
        private void ForzarAnchoInicialInspectorProyecto(SplitContainer split)
        {
            if (split == null ||
                split.IsDisposed ||
                split.Panel2Collapsed)
            {
                return;
            }

            int anchoDisponible = split.ClientSize.Width - split.SplitterWidth;
            if (anchoDisponible < 900)
            {
                return;
            }

            const int anchoInspector = 520;
            const int anchoMinimoEstructura = 700;

            split.Panel1MinSize = Math.Min(
                anchoMinimoEstructura,
                Math.Max(200, anchoDisponible - 480)
            );
            split.Panel2MinSize = Math.Min(
                480,
                Math.Max(160, anchoDisponible - split.Panel1MinSize)
            );

            int distancia = anchoDisponible - anchoInspector;
            distancia = Math.Max(
                split.Panel1MinSize,
                Math.Min(distancia, anchoDisponible - split.Panel2MinSize)
            );

            ajustandoSplitterInspectorProyecto = true;
            try
            {
                split.SplitterDistance = distancia;
                anchoInspectorProyecto = anchoInspector;
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                ajustandoSplitterInspectorProyecto = false;
            }
        }

'@

    $contenido =
        $contenido.Substring(0, $indiceMetodo) +
        $helper +
        $contenido.Substring($indiceMetodo)
}

if ($contenido -eq $original) {
    Write-Host ""
    Write-Host "El ajuste ya estaba aplicado o no hubo cambios." -ForegroundColor Yellow
    exit 0
}

$backup = "$ruta.backup_inspector_ancho_real_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Logica completa del ancho del inspector corregida." -ForegroundColor Green
Write-Host "Ancho inicial: 520 px | minimo: 480 px"
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
