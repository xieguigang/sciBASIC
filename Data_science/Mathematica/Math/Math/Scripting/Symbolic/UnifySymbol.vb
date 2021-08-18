Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace Scripting.MathExpression

    ''' <summary>
    ''' a unify symbol model
    ''' 
    ''' a * (x ^ n)
    ''' </summary>
    Public Class UnifySymbol : Inherits SymbolExpression

        Public Property factor As Expression = New Literal(1)
        Public Property power As Expression = New Literal(1)

        Public Sub New(symbol As SymbolExpression)
            MyBase.New(symbol.symbolName)
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return factor.Evaluate(env) * (MyBase.Evaluate(env) ^ power.Evaluate(env))
        End Function

        Public Overrides Function ToString() As String
            Return $"({factor} * ({symbolName} ^ {power}))"
        End Function
    End Class
End Namespace