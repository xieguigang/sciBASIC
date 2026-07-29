Imports System.Text
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports System.Text.RegularExpressions

Public Class TextRender : Inherits Render

    Public Overrides Function Document(html As String) As String
        Return StripHTMLTags(html)
    End Function

    Public Overrides Function Paragraph(text As String, Optional createParagraphs As Boolean = True) As String
        If createParagraphs Then
            Return text & vbCrLf
        End If
        Return text
    End Function

    Public Overrides Function Header(text As String, level As Integer) As String
        Return text & vbCrLf
    End Function

    Public Overrides Function HorizontalLine() As String
        Return New String("-"c, 60) & vbCrLf
    End Function

    Public Overrides Function NewLine() As String
        Return vbCrLf
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

    Public Overrides Function Strikethrough(text As String) As String
        Return text
    End Function

    Public Overrides Function CodeSpan(text As String) As String
        Return text
    End Function

    Public Overrides Function CodeBlock(text As String, language As String) As String
        Return text & vbCrLf
    End Function

    Public Overrides Function Image(url As String, alt As String, title As String) As String
        Return alt
    End Function

    Public Overrides Function AnchorLink(url As String, text As String, title As String) As String
        Return text
    End Function

    Public Overrides Function BlockQuote(text As String) As String
        Return "> " & text.Replace(vbLf, vbLf & "> ")
    End Function

    Public Overrides Function List(items As IEnumerable(Of String), orderList As Boolean, Optional startNumber As Integer = 1) As String
        Return items.JoinBy(vbLf) & vbLf
    End Function

    Public Overrides Function Table(head() As String, rows As IEnumerable(Of String()), Optional align() As String = Nothing) As String
        Dim t As New StringBuilder
        t.AppendLine(head.JoinBy(vbTab))
        For Each row In rows
            t.AppendLine(row.JoinBy(vbTab))
        Next
        Return t.ToString
    End Function

    Public Overrides Sub SetImageUrlRouter(router As Func(Of String, String))
        _router = router
    End Sub

    Shared ReadOnly htmlTag As New Regex("<[^>]+>", RegexOptions.Compiled)

    Shared Function StripHTMLTags(html As String) As String
        Return htmlTag.Replace(html, "")
    End Function
End Class
