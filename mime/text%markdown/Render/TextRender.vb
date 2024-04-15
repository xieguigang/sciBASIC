Imports System.Text

''' <summary>
''' plain text document
''' </summary>
Public Class TextRender : Inherits Render

    Public Overrides Function Paragraph(text As String, CreateParagraphs As Boolean) As String
        Return text
    End Function

    Public Overrides Function Header(text As String, level As Integer) As String
        Return text
    End Function

    Public Overrides Function CodeSpan(text As String) As String
        Return " " & text & " "
    End Function

    Public Overrides Function CodeBlock(code As String, lang As String) As String
        Return code
    End Function

    Public Overrides Function Document(text As String) As String
        Return text
    End Function

    Public Overrides Function HorizontalLine() As String
        Return "-----------------------------------------"
    End Function

    Public Overrides Function NewLine() As String
        Return vbLf
    End Function

    Public Overrides Function Image(url As String, altText As String, title As String) As String
        Return altText
    End Function

    Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
        Return text
    End Function

    Public Overrides Function Bold(text As String) As String
        Return text
    End Function

    Public Overrides Function Italic(text As String) As String
        Return text
    End Function

    Public Overrides Function Underline(text As String) As String
        Return text
    End Function

    Public Overrides Function BlockQuote(text As String) As String
        Return $"""{text}"""
    End Function

    Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean) As String
        Dim i As Integer = 1
        Dim sb As New StringBuilder

        For Each line As String In items
            Call sb.AppendLine($"[{i}] {line}")
        Next

        Return sb.ToString
    End Function

    Public Overrides Function Table(head() As String, rows As IEnumerable(Of String())) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine(head.JoinBy(vbTab))
        Call sb.AppendLine("----------------------------------------------")

        For Each row As String() In rows
            Call sb.AppendLine(row.JoinBy(vbTab))
        Next

        Return sb.ToString
    End Function
End Class
