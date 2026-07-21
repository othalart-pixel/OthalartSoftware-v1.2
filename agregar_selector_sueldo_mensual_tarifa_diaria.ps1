$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabEcuaciones.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabEcuaciones.cs. Ejecuta este script desde la carpeta raiz del proyecto."
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

# 1) Agrandar ventana y agregar una fila.
Reemplazar-Exacto `
    "tamano del dialogo" `
    '            dialogo.ClientSize = new Size(680, 465);' `
    '            dialogo.ClientSize = new Size(680, 505);'

Reemplazar-Exacto `
    "cantidad de filas" `
    '            tabla.RowCount = 10;' `
    '            tabla.RowCount = 12;'

# 2) Reemplazar la creacion de tarifa por selector mensual/diario.
$viejoTarifa = @'
            NumericUpDown nudTarifa = CrearNumericDialogo(0, 100000000, 0, 1000, 0);
            nudTarifa.Value = (decimal)Math.Max(0, Math.Min(100000000, trabajo.TarifaDiariaCLP));

            NumericUpDown nudHorasDia = CrearNumericDialogo(0.1M, 24, 8, 0.5M, 2);
'@

$nuevoTarifa = @'
            const decimal DiasLaboralesMesTarifa = 22M;

            ComboBox cmbFormaTarifa = new ComboBox();
            cmbFormaTarifa.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFormaTarifa.Dock = DockStyle.Fill;
            cmbFormaTarifa.Items.Add("Sueldo mensual");
            cmbFormaTarifa.Items.Add("Tarifa diaria");
            cmbFormaTarifa.SelectedIndex = 0;

            NumericUpDown nudSueldoMensual = CrearNumericDialogo(
                0,
                1000000000,
                0,
                10000,
                0
            );

            NumericUpDown nudTarifa = CrearNumericDialogo(
                0,
                100000000,
                0,
                1000,
                0
            );

            decimal tarifaDiariaInicial = (decimal)Math.Max(
                0,
                Math.Min(100000000, trabajo.TarifaDiariaCLP)
            );

            CategoriaTrabajador cargoTarifaInicial = obtenerCargoSeleccionado();
            decimal sueldoMensualInicial = cargoTarifaInicial == null
                ? tarifaDiariaInicial * DiasLaboralesMesTarifa
                : (decimal)Math.Max(
                    0,
                    cargoTarifaInicial.SueldoMensualCLPTipico
                );

            if (tarifaDiariaInicial <= 0 && sueldoMensualInicial > 0)
            {
                tarifaDiariaInicial = Math.Round(
                    sueldoMensualInicial / DiasLaboralesMesTarifa,
                    0
                );
            }

            nudSueldoMensual.Value = Math.Min(
                nudSueldoMensual.Maximum,
                sueldoMensualInicial
            );

            nudTarifa.Value = Math.Min(
                nudTarifa.Maximum,
                tarifaDiariaInicial
            );

            bool sincronizandoTarifas = false;

            Action actualizarEstadoFormaTarifa = () =>
            {
                bool ingresoMensual =
                    string.Equals(
                        cmbFormaTarifa.Text,
                        "Sueldo mensual",
                        StringComparison.OrdinalIgnoreCase
                    );

                nudSueldoMensual.Enabled = ingresoMensual;
                nudSueldoMensual.BackColor = ingresoMensual
                    ? Color.White
                    : Color.FromArgb(242, 242, 242);

                nudTarifa.Enabled = !ingresoMensual;
                nudTarifa.BackColor = ingresoMensual
                    ? Color.FromArgb(242, 242, 242)
                    : Color.White;
            };

            Action sincronizarDesdeMensual = () =>
            {
                if (sincronizandoTarifas)
                {
                    return;
                }

                sincronizandoTarifas = true;
                try
                {
                    decimal diaria = Math.Round(
                        nudSueldoMensual.Value / DiasLaboralesMesTarifa,
                        0
                    );

                    nudTarifa.Value = Math.Min(
                        nudTarifa.Maximum,
                        diaria
                    );
                }
                finally
                {
                    sincronizandoTarifas = false;
                }
            };

            Action sincronizarDesdeDiaria = () =>
            {
                if (sincronizandoTarifas)
                {
                    return;
                }

                sincronizandoTarifas = true;
                try
                {
                    decimal mensual = Math.Round(
                        nudTarifa.Value * DiasLaboralesMesTarifa,
                        0
                    );

                    nudSueldoMensual.Value = Math.Min(
                        nudSueldoMensual.Maximum,
                        mensual
                    );
                }
                finally
                {
                    sincronizandoTarifas = false;
                }
            };

            NumericUpDown nudHorasDia = CrearNumericDialogo(0.1M, 24, 8, 0.5M, 2);
'@

Reemplazar-Exacto "selector de forma de tarifa" $viejoTarifa $nuevoTarifa

# 3) Evitar que la vista previa reemplace la tarifa elegida cada vez.
$viejoAutoTarifa = @'
                CategoriaTrabajador cargo = obtenerCargoSeleccionado();
                if (cargo != null && nudTarifa.Value == 0)
                {
                    nudTarifa.Value = (decimal)Math.Round(cargo.SueldoMensualCLPTipico / 22.0, 0);
                }

                string nombreCompleto = cargo == null
'@

