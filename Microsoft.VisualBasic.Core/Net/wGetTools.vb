#Region "Microsoft.VisualBasic::d123268ab48cb110ebd63048c445139e, Net\wGetTools.vb"

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

Namespace Net

    ''' <summary>
    ''' 提供一些比较详细的数据信息和事件处理
    ''' </summary>
    Public Class wGetTools : Implements System.IDisposable

        ReadOnly downloadUrl As String
        ReadOnly fs As FileStream
        ReadOnly savePath As String

        ''' <summary>
        ''' Size that has been downloaded
        ''' </summary>
        Public ReadOnly Property CurrentSize As Long = 0
        ''' <summary>
        ''' Total size of the file that has to be downloaded
        ''' </summary>
        Public ReadOnly Property TotalSize As Long

        ''' <summary>
        ''' KB/sec
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DownloadSpeed As Double
            Get
                Return _speedSamples.Average
            End Get
        End Property

        Public Event DownloadProcess(wget As wGetTools, percentage As Double)

        ''' <summary>
        ''' Client status
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Downloading As Boolean

        Sub New(downloadUrl As String, SaveFile As String)
            Me.fs = New FileStream(SaveFile, FileMode.CreateNew) 'Create a new FileStream that writes to the desired download path
            Me.downloadUrl = downloadUrl
            Me.savePath = SaveFile
        End Sub

        Dim _speedSamples As List(Of Double)

        Public Sub StartTask()
            If Downloading Then Return

            _Downloading = True
            Call __downloadTask()
            _Downloading = False
        End Sub

        Private Sub __downloadTask()
            Dim req As WebRequest = WebRequest.Create(downloadUrl) 'Make a request for the url of the file to be downloaded
            Dim resp As WebResponse = req.GetResponse 'Ask for the response

            Dim buffer(8192) As Byte 'Make a buffer
            Dim downloadedsize As Long = 0
            Dim downloadedTime As Long = 0
            Dim dlSpeed As Long = 0

            _TotalSize = req.ContentLength
            _speedSamples = New List(Of Double)
            _CurrentSize = 0

            Do While _CurrentSize < _TotalSize
                Dim read As Integer = resp.GetResponseStream.Read(buffer, 0, 8192) 'Read the buffer from the response the WebRequest gave you

                fs.Write(buffer, 0, read) 'Write to filestream that you declared at the beginning of the DoWork sub

                _CurrentSize += read

                downloadedsize += read
                downloadedTime += 1 'Add 1 millisecond for every cycle the While field makes

                If downloadedTime = 1000 Then 'Then, if downloadedTime reaches 1000 then it will call this part
                    dlSpeed = (downloadedsize / TimeSpan.FromMilliseconds(downloadedTime).TotalSeconds) 'Calculate the download speed by dividing the downloadedSize by the total formatted seconds of the downloadedTime

                    downloadedTime = 0 'Reset downloadedTime and downloadedSize
                    downloadedsize = 0

                    Call _speedSamples.Add(dlSpeed)

                    RaiseEvent DownloadProcess(Me, 100 * CurrentSize / TotalSize)
                End If
            Loop

            fs.Close() 'Close the FileStream first, or the FileStream will crash.
            resp.Close() 'Close the response
        End Sub

        Public Overrides Function ToString() As String
            Return $"{downloadUrl} ==> {savePath.ToFileURL}   [{100 * _currentSize / _totalSize}%, {DownloadSpeed}KB/sec]"
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
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
