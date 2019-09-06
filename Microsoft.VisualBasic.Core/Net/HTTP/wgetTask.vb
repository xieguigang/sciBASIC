#Region "Microsoft.VisualBasic::d123268ab48cb110ebd63048c445139e, Microsoft.VisualBasic.Core\Net\wGetTools.vb"

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

'     Class wGetTools
' 
'         Properties: CurrentSize, Downloading, DownloadSpeed, TotalSize
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: ToString
' 
'         Sub: __downloadTask, (+2 Overloads) Dispose, StartTask
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net

Namespace Net.Http

    ''' <summary>
    ''' 提供一些比较详细的数据信息和事件处理
    ''' </summary>
    Public Class wgetTask : Implements IDisposable

        ReadOnly fs As FileStream

        Dim _speedSamples As List(Of Double)

        ''' <summary>
        ''' Size that has been downloaded
        ''' </summary>
        Public ReadOnly Property currentSize As Long = 0
        ''' <summary>
        ''' Total size of the file that has to be downloaded
        ''' </summary>
        Public ReadOnly Property totalSize As Long
        Public ReadOnly Property url As String
        Public ReadOnly Property saveFile As String

        ''' <summary>
        ''' KB/sec
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property downloadSpeed As Double
            Get
                If _speedSamples.Count = 0 Then
                    Return 0
                Else
                    Return _speedSamples.Average
                End If
            End Get
        End Property

        Public Event DownloadProcess(wget As wgetTask, percentage#)
        Public Event ReportRequest(req As WebRequest)

        ''' <summary>
        ''' Client status
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isDownloading As Boolean

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="downloadUrl"></param>
        ''' <param name="saveFile">
        ''' Module will create a new <see cref="FileStream"/> that writes to this desired download path
        ''' </param>
        Sub New(downloadUrl As String, saveFile As String)
            Me.fs = saveFile.Open(doClear:=True)
            Me.url = downloadUrl
            Me.saveFile = saveFile
        End Sub

        Public Sub StartTask()
            If isDownloading Then Return

            Call switchStat()
            Call doTaskInternal()
            Call switchStat()
        End Sub

        Private Sub switchStat()
            _isDownloading = Not isDownloading
        End Sub

        Private Sub doTaskInternal()
            ' Make a request for the url of the file to be downloaded
            Dim req As WebRequest = WebRequest.Create(url)
            ' Ask for the response
            Dim resp As WebResponse = req.GetResponse

            _totalSize = req.ContentLength
            _speedSamples = New List(Of Double)
            _currentSize = 0

            RaiseEvent ReportRequest(req)
            RaiseEvent DownloadProcess(Me, 100 * currentSize / totalSize)

            If totalSize = -1 Then
                Call taskWithNoContentLength(resp)
            Else
                Call taskWithContentLength(resp)
            End If

            resp.Close()
        End Sub

        Private Sub taskWithNoContentLength(resp As WebResponse)
            Throw New NotImplementedException
        End Sub

        Private Sub taskWithContentLength(resp As WebResponse)
            ' Make a buffer
            Dim buffer(8192) As Byte
            Dim downloadedsize As Long = 0
            Dim downloadedTime As Long = 0
            Dim dlSpeed As Long = 0

            Do While currentSize < totalSize
                ' Read the buffer from the response the WebRequest gave you
                Dim read As Integer = resp.GetResponseStream.Read(buffer, 0, 8192)

                ' Write to filestream that you declared at the beginning of the DoWork sub
                Call fs.Write(buffer, 0, read)

                _currentSize += read
                ' Add 1 millisecond for every cycle the While field makes
                downloadedsize += read
                downloadedTime += 1

                If downloadedTime = 1000 Then
                    ' Then, if downloadedTime reaches 1000 then it will call this part
                    ' Calculate the download speed by dividing the downloadedSize 
                    ' by the total formatted seconds of the downloadedTime
                    dlSpeed = (downloadedsize / TimeSpan.FromMilliseconds(downloadedTime).TotalSeconds)
                    ' Reset downloadedTime and downloadedSize
                    downloadedTime = 0
                    downloadedsize = 0

                    Call _speedSamples.Add(dlSpeed)

                    RaiseEvent DownloadProcess(Me, 100 * currentSize / totalSize)
                End If
            Loop
        End Sub

        Public Overrides Function ToString() As String
            Return $"> '{saveFile.FileName}'    {currentSize} [{(100 * _currentSize / _totalSize).ToString("F2")}%, {downloadSpeed}KB/sec]"
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call fs.Flush()
                    Call fs.Close()
                    Call fs.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
