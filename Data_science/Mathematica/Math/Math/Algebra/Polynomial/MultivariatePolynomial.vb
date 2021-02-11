Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    ''' <summary>
    ''' 多元多项式
    ''' 
    ''' ```
    ''' f(x1, x2, x3, ...) = a*x1 + b*x2 + c*x3 + ...
    ''' ```
    ''' </summary>
    Public Class MultivariatePolynomial : Inherits Formula

        ''' <summary>
        ''' sum(x * b) 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(ParamArray x() As Double) As Double
            Dim y As Double = 0

            For i As Integer = 0 To Factors.Length - 1
                y += x(i) * Factors(i)
            Next

            Return y
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString(format As String, Optional html As Boolean = False) As String
            Return Factors _
                .Select(Function(b, i) $"{b}*X{i + 1}") _
                .JoinBy(" + ")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
            Return Factors _
                .Select(Function(b, i) $"{b}*{variables(i)}") _
                .JoinBy(" + ")
        End Function
    End Class
End Namespace