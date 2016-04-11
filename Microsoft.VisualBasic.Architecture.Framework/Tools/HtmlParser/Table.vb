Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace HtmlParser

    Public Module TableParser

        Public Function GetTablesHTML(html As String) As String()
            Dim tbls As String() = Regex.Matches(html, "<table.+?</table>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return tbls
        End Function

        Public Function GetRowsHTML(table As String) As String()
            Dim rows As String() = Regex.Matches(table, "<tr.+?</tr>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return rows
        End Function

        ''' <summary>
        ''' The td tag is trimmed in this function.
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