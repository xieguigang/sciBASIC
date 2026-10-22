#Region "Microsoft.VisualBasic::45f3b2bffcc3294807f854490080e1cd, mime\text%markdown\MarkdownRender.vb"

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
    '    Code Lines: 25 (44.64%)
    ' Comment Lines: 22 (39.29%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 9 (16.07%)
    '     File Size: 1.93 KB


    ' Class MarkdownRender
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: GetTOC, Transform
    ' 
    '     Sub: SetImageUrlRouter
    ' 
    ' /********************************************************************************/

#End Region

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
    ''' Both ATX style headers (``#`` to ``######``) and setext style headers are
    ''' supported; the scan is shared with <see cref="TOC.GenerateToc"/> through
    ''' <see cref="MarkdownHeadings.Parse"/>.
    ''' </summary>
    ''' <param name="md">The markdown source text.</param>
    ''' <returns>标题文本及其层级。</returns>
    Public Shared Iterator Function GetTOC(md As String) As IEnumerable(Of NamedValue(Of Integer))
        For Each heading As HeadingInfo In MarkdownHeadings.Parse(md)
            Yield New NamedValue(Of Integer)(heading.Text, heading.Level)
        Next
    End Function
End Class
