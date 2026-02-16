' <copyright file="SmallWorld.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics.CodeAnalysis
Imports System.IO
Imports System.Linq
Imports System.Runtime.Serialization.Formatters.Binary

Namespace HNSW.Net

    ''' <summary>
    ''' <see href="https://arxiv.org/abs/1603.09320">Hierarchical Navigable Small World Graphs</see>.
    ''' </summary>
    ''' <typeparam name="TItem">The type of items to connect into small world.</typeparam>
    ''' <typeparam name="TDistance">The type of distance between items (expect any numeric type: float, double, decimal, int, ...).</typeparam>
    Public Partial Class SmallWorld(Of TItem, TDistance As IComparable(Of TDistance))
        ''' <summary>
        ''' The distance function in the items space.
        ''' </summary>
        Private ReadOnly distance As Func(Of TItem, TItem, TDistance)

        ''' <summary>
        ''' The hierarchical small world graph instance.
        ''' </summary>
        Private graphField As Graph

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SmallWorld(Of TItem,TDistance)"/> class.
        ''' </summary>
        ''' <param name="distance">The distance funtion to use in the small world.</param>
        Public Sub New(distance As Func(Of TItem, TItem, TDistance))
            Me.distance = distance
        End Sub

        ''' <summary>
        ''' Type of heuristic to select best neighbours for a node.
        ''' </summary>
        Public Enum NeighbourSelectionHeuristic
            ''' <summary>
            ''' Marker for the Algorithm 3 (SELECT-NEIGHBORS-SIMPLE) from the article.
            ''' Implemented in <see cref="SmallWorld(Of TItem,TDistance).NodeAlg3"/>
            ''' </summary>
            SelectSimple

            ''' <summary>
            ''' Marker for the Algorithm 4 (SELECT-NEIGHBORS-HEURISTIC) from the article.
            ''' Implemented in <see cref="SmallWorld(Of TItem,TDistance).NodeAlg4"/>
            ''' </summary>
            SelectHeuristic
        End Enum

        ''' <summary>
        ''' Builds hnsw graph from the items.
        ''' </summary>
        ''' <param name="items">The items to connect into the graph.</param>
        ''' <param name="generator">The random number generator for building graph.</param>
        ''' <param name="parameters">Parameters of the algorithm.</param>
        Public Sub BuildGraph(items As IList(Of TItem), generator As Random, parameters As Parameters)
            Dim graph = New Graph(distance, parameters)
            graph.Create(items, generator)
            graphField = graph
        End Sub

        ''' <summary>
        ''' Run knn search for a given item.
        ''' </summary>
        ''' <param name="item">The item to search nearest neighbours.</param>
        ''' <param name="k">The number of nearest neighbours.</param>
        ''' <returns>The list of found nearest neighbours.</returns>
        Public Function KNNSearch(item As TItem, k As Integer) As IList(Of KNNSearchResult)
            Dim destination = graphField.NewNode(-1, item, 0)
            Dim neighbourhood = graphField.KNearest(destination, k)
            Return neighbourhood.[Select](Function(n) New KNNSearchResult With {
.Id = n.Id,
.Item = n.Item,
.Distance = destination.TravelingCosts.From(n)
}).ToList()
        End Function

        ''' <summary>
        ''' Serializes the graph WITHOUT linked items.
        ''' </summary>
        ''' <returns>Bytes representing the graph.</returns>
        Public Function SerializeGraph() As Byte()
            If graphField Is Nothing Then
                Throw New InvalidOperationException("The graph does not exist")
            End If

            'Dim formatter = New BinaryFormatter()
            'Using stream = New MemoryStream()
            '    formatter.Serialize(stream, graphField.Parameters)

            '    Dim edgeBytes = graphField.Serialize()
            '    stream.Write(edgeBytes, 0, edgeBytes.Length)

            '    Return stream.ToArray()
            'End Using
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' Deserializes the graph from byte array.
        ''' </summary>
        ''' <param name="items">The items to assign to the graph's verticies.</param>
        ''' <param name="bytes">The serialized parameters and edges.</param>
        Public Sub DeserializeGraph(items As IList(Of TItem), bytes As Byte())
            'Dim formatter = New BinaryFormatter()
            'Using stream = New MemoryStream(bytes)
            '    Dim parameters = CType(formatter.Deserialize(stream), Parameters)

            '    Dim graph = New Graph(distance, parameters)
            '    graph.Deserialize(items, bytes.Skip(stream.Position).ToArray())

            '    graphField = graph
            'End Using
            Throw New NotImplementedException
        End Sub

        ''' <summary>
        ''' Prints edges of the graph.
        ''' Mostly for debug and test purposes.
        ''' </summary>
        ''' <returns>String representation of the graph's edges.</returns>
        Friend Function Print() As String
            Return graphField.Print()
        End Function

        ''' <summary>
        ''' Parameters of the algorithm.
        ''' </summary>
        <SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification:="By Design")>
        <Serializable>
        Public Class Parameters
            ''' <summary>
            ''' Initializes a new instance of the <see cref="Parameters"/> class.
            ''' </summary>
            Public Sub New()
                M = 10
                LevelLambda = 1 / Math.Log(M)
                NeighbourHeuristic = NeighbourSelectionHeuristic.SelectSimple
                ConstructionPruning = 200
                ExpandBestSelection = False
                KeepPrunedConnections = True
            End Sub

            ''' <summary>
            ''' Gets or sets the parameter which defines the maximum number of neighbors in the zero and above-zero layers.
            ''' The maximum number of neighbors for the zero layer is 2 * M.
            ''' The maximum number of neighbors for higher layers is M.
            ''' </summary>
            Public Property M As Integer

            ''' <summary>
            ''' Gets or sets the max level decay parameter.
            ''' https://en.wikipedia.org/wiki/Exponential_distribution
            ''' See 'mL' parameter in the HNSW article.
            ''' </summary>
            Public Property LevelLambda As Double

            ''' <summary>
            ''' Gets or sets parameter which specifies the type of heuristic to use for best neighbours selection.
            ''' </summary>
            Public Property NeighbourHeuristic As NeighbourSelectionHeuristic

            ''' <summary>
            ''' Gets or sets the number of candidates to consider as neighbousr for a given node at the graph construction phase.
            ''' See 'efConstruction' parameter in the article.
            ''' </summary>
            Public Property ConstructionPruning As Integer

            ''' <summary>
            ''' Gets or sets a value indicating whether to expand candidates if <see cref="NeighbourSelectionHeuristic.SelectHeuristic"/> is used.
            ''' See 'extendCandidates' parameter in the article.
            ''' </summary>
            Public Property ExpandBestSelection As Boolean

            ''' <summary>
            ''' Gets or sets a value indicating whether to keep pruned candidates if <see cref="NeighbourSelectionHeuristic.SelectHeuristic"/> is used.
            ''' See 'keepPrunedConnections' parameter in the article.
            ''' </summary>
            Public Property KeepPrunedConnections As Boolean
        End Class

        ''' <summary>
        ''' Representation of knn search result.
        ''' </summary>
        <SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification:="By Design")>
        Public Class KNNSearchResult
            ''' <summary>
            ''' Gets or sets the id of the item = rank of the item in source collection.
            ''' </summary>
            Public Property Id As Integer

            ''' <summary>
            ''' Gets or sets the item itself.
            ''' </summary>
            Public Property Item As TItem

            ''' <summary>
            ''' Gets or sets the distance between the item and the knn search query.
            ''' </summary>
            Public Property Distance As TDistance
        End Class
    End Class
End Namespace
