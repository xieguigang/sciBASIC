#Region "Microsoft.VisualBasic::40c740ce307b247baf9dfc001e135437, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Math\Subtraction.vb"

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

    '   Total Lines: 35
    '    Code Lines: 20 (57.14%)
    ' Comment Lines: 9 (25.71%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 6 (17.14%)
    '     File Size: 1.07 KB


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
            Dim m2 As New NumericMatrix(m.RowDimension, m.ColumnDimension)
            Dim buffer = m2.Array
            Dim v2 As Vector

            For i As Integer = 0 To m2.ColumnDimension - 1
                v2 = m2.ColumnVector(i)
                v2 = v - v2

                For j As Integer = 0 To buffer.Length - 1
                    Dim x = buffer(j)
                    x(i) = v2(j)
                Next
            Next

            Return m2
        End Function
    End Module
End Namespace
