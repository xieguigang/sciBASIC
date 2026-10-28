#Region "Microsoft.VisualBasic::524c551356a51eca8aeda84cce2ed6cf, Data_science\DataMining\DataMining\Clustering\Spectral.vb"

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

    '   Total Lines: 192
    '    Code Lines: 133 (69.27%)
    ' Comment Lines: 23 (11.98%)
    '    - Xml Docs: 39.13%
    ' 
    '   Blank Lines: 36 (18.75%)
    '     File Size: 6.41 KB


    '     Class Spectral
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: cluster, get_assignments, kernel
    ' 
    '         Sub: eigendecomposition, generate_kernel_matrix, kmeans, set_centers, set_constant
    '              set_gamma, set_kernel, set_max_iters, set_normalise, set_order
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.SIMD
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

        ''' <summary>
        ''' 允许的最大样本数（<c>0</c> 表示不限制，默认 20000）。
        ''' </summary>
        ''' <remarks>
        ''' 谱聚类需要 <c>n×n</c> 核矩阵（8·n² 字节）与 <c>O(n³)</c> 特征分解；
        ''' 超过该上限时 <see cref="cluster"/> 会抛出带说明的异常，而不是静默耗尽内存。
        ''' </remarks>
        Public Property MaxPoints As Integer = 20000

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
            Dim x_rows As Integer = X.RowDimension

            If MaxPoints > 0 AndAlso x_rows > MaxPoints Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(X),
                    $"spectral clustering builds an n×n kernel matrix and an O(n^3) eigendecomposition; " &
                    $"{x_rows} samples exceed MaxPoints({MaxPoints}). Please subsample or increase MaxPoints explicitly.")
            End If

            ' 先把行向量抽取到 Double()()，避免每一对 (i,j) 都重新构造一个 Vector 对象
            Dim rows As Double()() = New Double(x_rows - 1)() {}

            For i As Integer = 0 To x_rows - 1
                rows(i) = X(i, byRow:=True).ToArray
            Next

            K = New NumericMatrix(x_rows, x_rows)

            Dim gammaValue As Double = gamma
            Dim constantValue As Double = constant
            Dim orderValue As Double = order
            Dim polynomial As Boolean = kernel_type = 2

            ' 核矩阵只算上三角并对称写回：外循环按行块并行，行内用 SIMD 点积/平方距离，
            ' 不再经过 Vector.operator- 与 .Mod（每对都要分配多个临时数组）。
            Call SimdParallel.ForRange(x_rows,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        Dim a As Double() = rows(i)

                        For j As Integer = i To x_rows - 1
                            Dim vi As Double

                            If polynomial Then
                                vi = (SimdReduce.Dot(a, rows(j)) + constantValue) ^ orderValue
                            Else
                                vi = std.Exp(-gammaValue * SimdReduce.DistanceSquared(a, rows(j)))
                            End If

                            K(j, i) = vi
                            K(i, j) = vi
                        Next
                    Next
                End Sub, 32)

            ' Normalise kernel matrix: F·K·F，其中 F = diag(1/sqrt(rowsum))
            '
            ' 旧实现走 K.DotProduct(F)（n×n × n×n 稠密矩阵乘，O(n^3)），
            ' 但 F 是对角阵，等价于逐元素缩放 K(i,j) * d(i) * d(j)，这里直接这么做。
            Dim d As Vector = K.RowWise().Sum()

            d = 1.0 / Vector.Sqrt(d)

            For i As Integer = 0 To x_rows - 1
                Dim di As Double = d(i)

                For j As Integer = 0 To x_rows - 1
                    K(i, j) = K(i, j) * di * d(j)
                Next
            Next
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
        ''' kmeans on <see cref="eigenvectors"/> with required n <see cref="centers"/>.
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
