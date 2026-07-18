#Region "Microsoft.VisualBasic::9c63b8a4e4814820aa42e6a5bc6f643c, Data_science\MachineLearning\CellularAutomaton\GameOfLife.WinForms\MainForm.vb"

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

    '   Total Lines: 443
    '    Code Lines: 367 (82.84%)
    ' Comment Lines: 26 (5.87%)
    '    - Xml Docs: 23.08%
    ' 
    '   Blank Lines: 50 (11.29%)
    '     File Size: 17.06 KB


    ' Class MainForm
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CellAt, CurrentBoundary, CurrentNeighbor, MakeButton, SectionLabel
    ' 
    '     Sub: BtnClear_Click, BtnRandom_Click, BtnReset_Click, BtnStart_Click, BtnStep_Click
    '          BuildSidebar, Canvas_MouseDown, Canvas_MouseMove, Canvas_MouseUp, ClearGrid
    '          CreateSimulator, InitializeComponent, RandomizeGrid, Render, StepOnce
    '          Timer_Tick, UpdateStatus, WireEvents
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.MachineLearning.CellularAutomaton

''' <summary>
''' 康威生命游戏（Conway's Game of Life）可视化演示。
''' 直接复用本仓库 CellularAutomaton 类库的 <see cref="Simulator(Of T)"/> 与 <see cref="Individual"/>，
''' 可在冯·诺依曼型 / 摩尔型 / 扩展摩尔型三种邻居拓扑及有界/环面边界下运行，
''' 直观验证类库新增的邻居类型能力。
''' </summary>
Public Class MainForm
    Inherits Form

    ' ---- 配色（深色科技控制台风格）----
    Private ReadOnly ColBG As Color = Color.FromArgb(&HFF, &H16, &H16, &H1F)
    Private ReadOnly ColPanel As Color = Color.FromArgb(&HFF, &H1E, &H1E, &H2E)
    Private ReadOnly ColDead As Color = Color.FromArgb(&HFF, &H10, &H10, &H18)
    Private ReadOnly ColAlive As Color = Color.FromArgb(&HFF, &H0, &HE5, &HFF)
    Private ReadOnly ColAliveGlow As Color = Color.FromArgb(50, &H0, &HE5, &HFF)
    Private ReadOnly ColText As Color = Color.FromArgb(&HFF, &HE0, &HE0, &HE0)
    Private ReadOnly ColTextDim As Color = Color.FromArgb(&HFF, &HB0, &HC5)
    Private ReadOnly ColBtnBg As Color = Color.FromArgb(&HFF, &H2A, &H2A, &H3C)
    Private ReadOnly ColBtnHover As Color = Color.FromArgb(&HFF, &H29, &H79, &HFF)
    Private ReadOnly ColAccent As Color = Color.FromArgb(&HFF, &H0, &HE5, &HFF)

    ' ---- 控件 ----
    Private sidebar As Panel
    Private flow As FlowLayoutPanel
    Private scrollPanel As Panel
    Private canvas As PictureBox
    Private statusStrip As StatusStrip
    Private btnStart As Button
    Private btnStep As Button
    Private btnRandom As Button
    Private btnClear As Button
    Private btnReset As Button
    Private cmbNeighbor As ComboBox
    Private cmbBoundary As ComboBox
    Private numSize As NumericUpDown
    Private trkSpeed As TrackBar
    Private lblGen As ToolStripStatusLabel
    Private lblAlive As ToolStripStatusLabel
    Private lblFps As ToolStripStatusLabel
    Private lblRule As ToolStripStatusLabel

    ' ---- 状态 ----
    Private simulator As Simulator(Of ConwayCell)
    Private cellSize As Integer = 7
    Private generation As Integer = 0
    Private aliveCount As Integer = 0
    Private isRunning As Boolean = False
    Private bmp As Bitmap = Nothing
    Private ReadOnly rnd As New Random()
    Private WithEvents timer As New Timer()

    Public Sub New()
        MyBase.New()
        Call InitializeComponent()
        Call CreateSimulator(preserve:=False)
        Call RandomizeGrid()
    End Sub

    ' =====================================================================
    '  UI 构建
    ' =====================================================================
    Private Sub InitializeComponent()
        Me.Text = "Conway's Game of Life · 元胞自动机"
        Me.Size = New Size(1120, 740)
        Me.MinimumSize = New Size(860, 560)
        Me.BackColor = ColBG
        Me.Font = New Font("Segoe UI", 11)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.DoubleBuffered = True

        ' 侧边栏
        sidebar = New Panel() With {
            .Dock = DockStyle.Left,
            .Width = 320,
            .BackColor = ColPanel
        }
        flow = New FlowLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = ColPanel,
            .Padding = New Padding(18, 18, 18, 18)
        }
        sidebar.Controls.Add(flow)

        ' 画布容器
        scrollPanel = New Panel() With {
            .Dock = DockStyle.Fill,
            .BackColor = ColDead,
            .AutoScroll = True
        }
        canvas = New PictureBox() With {
            .Location = New Point(0, 0),
            .SizeMode = PictureBoxSizeMode.AutoSize,
            .BackColor = ColDead,
            .Cursor = Cursors.Cross
        }
        scrollPanel.Controls.Add(canvas)

        ' 状态栏
        statusStrip = New StatusStrip() With {
            .Dock = DockStyle.Bottom,
            .BackColor = ColPanel,
            .ForeColor = ColText,
            .SizingGrip = False
        }
        lblGen = New ToolStripStatusLabel("世代 0") With {.ForeColor = ColAccent}
        lblAlive = New ToolStripStatusLabel("存活 0") With {.ForeColor = ColText}
        lblFps = New ToolStripStatusLabel("FPS 0") With {.ForeColor = ColTextDim}
        lblRule = New ToolStripStatusLabel("规则 B3/S23") With {.ForeColor = ColTextDim, .Spring = True, .TextAlign = ContentAlignment.MiddleRight}
        statusStrip.Items.AddRange({lblGen, lblAlive, lblFps, lblRule})

        ' 布局顺序：先停靠侧边/底，最后 Fill 填充剩余
        Me.Controls.Add(sidebar)
        Me.Controls.Add(statusStrip)
        Me.Controls.Add(scrollPanel)

        Call BuildSidebar()
        Call WireEvents()
    End Sub

    Private Sub BuildSidebar()
        Dim title = New Label() With {
            .Text = "CONWAY'S GAME OF LIFE",
            .ForeColor = ColAccent,
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .Width = 284, .Height = 30
        }
        Dim subTitle = New Label() With {
            .Text = "Cellular Automaton · 元胞自动机 Demo",
            .ForeColor = ColTextDim,
            .Font = New Font("Segoe UI", 10),
            .Width = 284, .Height = 18,
            .Margin = New Padding(0, 2, 0, 14)
        }

        btnStart = MakeButton("▶ 开始", AddressOf BtnStart_Click)
        btnStep = MakeButton("⏭ 单步", AddressOf BtnStep_Click)
        btnRandom = MakeButton("🎲 随机", AddressOf BtnRandom_Click)
        btnClear = MakeButton("⌫ 清空", AddressOf BtnClear_Click)
        btnReset = MakeButton("↺ 重置", AddressOf BtnReset_Click)

        Dim secNeighbor = SectionLabel("邻居类型 (Neighborhood)")
        cmbNeighbor = New ComboBox() With {
            .Width = 284, .DropDownStyle = ComboBoxStyle.DropDownList,
            .BackColor = ColBtnBg, .ForeColor = ColText, .FlatStyle = FlatStyle.Flat
        }
        cmbNeighbor.Items.Add("冯·诺依曼型 Von Neumann (4)")
        cmbNeighbor.Items.Add("摩尔型 Moore (8)")
        cmbNeighbor.Items.Add("扩展摩尔型 Extended Moore (24)")
        cmbNeighbor.SelectedIndex = 1 ' 经典康威生命游戏使用摩尔型

        Dim secBoundary = SectionLabel("边界模式 (Boundary)")
        cmbBoundary = New ComboBox() With {
            .Width = 284, .DropDownStyle = ComboBoxStyle.DropDownList,
            .BackColor = ColBtnBg, .ForeColor = ColText, .FlatStyle = FlatStyle.Flat
        }
        cmbBoundary.Items.Add("有界 Bounded")
        cmbBoundary.Items.Add("环面 Toroidal")
        cmbBoundary.SelectedIndex = 1 ' 环面更适合演示

        Dim secSize = SectionLabel("网格尺寸 (宽=高)")
        numSize = New NumericUpDown() With {
            .Width = 284, .Minimum = 10, .Maximum = 200,
            .Value = 90, .Increment = 5,
            .BackColor = ColBtnBg, .ForeColor = ColText
        }

        Dim secSpeed = SectionLabel("演化速度 (Speed)")
        trkSpeed = New TrackBar() With {
            .Width = 284, .Minimum = 1, .Maximum = 100, .Value = 75,
            .TickStyle = TickStyle.None, .BackColor = ColPanel
        }

        Dim hint = New Label() With {
            .Text = "提示：暂停状态下可在网格上点击 / 拖拽绘制或擦除细胞。",
            .ForeColor = ColTextDim, .Font = New Font("Segoe UI", 9),
            .Width = 284, .Height = 30,
            .Margin = New Padding(0, 10, 0, 0)
        }

        flow.Controls.AddRange({
            title, subTitle,
            btnStart, btnStep, btnRandom, btnClear, btnReset,
            secNeighbor, cmbNeighbor,
            secBoundary, cmbBoundary,
            secSize, numSize,
            secSpeed, trkSpeed,
            hint
        })
    End Sub

    Private Function MakeButton(text As String, click As EventHandler) As Button
        Dim b = New Button() With {
            .Text = text,
            .Width = 284, .Height = 42,
            .FlatStyle = FlatStyle.Flat,
            .BackColor = ColBtnBg,
            .ForeColor = ColText,
            .Font = New Font("Segoe UI", 11),
            .Cursor = Cursors.Hand,
            .Margin = New Padding(0, 0, 0, 10)
        }
        b.FlatAppearance.BorderSize = 0
        AddHandler b.MouseEnter, Sub(s, e) b.BackColor = ColBtnHover
        AddHandler b.MouseLeave, Sub(s, e) b.BackColor = ColBtnBg
        AddHandler b.Click, click
        Return b
    End Function

    Private Function SectionLabel(text As String) As Label
        Return New Label() With {
            .Text = text,
            .ForeColor = ColAccent,
            .Font = New Font("Segoe UI", 11, FontStyle.Bold),
            .Width = 284, .Height = 22,
            .Margin = New Padding(0, 14, 0, 4)
        }
    End Function

    Private Sub WireEvents()
        AddHandler cmbNeighbor.SelectedIndexChanged, Sub(s, e) CreateSimulator(preserve:=True)
        AddHandler cmbBoundary.SelectedIndexChanged, Sub(s, e) CreateSimulator(preserve:=True)
        AddHandler numSize.ValueChanged, Sub(s, e) CreateSimulator(preserve:=True)
        AddHandler trkSpeed.ValueChanged, Sub(s, e)
                                              timer.Interval = Math.Max(16, 1000 - trkSpeed.Value * 9)
                                              Call UpdateStatus()
                                          End Sub

        AddHandler canvas.MouseDown, AddressOf Canvas_MouseDown
        AddHandler canvas.MouseMove, AddressOf Canvas_MouseMove
        AddHandler canvas.MouseUp, AddressOf Canvas_MouseUp
        AddHandler canvas.MouseLeave, AddressOf Canvas_MouseUp
    End Sub

    ' =====================================================================
    ' 模拟器管理
    ' =====================================================================
    Private Function CurrentNeighbor() As NeighborhoodType
        Return CType(cmbNeighbor.SelectedIndex, NeighborhoodType)
    End Function

    Private Function CurrentBoundary() As BoundaryMode
        Return CType(cmbBoundary.SelectedIndex, BoundaryMode)
    End Function

    Private Sub CreateSimulator(preserve As Boolean)
        Dim old = simulator
        Dim sz = New Size(CInt(numSize.Value), CInt(numSize.Value))
        Dim newSim = New Simulator(Of ConwayCell)(
            sz,
            Function() New ConwayCell(),
            CurrentNeighbor(),
            CurrentBoundary())

        If preserve AndAlso old IsNot Nothing Then
            Dim rows = Math.Min(sz.Height, old.size.Height)
            Dim cols = Math.Min(sz.Width, old.size.Width)
            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    newSim.CellData(i, j).State = old.CellData(i, j).State
                Next
            Next
        End If

        simulator = newSim
        generation = 0
        bmp = Nothing
        Call Render()
    End Sub

    Private Sub RandomizeGrid()
        If simulator Is Nothing Then Return
        For i As Integer = 0 To simulator.size.Height - 1
            For j As Integer = 0 To simulator.size.Width - 1
                simulator.CellData(i, j).State = (rnd.NextDouble() < 0.28)
            Next
        Next
        generation = 0
        Call Render()
    End Sub

    Private Sub ClearGrid()
        If simulator Is Nothing Then Return
        For i As Integer = 0 To simulator.size.Height - 1
            For j As Integer = 0 To simulator.size.Width - 1
                simulator.CellData(i, j).State = False
            Next
        Next
        generation = 0
        Call Render()
    End Sub

    ' =====================================================================
    ' 渲染
    ' =====================================================================
    Private Sub Render()
        If simulator Is Nothing Then Return
        Dim cols = simulator.size.Width
        Dim rows = simulator.size.Height
        Dim w = cols * cellSize
        Dim h = rows * cellSize

        If bmp Is Nothing OrElse bmp.Width <> w OrElse bmp.Height <> h Then
            If bmp IsNot Nothing Then bmp.Dispose()
            bmp = New Bitmap(w, h)
            canvas.Image = bmp
            canvas.Size = New Size(w, h)
        End If

        aliveCount = 0
        Using g = Graphics.FromImage(bmp)
            g.Clear(ColDead)
            Using glow = New SolidBrush(ColAliveGlow)
                Using brush = New SolidBrush(ColAlive)
                    For i As Integer = 0 To rows - 1
                        For j As Integer = 0 To cols - 1
                            If simulator.CellData(i, j).State Then
                                aliveCount += 1
                                g.FillRectangle(glow, j * cellSize, i * cellSize, cellSize, cellSize)
                                g.FillRectangle(brush, j * cellSize + 1, i * cellSize + 1, cellSize - 2, cellSize - 2)
                            End If
                        Next
                    Next
                End Using
            End Using
        End Using

        canvas.Invalidate()
        Call UpdateStatus()
    End Sub

    Private Sub UpdateStatus()
        Dim name As String = "摩尔型"
        Select Case CurrentNeighbor()
            Case NeighborhoodType.VonNeumann : name = "冯·诺依曼型 (4)"
            Case NeighborhoodType.Moore : name = "摩尔型 (8)"
            Case NeighborhoodType.ExtendedMoore : name = "扩展摩尔型 (24)"
        End Select

        Dim fps = If(isRunning, Math.Round(1000.0 / timer.Interval, 1), 0.0)

        lblGen.Text = "世代 " & generation
        lblAlive.Text = "存活 " & aliveCount
        lblFps.Text = "FPS " & fps
        lblRule.Text = "规则 B3/S23 · 邻居 " & name
    End Sub

    ' =====================================================================
    ' 演化与交互
    ' =====================================================================
    Private Sub StepOnce()
        simulator.Run(False)
        generation += 1
        Call Render()
    End Sub

    Private Sub BtnStart_Click(sender As Object, e As EventArgs)
        isRunning = Not isRunning
        If isRunning Then
            timer.Interval = Math.Max(16, 1000 - trkSpeed.Value * 9)
            timer.Start()
            btnStart.Text = "⏸ 暂停"
        Else
            timer.Stop()
            btnStart.Text = "▶ 开始"
        End If
        Call UpdateStatus()
    End Sub

    Private Sub BtnStep_Click(sender As Object, e As EventArgs)
        If isRunning Then
            isRunning = False
            timer.Stop()
            btnStart.Text = "▶ 开始"
        End If
        Call StepOnce()
    End Sub

    Private Sub BtnRandom_Click(sender As Object, e As EventArgs)
        Call RandomizeGrid()
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As EventArgs)
        Call ClearGrid()
    End Sub

    Private Sub BtnReset_Click(sender As Object, e As EventArgs)
        If isRunning Then
            isRunning = False
            timer.Stop()
            btnStart.Text = "▶ 开始"
        End If
        Call CreateSimulator(preserve:=False)
        Call RandomizeGrid()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
        Call StepOnce()
    End Sub

    ' ---- 鼠标绘制 ----
    Private drawing As Boolean = False
    Private drawValue As Boolean = False

    Private Function CellAt(x As Integer, y As Integer) As Point
        Dim j = x \ cellSize
        Dim i = y \ cellSize
        If i < 0 OrElse j < 0 OrElse i >= simulator.size.Height OrElse j >= simulator.size.Width Then
            Return New Point(-1, -1)
        End If
        Return New Point(i, j)
    End Function

    Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then Return
        Dim c = CellAt(e.X, e.Y)
        If c.X < 0 Then Return
        drawing = True
        drawValue = Not simulator.CellData(c.X, c.Y).State
        simulator.CellData(c.X, c.Y).State = drawValue
        Call Render()
    End Sub

    Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs)
        If Not drawing Then Return
        Dim c = CellAt(e.X, e.Y)
        If c.X < 0 Then Return
        simulator.CellData(c.X, c.Y).State = drawValue
        Call Render()
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As EventArgs)
        drawing = False
    End Sub

End Class
