Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Public Module MathFormula

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As String, Optional imputeNA As Boolean = False) As Double()
        Return table.Evaluate(Expression.Parse(formula), imputeNA)
    End Function

    <Extension>
    Public Function Evaluate(table As DataFrame, formula As Expression, Optional imputeNA As Boolean = False) As Double()
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
                Call engine.SetSymbol(var, toNumber(vectors(var)(i), imputeNA))
            Next

            result(i) = engine.Evaluate(formula)
        Next

        Return result
    End Function

    Private Function toNumber(any As Object, imputeNA As Boolean) As Double
        If any Is Nothing Then
            Return If(imputeNA, 0.0, Double.NaN)
        End If

        If DataFramework.IsNumericType(any.GetType) Then
            Dim x As Double = Conversion.Val(any)

            If x.IsNaNImaginary Then
                Return If(imputeNA, 0, x)
            Else
                Return x
            End If
        ElseIf TypeOf any Is Boolean Then
            Return If(CBool(any), 1, 0)
        Else
            Dim si As String = Microsoft.VisualBasic.Scripting.ToString(any)

            If si.StringEmpty(, True) Then
                Return If(imputeNA, 0.0, Double.NaN)
            ElseIf si.IsNumeric Then
                Return si.ParseDouble
            ElseIf si.StartsWith("<") Then
                ' 20250926 deal with some special situation, example like
                ' LC-MS/MS quantification result: <LOD
                Return 0
            Else
                Return If(imputeNA, 0.0, Double.NaN)
            End If
        End If
    End Function
End Module
