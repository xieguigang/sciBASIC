#Region "Microsoft.VisualBasic::bea6adda393bd339c30ba62c6f78eacd, Data_science\Graph\Model\Tree\KdTree\ApproximateNearNeighbor\ApproximateNearNeighbor.vb"

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

    '   Total Lines: 98
    '    Code Lines: 57
    ' Comment Lines: 25
    '   Blank Lines: 16
    '     File Size: 4.13 KB


    '     Module ApproximateNearNeighbor
    ' 
    '         Function: (+2 Overloads) FindNeighbors, PopulateVectors, RowMetric
    '         Class ParallelSearch
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Parallel

Namespace KdTree.ApproximateNearNeighbor

    ''' <summary>
    ''' K Nearest Neighbour Search
    ''' 
    ''' Uses a kd-tree to find the p number of near neighbours for each point in an input/output dataset.
    ''' </summary>
    Public Module ApproximateNearNeighbor

        Public Function FindNeighbors(data As GeneralMatrix, Optional k As Integer = 30) As IEnumerable(Of KNeighbors)
            Return data _
                .PopulateVectors _
                .FindNeighbors(k)
        End Function

        ''' <summary>
        ''' Convert the matrix rows as the indexed vector
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the vector is indexed via the input matrix row index
        ''' </remarks>
        <Extension>
        Public Iterator Function PopulateVectors(data As GeneralMatrix) As IEnumerable(Of TagVector)
            For Each d In data.RowVectors.SeqIterator
                Yield New TagVector With {
                    .index = d.i,
                    .vector = CType(d, Vector).ToArray
                }
            Next
        End Function

        ''' <summary>
        ''' the output keeps the same order as the given input <paramref name="data"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="k"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function FindNeighbors(data As IEnumerable(Of TagVector), Optional k As Integer = 30) As IEnumerable(Of KNeighbors)
            Dim allData As TagVector() = data.ToArray
            'Dim tree As New KdTree(Of TagVector)(allData, RowMetric(ncols:=allData(Scan0).size))
            'Dim knnQuery = From row As TagVector
            '               In allData.AsParallel
            '               Let nn2 = tree.nearest(row, maxNodes:=k).OrderBy(Function(i) i.distance).ToArray
            '               Select row.index, nn2
            '               Order By index
            Dim knnQuery = New ParallelSearch(allData, knn:=k).Run().DoCall(Function(t) DirectCast(t, ParallelSearch).result)

            For Each row In knnQuery
                Dim nn2 As KdNodeHeapItem(Of TagVector)() = row.nn2
                Dim index As Integer() = nn2.Select(Function(xi) xi.node.data.index).ToArray
                Dim weights As Double() = nn2.Select(Function(xi) xi.distance).ToArray

                Yield New KNeighbors(index.Length, index, weights)
            Next
        End Function

        Private Class ParallelSearch : Inherits VectorTask

            Friend ReadOnly allData As TagVector()
            Friend ReadOnly result As (index As Integer, nn2 As KdNodeHeapItem(Of TagVector)())()
            Friend ReadOnly tree As KdTree(Of TagVector)
            Friend ReadOnly k As Integer

            Sub New(allData As TagVector(), knn As Integer)
                Call MyBase.New(allData.Length)

                Me.allData = allData

                k = knn
                result = New(index As Integer, nn2 As KdNodeHeapItem(Of TagVector)())(allData.Length - 1) {}
                tree = New KdTree(Of TagVector)(allData, RowMetric(ncols:=allData(Scan0).size))
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For i As Integer = start To ends
                    Dim row = allData(i)
                    Dim nn2 = tree.nearest(row, maxNodes:=k).OrderBy(Function(ki) ki.distance).ToArray

                    result(i) = (row.index, nn2)
                Next
            End Sub
        End Class

        Private Function RowMetric(ncols As Integer) As KdNodeAccessor(Of TagVector)
            Return New VectorAccessor(ncols)
        End Function
    End Module

End Namespace
