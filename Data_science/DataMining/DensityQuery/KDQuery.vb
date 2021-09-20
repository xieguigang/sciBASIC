Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

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

Public Class Metric : Inherits KdNodeAccessor(Of ClusterEntity)

    ReadOnly dims As Dictionary(Of String, Integer)
    ReadOnly dimNames As String()

    Sub New(dims As Integer)
        Me.dims = New Dictionary(Of String, Integer)

        For i As Integer = 0 To dims - 1
            Me.dims(i.ToString) = i
        Next

        Me.dimNames = Me.dims.Keys.ToArray
    End Sub

    Public Overrides Sub setByDimensin(x As ClusterEntity, dimName As String, value As Double)
        x(dims(dimName)) = value
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return dimNames
    End Function

    Public Overrides Function metric(a As ClusterEntity, b As ClusterEntity) As Double
        Return DistanceMethods.EuclideanDistance(a.entityVector, b.entityVector)
    End Function

    Public Overrides Function getByDimension(x As ClusterEntity, dimName As String) As Double
        Return x(dims(dimName))
    End Function

    Public Overrides Function nodeIs(a As ClusterEntity, b As ClusterEntity) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As ClusterEntity
        Return New ClusterEntity With {
            .cluster = -1,
            .entityVector = New Double(dims.Count - 1) {},
            .uid = "n/a"
        }
    End Function
End Class