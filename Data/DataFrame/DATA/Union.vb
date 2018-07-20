Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace DATA

    Public NotInheritable Class CbindProvider(Of T)

        Shared ReadOnly schema As Dictionary(Of String, PropertyInfo) = DataFramework.Schema(Of T)(PropertyAccess.Readable, True, True)

        Private Sub New()
        End Sub

        Public Shared Function Union(dataset As DataSet, obj As T) As DataSet
            Dim d As New DataSet With {
                .ID = dataset.ID,
                .Properties = New Dictionary(Of String, Double)(dataset.Properties)
            }

            Static numericFields = CbindProvider(Of T) _
                .schema _
                .Where(Function(f)
                           Return f.Value.PropertyType.IsNumericType
                       End Function) _
                .ToArray

            For Each field In numericFields
                d(field.Key) = CDbl(field.Value.GetValue(obj))
            Next

            Return d
        End Function

        Public Shared Function Union(entity As EntityObject, obj As T) As EntityObject
            Dim e As New EntityObject With {
                .ID = entity.ID,
                .Properties = New Dictionary(Of String, String)(entity.Properties)
            }

            For Each field In CbindProvider(Of T).schema
                e(field.Key) = CStr(field.Value.GetValue(obj))
            Next

            Return e
        End Function
    End Class
End Namespace