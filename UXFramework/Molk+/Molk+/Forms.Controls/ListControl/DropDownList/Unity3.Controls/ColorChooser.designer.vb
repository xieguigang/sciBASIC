Namespace Unity3.Controls
	Partial Class ColorChooser
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

		#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Me.btnOK = New System.Windows.Forms.Button()
			Me.btnCancel = New System.Windows.Forms.Button()
			Me.colorPicker1 = New Unity3.Controls.ColorChooserControl()
			Me.SuspendLayout()
			' 
			' btnOK
			' 
			Me.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
			Me.btnOK.Location = New System.Drawing.Point(168, 305)
			Me.btnOK.Name = "btnOK"
			Me.btnOK.Size = New System.Drawing.Size(75, 23)
			Me.btnOK.TabIndex = 1
			Me.btnOK.Text = "OK"
			Me.btnOK.UseVisualStyleBackColor = True
			AddHandler Me.btnOK.Click, New System.EventHandler(AddressOf Me.btnOK_Click)
			' 
			' btnCancel
			' 
			Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
			Me.btnCancel.Location = New System.Drawing.Point(265, 305)
			Me.btnCancel.Name = "btnCancel"
			Me.btnCancel.Size = New System.Drawing.Size(75, 23)
			Me.btnCancel.TabIndex = 2
			Me.btnCancel.Text = "Cancel"
			Me.btnCancel.UseVisualStyleBackColor = True
			AddHandler Me.btnCancel.Click, New System.EventHandler(AddressOf Me.btnCancel_Click)
			' 
			' colorPicker1
			' 
			Me.colorPicker1.Location = New System.Drawing.Point(0, -1)
			Me.colorPicker1.Name = "colorPicker1"
			Me.colorPicker1.Size = New System.Drawing.Size(350, 300)
			Me.colorPicker1.TabIndex = 0
			' 
			' ColorChooser
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6F, 13F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(352, 337)
			Me.Controls.Add(Me.btnCancel)
			Me.Controls.Add(Me.btnOK)
			Me.Controls.Add(Me.colorPicker1)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.MaximizeBox = False
			Me.MinimizeBox = False
			Me.Name = "ColorChooser"
			Me.Text = "ColorChooser"
			Me.ResumeLayout(False)

		End Sub

		#End Region

		Private colorPicker1 As ColorChooserControl
		Private btnOK As System.Windows.Forms.Button
		Private btnCancel As System.Windows.Forms.Button
	End Class
End Namespace
