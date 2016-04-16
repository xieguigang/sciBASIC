Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace HtmlParser

    ''' <summary>
    ''' The string parser for the table html text block
    ''' </summary>
    Public Module TableParser

        ''' <summary>
        ''' Parsing the html text betweens the tag &lt;table>&lt;/table> by using regex expression.
        ''' </summary>
        ''' <param name="html"></param>
        ''' <returns></returns>
        Public Function GetTablesHTML(html As String) As String()
            Dim tbls As String() = Regex.Matches(html, "<table.+?</table>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return tbls
        End Function

        ''' <summary>
        ''' Parsing the html text betweens the tag &lt;tr>&lt;/tr> by using regex expression.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Function GetRowsHTML(table As String) As String()
            Dim rows As String() = Regex.Matches(table, "<tr.+?</tr>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return rows
        End Function

        ''' <summary>
        ''' The td tag is trimmed in this function.(请注意，在本函数之中，&lt;td>标签是被去除掉了的)
        ''' </summary>
        ''' <param name="row"></param>
        ''' <returns></returns>
        Public Function GetColumnsHTML(row As String) As String()
            Dim cols As String() = Regex.Matches(row, "<td.+?</td>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            cols = cols.ToArray(Function(s) s.GetValue)
            Return cols
        End Function
    End Module
End Namespace