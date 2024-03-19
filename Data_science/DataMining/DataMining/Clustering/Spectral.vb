Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Clustering

    ''' <summary>
    ''' Spectral Clustering
    ''' </summary>
    Public Class Spectral

        Public Const epsilon As Double = 0.00001

        Private X As NumericMatrix
        Private K As NumericMatrix
        Private eigenvectors As NumericMatrix
        Private eigenvalues As Vector
        Private cumulative As NumericMatrix
        Private centers As Integer
        Private kernel_type As Integer
        Private normalise As Integer
        Private max_iters As Integer
        Private gamma As Double
        Private constant As Double
        Private order As Double
        Private assignments As Integer()

        Public Sub New(d As NumericMatrix)
            centers = 2
            kernel_type = 1
            normalise = 1
            max_iters = 1000
            gamma = 0.001
            constant = 1.0
            order = 2.0

            X = d
        End Sub

        Public Sub set_centers(i As Integer)
            centers = i
        End Sub
        Public Sub set_kernel(i As Integer)
            kernel_type = i
        End Sub
        Public Sub set_normalise(i As Integer)
            normalise = i
        End Sub
        Public Sub set_gamma(i As Double)
            gamma = i
        End Sub
        Public Sub set_constant(i As Double)
            constant = i
        End Sub
        Public Sub set_order(i As Double)
            order = i
        End Sub
        Public Sub set_max_iters(i As Integer)
            max_iters = i
        End Sub
        Public Function cluster() As Integer()

            generate_kernel_matrix()
            eigendecomposition()
            kmeans()

            Return assignments.ToArray()
        End Function


        Public Function get_assignments() As Integer()
            Return assignments.ToArray()
        End Function

        ''' <summary>
        ''' Fill kernel matrix
        ''' </summary>
        Private Sub generate_kernel_matrix()
            Dim x_rows = X.RowDimension
            Dim vi As Double

            K = New NumericMatrix(x_rows, x_rows)
            For i = 0 To x_rows - 1
                For j = i To x_rows - 1
                    vi = kernel(X(i, byRow:=True), X(j, byRow:=True))
                    K(j, i) = vi
                    K(i, j) = vi
                    'if(i == 0) cout << K(i,j) << " ";
                Next
            Next

            ' Normalise kernel matrix
            Dim d As Vector = K.RowWise().Sum()

            d = 1.0 / Vector.Sqrt(d)


            Dim F = d.AsDiagonal()
            Dim l = CType(K.DotProduct(F), NumericMatrix)
            For i = 0 To l.RowDimension - 1
                For j = 0 To l.ColumnDimension - 1
                    l(i, j) = l(i, j) * d(i)
                Next
            Next

            K = l

        End Sub

        Private Function kernel(a As Vector, b As Vector) As Double
            Select Case kernel_type
                Case 2
                    Return (a.DotProduct(b) + constant) ^ order
                Case Else
                    Return std.Exp(-gamma * (a - b).Mod)
            End Select
        End Function

        Private Sub eigendecomposition()

            Dim edecomp = New EigenvalueDecomposition(K)
            eigenvalues = edecomp.RealEigenvalues.AsVector()
            eigenvectors = edecomp.V
            cumulative = New NumericMatrix(eigenvalues.Length, eigenvalues.Length)
            Dim eigen_pairs As List(Of (e As Double, eigen As Vector)) = New List(Of (Double, Vector))()
            Dim c = 0.0
            Dim norm As Double

            For i = 0 To eigenvectors.ColumnDimension - 1
                If normalise <> 0 Then
                    norm = eigenvectors(i, byRow:=False).SumMagnitude
                    eigenvectors(i, byRow:=False) /= norm
                End If

                Call eigen_pairs.Add((eigenvalues(i), eigenvectors(i, byRow:=False)))
            Next

            Call eigen_pairs.Sort(Function(a, b) If(a.e = b.e, 0, If(a.e > b.e, 1, -1)))

            If centers > eigen_pairs.Count Then
                centers = eigen_pairs.Count
            End If

            For i = 0 To eigen_pairs.Count - 1
                eigenvalues(i) = eigen_pairs(i).e
                c += eigenvalues(i)
                cumulative(i) = c
                eigenvectors(i, byRow:=False) = eigen_pairs(i).eigen
            Next

            ' 
            ' 		cout << "Sorted eigenvalues:" << endl;
            ' 		for(unsigned int i = 0; i < eigenvalues.RowDimension; i++){
            ' 			if(eigenvalues(i) > 0){
            ' 				cout << "PC " << i+1 << ": Eigenvalue: " << eigenvalues(i);
            ' 				printf("\t(%3.3f of variance, cumulative =  %3.3f)\n",eigenvalues(i)/eigenvalues.sum(),cumulative(i)/eigenvalues.sum());
            ' 				//cout << eigenvectors.col(i) << endl;
            ' 			}
            ' 		}
            ' 		cout << endl;
            ' 		

            ' Select top K eigenvectors where K = centers
            eigenvectors = DirectCast(eigenvectors.Transpose, NumericMatrix).Block(0, 0, eigenvectors.RowDimension, centers)
        End Sub


        ''' <summary>
        ''' kmeans on <seecref="eigenvectors"/> with required n <seecref="centers"/>.
        ''' </summary>
        Private Sub kmeans()
            Dim m As ClusterEntity() = eigenvectors.RowVectors().[Select](Function(r, i) New ClusterEntity((i + 1).ToString(), r)).ToArray()
            Dim alg As KMeansAlgorithm(Of ClusterEntity) = New KMeansAlgorithm(Of ClusterEntity)(max_iters:=max_iters)
            Dim clusters = alg.ClusterDataSet(m, k:=centers)

            assignments = New Integer(m.Length - 1) {}

            Dim cluster_id = 1

            For Each c In clusters
                For Each x As ClusterEntity In c
                    assignments(Integer.Parse(x.uid) - 1) = cluster_id
                Next

                cluster_id += 1
            Next
        End Sub

    End Class
End Namespace