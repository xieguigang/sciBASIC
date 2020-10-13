#Region "Microsoft.VisualBasic::65a79ec99c6b759492f9e2ac12130130, Microsoft.VisualBasic.Core\Net\HTTP\wgetTask.vb"

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

'     Class wgetTask
' 
'         Properties: currentSize, downloadSpeed, isDownloading, saveFile, totalSize
'                     url
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: ToString
' 
'         Sub: (+2 Overloads) Dispose, doTaskInternal, StartTask, switchStat, taskWithContentLength
'              taskWithNoContentLength
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic.Linq

Namespace Net.Http

    ''' <summary>
    ''' 提供一些比较详细的数据信息和事件处理
    ''' </summary>
    Public Class wgetTask : Implements IDisposable

        Friend ReadOnly fs As Stream

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
        Public Event ReportRequest(req As WebRequest, resp As WebResponse, remote$)

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
            Call Me.New(downloadUrl, saveFile.Open(doClear:=True))
        End Sub

        Sub New(downloadUrl As String, save As Stream)
            Me.fs = save
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

        Public Function StartTask(Optional doRetry As Boolean = True) As Boolean
            Dim retry As Integer = If(doRetry, 0, 5)
            Dim result As Boolean = True

            If isDownloading Then
                Return False
            Else
                Call switchStat()
            End If
RE:
            Try
                Call doTaskInternal()
            Catch ex As Exception
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

        Private Sub doTaskInternal()
            ' Make a request for the url of the file to be downloaded
            Dim req As WebRequest = WebRequest.Create(url)
            Dim remote$ = "NA"

            If TypeOf req Is HttpWebRequest Then
                DirectCast(req, HttpWebRequest).ServicePoint.BindIPEndPointDelegate =
                    Function(svrs, ip, counts)
                        remote = ip.ToString
                        Return Nothing
                    End Function
            End If

            ' Ask for the response
            Dim resp As WebResponse = req.GetResponse

            _totalSize = resp.ContentLength
            _speedSamples = New List(Of Double)
            _currentSize = 0
            _startTime = App.ElapsedMilliseconds

            RaiseEvent ReportRequest(req, resp, remote)
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
            Dim read As Integer
            Dim interval As Double

            Do While currentSize < totalSize
                ' Read the buffer from the response the WebRequest gave you
                read = resp.GetResponseStream.Read(buffer, 0, 8192)
                ' Write to filestream that you declared at the beginning 
                ' of the DoWork sub
                fs.Write(buffer, 0, read)

                _currentSize += read
                interval = TimeSpan.FromMilliseconds(App.ElapsedMilliseconds - _startTime).TotalSeconds

                ' Then, if downloadedTime reaches 1000 then it will call this part
                ' Calculate the download speed by dividing the downloadedSize 
                ' by the total formatted seconds of the downloadedTime
                Call (currentSize / interval).DoCall(AddressOf _speedSamples.Add)

                RaiseEvent DownloadProcess(Me, 100 * currentSize / totalSize)
            Loop
        End Sub

        Dim busy As Integer

        Public Overrides Function ToString() As String
            If busy = 5 Then
                busy = 1
            Else
                busy += 1
            End If

            Return $"> '{saveFile}'{New String("."c, busy)}  {StringFormats.Lanudry(currentSize)} [{(100 * _currentSize / _totalSize).ToString("F2")}%, {StringFormats.Lanudry(downloadSpeed)}/sec], elapsed {TimeSpan.FromMilliseconds(App.ElapsedMilliseconds - _startTime).FormatTime}"
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
