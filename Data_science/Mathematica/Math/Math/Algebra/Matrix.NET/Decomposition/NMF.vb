Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' implementation of Non-negative Matrix Factorisation Algorithms
    ''' </summary>
    ''' <remarks>
    ''' Non-Negative Matrix Factorization
    ''' 
    ''' Non-Negative Matrix Factorization (NMF) is a recent technique for dimensionality 
    ''' reduction and data analysis that yields a parts based, sparse nonnegative 
    ''' representation for nonnegative input data. NMF has found a wide variety of applications,
    ''' including text analysis, document clustering, face/image recognition, language
    ''' modeling, speech processing and many others. Despite these numerous applications,
    ''' the algorithmic development for computing the NMF factors has been relatively
    ''' deficient.
    '''
    ''' NMF can be applied To the statistical analysis Of multivariate data In the following 
    ''' manner. Given a Set Of Of multivariate n-dimensional data vectors, the vectors are 
    ''' placed In the columns Of an n x m matrix V where m Is the number Of examples In the 
    ''' data Set. This matrix Is Then approximately factorized into an n x r matrix W (weights
    ''' matrix) And an r x m matrix H (features matrix), where r Is the number Of features
    ''' defined by the user. Usually r Is chosen To be smaller than n Or m, so that W And H 
    ''' are smaller than the original matrix V. This results In a compressed version Of the
    ''' original data matrix.
    ''' 
    ''' NMF 算法将矩阵 A 分解为两个矩阵：基矩阵 W 和系数矩阵 H。这两个矩阵的乘积将尽可能地接近原始矩阵 A。
    '''
    ''' + 基矩阵 W 表示的是 A 的潜在特征或主题，
    ''' + 而系数矩阵 H 表示的是每个样本（在矩阵 A 中是每一行）在这些特征或主题上的分布。
    ''' 
    ''' 在 NMF 中，基矩阵 W 和系数矩阵 H 的乘积近似等于原始矩阵 A。通过这种方式，NMF 能够揭示矩阵 A 中的
    ''' 潜在结构和特征。例如，在基矩阵 W 的每一列可能代表了矩阵 A 中的主要特征或主题，而系数矩阵 H 的
    ''' 每一行则表示原始矩阵 A 中对应样本在这些特征或主题上的分布情况。
    ''' </remarks>
    Public Class NMF

        Public Property W As NumericMatrix
        Public Property H As NumericMatrix
        Public Property cost As Double
        Public Property errors As Double()

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
            Dim W As NumericMatrix = NumericMatrix.random(rowDimension:=m, columnDimension:=k)
            Dim H As NumericMatrix = NumericMatrix.random(rowDimension:=k, columnDimension:=n)
            Dim V As NumericMatrix
            Dim cost As Double
            Dim errors As New List(Of Double)

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
                errors.Add(cost)

                If cost <= tolerance Then
                    Exit For
                End If
            Next

            Return New NMF With {
                .cost = cost,
                .H = H,
                .W = W,
                .errors = errors.ToArray
            }
        End Function

    End Class
End Namespace