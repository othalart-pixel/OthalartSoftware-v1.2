$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

if ($contenido.Contains("ResolverTarifaRealCargosV14(")) {
    Write-Host "La correccion V14 ya esta aplicada." -ForegroundColor Yellow
    exit 0
}

function Obtener-RangoMetodo {
    param(
        [string]$Texto,
        [string]$Firma
    )

    $inicio = $Texto.IndexOf($Firma, [StringComparison]::Ordinal)
    if ($inicio -lt 0) {
        return $null
    }

    $llave = $Texto.IndexOf("{", $inicio, [StringComparison]::Ordinal)
    if ($llave -lt 0) {
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
        Texto = $Texto.Substring(
            $inicio,
            $fin - $inicio + 1
        )
    }
}

$firma =
    "        private double ObtenerTarifaHoraEfectivaV13("

$rango =
    Obtener-RangoMetodo $contenido $firma

if ($null -eq $rango) {
    throw "No se encontro ObtenerTarifaHoraEfectivaV13."
}

$nuevoMetodo = @'
        private double ObtenerTarifaHoraEfectivaV13(
            RequerimientoProduccionInterna requerimiento,
            double horasAnteriores,
            double costoAnterior)
        {
            if (requerimiento == null)
            {
                return 0.0;
            }

            // ResolverTarifaRealCargosV14:
            // La tarifa correcta se obtiene desde cada cargo participante,
            // no desde un costo histórico ya escalado.
            double tarifaCargos =
                ResolverTarifaRealCargosV14(
                    requerimiento
                );

            if (tarifaCargos > 0.0)
            {
                return tarifaCargos;
            }

            if (requerimiento.TarifaDiaCargoCLP > 0.0)
            {
                return
                    requerimiento.TarifaDiaCargoCLP / 8.0;
            }

            if (requerimiento.SueldoMensualCargoCLP > 0.0)
            {
                return
                    requerimiento.SueldoMensualCargoCLP /
                    22.0 /
                    8.0;
            }

            // Ultimo respaldo solamente para datos antiguos que no tengan
            // cargo ni tarifa. Nunca se usa si cargos.json resolvio algo.
            if (horasAnteriores > 0.0 &&
                costoAnterior > 0.0)
            {
                return
                    costoAnterior / horasAnteriores;
            }

            return 0.0;
        }

        private double ResolverTarifaRealCargosV14(
            RequerimientoProduccionInterna requerimiento)
        {
            if (requerimiento == null ||
                string.IsNullOrWhiteSpace(
                    requerimiento.CargoSugerido))
            {
                return 0.0;
            }

            double tarifaHoraTotal = 0.0;

            string contexto =
                (requerimiento.EtapaSugerida ?? "") +
                " " +
                (requerimiento.BloqueProductivo ?? "");

            IEnumerable<string> cargos =
                requerimiento.CargoSugerido
                    .Split(
                        new[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Select(c => c.Trim())
                    .Where(c =>
                        !string.IsNullOrWhiteSpace(c))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase
                    );

            foreach (string textoCargo in cargos)
            {
                string nombre =
                    textoCargo.Trim();

                string nivel =
                    requerimiento.NivelCargoSugerido;

                int indiceNivel =
                    nombre.LastIndexOf(
                        " (",
                        StringComparison.Ordinal
                    );

                if (indiceNivel > 0 &&
                    nombre.EndsWith(
                        ")",
                        StringComparison.Ordinal))
                {
                    nivel = nombre.Substring(
                        indiceNivel + 2,
                        nombre.Length -
                        indiceNivel -
                        3
                    ).Trim();

                    nombre = nombre.Substring(
                        0,
                        indiceNivel
                    ).Trim();
                }

                CategoriaTrabajador cargo =
                    BibliotecaCargosJsonService.BuscarCargo(
                        nombre,
                        contexto,
                        string.IsNullOrWhiteSpace(nivel)
                            ? "típico"
                            : nivel
                    );

                if (cargo == null ||
                    cargo.SueldoMensualCLPTipico <= 0.0)
                {
                    continue;
                }

                tarifaHoraTotal +=
                    cargo.SueldoMensualCLPTipico /
                    22.0 /
                    8.0;
            }

            return tarifaHoraTotal;
        }
'@

$contenido =
    $contenido.Substring(0, $rango.Inicio) +
    $nuevoMetodo +
    $contenido.Substring($rango.Fin + 1)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_tarifas_reales_v14_" +
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
Write-Host "Correccion V14 aplicada." -ForegroundColor Green
Write-Host "La tarifa se resolvera desde todos los cargos participantes en cargos.json."
Write-Host "Ejemplo esperado para 1 h: 5.682 + 6.818 = 12.500 CLP."
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
