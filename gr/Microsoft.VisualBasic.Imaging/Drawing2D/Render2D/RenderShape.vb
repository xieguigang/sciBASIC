#Region "Microsoft.VisualBasic::ad7edac65630bd7f89258ac00031abad, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Render2D\RenderShape.vb"

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

    '   Total Lines: 82
    '    Code Lines: 51 (62.20%)
    ' Comment Lines: 18 (21.95%)
    '    - Xml Docs: 61.11%
    ' 
    '   Blank Lines: 13 (15.85%)
    '     File Size: 2.90 KB


    '     Class RenderShape
    ' 
    '         Properties: fill, pen, shape
    ' 
    '         Sub: (+2 Overloads) RenderBoid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports std = System.Math

Namespace Drawing2D

    Public Class RenderShape

        Public Property pen As Pen
        Public Property fill As Brush
        Public Property shape As GraphicsPath

        Public Shared Narrowing Operator CType(s As RenderShape) As GraphicsPath
            Return s.shape
        End Operator

#If NET48 Then
        Shared ReadOnly boidArrow As Point() = New Point() {
            New Point(0, 0), New Point(-4, -1),
            New Point(0, 8),
            New Point(4, -1), New Point(0, 0)
        }

#Disable Warning
        ''' <summary>
        ''' Arrow drawing for the winform application
        ''' </summary>
        ''' <param name="gfx"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="angle"></param>
        ''' <param name="color"></param>
        Public Shared Sub RenderBoid(gfx As Graphics, x As Single, y As Single, angle As Single, color As Color)
            Using brush = New SolidBrush(color)
                gfx.TranslateTransform(x, y)
                gfx.RotateTransform(angle)
                gfx.FillClosedCurve(brush, boidArrow)
                gfx.ResetTransform()
            End Using
        End Sub
#Enable Warning
#End If

        ''' <summary>
        ''' Arrow drawing for the .net core console application
        ''' </summary>
        Public Shared Sub RenderBoid(gfx As IGraphics, x As Single, y As Single, vx As Single, vy As Single, color As Color,
                                     Optional l As Single = 8,
                                     Optional w As Single = 3)
            ' 计算单位方向向量
            Dim length As Single = std.Sqrt(vx * vx + vy * vy)
            Dim ux As Single = vx / length
            Dim uy As Single = vy / length

            ' 计算箭头的终点
            Dim endX As Single = x + l * ux
            Dim endY As Single = y + l * uy

            ' 定义箭头的头部大小
            Dim arrowHeadSize As Single = 10

            ' 计算箭头两翼的顶点
            Dim wing1X As Single = endX - arrowHeadSize * uy
            Dim wing1Y As Single = endY + arrowHeadSize * ux
            Dim wing2X As Single = endX - arrowHeadSize * uy
            Dim wing2Y As Single = endY - arrowHeadSize * ux

            ' 绘制箭身
            Call gfx.DrawLine(Pens.Black, x, y, endX, endY)

            ' 定义三角形的三个顶点
            Dim points As PointF() = {
                New PointF(endX, endY),
                New PointF(wing1X, wing1Y),
                New PointF(wing2X, wing2Y)
            }

            ' 绘制实心黑色三角形作为箭头
            Call gfx.FillPolygon(Brushes.Black, points)
        End Sub
    End Class
End Namespace
