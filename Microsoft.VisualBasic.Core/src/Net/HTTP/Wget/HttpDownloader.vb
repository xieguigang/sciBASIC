Imports System.IO
Imports System.Net.Http

Namespace Net.WebClient

    Public Class HttpDownloader : Inherits WebClient

        ReadOnly _url As String
        ReadOnly _localPath As String = Nothing
        ReadOnly _bufferSize As Integer = 8192
        ReadOnly _buffer As Stream = Nothing

        Public Overrides ReadOnly Property LocalSaveFile As String
            Get
                Return _localPath
            End Get
        End Property

        Public Sub New(url As String, localPath As String)
            _url = url
            _localPath = localPath
        End Sub

        Sub New(url As String, save As Stream)
            _url = url
            _buffer = save
        End Sub

        Public Overrides Async Function DownloadFileAsync() As Task
            Using httpClient As New HttpClient()
                Using response As HttpResponseMessage = Await httpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead)
                    Call response.EnsureSuccessStatusCode()
                    ' implements download file
                    Await RequestStream(response)
                End Using
            End Using

            Call ProgressFinished()
        End Function

        Private Async Function RequestStream(response As HttpResponseMessage) As Task
            Dim totalBytes As Long? = response.Content.Headers.ContentLength

            Using contentStream As Stream = Await response.Content.ReadAsStreamAsync()
                Dim fileStream As Stream = OpenSaveStream()
                Dim totalRead As Long = 0L
                Dim buffer As Byte() = New Byte(_bufferSize - 1) {}
                Dim isMoreToRead As Boolean = True
                Dim bytesRead As Integer

                Call ProgressUpdate(New ProgressChangedEventArgs(0, CLng(totalBytes)))

                While isMoreToRead
                    bytesRead = Await contentStream.ReadAsync(buffer, 0, buffer.Length)

                    If bytesRead <= 0 Then
                        isMoreToRead = False
                    Else
                        Await fileStream.WriteAsync(buffer, 0, bytesRead)
                        totalRead += bytesRead

                        Call ProgressUpdate(New ProgressChangedEventArgs(totalRead, CLng(totalBytes)))
                    End If
                End While

                Await fileStream.FlushAsync
            End Using
        End Function

        Protected Overrides Function OpenSaveStream() As Stream
            If Not _buffer Is Nothing Then
                Return _buffer
            End If
            Return LocalSaveFile.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False)
        End Function
    End Class
End Namespace