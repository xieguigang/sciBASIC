Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Namespace Mailto

    Public Class EmlReader

        Public Property Received
        Public Property Subject
        Public Property [Date] As Date
        Public Property MessageId As String
        Public Property MIMEVersion As Version
        Public Property ContentType As ContentType
        Public Property [From]
        Public Property [To]
        Public Property Encoding As String
        Public Property Content As String

        Public Shared Function ParseEMail(file As String) As EmlReader
            Dim raw As Dictionary(Of String, String) = ContentLoader(file.LineIterators.ToArray)

        End Function

        Private Shared Function ContentLoader(lines As String()) As Dictionary(Of String, String)
            Dim data As String = ""
            Dim tagHeader As New Regex("[^:]+[:]\s", RegexICSng)
            Dim contents As New Dictionary(Of String, String)

            For Each line As String In lines
                If tagHeader.Match(line).Success Then
                    If Not data.StringEmpty Then
                        Call AppendContent(contents, tagHeader, data)
                    End If

                    data = line
                Else
                    data = data & line
                End If
            Next

            If Not data.StringEmpty Then
                Call AppendContent(contents, tagHeader, data)
            End If

            Return contents
        End Function

        Private Shared Sub AppendContent(contents As Dictionary(Of String, String), tagHeader As Regex, data As String)
            Dim header As String = tagHeader.Match(data).Value
            Dim value As String = data.Substring(header.Length)

            header = header.Trim(" "c, ":"c)
            value = value.Trim
            contents.Add(header, value)
        End Sub

    End Class
End Namespace