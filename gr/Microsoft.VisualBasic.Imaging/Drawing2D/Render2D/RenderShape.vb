#Region "Microsoft.VisualBasic::1f6121c9ace59a780d3c7ee1a44bb749, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Render2D\RenderShape.vb"

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

    '   Total Lines: 33
    '    Code Lines: 27 (81.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1023 B


    '     Class RenderShape
    ' 
    '         Properties: fill, pen, shape
    ' 
    '         Sub: RenderBoid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Drawing2D

    Public Class RenderShape

        Public Property pen As Pen
        Public Property fill As Brush
        Public Property shape As GraphicsPath

        Public Shared Narrowing Operator CType(s As RenderShape) As GraphicsPath
            Return s.shape
        End Operator

        Shared ReadOnly boidArrow As Point() = New Point() {
            New Point(0, 0), New Point(-4, -1),
            New Point(0, 8),
            New Point(4, -1), New Point(0, 0)
        }

#Disable Warning
        Public Shared Sub RenderBoid(gfx As Graphics, x As Single, y As Single, angle As Single, color As Color)
            Using brush = New SolidBrush(color)
                gfx.TranslateTransform(x, y)
                gfx.RotateTransform(angle)
                gfx.FillClosedCurve(brush, boidArrow)
                gfx.ResetTransform()
            End Using
        End Sub
#Enable Warning
    End Class
End Namespace
