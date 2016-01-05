<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormBusyLoader
    Inherits Microsoft.VisualBasic.Forms.MetroUI.Form

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormBusyLoader))
        Me.AjaxLoaderSquaresCircles1 = New Microsoft.VisualBasic.Forms.MetroUI.AjaxLoaderSquaresCircles()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'AjaxLoaderSquaresCircles1
        '
        Me.AjaxLoaderSquaresCircles1.BackColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(229, Byte), Integer), CType(CType(248, Byte), Integer))
        Me.AjaxLoaderSquaresCircles1.BackgroundImage = CType(resources.GetObject("AjaxLoaderSquaresCircles1.BackgroundImage"), System.Drawing.Image)
        Me.AjaxLoaderSquaresCircles1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom
        Me.AjaxLoaderSquaresCircles1.Enabled = False
        Me.AjaxLoaderSquaresCircles1.Location = New System.Drawing.Point(21, 26)
        Me.AjaxLoaderSquaresCircles1.Name = "AjaxLoaderSquaresCircles1"
        Me.AjaxLoaderSquaresCircles1.Size = New System.Drawing.Size(34, 32)
        Me.AjaxLoaderSquaresCircles1.TabIndex = 0
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.FromArgb(CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Button1.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button1.Font = New System.Drawing.Font("Segoe UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.ForeColor = System.Drawing.Color.White
        Me.Button1.Location = New System.Drawing.Point(213, 129)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 29)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Dismiss"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label1.Location = New System.Drawing.Point(71, 26)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(217, 92)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Label1"
        '
        'FormBusyLoader
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(189, Byte), Integer), CType(CType(229, Byte), Integer), CType(CType(248, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(300, 170)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.AjaxLoaderSquaresCircles1)
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(300, 170)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(300, 170)
        Me.Name = "FormBusyLoader"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = ""
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents AjaxLoaderSquaresCircles1 As Microsoft.VisualBasic.Forms.MetroUI.AjaxLoaderSquaresCircles
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
