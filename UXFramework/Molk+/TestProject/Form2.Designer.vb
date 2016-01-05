<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.Button1 = New System.Windows.Forms.Button()
        Me.PreviewHandle1 = New Microsoft.VisualBasic.MolkPlusTheme.PreviewHandle()
        Me.Ping1 = New Microsoft.VisualBasic.MolkPlusTheme.Ping()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(58, 66)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 25)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'PreviewHandle1
        '
        Me.PreviewHandle1.BackColor = System.Drawing.Color.White
        Me.PreviewHandle1.BackgroundImage = CType(resources.GetObject("PreviewHandle1.BackgroundImage"), System.Drawing.Image)
        Me.PreviewHandle1.HighlightColor = System.Drawing.Color.AliceBlue
        Me.PreviewHandle1.Location = New System.Drawing.Point(295, 66)
        Me.PreviewHandle1.Name = "PreviewHandle1"
        Me.PreviewHandle1.Size = New System.Drawing.Size(257, 247)
        Me.PreviewHandle1.TabIndex = 1
        Me.PreviewHandle1.Value = "179"
        '
        'Ping1
        '
        Me.Ping1.BackgroundImage = CType(resources.GetObject("Ping1.BackgroundImage"), System.Drawing.Image)
        Me.Ping1.Interval = 3000
        Me.Ping1.IPAddress = "fe80::ad35:dac2:a25d:6d9%3"
        Me.Ping1.Location = New System.Drawing.Point(212, 319)
        Me.Ping1.Name = "Ping1"
        Me.Ping1.Size = New System.Drawing.Size(74, 20)
        Me.Ping1.TabIndex = 2
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(709, 485)
        Me.Controls.Add(Me.Ping1)
        Me.Controls.Add(Me.PreviewHandle1)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form2"
        Me.Text = "Form2"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents PreviewHandle1 As Microsoft.VisualBasic.MolkPlusTheme.PreviewHandle
    Friend WithEvents Ping1 As Microsoft.VisualBasic.MolkPlusTheme.Ping
End Class
