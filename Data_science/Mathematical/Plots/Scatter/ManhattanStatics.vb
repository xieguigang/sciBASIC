Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.SlideWindow
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Mathematical.Plots
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module ManhattanStatics

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <param name="fill$">正负误差之间的填充颜色</param>
    ''' <returns></returns>
    Public Function Plot(s As SerialData,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white",
                         Optional fill$ = Nothing) As Bitmap
        Dim fillColor As Color = If(
            String.IsNullOrEmpty(fill) OrElse fill = "none",
            Nothing,
            fill.ToColor)

        Return GraphicsPlots(
            size, margin, bg,
            Sub(ByRef g, grect)
                Dim serrPlus As New SerialData With {
                    .color = s.color,
                    .lineType = DashStyle.Dash,
                    .PointSize = s.PointSize,
                    .width = s.width,
                    .pts = s.pts _
                        .ToArray(Function(err) New PointData With {
                            .pt = New PointF(err.pt.X, err.errPlus)
                        })
                }
                Dim serrMinus As New SerialData With {
                    .color = s.color,
                    .lineType = DashStyle.Dash,
                    .PointSize = s.PointSize,
                    .width = s.width,
                    .pts = s.pts _
                        .ToArray(Function(err) New PointData With {
                            .pt = New PointF(err.pt.X, err.errMinus)
                        })
                }
                Dim mapper As New Scaling({serrPlus, s, serrMinus})

                For Each line As SerialData In mapper.ForEach(size, margin)
                    Dim pts = line.pts.SlideWindows(2)
                    Dim pen As New Pen(color:=line.color, width:=line.width) With {
                        .DashStyle = line.lineType
                    }
                    Dim br As New SolidBrush(line.color)
                    Dim d = line.PointSize
                    Dim r As Single = line.PointSize / 2
                    Dim bottom! = size.Height - margin.Height

                    For Each pt In pts
                        Dim a As PointData = pt.First
                        Dim b As PointData = pt.Last

                        Call g.DrawLine(pen, a.pt, b.pt)
                    Next
                Next
            End Sub)
    End Function
End Module
