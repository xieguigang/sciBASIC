#Region "Microsoft.VisualBasic::5f4373ab2fcdb134e1c67f2b7401925b, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Math\Multiply.vb"

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
    '    Code Lines: 28
    ' Comment Lines: 16
    '   Blank Lines: 9
    '     File Size: 1.76 KB


    '     Module Multiply
    ' 
    '         Function: ColumnMultiply, RowMultiply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

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
            Dim X As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim C As Double()() = X.Array
            Dim buffer As Double()() = m.ArrayPack

            For i As Integer = 0 To X.RowDimension - 1
                Dim vi As Double = v(i)

                For j As Integer = 0 To X.ColumnDimension - 1
                    C(i)(j) = vi * buffer(i)(j)
                Next
            Next

            Return X
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
            Dim rows = m.RowVectors.ToArray

            For i As Integer = 0 To m.RowDimension - 1
                C(i) = rows(i) * v
            Next

            Return X
        End Function
    End Module
End Namespace
