' IComanda - Parar todos os servicos (sem janela)
' Duplo clique: encerra backend e frontend silenciosamente

Dim oShell
Set oShell = CreateObject("WScript.Shell")

' Encerrar dotnet (backend)
oShell.Run "taskkill /F /IM dotnet.exe", 0, True

' Encerrar node (frontend - npm start)
oShell.Run "taskkill /F /IM node.exe", 0, True

' Notificacao simples via PowerShell
Dim psCmd
psCmd = "powershell.exe -WindowStyle Hidden -Command """ & _
        "Add-Type -AssemblyName System.Windows.Forms;" & _
        "$b=New-Object System.Windows.Forms.NotifyIcon;" & _
        "$b.Icon=[System.Drawing.SystemIcons]::Application;" & _
        "$b.BalloonTipIcon='Info';" & _
        "$b.BalloonTipTitle='iComanda';" & _
        "$b.BalloonTipText='Todos os servicos foram encerrados.';" & _
        "$b.Visible=$true;" & _
        "$b.ShowBalloonTip(4000);" & _
        "Start-Sleep 1;" & _
        "$b.Dispose()" & """"

oShell.Run psCmd, 0, False

Set oShell = Nothing
