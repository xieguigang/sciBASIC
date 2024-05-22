#Region "Microsoft.VisualBasic::6621368f3a5e5766e106ba0f16ff49ba, gr\network-visualization\network_layout\SpringForce\Layout\Layout\ForceDirected.vb"

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

    '   Total Lines: 417
    '    Code Lines: 295 (70.74%)
    ' Comment Lines: 62 (14.87%)
    '    - Xml Docs: 14.52%
    ' 
    '   Blank Lines: 60 (14.39%)
    '     File Size: 17.06 KB


    '     Class ForceDirected
    ' 
    '         Properties: damping, Entity, graph, Height, interactiveMode
    '                     parallel, radius, repulsion, stiffness, threshold
    '                     Width, withinThreshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: createSpring, GetSpring, getTotalEnergy, Nearest
    ' 
    '         Sub: (+2 Overloads) applyCoulombsLaw, (+2 Overloads) applyHookesLaw, attractToCentre, Calculate, Clear
    '              (+2 Overloads) Dispose, EachEdge, EachNode, Flush, SetPhysics
    '              updatePosition, updateVelocity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file ForceDirected.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief ForceDirected Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the ForceDirected Class.
'
'

Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces
Imports Microsoft.VisualBasic.Imaging.Physics
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

Namespace SpringForce

    Public MustInherit Class ForceDirected(Of Vector As AbstractVector)
        Implements IForceDirected
        Implements IDisposable
        Implements IContainer(Of LayoutPoint)

        Public Property stiffness As Double Implements IForceDirected.Stiffness
        Public Property repulsion As Double Implements IForceDirected.Repulsion
        Public Property damping As Double Implements IForceDirected.Damping
        Public Property threshold As Double Implements IForceDirected.Threshold
        Public Property withinThreshold As Boolean Implements IForceDirected.WithinThreshold
        Public Property parallel As Boolean = False
        Public Property radius As Double = 30

        Protected nodePoints As New Dictionary(Of String, LayoutPoint)
        Protected edgeSprings As New Dictionary(Of String, Spring)

        Dim disposedValue As Boolean
        Dim grid As Grid(Of LayoutPoint())

        Public Property graph As NetworkGraph Implements IForceDirected.graph
        Public Property interactiveMode As Boolean = False Implements IForceDirected.interactiveMode
        Private ReadOnly Property Entity As IReadOnlyCollection(Of LayoutPoint) Implements IContainer(Of LayoutPoint).Entity
            Get
                Return nodePoints.Values
            End Get
        End Property

        Public Property Width As Double = 1920 Implements IContainer(Of LayoutPoint).Width, IForceDirected.width
        Public Property Height As Double = 1080 Implements IContainer(Of LayoutPoint).Height, IForceDirected.height

        Public Sub Clear() Implements IForceDirected.Clear
            nodePoints.Clear()
            edgeSprings.Clear()
            graph.Clear()
        End Sub

        Public Sub New(igraph As NetworkGraph, stiffness As Double, repulsion As Double, damping As Double)
            Me.graph = igraph
            Me.stiffness = stiffness
            Me.repulsion = repulsion
            Me.damping = damping
            Me.threshold = 0.01F
        End Sub

        Public MustOverride Function GetPoint(iNode As Node) As LayoutPoint Implements IForceDirected.GetPoint

        Public Function GetSpring(edge As Edge) As Spring
            Dim check As Boolean

            SyncLock edgeSprings
                check = edgeSprings.ContainsKey(edge.ID)
            End SyncLock

            If Not check Then
                Return createSpring(edge)
            Else
                SyncLock edgeSprings
                    Return edgeSprings(edge.ID)
                End SyncLock
            End If
        End Function

        Private Function createSpring(edge As Edge) As Spring
            Dim length As Double = edge.data.length
            Dim existingSpring As Spring = Nothing
            Dim fromEdges As IEnumerable(Of Edge) = graph.GetEdges(edge.U, edge.V)

            If fromEdges IsNot Nothing Then
                For Each e As Edge In fromEdges
                    If existingSpring Is Nothing AndAlso edgeSprings.ContainsKey(e.ID) Then
                        existingSpring = edgeSprings(e.ID)
                        Exit For
                    End If
                Next
            End If

            If existingSpring IsNot Nothing Then
                Return New Spring(existingSpring.A, existingSpring.B, 0F, 0F)
            End If

            Dim toEdges As IEnumerable(Of Edge) = graph.GetEdges(edge.V, edge.U)

            If toEdges IsNot Nothing Then
                For Each e As Edge In toEdges
                    If existingSpring Is Nothing AndAlso edgeSprings.ContainsKey(e.ID) Then
                        existingSpring = edgeSprings(e.ID)
                        Exit For
                    End If
                Next
            End If

            If existingSpring IsNot Nothing Then
                Return New Spring(existingSpring.B, existingSpring.A, 0F, 0F)
            End If

            Dim U = GetPoint(edge.U)
            Dim V = GetPoint(edge.V)
            Dim link As New Spring(U, V, length, stiffness)

            SyncLock edgeSprings
                If Not edgeSprings.ContainsKey(edge.ID) Then
                    Call edgeSprings.Add(edge.ID, link)
                End If

                Return edgeSprings(edge.ID)
            End SyncLock
        End Function

        ''' <summary>
        ''' 库仑法则，所有的节点之间都存在着斥力
        ''' </summary>
        Protected Sub applyCoulombsLaw()
            If parallel Then
                Call graph.vertex _
                    .AsParallel _
                    .Select(Function(n1)
                                Call applyCoulombsLaw(n1, GetPoint(n1))
                                Return True
                            End Function) _
                    .ToArray
            Else
                For Each n1 As Node In graph.vertex
                    Call applyCoulombsLaw(n1, GetPoint(n1))
                Next
            End If
        End Sub

        Private Sub applyCoulombsLaw(n1 As Node, partner As LayoutPoint)
            ' Dim around = grid.SpatialLookup(partner, radius)

            For Each current As LayoutPoint In nodePoints.Values
                If partner IsNot current Then
                    Dim d As AbstractVector = partner.position - current.position
                    Dim distance As Double = d.Magnitude() + 0.1F
                    Dim direction As AbstractVector = d.Normalize()

                    If n1.pinned AndAlso current.node.pinned Then
                        partner.ApplyForce(direction * 0F)
                        current.ApplyForce(direction * 0F)
                    ElseIf n1.pinned Then
                        partner.ApplyForce(direction * 0F)
                        current.ApplyForce((direction * repulsion) / (distance * -1.0F))
                    ElseIf current.node.pinned Then
                        partner.ApplyForce((direction * repulsion) / (distance))
                        current.ApplyForce(direction * 0F)
                    Else
                        partner.ApplyForce((direction * repulsion) / (distance * 0.5F))
                        current.ApplyForce((direction * repulsion) / (distance * -0.5F))
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' 弹簧力，所有的通过边连接的节点间都存在着弹簧的牵引力
        ''' </summary>
        Protected Sub applyHookesLaw()
            If parallel Then
                Call graph.graphEdges _
                    .AsParallel _
                    .Select(Function(e)
                                Call applyHookesLaw(e)
                                Return True
                            End Function) _
                    .ToArray
            Else
                For Each e As Edge In graph.graphEdges
                    Call applyHookesLaw(e)
                Next
            End If
        End Sub

        Private Sub applyHookesLaw(e As Edge)
            Dim spring As Spring = GetSpring(e)
            Dim d As AbstractVector = spring.B.position - spring.A.position
            Dim displacement As Double = spring.length - d.Magnitude()
            Dim direction As AbstractVector = d.Normalize()

            If spring.A.node.pinned AndAlso spring.B.node.pinned Then
                spring.A.ApplyForce(direction * 0F)
                spring.B.ApplyForce(direction * 0F)
            ElseIf spring.A.node.pinned Then
                spring.A.ApplyForce(direction * 0F)
                spring.B.ApplyForce(direction * (spring.K * displacement))
            ElseIf spring.B.node.pinned Then
                spring.A.ApplyForce(direction * (spring.K * displacement * -1.0F))
                spring.B.ApplyForce(direction * 0F)
            Else
                spring.A.ApplyForce(direction * (spring.K * displacement * -0.5F))
                spring.B.ApplyForce(direction * (spring.K * displacement * 0.5F))
            End If
        End Sub

        Protected Sub attractToCentre()
            For Each n As Node In graph.vertex
                Dim point As LayoutPoint = GetPoint(n)

                If Not point.node.pinned Then
                    Dim direction As AbstractVector = point.position * -1.0F
                    Dim displacement As Double = direction.Magnitude()

                    direction = direction.Normalize()
                    point.ApplyForce(direction * (stiffness * displacement * 0.4F))
                End If
            Next
        End Sub

        Protected Sub updateVelocity(timeStep As Double)
            For Each n As Node In graph.vertex
                Dim point As LayoutPoint = GetPoint(n)

                point.velocity = point.velocity + (point.acceleration * timeStep)
                point.velocity = point.velocity * damping
                point.acceleration.SetZero()
            Next
        End Sub

        Protected Sub updatePosition(timeStep As Double)
            Dim point As LayoutPoint
            Dim delta As AbstractVector
            Dim x, y, z As Double
            Dim maxCanvas As Double = 1000000

            For Each n As Node In graph.vertex
                point = GetPoint(n)
                x = point.position.x
                y = point.position.y
                z = point.position.z
                delta = point.velocity * timeStep
                point.position += delta

                If interactiveMode Then
                    ' 20220625 
                    ' 在这里仅处理非实数的情况
                    ' 实数约束会使交互式模式下布局失效
                    If point.position.x.IsNaNImaginary Then
                        point.position.x = 0
                    End If
                    If point.position.y.IsNaNImaginary Then
                        point.position.y = 0
                    End If
                    If point.position.z.IsNaNImaginary Then
                        point.position.z = 0
                    End If
                Else
                    If point.position.x.IsNaNImaginary OrElse stdNum.Abs(point.position.x) > maxCanvas OrElse point.position.x < 0 Then
                        point.position.x = randf.NextDouble * x / 100
                    End If
                    If point.position.y.IsNaNImaginary OrElse stdNum.Abs(point.position.y) > maxCanvas OrElse point.position.y < 0 Then
                        point.position.y = randf.NextDouble * y / 100
                    End If
                    If point.position.z.IsNaNImaginary OrElse stdNum.Abs(point.position.z) > maxCanvas OrElse point.position.z < 0 Then
                        point.position.z = randf.NextDouble * z / 100
                    End If
                End If

                If point.position.x = 0.0 AndAlso point.position.y = 0.0 AndAlso point.position.z = 0.0 Then
                    point.position.x = randf.NextDouble * (maxCanvas)
                    point.position.y = randf.NextDouble * (maxCanvas)
                    point.position.z = randf.NextDouble * (maxCanvas)
                End If
            Next
        End Sub

        Protected Function getTotalEnergy() As Double
            Dim energy As Double = 0F

            For Each n As Node In graph.vertex
                Dim point As LayoutPoint = GetPoint(n)
                Dim speed As Double = point.velocity.Magnitude()

                energy += 0.5F * point.mass * speed * speed
            Next

            Return energy
        End Function

        Public Sub Calculate(Optional timeStep As Double = Double.NaN) Implements IForceDirected.Collide
            ' grid = Me.EncodeGrid(radius:=radius)

            ' time in second
            applyCoulombsLaw()
            applyHookesLaw()
            attractToCentre()
            updateVelocity(timeStep)
            updatePosition(timeStep)

            If getTotalEnergy() < threshold Then
                withinThreshold = True
            Else
                withinThreshold = False
            End If
        End Sub


        Public Sub EachEdge(del As EdgeAction) Implements IForceDirected.EachEdge
            For Each e As Edge In graph.graphEdges
                del(e, GetSpring(e))
            Next
        End Sub

        Public Sub EachNode(del As NodeAction) Implements IForceDirected.EachNode
            For Each n As Node In graph.vertex
                del(n, GetPoint(n))
            Next
        End Sub

        Public Function Nearest(position As AbstractVector) As NearestPoint Implements IForceDirected.Nearest
            Dim min As New NearestPoint()
            Dim point As LayoutPoint
            Dim distance As Double

            For Each n As Node In graph.vertex
                point = GetPoint(n)
                distance = (point.position - position).Magnitude()

                If min.distance Is Nothing OrElse distance < min.distance Then
                    min.node = n
                    min.point = point
                    min.distance = distance
                End If
            Next

            Return min
        End Function

        Public MustOverride Function GetBoundingBox() As BoundingBox Implements IForceDirected.GetBoundingBox

        Public Sub SetPhysics(Stiffness As Double, Repulsion As Double, Damping As Double) Implements IForceDirected.SetPhysics
            Me.stiffness = Stiffness
            Me.repulsion = Repulsion
            Me.damping = Damping
        End Sub

        ''' <summary>
        ''' write node layout position
        ''' </summary>
        Public Sub Flush()
            Call EachNode(Sub(node As Node, point As LayoutPoint)
                              node.data.initialPostion = point.position
                          End Sub)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call Flush()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
