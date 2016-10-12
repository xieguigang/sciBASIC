Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Structure DictionaryKey
    Implements IProperty

    Public ReadOnly Property Key As String Implements IReadOnlyId.Identity

    Sub New(name$)
        Key = name
    End Sub

    Public Sub SetValue(target As Object, value As Object) Implements IProperty.SetValue
        DirectCast(target, IDictionary)(Key) = value
    End Sub

    Public Function GetValue(target As Object) As Object Implements IProperty.GetValue
        Return DirectCast(target, IDictionary)(Key)
    End Function

    Public Overrides Function ToString() As String
        Return Key
    End Function
End Structure