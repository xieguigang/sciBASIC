#Region "Microsoft.VisualBasic::8cfb43e8b583f76c0cf36b72f508e908, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\Heatmaps\HeatmapTable.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Heatmap

    Public Module HeatmapTable

        ''' <summary>
        ''' 只能够用来表示两两变量之间的相关度
        ''' </summary>
        ''' <param name="fontStyle">对象标签的字体</param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional mapLevels% = 20,
                             Optional mapName$ = ColorMap.PatternJet,
                             Optional size$ = "1600,1600",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional rowDendrogramHeight% = 200,
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
                Sub(g As IGraphics, region As GraphicsRegion, args As PlotArguments)

                    ' 在绘制上三角的时候假设每一个对象的keys的顺序都是相同的
                    Dim dw! = args.dStep.Width, dh! = args.dStep.Height
                    Dim keys$() = array(Scan0).Properties.Keys.ToArray
                    Dim blockSize As New SizeF(dw, dw)  ' 每一个方格的大小
                    Dim i% = 1
                    Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                    Dim levels = args.levels
                    Dim colors = args.colors

                    For Each x As SeqValue(Of DataSet) In array.SeqIterator(offset:=1)  ' 在这里绘制具体的矩阵

                        ' X为矩阵之中的行数据
                        ' 下面的循环为横向绘制出三角形的每一行的图形
                        For Each key As String In keys
                            Dim c# = (+x)(key)
                            Dim rect As New RectangleF(New PointF(args.left, args.top), blockSize)
                            Dim labelbrush As SolidBrush = Nothing
                            Dim gridDraw As Boolean = drawGrid

                            If i > x.i Then ' 上三角部分不绘制任何图形
                                gridDraw = False
                                ' 绘制标签
                                If i = x.i + 1 Then
                                    Call text.DrawString(key, font, Brushes.Black, rect.Location, angle:=-45)
                                End If
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

                            args.left += dw!
                            i += 1
                        Next

                        args.left = margin.Left
                        args.top += dw!
                        i = 1

                        Dim sz As SizeF = g.MeasureString((+x).ID, font)
                        Dim y As Single = args.top - dw - (sz.Height - dw) / 2
                        Dim lx! = margin.Left - sz.Width - margin.Horizontal * 0.1

                        Call g.DrawString((+x).ID, font, Brushes.Black, New PointF(lx, y))
                    Next
                End Sub

            If range Is Nothing Then
                range = New DoubleRange(
                    array _
                    .Select(Function(x) x.Properties.Values) _
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

            Return __plotInterval(
                plotInternal, data.ToArray,
                font, DrawElements.None, DrawElements.Rows, DrawElements.Rows, (rowDendrogramHeight, 0),,
                mapLevels, mapName,
                gsize, margin, bg,
                legendTitle,
                CSSFont.TryParse(legendFont), CSSFont.TryParse(legendLabelFont), min, max,
                mainTitle, titleFont,
                120, legendLayout:=llayout)
        End Function
    End Module

End Namespace