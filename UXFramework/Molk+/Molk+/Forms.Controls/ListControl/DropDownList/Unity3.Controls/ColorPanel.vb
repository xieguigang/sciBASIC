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
