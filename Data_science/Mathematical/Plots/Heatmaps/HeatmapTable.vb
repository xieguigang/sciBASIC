#Region "Microsoft.VisualBasic::49988d2e7b07844d474b04de6f93ec57, ..\sciBASIC#\Data_science\Mathematical\Plots\Heatmaps\HeatmapTable.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting

Public Module HeatmapTable

    ''' <summary>
    ''' 只能够用来表示两两变量之间的相关度
    ''' </summary>
    ''' <param name="triangularStyle">
    ''' 是否下三角部分显示圆，默认是本三角样式
    ''' 圆的半径大小用来表示相关度的绝对值，颜色则是和普通的heatmap一样用来表示相关度的大小和方向
    ''' </param>
    ''' <param name="fontStyle">对象标签的字体</param>
    ''' <returns></returns>
    Public Function Plot(data As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double))),
                         Optional mapLevels% = 20,
                         Optional mapName$ = ColorMap.PatternJet,
                         Optional size$ = "1600,1600",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional triangularStyle As Boolean = True,
                         Optional fontStyle$ = CSSFont.Win10Normal,
                         Optional legendTitle$ = "Heatmap Color Legend",
                         Optional legendFont$ = CSSFont.Win7Large,
                         Optional legendLabelFont$ = CSSFont.PlotSubTitle,
                         Optional range As DoubleRange = Nothing,
                         Optional mainTitle$ = "heatmap",
                         Optional titleFont As Font = Nothing,
                         Optional drawGrid As Boolean = False,
                         Optional gridColor$ = NameOf(Color.Gray),
                         Optional drawValueLabel As Boolean = False,
                         Optional valuelabelFontCSS$ = CSSFont.PlotLabelNormal) As GraphicsData

        Dim margin As Padding = padding
        Dim valuelabelFont As Font = CSSFont.TryParse(valuelabelFontCSS)
        Dim array = data.ToArray
        Dim min#, max#
        Dim gridBrush As New Pen(gridColor.TranslateColor, 2)
        Dim font As Font = CSSFont.TryParse(fontStyle).GDIObject
        Dim plotInternal =
            Sub(g As IGraphics, region As GraphicsRegion,
                left As Value(Of Single),
                dw As Single,
                levels As Dictionary(Of Double, Integer),
                top As Value(Of Single),
                colors As Color())

                ' 在绘制上三角的时候假设每一个对象的keys的顺序都是相同的
                Dim keys$() = array(Scan0).Value.Keys.ToArray
                Dim blockSize As New SizeF(dw, dw)  ' 每一个方格的大小
                Dim i% = 1

                For Each x As SeqValue(Of NamedValue(Of Dictionary(Of String, Double))) In array.SeqIterator(offset:=1)  ' 在这里绘制具体的矩阵
                    ' X为矩阵之中的行数据
                    ' 下面的循环为横向绘制出三角形的每一行的图形
                    For Each key As String In keys
                        Dim c# = (+x).Value(key)
                        Dim rect As New RectangleF(New PointF(left, top), blockSize)
                        Dim labelbrush As SolidBrush = Nothing
                        Dim gridDraw As Boolean = drawGrid

                        If triangularStyle AndAlso i > x.i Then ' 上三角部分不绘制任何图形
                            gridDraw = False
                        Else
                            Dim level% = levels(c#)  '  得到等级
                            Dim color As Color = colors(   ' 得到当前的方格的颜色
                                If(level% > colors.Length - 1,
                                colors.Length - 1,
                                level))
                            Dim b As New SolidBrush(color)

                            If drawValueLabel Then
                                labelbrush = Brushes.White
                            End If

                            Call g.FillPie(b, rect.Left, rect.Top, dw, dw, 0, 360)
                        End If

                        If gridDraw Then
                            Call g.DrawRectangle(gridBrush, rect)
                        End If
                        If Not labelbrush Is Nothing Then
                            key = c.FormatNumeric(2)
                            Dim ksz As SizeF = g.MeasureString(key, valuelabelFont)
                            Dim kpos As New PointF With {
                                .X = rect.Left + (rect.Width - ksz.Width) / 2,
                                .Y = rect.Top + (rect.Height - ksz.Height) / 2
                            }
                            Call g.DrawString(key, valuelabelFont, labelbrush, kpos)
                        End If

                        left.value += dw!
                        i += 1
                    Next

                    left.value = margin.Left
                    top.value += dw!
                    i = 1

                    Dim sz As SizeF = g.MeasureString((+x).Name, font)
                    Dim y As Single = top.value - dw - (sz.Height - dw) / 2
                    Dim lx! = margin.Left - sz.Width - margin.Horizontal * 0.1

                    Call g.DrawString((+x).Name, font, Brushes.Black, New PointF(lx, y))
                Next

                If triangularStyle Then
                    Dim maxSize = g.MeasureString(keys.MaxLengthString, font)
                    Dim y! = 0

                    ' |\
                    ' ----
                    dw = Math.Sqrt(dw ^ 2 + dw ^ 2)

                    Using g2 As Graphics2D = New Size With {
                        .Width = maxSize.Width,
                        .Height = (maxSize.Height + dw!) * keys.Length
                    }.CreateGDIDevice(Color.Transparent)

                        For Each key As String In keys
                            Call g2.DrawString(key, font, Brushes.Black, New PointF(0, y))
                            y += dw!
                        Next

                        Dim labels As Image = g2.ImageResource.RotateImage(-45)
                        Dim offset! = Math.Sqrt(maxSize.Width ^ 2 / 2)

                        Call g.DrawImageUnscaled(
                            labels,
                            New Point(margin.Left + offset / 2, margin.Top - offset - maxSize.Height))
#Const DEBUG = False
#If DEBUG Then
                        Call g2.ImageResource.SaveAs("./labels.png")
                        Call labels.SaveAs("./labels-r45_degrees.png")
#End If
                    End Using
                End If
            End Sub

        If range Is Nothing Then
            range = New DoubleRange(
                array _
                .Select(Function(x) x.Value.Values) _
                .IteratesALL _
                .ToArray)
        End If

        With range
            min = .Min
            max = .Max
        End With
        With margin
            .Left = array _
                .Keys _
                .MaxLengthString _
                .MeasureString(font) _
                .Width * 1.5
            .Bottom = 50
        End With
        Dim gsize As Size = size.SizeParser
        Dim llayout As New Rectangle With {
            .Size = New Size(gsize.Width / 3, gsize.Height / 3),
            .Location = New Point(gsize.Width - .Size.Width - 50, margin.Top)
        }

        Return Heatmap.__plotInterval(
            plotInternal, data.ToArray,
            font, Not triangularStyle,,
            mapLevels, mapName,
            gsize, margin, bg,
            legendTitle,
            CSSFont.TryParse(legendFont), CSSFont.TryParse(legendLabelFont), min, max,
            mainTitle, titleFont,
            120, legendLayout:=llayout)
    End Function
End Module
