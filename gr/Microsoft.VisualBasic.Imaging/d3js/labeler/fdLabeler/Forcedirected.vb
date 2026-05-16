#Region "Microsoft.VisualBasic::27ad0f8913cd8872988932d954b935d4, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\fdLabeler\Forcedirected.vb"

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

    '   Total Lines: 225
    '    Code Lines: 169 (75.11%)
    ' Comment Lines: 12 (5.33%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 44 (19.56%)
    '     File Size: 7.80 KB


    '     Class Forcedirected
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Height, Start, Width
    ' 
    '         Sub: Collide, RejectRegions, reset, runAttraction, runRepulsive
    '              setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace d3js.Layout

    Public Class Forcedirected : Inherits DataLabeler

        Protected ReadOnly mDxMap As New Dictionary(Of String, Double)
        Protected ReadOnly mDyMap As New Dictionary(Of String, Double)

        Protected ReadOnly ejectFactor As Integer = 6
        Protected ReadOnly condenseFactor As Integer = 3
        Protected ReadOnly dist_thresh As DoubleRange

        Dim k As Double
        Dim maxtx As Integer = 4
        Dim maxty As Integer = 3
        Dim temperature As Double ' 引入温度变量

        ''' <summary>
        ''' 会尽量避免在这个区域内存在网络的节点，这个区域一般为legend的绘制区域
        ''' </summary>
        Protected ReadOnly avoidRegions As (rect As Rectangle2D, center As PointF)()

        Sub New(Optional ejectFactor As Integer = 6,
                Optional condenseFactor As Integer = 3,
                Optional dist$ = "30,250",
                Optional avoidRegions As RectangleF() = Nothing)

            Me.dist_thresh = dist.NumericRangeParser
            Me.condenseFactor = condenseFactor
            Me.ejectFactor = ejectFactor
            Me.avoidRegions = avoidRegions _
                .SafeQuery _
                .Select(Function(rect) (New Rectangle2D(rect), rect.Centre)) _
                .ToArray
        End Sub

        Public Overrides Function Width(x As Double) As DataLabeler
            CANVAS_WIDTH = x
            maxtx = CANVAS_WIDTH / 3
            Return Me
        End Function

        Public Overrides Function Height(x As Double) As DataLabeler
            CANVAS_HEIGHT = x
            maxty = CANVAS_HEIGHT / 3
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Collide()
            Call reset()
            Call runRepulsive()
            Call runAttraction()
            Call RejectRegions()
            Call setPosition()
        End Sub

        Protected Sub reset()
            For Each v As Label In m_labels
                mDxMap(v.text) = 0.0
                mDyMap(v.text) = 0.0
            Next
        End Sub

        Protected Sub RejectRegions()
            ' 优化：使用矩形相交检测，而不是中心点距离
            For Each region In avoidRegions
                For Each v As Label In m_labels
                    If v.pinned Then Continue For

                    ' 将 Label 转为 RectangleF 进行相交判断
                    Dim vRect As New RectangleF(v.X, v.Y, v.width, v.height)

                    ' 假设 Rectangle2D 可以转换为 RectangleF (根据你的框架调整)
                    Dim aRect As RectangleF = region.rect.Rectangle

                    If vRect.IntersectsWith(aRect) Then
                        ' 计算穿透深度，寻找最短逃逸路径
                        Dim overlapLeft = vRect.Right - aRect.Left
                        Dim overlapRight = aRect.Right - vRect.Left
                        Dim overlapTop = vRect.Bottom - aRect.Top
                        Dim overlapBottom = aRect.Bottom - vRect.Top

                        Dim minOverlap = std.Min(std.Min(overlapLeft, overlapRight), std.Min(overlapTop, overlapBottom))
                        Dim id = v.text

                        ' 增加一个较大的强制排斥力
                        Dim pushForce As Double = minOverlap * ejectFactor * 2

                        If minOverlap = overlapLeft Then mDxMap(id) -= pushForce
                        If minOverlap = overlapRight Then mDxMap(id) += pushForce
                        If minOverlap = overlapTop Then mDyMap(id) -= pushForce
                        If minOverlap = overlapBottom Then mDyMap(id) += pushForce
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 标签节点之间存在的排斥力
        ''' </summary>
        Protected Overridable Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim dx, dy As Double

            For Each v As Label In m_labels
                If v.pinned Then Continue For
                id = v.text

                For Each u As Label In m_labels.Where(Function(ui) Not ui Is v)
                    ' 修复：使用标签的中心点计算排斥力，而不是左上角
                    Dim vCenterX = v.X + v.width / 2
                    Dim vCenterY = v.Y + v.height / 2
                    Dim uCenterX = u.X + u.width / 2
                    Dim uCenterY = u.Y + u.height / 2

                    distX = vCenterX - uCenterX
                    distY = vCenterY - uCenterY
                    dist = std.Sqrt(distX * distX + distY * distY)

                    ' 移除了 dist_thresh.Max 的限制，只要靠得太近就排斥
                    ' 设定一个极小值下限防止除以0
                    If dist > 0.1 AndAlso dist < (k * 2) Then
                        ' 考虑矩形大小的排斥力 (越近越强烈)
                        dx = (distX / dist) * (k * k / dist) * ejectFactor
                        dy = (distY / dist) * (k * k / dist) * ejectFactor

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 标签节点与anchor之间得吸引力
        ''' </summary>
        Protected Overridable Sub runAttraction()
            Dim u As Label
            Dim v As Anchor
            Dim distX, distY, dist As Double
            Dim dx, dy As Double

            For i As Integer = 0 To m_labels.Length - 1
                u = m_labels(i)
                v = m_anchors(i)

                If u.pinned Then Continue For

                ' 修复：将标签中心点拉向锚点，而不是左上角
                Dim uCenterX = u.X + u.width / 2
                Dim uCenterY = u.Y + u.height / 2

                distX = uCenterX - v.x
                distY = uCenterY - v.y
                dist = std.Sqrt(distX * distX + distY * distY)

                dx = distX * dist / k * condenseFactor
                dy = distY * dist / k * condenseFactor

                ' 修复：移除 / u.text.Length，长文本不应受到更强的拉力
                mDxMap(u.text) = mDxMap(u.text) - dx
                mDyMap(u.text) = mDyMap(u.text) - dy
            Next
        End Sub

        Private Sub setPosition()
            Dim dx, dy As Double
            Dim x, y As Double

            For Each node As Label In m_labels.Where(Function(v) Not v.pinned)
                dx = mDxMap(node.text)
                dy = mDyMap(node.text)

                ' 修复：使用 Log(1 + |dx|) 防止小于1时对数变负导致方向反转
                If dx <> 0 Then
                    dx = std.Sign(dx) * std.Log(1 + std.Abs(dx))
                End If
                If dy <> 0 Then
                    dy = std.Sign(dy) * std.Log(1 + std.Abs(dy))
                End If

                ' 引入温度限制，随着迭代降温
                Dim limitedDx = std.Min(std.Abs(dx), maxtx * temperature) * std.Sign(dx)
                Dim limitedDy = std.Min(std.Abs(dy), maxty * temperature) * std.Sign(dy)

                x = node.X + limitedDx
                y = node.Y + limitedDy

                ' 边界检测修复：防止宽度大于画布时出现负值
                If x + node.width >= CANVAS_WIDTH Then
                    x = std.Max(offset.X, CANVAS_WIDTH - node.width)
                ElseIf x < offset.X Then
                    x = offset.X
                End If

                If y + node.height >= CANVAS_HEIGHT Then
                    y = std.Max(offset.Y, CANVAS_HEIGHT - node.height)
                ElseIf y < offset.Y Then
                    y = offset.Y
                End If

                node.X = x
                node.Y = y
            Next
        End Sub

        Public Overrides Function Start(Optional nsweeps As Integer = 2000, Optional showProgress As Boolean = True) As DataLabeler
            Me.k = std.Sqrt(CANVAS_WIDTH * CANVAS_HEIGHT / m_labels.Length)

            ' 初始温度设为 1.0
            Me.temperature = 1.0

            For i As Integer = 0 To nsweeps
                Call Collide()

                ' 模拟退火：温度线性递减到 0.1，保留微调能力
                Me.temperature = std.Max(0.1, 1.0 - (i / nsweeps))

                If showProgress AndAlso (100 * i / nsweeps) Mod 5 = 0 Then
                    Console.WriteLine($"- Completed {i + 1} of {nsweeps} [{CInt(100 * i / nsweeps)}%]")
                End If
            Next

            Return Me
        End Function
    End Class

End Namespace
