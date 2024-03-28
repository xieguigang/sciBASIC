Imports System.Runtime.CompilerServices

Public Module DataTableStream

    <Extension>
    Public Sub StreamTo(Of T As Class)(list As IEnumerable(Of T), table As DataTable)

    End Sub
End Module
