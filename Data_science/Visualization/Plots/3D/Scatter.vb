#Region "Microsoft.VisualBasic::03f93239e7c3a8318a0ba3f0fe63071e, Data_science\Visualization\Plots\3D\Scatter.vb"

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

    '   Total Lines: 153
    '    Code Lines: 106 (69.28%)
    ' Comment Lines: 32 (20.92%)
    '    - Xml Docs: 96.88%
    ' 
    '   Blank Lines: 15 (9.80%)
    '     File Size: 6.46 KB


    '     Module Scatter
    ' 
    '         Function: (+2 Overloads) Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
#End If

Namespace Plot3D

    ''' <summary>
    ''' 3D scatter charting
    ''' </summary>
    Public Module Scatter

        ''' <summary>
        ''' plot scatter 3D
        ''' </summary>
        ''' <param name="serials"></param>
        ''' <param name="camera"></param>
        ''' <param name="bg$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="axisLabelFontCSS$"></param>
        ''' <param name="boxStroke$"></param>
        ''' <param name="axisStroke$"></param>
        ''' <param name="showHull">show convex hull for each <paramref name="serials"/> data.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 首先要生成3维图表的模型元素，然后将这些元素混合在一起，最后按照Z深度的排序结果顺序绘制出来，才能够生成一幅有层次感的3维图表
        ''' </remarks>
        <Extension>
        Public Function Plot(serials As IEnumerable(Of Serial3D),
                             camera As Camera,
                             Optional bg$ = "white",
                             Optional padding$ = g.DefaultPadding,
                             Optional axisLabelFontCSS$ = CSSFont.Win7Normal,
                             Optional elementLabelFont$ = CSSFont.Win10Normal,
                             Optional boxStroke$ = Stroke.StrongHighlightStroke,
                             Optional axisStroke$ = Stroke.AxisStroke,
                             Optional labX$ = "X",
                             Optional labY$ = "Y",
                             Optional labZ$ = "Z",
                             Optional arrowFactor$ = "2,2",
                             Optional showLegend As Boolean = True,
                             Optional showHull As Boolean = True,
                             Optional hullAlpha As Integer = 150,
                             Optional hullBspline As Single = 2) As GraphicsData

            Dim size$ = $"{camera.screen.Width},{camera.screen.Height}"
            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .axisStroke = axisStroke,
                .axisLabelCSS = axisLabelFontCSS,
                .tagCSS = elementLabelFont,
                .drawLegend = showLegend,
                .legendBoxStroke = boxStroke
            }

            Return New Impl.Scatter3D(
                serials:=serials,
                camera:=camera,
                arrowFactor:=arrowFactor,
                showHull:=showHull,
                hullAlpha:=hullAlpha,
                hullBspline:=hullBspline,
                theme:=theme
            ) With {
                .xlabel = labX,
                .ylabel = labY,
                .zlabel = labZ
            }.Plot(size:=size)
        End Function

        ''' <summary>
        ''' 绘制三维散点图
        ''' </summary>
        ''' <param name="func"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="camera"></param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <param name="lineColor$"></param>
        ''' <param name="font"></param>
        ''' <param name="bg$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(func As Func(Of Double, Double, Double),
                             x As DoubleRange,
                             y As DoubleRange,
                             camera As Camera,
                             Optional xsteps! = 0.1,
                             Optional ysteps! = 0.1,
                             Optional lineColor$ = "red",
                             Optional font As Font = Nothing,
                             Optional bg$ = "white",
                             Optional padding$ = "padding: 5px 5px 5px 5px;") As GraphicsData

            Dim data As Point3D() = func _
                .Evaluate(x, y, xsteps, ysteps) _
                .IteratesALL _
                .Select(Function(o) New Point3D(o.X, o.y, o.z))
            Dim rect As RectangleF
            Dim previous As PointF
            Dim cur As PointF
            Dim lcolor As New Pen(lineColor.ToColor)

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    '  Call AxisDraw.DrawAxis(g, data, camera, font)

                    With camera

                        data(Scan0) = .Project(.Rotate(data(Scan0)))
                        previous = data(Scan0).PointXY(camera.screen)

                        For Each pt As Point3D In data.Skip(1)
                            pt = .Project(.Rotate(pt))   ' 3d project to 2d
                            cur = pt.PointXY(camera.screen)
                            rect = New RectangleF(cur, New SizeF(5, 5))

                            Call g.FillPie(Brushes.Red, rect, 0, 360)  ' 画点
                            Call g.DrawLine(lcolor, previous.X, previous.Y, cur.X, cur.Y)       ' 画线

                            previous = cur
                        Next
                    End With
                End Sub

            Return camera.screen.GraphicsPlots(padding, bg, plotInternal)
        End Function
    End Module
End Namespace
