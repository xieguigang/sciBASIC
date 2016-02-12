<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ImageCorping
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
        Me.crobPictureBox = New System.Windows.Forms.PictureBox()
        CType(Me.crobPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'crobPictureBox
        '
        Me.crobPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.crobPictureBox.Location = New System.Drawing.Point(99, 97)
        Me.crobPictureBox.Name = "crobPictureBox"
        Me.crobPictureBox.Size = New System.Drawing.Size(140, 139)
        Me.crobPictureBox.TabIndex = 0
        Me.crobPictureBox.TabStop = False
        '
        'ImageCorping
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.crobPictureBox)
        Me.DoubleBuffered = True
        Me.Name = "ImageCorping"
        Me.Size = New System.Drawing.Size(726, 451)
        CType(Me.crobPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents crobPictureBox As PictureBox
End Class
