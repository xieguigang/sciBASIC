﻿Imports System.Text.RegularExpressions

Public MustInherit Class Render

    Protected Shared ReadOnly _leadingWhitespace As New Regex("^[ ]*", RegexOptions.Compiled)

    Public MustOverride Function Paragraph(text As String, CreateParagraphs As Boolean) As String
    Public MustOverride Function Header(text As String, level As Integer) As String
    Public MustOverride Function CodeSpan(text As String) As String
    Public MustOverride Function CodeBlock(code As String, lang As String) As String
    Public MustOverride Function Document(text As String) As String
    Public MustOverride Function HorizontalLine() As String
    Public MustOverride Function NewLine() As String
    Public MustOverride Function Image(url As String, altText As String, title As String) As String
    Public MustOverride Function AnchorLink(url As String, text As String, title As String) As String
    Public MustOverride Function Bold(text As String) As String
    Public MustOverride Function Italic(text As String) As String
    Public MustOverride Function BlockQuote(text As String) As String
    Public MustOverride Function List(items As IEnumerable(Of String), orderList As Boolean) As String
    Public MustOverride Function Table(head As String(), rows As IEnumerable(Of String())) As String

End Class