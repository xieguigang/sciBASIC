#Region "Microsoft.VisualBasic::a7748d95cdf60597fed66d5a81920bbf, ..\sciBASIC#\Data_science\Mathematical\Plots\Bubble.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis

Public Module Bubble

    ''' <summary>
    ''' <see cref="PointData.value"/>是Bubble的半径大小
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <param name="legend"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(data As IEnumerable(Of SerialData),
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional bg As String = "white",
                         Optional legend As Boolean = True,
                         Optional logR As Boolean = False,
                         Optional legendBorder As Border = Nothing,
                         Optional xAxis$ = Nothing,
                         Optional yAxis$ = Nothing,
                         Optional xlabel$ = "",
                         Optional ylabel$ = "",
                         Optional axisLabelFontCSS$ = CSSFont.Win7LargeBold,
                         Optional tagFontCSS$ = CSSFont.Win10Normal) As Bitmap

        Dim margin As Padding = padding
        Dim tagLabelFont As Font = CSSFont.TryParse(tagFontCSS).GDIObject

        Dim plotInternal =
            Sub(ByRef g As Graphics, grect As GraphicsRegion)
                Dim array As SerialData() = data.ToArray
                Dim mapper As Mapper
                Dim rangeData As New Scaling(array, False)

                If xAxis.StringEmpty Then
                    ' 任意一个位空值就会使用普通的axis数据计算方法
                    mapper = New Mapper(rangeData) ' 这个并不是以y值来表示数量上的关系的，point是随机位置，所以在这里使用相对scalling
                Else
                    Dim yaxisData As New AxisProvider(rangeData.yrange.GetAxisValues)
                    mapper = New Mapper(x:=xAxis, y:=yaxisData, range:=rangeData)
                End If

                Dim scale As Func(Of Double, Double) =
                     [If](Of Func(Of Double, Double))(
                     logR, Function(r) Math.Log(r + 1) + 1,
                           Function(r) r)

                Call g.DrawAxis(size, margin, mapper, True,
                                xlabel:=xlabel,
                                ylabel:=ylabel,
                                labelFontStyle:=axisLabelFontCSS)

                For Each s As SerialData In mapper.ForEach(size, margin)
                    Dim b As New SolidBrush(s.color)

                    For Each pt As PointData In s
                        Dim r As Double = scale(pt.value)
                        Dim p As New Point(CInt(pt.pt.X - r), CInt(pt.pt.Y - r))
                        Dim rect As New Rectangle(p, New Size(r * 2, r * 2))

                        Call g.FillPie(b, rect, 0, 360)

                        If Not pt.Tag.StringEmpty Then
                            Call g.DrawString(pt.Tag, tagLabelFont, Brushes.Black, New PointF(rect.Right, rect.Top))
                        End If
                    Next
                Next

                If legend Then

                    Dim topLeft As New Point(size.Width * 0.8, margin.Top)
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
            End Sub

        Return GraphicsPlots(size, margin, bg, plotInternal)
    End Function
End Module
