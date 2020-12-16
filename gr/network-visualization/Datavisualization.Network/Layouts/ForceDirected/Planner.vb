#Region "Microsoft.VisualBasic::6e966f08ec33ae04d623bee94494c1b1, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\Planner.vb"

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

    '     Class Planner
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Collide, runAttraction, runRepulsive, setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace Layouts.ForceDirected

    Public Class Planner

        ReadOnly CANVAS_WIDTH As Integer = 1000
        ReadOnly CANVAS_HEIGHT As Integer = 1000

        ReadOnly g As NetworkGraph

        Dim mDxMap As New Dictionary(Of String, Double)
        Dim mDyMap As New Dictionary(Of String, Double)

        Dim k As Double
        Dim ejectFactor As Integer = 6
        Dim condenseFactor As Integer = 3
        Dim maxtx As Integer = 4
        Dim maxty As Integer = 3
        Dim dist_thresh As DoubleRange

        Sub New(g As NetworkGraph,
                Optional ejectFactor As Integer = 6,
                Optional condenseFactor As Integer = 3,
                Optional maxtx As Integer = 4,
                Optional maxty As Integer = 3,
                Optional dist_threshold$ = "30,250",
                Optional size$ = "1000,1000")

            Me.g = g

            With size.SizeParser
                CANVAS_WIDTH = .Width
                CANVAS_HEIGHT = .Height
            End With

            Me.dist_thresh = dist_threshold.NumericRangeParser
            Me.maxtx = maxtx
            Me.maxty = maxty
            Me.condenseFactor = condenseFactor
            Me.ejectFactor = ejectFactor
            Me.k = stdNum.Sqrt(CANVAS_WIDTH * CANVAS_HEIGHT / g.vertex.Count)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Collide()
            Call runRepulsive()
            Call runAttraction()
            Call setPosition()
        End Sub

        ''' <summary>
        ''' 节点之间的排斥力
        ''' </summary>
        Private Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim ejectFactor = Me.ejectFactor

            For Each v As Node In g.vertex
                id = v.label

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    distX = v.data.initialPostion.x - u.data.initialPostion.x
                    distY = v.data.initialPostion.y - u.data.initialPostion.y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)

                    If (dist < dist_thresh.Min) Then
                        ejectFactor = 5
                    End If

                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        mDxMap(id) = mDxMap(id) + (distX / dist * k * k / dist) * ejectFactor
                        mDyMap(id) = mDyMap(id) + (distY / dist * k * k / dist) * ejectFactor
                    End If
                Next
            Next
        End Sub

        Private Sub runAttraction()
            Dim u, v As Node
            Dim distX, distY, dist As Double

            For Each edge As Edge In g.graphEdges
                u = edge.U
                v = edge.V
                distX = u.data.initialPostion.x - v.data.initialPostion.x
                distY = u.data.initialPostion.y - v.data.initialPostion.y
                dist = stdNum.Sqrt(distX * distX + distY * distY)
                mDxMap(u.label) = mDxMap(u.label) - distX * dist / k * condenseFactor
                mDyMap(u.label) = mDyMap(u.label) - distY * dist / k * condenseFactor
                mDxMap(v.label) = mDxMap(v.label) + distX * dist / k * condenseFactor
                mDyMap(v.label) = mDyMap(v.label) + distY * dist / k * condenseFactor
            Next
        End Sub

        Private Sub setPosition()
            Dim dx, dy As Double
            Dim x, y As Double

            For Each node As Node In g.vertex
                dx = mDxMap(node.label)
                dy = mDyMap(node.label)

                If (dx < -maxtx) Then dx = -maxtx
                If (dx > maxtx) Then dx = maxtx
                If (dy < -maxty) Then dy = -maxty
                If (dy > maxty) Then dy = maxty

                x = node.data.initialPostion.x
                y = node.data.initialPostion.y
                x = If((x + dx) >= CANVAS_WIDTH OrElse (x + dx) <= 0, x - dx, x + dx)
                y = If((y + dy) >= CANVAS_HEIGHT OrElse (y + dy <= 0), y - dy, y + dy)

                node.data.initialPostion.x = x
                node.data.initialPostion.y = y
            Next
        End Sub
    End Class
End Namespace
