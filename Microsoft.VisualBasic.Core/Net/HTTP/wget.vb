#Region "Microsoft.VisualBasic::d64a23d994ed18fccdf8e5548a97dcbb, Microsoft.VisualBasic.Core\Net\HTTP\wget.vb"

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
    ' 
    '         Function: Download, ToString
    ' 
    '         Sub: ClearLine, clearOutput, DownloadProcess, ReportRequest, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net
Imports Microsoft.VisualBasic.Language

Namespace Net.Http

    ''' <summary>
    ''' 命令行下的下载程序组件
    ''' </summary>
    Public Class wget

        Dim WithEvents task As wgetTask
        Dim cursorTop%
        Dim originalTop%

        ''' <summary>
        ''' Create a new file download task
        ''' </summary>
        ''' <param name="url">The remote resource to download.</param>
        ''' <param name="save">The file save location</param>
        Sub New(url$, save$, headers As Dictionary(Of String, String))
            task = New wgetTask(url, save, headers)
            cursorTop = Console.CursorTop
            originalTop = Console.CursorTop
        End Sub

        ''' <summary>
        ''' Run the file download task
        ''' </summary>
        Public Sub Run()
            Call task.StartTask()

            If Not task.isDownloading Then
                Call task.Dispose()

                Call Console.WriteLine()
                Call Console.WriteLine($"{Now} ({StringFormats.Lanudry(task.downloadSpeed)}/s) - '{task.saveFile.FileName}' saved [{task.saveFile.FileLength}]")
                Call Console.WriteLine()
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return task.url
        End Function

        Private Sub DownloadProcess(wget As wgetTask, percentage As Double) Handles task.DownloadProcess
            Console.CursorTop = cursorTop
            ClearLine()
            Console.WriteLine(wget.ToString)
        End Sub

        Private Sub ClearLine()
            Console.CursorLeft = 0
            Console.Write(" ")
            Console.Write(New String(" "c, Console.BufferWidth - 1))
            Console.CursorLeft = 1
            Console.CursorTop -= 1
        End Sub

        ''' <summary>
        ''' Do task summary
        ''' </summary>
        ''' <param name="req"></param>
        ''' <param name="resp"></param>
        Private Sub ReportRequest(req As WebRequest, resp As WebResponse, remote$) Handles task.ReportRequest
            Dim domain As New DomainName(task.url)
            Dim contentSize$ = resp.ContentLength
            Dim sizePrettyPrint$ = StringFormats.Lanudry(resp.ContentLength) Or "N/A".When(resp.ContentLength = -1)

            If contentSize = "-1" Then
                contentSize = "<Unknown Size>"
            End If

            Call Console.WriteLine()

            Call ClearLine() : Console.WriteLine($"--{Now}--  {task.url}")
            Call ClearLine() : Console.WriteLine($"     => '{task.saveFile.FileName}'")
            Call ClearLine() : Console.WriteLine()
            Call ClearLine() : Console.WriteLine($"Resolving {resp.ResponseUri.Host} ({domain})... {remote}")
            Call ClearLine() : Console.WriteLine($"==> METHOD ... {req.Method}/{req.RequestUri.Scheme} {DirectCast(req, HttpWebRequest).ProtocolVersion}")
            Call ClearLine() : Console.WriteLine($"==> SIZE {task.saveFile.FileName} ... {contentSize}")
            Call ClearLine() : Console.WriteLine($"==> CONTENT-TYPE ... {resp.ContentType}")
            Call ClearLine() : Console.WriteLine($"Length: {contentSize} ({sizePrettyPrint})")

            Call Console.WriteLine()

            cursorTop = Console.CursorTop
        End Sub

        Private Sub clearOutput()
            Console.CursorTop = originalTop

            For i As Integer = 0 To 13
                Call ClearLine()
                Call Console.WriteLine()
            Next

            Console.CursorTop = originalTop
        End Sub

        ''' <summary>
        ''' 执行有详细进度信息显示的文件下载操作, 如果只需要调用一个单纯的文件下载函数, 
        ''' 请使用<see cref="DownloadFile(String, String, String, String, Integer, DownloadProgressChangedEventHandler, String, String)"/>拓展函数
        ''' </summary>
        ''' <param name="url$"></param>
        ''' <param name="save$"></param>
        ''' <returns></returns>
        Public Shared Function Download(url$,
                                        Optional save$ = Nothing,
                                        Optional headers As Dictionary(Of String, String) = Nothing,
                                        Optional clear As Boolean = False) As Boolean

            Dim local As New Value(Of String)
            Dim task As New wget(url, local = save Or $"./{url.Split("?"c).First.FileName}".AsDefault, headers)

            Call task.Run()

            If clear Then
                Call task.clearOutput()
            End If

            Return local.Value.FileLength > 0
        End Function
    End Class
End Namespace
