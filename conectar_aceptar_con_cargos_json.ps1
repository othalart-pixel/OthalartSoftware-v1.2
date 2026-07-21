$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabEcuaciones.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabEcuaciones.cs. Ejecuta este script desde la carpeta raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

# ---------------------------------------------------------
# 1. Conectar el boton Aceptar con cargos.json
# ---------------------------------------------------------
# Buscamos la asignacion final del trabajo dentro del dialogo.
$ancla = '            trabajo.TarifaDiariaCLP = (double)nudTarifa.Value;'

if (-not $contenido.Contains($ancla)) {
    throw "No se encontro la asignacion de TarifaDiariaCLP en el dialogo. Sube la version actual o revisa que el parche anterior este aplicado."
}

$inyeccion = @'
            trabajo.TarifaDiariaCLP = (double)nudTarifa.Value;

            CategoriaTrabajador cargoGlobalSeleccionado = obtenerCargoSeleccionado();
            if (cargoGlobalSeleccionado != null)
            {
                double sueldoMensualElegido = (double)nudSueldoMensual.Value;

                if (sueldoMensualElegido <= 0.0)
                {
                    sueldoMensualElegido = (double)nudTarifa.Value * 22.0;
                }

                ActualizarSueldoCargoGlobalDesdeEcuacion(
                    cargoGlobalSeleccionado.NombreCompleto,
                    cargoGlobalSeleccionado.Nombre,
                    cargoGlobalSeleccionado.Nivel,
                    sueldoMensualElegido
                );

                cargoGlobalSeleccionado.SueldoMensualCLPTipico =
                    sueldoMensualElegido;
            }
'@

$contenido = $contenido.Replace($ancla, $inyeccion)

# ---------------------------------------------------------
# 2. Agregar helper antes de CrearNumericDialogo
# ---------------------------------------------------------
$firmaHelper = "        private void ActualizarSueldoCargoGlobalDesdeEcuacion("
$anclaMetodo = "        private NumericUpDown CrearNumericDialogo("

