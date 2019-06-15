#Region "Microsoft.VisualBasic::3b8d61b2e49b24f41dbcb2d1e65263b8, Microsoft.VisualBasic.Core\Text\Parser\HtmlParser\Table.vb"

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

    '     Module TableParser
    ' 
    '         Function: GetColumnsHTML, GetRowsHTML, GetTablesHTML, ParseHtmlMeta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports r = System.Text.RegularExpressions.Regex

Namespace Text.Parser.HtmlParser

    ''' <summary>
    ''' The string parser for the table html text block
    ''' </summary>
    Public Module TableParser

        ''' <summary>
        ''' Parsing the html text betweens the tag ``&lt;table>&lt;/table>`` by using regex expression.
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetTablesHTML(html As String, Optional greedy As Boolean = False) As String()
            Dim regxp As String = If(greedy, "<table.+</table>", "<table.+?</table>")
            Dim tbls As String() = r.Matches(html, regxp, RegexICSng).ToArray
            Return tbls
        End Function

        Const RowPatterns$ = "<tr.+?</tr>"
        Const ColumnPatterns$ = "(<td.+?</td>)|(<th.+?</th>)"

        ''' <summary>
        ''' Parsing the html text betweens the tag ``&lt;tr>&lt;/tr>`` by using regex expression.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetRowsHTML(table As String) As String()
            If table Is Nothing Then
                Return {}
            Else
                Dim rows As String() = r.Matches(table, RowPatterns, RegexICSng).ToArray
                Return rows
            End If
        End Function

        ''' <summary>
        ''' The td tag is trimmed in this function.(请注意，在本函数之中，``&lt;td>``标签是被去除掉了的)
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GetColumnsHTML(row As String) As String()
            Dim cols As String() = r.Matches(row, ColumnPatterns, RegexICSng).ToArray
            cols = cols _
                .Select(Function(s) s.GetValue) _
                .ToArray
            Return cols
        End Function

        Const metaPattern$ = "<meta .+?/>"

        <Extension>
        Public Function ParseHtmlMeta(html As String) As Dictionary(Of String, String)
            Dim list = html.Matches(metaPattern, RegexICSng).ToArray
            Dim attrs As NamedValue(Of String)() = list _
                .Select(Function(meta) meta.TagAttributes) _
                .Select(Function(meta)
                            Dim name = meta.FirstOrDefault(Function(a) a.Name.TextEquals("name"))
                            Dim content = meta.FirstOrDefault(Function(a) a.Name.TextEquals("content"))

                            Return New NamedValue(Of String)(name, content)
                        End Function) _
                .Where(Function(meta) Not meta.Name.StringEmpty) _
                .ToArray
            Dim table = attrs.ToDictionary(Function(a) a.Name, Function(a) a.Value)

            Return table
        End Function
    End Module
End Namespace
