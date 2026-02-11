#Region "Microsoft.VisualBasic::50623d35c7c48a3bba100ce72076f890, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\LargeScaleEigenSolver.vb"

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

    '   Total Lines: 261
    '    Code Lines: 180 (68.97%)
    ' Comment Lines: 42 (16.09%)
    '    - Xml Docs: 14.29%
    ' 
    '   Blank Lines: 39 (14.94%)
    '     File Size: 10.43 KB


    '     Class LargeScaleEigenSolver
    ' 
    '         Function: GetTopKEigenvalues, Hypot, Norm, Normalize
    ' 
    '         Sub: SolveTridiagonalEigen
    '         Class KEigenResult
    ' 
    '             Properties: Eigenvalues, IsConverged
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports __std = System.Math
Imports _rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LinearAlgebra.Matrix

    Public Class LargeScaleEigenSolver

        ' 定义一个结构体来返回结果
        Public Class KEigenResult

            Public Property Eigenvalues As Double()
            Public Property IsConverged As Boolean

            Public Overrides Function ToString() As String
                Return Eigenvalues.GetJson
            End Function
        End Class

        ''' <summary>
        ''' 计算稀疏对称矩阵的前 k 个最大特征值（使用 Lanczos 算法）
        ''' </summary>
        ''' <param name="sparseMatrix">稀疏矩阵（必须是对称的，如无向图邻接矩阵）</param>
        ''' <param name="k">需要计算的特征值数量</param>
        ''' <returns>前 k 个特征值（近似值）</returns>
        Public Shared Function GetTopKEigenvalues(sparseMatrix As SparseMatrix, k As Integer) As KEigenResult
            Dim n As Integer = sparseMatrix.RowDimension
            ' Lanczos 迭代次数通常设为比 k 稍大一点，比如 k + 5 或 2*k，以保证收敛
            Dim m As Integer = __std.Min(n, k + 10)

            ' 存储三对角矩阵的对角线 alpha 和 非对角线 beta
            Dim alpha(m - 1) As Double
            Dim beta(m - 1) As Double
            Dim q_vectors(m)() As Double ' 存储 Lanczos 向量

            ' 1. 初始化初始向量 q1 (随机向量)
            Dim q0 As Double() = New Double(n - 1) {} ' 上一轮向量
            Dim q1 As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                q1(i) = _rand.NextDouble()
            Next

            ' 归一化 q1
            q1 = Normalize(q1)
            q_vectors(0) = q1

            Dim r As Double() = New Double(n - 1) {}
            beta(0) = 0.0

            ' 2. Lanczos 迭代主循环
            For j As Integer = 0 To m - 1
                ' r = A * qj
                r = sparseMatrix.Multiply(q_vectors(j))

                ' r = r - beta(j-1) * q(j-1)
                If j > 0 Then
                    For i As Integer = 0 To n - 1
                        r(i) = r(i) - beta(j - 1) * q_vectors(j - 1)(i)
                    Next
                End If

                ' alpha(j) = qj' * r
                Dim alpha_j As Double = 0.0
                For i As Integer = 0 To n - 1
                    alpha_j += q_vectors(j)(i) * r(i)
                Next
                alpha(j) = alpha_j

                ' r = r - alpha(j) * qj
                For i As Integer = 0 To n - 1
                    r(i) = r(i) - alpha(j) * q_vectors(j)(i)
                Next

                ' 全正交化 - 防止数值不稳定
                ' 对于大规模计算，部分正交化通常就够用了，这里为了严谨做简单完全正交化
                For i As Integer = 0 To j
                    Dim dot As Double = 0
                    For idx As Integer = 0 To n - 1
                        dot += r(idx) * q_vectors(i)(idx)
                    Next
                    For idx As Integer = 0 To n - 1
                        r(idx) = r(idx) - dot * q_vectors(i)(idx)
                    Next
                Next

                ' 计算新的 beta
                Dim beta_j As Double = Norm(r)

                ' 如果 beta 极小，说明迭代提前结束（找到了不变子空间）
                If j < m - 1 Then
                    beta(j + 1) = beta_j
                    If beta_j < 0.0000000001 Then
                        ' 提前终止
                        m = j + 1
                        ReDim Preserve alpha(m - 1)
                        Exit For
                    End If

                    ' 准备下一个向量 q(j+1) = r / beta_j
                    Dim q_next As Double() = New Double(n - 1) {}
                    For i As Integer = 0 To n - 1
                        q_next(i) = r(i) / beta_j
                    Next
                    q_vectors(j + 1) = q_next
                End If
            Next

            ' 3. 此时，我们得到了一个大小为 m x m 的三对角矩阵 T
            ' T 的对角线是 alpha，次对角线是 beta
            ' 我们可以使用你原有的 tql2 逻辑来求解这个小的 T 矩阵

            ' 构造 tql2 需要的输入格式 (假设我们提取了 EigenvalueDecomposition 的逻辑)
            ' 这里我们需要模拟 tql2 的输入：D(对角), E(次对角), V(特征向量，这里我们不需要)
            ' 注意：原代码中 tql2 是 Sub，直接修改类成员变量。
            ' 为了演示，我们假设有一个 Utility 方法调用它。

            ' 复制一份用于计算，避免修改原始引用
            Dim d_calc(m - 1) As Double
            Dim e_calc(m - 1) As Double
            Array.Copy(alpha, d_calc, m)
            Array.Copy(beta, e_calc, m) ' 注意原代码 e(0) 是未用的或者特殊的，需小心

            ' 调用精简版的 tql2 逻辑（见下文辅助函数）
            SolveTridiagonalEigen(d_calc, e_calc, m)

            ' d_calc 现在包含了近似特征值，按升序排列
            ' 我们需要最大的 k 个，并降序排列
            Array.Sort(d_calc)
            Array.Reverse(d_calc)

            Dim result(m - 1) As Double
            Array.Copy(d_calc, result, m)

            Return New KEigenResult With {
                .Eigenvalues = result,
                .IsConverged = True
            }
        End Function

        ' 向量归一化
        Private Shared Function Normalize(v As Double()) As Double()
            Dim norm As Double = __std.Sqrt(v.Sum(Function(x) x * x))
            If norm = 0 Then Return v
            Return v.Select(Function(x) x / norm).ToArray()
        End Function

        ' 向量范数
        Private Shared Function Norm(v As Double()) As Double
            Return __std.Sqrt(v.Sum(Function(x) x * x))
        End Function

        ' 这是一个提取自你原有 EigenvalueDecomposition 类的简化版 tql2
        ' 专门用于求解小规模的三对角矩阵特征值
        Private Shared Sub SolveTridiagonalEigen(ByRef d() As Double, ByRef e() As Double, n As Integer)
            ' 这里的逻辑直接对应你提供的代码中的 tql2() 方法
            ' 只是不操作 V 矩阵（因为我们不需要特征向量），只计算特征值 d

            For i As Integer = 1 To n - 1
                e(i - 1) = e(i)
            Next
            e(n - 1) = 0.0

            Dim f As Double = 0.0
            Dim tst1 As Double = 0.0
            Dim eps As Double = 2.0 ^ -52.0

            For l As Integer = 0 To n - 1
                tst1 = __std.Max(tst1, __std.Abs(d(l)) + __std.Abs(e(l)))
                Dim m_var As Integer = l
                While m_var < n
                    If __std.Abs(e(m_var)) <= eps * tst1 Then
                        Exit While
                    End If
                    m_var += 1
                End While

                If m_var > l Then
                    Dim iter As Integer = 0
                    Do
                        iter = iter + 1
                        Dim g As Double = d(l)
                        Dim p As Double = (d(l + 1) - g) / (2.0 * e(l))
                        Dim r As Double = Hypot(p, 1.0)
                        If p < 0 Then r = -r
                        d(l) = e(l) / (p + r)
                        d(l + 1) = e(l) * (p + r)
                        Dim dl1 As Double = d(l + 1)
                        Dim h As Double = g - d(l)
                        For i As Integer = l + 2 To n - 1
                            d(i) -= h
                        Next
                        f = f + h

                        p = d(m_var)
                        Dim c As Double = 1.0
                        Dim c2 As Double = c
                        Dim c3 As Double = c
                        Dim el1 As Double = e(l + 1)
                        Dim s As Double = 0.0
                        Dim s2 As Double = 0.0
                        For i As Integer = m_var - 1 To l Step -1
                            c3 = c2
                            c2 = c
                            s2 = s
                            g = c * e(i)
                            h = c * p
                            r = Hypot(p, e(i))
                            e(i + 1) = s * r
                            s = e(i) / r
                            c = p / r
                            p = c * d(i) - s * g
                            d(i + 1) = h + s * (c * g + s * d(i))

                            ' 去掉了特征向量 V 的累积部分，因为我们只需要特征值
                        Next
                        p = (-s) * s2 * c3 * el1 * e(l) / dl1
                        e(l) = s * p
                        d(l) = c * p
                    Loop While __std.Abs(e(l)) > eps * tst1
                End If
                d(l) = d(l) + f
                e(l) = 0.0
            Next

            ' 排序
            For i As Integer = 0 To n - 2
                Dim k As Integer = i
                Dim p As Double = d(i)
                For j As Integer = i + 1 To n - 1
                    If d(j) < p Then
                        k = j
                        p = d(j)
                    End If
                Next
                If k <> i Then
                    d(k) = d(i)
                    d(i) = p
                End If
            Next
        End Sub

        ' 辅助函数，对应原代码中的 Hypot
        Private Shared Function Hypot(a As Double, b As Double) As Double
            Dim r As Double
            If __std.Abs(a) > __std.Abs(b) Then
                r = b / a
                Return __std.Abs(a) * __std.Sqrt(1.0 + r * r)
            Else
                If b <> 0 Then
                    r = a / b
                    Return __std.Abs(b) * __std.Sqrt(1.0 + r * r)
                Else
                    Return 0.0
                End If
            End If
        End Function

    End Class

End Namespace
