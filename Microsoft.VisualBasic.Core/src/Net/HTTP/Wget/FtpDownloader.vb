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