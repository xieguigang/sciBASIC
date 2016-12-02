#Region "Microsoft.VisualBasic::38f9a739bdd9ca1fb4fd9140a3ed8a98, ..\sciBASIC#\Data_science\Mathematical\Plots\Scatter\Scatter.vb"

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
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.SlideWindow
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical

Public Module Scatter

    ''' <summary>
    ''' Scatter plot function.(绘图函数)
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(c As IEnumerable(Of SerialData),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional showGrid As Boolean = True,
                         Optional showLegend As Boolean = True,
                         Optional legendPosition As Point = Nothing,
                         Optional drawLine As Boolean = True,
                         Optional legendBorder As Border = Nothing,
                         Optional fill As Boolean = False,
                         Optional fillPie As Boolean = True,
                         Optional legendFontSize! = 24,
                         Optional absoluteScaling As Boolean = True) As Bitmap

        Return GraphicsPlots(
            size, margin, bg,
            Sub(ByRef g, grect)
                Dim array As SerialData() = c.ToArray
                Dim mapper As New Scaling(array, absoluteScaling)

                Call g.DrawAxis(size, margin, mapper, showGrid)

                For Each line As SerialData In mapper.ForEach(size, margin)
                    Dim pts = line.pts.SlideWindows(2)
                    Dim pen As New Pen(color:=line.color, width:=line.width) With {
                        .DashStyle = line.lineType
                    }
                    Dim br As New SolidBrush(line.color)
                    Dim d = line.PointSize
                    Dim r As Single = line.PointSize / 2
                    Dim bottom! = size.Height - margin.Height

                    For Each pt In pts
                        Dim a As PointData = pt.First
                        Dim b As PointData = pt.Last

                        If drawLine Then
                            Call g.DrawLine(pen, a.pt, b.pt)
                        End If
                        If fill Then
                            Dim path As New GraphicsPath
                            Dim ptbr As New PointF(b.pt.X, bottom)
                            Dim ptbl As New PointF(a.pt.X, bottom)

                            path.AddLine(a.pt, b.pt)
                            path.AddLine(b.pt, ptbr)
                            path.AddLine(ptbr, ptbl)
                            path.AddLine(ptbl, a.pt)
                            path.CloseFigure()

                            Call g.FillPath(br, path)
                        End If

                        If fillPie Then
                            Call g.FillPie(br, a.pt.X - r, a.pt.Y - r, d, d, 0, 360)
                            Call g.FillPie(br, b.pt.X - r, b.pt.Y - r, d, d, 0, 360)
                        End If
                    Next

                    If Not line.annotations.IsNullOrEmpty Then
                        Dim raw = array.Where(Function(s) s.title = line.title).First

                        For Each annotation As Annotation In line.annotations
                            Call annotation.Draw(g, mapper, raw, grect)
                        Next
                    End If

                    If showLegend Then
                        Dim legends As Legend() = LinqAPI.Exec(Of Legend) <=
 _
                            From x As SerialData
                            In array
                            Select New Legend With {
                                .color = x.color.RGBExpression,
                                .fontstyle = CSSFont.GetFontStyle(
                                    FontFace.MicrosoftYaHei,
                                    FontStyle.Regular,
                                    legendFontSize),
                                .style = LegendStyles.Circle,
                                .title = x.title
                            }

                        If legendPosition.IsEmpty Then
                            legendPosition = New Point(CInt(size.Width * 0.7), margin.Height)
                        End If

                        Call g.DrawLegends(legendPosition, legends,,, legendBorder)
                    End If
                Next
            End Sub)
    End Function

    <Extension>
    Public Function Plot(ode As ODE, Optional size As Size = Nothing, Optional margin As Size = Nothing, Optional bg As String = "white") As Bitmap
        Return {ode.FromODE("cyan")}.Plot(size, margin, bg)
    End Function

    <Extension>
    Public Function Plot(ode As ODEsOut,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 30,
                         Optional width As Single = 5) As Bitmap
        Return ode.FromODEs(, ptSize, width).Plot(size, margin, bg)
    End Function

    Public Function Plot(x As Vector,
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 15,
                         Optional width As Single = 5,
                         Optional drawLine As Boolean = False) As Bitmap
        Return {
            FromVector(x,,, ptSize, width)
        }.Plot(size, margin, bg, True, False, , drawLine)
    End Function

    Public Function FromVector(y As IEnumerable(Of Double),
                               Optional color As String = "black",
                               Optional dash As DashStyle = DashStyle.Dash,
                               Optional ptSize! = 30,
                               Optional width As Single = 5,
                               Optional xrange As IEnumerable(Of Double) = Nothing,
                               Optional title$ = "Vector Plot",
                               Optional alpha% = 255) As SerialData
        Dim array#()
        Dim y0#() = y.ToArray

        If xrange Is Nothing Then
            array = VBMathExtensions.seq(0, y0.Length, 1)
        Else
            array = xrange.ToArray
        End If

        Return New SerialData With {
            .color = Drawing.Color.FromArgb(alpha, color.ToColor),
            .lineType = dash,
            .PointSize = ptSize,
            .title = title,
            .width = width,
            .pts = LinqAPI.Exec(Of PointData) <=
 _
                From o As SeqValue(Of Double)
                In y0.SeqIterator
                Where Not o.obj.IsNaNImaginary
                Select New PointData With {
                    .pt = New PointF(array(o.i), CSng(o.obj))
                }
                    }
    End Function

    <Extension>
    Public Function FromODE(ode As ODE, color As String,
                            Optional dash As DashStyle = DashStyle.Dash,
                            Optional ptSize As Integer = 30,
                            Optional width As Single = 5) As SerialData

        Return New SerialData With {
            .title = ode.Id,
            .color = color.ToColor,
            .lineType = dash,
            .PointSize = ptSize,
            .width = width,
            .pts = LinqAPI.Exec(Of PointData) <=
                From x As SeqValue(Of Double)
                In ode.x.SeqIterator
                Select New PointData(CSng(x.obj), CSng(ode.y(x.i)))
        }
    End Function

    ''' <summary>
    ''' Convert ODEs result as scatter plot serial model.
    ''' </summary>
    ''' <param name="odes"></param>
    ''' <param name="colors"></param>
    ''' <param name="ptSize!"></param>
    ''' <param name="width"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FromODEs(odes As ODEsOut,
                             Optional colors As IEnumerable(Of String) = Nothing,
                             Optional ptSize! = 30,
                             Optional width As Single = 5) As SerialData()
        Dim c As Color() = If(
            colors.IsNullOrEmpty,
            ChartColors.Shuffles,
            colors.ToArray(AddressOf ToColor))

        Return LinqAPI.Exec(Of SerialData) <=
 _
            From y As SeqValue(Of NamedValue(Of Double()))
            In odes.y.Values.SeqIterator
            Let pts As PointData() = odes.x _
                .SeqIterator _
                .ToArray(Function(x) New PointData(CSng(+x), CSng(y.obj.Value(x))))
            Select New SerialData With {
                .color = c(y.i),
                .lineType = DashStyle.Solid,
                .PointSize = ptSize,
                .title = y.obj.Name,
                .width = width,
                .pts = pts
            }
    End Function

    <Extension>
    Public Function Plot(range As NamedValue(Of DoubleRange),
                         expression$,
                         Optional steps# = 0.01,
                         Optional lineColor$ = "black",
                         Optional lineWidth! = 10,
                         Optional bg$ = "white") As Bitmap

        Dim engine As New Expression
        Dim ranges As Double() = range.Value.seq(steps).ToArray
        Dim y As New List(Of Double)

        For Each x# In ranges
            Call engine _
                .SetVariable(range.Name, x)
            y += engine.Evaluation(expression)
        Next

        Dim serial As SerialData = FromVector(y, lineColor,,, lineWidth, ranges, expression,)
        Return Plot({serial}, ,, bg)
    End Function

    <Extension>
    Public Function Plot(range As DoubleRange,
                         expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional lineColor$ = "black",
                         Optional lineWidth! = 10,
                         Optional bg$ = "white",
                         Optional title$ = "Function Plot") As Bitmap

        Dim ranges As Double() = range.seq(steps).ToArray
        Dim y As New List(Of Double)

        For Each x# In ranges
            y += expression(x#)
        Next

        Dim serial As SerialData = FromVector(y, lineColor,,, lineWidth, ranges, title,)
        Return Plot({serial}, ,, bg)
    End Function

    Public Function Plot(points As IEnumerable(Of Point),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional lineColor$ = "black",
                         Optional bg$ = "white",
                         Optional title$ = "Plot Of Points",
                         Optional lineWidth! = 5.0!,
                         Optional ptSize! = 15.0!,
                         Optional lineType As DashStyle = DashStyle.Solid) As Bitmap
        Dim s As SerialData = points _
            .FromPoints(lineColor$,
                        title$,
                        lineWidth!,
                        ptSize!,
                        lineType)
        Return {s}.Plot(size:=size, margin:=margin, bg:=bg)
    End Function

    Public Function Plot(points As IEnumerable(Of PointF),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional lineColor$ = "black",
                         Optional bg$ = "white",
                         Optional title$ = "Plot Of Points",
                         Optional lineWidth! = 5.0!,
                         Optional ptSize! = 15.0!,
                         Optional lineType As DashStyle = DashStyle.Solid) As Bitmap
        Dim s As SerialData = points _
            .FromPoints(lineColor$,
                        title$,
                        lineWidth!,
                        ptSize!,
                        lineType)
        Return {s}.Plot(size:=size, margin:=margin, bg:=bg)
    End Function

    <Extension>
    Public Function FromPoints(points As IEnumerable(Of Point),
                               Optional lineColor$ = "black",
                               Optional title$ = "Plot Of Points",
                               Optional lineWidth! = 5.0!,
                               Optional ptSize! = 15.0!,
                               Optional lineType As DashStyle = DashStyle.Solid) As SerialData
        Return FromPoints(
            points.Select(
            Function(pt) New PointF With {
                .X = pt.X,
                .Y = pt.Y
            }),
            lineColor,
            title,
            lineWidth,
            ptSize,
            lineType)
    End Function

    <Extension>
    Public Function FromPoints(points As IEnumerable(Of PointF),
                               Optional lineColor$ = "black",
                               Optional title$ = "Plot Of Points",
                               Optional lineWidth! = 5.0!,
                               Optional ptSize! = 15.0!,
                               Optional lineType As DashStyle = DashStyle.Solid) As SerialData

        Return New SerialData With {
            .color = lineColor.ToColor,
            .lineType = lineType,
            .PointSize = ptSize,
            .width = lineWidth,
            .pts = points.ToArray(
                Function(pt) New PointData With {
                    .pt = pt
            }),
            .title = title
        }
    End Function
End Module
