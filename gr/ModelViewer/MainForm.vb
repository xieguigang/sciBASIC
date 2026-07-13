Public Class MainForm : Inherits Form

    Private renderer As New SceneRenderer()
    Private canvas As RenderPanel

    Private menuStrip As MenuStrip
    Private toolStrip As ToolStrip
    Private cboMode As ToolStripComboBox
    Private cboScheme As ToolStripComboBox
    Private chkEmbedded As ToolStripButton
    Private numPointSize As ToolStripComboBox
    Private btnReset As ToolStripButton

    Private statusStrip As StatusStrip
    Private lblStatus As ToolStripStatusLabel

    Private openFileDialog As OpenFileDialog

    Private dragging As Boolean = False
    Private panning As Boolean = False
    Private lastX As Integer = 0
    Private lastY As Integer = 0
    Private currentFile As String = ""

    Public Sub New()
        BuildUI()
    End Sub

    Private Sub BuildUI()
        Me.Text = "ModelViewer - 三维模型与点云查看器"
        Me.Size = New Size(1000, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(400, 300)

        ' ---- 菜单 ----
        menuStrip = New MenuStrip()
        Dim fileMenu = New ToolStripMenuItem("文件(&F)")
        Dim openItem = New ToolStripMenuItem("打开模型/点云...", Nothing, AddressOf OpenClick)
        fileMenu.DropDownItems.Add(openItem)
        menuStrip.Items.Add(fileMenu)
        Me.MainMenuStrip = menuStrip

        ' ---- 工具栏 ----
        toolStrip = New ToolStrip()

        toolStrip.Items.Add(New ToolStripLabel("渲染模式:"))
        cboMode = New ToolStripComboBox()
        cboMode.Items.AddRange(New Object() {"表面渲染", "三角形网格", "点云 (PLY)"})
        cboMode.SelectedIndex = 0
        cboMode.DropDownStyle = ComboBoxStyle.DropDownList
        AddHandler cboMode.SelectedIndexChanged, AddressOf ModeChanged
        toolStrip.Items.Add(cboMode)

        toolStrip.Items.Add(New ToolStripSeparator())

        toolStrip.Items.Add(New ToolStripLabel("点云配色:"))
        cboScheme = New ToolStripComboBox()
        cboScheme.Items.AddRange(New Object() {
            "viridis", "magma", "inferno", "plasma", "turbo",
            "jet", "rainbow", "cividis", "mako", "rocket", "viridis:rocket"})
        cboScheme.SelectedIndex = 0
        cboScheme.DropDownStyle = ComboBoxStyle.DropDownList
        AddHandler cboScheme.SelectedIndexChanged, AddressOf SchemeChanged
        toolStrip.Items.Add(cboScheme)

        chkEmbedded = New ToolStripButton()
        chkEmbedded.Text = "使用点云自带颜色"
        chkEmbedded.CheckOnClick = True
        AddHandler chkEmbedded.CheckedChanged, AddressOf EmbeddedChanged
        toolStrip.Items.Add(chkEmbedded)

        toolStrip.Items.Add(New ToolStripLabel("点径:"))
        numPointSize = New ToolStripComboBox()
        numPointSize.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "8", "10", "12"})
        numPointSize.SelectedIndex = 1
        numPointSize.DropDownStyle = ComboBoxStyle.DropDownList
        numPointSize.Width = 50
        AddHandler numPointSize.SelectedIndexChanged, AddressOf PointSizeChanged
        toolStrip.Items.Add(numPointSize)

        toolStrip.Items.Add(New ToolStripSeparator())

        btnReset = New ToolStripButton("重置视角")
        AddHandler btnReset.Click, AddressOf ResetViewClick
        toolStrip.Items.Add(btnReset)

        ' ---- 画布 ----
        canvas = New RenderPanel()
        canvas.Dock = DockStyle.Fill
        canvas.BackColor = Color.White
        canvas.TabStop = True
        AddHandler canvas.Paint, AddressOf Canvas_Paint
        AddHandler canvas.MouseDown, AddressOf Canvas_MouseDown
        AddHandler canvas.MouseMove, AddressOf Canvas_MouseMove
        AddHandler canvas.MouseUp, AddressOf Canvas_MouseUp
        AddHandler canvas.Zoom, AddressOf OnCanvasZoom

        ' ---- 状态栏 ----
        statusStrip = New StatusStrip()
        lblStatus = New ToolStripStatusLabel("请通过「文件 ▸ 打开」加载三维模型或 PLY 点云")
        statusStrip.Items.Add(lblStatus)

        ' ---- 布局 ----
        Me.Controls.Add(canvas)
        Me.Controls.Add(toolStrip)
        Me.Controls.Add(statusStrip)
        Me.Controls.Add(menuStrip)

        ' ---- 打开对话框 ----
        openFileDialog = New OpenFileDialog()
        openFileDialog.Filter =
            "3D 模型 (*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf)|*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf|" &
            "PLY 点云 (*.ply)|*.ply|所有文件 (*.*)|*.*"
        openFileDialog.Title = "打开三维模型或点云文件"
    End Sub

    ' ===================== 文件加载 =====================

    Private Sub OpenClick(sender As Object, e As EventArgs)
        If openFileDialog.ShowDialog() = DialogResult.OK Then
            OpenFile(openFileDialog.FileName)
        End If
    End Sub

    Private Sub OpenFile(path As String)
        currentFile = path
        Dim ext = IO.Path.GetExtension(path).ToLowerInvariant()
        Try
            renderer.Camera.Screen = canvas.ClientSize
            If ext = ".ply" Then
                renderer.LoadPointCloud(path)
                cboMode.SelectedIndex = 2
            Else
                renderer.LoadModel(path)
                cboMode.SelectedIndex = 0
            End If
            renderer.FitView()
            UpdateStatus()
            canvas.Invalidate()
        Catch ex As Exception
            MessageBox.Show("加载失败: " & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ===================== 模式 / 配色控件 =====================

    Private Sub ModeChanged(sender As Object, e As EventArgs)
        Select Case cboMode.SelectedIndex
            Case 0 : renderer.Mode = RenderMode.Surface
            Case 1 : renderer.Mode = RenderMode.Mesh
            Case 2 : renderer.Mode = RenderMode.PointCloud
        End Select
        UpdateStatus()
        canvas.Invalidate()
    End Sub

    Private Sub SchemeChanged(sender As Object, e As EventArgs)
        renderer.ColorScheme = CStr(cboScheme.Text)
        canvas.Invalidate()
    End Sub

    Private Sub EmbeddedChanged(sender As Object, e As EventArgs)
        renderer.UseEmbeddedColor = chkEmbedded.Checked
        canvas.Invalidate()
    End Sub

    Private Sub PointSizeChanged(sender As Object, e As EventArgs)
        renderer.PointSize = Integer.Parse(CStr(numPointSize.Text))
        canvas.Invalidate()
    End Sub

    Private Sub ResetViewClick(sender As Object, e As EventArgs)
        ResetView()
    End Sub

    Private Sub ResetView()
        renderer.Camera.AngleX = 20
        renderer.Camera.AngleY = -30
        renderer.Camera.AngleZ = 0
        renderer.Camera.Offset = New PointF(0, 0)
        renderer.FitView()
        UpdateStatus()
        canvas.Invalidate()
    End Sub

    ' ===================== 鼠标交互 =====================

    Private Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs)
        canvas.Focus()
        If e.Button = MouseButtons.Left Then
            dragging = True
            lastX = e.X : lastY = e.Y
        ElseIf e.Button = MouseButtons.Right Then
            panning = True
            lastX = e.X : lastY = e.Y
        End If
    End Sub

    Private Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs)
        If dragging Then
            Dim dx = e.X - lastX
            Dim dy = e.Y - lastY
            ' 左键拖拽：旋转模型（修改摄像机角度）
            renderer.Camera.AngleY = renderer.Camera.AngleY + dx * 0.4F
            renderer.Camera.AngleX = renderer.Camera.AngleX + dy * 0.4F
            lastX = e.X : lastY = e.Y
            UpdateStatus()
            canvas.Invalidate()
        ElseIf panning Then
            Dim dx = e.X - lastX
            Dim dy = e.Y - lastY
            ' 右键拖拽：平移画布（修改摄像机偏移）
            renderer.Camera.Offset = New PointF(renderer.Camera.Offset.X + dx, renderer.Camera.Offset.Y + dy)
            lastX = e.X : lastY = e.Y
            canvas.Invalidate()
        End If
    End Sub

    Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs)
        dragging = False
        panning = False
    End Sub

    Private Sub OnCanvasZoom(delta As Integer)
        ' 滚轮：调整摄像机视距（缩放）。向上滚近、向下滚远。
        Dim factor = If(delta > 0, 0.9F, 1.1F)
        Dim vd = renderer.Camera.ViewDistance * factor
        renderer.Camera.ViewDistance = Math.Max(1.0F, vd)
        UpdateStatus()
        canvas.Invalidate()
    End Sub

    ' ===================== 绘制 =====================

    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs)
        If renderer Is Nothing Then Return
        If renderer.HasData Then
            renderer.Draw(e.Graphics, canvas.ClientSize)
        Else
            e.Graphics.Clear(Color.White)
            e.Graphics.DrawString(
                "请通过「文件 ▸ 打开」加载三维模型或 PLY 点云",
                New Font("Segoe UI", 12),
                Brushes.Gray,
                New PointF(20, 20))
        End If
    End Sub

    Private Sub UpdateStatus()
        Dim modeText = renderer.Mode.ToString()
        lblStatus.Text =
            $"文件: {IO.Path.GetFileName(currentFile)}  |  " &
            $"模式: {modeText}  |  " &
            $"面: {renderer.SurfaceCount}  点: {renderer.PointCount}  |  " &
            $"角度X: {renderer.Camera.AngleX:F1}  Y: {renderer.Camera.AngleY:F1}  |  " &
            $"视距: {renderer.Camera.ViewDistance:F1}"
    End Sub

End Class


