Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class Halfedge

#Region "Pool"
        Private Shared pool As Queue(Of Halfedge) = New Queue(Of Halfedge)()

        Public Shared Function Create(edge As Edge, lr As LR) As Halfedge
            If pool.Count > 0 Then
                Return pool.Dequeue().Init(edge, lr)
            Else
                Return New Halfedge(edge, lr)
            End If
        End Function
        Public Shared Function CreateDummy() As Halfedge
            Return Create(Nothing, Nothing)
        End Function
#End Region

#Region "Object"
        Public edgeListLeftNeighbor As Halfedge
        Public edgeListRightNeighbor As Halfedge
        Public nextInPriorityQueue As Halfedge

        Public edge As Edge
        Public leftRight As LR
        Public vertex As Vertex

        ' The vertex's y-coordinate in the transformed Voronoi space V
        Public ystar As Single

        Public Sub New(edge As Edge, lr As LR)
            Init(edge, lr)
        End Sub

        Private Function Init(edge As Edge, lr As LR) As Halfedge
            Me.edge = edge
            leftRight = lr
            nextInPriorityQueue = Nothing
            vertex = Nothing

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "Halfedge (LeftRight: " & leftRight.ToString() & "; vertex: " & vertex.ToString() & ")"
        End Function

        Public Sub Dispose()
            If edgeListLeftNeighbor IsNot Nothing OrElse edgeListRightNeighbor IsNot Nothing Then
                ' still in EdgeList
                Return
            End If
            If nextInPriorityQueue IsNot Nothing Then
                ' still in PriorityQueue
                Return
            End If
            edge = Nothing
            leftRight = Nothing
            vertex = Nothing
            pool.Enqueue(Me)
        End Sub

        Public Sub ReallyDispose()
            edgeListLeftNeighbor = Nothing
            edgeListRightNeighbor = Nothing
            nextInPriorityQueue = Nothing
            edge = Nothing
            leftRight = Nothing
            vertex = Nothing
            pool.Enqueue(Me)
        End Sub

        Public Function IsLeftOf(p As Vector2D) As Boolean
            Dim topSite As Site
            Dim rightOfSite, above, fast As Boolean
            Dim dxp, dyp, dxs, t1, t2, t3, y1 As Single

            topSite = edge.RightSite
            rightOfSite = p.x > topSite.x
            If rightOfSite AndAlso leftRight Is LR.LEFT Then
                Return True
            End If
            If Not rightOfSite AndAlso leftRight Is LR.RIGHT Then
                Return False
            End If

            If edge.a = 1 Then
                dyp = p.y - topSite.y
                dxp = p.x - topSite.x
                fast = False
                If Not rightOfSite AndAlso edge.b < 0 OrElse rightOfSite AndAlso edge.b >= 0 Then
                    above = dyp >= edge.b * dxp
                    fast = above
                Else
                    above = p.x + p.y * edge.b > edge.c
                    If edge.b < 0 Then
                        above = Not above
                    End If
                    If Not above Then
                        fast = True
                    End If
                End If
                If Not fast Then
                    dxs = topSite.x - edge.LeftSite.x
                    above = edge.b * (dxp * dxp - dyp * dyp) < dxs * dyp * (1 + 2 * dxp / dxs + edge.b * edge.b)
                    If edge.b < 0 Then
                        above = Not above
                    End If
                End If
            Else
                y1 = edge.c - edge.a * p.x
                t1 = p.y - y1
                t2 = p.x - topSite.x
                t3 = y1 - topSite.y
                above = t1 * t1 > t2 * t2 + t3 * t3
            End If
            Return If(leftRight Is LR.LEFT, above, Not above)
        End Function
#End Region
    End Class
End Namespace
