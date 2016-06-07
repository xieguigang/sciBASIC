Imports Microsoft.VisualBasic.Serialization

Public Class Header : Inherits PlantText

    Public Property Level As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class