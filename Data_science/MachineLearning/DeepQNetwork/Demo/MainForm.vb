#Region "Microsoft.VisualBasic::d203d32eb9e276b77e61bcf480e6ce1c, Data_science\MachineLearning\DeepQNetwork\Demo\MainForm.vb"

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

    '   Total Lines: 482
    '    Code Lines: 390 (80.91%)
    ' Comment Lines: 32 (6.64%)
    '    - Xml Docs: 12.50%
    ' 
    '   Blank Lines: 60 (12.45%)
    '     File Size: 18.54 KB


    ' Class MainForm
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: MakeButton, MakeLabel, W2S
    ' 
    '     Sub: BuildControls, DrawBody, DrawChart, DrawGoal, DrawGrid
    '          DrawSkeleton, DrawWorld, EnableDoubleBuffer, OnDemo, OnReset
    '          OnSpeed, OnStart, OnTick, StepOnce, UpdateBadge
    '          UpdateHUD
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2018 GPL3 Licensed
' DQN 火柴人强化学习可视化 Demo：游戏循环驱动 智能体→环境→渲染，
' GDI+ 绘制地形与火柴人（相机水平跟随）、HUD、实时学习曲线。

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Imaging.Physics.Collision
Imports DeepQNetwork
Imports System.Reflection

