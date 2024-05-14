#Region "Microsoft.VisualBasic::0268e791285b8985610570d8c00ddd60, Data_science\Graph\Analysis\PageRank\WeightedPRGraph.vb"

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

    '   Total Lines: 172
    '    Code Lines: 104
    ' Comment Lines: 33
    '   Blank Lines: 35
    '     File Size: 6.22 KB


    '     Class WeightedPRNode
    ' 
    '         Properties: ConnectedTargets, Outbound, Weight
    ' 
    '     Class WeightedPRGraph
    ' 
    '         Function: (+2 Overloads) AddEdge
    ' 
    '         Sub: AddVertex
    ' 
    '     Module WeightedPageRank
    ' 
    '         Function: Rank
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math
Imports WeightTable = System.Collections.Generic.Dictionary(Of Integer, Double)

Namespace Analysis.PageRank

    ''' <summary>
    ''' Weighted pagerank node
    ''' </summary>
    Public Class WeightedPRNode : Inherits Vertex

        Public Property Weight As Double
        Public Property Outbound As Double
        Public Property ConnectedTargets As WeightTable

    End Class

    Public Class WeightedPRGraph : Inherits Graph(Of WeightedPRNode, Edge(Of WeightedPRNode), WeightedPRGraph)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overloads Sub AddVertex(id As Integer)
            Dim v As New WeightedPRNode With {
                .ID = id,
                .Label = id,
                .ConnectedTargets = New WeightTable
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
            If Not buffer.ContainsKey(i) Then
                Call AddVertex(id:=i)
            End If

            If Not buffer.ContainsKey(j) Then
                Call AddVertex(id:=j)
            End If

            Return AddEdge(buffer(key:=CUInt(i)).label, buffer(key:=CUInt(j)).label, weight)
        End Function

        ''' <summary>
        ''' <paramref name="u"/>和<paramref name="v"/>都是<see cref="WeightedPRNode.Label"/>
        ''' </summary>
        ''' <param name="u">the source node</param>
        ''' <param name="v">the target node</param>
        ''' <param name="weight"></param>
        ''' <returns></returns>
        Public Overrides Function AddEdge(u As String, v As String, Optional weight As Double = 0) As WeightedPRGraph
            Dim j% = vertices(v).ID

            vertices(u).Outbound += weight

            If Not ExistEdge(u, v) Then
                Call AddEdge(vertices(u), vertices(v))
            End If

            With QueryEdge(from:=u, [to]:=v)
                .weight += weight

                If .U.ConnectedTargets Is Nothing Then
                    .U.ConnectedTargets = New WeightTable
                End If

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
            Dim inverse# = 1 / g.size.vertex

            For Each vertex As WeightedPRNode In g _
                .Vertex _
                .Where(Function(v)
                           Return Not v _
                               .ConnectedTargets _
                               .IsNullOrEmpty
                       End Function) _
                .ToArray

                If vertex.Outbound > 0 Then
                    For Each target In vertex.ConnectedTargets.Keys.ToArray
                        vertex.ConnectedTargets(target) /= vertex.Outbound
                    Next
                End If
            Next

            For Each vertex As WeightedPRNode In g.Vertex
                vertex.Weight = inverse

                If vertex.ConnectedTargets Is Nothing Then
                    vertex.ConnectedTargets = New WeightTable
                End If
            Next

            Do While d >= e

                Dim nodes As New WeightTable
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
                        g.buffer(key:=CUInt(map.Key)).Weight += a * nodes(source) * map.Value ' weight 
                    Next

                    g.buffer(key:=CUInt(source)).Weight += (1 - a) * inverse + leak * inverse
                Next

                d = 0

                For Each v As WeightedPRNode In g.vertex
                    d += stdNum.Abs(v.Weight - nodes(v.ID))
                Next
            Loop

            ' 在这里不可以按照weight从大到小排序，因为这会打乱原文的顺序，
            ' 可能会造成NLP模块所产生的摘要文本语句之间的逻辑不顺
            Return g _
                .vertex _
                .ToDictionary(Function(v) v.label,
                              Function(v)
                                  Return v.Weight
                              End Function)
        End Function
    End Module
End Namespace
