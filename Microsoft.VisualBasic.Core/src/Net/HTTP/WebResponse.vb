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

        Public Function TryGetValue(header As HttpHeaderName) As String
            Return headers.TryGetValue(header)
        End Function

        Public Function TryGetValue(header As String) As String
            Return stringIndex.TryGetValue(Strings.LCase(header))
        End Function
    End Class
End Namespace