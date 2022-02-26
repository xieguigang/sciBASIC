#Region "Microsoft.VisualBasic::e7c0679c15bc970a1e6397b310215a49, gr\network-visualization\Datavisualization.Network\Layouts\SpringForce\Layout\Layout\ForceDirected.vb"

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

    '     Class ForceDirected
    ' 
    '         Properties: damping, graph, repulsion, stiffness, threshold
    '                     withinThreshold
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: createSpring, GetSpring, getTotalEnergy, Nearest
    ' 
    '         Sub: (+2 Overloads) applyCoulombsLaw, applyHookesLaw, attractToCentre, Calculate, Clear
    '              EachEdge, EachNode, SetPhysics, updatePosition, updateVelocity
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

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce.Interfaces

Namespace Layouts.SpringForce

    Public MustInherit Class ForceDirected(Of Vector As IVector)
        Implements IForceDirected
        Implements IDisposable

        Public Property stiffness As Double Implements IForceDirected.Stiffness
        Public Property repulsion As Double Implements IForceDirected.Repulsion
        Public Property damping As Double Implements IForceDirected.Damping
        Public Property threshold As Double Implements IForceDirected.Threshold
        Public Property withinThreshold As Boolean Implements IForceDirected.WithinThreshold

        Protected nodePoints As Dictionary(Of String, LayoutPoint)
        Protected edgeSprings As Dictionary(Of String, Spring)
        Private disposedValue As Boolean
        Public Property graph As NetworkGraph Implements IForceDirected.graph

        Public Sub Clear() Implements IForceDirected.Clear
            nodePoints.Clear()
            edgeSprings.Clear()
            graph.Clear()
        End Sub

        Public Sub New(iGraph As NetworkGraph, iStiffness As Double, iRepulsion As Double, iDamping As Double)
            graph = iGraph
            stiffness = iStiffness
            repulsion = iRepulsion
            damping = iDamping
            nodePoints = New Dictionary(Of String, LayoutPoint)()
            edgeSprings = New Dictionary(Of String, Spring)()
            threshold = 0.01F
        End Sub

        Public MustOverride Function GetPoint(iNode As Node) As LayoutPoint Implements IForceDirected.GetPoint

        Public Function GetSpring(iEdge As Edge) As Spring
            If Not (edgeSprings.ContainsKey(iEdge.ID)) Then
                Return createSpring(iEdge)
            Else
                Return edgeSprings(iEdge.ID)
            End If
        End Function

        Private Function createSpring(iedge As Edge) As Spring
            Dim length As Double = iedge.data.length
            Dim existingSpring As Spring = Nothing
            Dim fromEdges As IEnumerable(Of Edge) = graph.GetEdges(iedge.U, iedge.V)

            If fromEdges IsNot Nothing Then
                For Each e As Edge In fromEdges
                    If existingSpring Is Nothing AndAlso edgeSprings.ContainsKey(e.ID) Then
                        existingSpring = edgeSprings(e.ID)
                        Exit For
                    End If

                Next
            End If

            If existingSpring IsNot Nothing Then
                Return New Spring(existingSpring.point1, existingSpring.point2, 0F, 0F)
            End If

            Dim toEdges As IEnumerable(Of Edge) = graph.GetEdges(iedge.V, iedge.U)

            If toEdges IsNot Nothing Then
                For Each e As Edge In toEdges
                    If existingSpring Is Nothing AndAlso edgeSprings.ContainsKey(e.ID) Then
                        existingSpring = edgeSprings(e.ID)
                        Exit For
                    End If
                Next
            End If

            If existingSpring IsNot Nothing Then
                Return New Spring(existingSpring.point2, existingSpring.point1, 0F, 0F)
            End If

            Dim U = GetPoint(iedge.U)
            Dim V = GetPoint(iedge.V)
            Dim link As New Spring(U, V, length, stiffness)

            Call edgeSprings.Add(iedge.ID, link)

            Return edgeSprings(iedge.ID)
        End Function

        ''' <summary>
        ''' 库仑法则，所有的节点之间都存在着斥力
        ''' </summary>
        Protected Sub applyCoulombsLaw()
            For Each n1 As Node In graph.vertex
                Call applyCoulombsLaw(n1, GetPoint(n1))
            Next
        End Sub

        Private Sub applyCoulombsLaw(n1 As Node, point1 As LayoutPoint)
            For Each n2 As Node In graph.vertex
                Dim point2 As LayoutPoint = GetPoint(n2)

                If point1 IsNot point2 Then
                    Dim d As AbstractVector = point1.position - point2.position
                    Dim distance As Double = d.Magnitude() + 0.1F
                    Dim direction As AbstractVector = d.Normalize()

                    If n1.pinned AndAlso n2.pinned Then
                        point1.ApplyForce(direction * 0F)
                        point2.ApplyForce(direction * 0F)
                    ElseIf n1.pinned Then
                        point1.ApplyForce(direction * 0F)
                        'point2.ApplyForce((direction * Repulsion) / (distance * distance * -1.0f));
                        point2.ApplyForce((direction * repulsion) / (distance * -1.0F))
                    ElseIf n2.pinned Then
                        'point1.ApplyForce((direction * Repulsion) / (distance * distance));
                        point1.ApplyForce((direction * repulsion) / (distance))
                        point2.ApplyForce(direction * 0F)
                    Else
                        '                             point1.ApplyForce((direction * Repulsion) / (distance * distance * 0.5f));
                        '                             point2.ApplyForce((direction * Repulsion) / (distance * distance * -0.5f));
                        point1.ApplyForce((direction * repulsion) / (distance * 0.5F))
                        point2.ApplyForce((direction * repulsion) / (distance * -0.5F))
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' 弹簧力，所有的通过边连接的节点间都存在着弹簧的牵引力
        ''' </summary>
        Protected Sub applyHookesLaw()
            For Each e As Edge In graph.graphEdges
                Dim spring As Spring = GetSpring(e)
                Dim d As AbstractVector = spring.point2.position - spring.point1.position
                Dim displacement As Double = spring.length - d.Magnitude()
                Dim direction As AbstractVector = d.Normalize()

                If spring.point1.node.pinned AndAlso spring.point2.node.pinned Then
                    spring.point1.ApplyForce(direction * 0F)
                    spring.point2.ApplyForce(direction * 0F)
                ElseIf spring.point1.node.pinned Then
                    spring.point1.ApplyForce(direction * 0F)
                    spring.point2.ApplyForce(direction * (spring.K * displacement))
                ElseIf spring.point2.node.pinned Then
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -1.0F))
                    spring.point2.ApplyForce(direction * 0F)
                Else
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -0.5F))
                    spring.point2.ApplyForce(direction * (spring.K * displacement * 0.5F))


                End If
            Next
        End Sub

        Protected Sub attractToCentre()
            For Each n As Node In graph.vertex
                Dim point As LayoutPoint = GetPoint(n)
                If Not point.node.pinned Then
                    Dim direction As AbstractVector = point.position * -1.0F
                    'point.ApplyForce(direction * ((float)Math.Sqrt((double)(Repulsion / 100.0f))));


                    Dim displacement As Double = direction.Magnitude()
                    direction = direction.Normalize()
                    point.ApplyForce(direction * (stiffness * displacement * 0.4F))
                End If
            Next
        End Sub

        Protected Sub updateVelocity(iTimeStep As Double)
            For Each n As Node In graph.vertex
                Dim point As LayoutPoint = GetPoint(n)
                point.velocity.Add(point.acceleration * iTimeStep)
                point.velocity.Multiply(damping)
                point.acceleration.SetZero()
            Next
        End Sub

        Protected Sub updatePosition(iTimeStep As Double)
            Dim point As LayoutPoint
            Dim delta As AbstractVector

            For Each n As Node In graph.vertex
                point = GetPoint(n)
                delta = point.velocity * iTimeStep
                point.position.Add(delta)
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
