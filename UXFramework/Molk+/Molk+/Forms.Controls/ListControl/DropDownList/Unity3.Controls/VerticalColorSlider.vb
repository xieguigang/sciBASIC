#Region "Microsoft.VisualBasic::7b7d84e65be6c52f8308ea8ecfcc1aaa, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\VerticalColorSlider.vb"

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

'****************************************************************

'****                                                        ****

'****     Project:           Adobe Color Picker Clone 1      ****

'****     Filename:          ctrlVerticalColorSlider.cs      ****

'****     Original Author:   Danny Blanchard                 ****

'****                        - scrabcakes@gmail.com          ****

'****     Updates:	                                          ****

'****      3/28/2005 - Initial Version : Danny Blanchard     ****

'****                                                        ****

'****************************************************************


Imports System.Collections
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Windows.Forms

Namespace Unity3.Controls
	''' <summary>
	''' A vertical slider control that shows a range for a color property (a.k.a. Hue, Saturation, Brightness,
	''' Red, Green, Blue) and sends an event when the slider is changed.
	''' </summary>
	Public Class VerticalColorSlider
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



		'	Slider properties
		Private m_iMarker_Start_Y As Integer = 0
		Private m_bDragging As Boolean = False

		'	These variables keep track of how to fill in the content inside the box;
		Private m_eDrawStyle As eDrawStyle = eDrawStyle.Hue
		Private m_hsl As ColorManager.HSL
		Private m_rgb As Color

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
			' ctrl1DColorBar
			' 
			Me.Name = "ctrl1DColorBar"
			Me.Size = New System.Drawing.Size(40, 264)
			AddHandler Me.Resize, New System.EventHandler(AddressOf Me.ctrl1DColorBar_Resize)
			AddHandler Me.Load, New System.EventHandler(AddressOf Me.ctrl1DColorBar_Load)
			AddHandler Me.MouseUp, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl1DColorBar_MouseUp)
			AddHandler Me.Paint, New System.Windows.Forms.PaintEventHandler(AddressOf Me.ctrl1DColorBar_Paint)
			AddHandler Me.MouseMove, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl1DColorBar_MouseMove)
			AddHandler Me.MouseDown, New System.Windows.Forms.MouseEventHandler(AddressOf Me.ctrl1DColorBar_MouseDown)

		End Sub
		#End Region

		#Region "Control Events"

		Private Sub ctrl1DColorBar_Load(sender As Object, e As System.EventArgs)
			Redraw_Control()
		End Sub


		Private Sub ctrl1DColorBar_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If e.Button <> MouseButtons.Left Then
				'	Only respond to left mouse button events
				Return
			End If

			m_bDragging = True
			'	Begin dragging which notifies MouseMove function that it needs to update the marker
			Dim y As Integer
			y = e.Y
			y -= 4
			'	Calculate slider position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 9 Then
				y = Me.Height - 9
			End If

			If y = m_iMarker_Start_Y Then
				'	If the slider hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawSlider(y, False)
			'	Redraw the slider
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls slider(color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl1DColorBar_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If Not m_bDragging Then
				'	Only respond when the mouse is dragging the marker.
				Return
			End If

			Dim y As Integer
			y = e.Y
			y -= 4
			'	Calculate slider position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 9 Then
				y = Me.Height - 9
			End If

			If y = m_iMarker_Start_Y Then
				'	If the slider hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawSlider(y, False)
			'	Redraw the slider
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls slider(color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl1DColorBar_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs)
			If e.Button <> MouseButtons.Left Then
				'	Only respond to left mouse button events
				Return
			End If

			m_bDragging = False

			Dim y As Integer
			y = e.Y
			y -= 4
			'	Calculate slider position
			If y < 0 Then
				y = 0
			End If
			If y > Me.Height - 9 Then
				y = Me.Height - 9
			End If

			If y = m_iMarker_Start_Y Then
				'	If the slider hasn't moved, no need to redraw it.
				Return
			End If
			'	or send a scroll notification
			DrawSlider(y, False)
			'	Redraw the slider
			ResetHSLRGB()
			'	Reset the color
			'	Notify anyone who cares that the controls slider(color) has changed
			RaiseEvent Scroll(Me, e)
		End Sub


		Private Sub ctrl1DColorBar_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs)
			Redraw_Control()
		End Sub


		Private Sub ctrl1DColorBar_Resize(sender As Object, e As System.EventArgs)
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
				Reset_Slider(True)
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
				Reset_Slider(True)
				DrawContent()
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
				Reset_Slider(True)
				DrawContent()
			End Set
		End Property


		#End Region

		#Region "Private Methods"

		''' <summary>
		''' Redraws the background over the slider area on both sides of the control
		''' </summary>
		Private Sub ClearSlider()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()
			Dim brush As Brush = System.Drawing.SystemBrushes.Control
			g.FillRectangle(brush, 0, 0, 8, Me.Height)
			'	clear left hand slider
			g.FillRectangle(brush, Me.Width - 8, 0, 8, Me.Height)
			'	clear right hand slider
		End Sub


		''' <summary>
		''' Draws the slider arrows on both sides of the control.
		''' </summary>
		''' <param name="position">position value of the slider, lowest being at the bottom.  The range
		''' is between 0 and the controls height-9.  The values will be adjusted if too large/small</param>
		''' <param name="Unconditional">If Unconditional is true, the slider is drawn, otherwise some logic 
		''' is performed to determine is drawing is really neccessary.</param>
		Private Sub DrawSlider(position As Integer, Unconditional As Boolean)
			If position < 0 Then
				position = 0
			End If
			If position > Me.Height - 9 Then
				position = Me.Height - 9
			End If

			If m_iMarker_Start_Y = position AndAlso Not Unconditional Then
				'	If the marker position hasn't changed
				Return
			End If
			'	since the last time it was drawn and we don't HAVE to redraw
			'	then exit procedure
			m_iMarker_Start_Y = position
			'	Update the controls marker position
			Me.ClearSlider()
			'	Remove old slider
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim pencil As New Pen(Color.FromArgb(116, 114, 106))
			'	Same gray color Photoshop uses
			Dim brush As Brush = Brushes.White

			Dim arrow As Point() = New Point(6) {}
			'	 GGG
			arrow(0) = New Point(1, position)
			'	G   G
			arrow(1) = New Point(3, position)
			'	G    G
			arrow(2) = New Point(7, position + 4)
			'	G     G
			arrow(3) = New Point(3, position + 8)
			'	G      G
			arrow(4) = New Point(1, position + 8)
			'	G     G
			arrow(5) = New Point(0, position + 7)
			'	G    G
			arrow(6) = New Point(0, position + 1)
			'	G   G
			'	 GGG
			g.FillPolygon(brush, arrow)
			'	Fill left arrow with white
			g.DrawPolygon(pencil, arrow)
			'	Draw left arrow border with gray
			'	    GGG
			arrow(0) = New Point(Me.Width - 2, position)
			'	   G   G
			arrow(1) = New Point(Me.Width - 4, position)
			'	  G    G
			arrow(2) = New Point(Me.Width - 8, position + 4)
			'	 G     G
			arrow(3) = New Point(Me.Width - 4, position + 8)
			'	G      G
			arrow(4) = New Point(Me.Width - 2, position + 8)
			'	 G     G
			arrow(5) = New Point(Me.Width - 1, position + 7)
			'	  G    G
			arrow(6) = New Point(Me.Width - 1, position + 1)
			'	   G   G
			'	    GGG
			g.FillPolygon(brush, arrow)
			'	Fill right arrow with white
			g.DrawPolygon(pencil, arrow)
			'	Draw right arrow border with gray
		End Sub


		''' <summary>
		''' Draws the border around the control, in this case the border around the content area between
		''' the slider arrows.
		''' </summary>
		Private Sub DrawBorder()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim pencil As Pen

			'	To make the control look like Adobe Photoshop's the border around the control will be a gray line
			'	on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
			'	inside the gray/white rectangle

			pencil = New Pen(Color.FromArgb(172, 168, 153))
			'	The same gray color used by Photoshop
			g.DrawLine(pencil, Me.Width - 10, 2, 9, 2)
			'	Draw top line
			g.DrawLine(pencil, 9, 2, 9, Me.Height - 4)
			'	Draw left hand line
			pencil = New Pen(Color.White)
			g.DrawLine(pencil, Me.Width - 9, 2, Me.Width - 9, Me.Height - 3)
			'	Draw right hand line
			g.DrawLine(pencil, Me.Width - 9, Me.Height - 3, 9, Me.Height - 3)
			'	Draw bottome line
			pencil = New Pen(Color.Black)
			g.DrawRectangle(pencil, 10, 3, Me.Width - 20, Me.Height - 7)
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


		#Region "Draw_Style_X - Content drawing functions"

		'	The following functions do the real work of the control, drawing the primary content (the area between the slider)
		'	

		''' <summary>
		''' Fills in the content of the control showing all values of Hue (from 0 to 360)
		''' </summary>
		Private Sub Draw_Style_Hue()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim _hsl As New ColorManager.HSL()
			_hsl.S = 1.0
			'	S and L will both be at 100% for this DrawStyle
			_hsl.L = 1.0

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				_hsl.H = 1.0 - CDbl(i) / (Me.Height - 8)
				'	H (hue) is based on the current vertical position
				Dim pen As New Pen(ColorManager.HSL_to_RGB(_hsl))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		''' <summary>
		''' Fills in the content of the control showing all values of Saturation (0 to 100%) for the given
		''' Hue and Luminance.
		''' </summary>
		Private Sub Draw_Style_Saturation()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim _hsl As New ColorManager.HSL()
			_hsl.H = m_hsl.H
			'	Use the H and L values of the current color (m_hsl)
			_hsl.L = m_hsl.L

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				_hsl.S = 1.0 - CDbl(i) / (Me.Height - 8)
				'	S (Saturation) is based on the current vertical position
				Dim pen As New Pen(ColorManager.HSL_to_RGB(_hsl))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		''' <summary>
		''' Fills in the content of the control showing all values of Luminance (0 to 100%) for the given
		''' Hue and Saturation.
		''' </summary>
		Private Sub Draw_Style_Luminance()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			Dim _hsl As New ColorManager.HSL()
			_hsl.H = m_hsl.H
			'	Use the H and S values of the current color (m_hsl)
			_hsl.S = m_hsl.S

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				_hsl.L = 1.0 - CDbl(i) / (Me.Height - 8)
				'	L (Luminance) is based on the current vertical position
				Dim pen As New Pen(ColorManager.HSL_to_RGB(_hsl))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		''' <summary>
		''' Fills in the content of the control showing all values of Red (0 to 255) for the given
		''' Green and Blue.
		''' </summary>
		Private Sub Draw_Style_Red()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				Dim red As Integer = 255 - Round(255 * CDbl(i) / (Me.Height - 8))
				'	red is based on the current vertical position
				Dim pen As New Pen(Color.FromArgb(red, m_rgb.G, m_rgb.B))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		''' <summary>
		''' Fills in the content of the control showing all values of Green (0 to 255) for the given
		''' Red and Blue.
		''' </summary>
		Private Sub Draw_Style_Green()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				Dim green As Integer = 255 - Round(255 * CDbl(i) / (Me.Height - 8))
				'	green is based on the current vertical position
				Dim pen As New Pen(Color.FromArgb(m_rgb.R, green, m_rgb.B))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		''' <summary>
		''' Fills in the content of the control showing all values of Blue (0 to 255) for the given
		''' Red and Green.
		''' </summary>
		Private Sub Draw_Style_Blue()
			Dim g As System.Drawing.Graphics = Me.CreateGraphics()

			For i As Integer = 0 To Me.Height - 9
				'	i represents the current line of pixels we want to draw horizontally
				Dim blue As Integer = 255 - Round(255 * CDbl(i) / (Me.Height - 8))
				'	green is based on the current vertical position
				Dim pen As New Pen(Color.FromArgb(m_rgb.R, m_rgb.G, blue))
				'	Get the Color for this line
					'	Draw the line and loop back for next line
				g.DrawLine(pen, 11, i + 4, Me.Width - 11, i + 4)
			Next
		End Sub


		#End Region

		''' <summary>
		''' Calls all the functions neccessary to redraw the entire control.
		''' </summary>
		Private Sub Redraw_Control()
			DrawSlider(m_iMarker_Start_Y, True)
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
		End Sub


		''' <summary>
		''' Resets the vertical position of the slider to match the controls color.  Gives the option of redrawing the slider.
		''' </summary>
		''' <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
		Private Sub Reset_Slider(Redraw As Boolean)
			'	The position of the marker (slider) changes based on the current drawstyle:
			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * m_hsl.H)
					Exit Select
				Case eDrawStyle.Saturation
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * m_hsl.S)
					Exit Select
				Case eDrawStyle.Brightness
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * m_hsl.L)
					Exit Select
				Case eDrawStyle.Red
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * CDbl(m_rgb.R) / 255)
					Exit Select
				Case eDrawStyle.Green
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * CDbl(m_rgb.G) / 255)
					Exit Select
				Case eDrawStyle.Blue
					m_iMarker_Start_Y = (Me.Height - 8) - Round((Me.Height - 8) * CDbl(m_rgb.B) / 255)
					Exit Select
			End Select

			If Redraw Then
				DrawSlider(m_iMarker_Start_Y, True)
			End If
		End Sub


		''' <summary>
		''' Resets the controls color (both HSL and RGB variables) based on the current slider position
		''' </summary>
		Private Sub ResetHSLRGB()
			Select Case m_eDrawStyle
				Case eDrawStyle.Hue
					m_hsl.H = 1.0 - CDbl(m_iMarker_Start_Y) / (Me.Height - 9)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Saturation
					m_hsl.S = 1.0 - CDbl(m_iMarker_Start_Y) / (Me.Height - 9)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Brightness
					m_hsl.L = 1.0 - CDbl(m_iMarker_Start_Y) / (Me.Height - 9)
					m_rgb = ColorManager.HSL_to_RGB(m_hsl)
					Exit Select
				Case eDrawStyle.Red
					m_rgb = Color.FromArgb(255 - Round(255 * CDbl(m_iMarker_Start_Y) / (Me.Height - 9)), m_rgb.G, m_rgb.B)
					m_hsl = ColorManager.RGB_to_HSL(m_rgb)
					Exit Select
				Case eDrawStyle.Green
					m_rgb = Color.FromArgb(m_rgb.R, 255 - Round(255 * CDbl(m_iMarker_Start_Y) / (Me.Height - 9)), m_rgb.B)
					m_hsl = ColorManager.RGB_to_HSL(m_rgb)
					Exit Select
				Case eDrawStyle.Blue
					m_rgb = Color.FromArgb(m_rgb.R, m_rgb.G, 255 - Round(255 * CDbl(m_iMarker_Start_Y) / (Me.Height - 9)))
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


		#End Region
	End Class
End Namespace
