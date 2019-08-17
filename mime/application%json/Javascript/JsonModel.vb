Namespace Javascript

    ''' <summary>
    ''' The abstract model of the array/object
    ''' </summary>
    Public MustInherit Class JsonModel : Inherits JsonElement

#Region "Json property and value"
        Default Public Property Item(str As String) As JsonElement
            Get
                Return CType(Me, JsonObject)(str)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonObject)(str) = value
            End Set
        End Property

        Default Public Property Item(index As Integer) As JsonElement
            Get
                Return CType(Me, JsonArray)(index)
            End Get
            Set(value As JsonElement)
                CType(Me, JsonArray)(index) = value
            End Set
        End Property
#End Region

    End Class
End Namespace