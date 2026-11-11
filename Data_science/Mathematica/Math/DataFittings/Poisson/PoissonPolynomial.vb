Imports System.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Poisson

    ''' <summary>
    ''' 泊松回归模型的对数链接多项式：``mu = exp(c + b1*x1 + ... + bp*xp)``。
    ''' 
    ''' 与 <see cref="Ridge.RidgePolynomial"/> 等线性多项式不同，这里的
    ''' <see cref="Evaluate"/> 返回的是期望计数（单位暴露量之下的事件率），
    ''' 从而保证 <see cref="IFitted.GetY"/> 与二维表预测写回的语义一致：
    ''' 对数链接把取值域从实数映射到 ``(0, +Infinity)``，预测值恒为正。
    ''' </summary>
    Public Class PoissonPolynomial : Inherits Formula

        ''' <summary>
        ''' 模型的截距项（对数尺度上的基准率）
        ''' </summary>
        ''' <returns></returns>
        Public Property Intercept As Double

        ''' <summary>
        ''' mu = exp(c + sum(x * b))
        ''' </summary>
        ''' <param name="x">自变量向量，顺序与 <see cref="Formula.Factors"/> 一致</param>
        ''' <returns>单位暴露量之下的期望计数</returns>
        Public Overrides Function Evaluate(ParamArray x() As Double) As Double
            Dim eta As Double = Intercept
            Dim n As Integer = If(Factors Is Nothing, 0, Factors.Length)

            If x IsNot Nothing AndAlso x.Length < n Then
                Throw New ArgumentException($"the poisson model requires {n} feature values, but only {x.Length} were given!", NameOf(x))
            End If

            For i As Integer = 0 To n - 1
                eta += x(i) * Factors(i)
            Next

            Return std.Exp(PoissonFitting.ClampEta(eta))
        End Function

        ''' <summary>
        ''' 输出以 ``X1..Xn`` 为自变量名的公式文本
        ''' </summary>
        Public Overrides Function ToString(format As String, Optional html As Boolean = False) As String
            Return ToString(Nothing, format, html)
        End Function

        ''' <summary>
        ''' 输出以给定列名为自变量名的公式文本（``html`` 为接口兼容参数，不参与格式化）
        ''' </summary>
        Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
            Dim terms As New StringBuilder()
            Dim n As Integer = If(Factors Is Nothing, 0, Factors.Length)

            For i As Integer = 0 To n - 1
                If i > 0 Then
                    Call terms.Append(" + ")
                End If

                Dim name As String = If(variables IsNot Nothing AndAlso variables.Length > i,
                                        variables(i), $"X{i + 1}")

                Call terms.Append(Factors(i).ToString(format)).Append("*").Append(name)
            Next

            If n = 0 Then
                Return $"exp({Intercept.ToString(format)})"
            End If

            Return $"exp({Intercept.ToString(format)} + {terms})"
        End Function
    End Class
End Namespace
