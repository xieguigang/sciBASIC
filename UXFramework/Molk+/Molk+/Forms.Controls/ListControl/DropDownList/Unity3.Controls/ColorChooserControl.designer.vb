Namespace Unity3.Controls
	Partial Class ColorChooserControl
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
			Me.btnShowColorPicker = New System.Windows.Forms.Button()
			Me.SuspendLayout()
			' 
			' btnShowColorPicker
			' 
			Me.btnShowColorPicker.Location = New System.Drawing.Point(244, 274)
			Me.btnShowColorPicker.Name = "btnShowColorPicker"
			Me.btnShowColorPicker.Size = New System.Drawing.Size(106, 23)
			Me.btnShowColorPicker.TabIndex = 0
			Me.btnShowColorPicker.Text = "Custom Color"
			Me.btnShowColorPicker.UseVisualStyleBackColor = True
			AddHandler Me.btnShowColorPicker.Click, New System.EventHandler(AddressOf Me.btnShowColorPicker_Click)
			' 
			' ColorPicker
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6F, 13F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.Controls.Add(Me.btnShowColorPicker)
			Me.Name = "ColorPicker"
			Me.ResumeLayout(False)

		End Sub

		#End Region

		Private btnShowColorPicker As System.Windows.Forms.Button
	End Class
End Namespace
