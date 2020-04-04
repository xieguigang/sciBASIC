Public Class Factorial : Inherits Expression

    Public ReadOnly Property factor As Integer

    Sub New(factor As String)
        Me.factor = Val(factor)
    End Sub

    Public Overrides Function Evaluate(env As ExpressionEngine) As Double
        Return VBMath.Factorial(factor)
    End Function

    Public Overrides Function ToString() As String
        If factor < 0 Then
            Return $"({factor})!"
        Else
            Return $"{factor}!"
        End If
    End Function
End Class
