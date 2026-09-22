#Region "Microsoft.VisualBasic::140383d8bb8fc80010911ab59187cbcc, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Math\Multiply.vb"

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

    '   Total Lines: 48
    '    Code Lines: 23 (47.92%)
    ' Comment Lines: 18 (37.50%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 7 (14.58%)
    '     File Size: 1.89 KB


    '     Module Multiply
    ' 
    '         Function: ColumnMultiply, RowMultiply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SimdEngine = Microsoft.VisualBasic.Math.SIMD.SimdEngine
Imports SimdMatrix = Microsoft.VisualBasic.Math.SIMD.SimdMatrix

Namespace LinearAlgebra.Matrix

    Module Multiply

        ''' <summary>
        ''' the vector size is equals to the matrix rows,
        ''' each element in target vector is multiply to
        ''' each row in matrix
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RowMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            ' 第 i 行整体乘上标量 v(i)：逐行走向量化标量乘法
            Dim data As Double()() = SimdMatrix.MultiplyRows(m.ArrayPack(deepcopy:=False), v.Array)

            Return New NumericMatrix(data, m.RowDimension, m.ColumnDimension)
        End Function

        ''' <summary>
        ''' the vector size is equals to the matrix columns,
        ''' each element in target vector is multiply to
        ''' each column in matrix
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColumnMultiply(m As GeneralMatrix, v As Vector) As GeneralMatrix
            Dim X As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = X.Array
            Dim rows As Double()() = m.ArrayPack(deepcopy:=False)
            Dim values As Double() = v.Array

            ' 每一行与向量 v 做逐元素相乘（行内走 SIMD 乘法）
            For i As Integer = 0 To m.RowDimension - 1
                C(i) = SimdEngine.Multiply(Of Double)(rows(i), values)
            Next

            Return X
        End Function
    End Module
End Namespace
