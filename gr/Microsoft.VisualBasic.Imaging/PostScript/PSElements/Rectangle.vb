#Region "Microsoft.VisualBasic::3a2c7f17a7800fa06736f4685b007b58, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Rectangle.vb"

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

    '   Total Lines: 58
    '    Code Lines: 41 (70.69%)
    ' Comment Lines: 5 (8.62%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (20.69%)
    '     File Size: 2.16 KB


    '     Class Rectangle
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: ScaleTo, ToString
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace PostScript.Elements

    Public Class Rectangle : Inherits PSElement(Of Box)

        Sub New()
        End Sub

        Sub New(rect As System.Drawing.Rectangle, color As Color)
            shape = New Box(rect, color)
        End Sub

        Sub New(rect As RectangleF, color As Color)
            shape = New Box(rect, color)
        End Sub

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim rect As RectangleF = shape.DrawingRegion

            Call ps.rectangle(rect, shape.fill, False)

            If shape.border IsNot Nothing Then
                Call ps.rectangle(rect, shape.border.fill, True)
            End If
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Dim rect = shape.DrawingRegion

            If Not shape.border Is Nothing Then
                Dim pen = g.LoadEnvironment.GetPen(shape.border)
                Call g.DrawRectangle(pen, rect)
            Else
                ' 20250314
                ' draw rectangle of the fill color always black
                ' so if border is not nothing
                ' then it means is draw rectangle action
                ' skip of fill rectangle or the rectangle black always
                Call g.FillRectangle(shape.fill.GetBrush, rect)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return $"({shape.Location.X.ToString("F1")},{shape.Location.Y.ToString("F1")}) rectangle(fill={shape.fill}, size=[{shape.Size.Width.ToString("F1")},{shape.Size.Height.ToString("F1")}])"
        End Function

        Friend Overrides Function ScaleTo(scaleX As d3js.scale.LinearScale, scaleY As d3js.scale.LinearScale) As PSElement
            Dim box = shape.DrawingRegion
            box = New RectangleF(scaleX(box.X), scaleY(box.Y), scaleX(box.Width), scaleY(box.Height))
            Dim rect As New Rectangle(box, shape.fill.TranslateColor)
            rect.shape.border = shape.border
            rect.comment = comment
            Return rect
        End Function

        Friend Overrides Function GetXy() As PointF
            Return shape.Location
        End Function

        Friend Overrides Function GetSize() As SizeF
            Return shape.Size
        End Function
    End Class
End Namespace
