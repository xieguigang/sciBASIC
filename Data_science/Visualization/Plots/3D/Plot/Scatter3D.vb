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
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

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

            Call populateModels()
        End Sub

        Private Iterator Function populateModels() As IEnumerable(Of Element3D)
            Dim points As Point3D() = serials _
                .Select(Function(s) s.Points.Values) _
                .IteratesALL _
                .ToArray
            Dim axisLabelFont As Font = CSSFont.TryParse(theme.axisLabelCSS)

            ' 首先需要获取得到XYZ值的范围
            Dim X, Y, Z As Vector

            With points.VectorShadows
                X = DirectCast(.X, IEnumerable(Of Double)).Range.CreateAxisTicks
                Y = DirectCast(.Y, IEnumerable(Of Double)).Range.CreateAxisTicks
                Z = DirectCast(.Z, IEnumerable(Of Double)).Range.CreateAxisTicks
            End With

            ' 然后生成底部的网格
            For Each line As Line In GridBottom.Grid(X, Y, (X(1) - X(0), Y(1) - Y(0)), Z.Min)
                Yield line
            Next

            For Each item As Element3D In AxisDraw.Axis(
                    xrange:=X, yrange:=Y, zrange:=Z,
                    labelFont:=axisLabelFont,
                    labels:=(theme.xlabel, theme.ylabel, theme.zlabel),
                    strokeCSS:=theme.axisStroke,
                    arrowFactor:=arrowFactor
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
            Dim legends As Legend() = serials _
                .Select(Function(s)
                            Return New Legend With {
                                .color = s.Color.RGBExpression,
                                .fontstyle = theme.axisLabelCSS,
                                .style = s.Shape,
                                .title = s.Title
                            }
                        End Function) _
                .ToArray
            Dim font As Font = CSSFont.TryParse(theme.axisLabelCSS)
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
            Call populateModels.RenderAs3DChart(
                canvas:=g,
                camera:=camera,
                region:=canvas,
                labelFont:=CSSFont.TryParse(theme.tagCSS),
                labelerItr:=0,
                showLabel:=theme.drawLabels,
                labelColor:=labelColor
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