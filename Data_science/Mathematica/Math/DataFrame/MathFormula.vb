Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Public Module MathFormula

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As String) As Double()
        Return table.Evaluate(Expression.Parse(formula))
    End Function

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As Expression) As Double()
        Dim varNames As String() = formula.GetVariableSymbols.Distinct.ToArray
        Dim vectors As Dictionary(Of String, Func(Of Integer, Object)) = varNames _
            .ToDictionary(Function(name) name,
                          Function(name)
                              Return table(name).Getter
                          End Function)
        Dim nlen As Integer = table.nsamples
        Dim result As Double() = New Double(nlen - 1) {}
        Dim engine As New ExpressionEngine

        For i As Integer = 0 To nlen - 1
            For Each var As String In varNames
                Call engine.SetSymbol(var, Val(vectors(var)(i)))
            Next

            result(i) = engine.Evaluate(formula)
        Next

        Return result
    End Function
End Module
