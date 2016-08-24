#Region "Microsoft.VisualBasic::cfca4f819ea07b74a83d1b67a96bf77f, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorPanel.vb"

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
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel

Namespace Unity3.Controls
	<DefaultEvent("Click")> _
	Public Class ColorPanel
		Inherits Label
		Private _Color As Color
		Public Property Color() As Color
			Get
				Return _Color
			End Get
			Set
				_Color = value
				Me.Invalidate()
			End Set
		End Property

		Private _PaintColor As Boolean = True
		Public Property PaintColor() As Boolean
			Get
				Return _PaintColor
			End Get
			Set
				_PaintColor = value
				Me.Invalidate()
			End Set
		End Property

		Public Overrides Property AutoSize() As Boolean
			Get
				Return False
			End Get
			Set
			End Set
		End Property

		Public Sub New()
			Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
			Me.BorderStyle = BorderStyle.FixedSingle
		End Sub

		Protected Overrides Sub OnPaint(e As PaintEventArgs)
			If Not _PaintColor OrElse _Color.IsEmpty Then
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
				e.Graphics.Clear(Me.BackColor)
				e.Graphics.DrawLine(Pens.Black, 0, 0, Me.ClientSize.Width, Me.ClientSize.Height)
				e.Graphics.DrawLine(Pens.Black, Me.ClientSize.Width, 0, 0, Me.ClientSize.Height)
				Return
			End If

			If _Color.A <> 255 Then
				Dim b As Boolean = False
				Dim r As New Rectangle(0, 0, 8, 8)
				e.Graphics.Clear(Color.White)
				r.Y = 0
				While r.Y < Me.Height
					r.X = (If((InlineAssignHelper(b, Not b)), 0, 8))
					While r.X < Me.Width
						e.Graphics.FillRectangle(Brushes.LightGray, r)
						r.X += 16
					End While
					r.Y += 8
				End While
			End If

			Using br As New SolidBrush(_Color)
				e.Graphics.FillRectangle(br, Me.ClientRectangle)
			End Using
		End Sub
		Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
			target = value
			Return value
		End Function

	End Class
End Namespace
