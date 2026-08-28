#Region "Microsoft.VisualBasic::b90cb22a2d18e00d87e83e68ef6df784, Data_science\Mathematica\Math\DataFittings\Linear\MLR\LinearFitting.vb"

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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 101
    '    Code Lines: 56 (55.45%)
    ' Comment Lines: 35 (34.65%)
    '    - Xml Docs: 88.57%
    ' 
    '   Blank Lines: 10 (9.90%)
    '     File Size: 3.24 KB


    '     Module LinearFittingAlgorithm
    ' 
    '         Function: ConfidenceInterval, CurveScale, left, (+2 Overloads) LinearFitting, right
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Multivariate

    ''' <summary>
    ''' 多元线性回归拟合算法模块，提供拟合入口、特征曲线升维以及回归系数置信区间的计算。
    ''' (Multiple linear regression fitting algorithms: fit entry points, feature curve scaling, and confidence interval computation.)
    ''' </summary>
    Public Module LinearFittingAlgorithm

        ''' <summary>
        ''' 使用二维自变量数组 x 与观测值数组 y 执行多元线性回归拟合。(Performs multiple linear regression fitting using a 2D feature array and an observed value array.)
        ''' </summary>
        ''' <param name="x">自变量二维数组，每一行代表一个样本点的特征向量。(The 2D feature array, each row is a sample's feature vector.)</param>
        ''' <param name="y">与 x 各行对应的观测值数组。(The observed values corresponding to each row of x.)</param>
        ''' <returns>拟合得到的 <see cref="MLRFit"/> 模型。(The fitted multiple linear regression model.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LinearFitting(x As Double(,), y#()) As MLRFit
            Return New NumericMatrix(x.RowIterator.ToArray).LinearFitting(y)
        End Function

        ''' <summary>
        ''' 对给定的自变量矩阵与观测值向量执行多元线性回归(最小二乘)拟合，并返回拟合模型及其误差测试结果。
        ''' (Performs ordinary least squares multiple linear regression on the given feature matrix and observed values, returning the fitted model and its fit errors.)
        ''' </summary>
        ''' <param name="x">
        ''' 自变量矩阵，每一行是一个样本点，每一列是一个特征维度。例如：
        ''' (The feature matrix, each row is a sample point and each column is a feature dimension. e.g.)
        ''' 
        ''' ```
        ''' x1  x2  x3  x4
        '''  a   b   c   d
        '''  a   b   c   d
        '''  a   b   c   d 
        '''  a   b   c   d
        '''  a   b   c   d 
        ''' ```
        ''' </param>
        ''' <param name="f">
        ''' 观测值向量，与矩阵 x 的每一行一一对应。例如：
        ''' (The observed value vector, corresponding to each row of matrix x. e.g.)
        ''' 
        ''' ```
        ''' y1
        ''' y2
        ''' y3
        ''' y4
        ''' y5
        ''' ```
        ''' </param>
        ''' <returns>包含回归系数、拟合优度及逐点误差的 <see cref="MLRFit"/> 实例。(An <see cref="MLRFit"/> instance with regression coefficients, goodness-of-fit and per-point errors.)</returns>
        ''' 
        <Extension>
        Public Function LinearFitting(x As NumericMatrix, f As Vector) As MLRFit
            Dim N = f.Length
            Dim p = x.ColumnDimension
            Dim Y As New NumericMatrix(f, N)
            Dim mean# = f.Average
            Dim beta = x.QRD.Solve(Y)
            Dim SST = ((f - mean) ^ 2).Sum
            Dim residuals As NumericMatrix = x.Multiply(B:=beta) - Y
            Dim SSE = residuals.Norm2 ^ 2
            Dim MLR As New MLRFit With {
                .beta = x.ColumnDimension _
                    .Sequence _
                    .Select(Function(i) beta(i, 0)) _
                    .ToArray,
                .N = N,
                .p = p,
                .SSE = SSE,
                .SST = SST
            }

            MLR.ErrorTest = [Error] _
                .RunTest(MLR, x, f) _
                .Select(Function(pt) DirectCast(pt, IFitError)) _
                .ToArray

            Return MLR
        End Function

        ''' <summary>
        ''' 将自变量序列升维为多项式特征向量，用于构造形如以下形式的非线性(多项式)拟合：
        ''' (Scales the input sequence into a polynomial feature vector for building nonlinear/polynomial fits of the form:)
        ''' ```
        ''' h(X) = a + b*x1 + c*x2^2 + d*x3^3 + ... 
        ''' ```
        ''' </summary>
        ''' <param name="X">原始自变量序列。(The original independent variable sequence.)</param>
        ''' <returns>升维后的特征向量，第 i 个元素为 x^(i+1)。(The scaled feature vector where the i-th element is x^(i+1).)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CurveScale(X As IEnumerable(Of Double)) As Vector
            Return X.Select(Function(xi, i) xi ^ (i + 1)).AsVector
        End Function

        ''' <summary>
        ''' 计算回归系数置信区间的左边界。(Computes the lower bound of the confidence interval for a regression coefficient.)
        ''' </summary>
        ''' <param name="beta">回归系数估计值。(The estimated regression coefficient.)</param>
        ''' <param name="t">t 分布临界值(对应给定置信水平与自由度)。(The t-distribution critical value for the given confidence level and degrees of freedom.)</param>
        ''' <param name="S">回归系数的标准误。(The standard error of the regression coefficient.)</param>
        ''' <returns>置信区间下界 beta - t*S。(The lower bound beta - t*S.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function left(beta#, t#, S#) As Double
            Return beta - (t * S)
        End Function

        ''' <summary>
        ''' 计算回归系数置信区间的右边界。(Computes the upper bound of the confidence interval for a regression coefficient.)
        ''' </summary>
        ''' <param name="beta">回归系数估计值。(The estimated regression coefficient.)</param>
        ''' <param name="t">t 分布临界值(对应给定置信水平与自由度)。(The t-distribution critical value for the given confidence level and degrees of freedom.)</param>
        ''' <param name="S">回归系数的标准误。(The standard error of the regression coefficient.)</param>
        ''' <returns>置信区间上界 beta + t*S。(The upper bound beta + t*S.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function right(beta#, t#, S#) As Double
            Return beta + (t * S)
        End Function

        ''' <summary>
        ''' 计算回归系数在给定置信水平下的置信区间 [beta - t*S, beta + t*S]。(Computes the confidence interval [beta - t*S, beta + t*S] for a regression coefficient at the given confidence level.)
        ''' </summary>
        ''' <param name="beta">回归系数估计值。(The estimated regression coefficient.)</param>
        ''' <param name="t">t 分布临界值(对应给定置信水平与自由度)。(The t-distribution critical value for the given confidence level and degrees of freedom.)</param>
        ''' <param name="S">回归系数的标准误。(The standard error of the regression coefficient.)</param>
        ''' <returns>由下界与上界组成的 <see cref="DoubleRange"/> 置信区间。(A <see cref="DoubleRange"/> confidence interval consisting of the lower and upper bounds.)</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ConfidenceInterval(beta#, t#, S#) As DoubleRange
            Return {left(beta, t, S), right(beta, t, S)}
        End Function
    End Module
End Namespace
