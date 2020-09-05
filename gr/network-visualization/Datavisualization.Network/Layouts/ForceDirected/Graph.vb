Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports JetBrains.Annotations
Imports System.Runtime.InteropServices

Namespace Layouts.ForceDirected
    ''' <summary>
    ''' Class Graph. This class cannot be inherited.
    ''' </summary>
    Public Class Graph
        ''' <summary>
        ''' The edges
        ''' </summary>
        <NotNull>
        Private ReadOnly _edges As HashSet(Of Edge)

        ''' <summary>
        ''' The vertices
        ''' </summary>
        <NotNull>
        Private ReadOnly _vertices As ConcurrentDictionary(Of Vertex, HashSet(Of Edge))

        ''' <summary>
        ''' Gets all edges.
        ''' </summary>
        ''' <value>The edges.</value>
        <NotNull>
        Public ReadOnly Property Edges As IEnumerable(Of Edge)
            Get
                Return _edges
            End Get
        End Property

        ''' <summary>
        ''' Gets all vertices.
        ''' </summary>
        ''' <value>The vertices.</value>
        <NotNull>
        Public ReadOnly Property Vertices As IEnumerable(Of Vertex)
            Get
                Return _vertices.Keys
            End Get
        End Property

        ''' <summary>
        ''' Gets all edges of a given <paramrefname="vertex"/>.
        ''' </summary>
        ''' <value>The edges.</value>
        <NotNull>
        Default Public ReadOnly Property Item(
        <NotNull> ByVal vertex As Vertex) As IEnumerable(Of Edge)
            Get
                Return _vertices(vertex)
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <seecref="Graph"/> class.
        ''' </summary>
        ''' <paramname="edges">The edges.</param>
        Public Sub New(
<NotNull> ParamArray edges As Edge())
            Me.New(CType(edges, IReadOnlyCollection(Of Edge)))
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <seecref="Graph"/> class.
        ''' </summary>
        ''' <paramname="edges">The edges.</param>
        Public Sub New(
<NotNull> ByVal edges As IEnumerable(Of Edge))
            Dim edgeCollection = New HashSet(Of Edge)()
            Dim vertices = New ConcurrentDictionary(Of Vertex, HashSet(Of Edge))()

            For Each edge In edges
                ' skip duplicate edges
                If edgeCollection.Contains(edge) Then Continue For
                edgeCollection.Add(edge)
                AddToEdgeLookup(vertices, edge)
            Next

            _vertices = vertices
            _edges = edgeCollection
        End Sub

        ''' <summary>
        ''' Adds the <paramrefname="edge"/> to edge <paramrefname="lookup"/>.
        ''' </summary>
        ''' <paramname="lookup">The vertices.</param>
        ''' <paramname="edge">The edge.</param>
        Private Shared Sub AddToEdgeLookup(
<NotNull> ByVal lookup As ConcurrentDictionary(Of Vertex, HashSet(Of Edge)),
<NotNull> ByVal edge As Edge)
            AddToEdgeLookup(lookup, edge.Left, edge)
            AddToEdgeLookup(lookup, edge.Right, edge)
        End Sub

        ''' <summary>
        ''' Adds the <paramrefname="vertex"/> to edge <paramrefname="lookup"/>.
        ''' </summary>
        ''' <paramname="lookup">The vertices.</param>
        ''' <paramname="vertex">The vertex.</param>
        ''' <paramname="edge">The edge.</param>
        Private Shared Sub AddToEdgeLookup(
<NotNull> ByVal lookup As ConcurrentDictionary(Of Vertex, HashSet(Of Edge)),
<NotNull> ByVal vertex As Vertex,
<NotNull> ByVal edge As Edge)
            lookup.AddOrUpdate(vertex, Function(v) New HashSet(Of Edge) From {
                edge
            }, Function(v, list)
                   list.Add(edge)
                   Return list
               End Function)
        End Sub

        ''' <summary>
        ''' Tries to obtain the edge between two vertices.
        ''' </summary>
        ''' <paramname="firstVertex">The first vertex.</param>
        ''' <paramname="secondVertex">The second vertex.</param>
        ''' <paramname="edge">The edge.</param>
        ''' <returns><c>true</c> if such an edge exists, <c>false</c> otherwise.</returns>
        <ContractAnnotation("=>true,edge:notnull;=>false,edge:null")>
        Public Function TryGetEdge(
        <NotNull> ByVal firstVertex As Vertex,
        <NotNull> ByVal secondVertex As Vertex, <Out>
        <CanBeNull> ByRef edge As Edge) As Boolean
            edge = Nothing

            ' attempt to obtain the edge; should always succeed for existing nodes
            Dim edges As HashSet(Of Edge)
            If Not _vertices.TryGetValue(firstVertex, edges) Then Return False

            ' find the matching counterpart
            edge = edges.FirstOrDefault(Function(e) e.Equals(secondVertex)) ' TODO: optimize that
            Return edge IsNot Nothing
        End Function
    End Class
End Namespace
