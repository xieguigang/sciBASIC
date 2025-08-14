#Region "Microsoft.VisualBasic::58c3f7269345d7cfa1e09d15d4d3882e, Library\graphics\Plot2D\FillPolygons.vb"

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

    '   Total Lines: 104
    '    Code Lines: 88 (84.62%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (15.38%)
    '     File Size: 4.29 KB


    ' Class FillPolygons
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
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

Public Class PolygonGroup

    Public Property label As String
    Public Property subregions As Polygon2D()

    Public ReadOnly Property size As Integer
        Get
            Return subregions.TryCount
        End Get
    End Property

End Class

Public Class FillPolygons : Inherits Plot

    Dim polygons As (color As Color, regions As Polygon2D())()
    Dim dims As SizeF
    Dim union As Polygon2D
    Dim scatter As Boolean

    Public Sub New(polygonGroups As PolygonGroup(), scatter As Boolean, theme As Theme)
        Call MyBase.New(theme)

        Dim colors As New LoopArray(Of Color)(Designer.GetColors(theme.colorSet))

        Me.scatter = scatter
        Me.polygons = polygonGroups.Select(Function(r) (++colors, r.subregions)).ToArray
        Me.union = New Polygon2D(polygons.Select(Function(r) r.regions).IteratesALL.ToArray)
        Me.dims = union.GetRectangle.Size
    End Sub

    Public Sub New(polygons As Polygon2D(), scatter As Boolean, theme As Theme)
        MyBase.New(theme)

        Dim colors As New LoopArray(Of Color)(Designer.GetColors(theme.colorSet))

        Me.scatter = scatter
        Me.polygons = polygons.Select(Function(r) (++colors, {r})).ToArray
        Me.union = New Polygon2D(polygons)
        Me.dims = union.GetRectangle.Size
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim xTicks = union.xpoints.CreateAxisTicks
        Dim yTicks = union.ypoints.CreateAxisTicks
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim x = d3js.scale.linear.domain(values:=xTicks).range(values:=canvas.GetXLinearScaleRange(css))
        Dim y = d3js.scale.linear.domain(values:=yTicks).range(values:=canvas.GetYLinearScaleRange(css))
        Dim scaler As New DataScaler(rev:=True) With {
            .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
            .region = canvas.PlotRegion(css),
            .X = x,
            .Y = y
        }

        If theme.drawAxis Then
            Call Axis.DrawAxis(g, canvas, scaler, xlabel, ylabel, theme)
        End If

        For Each tuple As (color As Color, regions As Polygon2D()) In polygons
            For Each polygon In tuple.regions
                Dim fill As Color = tuple.color

                If scatter Then
                    Call g.FillCircles(New SolidBrush(fill), polygon.AsEnumerable.Select(Function(i) scaler.Translate(i)).ToArray, 2)
                Else
                    Dim path As New GraphicsPath
                    Dim start = scaler.Translate(polygon.AsEnumerable.First)

                    For Each pt As PointF In polygon.AsEnumerable.Skip(1).Select(AddressOf scaler.Translate)
                        path.AddLine(start, pt)
                        start = pt
                    Next

                    Call path.AddLine(start, scaler.Translate(polygon.AsEnumerable.First))
                    Call path.CloseFigure()
                    Call g.FillPath(New SolidBrush(fill), path)
                End If
            Next
        Next
    End Sub
End Class
