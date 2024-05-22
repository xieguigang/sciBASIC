#Region "Microsoft.VisualBasic::ada7bb2dfc9b5d7432688ef545abc7f1, gr\network-visualization\network_layout\EdgeBundling\Mingle\Bundler.vb"

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

    '   Total Lines: 513
    '    Code Lines: 406 (79.14%)
    ' Comment Lines: 31 (6.04%)
    '    - Xml Docs: 48.39%
    ' 
    '   Blank Lines: 76 (14.81%)
    '     File Size: 21.30 KB


    '     Class Bundler
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: buildKdTree, coalesceNodes, costFunction, EnumerateNodes, getCentroids
    '                   getCombinedNode, getInkValue, getMaximumInkSavingNeighbor, getMaxTurningAngleValue, goldenSectionSearch
    '                   ToString
    ' 
    '         Sub: buildNearestNeighborGraph, bundle, coalesceGraph, computeIntermediateNodePositions, MINGLE
    '              setNodes, updateGraph
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript
Imports number = System.Double
Imports stdNum = System.Math

Namespace EdgeBundling.Mingle

    'bundle = New Bundler({
    '  angleStrength: angleStrength
    '});
    'bundle.setNodes(json);
    'bundle.buildNearestNeighborGraph(neighbors);
    'bundle.MINGLE();

    ''' <summary>
    ''' Edge bundling algorithm class.
    ''' </summary>
    Public Class Bundler

        ReadOnly graph As New NetworkGraph
        ReadOnly options As Options

        Sub New()
            Call Me.New(New Options)
        End Sub

        Sub New(opts As Options)
            Me.options = opts
        End Sub

        Public Function EnumerateNodes() As IEnumerable(Of Node)
            Return graph.vertex
        End Function

        Public Overrides Function ToString() As String
            Return graph.ToString
        End Function

        ''' <summary>
        ''' 这里仅仅是计算节点分组，与边连接无关
        ''' 边连接在渲染的时候才会需要
        ''' </summary>
        ''' <param name="nodes">
        ''' 节点应该是已经完成了布局计算之后，已经具有位置信息的节点
        ''' </param>
        Public Sub setNodes(nodes As IEnumerable(Of Node))
            Call graph.Clear()

            For Each node As Node In nodes
                graph.AddNode(node)
            Next
        End Sub

        Private Function buildKdTree(ByRef kdTree As KdTree(Of GraphKdNode)) As Dictionary(Of String, GraphKdNode)
            Dim nodeArray As New Dictionary(Of String, GraphKdNode)

            For Each v As Node In graph.vertex
                Dim coords As Double() = DirectCast(v.data, MingleNodeData).coords
                Dim n As New GraphKdNode(v) With {
                    .x = coords(0),
                    .y = coords(1),
                    .z = coords(2),
                    .w = coords(3)
                }

                Call nodeArray.Add(v.label, n)
            Next

            kdTree = New KdTree(Of GraphKdNode)(nodeArray.Values.ToArray, New Accessor)

            Return nodeArray
        End Function

        Public Sub buildNearestNeighborGraph(Optional k As Integer = 10)
            Dim node As KdTreeNode(Of GraphKdNode)
            Dim dist As Double
            Dim kdTree As KdTree(Of GraphKdNode) = Nothing
            Dim hashList As Dictionary(Of String, GraphKdNode) = buildKdTree(kdTree)

            For Each n As Node In graph.vertex
                Dim nodes As KdNodeHeapItem(Of GraphKdNode)() = kdTree.nearest(hashList(n.label), k).ToArray

                For i As Integer = 0 To nodes.Length - 1
                    node = nodes(i).node
                    dist = nodes(i).distance

                    If node.data.v.ID <> n.ID Then
                        Call graph.CreateEdge(n, node.data.v)
                    End If
                Next
            Next
        End Sub

        Public Sub computeIntermediateNodePositions(node As Node)
            Dim centroids As Double()()
            Dim a As number, b As number, c As number, tau As number
            Dim f As Func(Of Double, Double), res As Double

            If DirectCast(node.data, MingleNodeData).nodes.IsNullOrEmpty Then
                Return
            End If
            centroids = getCentroids(DirectCast(node.data, MingleNodeData).nodes)
            f = Function(x As Double) costFunction(node, centroids, x)
            a = 0
            b = 1
            c = 0.72 ' because computers
            tau = 0.1
            res = goldenSectionSearch(a, b, c, tau, f)
            f(res) ' Set m1 And m2;
        End Sub

        Public Function costFunction(node As Node, centroids As number()(), x As number) As number
            Dim top As Double(), bottom As Double(), m1 As Double(), m2 As Double(), ink, alpha, p As Double
            x /= 2
            top = centroids(0)
            bottom = centroids(1)
            m1 = lerp(top, bottom, x)
            m2 = lerp(top, bottom, 1 - x)

            Dim data As MingleNodeData = node.data

            data.m1 = m1
            data.m2 = m2
            ' Delete node.data.ink
            ink = getInkValue(node)
            alpha = getMaxTurningAngleValue(node, m1, m2)
            p = If(options.angleStrength, 1.2)
            Return ink * (1 + stdNum.Sin(alpha) / p)
        End Function

        Public Function goldenSectionSearch(a As number, b As number, c As number, tau As number, f As Func(Of Double, Double)) As number
            Dim phi = MINGLE_PHI,
            resphi = 2 - phi,
             x As number

            If (c - b > b - a) Then
                x = b + resphi * (c - b)
            Else
                x = b - resphi * (b - a)
            End If
            If (stdNum.Abs(c - a) < tau * (stdNum.Abs(b) + stdNum.Abs(x))) Then
                Return (c + a) / 2
            End If
            If (f(x) < f(b)) Then
                If (c - b > b - a) Then
                    Return goldenSectionSearch(b, x, c, tau, f)
                End If
                Return goldenSectionSearch(a, x, b, tau, f)
            End If
            If (c - b > b - a) Then
                Return goldenSectionSearch(a, b, x, tau, f)
            End If
            Return goldenSectionSearch(x, b, c, tau, f)
        End Function

        Public Function getCentroids(nodes As Node()) As number()()
            Dim topCentroid As Double() = {0, 0},
            bottomCentroid As Double() = {0, 0},
            coords As number()
            Dim l As Integer = nodes.Length

            For i As Integer = 0 To nodes.Length - 1
                coords = DirectCast(nodes(i).data, MingleNodeData).coords
                topCentroid(0) += coords(0)
                topCentroid(1) += coords(1)
                bottomCentroid(0) += coords(2)
                bottomCentroid(1) += coords(3)
            Next

            topCentroid(0) /= l
            topCentroid(1) /= l
            bottomCentroid(0) /= l
            bottomCentroid(1) /= l

            Return {topCentroid, bottomCentroid}
        End Function

        Public Function getInkValue(Node As Node, Optional depth As number = 0) As Double
            Dim data As MingleNodeData = Node.data
            Dim coords As Double()
            Dim diffX, diffY As Double
            Dim m1 As number(), m2 As number()
            Dim acum As number, l As Integer, nodes As Node(), ni As Node

            ' bundled node
            If depth = 0 AndAlso (data.bundle IsNot Nothing OrElse Not data.nodes.IsNullOrEmpty) Then
                nodes = If(Not data.bundle Is Nothing, DirectCast(data.bundle.data, MingleNodeData).nodes, data.nodes)
                m1 = data.m1
                m2 = data.m2
                acum = 0
                l = nodes.Length

                For i As Integer = 0 To nodes.Length - 1
                    ni = nodes(i)
                    coords = DirectCast(ni.data, MingleNodeData).coords
                    diffX = m1(0) - coords(0)
                    diffY = m1(1) - coords(1)
                    acum += norm({diffX, diffY})
                    diffX = m2(0) - coords(2)
                    diffY = m2(1) - coords(3)
                    acum += norm({diffX, diffY})
                    acum += getInkValue(ni, depth + 1)
                Next
                If depth = 0 Then
                    acum += dist(m1, m2)
                End If

                Return DirectCast(Node.data, MingleNodeData).ink = acum
            End If

            ' coalesced node
            If Not data.parents.IsNullOrEmpty Then
                nodes = data.parents
                m1 = {data.coords(0), data.coords(1)}
                m2 = {data.coords(2), data.coords(3)}
                acum = 0
                For i As Integer = 0 To nodes.Length - 1
                    ni = nodes(i)
                    coords = DirectCast(ni.data, MingleNodeData).coords
                    diffX = m1(0) - coords(0)
                    diffY = m1(1) - coords(1)
                    acum += norm({diffX, diffY})
                    diffX = m2(0) - coords(2)
                    diffY = m2(1) - coords(3)
                    acum += norm({diffX, diffY})
                    acum += getInkValue(ni, depth + 1)
                Next
                ' only add the distance if this Is the first recursion
                If depth = 0 Then
                    acum += dist(m1, m2)
                End If
                Return DirectCast(Node.data, MingleNodeData).ink = acum
            End If

            ' simple node
            If depth <> 0 Then
                Return DirectCast(Node.data, MingleNodeData).ink = 0
            Else
                coords = DirectCast(Node.data, MingleNodeData).coords
                diffX = coords(0) - coords(2)
                diffY = coords(1) - coords(3)
            End If

            Return DirectCast(Node.data, MingleNodeData).ink = norm({diffX, diffY})
        End Function

        Public Function getMaxTurningAngleValue(node As Node, m1 As number(), m2 As number()) As Double
            Dim m2Tom1 = {m1(0) - m2(0), m1(1) - m2(1)},
            m1Tom2 = {-m2Tom1(0), -m2Tom1(1)},
            m1m2Norm = InternalMath.norm(m2Tom1),
            angle As Double = 0
            Dim nodes As Node()
            Dim vec As number(), norm As number, dot As number, angleValue As Double, coords As number()
            Dim data As MingleNodeData = DirectCast(node.data, MingleNodeData)

            If data.bundle IsNot Nothing OrElse Not data.nodes.IsNullOrEmpty Then
                nodes = If(data.bundle IsNot Nothing, DirectCast(data.bundle.data, MingleNodeData).nodes, data.nodes)
                For i As Integer = 0 To nodes.Length - 1
                    coords = DirectCast(nodes(i).data, MingleNodeData).coords
                    vec = {coords(0) - m1(0), coords(1) - m1(1)}
                    norm = InternalMath.norm(vec)
                    dot = vec(0) * m2Tom1(0) + vec(1) * m2Tom1(1)
                    angleValue = stdNum.Abs(stdNum.Acos(dot / norm / m1m2Norm))
                    angle = If(angle < angleValue, angleValue, angle)

                    vec = {coords(2) - m2(0), coords(3) - m2(1)}
                    norm = InternalMath.norm(vec)
                    dot = vec(0) * m1Tom2(0) + vec(1) * m1Tom2(1)
                    angleValue = stdNum.Abs(stdNum.Acos(dot / norm / m1m2Norm))
                    angle = If(angle < angleValue, angleValue, angle)
                Next

                Return angle
            End If

            Return -1
        End Function

        Public Function getCombinedNode(node1 As Node, node2 As Node, Optional data As MingleNodeData = Nothing) As Node
            node1 = If(DirectCast(node1.data, MingleNodeData).bundle, node1)
            node2 = If(DirectCast(node2.data, MingleNodeData).bundle, node2)

            ' Dim id = node1.ID & "-" & node2.ID
            Dim name = node1.label & "-" & node2.label
            Dim nodes1 = If(DirectCast(node1.data, MingleNodeData).nodes.IsNullOrEmpty, {node1}, DirectCast(node1.data, MingleNodeData).nodes)
            Dim nodes2 = If(DirectCast(node2.data, MingleNodeData).nodes.IsNullOrEmpty, {node2}, DirectCast(node2.data, MingleNodeData).nodes)

            Dim weight1 As Double = node1.data.mass,
            weight2 As Double = node2.data.mass,
            Nodes As New List(Of Node), ans As Node

            If (node1.ID = node2.ID) Then
                Return node1
            End If
            Nodes.AddRange(nodes1)
            Nodes.AddRange(nodes2)

            If data Is Nothing Then
                data = New MingleNodeData
            End If

            data.nodes = Nodes.ToArray
            data.nodeArray = DirectCast(node1.data, MingleNodeData).nodeArray _
                .JoinIterates(DirectCast(node2.data, MingleNodeData).nodeArray) _
                .ToArray
            data.mass = weight1 + weight2

            ans = New Node With {
                .label = name,
                .data = data
            }

            computeIntermediateNodePositions(ans)

            Return ans
        End Function

        ''' <summary>
        ''' the generated node id is assigned via the bundle object id
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Public Function coalesceNodes(nodes As Node()) As Node
            Dim node = nodes(0),
                Data As MingleNodeData = node.data,
                m1 = Data.m1,
                m2 = Data.m2,
                weight = nodes.reduce(Function(acum, n) acum + n.data.mass, 0),
                coords As Double() = Data.coords,
                bundle As Node = Data.bundle,
                nodeArray As New List(Of Node)

            If Not m1.IsNullOrEmpty Then
                coords = {m1(0), m1(1), m2(0), m2(1)}

                ' flattened nodes for cluster.
                For i As Integer = 0 To nodes.Length - 1
                    nodeArray.AddRange(If(DirectCast(nodes(i).data, MingleNodeData).nodeArray.IsNullOrEmpty, If(DirectCast(nodes(i).data, MingleNodeData).parents.IsNullOrEmpty, New Node() {}, {nodes(i)}), DirectCast(nodes(i).data, MingleNodeData).nodeArray))
                Next

                If Not options.sort Is Nothing Then
                    nodeArray = nodeArray.OrderBy(options.sort).ToList
                End If

                Return New Node With {
                    .ID = bundle.ID,
                    .label = bundle.label,
                    .data = New MingleNodeData With {
                        .nodeArray = nodeArray.ToArray,
                        .parents = nodes,
                        .coords = coords,
                        .mass = weight,
                        .parentsInk = CDbl(DirectCast(bundle.data, MingleNodeData).ink)
                    }
                }
            End If

            Return nodes(0)
        End Function

        Public Sub bundle(combinedNode As Node, node1 As Node, node2 As Node)
            DirectCast(node1.data, MingleNodeData).bundle = combinedNode
            DirectCast(node2.data, MingleNodeData).bundle = combinedNode

            DirectCast(node1.data, MingleNodeData).ink = CDbl(DirectCast(combinedNode.data, MingleNodeData).ink)
            DirectCast(node1.data, MingleNodeData).m1 = DirectCast(combinedNode.data, MingleNodeData).m1
            DirectCast(node1.data, MingleNodeData).m2 = DirectCast(combinedNode.data, MingleNodeData).m2
            ' node1.data.nodeArray = combinedNode.data.nodeArray

            DirectCast(node2.data, MingleNodeData).ink = CDbl(DirectCast(combinedNode.data, MingleNodeData).ink)
            DirectCast(node2.data, MingleNodeData).m1 = DirectCast(combinedNode.data, MingleNodeData).m1
            DirectCast(node2.data, MingleNodeData).m2 = DirectCast(combinedNode.data, MingleNodeData).m2
            ' node2.data.nodeArray = combinedNode.data.nodeArray
        End Sub

        Public Sub updateGraph(graph As NetworkGraph, groupedNode As Node, nodes As Node(), ids As Dictionary(Of String, Node))
            Dim n As Node, connections As New List(Of Node)
            Dim checkConnection = Sub(e As Edge)
                                      Dim nodeToId = e.V.label
                                      If ids.ContainsKey(nodeToId) Then
                                          connections.Add(e.V)
                                      End If
                                  End Sub
            For i As Integer = 0 To nodes.Length - 1
                n = nodes(i)
                connections = New List(Of Node)
                n.eachEdge(checkConnection)
                graph.RemoveNode(n.label)
            Next
            graph.AddNode(groupedNode, assignId:=False)
            For i As Integer = 0 To connections.Count - 1
                graph.AddEdge(groupedNode, connections(i))
            Next
        End Sub

        Public Sub coalesceGraph()
            Dim newGraph As New NetworkGraph()
            Dim groupsIds As New Dictionary(Of String, Dictionary(Of String, Node))
            Dim maxGroup As Integer = Integer.MinValue, Nodes As List(Of Node)
            Dim ids As Dictionary(Of String, Node), groupedNode As Node

            For Each node In graph.vertex
                Dim group = DirectCast(node.data, MingleNodeData).group
                If (maxGroup < group) Then
                    maxGroup = group
                End If

                Dim groupKey As String = group

                If Not groupsIds.ContainsKey(groupKey) Then
                    groupsIds(groupKey) = New Dictionary(Of String, Node)
                End If
                groupsIds(groupKey)(node.label) = node
            Next

            maxGroup += 1

            Do While maxGroup > 0
                maxGroup -= 1

                ids = groupsIds(maxGroup.ToString)
                Nodes = New List(Of Node)
                For Each i In ids.Keys
                    Nodes.Add(ids(i))
                Next
                If Nodes.Count <> 0 Then
                    groupedNode = coalesceNodes(Nodes.ToArray)
                    updateGraph(graph, groupedNode, Nodes.ToArray, ids)
                End If
            Loop
        End Sub

        Public Function getMaximumInkSavingNeighbor(n As Node) As MingleData
            Dim nodeFrom = n,
            inkFrom = getInkValue(nodeFrom),
            inkTotal = Double.PositiveInfinity,
            bundle As Node() = New Node() {Nothing, Nothing},
            combinedBundle As Node = Nothing

            n.eachEdge(Sub(e As Edge)
                           Dim nodeTo = e.V,
                inkTo = getInkValue(nodeTo),
                combined As Node = getCombinedNode(nodeFrom, nodeTo),
                inkUnion = getInkValue(combined),
                inkValue = inkUnion - (inkFrom + inkTo)

                           If (inkTotal > inkValue) Then
                               inkTotal = inkValue
                               bundle(0) = nodeFrom
                               bundle(1) = nodeTo
                               combinedBundle = combined
                           End If
                       End Sub)

            Return New MingleData With {
           .bundle = bundle,
           .inkTotal = inkTotal,
           .combined = combinedBundle
        }
        End Function

        Public Sub MINGLE()
            Dim edgeProximityGraph As NetworkGraph = graph,
            that = Me,
            totalGain = 0,
            ungrouped = -1,
            gain = 0,
            k = 0,
            clean = Sub(n As Node) DirectCast(n.data, MingleNodeData).group = ungrouped,
            nodeMingle = Sub(node As Node)
                             If DirectCast(node.data, MingleNodeData).group = ungrouped Then
                                 Dim ans = that.getMaximumInkSavingNeighbor(node),
                        bundle = ans.bundle,
                        u = bundle(0),
                        v = bundle(1),
                        combined = ans.combined,
                        gainUV = -ans.inkTotal

                                 ' graph has been collapsed And Is now only one node
                                 If u Is Nothing AndAlso v Is Nothing Then
                                     gain = Integer.MinValue
                                     Return
                                 End If

                                 If (gainUV > 0) Then
                                     that.bundle(combined, u, v)
                                     gain += gainUV
                                     If DirectCast(v.data, MingleNodeData).group <> ungrouped Then
                                         DirectCast(u.data, MingleNodeData).group = DirectCast(v.data, MingleNodeData).group
                                     Else
                                         DirectCast(v.data, MingleNodeData).group = k
                                         DirectCast(u.data, MingleNodeData).group = k
                                     End If
                                 Else
                                     DirectCast(u.data, MingleNodeData).group = k
                                 End If
                                 k += 1
                             End If
                         End Sub

            Do
                gain = 0
                k = 0
                edgeProximityGraph.Each(clean)
                edgeProximityGraph.Each(nodeMingle)
                coalesceGraph()
                totalGain += gain
            Loop While gain > 0
        End Sub
    End Class
End Namespace
