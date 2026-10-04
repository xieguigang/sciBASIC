#Region "Microsoft.VisualBasic::1fddd69a719582d81cb29e1e589f77be, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Math\Subtraction.vb"

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

    '   Total Lines: 33
    '    Code Lines: 15 (45.45%)
    ' Comment Lines: 13 (39.39%)
    '    - Xml Docs: 61.54%
    ' 
    '   Blank Lines: 5 (15.15%)
    '     File Size: 1.30 KB


    '     Module Subtraction
    ' 
    '         Function: RowSubtraction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine

Namespace LinearAlgebra.Matrix

    Module Subtraction

        ''' <summary>
        ''' the dimension of the vector should be equals
        ''' to the row dimension of the input matrix.
        ''' 
        ''' <paramref name="v"/> - each column in m
        ''' </summary>
        ''' <param name="v"></param>
        ''' <param name="m"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RowSubtraction(v As Vector, m As GeneralMatrix) As GeneralMatrix
            If v.Dim <> m.RowDimension Then
                Throw New InvalidDataContractException(
                    $"the dimension of the vector(dim={v.Dim}) should be equals to the row dimension({m.RowDimension}) of the input matrix!")
            End If

            ' 历史缺陷修正：旧实现从未读取 m 的元素值，结果矩阵的每一行 j 
            ' 都被整体填充为 v(j)（等于假设 m 全零）。现修正为文档声明的语义：
            ' result(i, j) = v(i) - m(i, j)，即把列向量 v 按行广播到矩阵上做减法。
            Dim m2 As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = m2.Array
            Dim values As Double() = v.Array

            ' 每行两步 SIMD 内核：先整行取负，再加标量 v(i)，无逐元素标量循环
            For i As Integer = 0 To m.RowDimension - 1
                Dim row As Double() = SimdEngine.MultiplyScalar(-1.0, m.X(i).Array)
                C(i) = SimdEngine.AddScalar(row, values(i))
            Next

            Return m2
        End Function
    End Module
End Namespace
