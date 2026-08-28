#Region "Microsoft.VisualBasic::62bfe86dece0db149bc7de2cb1953db3, Microsoft.VisualBasic.Core\src\Net\HTTP\Multipart.vb"

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

    '   Total Lines: 155
    '    Code Lines: 93 (60.00%)
    ' Comment Lines: 40 (25.81%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 22 (14.19%)
    '     File Size: 6.31 KB


    '     Class MultipartForm
    ' 
    '         Function: POST, ToBase64
    ' 
    '         Sub: (+2 Overloads) Add, (+2 Overloads) Dispose, Dump
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Http

    ' https://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data

    Public Class MultipartForm : Implements IDisposable

        ReadOnly buffer As New MemoryStream

        ''' <summary>
        ''' 需要使用<see cref="Encoding.ASCII"/>来进行编码
        ''' </summary>
        Public ReadOnly boundary$ = "---------------------------" & DateTime.Now.Ticks.ToString("x")

        Private disposedValue As Boolean

        ''' <summary>
        ''' Add form data.(添加键值对数据)
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="value$"></param>
        ''' <remarks>
        ''' 字符串键值对是使用<see cref="Encoding.UTF8"/>编码的
        ''' </remarks>
        Public Sub Add(name$, value$)
            With Me.buffer
                Call .WriteLine(, Encoding.ASCII)
                Call .WriteLine("--" & boundary, Encoding.ASCII)
                Call .WriteLine($"Content-Disposition: form-data; name=""{name}""")
                Call .WriteLine()
                Call .Write(value)
            End With
        End Sub

        ''' <summary>
        ''' 添加文件或者数据包等
        ''' </summary>
        ''' <param name="name$"></param>
        ''' <param name="buffer"></param>
        ''' <remarks>
        ''' 
        ''' </remarks>
        Public Sub Add(name$,
                       buffer As IEnumerable(Of Byte),
                       Optional fileName$ = "UnknownStream",
                       Optional contentType$ = ContentTypes.Unknown)

            With Me.buffer
                Call .WriteLine(, Encoding.ASCII)
                Call .WriteLine("--" & boundary, Encoding.ASCII)
                Call .WriteLine($"Content-Disposition: form-data; name=""{name}""; filename=""{fileName}""")
                Call .WriteLine($"Content-Type: {contentType}")
                Call .WriteLine()
            End With

            With buffer.ToArray
                Call Me.buffer.Write(.ByRef, Scan0, .Length)
            End With

            With Me.buffer
                Call .WriteLine(, Encoding.ASCII)
                Call .WriteLine("--" & boundary & "--", Encoding.ASCII)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Dump(path As String)
            Call buffer.ToArray.FlushStream(path)
        End Sub

        Public Function ToBase64() As String
            Return buffer.ToBase64String
        End Function

        ''' <summary>
        ''' POST this multipart form package to a specific web <paramref name="api"/>
        ''' </summary>
        ''' <param name="api$"></param>
        ''' <param name="headers"></param>
        ''' <returns></returns>
        Public Function POST(api$, Optional headers As Dictionary(Of String, String) = Nothing) As WebResponseResult
            Dim request As New HttpRequestMessage(HttpMethod.Post, api)
            request.Headers.TryAddWithoutValidation("Accept", "*/*")

            For Each header In headers.SafeQuery
                Call request.Headers.TryAddWithoutValidation(header.Key, header.Value)
            Next

            Dim content As New ByteArrayContent(buffer.ToArray)
            content.Headers.ContentType = New MediaTypeHeaderValue("multipart/form-data; boundary=" & boundary)
            request.Content = content

            ' NOTE: the previous HttpWebRequest pipeline used
            ' CredentialCache.DefaultCredentials; the shared HttpClient in
            ' HttpClientFactory does not enable UseDefaultCredentials, so NTLM
            ' default credentials are not forwarded here.
            Using response As HttpResponseMessage = HttpClientFactory.SendSync(request)
                If response.IsSuccessStatusCode Then
                    Dim responseText$ = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

                    Return New WebResponseResult With {
                        .url = api,
                        .headers = ResponseHeaders.Header200,
                        .html = responseText,
                        .timespan = 0,
                        .payload = ""
                    }
                Else
                    Using pageStream As Stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
                        Return New WebResponseResult With {
                            .html = WebServiceUtils.readStreamText(pageStream),
                            .headers = ResponseHeaders.Header500InternalServerError,
                            .url = api
                        }
                    End Using
                End If
            End Using
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call buffer.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
