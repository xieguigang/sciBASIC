Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace DocumentStream

    Public Class EntityObject : Inherits DynamicPropertyBase(Of String)
        Implements sIdEnumerable

        Public Property Identifier As String Implements sIdEnumerable.Identifier

        Public Shared Iterator Function LoadDataSet(path As String, uidMap As String) As IEnumerable(Of EntityObject)
            Dim map As New Dictionary(Of String, String) From {{uidMap, NameOf(EntityObject.Identifier)}}
            For Each x As EntityObject In path.LoadCsv(Of EntityObject)(explicit:=False, maps:=map)
                Yield x
            Next
        End Function
    End Class
End Namespace