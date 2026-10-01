#Region "Microsoft.VisualBasic::f598384011bdf34132cbe0c37b401799, Data_science\Mathematica\Math\Math\Algebra\SVD.vb"

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

    '   Total Lines: 360
    '    Code Lines: 275 (76.39%)
    ' Comment Lines: 28 (7.78%)
    '    - Xml Docs: 21.43%
    ' 
    '   Blank Lines: 57 (15.83%)
    '     File Size: 12.38 KB


    '     Module SVD
    ' 
    '         Sub: SVDecomposition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 历史说明：本文件原本是 Numerical Recipes svdcmp 算法（AForge 移植）的直译。
' 该旧实现与 Matrix.NET\Decomposition\SingularValueDecomposition（JAMA/LINPACK 系）
' 存在多处行为不一致：
'   1. 奇异值不排序（JAMA 版输出恒为降序）；
'   2. 收敛判据依赖浮点精确等式，全零/极小输入矩阵会退化；
'   3. 就地破坏输入矩阵 a（Householder 变换直接写入 a）。
' 本次重构将其统一为 JAMA 版 <see cref="SingularValueDecomposition"/> 的薄封装，
' 消除同一库内两套 SVD 的行为分歧。

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace LinearAlgebra

    ''' <summary>
    ''' Singular Value Decomposition
    ''' </summary>
    ''' <remarks>
    ''' 本模块现在是 <see cref="SingularValueDecomposition"/>（JAMA/LINPACK 系实现）
    ''' 的薄封装。与旧版 Numerical Recipes 直译实现相比，输出约定为：
    ''' 奇异值恒为非负数且按降序排列，输入矩阵 <paramref name="a"/> 不会被修改。
    ''' </remarks>
    Public Module SVD

        ''' <summary>
        ''' Singular Value Decomposition
        ''' </summary>
        ''' <param name="a">Number of rows in A must be greater or equal to number of columns</param>
        ''' <param name="w">
        ''' 输出：奇异值向量（非负、降序排列），满足 a = U * diag(w) * V^T
        ''' </param>
        ''' <param name="v">
        ''' 输出：右奇异向量矩阵（n x n），第 i 列为奇异值 w(i) 对应的右奇异向量
        ''' </param>
        Public Sub SVDecomposition(a As Double(,), ByRef w As Double(), ByRef v As Double(,))
            ' number of rows in A
            Dim m As Integer = a.GetLength(0)
            ' number of columns in A
            Dim n As Integer = a.GetLength(1)

            If m < n Then
                Throw New ArgumentException("Number of rows in A must be greater or equal to number of columns")
            End If

            ' JAMA 版构造函数内部会对输入做深拷贝（ArrayPack(deepcopy:=True)），
            ' 不会修改调用方传入的矩阵
            Dim svd As New SingularValueDecomposition(a)
            Dim singularValues As Double() = svd.SingularValues.Array

            w = New Double(n - 1) {}
            v = New Double(n - 1, n - 1) {}

            ' JAMA 内部存储长度为 min(m+1, n)，当 m >= n 时恰为 n
            For i As Integer = 0 To n - 1
                w(i) = singularValues(i)
            Next

            Dim V As GeneralMatrix = svd.V

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    v(i, j) = V(i, j)
                Next
            Next
        End Sub
    End Module
End Namespace