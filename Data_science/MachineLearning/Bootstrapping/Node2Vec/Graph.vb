Imports System.IO
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Edge = Microsoft.VisualBasic.Data.GraphTheory.VertexEdge
Imports Node = Microsoft.VisualBasic.Data.GraphTheory.Vertex

Namespace node2vec

    ''' <summary>
    ''' Created by freemso on 17-3-14.
    ''' </summary>
    Public Class Graph

        Private Const DEFAULT_WEIGHT As Double = 1

        Private nodeSet As ISet(Of Node) = New HashSet(Of Node)()
        Private edgeSet As ISet(Of Edge) = New HashSet(Of Edge)()

        Private directed As Boolean

        Private p, q As Double

        Private aliasNodes As IDictionary(Of Node, AliasMethod) = New Dictionary(Of Node, AliasMethod)()
        Private aliasEdges As IDictionary(Of Edge, AliasMethod) = New Dictionary(Of Edge, AliasMethod)()

        Public Sub New(file As String, directed As Boolean, Optional p As Double = 1, Optional q As Double = 1)
            Me.directed = directed
            Me.p = p
            Me.q = q

            loadGraphFrom(file)
            preprocess()
        End Sub

        ''' <summary>
        ''' load graph data from file
        ''' input format: node1_id_int node2_id_int &lt;weight_float> </summary>
        ''' <param name="file"> path of the input file </param>
        Private Sub loadGraphFrom(file As String)
            ' read graph info from file
            ' StreamReader fr = new StreamReader(file);
            Dim br As StreamReader = New StreamReader(file)
            Dim lineTxt As Value(Of String) = ""

            While (lineTxt = br.ReadLine()) IsNot Nothing
                ' parse the line text to get the edge info
                Dim strList = lineTxt.Split(" "c)
                Dim node1ID = Integer.Parse(strList(0))
                Dim node2ID = Integer.Parse(strList(1))
                ' add the nodes to the graph
                Dim node1 = addNode(node1ID)
                Dim node2 = addNode(node2ID)
                ' add the edge to the graph
                If strList.Length > 2 Then
                    Dim weight = Double.Parse(strList(2))
                    addEdge(node1, node2, weight)
                Else
                    addEdge(node1, node2, DEFAULT_WEIGHT)
                End If
            End While
        End Sub

        ''' <summary>
        ''' pre-processing of transition probabilities for guiding the random walks
        ''' </summary>
        Private Sub preprocess()
            For Each node As Vertex In nodeSet
                Dim neighbors = sortedNeighborList(node)
                Dim probs As IList(Of Double) = New List(Of Double)()
                Dim weightSum As Double = 0
                For Each neighbor In neighbors
                    ' assert has an edge
                    Dim weight = getEdge(node, neighbor).weight
                    probs.Add(weight)
                    weightSum += weight
                Next
                Dim norm = weightSum
                probs = probs.[Select](Function(aDouble, i) aDouble / norm).AsList()
                aliasNodes(node) = New AliasMethod(probs)
            Next
            For Each edge As Edge In edgeSet
                aliasEdges(edge) = computeAliasEdge(edge)
            Next
        End Sub

        ''' <summary>
        ''' to compute the alias method for an edge </summary>
        ''' <param name="edge"> the edge to compute </param>
        ''' <returns> the node2vec.AliasMethod object that store distribution information </returns>
        Private Function computeAliasEdge(edge As Edge) As AliasMethod
            Dim neighbors = sortedNeighborList(edge.V)
            Dim probs As List(Of Double) = New List(Of Double)()
            Dim weightSum As Double = 0
            For Each neighbor In neighbors
                Dim weight As Double
                If neighbor Is edge.U Then
                    weight = edge.weight / p
                ElseIf hasEdge(neighbor, edge.U) Then
                    weight = edge.weight
                Else
                    weight = edge.weight / q
                End If
                weightSum += weight
                probs.Add(weight)
            Next
            Dim norm = weightSum
            probs = probs.[Select](Function(aDouble, i) aDouble / norm).AsList()
            Return New AliasMethod(probs)
        End Function

        ''' <summary>
        ''' random walk in the graph starting from a node </summary>
        ''' <param name="walkLength"> the steps of this walk </param>
        ''' <param name="startNode"> the start node of this walk </param>
        ''' <returns> the path that we pass, expressed as a Node List </returns>
        Private Function walk(walkLength As Integer, startNode As Node) As IList(Of Node)
            Dim path As New List(Of Node)() From {startNode}

            While path.Count < walkLength
                Dim current = path(path.Count - 1) ' the last node on the path
                Dim neighbors = sortedNeighborList(current)
                If neighbors.Count > 0 Then
                    If path.Count = 1 Then
                        Dim nextIndex As Integer = aliasNodes(current).next()
                        path.Add(neighbors(nextIndex))
                    Else
                        Dim prev = path(path.Count - 2)
                        Dim nextIndex As Integer = aliasEdges(getEdge(prev, current)).next()
                        path.Add(neighbors(nextIndex))
                    End If
                Else
                    Exit While
                End If
            End While
            Return path
        End Function

        ''' <summary>
        ''' simulation of a bunch of walks </summary>
        ''' <param name="numWalks"> iteration times </param>
        ''' <param name="walkLength"> steps of every walk </param>
        ''' <returns> the list of paths that we've walked </returns>
        Public Overridable Iterator Function simulateWalks(numWalks As Integer, walkLength As Integer) As IEnumerable(Of IList(Of Node))
            Dim nodeList As List(Of Node) = New List(Of Node)(nodeSet)
            Console.WriteLine("Walk iteration:")
            For i As Integer = 0 To numWalks - 1
                Console.WriteLine(i + 1.ToString() & "/" & numWalks.ToString())
                nodeList.Shuffle()
                For Each node As Node In nodeList
                    Yield walk(walkLength, node)
                Next
            Next
        End Function

        ''' <summary>
        ''' get a node's neighbors in a sorted list
        ''' the set of the neighbors of node is defined as {x|node-->x}
        ''' sort the nodes according to its ids </summary>
        ''' <param name="node"> the node </param>
        ''' <returns> a sorted list of nodes </returns>
        Private Function sortedNeighborList(node As Node) As IList(Of Node)
            Dim neighborList As List(Of Node) = New List(Of Node)()
            For Each n In nodeSet
                If hasEdge(node, n) Then
                    neighborList.Add(n) ' only node-->n
                End If
            Next
            neighborList = neighborList.Sort(Function(n) n.ID).AsList()
            Return neighborList
        End Function

        ''' <summary>
        ''' check whether there is an edge between two nodes
        ''' note that all the edges in the graph are directive </summary>
        ''' <param name="src"> node1 </param>
        ''' <param name="dst"> node2 </param>
        ''' <returns> true is there is an edge </returns>
        Private Function hasEdge(src As Node, dst As Node) As Boolean
            Dim find As New Edge(src, dst)

            For Each edge As Edge In edgeSet
                If edge.Equals(find) Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' get the edge between two nodes </summary>
        ''' <param name="src"> node1 </param>
        ''' <param name="dst"> node2 </param>
        ''' <returns> the edge, null is not exist such an edge </returns>
        Private Function getEdge(src As Node, dst As Node) As Edge
            Dim find As New Edge(src, dst)

            For Each edge As Edge In edgeSet
                If edge.Equals(find) Then
                    Return edge
                End If
            Next
            Throw New MissingMemberException(find.ToString)
        End Function

        ''' <summary>
        ''' add a new edge to the graph
        ''' if such an edge already exists, update the weight
        ''' note that all the edges in the graph are directed
        ''' if the graph is not directed,
        ''' we just simply add two directed edges with the opposite directions
        ''' that connect two nodes </summary>
        ''' <param name="src"> first node of the edge </param>
        ''' <param name="dst"> second node of the edge </param>
        ''' <param name="weight"> of the edge </param>
        Private Sub addEdge(src As Node, dst As Node, weight As Double)
            If directed Then
                Dim edge As Edge
                If hasEdge(src, dst) Then
                    edge = getEdge(src, dst)
                    edge.weight = weight ' update the weight of the edge
                Else
                    edge = New Edge(src, dst, weight)
                    edgeSet.Add(edge) ' add it to edge set
                End If
            Else
                Dim edge1 As Edge
                Dim edge2 As Edge
                If hasEdge(src, dst) Then
                    edge1 = getEdge(src, dst)
                    edge2 = getEdge(dst, src)
                    ' update the weight of the edges
                    edge1.weight = weight
                    edge2.weight = weight
                Else
                    edge1 = New Edge(src, dst, weight)
                    edge2 = New Edge(dst, src, weight)
                    ' add it to edge set
                    edgeSet.Add(edge1)
                    edgeSet.Add(edge2)
                End If
            End If
        End Sub

        ''' <summary>
        ''' add a node with the id to the graph
        ''' if such a node already exists, return it and do nothing
        ''' if not, create a new node, add it to the graph and return it </summary>
        ''' <param name="id"> the id of the node </param>
        ''' <returns> the node found </returns>
        Private Function addNode(id As Integer) As Node
            For Each v In nodeSet
                If v.ID = id Then
                    Return v
                End If
            Next
            ' not exists, create a new node with the id
            Dim node As New Node(id)
            ' add it to the nodeSet
            nodeSet.Add(node)
            Return node
        End Function
    End Class



End Namespace
