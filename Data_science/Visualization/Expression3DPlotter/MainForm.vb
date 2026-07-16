Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Driver

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
    Private WithEvents chkAxes As CheckBox
    Private WithEvents btnDraw As Button
    Private WithEvents btnReset As Button

    Public Sub New()
        InitializeComponent()
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
End Class
