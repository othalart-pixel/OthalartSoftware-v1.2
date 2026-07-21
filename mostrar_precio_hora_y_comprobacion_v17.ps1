$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabEcuaciones.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabEcuaciones.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("MostrarPrecioHoraSiempreV17")) {
    Write-Host "La mejora V17 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

# ---------------------------------------------------------
# 1. Agregar columna de comprobacion si no existe.
# ---------------------------------------------------------
$anclaColumnas = @'
            dgvCargosParticipantesEcuacion.Columns.Add("CostoTotalCLP", "Costo total");
'@

$reemplazoColumnas = @'
            dgvCargosParticipantesEcuacion.Columns.Add("CostoTotalCLP", "Costo total");
            dgvCargosParticipantesEcuacion.Columns.Add(
                "ComprobacionCosto",
                "Calculo"
            );
'@

if (-not $contenido.Contains($anclaColumnas)) {
    throw "No se encontro la columna CostoTotalCLP."
}

$contenido = $contenido.Replace(
    $anclaColumnas,
    $reemplazoColumnas
)

# ---------------------------------------------------------
# 2. Ancho de la nueva columna.
# ---------------------------------------------------------
$anclaAnchos = @'
            dgvCargosParticipantesEcuacion.Columns["CostoTotalCLP"].Width = 120;
'@

$reemplazoAnchos = @'
            dgvCargosParticipantesEcuacion.Columns["CostoTotalCLP"].Width = 120;
            dgvCargosParticipantesEcuacion.Columns["ComprobacionCosto"].Width = 245;
'@

if (-not $contenido.Contains($anclaAnchos)) {
    throw "No se encontro el ancho de CostoTotalCLP."
}

$contenido = $contenido.Replace(
    $anclaAnchos,
    $reemplazoAnchos
)

# ---------------------------------------------------------
# 3. Renombrar y forzar visibilidad del precio/hora.
# ---------------------------------------------------------
$anclaVista = @'
            if (dgvCargosParticipantesEcuacion.Columns.Contains("TarifaHorariaCLP"))
            {
                dgvCargosParticipantesEcuacion.Columns["TarifaHorariaCLP"].HeaderText = "Tarifa base/h";
            }
'@

$reemplazoVista = @'
            // MostrarPrecioHoraSiempreV17
            if (dgvCargosParticipantesEcuacion.Columns.Contains("TarifaHorariaCLP"))
            {
                DataGridViewColumn columnaPrecioHora =
                    dgvCargosParticipantesEcuacion.Columns["TarifaHorariaCLP"];

                columnaPrecioHora.HeaderText = "Precio/hora";
                columnaPrecioHora.Visible = true;
                columnaPrecioHora.DisplayIndex = Math.Min(
                    columnaPrecioHora.DisplayIndex,
                    6
                );
                columnaPrecioHora.Width = 115;
            }

            if (dgvCargosParticipantesEcuacion.Columns.Contains("ComprobacionCosto"))
            {
                DataGridViewColumn columnaComprobacion =
                    dgvCargosParticipantesEcuacion.Columns["ComprobacionCosto"];

                columnaComprobacion.HeaderText = "Horas x precio/hora";
                columnaComprobacion.Visible = true;
                columnaComprobacion.Width = 245;
            }
'@

if (-not $contenido.Contains($anclaVista)) {
    throw "No se encontro el bloque de TarifaHorariaCLP."
}

$contenido = $contenido.Replace(
    $anclaVista,
    $reemplazoVista
)

# ---------------------------------------------------------
# 4. Inyectar la comprobacion al final de cada actualizacion
#    de la vista previa de cargos.
# ---------------------------------------------------------
$anclaMetodo = "        private void ActualizarVistaPreviaCargosParticipantes("
$inicio = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)

if ($inicio -lt 0) {
    throw "No se encontro ActualizarVistaPreviaCargosParticipantes."
}

$llave = $contenido.IndexOf("{", $inicio, [StringComparison]::Ordinal)
$profundidad = 0
$fin = -1

for ($i = $llave; $i -lt $contenido.Length; $i++) {
    if ($contenido[$i] -eq '{') {
        $profundidad++
    }
    elseif ($contenido[$i] -eq '}') {
        $profundidad--
        if ($profundidad -eq 0) {
            $fin = $i
            break
        }
    }
}

if ($fin -lt 0) {
    throw "No se encontro el final de ActualizarVistaPreviaCargosParticipantes."
}

$llamada = @'

            ActualizarComprobacionPrecioHoraV17();
'@

$contenido =
    $contenido.Substring(0, $fin) +
    $llamada +
    $contenido.Substring($fin)

# ---------------------------------------------------------
# 5. Insertar helper antes del evento SelectionChanged.
# ---------------------------------------------------------
$anclaHelper =
    "        private void DgvCargosParticipantesEcuacion_SelectionChanged("

$indiceHelper =
    $contenido.IndexOf($anclaHelper, [StringComparison]::Ordinal)

if ($indiceHelper -lt 0) {
    throw "No se encontro el punto para insertar el helper V17."
}

$helper = @'
        private void ActualizarComprobacionPrecioHoraV17()
        {
            if (dgvCargosParticipantesEcuacion == null ||
                dgvCargosParticipantesEcuacion.IsDisposed ||
                !dgvCargosParticipantesEcuacion.Columns.Contains("ComprobacionCosto"))
            {
                return;
            }

            foreach (DataGridViewRow fila in
                dgvCargosParticipantesEcuacion.Rows)
            {
                if (fila == null || fila.IsNewRow)
                {
                    continue;
                }

                double horas =
                    ConvertirNumeroFlexible(
                        fila.Cells["HorasAsignadas"].Value,
                        0.0
                    );

                double precioHora =
                    ConvertirNumeroFlexible(
                        fila.Cells["TarifaHorariaCLP"].Value,
                        0.0
                    );

                double costo =
                    ConvertirNumeroFlexible(
                        fila.Cells["CostoTotalCLP"].Value,
                        0.0
                    );

                string texto =
                    horas.ToString("0.##") +
                    " h x " +
                    FormatearMiles(precioHora) +
                    " CLP/h = " +
                    FormatearMiles(costo) +
                    " CLP";

                fila.Cells["ComprobacionCosto"].Value =
                    texto;
            }
        }

'@

$contenido =
    $contenido.Substring(0, $indiceHelper) +
    $helper +
    $contenido.Substring($indiceHelper)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_precio_hora_visible_v17_" +
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
Write-Host "Mejora V17 aplicada." -ForegroundColor Green
Write-Host "Precio/hora quedara siempre visible."
Write-Host "Se agrego la comprobacion Horas x Precio/hora = Costo."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
