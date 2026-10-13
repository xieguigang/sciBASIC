<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormPlayer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            If disposing AndAlso renderBuffer IsNot Nothing Then
                renderBuffer.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified by using the Windows Form Designer.
    'Do not modify it by using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormPlayer))

        Me.toolStrip = New System.Windows.Forms.ToolStrip()
        Me.btnOpen = New System.Windows.Forms.ToolStripButton()
        Me.btnReload = New System.Windows.Forms.ToolStripButton()
        Me.toolSep1 = New System.Windows.Forms.ToolStripSeparator()
        Me.btnPause = New System.Windows.Forms.ToolStripButton()
        Me.btnWireframe = New System.Windows.Forms.ToolStripButton()
        Me.btnClear = New System.Windows.Forms.ToolStripButton()
        Me.btnResetCamera = New System.Windows.Forms.ToolStripButton()
        Me.toolSep2 = New System.Windows.Forms.ToolStripSeparator()
        Me.lblScript = New System.Windows.Forms.ToolStripLabel()
        Me.lblStatus = New System.Windows.Forms.ToolStripLabel()
        Me.split = New System.Windows.Forms.SplitContainer()
        Me.viewport = New System.Windows.Forms.PictureBox()
        Me.consolePanel = New System.Windows.Forms.Panel()
        Me.consoleBox = New System.Windows.Forms.RichTextBox()
        Me.statusStrip = New System.Windows.Forms.StatusStrip()
        Me.statusText = New System.Windows.Forms.ToolStripStatusLabel()
        Me.timer = New System.Windows.Forms.Timer(Me.components)

        Me.toolStrip.SuspendLayout()
        Me.split.Panel1.SuspendLayout()
        Me.split.Panel2.SuspendLayout()
        Me.split.SuspendLayout()
        Me.consolePanel.SuspendLayout()
        Me.statusStrip.SuspendLayout()
        Me.SuspendLayout()

        '
        ' toolStrip
        '
        Me.toolStrip.BackColor = System.Drawing.Color.FromArgb(17, 28, 46)
        Me.toolStrip.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.toolStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.toolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {
            Me.btnOpen, Me.btnReload, Me.toolSep1,
            Me.btnPause, Me.btnWireframe, Me.btnClear, Me.btnResetCamera,
            Me.toolSep2, Me.lblScript, Me.lblStatus})
        Me.toolStrip.Location = New System.Drawing.Point(0, 0)
        Me.toolStrip.Name = "toolStrip"
        Me.toolStrip.Padding = New System.Windows.Forms.Padding(8, 4, 8, 4)
        Me.toolStrip.Size = New System.Drawing.Size(1180, 32)
        Me.toolStrip.TabIndex = 0
        Me.toolStrip.Text = "工具栏"

        '
        ' buttons
        '
        Me.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnOpen.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(56, 25)
        Me.btnOpen.Text = "打开脚本"
        Me.btnOpen.ToolTipText = "打开一个 JavaScript 场景脚本"

        Me.btnReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnReload.ForeColor = System.Drawing.Color.FromArgb(34, 211, 238)
        Me.btnReload.Name = "btnReload"
        Me.btnReload.Size = New System.Drawing.Size(44, 25)
        Me.btnReload.Text = "重载"

        Me.toolSep1.Name = "toolSep1"
        Me.toolSep1.Size = New System.Drawing.Size(6, 25)

        Me.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnPause.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.btnPause.Name = "btnPause"
        Me.btnPause.Size = New System.Drawing.Size(44, 25)
        Me.btnPause.Text = "暂停"

        Me.btnWireframe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnWireframe.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.btnWireframe.Name = "btnWireframe"
        Me.btnWireframe.Size = New System.Drawing.Size(44, 25)
        Me.btnWireframe.Text = "线框"

        Me.btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnClear.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(56, 25)
        Me.btnClear.Text = "清空物体"

        Me.btnResetCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.btnResetCamera.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240)
        Me.btnResetCamera.Name = "btnResetCamera"
        Me.btnResetCamera.Size = New System.Drawing.Size(56, 25)
        Me.btnResetCamera.Text = "重置视角"

        Me.toolSep2.Name = "toolSep2"
        Me.toolSep2.Size = New System.Drawing.Size(6, 25)

        Me.lblScript.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184)
        Me.lblScript.Name = "lblScript"
        Me.lblScript.Size = New System.Drawing.Size(80, 25)
        Me.lblScript.Text = "脚本: --"

        Me.lblStatus.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
        Me.lblStatus.ForeColor = System.Drawing.Color.FromArgb(52, 211, 153)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(56, 25)
        Me.lblStatus.Text = "运行中"

        '
        ' split
        '
        Me.split.Dock = System.Windows.Forms.DockStyle.Fill
        Me.split.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.split.Location = New System.Drawing.Point(0, 32)
        Me.split.Name = "split"
        Me.split.Panel1.Controls.Add(Me.viewport)
        Me.split.Panel2.Controls.Add(Me.consolePanel)
        Me.split.Size = New System.Drawing.Size(1180, 528)
        Me.split.SplitterDistance = 860
        Me.split.SplitterWidth = 6
        Me.split.TabIndex = 1

        '
        ' viewport
        '
        Me.viewport.BackColor = System.Drawing.Color.FromArgb(11, 18, 32)
        Me.viewport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.viewport.Location = New System.Drawing.Point(0, 0)
        Me.viewport.Name = "viewport"
        Me.viewport.Size = New System.Drawing.Size(860, 528)
        Me.viewport.TabIndex = 0
        Me.viewport.TabStop = False

        '
        ' consolePanel
        '
        Me.consolePanel.BackColor = System.Drawing.Color.FromArgb(11, 18, 32)
        Me.consolePanel.Controls.Add(Me.consoleBox)
        Me.consolePanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.consolePanel.Location = New System.Drawing.Point(0, 0)
        Me.consolePanel.Name = "consolePanel"
        Me.consolePanel.Size = New System.Drawing.Size(314, 528)
        Me.consolePanel.TabIndex = 0

        Me.consoleBox.BackColor = System.Drawing.Color.FromArgb(11, 18, 32)
        Me.consoleBox.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.consoleBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.consoleBox.Font = New System.Drawing.Font("Consolas", 9.0F)
        Me.consoleBox.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184)
        Me.consoleBox.Location = New System.Drawing.Point(0, 0)
        Me.consoleBox.Name = "consoleBox"
        Me.consoleBox.ReadOnly = True
        Me.consoleBox.Size = New System.Drawing.Size(314, 528)
        Me.consoleBox.TabIndex = 0
        Me.consoleBox.Text = ""

        '
        ' statusStrip
        '
        Me.statusStrip.BackColor = System.Drawing.Color.FromArgb(17, 28, 46)
        Me.statusStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.statusText})
        Me.statusStrip.Location = New System.Drawing.Point(0, 560)
        Me.statusStrip.Name = "statusStrip"
        Me.statusStrip.Size = New System.Drawing.Size(1180, 24)
        Me.statusStrip.TabIndex = 2
        Me.statusStrip.Text = "状态栏"

        Me.statusText.ForeColor = System.Drawing.Color.FromArgb(148, 163, 184)
        Me.statusText.Name = "statusText"
        Me.statusText.Size = New System.Drawing.Size(200, 18)
        Me.statusText.Text = "就绪"

        '
        ' timer
        '
        Me.timer.Interval = 16

        '
        ' FormPlayer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 15.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1180, 584)
        Me.Controls.Add(Me.split)
        Me.Controls.Add(Me.toolStrip)
        Me.Controls.Add(Me.statusStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(720, 420)
        Me.Name = "FormPlayer"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "World Player · JavaScript 3D 物理世界"

        Me.toolStrip.ResumeLayout(False)
        Me.toolStrip.PerformLayout()
        Me.split.Panel1.ResumeLayout(False)
        Me.split.Panel2.ResumeLayout(False)
        Me.split.ResumeLayout(False)
        Me.consolePanel.ResumeLayout(False)
        Me.statusStrip.ResumeLayout(False)
        Me.statusStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents toolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents btnOpen As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnReload As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolSep1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents btnPause As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnWireframe As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnClear As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnResetCamera As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolSep2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents lblScript As System.Windows.Forms.ToolStripLabel
    Friend WithEvents lblStatus As System.Windows.Forms.ToolStripLabel
    Friend WithEvents split As System.Windows.Forms.SplitContainer
    Friend WithEvents viewport As System.Windows.Forms.PictureBox
    Friend WithEvents consolePanel As System.Windows.Forms.Panel
    Friend WithEvents consoleBox As System.Windows.Forms.RichTextBox
    Friend WithEvents statusStrip As System.Windows.Forms.StatusStrip
    Friend WithEvents statusText As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents timer As System.Windows.Forms.Timer
End Class
