Imports System.Drawing
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

        Public Function ModernStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout, unsealSide)
                    Dim fillColor As New SolidBrush(color.Color.Alpha(180))

                    Call g.FillRectangle(fillColor, layout)
                    Call g.DrawRectangle(New Pen(color, 2), layout)
                End Sub

            Return del
        End Function
    End Module
End Namespace