Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Ridge

    ''' <summary>
    ''' 岭回归模型的多项式形式：``y = c + b1*x1 + b2*x2 + ...``
    ''' 
    ''' 既有的 <see cref="MultivariatePolynomial"/> 只计算 ``sum(x * b)`` 而没有
    ''' 截距项，而岭回归的截距是不施加惩罚的独立参数，因此这里单独给出一个
    ''' 带截距的多元多项式。
    ''' </summary>
    Public Class RidgePolynomial : Inherits Formula

        ''' <summary>
        ''' 模型的截距项
        ''' </summary>
        ''' <returns></returns>
        Public Property Intercept As Double

        ''' <summary>
        ''' y = c + sum(x * b)
        ''' </summary>
        ''' <param name="x">自变量向量</param>
        ''' <returns>模型对 x 的估计值</returns>
        Public Overrides Function Evaluate(ParamArray x() As Double) As Double
            Dim y As Double = Intercept

            For i As Integer = 0 To Factors.Length - 1
                y += x(i) * Factors(i)
            Next

            Return y
        End Function

        Public Overrides Function ToString(format As String, Optional html As Boolean = False) As String
            Return Intercept.ToString(format) & " + " & Factors _
                .Select(Function(b, i) $"{b.ToString(format)}*X{i + 1}") _
                .JoinBy(" + ")
        End Function

        Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
            Return Intercept.ToString(format) & " + " & Factors _
                .Select(Function(b, i) $"{b.ToString(format)}*{variables(i)}") _
                .JoinBy(" + ")
        End Function
    End Class
End Namespace
