
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.DataSourceModel

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Iterator Function ColRenames(Of T)(data As IEnumerable(Of DynamicPropertyBase(Of T)), newColNames$()) As IEnumerable(Of DynamicPropertyBase(Of T))
            Dim dataframe As DynamicPropertyBase(Of T)() = data.ToArray
            Dim oldColNames As String() = dataframe _
                .Select(Function(r) r.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            If oldColNames.Length <> newColNames.Length Then
                Throw New InvalidConstraintException($"the length of old column name array is not equals to the new column name array!")
            End If

            For Each item As DynamicPropertyBase(Of T) In dataframe
                Dim renames As New Dictionary(Of String, T)
                Dim rowData As Dictionary(Of String, T) = item.Properties

                For i As Integer = 0 To oldColNames.Length - 1
                    renames.Add(newColNames(i), rowData.TryGetValue(oldColNames(i)))
                Next

                item.Properties = renames

                Yield item
            Next
        End Function
    End Module
End Namespace