#Region "Microsoft.VisualBasic::3bdbd9b505ce5b02ce2bf962d791e9c8, sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\PCA\PCA.vb"

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

    '   Total Lines: 216
    '    Code Lines: 106
    ' Comment Lines: 84
    '   Blank Lines: 26
    '     File Size: 8.35 KB


    '     Class PCA
    ' 
    '         Properties: CumulativeVariance, Eigenvalues, ExplainedVariance, Loadings, StandardDeviations
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) adjust, Project
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace LinearAlgebra.Prcomp

    ''' <summary>
    ''' Performs principal component analysis on a set of data and returns the resulting data set. The
    ''' QR algorithm is used to find the eigenvalues and orthonormal eigenvectors of the covariance
    ''' matrix of the data set. The eigenvectors corresponding to the largest eigenvalues are the
    ''' principal components. The data file should be in the same directory as the PCA.class file.
    ''' All numbers should be tab-delimited. The first line of the data should be two numbers: the 
    ''' number of rows R followed by the number of columns C. After that, there should be R lines of 
    ''' C tab-delimited values. The columns would most likely represent the dimensions of measure; the
    ''' rows would each represent a single data point.
    ''' @author	Kushal Ranjan
    ''' @version	051513
    ''' </summary>
    ''' <remarks>
    ''' + https://github.com/kranjan94/Principal-Component-Analysis
    ''' + https://stats.stackexchange.com/questions/222/what-are-principal-component-scores
    ''' </remarks>
    Public Class PCA

        ''' <summary>
        ''' 矩阵之中的每一列的数据的平均值
        ''' </summary>
        Dim means As Vector
        ''' <summary>
        ''' 矩阵之中的每一列的数据的标准差
        ''' </summary>
        Dim stdevs As Vector

        ''' <summary>
        ''' Returns the Eigenvectors of the covariance matrix
        ''' </summary>
        Dim U As GeneralMatrix
        ''' <summary>
        ''' Returns the Eigenvalues (on the diagonal)
        ''' </summary>
        Dim S As Vector

        Dim center, scale As Boolean
        ''' <summary>
        ''' the raw input data
        ''' </summary>
        Dim matrix As Vector()

        Public ReadOnly Property Eigenvalues As Vector
            Get
                Return S.AsVector
            End Get
        End Property

        ''' <summary>
        ''' (Standard deviation) Returns the standard deviations of the principal components
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StandardDeviations As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Vector.Sqrt(S)
            End Get
        End Property

        ''' <summary>
        ''' Returns the loadings matrix
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Loadings As GeneralMatrix
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return U.Transpose
            End Get
        End Property

        ''' <summary>
        ''' (Proportion of Variance) Returns the proportion of variance for each component
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExplainedVariance As Vector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return S / S.Sum
            End Get
        End Property

        ''' <summary>
        ''' (Cumulative Proportion) Returns the cumulative proportion of variance
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CumulativeVariance As Vector
            Get
                Dim explained = ExplainedVariance

                For i As Integer = 1 To explained.Length - 1
                    explained(i) += explained(i - 1)
                Next

                Return explained
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dataset">
        ''' dataset is a two-dimensional array where rows represent the samples and columns the features
        ''' </param>
        ''' <param name="center"></param>
        ''' <param name="scale"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(dataset As IEnumerable(Of Double()), Optional center As Boolean = True, Optional scale As Boolean = False)
            Call Me.New(dataset.Select(Function(d) d.AsVector), center, scale)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(matrix As GeneralMatrix, Optional center As Boolean = True, Optional scale As Boolean = False)
            Call Me.New(vectors:=matrix.RowVectors, center:=center, scale:=scale)
        End Sub

        Sub New(vectors As IEnumerable(Of Vector), Optional center As Boolean = True, Optional scale As Boolean = False)
            Dim matrix = adjust(vectors.ToArray, center, scale, means, stdevs)
            Dim svd = New NumericMatrix(matrix).SVD()

            Me.center = center
            Me.scale = scale
            Me.matrix = matrix

            ' svd.rightSingularVectors;
            Me.U = svd.V
            ' svd.diagonal;
            Dim singularValues = svd.SingularValues
            Dim eigenvalues = (singularValues ^ 2) / (matrix.Length - 1)

            Me.S = eigenvalues
        End Sub

        ''' <summary>
        ''' Project the dataset into the PCA space.
        ''' (使用<see cref="Loadings"/>矩阵对所输入的原始数据进行线性变换，降低数据维度)
        ''' </summary>
        ''' <returns>
        ''' Each vector is a row in the input matrix
        ''' </returns>
        Public Function Project(nPC%) As Vector()
            'If center Then
            '    data = data _
            '        .Select(Function(r) r - means) _
            '        .ToArray

            '    If scale Then
            '        data = data _
            '            .Select(Function(r) r / stdevs) _
            '            .ToArray
            '    End If
            'End If
            Dim idx = nPC.Sequence.ToArray
            Dim U As GeneralMatrix = Me.Loadings(idx)
            Dim X As New NumericMatrix(matrix)
            Dim P As Vector() = (X * U) _
                .RowVectors _
                .ToArray

            Return P
        End Function

        ''' <summary>
        ''' 对数据集执行标准化
        ''' </summary>
        ''' <param name="dataset"></param>
        ''' <param name="center"></param>
        ''' <param name="scale"></param>
        ''' <returns></returns>
        Private Shared Function adjust(dataset As Vector(), center As Boolean, scale As Boolean, ByRef means As Vector, ByRef stdevs As Vector) As Vector()
            If center Then
                Dim columns = dataset(0) _
                    .Sequence _
                    .Select(Function(i)
                                Return dataset _
                                    .Select(Function(r) r(i)) _
                                    .AsVector
                            End Function) _
                    .ToArray
                Dim mu, std As Vector

                means = columns.Select(Function(c) c.Average).AsVector
                mu = means
                stdevs = columns.Select(Function(c) c.SD).AsVector
                std = stdevs
                dataset = dataset _
                    .Select(Function(r)
                                ' 在这里每一行数据减去每一列的平均值
                                ' 就是相当于这一行的每一个元素减去对应的列的平均值
                                Return r - mu
                            End Function) _
                    .ToArray

                If scale Then
                    dataset = dataset _
                        .Select(Function(r) r / std) _
                        .ToArray
                End If
            End If

            Return dataset
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function adjust(data As Double()(), center As Boolean, scale As Boolean) As Vector()
            Return adjust(data.Select(Function(r) r.AsVector).ToArray, center, scale, Nothing, Nothing)
        End Function
    End Class
End Namespace
