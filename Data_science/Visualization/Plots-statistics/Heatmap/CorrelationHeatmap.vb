#Region "Microsoft.VisualBasic::fcaaed37fca4f1c97d66dfc574835250, Data_science\Visualization\Plots-statistics\Heatmap\CorrelationHeatmap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Module CorrelationHeatmap
    ' 
    '         Function: Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace Heatmap

    Public Module CorrelationHeatmap

        ''' <summary>
        ''' 只能够用来表示两两变量之间的相关度
        ''' </summary>
        ''' <param name="rowLabelFontStyle">因为是三角形的矩阵，所以行和列的字体都使用相同的值了</param>
        ''' <param name="variantSize">热图之中的圆圈的半径大小是否随着相关度的值而发生改变？</param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional mapLevels% = 40,
                             Optional mapName$ = "lighter(" & ColorBrewer.DivergingSchemes.RdBu11 & ",0.05)",
                             Optional size$ = "1600,1600",
                             Optional padding$ = g.SmallPadding,
                             Optional bg$ = "white",
                             Optional logScale# = 0,
                             Optional rowDendrogramHeight% = 200,
                             Optional rowDendrogramClass As Dictionary(Of String, String) = Nothing,
                             Optional rowLabelFontStyle$ = CSSFont.Win10Normal,
                             Optional legendTitle$ = "Heatmap Color Legend",
                             Optional legendFont$ = CSSFont.Win7Large,
                             Optional legendLabelFont$ = CSSFont.PlotSubTitle,
                             Optional range As DoubleRange = Nothing,
                             Optional mainTitle$ = "heatmap",
                             Optional titleFont As Font = Nothing,
                             Optional drawGrid As Boolean = False,
                             Optional drawValueLabel As Boolean = False,
                             Optional valuelabelFontCSS$ = CSSFont.PlotLabelNormal,
                             Optional variantSize As Boolean = True,
                             Optional gridCSS$ = Stroke.HighlightStroke) As Image

            Dim margin As Padding = padding
            Dim valuelabelFont As Font = CSSFont.TryParse(valuelabelFontCSS)
            Dim array = data.ToArray
            Dim min#, max#
            Dim gridBrush As Pen = Stroke.TryParse(gridCSS).GDIObject
            Dim rowLabelFont As Font = CSSFont.TryParse(rowLabelFontStyle).GDIObject
            Dim keys$() = array(Scan0) _
                .Properties _
                .Keys _
                .ToArray
            Dim leftOffSet% = margin.Left / 1.5

            With range Or array _
                .Select(Function(x) x.Properties.Values) _
                .IteratesALL _
                .ToArray _
                .Range _
                .AsDefault

                min = .Min
                max = .Max

                range = {0, .Max}
            End With

            Dim plotInternal =
                Sub(g As IGraphics, region As GraphicsRegion, args As PlotArguments)

                    ' 在绘制上三角的时候假设每一个对象的keys的顺序都是相同的
                    Dim dw! = args.dStep.Width - gridBrush.Width * 2
                    Dim dh! = args.dStep.Height - gridBrush.Width * 2
                    Dim blockSize As New SizeF(dw, dw)  ' 每一个方格的大小是不变的
                    Dim i% = 1
                    Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                    Dim colors = args.colors
                    Dim radius As DoubleRange = {0R, dw}
                    Dim getRadius = Function(corr#) As Double
                                        If variantSize Then
                                            Return range.ScaleMapping(stdNum.Abs(corr), radius)
                                        Else
                                            Return dw
                                        End If
                                    End Function
                    Dim r!
                    Dim dr!
                    Dim left!

                    args.top += region.Padding.Top / 2

                    For Each x As SeqValue(Of DataSet) In array.SeqIterator(offset:=1)  ' 在这里绘制具体的矩阵
                        Dim levelRow As DataSet = args.levels(x.value.ID)

                        left = args.left

                        ' X为矩阵之中的行数据
                        ' 下面的循环为横向绘制出三角形的每一行的图形
                        For Each key As String In keys
                            Dim c# = (+x)(key)
                            Dim labelbrush As SolidBrush = Nothing
                            Dim gridDraw As Boolean = drawGrid
                            Dim rect As New RectangleF With {
                                .Location = New PointF(left, args.top),
                                .Size = blockSize
                            }

                            If i > x.i Then ' 上三角部分不绘制任何图形
                                gridDraw = False
                                ' 绘制标签
                                If i = x.i + 1 Then
                                    ' Call text.DrawString(key, rowLabelFont, Brushes.Black, rect.Location, angle:=-45)
                                End If
                            Else
                                Dim level% = levelRow(key)          ' 得到等级
                                Dim index% = If(
                                    level% > colors.Length - 1,
                                    colors.Length - 1,
                                    level)
                                Dim b As SolidBrush = colors(index) ' 得到当前的方格的颜色

                                If drawValueLabel Then
                                    labelbrush = Brushes.White
                                End If

                                r = getRadius(corr:=c)
                                dr = (dw - r) / 2

                                Call g.FillPie(b, rect.Left + dr, rect.Top + dr, r, r, 0, 360)
                            End If

                            If gridDraw Then
                                Call g.DrawRectangle(gridBrush, rect)
                            End If
                            If Not labelbrush Is Nothing Then

                                With c.ToString("F2")
                                    Dim ksz As SizeF = g.MeasureString(.ByRef, valuelabelFont)
                                    Dim kpos As New PointF With {
                                        .X = rect.Left + (rect.Width - ksz.Width) / 2,
                                        .Y = rect.Top + (rect.Height - ksz.Height) / 2
                                    }
                                    Call g.DrawString(.ByRef, valuelabelFont, labelbrush, kpos)
                                End With
                            End If

                            left += dw!
                            i += 1
                        Next

                        left = args.left
                        args.top += dw!
                        i = 1

                        Dim sz As SizeF = g.MeasureString((+x).ID, rowLabelFont)
                        Dim y As Single = args.top - dw - (sz.Height - dw) / 2
                        Dim lx! = args.left - sz.Width - margin.Horizontal * 0.1

                        Call g.DrawString((+x).ID, rowLabelFont, Brushes.Black, New PointF(lx, y))
                    Next

                    args.left -= dw / 1.5
                End Sub

            With margin
                .Left = array _
                    .Keys _
                    .MaxLengthString _
                    .MeasureSize(New Size(1, 1).CreateGDIDevice, rowLabelFont) _
                    .Width * 1.5
                .Bottom = 50
            End With

            Dim gSize As Size = size.SizeParser
            Dim llayout As New Size With {
                .Width = gSize.Width / 2,
                .Height = gSize.Height / 20
            }

            Return Internal.doPlot(
                plotInternal, data.ToArray,
                rowLabelFont, rowLabelFont, logScale,
                scaleMethod:=DrawElements.None, drawLabels:=DrawElements.Both, drawDendrograms:=DrawElements.None, drawClass:=(rowDendrogramClass, Nothing), dendrogramLayout:=(rowDendrogramHeight, 0),
                reverseClrSeq:=True, mapLevels:=mapLevels, mapName:=mapName,
                size:=gSize, padding:=margin, bg:=bg,
                legendTitle:=legendTitle,
                legendFont:=CSSFont.TryParse(legendFont), legendLabelFont:=CSSFont.TryParse(legendLabelFont), min:=min, max:=max,
                mainTitle:=mainTitle, titleFont:=titleFont,
                legendWidth:=120, legendSize:=llayout,
                rowXOffset:=leftOffSet) _
 _
                .AsGDIImage _
                .CorpBlank(margin.Left / 2)

        End Function
    End Module

End Namespace
