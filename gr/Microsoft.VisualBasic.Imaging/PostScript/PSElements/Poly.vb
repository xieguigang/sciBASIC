#Region "Microsoft.VisualBasic::537ac7b94170e12aa9b2095a2cb98ee0, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Poly.vb"

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

    '   Total Lines: 57
    '    Code Lines: 42 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (26.32%)
    '     File Size: 1.74 KB


    '     Class Polygon
    ' 
    '         Properties: fill, points, stroke
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
            Throw New NotImplementedException()
        End Sub
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

            Call g.DrawClosedCurve(g.LoadEnvironment.GetPen(stroke), points)
        End Sub
    End Class
End Namespace
