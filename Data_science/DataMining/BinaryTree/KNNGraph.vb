Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class KNNGraph

    ReadOnly tree As KdTree(Of ClusterEntity)
    ReadOnly raw As ClusterEntity()

    Sub New(data As IEnumerable(Of ClusterEntity))
        raw = data.ToArray
        tree = New KdTree(Of ClusterEntity)(raw, metric:=New NodeVisits(sizeDims(raw(0))))
    End Sub

    Private Shared Function sizeDims(e As ClusterEntity) As Dictionary(Of String, Integer)
        Dim dims As New Dictionary(Of String, Integer)

        For i As Integer = 0 To e.entityVector.Length - 1
            Call dims.Add(i.ToString, i)
        Next

        Return dims
    End Function

    ''' <summary>
    ''' Build a graph cluster result via KNN method
    ''' </summary>
    ''' <param name="k"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' the algorithm implements:
    ''' 
    ''' 1. search KNN via KDtree, get connection id of each node
    ''' 2. build cluster tree
    ''' 3. metric via the jaccard index between the node via in tree construction.
    ''' </remarks>
    Public Function GetGraph(k As Integer,
                             Optional jaccard_cutoff As Double = 0.85,
                             Optional div_factor As Double = 2) As BTreeCluster

        Dim knn = Me.KNN(k).ToArray
        Dim metric As New JaccardAlignment(
            knn, raw,
            equals:=jaccard_cutoff,
            gt:=jaccard_cutoff / div_factor
        )
        Dim btree As New AVLTree(Of String, String)(metric.GetComparer, Function(str) str)

        For Each id As String In list.Keys
            Call btree.Add(id, id, valueReplace:=False)
        Next

        Return BTreeCluster.GetClusters(btree, metric)
    End Function

    ''' <summary>
    ''' search KNN via KDTree
    ''' </summary>
    ''' <returns></returns>
    Private Iterator Function KNN(k As Integer) As IEnumerable(Of NamedCollection(Of String))
        For Each node As ClusterEntity In raw
            Dim q = tree.nearest(node, maxNodes:=k).ToArray
            Dim link As String() = q _
                .Select(Function(a) a.node.data.uid) _
                .ToArray

            Yield New NamedCollection(Of String) With {
                .name = node.uid,
                .value = link
            }
        Next
    End Function

    Private Class JaccardAlignment : Inherits ComparisonProvider

        ReadOnly linkSet As Dictionary(Of String, String())
        ReadOnly rawdata As Dictionary(Of String, ClusterEntity)

        Sub New(knn As IEnumerable(Of NamedCollection(Of String)),
                raw As IEnumerable(Of ClusterEntity),
                equals As Double,
                gt As Double)

            Call MyBase.New(equals, gt)

            rawdata = raw.ToDictionary(Function(a) a.uid)
            linkSet = knn _
                .ToDictionary(Function(a) a.name,
                              Function(a)
                                  Return a.value
                              End Function)
        End Sub

        Public Overrides Function GetSimilarity(x As String, y As String) As Double
            Dim vx As String() = linkSet(x)
            Dim vy As String() = linkSet(y)
            Dim j As Double = vx.jaccard_coeff(vy)
            Return j
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetObject(id As String) As Object
            Return rawdata(id)
        End Function
    End Class

    Private Class NodeVisits : Inherits KdNodeAccessor(Of ClusterEntity)

        ReadOnly dims As Dictionary(Of String, Integer)

        ''' <summary>
        ''' construct a data accessor with the dimension mapping
        ''' </summary>
        ''' <param name="dims"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(dims As Dictionary(Of String, Integer))
            Me.dims = dims
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As ClusterEntity, dimName As String, value As Double)
            x.entityVector(dims(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dims.Keys.ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As ClusterEntity, b As ClusterEntity) As Double
            Return a.DistanceTo(b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As ClusterEntity, dimName As String) As Double
            Return x.entityVector(dims(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As ClusterEntity, b As ClusterEntity) As Boolean
            Return a.uid = b.uid
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function activate() As ClusterEntity
            Return New ClusterEntity With {
                .uid = App.NanoTime.ToString,
                .entityVector = New Double(dims.Count - 1) {}
            }
        End Function
    End Class

End Class
