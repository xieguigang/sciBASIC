#Region "Microsoft.VisualBasic::b5f2255d65178b56f826438fa494eb1b, Data_science\Visualization\Plots\BarPlot\BarPlot2.vb"

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

    '   Total Lines: 118
    '    Code Lines: 83 (70.34%)
    ' Comment Lines: 16 (13.56%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 19 (16.10%)
    '     File Size: 5.34 KB


    '     Module BarPlotAPI
    ' 
    '         Function: Plot2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

Namespace BarPlot

    Partial Module BarPlotAPI

        ''' <summary>
        ''' Plot bar plot in different direction compare with <see cref="Plot"/>
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="size"></param>
        ''' <param name="padding"></param>
        ''' <param name="bg$"></param>
        ''' <param name="showGrid"></param>
        ''' <param name="stacked"></param>
        ''' <param name="showLegend"></param>
        ''' <param name="legendPos"></param>
        ''' <param name="legendBorder"></param>
        ''' <returns></returns>
        Public Function Plot2(data As BarDataGroup,
                              Optional size As Size = Nothing,
                              Optional padding$ = DefaultPadding,
                              Optional bg$ = "white",
                              Optional showGrid As Boolean = True,
                              Optional stacked As Boolean = False,
                              Optional showLegend As Boolean = True,
                              Optional legendPos As Point = Nothing,
                              Optional legendBorder As Stroke = Nothing) As GraphicsData

            Dim margin As Padding = padding
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)

                    Dim lefts! = region.PlotRegion.Left
                    Dim top! = region.PlotRegion.Top
                    Dim mapper As New Mapper(New Scaling(data, stacked, True))
                    Dim n As Integer = If(
                        stacked,
                        data.Samples.Length,
                        data.Samples.Sum(Function(x) x.data.Length))
                    Dim dy As Double = (size.Height - margin.Vertical - margin.Vertical) / n
                    Dim interval As Double = margin.Vertical / n
                    Dim sx As Func(Of Single, Single) = mapper.XScaler(size, margin)

                    ' Call g.DrawAxis(size, margin, mapper, showGrid)

                    For Each sample As SeqValue(Of BarDataSample) In data.Samples.SeqIterator
                        Dim y = top + interval

                        If stacked Then
                            ' 改变Y
                            Dim bottom! = y + dy
                            Dim right = sx(sample.value.StackedSum)
                            Dim canvasWidth = size.Height - (margin.Vertical)

                            For Each val As SeqValue(Of Double) In sample.value.data.SeqIterator
                                Dim rect As Rectangle = Rectangle(y, lefts, right, bottom)

                                Call g.FillRectangle(New SolidBrush(data.Serials(val.i).Value), rect)

                                top += ((val.value - mapper.xmin) / mapper.dx) * canvasWidth
                            Next

                            top += dy
                        Else
                            ' 改变X
                            For Each val As SeqValue(Of Double) In sample.value.data.SeqIterator
                                Dim bottom! = y
                                Dim right = sx(val.value)
                                Dim rect As Rectangle = Rectangle(bottom, lefts, right, bottom + dy)

                                Call g.FillRectangle(New SolidBrush(data.Serials(val.i).Value), rect)
                                Call g.DrawRectangle(Pens.Black, rect)

                                y += dy
                            Next
                        End If

                        top = y
                    Next

                    If showLegend Then
                        Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) <=
 _
                        From x As NamedValue(Of Color)
                        In data.Serials
                        Select New LegendObject With {
                            .color = x.Value.RGBExpression,
                            .fontstyle = CSSFont.GetFontStyle(
                                FontFace.MicrosoftYaHei,
                                FontStyle.Regular,
                                30),
                            .style = LegendStyles.Circle,
                            .title = x.Name
                        }

                        If legendPos.IsEmpty Then
                            legendPos = New Point(CInt(size.Width * 0.8), margin.Top)
                        End If

                        Call g.DrawLegends(legendPos, legends,,, shapeBorder:=legendBorder)
                    End If
                End Sub

            Return g.GraphicsPlots(size, margin, bg, plotInternal)
        End Function
    End Module
End Namespace
