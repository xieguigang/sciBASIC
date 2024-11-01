#Region "Microsoft.VisualBasic::955f9ab6e90ad7de08334758cec3effb, Data_science\Mathematica\Math\Math\Algebra\Matrix.NET\Decomposition\NMF.vb"

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

    '   Total Lines: 128
    '    Code Lines: 68 (53.12%)
    ' Comment Lines: 41 (32.03%)
    '    - Xml Docs: 85.37%
    ' 
    '   Blank Lines: 19 (14.84%)
    '     File Size: 5.90 KB


    '     Class NMF
    ' 
    '         Properties: cost, errors, H, W
    ' 
    '         Function: Factorisation
    ' 
    '         Sub: Factorisation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm

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
                                             Optional epsilon As Double = 0.0001,
                                             Optional tqdm As Boolean = True) As NMF

            Dim m As Integer = A.RowDimension
            Dim n As Integer = A.ColumnDimension
            ' initialize W,H as random matrix
            Dim W As NumericMatrix = NumericMatrix.random(rowDimension:=m, columnDimension:=k)
            Dim H As NumericMatrix = NumericMatrix.random(rowDimension:=k, columnDimension:=n)
            Dim V As NumericMatrix = Nothing
            Dim cost As Double
            Dim errors As New List(Of Double)
            Dim bar As Tqdm.ProgressBar = Nothing

            If tqdm Then
                For Each i As Integer In TqdmWrapper.Range(0, max_iterations, bar:=bar)
                    Call Factorisation(W, H, A, V, cost, errors)

                    If cost <= tolerance Then
                        Exit For
                    Else
                        Call bar.SetLabel(cost)
                    End If
                Next
            Else
                For i As Integer = 0 To max_iterations
                    Call Factorisation(W, H, A, V, cost, errors)

                    If cost <= tolerance Then
                        Exit For
                    End If
                Next
            End If

            Return New NMF With {
                .cost = cost,
                .H = H,
                .W = W,
                .errors = errors.ToArray
            }
        End Function

        Private Shared Sub Factorisation(<Out> ByRef W As NumericMatrix,
                                         <Out> ByRef H As NumericMatrix,
                                         <Out> ByRef A As NumericMatrix,
                                         <Out> ByRef V As NumericMatrix,
                                         <Out> ByRef cost As Double,
                                         <Out> ByRef errors As List(Of Double))

            Dim Wt As NumericMatrix = W.Transpose

            Dim HN = Wt.DotProduct(A)
            Dim HD = Wt.DotProduct(W)

            HD = HD.Dot(H)

            H = DirectCast(H * HN, NumericMatrix) / HD

            Dim Ht As NumericMatrix = H.Transpose

            Dim WN = A.DotProduct(Ht)
            Dim WD = W.DotProduct(H.DotProduct(Ht))

            W = DirectCast(W * WN, NumericMatrix) / WD

            V = W.DotProduct(H)
            cost = ((A - V) ^ 2).sum(axis:=-1).Sum
            errors.Add(cost)
        End Sub

    End Class
End Namespace
