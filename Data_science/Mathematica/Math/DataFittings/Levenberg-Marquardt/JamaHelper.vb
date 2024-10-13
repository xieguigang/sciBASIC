#Region "Microsoft.VisualBasic::bb3ff75a1738ad932944571e14f8125e, Data_science\Mathematica\Math\DataFittings\Levenberg-Marquardt\JamaHelper.vb"

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

    '   Total Lines: 41
    '    Code Lines: 22 (53.66%)
    ' Comment Lines: 15 (36.59%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (9.76%)
    '     File Size: 1.81 KB


    '     Class JamaHelper
    ' 
    '         Function: dotProduct, solvePSDMatrixEq
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports CholeskyDecomposition = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.CholeskyDecomposition

Namespace LevenbergMarquardt

    ''' <summary>
    ''' Created by duy on 31/1/15.
    ''' </summary>
    Public Class JamaHelper
        ''' <summary>
        ''' Solves the matrix equation A * x = b where A is assumed to be positive
        ''' definite by using Cholesky decomposition </summary>
        ''' <param name="A"> Matrix on the left-hand side of the equation </param>
        ''' <param name="b"> Matrix on the right-hand side of the equation </param>
        ''' <returns> The solution of the equation A * x = b, OR null if A is not
        '''         positive definite </returns>
        Public Shared Function solvePSDMatrixEq(A As Matrix, b As Matrix) As Matrix
            Dim cholesky As CholeskyDecomposition = A.chol()
            If Not cholesky.SPD Then
                Return Nothing
            End If
            Return CType(cholesky.Solve(b), Matrix)
        End Function

        ''' <summary>
        ''' Computes the dot product between vectors u and v </summary>
        ''' <param name="u"> A row or column vector </param>
        ''' <param name="v"> A row or column vector </param>
        ''' <returns> Real number which is the dot product between u and v </returns>
        Public Shared Function dotProduct(u As Matrix, v As Matrix) As Double
            If u.RowDimension <> 1 Then
                u = CType(u.Transpose(), Matrix)
            End If
            If v.ColumnDimension <> 1 Then
                v = CType(v.Transpose(), Matrix)
            End If
            Return (u * v).Array(0)(0)
        End Function
    End Class

End Namespace

