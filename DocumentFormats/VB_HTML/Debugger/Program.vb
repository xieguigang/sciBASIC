#Region "Microsoft.VisualBasic::7f15698eb8e8c7788132e021167f7ca2, ..\visualbasic_App\DocumentFormats\VB_HTML\Debugger\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.MarkupLanguage
Imports Microsoft.VisualBasic.MarkupLanguage.HTML
Imports Microsoft.VisualBasic.MarkupLanguage.MarkDown
Imports Microsoft.VisualBasic.MarkupLanguage.StreamWriter

Module Program

    Sub Main()

        Dim mmmmm As New MarkupLanguage.MarkDown.Markdown(New MarkdownOptions)

        Dim hhhh = mmmmm.Transform("F:\VisualBasic_AppFramework\DocumentFormats\VB_HTML\syntax_test.md".GET)

        Dim mark As Markup = MarkdownParser.MarkdownParser("F:\VisualBasic_AppFramework\DocumentFormats\VB_HTML\syntax_test.md")

        Dim html As String = mark.ToHTML
        Call html.SaveTo("x:\test.html")

        Dim doc = HtmlDocument.Load("G:\GCModeller\GCModeller Virtual Cell System\wwwroot\gcmodeller.org\index.html")
        Call doc.Save("./trest.html")
    End Sub
End Module
