Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Public Module MathFormula

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As String) As Double()
        Return table.Evaluate(Expression.Parse(formula))
    End Function

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As Expression) As Double()

    End Function

End Module
