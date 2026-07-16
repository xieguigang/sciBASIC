Imports System.Drawing
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

''' <summary>
''' 三维渲染核心：封装摄像机(Camera)与已生成的曲面/曲线数据，
''' 负责质心居中、自动适配视距、热图着色（Designer 连续颜色谱）以及
''' 坐标轴与底面网格的绘制。复用 Microsoft.VisualBasic.Imaging.Drawing3D 的
''' 画家算法 + 光照渲染能力。
''' </summary>
Public Class PlotScene

    Public Property Camera As New Camera()
    Public Property ColorScheme As String = "viridis"
    Public Property BackgroundColor As Color = Color.White
    Public Property ShowAxes As Boolean = True

    Private surfaces As New List(Of Surface)()
    Private curvePoints As Point3D() = Nothing
    Private curveColors As Color() = Nothing

    Private modelR As Double = 100
    Private modelCenter As Point3D = New Point3D(0, 0, 0)

    Private colorTable As Color() = Nothing
    Private colorBrushes As SolidBrush() = Nothing
    Private colorTableScheme As String = ""

    Private zMin As Double = 0
    Private zMax As Double = 1

    Public Sub New()
        Camera.AngleX = 20
        Camera.AngleY = -30
        Camera.AngleZ = 0
        Camera.FieldOfView = 256
        Camera.Offset = New PointF(0, 0)
        Camera.Screen = New Size(800, 600)
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
            Return surfaces.Count > 0 OrElse (curvePoints IsNot Nothing AndAlso curvePoints.Length > 1)
        End Get
    End Property

    Public ReadOnly Property ModelRadius As Double
        Get
            Return modelR
        End Get
    End Property

    ' ===================== 颜色谱 =====================

    Private Sub EnsureColorTable()
        If colorTable Is Nothing OrElse colorTableScheme <> ColorScheme Then
            colorTable = Designer.GetColors(ColorScheme, 256, 255)
            colorTableScheme = ColorScheme
            colorBrushes = colorTable.Select(Function(c) New SolidBrush(c)).ToArray()
        End If
    End Sub

    ' ===================== 数据生成：曲面 =====================

    ''' <summary>
    ''' 在 x∈[xMin,xMax]、y∈[yMin,yMax] 的网格上采样 z=f(x,y)，构造四边形面片，
    ''' 面片基色由平均 z 在 [zMin,zMax] 归一化后查 Designer 颜色谱得到。
    ''' </summary>
    Public Sub SetSurface(zEval As ExpressionEvaluator,
                          xMin#, xMax#, yMin#, yMax#, div%, scheme$)

        ColorScheme = scheme
        surfaces.Clear()
        curvePoints = Nothing
        EnsureColorTable()

        Dim nx = Math.Max(1, div), ny = Math.Max(1, div)
        Dim xs(nx) As Double, ys(ny) As Double
        For i As Integer = 0 To nx : xs(i) = xMin + (xMax - xMin) * i / nx : Next
        For j As Integer = 0 To ny : ys(j) = yMin + (yMax - yMin) * j / ny : Next

        ' 先整体求值，统计 z 范围（用于颜色归一化），并记录点
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

        Dim allPts As New List(Of Point3D)()
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
                allPts.AddRange({p00, p10, p11, p01})

                Dim avgZ = (z00 + z10 + z01 + z11) / 4
                Dim t = (avgZ - zMin) / (zMax - zMin)
                Dim cidx = CInt(Math.Min(255, Math.Max(0, t * 255)))
                surfaces.Add(New Surface With {.vertices = {p00, p10, p11, p01}, .brush = colorBrushes(cidx)})
            Next
        Next

        CenterModel(allPts)
        FitView()
    End Sub

    ' ===================== 数据生成：参数曲线 =====================

    ''' <summary>
    ''' 在 t∈[tMin,tMax] 采样得到三维点序列，连接成折线，并按参数 t 做渐变着色。
    ''' </summary>
    Public Sub SetCurve(xEval As ExpressionEvaluator, yEval As ExpressionEvaluator, zEval As ExpressionEvaluator,
                        tMin#, tMax#, div%, scheme$)
        ColorScheme = scheme
        surfaces.Clear()
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

        curvePoints = pts
        ReDim curveColors(n)
        For i As Integer = 0 To n
            Dim tt = i / n
            Dim cidx = CInt(Math.Min(255, Math.Max(0, tt * 255)))
            curveColors(i) = colorTable(cidx)
        Next

        CenterModel(pts.ToList())
        FitView()
    End Sub

    ' ===================== 居中 + 自动视距 =====================

    Private Sub CenterModel(allPts As List(Of Point3D))
        If allPts.Count = 0 Then
            modelCenter = New Point3D(0, 0, 0)
            modelR = 1
            Return
        End If

        Dim cx = allPts.Average(Function(p) p.X)
        Dim cy = allPts.Average(Function(p) p.Y)
        Dim cz = allPts.Average(Function(p) p.Z)
        modelCenter = New Point3D(cx, cy, cz)

        ' 以质心为原点居中，保证旋转围绕模型中心
        For i As Integer = 0 To surfaces.Count - 1
            surfaces(i) = New Surface With {
                .brush = surfaces(i).brush,
                .vertices = surfaces(i).vertices.Select(Function(p) p.Subtract(modelCenter)).ToArray()
            }
        Next
        If curvePoints IsNot Nothing Then
            curvePoints = curvePoints.Select(Function(p) p.Subtract(modelCenter)).ToArray()
        End If

        Dim maxR As Double = 0
        For Each p As Point3D In allPts
            Dim d = Point3D.Distance(p, modelCenter)
            If d > maxR Then maxR = d
        Next
        modelR = If(maxR <= 0, 1, maxR)
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

        If surfaces.Count > 0 Then
            If ShowAxes Then DrawGround(g)
            ' 画家算法 + 光照（热图基色在光照下叠加明暗）
            Camera.Draw(g, surfaces, drawPath:=False)
            If ShowAxes Then DrawAxes(g)
        ElseIf curvePoints IsNot Nothing AndAlso curvePoints.Length > 1 Then
            If ShowAxes Then DrawGround(g)
            DrawCurve(g)
            If ShowAxes Then DrawAxes(g)
        End If
    End Sub

    Private Function ToScreen(p As Point3D) As PointF
        Dim rotated = Camera.Rotate(p)
        Dim projected = Camera.Project(rotated)
        Return New PointF(CSng(projected.X), CSng(projected.Y))
    End Function

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
