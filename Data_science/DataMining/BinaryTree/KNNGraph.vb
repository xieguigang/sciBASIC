#Region "Microsoft.VisualBasic::a0ba457c570feba96ca50ca6e0a1525c, Data_science\DataMining\BinaryTree\KNNGraph.vb"

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

    '   Total Lines: 217
    '    Code Lines: 151 (69.59%)
    ' Comment Lines: 25 (11.52%)
    '    - Xml Docs: 92.00%
    ' 
    '   Blank Lines: 41 (18.89%)
    '     File Size: 7.80 KB


    ' Class KNNGraph
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetGraph, GetGraphMatrix, KNN, sizeDims
    '     Class JaccardAlignment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetObject, GetSimilarity
    ' 
    '     Class NodeVisits
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '         Sub: setByDimension
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class KNNGraph

    ReadOnly tree As KdTree(Of ClusterEntity)
    ReadOnly raw As ClusterEntity()

    ''' <summary>
    ''' construct a kdtree for the given dataset points
    ''' </summary>
    ''' <param name="data">a collection of dataset points</param>
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

    Public Iterator Function GetGraphMatrix(k As Integer, Optional aggregate As Func(Of IEnumerable(Of Double), Double) = Nothing) As IEnumerable(Of ClusterEntity)
        Dim knn = Me.KNN(k).ToArray
        Dim cols As SeqValue(Of String)() = knn _
            .Select(Function(a) a.value) _
            .IteratesALL _
            .Distinct _
            .SeqIterator _
            .ToArray

        If aggregate Is Nothing Then
            aggregate = AddressOf System.Linq.Enumerable.Average
        End If

        Dim table = raw.AsParallel _
            .Select(Function(a) (a.uid, aggregate(a.entityVector))) _
            .ToDictionary(Function(a) a.uid,
                          Function(a)
                              Return a.Item2
                          End Function)

        For Each q As NamedCollection(Of String) In knn
            Dim v As Double() = New Double(cols.Length - 1) {}

            For Each adjcent As String In q
                v(cols(adjcent)) = table(adjcent)
            Next

            Yield New ClusterEntity With {
                .uid = q.name,
                .entityVector = v
            }
        Next
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

        Dim knn As NamedCollection(Of String)() = Me.KNN(k).ToArray
        Dim metric As New JaccardAlignment(
            knn, raw,
            equals:=jaccard_cutoff,
            gt:=jaccard_cutoff / div_factor
        )
        Dim btree As New AVLTree(Of String, String)(metric.GetComparer, Function(str) str)

        Call VBDebugger.EchoLine("create binary avl-tree based on the jaccard alignment of the knn set...")

        For Each id As String In list.Keys
            Call btree.Add(id, id, valueReplace:=False)
        Next

        Call VBDebugger.EchoLine("export cluster tree!")

        Return BTreeCluster.GetClusters(btree, metric)
    End Function

    ''' <summary>
    ''' search KNN via KDTree
    ''' </summary>
    ''' <returns></returns>
    Private Iterator Function KNN(k As Integer) As IEnumerable(Of NamedCollection(Of String))
        Dim n As Integer = raw.Length
        Dim d As Integer = n / 25
        Dim i As Integer = 0

        Call VBDebugger.EchoLine("do knn query...")

        For Each node As ClusterEntity In raw
            Dim q = tree.nearest(node, maxNodes:=k).ToArray
            Dim link As String() = q _
                .Select(Function(a) a.node.data.uid) _
                .ToArray

            Yield New NamedCollection(Of String) With {
                .name = node.uid,
                .value = link
            }

            If i Mod d = 0 Then
                Call VBDebugger.Echo(CInt(i / n * 100) & vbTab)
            End If

            i += 1
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
        Public Overrides Sub setByDimension(x As ClusterEntity, dimName As String, value As Double)
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
