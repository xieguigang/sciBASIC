#Region "Microsoft.VisualBasic::e2f561de20747fa167eaa217bb586262, Microsoft.VisualBasic.Core\Net\HTTP\wget.vb"

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

    '     Class wget
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DownloadProcess, ReportRequest, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net

Namespace Net.Http

    ''' <summary>
    ''' 命令行下的下载程序组件
    ''' </summary>
    Public Class wget

        Dim WithEvents task As wgetTask
        Dim cursorTop%

        Sub New(url$, save$)
            task = New wgetTask(url, save)
            cursorTop = Console.CursorTop
        End Sub

        Public Sub Run()
            Call task.StartTask()

            If Not task.isDownloading Then
                Call task.Dispose()

                Call Console.WriteLine()
                Call Console.WriteLine($"{Now.ToString} ({task.downloadSpeed} KB/s) - '{task.saveFile.FileName}' saved [{task.saveFile.FileLength}]")
                Call Console.WriteLine()
            End If
        End Sub

        Private Sub DownloadProcess(wget As wgetTask, percentage As Double) Handles task.DownloadProcess
            Console.CursorTop = cursorTop
            Console.CursorLeft = 1
            Console.Write(New String(" "c, Console.BufferWidth))
            Console.CursorLeft = 1
            Console.WriteLine(wget.ToString)
        End Sub

        Private Sub ReportRequest(req As WebRequest) Handles task.ReportRequest
            Dim domain As New DomainName(task.url)

            Call Console.WriteLine($"--{Now.ToString}--  {task.url}")
            Call Console.WriteLine($"     => '{task.saveFile.FileName}'")
            Call Console.WriteLine($"Resolving {domain} ({domain})... {req.RequestUri.Host}")
            Call Console.WriteLine($"==> SIZE {task.saveFile.FileName} ... {req.ContentLength}")
            Call Console.WriteLine($"==> CONTENT-TYPE ... {req.ContentType}")
            Call Console.WriteLine($"Length: {req.ContentLength} ()")
            Call Console.WriteLine()

            cursorTop = Console.CursorTop
        End Sub
    End Class
End Namespace
