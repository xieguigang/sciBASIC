#Region "Microsoft.VisualBasic::2d314e905a30e608fd412f7dce56f4f6, Data_science\Visualization\Plots\3D\Plot\Scatter3D.vb"

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

    '   Total Lines: 164
    '    Code Lines: 130 (79.27%)
    ' Comment Lines: 15 (9.15%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 19 (11.59%)
    '     File Size: 6.66 KB


    '     Class Scatter3D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: populateModels
    ' 
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Device
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Model
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Plot3D.Impl

    Public Class Scatter3D : Inherits Plot

        ReadOnly serials As Serial3D()
        ReadOnly camera As Camera
        ReadOnly arrowFactor As String
        ReadOnly showHull As Boolean
        ReadOnly hullAlpha As Double
        ReadOnly hullBspline As Single

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="serials"></param>
        ''' <param name="camera"></param>
        ''' <param name="arrowFactor"></param>
        ''' <param name="showHull"></param>
        ''' <param name="hullAlpha">``[0, 255]``</param>
        ''' <param name="hullBspline"></param>
        ''' <param name="theme"></param>
        Public Sub New(serials As IEnumerable(Of Serial3D), camera As Camera, arrowFactor$, showHull As Boolean, hullAlpha As Double, hullBspline As Single, theme As Theme)
            MyBase.New(theme)

            Me.serials = serials.ToArray
            Me.camera = camera
            Me.arrowFactor = arrowFactor
            Me.showHull = showHull
            Me.hullAlpha = hullAlpha
            Me.hullBspline = hullBspline
        End Sub

        Private Iterator Function populateModels(css As CSSEnvirnment) As IEnumerable(Of Element3D)
            Dim points As Point3D() = serials _
                .Select(Function(s) s.Points.Values) _
                .IteratesALL _
                .ToArray

            ' 首先需要获取得到XYZ值的范围
            Dim X, Y, Z As Vector

            With points.VectorShadows
                X = DirectCast(.X, IEnumerable(Of Double)).Range.CreateAxisTicks
                Y = DirectCast(.Y, IEnumerable(Of Double)).Range.CreateAxisTicks
                Z = DirectCast(.Z, IEnumerable(Of Double)).Range.CreateAxisTicks
            End With

            ' 然后生成底部的网格
            For Each line As Line In Grids.Grid1(css, X, Y, (X(1) - X(0), Y(1) - Y(0)), Z.Min)
                Yield line
            Next

            For Each item As Element3D In AxisDraw.Axis(
                    css,
                    xrange:=X, yrange:=Y, zrange:=Z,
                    labelFontCss:=theme.axisLabelCSS,
                    labels:=(xlabel, ylabel, zlabel),
                    strokeCSS:=theme.axisStroke,
                    arrowFactor:=arrowFactor,
                    labelColorVal:=theme.mainTextColor
                )

                Yield item
            Next

            ' 最后混合进入系列点
            For Each serial As Serial3D In serials
                Dim data As NamedValue(Of Point3D)() = serial.Points
                Dim size As New Size With {
                    .Width = serial.PointSize,
                    .Height = serial.PointSize
                }
                Dim color As New SolidBrush(serial.Color)

                If showHull Then
                    Yield New ConvexHullPolygon With {
                        .Brush = New SolidBrush(serial.Color.Alpha(hullAlpha)),
                        .Path = data _
                            .Select(Function(pt) pt.Value) _
                            .ToArray,
                        .bspline = hullBspline
                    }
                End If

                For Each pt As NamedValue(Of Point3D) In data
                    Yield New ShapePoint With {
                        .Fill = color,
                        .Location = pt.Value,
                        .Size = size,
                        .Style = serial.Shape,
                        .Label = pt.Name
                    }
                Next
            Next
        End Function

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim legends As LegendObject() = serials _
                .Select(Function(s)
                            Return New LegendObject With {
                                .color = s.Color.RGBExpression,
                                .fontstyle = theme.axisLabelCSS,
                                .style = s.Shape,
                                .title = s.Title
                            }
                        End Function) _
                .ToArray
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim font As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS))
            Dim region As Rectangle = canvas.PlotRegion

            ' 绘制图例？？
            Dim legendHeight! = (legends.Length * (font.Height + 5))
            Dim legendTop! = (region.Height - legendHeight) / 2
            Dim maxLegendLabelSize As SizeF = legends.Select(Function(s) s.title) _
                .MaxLengthString _
                .MeasureSize(g, font)
            Dim legendWidth = maxLegendLabelSize.Height * 1.125
            Dim legendLeft! = region.Right - maxLegendLabelSize.Width - legendWidth
            Dim topLeft As New Point With {
                .X = legendLeft,
                .Y = legendTop
            }
            Dim legendSize$ = $"{legendWidth},{legendWidth}"
            Dim labelColor As New SolidBrush(theme.tagColor.TranslateColor)

            ' 要先绘制三维图形，要不然会将图例遮住的
            Call populateModels(css).RenderAs3DChart(
                canvas:=g,
                camera:=camera,
                region:=canvas,
                theme:=theme
            )

            If theme.drawLegend Then
                Call g.DrawLegends(
                    topLeft:=topLeft,
                    legends:=legends,
                    gSize:=legendSize,
                    d:=5,
                    regionBorder:=Stroke.AxisStroke
                )
            End If
        End Sub
    End Class
End Namespace
