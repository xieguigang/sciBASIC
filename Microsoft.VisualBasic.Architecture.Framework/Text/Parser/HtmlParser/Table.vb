#Region "Microsoft.VisualBasic::db106a8d5f98e862f0fa9905a770f5f6, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Text\Parser\HtmlParser\Table.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Text.HtmlParser

    ''' <summary>
    ''' The string parser for the table html text block
    ''' </summary>
    Public Module TableParser

        ''' <summary>
        ''' Parsing the html text betweens the tag &lt;table>&lt;/table> by using regex expression.
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetTablesHTML(html As String) As String()
            Dim tbls As String() = Regex.Matches(html, "<table.+?</table>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return tbls
        End Function

        ''' <summary>
        ''' Parsing the html text betweens the tag &lt;tr>&lt;/tr> by using regex expression.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetRowsHTML(table As String) As String()
            Dim rows As String() = Regex.Matches(table, "<tr.+?</tr>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return rows
        End Function

        ''' <summary>
        ''' The td tag is trimmed in this function.(请注意，在本函数之中，&lt;td>标签是被去除掉了的)
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetColumnsHTML(row As String) As String()
            Dim cols As String() = Regex.Matches(row, "<td.+?</td>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            cols = cols.ToArray(Function(s) s.GetValue)
            Return cols
        End Function

        Public Const PAGE_CONTENT_TITLE As String = "<title>.+</title>"

        <Extension>
        Public Function HTMLtitle(html As String) As String
            Dim title As String =
                Regex.Match(html, PAGE_CONTENT_TITLE, RegexOptions.IgnoreCase).Value

            If String.IsNullOrEmpty(title) Then
                title = "NULL_TITLE"
            Else
                title = title.GetValue
            End If

            Return title
        End Function
    End Module
End Namespace
