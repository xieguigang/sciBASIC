#Region "Microsoft.VisualBasic::7d4ea742408a44b8263061d20daf764e, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Line.vb"

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

    '   Total Lines: 59
    '    Code Lines: 46 (77.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (22.03%)
    '     File Size: 1.62 KB


    '     Class Line
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Html.Render

#If NETCOREAPP Then
#Else
Imports System.Drawing.Drawing2D
#End If

Namespace PostScript.Elements

    Public Class Line : Inherits PSElement(Of Shapes.Line)

        Sub New()
        End Sub

        Sub New(stroke As Pen, a As PointF, b As PointF)
            shape = New Shapes.Line(stroke, a, b)
        End Sub

        Sub New(stroke As Pen, a As Point, b As Point)
            shape = New Shapes.Line(stroke, a, b)
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim a = shape.A
            Dim b = shape.B
            Dim pen As Pen = ps.pen(shape.Stroke)
            Dim color As Color = shape.Stroke.fill.TranslateColor

            If pen.DashStyle <> DashStyle.Solid Then
                ps.dash({2, 3})
            Else
                ps.dash(Nothing)
            End If
            If color.A <> 255 Then
                ps.transparency(color.A / 255)
                ps.beginTransparent()
            End If

            Call ps.linewidth(pen.Width)
            Call ps.color(color)

            Call ps.line(a.X, a.Y, b.X, b.Y)

            If color.A <> 255 Then
                ps.endTransparent()
            End If
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Dim a = shape.A
            Dim b = shape.B
            Dim pen As Pen = g.LoadEnvironment.GetPen(shape.Stroke)

            Call g.DrawLine(pen, a, b)
        End Sub
    End Class
End Namespace
