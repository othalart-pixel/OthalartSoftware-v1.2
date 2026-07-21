$ErrorActionPreference = "Stop"

$raiz = Get-Location
$cmd = Join-Path $raiz "Abrir Cotizador Othalart.cmd"

if (-not (Test-Path $cmd)) {
    throw "No se encontro 'Abrir Cotizador Othalart.cmd'. Ejecuta este hotfix desde la carpeta raiz del proyecto."
}

$backup = "$cmd.backup_dotnet_host_" + (Get-Date -Format "yyyyMMdd_HHmmss")
Copy-Item $cmd $backup -Force

$contenido = @'
@echo off
setlocal EnableExtensions EnableDelayedExpansion

title Cotizador Othalart
cd /d "%~dp0"

echo.
echo ==========================================
echo        COTIZADOR OTHALART
echo ==========================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: No se encontro dotnet.
    echo Instala o repara el SDK de .NET.
    echo.
    pause
    exit /b 1
)

set "CSPROJ="

if exist "OthalartSoftware v1.0.csproj" (
    set "CSPROJ=OthalartSoftware v1.0.csproj"
) else (
    for %%F in (*.csproj) do (
        if not defined CSPROJ set "CSPROJ=%%F"
    )
)

if not defined CSPROJ (
    echo ERROR: No se encontro ningun archivo .csproj.
    echo.
    pause
    exit /b 1
)

echo Proyecto detectado:
echo !CSPROJ!
echo.

echo Compilando...
dotnet build "!CSPROJ!" -c Debug --nologo

if errorlevel 1 (
    echo.
    echo ==========================================
    echo ERROR DE COMPILACION
    echo ==========================================
    pause
    exit /b 1
)

set "DLL="

if exist "bin\Debug\net8.0-windows\OthalartCotizadorDev.dll" (
    set "DLL=bin\Debug\net8.0-windows\OthalartCotizadorDev.dll"
)

if not defined DLL (
    for /r "bin\Debug" %%F in (*.dll) do (
        echo %%~nxF | findstr /I "OthalartCotizadorDev.dll" >nul
        if not errorlevel 1 (
            if not defined DLL set "DLL=%%F"
        )
    )
)

if not defined DLL (
    echo.
    echo ERROR: Se compilo el proyecto pero no se encontro OthalartCotizadorDev.dll.
    echo.
    pause
    exit /b 1
)

echo.
echo Abriendo mediante el host firmado de .NET:
echo !DLL!
echo.

dotnet "!DLL!"

if errorlevel 1 (
    echo.
    echo ERROR: La aplicacion termino con codigo !errorlevel!.
    echo.
    pause
    exit /b 1
)

exit /b 0
'@

$encoding = New-Object System.Text.UTF8Encoding($false)

[System.IO.File]::WriteAllText(
    $cmd,
    $contenido,
    $encoding
)

Write-Host ""
Write-Host "HOTFIX APLICADO CORRECTAMENTE" -ForegroundColor Green
Write-Host ""
Write-Host "El lanzador ya no ejecutara el .exe bloqueado."
Write-Host "Ahora abrira OthalartCotizadorDev.dll mediante dotnet.exe."
Write-Host ""
Write-Host "Backup: $backup"
Write-Host "Abre con doble clic: Abrir Cotizador Othalart.cmd"
