$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabEcuacionesAmigable.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabEcuacionesAmigable.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

function Reemplazar-Exacto {
    param(
        [string]$Nombre,
        [string]$Viejo,
        [string]$Nuevo
    )

    if (-not $script:contenido.Contains($Viejo)) {
        throw "No se encontro el bloque esperado: $Nombre. No se modifico el archivo."
    }

    $script:contenido = $script:contenido.Replace($Viejo, $Nuevo)
}

# 1. Renombrar la opcion para que sea inequívoca.
Reemplazar-Exacto `
    "nombre del metodo manual" `
    '                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.Manual, Nombre = "Manual / fórmula avanzada" },' `
    '                new OpcionMetodoSimpleEcuacion { Metodo = MetodoCalculoProceso.Manual, Nombre = "Horas declaradas manualmente en el proyecto" },'

# 2. Hacer explícita la interfaz: no necesita valor ni dependencia.
$viejoCampos = @'
                case MetodoCalculoProceso.PorCapacidad:
                    lblValorSimpleEcuacion.Text = "Capacidad";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
                default:
                    lblValorSimpleEcuacion.Text = "Valor";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
            }

            if (metodo == MetodoCalculoProceso.Manual && !pnlAvanzadoEcuacion.Visible)
            {
                pnlAvanzadoEcuacion.Visible = true;
                btnAlternarAvanzadoEcuacion.Text = "▾ Ocultar opciones avanzadas";
                btnAlternarAvanzadoEcuacion.Width = 230;
            }
'@

$nuevoCampos = @'
                case MetodoCalculoProceso.PorCapacidad:
                    lblValorSimpleEcuacion.Text = "Capacidad";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
                case MetodoCalculoProceso.Manual:
                    lblValorSimpleEcuacion.Text = "Horas";
                    nudValorSimpleEcuacion.Enabled = false;
                    cmbDependenciaSimpleEcuacion.Enabled = false;
                    break;
                default:
                    lblValorSimpleEcuacion.Text = "Valor";
                    nudValorSimpleEcuacion.Enabled = false;
                    break;
            }

            if (metodo == MetodoCalculoProceso.Manual)
            {
                lblExplicacionSimpleEcuacion.Text =
                    "Regla: el sistema no calcula horas automáticamente. " +
                    "Las horas deben declararse manualmente dentro del proyecto.";

                nudValorSimpleEcuacion.Value = 0;
            }
'@

Reemplazar-Exacto "campos visibles del metodo manual" $viejoCampos $nuevoCampos

# 3. Al seleccionar Manual, limpiar formula y dependencias técnicas.
$viejoFormula = @'
                case MetodoCalculoProceso.Manual:
                case MetodoCalculoProceso.NoDefinido:
                    break;
'@

$nuevoFormula = @'
                case MetodoCalculoProceso.Manual:
                    txtEcuacionFormula.Text = "";
                    cmbDependenciaSimpleEcuacion.SelectedIndex =
                        cmbDependenciaSimpleEcuacion.Items.Count > 0 ? 0 : -1;
                    break;
                case MetodoCalculoProceso.NoDefinido:
                    break;
'@

Reemplazar-Exacto "formula del metodo manual" $viejoFormula $nuevoFormula

# 4. Guardar el metodo sin dependencias residuales.
$viejoAplicar = @'
            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            fila.Cells["MetodoCalculo"].Value = metodo.ToString();
            fila.Cells["TipoProceso"].Value = ObtenerTipoTrabajoSimpleEcuacion().ToString();
            string dependencia = ObtenerDependenciaSimpleEcuacion();
            fila.Cells["DependenciasJson"].Value =
                ReglaCalculoProcesoService.CrearDependenciasJson(new[] { dependencia });
'@

$nuevoAplicar = @'
            MetodoCalculoProceso metodo = ObtenerMetodoSimpleEcuacion();
            fila.Cells["MetodoCalculo"].Value = metodo.ToString();
            fila.Cells["TipoProceso"].Value = ObtenerTipoTrabajoSimpleEcuacion().ToString();

            string dependencia = metodo == MetodoCalculoProceso.Manual
                ? ""
                : ObtenerDependenciaSimpleEcuacion();

            fila.Cells["DependenciasJson"].Value =
                ReglaCalculoProcesoService.CrearDependenciasJson(
                    string.IsNullOrWhiteSpace(dependencia)
                        ? Array.Empty<string>()
                        : new[] { dependencia }
                );

            if (metodo == MetodoCalculoProceso.Manual)
            {
                fila.Cells["FormulaReferencia"].Value = "";
            }
'@

Reemplazar-Exacto "guardado limpio del metodo manual" $viejoAplicar $nuevoAplicar

# 5. Vista previa clara y sin falsa formula.
$viejoPreview = @'
                case MetodoCalculoProceso.Manual:
                    regla = "Fórmula configurada manualmente";
                    ejemplo = string.IsNullOrWhiteSpace(txtEcuacionFormula.Text)
                        ? "Abre Opciones avanzadas para escribir la fórmula"
                        : txtEcuacionFormula.Text;
                    break;
'@

$nuevoPreview = @'
                case MetodoCalculoProceso.Manual:
                    regla = "Horas declaradas manualmente en el proyecto";
                    ejemplo =
                        "Esta regla no calcula horas desde segundos, cantidades, " +
                        "capacidades ni fórmulas. El resultado queda pendiente hasta " +
                        "que se ingresen las horas destinadas al equipo.";
                    break;
'@

Reemplazar-Exacto "vista previa del metodo manual" $viejoPreview $nuevoPreview

# 6. Agregar aviso visible en la vista previa.
$viejoAvisos = @'
            if (metodo == MetodoCalculoProceso.PorPorcentajeProduccion &&
                string.IsNullOrWhiteSpace(dependencia))
            {
                avisos.Add("⚠ Selecciona el trabajo del que depende.");
            }
            if (cargos == 0)
'@

$nuevoAvisos = @'
            if (metodo == MetodoCalculoProceso.PorPorcentajeProduccion &&
                string.IsNullOrWhiteSpace(dependencia))
            {
                avisos.Add("⚠ Selecciona el trabajo del que depende.");
            }

            if (metodo == MetodoCalculoProceso.Manual)
            {
                avisos.Add(
                    "Pendiente: las horas deberán ingresarse manualmente en el proyecto."
                );
            }

            if (cargos == 0)
'@

Reemplazar-Exacto "aviso de horas pendientes" $viejoAvisos $nuevoAvisos

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_horas_declaradas_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Metodo Horas declaradas manualmente aplicado correctamente." -ForegroundColor Green
Write-Host "Se reutiliza MetodoCalculoProceso.Manual existente."
Write-Host "No se modificaron los JSON ni los modelos."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
