Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.DocumentStream

Public Module DataEnumerator

    <Extension>
    Public Iterator Function [Where](df As DataFrame, query$) As IEnumerable(Of Dictionary(Of String, String))
        Dim exp As Expression = query.Build
        Dim def As New IObject(df.SchemaOridinal.Keys)

        For Each row In df.EnumerateData
            If exp.Evaluate(def, row) Then
                Yield row
            End If
        Next
    End Function
End Module
