#Region "Microsoft.VisualBasic::7085476f7f5e7707085f4bf204557d64, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\GeneralPath.vb"

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

    '   Total Lines: 118
    '    Code Lines: 93 (78.81%)
    ' Comment Lines: 4 (3.39%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 21 (17.80%)
    '     File Size: 3.90 KB


    '     Class GeneralPath
    ' 
    '         Properties: dimension, level
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: AddPolygon, GetContour, GetPolygons, ToString
    ' 
    '         Sub: ClosePath, Discard, Draw, Fill, LineTo
    '              MoveTo, warning
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Interpolation

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' a collection of the polygon data
    ''' </summary>
    Public Class GeneralPath

        Friend ReadOnly polygons As New List(Of PointF())
        Friend ReadOnly temp As New List(Of PointF)

        Public ReadOnly Property level As Double
        Public Property dimension As Size

        Sub New(level As Double)
            Me.level = level
        End Sub

        Sub New(contour As ContourLayer)
            level = contour.threshold
            polygons = contour.shapes _
                .Select(Function(p) p.ToArray) _
                .AsList
        End Sub

        Public Function GetContour() As ContourLayer
            Return New ContourLayer With {
                .threshold = level,
                .shapes = polygons _
                    .Select(Function(list)
                                Return New Polygon2D With {
                                    .x = list.Select(Function(p) CInt(p.X)).ToArray,
                                    .y = list.Select(Function(p) CInt(p.Y)).ToArray
                                }
                            End Function) _
                    .ToArray
            }
        End Function

        Public Function AddPolygon(poly As IEnumerable(Of PointF)) As GeneralPath
            polygons.Add(poly.ToArray)
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"{polygons.Count} polygons under threshold {level}"
        End Function

        Friend Sub MoveTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Friend Sub LineTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Public Iterator Function GetPolygons(Optional scaleX As d3js.scale.LinearScale = Nothing, Optional scaleY As d3js.scale.LinearScale = Nothing) As IEnumerable(Of PointF())
            If scaleX Is Nothing OrElse scaleY Is Nothing Then
                For Each raw In polygons
                    Yield raw
                Next
            Else
                For Each raw In polygons
                    Yield raw.Select(Function(p) New PointF(scaleX(p.X), scaleY(p.Y))).ToArray
                Next
            End If
        End Function

        Public Sub Fill(canvas As IGraphics, color As Brush, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim i As Integer = 0

            For Each polygon In GetPolygons(scaleX, scaleY)
                If polygon.Length < 3 Then
                    i += 1
                Else
                    Call canvas.FillPolygon(color, polygon)
                End If
            Next

            If i > 0 Then
                Call warning(i)
            End If
        End Sub

        Private Sub warning(count As Integer)
            Call $"missing {count} polygon!".Warning
        End Sub

        Public Sub Draw(canvas As IGraphics, border As Pen, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            Dim i As Integer = 0

            For Each polygon In GetPolygons(scaleX, scaleY)
                If polygon.Length < 3 Then
                    ' do warning?
                    i += 1
                Else
                    Call canvas.DrawPolygon(border, polygon)
                End If
            Next

            If i > 0 Then
                Call warning(i)
            End If
        End Sub

        ''' <summary>
        ''' clear current temp data
        ''' </summary>
        Public Sub Discard()
            Call temp.Clear()
        End Sub

        Friend Sub ClosePath()
            If temp > 0 Then
                Call polygons.Add(temp.PopAll)
            End If
        End Sub

        Public Function FilterSmallPolygon(q As Double) As GeneralPath
            Dim max_size As Integer = polygons.Select(Function(g) g.Length).Max

        End Function

        Public Function Cubic(Optional resolution As Integer = 1000) As GeneralPath
            Dim smooth As New List(Of PointF())
            Dim path As New GeneralPath(level) With {.dimension = dimension}

            For Each polygon As PointF() In polygons
                polygon = polygon.CubicSpline(resolution)
                path.AddPolygon(polygon)
            Next

            Return path
        End Function

        Public Function Bspline(Optional degree As Integer = 2, Optional resolution As Integer = 10) As GeneralPath
            Dim smooth As New List(Of PointF())
            Dim path As New GeneralPath(level) With {.dimension = dimension}

            For Each polygon As PointF() In polygons
                polygon = B_Spline.BSpline(polygon, degree, resolution).ToArray
                path.AddPolygon(polygon)
            Next

            Return path
        End Function
    End Class
End Namespace
