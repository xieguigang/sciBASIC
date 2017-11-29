Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports defaultStyle = Microsoft.VisualBasic.Language.DefaultValue(Of Microsoft.VisualBasic.Data.ChartPlots.BarPlot.RectangleStyling)

Namespace BarPlot

    Public Delegate Sub RectangleStyling(g As IGraphics, color As SolidBrush, layout As Rectangle)

    Public Module RectangleStyles

        Public Function DefaultStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout)
                    Call g.FillRectangle(color, layout)
                End Sub

            Return del
        End Function

        Public Function RectangleBorderStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout)
                    Call g.FillRectangle(color, layout)
                    Call g.DrawRectangle(Pens.Black, layout)
                End Sub

            Return del
        End Function

        Public Function ModernStyle() As defaultStyle
            Dim del As RectangleStyling =
                Sub(g, color, layout)
                    Dim fillColor As New SolidBrush(color.Color.Alpha(245))

                    Call g.FillRectangle(fillColor, layout)
                    Call g.DrawRectangle(New Pen(color, 2), layout)
                End Sub

            Return del
        End Function
    End Module
End Namespace