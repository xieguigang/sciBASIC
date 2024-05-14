#Region "Microsoft.VisualBasic::242222728f495aec92ab9d1c7c9c9d1c, Data_science\Graph\KNN\KNN.vb"

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

    '   Total Lines: 69
    '    Code Lines: 43
    ' Comment Lines: 14
    '   Blank Lines: 12
    '     File Size: 2.71 KB


    '     Class KNN
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) FindNeighbors, KNeighbors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree.ApproximateNearNeighbor
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace KNearNeighbors

    ''' <summary>
    ''' KNN search handler for phenograph
    ''' </summary>
    Public Class KNN

        ReadOnly score As ScoreMetric

        ''' <summary>
        ''' knn score cutoff
        ''' </summary>
        ReadOnly cutoff As Double

        Sub New(metric As ScoreMetric, knn_cutoff As Double)
            Me.score = metric
            Me.cutoff = knn_cutoff

            score.cutoff = knn_cutoff
        End Sub

        ''' <summary>
        ''' the output keeps the same order as the given input <paramref name="data"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="k"></param>
        ''' <returns>
        ''' the generates index value keeps the same order with input original <paramref name="data"/> matrix rows.
        ''' </returns>
        Public Function FindNeighbors(data As GeneralMatrix, Optional k As Integer = 30) As IEnumerable(Of KNeighbors)
            Dim matrix As TagVector() = data.PopulateVectors.ToArray
            Dim knnQuery = matrix _
                .AsParallel _
                .Select(Function(v)
                            Return (v.index, FindNeighbors(v, matrix, k, score))
                        End Function) _
                .OrderBy(Function(r) r.index) _
                .ToArray

            Return KNeighbors(knnQuery.Select(Function(r) r.Item2))
        End Function

        Public Shared Iterator Function KNeighbors(knn As IEnumerable(Of (TagVector, w As Double)())) As IEnumerable(Of KNeighbors)
            For Each nn2 As (TagVector, w As Double)() In knn
                Dim index As Integer() = nn2.Select(Function(xi) xi.Item1.index).ToArray
                Dim weights As Double() = nn2.Select(Function(xi) xi.w).ToArray

                Yield New KNeighbors(index.Length, index, weights)
            Next
        End Function

        Public Shared Function FindNeighbors(v As TagVector, matrix As TagVector(), k As Integer, score As ScoreMetric) As (TagVector, w As Double)()
            Dim vec As Double() = v.vector.ToArray

            Return matrix _
                .Select(Function(i)
                            Dim w As Double = score.eval(vec, i.vector)
                            Return (i, w)
                        End Function) _
                .Where(Function(a) a.w > score.cutoff) _
                .OrderByDescending(Function(a) a.w) _
                .Take(k) _
                .ToArray
        End Function
    End Class
End Namespace
