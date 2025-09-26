#Region "Microsoft.VisualBasic::71928c218dca0e4777dc47e8e8944840, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Text\Nudge\SimpleNudge.vb"

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

    '   Total Lines: 98
    '    Code Lines: 53 (54.08%)
    ' Comment Lines: 28 (28.57%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 17 (17.35%)
    '     File Size: 4.20 KB


    '     Class SimpleNudge
    ' 
    '         Function: CalculateAdjustment, CheckOverlap, ReduceOverlap
    ' 
    '         Sub: ApplyAdjustment
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports std = System.Math

Namespace Drawing2D.Text.Nudge

    Public Class SimpleNudge

        ''' <summary>
        ''' 减少标签重叠的主优化方法
        ''' </summary>
        ''' <param name="labels">待优化的标签列表</param>
        ''' <param name="maxIterations">最大迭代次数</param>
        ''' <param name="minDistance">最小允许距离</param>
        ''' <returns>优化后的标签列表</returns>
        Public Shared Function ReduceOverlap(labels As List(Of Label), Optional maxIterations As Integer = 50, Optional minDistance As Single = 5.0F) As List(Of Label)
            Dim optimizedLabels = New List(Of Label)(labels)
            Dim iteration = 0

            While iteration < maxIterations
                Dim hasOverlap = False

                For i = 0 To optimizedLabels.Count - 1
                    ' 跳过已固定的标签[1](@ref)
                    If optimizedLabels(i).pinned Then Continue For

                    For j = i + 1 To optimizedLabels.Count - 1
                        ' 检查两个标签是否重叠[6](@ref)
                        If CheckOverlap(optimizedLabels(i), optimizedLabels(j)) Then
                            hasOverlap = True
                            ' 计算调整向量
                            Dim adjustment = CalculateAdjustment(optimizedLabels(i), optimizedLabels(j), minDistance)
                            ' 应用位置调整
                            ApplyAdjustment(optimizedLabels(i), adjustment)
                        End If
                    Next
                Next

                iteration += 1
                ' 如果没有检测到重叠，提前结束
                If Not hasOverlap Then Exit While
            End While

            Return optimizedLabels
        End Function

        ''' <summary>
        ''' 检查两个标签是否重叠
        ''' </summary>
        Private Shared Function CheckOverlap(label1 As Label, label2 As Label) As Boolean
            ' 获取两个标签的矩形区域[6](@ref)
            Dim rect1 As New RectangleF(CSng(label1.X), CSng(label1.Y), CSng(label1.width), CSng(label1.height))
            Dim rect2 As New RectangleF(CSng(label2.X), CSng(label2.Y), CSng(label2.width), CSng(label2.height))

            ' 检查矩形是否相交[6](@ref)
            Return rect1.IntersectsWith(rect2)
        End Function

        ''' <summary>
        ''' 计算需要调整的方向和距离
        ''' </summary>
        Private Shared Function CalculateAdjustment(label1 As Label, label2 As Label, minDistance As Single) As PointF
            ' 计算两个标签中心的向量
            Dim center1 As New PointF(CSng(label1.X + label1.width / 2), CSng(label1.Y + label1.height / 2))
            Dim center2 As New PointF(CSng(label2.X + label2.width / 2), CSng(label2.Y + label2.height / 2))

            ' 计算中心连线方向
            Dim dx = center1.X - center2.X
            Dim dy = center1.Y - center2.Y
            Dim distance = CSng(std.Sqrt(dx * dx + dy * dy))

            ' 如果距离太小，使用默认方向
            If distance < 0.001 Then
                dx = 1.0F
                dy = 1.0F
                distance = 1.0F
            End If

            ' 计算需要移动的距离（当前重叠距离 + 最小间距）
            Dim overlapX = (label1.width + label2.width) / 2 - std.Abs(dx)
            Dim overlapY = (label1.height + label2.height) / 2 - std.Abs(dy)

            Dim moveX = (dx / distance) * (overlapX + minDistance) * 0.5F
            Dim moveY = (dy / distance) * (overlapY + minDistance) * 0.5F

            Return New PointF(moveX, moveY)
        End Function

        ''' <summary>
        ''' 应用位置调整到标签
        ''' </summary>
        Private Shared Sub ApplyAdjustment(label As Label, adjustment As PointF)
            ' 更新标签位置[1](@ref)
            label.X += adjustment.X
            label.Y += adjustment.Y
        End Sub
    End Class
End Namespace
