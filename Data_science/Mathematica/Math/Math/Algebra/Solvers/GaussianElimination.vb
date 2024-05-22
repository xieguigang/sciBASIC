#Region "Microsoft.VisualBasic::2a702a7310410631d9a9b52410e029b1, Data_science\Mathematica\Math\Math\Algebra\Solvers\GaussianElimination.vb"

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

    '   Total Lines: 117
    '    Code Lines: 44 (37.61%)
    ' Comment Lines: 62 (52.99%)
    '    - Xml Docs: 88.71%
    ' 
    '   Blank Lines: 11 (9.40%)
    '     File Size: 4.54 KB


    '     Module GaussianElimination
    ' 
    '         Function: Solve, UpTri
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace LinearAlgebra.Solvers

    ''' <summary>
    ''' ### Gaussian elimination
    ''' 
    ''' In mathematics, Gaussian elimination, also known as row reduction, is an algorithm 
    ''' for solving systems of linear equations. It consists of a sequence of row-wise 
    ''' operations performed on the corresponding matrix of coefficients. This method can 
    ''' also be used to compute the rank of a matrix, the determinant of a square matrix, 
    ''' and the inverse of an invertible matrix. The method is named after Carl Friedrich 
    ''' Gauss (1777–1855). To perform row reduction on a matrix, one uses a sequence of
    ''' elementary row operations to modify the matrix until the lower left-hand corner of 
    ''' the matrix is filled with zeros, as much as possible. There are three types of 
    ''' elementary row operations:
    ''' 
    ''' + Swapping two rows,
    ''' + Multiplying a row by a nonzero number,
    ''' + Adding a multiple Of one row To another row.
    ''' 
    ''' Using these operations, a matrix can always be transformed into an upper triangular
    ''' matrix, And In fact one that Is In row echelon form. Once all Of the leading coefficients 
    ''' (the leftmost nonzero entry In Each row) are 1, And every column containing a leading
    ''' coefficient has zeros elsewhere, the matrix Is said To be In reduced row echelon form.
    ''' This final form Is unique; In other words, it Is independent Of the sequence Of row
    ''' operations used. For example, In the following sequence Of row operations (where two 
    ''' elementary operations On different rows are done at the first And third steps), the 
    ''' third And fourth matrices are the ones In row echelon form, And the final matrix Is 
    ''' the unique reduced row echelon form.
    ''' </summary>
    Public Module GaussianElimination

        ''' <summary>
        ''' solve a system of equation
        ''' 
        ''' ```
        ''' x * A = b
        ''' ```
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns>x</returns>
        ''' <remarks></remarks>
        ''' <example>
        ''' Dim a As New NumericMatrix({
        '''     { 2,  1, -1},
        '''     {-3, -1,  2},
        '''     {-2,  1,  2}
        ''' })
        ''' Dim b As Vector = {8, -11, -3}
        ''' Dim x As Vector = GaussianElimination.Solve(a, b)
        ''' 
        '''  2x + y -  z =  8
        ''' -3x - y + 2z = -11
        ''' -2x + y + 2z = -3
        ''' 
        ''' ' &lt;dims: 3> [2, 3, -1...]
        ''' Console.WriteLine(x)
        ''' </example>
        Public Function Solve(A As GeneralMatrix, b As Vector) As Vector
            Dim n As Integer = b.Dim
            Dim TMP As Double
            Dim Ab As New NumericMatrix(n, n + 1)

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Ab(i, j) = A(i, j)
                Next
                Ab(i, n) = b(i)
            Next

            ' Gaussian Elimination Core
            For k As Integer = 0 To n - 2
                For i As Integer = k + 1 To n - 1
                    TMP = Ab(i, k) / Ab(k, k)
                    For j As Integer = 0 To n
                        Ab(i, j) = Ab(i, j) - TMP * Ab(k, j)
                    Next
                Next
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    A(i, j) = Ab(i, j)
                Next
                b(i) = Ab(i, n)
            Next

            Return UpTri(A, b)
        End Function

        ''' <summary>
        ''' 上三角矩阵方程解法
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function UpTri(A As GeneralMatrix, b As Vector) As Vector
            Dim N As Integer = A.ColumnDimension
            Dim x As New Vector(N)

            x(N - 1) = b(N - 1) / A(N - 1, N - 1)

            For i As Integer = N - 2 To 0 Step -1
                x(i) = b(i)
                For j As Integer = i + 1 To N - 1
                    x(i) -= A(i, j) * x(j)
                Next
                x(i) /= A(i, i)
            Next

            Return x
        End Function
    End Module
End Namespace
