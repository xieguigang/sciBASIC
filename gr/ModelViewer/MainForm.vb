Imports Microsoft.VisualBasic.Imaging.Drawing3D

Public Class MainForm : Inherits Form

    Private WithEvents renderer As New SceneRenderer()
    Private WithEvents canvas As RenderPanel

    Private WithEvents menuStrip As MenuStrip
    Private WithEvents toolStrip As ToolStrip
    Private WithEvents cboMode As ToolStripComboBox
    Private WithEvents cboScheme As ToolStripComboBox
    Private WithEvents chkEmbedded As ToolStripButton
    Private WithEvents numPointSize As ToolStripComboBox
    Private WithEvents btnReset As ToolStripButton

    Private WithEvents statusStrip As StatusStrip
    Private WithEvents lblStatus As ToolStripStatusLabel

    Private WithEvents openFileDialog As OpenFileDialog

    Dim WithEvents fileMenu As New ToolStripMenuItem
    Dim WithEvents openItem As ToolStripMenuItem

    ' ---- 光照调节状态 ----
    Private WithEvents lightPanel As Panel
    Private WithEvents lblAmbient As Label
    Private WithEvents lblIntensity As Label
    Private WithEvents lblElevation As Label
    Private WithEvents lblAzimuth As Label
    Private WithEvents trkAmbient As TrackBar
    Private WithEvents trkIntensity As TrackBar
    Private WithEvents trkElevation As TrackBar
    Private WithEvents trkAzimuth As TrackBar
    Private WithEvents btnLightColor As Button
    Private WithEvents lblLightColor As Label
    Private WithEvents btnResetLight As Button

    Private baseLightColor As Color = Color.White
    Private lightIntensity As Double = 0.65

    Private dragging As Boolean = False
    Private panning As Boolean = False
    Private lastX As Integer = 0
    Private lastY As Integer = 0
    Private currentFile As String = ""

    Public Sub New()
        InitializeComponent()
        BuildUI()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        SuspendLayout()
        ' 
        ' MainForm
        ' 
        ClientSize = New Size(931, 611)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "MainForm"
        Me.Text = "ModelViewer - 三维模型与点云查看器"
        Me.Size = New Size(1000, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.MinimumSize = New Size(400, 300)


        ' ---- 菜单 ----
        menuStrip = New MenuStrip()
        fileMenu = New ToolStripMenuItem("文件(&F)")
        openItem = New ToolStripMenuItem("打开模型/点云...")
        fileMenu.DropDownItems.Add(openItem)
        menuStrip.Items.Add(fileMenu)
        Me.MainMenuStrip = menuStrip

        Me.Controls.Add(menuStrip)

        ResumeLayout(False)

    End Sub

    Private Sub BuildUI()




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

        ' ---- 光照参数面板 ----
        BuildLightingPanel()
        Me.Controls.Add(lightPanel)

        ' ---- 布局 ----
        Me.Controls.Add(canvas)
        Me.Controls.Add(toolStrip)
        Me.Controls.Add(statusStrip)


        ' ---- 打开对话框 ----
        openFileDialog = New OpenFileDialog()
        openFileDialog.Filter =
            "3D 模型 (*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf)|*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf|" &
            "PLY 点云 (*.ply)|*.ply|所有文件 (*.*)|*.*"
        openFileDialog.Title = "打开三维模型或点云文件"
    End Sub

    ' ===================== 光照参数面板 =====================

    Private Sub BuildLightingPanel()
        lightPanel = New Panel()
        lightPanel.Dock = DockStyle.Right
        lightPanel.Width = 250
        lightPanel.BackColor = SystemColors.Control
        lightPanel.BorderStyle = BorderStyle.FixedSingle
        lightPanel.Padding = New Padding(8)

        Dim title = New Label()
        title.Text = "光照参数"
        title.Font = New Font(title.Font, FontStyle.Bold)
        title.AutoSize = True
        title.Top = 8
        title.Left = 8
        lightPanel.Controls.Add(title)

        Dim top = 36
        trkAmbient = MakeSlider(lightPanel, lblAmbient, "环境光强度 (Ambient): 25%", 0, 100, 25, top, AddressOf LightingScroll)
        top += 50
        trkIntensity = MakeSlider(lightPanel, lblIntensity, "光照亮度 (消除发白): 65%", 0, 100, 65, top, AddressOf LightingScroll)
        top += 50
        trkElevation = MakeSlider(lightPanel, lblElevation, "光源仰角: 45°", -90, 90, 45, top, AddressOf LightingScroll)
        top += 50
        trkAzimuth = MakeSlider(lightPanel, lblAzimuth, "光源方位: -30°", -360, 360, -30, top, AddressOf LightingScroll)
        top += 56

        btnLightColor = New Button()
        btnLightColor.Text = "灯光颜色"
        btnLightColor.Left = 8
        btnLightColor.Top = top
        btnLightColor.Width = 96
        AddHandler btnLightColor.Click, AddressOf LightColorClick
        lightPanel.Controls.Add(btnLightColor)

        lblLightColor = New Label()
        lblLightColor.Text = ""
        lblLightColor.BorderStyle = BorderStyle.FixedSingle
        lblLightColor.BackColor = baseLightColor
        lblLightColor.Left = 112
        lblLightColor.Top = top + 1
        lblLightColor.Width = 48
        lblLightColor.Height = 22
        lightPanel.Controls.Add(lblLightColor)

        top += 32
        btnResetLight = New Button()
        btnResetLight.Text = "重置光照"
        btnResetLight.Left = 8
        btnResetLight.Top = top
        btnResetLight.Width = 152
        AddHandler btnResetLight.Click, AddressOf ResetLightClick
        lightPanel.Controls.Add(btnResetLight)

        ' 应用默认光照（避免开箱即纯白）
        ResetLighting()
    End Sub

    Private Function MakeSlider(parent As Control, ByRef caption As Label, title As String, min As Integer, max As Integer, val As Integer, top As Integer, handler As EventHandler) As TrackBar
        caption = New Label()
        caption.Text = title
        caption.AutoSize = True
        caption.Top = top
        caption.Left = 8
        parent.Controls.Add(caption)

        Dim tb = New TrackBar()
        tb.Minimum = min
        tb.Maximum = max
        tb.Value = val
        tb.TickStyle = TickStyle.BottomRight
        tb.Left = 8
        tb.Top = top + 16
        tb.Width = parent.ClientSize.Width - 16
        AddHandler tb.Scroll, handler
        parent.Controls.Add(tb)
        Return tb
    End Function

    Private Sub LightingScroll(sender As Object, e As EventArgs)
        ApplyLighting()
    End Sub

    Private Sub LightColorClick(sender As Object, e As EventArgs)
        Using dlg As New ColorDialog()
            dlg.Color = baseLightColor
            If dlg.ShowDialog() = DialogResult.OK Then
                baseLightColor = dlg.Color
                lblLightColor.BackColor = baseLightColor
                ApplyLighting()
            End If
        End Using
    End Sub

    Private Sub ResetLightClick(sender As Object, e As EventArgs)
        ResetLighting()
    End Sub

    Private Sub ResetLighting()
        trkAmbient.Value = 25
        trkIntensity.Value = 65
        trkElevation.Value = 45
        trkAzimuth.Value = -30
        baseLightColor = Color.White
        lblLightColor.BackColor = baseLightColor
        ApplyLighting()
    End Sub

    ''' <summary>
    ''' 将当前 UI 控件的值写入 camera 的光照属性。
    ''' </summary>
    Private Sub ApplyLighting()
        lightIntensity = trkIntensity.Value / 100.0
        renderer.Camera.AmbientStrength = trkAmbient.Value / 100.0
        renderer.Camera.LightColor = ScaleLightColor(baseLightColor, lightIntensity)
        renderer.Camera.LightDirection = LightDirFromAngles(trkElevation.Value, trkAzimuth.Value)

        lblAmbient.Text = $"环境光强度 (Ambient): {trkAmbient.Value}%"
        lblIntensity.Text = $"光照亮度 (消除发白): {trkIntensity.Value}%"
        lblElevation.Text = $"光源仰角: {trkElevation.Value}°"
        lblAzimuth.Text = $"光源方位: {trkAzimuth.Value}°"

        UpdateStatus()
        canvas.Invalidate()
    End Sub

    ''' <summary>
    ''' 按亮度系数缩放灯光颜色（降低亮度可削弱被照亮面的发白）。
    ''' </summary>
    Private Function ScaleLightColor(base As Color, k As Double) As Color
        Dim r = CInt(base.R * k)
        Dim g = CInt(base.G * k)
        Dim b = CInt(base.B * k)
        r = System.Math.Max(0, System.Math.Min(255, r))
        g = System.Math.Max(0, System.Math.Min(255, g))
        b = System.Math.Max(0, System.Math.Min(255, b))
        Return Color.FromArgb(base.A, r, g, b)
    End Function

    ''' <summary>
    ''' 由仰角/方位角（度）构造指向光源的单位向量。
    ''' </summary>
    Private Function LightDirFromAngles(elevDeg As Integer, azimDeg As Integer) As Point3D
        Dim el = elevDeg * System.Math.PI / 180.0
        Dim az = azimDeg * System.Math.PI / 180.0
        Dim x = System.Math.Cos(el) * System.Math.Cos(az)
        Dim y = System.Math.Cos(el) * System.Math.Sin(az)
        Dim z = System.Math.Sin(el)
        Return New Point3D(x, y, z).Normalize()
    End Function

    ' ===================== 文件加载 =====================

    Private Sub OpenClick(sender As Object, e As EventArgs) Handles openItem.Click
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
        ResetLighting()
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
            $"视距: {renderer.Camera.ViewDistance:F1}  |  " &
            $"环境光: {renderer.Camera.AmbientStrength:P0}  亮度: {lightIntensity:P0}"
    End Sub
End Class


