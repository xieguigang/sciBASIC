#Region "Microsoft.VisualBasic::3434fa869eb6633702c44719dffdc50f, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\Layouts\ForceDirected\Layout\ForceDirected.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Timers
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces

Namespace Layouts

    Public Class NearestPoint

        Public Sub New()
            node = Nothing
            point = Nothing
            distance = Nothing
        End Sub

        Public node As Node
        Public point As LayoutPoint
        Public distance As System.Nullable(Of Single)
    End Class

    Public Class BoundingBox
        Public Shared defaultBB As Single = 2.0F
        Public Shared defaultPadding As Single = 0.07F
        ' ~5% padding

        Public Sub New()
            topRightBack = Nothing
            bottomLeftFront = Nothing
        End Sub

        Public topRightBack As AbstractVector
        Public bottomLeftFront As AbstractVector
    End Class

    Public MustInherit Class ForceDirected(Of Vector As IVector)
        Implements IForceDirected

        Public Property Stiffness() As Single Implements IForceDirected.Stiffness
        Public Property Repulsion() As Single Implements IForceDirected.Repulsion
        Public Property Damping() As Single Implements IForceDirected.Damping
        Public Property Threadshold() As Single Implements IForceDirected.Threadshold
        Public Property WithinThreashold() As Boolean Implements IForceDirected.WithinThreashold

        Protected m_nodePoints As Dictionary(Of String, LayoutPoint)
        Protected m_edgeSprings As Dictionary(Of String, Spring)
        Public Property graph() As IGraph Implements IForceDirected.graph

        Public Sub Clear() Implements IForceDirected.Clear
            m_nodePoints.Clear()
            m_edgeSprings.Clear()
            graph.Clear()
        End Sub

        Public Sub New(iGraph As IGraph, iStiffness As Single, iRepulsion As Single, iDamping As Single)
            graph = iGraph
            Stiffness = iStiffness
            Repulsion = iRepulsion
            Damping = iDamping
            m_nodePoints = New Dictionary(Of String, LayoutPoint)()
            m_edgeSprings = New Dictionary(Of String, Spring)()

            Threadshold = 0.01F
        End Sub

        Public MustOverride Function GetPoint(iNode As Node) As LayoutPoint Implements IForceDirected.GetPoint

        Public Function GetSpring(iEdge As Edge) As Spring
            If Not (m_edgeSprings.ContainsKey(iEdge.ID)) Then
                Dim length As Single = iEdge.Data.length
                Dim existingSpring As Spring = Nothing

                Dim fromEdges As List(Of Edge) = graph.GetEdges(iEdge.Source, iEdge.Target)
                If fromEdges IsNot Nothing Then
                    For Each e As Edge In fromEdges
                        If existingSpring Is Nothing AndAlso m_edgeSprings.ContainsKey(e.ID) Then
                            existingSpring = m_edgeSprings(e.ID)
                            Exit For
                        End If

                    Next
                End If
                If existingSpring IsNot Nothing Then
                    Return New Spring(existingSpring.point1, existingSpring.point2, 0F, 0F)
                End If

                Dim toEdges As List(Of Edge) = graph.GetEdges(iEdge.Target, iEdge.Source)
                If toEdges IsNot Nothing Then
                    For Each e As Edge In toEdges
                        If existingSpring Is Nothing AndAlso m_edgeSprings.ContainsKey(e.ID) Then
                            existingSpring = m_edgeSprings(e.ID)
                            Exit For
                        End If
                    Next
                End If

                If existingSpring IsNot Nothing Then
                    Return New Spring(existingSpring.point2, existingSpring.point1, 0F, 0F)
                End If

                m_edgeSprings(iEdge.ID) = New Spring(GetPoint(iEdge.Source), GetPoint(iEdge.Target), length, Stiffness)
            End If
            Return m_edgeSprings(iEdge.ID)
        End Function

        ''' <summary>
        ''' 库仑法则，所有的节点之间都存在着斥力
        ''' </summary>
        Protected Sub applyCoulombsLaw()
            For Each n1 As Node In graph.nodes
                Dim point1 As LayoutPoint = GetPoint(n1)
                For Each n2 As Node In graph.nodes
                    Dim point2 As LayoutPoint = GetPoint(n2)
                    If point1 IsNot point2 Then
                        Dim d As AbstractVector = point1.position - point2.position
                        Dim distance As Single = d.Magnitude() + 0.1F
                        Dim direction As AbstractVector = d.Normalize()
                        If n1.Pinned AndAlso n2.Pinned Then
                            point1.ApplyForce(direction * 0F)
                            point2.ApplyForce(direction * 0F)
                        ElseIf n1.Pinned Then
                            point1.ApplyForce(direction * 0F)
                            'point2.ApplyForce((direction * Repulsion) / (distance * distance * -1.0f));
                            point2.ApplyForce((direction * Repulsion) / (distance * -1.0F))
                        ElseIf n2.Pinned Then
                            'point1.ApplyForce((direction * Repulsion) / (distance * distance));
                            point1.ApplyForce((direction * Repulsion) / (distance))
                            point2.ApplyForce(direction * 0F)
                        Else
                            '                             point1.ApplyForce((direction * Repulsion) / (distance * distance * 0.5f));
                            '                             point2.ApplyForce((direction * Repulsion) / (distance * distance * -0.5f));
                            point1.ApplyForce((direction * Repulsion) / (distance * 0.5F))
                            point2.ApplyForce((direction * Repulsion) / (distance * -0.5F))

                        End If
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 弹簧力，所有的通过边连接的节点间都存在着弹簧的牵引力
        ''' </summary>
        Protected Sub applyHookesLaw()
            For Each e As Edge In graph.edges
                Dim spring As Spring = GetSpring(e)
                Dim d As AbstractVector = spring.point2.position - spring.point1.position
                Dim displacement As Single = spring.Length - d.Magnitude()
                Dim direction As AbstractVector = d.Normalize()

                If spring.point1.node.Pinned AndAlso spring.point2.node.Pinned Then
                    spring.point1.ApplyForce(direction * 0F)
                    spring.point2.ApplyForce(direction * 0F)
                ElseIf spring.point1.node.Pinned Then
                    spring.point1.ApplyForce(direction * 0F)
                    spring.point2.ApplyForce(direction * (spring.K * displacement))
                ElseIf spring.point2.node.Pinned Then
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -1.0F))
                    spring.point2.ApplyForce(direction * 0F)
                Else
                    spring.point1.ApplyForce(direction * (spring.K * displacement * -0.5F))
                    spring.point2.ApplyForce(direction * (spring.K * displacement * 0.5F))


                End If
            Next
        End Sub

        Protected Sub attractToCentre()
            For Each n As Node In graph.nodes
                Dim point As LayoutPoint = GetPoint(n)
                If Not point.node.Pinned Then
                    Dim direction As AbstractVector = point.position * -1.0F
                    'point.ApplyForce(direction * ((float)Math.Sqrt((double)(Repulsion / 100.0f))));


                    Dim displacement As Single = direction.Magnitude()
                    direction = direction.Normalize()
                    point.ApplyForce(direction * (Stiffness * displacement * 0.4F))
                End If
            Next
        End Sub

        Protected Sub updateVelocity(iTimeStep As Single)
            For Each n As Node In graph.nodes
                Dim point As LayoutPoint = GetPoint(n)
                point.velocity.Add(point.acceleration * iTimeStep)
                point.velocity.Multiply(Damping)
                point.acceleration.SetZero()
            Next
        End Sub

        Protected Sub updatePosition(iTimeStep As Single)
            For Each n As Node In graph.nodes
                Dim point As LayoutPoint = GetPoint(n)
                point.position.Add(point.velocity * iTimeStep)
            Next
        End Sub

        Protected Function getTotalEnergy() As Single
            Dim energy As Single = 0F
            For Each n As Node In graph.nodes
                Dim point As LayoutPoint = GetPoint(n)
                Dim speed As Single = point.velocity.Magnitude()
                energy += 0.5F * point.mass * speed * speed
            Next
            Return energy
        End Function

        Public Sub Calculate(iTimeStep As Single) Implements IForceDirected.Calculate
            ' time in second
            applyCoulombsLaw()
            applyHookesLaw()
            attractToCentre()
            updateVelocity(iTimeStep)
            updatePosition(iTimeStep)
            If getTotalEnergy() < Threadshold Then
                WithinThreashold = True
            Else
                WithinThreashold = False
            End If
        End Sub


        Public Sub EachEdge(del As EdgeAction) Implements IForceDirected.EachEdge
            For Each e As Edge In graph.edges
                del(e, GetSpring(e))
            Next
        End Sub

        Public Sub EachNode(del As NodeAction) Implements IForceDirected.EachNode
            For Each n As Node In graph.nodes
                del(n, GetPoint(n))
            Next
        End Sub

        Public Function Nearest(position As AbstractVector) As NearestPoint Implements IForceDirected.Nearest
            Dim min As New NearestPoint()
            For Each n As Node In graph.nodes
                Dim point As LayoutPoint = GetPoint(n)
                Dim distance As Single = (point.position - position).Magnitude()
                If min.distance Is Nothing OrElse distance < min.distance Then
                    min.node = n
                    min.point = point
                    min.distance = distance
                End If
            Next
            Return min
        End Function

        Public MustOverride Function GetBoundingBox() As BoundingBox Implements IForceDirected.GetBoundingBox

        Public Sub SetPhysics(Stiffness As Single, Repulsion As Single, Damping As Single) Implements IForceDirected.SetPhysics
            Me.Stiffness = Stiffness
            Me.Repulsion = Repulsion
            Me.Damping = Damping
        End Sub
    End Class

    ''' <summary>
    ''' Layout provider engine for the 2D network graphics.
    ''' </summary>
    Public Class ForceDirected2D
        Inherits ForceDirected(Of FDGVector2)

        Public Sub New(iGraph As IGraph, iStiffness As Single, iRepulsion As Single, iDamping As Single)
            MyBase.New(iGraph, iStiffness, iRepulsion, iDamping)
        End Sub

        Public Overrides Function GetPoint(iNode As Node) As LayoutPoint
            If Not (m_nodePoints.ContainsKey(iNode.ID)) Then
                Dim iniPosition As FDGVector2 = TryCast(iNode.Data.initialPostion, FDGVector2)
                If iniPosition Is Nothing Then
                    iniPosition = TryCast(FDGVector2.Random(), FDGVector2)
                End If
                m_nodePoints(iNode.ID) = New LayoutPoint(iniPosition, FDGVector2.Zero(), FDGVector2.Zero(), iNode)
            End If
            Return m_nodePoints(iNode.ID)
        End Function

        Public Overrides Function GetBoundingBox() As BoundingBox
            Dim boundingBox__1 As New BoundingBox()
            Dim bottomLeft As FDGVector2 = TryCast(FDGVector2.Identity().Multiply(BoundingBox.defaultBB * -1.0F), FDGVector2)
            Dim topRight As FDGVector2 = TryCast(FDGVector2.Identity().Multiply(BoundingBox.defaultBB), FDGVector2)
            For Each n As Node In graph.nodes
                Dim position As FDGVector2 = TryCast(GetPoint(n).position, FDGVector2)

                If position.x < bottomLeft.x Then
                    bottomLeft.x = position.x
                End If
                If position.y < bottomLeft.y Then
                    bottomLeft.y = position.y
                End If
                If position.x > topRight.x Then
                    topRight.x = position.x
                End If
                If position.y > topRight.y Then
                    topRight.y = position.y
                End If
            Next
            Dim padding As AbstractVector = (topRight - bottomLeft).Multiply(BoundingBox.defaultPadding)
            boundingBox__1.bottomLeftFront = bottomLeft.Subtract(padding)
            boundingBox__1.topRightBack = topRight.Add(padding)
            Return boundingBox__1

        End Function
    End Class

    Public Class ForceDirected3D
        Inherits ForceDirected(Of FDGVector3)

        Public Sub New(iGraph As IGraph, iStiffness As Single, iRepulsion As Single, iDamping As Single)
            MyBase.New(iGraph, iStiffness, iRepulsion, iDamping)
        End Sub

        Public Overrides Function GetPoint(iNode As Node) As LayoutPoint
            If Not (m_nodePoints.ContainsKey(iNode.ID)) Then
                Dim iniPosition As FDGVector3 = TryCast(iNode.Data.initialPostion, FDGVector3)
                If iniPosition Is Nothing Then
                    iniPosition = TryCast(FDGVector3.Random(), FDGVector3)
                End If
                m_nodePoints(iNode.ID) = New LayoutPoint(iniPosition, FDGVector3.Zero(), FDGVector3.Zero(), iNode)
            End If
            Return m_nodePoints(iNode.ID)
        End Function

        Public Overrides Function GetBoundingBox() As BoundingBox
            Dim boundingBox__1 As New BoundingBox()
            Dim bottomLeft As FDGVector3 = TryCast(FDGVector3.Identity().Multiply(BoundingBox.defaultBB * -1.0F), FDGVector3)
            Dim topRight As FDGVector3 = TryCast(FDGVector3.Identity().Multiply(BoundingBox.defaultBB), FDGVector3)
            For Each n As Node In graph.nodes
                Dim position As FDGVector3 = TryCast(GetPoint(n).position, FDGVector3)
                If position.x < bottomLeft.x Then
                    bottomLeft.x = position.x
                End If
                If position.y < bottomLeft.y Then
                    bottomLeft.y = position.y
                End If
                If position.z < bottomLeft.z Then
                    bottomLeft.z = position.z
                End If
                If position.x > topRight.x Then
                    topRight.x = position.x
                End If
                If position.y > topRight.y Then
                    topRight.y = position.y
                End If
                If position.z > topRight.z Then
                    topRight.z = position.z
                End If
            Next
            Dim padding As AbstractVector = (topRight - bottomLeft).Multiply(BoundingBox.defaultPadding)
            boundingBox__1.bottomLeftFront = bottomLeft.Subtract(padding)
            boundingBox__1.topRightBack = topRight.Add(padding)
            Return boundingBox__1

        End Function
    End Class
End Namespace
