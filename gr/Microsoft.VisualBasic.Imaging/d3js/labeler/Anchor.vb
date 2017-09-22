Imports System.Drawing

Namespace d3js.Layout

    Public Class Anchor

        ''' <summary>
        ''' the x-coordinate of the anchor.
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Double
        ''' <summary>
        ''' the y-coordinate of the anchor.
        ''' </summary>
        ''' <returns></returns>
        Public Property y As Double
        ''' <summary>
        ''' the anchor radius (assuming anchor is a circle).
        ''' </summary>
        ''' <returns></returns>
        Public Property r As Double

        Public Shared Widening Operator CType(anchor As Anchor) As Point
            With anchor
                Return New Point(.x, .y)
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As PointF
            With anchor
                Return New PointF(.x, .y)
            End With
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As RectangleF
            Dim r# = anchor.r

            Return New RectangleF With {
                .Location = anchor,
                .Size = New SizeF(r, r)
            }
        End Operator

        Public Shared Widening Operator CType(anchor As Anchor) As Rectangle
            With CType(anchor, RectangleF)
                Return New Rectangle(.Location.ToPoint, .Size.ToSize)
            End With
        End Operator
    End Class
End Namespace
