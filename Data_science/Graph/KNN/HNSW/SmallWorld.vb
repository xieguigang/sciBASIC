' <copyright file="SmallWorld.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' <see href="https://arxiv.org/abs/1603.09320">Hierarchical Navigable Small World Graphs</see>.
    ''' </summary>
    ''' <typeparam name="TItem">The type of items to connect into small world.</typeparam>
    ''' <typeparam name="TDistance">The type of distance between items (expect any numeric type: float, double, decimal, int, ...).</typeparam>
    Partial Public Class SmallWorld(Of TItem, TDistance As IComparable(Of TDistance))
        ''' <summary>
        ''' The distance function in the items space.
        ''' </summary>
        Private ReadOnly distance As Func(Of TItem, TItem, TDistance)

        ''' <summary>
        ''' The hierarchical small world graph instance.
        ''' </summary>
        Private graph As Graph(Of TItem, TDistance)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SmallWorld(Of TItem,TDistance)"/> class.
        ''' </summary>
        ''' <param name="distance">The distance funtion to use in the small world.</param>
        Public Sub New(distance As Func(Of TItem, TItem, TDistance))
            Me.distance = distance
        End Sub

        ''' <summary>
        ''' Builds hnsw graph from the items.
        ''' </summary>
        ''' <param name="items">The items to connect into the graph.</param>
        ''' <param name="generator">The random number generator for building graph.</param>
        ''' <param name="parameters">Parameters of the algorithm.</param>
        Public Sub BuildGraph(items As IList(Of TItem), generator As Random, parameters As Parameters(Of TItem, TDistance))
            graph = New Graph(Of TItem, TDistance)(distance, parameters)
            graph.Create(items, generator)
        End Sub

        ''' <summary>
        ''' Run knn search for a given item.
        ''' </summary>
        ''' <param name="item">The item to search nearest neighbours.</param>
        ''' <param name="k">The number of nearest neighbours.</param>
        ''' <returns>The list of found nearest neighbours.</returns>
        Public Function KNNSearch(item As TItem, k As Integer) As IList(Of KNNSearchResult(Of TItem, TDistance))
            Dim destination = graph.NewNode(-1, item, 0)
            Dim neighbourhood = graph.KNearest(destination, k)
            Return neighbourhood.[Select](Function(n) New KNNSearchResult(Of TItem, TDistance) With {
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
            If graph Is Nothing Then
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
            Return graph.Print()
        End Function
    End Class
End Namespace
