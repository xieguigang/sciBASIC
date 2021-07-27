#Region "Microsoft.VisualBasic::34b237b3e3952bafd66c908122423afb, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\GeneralPath.vb"

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

    '     Class GeneralPath
    ' 
    '         Properties: dimension, level
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetContour, GetPolygons, ToString
    ' 
    '         Sub: ClosePath, Draw, Fill, LineTo, MoveTo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class GeneralPath

        Dim polygons As New List(Of PointF())
        Dim temp As New List(Of PointF)

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
            For Each polygon In GetPolygons(scaleX, scaleY)
                Call canvas.FillPolygon(color, polygon)
            Next
        End Sub

        Public Sub Draw(canvas As IGraphics, border As Pen, scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale)
            For Each polygon In GetPolygons(scaleX, scaleY)
                Call canvas.DrawPolygon(border, polygon)
            Next
        End Sub

        Friend Sub ClosePath()
            polygons.Add(temp.PopAll)
        End Sub
    End Class
End Namespace
