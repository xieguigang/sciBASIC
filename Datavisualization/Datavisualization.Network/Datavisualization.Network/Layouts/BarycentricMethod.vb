Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataVisualization.Network.Graph
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
        Public Sub doLayout(Network As NetworkGraph, iterations As Integer, size As Size)
            Dim nodes As Node() = Network.connectedNodes
            Dim edges As Edge() = Network.edges

            Dim k As Double = size.Width * size.Height / (nodes.Count * 1000)
            Dim c As Double = 3
            ' Repulsive forces between nodes that are further apart than this are ignored.
            Dim maxRepulsiveForceDistance As Double = 10

            ' For each iteration...
            For it As Integer = 0 To iterations - 1

                ' Calculate forces acting on nodes due to node-node repulsions...
                For Each NodeA In nodes
                    For Each NodeB In nodes

                        If NodeA.Equals(NodeB) Then
                            Continue For
                        End If

                        Dim deltaX As Double = NodeB.Location.X - NodeA.Location.X
                        Dim deltaY As Double = NodeB.Location.Y - NodeA.Location.Y

                        Dim distanceSquared As Double = deltaX * deltaX + deltaY * deltaY

                        If distanceSquared < 0.01 Then
                            deltaX = (New Random(1)).NextDouble() / 10 + 0.1
                            deltaY = (New Random(2)).NextDouble() / 10 + 0.1
                            distanceSquared = deltaX * deltaX + deltaY * deltaY
                        End If

                        Dim distance As Double = System.Math.Sqrt(distanceSquared)

                        If distance < maxRepulsiveForceDistance Then
                            Dim repulsiveForce As Double = (k * k / distance)

                            NodeB._force.X = NodeB._force.X + (repulsiveForce * deltaX / distance)
                            NodeB._force.Y = NodeB._force.Y + (repulsiveForce * deltaY / distance)
                            NodeA._force.X = NodeA._force.X - (repulsiveForce * deltaX / distance)
                            NodeA._force.Y = NodeA._force.Y - (repulsiveForce * deltaY / distance)
                        End If
                    Next
                Next

                ' Calculate forces acting on nodes due to edge attractions.
                For e As Integer = 0 To edges.Length - 1
                    Dim edge As Edge = edges(e)
                    Dim nodeA As Node = edge.U
                    Dim nodeB As Node = edge.V

                    Dim deltaX As Double = nodeB.Location.X - nodeA.Location.X
                    Dim deltaY As Double = nodeB.Location.Y - nodeA.Location.Y

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
                    Dim weight As Double = edge._weight

                    attractiveForce *= (System.Math.Log(weight) * 0.5) + 1

                    nodeB._force.X = nodeB._force.X - attractiveForce * deltaX / distance
                    nodeB._force.Y = nodeB._force.Y - attractiveForce * deltaY / distance
                    nodeA._force.X = nodeA._force.X + attractiveForce * deltaX / distance
                    nodeA._force.Y = nodeA._force.Y + attractiveForce * deltaY / distance
                Next

                ' Now move each node to its new location...
                For a As Integer = 0 To nodes.Length - 1
                    Dim node As Node = nodes(a)

                    Dim xMovement As Double = c * node._force.X
                    Dim yMovement As Double = c * node._force.Y

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

                    node.Location = New Point(node.Location.X + xMovement, node.Location.Y + yMovement)
                    node._force = New Point
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
        Public Function ForceDirectedLayout(Network As NetworkGraph, size As Size, Optional cutoff As Double = 100, Optional _DEBUG_EXPORT As String = "") As NetworkGraph
            Network._nodesInnerList = (From Node In Network.nodes
                                       Let randl = New Point(size.Width * RandomDouble(), size.Height * RandomDouble())
                                       Select Node.SetLocation(randl)).ToList
            Call doLayout(Network, 1)

            Return Network
        End Function
    End Module
End Namespace