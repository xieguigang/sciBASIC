#Region "Microsoft.VisualBasic::47a4b23d9e8b1268fcd33035bd2cb2c2, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Poly.vb"

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

    '   Total Lines: 90
    '    Code Lines: 72 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (20.00%)
    '     File Size: 3.10 KB


    '     Class Polygon
    ' 
    '         Properties: fill, points, stroke
    ' 
    '         Function: GetSize, GetXy, ScaleTo
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    '     Class Poly
    ' 
    '         Properties: closedPath
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Polygon : Inherits PSElement

        Public Property points As PointF()
        Public Property stroke As Stroke
        Public Property fill As String

        Friend Overrides Sub WriteAscii(ps As Writer)
            Throw New NotImplementedException()
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            If Not fill.StringEmpty(, True) Then
                Call g.FillPolygon(fill.GetBrush, points)
            End If
            If stroke IsNot Nothing Then
                Call g.DrawPolygon(g.LoadEnvironment.GetPen(stroke), points)
            End If
        End Sub

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Return New Polygon With {
                .comment = comment,
                .fill = fill,
                .stroke = stroke,
                .points = points _
                    .SafeQuery _
                    .Select(Function(i)
                                Return New PointF(scaleX(i.X), scaleY(i.Y))
                            End Function) _
                    .ToArray
            }
        End Function

        Friend Overrides Function GetXy() As PointF
            Return New PointF(points.Average(Function(p) p.X), points.Average(Function(p) p.Y))
        End Function

        Friend Overrides Function GetSize() As SizeF
            Dim x As New List(Of Single)
            Dim y As New List(Of Single)

            Return New SizeF(New DoubleRange(x).Length, New DoubleRange(y).Length)
        End Function
    End Class

    Public Class Poly : Inherits Polygon

        Public Property closedPath As Boolean = True

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim pen As Pen = ps.pen(stroke)
            Dim points As PointF() = Me.points

            If points Is Nothing OrElse points.Length < 3 Then
                Throw New ArgumentException("At least 3 data points is required for draw a closed shape!")
            End If

            Call ps.color(pen.Color)
            Call ps.linewidth(pen.Width)
            Call ps.moveto(points(0))

            For i As Integer = 1 To points.Length - 1
                Call ps.lineto(points(i).X, points(i).Y)
            Next

            If closedPath Then
                Call ps.closepath()
            End If

            Call ps.stroke()
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            If Not fill.StringEmpty(, True) Then
                Call g.FillClosedCurve(fill.GetBrush, points)
            End If
            If stroke IsNot Nothing Then
                Call g.DrawClosedCurve(g.LoadEnvironment.GetPen(stroke), points)
            End If
        End Sub
    End Class
End Namespace
