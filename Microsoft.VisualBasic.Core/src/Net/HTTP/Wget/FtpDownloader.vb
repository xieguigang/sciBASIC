#Region "Microsoft.VisualBasic::230f3901acc08000c2d05f49fa574a16, Microsoft.VisualBasic.Core\src\Net\HTTP\Wget\FtpDownloader.vb"

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
    '         Function: ftpRequest, OpenSaveStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net

Namespace Net.WebClient

    Public Class FtpDownloader : Inherits WebClient

        ReadOnly ftpUri As Uri
        ReadOnly localFilePath As String
        ReadOnly request As FtpWebRequest
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
            Me.request = ftpRequest(user, password)
        End Sub

        Sub New(ftpUri As String, buffer As Stream,
                Optional user As String = "anonymous",
                Optional password As String = "user@example.com")

            Me.ftpUri = New Uri(ftpUri)
            Me.request = ftpRequest(user, password)
            Me._buffer = buffer
        End Sub

        Private Function ftpRequest(user As String, password As String) As FtpWebRequest
#Disable Warning
            Dim request = DirectCast(WebRequest.Create(Me.ftpUri), FtpWebRequest)
#Enable Warning

            request.Credentials = New NetworkCredential(user, password) ' 匿名登录
            request.Method = WebRequestMethods.Ftp.DownloadFile
            request.Proxy = Nothing
            request.KeepAlive = False
            request.UseBinary = True
            request.EnableSsl = False
            request.Timeout = 5000 ' 设置超时时间（毫秒）

            Return request
        End Function

        Public Overrides Async Function DownloadFileAsync() As Task
            Using response As FtpWebResponse = Await request.GetResponseAsync()
                Await RequestStream(response)
            End Using
        End Function

        Private Async Function RequestStream(response As FtpWebResponse) As Task
            Dim totalBytes As Long = response.ContentLength

            Call ProgressUpdate(New ProgressChangedEventArgs(0, CLng(totalBytes)))

            Using responseStream As Stream = response.GetResponseStream()
                Dim localFileStream As Stream = OpenSaveStream()
                Dim buffer As Byte() = New Byte(_bufferSize - 1) {}
                Dim bytesRead As Integer = Await responseStream.ReadAsync(buffer, 0, buffer.Length)
                Dim totalBytesRead As Long = bytesRead

                While bytesRead > 0
                    Await localFileStream.WriteAsync(buffer, 0, bytesRead)

                    bytesRead = Await responseStream.ReadAsync(buffer, 0, buffer.Length)
                    totalBytesRead += bytesRead

                    Call ProgressUpdate(New ProgressChangedEventArgs(totalBytesRead, CLng(totalBytes)))
                End While

                Await localFileStream.FlushAsync
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
