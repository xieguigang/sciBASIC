Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace DecisionTree.Data

    <HideModuleName> Public Module DataImports

        <Extension>
        Public Function [Imports](table As System.Data.DataTable) As DataTable
            Dim headers = Iterator Function() As IEnumerable(Of String)
                              For i As Integer = 0 To table.Columns.Count - 1
                                  Yield table.Columns(i).ToString
                              Next
                          End Function().ToArray
            Dim data As Entity() = table.createEntity.ToArray

            Return New DataTable With {
                .headers = headers,
                .rows = data
            }
        End Function

        <Extension>
        Private Iterator Function createEntity(table As System.Data.DataTable) As IEnumerable(Of Entity)
            Dim row As New List(Of String)

            For i As Integer = 0 To table.Rows.Count - 1
                For j As Integer = 0 To table.Columns.Count - 1
                    row += table.Rows(i)(j).ToString
                Next

                Yield New Entity With {
                    .entityVector = row.ToArray
                }

                row *= 0
            Next
        End Function
    End Module
End Namespace