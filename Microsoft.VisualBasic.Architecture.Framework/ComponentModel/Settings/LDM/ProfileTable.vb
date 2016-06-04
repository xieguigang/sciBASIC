Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Settings

    Public Class ProfileTable : Implements sIdEnumerable
        Implements IProfileTable

        Public Property Name As String Implements sIdEnumerable.Identifier, IProfileTable.Name
        Public Property value As String Implements IProfileTable.value
        Public Property Type As ValueTypes Implements IProfileTable.Type
        Public Property Description As String Implements IProfileTable.Description

        Sub New()
        End Sub

        Sub New(x As BindMapping)
            Name = x.Name
            value = x.Value
            Type = x.Type
            Description = x.Description
        End Sub

        Public Sub SetValue(Of T As IProfile)(config As Settings(Of T))
            Call config.Set(Name.ToLower, value)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Interface IProfileTable

        Property Name As String
        ReadOnly Property value As String
        Property Type As ValueTypes
        Property Description As String

    End Interface
End Namespace