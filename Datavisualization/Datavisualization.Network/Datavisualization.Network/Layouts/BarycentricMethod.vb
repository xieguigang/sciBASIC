Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Layouts

    <PackageNamespace("Network.Layout.Barycentric", Publisher:="xie.guigang@gmail.com")>
    Public Module BarycentricMethod

        ''' <summary>
        ''' Applies the spring embedder.
        ''' </summary>
        ''' <param name="Network"></param>
        ''' <param name="iterations"></param>
        <ExportAPI("Layout.SpringEmbedder")>
        <Extension>
        Public Sub doLayout(Network As NetworkGraph, iterations As Integer, size As Size)
            Dim nodes As Node() = Network.connectedNodes
            Dim edges As Edge() = Network.edges

            Dim k As Double = size.Width * size.Height / (nodes.Length * 1000)
            Dim c As Double = 3
            ' Repulsive forces between nodes that are further apart than this are ignored.
            Dim maxRepulsiveForceDistance As Double = 10

            ' For each iteration...
            For it As Integer = 0 To iterations - 1

                ' Calculate forces acting on nodes due to node-node repulsions...
                For Each NodeA As Node In nodes
                    For Each NodeB In nodes

                        If NodeA.Equals(NodeB) Then
                            Continue For
                        End If

                        Dim deltaX As Double = NodeB.Data.initialPostion.x - NodeA.Data.initialPostion.x
                        Dim deltaY As Double = NodeB.Data.initialPostion.y - NodeA.Data.initialPostion.y

                        Dim distanceSquared As Double = deltaX * deltaX + deltaY * deltaY

                        If distanceSquared < 0.01 Then
                            deltaX = (New Random(1)).NextDouble() / 10 + 0.1
                            deltaY = (New Random(2)).NextDouble() / 10 + 0.1
                            distanceSquared = deltaX * deltaX + deltaY * deltaY
                        End If

                        Dim distance As Double = System.Math.Sqrt(distanceSquared)

                        If distance < maxRepulsiveForceDistance Then
                            Dim repulsiveForce As Double = (k * k / distance)
                            Dim fa As Point = NodeA.Data.Force
                            Dim fb As Point = NodeB.Data.Force

                            fb.X = fb.X + (repulsiveForce * deltaX / distance)
                            fb.Y = fb.Y + (repulsiveForce * deltaY / distance)
                            fa.X = fa.X - (repulsiveForce * deltaX / distance)
                            fa.Y = fa.Y - (repulsiveForce * deltaY / distance)

                            NodeA.Data.Force = fa
                            NodeB.Data.Force = fb
                        End If
                    Next
                Next

                ' Calculate forces acting on nodes due to edge attractions.
                For e As Integer = 0 To edges.Length - 1
                    Dim edge As Edge = edges(e)
                    Dim nodeA As Node = edge.Source
                    Dim nodeB As Node = edge.Target

                    Dim deltaX As Double = nodeB.Data.initialPostion.x - nodeA.Data.initialPostion.x
                    Dim deltaY As Double = nodeB.Data.initialPostion.y - nodeA.Data.initialPostion.y

                    Dim distanceSquared As Double = deltaX * deltaX + deltaY * deltaY

                    ' Avoid division by zero error or Nodes flying off to
                    ' infinity.  Pretend there is an arbitrary distance between
                    ' the Nodes.
                    If distanceSquared < 0.01 Then
                        deltaX = (New Random(3)).NextDouble() / 10 + 0.1
                        deltaY = (New Random(4)).NextDouble() / 10 + 0.1
                        distanceSquared = deltaX * deltaX + deltaY * deltaY
                    End If

                    Dim distance As Double = System.Math.Sqrt(distanceSquared)

                    If distance > maxRepulsiveForceDistance Then
                        distance = maxRepulsiveForceDistance
                    End If

                    distanceSquared = distance * distance

                    Dim attractiveForce As Double = (distanceSquared - k * k) / k

                    ' Make edges stronger if people know each other.
                    Dim weight As Double = edge.Data.weight

                    attractiveForce *= (System.Math.Log(weight) * 0.5) + 1

                    Dim fa As Point = nodeA.Data.Force
                    Dim fb As Point = nodeB.Data.Force

                    fb.X = nodeB.Data.Force.X - attractiveForce * deltaX / distance
                    fb.Y = nodeB.Data.Force.Y - attractiveForce * deltaY / distance
                    fa.X = nodeA.Data.Force.X + attractiveForce * deltaX / distance
                    fa.Y = nodeA.Data.Force.Y + attractiveForce * deltaY / distance

                    nodeA.Data.Force = fa
                    nodeB.Data.Force = fb
                Next

                ' Now move each node to its new location...
                For a As Integer = 0 To nodes.Length - 1
                    Dim node As Node = nodes(a)

                    Dim xMovement As Double = c * node.Data.Force.X
                    Dim yMovement As Double = c * node.Data.Force.Y

                    ' Limit movement values to stop nodes flying into oblivion.
                    Dim max As Double = 100
                    If xMovement > max Then
                        xMovement = max
                    ElseIf xMovement < -max Then
                        xMovement = -max
                    End If
                    If yMovement > max Then
                        yMovement = max
                    ElseIf yMovement < -max Then
                        yMovement = -max
                    End If

                    node.Data.initialPostion.Point2D = New Point(node.Data.initialPostion.x + xMovement, node.Data.initialPostion.y + yMovement)
                    node.Data.Force = New Point
                Next
            Next
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Network"></param>
        ''' <param name="cutoff"></param>
        ''' <param name="_DEBUG_EXPORT"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Layout.ForceDirected")>
        Public Function ForceDirectedLayout(Network As NetworkGraph,
                                            size As Size,
                                            Optional cutoff As Double = 100,
                                            Optional _DEBUG_EXPORT As String = "") As NetworkGraph
            Network.nodes =
                LinqAPI.MakeList(Of Node) <= From node As Node
                                             In Network.nodes
                                             Let randl As Point = New Point(
                                                 size.Width * RandomDouble(),
                                                 size.Height * RandomDouble())
                                             Select node.__setLoci(randl)
            Call doLayout(Network, 1, size)

            Return Network
        End Function

        <Extension>
        Private Function __setLoci(node As Node, randl As Point) As Node
            node.Data.initialPostion.Point2D = randl
            Return node
        End Function
    End Module
End Namespace