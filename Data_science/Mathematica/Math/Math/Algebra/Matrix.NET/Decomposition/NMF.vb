Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' implementation of Non-negative Matrix Factorisation Algorithms
    ''' </summary>
    ''' <remarks>
    ''' Implements Lee and Seungs Multiplicative Update Algorithm
    ''' </remarks>
    Public Class NMF

        Public Property W As NumericMatrix
        Public Property H As NumericMatrix
        Public Property cost As Double

        ''' <summary>
        ''' Implements Lee and Seungs Multiplicative Update Algorithm
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="k">number of features</param>
        ''' <param name="max_iterations"></param>
        ''' <param name="tolerance"></param>
        ''' <returns></returns>
        Public Shared Function Factorisation(A As NumericMatrix,
                                             Optional k As Integer = 2,
                                             Optional max_iterations As Integer = 1000,
                                             Optional tolerance As Double = 0.001,
                                             Optional epsilon As Double = 0.0001) As NMF

            Dim m As Integer = A.RowDimension
            Dim n As Integer = A.ColumnDimension
            ' initialize W,H as random matrix
            Dim W As NumericMatrix = NumericMatrix.Gauss(m, k)
            Dim H As NumericMatrix = NumericMatrix.Gauss(k, n)
            Dim V As NumericMatrix
            Dim cost As Double
            Dim x1, x2 As NumericMatrix

            For i As Integer = 0 To max_iterations
                ' H = H .* (W'A) ./ (W'WH + epsilon);
                x1 = W.Transpose.Dot(W).Dot(H)
                H = (H * (W.Transpose.Dot(A))) / (x1 + epsilon)
                ' W = W .* (AH' ) ./ (WHH' + epsilon);
                x2 = W.DotProduct(H).Dot(H.Transpose)
                W = (W * A.DotProduct(H.Transpose)) / (x2 + epsilon)
                V = W.DotProduct(H)
                cost = ((A - V) ^ 2).sum(axis:=-1).Sum

                If cost <= tolerance Then
                    Exit For
                End If
            Next

            Return New NMF With {
                .cost = cost,
                .H = H,
                .W = W
            }
        End Function

    End Class
End Namespace