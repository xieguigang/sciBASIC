Imports System.Runtime.CompilerServices

Namespace Analysis.PageRank

    ''' <summary>
    ''' Weighted pagerank node
    ''' </summary>
    Public Class WeightedPRNode : Inherits Vertex

        Public Property Weight As Double
        Public Property Outbound As Double
        Public Property ConnectedTargets As Dictionary(Of Integer, Double)

    End Class

    Public Class WeightedPRGraph : Inherits Graph(Of WeightedPRNode, Edge(Of WeightedPRNode), WeightedPRGraph)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overloads Sub AddVertex(id As Integer)
            Dim v As New WeightedPRNode With {
                .ID = id,
                .Label = id,
                .ConnectedTargets = New Dictionary(Of Integer, Double)
            }
            Call AddVertex(v)
        End Sub

        ''' <summary>
        ''' Link creates a weighted edge between a source-target node pair.
        ''' If the edge already exists, the weight is incremented.
        ''' </summary>
        ''' <param name="i%">The source</param>
        ''' <param name="j%">The target</param>
        ''' <param name="weight#">Weight value of this edge, default is no weight.</param>
        ''' <returns></returns>
        Public Overrides Function AddEdge(i%, j%, Optional weight# = 0) As WeightedPRGraph
            If Not buffer.Contains(i) Then
                Call AddVertex(id:=i)
            End If

            If Not buffer.Contains(j) Then
                Call AddVertex(id:=j)
            End If

            Return AddEdge(buffer(i).Label, buffer(j).Label, weight)
        End Function

        Public Overrides Function AddEdge(u As String, v As String, Optional weight As Double = 0) As WeightedPRGraph
            Dim edgeKey$ = Edge.EdgeKey(u, v)
            Dim j% = vertices(v).ID

            vertices(u).Outbound += weight

            If Not edges.ContainsKey(edgeKey) Then
                Call AddEdge(vertices(u), vertices(v))
            End If

            With edges(edgeKey)
                .Weight += weight

                If Not .U.ConnectedTargets.ContainsKey(j) Then
                    .U.ConnectedTargets.Add(j, 0)
                End If

                .U.ConnectedTargets(j) += weight
            End With

            Return Me
        End Function
    End Class

    ''' <summary>
    ''' Package pagerank implements the **weighted** PageRank algorithm.
    ''' </summary>
    Public Module WeightedPageRank

        ''' <summary>
        ''' Package pagerank implements the **weighted** PageRank algorithm.
        ''' 
        ''' Rank computes the PageRank of every node in the directed graph.
        ''' This method will run as many iterations as needed, until the graph converges.
        ''' </summary>
        ''' <param name="a#">(alpha) Is the damping factor, usually set to 0.85.</param>
        ''' <param name="e#">(epsilon) Is the convergence criteria, usually set to a tiny value.</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Rank(g As WeightedPRGraph, Optional a# = 0.85, Optional e# = 0.000001) As Dictionary(Of String, Double)
            Dim d# = 1
            Dim inverse# = 1 / g.Vertex.Length

            For Each vertex As WeightedPRNode In g _
                .Vertex _
                .Where(Function(v) v.ConnectedTargets.Count > 0) _
                .ToArray

                If vertex.Outbound > 0 Then
                    For Each target In vertex.ConnectedTargets.Keys.ToArray
                        vertex.ConnectedTargets(target) /= vertex.Outbound
                    Next
                End If
            Next

            For Each vertex As WeightedPRNode In g.Vertex
                vertex.Weight = inverse
            Next

            Do While d >= e

                Dim nodes As New Dictionary(Of Integer, Double)
                Dim leak# = 0

                For Each v As WeightedPRNode In g.Vertex
                    nodes(v.ID) = v.Weight

                    If v.Outbound = 0R Then
                        leak += v.Weight
                    End If

                    v.Weight = 0
                Next

                leak *= a

                For Each edge As WeightedPRNode In g.Vertex
                    Dim source As Integer = edge.ID

                    For Each map In edge.ConnectedTargets
                        g.buffer(map.Key).Weight += a * nodes(source) * map.Value ' weight 
                    Next

                    g.buffer(source).Weight += (1 - a) * inverse + leak * inverse
                Next

                d = 0

                For Each v As WeightedPRNode In g.Vertex
                    d += Math.Abs(v.Weight - nodes(v.ID))
                Next
            Loop

            Return g _
                .Vertex _
                .ToDictionary(Function(v) v.Label,
                              Function(v) v.Weight)
        End Function
    End Module
End Namespace