#Region "Microsoft.VisualBasic::3100d246a5018154f2260360999b7582, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorChooserControl.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
