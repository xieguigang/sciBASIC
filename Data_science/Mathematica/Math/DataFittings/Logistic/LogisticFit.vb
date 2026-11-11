#Region "Microsoft.VisualBasic::9af27ab260647b89731babeada3a2abb, Data_science\Mathematica\Math\DataFittings\Logistic\LogisticFit.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Logistic

    ''' <summary>
    ''' 逻辑回归的 <see cref="IFitted"/> 适配结果。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <c>GetY</c>、<c>Logistic.predict</c> 与 <c>ErrorTest.Yfit</c> 全部经由
    ''' 同一个 <c>PredictProbability</c> 实现计算，返回同一个正类概率
    ''' <c>P(y=1|x)</c>；历史上三者方向不一致的问题已消除。
    ''' </para>
    ''' <para>
    ''' <c>R2</c> 现在由误差记录按 <c>1 - SSE/SST</c> 真实计算（与
    ''' <c>RidgeFit</c> / <c>LMA</c> 的既有惯例一致），不再是硬编码的 1；
    ''' 分类任务更合适的指标另见 <c>LogLoss</c> 与 <c>Accuracy</c>。
    ''' </para>
    ''' </remarks>
    Public Class LogisticFit : Implements IFitted

        ''' <summary>由 <c>ErrorTest</c> 计算的类 R2 指标</summary>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                Dim test As IFitError() = ErrorTest

                If test Is Nothing OrElse test.Length = 0 Then
                    Return 0.0R
                End If

                Dim meanY As Double = 0.0R
                Dim sse As Double = 0.0R
                Dim sst As Double = 0.0R

                For Each e In test
                    meanY += e.Y
                Next

                meanY /= test.Length

                For Each e In test
                    sse += (e.Y - e.Yfit) ^ 2
                    sst += (e.Y - meanY) ^ 2
                Next

                Dim value As Double = If(sst > 0.0R,
                                         1.0R - sse / sst,
                                         If(sse <= 0.0R, 1.0R, 0.0R))

                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Return 0.0R
                End If

                Return value
            End Get
        End Property

        Public Property Polynomial As Formula Implements IFitted.Polynomial
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        ''' <summary>独立截距标量（不占用 <c>Formula.Factors</c> 的位置）</summary>
        Public Property Intercept As Double
        ''' <summary>训练与推理共用的按列标准化参数</summary>
        Public Property Standardization As Standardization
        ''' <summary>训练集上的平均交叉熵（数值稳定的 softplus 形式）</summary>
        Public Property LogLoss As Double

        ''' <summary>以 0.5 为阈值的训练集分类准确率</summary>
        Public ReadOnly Property Accuracy As Double
            Get
                Dim test As IFitError() = ErrorTest

                If test Is Nothing OrElse test.Length = 0 Then
                    Return 0.0R
                End If

                Dim hit As Integer = 0

                For Each e In test
                    If If(e.Yfit >= 0.5R, 1, 0) = If(e.Y >= 0.5R, 1, 0) Then
                        hit += 1
                    End If
                Next

                Return hit / CDbl(test.Length)
            End Get
        End Property

        ''' <summary>
        ''' 返回正类概率 <c>sigmoid(w*x + b)</c>。这是模型唯一的概率出口。
        ''' </summary>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Return PredictProbability(Polynomial.Factors, Intercept, Standardization, x)
        End Function

        ''' <summary>共享的概率实现</summary>
        Friend Shared Function PredictProbability(factors As Double(),
                                                  intercept As Double,
                                                  norm As Standardization,
                                                  x As Double()) As Double
            Dim logit As Double = 0.0R
            Return PredictProbability(factors, intercept, norm, x, logit)
        End Function

        ''' <summary>共享的概率实现，同时带出 logit 以便一次性计算交叉熵</summary>
        Friend Shared Function PredictProbability(factors As Double(),
                                                  intercept As Double,
                                                  norm As Standardization,
                                                  x As Double(),
                                                  ByRef logit As Double) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x), "the feature vector can not be NULL!")
            End If
            If factors Is Nothing Then
                Throw New ArgumentNullException(NameOf(factors), "the model coefficients can not be NULL!")
            End If

            Dim row As Double() = If(norm.IsEmpty, x, norm.Apply(x))

            If row.Length <> factors.Length Then
                Throw New ArgumentException($"feature size not agree: {row.Length} vs {factors.Length}!", NameOf(x))
            End If

            logit = SimdReduce.Dot(factors, row) + intercept

            Return Logistic.sigmoid(logit)
        End Function

        ''' <summary>构造拟合结果：预分配误差数组，避免两层 LINQ 与 DirectCast 链</summary>
        Friend Shared Function CreateFit(log As Logistic, matrix As Instance()) As LogisticFit
            Dim factors As Double() = log.theta.ToArray()
            Dim norm As Standardization = log.Standardize
            Dim fit As New LogisticFit With {
                .Polynomial = New Polynomial With {.Factors = factors},
                .Intercept = log.Intercept,
                .Standardization = norm
            }
            Dim test As IFitError() = New IFitError(matrix.Length - 1) {}
            Dim lossSum As Double = 0.0R

            For i As Integer = 0 To matrix.Length - 1
                Dim item As Instance = matrix(i)
                Dim logit As Double = 0.0R
                Dim p As Double = PredictProbability(factors, fit.Intercept, norm, item.x, logit)

                test(i) = New [Error] With {
                    .X = item.x.AsVector,
                    .Y = item.label,
                    .Yfit = p
                }

                ' softplus(z) - y*z，用 exp(-|z|) 做区间约减，杜绝 log(0)
                lossSum += std.Max(logit, 0.0R) - item.label * logit +
                           std.Log(1.0R + std.Exp(-std.Abs(logit)))
            Next

            fit.ErrorTest = test
            fit.LogLoss = lossSum / matrix.Length

            Return fit
        End Function
    End Class
End Namespace
