#Region "Microsoft.VisualBasic::e98183971f9ec46aae479dd6f8ed6093, Microsoft.VisualBasic.Core\src\Net\Wget\wgetTask.vb"

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


    ' Code Statistics:

    '   Total Lines: 261
    '    Code Lines: 168 (64.37%)
    ' Comment Lines: 51 (19.54%)
    '    - Xml Docs: 45.10%
    ' 
    '   Blank Lines: 42 (16.09%)
    '     File Size: 9.92 KB


    '     Class wgetTask
    ' 
    '         Properties: currentSize, downloadSpeed, headers, isDownloading, saveFile
    '                     StreamSize, totalSize, url
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: StartTask, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, doDownloadTask, doTaskInternal, switchStat
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Parallel

Namespace Net.WebClient

    ''' <summary>
    ''' 提供一些比较详细的数据信息和事件处理
    ''' </summary>
    Public Class wgetTask : Implements IDisposable

        Dim _stream As WriteData
        Dim _speedSamples As List(Of Double)
        Dim _isUnknownContentSize As Boolean = False

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

        Public ReadOnly Property StreamSize As Long
            Get
                Return _stream.Length
            End Get
        End Property

        Public Event DownloadProcess(wget As wgetTask, percentage#)
        Public Event ReportRequest(req As WebRequest, resp As WebResponse, remote$)

        ''' <summary>
        ''' Client status
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isDownloading As Boolean
        Public ReadOnly Property headers As Dictionary(Of String, String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="downloadUrl"></param>
        ''' <param name="saveFile">
        ''' Module will create a new <see cref="FileStream"/> that writes to this desired download path
        ''' </param>
        Sub New(downloadUrl As String, saveFile As String, headers As Dictionary(Of String, String))
            Call Me.New(downloadUrl, saveFile.Open(doClear:=True), headers)
        End Sub

        Sub New(downloadUrl As String, pip As DuplexPipe, headers As Dictionary(Of String, String))
            Me.headers = headers
            Me._stream = New WriteData(pip)
            Me.url = downloadUrl
            Me.saveFile = "<duplex_pipeline>"
        End Sub

        Sub New(downloadUrl As String, save As Stream, headers As Dictionary(Of String, String))
            Me.headers = headers
            Me._stream = New WriteData(save)
            Me.url = downloadUrl

            Select Case save.GetType
                Case GetType(FileStream)
                    Me.saveFile = DirectCast(save, FileStream).Name
                Case GetType(MemoryStream)
                    Me.saveFile = $"<in_memory_{save.GetHashCode.ToHexString}>"
                Case Else
                    Me.saveFile = $"<unknown_{save.GetHashCode.ToHexString}>"
            End Select
        End Sub

        Public Function StartTask(Optional doRetry As Boolean = True, Optional bufferSize% = 1024) As Boolean
            Dim retry As Integer = If(doRetry, 0, 5)
            Dim result As Boolean = True

            If isDownloading Then
                Return False
            Else
                Call switchStat()
            End If
RE:
            Try
                Call doTaskInternal(bufferSize)
            Catch ex As Exception When Not TypeOf ex Is NotImplementedException
                Call ex.PrintException

                If retry < 3 Then
                    retry += 1
                    GoTo RE
                Else
                    ' exit download
                    result = False
                End If
            Finally
                Call switchStat()
            End Try

            Return result
        End Function

        Private Sub switchStat()
            _isDownloading = Not isDownloading
        End Sub

        Dim _startTime&

        Private Sub doTaskInternal(bufferSize As Integer)
            ' Make a request for the url of the file to be downloaded
            Dim req As WebRequest = HttpGet.BuildWebRequest(url, headers, Nothing, UserAgent.GoogleChrome)
            Dim remote$ = "NA"

            If TypeOf req Is HttpWebRequest Then
                DirectCast(req, HttpWebRequest).ServicePoint.BindIPEndPointDelegate =
                    Function(svrs, ip, counts)
                        remote = ip.ToString
                        Return Nothing
                    End Function
            End If

            ' Ask for the response
            Dim _resp As WebResponse = req.GetResponse

            _totalSize = _resp.ContentLength
            _speedSamples = New List(Of Double)
            _currentSize = 0
            _startTime = App.ElapsedMilliseconds
            _isUnknownContentSize = totalSize < 0

            RaiseEvent ReportRequest(req, _resp, remote)
            RaiseEvent DownloadProcess(Me, 100 * currentSize / totalSize)

            If totalSize = -1 Then
                ' task with no Content-Length
                Call doDownloadTask(_resp, bufferSize, Function(read) read = 0)
            Else
                Call doDownloadTask(_resp, bufferSize, Function() currentSize >= totalSize)
            End If

            Call _resp.Close()
            Call _stream.Flush()
        End Sub

        Private Sub doDownloadTask(resp As WebResponse, bufferSize%, exitJob As Func(Of Integer, Boolean))
            ' Make a buffer
            Dim buffer(bufferSize - 1) As Byte
            Dim read As Integer = Integer.MaxValue
            Dim interval As Double
            Dim secondAgo As Double

            Do While Not exitJob(read)
                ' Read the buffer from the response the WebRequest gave you
                read = resp.GetResponseStream.Read(buffer, Scan0, buffer.Length)
                ' Write to filestream that you declared at the beginning 
                ' of the DoWork sub
                ' 20201101 如果不使用Take函数进行额外数据的清除操作
                ' 下载的文件会和原始文件不一致，这个非常有可能可能会导致文件无法打开的问题
                _stream.Write(buffer.Take(read).ToArray)

                _currentSize += read
                interval = TimeSpan.FromMilliseconds(App.ElapsedMilliseconds - _startTime).TotalSeconds

                ' Then, if downloadedTime reaches 1000 then it will call this part
                ' Calculate the download speed by dividing the downloadedSize 
                ' by the total formatted seconds of the downloadedTime
                Call (currentSize / interval).DoCall(AddressOf _speedSamples.Add)

                If interval - secondAgo > 1 Then
                    secondAgo = interval
                    RaiseEvent DownloadProcess(Me, 100 * currentSize / totalSize)
                End If
            Loop
        End Sub

        Dim busy As Integer

        Public Overrides Function ToString() As String
            If busy = 5 Then
                busy = 1
            Else
                busy += 1
            End If

            Dim progress$
            Dim ETA$

            If totalSize > 0 Then
                If downloadSpeed <= 0 Then
                    ETA = "n/a"
                Else
                    ETA = TimeSpan.FromSeconds((totalSize - currentSize) / downloadSpeed).FormatTime
                End If
            Else
                ETA = "n/a"
            End If

            If _isUnknownContentSize Then
                progress = $"<unknown>%, {StringFormats.Lanudry(downloadSpeed)}/sec"
            Else
                progress = $"{(100 * _currentSize / _totalSize).ToString("F2")}%, {StringFormats.Lanudry(downloadSpeed)}/sec"
            End If

            Dim elapsed$ = TimeSpan.FromMilliseconds(App.ElapsedMilliseconds - _startTime).FormatTime
            Dim busyStr As New String("."c, busy)

            Return $"> '{saveFile.FileName}'{busyStr}  {StringFormats.Lanudry(currentSize)} [{progress}], elapsed {elapsed}, [ETA {ETA}]"
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call _stream.Dispose()
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
