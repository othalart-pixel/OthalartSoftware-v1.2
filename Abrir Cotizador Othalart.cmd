@echo off
setlocal
pushd "%~dp0"

set "PROJECT=OthalartSoftware v1.0.csproj"
set "RUN_DIR=%~dp0bin\Debug\net8.0-windows"
set "APP=%RUN_DIR%\OthalartCotizadorDev.dll"
set "OTHALART_PROJECT_ROOT=%~dp0"

echo.
echo Preparando Cotizador Othalart...
echo.

dotnet build "%PROJECT%" --no-restore -c Debug /clp:ErrorsOnly

if errorlevel 1 (
  echo.
  echo No se pudo compilar la app. Revisa el error anterior.
  echo.
  pause
  popd
  exit /b 1
)

if not exist "%APP%" (
  echo No existe la app publicada en:
  echo %APP%
  echo.
  echo Ejecuta primero:
  echo dotnet build "OthalartSoftware v1.0.csproj" --no-restore -c Debug
  pause
  popd
  exit /b 1
)

pushd "%RUN_DIR%"
dotnet "%APP%"
popd
popd
