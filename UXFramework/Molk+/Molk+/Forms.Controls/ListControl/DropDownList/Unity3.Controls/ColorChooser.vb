Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms

Namespace Unity3.Controls
	Public Partial Class ColorChooser
		Inherits Form
		Public ReadOnly Property Color() As Color
			Get
				Return colorPicker1.Color
			End Get
		End Property

		Public Sub New(color As Color)
			InitializeComponent()
			colorPicker1.Color = color
		End Sub

		Private Sub btnOK_Click(sender As Object, e As EventArgs)
			Me.Close()
		End Sub

		Private Sub btnCancel_Click(sender As Object, e As EventArgs)
			Me.Close()
		End Sub
	End Class
End Namespace
