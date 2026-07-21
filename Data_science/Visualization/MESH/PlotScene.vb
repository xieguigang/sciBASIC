Imports System.Drawing
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.Drawing3D

''' <summary>三维图形渲染模式。</summary>
Public Enum RenderMode3D
    ''' <summary>完整曲面（光照填充），不影响 line / scatter。</summary>
    Surface = 0
    ''' <summary>点云：仅显示三维图形顶点，影响 surface 与 line，不影响 scatter。</summary>
    PointCloud = 1
    ''' <summary>边线：仅绘制 surface 多边形边（不填充），不影响 line / scatter。</summary>
    Edge = 2
End Enum

''' <summary>
''' 三维渲染核心：封装摄像机(Camera)与已生成的曲面/曲线/散点数据，
''' 负责质心居中、自动适配视距、热图着色（Designer 连续颜色谱）以及
''' 坐标轴、底面网格、盒子网格面与带刻度尺三维坐标轴的绘制。
''' 复用 Microsoft.VisualBasic.Imaging.Drawing3D 的画家算法 + 光照渲染能力。
''' </summary>
Public Class PlotScene

    Public Property Camera As New Camera()
    Public Property ColorScheme As String = "viridis"
    Public Property BackgroundColor As Color = Color.White
    Public Property ShowAxes As Boolean = True
    ''' <summary>是否绘制基于数据包围盒的三维盒子六个面的网格</summary>
    Public Property ShowBox As Boolean = True
    ''' <summary>是否绘制从包围盒角出发、带数字刻度的三维坐标轴</summary>
    Public Property ShowTicks As Boolean = False
    ''' <summary>三维图形渲染模式（surface / point cloud / edge）。</summary>
    Public Property RenderMode As RenderMode3D = RenderMode3D.Surface

    ' 原始（未居中）数据
    Private rawSurfaces As New List(Of Surface)()
    Private rawCurve As Point3D() = Nothing
    Private rawScatter As Point3D() = Nothing

    ' 居中后用于绘制
    Private surfaces As New List(Of Surface)()
    Private curvePoints As Point3D() = Nothing
    Private scatterPoints As Point3D() = Nothing

    Private modelR As Double = 100
    Private modelCenter As Point3D = New Point3D(0, 0, 0)

    Private colorTable As Color() = Nothing
    Private colorBrushes As Microsoft.VisualBasic.Imaging.SolidBrush() = Nothing
    Private colorTableScheme As String = ""

    Dim curveColors As Color()

    Private zMin As Double = 0
    Private zMax As Double = 1

    ' 数据包围盒（用于盒子网格与刻度尺）
    Private bxmin, bxmax, bymin, bymax, bzmin, bzmax As Double

    Public Sub New()
        Camera.AngleX = 20
        Camera.AngleY = -30
        Camera.AngleZ = 0
        Camera.FieldOfView = 256
        Camera.Offset = New PointF(0, 0)
        Camera.Screen = New Size(800, 600)
        Camera.AmbientStrength = 0
    End Sub

    Public ReadOnly Property SurfaceCount As Integer
        Get
            Return surfaces.Count
        End Get
    End Property

    Public ReadOnly Property PointCount As Integer
        Get
            Return If(curvePoints Is Nothing, 0, curvePoints.Length)
        End Get
    End Property

    Public ReadOnly Property HasData As Boolean
        Get
            Return surfaces.Count > 0 OrElse
                   (curvePoints IsNot Nothing AndAlso curvePoints.Length > 1) OrElse
                   (scatterPoints IsNot Nothing AndAlso scatterPoints.Length > 0)
        End Get
    End Property

    Public ReadOnly Property ModelRadius As Double
        Get
            Return modelR
        End Get
    End Property

    ' ===================== 清空 / 颜色谱 =====================

    Public Sub Clear()
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        surfaces.Clear()
        curvePoints = Nothing
        scatterPoints = Nothing
    End Sub

    Public Sub EnsureColorTable()
        If colorTable Is Nothing OrElse colorTableScheme <> ColorScheme Then
            colorTable = Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors(ColorScheme, 256, 255)
            colorTableScheme = ColorScheme
            colorBrushes = colorTable.Select(Function(c) New Microsoft.VisualBasic.Imaging.SolidBrush(c)).ToArray()
        End If
    End Sub

    ''' <summary>
    ''' 依据当前 colorTable 重新为所有已生成数据（曲面面片 / 曲线 / 折线）着色，
    ''' 并重建绘制用的居中数据，使切换颜色板后立即生效，且不改变当前视角与视距。
    ''' </summary>
    Public Sub Recolor()
        EnsureColorTable()
        If colorBrushes Is Nothing Then Return

        ' 重新计算曲面面片基色（按原始 z 在 [zMin, zMax] 归一化查色）
        For Each s In rawSurfaces
            Dim avgZ = s.vertices.Average(Function(p) p.Z)
            Dim t = (avgZ - zMin) / (zMax - zMin)
            Dim cidx = CInt(Math.Min(255, Math.Max(0, t * 255)))
            s.brush = colorBrushes(cidx)
        Next

        ' 重新计算曲线 / 折线渐变着色
        If curveColors IsNot Nothing AndAlso curveColors.Length > 1 Then
            Dim n = curveColors.Length - 1
            For i As Integer = 0 To n
                Dim cidx = CInt(Math.Min(255, Math.Max(0, i / n * 255)))
                curveColors(i) = colorTable(cidx)
            Next
        End If

        ' 重建居中后的绘制数据（沿用既有 modelCenter，不重置视角）
        RebuildCenteredData()
    End Sub

    ''' <summary>
    ''' 仅重建居中后的绘制数据（surfaces / curvePoints / scatterPoints），
    ''' 保持 modelCenter、modelR、包围盒与视距不变。
    ''' </summary>
    Private Sub RebuildCenteredData()
        surfaces = rawSurfaces.Select(Function(s) New Surface With {
            .brush = s.brush,
            .vertices = s.vertices.Select(Function(p) p.Subtract(modelCenter)).ToArray()
        }).ToList()
        curvePoints = If(rawCurve Is Nothing, Nothing, rawCurve.Select(Function(p) p.Subtract(modelCenter)).ToArray())
        scatterPoints = If(rawScatter Is Nothing, Nothing, rawScatter.Select(Function(p) p.Subtract(modelCenter)).ToArray())
    End Sub

    ' ===================== 数据生成：曲面（表达式） =====================

    ''' <summary>
    ''' 在 x∈[xMin,xMax]、y∈[yMin,yMax] 的网格上采样 z=f(x,y)，构造四边形面片，
    ''' 面片基色由平均 z 在 [zMin,zMax] 归一化后查 Designer 颜色谱得到。
    ''' </summary>
    Public Sub SetSurface(zEval As ExpressionEvaluator,
                          xMin#, xMax#, yMin#, yMax#, div%, scheme$)

        ColorScheme = scheme
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        EnsureColorTable()

        Dim nx = Math.Max(1, div), ny = Math.Max(1, div)
        Dim xs(nx) As Double, ys(ny) As Double
        For i As Integer = 0 To nx : xs(i) = xMin + (xMax - xMin) * i / nx : Next
        For j As Integer = 0 To ny : ys(j) = yMin + (yMax - yMin) * j / ny : Next

        ' 先整体求值，统计 z 范围（用于颜色归一化）
        Dim zs(nx, ny) As Double
        zMin = Double.MaxValue : zMax = Double.MinValue
        For i As Integer = 0 To nx
            For j As Integer = 0 To ny
                Dim z = zEval.Evaluate(xs(i), ys(j))
                zs(i, j) = z
                If Double.IsFinite(z) Then
                    If z < zMin Then zMin = z
                    If z > zMax Then zMax = z
                End If
            Next
        Next
        If zMax <= zMin Then zMax = zMin + 1

        For i As Integer = 0 To nx - 1
            For j As Integer = 0 To ny - 1
                Dim z00 = zs(i, j), z10 = zs(i + 1, j), z01 = zs(i, j + 1), z11 = zs(i + 1, j + 1)
                ' 含非有限值（NaN/Infinity）的四边形直接跳过，避免投影异常
                If Not (Double.IsFinite(z00) AndAlso Double.IsFinite(z10) AndAlso Double.IsFinite(z01) AndAlso Double.IsFinite(z11)) Then
                    Continue For
                End If

                Dim p00 = New Point3D(xs(i), ys(j), z00)
                Dim p10 = New Point3D(xs(i + 1), ys(j), z10)
                Dim p11 = New Point3D(xs(i + 1), ys(j + 1), z11)
                Dim p01 = New Point3D(xs(i), ys(j + 1), z01)

                Dim avgZ = (z00 + z10 + z01 + z11) / 4
                Dim t = (avgZ - zMin) / (zMax - zMin)
                Dim cidx = CInt(Math.Min(255, Math.Max(0, t * 255)))
                rawSurfaces.Add(New Surface With {.vertices = {p00, p10, p11, p01}, .brush = colorBrushes(cidx)})
            Next
        Next

        Recenter()
    End Sub

    ' ===================== 数据生成：参数曲线（表达式） =====================

    ''' <summary>
    ''' 在 t∈[tMin,tMax] 采样得到三维点序列，连接成折线，并按参数 t 做渐变着色。
    ''' </summary>
    Public Sub SetCurve(xEval As ExpressionEvaluator, yEval As ExpressionEvaluator, zEval As ExpressionEvaluator,
                        tMin#, tMax#, div%, scheme$)
        ColorScheme = scheme
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        EnsureColorTable()

        Dim n = Math.Max(1, div)
        Dim pts(n) As Point3D
        For i As Integer = 0 To n
            Dim t = tMin + (tMax - tMin) * i / n
            Dim x = xEval.Evaluate(t)
            Dim y = yEval.Evaluate(t)
            Dim z = zEval.Evaluate(t)
            ' 非有限值用 0 占位，避免折线断裂
            If Not Double.IsFinite(x) Then x = 0
            If Not Double.IsFinite(y) Then y = 0
            If Not Double.IsFinite(z) Then z = 0
            pts(i) = New Point3D(x, y, z)
        Next

        rawCurve = pts
        ReDim curveColors(n)
        For i As Integer = 0 To n
            Dim tt = i / n
            Dim cidx = CInt(Math.Min(255, Math.Max(0, tt * 255)))
            curveColors(i) = colorTable(cidx)
        Next

        Recenter()
    End Sub

    ' ===================== 数据生成：向量（脚本引擎产出） =====================

    ''' <summary>由脚本引擎产出的向量数据绘制三维/二维散点。</summary>
    Public Sub SetScatter(x As Double(), y As Double(), z As Double())
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        EnsureColorTable()

        Dim pts As New List(Of Point3D)()
        Dim n = If(x Is Nothing, 0, x.Length)
        For i As Integer = 0 To n - 1
            Dim zz = If(z IsNot Nothing AndAlso i < z.Length, z(i), 0)
            pts.Add(New Point3D(x(i), y(i), zz))
        Next
        rawScatter = pts.ToArray()
        Recenter()
    End Sub

    ''' <summary>由脚本引擎产出的向量数据绘制三维/二维曲线。</summary>
    Public Sub SetLine(x As Double(), y As Double(), z As Double())
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        EnsureColorTable()

        Dim n = If(x Is Nothing, 0, x.Length)
        Dim pts(n - 1) As Point3D
        For i As Integer = 0 To n - 1
            Dim zz = If(z IsNot Nothing AndAlso i < z.Length, z(i), 0)
            pts(i) = New Point3D(x(i), y(i), zz)
        Next
        rawCurve = pts

        ReDim curveColors(n - 1)
        For i As Integer = 0 To n - 1
            Dim tt = If(n > 1, i / (n - 1), 0)
            Dim cidx = CInt(Math.Min(255, Math.Max(0, tt * 255)))
            curveColors(i) = colorTable(cidx)
        Next

        Recenter()
    End Sub

    ''' <summary>
    ''' 由脚本引擎产出的网格数据绘制三维曲面。
    ''' ZGrid(i)(j)：i 沿 Y 轴、j 沿 X 轴。
    ''' </summary>
    Public Sub SetSurface(x As Double(), y As Double(), zGrid As Double()())
        rawSurfaces.Clear()
        rawCurve = Nothing
        rawScatter = Nothing
        EnsureColorTable()

        zMin = Double.MaxValue : zMax = Double.MinValue
        For i As Integer = 0 To y.Length - 1
            For j As Integer = 0 To x.Length - 1
                Dim z = zGrid(i)(j)
                If Double.IsFinite(z) Then
                    If z < zMin Then zMin = z
                    If z > zMax Then zMax = z
                End If
            Next
        Next
        If zMax <= zMin Then zMax = zMin + 1

        For iX As Integer = 0 To x.Length - 2
            For jY As Integer = 0 To y.Length - 2
                Dim z00 = zGrid(jY)(iX)
                Dim z10 = zGrid(jY + 1)(iX)
                Dim z11 = zGrid(jY + 1)(iX + 1)
                Dim z01 = zGrid(jY)(iX + 1)
                If Not (Double.IsFinite(z00) AndAlso Double.IsFinite(z10) AndAlso Double.IsFinite(z01) AndAlso Double.IsFinite(z11)) Then
                    Continue For
                End If

                Dim p00 = New Point3D(x(iX), y(jY), z00)
                Dim p10 = New Point3D(x(iX), y(jY + 1), z10)
                Dim p11 = New Point3D(x(iX + 1), y(jY + 1), z11)
                Dim p01 = New Point3D(x(iX + 1), y(jY), z01)

                Dim avgZ = (z00 + z10 + z01 + z11) / 4
                Dim t = (avgZ - zMin) / (zMax - zMin)
                Dim cidx = CInt(Math.Min(255, Math.Max(0, t * 255)))
                rawSurfaces.Add(New Surface With {.vertices = {p00, p10, p11, p01}, .brush = colorBrushes(cidx)})
            Next
        Next

        Recenter()
    End Sub

    ' ===================== 居中 + 自动视距 =====================

    Private Sub Recenter()
        Dim allPts As New List(Of Point3D)()
        For Each s In rawSurfaces : allPts.AddRange(s.vertices)
        Next
        If rawCurve IsNot Nothing Then allPts.AddRange(rawCurve)
        If rawScatter IsNot Nothing Then allPts.AddRange(rawScatter)

        If allPts.Count = 0 Then
            modelCenter = New Point3D(0, 0, 0)
            modelR = 1
            bxmin = 0 : bxmax = 0 : bymin = 0 : bymax = 0 : bzmin = 0 : bzmax = 0
            surfaces.Clear()
            curvePoints = Nothing
            scatterPoints = Nothing
            Return
        End If

        Dim cx = allPts.Average(Function(p) p.X)
        Dim cy = allPts.Average(Function(p) p.Y)
        Dim cz = allPts.Average(Function(p) p.Z)
        modelCenter = New Point3D(cx, cy, cz)

        ' 以质心为原点居中，保证旋转围绕模型中心
        RebuildCenteredData()

        ' 数据包围盒
        bxmin = allPts.Min(Function(p) p.X) : bxmax = allPts.Max(Function(p) p.X)
        bymin = allPts.Min(Function(p) p.Y) : bymax = allPts.Max(Function(p) p.Y)
        bzmin = allPts.Min(Function(p) p.Z) : bzmax = allPts.Max(Function(p) p.Z)

        Dim maxR As Double = allPts.Max(Function(p) Point3D.Distance(p, modelCenter))
        modelR = If(maxR <= 0, 1, maxR)
        FitView()
    End Sub

    ''' <summary>
    ''' 依据模型包围球半径与画布尺寸自动计算视距，使模型初始完整可见；
    ''' 通过放大 FOV 与视距（perspectiveK）弱化透视畸变。
    ''' </summary>
    Public Sub FitView()
        Dim minDim = Math.Min(Camera.Screen.Width, Camera.Screen.Height)
        If minDim <= 0 Then minDim = 600
        Const perspectiveK As Double = 8
        Camera.FieldOfView = CSng(256 * perspectiveK)
        Dim vd As Double = modelR * Camera.FieldOfView / (0.4 * minDim)
        If vd < 1 Then vd = 1
        Camera.ViewDistance = CSng(vd)
    End Sub

    ' ===================== 绘制 =====================

    Public Sub Draw(g As Graphics, canvas As Size)
        Camera.Screen = canvas
        g.Clear(BackgroundColor)

        Dim hasSurf = surfaces.Count > 0
        Dim hasLine = curvePoints IsNot Nothing AndAlso curvePoints.Length > 1
        Dim hasScatter = scatterPoints IsNot Nothing AndAlso scatterPoints.Length > 0
        If Not (hasSurf OrElse hasLine OrElse hasScatter) Then Return

        If ShowBox Then DrawBox(g)
        If ShowTicks Then DrawRuler(g)
        If ShowAxes Then DrawGround(g)

        ' 画家算法 + 光照（热图基色在光照下叠加明暗），按渲染模式分派
        If hasSurf Then
            Select Case RenderMode
                Case RenderMode3D.Surface
                    Camera.Draw(g, surfaces, drawPath:=False)
                Case RenderMode3D.PointCloud
                    DrawSurfacePoints(g)
                Case RenderMode3D.Edge
                    DrawSurfaceEdges(g)
            End Select
        End If
        If hasLine Then
            If RenderMode = RenderMode3D.PointCloud Then DrawCurvePoints(g) Else DrawCurve(g)
        End If
        If hasScatter Then DrawScatter(g)
        If ShowAxes Then DrawAxes(g)
    End Sub

    ''' <summary>从 Brush 提取颜色；仅 SolidBrush 可提色，其余回退为 Black。</summary>
    Private Function GetBrushColor(b As Brush) As Color
        If TypeOf b Is SolidBrush Then Return DirectCast(b, SolidBrush).Color
        Return Color.Black
    End Function

    ''' <summary>点云模式：将每个 surface 多边形的顶点绘制为小圆点，使用各自基色。</summary>
    Private Sub DrawSurfacePoints(g As Graphics)
        If surfaces.Count = 0 Then Return
        Dim polys = Camera.PainterBuffer(surfaces, illumination:=False).ToArray()
        Dim r As Single = 2.5F
        For Each poly In polys
            Dim col = GetBrushColor(poly.brush)
            Using b As New SolidBrush(col)
                For Each pt In poly.points
                    g.FillEllipse(b, pt.X - r, pt.Y - r, 2 * r, 2 * r)
                Next
            End Using
        Next
    End Sub

    ''' <summary>边线模式：仅绘制 surface 多边形边，不填充；边框色取自 surface 的 brush 基色。</summary>
    Private Sub DrawSurfaceEdges(g As Graphics)
        If surfaces.Count = 0 Then Return
        Dim polys = Camera.PainterBuffer(surfaces, illumination:=False).ToArray()
        For Each poly In polys
            Dim col = GetBrushColor(poly.brush)
            Using pen As New Pen(col, 1)
                g.DrawPolygon(pen, poly.points)
            End Using
        Next
    End Sub

    ''' <summary>点云模式：将折线的每个顶点绘制为小圆点，使用既有的曲线渐变色。</summary>
    Private Sub DrawCurvePoints(g As Graphics)
        If curvePoints Is Nothing Then Return
        Dim r As Single = 3.0F
        For i As Integer = 0 To curvePoints.Length - 1
            Dim s = ToScreen(curvePoints(i))
            Dim ci = If(i < curveColors.Length, i, If(curveColors.Length > 0, curveColors.Length - 1, 0))
            Dim col = If(curveColors IsNot Nothing AndAlso curveColors.Length > 0, curveColors(ci), Color.Black)
            Using b As New SolidBrush(col)
                g.FillEllipse(b, s.X - r, s.Y - r, 2 * r, 2 * r)
            End Using
        Next
    End Sub

    Private Function ToScreen(p As Point3D) As PointF
        Dim rotated = Camera.Rotate(p)
        Dim projected = Camera.Project(rotated)
        Return New PointF(CSng(projected.X), CSng(projected.Y))
    End Function

    ''' <summary>将“数据坐标”点先居中再投影到屏幕（盒子/刻度尺使用）。</summary>
    Private Function ToScreenData(p As Point3D) As PointF
        Return ToScreen(New Point3D(p.X - modelCenter.X, p.Y - modelCenter.Y, p.Z - modelCenter.Z))
    End Function

    ''' <summary>绘制基于数据包围盒的三维盒子六个面的网格。</summary>
    Private Sub DrawBox(g As Graphics)
        Dim x0 = bxmin, x1 = bxmax, y0 = bymin, y1 = bymax, z0 = bzmin, z1 = bzmax
        Dim c(7) As Point3D
        c(0) = New Point3D(x0, y0, z0) : c(1) = New Point3D(x1, y0, z0)
        c(2) = New Point3D(x1, y1, z0) : c(3) = New Point3D(x0, y1, z0)
        c(4) = New Point3D(x0, y0, z1) : c(5) = New Point3D(x1, y0, z1)
        c(6) = New Point3D(x1, y1, z1) : c(7) = New Point3D(x0, y1, z1)

        Dim edges = {
            New Integer() {0, 1}, New Integer() {1, 2}, New Integer() {2, 3}, New Integer() {3, 0},
            New Integer() {4, 5}, New Integer() {5, 6}, New Integer() {6, 7}, New Integer() {7, 4},
            New Integer() {0, 4}, New Integer() {1, 5}, New Integer() {2, 6}, New Integer() {3, 7}
        }

        Using pen As New Pen(Color.FromArgb(150, 170, 170, 170), 1)
            For Each e In edges
                g.DrawLine(pen, ToScreenData(c(e(0))), ToScreenData(c(e(1))))
            Next

            Dim div = 4
            ' X-Y 面 (z0, z1)
            For f = 0 To 1
                Dim z = If(f = 0, z0, z1)
                For i = 1 To div - 1
                    Dim tx = x0 + (x1 - x0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(tx, y0, z)), ToScreenData(New Point3D(tx, y1, z)))
                    Dim ty = y0 + (y1 - y0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(x0, ty, z)), ToScreenData(New Point3D(x1, ty, z)))
                Next
            Next
            ' X-Z 面 (y0, y1)
            For f = 0 To 1
                Dim y = If(f = 0, y0, y1)
                For i = 1 To div - 1
                    Dim tx = x0 + (x1 - x0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(tx, y, z0)), ToScreenData(New Point3D(tx, y, z1)))
                    Dim tz = z0 + (z1 - z0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(x0, y, tz)), ToScreenData(New Point3D(x1, y, tz)))
                Next
            Next
            ' Y-Z 面 (x0, x1)
            For f = 0 To 1
                Dim x = If(f = 0, x0, x1)
                For i = 1 To div - 1
                    Dim ty = y0 + (y1 - y0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(x, ty, z0)), ToScreenData(New Point3D(x, ty, z1)))
                    Dim tz = z0 + (z1 - z0) * i / div
                    g.DrawLine(pen, ToScreenData(New Point3D(x, y0, tz)), ToScreenData(New Point3D(x, y1, tz)))
                Next
            Next
        End Using
    End Sub

    ''' <summary>绘制从包围盒角出发、带数字刻度的三维坐标轴（带刻度尺）。</summary>
    Private Sub DrawRuler(g As Graphics)
        Dim x0 = bxmin, x1 = bxmax, y0 = bymin, y1 = bymax, z0 = bzmin, z1 = bzmax
        Dim o = ToScreenData(New Point3D(x0, y0, z0))
        Dim ax = ToScreenData(New Point3D(x1, y0, z0))
        Dim ay = ToScreenData(New Point3D(x0, y1, z0))
        Dim az = ToScreenData(New Point3D(x0, y0, z1))
        Using penX As New Pen(Color.DarkRed, 2) : g.DrawLine(penX, o, ax) : End Using
        Using penY As New Pen(Color.DarkGreen, 2) : g.DrawLine(penY, o, ay) : End Using
        Using penZ As New Pen(Color.DarkBlue, 2) : g.DrawLine(penZ, o, az) : End Using

        Using f As New Font("Segoe UI", 8)
            For Each tk In NiceTicks(x0, x1)
                Dim p1 = ToScreenData(New Point3D(tk, y0, z0))
                Dim p2 = ToScreenData(New Point3D(tk, y0 - (y1 - y0) * 0.02, z0))
                g.DrawLine(Pens.Gray, p1, p2)
                g.DrawString(FormatTick(tk), f, Brushes.DarkRed, p1)
            Next
            For Each tk In NiceTicks(y0, y1)
                Dim p1 = ToScreenData(New Point3D(x0, tk, z0))
                Dim p2 = ToScreenData(New Point3D(x0 - (x1 - x0) * 0.02, tk, z0))
                g.DrawLine(Pens.Gray, p1, p2)
                g.DrawString(FormatTick(tk), f, Brushes.DarkGreen, p1)
            Next
            For Each tk In NiceTicks(z0, z1)
                Dim p1 = ToScreenData(New Point3D(x0, y0, tk))
                Dim p2 = ToScreenData(New Point3D(x0 - (x1 - x0) * 0.02, y0, tk))
                g.DrawLine(Pens.Gray, p1, p2)
                g.DrawString(FormatTick(tk), f, Brushes.DarkBlue, p1)
            Next
        End Using
    End Sub

    Private Function NiceTicks(min As Double, max As Double) As Double()
        If Not Double.IsFinite(min) OrElse Not Double.IsFinite(max) OrElse max <= min Then Return New Double() {}
        Dim span = max - min
        Dim rawStep = span / 6
        Dim mag = Math.Pow(10, Math.Floor(Math.Log10(rawStep)))
        Dim norm = rawStep / mag
        Dim stepv = If(norm < 1.5, 1, If(norm < 3, 2, If(norm < 7, 5, 10))) * mag
        Dim ticks As New List(Of Double)()
        Dim start = Math.Ceiling(min / stepv) * stepv
        Dim v = start
        Dim guard = 0
        Do While v <= max + stepv * 0.5 AndAlso guard < 1000
            ticks.Add(v)
            v += stepv
            guard += 1
        Loop
        Return ticks.ToArray()
    End Function

    Private Function FormatTick(v As Double) As String
        If Math.Abs(v) >= 1000 OrElse (Math.Abs(v) > 0 AndAlso Math.Abs(v) < 0.001) Then
            Return v.ToString("0.##E+0")
        End If
        Return v.ToString("0.##")
    End Function

    ''' <summary>绘制三维散点。</summary>
    Private Sub DrawScatter(g As Graphics)
        Using b As New SolidBrush(Color.FromArgb(210, 30, 80, 180))
            For Each p In scatterPoints
                Dim s = ToScreen(p)
                g.FillEllipse(b, s.X - 3, s.Y - 3, 6, 6)
            Next
        End Using
    End Sub

    ''' <summary>在模型底部（z = -modelR）绘制 X-Y 参考网格。</summary>
    Private Sub DrawGround(g As Graphics)
        Dim half As Double = modelR
        Dim divisions As Integer = 10
        Dim stepv = (2 * half) / divisions
        Dim z As Double = -modelR
        Using pen As New Pen(Color.FromArgb(200, 210, 210, 210), 1)
            For i As Integer = 0 To divisions
                Dim t = -half + i * stepv
                Dim a = ToScreen(New Point3D(-half, t, z))
                Dim b = ToScreen(New Point3D(half, t, z))
                g.DrawLine(pen, a, b)
                Dim c = ToScreen(New Point3D(t, -half, z))
                Dim d = ToScreen(New Point3D(t, half, z))
                g.DrawLine(pen, c, d)
            Next
        End Using
    End Sub

    ''' <summary>绘制 X/Y/Z 三坐标轴及标签。</summary>
    Private Sub DrawAxes(g As Graphics)
        Dim o = ToScreen(New Point3D(0, 0, 0))
        Dim ax = ToScreen(New Point3D(modelR, 0, 0))
        Dim ay = ToScreen(New Point3D(0, modelR, 0))
        Dim az = ToScreen(New Point3D(0, 0, modelR))
        Using penX As New Pen(Color.FromArgb(220, 200, 60, 60), 2) : g.DrawLine(penX, o, ax) : End Using
        Using penY As New Pen(Color.FromArgb(220, 60, 150, 60), 2) : g.DrawLine(penY, o, ay) : End Using
        Using penZ As New Pen(Color.FromArgb(220, 60, 60, 200), 2) : g.DrawLine(penZ, o, az) : End Using
        Using f As New Font("Segoe UI", 10, FontStyle.Bold)
            g.DrawString("X", f, Brushes.DarkRed, ax)
            g.DrawString("Y", f, Brushes.DarkGreen, ay)
            g.DrawString("Z", f, Brushes.DarkBlue, az)
        End Using
    End Sub

    Private Sub DrawCurve(g As Graphics)
        For i As Integer = 0 To curvePoints.Length - 2
            Dim a = ToScreen(curvePoints(i))
            Dim b = ToScreen(curvePoints(i + 1))
            Using pen As New Pen(curveColors(i), 2)
                g.DrawLine(pen, a, b)
            End Using
        Next
    End Sub

End Class
