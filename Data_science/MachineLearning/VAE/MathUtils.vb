#Region "Microsoft.VisualBasic::00a1e90a2a41aad4fb5bdca0f696a4de, Data_science\MachineLearning\VAE\MathUtils.vb"

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

    '   Total Lines: 158
    '    Code Lines: 105 (66.46%)
    ' Comment Lines: 33 (20.89%)
    '    - Xml Docs: 45.45%
    ' 
    '   Blank Lines: 20 (12.66%)
    '     File Size: 4.61 KB


    ' Module MathUtils
    ' 
    '     Function: Determinant, EuclideanDistance, MatrixInverse, Randn
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' MathUtils.vb - 数学辅助函数库
'
' 提供 GMM 和 GMVAE 共用的数学工具函数
' 作者: Qingyan Agent
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 数学辅助函数库
''' </summary>
Public Module MathUtils

    ''' <summary>
    ''' 计算矩阵的行列式 (使用 LU 分解)
    ''' </summary>
    Public Function Determinant(mat As Tensor) As Double
        Dim n = mat.Shape(0)
        If mat.Shape(1) <> n Then
            Throw New ArgumentException("行列式计算需要方阵")
        End If

        ' 复制矩阵
        Dim a = New Double(n - 1, n - 1) {}
        For i = 0 To n - 1
            For j = 0 To n - 1
                a(i, j) = mat(i, j)
            Next
        Next

        Dim det As Double = 1
        For i = 0 To n - 1
            ' 找主元
            Dim maxRow = i
            For k = i + 1 To n - 1
                If std.Abs(a(k, i)) > std.Abs(a(maxRow, i)) Then maxRow = k
            Next

            ' 交换行
            If maxRow <> i Then
                For k = 0 To n - 1
                    Dim tmp = a(i, k)
                    a(i, k) = a(maxRow, k)
                    a(maxRow, k) = tmp
                Next
                det = -det
            End If

            ' 奇异矩阵
            If std.Abs(a(i, i)) < 1.0E-15 Then Return 0

            ' 消元
            For k = i + 1 To n - 1
                Dim factor = a(k, i) / a(i, i)
                For j = i To n - 1
                    a(k, j) -= factor * a(i, j)
                Next
            Next

            det *= a(i, i)
        Next

        Return det
    End Function

    ''' <summary>
    ''' 矩阵求逆 (使用 Gauss-Jordan 消元法)
    ''' </summary>
    Public Function MatrixInverse(mat As Tensor) As Tensor
        Dim n = mat.Shape(0)
        If mat.Shape(1) <> n Then
            Throw New ArgumentException("矩阵求逆需要方阵")
        End If

        ' 增广矩阵 [A | I]
        Dim aug = New Double(n - 1, 2 * n - 1) {}
        For i = 0 To n - 1
            For j = 0 To n - 1
                aug(i, j) = mat(i, j)
            Next
            aug(i, i + n) = 1
        Next

        ' Gauss-Jordan 消元
        For i = 0 To n - 1
            ' 找主元
            Dim maxRow = i
            For k = i + 1 To n - 1
                If std.Abs(aug(k, i)) > std.Abs(aug(maxRow, i)) Then maxRow = k
            Next

            ' 交换行
            If maxRow <> i Then
                For j = 0 To 2 * n - 1
                    Dim tmp = aug(i, j)
                    aug(i, j) = aug(maxRow, j)
                    aug(maxRow, j) = tmp
                Next
            End If

            ' 主元归一化
            Dim pivot = aug(i, i)
            If std.Abs(pivot) < 1.0E-15 Then
                Throw New InvalidOperationException("矩阵不可逆 (奇异矩阵)")
            End If
            For j = 0 To 2 * n - 1
                aug(i, j) /= pivot
            Next

            ' 消去其他行
            For k = 0 To n - 1
                If k <> i Then
                    Dim factor = aug(k, i)
                    For j = 0 To 2 * n - 1
                        aug(k, j) -= factor * aug(i, j)
                    Next
                End If
            Next
        Next

        ' 提取逆矩阵
        Dim result = New Tensor(n, n)
        For i = 0 To n - 1
            For j = 0 To n - 1
                result(i, j) = aug(i, j + n)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 生成 Box-Muller 正态分布随机数
    ''' </summary>
    Public Function Randn(rng As Random, Optional mean As Double = 0, Optional stdDev As Double = 1) As Double
        Dim u1 = 1.0 - rng.NextDouble()
        Dim u2 = 1.0 - rng.NextDouble()
        Dim z = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
        Return mean + stdDev * z
    End Function

    ''' <summary>
    ''' 计算两个向量的欧氏距离
    ''' </summary>
    Public Function EuclideanDistance(a As Tensor, b As Tensor) As Double
        If a.Length <> b.Length Then
            Throw New ArgumentException("向量长度不匹配")
        End If
        Dim sumSq As Double = 0
        For i = 0 To a.Length - 1
            Dim diff = a(i) - b(i)
            sumSq += diff * diff
        Next
        Return std.Sqrt(sumSq)
    End Function

End Module
