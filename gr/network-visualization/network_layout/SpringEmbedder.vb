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
    '    Code Lines: 119 (64.32%)
    ' Comment Lines: 32 (17.30%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 34 (18.38%)
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
    ReadOnly maxRepulsiveForceDistance As Double
    ReadOnly c As Double = 3

    ReadOnly nodes As Node()
    ReadOnly edges As Edge()

    ''' <summary>
    ''' 合力累加器（双精度）。原先累加到 <see cref="NodeData.force"/>（System.Drawing.Point，X/Y 为整型），
    ''' 每次赋值都会被截断为整数，导致力被严重量化、布局质量差。这里改用双精度字典绕开该限制。
    ''' </summary>
    ReadOnly forceX As New Dictionary(Of String, Double)
    ReadOnly forceY As New Dictionary(Of String, Double)

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="size"></param>
    ''' <param name="maxRepulsiveForceDistance">
    ''' Repulsive forces between nodes that are further apart than this are ignored.
    ''' 若未指定（传入 NaN），则依据画布尺度给出合理默认值，避免仅在 10px 内生效导致网络无法铺开。
    ''' </param>
    Sub New(g As NetworkGraph, size As Size,
            Optional maxRepulsiveForceDistance As Double = Double.NaN,
            Optional c As Double = 3)

        Me.nodes = g.connectedNodes
        Me.edges = g.graphEdges.ToArray
        Me.c = c
        Me.k = size.Width * size.Height / (nodes.Length * 1000)

        If Double.IsNaN(maxRepulsiveForceDistance) OrElse maxRepulsiveForceDistance <= 0 Then
            ' 与画布尺度相关的合理默认作用距离，使排斥力能够覆盖整个画布
            Me.maxRepulsiveForceDistance = System.Math.Min(size.Width, size.Height) * 0.5
        Else
            Me.maxRepulsiveForceDistance = maxRepulsiveForceDistance
        End If

        For Each n As Node In nodes
            forceX(n.label) = 0.0
            forceY(n.label) = 0.0
        Next
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
        ' 每轮重置双精度合力累加器
        For Each n As Node In nodes
            forceX(n.label) = 0.0
            forceY(n.label) = 0.0
        Next

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

                    ' a 受到背离 b 的排斥力（方向 a - b = -delta）
                    forceX(a.label) = forceX(a.label) - (repulsiveForce * deltaX / distance)
                    forceY(a.label) = forceY(a.label) - (repulsiveForce * deltaY / distance)
                    ' b 受到背离 a 的排斥力（方向 b - a = +delta）
                    forceX(b.label) = forceX(b.label) + (repulsiveForce * deltaX / distance)
                    forceY(b.label) = forceY(b.label) + (repulsiveForce * deltaY / distance)
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

            ' FR 吸引力为 d^2/k，依定义本就不应对距离做“排斥力上限”截断。
            ' 原代码将其截断到 maxRepulsiveForceDistance（默认 10），
            ' 使得 attractiveForce = (100 - k*k)/k ≈ 0，吸引力恒近似为 0，网络无法收拢。
            Dim attractiveForce As Double = (distanceSquared - k * k) / k

            ' Make edges stronger if people know each other.
            Dim weight As Double = edge.weight

            ' 防止 weight <= 0 时 Log 产生 NaN/负无穷
            If weight <= 0 Then
                weight = 1
            End If

            attractiveForce *= (System.Math.Log(weight) * 0.5) + 1

            ' nodeA 受到朝向 nodeB 的吸引力（方向 B - A = +delta）
            forceX(nodeA.label) = forceX(nodeA.label) + (attractiveForce * deltaX / distance)
            forceY(nodeA.label) = forceY(nodeA.label) + (attractiveForce * deltaY / distance)
            ' nodeB 受到朝向 nodeA 的吸引力（方向 A - B = -delta）
            forceX(nodeB.label) = forceX(nodeB.label) - (attractiveForce * deltaX / distance)
            forceY(nodeB.label) = forceY(nodeB.label) - (attractiveForce * deltaY / distance)
        Next
    End Sub

    ''' <summary>
    ''' Now move each node to its new location...
    ''' </summary>
    Private Sub setLocation(c As Double)
        For a As Integer = 0 To nodes.Length - 1
            Dim node As Node = nodes(a)

            Dim xMovement As Double = c * forceX(node.label)
            Dim yMovement As Double = c * forceY(node.label)

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
