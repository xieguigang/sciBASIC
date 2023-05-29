#Region "Microsoft.VisualBasic::af2a1d9931655c545603c4984f7c8cea, sciBASIC#\Microsoft.VisualBasic.Core\src\Net\HTTP\WebResponse.vb"

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

'   Total Lines: 56
'    Code Lines: 40
' Comment Lines: 0
'   Blank Lines: 16
'     File Size: 1.66 KB


'     Class WebResponseResult
' 
'         Properties: headers, html, timespan, url
' 
'     Class ResponseHeaders
' 
'         Properties: customHeaders, headers
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: Header200, Header404NotFound, (+2 Overloads) TryGetValue
' 
' 
' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace Net.Http

    Public Class WebResponseResult

        ''' <summary>
        ''' the text content result data of the current web http request
        ''' </summary>
        ''' <returns></returns>
        Public Property html As String
        Public Property headers As ResponseHeaders
        Public Property timespan As Long
        Public Property url As String
        Public Property payload As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return html
        End Function

    End Class

    Public Class ResponseHeaders

        Public Property headers As New Dictionary(Of HttpHeaderName, String)
        Public Property customHeaders As New Dictionary(Of String, String)
        Public ReadOnly Property httpCode As HTTP_RFC
            Get
                Return code
            End Get
        End Property

        Dim stringIndex As New Dictionary(Of String, String)
        Dim code As HTTP_RFC = HTTP_RFC.RFC_OK

        Sub New(raw As WebHeaderCollection)
            Dim header As HttpHeaderName

            For Each key As String In raw.AllKeys
                header = ParseHeaderName(key)

                If header = HttpHeaderName.Unknown Then
                    customHeaders(key) = raw.Get(key)
                Else
                    headers(header) = raw.Get(key)
                End If

                stringIndex(key.ToLower) = raw.Get(key)
            Next
        End Sub

        Private Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Header404NotFound() As ResponseHeaders
            Return New ResponseHeaders With {
                .headers = New Dictionary(Of HttpHeaderName, String) From {
                    {HttpHeaderName.ContentType, MIME.Text}
                },
                .code = HTTP_RFC.RFC_NOT_FOUND
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Header500InternalServerError() As ResponseHeaders
            Return New ResponseHeaders With {
                .headers = New Dictionary(Of HttpHeaderName, String) From {
                    {HttpHeaderName.ContentType, MIME.Text}
                },
                .code = HTTP_RFC.RFC_INTERNAL_SERVER_ERROR
            }
        End Function

        Public Shared Function HttpRequestError(err As Integer) As ResponseHeaders
            Return New ResponseHeaders With {
                .headers = New Dictionary(Of HttpHeaderName, String) From {
                    {HttpHeaderName.ContentType, MIME.Text}
                },
                .code = err
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Header200() As ResponseHeaders
            Return New ResponseHeaders With {
                .headers = New Dictionary(Of HttpHeaderName, String) From {
                    {HttpHeaderName.ContentType, MIME.Text}
                }
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TryGetValue(header As HttpHeaderName) As String
            Return headers.TryGetValue(header)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TryGetValue(header As String) As String
            Return stringIndex.TryGetValue(Strings.LCase(header))
        End Function
    End Class
End Namespace