if (-not $contenido.Contains($firmaHelper)) {
    $indice = $contenido.IndexOf($anclaMetodo, [StringComparison]::Ordinal)

    if ($indice -lt 0) {
        throw "No se encontro CrearNumericDialogo para insertar el helper."
    }

    $helper = @'
        private void ActualizarSueldoCargoGlobalDesdeEcuacion(
            string nombreCompleto,
            string nombreBase,
            string nivel,
            double sueldoMensualCLP)
        {
            if (sueldoMensualCLP < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sueldoMensualCLP),
                    "El sueldo mensual no puede ser negativo."
                );
            }

            string raizBibliotecas = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Bibliotecas"
            );

            List<string> candidatos = new List<string>();

            if (Directory.Exists(raizBibliotecas))
            {
                candidatos.AddRange(
                    Directory.GetFiles(
                        raizBibliotecas,
                        "cargos.json",
                        SearchOption.AllDirectories
                    )
                );
            }

            string carpetaProyecto = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo actual = new DirectoryInfo(carpetaProyecto);

            for (int i = 0; i < 6 && actual != null; i++, actual = actual.Parent)
            {
                string posible = Path.Combine(
                    actual.FullName,
                    "Bibliotecas"
                );

                if (Directory.Exists(posible))
                {
                    candidatos.AddRange(
                        Directory.GetFiles(
                            posible,
                            "cargos.json",
                            SearchOption.AllDirectories
                        )
                    );
                }
            }

            string rutaCargos = candidatos
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(r =>
                    r.IndexOf(
                        Path.Combine("Bibliotecas", "default"),
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(rutaCargos) ||
                !File.Exists(rutaCargos))
            {
                throw new FileNotFoundException(
                    "No se encontro cargos.json dentro de Bibliotecas."
                );
            }

            string json = File.ReadAllText(rutaCargos);
            System.Text.Json.Nodes.JsonNode raiz =
                System.Text.Json.Nodes.JsonNode.Parse(json);

            if (raiz == null)
            {
                throw new InvalidOperationException(
                    "cargos.json no contiene un JSON valido."
                );
            }

            bool actualizado = ActualizarNodoCargoRecursivo(
                raiz,
                nombreCompleto,
                nombreBase,
                nivel,
                sueldoMensualCLP
            );

            if (!actualizado)
            {
                throw new InvalidOperationException(
                    "No se encontro el cargo '" +
                    nombreCompleto +
                    "' en cargos.json."
                );
            }

            string backup = rutaCargos +
                ".backup_sueldo_" +
                DateTime.Now.ToString("yyyyMMdd_HHmmss");

            File.Copy(rutaCargos, backup, true);

            System.Text.Json.JsonSerializerOptions opciones =
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                };

            File.WriteAllText(
                rutaCargos,
                raiz.ToJsonString(opciones),
                new System.Text.UTF8Encoding(true)
            );
        }

        private bool ActualizarNodoCargoRecursivo(
            System.Text.Json.Nodes.JsonNode nodo,
            string nombreCompleto,
            string nombreBase,
            string nivel,
            double sueldoMensualCLP)
        {
            if (nodo is System.Text.Json.Nodes.JsonObject objeto)
            {
                string nombreNodo = LeerTextoNodoCargo(
                    objeto,
                    "NombreCompleto",
                    "nombreCompleto",
                    "Nombre",
                    "nombre"
                );

                string nivelNodo = LeerTextoNodoCargo(
                    objeto,
                    "Nivel",
                    "nivel"
                );

                bool coincideNombre =
                    (!string.IsNullOrWhiteSpace(nombreCompleto) &&
                     string.Equals(
                         nombreNodo,
                         nombreCompleto,
                         StringComparison.OrdinalIgnoreCase
                     )) ||
                    (!string.IsNullOrWhiteSpace(nombreBase) &&
                     !string.IsNullOrWhiteSpace(nombreNodo) &&
                     nombreNodo.IndexOf(
                         nombreBase,
                         StringComparison.OrdinalIgnoreCase
                     ) >= 0);

                bool coincideNivel =
                    string.IsNullOrWhiteSpace(nivel) ||
                    string.IsNullOrWhiteSpace(nivelNodo) ||
                    string.Equals(
                        nivelNodo,
                        nivel,
                        StringComparison.OrdinalIgnoreCase
                    );

                if (coincideNombre && coincideNivel)
                {
                    string propiedadSueldo = objeto.ContainsKey(
                        "SueldoMensualCLPTipico"
                    )
                        ? "SueldoMensualCLPTipico"
                        : objeto.ContainsKey("sueldoMensualCLPTipico")
                            ? "sueldoMensualCLPTipico"
                            : "SueldoMensualCLPTipico";

                    objeto[propiedadSueldo] = sueldoMensualCLP;

                    if (objeto.ContainsKey("TarifaDiariaCLP"))
                    {
                        objeto["TarifaDiariaCLP"] =
                            Math.Round(sueldoMensualCLP / 22.0, 2);
                    }

                    if (objeto.ContainsKey("tarifaDiariaCLP"))
                    {
                        objeto["tarifaDiariaCLP"] =
                            Math.Round(sueldoMensualCLP / 22.0, 2);
                    }

                    return true;
                }

                foreach (KeyValuePair<string, System.Text.Json.Nodes.JsonNode> par
                    in objeto.ToList())
                {
                    if (par.Value != null &&
                        ActualizarNodoCargoRecursivo(
                            par.Value,
                            nombreCompleto,
                            nombreBase,
                            nivel,
                            sueldoMensualCLP))
                    {
                        return true;
                    }
                }
            }
            else if (nodo is System.Text.Json.Nodes.JsonArray arreglo)
            {
                foreach (System.Text.Json.Nodes.JsonNode hijo in arreglo)
                {
                    if (hijo != null &&
                        ActualizarNodoCargoRecursivo(
                            hijo,
                            nombreCompleto,
                            nombreBase,
                            nivel,
                            sueldoMensualCLP))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string LeerTextoNodoCargo(
            System.Text.Json.Nodes.JsonObject objeto,
            params string[] propiedades)
        {
            foreach (string propiedad in propiedades)
            {
                if (!objeto.ContainsKey(propiedad) ||
                    objeto[propiedad] == null)
                {
                    continue;
                }

                try
                {
                    return objeto[propiedad].GetValue<string>() ?? "";
                }
                catch
                {
                    return objeto[propiedad].ToString();
                }
            }

            return "";
        }

'@

    $contenido =
        $contenido.Substring(0, $indice) +
        $helper +
        $contenido.Substring($indice)
}

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backupCodigo = "$ruta.backup_guardar_cargos_json_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $ruta $backupCodigo

$utf8ConBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($ruta, $contenido, $utf8ConBom)

Write-Host ""
Write-Host "Aceptar ahora actualiza cargos.json global." -ForegroundColor Green
Write-Host "El JSON crea su propio backup antes de guardar."
Write-Host "Backup del codigo: $backupCodigo"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
