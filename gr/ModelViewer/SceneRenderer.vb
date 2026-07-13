Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Landscape.Ply

''' <summary>
''' 封装摄像机(Camera)与已加载的模型/点云状态，负责居中、自动适配视距以及三种渲染模式。
''' 复用 Microsoft.VisualBasic.Imaging.Drawing3D 的三维渲染能力，点云热图颜色来自
''' Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors。
''' </summary>
Public Class SceneRenderer

    Public Property Camera As New Camera()
    Public Property Mode As RenderMode = RenderMode.Surface
    ''' <summary>传入 Designer.GetColors 的 term，例如 "viridis" / "magma" / "turbo" 等</summary>
    Public Property ColorScheme As String = "viridis"
    Public Property PointSize As Integer = 2
    Public Property PointAlpha As Integer = 255
    Public Property UseEmbeddedColor As Boolean = False
    ''' <summary>是否在模型底部绘制表示地面的方形网格。</summary>
    Public Property ShowGround As Boolean = True
    ''' <summary>地面网格的线条颜色。</summary>
    Public Property GroundColor As Color = Color.Gray
    ''' <summary>画布（天空盒）背景色。</summary>
    Public Property BackgroundColor As Color = Color.White

    Private surfaces As New List(Of Surface)()
    Private cloud As PointCloud() = Nothing
    Private modelRadius As Double = 100
    Private intensityMin As Double = 0
    Private intensityMax As Double = 1

    Private colorTable As Color() = Nothing
    Private colorBrushes As System.Drawing.SolidBrush() = Nothing
    Private colorTableScheme As String = ""
    Private colorTableAlpha As Integer = -1
    Private embeddedCache As New Dictionary(Of String, System.Drawing.SolidBrush)()
    ''' <summary>地面网格所在的 Z 高度（模型最低点），保证网格正好托在模型底部。</summary>
    Private groundZ As Double = -100

    Public ReadOnly Property SurfaceCount As Integer
        Get
            Return surfaces.Count
        End Get
    End Property

    Public ReadOnly Property PointCount As Integer
        Get
            If cloud Is Nothing Then Return 0
            Return cloud.Length
        End Get
    End Property

    Public ReadOnly Property HasData As Boolean
        Get
            Return surfaces.Count > 0 OrElse (cloud IsNot Nothing AndAlso cloud.Length > 0)
        End Get
    End Property

    Public Sub New()
        Camera.AngleX = 20
        Camera.AngleY = -30
        Camera.AngleZ = 0
        Camera.FieldOfView = 256
        Camera.Offset = New PointF(0, 0)
        Camera.Screen = New Size(800, 600)
    End Sub

    ''' <summary>
    ''' 加载多格式三维模型（STL/glTF/GLB/OBJ/DAE/3DS/3MF），统一转为 Drawing3D.Surface 并居中适配。
    ''' </summary>
    Public Sub LoadModel(filePath As String)
        Dim scene = Microsoft.VisualBasic.Imaging.Landscape.Data.ModelLoader.LoadModel(filePath)
        Me.cloud = Nothing
        surfaces.Clear()

        Dim allPts As New List(Of Point3D)()
        If scene.Surfaces IsNot Nothing Then
            For Each s In scene.Surfaces
                Dim obj = s.CreateObject()
                If obj IsNot Nothing AndAlso obj.vertices IsNot Nothing AndAlso obj.vertices.Length >= 3 Then
                    allPts.AddRange(obj.vertices)
                    surfaces.Add(obj)
                End If
            Next
        End If

        Dim center = Centroid(allPts)
        modelRadius = Radius(allPts, center)

        ' 以质心为原点居中，保证旋转围绕模型中心
        For i = 0 To surfaces.Count - 1
            Dim s = surfaces(i)
            surfaces(i) = New Surface With {
                .brush = s.brush,
                .vertices = s.vertices.Select(Function(p) p.Subtract(center)).ToArray()
            }
        Next

        ' 记录模型最低点，作为地面网格的高度
        groundZ = Double.MaxValue
        For Each s In surfaces
            For Each p In s.vertices
                If p.Z < groundZ Then groundZ = p.Z
            Next
        Next
        If groundZ = Double.MaxValue Then groundZ = -modelRadius

        Camera.AngleX = 20
        Camera.AngleY = -30
        Camera.AngleZ = 0
        Camera.Offset = New PointF(0, 0)
        Mode = RenderMode.Surface
    End Sub

    ''' <summary>
    ''' 加载 PLY 点云，居中适配，并记录 intensity 范围用于热图着色。
    ''' </summary>
    Public Sub LoadPointCloud(filePath As String)
        Me.cloud = Microsoft.VisualBasic.Imaging.Landscape.Ply.PlyReader.ReadFile(filePath)
        surfaces.Clear()
        embeddedCache.Clear()

        If cloud Is Nothing Then cloud = New PointCloud() {}

        Dim pts = cloud.Select(Function(c) New Point3D(c.x, c.y, c.z)).ToList()
        Dim center = Centroid(pts)
        modelRadius = Radius(pts, center)

        Dim imin = Double.MaxValue, imax = Double.MinValue
        For i = 0 To cloud.Length - 1
            cloud(i).x -= center.X
            cloud(i).y -= center.Y
            cloud(i).z -= center.Z
            If cloud(i).intensity < imin Then imin = cloud(i).intensity
            If cloud(i).intensity > imax Then imax = cloud(i).intensity
        Next
        If imax <= imin Then
            imin = 0 : imax = 1
        End If
        intensityMin = imin
        intensityMax = imax

        ' 记录点云最低点，作为地面网格的高度
        groundZ = Double.MaxValue
        For i = 0 To cloud.Length - 1
            If cloud(i).z < groundZ Then groundZ = cloud(i).z
        Next
        If groundZ = Double.MaxValue Then groundZ = -modelRadius

        Camera.AngleX = 20
        Camera.AngleY = -30
        Camera.AngleZ = 0
        Camera.Offset = New PointF(0, 0)
        Mode = RenderMode.PointCloud
        colorTable = Nothing
    End Sub

        ''' <summary>
        ''' 根据模型包围球半径与当前画布尺寸自动计算视距，使模型初始完整可见。
        ''' 引入透视强度系数 perspectiveK：让 viewDistance 远大于模型半径，
        ''' 从而把近处面的透视放大比降到自然范围（≈1.2×），避免模型被夸张畸变。
        ''' 由于铺满比例 modelRadius·fov/viewDistance = 0.4·minDim 与 fov 无关，
        ''' 同步放大 fov 与 viewDistance 可保持模型正好铺满、同时弱化透视。
        ''' </summary>
        Public Sub FitView()
            Dim minDim = Math.Min(Camera.Screen.Width, Camera.Screen.Height)
            If minDim <= 0 Then minDim = 600
            ' 透视强度系数：越大透视越弱（更接近等距投影）。可在此调节。
            Const perspectiveK As Double = 8
            Camera.FieldOfView = CSng(256 * perspectiveK)
            Dim vd As Double = modelRadius * Camera.FieldOfView / (0.4 * minDim)
            If vd < 1 Then vd = 1
            Camera.ViewDistance = CSng(vd)
        End Sub

    Public Sub Draw(g As Graphics, canvas As Size)
        Camera.Screen = canvas
        g.Clear(BackgroundColor)

        If surfaces.Count > 0 Then
            DrawGround(g)
            Select Case Mode
                Case RenderMode.Mesh
                    DrawMesh(g)
                Case RenderMode.PointCloud
                    ' 即使加载的是实体模型，也强制以点云（仅顶点）形式显示，
                    ' 顶点颜色按光照强度做热图着色。
                    DrawModelAsPointCloud(g)
                Case Else
                    ' 表面模式：复用库内 Camera.Draw（含旋转/投影/画家算法/光照）
                    Camera.Draw(g, surfaces, drawPath:=False)
            End Select
        ElseIf cloud IsNot Nothing AndAlso cloud.Length > 0 Then
            DrawGround(g)
            DrawPointCloud(g)
        End If
    End Sub

    ''' <summary>
    ''' 在模型底部绘制表示地面的方形三维网格。先绘制于模型之前，
    ''' 使不透明的模型表面能正确遮挡位于其后的网格线（近处地面在屏幕下方，
    ''' 通常不与模型重叠，因而仍可见）。网格位于 X-Y 平面、Z = groundZ，
    ''' 随相机一起旋转与透视投影，呈现为水平地面。
    ''' </summary>
    Private Sub DrawGround(g As Graphics)
        If Not ShowGround Then Return

        Dim half As Double = modelRadius * 2.0
        Dim divisions As Integer = 20
        Dim stepv As Double = (2.0 * half) / divisions
        Dim z As Double = groundZ

        Using pen As New System.Drawing.Pen(GroundColor, 1)
            For i As Integer = 0 To divisions
                Dim t = -half + i * stepv
                ' 沿 X 方向（y = t 固定）
                Dim a = ToScreen(New Point3D(-half, t, z))
                Dim b = ToScreen(New Point3D(half, t, z))
                g.DrawLine(pen, a, b)
                ' 沿 Y 方向（x = t 固定）
                Dim c = ToScreen(New Point3D(t, -half, z))
                Dim d = ToScreen(New Point3D(t, half, z))
                g.DrawLine(pen, c, d)
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 将世界坐标点经摄像机旋转、投影后转换为屏幕二维坐标。
    ''' </summary>
    Private Function ToScreen(p As Point3D) As PointF
        Dim rotated = Camera.Rotate(p)
        Dim projected = Camera.Project(rotated)
        Return New PointF(CSng(projected.X), CSng(projected.Y))
    End Function

    ''' <summary>
    ''' 三角网格（线框）模式：对每个面旋转+投影后仅描边，不填充。
    ''' </summary>
    Private Sub DrawMesh(g As Graphics)
        Using pen As New System.Drawing.Pen(Color.FromArgb(200, 30, 30, 30), 1)
            For Each s In surfaces
                Dim projected = Camera.Project(Camera.Rotate(s.vertices)).ToArray()
                Dim pts = projected.Select(Function(p) p.PointXY(Camera.Screen)).ToArray()
                If pts.Length >= 2 Then
                    g.DrawPolygon(pen, pts)
                End If
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 点云模式：投影后用热图颜色（Designer.GetColors）或 PLY 自带颜色绘制填充点。
    ''' </summary>
    Private Sub DrawPointCloud(g As Graphics)
        Dim colors = GetColorTable()
        Dim n = colors.Length
        Dim sz = Math.Max(1, PointSize)
        Dim half = sz / 2.0F

        Dim pts3 = cloud.Select(Function(c) New Point3D(c.x, c.y, c.z)).ToArray()
        Dim rotated = Camera.Rotate(pts3).ToArray()
        Dim projected = Camera.Project(rotated).ToArray()

        For i = 0 To cloud.Length - 1
            Dim xy = projected(i).PointXY(Camera.Screen)
            Dim brush As System.Drawing.Brush

            If UseEmbeddedColor AndAlso Not String.IsNullOrEmpty(cloud(i).color) Then
                brush = GetEmbeddedBrush(cloud(i).color)
            Else
                Dim v = If(cloud(i).intensity <> 0, cloud(i).intensity, cloud(i).z)
                Dim t = (v - intensityMin) / (intensityMax - intensityMin)
                If t < 0 Then t = 0
                If t > 1 Then t = 1
                Dim cidx = CInt(t * (n - 1))
                brush = colorBrushes(cidx)
            End If

            g.FillRectangle(brush, xy.X - half, xy.Y - half, sz, sz)
        Next
    End Sub

    ''' <summary>
    ''' 将已加载的实体模型强制以点云（仅顶点）形式显示：投影所有顶点，
    ''' 并按每个面受到的光照强度（与表面渲染一致的光照公式）进行热图着色。
    ''' 复用 cboScheme 指定的配色与 PointSize / PointAlpha。
    ''' </summary>
    Private Sub DrawModelAsPointCloud(g As Graphics)
        Dim pts As New List(Of PointF)()
        Dim intensities As New List(Of Double)()

        For Each s In surfaces
            ' 与表面渲染完全相同的光照强度（面法线·光向，环境光保底）
            Dim factor = FaceLightFactor(s)
            Dim projected = Camera.Project(Camera.Rotate(s.vertices)).ToArray()

            For i = 0 To projected.Length - 1
                pts.Add(New PointF(CSng(projected(i).X), CSng(projected(i).Y)))
                intensities.Add(factor)
            Next
        Next

        If pts.Count = 0 Then Return

        ' 将光照强度归一化到 [0,1] 以映射到热图
        Dim minI = intensities.Min()
        Dim maxI = intensities.Max()
        Dim range = maxI - minI
        If range < 1.0E-9 Then range = 1

        Dim colors = GetColorTable()
        Dim n = colors.Length
        Dim sz = Math.Max(1, PointSize)
        Dim half = sz / 2.0F

        For i = 0 To pts.Count - 1
            Dim t = (intensities(i) - minI) / range
            If t < 0 Then t = 0 Else If t > 1 Then t = 1
            Dim cidx = CInt(t * (n - 1))
            g.FillRectangle(colorBrushes(cidx), pts(i).X - half, pts(i).Y - half, sz, sz)
        Next
    End Sub

    ''' <summary>
    ''' 计算单个面的光照强度因子，与 Light.ComputeLighting 完全一致：
    ''' 由面法线（朝向观察者）与光向求点积得到 diffuse，再
    ''' factor = ambient + (1 - ambient) * diffuse。
    ''' </summary>
    Private Function FaceLightFactor(face As Surface) As Double
        Dim v = face.vertices
        If v Is Nothing OrElse v.Length < 3 Then Return Camera.AmbientStrength

        Dim a = v(0), b = v(1), c = v(2)
        Dim normal = (b - a).CrossProduct(c - a)
        Dim mag = normal.Length()
        If mag = 0 Then Return Camera.AmbientStrength

        normal = normal.Multiply(1 / mag)
        If normal.Z < 0 Then normal = normal.Multiply(-1)

        Dim diffuse = Math.Max(0, normal.DotProduct(Camera.LightDirection))
        Return Camera.AmbientStrength + (1 - Camera.AmbientStrength) * diffuse
    End Function

    Private Function GetColorTable() As Color()
        If colorTable Is Nothing OrElse colorTableScheme <> ColorScheme OrElse colorTableAlpha <> PointAlpha Then
            colorTable = Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer.GetColors(ColorScheme, 256, PointAlpha)
            colorTableScheme = ColorScheme
            colorTableAlpha = PointAlpha
            colorBrushes = colorTable.Select(Function(c) New System.Drawing.SolidBrush(c)).ToArray()
        End If
        Return colorTable
    End Function

    Private Function GetEmbeddedBrush(c As String) As System.Drawing.Brush
        If embeddedCache.ContainsKey(c) Then
            Return embeddedCache(c)
        End If
        Dim col As Color = c.TranslateColor(throwEx:=False)
        Dim b = New System.Drawing.SolidBrush(col)
        embeddedCache(c) = b
        Return b
    End Function

    Private Function Centroid(pts As List(Of Point3D)) As Point3D
        If pts.Count = 0 Then Return New Point3D(0, 0, 0)
        Dim cx = pts.Average(Function(p) p.X)
        Dim cy = pts.Average(Function(p) p.Y)
        Dim cz = pts.Average(Function(p) p.Z)
        Return New Point3D(cx, cy, cz)
    End Function

    Private Function Radius(pts As List(Of Point3D), center As Point3D) As Double
        Dim maxR As Double = 0
        For Each p In pts
            Dim d = Point3D.Distance(p, center)
            If d > maxR Then maxR = d
        Next
        Return If(maxR <= 0, 1, maxR)
    End Function

End Class


