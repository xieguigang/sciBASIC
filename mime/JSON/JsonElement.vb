
Public Class JsonElement

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

    Public Property Value() As Object
        Get
            Return CType(Me, JsonValue).Value
        End Get
        Set(value As Object)
            CType(Me, JsonValue).Value = value
        End Set
    End Property

    Public Function BuildJsonString() As String
        Return ""
    End Function
End Class