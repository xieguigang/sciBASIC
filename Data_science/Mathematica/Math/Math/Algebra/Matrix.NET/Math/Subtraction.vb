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
            ' 注意：这里保留了历史可观察行为 —— 原实现从未读取 m 的元素值，
            ' 结果矩阵的每一行 j 都被整体填充为 v(j)（疑似历史缺陷）。
            ' 本次只做向量化/块写入改造，不改变可观察行为。
            Dim m2 As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = m2.Array
            Dim values As Double() = v.Array

            For j As Integer = 0 To m.RowDimension - 1
                ' Array.Fill 走 Span.Fill 的向量化填充，取代逐元素赋值
                Call System.Array.Fill(C(j), values(j))
            Next

            Return m2
        End Function
    End Module
End Namespace
