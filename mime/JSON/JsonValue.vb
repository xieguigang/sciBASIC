
''' <summary>
''' Primitive value
''' </summary>
Public Class JsonValue : Inherits JsonElement

    Public Overloads Property Value As Object

    Public Sub New()
    End Sub

    Public Sub New(obj As Object)
        Value = obj
    End Sub

    Public Overrides Function ToString() As String
        Return "{" & Scripting.ToString(Value, "null") & "}"
    End Function
End Class