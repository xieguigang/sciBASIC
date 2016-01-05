Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Text
Imports System.Windows.Forms

Namespace Unity3.Controls
	Public Partial Class ColorChooserControl
		Inherits UserControl
		Private curControl As UserControl = Nothing

		Private _color As Color
		Public Property Color() As Color
			Get
				Return DirectCast(curControl, IColorPicker).Color
			End Get
			Set
				DirectCast(curControl, IColorPicker).Color = value
			End Set
		End Property

		Public Sub New()
				'custom picker
			showControl(0)
		End Sub

		Public Sub New(color As Color)
			InitializeComponent()
			_color = color
				'custom picker
			showControl(0)
		End Sub

		Private Sub btnShowColorPicker_Click(sender As Object, e As EventArgs)
			If btnShowColorPicker.Text = "Color Picker" Then
				showControl(0)
			End If
		End Sub

		Private Sub showControl(index As Byte)
			If curControl IsNot Nothing Then
				_color = DirectCast(curControl, IColorPicker).Color
				Me.Controls.Remove(curControl)
				curControl.Dispose()
				curControl = Nothing
			End If
			Select Case index
				Case 0
					'custom picker
					curControl = New CustomColorPicker(_color)
					Exit Select
			End Select
			If curControl Is Nothing Then
				Throw New ArgumentException("The specified color picker could not be loaded!")
			End If

			curControl.Bounds = New Rectangle(0, 0, 350, 270)
			Me.Controls.Add(curControl)
		End Sub
	End Class
End Namespace
