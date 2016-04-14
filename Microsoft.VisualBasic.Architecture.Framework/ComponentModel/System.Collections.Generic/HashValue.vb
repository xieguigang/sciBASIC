Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization

Public Structure HashValue
    Implements sIdEnumerable

    Public Property Identifier As String Implements sIdEnumerable.Identifier
    Public Property value As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Operator +(hash As Dictionary(Of String, String), tag As HashValue) As Dictionary(Of String, String)
        Call hash.Add(tag.Identifier, tag.value)
        Return hash
    End Operator
End Structure
