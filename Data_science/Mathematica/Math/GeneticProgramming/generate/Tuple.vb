Public Class Tuple

    Public ReadOnly x As Double
    Public ReadOnly y As Double

    Public Sub New(x As Double, y As Double)
        Me.x = x
        Me.y = y
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("[{0:g},{1:g}]", x, y)
    End Function

End Class
