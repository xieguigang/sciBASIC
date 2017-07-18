Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.ConvexHull

    Public Module GrahamScan

        Private Sub keepLeft(hull As List(Of Point), r As Point)
            While hull.Count > 1 AndAlso turn(hull(hull.Count - 2), hull(hull.Count - 1), r) <> TURN__LEFT
                hull.RemoveAt(hull.Count - 1)
            End While
            If hull.Count = 0 OrElse Not hull(hull.Count - 1) = r Then
                hull.Add(r)
            End If
        End Sub

        Private Function MergeSort(p0 As Point, vector As Point()) As Point()
            If vector.Length = 1 Then
                Return vector
            End If

            Dim sorted As New List(Of Point)()
            Dim middle As Integer = CInt(vector.Length) \ 2
            Dim lefts As Point() = vector.GetRange(0, middle)
            Dim right As Point() = vector.GetRange(middle, vector.Length - middle)

            lefts = MergeSort(p0, lefts)
            right = MergeSort(p0, right)

            Dim plefts%, pright%

            For i As Integer = 0 To lefts.Length + (right.Length - 1)
                If plefts = lefts.Length Then
                    sorted.Add(right(pright))
                    pright += 1
                ElseIf pright = right.Length Then
                    sorted.Add(lefts(plefts))
                    plefts += 1
                ElseIf p0.CalculateAngle(lefts(plefts)) < p0.CalculateAngle(right(pright)) Then
                    sorted.Add(lefts(plefts))
                    plefts += 1
                Else
                    sorted.Add(right(pright))
                    pright += 1
                End If
            Next
            Return sorted.ToArray
        End Function

        Public Function ConvexHull(points As IEnumerable(Of Point)) As Point()
            Dim vector As Point() = points.ToArray
            Dim p0 As Point = Nothing

            For Each x As Point In vector
                If p0.IsEmpty Then
                    p0 = x
                Else
                    If p0.Y > x.Y Then
                        p0 = x
                    End If
                End If
            Next

            Dim order As New List(Of Point)()
            For Each x As Point In vector
                If Not p0 = x Then
                    Call order.Add(x)
                End If
            Next

            order = MergeSort(p0, order.ToArray).AsList

            Dim result As New List(Of Point)()
            result.Add(p0)
            result.Add(order(0))
            result.Add(order(1))

            order.RemoveAt(0)
            order.RemoveAt(0)

            For Each x As Point In order
                keepLeft(result, x)
            Next

            Return result
        End Function
    End Module
End Namespace
