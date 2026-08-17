#Region "Microsoft.VisualBasic::230f3901acc08000c2d05f49fa574a16, Microsoft.VisualBasic.Core\src\Net\Wget\FtpDownloader.vb"

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

    '   Total Lines: 94
    '    Code Lines: 72 (76.60%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 22 (23.40%)
    '     File Size: 3.48 KB


    '     Class FtpDownloader
    ' 
    '         Properties: LocalSaveFile
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: DownloadFileAsync, OpenSaveStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.FTP

Namespace Net.WebClient

    ''' <summary>
    ''' 基于 <see cref="FtpClient"/> 实现的 FTP 文件下载器。
    ''' 用于替代基于已过时 <see cref="System.Net.FtpWebRequest"/> 的旧实现。
    ''' </summary>
    Public Class FtpDownloader : Inherits WebClient

        ReadOnly ftpUri As Uri
        ReadOnly localFilePath As String
        ReadOnly _client As FtpClient
        ReadOnly _remotePath As String
        ReadOnly _bufferSize As Integer = 8192
        ReadOnly _buffer As Stream

        Public Overrides ReadOnly Property LocalSaveFile As String
            Get
                Return localFilePath
            End Get
        End Property

        Public Sub New(ftpUri As String, localPath As String,
                   Optional user As String = "anonymous",
                   Optional password As String = "user@example.com")

            Me.ftpUri = New Uri(ftpUri)
            Me.localFilePath = localPath
            Me._client = CreateClient(user, password)
            Me._remotePath = Me.ftpUri.AbsolutePath
        End Sub

        Sub New(ftpUri As String, buffer As Stream,
                Optional user As String = "anonymous",
                Optional password As String = "user@example.com")

            Me.ftpUri = New Uri(ftpUri)
            Me._client = CreateClient(user, password)
            Me._remotePath = Me.ftpUri.AbsolutePath
            Me._buffer = buffer
        End Sub

        Private Function CreateClient(user As String, password As String) As FtpClient
            Dim port As Integer = ftpUri.Port
            If port < 0 Then port = 21

            Dim creds As New FtpCredentials(user, password)
            Return New FtpClient(ftpUri.Host, port, Nothing, creds)
        End Function

        Public Overrides Async Function DownloadFileAsync() As Task
            ' 进度映射: FtpDownloadProgress -> ProgressChangedEventArgs
            Dim progress As New Progress(Of FtpDownloadProgress)(
                Sub(p)
                    Dim bytesReceived As Long = p.BytesTransferred
                    Dim totalBytes As Long = p.TotalBytes

                    Call ProgressUpdate(New ProgressChangedEventArgs(bytesReceived, totalBytes))
                End Sub)

            Using client As FtpClient = _client
                If _buffer Is Nothing Then
                    ' 下载到本地文件，完成后关闭文件流
                    Using saveStream As Stream = OpenSaveStream()
                        Await client.DownloadAsync(_remotePath, saveStream, progress:=progress)
                        Await saveStream.FlushAsync()
                    End Using
                Else
                    ' 下载到调用方传入的流，不关闭该流 (兼容原行为)
                    Await client.DownloadAsync(_remotePath, _buffer, progress:=progress)
                    Await _buffer.FlushAsync()
                End If
            End Using

            Call ProgressFinished()
        End Function

        Protected Overrides Function OpenSaveStream() As Stream
            If Not _buffer Is Nothing Then
                Return _buffer
            End If

            Return localFilePath.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
        End Function
    End Class
End Namespace
