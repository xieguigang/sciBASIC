<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormCanvas
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
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
        Me.SaveAsSVGToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RefreshParametersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AutoRotateToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ShowLabelsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
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
        Me.MenuStrip1.Size = New System.Drawing.Size(688, 24)
        Me.MenuStrip1.TabIndex = 0
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveAsSVGToolStripMenuItem, Me.RefreshParametersToolStripMenuItem, Me.DToolStripMenuItem, Me.AutoRotateToolStripMenuItem, Me.ShowLabelsToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'SaveAsSVGToolStripMenuItem
        '
        Me.SaveAsSVGToolStripMenuItem.Name = "SaveAsSVGToolStripMenuItem"
        Me.SaveAsSVGToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.SaveAsSVGToolStripMenuItem.Text = "Save As SVG"
        '
        'RefreshParametersToolStripMenuItem
        '
        Me.RefreshParametersToolStripMenuItem.Name = "RefreshParametersToolStripMenuItem"
        Me.RefreshParametersToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.RefreshParametersToolStripMenuItem.Text = "Refresh Parameters"
        '
        'DToolStripMenuItem
        '
        Me.DToolStripMenuItem.CheckOnClick = True
        Me.DToolStripMenuItem.Name = "DToolStripMenuItem"
        Me.DToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.DToolStripMenuItem.Text = "3D"
        '
        'AutoRotateToolStripMenuItem
        '
        Me.AutoRotateToolStripMenuItem.Checked = True
        Me.AutoRotateToolStripMenuItem.CheckOnClick = True
        Me.AutoRotateToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.AutoRotateToolStripMenuItem.Name = "AutoRotateToolStripMenuItem"
        Me.AutoRotateToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.AutoRotateToolStripMenuItem.Text = "Auto Rotate"
        '
        'ShowLabelsToolStripMenuItem
        '
        Me.ShowLabelsToolStripMenuItem.Checked = True
        Me.ShowLabelsToolStripMenuItem.CheckOnClick = True
        Me.ShowLabelsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ShowLabelsToolStripMenuItem.Name = "ShowLabelsToolStripMenuItem"
        Me.ShowLabelsToolStripMenuItem.Size = New System.Drawing.Size(175, 22)
        Me.ShowLabelsToolStripMenuItem.Text = "Show Labels"
        '
        'TrackBar1
        '
        Me.TrackBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TrackBar1.Location = New System.Drawing.Point(537, 422)
        Me.TrackBar1.Maximum = 0
        Me.TrackBar1.Minimum = -60
        Me.TrackBar1.Name = "TrackBar1"
        Me.TrackBar1.Size = New System.Drawing.Size(139, 45)
        Me.TrackBar1.TabIndex = 1
        '
        'FormCanvas
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(688, 479)
        Me.Controls.Add(Me.TrackBar1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "FormCanvas"
        Me.Text = "Network Canvas"
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents MenuStrip1 As Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents SaveAsSVGToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents RefreshParametersToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents DToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents TrackBar1 As Windows.Forms.TrackBar
    Friend WithEvents AutoRotateToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShowLabelsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
