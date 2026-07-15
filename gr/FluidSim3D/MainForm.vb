' /********************************************************************************/
'
'     MainForm - the 3D water simulator desktop application.
'
'     * a PropertyGrid (left) tunes the SPH parameters live,
'     * the main canvas renders the cuboid container + the water point cloud,
'     * the mouse rotates / pans / zooms the camera,
'     * dragging the window on the desktop injects an inertial disturbance into
'       the fluid (engine.DisturbAccel) so the water sloshes after a shake.
'
' /********************************************************************************/

Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Physics
Imports P3 = Microsoft.VisualBasic.Imaging.Physics.Vector3

Namespace FluidSim3D

    Public Class MainForm : Inherits Form

        ' ---- rendering / simulation state ----
        Private WithEvents renderer As New WaterRenderer()
        Private WithEvents canvas As RenderPanel
        Private engine As FluidEngine3D
        Private params As New FluidParameters()
        Private running As Boolean = True
        Private showBoundary As Boolean = True

        ' ---- shake (window position) tracking ----
        Private WithEvents simTimer As New Timer()
        Private lastTick As Long = 0
        Private lastLoc As Point
        Private lastVel As New PointF(0, 0)
        Private fps As Double = 0

        ' ---- UI controls ----
        Private WithEvents toolStrip As ToolStrip
        Private WithEvents btnPlay As ToolStripButton
        Private WithEvents btnResetView As ToolStripButton
        Private WithEvents btnApply As ToolStripButton
        Private WithEvents btnResetSim As ToolStripButton
        Private WithEvents btnBoundary As ToolStripButton
        Private WithEvents leftPanel As Panel
        Private WithEvents propGrid As PropertyGrid
        Private WithEvents lblHeader As Label
        Private WithEvents statusStrip As StatusStrip
        Private WithEvents lblStatus As ToolStripStatusLabel

        Private dragging As Boolean = False
        Private panning As Boolean = False
        Private lastX As Integer = 0
        Private lastY As Integer = 0

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            ' ---------- form ----------
            Me.Text = "FluidSim3D - 三维水模拟器"
            Me.ClientSize = New Size(1100, 720)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.FromArgb(11, 15, 23)
            Me.ForeColor = Color.FromArgb(230, 240, 255)
            Me.MinimumSize = New Size(640, 480)


            ' ---------- render canvas ----------
            canvas = New RenderPanel()
            canvas.Dock = DockStyle.Fill
            canvas.BackColor = Color.FromArgb(8, 12, 20)
            canvas.TabStop = True

            ' ---------- tool strip (dark) ----------
            toolStrip = New ToolStrip()
            toolStrip.RenderMode = ToolStripRenderMode.Professional
            toolStrip.Renderer = New ToolStripProfessionalRenderer(New DarkToolStripColorTable())
            toolStrip.GripStyle = ToolStripGripStyle.Hidden
            toolStrip.Dock = DockStyle.Top
            toolStrip.BackColor = Color.FromArgb(16, 23, 38)
            toolStrip.Padding = New Padding(4, 2, 4, 2)

            btnPlay = New ToolStripButton("⏸ 暂停模拟")
            btnPlay.BackColor = Color.FromArgb(30, 41, 59)
            btnPlay.ForeColor = Color.FromArgb(230, 240, 255)
            btnPlay.Font = New Font("Segoe UI", 10, FontStyle.Bold)

            btnResetView = New ToolStripButton("↺ 重置视角")
            btnBoundary = New ToolStripButton("▦ 显示边界")
            btnBoundary.CheckOnClick = True
            btnBoundary.Checked = True
            btnApply = New ToolStripButton("✔ 应用参数")
            btnResetSim = New ToolStripButton("⟳ 重置模拟")

            For Each b In New ToolStripButton() {btnPlay, btnResetView, btnBoundary, btnApply, btnResetSim}
                b.BackColor = Color.FromArgb(30, 41, 59)
                b.ForeColor = Color.FromArgb(230, 240, 255)
                b.Margin = New Padding(2, 1, 2, 1)
                toolStrip.Items.Add(b)
            Next
            btnPlay.BackColor = Color.FromArgb(14, 58, 102)
            btnApply.BackColor = Color.FromArgb(46, 155, 255)

            ' ---------- left parameter panel ----------
            leftPanel = New Panel()
            leftPanel.Dock = DockStyle.Left
            leftPanel.Width = 300
            leftPanel.BackColor = Color.FromArgb(13, 19, 32)
            leftPanel.Padding = New Padding(6)
            leftPanel.BorderStyle = BorderStyle.None

            lblHeader = New Label()
            lblHeader.Dock = DockStyle.Top
            lblHeader.Height = 30
            lblHeader.Text = "  模拟参数  Simulation"
            lblHeader.Font = New Font("Segoe UI", 12, FontStyle.Bold)
            lblHeader.ForeColor = Color.FromArgb(127, 227, 255)
            lblHeader.BackColor = Color.FromArgb(13, 19, 32)

            propGrid = New PropertyGrid()
            propGrid.Dock = DockStyle.Fill
            propGrid.SelectedObject = params
            propGrid.HelpVisible = True
            propGrid.ToolbarVisible = False
            propGrid.BackColor = Color.FromArgb(16, 23, 38)
            propGrid.LineColor = Color.FromArgb(46, 155, 255)
            propGrid.CategoryForeColor = Color.FromArgb(127, 227, 255)
            propGrid.ViewBackColor = Color.FromArgb(16, 23, 38)
            propGrid.ViewForeColor = Color.FromArgb(230, 240, 255)
            propGrid.HelpBackColor = Color.FromArgb(11, 15, 23)
            propGrid.HelpForeColor = Color.FromArgb(159, 179, 200)
            propGrid.CommandsBackColor = Color.FromArgb(16, 23, 38)
            propGrid.CommandsForeColor = Color.FromArgb(159, 179, 200)

            leftPanel.Controls.Add(propGrid)
            leftPanel.Controls.Add(lblHeader)



            ' ---------- status strip ----------
            statusStrip = New StatusStrip()
            statusStrip.RenderMode = ToolStripRenderMode.Professional
            statusStrip.Renderer = New ToolStripProfessionalRenderer(New DarkToolStripColorTable())
            statusStrip.Dock = DockStyle.Bottom
            statusStrip.BackColor = Color.FromArgb(16, 23, 38)
            lblStatus = New ToolStripStatusLabel()
            lblStatus.Text = "就绪"
            lblStatus.Spring = True
            lblStatus.ForeColor = Color.FromArgb(159, 179, 200)
            statusStrip.Items.Add(lblStatus)

            ' ---------- assembly ----------
            Controls.Add(toolStrip)
            Controls.Add(statusStrip)
            Controls.Add(leftPanel)
            Controls.Add(canvas)

            ' ---------- simulation timer (~60Hz) ----------
            simTimer.Interval = 16
            simTimer.Enabled = True
        End Sub

        ' ============================================================
        '   engine lifecycle
        ' ============================================================

        Private Sub BuildEngine()
            Dim box = params.ToBoxSize()
            engine = New FluidEngine3D(params.ParticleCount, box, params.SmoothingRadius)
            SyncParamsToEngine(engine)
            renderer.FitView(box, canvas.ClientSize)
            UpdateStatus()
        End Sub

        Private Sub SyncParamsToEngine(e As FluidEngine3D)
            e.Gravity = params.Gravity
            e.DeltaTime = params.DeltaTime
            e.TargetDensity = params.TargetDensity
            e.PressureMultiplier = params.PressureMultiplier
            e.NearPressureMultiplier = params.NearPressureMultiplier
            e.ViscosityStrength = params.ViscosityStrength
            e.CollisionDamping = params.CollisionDamping
            e.SmoothingRadius = params.SmoothingRadius
            e.ParticleSize = params.ParticleSize
        End Sub

        Private Sub ApplyParameters()
            Dim needRebuild = (engine Is Nothing) OrElse
                (engine.Count <> params.ParticleCount) OrElse
                (engine.BoxSize.x <> params.BoxX) OrElse
                (engine.BoxSize.y <> params.BoxY) OrElse
                (engine.BoxSize.z <> params.BoxZ)

            If needRebuild Then
                BuildEngine()
            Else
                SyncParamsToEngine(engine)
            End If

            canvas.Invalidate()
        End Sub

        ' ============================================================
        '   main loop: shake sampling + simulation step + render
        ' ============================================================

        Private Sub SimTick(sender As Object, e As EventArgs) Handles simTimer.Tick
            Dim now = Stopwatch.GetTimestamp()
            Dim dt As Double = 0
            If lastTick > 0 Then
                dt = CDbl(now - lastTick) / Stopwatch.Frequency
            End If
            lastTick = now

            If dt > 0 Then
                Dim instFps = 1000.0 / (dt * 1000.0)
                fps = fps * 0.9 + instFps * 0.1
            End If

            SampleShake(dt)

            If running AndAlso engine IsNot Nothing Then
                engine.RunSimulationStep()
            End If

            canvas.Invalidate()
            UpdateStatus()
        End Sub

        ''' <summary>
        ''' sample the window location each tick, derive the desktop acceleration
        ''' of the window and inject it (reversed, as an inertial force) into the
        ''' fluid engine as a disturbance acceleration.
        ''' </summary>
        Private Sub SampleShake(dt As Double)
            If engine Is Nothing Then
                lastLoc = Me.Location
                Return
            End If

            Dim loc = Me.Location
            If dt > 0 Then
                Dim vx = (loc.X - lastLoc.X) / dt
                Dim vy = (loc.Y - lastLoc.Y) / dt
                Dim ax = (vx - lastVel.X) / dt
                Dim ay = (vy - lastVel.Y) / dt
                lastVel = New PointF(CSng(vx), CSng(vy))

                Dim scale = 0.002F * params.ShakeSensitivity
                Dim d = New P3(-ax * scale, 0, -ay * scale)

                ' clamp the disturbance so the simulation never explodes
                If d.Magnitude > 600 Then
                    d = d.Normalize() * 600
                End If

                engine.DisturbAccel = d
            Else
                lastVel = New PointF(0, 0)
            End If

            lastLoc = loc
        End Sub

        ' ============================================================
        '   rendering
        ' ============================================================

        Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs) Handles canvas.Paint
            If engine Is Nothing Then
                renderer.Camera.Screen = canvas.ClientSize
                renderer.Draw(e.Graphics, canvas.ClientSize, Nothing, params, False)
                Return
            End If

            renderer.Draw(e.Graphics, canvas.ClientSize, engine, params, showBoundary)
        End Sub

        ' ============================================================
        '   mouse interaction
        ' ============================================================

        Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles canvas.MouseDown
            canvas.Focus()
            If e.Button = MouseButtons.Left Then
                dragging = True
                lastX = e.X : lastY = e.Y
            ElseIf e.Button = MouseButtons.Right Then
                panning = True
                lastX = e.X : lastY = e.Y
            End If
        End Sub

        Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs) Handles canvas.MouseMove
            If dragging Then
                Dim dx = e.X - lastX
                Dim dy = e.Y - lastY
                renderer.Camera.AngleY = renderer.Camera.AngleY + dx * 0.4F
                renderer.Camera.AngleX = renderer.Camera.AngleX + dy * 0.4F
                lastX = e.X : lastY = e.Y
                canvas.Invalidate()
            ElseIf panning Then
                Dim dx = e.X - lastX
                Dim dy = e.Y - lastY
                renderer.Camera.Offset = New PointF(renderer.Camera.Offset.X + dx, renderer.Camera.Offset.Y + dy)
                lastX = e.X : lastY = e.Y
                canvas.Invalidate()
            End If
        End Sub

        Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles canvas.MouseUp
            dragging = False
            panning = False
        End Sub

        Private Sub Canvas_Zoom(delta As Integer) Handles canvas.Zoom
            Dim factor = If(delta > 0, 0.9F, 1.1F)
            Dim vd = renderer.Camera.ViewDistance * factor
            renderer.Camera.ViewDistance = System.Math.Max(1.0F, vd)
            canvas.Invalidate()
        End Sub

        ' ============================================================
        '   toolbar
        ' ============================================================

        Private Sub PlayClick(sender As Object, e As EventArgs) Handles btnPlay.Click
            running = Not running
            btnPlay.Text = If(running, "⏸ 暂停模拟", "▶ 开始模拟")
            btnPlay.BackColor = If(running, Color.FromArgb(14, 58, 102), Color.FromArgb(30, 41, 59))
        End Sub

        Private Sub ResetViewClick(sender As Object, e As EventArgs) Handles btnResetView.Click
            renderer.Camera.AngleX = 20
            renderer.Camera.AngleY = -30
            renderer.Camera.AngleZ = 0
            renderer.Camera.Offset = New PointF(0, 0)
            If engine IsNot Nothing Then renderer.FitView(engine.BoxSize, canvas.ClientSize)
            canvas.Invalidate()
        End Sub

        Private Sub BoundaryClick(sender As Object, e As EventArgs) Handles btnBoundary.CheckedChanged
            showBoundary = btnBoundary.Checked
            canvas.Invalidate()
        End Sub

        Private Sub ApplyClick(sender As Object, e As EventArgs) Handles btnApply.Click
            ApplyParameters()
        End Sub

        Private Sub ResetSimClick(sender As Object, e As EventArgs) Handles btnResetSim.Click
            If engine IsNot Nothing Then
                engine.Reset()
                canvas.Invalidate()
            End If
        End Sub

        ' ============================================================
        '   status
        ' ============================================================

        Private Sub UpdateStatus()
            If lblStatus Is Nothing OrElse engine Is Nothing Then Return

            Dim disturb = engine.DisturbAccel.Magnitude
            lblStatus.Text =
                $"FPS: {fps:F0}   |   " &
                $"粒子: {engine.Count}   |   " &
                $"角度X: {renderer.Camera.AngleX:F1}  Y: {renderer.Camera.AngleY:F1}  |   " &
                $"视距: {renderer.Camera.ViewDistance:F0}   |   " &
                $"晃动加速度: {disturb:F1}"
        End Sub

        Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
            lastLoc = Me.Location
            BuildEngine()
            canvas.Invalidate()
        End Sub

        ' ============================================================
        '   dark tool strip colour table
        ' ============================================================

        Private Class DarkToolStripColorTable : Inherits ProfessionalColorTable

            Public Overrides ReadOnly Property ToolStripGradientBegin As Color
                Get
                    Return Color.FromArgb(13, 19, 32)
                End Get
            End Property

            Public Overrides ReadOnly Property ToolStripGradientMiddle As Color
                Get
                    Return Color.FromArgb(16, 23, 38)
                End Get
            End Property

            Public Overrides ReadOnly Property ToolStripGradientEnd As Color
                Get
                    Return Color.FromArgb(16, 23, 38)
                End Get
            End Property

            Public Overrides ReadOnly Property MenuStripGradientBegin As Color
                Get
                    Return Color.FromArgb(13, 19, 32)
                End Get
            End Property

            Public Overrides ReadOnly Property MenuStripGradientEnd As Color
                Get
                    Return Color.FromArgb(16, 23, 38)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonSelectedGradientBegin As Color
                Get
                    Return Color.FromArgb(30, 90, 150)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonSelectedGradientEnd As Color
                Get
                    Return Color.FromArgb(30, 90, 150)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonPressedGradientBegin As Color
                Get
                    Return Color.FromArgb(14, 58, 102)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonPressedGradientEnd As Color
                Get
                    Return Color.FromArgb(14, 58, 102)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonCheckedGradientBegin As Color
                Get
                    Return Color.FromArgb(46, 155, 255)
                End Get
            End Property

            Public Overrides ReadOnly Property ButtonCheckedGradientEnd As Color
                Get
                    Return Color.FromArgb(30, 111, 208)
                End Get
            End Property

            Public Overrides ReadOnly Property SeparatorDark As Color
                Get
                    Return Color.FromArgb(46, 155, 255)
                End Get
            End Property

            Public Overrides ReadOnly Property SeparatorLight As Color
                Get
                    Return Color.FromArgb(16, 23, 38)
                End Get
            End Property

        End Class

    End Class

End Namespace
