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

    Private Function BuildSurfaces(v As BodyVisual) As List(Of Surface3D)
        Dim out As New List(Of Surface3D)()
        Dim mesh As ConvexMesh3D = TryCast(v.Mesh, ConvexMesh3D)

        If mesh Is Nothing Then Return out

        Dim body As RigidBody3D = v.Body
        Dim q As Quaternion = body.Orientation
        Dim pos As Vector3 = body.Position
        Dim verts As Vector3() = mesh.Vertices
        Dim faces As Integer()() = mesh.Faces
        Dim brush As New SolidBrush3D(Tint(v))

        For i As Integer = 0 To faces.Length - 1
            Dim f As Integer() = faces(i)
            Dim pts(f.Length - 1) As Point3D

            For k As Integer = 0 To f.Length - 1
                Dim w As Vector3 = pos + q.Rotate(verts(f(k)))

                pts(k) = rig.ToRender(w.x, w.y, w.z)
            Next

            out.Add(New Surface3D(pts, brush))
        Next

        Return out
    End Function

    ''' <summary>生成高亮 / 休眠对颜色的影响。</summary>
    Private Function Tint(v As BodyVisual) As Color
        Dim c As Color = v.Color

        ' 刚生成的 0.6 秒内混入高亮青色
        If v.Age < 0.6 Then
            Dim t As Double = 1.0 - v.Age / 0.6

            c = Blend(c, Color.FromArgb(34, 211, 238), t * 0.75)
        End If

        ' 休眠刚体降饱和度
        If v.Body.IsSleeping Then
            c = Desaturate(c, 0.45)
        End If

        Return c
    End Function

    Private Shared Function Blend(a As Color, b As Color, t As Double) As Color
        Dim x As Double = std.Max(0.0, std.Min(1.0, t))

        Return Color.FromArgb(a.A,
                              CInt(a.R + (b.R - a.R) * x),
                              CInt(a.G + (b.G - a.G) * x),
                              CInt(a.B + (b.B - a.B) * x))
    End Function

    Private Shared Function Desaturate(c As Color, t As Double) As Color
        Dim g As Integer = CInt(0.299 * c.R + 0.587 * c.G + 0.114 * c.B)

        Return Color.FromArgb(c.A,
                              CInt(c.R + (g - c.R) * t),
                              CInt(c.G + (g - c.G) * t),
                              CInt(c.B + (g - c.B) * t))
    End Function

    ' /********************************************************************************/
    '  辅助几何
    ' /********************************************************************************/

    ''' <summary>把物理坐标的点投影到屏幕。</summary>
    Private Function Project(x As Double, y As Double, z As Double) As PointF
        Dim r As Point3D = rig.ToRender(x, y, z)

        Return rig.Camera.Project(rig.Camera.Rotate(r)).PointXY(rig.Camera.Screen)
    End Function

    Private Sub DrawGrid(g As Graphics)
        If Not ShowGrid Then Return

        Dim extent As Integer = 30
        Dim stride As Integer = 2

        Using minor As New Pen(Color.FromArgb(38, 56, 84), 1.0F)
            Using major As New Pen(Color.FromArgb(58, 86, 120), 1.0F)
                For i As Integer = -extent To extent Step stride
                    Dim pen As Pen = If(i = 0, major, minor)

                    Call g.DrawLine(pen, Project(i, GroundY, -extent), Project(i, GroundY, extent))
                    Call g.DrawLine(pen, Project(-extent, GroundY, i), Project(extent, GroundY, i))
                Next
            End Using
        End Using
    End Sub

    Private Sub DrawAxes(g As Graphics)
        If Not ShowAxes Then Return

        Using xPen As New Pen(Color.FromArgb(210, 96, 120), 1.5F)
            Using zPen As New Pen(Color.FromArgb(120, 160, 210), 1.5F)
                Using yPen As New Pen(Color.FromArgb(96, 210, 170), 1.5F)
                    Call g.DrawLine(xPen, Project(0, GroundY, 0), Project(6, GroundY, 0))
                    Call g.DrawLine(zPen, Project(0, GroundY, 0), Project(0, GroundY, 6))
                    Call g.DrawLine(yPen, Project(0, GroundY, 0), Project(0, GroundY + 6, 0))
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
            all.AddRange(BuildSurfaces(v))
        Next

        If all.Count > 0 Then
            Try
                Call rig.Camera.Draw(g, all, Wireframe, illumination:=Not Wireframe)
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
