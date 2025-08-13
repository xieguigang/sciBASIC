Imports System.Collections.Generic

Namespace Drawing2D.Math2D.DelaunayVoronoi

    Public Class LineSegment

        Public Shared Function VisibleLineSegments(edges As List(Of Edge)) As List(Of LineSegment)
            Dim segments As List(Of LineSegment) = New List(Of LineSegment)()

            For Each edge In edges
                If edge.Visible() Then
                    Dim p1 = edge.ClippedEnds(LR.LEFT)
                    Dim p2 = edge.ClippedEnds(LR.RIGHT)
                    segments.Add(New LineSegment(p1, p2))
                End If
            Next

            Return segments
        End Function

        Public Shared Function CompareLengths_MAX(segment0 As LineSegment, segment1 As LineSegment) As Single
            Dim length0 = (segment0.p0 - segment0.p1).Length
            Dim length1 = (segment1.p0 - segment1.p1).Length
            If length0 < length1 Then
                Return 1
            End If
            If length0 > length1 Then
                Return -1
            End If
            Return 0
        End Function

        Public Shared Function CompareLengths(edge0 As LineSegment, edge1 As LineSegment) As Single
            Return -CompareLengths_MAX(edge0, edge1)
        End Function

        Public p0 As Vector2
        Public p1 As Vector2

        Public Sub New(p0 As Vector2, p1 As Vector2)
            Me.p0 = p0
            Me.p1 = p1
        End Sub
    End Class
End Namespace
