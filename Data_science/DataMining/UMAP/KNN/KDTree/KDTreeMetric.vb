#Region "Microsoft.VisualBasic::da4ecd00c3bf69d186e2ef3c2e16eeba, Data_science\DataMining\UMAP\KNN\KDTree\KDTreeMetric.vb"

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

    '   Total Lines: 45
    '    Code Lines: 40 (88.89%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (11.11%)
    '     File Size: 1.69 KB


    '     Module KDTreeMetric
    ' 
    '         Function: GetKNN
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Linq

Namespace KNN.KDTreeMethod

    Module KDTreeMetric

        Public Function GetKNN(data As Double()(), k As Integer) As KNNState
            Dim vectors As KDPoint() = data _
                .Select(Function(v, i)
                            Return New KDPoint With {
                                .vector = v,
                                .id = i
                            }
                        End Function) _
                .ToArray
            Dim tree As New KdTree(Of KDPoint)(vectors, New KDAccessor(dims:=vectors(Scan0).size))
            Dim knnSearch = vectors _
                .SeqIterator _
                .AsParallel _
                .Select(Function(p)
                            Return (p.i, tree.nearest(p.value, k).ToArray)
                        End Function) _
                .OrderBy(Function(p) p.i) _
                .Select(Function(i) i.ToArray) _
                .ToArray
            Dim index As Integer()() = knnSearch _
                .Select(Function(r)
                            Return r.Select(Function(d) d.node.data.id).ToArray
                        End Function) _
                .ToArray
            Dim weights As Double()() = knnSearch _
                .Select(Function(r)
                            Return r.Select(Function(d) d.distance).ToArray
                        End Function) _
                .ToArray

            Return New KNNState With {
                .knnDistances = weights,
                .knnIndices = index
            }
        End Function

    End Module
End Namespace
