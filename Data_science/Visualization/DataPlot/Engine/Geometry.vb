' ---------------------------------------------------------------------------
'  DataPlot / Engine / Geometry.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' ---------------------------------------------------------------------------

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

''' <summary>
''' 与具体图表无关的通用数值 / 几何工具：分位数、刻度算法、核密度估计、样条平滑、
''' 抖动、矩阵行列表标准化。多个图表类型（箱线、小提琴、密度图、轮廓图、聚类热图……）
''' 共用这里的实现，避免出现多份不一致的副本。
''' </summary>
Public Module Geometry

    ''' <summary>线性插值分位数（要求输入已升序排序）</summary>
    ''' <param name="sorted">升序排列的样本</param>
    ''' <param name="p">分位点，取值为 0~1</param>
    Public Function Quantile(sorted As Double(), p As Double) As Double
        If sorted Is Nothing OrElse sorted.Length = 0 Then Return Double.NaN
        If sorted.Length = 1 Then Return sorted(0)
        If p <= 0 Then Return sorted(0)
        If p >= 1 Then Return sorted(sorted.Length - 1)

        Dim idx = p * (sorted.Length - 1)
        Dim lo = CInt(std.Floor(idx))
        Dim hi = CInt(std.Ceiling(idx))
        If lo = hi Then Return sorted(lo)

        Return sorted(lo) + (sorted(hi) - sorted(lo)) * (idx - lo)
    End Function

    ''' <summary>基于任意 nmber 序列计算分位数（内部自动排序）</summary>
    Public Function QuantileOf(values As IEnumerable(Of Double), p As Double) As Double
        Dim buf = values.SafeQuery.OrderBy(Function(x) x).ToArray()
        Return Quantile(buf, p)
    End Function

    ''' <summary>nice number 算法：把范围换算成人眼友好的刻度间隔</summary>
    Public Function NiceStep(range As Double, tickCount As Integer) As Double
        If range <= 0 OrElse Double.IsNaN(range) OrElse Double.IsInfinity(range) Then Return 1

        Dim rough = range / If(tickCount <= 0, 6, tickCount)
        Dim pow = std.Pow(10, std.Floor(std.Log10(rough)))
        Dim norm = rough / pow
        Dim [step] As Double

        If norm < 1.5 Then
            [step] = 1
        ElseIf norm < 3 Then
            [step] = 2
        ElseIf norm < 7 Then
            [step] = 5
        Else
            [step] = 10
        End If

        Return [step] * pow
    End Function

    ''' <summary>生成覆盖 [min, max] 的刻度序列</summary>
    Public Function NiceTicks(min As Double, max As Double, Optional tickCount As Integer = 6) As Double()
        Dim list As New List(Of Double)()

        If max <= min OrElse Double.IsNaN(min) OrElse Double.IsNaN(max) Then
            Return {min}
        End If

        Dim [step] = NiceStep(max - min, tickCount)
        Dim v = std.Ceiling(min / [step]) * [step]

        Do While v <= max + [step] * 0.001
            list.Add(std.Round(v, 8))
            v += [step]
        Loop

        Return list.ToArray()
    End Function

    ''' <summary>自动化值域：把 min/max 撑到有效范围并留出余量</summary>
    Public Sub ExpandRange(ByRef min As Double, ByRef max As Double, Optional pad As Double = 0.05)
        If Double.IsNaN(min) OrElse Double.IsNaN(max) Then
            min = 0
            max = 1
            Return
        End If
        If max < min Then
            Dim tmp = min
            min = max
            max = tmp
        End If
        If std.Abs(max - min) < 0.000000000001 Then
            min -= 1
            max += 1
        End If

        Dim d = (max - min) * pad
        min -= d
        max += d
    End Sub

    ''' <summary>刻度标签的统一数字格式化（与 <c>PlotEngine.FormatNumber</c> 行为一致）</summary>
    Public Function NumberLabel(v As Double) As String
        Dim a = std.Abs(v)
        If Double.IsNaN(v) Then Return "NaN"
        If a = 0 Then Return "0"
        If a >= 10000 OrElse a < 0.01 Then
            Return v.ToString("0.#E+0")
        ElseIf a >= 100 Then
            Return v.ToString("0")
        ElseIf a >= 1 Then
            Return v.ToString("0.##")
        Else
            Return v.ToString("0.###")
        End If
    End Function

    ''' <summary>线性 / 双线性插值：把 <paramref name="v"/> 从 [vmin,vmax] 映射到 [omin,omax]</summary>
    Public Function Remap(v As Double, vmin As Double, vmax As Double, omin As Double, omax As Double) As Double
        If vmax - vmin = 0 Then Return omin
        Dim t = (v - vmin) / (vmax - vmin)
        t = std.Max(0, std.Min(1, t))
        Return omin + t * (omax - omin)
    End Function

    ''' <summary>
    ''' 抖动（jitter）：给数据加上幅度可控的均匀噪声，用来打散散点图中重叠的离散取值。
    ''' </summary>
    ''' <param name="values">原始样本</param>
    ''' <param name="amount">抖动幅度（相对值域的比例，0 表示不抖动）</param>
    ''' <param name="seed">随机数种子（给定后结果可复现）</param>
    Public Function MakeJitter(values As IEnumerable(Of Double), Optional amount As Double = 0.02,
                               Optional seed As Integer = -1) As Double()
        Dim buf = values.SafeQuery.ToArray()
        If buf.Length = 0 Then Return buf
        If amount <= 0 Then Return buf

        Dim rnd As Random = If(seed >= 0, New Random(seed), New Random())
        Dim vmin = buf.Min(), vmax = buf.Max()
        Dim width = (vmax - vmin) * amount
        Dim result(buf.Length - 1) As Double

        If width = 0 Then width = amount

        For i = 0 To buf.Length - 1
            result(i) = buf(i) + (rnd.NextDouble() - 0.5) * width
        Next

        Return result
    End Function

    ''' <summary>高斯核密度估计（1D），返回给定网格点上的密度值</summary>
    ''' <param name="sample">样本</param>
    ''' <param name="n">网格点数</param>
    ''' <param name="bandwidth">带宽，留空时按 Silverman 经验法则估计</param>
    Public Function Density1D(sample As IEnumerable(Of Double), Optional n As Integer = 256,
                              Optional bandwidth As Double = 0) As (x As Double(), y As Double())
        Dim buf = sample.SafeQuery.Where(Function(x) Not Double.IsNaN(x)).ToArray()
        If buf.Length = 0 Then Return (New Double() {}, New Double() {})

        Dim lo = buf.Min(), hi = buf.Max()
        ExpandRange(lo, hi, 0.08)

        If bandwidth <= 0 Then
            Dim mean = buf.Average()
            Dim sd = std.Sqrt(buf.Sum(Function(x) (x - mean) ^ 2) / buf.Length)
            If sd <= 0 Then sd = If(hi - lo = 0, 1, (hi - lo) / 4)
            bandwidth = 0.9 * sd * std.Pow(buf.Length, -0.2)
        End If

        Dim xs(n - 1) As Double
        Dim ys(n - 1) As Double
        Dim [step] = (hi - lo) / (n - 1)
        Dim norm = 1 / (bandwidth * std.Sqrt(2 * std.PI) * buf.Length)

        For i = 0 To n - 1
            Dim x = lo + i * [step]
            Dim sum As Double = 0

            For Each v In buf
                Dim z = (x - v) / bandwidth
                sum += std.Exp(-0.5 * z * z)
            Next

            xs(i) = x
            ys(i) = sum * norm
        Next

        Return (xs, ys)
    End Function

    ''' <summary>
    ''' 二维核密度估计：把散点铺成网格密度矩阵（行 = y 方向，列 = x 方向）。
    ''' 用于密度图 / 等值线图的数据源，避免为一张图引入专门的网格依赖。
    ''' </summary>
    ''' <param name="points">输入点集（可为任意坐标系，通常是像素坐标）</param>
    ''' <param name="bounds">网格覆盖的矩形</param>
    ''' <param name="cols">列数</param>
    ''' <param name="rows">行数</param>
    ''' <param name="radius">核半径（像素），小于等于 0 时按网格尺寸推算</param>
    ''' <returns>取值被归一化到 0~1 的 rows×cols 矩阵</returns>
    Public Function Density2D(points As IEnumerable(Of PointF), bounds As RectangleF,
                              cols As Integer, rows As Integer, Optional radius As Single = -1) As Double(,)
        Dim grid(rows - 1, cols - 1) As Double
        If cols <= 0 OrElse rows <= 0 OrElse bounds.Width <= 0 OrElse bounds.Height <= 0 Then
            Return grid
        End If

        Dim cellW = bounds.Width / cols
        Dim cellH = bounds.Height / rows

        If radius <= 0 Then
            radius = CSng(std.Max(cellW, cellH)) * 2
        End If
        If radius <= 0 Then Return grid

        Dim inv2sigma2 = 1.0 / (2 * (radius / 2) * (radius / 2))

        For Each pt In points.SafeQuery
            ' 只处理落在格网附近 ±3r 范围内的单元
            Dim c0 = CInt(std.Floor((pt.X - radius - bounds.Left) / cellW))
            Dim c1 = CInt(std.Ceiling((pt.X + radius - bounds.Left) / cellW))
            Dim r0 = CInt(std.Floor((pt.Y - radius - bounds.Top) / cellH))
            Dim r1 = CInt(std.Ceiling((pt.Y + radius - bounds.Top) / cellH))

            c0 = std.Max(0, c0) : c1 = std.Min(cols - 1, c1)
            r0 = std.Max(0, r0) : r1 = std.Min(rows - 1, r1)

            For ci As Integer = c0 To c1
                For ri As Integer = r0 To r1
                    Dim cx = bounds.Left + (ci + 0.5) * cellW
                    Dim cy = bounds.Top + (ri + 0.5) * cellH
                    Dim d2 = (cx - pt.X) ^ 2 + (cy - pt.Y) ^ 2

                    If d2 <= radius * radius Then
                        grid(ri, ci) += std.Exp(-d2 * inv2sigma2)
                    End If
                Next
            Next
        Next

        Dim maxV As Double = 0
        For i = 0 To rows - 1
            For j = 0 To cols - 1
                If grid(i, j) > maxV Then maxV = grid(i, j)
            Next
        Next
        If maxV > 0 Then
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    grid(i, j) /= maxV
                Next
            Next
        End If

        Return grid
    End Function

    ''' <summary>Catmull-Rom 样条平滑：把折线变成光滑曲线</summary>
    ''' <param name="points">原始顶点</param>
    ''' <param name="perSegment">每段插值点数</param>
    Public Function SmoothSpline(points As IEnumerable(Of PointF), Optional perSegment As Integer = 12) As PointF()
        Dim pts = points.SafeQuery.ToArray()
        If pts.Length < 3 OrElse perSegment < 1 Then Return pts

        Dim out As New List(Of PointF)(pts.Length * perSegment)

        For i = 0 To pts.Length - 2
            Dim p0 = pts(If(i > 0, i - 1, 0))
            Dim p1 = pts(i)
            Dim p2 = pts(i + 1)
            Dim p3 = pts(If(i + 2 < pts.Length, i + 2, pts.Length - 1))

            For s = 0 To perSegment - 1
                Dim t = s / perSegment
                Dim t2 = t * t
                Dim t3 = t2 * t

                Dim x = 0.5 * ((2 * p1.X) +
                               (-p0.X + p2.X) * t +
                               (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 +
                               (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3)
                Dim y = 0.5 * ((2 * p1.Y) +
                               (-p0.Y + p2.Y) * t +
                               (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                               (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3)

                out.Add(New PointF(CSng(x), CSng(y)))
            Next
        Next

        out.Add(pts(pts.Length - 1))
        Return out.ToArray()
    End Function

    ''' <summary>矩阵行 / 列 / 全局 min-max 缩放，用于热图数据预处理</summary>
    Public Enum ScaleMode
        None
        ByRow
        ByColumn
        ByGlobal
    End Enum

    ''' <summary>对矩阵做 min-max 归一化，返回新的矩阵（不修改入参）</summary>
    Public Function Normalize(matrix As Double(,), mode As ScaleMode) As Double(,)
        If matrix Is Nothing Then Throw New ArgumentNullException(NameOf(matrix))
        Dim rows = matrix.GetLength(0)
        Dim cols = matrix.GetLength(1)
        Dim out(rows - 1, cols - 1) As Double

        Select Case mode
            Case ScaleMode.None
                Array.Copy(matrix, out, matrix.Length)
                Return out

            Case ScaleMode.ByRow
                For i = 0 To rows - 1
                    NormalizeRange(matrix, out, i, rows, cols, True)
                Next
            Case ScaleMode.ByColumn
                For j = 0 To cols - 1
                    NormalizeRange(matrix, out, j, rows, cols, False)
                Next
            Case Else
                Dim lo = Double.MaxValue, hi = Double.MinValue
                For i = 0 To rows - 1
                    For j = 0 To cols - 1
                        If matrix(i, j) < lo Then lo = matrix(i, j)
                        If matrix(i, j) > hi Then hi = matrix(i, j)
                    Next
                Next
                For i = 0 To rows - 1
                    For j = 0 To cols - 1
                        out(i, j) = Remap(matrix(i, j), lo, hi, 0, 1)
                    Next
                Next
        End Select

        Return out
    End Function

    Private Sub NormalizeRange(src As Double(,), dst As Double(,), k As Integer,
                               rows As Integer, cols As Integer, byRow As Boolean)
        Dim lo = Double.MaxValue, hi = Double.MinValue
        Dim n = If(byRow, cols, rows)

        For i = 0 To n - 1
            Dim v = If(byRow, src(k, i), src(i, k))
            If v < lo Then lo = v
            If v > hi Then hi = v
        Next

        For i = 0 To n - 1
            Dim v = If(byRow, src(k, i), src(i, k))
            If byRow Then
                dst(k, i) = Remap(v, lo, hi, 0, 1)
            Else
                dst(i, k) = Remap(v, lo, hi, 0, 1)
            End If
        Next
    End Sub

    ''' <summary>Z-score 标准化一组向量（按列），返回新矩阵；对 mzKit 风格数据列可选 center/scale</summary>
    Public Function Standardize(matrix As Double(,), Optional center As Boolean = True,
                                Optional scale As Boolean = True) As Double(,)
        If matrix Is Nothing Then Throw New ArgumentNullException(NameOf(matrix))
        Dim rows = matrix.GetLength(0)
        Dim cols = matrix.GetLength(1)
        Dim out As Double(,) = CType(matrix.Clone(), Double(,))

        Dim mean(cols - 1) As Double
        Dim sd(cols - 1) As Double

        For j = 0 To cols - 1
            Dim sum As Double = 0
            For i = 0 To rows - 1
                sum += matrix(i, j)
            Next
            mean(j) = sum / rows

            Dim ss As Double = 0
            For i = 0 To rows - 1
                ss += (matrix(i, j) - mean(j)) ^ 2
            Next
            sd(j) = std.Sqrt(ss / If(rows > 1, rows - 1, 1))
        Next

        For i = 0 To rows - 1
            For j = 0 To cols - 1
                Dim v = matrix(i, j)
                If center Then v -= mean(j)
                If scale AndAlso sd(j) > 0 Then v /= sd(j)
                out(i, j) = v
            Next
        Next

        Return out
    End Function

    ''' <summary>把矩形限制为互不重叠的绘制区（简单的一维压缩，用于标签避让）</summary>
    Public Function PackHorizontally(widths As Single(), available As Single, min As Single) As Single()
        Dim n = widths.Length
        Dim pos(n - 1) As Single
        If n = 0 Then Return pos

        Dim scale As Double = 1
        Dim need = widths.Sum(Function(w) w) + min * (n - 1)
        If need > available AndAlso need > 0 Then scale = available / need

        Dim cursor As Single = 0
        For i = 0 To n - 1
            pos(i) = cursor
            cursor += CSng(widths(i) * scale) + CSng(min * scale)
        Next

        Return pos
    End Function

    ''' <summary>两个圆盘的交集面积（文氏图布局需要）</summary>
    Public Function CircleOverlapArea(r1 As Double, r2 As Double, d As Double) As Double
        If d <= 0 Then
            Dim r = std.Min(r1, r2)
            Return std.PI * r * r
        End If
        If d >= r1 + r2 Then Return 0
        If d <= std.Abs(r1 - r2) Then
            Dim r = std.Min(r1, r2)
            Return std.PI * r * r
        End If

        Dim a1 = r1 * r1 * std.Acos((d * d + r1 * r1 - r2 * r2) / (2 * d * r1))
        Dim a2 = r2 * r2 * std.Acos((d * d + r2 * r2 - r1 * r1) / (2 * d * r2))
        Dim a3 = 0.5 * std.Sqrt((-d + r1 + r2) * (d + r1 - r2) * (d - r1 + r2) * (d + r1 + r2))

        Return a1 + a2 - a3
    End Function

    ''' <summary>
    ''' Andrew monotone chain 凸包：返回按逆时针排列的边界顶点。
    ''' 用于把一组散点之外包络出来的轮廓（例如聚类分组的 Hull 显示）。
    ''' </summary>
    Public Function ConvexHull(points As IEnumerable(Of PointF)) As PointF()
        Dim pts = points.SafeQuery.ToArray()
        If pts.Length < 3 Then Return pts

        Dim sorted = pts.OrderBy(Function(p) p.X).ThenBy(Function(p) p.Y).ToArray()
        Dim lower As New List(Of PointF)()
        Dim upper As New List(Of PointF)()

        For Each p In sorted
            While lower.Count >= 2 AndAlso Cross(lower(lower.Count - 2), lower(lower.Count - 1), p) <= 0
                lower.RemoveAt(lower.Count - 1)
            End While
            lower.Add(p)
        Next

        For i = sorted.Length - 1 To 0 Step -1
            Dim p = sorted(i)
            While upper.Count >= 2 AndAlso Cross(upper(upper.Count - 2), upper(upper.Count - 1), p) <= 0
                upper.RemoveAt(upper.Count - 1)
            End While
            upper.Add(p)
        Next

        lower.RemoveAt(lower.Count - 1)
        upper.RemoveAt(upper.Count - 1)

        Return lower.Concat(upper).ToArray()
    End Function

    ''' <summary>由一组点估计出的置信椭圆参数</summary>
    Public Structure EllipseFit
        Public Property Center As PointF
        Public Property RadiusX As Single
        Public Property RadiusY As Single
        ''' <summary>椭圆长轴相对 X 轴的夹角（弧度）</summary>
        Public Property Rotation As Single
    End Structure

    ''' <summary>
    ''' 按二维协方差估计置信椭圆：中心在均值，长短轴来自协方差矩阵的特征值，
    ''' 夹角来自最大特征向量。<paramref name="k"/> 是标准差倍数（2 ≈ 95%）。
    ''' </summary>
    Public Function EstimateEllipse(points As IEnumerable(Of PointF), Optional k As Double = 2) As EllipseFit
        Dim pts = points.SafeQuery.ToArray()
        If pts.Length < 3 Then
            Return New EllipseFit With {.Center = If(pts.Length > 0, New PointF(pts.Average(Function(p) p.X), pts.Average(Function(p) p.Y)), New PointF()),
                                        .RadiusX = 1, .RadiusY = 1, .Rotation = 0}
        End If

        Dim mx = pts.Average(Function(p) p.X)
        Dim my = pts.Average(Function(p) p.Y)
        Dim sxx As Double = 0, syy As Double = 0, sxy As Double = 0

        For Each p In pts
            sxx += (p.X - mx) ^ 2
            syy += (p.Y - my) ^ 2
            sxy += (p.X - mx) * (p.Y - my)
        Next

        Dim n = pts.Length - 1
        sxx /= n : syy /= n : sxy /= n

        ' 2x2 对称矩阵的特征值解析解
        Dim tr = sxx + syy
        Dim det = sxx * syy - sxy * sxy
        Dim disc = std.Sqrt(std.Max(0, tr * tr / 4 - det))
        Dim l1 = tr / 2 + disc
        Dim l2 = tr / 2 - disc
        Dim angle = If(std.Abs(sxy) < 0.000000001,
                       If(sxx >= syy, 0.0, std.PI / 2),
                       std.Atan2(l1 - sxx, sxy))

        Return New EllipseFit With {
            .Center = New PointF(CSng(mx), CSng(my)),
            .RadiusX = CSng(k * std.Sqrt(std.Max(l1, 0))),
            .RadiusY = CSng(k * std.Sqrt(std.Max(l2, 0))),
            .Rotation = CSng(angle)
        }
    End Function

    Private Function Cross(o As PointF, a As PointF, b As PointF) As Double
        Return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X)
    End Function
End Module
