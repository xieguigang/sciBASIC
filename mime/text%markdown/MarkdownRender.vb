Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Markdown text document transform its markup language format into html text format.
''' </summary>
Public Class MarkdownRender

    Dim render As Render

    Sub New()
        Call Me.New(New HtmlRender)
    End Sub

    Sub New(render As Render)
        Me.render = render
    End Sub

    ''' <summary>
    ''' Set a url router for process the image url location its target location in the
    ''' output html document.
    ''' </summary>
    Public Sub SetImageUrlRouter(router As Func(Of String, String))
        render.SetImageUrlRouter(router)
    End Sub

    'Public Function SetImageUrlRouter(router As Func(Of String, String)) As Render
    '    Call render.SetImageUrlRouter(router)
    '    Return render
    'End Function

    ''' <summary>
    ''' Transform the markdown text into html text.
    ''' </summary>
    ''' <param name="markdown"></param>
    ''' <returns></returns>
    Public Function Transform(markdown As String) As String
        Dim parser As New MarkdownParser(render)
        Return render.Document(parser.Parse(markdown))
    End Function

    ''' <summary>
    ''' Get table of content of the target markdown document.
    ''' (Only ATX style headers ``#`` to ``######`` are supported.)
    ''' </summary>
    ''' <param name="md"></param>
    ''' <returns></returns>
    Shared ReadOnly headers As New Regex("^[#]+.+$", RegexOptions.Compiled Or RegexOptions.Multiline)

    Public Shared Iterator Function GetTOC(md As String) As IEnumerable(Of NamedValue(Of Integer))
        For Each level As String In headers.Matches(md).ToArray
            Yield New NamedValue(Of Integer)(level.Trim(" "c, "#"c), level.TakeWhile(Function(c) c = "#"c).Count)
        Next
    End Function
End Class
