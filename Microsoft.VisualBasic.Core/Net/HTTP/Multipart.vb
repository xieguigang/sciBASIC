#Region "Microsoft.VisualBasic::5b37ca2ecbcbc5969c0b2eb7bdb64de2, Microsoft.VisualBasic.Core\Net\HTTP\Multipart.vb"

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

    '     Class MultipartForm
    ' 
    '         Function: POST
    ' 
    '         Sub: (+2 Overloads) Add, Dump
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Http

    ' https://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data

    Public Class MultipartForm

        ReadOnly buffer As New MemoryStream
        ''' <summary>
        ''' 需要使用<see cref="Encoding.ASCII"/>来进行编码
        ''' </summary>
        ReadOnly boundary$ = "---------------------------" & DateTime.Now.Ticks.ToString("x")

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

        ''' <summary>
        ''' POST this multipart form package to a specific web <paramref name="api"/>
        ''' </summary>
        ''' <param name="api$"></param>
        ''' <param name="headers"></param>
        ''' <returns></returns>
        Public Function POST(api$, Optional headers As Dictionary(Of String, String) = Nothing) As String
            Dim request As HttpWebRequest = WebRequest.Create(api)
            request.ContentType = "multipart/form-data; boundary=" & boundary
            request.Method = "POST"
            request.KeepAlive = True
            request.Credentials = CredentialCache.DefaultCredentials

            For Each header In headers.SafeQuery
                Call request.Headers.Add(header.Key, header.Value)
            Next

            Using requestStream As Stream = request.GetRequestStream
                buffer.Position = Scan0
                requestStream.Write(buffer.ToArray, Scan0, buffer.Length)
                requestStream.Flush()
            End Using

            Using response = request.GetResponse, responseStream As Stream = response.GetResponseStream
                Using responseReader As New IO.StreamReader(responseStream)
                    Dim responseText = responseReader.ReadToEnd()
                    Return responseText
                End Using
            End Using
        End Function
    End Class
End Namespace
