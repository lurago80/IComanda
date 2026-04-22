' IComanda - Launcher oculto
' Duplo clique neste arquivo: inicia tudo sem abrir nenhuma janela

Dim oShell, scriptDir
Set oShell = CreateObject("WScript.Shell")

scriptDir = Left(WScript.ScriptFullName, InStrRev(WScript.ScriptFullName, "\"))
Dim ps1File
ps1File = scriptDir & "iniciar-oculto.ps1"

' WindowStyle 0 = completamente oculto
' bWaitOnReturn False = nao travar o VBS esperando o PS1 terminar
oShell.Run "powershell.exe -WindowStyle Hidden -ExecutionPolicy Bypass -NonInteractive -File """ & ps1File & """", 0, False

Set oShell = Nothing
