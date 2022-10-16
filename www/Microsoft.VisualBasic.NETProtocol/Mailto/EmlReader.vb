Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text

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
        Public Property BodyContent As String

        Public Shared Function ParseEMail(file As String) As EmlReader
            Dim raw As Dictionary(Of String, String) = ContentLoader(file.LineIterators.ToArray)
            Dim mail As New EmlReader With {
                .BodyContent = raw("Body"),
                .Encoding = raw("Content-Transfer-Encoding"),
                .[Date] = DateParser(raw("Date"))
            }

            If mail.Encoding.TextEquals("base64") Then
                mail.BodyContent = Encodings.GB2312.GetString(mail.BodyContent.Base64RawBytes)
            End If

            Return mail
        End Function

        Private Shared Function DateParser(val As String) As Date
            Dim text = val.Split(","c).Last.Trim
            Dim ddmmyyyy = text.Split.Take(3).JoinBy("/")
            Dim d As Date = Date.Parse(ddmmyyyy)

            Return d
        End Function

        Private Shared Function ContentLoader(lines As String()) As Dictionary(Of String, String)
            Dim data As String = ""
            Dim tagHeader As New Regex("[^:]+[:]\s", RegexICSng)
            Dim contents As New Dictionary(Of String, String)
            Dim body As String = ""
            Dim loadBody As Boolean = False

            For Each line As String In lines
                If loadBody Then
                    body = body & line
                Else
                    If tagHeader.Match(line).Success Then
                        If Not data.StringEmpty Then
                            Call AppendContent(contents, tagHeader, data)
                        End If

                        data = line
                    Else
                        data = data & line
                    End If

                    If line.StringEmpty Then
                        If Not data.StringEmpty Then
                            Call AppendContent(contents, tagHeader, data)
                        End If

                        ' begin email body
                        loadBody = True
                    End If
                End If
            Next

            contents.Add("Body", body)

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