''' <summary>
''' DQN Stickman Runner — dark-dashboard WinForm that trains (or demos) a stickman
''' ragdoll to run / climb steps / jump obstacles via deep Q-learning.
''' </summary>
Public Class MainForm : Inherits Form

    ' ---- theme ----
    Shared ReadOnly BG As Color = Color.FromArgb(15, 23, 42)        ' #0F172A
    Shared ReadOnly PanelBG As Color = Color.FromArgb(30, 41, 59)   ' #1E293B
    Shared ReadOnly Cyan As Color = Color.FromArgb(34, 211, 238)    ' #22D3EE
    Shared ReadOnly Orange As Color = Color.FromArgb(245, 158, 11)  ' #F59E0B
    Shared ReadOnly Green As Color = Color.FromArgb(52, 211, 153)   ' #34D399
    Shared ReadOnly RedC As Color = Color.FromArgb(248, 113, 113)   ' #F87171
    Shared ReadOnly TextLight As Color = Color.FromArgb(226, 232, 240)
    Shared ReadOnly TextDim As Color = Color.FromArgb(148, 163, 184)
    Shared ReadOnly Consolas10 As New Font("Consolas", 10)
    Shared ReadOnly Consolas12 As New Font("Consolas", 12, FontStyle.Bold)
    Shared ReadOnly Consolas14 As New Font("Consolas", 14, FontStyle.Bold)

    ' ---- runtime ----
    Private env As StickmanEnv
    Private agent As QAgent
    Private running As Boolean = False
    Private demoMode As Boolean = False
    Private currentState As Double()
    Private prevState As Double()
    Private episode As Integer = 0
    Private episodeReward As Double = 0
    Private lastAction As Integer = 0
    Private speed As Integer = 2

    ' ---- camera ----
    Private camX As Double = 0
    Private ReadOnly camY As Double = 240
    Private ReadOnly scale As Double = 1.0

    ' ---- learning curves ----
    Private episodeRewards As New List(Of Double)
    Private episodeDist As New List(Of Double)

    ' ---- controls ----
    Private topPanel As Panel
    Private rightPanel As Panel
    Private worldPanel As Panel
    Private bottomPanel As Panel
    Private titleLabel As Label
    Private statusBadge As Label
    Private btnStart As Button
    Private btnReset As Button
    Private btnDemo As Button
    Private speedBar As TrackBar
    Private lblSpeed As Label
    Private lblEpisode, lblReward, lblSteps, lblEpsilon, lblDistance, lblAction, lblPhase As Label
    Private timer As New Timer

    Sub New()
        Me.Text = "DQN Stickman Runner"
        Me.Size = New Size(1280, 760)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.DoubleBuffered = True
        Me.BackColor = BG
        Me.ForeColor = TextLight

        BuildControls()

        ' 构建环境 + 智能体
        env = New StickmanEnv()
        currentState = env.Reset()
        prevState = currentState
        agent = New QAgent(New QNetwork(env.StateSize, GetType(StickmanAction), New Integer() {64, 64, 32}, 0.1), True, 50000)
        agent.BeginEpisode()

        AddHandler worldPanel.Paint, AddressOf DrawWorld
        AddHandler bottomPanel.Paint, AddressOf DrawChart
        AddHandler timer.Tick, AddressOf OnTick
        timer.Interval = 16
        timer.Start()

        UpdateHUD()
        worldPanel.Invalidate()
    End Sub

    ' ---------------- UI 构建 ----------------

    Private Sub BuildControls()
        topPanel = New Panel() With {.Dock = DockStyle.Top, .Height = 46, .BackColor = PanelBG}
        rightPanel = New Panel() With {.Dock = DockStyle.Right, .Width = 300, .BackColor = PanelBG}
        bottomPanel = New Panel() With {.Dock = DockStyle.Bottom, .Height = 200, .BackColor = BG}
        worldPanel = New Panel() With {.Dock = DockStyle.Fill, .BackColor = BG}
        EnableDoubleBuffer(worldPanel)
        EnableDoubleBuffer(bottomPanel)

        ' 顶栏
        titleLabel = New Label() With {
            .Text = "DQN Stickman Runner",
            .Font = Consolas14, .ForeColor = Cyan,
            .Location = New Point(14, 11), .AutoSize = True
        }
        statusBadge = New Label() With {
            .Text = "已暂停", .Font = Consolas12, .ForeColor = Orange,
            .Location = New Point(260, 13), .AutoSize = True
        }
        topPanel.Controls.Add(titleLabel)
        topPanel.Controls.Add(statusBadge)

        ' 右侧控制面板
        btnStart = MakeButton("开始训练", 14, 14, 130, 36, AddressOf OnStart)
        btnReset = MakeButton("重置", 156, 14, 130, 36, AddressOf OnReset)
        btnDemo = MakeButton("贪心演示", 14, 58, 272, 36, AddressOf OnDemo)

        lblSpeed = New Label() With {
            .Text = "仿真倍速: 2×", .Font = Consolas10, .ForeColor = TextLight,
            .Location = New Point(16, 104), .AutoSize = True
        }
        speedBar = New TrackBar() With {
            .Minimum = 1, .Maximum = 8, .Value = 2, .TickFrequency = 1,
            .Location = New Point(14, 124), .Width = 272, .Height = 30
        }
        AddHandler speedBar.Scroll, AddressOf OnSpeed
        rightPanel.Controls.Add(lblSpeed)
        rightPanel.Controls.Add(speedBar)

        ' HUD
        Dim y0 = 172
        lblEpisode = MakeLabel("回合 Episode: 0", 16, y0)
        lblReward = MakeLabel("累计奖励: 0.0", 16, y0 + 26)
        lblSteps = MakeLabel("步数: 0", 16, y0 + 52)
        lblEpsilon = MakeLabel("ε (探索率): 1.00", 16, y0 + 78)
        lblDistance = MakeLabel("前进距离: 0 px", 16, y0 + 104)
        lblAction = MakeLabel("当前动作: Idle", 16, y0 + 130)
        lblPhase = MakeLabel("关卡阶段: 平地奔跑", 16, y0 + 156)

        ' 添加到窗体（Fill 最后添加）
        Controls.Add(topPanel)
        Controls.Add(rightPanel)
        Controls.Add(bottomPanel)
        Controls.Add(worldPanel)
    End Sub

    Private Function MakeButton(text As String, x As Integer, y As Integer, w As Integer, h As Integer, handler As EventHandler) As Button
        Dim b = New Button() With {
            .Text = text,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = PanelBG,
            .ForeColor = Cyan,
            .Font = Consolas10,
            .Location = New Point(x, y),
            .Width = w,
            .Height = h
        }
        b.FlatAppearance.BorderColor = Cyan
        b.FlatAppearance.BorderSize = 1
        AddHandler b.Click, handler
        AddHandler b.MouseEnter, Sub(s2, e2) b.BackColor = Color.FromArgb(45, 55, 75)
        AddHandler b.MouseLeave, Sub(s2, e2) b.BackColor = PanelBG
        rightPanel.Controls.Add(b)
        Return b
    End Function

    Private Function MakeLabel(text As String, x As Integer, y As Integer) As Label
        Dim l = New Label() With {
            .Text = text, .Font = Consolas10, .ForeColor = TextLight,
            .Location = New Point(x, y), .AutoSize = True
        }
        rightPanel.Controls.Add(l)
        Return l
    End Function

    ' ---------------- 控件事件 ----------------

    Private Sub OnStart(sender As Object, e As EventArgs)
        running = Not running
        btnStart.Text = If(running, "暂停", "开始训练")
        UpdateBadge()
    End Sub

    Private Sub OnReset(sender As Object, e As EventArgs)
        running = False
        btnStart.Text = "开始训练"
        episode = 0
        episodeReward = 0
        lastAction = 0
        episodeRewards.Clear()
        episodeDist.Clear()
        camX = 0
        ' 全新随机权重的智能体
        agent = New QAgent(New QNetwork(env.StateSize, GetType(StickmanAction), New Integer() {64, 64, 32}, 0.005), True, 50000)
        agent.BeginEpisode()
        currentState = env.Reset()
        prevState = currentState
        UpdateBadge()
        UpdateHUD()
        worldPanel.Invalidate()
        bottomPanel.Invalidate()
    End Sub

    Private Sub OnDemo(sender As Object, e As EventArgs)
        demoMode = Not demoMode
        btnDemo.Text = If(demoMode, "退出演示", "贪心演示")
        If demoMode Then
            running = True
            btnStart.Text = "暂停"
        End If
        UpdateBadge()
    End Sub

    Private Sub OnSpeed(sender As Object, e As EventArgs)
        speed = speedBar.Value
        lblSpeed.Text = "仿真倍速: " & speed & "×"
    End Sub

    Private Sub UpdateBadge()
        If demoMode Then
            statusBadge.Text = "贪心演示"
            statusBadge.ForeColor = Cyan
        ElseIf running Then
            statusBadge.Text = "训练中"
            statusBadge.ForeColor = Green
        Else
            statusBadge.Text = "已暂停"
            statusBadge.ForeColor = Orange
        End If
    End Sub

    ' ---------------- 主循环 ----------------

    Private Sub OnTick(sender As Object, e As EventArgs)
        If Not running Then Return
        For k As Integer = 1 To speed
            StepOnce()
        Next
        worldPanel.Invalidate()
        bottomPanel.Invalidate()
        UpdateHUD()
    End Sub

    Private Sub StepOnce()
        Dim a As Integer
        If demoMode Then
            a = agent.GreedyAction(currentState)
        Else
            a = agent.act(currentState)
        End If

        Dim res = env.[Step](a)
        lastAction = a

        If Not demoMode Then
            agent.RecordReward(res.reward)
            agent.Store(prevState, a, res.reward, res.state, res.done)
            agent.Learn()
            episodeReward += res.reward
        End If

        prevState = res.state

        If res.done Then
            If Not demoMode Then
                episodeRewards.Add(episodeReward)
                episodeDist.Add(env.stick.TorsoX() - StickmanEnv.StartX)
                episode += 1
            End If
            currentState = env.Reset()
            prevState = currentState
            agent.BeginEpisode()
            episodeReward = 0
        Else
            currentState = res.state
        End If
    End Sub

    Private Sub UpdateHUD()
        lblEpisode.Text = "回合 Episode: " & episode
        lblReward.Text = "累计奖励: " & episodeReward.ToString("F2")
        lblSteps.Text = "步数: " & agent.stepsThisEpisode
        lblEpsilon.Text = "ε (探索率): " & agent.epsilon.ToString("F2")
        lblDistance.Text = "前进距离: " & CInt(env.stick.TorsoX() - StickmanEnv.StartX) & " px"
        lblAction.Text = "当前动作: " & [Enum].GetName(GetType(StickmanAction), lastAction)
        lblPhase.Text = "关卡阶段: " & env.PhaseText()
    End Sub

    ' ---------------- 渲染 ----------------

    Private Function W2S(p As Vector2) As PointF
        Return New PointF(CSng((p.x - camX) * scale), CSng((p.y - camY) * scale))
    End Function

    Private Sub DrawWorld(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(BG)

        If env Is Nothing OrElse env.stick Is Nothing Then Return

        ' 相机水平平滑跟随躯干
        Dim torsoX = env.stick.TorsoX()
        Dim targetCamX = torsoX - worldPanel.Width * 0.35
        If targetCamX < 0 Then targetCamX = 0
        camX = camX + (targetCamX - camX) * 0.08

        ' 背景网格（轻微）
        DrawGrid(g)

        ' 地形
        Dim terrainFill = New SolidBrush(PanelBG)
        Dim terrainPen = New Pen(Color.FromArgb(71, 85, 105), 2)
        For Each b In env.terrainBodies
            DrawBody(g, b, terrainFill, terrainPen)
        Next

        ' 终点旗
        DrawGoal(g)

        ' 火柴人（脚掌高亮）
        Dim limbFill = New SolidBrush(Color.FromArgb(70, 34, 211, 238))
        Dim limbPen = New Pen(Cyan, 2.5F)
        Dim footFill = New SolidBrush(Color.FromArgb(120, 245, 158, 11))
        Dim footPen = New Pen(Orange, 2.5F)
        For Each b In env.stick.Bodies()
            If b Is env.stick.footL OrElse b Is env.stick.footR Then
                DrawBody(g, b, footFill, footPen)
            ElseIf TypeOf b.Shape Is CircleCollider Then
                DrawBody(g, b, New SolidBrush(Cyan), New Pen(Color.FromArgb(190, 240, 255), 2))
            Else
                DrawBody(g, b, limbFill, limbPen)
            End If
        Next

        ' 骨架连线（更高亮的霓虹线）
        DrawSkeleton(g)
    End Sub

    Private Sub EnableDoubleBuffer(ctl As Control)
        Dim pi = GetType(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic Or BindingFlags.Instance)
        pi.SetValue(ctl, True)
    End Sub

    Private Sub DrawGrid(g As Graphics)
        Using pen = New Pen(Color.FromArgb(20, 30, 48), 1)
            Dim gridStep As Single = 80.0F
            Dim sx As Single = CSng(-(camX * scale) Mod gridStep)
            Dim x As Single = sx
            While x < worldPanel.Width
                g.DrawLine(pen, x, 0.0F, x, CSng(worldPanel.Height))
                x += gridStep
            End While
            Dim y As Single = 0.0F
            While y < worldPanel.Height
                g.DrawLine(pen, 0.0F, y, CSng(worldPanel.Width), y)
                y += gridStep
            End While
        End Using
    End Sub

    Private Sub DrawBody(g As Graphics, b As RigidBody, fill As Brush, stroke As Pen)
        If TypeOf b.Shape Is PolygonCollider Then
            Dim poly = CType(b.Shape, PolygonCollider)
            Dim pts(poly.Count - 1) As PointF
            For i As Integer = 0 To poly.Count - 1
                Dim w = b.Position + Rotate(poly.vertices(i), b.Rotation)
                pts(i) = W2S(w)
            Next
            g.FillPolygon(fill, pts)
            g.DrawPolygon(stroke, pts)
        ElseIf TypeOf b.Shape Is CircleCollider Then
            Dim c = CType(b.Shape, CircleCollider)
            Dim ctr = W2S(b.Position)
            Dim r = CSng(c.Radius * scale)
            g.FillEllipse(fill, ctr.X - r, ctr.Y - r, r * 2, r * 2)
            g.DrawEllipse(stroke, ctr.X - r, ctr.Y - r, r * 2, r * 2)
            Dim rr = Rotate(New Vector2(c.Radius, 0), b.Rotation)
            g.DrawLine(stroke, ctr.X, ctr.Y, ctr.X + CSng(rr.x * scale), ctr.Y + CSng(rr.y * scale))
        End If
    End Sub

    Private Sub DrawSkeleton(g As Graphics)
        Using pen = New Pen(Color.FromArgb(150, 240, 255), 1.5F)
            For Each seg In env.stick.Skeleton()
                Dim a = W2S(seg.a)
                Dim b = W2S(seg.b)
                g.DrawLine(pen, a, b)
            Next
        End Using
    End Sub

    Private Sub DrawGoal(g As Graphics)
        Dim basePt = W2S(New Vector2(StickmanEnv.GoalX, StickmanEnv.GroundTopY))
        Dim topY = StickmanEnv.GroundTopY - 90
        Dim topPt = W2S(New Vector2(StickmanEnv.GoalX, topY))
        Using pole = New Pen(Green, 3), flag = New SolidBrush(Orange)
            g.DrawLine(pole, basePt.X, basePt.Y, topPt.X, topPt.Y)
            Dim fx = topPt.X
            Dim fy = topPt.Y
            g.FillPolygon(flag, New PointF() {
                    New PointF(fx, fy),
                    New PointF(fx + 34, fy + 12),
                    New PointF(fx, fy + 24)
                })
        End Using
        Dim f = Consolas10
        Dim br = New SolidBrush(Green)
        g.DrawString("终点 GOAL", f, br, topPt.X - 6, topPt.Y - 18)
    End Sub

    ' ---------------- 学习曲线 ----------------

    Private Sub DrawChart(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.Clear(BG)

        If episodeRewards.Count = 0 Then
            Dim ft = Consolas10, br = New SolidBrush(TextDim)
            g.DrawString("训练开始后，将在此显示每回合累计奖励（青）与前进距离（橙）的学习曲线", ft, br, 14, 12)
            Return
        End If

        Dim W = bottomPanel.Width, H = bottomPanel.Height
        Dim padL = 46, padR = 14, padT = 26, padB = 26
        Dim plotW = W - padL - padR
        Dim plotH = H - padT - padB

        Dim n = episodeRewards.Count
        Dim maxR = 1.0, maxD = 1.0
        For Each v In episodeRewards
            If v > maxR Then maxR = v
        Next
        For Each v In episodeDist
            If v > maxD Then maxD = v
        Next

        Using axis = New Pen(Color.FromArgb(71, 85, 105), 1)
            g.DrawLine(axis, padL, padT, padL, padT + plotH)
            g.DrawLine(axis, padL, padT + plotH, padL + plotW, padT + plotH)
        End Using

        ' 奖励（青）
        Using pen = New Pen(Cyan, 2)
            For i As Integer = 1 To n - 1
                Dim x0 = padL + (i - 1) / (n - 1) * plotW
                Dim x1 = padL + i / (n - 1) * plotW
                Dim y0 = padT + plotH - (episodeRewards(i - 1) / maxR) * plotH
                Dim y1 = padT + plotH - (episodeRewards(i) / maxR) * plotH
                g.DrawLine(pen, CSng(x0), CSng(y0), CSng(x1), CSng(y1))
            Next
        End Using

        ' 距离（橙）
        Using pen = New Pen(Orange, 2)
            For i As Integer = 1 To n - 1
                Dim x0 = padL + (i - 1) / (n - 1) * plotW
                Dim x1 = padL + i / (n - 1) * plotW
                Dim y0 = padT + plotH - (episodeDist(i - 1) / maxD) * plotH
                Dim y1 = padT + plotH - (episodeDist(i) / maxD) * plotH
                g.DrawLine(pen, CSng(x0), CSng(y0), CSng(x1), CSng(y1))
            Next
        End Using

        ' 图例 + 轴标
        Dim f = Consolas10
        g.DrawString("累计奖励", f, New SolidBrush(Cyan), padL + 4, 6)
        g.DrawString("前进距离(px)", f, New SolidBrush(Orange), padL + 100, 6)
        g.DrawString("回合 →", f, New SolidBrush(TextDim), padL + plotW - 44, padT + plotH + 6)
        g.DrawString("maxR=" & maxR.ToString("F1"), f, New SolidBrush(TextDim), 2, padT)
        g.DrawString("maxD=" & CInt(maxD), f, New SolidBrush(TextDim), 2, padT + plotH - 14)
    End Sub

End Class
