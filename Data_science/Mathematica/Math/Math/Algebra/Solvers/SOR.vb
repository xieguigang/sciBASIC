#Region "Microsoft.VisualBasic::11c96f53df5909efd24deeab80001ce5, Data_science\Mathematica\Math\Math\Algebra\Solvers\SOR.vb"

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

    '   Total Lines: 53
    '    Code Lines: 33 (62.26%)
    ' Comment Lines: 9 (16.98%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 11 (20.75%)
    '     File Size: 1.73 KB


    '     Module SOR
    ' 
    '         Function: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace LinearAlgebra.Solvers

    Public Module SOR

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="b"></param>
        ''' <param name="Omiga">松弛因子</param>
        ''' <param name="e">误差容限</param>
        ''' <param name="Iteration">最大允许迭代次数</param>
        ''' <returns></returns>
        Public Function Solve(A As GeneralMatrix, b As Vector,
                              Optional Omiga As Double = 1.2,
                              Optional e As Double = 0.00000001,
                              Optional Iteration As Integer = 50) As Vector

            Dim N As Integer = A.ColumnDimension
            Dim x1 As Vector = New Vector(N), x As Vector = New Vector(N)
            Dim sum As Double
            Dim converged As Boolean = False

            For k As Integer = 0 To Iteration
                For i As Integer = 0 To N - 1
                    ' 准确度修正：对角零元素守卫 —— A(i,i) 为除数，
                    ' 零元素会产生除零/NaN 并污染整个迭代序列
                    If A(i, i) = 0 Then
                        Throw New InvalidOperationException(
                            $"SOR solver requires non-zero diagonal elements: A({i},{i}) = 0.")
                    End If

                    sum = 0

                    For j As Integer = 0 To N - 1
                        If j < i Then
                            sum += A(i, j) * x(j)
                        ElseIf j > i Then
                            sum += A(i, j) * x1(j)
                        End If
                    Next

                    x(i) = (b(i) - sum) * Omiga / A(i, i) + (1.0 - Omiga) * x1(i)
                Next

                Dim dx As Vector = x - x1, err As Double = std.Sqrt(dx.Mod)

                If err < e Then
                    converged = True
                    Exit For
                End If

                ' 准确度修正：Vector 是引用类型，直接赋值会让 x1 与 x 共享
                ' 同一缓冲，下一轮迭代中 x 的就地更新会同步改写 x1，
                ' 导致误差恒为 0 而提前退出（静默返回未收敛的解）。
                ' 必须深拷贝。
                x1 = x.Clone()
            Next

            If Not converged Then
                Call Console.WriteLine(
                    $"WARNING: SOR iteration does not converge after {Iteration + 1} iterations (final error > {e}).")
            End If

            Return x
        End Function
    End Module
End Namespace
