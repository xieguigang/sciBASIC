#Region "Microsoft.VisualBasic::e0ffa564643c4fd271acd5d9e2895590, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Scatter.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Plot3D

    ''' <summary>
    ''' 3D scatter charting
    ''' </summary>
    Public Module Scatter

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
                             Optional padding As Padding = Nothing) As GraphicsData

            Dim data As Point3D() = func _
                .Evaluate(x, y, xsteps, ysteps) _
                .IteratesALL _
                .ToArray(Function(o) New Point3D(o.X, o.y, o.z))
            Dim rect As Rectangle
            Dim previous As Point
            Dim cur As Point
            Dim lcolor As New Pen(lineColor.ToColor)

            If padding.IsEmpty Then
                padding = "padding: 5px 5px 5px 5px;"
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Call AxisDraw.DrawAxis(g, data, camera, font)

                    With camera

                        data(Scan0) = .Project(.Rotate(data(Scan0)))
                        previous = data(Scan0).PointXY(camera.screen)

                        For Each pt As Point3D In data.Skip(1)
                            pt = .Project(.Rotate(pt))   ' 3d project to 2d
                            cur = pt.PointXY(camera.screen)
                            rect = New Rectangle(cur, New Size(5, 5))

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
