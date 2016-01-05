<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form7
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
        Me.ToastNotification1 = New Microsoft.VisualBasic.MolkPlusTheme.ToastNotification()
        Me.SuspendLayout()
        '
        'ToastNotification1
        '
        Me.ToastNotification1.BackColor = System.Drawing.Color.FromArgb(CType(CType(249, Byte), Integer), CType(CType(244, Byte), Integer), CType(CType(213, Byte), Integer))
        Me.ToastNotification1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.ToastNotification1.InvokeClose = Nothing
        Me.ToastNotification1.InvokeDisplay = Nothing
        Me.ToastNotification1.Location = New System.Drawing.Point(0, 299)
        Me.ToastNotification1.Name = "ToastNotification1"
        Me.ToastNotification1.Size = New System.Drawing.Size(803, 93)
        Me.ToastNotification1.TabIndex = 0
        '
        'Form7
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(787, 348)
        Me.Controls.Add(Me.ToastNotification1)
        Me.Name = "Form7"
        Me.Text = "Form7"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ToastNotification1 As MolkPlusTheme.ToastNotification
End Class
