$ErrorActionPreference = "Stop"

$ruta = Join-Path (Get-Location) "UI\Form1.TabProyecto.cs"

if (-not (Test-Path $ruta)) {
    throw "No se encontro UI\Form1.TabProyecto.cs. Ejecuta este hotfix desde la raiz del proyecto."
}

$contenido = [System.IO.File]::ReadAllText($ruta)
$original = $contenido

$patron = @'
                if (copia.BriefProducto == null)
                {
                    copia.BriefProducto =
                        new BriefProducto();
                }
'@

$reemplazo = @'
                if (copia.BriefProducto == null)
                {
                    System.Reflection.PropertyInfo propiedadBrief =
                        typeof(Cotizacion).GetProperty(
                            nameof(Cotizacion.BriefProducto)
                        );

                    if (propiedadBrief == null)
                    {
                        return null;
                    }

                    object nuevoBrief =
                        Activator.CreateInstance(
                            propiedadBrief.PropertyType
                        );

                    if (nuevoBrief == null)
                    {
                        return null;
                    }

                    propiedadBrief.SetValue(
                        copia,
                        nuevoBrief
                    );
                }
'@

if (-not $contenido.Contains($patron)) {
    throw "No se encontro el bloque con new BriefProducto(). Puede que ya se haya corregido o el archivo sea distinto."
}

$contenido = $contenido.Replace(
    $patron,
    $reemplazo
)

if ($contenido -eq $original) {
    throw "No se generaron cambios."
}

$backup =
    "$ruta.backup_fix_brief_tipo_" +
    (Get-Date -Format "yyyyMMdd_HHmmss")

Copy-Item $ruta $backup -Force

$utf8ConBom =
    New-Object System.Text.UTF8Encoding($true)

[System.IO.File]::WriteAllText(
    $ruta,
    $contenido,
    $utf8ConBom
)

Write-Host ""
Write-Host "HOTFIX APLICADO CORRECTAMENTE" -ForegroundColor Green
Write-Host ""
Write-Host "Se elimino la referencia inexistente a BriefProducto."
Write-Host "Ahora se usa el tipo real declarado en Cotizacion.BriefProducto."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host ""
Write-Host 'Compila con: dotnet build "OthalartSoftware v1.0.csproj" -c Debug'
