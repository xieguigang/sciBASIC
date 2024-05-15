#Region "Microsoft.VisualBasic::ddd0bb83d5ba39688b9fac70014f658a, gr\network-visualization\network_layout\SpringForce\Layout\Layout\ForceDirected3D.vb"

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

    '   Total Lines: 61
    '    Code Lines: 49
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.47 KB


    '     Class ForceDirected3D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetBoundingBox, GetPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace SpringForce

    Public Class ForceDirected3D
        Inherits ForceDirected(Of FDGVector3)

        Public Sub New(iGraph As NetworkGraph, iStiffness As Double, iRepulsion As Double, iDamping As Double)
            MyBase.New(iGraph, iStiffness, iRepulsion, iDamping)
        End Sub

        Public Overrides Function GetPoint(v As Node) As LayoutPoint
            If Not nodePoints.ContainsKey(v.label) Then
                Dim iniPosition As FDGVector3 = TryCast(v.data.initialPostion, FDGVector3)

                If iniPosition Is Nothing Then
                    iniPosition = TryCast(FDGVector3.Random(), FDGVector3)
                End If

                nodePoints(v.label) = New LayoutPoint(iniPosition, FDGVector3.Zero(), FDGVector3.Zero(), v)
            End If

            Return nodePoints(v.label)
        End Function

        Public Overrides Function GetBoundingBox() As BoundingBox
            Dim boundingBox As New BoundingBox()
            Dim bottomLeft As FDGVector3 = TryCast(FDGVector3.Identity() * (BoundingBox.defaultBB * -1.0F), FDGVector3)
            Dim topRight As FDGVector3 = TryCast(FDGVector3.Identity() * (BoundingBox.defaultBB), FDGVector3)

            For Each n As Node In graph.vertex
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

            Dim padding As AbstractVector = (topRight - bottomLeft) * BoundingBox.defaultPadding

            boundingBox.bottomLeftFront = bottomLeft - padding
            boundingBox.topRightBack = topRight + padding

            Return boundingBox
        End Function
    End Class
End Namespace
