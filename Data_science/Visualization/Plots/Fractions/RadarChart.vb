#Region "Microsoft.VisualBasic::b4476c51fa46ecc17b1f9e032c323d5f, Data_science\Visualization\Plots\Fractions\RadarChart.vb"

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

    '   Total Lines: 400
    '    Code Lines: 256 (64.00%)
    ' Comment Lines: 93 (23.25%)
    '    - Xml Docs: 45.16%
    ' 
    '   Blank Lines: 51 (12.75%)
    '     File Size: 20.24 KB


    '     Module RadarChart
    ' 
    '         Function: Plot, PlotSingleLayer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace Fractions

    ''' <summary>
    ''' 雷达图
    ''' </summary>
    Public Module RadarChart

        ''' <summary>
        ''' 函数使用的是<see cref="FractionData.Value"/>的数据来进行作图
        ''' </summary>
        ''' <param name="serials"></param>
        ''' <param name="size$"></param>
        ''' <param name="margin$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="regionFill$"></param>
        ''' <param name="serialColorSchema$"></param>
        ''' <param name="labelTextColor$"></param>
        ''' <param name="colorAlpha%"></param>
        ''' <param name="axisRange"></param>
        ''' <param name="shapeBorderWidth!"></param>
        ''' <param name="pointRadius!"></param>
        ''' <param name="labelFontCSS$"></param>
        ''' <param name="axisStrokeStyle$"></param>
        ''' <param name="spline"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PlotSingleLayer(serials As IEnumerable(Of FractionData),
                                        Optional size$ = "3000,2700",
                                        Optional margin$ = g.DefaultPadding,
                                        Optional bg$ = "white",
                                        Optional regionFill$ = "#fafafa",
                                        Optional serialColorSchema$ = "alpha(Set1:c8, 0.5)",
                                        Optional labelTextColor$ = "black",
                                        Optional colorAlpha% = 120,
                                        Optional axisRange As DoubleRange = Nothing,
                                        Optional shapeBorderWidth! = 10,
                                        Optional pointRadius! = 30,
                                        Optional labelFontCSS$ = CSSFont.Win7VeryVeryLarge,
                                        Optional axisStrokeStyle$ = Stroke.WhiteLineStroke,
                                        Optional spline As Boolean = True,
                                        Optional textWrap As Integer = 16) As GraphicsData
            Return {
                New NamedValue(Of FractionData())("", serials.ToArray)
            }.Plot(size:=size,
                   margin:=margin,
                   bg:=bg,
                   regionFill:=regionFill,
                   serialColorSchema:=serialColorSchema,
                   axisRange:=axisRange,
                   axisStrokeStyle:=axisStrokeStyle,
                   colorAlpha:=colorAlpha,
                   labelFontCSS:=labelFontCSS,
                   labelTextColor:=labelTextColor,
                   pointRadius:=pointRadius,
                   shapeBorderWidth:=shapeBorderWidth,
                   spline:=spline,
                   textWrap:=textWrap
              )
        End Function

        ''' <summary>
        ''' 这个函数是用于绘制多层的雷达图
        ''' </summary>
        ''' <param name="serials">函数使用的是<see cref="FractionData.Value"/>的数据来进行作图</param>
        ''' <param name="size$"></param>
        ''' <param name="margin$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="regionFill$"></param>
        ''' <param name="serialColorSchema$"></param>
        ''' <param name="colorAlpha%"></param>
        ''' <param name="axisRange"></param>
        ''' <param name="shapeBorderWidth!"></param>
        ''' <param name="pointRadius!"></param>
        ''' <param name="labelFontCSS$"></param>
        ''' <param name="axisStrokeStyle$"></param>
        ''' <param name="spline%">
        ''' 如果这个参数设置为``False``的话，那么项目之间的连接将会直接使用直线进行连接
        ''' 这个函数参数的默认值为``True``程序会使用两个相邻的项目的坐标点中间的平均值
        ''' 处作为贝塞尔曲线的控制点进行插值曲线化处理
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(serials As NamedValue(Of FractionData())(),
                             Optional size$ = "3000,2700",
                             Optional margin$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional regionFill$ = "#fafafa",
                             Optional serialColorSchema$ = "alpha(Set1:c8, 0.5)",
                             Optional labelTextColor$ = "black",
                             Optional colorAlpha% = 120,
                             Optional axisRange As DoubleRange = Nothing,
                             Optional shapeBorderWidth! = 10,
                             Optional pointRadius! = 30,
                             Optional labelFontCSS$ = CSSFont.Win7VeryVeryLarge,
                             Optional axisStrokeStyle$ = Stroke.WhiteLineStroke,
                             Optional spline As Boolean = True,
                             Optional textWrap As Integer = 16,
                             Optional ppi As Integer = 100) As GraphicsData

            Dim serialColors As Color() = Designer.GetColors(serialColorSchema) _
                                                  .Select(Function(c) c.Alpha(colorAlpha)) _
                                                  .ToArray
            Dim borderPens As Pen() = serialColors _
                .Select(Function(c) New Pen(c.Alpha(255), shapeBorderWidth)) _
                .ToArray
            Dim directions$() = serials.Select(Function(s) s.Value) _
                                       .IteratesALL _
                                       .Keys _
                                       .Distinct _
                                       .ToArray
            Dim dDegree# = 360 / directions.Length
            Dim axisPen As Pen = Stroke.TryParse(axisStrokeStyle).GDIObject
            Dim regionFillColor As New SolidBrush(regionFill.TranslateColor)

            If axisRange Is Nothing Then
                axisRange = serials.Values _
                                   .IteratesALL _
                                   .Select(Function(f) f.Value) _
                                   .AsVector _
                                   .CreateAxisTicks
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim plotRect = region.PlotRegion
                    Dim center As PointF = plotRect.Centre
                    Dim css As CSSEnvirnment = g.LoadEnvironment
                    Dim labelFont As Font = css.GetFont(CSSFont.TryParse(labelFontCSS))
                    Dim radius As DoubleRange = {0, std.Min(plotRect.Width, plotRect.Height) / 2}
                    Dim serial As NamedValue(Of FractionData())
                    Dim r#
                    Dim alpha! = -90
                    Dim shape As New List(Of PointF)
                    Dim polarShape As New List(Of PolarPoint)
                    Dim axisPoints As New List(Of PointF)
                    Dim f As FractionData
                    Dim value#
                    Dim color As Color
                    Dim pen As Pen
                    Dim maxAxis As PointF

                    ' 绘制出中心点以及坐标轴
                    ' Call g.FillPie(Brushes.Gray, New Rectangle(center.X - 2, center.Y - 2, 4, 4), 0, 360)

                    Dim dr = radius.Max / 5
                    Dim grayColor% = 230
                    Dim ellipsePen As New Pen(Color.White, axisPen.Width * 1.25) With {
                        .DashStyle = axisPen.DashStyle
                    }

                    ' 填充坐标轴区域
                    r = dr * 5
                    g.FillEllipse(regionFillColor, New RectangleF(center.OffSet2D(-r, -r), New SizeF(r * 2, r * 2)))

                    ' 区域是从大到小进行填充
                    For i As Integer = 4 To 1 Step -1
                        r = dr * i
                        g.FillEllipse(New SolidBrush(Color.FromArgb(grayColor, grayColor, grayColor)), New RectangleF(center.OffSet2D(-r, -r), New SizeF(r * 2, r * 2)))
                        grayColor = grayColor * 0.95
                    Next

                    For i As Integer = 1 To 4
                        r = dr * i
                        g.DrawEllipse(ellipsePen, New RectangleF(center.OffSet2D(-r, -r), New SizeF(r * 2, r * 2)))
                    Next

                    ' 绘制极坐标轴
                    For i As Integer = 0 To directions.Length - 1
                        maxAxis = (radius.Max, alpha).ToCartesianPoint.OffSet2D(center)
                        g.DrawLine(axisPen, maxAxis, center)
                        alpha += dDegree
                    Next

                    For i As Integer = 0 To serials.Length - 1
                        serial = serials(i)
                        color = serialColors(i)
                        pen = borderPens(i)
                        shape *= 0
                        polarShape *= 0
                        axisPoints *= 0
                        alpha = -90

                        With serial.Value.ToDictionary
                            For Each key As String In directions
                                f = .Item(key)

                                If f Is Nothing Then
                                    value = axisRange.Min
                                Else
                                    value = f.Value
                                End If

                                ' 通过计算出每一个坐标轴的半径值和角度转换为笛卡尔坐标系的坐标点
                                ' 然后构成雷达图之中的喷涂路径
                                r = axisRange.ScaleMapping(value, radius)
                                ' 在这里只添加极坐标点，方便后面的贝塞尔曲线插值处理
                                polarShape += (r, alpha)
                                axisPoints += (r, alpha).ToCartesianPoint.OffSet2D(center)
                                alpha += dDegree
                            Next

                            If spline Then
                                shape = (polarShape + polarShape(0)) _
                                    .Select(Function(p)
                                                Return p.Point.OffSet2D(center)
                                            End Function) _
                                    .AsList
                                shape = CubicSpline.RecalcSpline(shape, expected:=50).AsList

                                '' 使用AB两个坐标轴的中间夹角处作为控制点
                                '' 控制点的值为AB两个点的平均值
                                '' 使用滑窗进行计算
                                'Dim avg#
                                'Dim centerAngle!
                                'Dim adjacent = (polarShape + polarShape(0)).SlideWindows(winSize:=2).ToArray
                                'Dim control As PointF
                                'Dim c1, c2 As PointF

                                'shape *= 0

                                'For Each between As SlideWindow(Of PolarPoint) In adjacent
                                '    avg = between.Average(Function(p) p.Radius)

                                '    If between.Index = adjacent.Length - 1 Then
                                '        ' 2018-5-3
                                '        '
                                '        ' 由于雷达图的绘制是从-90度开始的，所以-90度相当于0，270度相当于360
                                '        ' 因为最后一个滑窗为240度左右回到原点-90度
                                '        ' 所以直接计算平均值的话会出现 (240 + -90) / 2 = 70 的错误
                                '        '
                                '        ' 需要进行额外处理
                                '        centerAngle = (270 + between.First.Angle) / 2
                                '    Else
                                '        centerAngle = between.Average(Function(p) p.Angle)
                                '    End If
                                '    ' 得到中间的这个控制点
                                '    control = (avg, centerAngle).ToCartesianPoint.OffSet2D(center)
                                '    ' 进行贝塞尔曲线插值
                                '    c1 = between.First.Point.OffSet2D(center)
                                '    c2 = between.Last.Point.OffSet2D(center)

                                '    Dim seq = {c1, control, c2}.NewtonPolynomial(3)

                                '    shape += seq.Take(seq.Length - 1)
                                'Next

                                'shape = shape _
                                '    .Where(Function(p) Not p.Y.IsNaNImaginary) _
                                '    .AsList
                            Else
                                shape = polarShape.Select(Function(p)
                                                              Return p.Point.OffSet2D(center)
                                                          End Function) _
                                                  .AsList
                            End If

                            If shape > 0 Then
                                ' 填充区域
                                With New GraphicsPath
                                    Call .AddPolygon(shape)
                                    Call .CloseAllFigures()

                                    Call g.ShapeGlow(.ByRef, color.Lighten(0.75), shapeBorderWidth * 3)
                                    Call g.FillPath(New SolidBrush(color), .ByRef)
                                    Call g.DrawPath(pen, .ByRef)

#If DEBUG Then
                                    Dim debugFont As New Font(FontFace.MicrosoftYaHeiUI, 12)

                                    For Each point In shape
                                        Call g.DrawString(point.ToString, debugFont, Brushes.Gray, point)
                                    Next
#End If
                                End With
                            Else
                                Call "No shape area could be fill, please check your data as it is empty!".Warning
                            End If

                            color = color.Alpha(255)

                            For Each point As PointF In axisPoints
                                Call g.DrawCircle(point, pointRadius, New SolidBrush(color))
                            Next
                        End With
                    Next

                    Dim labelSize As SizeF
                    Dim labelColor As New SolidBrush(labelTextColor.TranslateColor)
                    Dim label$

                    alpha! = -90

                    ' 绘制极坐标轴标签
                    ' 标签应该在最后绘制，否则可能会被盖住
                    For i As Integer = 0 To directions.Length - 1
                        maxAxis = (radius.Max, alpha).ToCartesianPoint.OffSet2D(center)
                        alpha += dDegree

                        ' 绘制坐标轴标签
                        label = directions(i)

                        If label.Length > textWrap Then
                            label = label.DoWordWrap(textWrap)
                        End If

                        labelSize = g.MeasureString(label, labelFont)

                        Select Case center.QuadrantRegion(maxAxis)

                            Case QuadrantRegions.RightTop
                                ' 右上角
                                maxAxis = maxAxis.OffSet2D(0, -labelSize.Height * 1.25)

                            Case QuadrantRegions.YTop
                                maxAxis = maxAxis.OffSet2D(-labelSize.Width * 1.25 / 2, -labelSize.Height * 1.25)

                            Case QuadrantRegions.LeftTop
                                ' 左上角
                                maxAxis = maxAxis.OffSet2D(-labelSize.Width * 1.25, -labelSize.Height * 1.25)

                            Case QuadrantRegions.XLeft
                                maxAxis = maxAxis.OffSet2D(-labelSize.Width * 1.25, -labelSize.Height * 1.25 / 2)

                            Case QuadrantRegions.LeftBottom
                                ' 左下角
                                maxAxis = maxAxis.OffSet2D(-labelSize.Width * 1.25, 0)

                            Case QuadrantRegions.YBottom
                                maxAxis = maxAxis.OffSet2D(-labelSize.Width * 1.25 / 2, 0)

                            Case QuadrantRegions.XRight
                                maxAxis = maxAxis.OffSet2D(0, -labelSize.Height * 1.25 / 2)

                            Case Else
                                ' 右下角
                                maxAxis = maxAxis.OffSet2D(0, 0)

                        End Select

                        ' 计算标签是否已经越出绘图的边界线而无法被显示出来了
                        Dim labelRect As New RectangleF(maxAxis, labelSize)

                        If labelRect.Top < 0 Then
                            maxAxis = New PointF(maxAxis.X, 1)
                            labelRect = New RectangleF(maxAxis, labelSize)
                        End If
                        If labelRect.Left < 0 Then
                            maxAxis = New PointF(1, maxAxis.Y)
                            labelRect = New RectangleF(maxAxis, labelSize)
                        End If
                        If labelRect.Right > region.Size.Width Then
                            maxAxis = New PointF With {
                                .X = region.Size.Width - labelSize.Width - 1,
                                .Y = maxAxis.Y
                            }
                            labelRect = New RectangleF(maxAxis, labelSize)
                        End If
                        If labelRect.Bottom > region.Size.Height Then
                            maxAxis = New PointF With {
                                .X = maxAxis.X,
                                .Y = region.Size.Height - labelSize.Height - 1
                            }
                            labelRect = New RectangleF(maxAxis, labelSize)
                        End If

                        g.DrawString(label, labelFont, labelColor, maxAxis)
                    Next
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, margin,
                bg,
                plotInternal
            )
        End Function
    End Module
End Namespace
