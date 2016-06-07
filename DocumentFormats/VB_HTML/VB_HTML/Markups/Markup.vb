Imports Microsoft.VisualBasic.Serialization

Public Class Markup

End Class

Public Class Header
    Public Property Level As Integer
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class