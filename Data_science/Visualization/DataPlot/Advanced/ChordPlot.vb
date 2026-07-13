#Region "Microsoft.VisualBasic::f5cda3fd6c4739049d6570ae3e66c262, Data_science\Visualization\DataPlot\Advanced\ChordPlot.vb"

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

    '   Total Lines: 186
    '    Code Lines: 142 (76.34%)
    ' Comment Lines: 22 (11.83%)
    '    - Xml Docs: 54.55%
    ' 
    '   Blank Lines: 22 (11.83%)
    '     File Size: 7.99 KB


    ' Class ChordLink
    ' 
    '     Properties: Color, Source, Target, Value
    ' 
    ' Class ChordPlot
    ' 
    '     Properties: ChordAlpha, GapAngle, Links, Matrix, NodeColors
    '                 NodeLabels, ShowSelfLoops, StartAngle, Symmetric
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: DrawArcBand, Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports StringAlignment = Microsoft.VisualBasic.Imaging.StringAlignment
Imports StringFormat = Microsoft.VisualBasic.Imaging.StringFormat
Imports Microsoft.VisualBasic.Imaging

''' <summary>和弦图连接（可选辅助结构，用于显式指定连接而非矩阵）</summary>
Public Class ChordLink
    Public Property Source As Integer
    Public Property Target As Integer
    Public Property Value As Double
    Public Property Color As Color? = Nothing
End Class

''' <summary>和弦图（圆形布局，节点弧段 + 贝塞尔曲线和弦）</summary>
Public Class ChordPlot
    Inherits PlotEngine

    ''' <summary>节点标签</summary>
    Public Property NodeLabels As String() = {}
    ''' <summary>邻接矩阵 (n×n)，Matrix(i,j) 表示从 i 到 j 的流量</summary>
    Public Property Matrix As Double(,) = Nothing
    ''' <summary>显式连接列表（可选，优先于 Matrix）</summary>
    Public Property Links As New List(Of ChordLink)()
    ''' <summary>节点颜色</summary>
    Public Property NodeColors As Color() = Nothing
    ''' <summary>和弦透明度（0-255）</summary>
    Public Property ChordAlpha As Integer = 100
    ''' <summary>是否对称化矩阵（i↔j 流量合并）</summary>
    Public Property Symmetric As Boolean = True
    ''' <summary>仅绘制显式连接（不画对角线自环）</summary>
    Public Property ShowSelfLoops As Boolean = False
    ''' <summary>起始角度（度，默认 -90 即 12 点方向）</summary>
    Public Property StartAngle As Single = -90
    ''' <summary>节点弧段间隙（度）</summary>
    Public Property GapAngle As Single = 1.5F

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        Dim n = NodeLabels.Length
        If n = 0 Then Return

        ' 构建连接列表
        Dim allLinks As New List(Of ChordLink)()
        If Links.Count > 0 Then
            allLinks = Links.ToList()
        ElseIf Matrix IsNot Nothing Then
            For i = 0 To n - 1
                For j = 0 To n - 1
                    If i = j AndAlso Not ShowSelfLoops Then Continue For
                    If Matrix(i, j) <> 0 Then
                        allLinks.Add(New ChordLink With {.Source = i, .Target = j, .Value = Matrix(i, j)})
                    End If
                Next
            Next
        End If

        ' 对称化：合并 i→j 与 j→i
        If Symmetric Then
            Dim merged As New Dictionary(Of Long, ChordLink)()
            For Each l In allLinks
                Dim a = Math.Min(l.Source, l.Target)
                Dim b = Math.Max(l.Source, l.Target)
                Dim key = CLng(a) * n + b
                If merged.ContainsKey(key) Then
                    merged(key).Value += l.Value
                Else
                    merged(key) = New ChordLink With {.Source = a, .Target = b, .Value = l.Value}
                End If
            Next
            allLinks = merged.Values.ToList()
        End If

        Dim palette = If(NodeColors, Theme.Palette)
        Dim cx = _width / 2.0F
        Dim cy = (_height + Theme.MarginTop) / 2.0F
        Dim R = Math.Min(_width - Theme.MarginLeft - Theme.MarginRight,
                         _height - Theme.MarginTop - Theme.MarginBottom) * 0.38F

        ' ---- 计算每个节点总流量与弧段占比 ----
        Dim nodeTotal(n - 1) As Double
        Dim grandTotal = 0.0
        For Each l In allLinks
            nodeTotal(l.Source) += l.Value
            If Not Symmetric Then nodeTotal(l.Target) += l.Value
            grandTotal += l.Value
        Next
        If Symmetric Then
            ' 对称模式下每个连接两端都计入
            Array.Clear(nodeTotal, 0, n)
            grandTotal = 0
            For Each l In allLinks
                nodeTotal(l.Source) += l.Value
                nodeTotal(l.Target) += l.Value
                grandTotal += l.Value * 2
            Next
        End If
        If grandTotal <= 0 Then Return

        ' 每个节点的弧段角度范围
        Dim totalGap = GapAngle * n
        Dim availAngle = 360.0F - totalGap
        Dim nodeStart(n - 1) As Single
        Dim nodeSweep(n - 1) As Single
        Dim curAngle = StartAngle
        For i = 0 To n - 1
            nodeSweep(i) = CSng(nodeTotal(i) / grandTotal * availAngle)
            nodeStart(i) = curAngle
            curAngle += nodeSweep(i) + GapAngle
        Next

        ' ---- 绘制节点弧段（色带）----
        Dim bandWidth = Math.Max(8.0F, R * 0.08F)
        For i = 0 To n - 1
            If nodeSweep(i) <= 0 Then Continue For
            Dim color = palette(i Mod palette.Length)
            DrawArcBand(cx, cy, R, R + bandWidth, nodeStart(i), nodeSweep(i), color)
        Next

        ' ---- 绘制和弦（贝塞尔曲线穿过圆心附近）----
        For Each l In allLinks
            If l.Source = l.Target AndAlso Not ShowSelfLoops Then Continue For
            If l.Value <= 0 Then Continue For

            ' 源端与目标端在各自弧段中点的角度
            Dim srcA = (nodeStart(l.Source) + nodeSweep(l.Source) / 2) * Math.PI / 180
            Dim tgtA = (nodeStart(l.Target) + nodeSweep(l.Target) / 2) * Math.PI / 180

            Dim p1 = New PointF(cx + CSng(Math.Cos(srcA)) * R, cy + CSng(Math.Sin(srcA)) * R)
            Dim p2 = New PointF(cx + CSng(Math.Cos(tgtA)) * R, cy + CSng(Math.Sin(tgtA)) * R)

            ' 控制点在圆心附近，使曲线弯曲穿过中心区域
            Dim ctrl1 = New PointF(cx + (p1.X - cx) * 0.25F, cy + (p1.Y - cy) * 0.25F)
            Dim ctrl2 = New PointF(cx + (p2.X - cx) * 0.25F, cy + (p2.Y - cy) * 0.25F)

            Dim color = If(l.Color, palette(l.Source Mod palette.Length))
            Dim path As New GraphicsPath()
            path.AddBezier(p1, ctrl1, ctrl2, p2)
            Using br As New SolidBrush(Color.FromArgb(ChordAlpha, color))
                _g.FillPath(br, path)
            End Using
        Next

        ' ---- 节点标签 ----
        Using br As New SolidBrush(Theme.TextColor),
              sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center
            For i = 0 To n - 1
                If nodeSweep(i) <= 0 Then Continue For
                Dim midA = (nodeStart(i) + nodeSweep(i) / 2) * Math.PI / 180
                Dim labelR = R + bandWidth + 16
                Dim lx = cx + CSng(Math.Cos(midA)) * labelR
                Dim ly = cy + CSng(Math.Sin(midA)) * labelR
                _g.DrawString(NodeLabels(i), Theme.TickLabelFont, br, lx, ly)
            Next
        End Using
    End Sub

    ''' <summary>绘制圆弧色带（外环）</summary>
    Private Sub DrawArcBand(cx As Single, cy As Single, rInner As Single, rOuter As Single,
                            startAngle As Single, sweepAngle As Single, color As Color)
        Dim path As New GraphicsPath()
        Dim rectOuter = New RectangleF(cx - rOuter, cy - rOuter, rOuter * 2, rOuter * 2)
        Dim rectInner = New RectangleF(cx - rInner, cy - rInner, rInner * 2, rInner * 2)

        path.AddArc(rectOuter, startAngle, sweepAngle)
        path.AddArc(rectInner, startAngle + sweepAngle, -sweepAngle)
        path.CloseFigure()

        Using br As New SolidBrush(color),
              pen As New Pen(Theme.BackgroundColor, 0.5F)
            _g.FillPath(br, path)
            _g.DrawPath(pen, path)
        End Using
    End Sub
End Class
