Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.DocumentStream

Public Module DataEnumerator

    <Extension>
    Public Iterator Function [Where](df As DataFrame, query$) As IEnumerable(Of Dictionary(Of String, String))
        Dim exp As Expression = query.Build


    End Function
End Module
