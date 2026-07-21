$ErrorActionPreference = "Stop"

$rutaEditor = Join-Path (Get-Location) "UI\Form1.TabEcuacionesAmigable.cs"
$rutaRuntime = Join-Path (Get-Location) "Services\EcuacionProductivaRuntimeService.cs"

foreach ($ruta in @($rutaEditor, $rutaRuntime)) {
    if (-not (Test-Path $ruta)) {
        throw "No se encontro $ruta. Ejecuta este script desde la carpeta raiz del proyecto."
    }
}

$editor = [System.IO.File]::ReadAllText($rutaEditor)
$runtime = [System.IO.File]::ReadAllText($rutaRuntime)
$editorOriginal = $editor
$runtimeOriginal = $runtime

# =========================================================
# 1. OCULTAR METODOS QUE EL RUNTIME TODAVIA NO SOPORTA
# =========================================================
$patronMetodos = '(?ms)cmbMetodoCalculoSimpleEcuacion\.Items\.AddRange\(new object\[\]\s*\{\s*.*?\s*\}\);'

$nuevosMetodos = @'
cmbMetodoCalculoSimpleEcuacion.Items.AddRange(new object[]
            {
                new OpcionMetodoSimpleEcuacion
                {
                    Metodo = MetodoCalculoProceso.PorCapacidad,
                    Nombre = "Cantidad según capacidad del cargo"
                },
                new OpcionMetodoSimpleEcuacion
                {
                    Metodo = MetodoCalculoProceso.Manual,
                    Nombre = "Horas declaradas manualmente en el proyecto"
                },
                new OpcionMetodoSimpleEcuacion
                {
                    Metodo = MetodoCalculoProceso.NoDefinido,
                    Nombre = "Sin definir"
                }
            });
'@

$editorNuevo = [regex]::Replace($editor, $patronMetodos, $nuevosMetodos, 1)

if ($editorNuevo -eq $editor) {
    throw "No se encontro la lista de metodos del editor simple. No se modifico nada."
}

$editor = $editorNuevo

# =========================================================
# 2. PREVISUALIZACION DE BIBLIOTECA PARA METODO MANUAL
# No intenta calcular sin un proyecto.
# =========================================================
$anclaPrueba = @'
            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;

            DeterminarEntradaPrueba(efectiva, out double cantidad, out string unidad);
'@

$reemplazoPrueba = @'
            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;

            if (efectiva.MetodoCalculo == MetodoCalculoProceso.Manual)
            {
                resultado.Advertencias.Add(
                    "Esta regla no calcula horas en la biblioteca. " +
                    "Las horas deben declararse manualmente dentro del proyecto."
                );
                return resultado;
            }

            DeterminarEntradaPrueba(efectiva, out double cantidad, out string unidad);
'@

if (-not $runtime.Contains($anclaPrueba)) {
    throw "No se encontro el bloque de ProbarEcuacion esperado. No se modifico nada."
}
$runtime = $runtime.Replace($anclaPrueba, $reemplazoPrueba)

# =========================================================
# 3. CONECTAR HORAS MANUALES EN EVALUAR REQUERIMIENTO
# Usa HorasEstandar como entrada declarada del proyecto.
# =========================================================
$anclaRequerimiento = @'
            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;
            resultado.CantidadPrueba = requerimiento.Cantidad;
            resultado.UnidadPrueba = requerimiento.Unidad ?? "";

            if (resultado.CantidadPrueba <= 0.0)
'@

$reemplazoRequerimiento = @'
            EcuacionProductivaDefinicion efectiva = ResolverHerenciaSimple(ecuacion, biblioteca);
            resultado.Clave = efectiva.Clave;
            resultado.Nombre = efectiva.NombreVisible;
            resultado.FormulaMadre = efectiva.EcuacionBase;
            resultado.CantidadPrueba = requerimiento.Cantidad;
            resultado.UnidadPrueba = requerimiento.Unidad ?? "";

            if (efectiva.MetodoCalculo == MetodoCalculoProceso.Manual)
            {
                return EvaluarHorasDeclaradasManualmente(
                    efectiva,
                    requerimiento,
                    resultado
                );
            }

            if (resultado.CantidadPrueba <= 0.0)
'@

if (-not $runtime.Contains($anclaRequerimiento)) {
    throw "No se encontro el bloque de EvaluarRequerimiento esperado. No se modifico nada."
}
$runtime = $runtime.Replace($anclaRequerimiento, $reemplazoRequerimiento)

