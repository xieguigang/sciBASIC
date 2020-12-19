Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math

Namespace Layouts.ForceDirected

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