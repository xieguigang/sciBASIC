Imports System.IO
Imports System.Net
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Http

    ' https://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data

    Public Class MultipartForm

        Dim buffer As New MemoryStream

        Const Boundary$ = "------WebKitFormBoundaryBpijhG6dKsQpCMdN--"

        ''' <summary>
        ''' 添加键值对
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value$"></param>
        ''' <remarks>
        ''' 字符串键值对是使用<see cref="Encoding.UTF8"/>编码的
        ''' </remarks>
        Public Sub Add(name$, value$)
            With New StreamWriter(buffer, Encoding.UTF8) With {
                .NewLine = vbCrLf
            }
                Call .WriteLine()
                Call .WriteLine(Boundary)
                Call .WriteLine($"Content-Disposition: form-data; name=""{name}"";")
                Call .WriteLine()
                Call .WriteLine(value)

                Call .Flush()
            End With
        End Sub

        ''' <summary>
        ''' 添加文件或者数据包等
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="buffer"></param>
        ''' <remarks>
        ''' 数据包则需要使用<see cref="Encoding.ASCII"/>来进行编码
        ''' </remarks>
        Public Sub Add(name$,
                       buffer As IEnumerable(Of Byte),
                       Optional fileName$ = "UnknownStream",
                       Optional contentType$ = ContentTypes.Unknown)

            With Me.buffer
                Call .WriteLine()
                Call .WriteLine(Boundary, Encoding.ASCII)
                Call .WriteLine($"Content-Disposition: form-data; name=""{name}""; filename=""{fileName}""")
                Call .WriteLine($"Content-Type: {contentType}")
                Call .WriteLine()
            End With

            With buffer.ToArray
                Call Me.buffer.Write(.ByRef, Scan0, .Length)
            End With

            With Me.buffer
                Call .WriteLine()
                Call .WriteLine(Boundary & "--", Encoding.ASCII)
            End With
        End Sub

        ''' <summary>
        ''' POST this multipart form package to a specific web <paramref name="api"/>
        ''' </summary>
        ''' <param name="api$"></param>
        ''' <param name="headers"></param>
        ''' <returns></returns>
        Public Function POST(api$, Optional headers As Dictionary(Of String, String) = Nothing) As String
            Dim request As HttpWebRequest = WebRequest.Create(api)
            request.ContentType = "multipart/form-data; boundary=" & Boundary
            request.Method = "POST"
            request.KeepAlive = True
            request.ContentLength = buffer.Length

            For Each header In headers.SafeQuery
                Call request.Headers.Add(header.Key, header.Value)
            Next

            Using requestStream As Stream = request.GetRequestStream
                buffer.Position = Scan0
                requestStream.Write(buffer.ToArray, Scan0, buffer.Length)
                requestStream.Flush()
            End Using

            Using response = request.GetResponse, stream As Stream = response.GetResponseStream
                Return New StreamReader(stream).ReadToEnd
            End Using
        End Function
    End Class
End Namespace
