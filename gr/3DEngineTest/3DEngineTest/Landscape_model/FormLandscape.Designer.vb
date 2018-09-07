<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormLandscape
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormLandscape))
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadModelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Load3mfToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripSeparator()
        Me.IsometricComplexExampleToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.IsometricKnotToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.IsometricGridToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AutoRotateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RotateXToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RotateYToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RotateZToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ResetToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.LightToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SetLightColorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetToolStripMenuItem2 = New System.Windows.Forms.ToolStripMenuItem()
        Me.RemoveTexturesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SetBackgroundColorToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripSeparator()
        Me.ResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TrackBar1 = New System.Windows.Forms.TrackBar()
        Me.trbFOV = New System.Windows.Forms.TrackBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.IsometricPieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trbFOV, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(742, 24)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LoadModelToolStripMenuItem, Me.AutoRotateToolStripMenuItem, Me.LightToolStripMenuItem, Me.RemoveTexturesToolStripMenuItem, Me.SetBackgroundColorToolStripMenuItem, Me.ToolStripMenuItem3, Me.ResetToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'LoadModelToolStripMenuItem
        '
        Me.LoadModelToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Load3mfToolStripMenuItem, Me.ToolStripMenuItem2, Me.IsometricComplexExampleToolStripMenuItem, Me.IsometricKnotToolStripMenuItem, Me.IsometricGridToolStripMenuItem, Me.IsometricPieToolStripMenuItem})
        Me.LoadModelToolStripMenuItem.Name = "LoadModelToolStripMenuItem"
        Me.LoadModelToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.LoadModelToolStripMenuItem.Text = "Load Model"
        '
        'Load3mfToolStripMenuItem
        '
        Me.Load3mfToolStripMenuItem.Name = "Load3mfToolStripMenuItem"
        Me.Load3mfToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
        Me.Load3mfToolStripMenuItem.Text = "Load *.3mf"
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        Me.ToolStripMenuItem2.Size = New System.Drawing.Size(223, 6)
        '
        'IsometricComplexExampleToolStripMenuItem
        '
        Me.IsometricComplexExampleToolStripMenuItem.Name = "IsometricComplexExampleToolStripMenuItem"
        Me.IsometricComplexExampleToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
        Me.IsometricComplexExampleToolStripMenuItem.Text = "Isometric - complex Example"
        '
        'IsometricKnotToolStripMenuItem
        '
        Me.IsometricKnotToolStripMenuItem.Name = "IsometricKnotToolStripMenuItem"
        Me.IsometricKnotToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
        Me.IsometricKnotToolStripMenuItem.Text = "Isometric - knot"
        '
        'IsometricGridToolStripMenuItem
        '
        Me.IsometricGridToolStripMenuItem.Name = "IsometricGridToolStripMenuItem"
        Me.IsometricGridToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
        Me.IsometricGridToolStripMenuItem.Text = "Isometric - Grid"
        '
        'AutoRotateToolStripMenuItem
        '
        Me.AutoRotateToolStripMenuItem.Checked = True
        Me.AutoRotateToolStripMenuItem.CheckOnClick = True
        Me.AutoRotateToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AutoRotateToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RotateXToolStripMenuItem, Me.RotateYToolStripMenuItem, Me.RotateZToolStripMenuItem, Me.ToolStripMenuItem1, Me.ResetToolStripMenuItem1})
        Me.AutoRotateToolStripMenuItem.Name = "AutoRotateToolStripMenuItem"
        Me.AutoRotateToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.AutoRotateToolStripMenuItem.Text = "Auto Rotate"
        '
        'RotateXToolStripMenuItem
        '
        Me.RotateXToolStripMenuItem.Checked = True
        Me.RotateXToolStripMenuItem.CheckOnClick = True
        Me.RotateXToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.RotateXToolStripMenuItem.Name = "RotateXToolStripMenuItem"
        Me.RotateXToolStripMenuItem.Size = New System.Drawing.Size(115, 22)
        Me.RotateXToolStripMenuItem.Text = "rotate X"
        '
        'RotateYToolStripMenuItem
        '
        Me.RotateYToolStripMenuItem.Checked = True
        Me.RotateYToolStripMenuItem.CheckOnClick = True
        Me.RotateYToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.RotateYToolStripMenuItem.Name = "RotateYToolStripMenuItem"
        Me.RotateYToolStripMenuItem.Size = New System.Drawing.Size(115, 22)
        Me.RotateYToolStripMenuItem.Text = "rotate Y"
        '
        'RotateZToolStripMenuItem
        '
        Me.RotateZToolStripMenuItem.Checked = True
        Me.RotateZToolStripMenuItem.CheckOnClick = True
        Me.RotateZToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.RotateZToolStripMenuItem.Name = "RotateZToolStripMenuItem"
        Me.RotateZToolStripMenuItem.Size = New System.Drawing.Size(115, 22)
        Me.RotateZToolStripMenuItem.Text = "rotate Z"
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(112, 6)
        '
        'ResetToolStripMenuItem1
        '
        Me.ResetToolStripMenuItem1.Name = "ResetToolStripMenuItem1"
        Me.ResetToolStripMenuItem1.Size = New System.Drawing.Size(115, 22)
        Me.ResetToolStripMenuItem1.Text = "Reset"
        '
        'LightToolStripMenuItem
        '
        Me.LightToolStripMenuItem.Checked = True
        Me.LightToolStripMenuItem.CheckOnClick = True
        Me.LightToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.LightToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SetLightColorToolStripMenuItem, Me.ResetToolStripMenuItem2})
        Me.LightToolStripMenuItem.Name = "LightToolStripMenuItem"
        Me.LightToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.LightToolStripMenuItem.Text = "Light"
        '
        'SetLightColorToolStripMenuItem
        '
        Me.SetLightColorToolStripMenuItem.Name = "SetLightColorToolStripMenuItem"
        Me.SetLightColorToolStripMenuItem.Size = New System.Drawing.Size(149, 22)
        Me.SetLightColorToolStripMenuItem.Text = "Set LightColor"
        '
        'ResetToolStripMenuItem2
        '
        Me.ResetToolStripMenuItem2.Name = "ResetToolStripMenuItem2"
        Me.ResetToolStripMenuItem2.Size = New System.Drawing.Size(149, 22)
        Me.ResetToolStripMenuItem2.Text = "Reset"
        '
        'RemoveTexturesToolStripMenuItem
        '
        Me.RemoveTexturesToolStripMenuItem.Name = "RemoveTexturesToolStripMenuItem"
        Me.RemoveTexturesToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.RemoveTexturesToolStripMenuItem.Text = "Remove Textures"
        '
        'SetBackgroundColorToolStripMenuItem
        '
        Me.SetBackgroundColorToolStripMenuItem.Name = "SetBackgroundColorToolStripMenuItem"
        Me.SetBackgroundColorToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.SetBackgroundColorToolStripMenuItem.Text = "Set BackgroundColor"
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        Me.ToolStripMenuItem3.Size = New System.Drawing.Size(183, 6)
        '
        'ResetToolStripMenuItem
        '
        Me.ResetToolStripMenuItem.Name = "ResetToolStripMenuItem"
        Me.ResetToolStripMenuItem.Size = New System.Drawing.Size(186, 22)
        Me.ResetToolStripMenuItem.Text = "Reset"
        '
        'TrackBar1
        '
        Me.TrackBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TrackBar1.AutoSize = False
        Me.TrackBar1.BackColor = System.Drawing.Color.LightBlue
        Me.TrackBar1.LargeChange = 10
        Me.TrackBar1.Location = New System.Drawing.Point(543, 462)
        Me.TrackBar1.Maximum = 500
        Me.TrackBar1.Minimum = -500
        Me.TrackBar1.Name = "TrackBar1"
        Me.TrackBar1.Size = New System.Drawing.Size(177, 19)
        Me.TrackBar1.SmallChange = 5
        Me.TrackBar1.TabIndex = 1
        Me.TrackBar1.Value = -5
        '
        'trbFOV
        '
        Me.trbFOV.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.trbFOV.AutoSize = False
        Me.trbFOV.BackColor = System.Drawing.Color.LightBlue
        Me.trbFOV.LargeChange = 10
        Me.trbFOV.Location = New System.Drawing.Point(543, 437)
        Me.trbFOV.Maximum = 500
        Me.trbFOV.Name = "trbFOV"
        Me.trbFOV.Size = New System.Drawing.Size(177, 19)
        Me.trbFOV.SmallChange = 5
        Me.trbFOV.TabIndex = 2
        Me.trbFOV.Value = 256
        '
        'Label1
        '
        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.LightBlue
        Me.Label1.Location = New System.Drawing.Point(506, 437)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(31, 13)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "FOV:"
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.BackColor = System.Drawing.Color.LightBlue
        Me.Label2.Location = New System.Drawing.Point(459, 462)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(78, 13)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "View Distance:"
        '
        'IsometricPieToolStripMenuItem
        '
        Me.IsometricPieToolStripMenuItem.Name = "IsometricPieToolStripMenuItem"
        Me.IsometricPieToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
        Me.IsometricPieToolStripMenuItem.Text = "Isometric - Pie"
        '
        'FormLandscape
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(742, 493)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.trbFOV)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.TrackBar1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FormLandscape"
        Me.Text = "VB.NET 3D graphics engine demo"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trbFOV, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadModelToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AutoRotateToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TrackBar1 As TrackBar
    Friend WithEvents RotateXToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RotateYToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RotateZToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem1 As ToolStripSeparator
    Friend WithEvents ResetToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents LightToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents RemoveTexturesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents trbFOV As TrackBar
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents IsometricComplexExampleToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Load3mfToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As ToolStripSeparator
    Friend WithEvents IsometricKnotToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents IsometricGridToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SetLightColorToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetToolStripMenuItem2 As ToolStripMenuItem
    Friend WithEvents SetBackgroundColorToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem3 As ToolStripSeparator
    Friend WithEvents IsometricPieToolStripMenuItem As ToolStripMenuItem
End Class
