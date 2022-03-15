#Region "Microsoft.VisualBasic::127d85c8ae8ca614fe863f4df0569911, sciBASIC#\Microsoft.VisualBasic.Core\src\Net\HTTP\WebResponse.vb"

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

    '   Total Lines: 52
    '    Code Lines: 37
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 1.53 KB


    '     Class WebResponseResult
    ' 
    '         Properties: headers, html, timespan, url
    ' 
    '     Class ResponseHeaders
    ' 
    '         Properties: customHeaders, headers
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Header404NotFound, (+2 Overloads) TryGetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Net

Namespace Net.Http

    Public Class WebResponseResult

        Public Property html As String
        Public Property headers As ResponseHeaders
        Public Property timespan As Long
        Public Property url As String

    End Class

    Public Class ResponseHeaders

        Public Property headers As New Dictionary(Of HttpHeaderName, String)
        Public Property customHeaders As New Dictionary(Of String, String)

        Dim stringIndex As New Dictionary(Of String, String)

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

        Public Shared Function Header404NotFound() As ResponseHeaders
            Return New ResponseHeaders
        End Function

        Public Function TryGetValue(header As HttpHeaderName) As String
            Return headers.TryGetValue(header)
        End Function

        Public Function TryGetValue(header As String) As String
            Return stringIndex.TryGetValue(Strings.LCase(header))
        End Function
    End Class
End Namespace
