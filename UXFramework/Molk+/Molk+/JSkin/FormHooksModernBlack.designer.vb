Partial Class FormHooksModernBlack
    ''' <summary> 
    ''' 
    ''' </summary>
    Private components As System.ComponentModel.IContainer = Nothing

   
    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

#Region ""


    Private Sub InitializeComponent()
        Dim resources As New System.ComponentModel.ComponentResourceManager(GetType(FormHooksModernBlack))
        Me.pButtons = New System.Windows.Forms.PictureBox()
        Me.pLayout = New System.Windows.Forms.PictureBox()
        DirectCast(Me.pButtons, System.ComponentModel.ISupportInitialize).BeginInit()
        DirectCast(Me.pLayout, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        ' 
        ' pButtons
        ' 
        Me.pButtons.Image = DirectCast(resources.GetObject("pButtons.Image"), System.Drawing.Image)
        Me.pButtons.Location = New System.Drawing.Point(82, 0)
        Me.pButtons.Name = "pButtons"
        Me.pButtons.Size = New System.Drawing.Size(105, 105)
        Me.pButtons.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.pButtons.TabIndex = 1
        Me.pButtons.TabStop = False
        Me.pButtons.Visible = False
        ' 
        ' pLayout
        ' 
        Me.pLayout.Image = DirectCast(resources.GetObject("pLayout.Image"), System.Drawing.Image)
        Me.pLayout.Location = New System.Drawing.Point(0, 0)
        Me.pLayout.Name = "pLayout"
        Me.pLayout.Size = New System.Drawing.Size(64, 64)
        Me.pLayout.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.pLayout.TabIndex = 0
        Me.pLayout.TabStop = False
        Me.pLayout.Visible = False
        ' 
        ' ctlModernBlack
        ' 
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 12.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pButtons)
        Me.Controls.Add(Me.pLayout)
        Me.DoubleBuffered = True
        Me.Name = "ctlModernBlack"
        Me.Size = New System.Drawing.Size(220, 151)
        AddHandler Me.Load, New System.EventHandler(AddressOf Me.ctlModernBlack_Load)
        AddHandler Me.Paint, New System.Windows.Forms.PaintEventHandler(AddressOf Me.ctlSkin_Paint)
        AddHandler Me.MouseDoubleClick, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctlSkin_MouseDoubleClick)
        AddHandler Me.MouseDown, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctlSkin_MouseDown)
        AddHandler Me.MouseLeave, New System.EventHandler(AddressOf Me.ctlSkin_MouseLeave)
        AddHandler Me.MouseMove, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctlSkin_MouseMove)
        AddHandler Me.MouseUp, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctlSkin_MouseUp)
        AddHandler Me.Resize, New System.EventHandler(AddressOf Me.ctlSkin_Resize)
        DirectCast(Me.pButtons, System.ComponentModel.ISupportInitialize).EndInit()
        DirectCast(Me.pLayout, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private pLayout As System.Windows.Forms.PictureBox
    Private pButtons As System.Windows.Forms.PictureBox
End Class
