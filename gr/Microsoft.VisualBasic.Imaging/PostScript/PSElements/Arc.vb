#Region "Microsoft.VisualBasic::2ae98d6c966a89573901ddbdd09861ac, gr\Microsoft.VisualBasic.Imaging\PostScript\PSElements\Arc.vb"

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

    '   Total Lines: 43
    '    Code Lines: 36 (83.72%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.73 KB


    '     Class Arc
    ' 
    '         Properties: height, startAngle, stroke, sweepAngle, width
    '                     x, y
    ' 
    '         Sub: Paint, WriteAscii
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

#If NET48 Then
Imports System.Drawing
#End If

Namespace PostScript.Elements

    Public Class Arc : Inherits PSElement

        Public Property stroke As Stroke
        Public Property x As Single
        Public Property y As Single
        Public Property width As Single
        Public Property height As Single
        Public Property startAngle As Single
        Public Property sweepAngle As Single

        Friend Overrides Sub WriteAscii(ps As Writer)
            Dim startAngleRad As Double = startAngle * (std.PI / 180)
            Dim sweepAngleRad As Double = sweepAngle * (std.PI / 180)
            Dim startX As Double = x + (width / 2) + std.Cos(startAngleRad) * (width / 2)
            Dim startY As Double = y + (height / 2) + std.Sin(startAngleRad) * (height / 2)
            Dim endX As Double = x + (width / 2) + std.Cos(startAngleRad + sweepAngleRad) * (width / 2)
            Dim endY As Double = y + (height / 2) + std.Sin(startAngleRad + sweepAngleRad) * (height / 2)
            Dim pen As Pen = ps.pen(stroke)

            Call ps.linewidth(pen.Width)
            Call ps.color(pen.Color)
            Call ps.moveto(startX, startY)
            Call ps.arct(x, y, width, height, startAngle, sweepAngle)
            Call ps.stroke()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Overrides Sub Paint(g As IGraphics)
            Call g.DrawArc(g.LoadEnvironment.GetPen(stroke), x, y, width, height, startAngle, sweepAngle)
        End Sub
    End Class
End Namespace
