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

        Dim colors As Color() = Designer.GetColors(mapName, mapLevels)
        Dim font As Font = CSSFont.TryParse(fontStyle).GDIObject
        Dim array As NamedValue(Of
            Dictionary(Of String, Double))() = data.ToArray
        Dim angle! = 45.0F

        If margin.IsEmpty Then
            Dim maxLabel As String = LinqAPI.DefaultFirst(Of String) <=
                From x
                In array
                Select x.Name
                Order By Name.Length Descending

            Dim sz As Size = maxLabel.MeasureString(font)

            margin = New Size(sz.Width * 1.5, sz.Width * 1.5)
        End If

        size = If(size.IsEmpty, New Size(2000, 1600), size)

        Return GraphicsPlots(
            size, margin,
            bg$,
            Sub(ByRef g, region)
                Dim dw!? = CSng((size.Height - 2 * margin.Width) / array.Length)
                Dim correl#() = array _
                    .Select(Function(x) x.Value.Values) _
                    .IteratesALL _
                    .Join(min, max) _
                    .Distinct _
                    .ToArray
                Dim lvs As Dictionary(Of Double, Integer) =
                    correl _
                    .GenerateMapping(mapLevels, offset:=0) _
                    .SeqIterator _
                    .ToDictionary(Function(x) correl(x.i),
                                  Function(x) x.value)

                Dim left! = margin.Width, top! = margin.Height
                Dim blockSize As New SizeF(dw, dw)
                Dim keys$() = array(Scan0).Value.Keys.ToArray

                If colors.IsNullOrEmpty Then
                    colors = Designer.GetColors(mapName, mapLevels)
                End If

                If valuelabelFont Is Nothing Then
                    valuelabelFont = New Font(FontFace.CambriaMath, 16, Drawing.FontStyle.Bold)
                End If

                For Each x As NamedValue(Of Dictionary(Of String, Double)) In array
                    For Each key$ In keys
                        Dim c# = x.Value(key)
                        Dim level% = lvs(c#)  '  得到等级
                        Dim color As Color = colors(
                            If(level% > colors.Length - 1,
                            colors.Length - 1,
                            level))
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim b As New SolidBrush(color)

                        Call g.FillRectangle(b, rect)

                        If drawGrid Then
                            Call g.DrawRectangles(Pens.WhiteSmoke, {rect})
                        End If
                        If drawValueLabel Then
                            key = c.FormatNumeric(2)
                            Dim ksz As SizeF = g.MeasureString(key, valuelabelFont)
                            Dim kpos As New PointF(rect.Left + (rect.Width - ksz.Width) / 2, rect.Top + (rect.Height - ksz.Height) / 2)
                            Call g.DrawString(key, valuelabelFont, Brushes.White, kpos)
                        End If

                        left += dw!
                    Next

                    left = margin.Width
                    top += dw!

                    Dim sz As SizeF = g.MeasureString(x.Name, font)
                    Dim y As Single = top - dw - (sz.Height - dw) / 2
                    Dim lx As Single =
                        margin.Width - sz.Width - margin.Width * 0.1

                    Call g.DrawString(x.Name, font, Brushes.Black, New PointF(lx, y))
                Next

                angle = -angle
                left += dw / 2

                For Each key$ In keys
                    Dim sz = g.MeasureString(key$, font) ' 得到斜边的长度
                    Dim dx! = sz.Width * Math.Cos(angle)
                    Dim dy! = sz.Width * Math.Sin(angle)
                    Call g.DrawString(key$, font, Brushes.Black, left - dx, top - dy, angle)
                    left += dw
                Next

                ' Draw legends
                Dim legend As Bitmap = colors.ColorMapLegend(
                    haveUnmapped:=False,
                    min:=Math.Round(correl.Min, 1),
                    max:=Math.Round(correl.Max, 1),
                    title:=legendTitle,
                    titleFont:=legendFont)
                Dim lsize As Size = legend.Size
                Dim lmargin As Integer = size.Width - size.Height + margin.Width

                left = size.Width - lmargin
                top = size.Height / 3

                Dim scale# = lmargin / lsize.Width
                Dim lh% = CInt(scale * (size.Height * 2 / 3))

                Call g.DrawImage(
                    legend, CInt(left), CInt(top), lmargin, lh)

                If titleFont Is Nothing Then
                    titleFont = New Font(FontFace.BookmanOldStyle, 30, Drawing.FontStyle.Bold)
                End If

                Dim titleSize = g.MeasureString(mainTitle, titleFont)
                Dim titlePosi As New PointF((left - titleSize.Width) / 2, (margin.Height - titleSize.Height) / 2)

                Call g.DrawString(mainTitle, titleFont, Brushes.Black, titlePosi)

            End Sub)
    End Function
End Module
