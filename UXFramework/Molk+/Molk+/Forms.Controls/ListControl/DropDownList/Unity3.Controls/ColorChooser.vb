#Region "Microsoft.VisualBasic::b2511b7c7dc51df916b20a9f84f98209, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorChooser.vb"

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
