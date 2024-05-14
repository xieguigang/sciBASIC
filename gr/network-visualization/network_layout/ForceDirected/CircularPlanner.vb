#Region "Microsoft.VisualBasic::ca88e26411eda790557a54081c936bbd, gr\network-visualization\network_layout\ForceDirected\CircularPlanner.vb"

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

    '   Total Lines: 75
    '    Code Lines: 51
    ' Comment Lines: 7
    '   Blank Lines: 17
    '     File Size: 2.85 KB


    '     Class CircularPlanner
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: runRepulsive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace ForceDirected

    Public Class CircularPlanner : Inherits Planner

        Dim center As PointF
        Dim maxRadius As Double

        Public Sub New(g As NetworkGraph,
                       Optional ejectFactor As Integer = 6,
                       Optional condenseFactor As Integer = 3,
                       Optional maxtx As Integer = 4,
                       Optional maxty As Integer = 3,
                       Optional dist_threshold As String = "30,250",
                       Optional size As String = "1000,1000")

            MyBase.New(g, ejectFactor, condenseFactor, maxtx, maxty, dist_threshold, size)

            Me.center = New PointF(CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2)
            Me.maxRadius = stdNum.Min(CANVAS_HEIGHT, CANVAS_WIDTH) / 2
        End Sub

        Protected Overrides Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim ejectFactor = Me.ejectFactor
            Dim dx, dy As Double

            For Each v As Node In g.vertex
                id = v.label

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Node In g.vertex.Where(Function(ui) Not ui Is v)
                    distX = v.data.initialPostion.x - u.data.initialPostion.x
                    distY = v.data.initialPostion.y - u.data.initialPostion.y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)

                    'If (dist < dist_thresh.Min) Then
                    '    ejectFactor = 5
                    'End If

                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        dx = (distX / dist * k * k / dist) * ejectFactor
                        dy = (distY / dist * k * k / dist) * ejectFactor

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next

                ' 还会被中心点排斥
                distX = v.data.initialPostion.x - center.X
                distY = v.data.initialPostion.y - center.Y
                dist = stdNum.Sqrt(distX * distX + distY * distY)

                'If (dist < dist_thresh.Min) Then
                '    ejectFactor = 5
                'End If

                If dist > 0 AndAlso dist < maxRadius Then
                    dx = (distX / dist * k * k / dist) * ejectFactor * 100
                    dy = (distY / dist * k * k / dist) * ejectFactor * 100

                    mDxMap(id) = mDxMap(id) + dx
                    mDyMap(id) = mDyMap(id) + dy
                End If
            Next
        End Sub
    End Class
End Namespace
