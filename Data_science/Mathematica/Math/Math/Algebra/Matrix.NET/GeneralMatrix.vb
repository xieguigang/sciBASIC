#Region "Microsoft.VisualBasic::87a182cabce23a116da2ba1e2464a7db, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\GeneralMatrix.vb"

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

    '   Total Lines: 77
    '    Code Lines: 19 (24.68%)
    ' Comment Lines: 47 (61.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (14.29%)
    '     File Size: 2.68 KB


    '     Interface INumericMatrix
    ' 
    '         Function: ArrayPack
    ' 
    '     Interface GeneralMatrix
    ' 
    '         Properties: ColumnDimension, RowDimension, (+4 Overloads) X
    ' 
    '         Function: Dot, GetMatrix, Resize, RowVectors, Transpose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Vectorization

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' an abstract model of the numeric matrix object
    ''' </summary>
    Public Interface INumericMatrix

        ''' <summary>Copy the internal two-dimensional array.</summary>
        ''' <returns>Two-dimensional array copy of matrix elements.
        ''' </returns>
        Function ArrayPack(Optional deepcopy As Boolean = False) As Double()()

    End Interface

    ''' <summary>
    ''' [m,n]
    ''' </summary>
    Public Interface GeneralMatrix : Inherits INumericMatrix

        ''' <summary>
        ''' get/set cell element value
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <returns></returns>
        Default Property X(i As Integer, j As Integer) As Double
        Default Property X(i As Integer, Optional byRow As Boolean = True) As Vector

        ''' <summary>
        ''' column projection via column index
        ''' </summary>
        ''' <remarks>
        ''' select column values for each row for create a new matrix
        ''' </remarks>
        ''' <param name="indices"></param>
        ''' <returns></returns>
        Default ReadOnly Property X(indices As IEnumerable(Of Integer)) As GeneralMatrix
        Default ReadOnly Property X(rowIdx As BooleanVector) As GeneralMatrix

        ''' <summary>
        ''' m
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property RowDimension As Integer
        ''' <summary>
        ''' n
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ColumnDimension As Integer

        Function Transpose() As GeneralMatrix
        Function Resize(m As Integer, n As Integer) As GeneralMatrix
        Function RowVectors() As IEnumerable(Of Vector)

        ''' <summary>Get a submatrix.</summary>
        ''' <param name="r">   Array of row indices.
        ''' </param>
        ''' <param name="j0">  Initial column index
        ''' </param>
        ''' <param name="j1">  Final column index
        ''' </param>
        ''' <returns>     A(r(:),j0:j1)
        ''' </returns>
        ''' <exception cref="System.IndexOutOfRangeException">   Submatrix indices
        ''' </exception>
        Function GetMatrix(r As Integer(), j0 As Integer, j1 As Integer) As GeneralMatrix
        ''' <summary>
        ''' matrix dot product
        ''' </summary>
        ''' <param name="m2"></param>
        ''' <returns></returns>
        Function Dot(m2 As GeneralMatrix) As GeneralMatrix

    End Interface
End Namespace
