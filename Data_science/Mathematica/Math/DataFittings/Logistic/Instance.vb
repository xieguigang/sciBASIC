Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Instance

    Public Property label As Integer
    Public Property x As Double()

    Public Sub New(label As Integer, x As Integer())
        Me.label = label
        Me.x = x.Select(Function(d) CDbl(d)).ToArray
    End Sub

    Public Sub New(label As Integer, x As Double())
        Me.label = label
        Me.x = x
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{label}] {x.GetJson}"
    End Function
End Class