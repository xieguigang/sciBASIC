Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Module Bubble

    ''' <summary>
    ''' <see cref="PointData.value"/>是Bubble的半径大小
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="legend"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of SerialData),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional legend As Boolean = True,
                         Optional logR As Boolean = False,
                         Optional legendBorder As Border = Nothing) As Bitmap

        Return GraphicsPlots(
            size, margin, bg,
            Sub(g, grect)
                Dim array As SerialData() = data.ToArray
                Dim mapper As New Scaling(array)
                Dim scale As Func(Of Double, Double) =
                    If(logR,
                    Function(r) Math.Log(r + 1) + 1,
                    Function(r) r)

                Call g.DrawAxis(size, margin, mapper, True)

                For Each s As SerialData In mapper.ForEach(size, margin)
                    Dim b As New SolidBrush(s.color)

                    For Each pt As PointData In s
                        Dim r As Double = scale(pt.value)
                        Dim p As New Point(pt.pt.X - r, pt.pt.Y - r)
                        Dim rect As New Rectangle(p, New Size(r * 2, r * 2))

                        Call g.FillPie(b, rect, 0, 360)
                    Next
                Next

                If legend Then

                    Dim topLeft As New Point(size.Width * 0.8, margin.Height)
                    Dim legends = LinqAPI.Exec(Of Legend) <=
 _
                        From x As SerialData
                        In array
                        Select New Legend With {
                            .color = x.color.RGBExpression,
                            .fontstyle = CSSFont.GetFontStyle(FontFace.MicrosoftYaHei, FontStyle.Regular, 20),
                            .style = LegendStyles.Circle,
                            .title = x.title
                        }

                    Call g.DrawLegends(topLeft, legends,,, legendBorder)
                End If
            End Sub)
    End Function
End Module
