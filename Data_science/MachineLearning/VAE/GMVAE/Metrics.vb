#Region "Microsoft.VisualBasic::f371d457385bf34682b9a8b06b7ac96d, Data_science\MachineLearning\VAE\GMVAE\Metrics.vb"

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

    '   Total Lines: 37
    '    Code Lines: 21 (56.76%)
    ' Comment Lines: 10 (27.03%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.29 KB


    ' Class Metrics
    ' 
    '     Function: cluster_acc, nmi
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' Metrics used to evaluate our model
''' </summary>
Public Class Metrics

    ''' <summary>
    ''' Code taken from the work 
    ''' VaDE (Variational Deep Embedding:A Generative Approach to Clustering)
    ''' </summary>
    ''' <param name="Y_pred"></param>
    ''' <param name="Y"></param>
    ''' <returns></returns>
    Public Function cluster_acc(Y_pred As Vector, Y As Vector) As Double
        Dim D = std.Max(Y_pred.Max(), Y.Max()) + 1
        Dim w = NumericMatrix.Zero(D, D)

        For i As Integer = 0 To Y_pred.Dim - 1
            w(CInt(Y_pred(i)), CInt(Y(i))) += 1.0
        Next

        Dim max = linear.linear_sum_assignment(w.max - w)
        Dim row = max.row, col = max.col
        Dim sum = Enumerable.Range(0, row.Dim).Select(Function(i) w(CInt(row(i)), CInt(col(i)))).Sum

        Return sum * 1.0 / Y_pred.Dim
    End Function

    Public Function nmi(Y_pred As Vector, Y As Vector)
        Return linear.normalized_mutual_info_score(Y_pred, Y, average_method:="arithmetic")
    End Function
End Class
