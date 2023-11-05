

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations

Public Class KNNGraph

    ReadOnly tree As KdTree(Of EntityClusterModel)
    ReadOnly all As Dictionary(Of String, EntityClusterModel)

    Sub New(rawdata As IEnumerable(Of EntityClusterModel))
        all = rawdata.ToDictionary(Function(a) a.ID)
        tree = New KdTree(Of EntityClusterModel)(all.Values, metric:=New ObjAccess(all.First.Value.EnumerateKeys))
    End Sub

    Public Function BuildClusterGraph(k As Integer) As Dictionary(Of String, String())
        Dim assigned As New Index(Of String)
        Dim clusters As New Dictionary(Of String, String())
        Dim temp As New List(Of KdNodeHeapItem(Of EntityClusterModel))
        Dim all As New Dictionary(Of String, EntityClusterModel)(Me.all)
        Dim seed As EntityClusterModel
        Dim near As KdNodeHeapItem(Of EntityClusterModel)()
        Dim meanDist As Double

        Do While all.Count > 0
            seed = all.First.Value
            near = tree.nearest(seed, maxNodes:=k).ToArray
            temp.AddRange(near)
            meanDist = Aggregate xi As KdNodeHeapItem(Of EntityClusterModel)
                       In temp
                       Into Average(xi.distance)

            Do While True

            Loop
        Loop

        Return clusters
    End Function

    Private Class ObjAccess : Inherits KdNodeAccessor(Of EntityClusterModel)

        ReadOnly dims As String()
        ReadOnly cache As New Dictionary(Of String, Double())

        Sub New(dims As String())
            Me.dims = dims
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As EntityClusterModel, dimName As String, value As Double)
            x(dimName) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dims
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As EntityClusterModel, b As EntityClusterModel) As Double
            Dim x, y As Double()

            If Not cache.ContainsKey(a.ID) Then
                cache.Add(a.ID, a(dims))
            End If
            If Not cache.ContainsKey(b.ID) Then
                cache.Add(b.ID, b(dims))
            End If

            x = cache(a.ID)
            y = cache(b.ID)

            Return x.EuclideanDistance(y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As EntityClusterModel, dimName As String) As Double
            Return x(dimName)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As EntityClusterModel, b As EntityClusterModel) As Boolean
            Return a.ID = b.ID
        End Function

        Public Overrides Function activate() As EntityClusterModel
            Return New EntityClusterModel
        End Function
    End Class

End Class