Imports System.Reflection

Public Class AttributeMetadata

    Public Property name As String
    Public Property type As String
    Public Property data As Byte()

    Public ReadOnly Property GetUnderlyingType As Type
        Get
            If type.StringEmpty Then
                Return GetType(Void)
            Else
                Return TypeInfo.GetType(type)
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"Dim {name} As {type} = binary({StringFormats.Lanudry(data.Length)})"
    End Function

End Class