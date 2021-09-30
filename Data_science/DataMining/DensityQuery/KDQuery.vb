Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.KMeans

Public Class KDQuery : Implements IQueryDensity(Of ClusterEntity)

    ReadOnly tree As KdTree(Of ClusterEntity)
    ReadOnly raws As ClusterEntity()

    <DebuggerStepThrough>
    Sub New(raw As IEnumerable(Of ClusterEntity))
        Me.raws = raw.ToArray
        Me.tree = New KdTree(Of ClusterEntity)(Me.raws, New Metric(Me.raws.First.Length))
    End Sub

    Public Function QueryDensity(row As ClusterEntity, k As Integer) As NamedValue(Of Double) Implements IQueryDensity(Of ClusterEntity).QueryDensity
        Dim nearest As Double() = tree.nearest(row, maxNodes:=k + 1) _
            .Where(Function(p) Not p.node.data Is row) _
            .Select(Function(p) p.distance) _
            .Take(k) _
            .ToArray
        Dim mean As Double

        If nearest.Length = 0 Then
            mean = 10000
        Else
            mean = nearest.Average
        End If

        Return New NamedValue(Of Double) With {
            .Name = row.uid,
            .Value = 1 / mean,
            .Description = nearest _
                .Select(Function(di) di.ToString("F2")) _
                .JoinBy("; ")
        }
    End Function

    Public Function Raw() As IEnumerable(Of ClusterEntity) Implements IQueryDensity(Of ClusterEntity).Raw
        Return raws
    End Function
End Class