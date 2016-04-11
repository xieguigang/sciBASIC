Imports System.Text.RegularExpressions

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

        Public Function GetColumnsHTML(row As String) As String()
            Dim cols As String() = Regex.Matches(row, "<td.+?</td>", RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToArray
            Return cols
        End Function
    End Module
End Namespace