#Region "Microsoft.VisualBasic::3ec0f424cfde435fa7a4ba4fe4cef64e, Data_science\Visualization\Expression3DPlotter\MainForm.vb"

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

'   Total Lines: 337
'    Code Lines: 267 (79.23%)
' Comment Lines: 19 (5.64%)
'    - Xml Docs: 26.32%
' 
'   Blank Lines: 51 (15.13%)
'     File Size: 14.69 KB


' Class MainForm
' 
'     Constructor: (+1 Overloads) Sub New
' 
'     Function: Btn, BuildPresets, Cbo, Chk, Lbl
'               Num, Txt
' 
'     Sub: ApplyMode, AxesChanged, InitializeComponent, MainForm_Load, ModeChanged
'          OnDraw, OnReset, OnZoom, PresetSelected, UpdateStatus
'     Class PresetInfo
' 
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Microsoft.VisualBasic.Math.Scripting

''' <summary>
''' 三维数学表达式绘图器主窗体（light 亮色主题）。
''' 提供表达式输入框、采样范围/分辨率、配色选择、绘制与重置按钮，
''' 以及可交互的三维画布（左键旋转 / 右键平移 / 滚轮缩放）。
''' </summary>
Public Class MainForm : Inherits Form

    Private WithEvents canvas As SurfaceCanvas
    Private WithEvents topPanel As Panel
    Private WithEvents statusStrip As StatusStrip
    Private WithEvents lblStatus As ToolStripStatusLabel

    ' 模式与表达式
    Private WithEvents cboMode As ComboBox
    Private WithEvents lblExpr As Label
    Private WithEvents txtSurface As TextBox
    Private WithEvents txtCurveX As TextBox
    Private WithEvents txtCurveY As TextBox
    Private WithEvents txtCurveZ As TextBox

    ' 曲面范围
    Private WithEvents lblXr As Label
    Private WithEvents numXmin As NumericUpDown
    Private WithEvents lblXto As Label
    Private WithEvents numXmax As NumericUpDown
    Private WithEvents lblYr As Label
    Private WithEvents numYmin As NumericUpDown
    Private WithEvents lblYto As Label
    Private WithEvents numYmax As NumericUpDown

    ' 曲线范围
    Private WithEvents lblTr As Label
    Private WithEvents numTmin As NumericUpDown
    Private WithEvents lblTto As Label
    Private WithEvents numTmax As NumericUpDown

    ' 其它
    Private WithEvents lblDiv As Label
    Private WithEvents numDiv As NumericUpDown
    Private WithEvents lblScheme As Label
    Private WithEvents cboScheme As ComboBox
    Private WithEvents cboPreset As ComboBox
    Private WithEvents lblPreset As Label
    Private WithEvents chkAxes As CheckBox
    Private WithEvents btnDraw As Button
    Private WithEvents btnReset As Button

    ' 脚本模式相关
    Private pic2D As PictureBox
    Private btnScript As Button
    Private chkBox As CheckBox
    Private chkTicks As CheckBox
    Private editor As ScriptEditorForm = Nothing

    Public Sub New()
        InitializeComponent()
        SetupScriptUI()
    End Sub

    Private Sub InitializeComponent()
        ' ---- 窗体 ----
        Me.Text = "三维数学表达式绘图器"
        Me.Size = New Size(1000, 720)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = SystemColors.Control
        Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)
        Me.MinimumSize = New Size(600, 420)

        ' ---- 顶部工具栏面板 ----
        topPanel = New Panel() With {
            .Dock = DockStyle.Top, .Height = 86, .BackColor = Color.White,
            .Padding = New Padding(8), .BorderStyle = BorderStyle.FixedSingle}
        Me.Controls.Add(topPanel)

        ' ---- 画布 ----
        canvas = New SurfaceCanvas() With {.Dock = DockStyle.Fill, .BackColor = Color.White}
        canvas.Scene = New PlotScene()
        Me.Controls.Add(canvas)

        ' ---- 状态栏 ----
        statusStrip = New StatusStrip()
        lblStatus = New ToolStripStatusLabel() With {.Text = "就绪"}
        statusStrip.Items.Add(lblStatus)
        statusStrip.Dock = DockStyle.Bottom
        Me.Controls.Add(statusStrip)



        lblExpr = Lbl("z = f(x, y) :", 136, 10, 96)
        txtSurface = Txt("sin(sqrt(x*x + y*y))", 236, 8, 240)
        txtCurveX = Txt("2*sin(t)", 236, 8, 78)
        txtCurveY = Txt("2*cos(t)", 320, 8, 78)
        txtCurveZ = Txt("0.3*t", 404, 8, 78)

        btnDraw = Btn("绘制", 492, 7, 70)
        btnReset = Btn("重置视角", 570, 7, 84)

        ' 第二行：范围
        lblXr = Lbl("X:", 8, 52, 18)
        numXmin = Num(28, 50, 64, -8, -100000, 100000, 1)
        lblXto = Lbl("~", 94, 52, 14)
        numXmax = Num(108, 50, 64, 8, -100000, 100000, 1)
        lblYr = Lbl("Y:", 180, 52, 18)
        numYmin = Num(200, 50, 64, -8, -100000, 100000, 1)
        lblYto = Lbl("~", 266, 52, 14)
        numYmax = Num(280, 50, 64, 8, -100000, 100000, 1)

        lblTr = Lbl("t:", 8, 52, 18)
        numTmin = Num(28, 50, 64, 0, -100000, 100000, 1)
        lblTto = Lbl("~", 94, 52, 14)
        numTmax = Num(108, 50, 64, 12.566, -100000, 100000, 2)

        lblDiv = Lbl("分辨率:", 360, 52, 50)
        numDiv = Num(414, 50, 56, 60, 5, 400, 0)

        lblScheme = Lbl("配色:", 480, 52, 40)
        cboScheme = Cbo(524, 50, 100, {"viridis", "magma", "inferno", "plasma", "turbo", "jet", "rainbow", "cividis", "mako", "rocket"})
        cboScheme.SelectedIndex = 0

        chkAxes = Chk("显示坐标轴", 632, 51, 92)
        ' ===================== 顶部控件 =====================
        cboMode = Cbo(8, 8, 120, {"曲面  z = f(x, y)", "曲线  x(t), y(t), z(t)"})
        cboMode.SelectedIndex = 0
        ApplyMode()

        ' ---- 预设曲面下拉框 ----
        lblPreset = Lbl("预设:", 660, 10, 40)
        Dim presetNames = BuildPresets().Select(Function(p) p.Name).ToArray()
        cboPreset = Cbo(704, 8, 220, presetNames)
        cboPreset.SelectedIndex = -1
    End Sub

#Region "控件辅助构造"

    Private Function Lbl(text$, x%, y%, w%) As Label
        Dim c = New Label() With {
            .Text = text, .Location = New Point(x, y), .Size = New Size(w, 20),
            .ForeColor = Color.FromArgb(33, 33, 33), .Font = New Font("Segoe UI", 9, FontStyle.Regular),
            .TextAlign = ContentAlignment.MiddleLeft}
        topPanel.Controls.Add(c)
        Return c
    End Function

    Private Function Txt(text$, x%, y%, w%) As TextBox
        Dim c = New TextBox() With {
            .Text = text, .Location = New Point(x, y), .Size = New Size(w, 23),
            .Font = New Font("Segoe UI", 10, FontStyle.Regular), .BorderStyle = BorderStyle.FixedSingle}
        topPanel.Controls.Add(c)
        Return c
    End Function

    Private Function Num(x%, y%, w%, v#, mn#, mx#, decimals%) As NumericUpDown
        Dim c = New NumericUpDown() With {
            .Location = New Point(x, y), .Size = New Size(w, 23),
             .Minimum = CDec(mn), .Maximum = CDec(mx), .DecimalPlaces = decimals,
            .Increment = CDec(0.5), .Font = New Font("Segoe UI", 9, FontStyle.Regular), .Value = CDec(v)}
        topPanel.Controls.Add(c)
        Return c
    End Function

    Private Function Cbo(x%, y%, w%, items$()) As ComboBox
        Dim c = New ComboBox() With {
            .Location = New Point(x, y), .Size = New Size(w, 23),
            .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Segoe UI", 9, FontStyle.Regular)}
        c.Items.AddRange(items)
        topPanel.Controls.Add(c)
        Return c
    End Function

    Private Function Btn(text$, x%, y%, w%) As Button
        Dim c = New Button() With {
            .Text = text, .Location = New Point(x, y), .Size = New Size(w, 26),
            .Font = New Font("Segoe UI", 9, FontStyle.Regular), .BackColor = Color.FromArgb(21, 101, 192),
            .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .Cursor = Cursors.Hand}
        c.FlatAppearance.BorderSize = 0
        topPanel.Controls.Add(c)
        Return c
    End Function

    Private Function Chk(text$, x%, y%, w%) As CheckBox
        Dim c = New CheckBox() With {
            .Text = text, .Location = New Point(x, y), .Size = New Size(w, 23),
            .ForeColor = Color.FromArgb(33, 33, 33), .Font = New Font("Segoe UI", 9, FontStyle.Regular),
            .Checked = True}
        topPanel.Controls.Add(c)
        Return c
    End Function

#End Region

#Region "交互逻辑"

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Call ImageDriver.Register()
        ' 默认演示：启动即渲染一个三维曲面
        OnDraw(Me, EventArgs.Empty)
    End Sub

    Private Sub ModeChanged(sender As Object, e As EventArgs) Handles cboMode.SelectedIndexChanged
        ApplyMode()
        If cboMode.SelectedIndex = 0 Then
            txtSurface.Text = "sin(sqrt(x*x + y*y))"
        Else
            txtCurveX.Text = "2*sin(t)" : txtCurveY.Text = "2*cos(t)" : txtCurveZ.Text = "0.3*t"
        End If
    End Sub

    Private Sub ApplyMode()
        Dim isSurface = (cboMode.SelectedIndex = 0)
        txtSurface.Visible = isSurface
        txtCurveX.Visible = Not isSurface
        txtCurveY.Visible = Not isSurface
        txtCurveZ.Visible = Not isSurface
        lblExpr.Text = If(isSurface, "z = f(x, y) :", "x(t), y(t), z(t) :")

        lblXr.Visible = isSurface : numXmin.Visible = isSurface : lblXto.Visible = isSurface : numXmax.Visible = isSurface
        lblYr.Visible = isSurface : numYmin.Visible = isSurface : lblYto.Visible = isSurface : numYmax.Visible = isSurface

        lblTr.Visible = Not isSurface : numTmin.Visible = Not isSurface : lblTto.Visible = Not isSurface : numTmax.Visible = Not isSurface
    End Sub

    Private Sub OnDraw(sender As Object, e As EventArgs) Handles btnDraw.Click
        If canvas.Scene Is Nothing Then Return
        canvas.Visible = True
        If pic2D IsNot Nothing Then pic2D.Visible = False
        Try
            txtSurface.BackColor = Color.White
            txtCurveX.BackColor = Color.White
            txtCurveY.BackColor = Color.White
            txtCurveZ.BackColor = Color.White

            If cboMode.SelectedIndex = 0 Then
                Dim zEval = New ExpressionEvaluator(txtSurface.Text)
                canvas.Scene.SetSurface(zEval,
                    CDbl(numXmin.Value), CDbl(numXmax.Value),
                    CDbl(numYmin.Value), CDbl(numYmax.Value),
                    CInt(numDiv.Value), CStr(cboScheme.Text))
            Else
                Dim xE = New ExpressionEvaluator(txtCurveX.Text)
                Dim yE = New ExpressionEvaluator(txtCurveY.Text)
                Dim zE = New ExpressionEvaluator(txtCurveZ.Text)
                canvas.Scene.SetCurve(xE, yE, zE,
                    CDbl(numTmin.Value), CDbl(numTmax.Value),
                    CInt(numDiv.Value), CStr(cboScheme.Text))
            End If

            canvas.Invalidate()
            UpdateStatus("就绪")
        Catch ex As Exception
            UpdateStatus("表达式错误: " & ex.Message)
            If cboMode.SelectedIndex = 0 Then
                txtSurface.BackColor = Color.FromArgb(255, 255, 200, 200)
            Else
                txtCurveX.BackColor = Color.FromArgb(255, 255, 200, 200)
                txtCurveY.BackColor = Color.FromArgb(255, 255, 200, 200)
                txtCurveZ.BackColor = Color.FromArgb(255, 255, 200, 200)
            End If
        End Try
    End Sub

    Private Sub OnReset(sender As Object, e As EventArgs) Handles btnReset.Click
        If canvas.Scene Is Nothing Then Return
        canvas.Scene.Camera.AngleX = 20
        canvas.Scene.Camera.AngleY = -30
        canvas.Scene.Camera.AngleZ = 0
        canvas.Scene.Camera.Offset = New PointF(0, 0)
        canvas.Scene.FitView()
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub AxesChanged(sender As Object, e As EventArgs) Handles chkAxes.CheckedChanged
        canvas.Scene.ShowAxes = chkAxes.Checked
        canvas.Invalidate()
    End Sub

    Private Sub OnZoom(delta As Integer) Handles canvas.Zoom
        If canvas.Scene Is Nothing Then Return
        Dim factor = If(delta > 0, 0.9F, 1.1F)
        canvas.Scene.Camera.ViewDistance = Math.Max(1.0F, canvas.Scene.Camera.ViewDistance * factor)
        canvas.Invalidate()
        UpdateStatus()
    End Sub

    Private Sub UpdateStatus(Optional msg$ = "")
        If canvas.Scene Is Nothing Then Return
        Dim s = canvas.Scene
        lblStatus.Text =
            $"{msg}  |  角度X:{s.Camera.AngleX:F1} Y:{s.Camera.AngleY:F1}  |  " &
            $"视距:{s.Camera.ViewDistance:F1}  |  半径:{s.ModelRadius:F1}  |  " &
            $"面:{s.SurfaceCount} 点:{s.PointCount}  |  配色:{cboScheme.Text}"
    End Sub

#End Region

#Region "预设曲面"

    Private Class PresetInfo
        Public Name As String
        Public Expression As String
        Public XMin As Double
        Public XMax As Double
        Public YMin As Double
        Public YMax As Double
    End Class

    Private Function BuildPresets() As List(Of PresetInfo)
        Return New List(Of PresetInfo) From {
            New PresetInfo With {.Name = "正弦波纹 (Sine Ripple)", .Expression = "sin(sqrt(x*x + y*y))", .XMin = -8, .XMax = 8, .YMin = -8, .YMax = 8},
            New PresetInfo With {.Name = "抛物面 (Paraboloid)", .Expression = "x*x + y*y", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "鞍面 (Saddle)", .Expression = "x*x - y*y", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "双曲抛物面 (Hyperbolic Saddle)", .Expression = "x*y", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "高斯钟形 (Gaussian Bell)", .Expression = "exp(-(x*x + y*y)/5)", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "余弦波纹 (Cosine Ripple)", .Expression = "cos(x) * cos(y)", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "墨西哥帽 (Mexican Hat)", .Expression = "(1 - x*x - y*y) * exp(-(x*x + y*y)/2)", .XMin = -4, .XMax = 4, .YMin = -4, .YMax = 4},
            New PresetInfo With {.Name = "倒置抛物面 (Inverted Paraboloid)", .Expression = "-(x*x + y*y)", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "猴鞍面 (Monkey Saddle)", .Expression = "x*x*x - 3*x*y*y", .XMin = -3, .XMax = 3, .YMin = -3, .YMax = 3},
            New PresetInfo With {.Name = "混合正弦 (Sine-Cosine Mix)", .Expression = "sin(x) + cos(y)", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "对数曲面 (Logarithmic)", .Expression = "log(1 + x*x + y*y)", .XMin = -5, .XMax = 5, .YMin = -5, .YMax = 5},
            New PresetInfo With {.Name = "旋臂曲面 (Spiral Arms)", .Expression = "sin(x*y)", .XMin = -4, .XMax = 4, .YMin = -4, .YMax = 4}
        }
    End Function

    Private Sub PresetSelected(sender As Object, e As EventArgs) Handles cboPreset.SelectedIndexChanged
        If cboPreset.SelectedIndex < 0 Then Return
        Dim presets = BuildPresets()
        Dim p = presets(cboPreset.SelectedIndex)

        ' 切换到曲面模式
        cboMode.SelectedIndex = 0
        txtSurface.Text = p.Expression
        numXmin.Value = CDec(p.XMin)
        numXmax.Value = CDec(p.XMax)
        numYmin.Value = CDec(p.YMin)
        numYmax.Value = CDec(p.YMax)

        ' 自动绘制
        OnDraw(sender, e)
    End Sub

#End Region

    ' ===================== 脚本模式 =====================

    Private Sub SetupScriptUI()
        ' 二维画布（默认隐藏，脚本产出二维图时显示）
        pic2D = New PictureBox() With {.Dock = DockStyle.Fill, .Visible = False, .BackColor = Color.White}
        Me.Controls.Add(pic2D)

        ' 顶部面板加高，新增第三行放置脚本模式与三维开关
        topPanel.Height = 116

        btnScript = Btn("脚本模式", 8, 88, 100)
        chkBox = Chk("显示盒子网格面", 120, 89, 130)
        chkTicks = Chk("带刻度坐标轴", 260, 89, 120)
        chkBox.Checked = True
        chkTicks.Checked = False

        AddHandler btnScript.Click, AddressOf OnScriptClick
        AddHandler chkBox.CheckedChanged, AddressOf OnBoxChecked
        AddHandler chkTicks.CheckedChanged, AddressOf OnTicksChecked
    End Sub

    Private Sub OnScriptClick(sender As Object, e As EventArgs)
        If editor Is Nothing OrElse editor.IsDisposed Then
            editor = New ScriptEditorForm()
            AddHandler editor.ScriptExecuted, AddressOf OnScriptExecuted
        End If
        If editor.Visible Then editor.BringToFront() Else editor.Show(Me)
    End Sub

    Private Sub OnBoxChecked(sender As Object, e As EventArgs)
        If canvas.Scene IsNot Nothing Then canvas.Scene.ShowBox = chkBox.Checked
        canvas.Invalidate()
    End Sub

    Private Sub OnTicksChecked(sender As Object, e As EventArgs)
        If canvas.Scene IsNot Nothing Then canvas.Scene.ShowTicks = chkTicks.Checked
        canvas.Invalidate()
    End Sub

    Private Sub OnScriptExecuted(result As ScriptResult)
        If result Is Nothing OrElse Not result.Success Then
            UpdateStatus(If(result Is Nothing, "无结果", "脚本错误：" & result.ErrorMessage))
            Return
        End If
        If result.Commands.Count = 0 Then
            UpdateStatus("脚本未产生任何绘图指令")
            Return
        End If

        Dim cmds3D As New List(Of PlotCommand)()
        For Each c In result.Commands
            If c.Is3D Then cmds3D.Add(c)
        Next

        If cmds3D.Count > 0 Then
            canvas.Scene.Clear()
            For Each c In cmds3D
                Select Case c.Kind
                    Case PlotKind.Surface
                        canvas.Scene.SetSurface(c.X, c.Y, c.ZGrid)
                    Case PlotKind.Scatter
                        canvas.Scene.SetScatter(c.X, c.Y, c.Z)
                    Case PlotKind.Line
                        canvas.Scene.SetLine(c.X, c.Y, c.Z)
                End Select
            Next
            canvas.Visible = True
            If pic2D IsNot Nothing Then pic2D.Visible = False
            canvas.Invalidate()
            UpdateStatus("已渲染 " & cmds3D.Count & " 个三维绘图指令")
        Else
            Dim w = Math.Max(10, canvas.Width)
            Dim h = Math.Max(10, canvas.Height)
            Dim bmp = DataPlotView.Render(result.Commands, w, h)
            If pic2D.Image IsNot Nothing Then pic2D.Image.Dispose()
            pic2D.Image = bmp
            pic2D.Visible = True
            canvas.Visible = False
            UpdateStatus("已渲染 " & result.Commands.Count & " 个二维绘图指令")
        End If
    End Sub

End Class

