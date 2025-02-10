#Region "Microsoft.VisualBasic::8b12b62b1875f35258c7bcf44918d0f4, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Rectangle.vb"

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

    '   Total Lines: 34
    '    Code Lines: 25 (73.53%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (26.47%)
    '     File Size: 1020 B


    '     Class Rectangle
    ' 
    '         Constructor: (+3 Overloads) Sub New
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
            Call g.DrawRectangle(g.LoadEnvironment.GetPen(shape.border), shape.DrawingRegion)
        End Sub
    End Class
End Namespace
