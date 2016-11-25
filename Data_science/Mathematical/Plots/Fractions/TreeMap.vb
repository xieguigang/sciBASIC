Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.g
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module TreeMap

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of Fractions),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white") As Bitmap

        Dim array As List(Of Fractions) =
            data _
            .OrderByDescending(Function(x) x.Percentage) _
            .ToList

        Return GraphicsPlots(
            size, margin,
            bg,
            Sub(ByRef g, region)

                Dim rect As New Rectangle(
                    New Point(margin.Width, margin.Height),
                    region.PlotRegion.Size)

                Dim f As Boolean = True ' true -> width percentage; false -> height percentage
                Dim width! = rect.Width, height! = rect.Height
                Dim x! = margin.Width, y! = margin.Height
                Dim drawW!, drawH!

                For Each p As Fractions In array
                    If f Then  ' 计算宽度百分比
                        drawW = p.Percentage * width
                        drawH = height

                        Call g.FillRectangle(
                            New SolidBrush(p.Color),
                            New RectangleF(New PointF(x, y), New SizeF(drawW, drawH)))

                        x = x + drawW
                        width = width - drawW
                    Else ' 计算高度百分比
                        drawW = width
                        drawH = p.Percentage * height

                        Call g.FillRectangle(
                           New SolidBrush(p.Color),
                           New RectangleF(New PointF(x, y), New SizeF(drawW, drawH)))

                        y += drawH
                        height = height - drawH
                    End If

                    f = Not f  ' swap
                Next
            End Sub)
    End Function
End Module
