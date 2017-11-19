Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Collection.Generic

    Public Class NamedValueList(Of T) : Inherits List(Of NamedValue(Of T))

        Sub New()
            Call MyBase.New()
        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.Keys.GetJson
        End Function

        Public Overloads Shared Operator +(table As Dictionary(Of String, T), list As NamedValueList(Of T)) As Dictionary(Of String, T)
            For Each item As NamedValue(Of T) In list
                Call table.Add(item.Name, item.Value)
            Next

            Return table
        End Operator
    End Class
End Namespace