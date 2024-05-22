#Region "Microsoft.VisualBasic::467b8ab3cf58cfaf3d7b2a7310434835, Data_science\Visualization\Plots\3D\ScatterHeatmap.vb"

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

    '   Total Lines: 266
    '    Code Lines: 204 (76.69%)
    ' Comment Lines: 35 (13.16%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 27 (10.15%)
    '     File Size: 11.44 KB


    '     Module ScatterHeatmap
    ' 
    '         Function: (+2 Overloads) GetPlotFunction, (+3 Overloads) Plot
    '         Structure __plot
    ' 
    '             Sub: Plot
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports stdNum = System.Math

Namespace Plot3D

    Public Module ScatterHeatmap

        <Extension>
        Public Function Plot(f As Func(Of Double, Double, (Z#, color#)),
                             xrange As DoubleRange,
                             yrange As DoubleRange,
                             camera As Camera,
                             Optional xn% = 100,
                             Optional yn% = 100,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional parallel As Boolean = False,
                             Optional matrix As List(Of EntityObject) = Nothing,
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True) As GraphicsData

            Dim data As (sf As Surface, C As Double())() =
                f.Surface(
                xrange, yrange,
                xrange.Length / xn,
                yrange.Length / yn,
                parallel, matrix).ToArray

            Return data.Plot(
                camera, legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont, showLegend
            )
        End Function

        ''' <summary>
        ''' DEBUG模式之下会将网格给绘制出来，这个在Release模式之下是不会出现的。
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="xrange"></param>
        ''' <param name="yrange"></param>
        ''' <param name="xn%"></param>
        ''' <param name="yn%"></param>
        ''' <param name="legendTitle$"></param>
        ''' <param name="mapName$"></param>
        ''' <param name="mapLevels%"></param>
        ''' <param name="bg$"></param>
        ''' <param name="parallel"></param>
        ''' <param name="matrix"></param>
        ''' <param name="axisFont$"></param>
        ''' <param name="legendFont"></param>
        ''' <param name="showLegend"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetPlotFunction(f As Func(Of Double, Double, (Z#, color#)),
                                        xrange As DoubleRange,
                                        yrange As DoubleRange,
                                        Optional xn% = 100,
                                        Optional yn% = 100,
                                        Optional legendTitle$ = "3D scatter heatmap",
                                        Optional mapName$ = "Spectral:c10",
                                        Optional mapLevels% = 25,
                                        Optional bg$ = "white",
                                        Optional parallel As Boolean = False,
                                        Optional matrix As List(Of EntityObject) = Nothing,
                                        Optional axisFont$ = CSSFont.Win10Normal,
                                        Optional legendFont As Font = Nothing,
                                        Optional showLegend As Boolean = True) As DrawGraphics

            Dim data As (sf As Surface, C As Double())() =
                f.Surface(
                     xrange, yrange,
                     xrange.Length / xn,
                     yrange.Length / yn,
                     parallel, matrix) _
                 .ToArray

            Return data.GetPlotFunction(
                legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont, showLegend)
        End Function

        <Extension>
        Public Function GetPlotFunction(data As (sf As Surface, c As Double())(),
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True) As DrawGraphics

            Dim averages As Double() = data _
                .Select(Function(c) c.c.Average).ToArray
            Dim levels As Integer() = averages _
                .GenerateMapping(mapLevels) _
                .ToArray
            Dim colors As SolidBrush() =
                Designer _
                .GetBrushes(mapName, mapLevels,)
            Dim rawPoints As Point3D() = data _
                .Select(Function(s) s.sf.vertices) _
                .IteratesALL _
                .ToArray
            Dim internal As New __plot With {
                .averages = averages,
                .colors = colors,
                .data = data,
                .legendFont = legendFont,
                .legendTitle = legendTitle,
                .levels = levels,
                .rawPoints = rawPoints,
                .showLegend = showLegend
            }

            Return Sub(ByRef g, camera)
                       Call g.Clear(bg.ToColor)
                       Call internal.Plot(g, camera)
                   End Sub
        End Function

        <Extension>
        Public Function Plot(data As (sf As Surface, c As Double())(),
                             camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing,
                             Optional showLegend As Boolean = True,
                             Optional padding$ = g.ZeroPadding) As GraphicsData

            Dim modelPlot As DrawGraphics =
                data _
                .GetPlotFunction(legendFont:=legendFont,
                                 axisFont:=axisFont,
                                 bg:=bg,
                                 legendTitle:=legendTitle,
                                 mapLevels:=mapLevels,
                                 mapName:=mapName,
                                 showLegend:=showLegend)
            Dim margin As CSS.Padding = padding

            Return GraphicsPlots(
                camera.screen, margin,
                bg$,
                driver:=Drivers.GDI,
                plotAPI:=Sub(ByRef g, region)
                             Call modelPlot(DirectCast(g, Graphics2D).Graphics, camera)
                         End Sub)
        End Function

        Private Structure __plot

            Dim averages As Double()
            Dim levels As Integer()
            Dim colors As SolidBrush()
            Dim rawPoints As Point3D()
            Dim data As (sf As Surface, c As Double())()
            Dim legendTitle$
            Dim legendFont As Font
            Dim showLegend As Boolean

            Public Sub Plot(g As Graphics, camera As Camera)

                'Call g.DrawAxis(
                '    rawPoints,
                '    camera,
                '    CSSFont.TryParse(axisFont).GDIObject)

                With camera
                    Dim surfaces As New List(Of Surface)

                    ' 绘制通过函数所计算出来的三维表面
                    For Each sf In data.SeqIterator
                        Dim surface As Surface = (+sf).sf
                        Dim level% = levels(sf.i)

                        If level > colors.Length - 1 Then
                            level = colors.Length - 1
                        ElseIf level < 0 Then
                            level = 0
                        End If

                        surfaces += New Surface With {
                            .brush = colors(level),
                            .vertices = camera _
                            .Rotate(surface.vertices) _
                            .ToArray
                        }
                    Next

                    Call g.SurfacePainter(camera, surfaces)
                End With

                If showLegend Then ' Draw legends
                    Dim drawSize As New Size With {
                        .Width = camera.screen.Width * 0.15,
                        .Height = 5 / 4 * .Width
                    }
                    Dim legend As GraphicsData = colors.ColorMapLegend(
                        haveUnmapped:=False,
                        min:=stdNum.Round(averages.Min, 1),
                        max:=stdNum.Round(averages.Max, 1),
                        title:=legendTitle,
                        titleFont:=legendFont)
                    Dim lsize As Size = legend.Layout.Size
                    Dim left% = camera.screen.Width - lsize.Width + 150
                    Dim top% = camera.screen.Height / 3

                    Call g.DrawImageUnscaled(DirectCast(legend, ImageData).Image, left, top)
                End If
            End Sub
        End Structure

        ''' <summary>
        ''' 3D heatmap plot from matrix data
        ''' </summary>
        ''' <param name="matrix"></param>
        ''' <param name="Camera"></param>
        ''' <param name="legendTitle$"></param>
        ''' <param name="mapName$"></param>
        ''' <param name="mapLevels%"></param>
        ''' <param name="bg$"></param>
        ''' <param name="axisFont$"></param>
        ''' <param name="legendFont"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(matrix As IEnumerable(Of EntityObject),
                             Camera As Camera,
                             Optional legendTitle$ = "3D scatter heatmap",
                             Optional mapName$ = "Spectral:c10",
                             Optional mapLevels% = 25,
                             Optional bg$ = "white",
                             Optional axisFont$ = CSSFont.Win10Normal,
                             Optional legendFont As Font = Nothing) As GraphicsData

            Return matrix.Surface.ToArray _
                .Plot(
                Camera, legendTitle,
                mapName, mapLevels,
                bg,
                axisFont, legendFont)
        End Function
    End Module
End Namespace
