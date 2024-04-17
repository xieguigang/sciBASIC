Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

''' <summary>
''' plain text document
''' </summary>
''' <remarks>
''' not only the markdown format tag will be removed from this render, and also 
''' the additional html tag inside the given markdown document will be removed.
''' </remarks>
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

    ''' <summary>
    ''' additional html tag inside the document will be removed
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Overrides Function Document(text As String) As String
        Return text.StripHTMLTags
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
        Return items.JoinBy(vbLf)
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
