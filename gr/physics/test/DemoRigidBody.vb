' Copyright (c) 2018 GPL3 Licensed
' 物理引擎演示：刚体落地/堆叠/反弹、铰链摆、风力场、粒子爆发。
' 仅依赖 System.Windows.Forms / System.Drawing，可直接运行。

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Math
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports Microsoft.VisualBasic.Imaging.Physics.RigidBody
Imports Microsoft.VisualBasic.Imaging.Physics.Joints
Imports Microsoft.VisualBasic.Imaging.Physics.ForceFields
Imports Microsoft.VisualBasic.Imaging.Physics.Particles2D

Namespace test

    ''' <summary>
    ''' 游戏物理引擎演示窗体。展示：刚体重力下落与堆叠、圆与多边形碰撞反弹、
    ''' 铰链摆、风力场对物体的影响、点击爆发粒子。按 W 切换风力，按 G 切换全局重力。
    ''' </summary>
    Public Class DemoRigidBody : Inherits Form

        Private world As New PhysicsWorld
        Private particles As New ParticleSystem
        Private wind As WindField
        Private pendulum As RigidBody
        Private windOn As Boolean = False
        Private timer As New Timer

        Sub New()
            Me.Text = "2D 游戏物理引擎演示 (RigidBody + 粒子 + 力场)"
            Me.Size = New Size(980, 640)
            Me.DoubleBuffered = True
            Me.KeyPreview = True

            BuildScene()

            timer.Interval = 16
            AddHandler timer.Tick, AddressOf OnTick
            timer.Start()
        End Sub

        Private Sub BuildScene()
            world.Gravity = New Vector2(0, 600.0)
            world.FixedDt = 1.0 / 60.0
            world.Substeps = 2
            world.Iterations = 12

            Dim W = 960, H = 600

            ' 地面（静态）
            Dim ground = PhysicsWorld.Box(W, 40, 0)
            ground.SetStatic()
            ground.Position = New Vector2(W / 2, H - 20)
            ground.Material = New PhysicsMaterial(0.5, 0.1)
            world.Add(ground)

            ' 左右墙（静态，防止飞出）
            Dim wallL = PhysicsWorld.Box(20, H, 0) : wallL.SetStatic() : wallL.Position = New Vector2(10, H / 2) : world.Add(wallL)
            Dim wallR = PhysicsWorld.Box(20, H, 0) : wallR.SetStatic() : wallR.Position = New Vector2(W - 10, H / 2) : world.Add(wallR)

            ' 堆叠的盒子
            For i = 0 To 4
                Dim box = PhysicsWorld.Box(46, 46, 1.0, New PhysicsMaterial(0.4, 0.05))
                box.Position = New Vector2(W / 2, H - 40 - 25 - i * 48)
                box.MaxSpeed = 2000
                world.Add(box)
            Next

            ' 一个弹跳的球
            Dim ball = PhysicsWorld.Circle(24, 1.2, New PhysicsMaterial(0.3, 0.7))
            ball.Position = New Vector2(300, 200)
            ball.Velocity = New Vector2(180, 0)
            ball.MaxSpeed = 2000
            world.Add(ball)

            ' 铰链摆：静态锚 + 动态杆
            Dim anchor = PhysicsWorld.Box(20, 20, 0)
            anchor.SetStatic()
            anchor.Position = New Vector2(720, 140)
            world.Add(anchor)

            pendulum = PhysicsWorld.Box(20, 120, 1.0, New PhysicsMaterial(0.3, 0.2))
            pendulum.Position = New Vector2(720, 230)
            pendulum.MaxSpeed = 2000
            world.Add(pendulum)

            world.Add(New RevoluteJoint(anchor, pendulum, New Vector2(0, 50), New Vector2(0, -60)))

            ' 风力场（默认关闭）
            wind = New WindField(New Vector2(1, 0), 250.0, 0.4)
            wind.Region = New AABB With {
                .min = New Vector2(0, 0),
                .max = New Vector2(W, H)
            }
        End Sub

        Private Sub OnTick(sender As Object, e As EventArgs)
            world.Step(0.016)
            particles.Step(0.016)
            Me.Invalidate()
        End Sub

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            If e.KeyCode = Keys.W Then
                windOn = Not windOn
                If windOn Then world.Add(wind) Else world.ForceFields.Remove(wind)
            ElseIf e.KeyCode = Keys.G Then
                world.Gravity = If(world.Gravity.y = 0, New Vector2(0, 600.0), Vector2.Zero)
            End If
            MyBase.OnKeyDown(e)
        End Sub

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            If e.Button = MouseButtons.Left Then
                Dim em = New Emitter(New Vector2(e.X, e.Y), 200.0, 1.2) With {
                    .Spread = 2 * Math.PI,
                    .Color = Color.OrangeRed,
                    .Size = 3.0
                }
                particles.Emit(em, 60)
            End If
            MyBase.OnMouseDown(e)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Color.FromArgb(28, 30, 38))

            ' 力场提示
            If windOn Then
                Using f = New Font("Consolas", 12)
                    g.DrawString("风力场 ON (W 关闭)  重力: " & If(world.Gravity.y <> 0, "ON", "OFF") & " (G 切换)", f, Brushes.LightGray, 10, 10)
                End Using
            Else
                Using f = New Font("Consolas", 12)
                    g.DrawString("风力场 OFF (W 开启)  重力: " & If(world.Gravity.y <> 0, "ON", "OFF") & " (G 切换)  点击=粒子爆发", f, Brushes.LightGray, 10, 10)
                End Using
            End If

            ' 刚体
            For Each b In world.Bodies
                DrawBody(g, b)
            Next

            ' 粒子
            For Each p In particles.Particles
                Dim alpha = CInt(255 * Math.Max(0, p.Life / p.MaxLife))
                Using br = New SolidBrush(Color.FromArgb(alpha, p.Color))
                    g.FillEllipse(br, CSng(p.Position.x - p.Size), CSng(p.Position.y - p.Size), p.Size * 2, p.Size * 2)
                End Using
            Next
        End Sub

        Private Sub DrawBody(g As Graphics, b As RigidBody)
            If TypeOf b.Shape Is PolygonCollider Then
                Dim poly = CType(b.Shape, PolygonCollider)
                Dim pts(poly.Count - 1) As PointF
                For i = 0 To poly.Count - 1
                    Dim w = b.Position + Rotate(poly.vertices(i), b.Rotation)
                    pts(i) = New PointF(CSng(w.x), CSng(w.y))
                Next
                Dim fill = If(b.IsStatic, Color.FromArgb(90, 120, 140), Color.FromArgb(70, 130, 180))
                Using br = New SolidBrush(fill), pen = New Pen(Color.LightSteelBlue, 1.5F)
                    g.FillPolygon(br, pts)
                    g.DrawPolygon(pen, pts)
                End Using
            ElseIf TypeOf b.Shape Is CircleCollider Then
                Dim c = CType(b.Shape, CircleCollider)
                Dim fill = If(b.IsStatic, Color.FromArgb(90, 120, 140), Color.FromArgb(200, 140, 70))
                Using br = New SolidBrush(fill), pen = New Pen(Color.Gold, 1.5F)
                    g.FillEllipse(br, CSng(b.Position.x - c.Radius), CSng(b.Position.y - c.Radius), CSng(c.Radius * 2), CSng(c.Radius * 2))
                    g.DrawEllipse(pen, CSng(b.Position.x - c.Radius), CSng(b.Position.y - c.Radius), CSng(c.Radius * 2), CSng(c.Radius * 2))
                    ' 画一条半径线表示旋转
                    Dim r = Rotate(New Vector2(c.Radius, 0), b.Rotation)
                    g.DrawLine(pen, CSng(b.Position.x), CSng(b.Position.y), CSng(b.Position.x + r.x), CSng(b.Position.y + r.y))
                End Using
            End If
        End Sub
    End Class
End Namespace
