Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Engine.Physics
Imports Microsoft.VisualBasic.Engine.Physics.Collision3D
Imports Microsoft.VisualBasic.Engine.Physics.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Point3D = Microsoft.VisualBasic.Imaging.Drawing3D.Point3D
Imports Surface3D = Microsoft.VisualBasic.Imaging.Drawing3D.Surface
Imports SolidBrush3D = Microsoft.VisualBasic.Imaging.SolidBrush
Imports std = System.Math

''' <summary>
''' 场景渲染器：把 <see cref="BodyVisual"/> 列表转换成 Drawing3D 的
''' <see cref="Surface"/>，再交给 <c>Camera.Draw</c>（画家算法 + Lambert 光照）。
''' </summary>
''' <remarks>
''' 坐标映射：物理空间 Y 向上；渲染空间 X→右、Y→下、Z→远。
''' 使用 <c>Point3D(phys.x, -phys.z, phys.y)</c>（det = +1 的真旋转），
''' 避免镜像导致法向 / 光照翻转与左右手错乱。
''' </remarks>
Public Class SceneRenderer

    Private ReadOnly rig As CameraRig
    Private ReadOnly host As WorldHost

    ''' <summary>背景色。</summary>
    Public Property BackgroundColor As Color = Color.FromArgb(11, 18, 32)
    ''' <summary>是否线框模式。</summary>
    Public Property Wireframe As Boolean = False
    ''' <summary>是否绘制地面网格。</summary>
    Public Property ShowGrid As Boolean = True
    ''' <summary>是否绘制坐标轴。</summary>
    Public Property ShowAxes As Boolean = True
    ''' <summary>地面高度。</summary>
    Public Property GroundY As Double = 0.0
    ''' <summary>JS 侧自定义的 HUD 附加文本（可为空）。</summary>
    Public Property HudText As String = ""

    ''' <summary>鼠标悬停位置（画布像素），负值表示不在视口内。</summary>
    Public Property HoverX As Integer = -1
    Public Property HoverY As Integer = -1

    Public Sub New(rig As CameraRig, host As WorldHost)
        Me.rig = rig
        Me.host = host
    End Sub

    ' /********************************************************************************/
    '  刚体 → Surface
    ' /********************************************************************************/

    ''' <summary>着色用的环境光强度。</summary>
    Private Const Ambient As Double = 0.34

    ''' <summary>
    ''' 光源方向（渲染空间，由表面指向光源）。
    ''' 渲染空间是 <b>Y 向下</b>、<b>Z 向远</b>，因此"从左上前方打光"即
    ''' <c>(-x, -y, +z)</c>：-Y 表示来自上方，-X 表示来自左侧。
    ''' </summary>
    Private Shared ReadOnly LightDir As Double() = Normalize(-0.42, -0.68, 0.6)

    Private Shared Function Normalize(x As Double, y As Double, z As Double) As Double()
        Dim n As Double = std.Sqrt(x * x + y * y + z * z)

        If n < 1.0E-9 Then Return New Double() {0, -1, 0}

        Return New Double() {x / n, y / n, z / n}
    End Function

    ''' <summary>
    ''' 逐面平面着色：<c>factor = ambient + (1-ambient)·max(0, n·L)</c>，
    ''' 直接把基色按 factor 缩放。
    ''' </summary>
    ''' <remarks>
    ''' 不使用库内的 <c>Camera.Lighting</c>：它是把基色朝"灯光颜色"混合，
    ''' 受光面会被直接混成纯白，深色材质（如 #1E293B 的平台）会整片发白。
    ''' </remarks>
    Private Function ShadeVertexColor(baseColor As Color, normal As Point3D) As Color
        Dim diff As Double = normal.X * LightDir(0) + normal.Y * LightDir(1) + normal.Z * LightDir(2)

        If diff < 0.0 Then diff = 0.0

        Dim f As Double = Ambient + (1.0 - Ambient) * diff
        Dim scale As Double = std.Pow(f, 0.9)

        Return Color.FromArgb(baseColor.A,
                              Channel(baseColor.R * scale),
                              Channel(baseColor.G * scale),
                              Channel(baseColor.B * scale))
    End Function

    Private Function BuildSurfaces(v As BodyVisual) As List(Of Surface3D)
        Dim out As New List(Of Surface3D)()
        Dim mesh As ConvexMesh3D = TryCast(v.Mesh, ConvexMesh3D)

        If mesh Is Nothing Then Return out

        Dim body As RigidBody3D = v.Body
        Dim q As Quaternion = body.Orientation
        Dim pos As Vector3 = body.Position
        Dim verts As Vector3() = mesh.Vertices
        Dim faces As Integer()() = mesh.Faces
        Dim normals As Vector3() = mesh.FaceNormals
        ' 注意：局部变量名不能与 Tint 同名（VB 大小写不敏感，会把调用解析成索引访问）
        Dim baseTint As Color = Tint(v)

        ' 先把所有顶点变换到渲染空间，避免重复计算
        Dim rv(verts.Length - 1) As Point3D

        For k As Integer = 0 To verts.Length - 1
            Dim w As Vector3 = pos + q.Rotate(verts(k))

            rv(k) = rig.ToRender(w.x, w.y, w.z)
        Next

        For i As Integer = 0 To faces.Length - 1
            Dim f As Integer() = faces(i)

            ' 面法向：物理局部 → 世界 → 渲染空间（纯旋转，不含平移）
            Dim wn As Vector3 = q.Rotate(normals(i))
            Dim rn As New Point3D(wn.x, -wn.z, wn.y)
            Dim brush As New SolidBrush3D(ShadeVertexColor(baseTint, rn))

            If f.Length = 3 Then
                out.Add(New Surface3D({rv(f(0)), rv(f(1)), rv(f(2))}, brush))
            Else
                ' 扇形三角化：画家算法按"面平均深度"排序，单个大多边形（例如 12x12 的平台顶面）
                ' 的平均深度可能比它上面的小物体更近，从而把后者整块盖住。
                ' 拆成三角形后排序精度足够，这是画家算法的标准缓解手段。
                For k As Integer = 1 To f.Length - 2
                    out.Add(New Surface3D({rv(f(0)), rv(f(k)), rv(f(k + 1))}, brush))
                Next
            End If
        Next

        Return out
    End Function

    ''' <summary>生成高亮 / 休眠对颜色的影响。</summary>
    Private Function Tint(v As BodyVisual) As Color
        Dim c As Color = v.Color

        ' 刚生成的 0.6 秒内混入高亮青色
        If v.Age < 0.6 Then
            Dim t As Double = 1.0 - v.Age / 0.6

            c = Mix(c, Color.FromArgb(34, 211, 238), t * 0.75)
        End If

        ' 休眠刚体降饱和度
        If v.Body.IsSleeping Then
            c = Desaturate(c, 0.45)
        End If

        Return c
    End Function

    ''' <summary>
    ''' 把任意实数安全地收敛到 0..255。
    ''' <see cref="Color.FromArgb(Integer, Integer, Integer, Integer)"/> 对越界值不友好，
    ''' 而 <c>CInt(NaN)</c> 会直接抛 OverflowException，因此所有通道都要经过这里。
    ''' </summary>
    ''' <summary>
    ''' 通道线性插值。
    ''' </summary>
    ''' <remarks>
    ''' 注意：<see cref="Color.R"/> 等属性是 Byte，VB 中 <c>Byte - Byte</c> 仍是 Byte 的
    ''' <b>受检算术</b>，当 <paramref name="to"/> 的分量小于 <paramref name="from"/> 时
    ''' 会直接下溢抛 OverflowException。因此必须先提升为 Integer 再参与运算。
    ''' </remarks>
    Private Shared Function Mix(src As Color, dst As Color, t As Double) As Color
        Dim x As Double = std.Max(0.0, std.Min(1.0, t))
        Dim fr As Integer = src.R
        Dim fg As Integer = src.G
        Dim fb As Integer = src.B
        Dim tr As Integer = dst.R
        Dim tg As Integer = dst.G
        Dim tb As Integer = dst.B

        Return Color.FromArgb(src.A,
                              Channel(fr + (tr - fr) * x),
                              Channel(fg + (tg - fg) * x),
                              Channel(fb + (tb - fb) * x))
    End Function

    Private Shared Function Desaturate(c As Color, t As Double) As Color
        Dim r As Integer = c.R, g As Integer = c.G, b As Integer = c.B
        Dim gray As Double = 0.299 * r + 0.587 * g + 0.114 * b
        Dim x As Double = std.Max(0.0, std.Min(1.0, t))

        Return Color.FromArgb(c.A,
                              Channel(r + (gray - r) * x),
                              Channel(g + (gray - g) * x),
                              Channel(b + (gray - b) * x))
    End Function

    Private Shared Function Channel(v As Double) As Integer
        If Double.IsNaN(v) Then Return 0
        If v <= 0.0 Then Return 0
        If v >= 255.0 Then Return 255

        Return CInt(std.Round(v))
    End Function

    ' /********************************************************************************/
    '  辅助几何
    ' /********************************************************************************/

    ''' <summary>
    ''' 相机前方的最近可见深度。库内投影在 <c>depth <= 0</c> 时会把点塌缩到屏幕中心，
    ''' 因此相机后方的几何必须由我们剔除，否则会画出"糊在屏幕上"的退化形状。
    ''' </summary>
    Private Const MinDepth As Double = 1.0

    ''' <summary>把物理坐标的点投影到屏幕；点在相机后方或近平面上时返回 False。</summary>
    Private Function TryProject(x As Double, y As Double, z As Double, ByRef p As PointF) As Boolean
        Dim r As Point3D = rig.ToRender(x, y, z)
        Dim rot As Point3D = rig.Camera.Rotate(r)
        Dim depth As Double = rig.Camera.ViewDistance + rot.Z

        If depth < MinDepth Then Return False

        p = rig.Camera.Project(rot).PointXY(rig.Camera.Screen)

        Return True
    End Function

    ''' <summary>刚体是否整个位于近平面前方。</summary>
    Private Function Visible(v As BodyVisual) As Boolean
        Dim mesh As ConvexMesh3D = TryCast(v.Mesh, ConvexMesh3D)
        Dim margin As Double = If(mesh Is Nothing, 0.0, mesh.BoundRadius)
        Dim r As Point3D = rig.ToRender(v.Body.Position.x, v.Body.Position.y, v.Body.Position.z)
        Dim depth As Double = rig.Camera.ViewDistance + rig.Camera.Rotate(r).Z

        Return depth > (MinDepth + margin)
    End Function

    Private Sub DrawGrid(g As Graphics)
        If Not ShowGrid Then Return

        Dim extent As Integer = 16
        Dim stride As Integer = 2
        Dim a As PointF
        Dim b As PointF
        Dim c As PointF
        Dim d As PointF

        Using minor As New Pen(Color.FromArgb(38, 56, 84), 1.0F)
            Using major As New Pen(Color.FromArgb(58, 86, 120), 1.0F)
                For i As Integer = -extent To extent Step stride
                    Dim pen As Pen = If(i = 0, major, minor)

                    If TryProject(i, GroundY, -extent, a) AndAlso TryProject(i, GroundY, extent, b) Then
                        Call g.DrawLine(pen, a, b)
                    End If

                    If TryProject(-extent, GroundY, i, c) AndAlso TryProject(extent, GroundY, i, d) Then
                        Call g.DrawLine(pen, c, d)
                    End If
                Next
            End Using
        End Using
    End Sub

    Private Sub DrawAxes(g As Graphics)
        If Not ShowAxes Then Return

        Dim o As PointF
        Dim ax As PointF
        Dim az As PointF
        Dim ay As PointF

        If Not TryProject(0, GroundY, 0, o) Then Return

        Using xPen As New Pen(Color.FromArgb(210, 96, 120), 1.5F)
            Using zPen As New Pen(Color.FromArgb(120, 160, 210), 1.5F)
                Using yPen As New Pen(Color.FromArgb(96, 210, 170), 1.5F)
                    If TryProject(6, GroundY, 0, ax) Then Call g.DrawLine(xPen, o, ax)
                    If TryProject(0, GroundY, 6, az) Then Call g.DrawLine(zPen, o, az)
                    If TryProject(0, GroundY + 6, 0, ay) Then Call g.DrawLine(yPen, o, ay)
                End Using
            End Using
        End Using
    End Sub

    ' /********************************************************************************/
    '  主渲染
    ' /********************************************************************************/

    Public Sub Render(g As Graphics, size As Size)
        rig.SetScreen(size)

        Call g.Clear(BackgroundColor)
        g.SmoothingMode = SmoothingMode.AntiAlias

        Call DrawGrid(g)
        Call DrawAxes(g)

        Dim all As New List(Of Surface3D)()

        For Each v In host.Bodies
            ' 相机后方的几何必须整体丢弃，否则投影会塌缩成贴在屏幕上的假形状
            If Not Visible(v) Then Continue For

            all.AddRange(BuildSurfaces(v))
        Next

        If all.Count > 0 Then
            Try
                ' 面颜色已在 BuildSurfaces 里做过平面着色，这里关闭库内光照
                Call rig.Camera.Draw(g, all, Wireframe, illumination:=False)
            Catch
                ' 单帧渲染失败不应让模拟器崩溃
            End Try
        End If

        Call DrawHud(g, size)
        Call DrawCrosshair(g)
    End Sub

    Private Sub DrawHud(g As Graphics, size As Size)
        Using small As New Font("Consolas", 8.5F)
            Using text As New SolidBrush(Color.FromArgb(226, 232, 240))
                Using panel As New SolidBrush(Color.FromArgb(150, 11, 18, 32))
                    Using errorPen As New SolidBrush(Color.FromArgb(248, 113, 113))
                        Dim y As Single = 12.0F
                        Dim pad As Single = 10.0F
                        Dim lines As New List(Of String)

                        lines.Add($"刚体 {host.DynamicCount}   静态 {host.Bodies.Count - host.DynamicCount}")
                        lines.Add($"休眠 {host.SleepingCount}   接触 {host.ContactCount}")
                        lines.Add($"仿真 {host.SimTime:F1}s   " & If(host.Paused, "已暂停", "运行中"))

                        If Not String.IsNullOrEmpty(HudText) Then
                            lines.Add(HudText)
                        End If

                        For Each line In lines
                            Dim sz As SizeF = g.MeasureString(line, small)

                            Call g.FillRectangle(panel, pad, y, sz.Width + 12.0F, sz.Height + 6.0F)
                            Call g.DrawString(line, small, text, pad + 6.0F, y + 3.0F)
                            y += sz.Height + 9.0F
                        Next

                        If host.LastError IsNot Nothing Then
                            Call g.DrawString("脚本错误: " & host.LastError, small, errorPen, pad, size.Height - 30.0F)
                        End If
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Sub DrawCrosshair(g As Graphics)
        If HoverX < 0 OrElse HoverY < 0 Then Return

        Using pen As New Pen(Color.FromArgb(90, 34, 211, 238), 1.0F)
            Dim r As Integer = 9

            Call g.DrawLine(pen, HoverX - r, HoverY, HoverX - 3, HoverY)
            Call g.DrawLine(pen, HoverX + 3, HoverY, HoverX + r, HoverY)
            Call g.DrawLine(pen, HoverX, HoverY - r, HoverX, HoverY - 3)
            Call g.DrawLine(pen, HoverX, HoverY + 3, HoverX, HoverY + r)
        End Using
    End Sub
End Class
