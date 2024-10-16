#Region "Microsoft.VisualBasic::d31253972c479b77683e89c4c6f68412, Data_science\Visualization\Plots\Scatter\Plot\PolygonPlot2D.vb"

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

    '   Total Lines: 140
    '    Code Lines: 124 (88.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (11.43%)
    '     File Size: 6.07 KB


    '     Class PolygonPlot2D
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateSerial
    ' 
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

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

Namespace Plots

    Public Class PolygonPlot2D : Inherits Plot

        ReadOnly polygons As SerialData()
        ReadOnly reverse As Boolean = False

        Public Sub New(data As IEnumerable(Of SerialData), theme As Theme)
            MyBase.New(theme)

            polygons = data.ToArray
        End Sub

        Sub New(data As IEnumerable(Of GeneralPath), theme As Theme, Optional names As String() = Nothing, Optional reverse As Boolean = False)
            Call Me.New(CreateSerial(data.ToArray, theme, names), theme)

            Me.reverse = reverse
        End Sub

        Private Shared Iterator Function CreateSerial(data As GeneralPath(), theme As Theme, names As String()) As IEnumerable(Of SerialData)
            Dim colors As Color() = Designer.GetColors(theme.colorSet, data.Length)
            Dim i As Integer = 0
            Dim titleName As String

            For Each path As GeneralPath In data
                If names.IsNullOrEmpty OrElse names.ElementAtOrDefault(i).StringEmpty Then
                    titleName = colors(i).ToHtmlColor
                Else
                    titleName = names(i)
                End If

                For Each part As PointF() In path.GetPolygons
                    Yield New SerialData With {
                        .color = colors(i),
                        .title = titleName,
                        .lineType = DashStyle.Solid,
                        .pointSize = 5,
                        .shape = LegendStyles.Circle,
                        .width = 2,
                        .pts = part _
                            .Select(Function(p)
                                        Return New PointData With {
                                            .pt = p
                                        }
                                    End Function) _
                            .ToArray
                    }
                Next

                i += 1
            Next
        End Function

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim xTicks As Double() = polygons.Select(Function(p) p.pts.Select(Function(pi) CDbl(pi.pt.X))).IteratesALL.CreateAxisTicks
            Dim yTicks As Double() = polygons.Select(Function(p) p.pts.Select(Function(pi) CDbl(pi.pt.Y))).IteratesALL.CreateAxisTicks
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim rect = canvas.PlotRegion(css)
            Dim xscale = d3js.scale.linear.domain(values:=xTicks).range(values:=New Double() {rect.Left, rect.Right})
            Dim yscale = d3js.scale.linear.domain(values:=yTicks).range(values:=New Double() {rect.Top, rect.Bottom})
            Dim shape As PointF()
            Dim scale As New DataScaler() With {
                .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
                .region = rect,
                .X = xscale,
                .Y = yscale
            }
            Dim stroke As Stroke = Stroke.TryParse(theme.lineStroke)
            Dim pen As Pen = css.GetPen(stroke)

            Call Axis.DrawAxis(
                g, canvas, scale,
                showGrid:=theme.drawGrid,
                xlabel:=xlabel,
                ylabel:=ylabel,
                gridFill:=theme.gridFill
            )

            For Each polygon As SerialData In polygons
                shape = polygon.pts _
                    .Select(Function(p)
                                If reverse Then
                                    Return New PointF With {
                                        .X = scale.TranslateX(p.pt.X),
                                        .Y = canvas.Height - scale.TranslateY(p.pt.Y)
                                    }
                                Else
                                    Return scale.Translate(p.pt)
                                End If
                            End Function) _
                    .ToArray

                Call g.FillPolygon(New SolidBrush(polygon.color), shape)
                Call g.DrawPolygon(pen, shape)
            Next
        End Sub
    End Class
End Namespace
