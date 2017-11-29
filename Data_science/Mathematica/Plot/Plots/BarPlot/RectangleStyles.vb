Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports defaultStyle = Microsoft.VisualBasic.Language.DefaultValue(Of Microsoft.VisualBasic.Data.ChartPlots.BarPlot.RectangleStyling)

Namespace BarPlot

    Public Enum RectangleSides As Byte
        ALL = 0
        Top
        Right
        Bottom
        Left
    End Enum

    Public Delegate Sub RectangleStyling(g As IGraphics, color As SolidBrush, layout As Rectangle, unsealSide As RectangleSides)

    Public Module RectangleStyles

        Public Function DefaultStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Call g.FillRectangle(color, layout)
                End Sub

            Return del
        End Function

        Public Function RectangleBorderStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Call g.FillRectangle(color, layout)
                    Call g.DrawRectangle(Pens.Black, layout)
                End Sub

            Return del
        End Function

        <Extension>
        Public Function UnsealTopPath(layout As Rectangle) As GraphicsPath
            Dim path As New GraphicsPath
            Dim a As New Point(layout.Left, layout.Top)
            Dim b As New Point(layout.Left, layout.Bottom)
            Dim c As New Point(layout.Right, layout.Bottom)
            Dim d As New Point(layout.Right, layout.Top)

            Call path.AddLine(a, b)
            Call path.AddLine(b, c)
            Call path.AddLine(c, d)

            Return path
        End Function

        <Extension>
        Public Function UnsealBottomPath(layout As Rectangle) As GraphicsPath
            Dim path As New GraphicsPath
            Dim a As New Point(layout.Left, layout.Bottom)
            Dim b As New Point(layout.Left, layout.Top)
            Dim c As New Point(layout.Right, layout.Top)
            Dim d As New Point(layout.Right, layout.Bottom)

            Call path.AddLine(a, b)
            Call path.AddLine(b, c)
            Call path.AddLine(c, d)

            Return path
        End Function

        <Extension>
        Public Function UnsealPath(layout As Rectangle, side As RectangleSides) As GraphicsPath
            Select Case side
                Case RectangleSides.Bottom
                    Return layout.UnsealBottomPath
                Case RectangleSides.Left
                Case RectangleSides.Right
                Case RectangleSides.Top
                    Return layout.UnsealTopPath
                Case Else

            End Select

            Throw New NotImplementedException
        End Function

        Public Function ModernStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Dim fillColor As New SolidBrush(color.Color.Alpha(200))
                    Dim path = layout.UnsealPath(unsealSide)

                    Call g.FillRectangle(fillColor, layout)
                    Call g.DrawPath(New Pen(color, 2), path)
                End Sub

            Return del
        End Function
    End Module
End Namespace