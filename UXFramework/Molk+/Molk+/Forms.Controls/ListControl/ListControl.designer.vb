<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ListControl
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose( disposing As Boolean)
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
        Me.flpListBox = New System.Windows.Forms.FlowLayoutPanel()
        Me.SuspendLayout()
        '
        'flpListBox
        '
        Me.flpListBox.AutoScroll = True
        Me.flpListBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.flpListBox.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flpListBox.Location = New System.Drawing.Point(0, 0)
        Me.flpListBox.Margin = New System.Windows.Forms.Padding(0)
        Me.flpListBox.Name = "flpListBox"
        Me.flpListBox.Size = New System.Drawing.Size(150, 150)
        Me.flpListBox.TabIndex = 0
        Me.flpListBox.WrapContents = False
        '
        'ListControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.Controls.Add(Me.flpListBox)
        Me.Margin = New System.Windows.Forms.Padding(0)
        Me.Name = "ListControl"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents flpListBox As System.Windows.Forms.FlowLayoutPanel

End Class
