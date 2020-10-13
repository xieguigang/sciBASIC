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

Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.Language

Namespace Net.Http

    ''' <summary>
    ''' 命令行下的下载程序组件
    ''' </summary>
    Public Class wget : Implements IDisposable

        Dim WithEvents task As wgetTask
        Dim cursorTop%

        Private disposedValue As Boolean

        Sub New(url$, save$)
            task = New wgetTask(url, save)
            cursorTop = Console.CursorTop
        End Sub

        Sub New(url$, save As Stream)
            task = New wgetTask(url, save)
            cursorTop = Console.CursorTop
        End Sub

        Public Sub Run()
            Call task.StartTask()

            If Not task.isDownloading Then
                Call task.Dispose()

                Call Console.WriteLine()
                Call Console.WriteLine($"{Now.ToString} ({StringFormats.Lanudry(task.downloadSpeed)}/s) - '{task.saveFile}' saved [{task.fs.Length}]")
                Call Console.WriteLine()
            End If
        End Sub

        Private Sub DownloadProcess(wget As wgetTask, percentage As Double) Handles task.DownloadProcess
            Console.CursorTop = cursorTop
            ClearLine()
            Console.WriteLine(wget.ToString)
        End Sub

        Private Sub ClearLine()
            Console.CursorLeft = 1
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

            Call Console.WriteLine()

            Call ClearLine() : Console.WriteLine($"--{Now.ToString}--  {task.url}")
            Call ClearLine() : Console.WriteLine($"     => '{task.saveFile}'")
            Call ClearLine() : Console.WriteLine()
            Call ClearLine() : Console.WriteLine($"Resolving {resp.ResponseUri.Host} ({domain})... {remote}")
            Call ClearLine() : Console.WriteLine($"==> METHOD ... {req.Method}/{req.RequestUri.Scheme} {DirectCast(req, HttpWebRequest).ProtocolVersion}")
            Call ClearLine() : Console.WriteLine($"==> SIZE {task.saveFile} ... {resp.ContentLength}")
            Call ClearLine() : Console.WriteLine($"==> CONTENT-TYPE ... {resp.ContentType}")
            Call ClearLine() : Console.WriteLine($"Length: {resp.ContentLength} ({StringFormats.Lanudry(resp.ContentLength)})")

            Call Console.WriteLine()

            cursorTop = Console.CursorTop
        End Sub

        Public Shared Function Download(url$, Optional save$ = Nothing) As Boolean
            Dim local As New Value(Of String)
            Dim task As New wget(url, local = save Or $"./{url.Split("?"c).First.FileName}".AsDefault)

            Call task.Run()

            Return local.Value.FileLength > 0
        End Function

        Public Shared Function Download(url$, save As Stream) As Boolean
            Using task As New wget(url, save)
                Call task.Run()
            End Using

            Return True
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call task.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
