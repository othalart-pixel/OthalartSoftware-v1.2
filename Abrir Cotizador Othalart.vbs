Set shell = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")

root = fso.GetParentFolderName(WScript.ScriptFullName)
exe = root & "\bin\Release\net8.0-windows\OthalartCotizadorDev.exe"
project = root & "\OthalartSoftware v1.0.csproj"
logFile = root & "\build-release.log"

shell.CurrentDirectory = root
buildCmd = "cmd /c dotnet build """ & project & """ --no-restore -c Release /clp:ErrorsOnly > """ & logFile & """ 2>&1"
exitCode = shell.Run(buildCmd, 0, True)

If exitCode <> 0 Then
    MsgBox "No se pudo compilar la version nueva. No se abrira un ejecutable antiguo." & vbCrLf & vbCrLf & "Detalle: " & logFile, 48, "Cotizador Othalart"
    WScript.Quit exitCode
End If

If fso.FileExists(exe) Then
    shell.CurrentDirectory = fso.GetParentFolderName(exe)
    ' 1 = ventana normal. Con 0 Windows iniciaba la aplicación completamente oculta.
    shell.Run """" & exe & """", 1, False
Else
    MsgBox "La compilacion termino, pero no se encontro el ejecutable: " & exe, 48, "Cotizador Othalart"
End If
