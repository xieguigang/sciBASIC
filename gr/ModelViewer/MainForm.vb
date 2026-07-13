Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Window
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Math.Statistics

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
    Friend WithEvents lblAmbient As Label
    Private WithEvents lblIntensity As Label
    Private WithEvents lblElevation As Label
    Private WithEvents lblAzimuth As Label
    Friend WithEvents trkAmbient As TrackBar
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
    Dim WithEvents title As Label
    Private currentFile As String = ""

    Public Sub New()
        InitializeComponent()
        BuildUI()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        menuStrip = New MenuStrip()
        fileMenu = New ToolStripMenuItem()
        openItem = New ToolStripMenuItem()
        toolStrip = New ToolStrip()
        cboMode = New ToolStripComboBox()
        cboScheme = New ToolStripComboBox()
        chkEmbedded = New ToolStripButton()
        numPointSize = New ToolStripComboBox()
        btnReset = New ToolStripButton()
        canvas = New RenderPanel()
        statusStrip = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()
        lightPanel = New Panel()
        title = New Label()
        btnLightColor = New Button()
        lblLightColor = New Label()
        btnResetLight = New Button()
        lblAmbient = New Label()
        trkAmbient = New TrackBar()
        lblIntensity = New Label()
        trkIntensity = New TrackBar()
        menuStrip.SuspendLayout()
        toolStrip.SuspendLayout()
        statusStrip.SuspendLayout()
        lightPanel.SuspendLayout()
        CType(trkAmbient, ComponentModel.ISupportInitialize).BeginInit()
        CType(trkIntensity, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' menuStrip
        ' 
        menuStrip.Items.AddRange(New ToolStripItem() {fileMenu})
        menuStrip.Location = New Point(0, 0)
        menuStrip.Name = "menuStrip"
        menuStrip.RenderMode = ToolStripRenderMode.System
        menuStrip.Size = New Size(734, 24)
        menuStrip.TabIndex = 0
        ' 
        ' fileMenu
        ' 
        fileMenu.DropDownItems.AddRange(New ToolStripItem() {openItem})
        fileMenu.Name = "fileMenu"
        fileMenu.Size = New Size(59, 20)
        fileMenu.Text = "文件(&F)"
        ' 
        ' openItem
        ' 
        openItem.Name = "openItem"
        openItem.Size = New Size(166, 22)
        openItem.Text = "打开模型/点云..."
        ' 
        ' toolStrip
        ' 
        toolStrip.Items.AddRange(New ToolStripItem() {cboMode, cboScheme, chkEmbedded, numPointSize, btnReset})
        toolStrip.Location = New Point(0, 24)
        toolStrip.Name = "toolStrip"
        toolStrip.Size = New Size(734, 25)
        toolStrip.TabIndex = 1
        ' 
        ' cboMode
        ' 
        cboMode.DropDownStyle = ComboBoxStyle.DropDownList
        cboMode.Items.AddRange(New Object() {"表面渲染", "三角形网格", "点云 (PLY)"})
        cboMode.Name = "cboMode"
        cboMode.Size = New Size(121, 25)
        ' 
        ' cboScheme
        ' 
        cboScheme.DropDownStyle = ComboBoxStyle.DropDownList
        cboScheme.Items.AddRange(New Object() {"viridis", "magma", "inferno", "plasma", "turbo", "jet", "rainbow", "cividis", "mako", "rocket", "viridis:rocket"})
        cboScheme.Name = "cboScheme"
        cboScheme.Size = New Size(121, 25)
        ' 
        ' chkEmbedded
        ' 
        chkEmbedded.CheckOnClick = True
        chkEmbedded.Name = "chkEmbedded"
        chkEmbedded.Size = New Size(115, 22)
        chkEmbedded.Text = "使用点云自带颜色"
        ' 
        ' numPointSize
        ' 
        numPointSize.DropDownStyle = ComboBoxStyle.DropDownList
        numPointSize.Items.AddRange(New Object() {"1", "2", "3", "4", "5", "6", "8", "10", "12"})
        numPointSize.Name = "numPointSize"
        numPointSize.Size = New Size(121, 25)
        ' 
        ' btnReset
        ' 
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(63, 22)
        btnReset.Text = "重置视角"
        ' 
        ' canvas
        ' 
        canvas.BackColor = Color.White
        canvas.Dock = DockStyle.Fill
        canvas.Location = New Point(0, 49)
        canvas.Name = "canvas"
        canvas.Size = New Size(734, 590)
        canvas.TabIndex = 0
        canvas.TabStop = True
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {lblStatus})
        statusStrip.Location = New Point(0, 639)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New Size(734, 22)
        statusStrip.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(282, 17)
        lblStatus.Text = "请通过「文件 ▸ 打开」加载三维模型或 PLY 点云"
        ' 
        ' lightPanel
        ' 
        lightPanel.BackColor = SystemColors.Control
        lightPanel.BorderStyle = BorderStyle.FixedSingle
        lightPanel.Controls.Add(title)
        lightPanel.Controls.Add(btnLightColor)
        lightPanel.Controls.Add(lblLightColor)
        lightPanel.Controls.Add(btnResetLight)
        lightPanel.Controls.Add(lblAmbient)
        lightPanel.Controls.Add(trkAmbient)
        lightPanel.Controls.Add(lblIntensity)
        lightPanel.Controls.Add(trkIntensity)
        lightPanel.Dock = DockStyle.Right
        lightPanel.Location = New Point(734, 0)
        lightPanel.Name = "lightPanel"
        lightPanel.Padding = New Padding(8)
        lightPanel.Size = New Size(250, 661)
        lightPanel.TabIndex = 0
        ' 
        ' title
        ' 
        title.AutoSize = True
        title.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        title.Location = New Point(8, 8)
        title.Name = "title"
        title.Size = New Size(63, 15)
        title.TabIndex = 0
        title.Text = "光照参数"
        ' 
        ' btnLightColor
        ' 
        btnLightColor.Location = New Point(13, 561)
        btnLightColor.Name = "btnLightColor"
        btnLightColor.Size = New Size(96, 23)
        btnLightColor.TabIndex = 1
        btnLightColor.Text = "灯光颜色"
        ' 
        ' lblLightColor
        ' 
        lblLightColor.BorderStyle = BorderStyle.FixedSingle
        lblLightColor.Location = New Point(117, 561)
        lblLightColor.Name = "lblLightColor"
        lblLightColor.Size = New Size(48, 22)
        lblLightColor.TabIndex = 2
        ' 
        ' btnResetLight
        ' 
        btnResetLight.Location = New Point(13, 602)
        btnResetLight.Name = "btnResetLight"
        btnResetLight.Size = New Size(152, 23)
        btnResetLight.TabIndex = 3
        btnResetLight.Text = "重置光照"
        ' 
        ' lblAmbient
        ' 
        lblAmbient.AutoSize = True
        lblAmbient.Location = New Point(8, 36)
        lblAmbient.Name = "lblAmbient"
        lblAmbient.Size = New Size(157, 15)
        lblAmbient.TabIndex = 4
        lblAmbient.Text = "环境光强度 (Ambient): 25%"
        ' 
        ' trkAmbient
        ' 
        trkAmbient.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        trkAmbient.Location = New Point(5, 54)
        trkAmbient.Maximum = 100
        trkAmbient.Name = "trkAmbient"
        trkAmbient.Size = New Size(232, 45)
        trkAmbient.TabIndex = 5
        trkAmbient.Value = 25
        ' 
        ' lblIntensity
        ' 
        lblIntensity.AutoSize = True
        lblIntensity.Location = New Point(13, 263)
        lblIntensity.Name = "lblIntensity"
        lblIntensity.Size = New Size(150, 15)
        lblIntensity.TabIndex = 6
        lblIntensity.Text = "光照亮度 (消除发白): 65%"
        ' 
        ' trkIntensity
        ' 
        trkIntensity.Location = New Point(1, 293)
        trkIntensity.Maximum = 100
        trkIntensity.Name = "trkIntensity"
        trkIntensity.Size = New Size(248, 45)
        trkIntensity.TabIndex = 7
        trkIntensity.Value = 65
        ' 
        ' MainForm
        ' 
        ClientSize = New Size(984, 661)
        Controls.Add(canvas)
        Controls.Add(statusStrip)
        Controls.Add(toolStrip)
        Controls.Add(menuStrip)
        Controls.Add(lightPanel)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = menuStrip
        MinimumSize = New Size(400, 300)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "ModelViewer - 三维模型与点云查看器"
        menuStrip.ResumeLayout(False)
        menuStrip.PerformLayout()
        toolStrip.ResumeLayout(False)
        toolStrip.PerformLayout()
        statusStrip.ResumeLayout(False)
        statusStrip.PerformLayout()
        lightPanel.ResumeLayout(False)
        lightPanel.PerformLayout()
        CType(trkAmbient, ComponentModel.ISupportInitialize).EndInit()
        CType(trkIntensity, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Private Sub BuildUI()















        ' ---- 光照参数面板 ----
        BuildLightingPanel()




    End Sub

    ' ===================== 光照参数面板 =====================

    Private Sub BuildLightingPanel()




        Dim top = 36

        top += 50

        top += 50
        trkElevation = MakeSlider(lightPanel, lblElevation, "光源仰角: 45°", -90, 90, 45, top, AddressOf LightingScroll)
        top += 50
        trkAzimuth = MakeSlider(lightPanel, lblAzimuth, "光源方位: -30°", -360, 360, -30, top, AddressOf LightingScroll)
        top += 56









        top += 32


        ' 应用默认光照（避免开箱即纯白）
        ResetLighting()
    End Sub

    Private Function MakeSlider(lightPanel As Control, ByRef caption As Label, title As String, min As Integer, max As Integer, val As Integer, top As Integer, handler As EventHandler) As TrackBar
        caption = New Label()
        caption.Text = title
        caption.AutoSize = True
        caption.Top = top
        caption.Left = 8
        lightPanel.Controls.Add(caption)

        Dim tb = New TrackBar()
        tb.Minimum = min
        tb.Maximum = max
        tb.Value = val
        tb.TickStyle = TickStyle.BottomRight
        tb.Left = 8
        tb.Top = top + 16
        tb.Width = lightPanel.ClientSize.Width - 16
        AddHandler tb.Scroll, handler
        lightPanel.Controls.Add(tb)
        Return tb
    End Function

    Private Sub LightingScroll(sender As Object, e As EventArgs) Handles trkAmbient.Scroll, trkIntensity.Scroll
        ApplyLighting()
    End Sub

    Private Sub LightColorClick(sender As Object, e As EventArgs) Handles btnLightColor.Click
        Using dlg As New ColorDialog()
            dlg.Color = baseLightColor
            If dlg.ShowDialog() = DialogResult.OK Then
                baseLightColor = dlg.Color
                lblLightColor.BackColor = baseLightColor
                ApplyLighting()
            End If
        End Using
    End Sub

    Private Sub ResetLightClick(sender As Object, e As EventArgs) Handles btnResetLight.Click
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

    Private Sub ModeChanged(sender As Object, e As EventArgs) Handles cboMode.SelectedIndexChanged
        Select Case cboMode.SelectedIndex
            Case 0 : renderer.Mode = RenderMode.Surface
            Case 1 : renderer.Mode = RenderMode.Mesh
            Case 2 : renderer.Mode = RenderMode.PointCloud
        End Select
        UpdateStatus()
        canvas.Invalidate()
    End Sub

    Private Sub SchemeChanged(sender As Object, e As EventArgs) Handles cboScheme.SelectedIndexChanged
        renderer.ColorScheme = CStr(cboScheme.Text)
        canvas.Invalidate()
    End Sub

    Private Sub EmbeddedChanged(sender As Object, e As EventArgs) Handles chkEmbedded.CheckedChanged
        renderer.UseEmbeddedColor = chkEmbedded.Checked
        canvas.Invalidate()
    End Sub

    Private Sub PointSizeChanged(sender As Object, e As EventArgs) Handles numPointSize.SelectedIndexChanged
        renderer.PointSize = Integer.Parse(CStr(numPointSize.Text))
        canvas.Invalidate()
    End Sub

    Private Sub ResetViewClick(sender As Object, e As EventArgs) Handles btnReset.Click
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

    Private Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles canvas.MouseUp
        dragging = False
        panning = False
    End Sub

    Private Sub OnCanvasZoom(delta As Integer) Handles canvas.Zoom
        ' 滚轮：调整摄像机视距（缩放）。向上滚近、向下滚远。
        Dim factor = If(delta > 0, 0.9F, 1.1F)
        Dim vd = renderer.Camera.ViewDistance * factor
        renderer.Camera.ViewDistance = Math.Max(1.0F, vd)
        UpdateStatus()
        canvas.Invalidate()
    End Sub

    ' ===================== 绘制 =====================

    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs) Handles canvas.Paint
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

        If lblStatus IsNot Nothing Then
            lblStatus.Text =
                $"文件: {IO.Path.GetFileName(currentFile)}  |  " &
                $"模式: {modeText}  |  " &
                $"面: {renderer.SurfaceCount}  点: {renderer.PointCount}  |  " &
                $"角度X: {renderer.Camera.AngleX:F1}  Y: {renderer.Camera.AngleY:F1}  |  " &
                $"视距: {renderer.Camera.ViewDistance:F1}  |  " &
                $"环境光: {renderer.Camera.AmbientStrength:P0}  亮度: {lightIntensity:P0}"
        End If
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' ---- 打开对话框 ----
        openFileDialog = New OpenFileDialog()
        openFileDialog.Filter =
            "3D 模型 (*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf)|*.stl;*.obj;*.gltf;*.glb;*.dae;*.3ds;*.3mf|" &
            "PLY 点云 (*.ply)|*.ply|所有文件 (*.*)|*.*"
        openFileDialog.Title = "打开三维模型或点云文件"
    End Sub
End Class


