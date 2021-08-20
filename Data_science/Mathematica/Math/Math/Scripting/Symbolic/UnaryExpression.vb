Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting.MathExpression

    Public Class UnaryExpression : Inherits Expression

        Public Property [operator] As String
        Public Property value As Expression

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim value As Double = Me.value.Evaluate(env)

            Select Case [operator]
                Case "+" : Return value
                Case "-" : Return -value
                Case Else
                    Throw New NotImplementedException([operator])
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{[operator]}{value}"
        End Function
    End Class
End Namespace