Partial Class Form1
	''' <summary>
	''' Required designer variable.
	''' </summary>
	Private components As System.ComponentModel.IContainer = Nothing

	''' <summary>
	''' Clean up any resources being used.
	''' </summary>
	''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	Protected Overrides Sub Dispose(disposing As Boolean)
		If disposing AndAlso (components IsNot Nothing) Then
			components.Dispose()
		End If
		MyBase.Dispose(disposing)
	End Sub

	#Region "Windows Form Designer generated code"

	''' <summary>
	''' Required method for Designer support - do not modify
	''' the contents of this method with the code editor.
	''' </summary>
	Private Sub InitializeComponent()
        Me.pnlRight = New System.Windows.Forms.Panel()
        Me.propertyGrid1 = New System.Windows.Forms.PropertyGrid()
        Me.label6 = New System.Windows.Forms.Label()
        Me.panel1 = New System.Windows.Forms.Panel()
        Me.cmbTextMode = New System.Windows.Forms.ComboBox()
        Me.label12 = New System.Windows.Forms.Label()
        Me.trkFocusedSurfaceBrightness = New System.Windows.Forms.TrackBar()
        Me.label10 = New System.Windows.Forms.Label()
        Me.trkSurfaceBrightness = New System.Windows.Forms.TrackBar()
        Me.label11 = New System.Windows.Forms.Label()
        Me.trkFocusedEdgeBrightness = New System.Windows.Forms.TrackBar()
        Me.label8 = New System.Windows.Forms.Label()
        Me.trkEdgeBrightness = New System.Windows.Forms.TrackBar()
        Me.label9 = New System.Windows.Forms.Label()
        Me.trkFocusedSurfaceTransparency = New System.Windows.Forms.TrackBar()
        Me.label7 = New System.Windows.Forms.Label()
        Me.trkSurfaceTransparency = New System.Windows.Forms.TrackBar()
        Me.label5 = New System.Windows.Forms.Label()
        Me.chkShowToolTips = New System.Windows.Forms.CheckBox()
        Me.chkShowEdges = New System.Windows.Forms.CheckBox()
        Me.trkRadius = New System.Windows.Forms.TrackBar()
        Me.label4 = New System.Windows.Forms.Label()
        Me.chkAutoSizeRadius = New System.Windows.Forms.CheckBox()
        Me.trkThickness = New System.Windows.Forms.TrackBar()
        Me.label3 = New System.Windows.Forms.Label()
        Me.trkIncline = New System.Windows.Forms.TrackBar()
        Me.label2 = New System.Windows.Forms.Label()
        Me.trkRotation = New System.Windows.Forms.TrackBar()
        Me.label1 = New System.Windows.Forms.Label()
        Me.mnuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFileSaveAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuFilePrint = New System.Windows.Forms.ToolStripMenuItem()
        Me.toolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuFileExit = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.PieChart1 = New Global.System.Windows.Forms.Nexus.PieChart()
        Me.pnlRight.SuspendLayout()
        Me.panel1.SuspendLayout()
        CType(Me.trkFocusedSurfaceBrightness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkSurfaceBrightness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkFocusedEdgeBrightness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkEdgeBrightness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkFocusedSurfaceTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkSurfaceTransparency, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkRadius, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkThickness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkIncline, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trkRotation, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.menuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlRight
        '
        Me.pnlRight.Controls.Add(Me.propertyGrid1)
        Me.pnlRight.Controls.Add(Me.label6)
        Me.pnlRight.Controls.Add(Me.panel1)
        Me.pnlRight.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlRight.Location = New System.Drawing.Point(415, 24)
        Me.pnlRight.Name = "pnlRight"
        Me.pnlRight.Size = New System.Drawing.Size(237, 562)
        Me.pnlRight.TabIndex = 1
        '
        'propertyGrid1
        '
        Me.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.propertyGrid1.HelpVisible = False
        Me.propertyGrid1.Location = New System.Drawing.Point(0, 460)
        Me.propertyGrid1.Name = "propertyGrid1"
        Me.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Alphabetical
        Me.propertyGrid1.Size = New System.Drawing.Size(237, 102)
        Me.propertyGrid1.TabIndex = 15
        Me.propertyGrid1.ToolbarVisible = False
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Dock = System.Windows.Forms.DockStyle.Top
        Me.label6.Location = New System.Drawing.Point(0, 448)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(65, 12)
        Me.label6.TabIndex = 14
        Me.label6.Text = "Edit items"
        '
        'panel1
        '
        Me.panel1.Controls.Add(Me.cmbTextMode)
        Me.panel1.Controls.Add(Me.label12)
        Me.panel1.Controls.Add(Me.trkFocusedSurfaceBrightness)
        Me.panel1.Controls.Add(Me.label10)
        Me.panel1.Controls.Add(Me.trkSurfaceBrightness)
        Me.panel1.Controls.Add(Me.label11)
        Me.panel1.Controls.Add(Me.trkFocusedEdgeBrightness)
        Me.panel1.Controls.Add(Me.label8)
        Me.panel1.Controls.Add(Me.trkEdgeBrightness)
        Me.panel1.Controls.Add(Me.label9)
        Me.panel1.Controls.Add(Me.trkFocusedSurfaceTransparency)
        Me.panel1.Controls.Add(Me.label7)
        Me.panel1.Controls.Add(Me.trkSurfaceTransparency)
        Me.panel1.Controls.Add(Me.label5)
        Me.panel1.Controls.Add(Me.chkShowToolTips)
        Me.panel1.Controls.Add(Me.chkShowEdges)
        Me.panel1.Controls.Add(Me.trkRadius)
        Me.panel1.Controls.Add(Me.label4)
        Me.panel1.Controls.Add(Me.chkAutoSizeRadius)
        Me.panel1.Controls.Add(Me.trkThickness)
        Me.panel1.Controls.Add(Me.label3)
        Me.panel1.Controls.Add(Me.trkIncline)
        Me.panel1.Controls.Add(Me.label2)
        Me.panel1.Controls.Add(Me.trkRotation)
        Me.panel1.Controls.Add(Me.label1)
        Me.panel1.Dock = System.Windows.Forms.DockStyle.Top
        Me.panel1.Location = New System.Drawing.Point(0, 0)
        Me.panel1.Name = "panel1"
        Me.panel1.Size = New System.Drawing.Size(237, 448)
        Me.panel1.TabIndex = 28
        '
        'cmbTextMode
        '
        Me.cmbTextMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbTextMode.FormattingEnabled = True
        Me.cmbTextMode.Location = New System.Drawing.Point(15, 353)
        Me.cmbTextMode.Name = "cmbTextMode"
        Me.cmbTextMode.Size = New System.Drawing.Size(208, 20)
        Me.cmbTextMode.TabIndex = 52
        '
        'label12
        '
        Me.label12.AutoSize = True
        Me.label12.Location = New System.Drawing.Point(9, 338)
        Me.label12.Name = "label12"
        Me.label12.Size = New System.Drawing.Size(59, 12)
        Me.label12.TabIndex = 51
        Me.label12.Text = "Text mode"
        '
        'trkFocusedSurfaceBrightness
        '
        Me.trkFocusedSurfaceBrightness.Location = New System.Drawing.Point(124, 305)
        Me.trkFocusedSurfaceBrightness.Maximum = 100
        Me.trkFocusedSurfaceBrightness.Minimum = -100
        Me.trkFocusedSurfaceBrightness.Name = "trkFocusedSurfaceBrightness"
        Me.trkFocusedSurfaceBrightness.Size = New System.Drawing.Size(110, 45)
        Me.trkFocusedSurfaceBrightness.TabIndex = 50
        Me.trkFocusedSurfaceBrightness.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkFocusedSurfaceBrightness.Value = 30
        '
        'label10
        '
        Me.label10.AutoSize = True
        Me.label10.Location = New System.Drawing.Point(122, 287)
        Me.label10.Name = "label10"
        Me.label10.Size = New System.Drawing.Size(59, 12)
        Me.label10.TabIndex = 49
        Me.label10.Text = "(focused)"
        '
        'trkSurfaceBrightness
        '
        Me.trkSurfaceBrightness.Location = New System.Drawing.Point(6, 305)
        Me.trkSurfaceBrightness.Maximum = 100
        Me.trkSurfaceBrightness.Minimum = -100
        Me.trkSurfaceBrightness.Name = "trkSurfaceBrightness"
        Me.trkSurfaceBrightness.Size = New System.Drawing.Size(110, 45)
        Me.trkSurfaceBrightness.TabIndex = 48
        Me.trkSurfaceBrightness.TickStyle = System.Windows.Forms.TickStyle.None
        '
        'label11
        '
        Me.label11.AutoSize = True
        Me.label11.Location = New System.Drawing.Point(4, 287)
        Me.label11.Name = "label11"
        Me.label11.Size = New System.Drawing.Size(113, 12)
        Me.label11.TabIndex = 47
        Me.label11.Text = "Surface Brightness"
        '
        'trkFocusedEdgeBrightness
        '
        Me.trkFocusedEdgeBrightness.Location = New System.Drawing.Point(124, 258)
        Me.trkFocusedEdgeBrightness.Maximum = 100
        Me.trkFocusedEdgeBrightness.Minimum = -100
        Me.trkFocusedEdgeBrightness.Name = "trkFocusedEdgeBrightness"
        Me.trkFocusedEdgeBrightness.Size = New System.Drawing.Size(110, 45)
        Me.trkFocusedEdgeBrightness.TabIndex = 46
        Me.trkFocusedEdgeBrightness.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkFocusedEdgeBrightness.Value = -30
        '
        'label8
        '
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(122, 240)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(59, 12)
        Me.label8.TabIndex = 45
        Me.label8.Text = "(focused)"
        '
        'trkEdgeBrightness
        '
        Me.trkEdgeBrightness.Location = New System.Drawing.Point(6, 258)
        Me.trkEdgeBrightness.Maximum = 100
        Me.trkEdgeBrightness.Minimum = -100
        Me.trkEdgeBrightness.Name = "trkEdgeBrightness"
        Me.trkEdgeBrightness.Size = New System.Drawing.Size(110, 45)
        Me.trkEdgeBrightness.TabIndex = 44
        Me.trkEdgeBrightness.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkEdgeBrightness.Value = -30
        '
        'label9
        '
        Me.label9.AutoSize = True
        Me.label9.Location = New System.Drawing.Point(4, 240)
        Me.label9.Name = "label9"
        Me.label9.Size = New System.Drawing.Size(95, 12)
        Me.label9.TabIndex = 43
        Me.label9.Text = "Edge Brightness"
        '
        'trkFocusedSurfaceTransparency
        '
        Me.trkFocusedSurfaceTransparency.Location = New System.Drawing.Point(123, 210)
        Me.trkFocusedSurfaceTransparency.Maximum = 100
        Me.trkFocusedSurfaceTransparency.Name = "trkFocusedSurfaceTransparency"
        Me.trkFocusedSurfaceTransparency.Size = New System.Drawing.Size(110, 45)
        Me.trkFocusedSurfaceTransparency.TabIndex = 42
        Me.trkFocusedSurfaceTransparency.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkFocusedSurfaceTransparency.Value = 100
        '
        'label7
        '
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(121, 193)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(59, 12)
        Me.label7.TabIndex = 41
        Me.label7.Text = "(focused)"
        '
        'trkSurfaceTransparency
        '
        Me.trkSurfaceTransparency.Location = New System.Drawing.Point(5, 210)
        Me.trkSurfaceTransparency.Maximum = 100
        Me.trkSurfaceTransparency.Name = "trkSurfaceTransparency"
        Me.trkSurfaceTransparency.Size = New System.Drawing.Size(110, 45)
        Me.trkSurfaceTransparency.TabIndex = 40
        Me.trkSurfaceTransparency.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkSurfaceTransparency.Value = 100
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(3, 193)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(125, 12)
        Me.label5.TabIndex = 39
        Me.label5.Text = "Surface Transparency"
        '
        'chkShowToolTips
        '
        Me.chkShowToolTips.AutoSize = True
        Me.chkShowToolTips.Location = New System.Drawing.Point(15, 400)
        Me.chkShowToolTips.Name = "chkShowToolTips"
        Me.chkShowToolTips.Size = New System.Drawing.Size(108, 16)
        Me.chkShowToolTips.TabIndex = 38
        Me.chkShowToolTips.Text = "Show tool tips"
        Me.chkShowToolTips.UseVisualStyleBackColor = True
        '
        'chkShowEdges
        '
        Me.chkShowEdges.AutoSize = True
        Me.chkShowEdges.Location = New System.Drawing.Point(15, 378)
        Me.chkShowEdges.Name = "chkShowEdges"
        Me.chkShowEdges.Size = New System.Drawing.Size(84, 16)
        Me.chkShowEdges.TabIndex = 37
        Me.chkShowEdges.Text = "Show edges"
        Me.chkShowEdges.UseVisualStyleBackColor = True
        '
        'trkRadius
        '
        Me.trkRadius.Location = New System.Drawing.Point(5, 163)
        Me.trkRadius.Maximum = 1500
        Me.trkRadius.Minimum = 10
        Me.trkRadius.Name = "trkRadius"
        Me.trkRadius.Size = New System.Drawing.Size(218, 45)
        Me.trkRadius.TabIndex = 36
        Me.trkRadius.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkRadius.Value = 10
        '
        'label4
        '
        Me.label4.AutoSize = True
        Me.label4.Location = New System.Drawing.Point(3, 146)
        Me.label4.Name = "label4"
        Me.label4.Size = New System.Drawing.Size(41, 12)
        Me.label4.TabIndex = 35
        Me.label4.Text = "Radius"
        '
        'chkAutoSizeRadius
        '
        Me.chkAutoSizeRadius.AutoSize = True
        Me.chkAutoSizeRadius.Location = New System.Drawing.Point(115, 378)
        Me.chkAutoSizeRadius.Name = "chkAutoSizeRadius"
        Me.chkAutoSizeRadius.Size = New System.Drawing.Size(120, 16)
        Me.chkAutoSizeRadius.TabIndex = 34
        Me.chkAutoSizeRadius.Text = "Auto size radius"
        Me.chkAutoSizeRadius.UseVisualStyleBackColor = True
        '
        'trkThickness
        '
        Me.trkThickness.Location = New System.Drawing.Point(6, 116)
        Me.trkThickness.Maximum = 200
        Me.trkThickness.Name = "trkThickness"
        Me.trkThickness.Size = New System.Drawing.Size(218, 45)
        Me.trkThickness.TabIndex = 33
        Me.trkThickness.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkThickness.Value = 1
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(3, 101)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(59, 12)
        Me.label3.TabIndex = 32
        Me.label3.Text = "Thickness"
        '
        'trkIncline
        '
        Me.trkIncline.Location = New System.Drawing.Point(6, 69)
        Me.trkIncline.Maximum = 89
        Me.trkIncline.Minimum = 1
        Me.trkIncline.Name = "trkIncline"
        Me.trkIncline.Size = New System.Drawing.Size(218, 45)
        Me.trkIncline.TabIndex = 31
        Me.trkIncline.TickStyle = System.Windows.Forms.TickStyle.None
        Me.trkIncline.Value = 1
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(3, 54)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(71, 12)
        Me.label2.TabIndex = 30
        Me.label2.Text = "Inclination"
        '
        'trkRotation
        '
        Me.trkRotation.Location = New System.Drawing.Point(6, 24)
        Me.trkRotation.Maximum = 360
        Me.trkRotation.Name = "trkRotation"
        Me.trkRotation.Size = New System.Drawing.Size(218, 45)
        Me.trkRotation.TabIndex = 29
        Me.trkRotation.TickStyle = System.Windows.Forms.TickStyle.None
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(3, 8)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(53, 12)
        Me.label1.TabIndex = 28
        Me.label1.Text = "Rotation"
        '
        'mnuFile
        '
        Me.mnuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFileSaveAs, Me.mnuFilePrint, Me.toolStripMenuItem1, Me.mnuFileExit})
        Me.mnuFile.Name = "mnuFile"
        Me.mnuFile.Size = New System.Drawing.Size(37, 20)
        Me.mnuFile.Text = "&File"
        '
        'mnuFileSaveAs
        '
        Me.mnuFileSaveAs.Name = "mnuFileSaveAs"
        Me.mnuFileSaveAs.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.S), System.Windows.Forms.Keys)
        Me.mnuFileSaveAs.Size = New System.Drawing.Size(166, 22)
        Me.mnuFileSaveAs.Text = "&Save As ..."
        '
        'mnuFilePrint
        '
        Me.mnuFilePrint.Name = "mnuFilePrint"
        Me.mnuFilePrint.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.P), System.Windows.Forms.Keys)
        Me.mnuFilePrint.Size = New System.Drawing.Size(166, 22)
        Me.mnuFilePrint.Text = "&Print ..."
        '
        'toolStripMenuItem1
        '
        Me.toolStripMenuItem1.Name = "toolStripMenuItem1"
        Me.toolStripMenuItem1.Size = New System.Drawing.Size(163, 6)
        '
        'mnuFileExit
        '
        Me.mnuFileExit.Name = "mnuFileExit"
        Me.mnuFileExit.Size = New System.Drawing.Size(166, 22)
        Me.mnuFileExit.Text = "E&xit"
        '
        'menuStrip1
        '
        Me.menuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFile})
        Me.menuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.menuStrip1.Name = "menuStrip1"
        Me.menuStrip1.Size = New System.Drawing.Size(652, 24)
        Me.menuStrip1.TabIndex = 2
        Me.menuStrip1.Text = "menuStrip1"
        '
        'PieChart1
        '
        Me.PieChart1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PieChart1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PieChart1.Location = New System.Drawing.Point(0, 24)
        Me.PieChart1.Name = "PieChart1"
        Me.PieChart1.Radius = 150.0!
        Me.PieChart1.Size = New System.Drawing.Size(415, 562)
        Me.PieChart1.TabIndex = 0
        Me.PieChart1.Text = "PieChart1"
        Me.PieChart1.Thickness = 50.0!
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(652, 586)
        Me.Controls.Add(Me.PieChart1)
        Me.Controls.Add(Me.pnlRight)
        Me.Controls.Add(Me.menuStrip1)
        Me.MainMenuStrip = Me.menuStrip1
        Me.Name = "Form1"
        Me.Text = "PieChartTest - [no item focused]"
        Me.pnlRight.ResumeLayout(False)
        Me.pnlRight.PerformLayout()
        Me.panel1.ResumeLayout(False)
        Me.panel1.PerformLayout()
        CType(Me.trkFocusedSurfaceBrightness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkSurfaceBrightness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkFocusedEdgeBrightness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkEdgeBrightness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkFocusedSurfaceTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkSurfaceTransparency, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkRadius, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkThickness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkIncline, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trkRotation, System.ComponentModel.ISupportInitialize).EndInit()
        Me.menuStrip1.ResumeLayout(False)
        Me.menuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private WithEvents PieChart1 As Global.System.Windows.Forms.Nexus.PieChart
    Private WithEvents pnlRight As System.Windows.Forms.Panel
    Private WithEvents propertyGrid1 As System.Windows.Forms.PropertyGrid
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents panel1 As System.Windows.Forms.Panel
    Private WithEvents cmbTextMode As System.Windows.Forms.ComboBox
    Private WithEvents label12 As System.Windows.Forms.Label
    Private WithEvents trkFocusedSurfaceBrightness As System.Windows.Forms.TrackBar
    Private WithEvents label10 As System.Windows.Forms.Label
    Private WithEvents trkSurfaceBrightness As System.Windows.Forms.TrackBar
    Private WithEvents label11 As System.Windows.Forms.Label
    Private WithEvents trkFocusedEdgeBrightness As System.Windows.Forms.TrackBar
    Private WithEvents label8 As System.Windows.Forms.Label
    Private WithEvents trkEdgeBrightness As System.Windows.Forms.TrackBar
    Private WithEvents label9 As System.Windows.Forms.Label
    Private WithEvents trkFocusedSurfaceTransparency As System.Windows.Forms.TrackBar
    Private WithEvents label7 As System.Windows.Forms.Label
    Private WithEvents trkSurfaceTransparency As System.Windows.Forms.TrackBar
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents chkShowToolTips As System.Windows.Forms.CheckBox
    Private WithEvents chkShowEdges As System.Windows.Forms.CheckBox
    Private WithEvents trkRadius As System.Windows.Forms.TrackBar
    Private WithEvents label4 As System.Windows.Forms.Label
    Private WithEvents chkAutoSizeRadius As System.Windows.Forms.CheckBox
    Private WithEvents trkThickness As System.Windows.Forms.TrackBar
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents trkIncline As System.Windows.Forms.TrackBar
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents trkRotation As System.Windows.Forms.TrackBar
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents mnuFile As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFileSaveAs As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents mnuFilePrint As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents toolStripMenuItem1 As System.Windows.Forms.ToolStripSeparator
    Private WithEvents mnuFileExit As System.Windows.Forms.ToolStripMenuItem
    Private WithEvents menuStrip1 As System.Windows.Forms.MenuStrip
End Class

