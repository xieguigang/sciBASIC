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