# =========================================================
# 4. HELPER DE CALCULO MANUAL
# =========================================================
$anclaHelper = @'
        private static ResultadoCargo EvaluarCargo(
'@

if (-not $runtime.Contains($anclaHelper)) {
    throw "No se encontro el punto para insertar el evaluador manual."
}

$helper = @'
        private static ResultadoPrueba EvaluarHorasDeclaradasManualmente(
            EcuacionProductivaDefinicion ecuacion,
            RequerimientoProduccionInterna requerimiento,
            ResultadoPrueba resultado)
        {
            double horasDeclaradas = requerimiento.HorasEstandar > 0.0
                ? requerimiento.HorasEstandar
                : requerimiento.HorasMinimas > 0.0
                    ? requerimiento.HorasMinimas
                    : requerimiento.HorasHolgura;

            if (horasDeclaradas <= 0.0)
            {
                resultado.Advertencias.Add(
                    "Pendiente: ingresa las horas destinadas al equipo en el proyecto."
                );
                resultado.DiasTecnicos = 0.0;
                resultado.CostoCLP = 0.0;
                return resultado;
            }

            List<CargoVector> vector = ObtenerVectorCargos(ecuacion);

            if (vector.Count == 0 && !string.IsNullOrWhiteSpace(requerimiento.CargoSugerido))
            {
                vector = ParsearVectorCargos(requerimiento.CargoSugerido);
            }

            if (vector.Count == 0)
            {
                resultado.Errores.Add(
                    "No hay cargos participantes definidos para distribuir las horas manuales."
                );
                return resultado;
            }

            List<CategoriaTrabajador> cargos = Cargos.CrearBibliotecaCompleta();

            foreach (CargoVector cargoVector in vector)
            {
                ResultadoCargo item = new ResultadoCargo
                {
                    CargoSolicitado = cargoVector.Cargo,
                    Dedicacion = cargoVector.Dedicacion,
                    CantidadPrueba = horasDeclaradas,
                    UnidadPrueba = "horas",
                    RequiereRendimientoProductivo = false,
                    RendimientoExiste = true
                };

                CategoriaTrabajador cargo = BuscarCargo(cargos, cargoVector.Cargo);
                if (cargo == null)
                {
                    item.CargoExiste = false;
                    item.Diagnostico = "Cargo no encontrado en biblioteca de cargos.";
                    resultado.Cargos.Add(item);
                    resultado.Errores.Add(
                        ecuacion.Clave + ": cargo no encontrado: " + cargoVector.Cargo
                    );
                    continue;
                }

                item.CargoExiste = true;
                item.CargoResuelto = cargo.NombreCompleto;
                item.TarifaDiaCLP = cargo.SueldoMensualCLPTipico / 22.0;
                item.HorasPorDia = 8.0;
                item.TarifaHoraCLP = item.TarifaDiaCLP / item.HorasPorDia;

                // La dedicacion representa qué fracción de las horas declaradas
                // corresponde a este cargo.
                item.HorasTecnicas = horasDeclaradas * cargoVector.Dedicacion;
                item.DiasTecnicos = item.HorasTecnicas / item.HorasPorDia;
                item.TarifaDiaPonderadaCLP =
                    item.TarifaDiaCLP * cargoVector.Dedicacion;
                item.TarifaHoraPonderadaCLP =
                    item.TarifaHoraCLP * cargoVector.Dedicacion;

                // No se vuelve a ponderar la tarifa: las horas ya fueron
                // distribuidas mediante Dedicacion.
                item.CostoCLP = item.HorasTecnicas * item.TarifaHoraCLP;
                item.Diagnostico =
                    "Horas declaradas manualmente en el proyecto.";

                resultado.Cargos.Add(item);
            }

            resultado.DiasTecnicos = resultado.Cargos
                .Where(c => c.CargoExiste)
                .Select(c => c.DiasTecnicos)
                .DefaultIfEmpty(0.0)
                .Max();

            resultado.CostoCLP = resultado.Cargos.Sum(c => c.CostoCLP);
            resultado.CantidadPrueba = horasDeclaradas;
            resultado.UnidadPrueba = "horas";

            return resultado;
        }

'@

$runtime = $runtime.Replace($anclaHelper, $helper + $anclaHelper)

# =========================================================
# 5. VALIDACION: MANUAL NO REQUIERE FORMULA VISIBLE
# =========================================================
$viejoValidacion = @'
                if (e.Activa && string.IsNullOrWhiteSpace(e.FormulaReferencia))
                {
                    diagnostico.Advertencias.Add(clave + ": formula/calculo visible no definido.");
                }
'@

$nuevoValidacion = @'
                if (e.Activa &&
                    e.MetodoCalculo != MetodoCalculoProceso.Manual &&
                    string.IsNullOrWhiteSpace(e.FormulaReferencia))
                {
                    diagnostico.Advertencias.Add(
                        clave + ": formula/calculo visible no definido."
                    );
                }
'@

if ($runtime.Contains($viejoValidacion)) {
    $runtime = $runtime.Replace($viejoValidacion, $nuevoValidacion)
}

# =========================================================
# GUARDAR CON BACKUPS
# =========================================================
$marca = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupEditor = "$rutaEditor.backup_metodos_reales_$marca"
$backupRuntime = "$rutaRuntime.backup_horas_manual_runtime_$marca"

Copy-Item $rutaEditor $backupEditor
Copy-Item $rutaRuntime $backupRuntime

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($rutaEditor, $editor, $utf8ConBom)
[System.IO.File]::WriteAllText($rutaRuntime, $runtime, $utf8ConBom)

Write-Host ""
Write-Host "Metodos no funcionales ocultados y horas manuales conectadas." -ForegroundColor Green
Write-Host ""
Write-Host "Metodos visibles:"
Write-Host " - Cantidad segun capacidad del cargo"
Write-Host " - Horas declaradas manualmente en el proyecto"
Write-Host " - Sin definir"
Write-Host ""
Write-Host "Backups:"
Write-Host " - $backupEditor"
Write-Host " - $backupRuntime"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
