Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace DocumentStream

    Public Class EntityObject : Inherits DynamicPropertyBase(Of String)
        Implements sIdEnumerable

        ''' <summary>
        ''' This object identifier
        ''' </summary>
        ''' <returns></returns>
        Public Property Identifier As String Implements sIdEnumerable.Identifier

        Public Shared Iterator Function LoadDataSet(path As String, uidMap As String) As IEnumerable(Of EntityObject)
            Dim map As New Dictionary(Of String, String) From {{uidMap, NameOf(EntityObject.Identifier)}}
            For Each x As EntityObject In path.LoadCsv(Of EntityObject)(explicit:=False, maps:=map)
                Yield x
            Next
        End Function
    End Class

    Public Class DataSet : Inherits DynamicPropertyBase(Of Double)
        Implements sIdEnumerable

        Public Property Identifier As String Implements sIdEnumerable.Identifier

        Public Shared Iterator Function LoadDataSet(path As String, uidMap As String) As IEnumerable(Of DataSet)
            Dim map As New Dictionary(Of String, String) From {{uidMap, NameOf(DataSet.Identifier)}}
            For Each x As DataSet In path.LoadCsv(Of DataSet)(explicit:=False, maps:=map)
                Yield x
            Next
        End Function
    End Class
End Namespace