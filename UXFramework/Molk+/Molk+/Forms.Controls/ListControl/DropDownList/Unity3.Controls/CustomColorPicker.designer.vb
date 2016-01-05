Namespace Unity3.Controls
	Partial Class CustomColorPicker
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

		#Region "Component Designer generated code"

		''' <summary> 
		''' Required method for Designer support - do not modify 
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Dim hsl3 As New Unity3.Controls.ColorManager.HSL()
			Dim hsl4 As New Unity3.Controls.ColorManager.HSL()
			Me.colorBox = New Unity3.Controls.ColorBox()
			Me.lblOriginalColor = New System.Windows.Forms.Label()
			Me.colorSlider = New Unity3.Controls.VerticalColorSlider()
			Me.rbBlue = New System.Windows.Forms.RadioButton()
			Me.rbGreen = New System.Windows.Forms.RadioButton()
			Me.rbRed = New System.Windows.Forms.RadioButton()
			Me.rbBrightness = New System.Windows.Forms.RadioButton()
			Me.rbSat = New System.Windows.Forms.RadioButton()
			Me.rbHue = New System.Windows.Forms.RadioButton()
			Me.txtBlue = New System.Windows.Forms.TextBox()
			Me.txtGreen = New System.Windows.Forms.TextBox()
			Me.txtRed = New System.Windows.Forms.TextBox()
			Me.txtBrightness = New System.Windows.Forms.TextBox()
			Me.txtSat = New System.Windows.Forms.TextBox()
			Me.txtHue = New System.Windows.Forms.TextBox()
			Me.tbAlpha = New System.Windows.Forms.TrackBar()
			Me.lblAlpha = New System.Windows.Forms.Label()
			Me.colorPanelPending = New Unity3.Controls.ColorPanel()
			DirectCast(Me.tbAlpha, System.ComponentModel.ISupportInitialize).BeginInit()
			Me.SuspendLayout()
			' 
			' colorBox
			' 
			Me.colorBox.DrawStyle = Unity3.Controls.ColorBox.eDrawStyle.Hue
			hsl3.H = 0
			hsl3.L = 1
			hsl3.S = 1
			Me.colorBox.HSL = hsl3
			Me.colorBox.Location = New System.Drawing.Point(0, 0)
			Me.colorBox.Name = "colorBox"
			Me.colorBox.RGB = System.Drawing.Color.FromArgb(CInt(CByte(255)), CInt(CByte(0)), CInt(CByte(0)))
			Me.colorBox.Size = New System.Drawing.Size(212, 181)
			Me.colorBox.TabIndex = 0
			AddHandler Me.colorBox.Scroll, New System.EventHandler(AddressOf Me.colorBox_Scroll)
			' 
			' lblOriginalColor
			' 
			Me.lblOriginalColor.BackColor = System.Drawing.SystemColors.Control
			Me.lblOriginalColor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
			Me.lblOriginalColor.Location = New System.Drawing.Point(274, 82)
			Me.lblOriginalColor.Name = "lblOriginalColor"
			Me.lblOriginalColor.Size = New System.Drawing.Size(60, 34)
			Me.lblOriginalColor.TabIndex = 39
			' 
			' colorSlider
			' 
			Me.colorSlider.DrawStyle = Unity3.Controls.VerticalColorSlider.eDrawStyle.Hue
			hsl4.H = 0
			hsl4.L = 1
			hsl4.S = 1
			Me.colorSlider.HSL = hsl4
			Me.colorSlider.Location = New System.Drawing.Point(218, 0)
			Me.colorSlider.Name = "colorSlider"
			Me.colorSlider.RGB = System.Drawing.Color.FromArgb(CInt(CByte(255)), CInt(CByte(0)), CInt(CByte(0)))
			Me.colorSlider.Size = New System.Drawing.Size(40, 181)
			Me.colorSlider.TabIndex = 40
			AddHandler Me.colorSlider.Scroll, New System.EventHandler(AddressOf Me.colorSlider_Scroll)
			' 
			' rbBlue
			' 
			Me.rbBlue.Location = New System.Drawing.Point(108, 239)
			Me.rbBlue.Name = "rbBlue"
			Me.rbBlue.Size = New System.Drawing.Size(35, 24)
			Me.rbBlue.TabIndex = 53
			Me.rbBlue.Text = "B:"
			AddHandler Me.rbBlue.CheckedChanged, New System.EventHandler(AddressOf Me.rbBlue_CheckedChanged)
			' 
			' rbGreen
			' 
			Me.rbGreen.Location = New System.Drawing.Point(108, 214)
			Me.rbGreen.Name = "rbGreen"
			Me.rbGreen.Size = New System.Drawing.Size(35, 24)
			Me.rbGreen.TabIndex = 52
			Me.rbGreen.Text = "G:"
			AddHandler Me.rbGreen.CheckedChanged, New System.EventHandler(AddressOf Me.rbGreen_CheckedChanged)
			' 
			' rbRed
			' 
			Me.rbRed.Location = New System.Drawing.Point(108, 189)
			Me.rbRed.Name = "rbRed"
			Me.rbRed.Size = New System.Drawing.Size(35, 24)
			Me.rbRed.TabIndex = 51
			Me.rbRed.Text = "R:"
			AddHandler Me.rbRed.CheckedChanged, New System.EventHandler(AddressOf Me.rbRed_CheckedChanged)
			' 
			' rbBrightness
			' 
			Me.rbBrightness.Location = New System.Drawing.Point(12, 239)
			Me.rbBrightness.Name = "rbBrightness"
			Me.rbBrightness.Size = New System.Drawing.Size(35, 24)
			Me.rbBrightness.TabIndex = 50
			Me.rbBrightness.Text = "B:"
			AddHandler Me.rbBrightness.CheckedChanged, New System.EventHandler(AddressOf Me.rbBrightness_CheckedChanged)
			' 
			' rbSat
			' 
			Me.rbSat.Location = New System.Drawing.Point(12, 214)
			Me.rbSat.Name = "rbSat"
			Me.rbSat.Size = New System.Drawing.Size(35, 24)
			Me.rbSat.TabIndex = 49
			Me.rbSat.Text = "S:"
			AddHandler Me.rbSat.CheckedChanged, New System.EventHandler(AddressOf Me.rbSat_CheckedChanged)
			' 
			' rbHue
			' 
			Me.rbHue.Location = New System.Drawing.Point(12, 189)
			Me.rbHue.Name = "rbHue"
			Me.rbHue.Size = New System.Drawing.Size(35, 24)
			Me.rbHue.TabIndex = 48
			Me.rbHue.Text = "H:"
			AddHandler Me.rbHue.CheckedChanged, New System.EventHandler(AddressOf Me.rbHue_CheckedChanged)
			' 
			' txtBlue
			' 
			Me.txtBlue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtBlue.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtBlue.Location = New System.Drawing.Point(145, 239)
			Me.txtBlue.Name = "txtBlue"
			Me.txtBlue.Size = New System.Drawing.Size(35, 21)
			Me.txtBlue.TabIndex = 46
			AddHandler Me.txtBlue.Leave, New System.EventHandler(AddressOf Me.txtBlue_Leave)
			' 
			' txtGreen
			' 
			Me.txtGreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtGreen.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtGreen.Location = New System.Drawing.Point(145, 214)
			Me.txtGreen.Name = "txtGreen"
			Me.txtGreen.Size = New System.Drawing.Size(35, 21)
			Me.txtGreen.TabIndex = 45
			AddHandler Me.txtGreen.Leave, New System.EventHandler(AddressOf Me.txtGreen_Leave)
			' 
			' txtRed
			' 
			Me.txtRed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtRed.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtRed.Location = New System.Drawing.Point(145, 189)
			Me.txtRed.Name = "txtRed"
			Me.txtRed.Size = New System.Drawing.Size(35, 21)
			Me.txtRed.TabIndex = 44
			AddHandler Me.txtRed.Leave, New System.EventHandler(AddressOf Me.txtRed_Leave)
			' 
			' txtBrightness
			' 
			Me.txtBrightness.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtBrightness.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtBrightness.Location = New System.Drawing.Point(49, 239)
			Me.txtBrightness.Name = "txtBrightness"
			Me.txtBrightness.Size = New System.Drawing.Size(35, 21)
			Me.txtBrightness.TabIndex = 43
			AddHandler Me.txtBrightness.Leave, New System.EventHandler(AddressOf Me.txtBrightness_Leave)
			' 
			' txtSat
			' 
			Me.txtSat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtSat.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtSat.Location = New System.Drawing.Point(49, 214)
			Me.txtSat.Name = "txtSat"
			Me.txtSat.Size = New System.Drawing.Size(35, 21)
			Me.txtSat.TabIndex = 42
			AddHandler Me.txtSat.Leave, New System.EventHandler(AddressOf Me.txtSat_Leave)
			' 
			' txtHue
			' 
			Me.txtHue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
			Me.txtHue.Font = New System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
			Me.txtHue.Location = New System.Drawing.Point(49, 189)
			Me.txtHue.Name = "txtHue"
			Me.txtHue.Size = New System.Drawing.Size(35, 21)
			Me.txtHue.TabIndex = 41
			AddHandler Me.txtHue.Leave, New System.EventHandler(AddressOf Me.txtHue_Leave)
			' 
			' tbAlpha
			' 
			Me.tbAlpha.Location = New System.Drawing.Point(198, 200)
			Me.tbAlpha.Maximum = 255
			Me.tbAlpha.Name = "tbAlpha"
			Me.tbAlpha.Size = New System.Drawing.Size(136, 45)
			Me.tbAlpha.TabIndex = 54
			Me.tbAlpha.TickFrequency = 20
			Me.tbAlpha.Value = 255
			AddHandler Me.tbAlpha.ValueChanged, New System.EventHandler(AddressOf Me.tbAlpha_ValueChanged)
			' 
			' lblAlpha
			' 
			Me.lblAlpha.AutoSize = True
			Me.lblAlpha.Location = New System.Drawing.Point(229, 232)
			Me.lblAlpha.Name = "lblAlpha"
			Me.lblAlpha.Size = New System.Drawing.Size(72, 13)
			Me.lblAlpha.TabIndex = 55
			Me.lblAlpha.Text = "Transparency"
			' 
			' colorPanelPending
			' 
			Me.colorPanelPending.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
			Me.colorPanelPending.Color = System.Drawing.Color.FromArgb(CInt(CByte(255)), CInt(CByte(192)), CInt(CByte(192)))
			Me.colorPanelPending.Location = New System.Drawing.Point(274, 48)
			Me.colorPanelPending.Name = "colorPanelPending"
			Me.colorPanelPending.PaintColor = True
			Me.colorPanelPending.Size = New System.Drawing.Size(60, 34)
			Me.colorPanelPending.TabIndex = 56
			Me.colorPanelPending.Text = "colorPanel1"
			' 
			' CustomColorPicker
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6F, 13F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.Controls.Add(Me.colorPanelPending)
			Me.Controls.Add(Me.lblAlpha)
			Me.Controls.Add(Me.tbAlpha)
			Me.Controls.Add(Me.rbBlue)
			Me.Controls.Add(Me.rbGreen)
			Me.Controls.Add(Me.rbRed)
			Me.Controls.Add(Me.rbBrightness)
			Me.Controls.Add(Me.rbSat)
			Me.Controls.Add(Me.rbHue)
			Me.Controls.Add(Me.txtBlue)
			Me.Controls.Add(Me.txtGreen)
			Me.Controls.Add(Me.txtRed)
			Me.Controls.Add(Me.txtBrightness)
			Me.Controls.Add(Me.txtSat)
			Me.Controls.Add(Me.txtHue)
			Me.Controls.Add(Me.colorSlider)
			Me.Controls.Add(Me.lblOriginalColor)
			Me.Controls.Add(Me.colorBox)
			Me.Name = "CustomColorPicker"
			Me.Size = New System.Drawing.Size(350, 270)
			DirectCast(Me.tbAlpha, System.ComponentModel.ISupportInitialize).EndInit()
			Me.ResumeLayout(False)
			Me.PerformLayout()

		End Sub

		#End Region

		Private colorBox As ColorBox
		Private lblOriginalColor As System.Windows.Forms.Label
		Private colorSlider As VerticalColorSlider
		Private rbBlue As System.Windows.Forms.RadioButton
		Private rbGreen As System.Windows.Forms.RadioButton
		Private rbRed As System.Windows.Forms.RadioButton
		Private rbBrightness As System.Windows.Forms.RadioButton
		Private rbSat As System.Windows.Forms.RadioButton
		Private rbHue As System.Windows.Forms.RadioButton
		Private txtBlue As System.Windows.Forms.TextBox
		Private txtGreen As System.Windows.Forms.TextBox
		Private txtRed As System.Windows.Forms.TextBox
		Private txtBrightness As System.Windows.Forms.TextBox
		Private txtSat As System.Windows.Forms.TextBox
		Private txtHue As System.Windows.Forms.TextBox
		Private tbAlpha As System.Windows.Forms.TrackBar
		Private lblAlpha As System.Windows.Forms.Label
		Private colorPanelPending As ColorPanel
	End Class
End Namespace
