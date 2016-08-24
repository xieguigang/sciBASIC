#Region "Microsoft.VisualBasic::a2ee9d6d2b90861b56e80ef6d8b4b4da, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorBox.vb"

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

Imports System.Collections
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Data
Imports System.Windows.Forms

Namespace Unity3.Controls
	''' <summary>
	''' Provides a interface to edit color values
	''' </summary>
	Public Class ColorBox
		Inherits System.Windows.Forms.UserControl
		#Region "Class Variables"

		Public Enum eDrawStyle
			Hue
			Saturation
			Brightness
			Red
			Green
			Blue
		End Enum

		Private m_iMarker_X As Integer = 0
		Private m_iMarker_Y As Integer = 0
		Private m_bDragging As Boolean = False

		'	These variables keep track of how to fill in the content inside the box;
		Private m_eDrawStyle As eDrawStyle = eDrawStyle.Hue
		Private m_hsl As ColorManager.HSL
		Private m_rgb As Color

		''' <summary> 
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.Container = Nothing

		#End Region

		#Region "Constructors / Destructors"

		Public Sub New()
			' This call is required by the Windows.Forms Form Designer.
			InitializeComponent()

			'	Initialize Colors
			m_hsl = New ColorManager.HSL()
			m_hsl.H = 1.0
			m_hsl.S = 1.0
			m_hsl.L = 1.0
			m_rgb = ColorManager.HSL_to_RGB(m_hsl)
			m_eDrawStyle = eDrawStyle.Hue
		End Sub


		''' <summary> 
		''' Clean up any resources being used.
		''' </summary>
		Protected Overrides Sub Dispose(disposing As Boolean)
			If disposing Then
				If components IsNot Nothing Then
					components.Dispose()
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub


		#End Region

		#Region "Component Designer generated code"
		''' <summary> 
		''' Required method for Designer support - do not modify 
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			' 
			' ctrl2DColorBox
			' 
			Me.Name = "ctrl2DColorBox"
			Me.Size = New System.Drawing.Size(260, 260)
			AddHandler Me.Resize, New System.EventHandler(AddressOf Me.ctrl2DColorBox_Resize)
			AddHandler Me.Load, New System.EventHandler(AddressOf Me.ctrl2DColorBox_Load)
			AddHandler Me.MouseUp, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl2DColorBox_MouseUp)
			AddHandler Me.Paint, New System.Windows.Forms.PaintEventHandler(AddressOf Me.ctrl2DColorBox_Paint)
			AddHandler Me.MouseMove, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl2DColorBox_MouseMove)
			AddHandler Me.MouseDown, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl2DColorBox_MouseDown)

		End Sub
		#End Region

		#Region "Control Events"

		Private Sub ctrl2DColorBox_Load(sender As Object, e As System.EventArgs)
			Redraw_Control()
		End Sub


		Private Sub ctrl2DColorBox_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If e.Button <> MouseButtons.Left Then
				'	Only respond to left mouse button events
				Return
			End If

			m_bDragging = True
			'	Begin dragging which notifies MouseMove function that it needs to update the marker
			Dim x As Integer = e.X - 2, y As Integer = e.Y - 2
			If x < 0 Then
				x = 0
			End If
			If x > Me.Width - 4 Then
				x = Me.Width - 4
			End If
			'	Calculate marker position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 4 Then
				y = Me.Height - 4
			End If

			If x = m_iMarker_X AndAlso y = m_iMarker_Y Then
				'	If the marker hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawMarker(x, y, True)
			'	Redraw the marker
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls marker (selected color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl2DColorBox_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If Not m_bDragging Then
				'	Only respond when the mouse is dragging the marker.
				Return
			End If

			Dim x As Integer = e.X - 2, y As Integer = e.Y - 2
			If x < 0 Then
				x = 0
			End If
			If x > Me.Width - 4 Then
				x = Me.Width - 4
			End If
			'	Calculate marker position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 4 Then
				y = Me.Height - 4
			End If

			If x = m_iMarker_X AndAlso y = m_iMarker_Y Then
				'	If the marker hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawMarker(x, y, True)
			'	Redraw the marker
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls marker (selected color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl2DColorBox_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If e.Button <> MouseButtons.Left Then
				'	Only respond to left mouse button events
				Return
			End If

			If Not m_bDragging Then
				Return
			End If

			m_bDragging = False

			Dim x As Integer = e.X - 2, y As Integer = e.Y - 2
			If x < 0 Then
				x = 0
			End If
			If x > Me.Width - 4 Then
				x = Me.Width - 4
			End If
			'	Calculate marker position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 4 Then
				y = Me.Height - 4
			End If

			If x = m_iMarker_X AndAlso y = m_iMarker_Y Then
				'	If the marker hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawMarker(x, y, True)
			'	Redraw the marker
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls marker (selected color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl2DColorBox_Resize(sender As Object, e As System.EventArgs)
			Redraw_Control()
		End Sub


		Private Sub ctrl2DColorBox_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs)
			Redraw_Control()
		End Sub


#End Region

#Region "Events"

#Disable Warning
        Public Event Scroll As EventHandler
#Enable Warning

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' The drawstyle of the contol (Hue, Saturation, Brightness, Red, Green or Blue)
        ''' </summary>
        Public Property DrawStyle() As eDrawStyle
			Get
				Return m_eDrawStyle
			End Get
			Set
				m_eDrawStyle = value

				'	Redraw the control based on the new eDrawStyle
				Reset_Marker(True)
				Redraw_Control()
			End Set
		End Property


		''' <summary>
		''' The HSL color of the control, changing the HSL will automatically change the RGB color for the control.
		''' </summary>
		Public Property HSL() As ColorManager.HSL
			Get
				Return m_hsl
			End Get
			Set
				m_hsl = value
				m_rgb = ColorManager.HSL_to_RGB(m_hsl)

				'	Redraw the control based on the new color.
				Reset_Marker(True)
				Redraw_Control()
			End Set
		End Property


		''' <summary>
		''' The RGB color of the control, changing the RGB will automatically change the HSL color for the control.
		''' </summary>
		Public Property RGB() As Color
			Get
				Return m_rgb
			End Get
			Set
				m_rgb = value
				m_hsl = ColorManager.RGB_to_HSL(m_rgb)

				'	Redraw the control based on the new color.
				Reset_Marker(True)
				Redraw_Control()
			End Set
		End Property


		#End Region

		#Region "Private Methods"

		''' <summary>
		''' Redraws only the content over the marker
		''' </summary>
		Private Sub ClearMarker()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			'	Determine the area that needs to be redrawn
			Dim start_x As Integer, start_y As Integer, end_x As Integer, end_y As Integer
			Dim red As Integer = 0
			Dim green As Integer = 0
			Dim blue As Integer = 0
			Dim hsl_start As New ColorManager.HSL()
			Dim hsl_end As New ColorManager.HSL()

			'	Find the markers corners
			start_x = m_iMarker_X - 5
			start_y = m_iMarker_Y - 5
			end_x = m_iMarker_X + 5
			end_y = m_iMarker_Y + 5
			'	Adjust the area if part of it hangs outside the content area
			If start_x < 0 Then
				start_x = 0
			End If
			If start_y < 0 Then
				start_y = 0
			End If
			If end_x > Me.Width - 4 Then
				end_x = Me.Width - 4
			End If
			If end_y > Me.Height - 4 Then
				end_y = Me.Height - 4
			End If

			'	Redraw the content based on the current draw style:
			'	The code get's a little messy from here
			Select Case m_eDrawStyle
				'		  S=0,S=1,S=2,S=3.....S=100
				'	L=100
				'	L=99
				'	L=98		Drawstyle
				'	L=97		   Hue
				'	...
				'	L=0
				Case eDrawStyle.Hue

					hsl_start.H = m_hsl.H
					hsl_end.H = m_hsl.H
					'	Hue is constant
					hsl_start.S = CDbl(start_x) / (Me.Width - 4)
					'	Because we're drawing horizontal lines, s will not change
					hsl_end.S = CDbl(end_x) / (Me.Width - 4)
					'	from line to line
					For i As Integer = start_y To end_y
						'	For each horizontal line:
						hsl_start.L = 1.0 - CDbl(i) / (Me.Height - 4)
						'	Brightness (L) WILL change for each horizontal
						hsl_end.L = hsl_start.L
						'	line drawn
						Dim br As New LinearGradientBrush(New Rectangle(start_x + 1, i + 2, end_x - start_x + 1, 1), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 0, False)
						g.FillRectangle(br, New Rectangle(start_x + 2, i + 2, end_x - start_x + 1, 1))
					Next

					Exit Select
				'		  H=0,H=1,H=2,H=3.....H=360
				'	L=100
				'	L=99
				'	L=98		Drawstyle
				'	L=97		Saturation
				'	...
				'	L=0
				Case eDrawStyle.Saturation

					hsl_start.S = m_hsl.S
					hsl_end.S = m_hsl.S
					'	Saturation is constant
					hsl_start.L = 1.0 - CDbl(start_y) / (Me.Height - 4)
					'	Because we're drawing vertical lines, L will 
					hsl_end.L = 1.0 - CDbl(end_y) / (Me.Height - 4)
					'	not change from line to line
					For i As Integer = start_x To end_x
						'	For each vertical line:
						hsl_start.H = CDbl(i) / (Me.Width - 4)
						'	Hue (H) WILL change for each vertical
						hsl_end.H = hsl_start.H
						'	line drawn
						Dim br As New LinearGradientBrush(New Rectangle(i + 2, start_y + 1, 1, end_y - start_y + 2), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 90, False)
						g.FillRectangle(br, New Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1))
					Next
					Exit Select
				'		  H=0,H=1,H=2,H=3.....H=360
				'	S=100
				'	S=99
				'	S=98		Drawstyle
				'	S=97		Brightness
				'	...
				'	S=0
				Case eDrawStyle.Brightness

					hsl_start.L = m_hsl.L
					hsl_end.L = m_hsl.L
					'	Luminance is constant
					hsl_start.S = 1.0 - CDbl(start_y) / (Me.Height - 4)
					'	Because we're drawing vertical lines, S will 
					hsl_end.S = 1.0 - CDbl(end_y) / (Me.Height - 4)
					'	not change from line to line
					For i As Integer = start_x To end_x
						'	For each vertical line:
						hsl_start.H = CDbl(i) / (Me.Width - 4)
						'	Hue (H) WILL change for each vertical
						hsl_end.H = hsl_start.H
						'	line drawn
						Dim br As New LinearGradientBrush(New Rectangle(i + 2, start_y + 1, 1, end_y - start_y + 2), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 90, False)
						g.FillRectangle(br, New Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1))
					Next

					Exit Select
				'		  B=0,B=1,B=2,B=3.....B=100
				'	G=100
				'	G=99
				'	G=98		Drawstyle
				'	G=97		   Red
				'	...
				'	G=0
				Case eDrawStyle.Red

					red = m_rgb.R
					'	Red is constant
					Dim start_b As Integer = Round(255 * CDbl(start_x) / (Me.Width - 4))
					'	Because we're drawing horizontal lines, B
					Dim end_b As Integer = Round(255 * CDbl(end_x) / (Me.Width - 4))
					'	will not change from line to line
					For i As Integer = start_y To end_y
						'	For each horizontal line:
						green = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))
						'	green WILL change for each horizontal line drawn
						Dim br As New LinearGradientBrush(New Rectangle(start_x + 1, i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b), Color.FromArgb(red, green, end_b), 0, False)
						g.FillRectangle(br, New Rectangle(start_x + 2, i + 2, end_x - start_x + 1, 1))
					Next

					Exit Select
				'		  B=0,B=1,B=2,B=3.....B=100
				'	R=100
				'	R=99
				'	R=98		Drawstyle
				'	R=97		  Green
				'	...
				'	R=0
				Case eDrawStyle.Green

					green = m_rgb.G
					

					'	Green is constant
					Dim start_b2 As Integer = Round(255 * CDbl(start_x) / (Me.Width - 4))
					'	Because we're drawing horizontal lines, B
					Dim end_b2 As Integer = Round(255 * CDbl(end_x) / (Me.Width - 4))
					'	will not change from line to line
					For i As Integer = start_y To end_y
						'	For each horizontal line:
						red = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))
						'	red WILL change for each horizontal line drawn
						Dim br As New LinearGradientBrush(New Rectangle(start_x + 1, i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b2), Color.FromArgb(red, green, end_b2), 0, False)
						g.FillRectangle(br, New Rectangle(start_x + 2, i + 2, end_x - start_x + 1, 1))
					Next

					Exit Select
				'		  R=0,R=1,R=2,R=3.....R=100
				'	G=100
				'	G=99
				'	G=98		Drawstyle
				'	G=97		   Blue
				'	...
				'	G=0
				Case eDrawStyle.Blue

					blue = m_rgb.B
					

					'	Blue is constant
					Dim start_r As Integer = Round(255 * CDbl(start_x) / (Me.Width - 4))
					'	Because we're drawing horizontal lines, R
					Dim end_r As Integer = Round(255 * CDbl(end_x) / (Me.Width - 4))
					'	will not change from line to line
					For i As Integer = start_y To end_y
						'	For each horizontal line:
						green = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))
						'	green WILL change for each horizontal line drawn
						Dim br As New LinearGradientBrush(New Rectangle(start_x + 1, i + 2, end_x - start_x + 1, 1), Color.FromArgb(start_r, green, blue), Color.FromArgb(end_r, green, blue), 0, False)
						g.FillRectangle(br, New Rectangle(start_x + 2, i + 2, end_x - start_x + 1, 1))
					Next

					Exit Select
			End Select
		End Sub


		''' <summary>
		''' Draws the marker (circle) inside the box
		''' </summary>
		''' <param name="x"></param>
		''' <param name="y"></param>
		''' <param name="Unconditional"></param>
		Private Sub DrawMarker(x As Integer, y As Integer, Unconditional As Boolean)
		'	   *****
			'	  *  |  *
			If x < 0 Then
				x = 0
			End If
			'	 *   |   *
			If x > Me.Width - 4 Then
				x = Me.Width - 4
			End If
			'	*    |    *
			If y < 0 Then
				y = 0
			End If
			'	*    |    *
			If y > Me.Height - 4 Then
				y = Me.Height - 4
			End If
			'	*----X----*
			'	*    |    *
			If m_iMarker_Y = y AndAlso m_iMarker_X = x AndAlso Not Unconditional Then
				'	*    |    *
				Return
			End If
			'	 *   |   *
			'	  *  |  *
			ClearMarker()
			'	   *****
			m_iMarker_X = x
			m_iMarker_Y = y

			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim pen As Pen
			Dim _hsl As ColorManager.HSL = GetColor(x, y)
			'	The selected color determines the color of the marker drawn over
			'	it (black or white)
			If _hsl.L < CDbl(200) / 255 Then
				pen = New Pen(Color.White)
			'	White marker if selected color is dark
			ElseIf _hsl.H < CDbl(26) / 360 OrElse _hsl.H > CDbl(200) / 360 Then
				If _hsl.S > CDbl(70) / 255 Then
					pen = New Pen(Color.White)
				Else
					pen = New Pen(Color.Black)
				End If
			Else
				'	Else use a black marker for lighter colors
				pen = New Pen(Color.Black)
			End If

			g.DrawEllipse(pen, x - 3, y - 3, 10, 10)
			'	Draw the marker : 11 x 11 circle
			DrawBorder()
			'	Force the border to be redrawn, just in case the marker has been drawn over it.
		End Sub


		''' <summary>
		''' Draws the border around the control.
		''' </summary>
		Private Sub DrawBorder()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim pencil As Pen

			'	To make the control look like Adobe Photoshop's the border around the control will be a gray line
			'	on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
			'	inside the gray/white rectangle

			pencil = New Pen(Color.FromArgb(172, 168, 153))
			'	The same gray color used by Photoshop
			g.DrawLine(pencil, Me.Width - 2, 0, 0, 0)
			'	Draw top line
			g.DrawLine(pencil, 0, 0, 0, Me.Height - 2)
			'	Draw left hand line
			pencil = New Pen(Color.White)
			g.DrawLine(pencil, Me.Width - 1, 0, Me.Width - 1, Me.Height - 1)
			'	Draw right hand line
			g.DrawLine(pencil, Me.Width - 1, Me.Height - 1, 0, Me.Height - 1)
			'	Draw bottome line
			pencil = New Pen(Color.Black)
			g.DrawRectangle(pencil, 1, 1, Me.Width - 3, Me.Height - 3)
			'	Draw inner black rectangle
		End Sub


		''' <summary>
		''' Evaluates the DrawStyle of the control and calls the appropriate
		''' drawing function for content
		''' </summary>
		Private Sub DrawContent()
			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					Draw_Style_Hue()
					Exit Select
				Case eDrawStyle.Saturation
					Draw_Style_Saturation()
					Exit Select
				Case eDrawStyle.Brightness
					Draw_Style_Luminance()
					Exit Select
				Case eDrawStyle.Red
					Draw_Style_Red()
					Exit Select
				Case eDrawStyle.Green
					Draw_Style_Green()
					Exit Select
				Case eDrawStyle.Blue
					Draw_Style_Blue()
					Exit Select
			End Select
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Hue value.
		''' </summary>
		Private Sub Draw_Style_Hue()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim hsl_start As New ColorManager.HSL()
			Dim hsl_end As New ColorManager.HSL()
			hsl_start.H = m_hsl.H
			hsl_end.H = m_hsl.H
			hsl_start.S = 0.0
			hsl_end.S = 1.0

			For i As Integer = 0 To Me.Height - 5
				'	For each horizontal line in the control:
				hsl_start.L = 1.0 - CDbl(i) / (Me.Height - 4)
				'	Calculate luminance at this line (Hue and Saturation are constant)
				hsl_end.L = hsl_start.L

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, Me.Width - 4, 1), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 0, False)
				g.FillRectangle(br, New Rectangle(2, i + 2, Me.Width - 4, 1))
			Next
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Saturation value.
		''' </summary>
		Private Sub Draw_Style_Saturation()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim hsl_start As New ColorManager.HSL()
			Dim hsl_end As New ColorManager.HSL()
			hsl_start.S = m_hsl.S
			hsl_end.S = m_hsl.S
			hsl_start.L = 1.0
			hsl_end.L = 0.0

			For i As Integer = 0 To Me.Width - 5
				'	For each vertical line in the control:
				hsl_start.H = CDbl(i) / (Me.Width - 4)
				'	Calculate Hue at this line (Saturation and Luminance are constant)
				hsl_end.H = hsl_start.H

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, 1, Me.Height - 4), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 90, False)
				g.FillRectangle(br, New Rectangle(i + 2, 2, 1, Me.Height - 4))
			Next
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Luminance or Brightness value.
		''' </summary>
		Private Sub Draw_Style_Luminance()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim hsl_start As New ColorManager.HSL()
			Dim hsl_end As New ColorManager.HSL()
			hsl_start.L = m_hsl.L
			hsl_end.L = m_hsl.L
			hsl_start.S = 1.0
			hsl_end.S = 0.0

			For i As Integer = 0 To Me.Width - 5
				'	For each vertical line in the control:
				hsl_start.H = CDbl(i) / (Me.Width - 4)
				'	Calculate Hue at this line (Saturation and Luminance are constant)
				hsl_end.H = hsl_start.H

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, 1, Me.Height - 4), ColorManager.HSL_to_RGB(hsl_start), ColorManager.HSL_to_RGB(hsl_end), 90, False)
				g.FillRectangle(br, New Rectangle(i + 2, 2, 1, Me.Height - 4))
			Next
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Red value.
		''' </summary>
		Private Sub Draw_Style_Red()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim red As Integer = m_rgb.R
			


			For i As Integer = 0 To Me.Height - 5
				'	For each horizontal line in the control:
				'	Calculate Green at this line (Red and Blue are constant)
				Dim green As Integer = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, Me.Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, False)
				g.FillRectangle(br, New Rectangle(2, i + 2, Me.Width - 4, 1))
			Next
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Green value.
		''' </summary>
		Private Sub Draw_Style_Green()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim green As Integer = m_rgb.G
			


			For i As Integer = 0 To Me.Height - 5
				'	For each horizontal line in the control:
				'	Calculate Red at this line (Green and Blue are constant)
				Dim red As Integer = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, Me.Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, False)
				g.FillRectangle(br, New Rectangle(2, i + 2, Me.Width - 4, 1))
			Next
		End Sub


		''' <summary>
		''' Draws the content of the control filling in all color values with the provided Blue value.
		''' </summary>
		Private Sub Draw_Style_Blue()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim blue As Integer = m_rgb.B
			


			For i As Integer = 0 To Me.Height - 5
				'	For each horizontal line in the control:
				'	Calculate Green at this line (Red and Blue are constant)
				Dim green As Integer = Round(255 - (255 * CDbl(i) / (Me.Height - 4)))

				Dim br As New LinearGradientBrush(New Rectangle(2, 2, Me.Width - 4, 1), Color.FromArgb(0, green, blue), Color.FromArgb(255, green, blue), 0, False)
				g.FillRectangle(br, New Rectangle(2, i + 2, Me.Width - 4, 1))
			Next
		End Sub


		''' <summary>
		''' Calls all the functions neccessary to redraw the entire control.
		''' </summary>
		Private Sub Redraw_Control()
			DrawBorder()

			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					Draw_Style_Hue()
					Exit Select
				Case eDrawStyle.Saturation
					Draw_Style_Saturation()
					Exit Select
				Case eDrawStyle.Brightness
					Draw_Style_Luminance()
					Exit Select
				Case eDrawStyle.Red
					Draw_Style_Red()
					Exit Select
				Case eDrawStyle.Green
					Draw_Style_Green()
					Exit Select
				Case eDrawStyle.Blue
					Draw_Style_Blue()
					Exit Select
			End Select

			DrawMarker(m_iMarker_X, m_iMarker_Y, True)
		End Sub


		''' <summary>
		''' Resets the marker position of the slider to match the controls color.  Gives the option of redrawing the slider.
		''' </summary>
		''' <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
		Private Sub Reset_Marker(Redraw As Boolean)
			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					m_iMarker_X = Round((Me.Width - 4) * m_hsl.S)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - m_hsl.L))
					Exit Select
				Case eDrawStyle.Saturation
					m_iMarker_X = Round((Me.Width - 4) * m_hsl.H)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - m_hsl.L))
					Exit Select
				Case eDrawStyle.Brightness
					m_iMarker_X = Round((Me.Width - 4) * m_hsl.H)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - m_hsl.S))
					Exit Select
				Case eDrawStyle.Red
					m_iMarker_X = Round((Me.Width - 4) * CDbl(m_rgb.B) / 255)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - CDbl(m_rgb.G) / 255))
					Exit Select
				Case eDrawStyle.Green
					m_iMarker_X = Round((Me.Width - 4) * CDbl(m_rgb.B) / 255)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - CDbl(m_rgb.R) / 255))
					Exit Select
				Case eDrawStyle.Blue
					m_iMarker_X = Round((Me.Width - 4) * CDbl(m_rgb.R) / 255)
					m_iMarker_Y = Round((Me.Height - 4) * (1.0 - CDbl(m_rgb.G) / 255))
					Exit Select
			End Select

			If Redraw Then
				DrawMarker(m_iMarker_X, m_iMarker_Y, True)
			End If
		End Sub


		''' <summary>
		''' Resets the controls color (both HSL and RGB variables) based on the current marker position
		''' </summary>
		Private Sub ResetHSLRGB()
			Dim red As Integer, green As Integer, blue As Integer

			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					m_hsl.S = CDbl(m_iMarker_X) / (Me.Width - 4)
					m_hsl.L = 1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Saturation
					m_hsl.H = CDbl(m_iMarker_X) / (Me.Width - 4)
					m_hsl.L = 1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Brightness
					m_hsl.H = CDbl(m_iMarker_X) / (Me.Width - 4)
					m_hsl.S = 1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Red
					blue = Round(255 * CDbl(m_iMarker_X) / (Me.Width - 4))
					green = Round(255 * (1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)))
					m_rgb = Color.FromArgb(m_rgb.R, green, blue)
					m_hsl = ColorManager.RGB_to_HSL(m_rgb)
					Exit Select
				Case eDrawStyle.Green
					blue = Round(255 * CDbl(m_iMarker_X) / (Me.Width - 4))
					red = Round(255 * (1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)))
					m_rgb = Color.FromArgb(red, m_rgb.G, blue)
					m_hsl = ColorManager.RGB_to_HSL(m_rgb)
					Exit Select
				Case eDrawStyle.Blue
					red = Round(255 * CDbl(m_iMarker_X) / (Me.Width - 4))
					green = Round(255 * (1.0 - CDbl(m_iMarker_Y) / (Me.Height - 4)))
					m_rgb = Color.FromArgb(red, green, m_rgb.B)
					m_hsl = ColorManager.RGB_to_HSL(m_rgb)
					Exit Select
			End Select
		End Sub


		''' <summary>
		''' Kindof self explanitory, I really need to look up the .NET function that does this.
		''' </summary>
		''' <param name="val">double value to be rounded to an integer</param>
		''' <returns></returns>
		Private Function Round(val As Double) As Integer
			Dim ret_val As Integer = CInt(Math.Truncate(val))

			Dim temp As Integer = CInt(Math.Truncate(val * 100))

			If (temp Mod 100) >= 50 Then
				ret_val += 1
			End If

			Return ret_val

		End Function


		''' <summary>
		''' Returns the graphed color at the x,y position on the control
		''' </summary>
		''' <param name="x"></param>
		''' <param name="y"></param>
		''' <returns></returns>
		Private Function GetColor(x As Integer, y As Integer) As ColorManager.HSL

			Dim _hsl As New ColorManager.HSL()

			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					_hsl.H = m_hsl.H
					_hsl.S = CDbl(x) / (Me.Width - 4)
					_hsl.L = 1.0 - CDbl(y) / (Me.Height - 4)
					Exit Select
				Case eDrawStyle.Saturation
					_hsl.S = m_hsl.S
					_hsl.H = CDbl(x) / (Me.Width - 4)
					_hsl.L = 1.0 - CDbl(y) / (Me.Height - 4)
					Exit Select
				Case eDrawStyle.Brightness
					_hsl.L = m_hsl.L
					_hsl.H = CDbl(x) / (Me.Width - 4)
					_hsl.S = 1.0 - CDbl(y) / (Me.Height - 4)
					Exit Select
				Case eDrawStyle.Red
					_hsl = ColorManager.RGB_to_HSL(Color.FromArgb(m_rgb.R, Round(255 * (1.0 - CDbl(y) / (Me.Height - 4))), Round(255 * CDbl(x) / (Me.Width - 4))))
					Exit Select
				Case eDrawStyle.Green
					_hsl = ColorManager.RGB_to_HSL(Color.FromArgb(Round(255 * (1.0 - CDbl(y) / (Me.Height - 4))), m_rgb.G, Round(255 * CDbl(x) / (Me.Width - 4))))
					Exit Select
				Case eDrawStyle.Blue
					_hsl = ColorManager.RGB_to_HSL(Color.FromArgb(Round(255 * CDbl(x) / (Me.Width - 4)), Round(255 * (1.0 - CDbl(y) / (Me.Height - 4))), m_rgb.B))
					Exit Select
			End Select

			Return _hsl
		End Function


		#End Region
	End Class
End Namespace
