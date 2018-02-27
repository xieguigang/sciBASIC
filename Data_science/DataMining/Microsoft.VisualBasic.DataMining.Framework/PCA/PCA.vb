#Region "Microsoft.VisualBasic::98b7ab5bef228d13108bacac023a6a74, Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\PCA\PCA.vb"

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

'     Class PCA
' 
'         Properties: B, C, S, SVD, V
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace PCA

    Public Class PCA

        Dim means As Vector
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
        ''' 
        ''' </summary>
        ''' <param name="dataset">
        ''' dataset is a two-dimensional array where rows represent the samples and columns the features
        ''' </param>
        ''' <param name="center"></param>
        ''' <param name="scale"></param>
        Sub New(dataset As IEnumerable(Of DataSet), Optional center As Boolean = True, Optional scale As Boolean = False)
            Dim matrix = adjust(dataset.Matrix.ToArray, center, scale)
            Dim svd = New GeneralMatrix(matrix).SVD()

            Me.center = center
            Me.scale = scale

            ' svd.rightSingularVectors;
            Me.U = svd.V
            ' svd.diagonal;
            Dim singularValues = svd.SingularValues
            Dim eigenvalues = (singularValues ^ 2) / (matrix.Length - 1)

            Me.S = eigenvalues
        End Sub

        ''' <summary>
        ''' Project the dataset into the PCA space
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Function Project(data As Vector()) As Vector()
            If center Then
                data = data.Select(Function(r) r - means).ToArray

                If scale Then
                    data = data.Select(Function(r) r / stdevs).ToArray
                End If
            End If

            Dim predictions = New GeneralMatrix(data).Multiply(U)
            Return predictions.ToArray.Select(Function(r) r.AsVector).ToArray
        End Function

        Private Function adjust(data As Double()(), center As Boolean, scale As Boolean) As Vector()
            Dim dataset = data.Select(Function(r) r.AsVector).ToArray

            If center Then
                Dim columns = data(0).Sequence.Select(Function(i) dataset.Select(Function(r) r(i)).AsVector).ToArray

                means = columns.Select(Function(c) c.Average).AsVector
                dataset = dataset.Select(Function(r) r - means).ToArray
                stdevs = columns.Select(Function(c) c.StdError).AsVector

                If scale Then
                    dataset = dataset.Select(Function(r) r / stdevs).ToArray
                End If
            End If

            Return dataset
        End Function

        ''' <summary>
        ''' Returns the standard deviations of the principal components
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StandardDeviations As Vector
            Get
                Return Vector.Sqrt(S)
            End Get
        End Property

        ''' <summary>
        ''' Returns the loadings matrix
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Loadings As GeneralMatrix
            Get
                Return U.Transpose
            End Get
        End Property

        ''' <summary>
        ''' Returns the proportion of variance for each component
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ExplainedVariance As Vector
            Get
                Return S / S.Sum
            End Get
        End Property

        ''' <summary>
        ''' Returns the cumulative proportion of variance
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
    End Class
End Namespace
