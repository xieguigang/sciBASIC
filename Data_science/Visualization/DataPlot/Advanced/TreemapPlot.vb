Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

''' <summary>矩形树图节点</summary>
Public Class TreemapNode
    Public Property Label As String = ""
    Public Property Value As Double
    Public Property Color As Color? = Nothing
    ''' <summary>分组（用于着色，可选）</summary>
    Public Property Group As String = ""
    ''' <summary>运行时：布局后的矩形</summary>
    Friend Property Rect As RectangleF
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

    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing)
        MyBase.New(width, height, theme)
    End Sub

    Public Sub Plot()
        DrawBackground()
        DrawTitle()

        If Nodes.Count = 0 Then Return

        Dim total = Nodes.Sum(Function(n) Math.Abs(n.Value))
        If total <= 0 Then Return

        AssignColors()

        _plotArea = New RectangleF(Theme.MarginLeft, Theme.MarginTop,
                                   _width - Theme.MarginLeft - Theme.MarginRight,
                                   _height - Theme.MarginTop - Theme.MarginBottom)

        ' 按值降序排序（Squarified 算法要求）
        Dim sorted = Nodes.OrderByDescending(Function(n) Math.Abs(n.Value)).ToList()

        ' 将节点归一化为面积元组列表
        Dim areaTotal = CSng(_plotArea.Width * _plotArea.Height)
        Dim items As New List(Of Tuple(Of TreemapNode, Single))()
        For Each n In sorted
            items.Add(Tuple.Create(n, CSng(Math.Abs(n.Value) / total * areaTotal)))
        Next

        ' 执行 Squarified 布局
        Squarify(items, _plotArea)

        ' 绘制矩形
        For Each n In Nodes
            If n.Rect.Width < 1 OrElse n.Rect.Height < 1 Then Continue For
            Dim color = If(n.Color, Theme.Palette(0))
            Using br As New SolidBrush(color),
                  pen As New Pen(Theme.BackgroundColor, BorderWidth)
                _g.FillRectangle(br, n.Rect)
                _g.DrawRectangle(pen, n.Rect.X, n.Rect.Y, n.Rect.Width, n.Rect.Height)
            End Using

            If ShowLabels AndAlso n.Rect.Width > 30 AndAlso n.Rect.Height > 18 Then
                DrawNodeLabel(n, color)
            End If
        Next
    End Sub

    ''' <summary>按 Group 分配颜色（同一组同色），否则按索引取调色板</summary>
    Private Sub AssignColors()
        If ColorByGroup Then
            Dim groups = Nodes.Select(Function(n) n.Group).Distinct().ToList()
            For i = 0 To Nodes.Count - 1
                If Not Nodes(i).Color.HasValue Then
                    Dim gi = groups.IndexOf(Nodes(i).Group)
                    Nodes(i).Color = Theme.Palette(gi Mod Theme.Palette.Length)
                End If
            Next
        Else
            For i = 0 To Nodes.Count - 1
                If Not Nodes(i).Color.HasValue Then
                    Nodes(i).Color = Theme.Palette(i Mod Theme.Palette.Length)
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
        Return Math.Max(s2 * maxArea / sum2, sum2 / (s2 * Math.Max(minArea, 0.000001)))
    End Function

    Private Function ShortestSide(area As RectangleF) As Single
        Return Math.Min(area.Width, area.Height)
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
            label &= vbNewLine & FormatNumber(n.Value)
        End If

        Dim font = Theme.TickLabelFont
        Dim minDim = Math.Min(n.Rect.Width, n.Rect.Height)
        If AutoFontSize AndAlso minDim < 50 Then
            Dim size = Math.Max(6, CSng(minDim / 8))
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