$nuevoAutoTarifa = @'
                CategoriaTrabajador cargo = obtenerCargoSeleccionado();

                string nombreCompleto = cargo == null
'@

Reemplazar-Exacto "autocompletado antiguo de tarifa" $viejoAutoTarifa $nuevoAutoTarifa

# 4) Eventos de sincronizacion.
$viejoEventos = @'
            nudDedicacion.ValueChanged += (s, e) => actualizarPreviewDialogo();
            nudTarifa.ValueChanged += (s, e) => actualizarPreviewDialogo();
            nudHorasDia.ValueChanged += (s, e) => actualizarPreviewDialogo();
'@

$nuevoEventos = @'
            cmbFormaTarifa.SelectedIndexChanged += (s, e) =>
            {
                actualizarEstadoFormaTarifa();

                if (string.Equals(
                    cmbFormaTarifa.Text,
                    "Sueldo mensual",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sincronizarDesdeMensual();
                }
                else
                {
                    sincronizarDesdeDiaria();
                }

                actualizarPreviewDialogo();
            };

            nudDedicacion.ValueChanged += (s, e) => actualizarPreviewDialogo();

            nudSueldoMensual.ValueChanged += (s, e) =>
            {
                if (string.Equals(
                    cmbFormaTarifa.Text,
                    "Sueldo mensual",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sincronizarDesdeMensual();
                    actualizarPreviewDialogo();
                }
            };

            nudTarifa.ValueChanged += (s, e) =>
            {
                if (string.Equals(
                    cmbFormaTarifa.Text,
                    "Tarifa diaria",
                    StringComparison.OrdinalIgnoreCase))
                {
                    sincronizarDesdeDiaria();
                    actualizarPreviewDialogo();
                }
            };

            nudHorasDia.ValueChanged += (s, e) => actualizarPreviewDialogo();
'@

Reemplazar-Exacto "eventos de tarifa" $viejoEventos $nuevoEventos

# 5) Filas del dialogo.
$viejoFilas = @'
            AgregarFilaDialogoCargo(tabla, 0, "Que cargo participa?", cmbCargo);
            AgregarFilaDialogoCargo(tabla, 1, "Nivel", cmbNivel);
            AgregarFilaDialogoCargo(tabla, 2, "Como participa?", cmbModo);
            AgregarFilaDialogoCargo(tabla, 3, "Que variable controla su tiempo?", cmbVariable);
            AgregarFilaDialogoCargo(tabla, 4, "Que dedicacion tiene sobre el proceso? (%)", nudDedicacion);
            AgregarFilaDialogoCargo(tabla, 5, "Tarifa diaria CLP", nudTarifa);
            AgregarFilaDialogoCargo(tabla, 6, "Horas por dia", nudHorasDia);
            AgregarFilaDialogoCargo(tabla, 7, "Rendimiento o factor base", nudFactor);
            AgregarFilaDialogoCargo(tabla, 8, "Formula personalizada", txtFormula);
            AgregarFilaDialogoCargo(tabla, 9, "Que costo deriva?", lblCosto);
'@

$nuevoFilas = @'
            AgregarFilaDialogoCargo(tabla, 0, "Que cargo participa?", cmbCargo);
            AgregarFilaDialogoCargo(tabla, 1, "Nivel", cmbNivel);
            AgregarFilaDialogoCargo(tabla, 2, "Como participa?", cmbModo);
            AgregarFilaDialogoCargo(tabla, 3, "Que variable controla su tiempo?", cmbVariable);
            AgregarFilaDialogoCargo(tabla, 4, "Que dedicacion tiene sobre el proceso? (%)", nudDedicacion);
            AgregarFilaDialogoCargo(tabla, 5, "Como ingresas la tarifa?", cmbFormaTarifa);
            AgregarFilaDialogoCargo(tabla, 6, "Sueldo mensual CLP", nudSueldoMensual);
            AgregarFilaDialogoCargo(tabla, 7, "Tarifa diaria CLP", nudTarifa);
            AgregarFilaDialogoCargo(tabla, 8, "Horas por dia", nudHorasDia);
            AgregarFilaDialogoCargo(tabla, 9, "Rendimiento o factor base", nudFactor);
            AgregarFilaDialogoCargo(tabla, 10, "Formula personalizada", txtFormula);
            AgregarFilaDialogoCargo(tabla, 11, "Que costo deriva?", lblCosto);
'@

Reemplazar-Exacto "filas del dialogo" $viejoFilas $nuevoFilas

# 6) Inicializacion final antes de mostrar.
$viejoInicioPreview = @'
            dialogo.AcceptButton = aceptar;
            dialogo.CancelButton = cancelar;
            actualizarPreviewDialogo();
'@

$nuevoInicioPreview = @'
            dialogo.AcceptButton = aceptar;
            dialogo.CancelButton = cancelar;

            actualizarEstadoFormaTarifa();
            sincronizarDesdeMensual();
            actualizarPreviewDialogo();
'@

Reemplazar-Exacto "inicializacion de tarifa" $viejoInicioPreview $nuevoInicioPreview

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup = "$ruta.backup_tarifa_mensual_diaria_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backup

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Selector Sueldo mensual / Tarifa diaria aplicado correctamente." -ForegroundColor Green
Write-Host "Conversion usada: 22 dias laborales por mes."
Write-Host "Internamente se conserva TarifaDiariaCLP para mantener compatibilidad."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
