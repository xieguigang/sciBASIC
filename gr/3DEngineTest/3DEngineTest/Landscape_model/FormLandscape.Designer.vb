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
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LoadModelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AutoRotateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TrackBar1 = New System.Windows.Forms.TrackBar()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LoadModelToolStripMenuItem, Me.AutoRotateToolStripMenuItem, Me.ResetToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'LoadModelToolStripMenuItem
        '
        Me.LoadModelToolStripMenuItem.Name = "LoadModelToolStripMenuItem"
        Me.LoadModelToolStripMenuItem.Size = New System.Drawing.Size(137, 22)
        Me.LoadModelToolStripMenuItem.Text = "Load Model"
        '
        'AutoRotateToolStripMenuItem
        '
        Me.AutoRotateToolStripMenuItem.Checked = True
        Me.AutoRotateToolStripMenuItem.CheckOnClick = True
        Me.AutoRotateToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AutoRotateToolStripMenuItem.Name = "AutoRotateToolStripMenuItem"
        Me.AutoRotateToolStripMenuItem.Size = New System.Drawing.Size(137, 22)
        Me.AutoRotateToolStripMenuItem.Text = "Auto Rotate"
        '
        'ResetToolStripMenuItem
        '
        Me.ResetToolStripMenuItem.Name = "ResetToolStripMenuItem"
        Me.ResetToolStripMenuItem.Size = New System.Drawing.Size(137, 22)
        Me.ResetToolStripMenuItem.Text = "Reset"
        '
        'TrackBar1
        '
        Me.TrackBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TrackBar1.LargeChange = 10
        Me.TrackBar1.Location = New System.Drawing.Point(490, 436)
        Me.TrackBar1.Maximum = -5
        Me.TrackBar1.Minimum = -500
        Me.TrackBar1.Name = "TrackBar1"
        Me.TrackBar1.Size = New System.Drawing.Size(230, 45)
        Me.TrackBar1.SmallChange = 5
        Me.TrackBar1.TabIndex = 1
        Me.TrackBar1.Value = -5
        '
        'FormLandscape
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(742, 493)
        Me.Controls.Add(Me.TrackBar1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FormLandscape"
        Me.Text = "VB.NET landscape model demo"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LoadModelToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AutoRotateToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ResetToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TrackBar1 As TrackBar
End Class
