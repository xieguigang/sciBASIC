#Region "Microsoft.VisualBasic::921fe3f7c338f988a059d0d9055a2010, gr\network-visualization\network_layout\SpringForce\Layout\Layout\ForceDirected2D.vb"

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

    '   Total Lines: 73
    '    Code Lines: 55
    ' Comment Lines: 3
    '   Blank Lines: 15
    '     File Size: 2.69 KB


    '     Class ForceDirected2D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBoundingBox, GetPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace SpringForce

    ''' <summary>
    ''' Layout provider engine for the 2D network graphics.
    ''' </summary>
    Public Class ForceDirected2D : Inherits ForceDirected(Of FDGVector2)

        Public Sub New(igraph As NetworkGraph, stiffness As Double, repulsion As Double, damping As Double)
            MyBase.New(igraph, stiffness, repulsion, damping)
        End Sub

        Public Overrides Function GetPoint(v As Node) As LayoutPoint
            Dim init0 As FDGVector2
            Dim has As Boolean

            SyncLock nodePoints
                has = nodePoints.ContainsKey(v.label)
            End SyncLock

            If Not has Then
                init0 = TryCast(v.data.initialPostion, FDGVector2)

                If init0 Is Nothing Then
                    init0 = TryCast(FDGVector2.Random(), FDGVector2)
                End If

                Dim v1 As New LayoutPoint(
                    position:=init0,
                    velocity:=FDGVector2.Zero(),
                    acceleration:=FDGVector2.Zero(),
                    node:=v
                )

                SyncLock nodePoints
                    Call nodePoints.Add(v.label, v1)
                End SyncLock
            End If

            Return nodePoints(v.label)
        End Function

        Public Overrides Function GetBoundingBox() As BoundingBox
            Dim boundingBox As New BoundingBox()
            Dim bottomLeft As FDGVector2 = TryCast(FDGVector2.Identity() * (BoundingBox.defaultBB * -1.0F), FDGVector2)
            Dim topRight As FDGVector2 = TryCast(FDGVector2.Identity() * (BoundingBox.defaultBB), FDGVector2)

            For Each v As Node In graph.vertex
                Dim position As FDGVector2 = TryCast(GetPoint(v).position, FDGVector2)

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

            Dim padding As AbstractVector = (topRight - bottomLeft) * BoundingBox.defaultPadding
            boundingBox.bottomLeftFront = bottomLeft - padding
            boundingBox.topRightBack = topRight + padding

            Return boundingBox
        End Function
    End Class
End Namespace
