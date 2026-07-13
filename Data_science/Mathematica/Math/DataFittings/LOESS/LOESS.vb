#Region "Microsoft.VisualBasic::db0db23a9c4750b7097a7eea79af7864, Data_science\Mathematica\Math\DataFittings\LOESS\LOESS.vb"

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

    '   Total Lines: 204
    '    Code Lines: 127 (62.25%)
    ' Comment Lines: 48 (23.53%)
    '    - Xml Docs: 60.42%
    ' 
    '   Blank Lines: 29 (14.22%)
    '     File Size: 7.36 KB


    ' Module LOESS
    ' 
    '     Function: FitLOESS, PredictLOESS, SolveLinearSystem, WeightedPolynomialFit
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' ========================================================================
'''    LOESS回归实现
''' ========================================================================
''' </summary>
Public Module LOESS

    ''' <summary>
    ''' 拟合LOESS模型
    ''' 
    ''' LOESS（LOcally Estimated Scatterplot Smoothing）是一种非参数的局部回归方法。
    ''' 对于每个预测点，在其邻域内使用加权最小二乘法拟合一个低阶多项式，
    ''' 权重随距离增加而减小（使用三立方核函数）。
    ''' </summary>
    ''' <param name="x">自变量数组</param>
    ''' <param name="y">因变量数组</param>
    ''' <param name="span">带宽参数（0~1），控制邻域大小</param>
    ''' <param name="degree">多项式阶数（1或2）</param>
    ''' <returns>LOESS模型</returns>
    Public Function FitLOESS(x As Double(), y As Double(), span As Double, degree As Integer) As LOESSModel
        If x.Length <> y.Length Then
            Throw New ArgumentException("x和y数组长度必须相同")
        End If

        ' 按x排序
        Dim indices As Integer() = Enumerable.Range(0, x.Length).OrderBy(Function(i) x(i)).ToArray()
        Dim sortedX As Double() = New Double(x.Length - 1) {}
        Dim sortedY As Double() = New Double(y.Length - 1) {}

        For i As Integer = 0 To indices.Length - 1
            sortedX(i) = x(indices(i))
            sortedY(i) = y(indices(i))
        Next

        Dim model As New LOESSModel()
        model.X = sortedX
        model.Y = sortedY
        model.Span = std.Max(0.1, std.Min(1.0, span))
        model.Degree = degree

        Return model
    End Function

    ''' <summary>
    ''' 使用LOESS模型进行预测
    ''' </summary>
    ''' <param name="model">LOESS模型</param>
    ''' <param name="x0">预测点</param>
    ''' <returns>预测值</returns>
    <Extension>
    Public Function PredictLOESS(model As LOESSModel, x0 As Double) As Double
        Dim n As Integer = model.X.Length
        Dim k As Integer = CInt(std.Ceiling(model.Span * n))
        k = std.Max(k, model.Degree + 1) ' 确保有足够的点拟合多项式
        k = std.Min(k, n)

        ' 找到距离x0最近的k个点
        Dim distances As Double() = New Double(n - 1) {}
        For i As Integer = 0 To n - 1
            distances(i) = std.Abs(model.X(i) - x0)
        Next

        Dim sortedIndices As Integer() = Enumerable.Range(0, n).OrderBy(Function(i) distances(i)).Take(k).ToArray()
        Dim maxDist As Double = distances(sortedIndices(k - 1))

        ' 避免除零
        If maxDist < Double.Epsilon Then
            ' x0与某个训练点完全重合
            For i As Integer = 0 To k - 1
                If distances(sortedIndices(i)) < Double.Epsilon Then
                    Return model.Y(sortedIndices(i))
                End If
            Next
            Return model.Y(sortedIndices(0))
        End If

        ' 计算三立方核权重
        Dim weights As Double() = New Double(k - 1) {}
        For i As Integer = 0 To k - 1
            Dim u As Double = distances(sortedIndices(i)) / maxDist
            ' 三立方核函数: w(u) = (1 - u^3)^3
            weights(i) = std.Pow(1.0 - std.Pow(u, 3), 3)
        Next

        ' 加权最小二乘法拟合多项式
        Dim xPts As Double() = New Double(k - 1) {}
        Dim yPts As Double() = New Double(k - 1) {}
        For i As Integer = 0 To k - 1
            xPts(i) = model.X(sortedIndices(i))
            yPts(i) = model.Y(sortedIndices(i))
        Next

        ' 使用加权最小二乘法
        Dim coeffs As Double() = WeightedPolynomialFit(xPts, yPts, weights, model.Degree)

        ' 在x0处预测
        Dim y0 As Double = 0.0
        For j As Integer = 0 To coeffs.Length - 1
            y0 += coeffs(j) * std.Pow(x0, j)
        Next

        Return y0
    End Function

    ''' <summary>
    ''' 加权多项式拟合
    ''' 使用正规方程求解加权最小二乘问题
    ''' </summary>
    Private Function WeightedPolynomialFit(x As Double(), y As Double(), w As Double(), degree As Integer) As Double()
        Dim n As Integer = x.Length
        Dim m As Integer = degree + 1 ' 系数个数

        ' 构建正规方程 (X^T W X) * beta = X^T W y
        Dim xtwx As Double(,) = New Double(m - 1, m - 1) {}
        Dim xtwy As Double() = New Double(m - 1) {}

        For i As Integer = 0 To n - 1
            Dim xi As Double() = New Double(m - 1) {}
            For j As Integer = 0 To m - 1
                xi(j) = std.Pow(x(i), j)
            Next

            For j As Integer = 0 To m - 1
                xtwy(j) += w(i) * xi(j) * y(i)
                For k As Integer = 0 To m - 1
                    xtwx(j, k) += w(i) * xi(j) * xi(k)
                Next
            Next
        Next

        ' 使用高斯消元法求解线性方程组
        Return SolveLinearSystem(xtwx, xtwy)
    End Function

    ''' <summary>
    ''' 高斯消元法求解线性方程组 Ax = b
    ''' </summary>
    Private Function SolveLinearSystem(A As Double(,), b As Double()) As Double()
        Dim n As Integer = b.Length
        ' 创建增广矩阵
        Dim aug As Double(,) = New Double(n - 1, n) {}

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                aug(i, j) = A(i, j)
            Next
            aug(i, n) = b(i)
        Next

        ' 前向消元（部分主元选取）
        For col As Integer = 0 To n - 1
            ' 找到主元
            Dim maxRow As Integer = col
            Dim maxVal As Double = std.Abs(aug(col, col))
            For row As Integer = col + 1 To n - 1
                If std.Abs(aug(row, col)) > maxVal Then
                    maxVal = std.Abs(aug(row, col))
                    maxRow = row
                End If
            Next

            ' 交换行
            If maxRow <> col Then
                For j As Integer = col To n
                    Dim temp As Double = aug(col, j)
                    aug(col, j) = aug(maxRow, j)
                    aug(maxRow, j) = temp
                Next
            End If

            ' 消元
            Dim pivot As Double = aug(col, col)
            If std.Abs(pivot) < Double.Epsilon Then
                ' 奇异矩阵，返回零向量
                Dim result As Double() = New Double(n - 1) {}
                Return result
            End If

            For row As Integer = col + 1 To n - 1
                Dim factor As Double = aug(row, col) / pivot
                For j As Integer = col To n
                    aug(row, j) -= factor * aug(col, j)
                Next
            Next
        Next

        ' 回代
        Dim x As Double() = New Double(n - 1) {}
        For i As Integer = n - 1 To 0 Step -1
            x(i) = aug(i, n)
            For j As Integer = i + 1 To n - 1
                x(i) -= aug(i, j) * x(j)
            Next
            x(i) /= aug(i, i)
        Next

        Return x
    End Function

End Module
