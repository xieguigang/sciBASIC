#Region "Microsoft.VisualBasic::2809931c6aefdaf62998695740402520, Data_science\Visualization\DataPlot\Advanced\TreemapPlot.vb"

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

    '   Total Lines: 204
    '    Code Lines: 151 (74.02%)
    ' Comment Lines: 26 (12.75%)
    '    - Xml Docs: 53.85%
    ' 
    '   Blank Lines: 27 (13.24%)
    '     File Size: 8.42 KB


    ' Class TreemapNode
    ' 
    '     Properties: Color, Group, Label, Rect, Value
    ' 
    ' Class TreemapPlot
    ' 
    '     Properties: AutoFontSize, BorderWidth, ColorByGroup, Nodes, ShowLabels
    '                 ShowValues
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ShortestSide, WorstAspect
    ' 
    '     Sub: AssignColors, DrawNodeLabel, LayoutRow, Plot, Squarify
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

''' <summary>矩形树图节点</summary>
Public Class TreemapNode
    Public Property Label As String = ""
    Public Property Value As Double
    Public Property Color As Color? = Nothing
    ''' <summary>分组（用于着色，可选）</summary>
    Public Property Group As String = ""
    ''' <summary>运行时：布局后的矩形（公开给宿主做命中测试与 3D 挤出）</summary>
    Public Property Rect As RectangleF
    ''' <summary>
    ''' 子节点；为空时当前节点即为叶子，行为与旧版的扁平节点完全一致。
    ''' 容器节点的 <see cref="Value"/> 在布局时会被自动替换为所有子节点
    ''' 绝对值之和（仅当调用方没有显式给出正值时）。
    ''' </summary>
    Public Property Children As New List(Of TreemapNode)()
    ''' <summary>当前节点在树中的深度，根节点为 0</summary>
    Public Property Depth As Integer
    ''' <summary>宿主机自由挂载的业务对象（例如一个代码符号）</summary>
    Public Property Tag As Object

    ''' <summary>没有子节点即为叶子节点</summary>
    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Children Is Nothing OrElse Children.Count = 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{Label} = {Value}"
    End Function
End Class

''' <summary>矩形树图（Squarified 布局算法，优化子矩形宽高比接近 1:1）</summary>
Public Class TreemapPlot
    Inherits PlotEngine

    Public Property Nodes As New List(Of TreemapNode)()
    ''' <summary>显示标签</summary>
    Public Property ShowLabels As Boolean = True
    ''' <summary>显示数值</summary>
    Public Property ShowValues As Boolean = True
    ''' <summary>标签字体大小自适应（按矩形面积缩放）</summary>
    Public Property AutoFontSize As Boolean = True
    ''' <summary>按 Group 分组着色（同一 Group 同色）</summary>
    Public Property ColorByGroup As Boolean = True
    ''' <summary>边框宽度</summary>
    Public Property BorderWidth As Single = 1.0F
    ''' <summary>容器节点的内边距（子节点布局时从父矩形四周扣除）</summary>
    Public Property GroupPadding As Single = 2.0F
    ''' <summary>容器节点顶部标题条的高度，子节点布局时从父矩形顶部扣除</summary>
    Public Property HeaderHeight As Single = 15.0F
    ''' <summary>超过该深度的子节点只布局不绘制细节，用于限制巨树的绘制成本</summary>
    Public Property MaxRenderDepth As Integer = 8

    ''' <summary>最近一次布局的结果，按深度优先顺序排列（父节点在前，子节点在后）</summary>
    Dim _layout As New List(Of TreemapNode)()

    ''' <summary>
    ''' 最近一次布局的结果；未布局过则为空列表。
    ''' 宿主可以据此做命中测试，或把 [x,y] 矩形挤出为 3D 体块。
    ''' </summary>
    Public ReadOnly Property Layout As List(Of TreemapNode)
        Get
            Return _layout
        End Get
    End Property

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    ''' <summary>
    ''' 直接在外部提供的绘图设备上绘制，例如
    ''' <c>Microsoft.VisualBasic.Drawing.DirectX.DxGraphics</c>（Direct2D/D3D11 GPU 画布）。
    ''' </summary>
    ''' <param name="g">已经绑定到目标表面的绘图设备，画布的所有权仍归调用方。</param>
    ''' <param name="theme">主题（省略时用浅色主题）。</param>
    ''' <remarks>
    ''' 画布尺寸从 <c>g.Size</c> 取，宿主尺寸变化后需要重新构造本对象。
    ''' </remarks>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        If Nodes.Count = 0 Then Return

        Call LayoutNodes(_width, _height)

        For Each n In _layout
            If n.Rect.Width < 1 OrElse n.Rect.Height < 1 Then Continue For

            Dim color = If(n.Color, Theme.Palette(0))

            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BackgroundColor, BorderWidth)
                _g.FillRectangle(br, n.Rect)
                _g.DrawRectangle(pen, n.Rect.X, n.Rect.Y, n.Rect.Width, n.Rect.Height)
            End Using

            If Not ShowLabels Then Continue For

            If n.IsLeaf Then
                If n.Rect.Width > 30 AndAlso n.Rect.Height > 18 Then
                    DrawNodeLabel(n, color)
                End If
            Else
                Call DrawGroupHeader(n, color)
            End If
        Next
    End Sub

    ''' <summary>
    ''' 只做 squarified 布局、不做任何绘制，返回所有节点（含布局后的 <see cref="TreemapNode.Rect"/>）。
    ''' </summary>
    ''' <param name="width">画布宽度（像素），用于计算绘图区。</param>
    ''' <param name="height">画布高度（像素），用于计算绘图区。</param>
    ''' <returns>
    ''' 深度优先顺序的节点列表；父节点排在其子节点之前，因此按该顺序绘制即为
    ''' "先画容器后画内容"。
    ''' </returns>
    Public Function LayoutNodes(width As Integer, height As Integer) As List(Of TreemapNode)
        _layout = New List(Of TreemapNode)()

        If Nodes.Count = 0 Then
            Return _layout
        End If

        _width = width
        _height = height

        ' 先把整棵树摊平并补齐容器节点的度量值
        For Each n As TreemapNode In Nodes
            Call Normalize(n, 0)
        Next

        Dim total = Nodes.Sum(Function(n) std.Abs(n.Value))

        If total <= 0 Then Return _layout

        Call AssignColors()

        _plotArea = New RectangleF(Theme.MarginLeft, Theme.MarginTop,
                                   _width - Theme.MarginLeft - Theme.MarginRight,
                                   _height - Theme.MarginTop - Theme.MarginBottom)

        Call SquarifyNodes(Nodes, _plotArea, 0)

        Return _layout
    End Function

    ''' <summary>
    ''' 命中测试：返回覆盖给定画布坐标的、深度最大的节点。
    ''' </summary>
    Public Function HitTest(x As Single, y As Single) As TreemapNode
        If _layout Is Nothing Then
            Return Nothing
        End If

        ' _layout 是深度优先顺序，越靠后越深，所以倒序查找即为"最上层"
        For i As Integer = _layout.Count - 1 To 0 Step -1
            If _layout(i).Rect.Contains(x, y) Then
                Return _layout(i)
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' 摊平节点树：登记深度、把容器节点的 Value 补齐为子节点之和。
    ''' </summary>
    Private Sub Normalize(n As TreemapNode, depth As Integer)
        n.Depth = depth
        _layout.Add(n)

        If n.IsLeaf Then
            Return
        End If

        Dim sum As Double = 0

        For Each child As TreemapNode In n.Children
            Call Normalize(child, depth + 1)
            sum += std.Abs(child.Value)
        Next

        If n.Value <= 0 Then
            n.Value = sum
        End If
    End Sub

    ''' <summary>
    ''' 对一组兄弟节点做 squarified 布局，并在每个容器内部递归布局其子节点。
    ''' </summary>
    Private Sub SquarifyNodes(nodes As List(Of TreemapNode), area As RectangleF, depth As Integer)
        Dim total As Double = nodes.Sum(Function(n) std.Abs(n.Value))

        If total <= 0 OrElse area.Width <= 1 OrElse area.Height <= 1 Then
            Return
        End If

        ' 按值降序排序（Squarified 算法要求）
        Dim sorted = nodes.OrderByDescending(Function(n) std.Abs(n.Value)).ToList()
        Dim areaTotal As Single = CSng(area.Width * area.Height)
        Dim items As New List(Of Tuple(Of TreemapNode, Single))()

        For Each n In sorted
            items.Add(Tuple.Create(n, CSng(std.Abs(n.Value) / total * areaTotal)))
        Next

        Dim rest As RectangleF = area

        Call Squarify(items, rest)

        If depth >= MaxRenderDepth Then
            Return
        End If

        For Each n As TreemapNode In sorted
            If n.IsLeaf Then
                Continue For
            End If

            Dim inner As RectangleF = InnerRect(n.Rect)

            If inner.Width <= 1 OrElse inner.Height <= 1 Then
                Continue For
            End If

            Call SquarifyNodes(n.Children, inner, depth + 1)
        Next
    End Sub

    ''' <summary>容器节点扣除标题条与内边距之后，留给子节点的内容区</summary>
    ''' <remarks>
    ''' 一个很小的容器负担不起标题条和内边距：它们会把整个矩形吃光，导致子节点
    ''' 拿不到任何面积。因此这里按容器的实际尺寸自适应地收缩，深层次的树图在
    ''' 有限的画布上仍然能保留住每一级的内容区。
    ''' </remarks>
    Private Function InnerRect(r As RectangleF) As RectangleF
        Dim pad As Single = If(r.Width > 20.0F AndAlso r.Height > 20.0F, GroupPadding, 0.0F)
        Dim header As Single = 0.0F

        If r.Height > 36.0F Then
            header = std.Min(HeaderHeight, r.Height * 0.3F)
        End If

        Dim w As Single = r.Width - pad * 2.0F
        Dim h As Single = r.Height - header - pad

        If w < 0 Then w = 0
        If h < 0 Then h = 0

        Return New RectangleF(r.X + pad, r.Y + header, w, h)
    End Function

    ''' <summary>在容器节点的顶部标题条里绘制分组名</summary>
    Private Sub DrawGroupHeader(n As TreemapNode, bgColor As Color)
        Dim header As New RectangleF(n.Rect.X, n.Rect.Y, n.Rect.Width, HeaderHeight)

        If header.Width < 24 OrElse header.Height < 8 Then
            Return
        End If

        Dim brightness = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255
        Dim txtColor = If(brightness > 0.55, Color.Black, Color.White)
        Dim size As Single = std.Max(6.0F, std.Min(HeaderHeight - 3.0F, 11.0F))
        Dim font As New Font("Microsoft YaHei", size, FontStyle.Bold)
        Dim text As String = n.Label

        If ShowValues Then
            text &= $" ({FormatNumber(n.Value)})"
        End If

        Using br As New SolidBrush(txtColor)
            _g.DrawString(text, font, br, header.X + 2.0F, header.Y)
        End Using
    End Sub

    ''' <summary>
    ''' 按 Group 分配颜色（同一组同色），否则按索引取调色板。
    ''' 层次树中会对整棵树的所有节点生效。
    ''' </summary>
    Private Sub AssignColors()
        ' 摊平后的列表覆盖整棵树；尚未布局时退化为顶层节点
        Dim all As List(Of TreemapNode) = If(_layout.Count > 0, _layout, Nodes)

        If ColorByGroup Then
            Dim groups = all.Select(Function(n) n.Group).Distinct().ToList()

            For Each n As TreemapNode In all
                If Not n.Color.HasValue Then
                    Dim gi = groups.IndexOf(n.Group)

                    If gi < 0 Then gi = 0

                    n.Color = Theme.Palette(gi Mod Theme.Palette.Length)
                End If
            Next
        Else
            For i = 0 To all.Count - 1
                If Not all(i).Color.HasValue Then
                    all(i).Color = Theme.Palette(i Mod Theme.Palette.Length)
                End If
            Next
        End If
    End Sub

    ''' <summary>Squarified 算法：沿最短边逐行切分</summary>
    Private Sub Squarify(items As List(Of Tuple(Of TreemapNode, Single)), area As RectangleF)
        Dim idx = 0
        Dim row As New List(Of Tuple(Of TreemapNode, Single))()

        While idx < items.Count
            Dim side = ShortestSide(area)
            Dim item = items(idx)
            ' 尝试把当前项加入行，比较加入前后的最差宽高比
            Dim testRow = New List(Of Tuple(Of TreemapNode, Single))(row)
            testRow.Add(item)
            Dim worstWith = WorstAspect(testRow, side)
            Dim worstWithout = If(row.Count > 0, WorstAspect(row, side), Double.MaxValue)

            If worstWith <= worstWithout Then
                ' 继续累积到当前行
                row.Add(item)
                idx += 1
            Else
                ' 当前行已最优，布局该行并开始新行
                If row.Count > 0 Then
                    LayoutRow(row, area)
                    row.Clear()
                End If
                ' 不递增 idx：下一轮把当前 item 加入新行
            End If
        End While

        ' 布局剩余行
        If row.Count > 0 Then
            LayoutRow(row, area)
        End If
    End Sub

    ''' <summary>计算一行中所有矩形的最大宽高比（越接近 1 越好）</summary>
    Private Function WorstAspect(row As List(Of Tuple(Of TreemapNode, Single)), side As Single) As Double
        If row.Count = 0 Then Return Double.MaxValue
        Dim sum = row.Sum(Function(r) r.Item2)
        If sum <= 0 Then Return Double.MaxValue
        Dim maxArea = row.Max(Function(r) r.Item2)
        Dim minArea = row.Min(Function(r) r.Item2)
        Dim s2 = side * side
        Dim sum2 = sum * sum
        Return std.Max(s2 * maxArea / sum2, sum2 / (s2 * std.Max(minArea, 0.000001)))
    End Function

    Private Function ShortestSide(area As RectangleF) As Single
        Return std.Min(area.Width, area.Height)
    End Function

    ''' <summary>布局一行（沿当前最短边排列），并从 area 中扣除已用部分</summary>
    Private Sub LayoutRow(row As List(Of Tuple(Of TreemapNode, Single)), ByRef area As RectangleF)
        If row.Count = 0 Then Return
        Dim sum = row.Sum(Function(r) r.Item2)
        If sum <= 0 Then Return

        If area.Width <= area.Height Then
            ' 短边是 Width：行占据顶部条带，高度 = sum/width，矩形沿 X 并排
            Dim rowH = sum / area.Width
            Dim x = area.X
            For Each item In row
                Dim w = item.Item2 / rowH
                item.Item1.Rect = New RectangleF(x, area.Y, w, rowH)
                x += w
            Next
            area = New RectangleF(area.X, area.Y + rowH, area.Width, area.Height - rowH)
        Else
            ' 短边是 Height：行占据左侧条带，宽度 = sum/height，矩形沿 Y 堆叠
            Dim rowW = sum / area.Height
            Dim y = area.Y
            For Each item In row
                Dim h = item.Item2 / rowW
                item.Item1.Rect = New RectangleF(area.X, y, rowW, h)
                y += h
            Next
            area = New RectangleF(area.X + rowW, area.Y, area.Width - rowW, area.Height)
        End If
    End Sub

    ''' <summary>绘制节点标签（自适应字体大小，文字颜色按背景亮度反色）</summary>
    Private Sub DrawNodeLabel(n As TreemapNode, bgColor As Color)
        Dim label = n.Label
        If ShowValues Then
            label &= vbCrLf & FormatNumber(n.Value)
        End If

        Dim font = Theme.TickLabelFont
        Dim minDim = std.Min(n.Rect.Width, n.Rect.Height)
        If AutoFontSize AndAlso minDim < 50 Then
            Dim size = std.Max(6, CSng(minDim / 8))
            font = New Font("Microsoft YaHei", size, FontStyle.Regular)
        End If

        ' 文字颜色：背景亮用黑字，背景暗用白字
        Dim brightness = (0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B) / 255
        Dim txtColor = If(brightness > 0.55, Color.Black, Color.White)

        Using br As New SolidBrush(txtColor),
              sf As New StringFormat()
            sf.Alignment = StringAlignment.Center
            sf.LineAlignment = StringAlignment.Center
            _g.DrawString(label, font, br,
                          n.Rect.X + n.Rect.Width / 2, n.Rect.Y + n.Rect.Height / 2)
        End Using
    End Sub
End Class
