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