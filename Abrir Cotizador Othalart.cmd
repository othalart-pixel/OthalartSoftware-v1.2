@echo off
setlocal
set "ROOT=%~dp0"
set "LAUNCHER=%ROOT%Abrir Cotizador Othalart.vbs"

if exist "%LAUNCHER%" (
  wscript.exe "%LAUNCHER%"
  exit /b 0
)

start "" "%ROOT%bin\Release\net8.0-windows\OthalartCotizadorDev.exe"
