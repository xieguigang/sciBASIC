#Region "Microsoft.VisualBasic::48d1b7e9aa058516291f6dfd4b5353b9, vs_solutions\installer\bootstrap\BootstrapLoader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Class BootstrapLoader
    ' 
    '     Sub: BootstrapLoader_Load, timer_Tick
    ' 
    ' /********************************************************************************/

#End Region

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
