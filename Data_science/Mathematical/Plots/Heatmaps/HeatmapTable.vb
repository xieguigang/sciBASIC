Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Mathematical

Public Module HeatmapTable

    ''' <summary>
    ''' 只能够用来表示两两变量之间的相关度
    ''' </summary>
    ''' <param name="triangularStyle">
    ''' 是否上部分显示圆，下部分显示数值？默认是全部都显示圆圈
    ''' 圆的半径大小用来表示相关度的绝对值，颜色则是和普通的heatmap一样用来表示相关度的大小和方向
    ''' </param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional mapLevels% = 100,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white",
                         Optional triangularStyle As Boolean = False,
                         Optional fontStyle$ = CSSFont.Win10Normal,
                         Optional legendTitle$ = "Heatmap Color Legend",
                         Optional legendFont As Font = Nothing,
                         Optional min# = -1,
                         Optional max# = 1,
                         Optional mainTitle$ = "heatmap",
                         Optional titleFont As Font = Nothing,
                         Optional drawGrid As Boolean = True,
                         Optional drawValueLabel As Boolean = True,
                         Optional valuelabelFont As Font = Nothing) As Bitmap

        Return Heatmap.__plotInterval(
            Sub(g, region, array, left, font, dw, levels, top, colors)

                Dim keys$() = array(Scan0).Value.Keys.ToArray
                Dim blockSize As New SizeF(dw, dw)  ' 每一个方格的大小

                If valuelabelFont Is Nothing Then
                    valuelabelFont = New Font(FontFace.CambriaMath, 16, Drawing.FontStyle.Bold)
                End If

                For Each x As NamedValue(Of Dictionary(Of String, Double)) In array   ' 在这里绘制具体的矩阵
                    For Each key$ In keys
                        Dim c# = x.Value(key)
                        Dim level% = levels(c#)  '  得到等级
                        Dim color As Color = colors(   ' 得到当前的方格的颜色
                            If(level% > colors.Length - 1,
                            colors.Length - 1,
                            level))
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim b As New SolidBrush(color)
                        Dim r As Single = Math.Abs(c) * dw / 2 ' 计算出半径的大小
                        Dim d = dw / 2 - r

                        Call g.FillPie(b, rect.Left + d, rect.Top + d, r, r, 0, 360)

                        If drawGrid Then
                            Call g.DrawRectangles(Pens.WhiteSmoke, {rect})
                        End If
                        If drawValueLabel Then
                            key = c.FormatNumeric(2)
                            Dim ksz As SizeF = g.MeasureString(key, valuelabelFont)
                            Dim kpos As New PointF(rect.Left + (rect.Width - ksz.Width) / 2, rect.Top + (rect.Height - ksz.Height) / 2)
                            Call g.DrawString(key, valuelabelFont, Brushes.White, kpos)
                        End If

                        left.value += dw!
                    Next

                    left.value = margin.Width
                    top.value += dw!

                    Dim sz As SizeF = g.MeasureString(x.Name, font)
                    Dim y As Single = top.value - dw - (sz.Height - dw) / 2
                    Dim lx! = margin.Width - sz.Width - margin.Width * 0.1

                    Call g.DrawString(x.Name, font, Brushes.Black, New PointF(lx, y))
                Next

            End Sub,
            data.ToArray,,
            mapLevels, mapName, size, margin, bg, fontStyle, legendTitle, legendFont, min, max, mainTitle, titleFont)
    End Function
End Module
