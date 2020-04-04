Imports Microsoft.VisualBasic.Math.Scripting.Helpers

Public Class BinaryExpression : Inherits Expression

    Public ReadOnly Property left As Expression
    Public ReadOnly Property right As Expression
    Public ReadOnly Property [operator] As Char

    Sub New(left As Expression, right As Expression, op As Char)
        Me.left = left
        Me.right = right
        Me.operator = op
    End Sub

    Public Overrides Function Evaluate(env As ExpressionEngine) As Double
        Dim left As Double = Me.left.Evaluate(env)
        Dim right As Double = Me.right.Evaluate(env)
        Dim result As Double = Arithmetic.Evaluate(left, right, [operator])

        Return result
    End Function

    Public Overrides Function ToString() As String
        Return $"{left} {[operator]} {right}"
    End Function
End Class
