#Region "Microsoft.VisualBasic::45ac9a33c947aee64ce856d2139c6b56, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\Anchor.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Anchor
    ' 
    '         Properties: r, x, y
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports sys = System.Math

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

        Sub New()
        End Sub

        Sub New(location As Point, r#)
            Me.r = r

            x = location.X
            y = location.Y
        End Sub

        ''' <summary>
        ''' 目标节点的绘图模型
        ''' </summary>
        ''' <param name="circle">假设anchor是一个圆，画圆的时候是依据矩形框来建模的</param>
        Sub New(circle As Rectangle)
            r = sys.Min(circle.Width, circle.Height) / 2
            x = circle.Left + r
            y = circle.Top + r
        End Sub

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
