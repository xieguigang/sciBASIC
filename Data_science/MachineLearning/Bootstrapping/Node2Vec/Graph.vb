Imports System.IO
Imports Microsoft.VisualBasic.Math

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

        Public Sub New(file As String, directed As Boolean, p As Double, q As Double)
            Me.directed = directed
            Me.p = p
            Me.q = q

            loadGraphFrom(file)
            preprocess()
        End Sub

        ''' <summary>
        ''' load graph data from file
        ''' input format: node1_id_int node2_id_int <weight_float> </summary>
        ''' <param name="file"> path of the input file </param>
        Private Sub loadGraphFrom(file As String)
            ' read graph info from file
            ' StreamReader fr = new StreamReader(file);
            Dim br As StreamReader = New StreamReader(file)
            Dim lineTxt As String
            While Not String.ReferenceEquals((CSharpImpl.__Assign(lineTxt, br.ReadLine())), Nothing)
                ' parse the line text to get the edge info
                Dim strList = lineTxt.Split(" ")
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
            For Each node In nodeSet
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
                probs = probs.[Select](Function(aDouble, i) CSharpImpl.__Assign(aDouble, norm)).ToList()
                aliasNodes(node) = New AliasMethod(probs)
            Next
            For Each edge In edgeSet
                aliasEdges(edge) = computeAliasEdge(edge)
            Next
        End Sub

        ''' <summary>
        ''' to compute the alias method for an edge </summary>
        ''' <param name="edge"> the edge to compute </param>
        ''' <returns> the node2vec.AliasMethod object that store distribution information </returns>
        Private Function computeAliasEdge(edge As Edge) As AliasMethod
            Dim neighbors = sortedNeighborList(edge.dst)
            Dim probs As List(Of Double) = New List(Of Double)()
            Dim weightSum As Double = 0
            For Each neighbor In neighbors
                Dim weight As Double
                If neighbor Is edge.src Then
                    weight = edge.weight / p
                ElseIf hasEdge(neighbor, edge.src) Then
                    weight = edge.weight
                Else
                    weight = edge.weight / q
                End If
                weightSum += weight
                probs.Add(weight)
            Next
            Dim norm = weightSum
            probs = probs.[Select](Function(aDouble, i) CSharpImpl.__Assign(aDouble, norm)).ToList()
            Return New AliasMethod(probs)
        End Function

        ''' <summary>
        ''' random walk in the graph starting from a node </summary>
        ''' <param name="walkLength"> the steps of this walk </param>
        ''' <param name="startNode"> the start node of this walk </param>
        ''' <returns> the path that we pass, expressed as a Node List </returns>
        Private Function walk(walkLength As Integer, startNode As Node) As IList(Of Node)
            Dim path As IList(Of Node) = New List(Of Node)()
            path.Add(startNode)

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
        Public Overridable Function simulateWalks(numWalks As Integer, walkLength As Integer) As List(Of IList(Of Node))
            Dim pathList As List(Of IList(Of Node)) = New List(Of IList(Of Node))()
            Console.WriteLine("Walk iteration:")
            Dim nodeList As List(Of Node) = New List(Of Node)(nodeSet)
            For i = 0 To numWalks - 1
                Console.WriteLine(i + 1.ToString() & "/" & numWalks.ToString())
                nodeList.Shuffle()
                For Each node In nodeList
                    pathList.Add(walk(walkLength, node))
                Next
            Next
            Return pathList
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
            neighborList = neighborList.Sort(Function(n) n.idField).ToList()
            Return neighborList
        End Function

        ''' <summary>
        ''' check whether there is an edge between two nodes
        ''' note that all the edges in the graph are directive </summary>
        ''' <param name="src"> node1 </param>
        ''' <param name="dst"> node2 </param>
        ''' <returns> true is there is an edge </returns>
        Private Function hasEdge(src As Node, dst As Node) As Boolean
            For Each edge In edgeSet
                If edge.Equals(New Edge(Me, src, dst)) Then
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
            For Each edge In edgeSet
                If edge.Equals(New Edge(Me, src, dst)) Then
                    Return edge
                End If
            Next
            Throw New Exception()
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
                    edge = New Edge(Me, src, dst, weight)
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
                    edge1 = New Edge(Me, src, dst, weight)
                    edge2 = New Edge(Me, dst, src, weight)
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
                If v.idField = id Then
                    Return v
                End If
            Next
            ' not exists, create a new node with the id
            Dim node As Node = New Node(Me, id)
            ' add it to the nodeSet
            nodeSet.Add(node)
            Return node
        End Function

        Public Class Node
            Private ReadOnly outerInstance As Graph


            Friend idField As Integer

            Friend Sub New(outerInstance As Graph, id As Integer)
                Me.outerInstance = outerInstance
                idField = id
            End Sub

            Friend Overridable Function Equals(that As Node) As Boolean
                Return idField = that.idField
            End Function

            Public Overridable ReadOnly Property Id As Integer
                Get
                    Return idField
                End Get
            End Property
        End Class

        Friend Class Edge
            Private ReadOnly outerInstance As Graph


            Friend src, dst As Node
            Friend weight As Double

            Friend Sub New(outerInstance As Graph, src As Node, dst As Node)
                Me.outerInstance = outerInstance
                If src Is Nothing OrElse dst Is Nothing Then
                    Throw New ArgumentException()
                End If
                Me.src = src
                Me.dst = dst
            End Sub

            Friend Sub New(outerInstance As Graph, src As Node, dst As Node, weight As Double)
                Me.outerInstance = outerInstance
                If src Is Nothing OrElse dst Is Nothing Then
                    Throw New ArgumentException()
                End If
                Me.src = src
                Me.dst = dst
                Me.weight = weight
            End Sub

            ''' <summary>
            ''' two edges are equal if and only if they start at the same node
            ''' and end at the same node </summary>
            ''' <param name="that"> the node to compare </param>
            ''' <returns> true if two are equal </returns>
            Friend Overridable Function Equals(that As Edge) As Boolean
                Return src.Equals(that.src) AndAlso dst.Equals(that.dst)
            End Function
        End Class

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class

    End Class



End Namespace
