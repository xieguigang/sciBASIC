#Region "Microsoft.VisualBasic::49ff9aaec38888dbc6d559be2402b719, gr\network-visualization\network_layout\SpringEmbedder.vb"

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

    '   Total Lines: 185
    '    Code Lines: 119
    ' Comment Lines: 32
    '   Blank Lines: 34
    '     File Size: 6.94 KB


    ' Class SpringEmbedder
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ForceDirectedLayout
    ' 
    '     Sub: Collide, doLayout, edgeAttractions, repulsions, setLocation
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Class SpringEmbedder : Implements IPlanner

    ReadOnly k As Double
    ReadOnly maxRepulsiveForceDistance As Double = 10
    ReadOnly c As Double = 3

    ReadOnly nodes As Node()
    ReadOnly edges As Edge()

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="size"></param>
    ''' <param name="maxRepulsiveForceDistance">
    ''' Repulsive forces between nodes that are further apart than this are ignored.
    ''' </param>
    Sub New(g As NetworkGraph, size As Size,
            Optional maxRepulsiveForceDistance As Double = 10,
            Optional c As Double = 3)

        Me.nodes = g.connectedNodes
        Me.edges = g.graphEdges.ToArray
        Me.c = c
        Me.maxRepulsiveForceDistance = maxRepulsiveForceDistance
        Me.k = size.Width * size.Height / (nodes.Length * 1000)
    End Sub

    ''' <summary>
    ''' Applies the spring embedder.
    ''' </summary>
    ''' <param name="iterations"></param>
    Public Sub doLayout(iterations As Integer)
        ' For each iteration...
        For it As Integer = 0 To iterations - 1
            Call Collide(Me.c)
        Next
    End Sub

    Public Sub Collide(Optional timeStep As Double = Double.NaN) Implements IPlanner.Collide
        Call repulsions()
        Call edgeAttractions()
        Call setLocation(c:=timeStep)
    End Sub

    ''' <summary>
    ''' Calculate forces acting on nodes due to node-node repulsions...
    ''' </summary>
    Private Sub repulsions()
        For Each a As Node In nodes
            For Each b In nodes.Where(Function(ni) Not ni Is a)
                Dim deltaX As Double = b.data.initialPostion.x - a.data.initialPostion.x
                Dim deltaY As Double = b.data.initialPostion.y - a.data.initialPostion.y
                Dim distanceSquared As Double = deltaX * deltaX + deltaY * deltaY

                If distanceSquared < 0.01 Then
                    deltaX = randf.seeds.NextDouble() / 10 + 0.1
                    deltaY = randf.seeds.NextDouble() / 10 + 0.1
                    distanceSquared = deltaX * deltaX + deltaY * deltaY
                End If

                Dim distance As Double = System.Math.Sqrt(distanceSquared)

                If distance < maxRepulsiveForceDistance Then
                    Dim repulsiveForce As Double = (k * k / distance)
                    Dim fa As Point = a.data.force
                    Dim fb As Point = b.data.force

                    fb.X = fb.X + (repulsiveForce * deltaX / distance)
                    fb.Y = fb.Y + (repulsiveForce * deltaY / distance)
                    fa.X = fa.X - (repulsiveForce * deltaX / distance)
                    fa.Y = fa.Y - (repulsiveForce * deltaY / distance)

                    a.data.force = fa
                    b.data.force = fb
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Calculate forces acting on nodes due to edge attractions.
    ''' </summary>
    Private Sub edgeAttractions()
        For e As Integer = 0 To edges.Length - 1
            Dim edge As Edge = edges(e)
            Dim nodeA As Node = edge.U
            Dim nodeB As Node = edge.V

            Dim deltaX As Double = nodeB.data.initialPostion.x - nodeA.data.initialPostion.x
            Dim deltaY As Double = nodeB.data.initialPostion.y - nodeA.data.initialPostion.y

            Dim distanceSquared As Double = deltaX * deltaX + deltaY * deltaY

            ' Avoid division by zero error or Nodes flying off to
            ' infinity.  Pretend there is an arbitrary distance between
            ' the Nodes.
            If distanceSquared < 0.01 Then
                deltaX = randf.seeds.NextDouble() / 10 + 0.1
                deltaY = randf.seeds.NextDouble() / 10 + 0.1
                distanceSquared = deltaX * deltaX + deltaY * deltaY
            End If

            Dim distance As Double = System.Math.Sqrt(distanceSquared)

            If distance > maxRepulsiveForceDistance Then
                distance = maxRepulsiveForceDistance
            End If

            distanceSquared = distance * distance

            Dim attractiveForce As Double = (distanceSquared - k * k) / k

            ' Make edges stronger if people know each other.
            Dim weight As Double = edge.weight

            attractiveForce *= (System.Math.Log(weight) * 0.5) + 1

            Dim fa As Point = nodeA.data.force
            Dim fb As Point = nodeB.data.force

            fb.X = nodeB.data.force.X - attractiveForce * deltaX / distance
            fb.Y = nodeB.data.force.Y - attractiveForce * deltaY / distance
            fa.X = nodeA.data.force.X + attractiveForce * deltaX / distance
            fa.Y = nodeA.data.force.Y + attractiveForce * deltaY / distance

            nodeA.data.force = fa
            nodeB.data.force = fb
        Next
    End Sub

    ''' <summary>
    ''' Now move each node to its new location...
    ''' </summary>
    Private Sub setLocation(c As Double)
        For a As Integer = 0 To nodes.Length - 1
            Dim node As Node = nodes(a)

            Dim xMovement As Double = c * node.data.force.X
            Dim yMovement As Double = c * node.data.force.Y

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

            node.data.initialPostion.Point2D = New Point(node.data.initialPostion.x + xMovement, node.data.initialPostion.y + yMovement)
            node.data.force = New Point
        Next
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="network"></param>
    ''' <returns></returns>
    Public Shared Function ForceDirectedLayout(network As NetworkGraph, size As Size, Optional iterations% = 1000) As NetworkGraph
        Call network.vertex _
            .DoEach(Sub(node As Node)
                        Dim randl As New Point With {
                            .X = size.Width * seeds.NextDouble(),
                            .Y = size.Height * seeds.NextDouble()
                        }

                        node.data.initialPostion.Point2D = randl
                    End Sub)

        Call New SpringEmbedder(network, size).doLayout(iterations)

        Return network
    End Function
End Class
