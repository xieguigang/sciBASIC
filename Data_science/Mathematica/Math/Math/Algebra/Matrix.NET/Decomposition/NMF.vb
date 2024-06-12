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

            For i As Integer = 0 To max_iterations
                Dim HN = W.Transpose.Dot(A)
                Dim HD = W.Transpose.Dot(W)

                HD = HD.Dot(H)

                H = DirectCast(H * HN, NumericMatrix) / HD

                Dim WN = A.DotProduct(H.Transpose)
                Dim WD = W.DotProduct(H.DotProduct(H.Transpose))

                W = DirectCast(W * WN, NumericMatrix) / WD

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