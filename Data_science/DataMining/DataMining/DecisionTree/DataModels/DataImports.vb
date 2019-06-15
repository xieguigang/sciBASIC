Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace DecisionTree.Data

    <HideModuleName> Public Module DataImports

        <Extension>
        Public Function [Imports](csv As IEnumerable(Of IEnumerable(Of String))) As DataTable
            Dim matrix As IEnumerable(Of String)() = csv.ToArray
            Dim headers As String() = matrix(Scan0).ToArray
            Dim data As Entity() = matrix.Skip(1) _
                .Select(Function(row)
                            Return New Entity With {
                                .entityVector = row.ToArray
                            }
                        End Function) _
                .ToArray

            Return New DataTable With {
                .headers = headers,
                .rows = data
            }
        End Function

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

        ''' <summary>
        ''' Populate validation data set from table
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function AsValidateSet(table As DataTable) As IEnumerable(Of NamedValue(Of Dictionary(Of String, String)))
            Dim properties As SeqValue(Of String)() = table.headers _
                .Take(table.columns - 1) _
                .SeqIterator _
                .ToArray

            For Each test As Entity In table.rows
                Dim result = test.decisions
                Dim query = properties.ToDictionary(Function(header) header.value, Function(column) test(column))

                Yield New NamedValue(Of Dictionary(Of String, String)) With {
                    .Name = result,
                    .Value = query
                }
            Next
        End Function
    End Module
End Namespace