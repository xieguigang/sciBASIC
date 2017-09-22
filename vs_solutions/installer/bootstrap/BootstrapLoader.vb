Imports System.Threading

Public Class BootstrapLoader

    Dim WithEvents timer As New System.Windows.Forms.Timer

    Private Sub BootstrapLoader_Load(sender As Object, e As EventArgs) Handles Me.Load
        Opacity = 0
        timer.Interval = 1
        timer.Start()
    End Sub

    Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
        If Opacity < 1 Then
            Opacity += 0.05
        Else
            ' 释放安装程序资源
            Dim PID = Process.GetCurrentProcess.Id
            Dim DIR$ = FileIO.FileSystem.GetTempFileName & "-" & PID
            Dim temp$ = DIR & ".zip"
            Dim installer$ = DIR & "/installer.exe"

            Call FileSystem.MkDir(DIR)
            Call FileIO.FileSystem.WriteAllBytes(temp, My.Resources.installer, append:=False)
            Call GZip.ImprovedExtractToDirectory(temp, DIR, Overwrite.Always)

            ' 必须要先于启动安装程序之前停掉计时器，否则会一直请求管理员权限，启动多个任务进程的
            timer.Stop()

            Call Thread.Sleep(1500)
            Call Close()
            Call Process.Start(installer)

            timer.Dispose()
        End If
    End Sub
End Class
