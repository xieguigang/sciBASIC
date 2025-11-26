#Region "Microsoft.VisualBasic::15f36be002093f806a54ef0a62c0c0e8, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Render2D\RenderShape.vb"

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

    '   Total Lines: 149
    '    Code Lines: 94 (63.09%)
    ' Comment Lines: 33 (22.15%)
    '    - Xml Docs: 54.55%
    ' 
    '   Blank Lines: 22 (14.77%)
    '     File Size: 6.16 KB


    '     Class RenderShape
    ' 
    '         Properties: fill, pen, shape
    ' 
    '         Sub: DrawArrowHead, (+2 Overloads) RenderBoid, RenderForceVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Net
Imports std = System.Math

#If NET48 Then
Imports System.Drawing.Drawing2D
#End If

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

        Public Shared Sub RenderForceVector(g As IGraphics, startPoint As PointF, vector As PointF, color As Color,
                                            Optional scale As Single = 5,
                                            Optional w As Single = 3)
            ' 应用缩放
            Dim scaledFx As Single = vector.X * scale
            Dim scaledFy As Single = vector.Y * scale

            ' 计算箭头终点
            Dim endPoint As PointF = New PointF(startPoint.X + scaledFx, startPoint.Y + scaledFy)
            ' 定义箭头样式
            Dim arrowThickness As Integer = w
            Using arrowPen As New Pen(color, arrowThickness)
                ' 绘制箭头主体（一条直线）
                g.DrawLine(arrowPen, startPoint, endPoint)

                ' 计算并绘制箭头头部
                DrawArrowHead(g, arrowPen, startPoint, endPoint)
            End Using
        End Sub

        ''' <summary>
        ''' 在指定终点绘制箭头头部
        ''' </summary>
        Private Shared Sub DrawArrowHead(g As IGraphics, pen As Pen, startPoint As PointF, endPoint As PointF)
            Dim dx As Single = endPoint.X - startPoint.X
            Dim dy As Single = endPoint.Y - startPoint.Y

            ' 如果矢量长度为0，则不绘制箭头
            If dx = 0 AndAlso dy = 0 Then Return

            Dim angle As Single = std.Atan2(dy, dx) ' 计算主线角度（弧度）
            Dim headLength As Single = 15.0F          ' 箭头头部的长度
            Dim headAngle As Single = 25 * std.PI / 180.0F ' 箭头张角的一半（弧度）

            ' 计算箭头头部两个侧翼点的坐标
            Dim p1 As PointF = New PointF(
            endPoint.X - headLength * std.Cos(angle - headAngle),
            endPoint.Y - headLength * std.Sin(angle - headAngle)
        )

            Dim p2 As PointF = New PointF(
            endPoint.X - headLength * std.Cos(angle + headAngle),
            endPoint.Y - headLength * std.Sin(angle + headAngle)
        )

            ' 绘制箭头的两条侧翼线
            g.DrawLine(pen, endPoint, p1)
            g.DrawLine(pen, endPoint, p2)
        End Sub

        ''' <summary>
        ''' Arrow drawing for the .net core console application
        ''' </summary>
        ''' <param name="maxL">max length of the force</param>
        ''' <param name="w">
        ''' the pen width
        ''' </param>
        Public Shared Sub RenderBoid(gfx As IGraphics, x As Single, y As Single, vx As Single, vy As Single, color As Color,
                                     Optional maxL As Single = 8,
                                     Optional w As Single = 3)
            ' 计算矢量的原始长度
            Dim originalLength As Double = std.Sqrt(vx * vx + vy * vy)
            Dim pen As New Pen(color, w)
            ' 确定缩放比例和实际终点坐标
            Dim scaleFactor As Double

            If originalLength > maxL Then
                scaleFactor = maxL / originalLength
            Else
                scaleFactor = 1.0
            End If

            Dim scaledVx As Double = vx * scaleFactor
            Dim scaledVy As Double = vy * scaleFactor
            Dim endX As Double = x + scaledVx
            Dim endY As Double = y + scaledVy

            ' 1. 绘制箭头主线
            gfx.DrawLine(pen, x, y, CSng(endX), CSng(endY))

            ' 2. 绘制箭头头部（三角形）
            Dim headLength As Double = 10.0 ' 箭头头部长度，可根据需要调整
            Dim headWidth As Double = 7.0   ' 箭头头部宽度，可根据需要调整
            ' 计算矢量的角度（弧度）
            Dim angle As Double = std.Atan2(scaledVy, scaledVx)
            ' 计算箭头头部的两个侧点
            Dim headPoint1 As New PointF(
                CSng(endX - headLength * std.Cos(angle) + headWidth * std.Sin(angle)),
                CSng(endY - headLength * std.Sin(angle) - headWidth * std.Cos(angle))
            )
            Dim headPoint2 As New PointF(
                CSng(endX - headLength * std.Cos(angle) - headWidth * std.Sin(angle)),
                CSng(endY - headLength * std.Sin(angle) + headWidth * std.Cos(angle))
            )

            ' 绘制头部三角形（从终点连接到两个侧点）
            gfx.DrawLine(pen, CSng(endX), CSng(endY), CSng(headPoint1.X), CSng(headPoint1.Y))
            gfx.DrawLine(pen, CSng(endX), CSng(endY), CSng(headPoint2.X), CSng(headPoint2.Y))
            gfx.DrawLine(pen, CSng(headPoint1.X), CSng(headPoint1.Y), CSng(headPoint2.X), CSng(headPoint2.Y))
        End Sub
    End Class
End Namespace
