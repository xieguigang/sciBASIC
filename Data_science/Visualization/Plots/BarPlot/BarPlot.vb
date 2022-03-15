#Region "Microsoft.VisualBasic::5ac004eb29a580d334f31e3775433922, sciBASIC#\Data_science\Visualization\Plots\BarPlot\BarPlot.vb"

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


    ' Code Statistics:

    '   Total Lines: 254
    '    Code Lines: 178
    ' Comment Lines: 42
    '   Blank Lines: 34
    '     File Size: 10.70 KB


    '     Module BarPlotAPI
    ' 
    '         Function: FromData, Plot, Rectangle
    ' 
    '         Sub: plotImpl
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports stdNum = System.Math

Namespace BarPlot

    ''' <summary>
    ''' 这个不像<see cref="Histogram"/>用于描述若干组连续的数据，这个是将数据按照标签分组来表述出来的
    ''' </summary>
    <HideModuleName> Public Module BarPlotAPI

        ''' <summary>
        ''' Bar data plot
        ''' </summary>
        ''' <param name="data">Data groups</param>
        ''' <param name="size">image output size</param>
        ''' <param name="padding">margin blank of the plots region</param>
        ''' <param name="bg$">Background color expression</param>
        ''' <param name="showGrid">Show axis grid?</param>
        ''' <param name="stacked">Bar plot is stacked of each sample?</param>
        ''' <param name="stackReordered">reorder bar data? Only works in stacked mode</param>
        ''' <param name="showLegend">Show data legend?</param>
        ''' <param name="legendPos">Position of the data legend on the image</param>
        ''' <param name="legendBorder">legend shape border style.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As BarDataGroup,
                             Optional size As Size = Nothing,
                             Optional padding$ = "padding: 300 120 300 120;",
                             Optional bg$ = "white",
                             Optional showGrid As Boolean = True,
                             Optional stacked As Boolean = False,
                             Optional stackReordered? As Boolean = True,
                             Optional showLegend As Boolean = True,
                             Optional legendPos As Point = Nothing,
                             Optional legendBorder As Stroke = Nothing,
                             Optional legendFont As Font = Nothing) As GraphicsData

            Dim margin As Padding = padding

            Return GraphicsPlots(
                size, margin,
                bg,
                Sub(ByRef g, grect) Call plotImpl(
                    g, grect,
                    data,
                    bg,
                    showGrid, stacked, stackReordered,
                    showLegend, legendPos, legendBorder, legendFont))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">gdi+</param>
        ''' <param name="grect"></param>
        ''' <param name="data"></param>
        ''' <param name="bg$"></param>
        ''' <param name="showGrid"></param>
        ''' <param name="stacked"></param>
        ''' <param name="stackReorder"></param>
        ''' <param name="showLegend"></param>
        ''' <param name="legendPos"></param>
        ''' <param name="legendBorder"></param>
        Private Sub plotImpl(ByRef g As IGraphics, grect As GraphicsRegion,
                             data As BarDataGroup,
                             bg$,
                             showGrid As Boolean,
                             stacked As Boolean,
                             stackReorder As Boolean,
                             showLegend As Boolean,
                             legendPos As Point,
                             legendBorder As Stroke,
                             legendFont As Font)

            Dim scaler As New Scaling(data, stacked, False)
            Dim mapper As New Mapper(scaler)
            Dim n As Integer

            If stacked Then
                n = data.Samples.Length
            Else
                n = data.Samples.Sum(Function(x) x.data.Length) - 1
            End If

            Dim dxStep As Double = (grect.Size.Width - grect.Padding.Horizontal - 2 * grect.Padding.Horizontal) / n
            Dim interval As Double = grect.Padding.Horizontal / n
            Dim left As Single = grect.Padding.Left
            Dim sy As Func(Of Single, Single) = mapper.YScaler(grect.Size, grect.Padding)
            Dim bottom = grect.Size.Height - grect.Padding.Bottom
            Dim angle! = -45
            Dim leftMargins As New List(Of Double)

            ' Call g.DrawAxis(grect.Size, grect.Padding, mapper, showGrid)

            For Each sample As SeqValue(Of BarDataSample) In data.Samples.SeqIterator
                Dim x = left + interval

                leftMargins += x

                If stacked Then
                    ' 改变Y
                    Dim right = x + dxStep
                    Dim top = sy(sample.value.StackedSum)
                    ' 畫布的高度
                    Dim canvasHeight = grect.Size.Height - (grect.Padding.Vertical)
                    ' 底部減去最高的就是實際的高度（縂的）
                    Dim actualHeight = bottom - top
                    Dim barWidth = dxStep

                    Dim stack As IEnumerable(Of SeqValue(Of Double))

                    If stackReorder Then
                        stack = sample.value _
                            .data _
                            .SeqIterator _
                            .OrderBy(Function(o) o.value)
                    Else
                        stack = sample.value _
                            .data _
                            .SeqIterator
                    End If

                    For Each val As SeqValue(Of Double) In stack
                        Dim topleft As New Point(x, top)
                        ' 百分比
                        Dim barHeight! = (+val) / (+sample).StackedSum
                        barHeight = barHeight * actualHeight
                        Dim barSize As New Size(barWidth, barHeight)
                        Dim rect As New Rectangle(topleft, barSize)

                        Call g.FillRectangle(New SolidBrush(data.Serials(val.i).Value), rect)

                        top += barHeight
                    Next

                    x += dxStep
                Else
                    ' 改变X
                    For Each val As SeqValue(Of Double) In sample.value.data.SeqIterator
                        Dim right = x + dxStep
                        Dim top = sy(val.value)
                        Dim rect As Rectangle = Rectangle(top, x, right, grect.Size.Height - grect.Padding.Bottom)

                        Call g.DrawRectangle(Pens.Black, rect)
                        Call g.FillRectangle(
                            New SolidBrush(data.Serials(val.i).Value),
                            Rectangle(top + 1,
                                      x + 1,
                                      right - 1,
                                      grect.Size.Height - grect.Padding.Bottom - 1))
                        x += dxStep
                    Next
                End If

                left = x
            Next

            Dim keys$() = data.Samples _
                .Select(Function(s) s.Tag) _
                .ToArray
            Dim font As New Font(FontFace.SegoeUI, 28)
            Dim dd = leftMargins(1) - leftMargins(0)

            bottom += 80

            For Each key As SeqValue(Of String) In keys.SeqIterator
                left = leftMargins(index:=key.i) + dd / 2 - If(Not stacked, dxStep / 2, 0)

                ' 得到斜边的长度
                Dim sz = g.MeasureString((+key), font)
                Dim dx! = sz.Width * stdNum.Cos(angle)
                Dim dy! = sz.Width * stdNum.Sin(angle)

                Call g.DrawString(key, font, Brushes.Black, left - dx, bottom, angle)
            Next

            If showLegend Then
                If legendFont Is Nothing Then
                    legendFont = New Font(FontFace.SegoeUI, 30, FontStyle.Regular)
                End If

                Dim cssStyle As String = CSSFont.GetFontStyle(legendFont)
                Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) <=
 _
                From x As NamedValue(Of Color)
                In data.Serials
                Select New LegendObject With {
                    .color = x.Value.RGBExpression,
                    .fontstyle = cssStyle,
                    .style = LegendStyles.Circle,
                    .title = x.Name
                }

                If legendPos.IsEmpty Then
                    Dim Y% = grect.Padding.Bottom / legends.Length
                    Dim X%
                    Dim gr As IGraphics = g
                    Dim maxW As Single = legends.Max(
                    Function(l) gr _
                        .MeasureString(l.title, legendFont) _
                        .Width)

                    X = grect.Size.Width - maxW - 145

                    legendPos = New Point(X, Y)
                End If

                Call g.DrawLegends(legendPos, legends,,, shapeBorder:=legendBorder)
            End If
        End Sub

        Public Function Rectangle(top As Single, left As Single, right As Single, bottom As Single) As Rectangle
            Dim pt As New Point(left, top)
            Dim size As New Size(right - left, bottom - top)
            Return New Rectangle(pt, size)
        End Function

        ''' <summary>
        ''' Creates sample groups from a data vector
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FromData(data As IEnumerable(Of Double)) As BarDataGroup
            Return New BarDataGroup With {
                .Serials = {
                    New NamedValue(Of Color) With {
                        .Name = "",
                        .Value = Color.Lime
                    }
                },
                .Samples = LinqAPI.Exec(Of BarDataSample) <=
                    From n
                    In data.SeqIterator
                    Select New BarDataSample With {
                        .data = {n.value},
                        .Tag = n.i
                    }
            }
        End Function
    End Module
End Namespace
