#Region "Microsoft.VisualBasic::3af6945ecf9effa98cff76632b671c07, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\WindowsVista\WindowsVistaRenderer.vb"

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
Imports System.Drawing.Drawing2D

''' <summary>
''' Renders toolstrip items using Windows Vista look and feel
''' </summary>
''' <remarks>
''' 2007 José Manuel Menéndez Poo
''' Visit my blog for upgrades and other renderers - www.menendezpoo.com
''' </remarks>
Public Class WindowsVistaRenderer
	Inherits ToolStripRenderer
	#Region "Static"

	''' <summary>
	''' Creates the glow of the buttons
	''' </summary>
	''' <param name="rectangle"></param>
	''' <returns></returns>
	Private Shared Function CreateBottomRadialPath(rectangle As Rectangle) As GraphicsPath
		Dim path As New GraphicsPath()
		Dim rect As RectangleF = rectangle
		rect.X -= rect.Width * 0.35F
		rect.Y -= rect.Height * 0.15F
		rect.Width *= 1.7F
		rect.Height *= 2.3F
		path.AddEllipse(rect)
		path.CloseFigure()
		Return path
	End Function

	''' <summary>
	''' Creates the chevron for the overflow button
	''' </summary>
	''' <param name="overflowButtonSize"></param>
	''' <returns></returns>
	Private Shared Function CreateOverflowChevron(overflowButtonSize As Size) As GraphicsPath
		Dim r As New Rectangle(Point.Empty, overflowButtonSize)
		Dim path As New GraphicsPath()

		Dim segmentWidth As Integer = 3
		Dim segmentHeight As Integer = 3
		Dim segmentSeparation As Integer = 5
		Dim chevronWidth As Integer = segmentWidth + segmentSeparation
		Dim chevronHeight As Integer = segmentHeight * 2
		Dim chevronLeft As Integer = (r.Width - chevronWidth) \ 2
		Dim chevronTop As Integer = (r.Height - chevronHeight) \ 2

		' Segment \
		path.AddLine(New Point(chevronLeft, chevronTop), New Point(chevronLeft + segmentWidth, chevronTop + segmentHeight))

		' Segment /
		path.AddLine(New Point(chevronLeft + segmentWidth, chevronTop + segmentHeight), New Point(chevronLeft, chevronTop + segmentHeight * 2))

		path.StartFigure()

		' Segment \
		path.AddLine(New Point(segmentSeparation + chevronLeft, chevronTop), New Point(segmentSeparation + chevronLeft + segmentWidth, chevronTop + segmentHeight))

		' Segment /
		path.AddLine(New Point(segmentSeparation + chevronLeft + segmentWidth, chevronTop + segmentHeight), New Point(segmentSeparation + chevronLeft, chevronTop + segmentHeight * 2))


		Return path
	End Function

	#End Region

	#Region "Fields"

	Private _colorTable As WindowsVistaColorTable
	Private _glossyEffect As Boolean
	Private _bgglow As Boolean
	Private _toolstripRadius As Integer
	Private _buttonRadius As Integer

	#End Region

	#Region "Ctor"

	Public Sub New()
		ColorTable = New WindowsVistaColorTable()

		GlossyEffect = True
		BackgroundGlow = True
		ToolStripRadius = 2
		ButtonRadius = 2
	End Sub

	#End Region

	#Region "Properties"

	''' <summary>
	''' Gets or sets the buttons rectangle radius
	''' </summary>
	Public Property ButtonRadius() As Integer
		Get
			Return _buttonRadius
		End Get
		Set
			_buttonRadius = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the radius of the rectangle of the hole ToolStrip
	''' </summary>
	Public Property ToolStripRadius() As Integer
		Get
			Return _toolstripRadius
		End Get
		Set
			_toolstripRadius = value
		End Set
	End Property

	''' <summary>
	''' Gets ors sets if background glow should be rendered
	''' </summary>
	Public Property BackgroundGlow() As Boolean
		Get
			Return _bgglow
		End Get
		Set
			_bgglow = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets if glossy effect should be rendered
	''' </summary>
	Public Property GlossyEffect() As Boolean
		Get
			Return _glossyEffect
		End Get
		Set
			_glossyEffect = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the color table of the renderer
	''' </summary>
	Public Property ColorTable() As WindowsVistaColorTable
		Get
			Return _colorTable
		End Get
		Set
			_colorTable = value
		End Set
	End Property


	#End Region

	#Region "Methods"

	''' <summary>
	''' Initializes properties for ToolStripMenuItem objects
	''' </summary>
	''' <param name="item"></param>
	Protected Overridable Sub InitializeToolStripMenuItem(item As ToolStripMenuItem)
		item.AutoSize = False
		item.Height = 26
		item.TextAlign = ContentAlignment.MiddleLeft

		For Each subitem As ToolStripItem In item.DropDownItems
			If TypeOf subitem Is ToolStripMenuItem Then
				InitializeToolStripMenuItem(TryCast(subitem, ToolStripMenuItem))
			End If
		Next
	End Sub

	''' <summary>
	''' Gets a rounded rectangle representing the hole area of the toolstrip
	''' </summary>
	''' <param name="toolStrip"></param>
	''' <returns></returns>
	Private Function GetToolStripRectangle(toolStrip As ToolStrip) As GraphicsPath
		Return GraphicsTools.CreateRoundRectangle(New Rectangle(0, 0, toolStrip.Width - 1, toolStrip.Height - 1), ToolStripRadius)
	End Function

	''' <summary>
	''' Draws the glossy effect on the toolbar
	''' </summary>
	''' <param name="g"></param>
	''' <param name="t"></param>
    Private Sub DrawGlossyEffect(g As Graphics, t As ToolStrip)
        DrawGlossyEffect(g, t, 0)
    End Sub

	''' <summary>
	''' Draws the glossy effect on the toolbar
	''' </summary>
	''' <param name="g"></param>
	''' <param name="t"></param>
    Private Sub DrawGlossyEffect(g As Graphics, t As ToolStrip, offset As Integer)
        Dim glossyRect As New Rectangle(0, offset, t.Width - 1, (t.Height - 1) \ 2)

        Using b As New LinearGradientBrush(glossyRect.Location, New PointF(0, glossyRect.Bottom), ColorTable.GlossyEffectNorth, ColorTable.GlossyEffectSouth)
            Using border As GraphicsPath = GraphicsTools.CreateTopRoundRectangle(glossyRect, ToolStripRadius)
                g.FillPath(b, border)
            End Using
        End Using
    End Sub

	''' <summary>
	''' Renders the background of a button
	''' </summary>
	''' <param name="e"></param>
	Private Sub DrawVistaButtonBackground(e As ToolStripItemRenderEventArgs)
		Dim chk As Boolean = False

		If TypeOf e.Item Is ToolStripButton Then
			chk = TryCast(e.Item, ToolStripButton).Checked
		End If

		DrawVistaButtonBackground(e.Graphics, New Rectangle(Point.Empty, e.Item.Size), e.Item.Selected, e.Item.Pressed, chk)
	End Sub

	''' <summary>
	''' Renders the background of a button on the specified rectangle using the specified device
	''' </summary>
	Private Sub DrawVistaButtonBackground(g As Graphics, r As Rectangle, selected As Boolean, pressed As Boolean, checkd As Boolean)
		g.SmoothingMode = SmoothingMode.AntiAlias

		Dim outerBorder As New Rectangle(r.Left, r.Top, r.Width - 1, r.Height - 1)
		Dim border As Rectangle = outerBorder
		border.Inflate(-1, -1)
		Dim innerBorder As Rectangle = border
		innerBorder.Inflate(-1, -1)
		Dim glossy As Rectangle = outerBorder
        glossy.Height = CInt(glossy.Height / 2)
		Dim fill As Rectangle = innerBorder
        fill.Height = CInt(fill.Height / 2)
        Dim glow As Rectangle = System.Drawing.Rectangle.FromLTRB(outerBorder.Left, outerBorder.Top + Convert.ToInt32(Convert.ToSingle(outerBorder.Height) * 0.5F), outerBorder.Right, outerBorder.Bottom)

		If selected OrElse pressed OrElse checkd Then
			'#Region "Layers"

			'Outer border
			Using path As GraphicsPath = GraphicsTools.CreateRoundRectangle(outerBorder, ButtonRadius)
				Using p As New Pen(ColorTable.ButtonOuterBorder)
					g.DrawPath(p, path)
				End Using
			End Using

			'Checked fill
			If checkd Then
				Using path As GraphicsPath = GraphicsTools.CreateRoundRectangle(innerBorder, 2)
					Using b As Brush = New SolidBrush(If(selected, ColorTable.CheckedButtonFillHot, ColorTable.CheckedButtonFill))
						g.FillPath(b, path)
					End Using
				End Using
			End If

			'Glossy effefct
			Using path As GraphicsPath = GraphicsTools.CreateTopRoundRectangle(glossy, ButtonRadius)
				Using b As Brush = New LinearGradientBrush(New Point(0, glossy.Top), New Point(0, glossy.Bottom), ColorTable.GlossyEffectNorth, ColorTable.GlossyEffectSouth)
					g.FillPath(b, path)
				End Using
			End Using

			'Border
			Using path As GraphicsPath = GraphicsTools.CreateRoundRectangle(border, ButtonRadius)
				Using p As New Pen(ColorTable.ButtonBorder)
					g.DrawPath(p, path)
				End Using
			End Using

			Dim fillNorth As Color = If(pressed, ColorTable.ButtonFillNorthPressed, ColorTable.ButtonFillNorth)
			Dim fillSouth As Color = If(pressed, ColorTable.ButtonFillSouthPressed, ColorTable.ButtonFillSouth)

			'Fill
			Using path As GraphicsPath = GraphicsTools.CreateTopRoundRectangle(fill, ButtonRadius)
				Using b As Brush = New LinearGradientBrush(New Point(0, fill.Top), New Point(0, fill.Bottom), fillNorth, fillSouth)
					g.FillPath(b, path)
				End Using
			End Using

			Dim innerBorderColor As Color = If(pressed OrElse checkd, ColorTable.ButtonInnerBorderPressed, ColorTable.ButtonInnerBorder)

			'Inner border
			Using path As GraphicsPath = GraphicsTools.CreateRoundRectangle(innerBorder, ButtonRadius)
				Using p As New Pen(innerBorderColor)
					g.DrawPath(p, path)
				End Using
			End Using

			'Glow
			Using clip As GraphicsPath = GraphicsTools.CreateRoundRectangle(glow, 2)
				g.SetClip(clip, CombineMode.Intersect)

				Dim glowColor As Color = ColorTable.Glow

				If checkd Then
					If selected Then
						glowColor = ColorTable.CheckedGlowHot
					Else
						glowColor = ColorTable.CheckedGlow
					End If
				End If

				Using brad As GraphicsPath = CreateBottomRadialPath(glow)
					Using pgr As New PathGradientBrush(brad)
												Dim opacity As Integer = 255
						Dim bounds As RectangleF = brad.GetBounds()
						pgr.CenterPoint = New PointF((bounds.Left + bounds.Right) / 2F, (bounds.Top + bounds.Bottom) / 2F)
						pgr.CenterColor = Color.FromArgb(opacity, glowColor)
						pgr.SurroundColors = New Color() {Color.FromArgb(0, glowColor)}

						g.FillPath(pgr, brad)
					End Using
				End Using
				g.ResetClip()




				'#End Region
			End Using
		End If
	End Sub

	''' <summary>
	''' Draws the background of a menu, vista style
	''' </summary>
	''' <param name="e"></param>
	Private Sub DrawVistaMenuBackground(e As ToolStripItemRenderEventArgs)

		DrawVistaMenuBackground(e.Graphics, New Rectangle(Point.Empty, e.Item.Size), e.Item.Selected, TypeOf e.Item.Owner Is MenuStrip)

	End Sub

	''' <summary>
	''' Draws the background of a menu, vista style
	''' </summary>
	''' <param name="g"></param>
	''' <param name="r"></param>
	''' <param name="highlighted"></param>
	Private Sub DrawVistaMenuBackground(g As Graphics, r As Rectangle, highlighted As Boolean, isMainMenu As Boolean)
		'g.Clear(ColorTable.MenuBackground);

		Dim margin As Integer = 2
		Dim left As Integer = 22

		'#Region "IconSeparator"

		If Not isMainMenu Then
			Using p As New Pen(ColorTable.MenuDark)
				g.DrawLine(p, New Point(r.Left + left, r.Top), New Point(r.Left + left, r.Height - margin))
			End Using


			Using p As New Pen(ColorTable.MenuLight)
				g.DrawLine(p, New Point(r.Left + left + 1, r.Top), New Point(r.Left + left + 1, r.Height - margin))
			End Using
		End If

		'#End Region

		If highlighted Then
			'#Region "Draw Rectangle"

			Using path As GraphicsPath = GraphicsTools.CreateRoundRectangle(New Rectangle(r.X + margin, r.Y + margin, r.Width - margin * 2, r.Height - margin * 2), 3)

				Using b As Brush = New LinearGradientBrush(New Point(0, 2), New Point(0, r.Height - 2), ColorTable.MenuHighlightNorth, ColorTable.MenuHighlightSouth)
					g.FillPath(b, path)
				End Using

				Using p As New Pen(ColorTable.MenuHighlight)
					g.DrawPath(p, path)
				End Using

				'#End Region
			End Using
		End If

	End Sub

	''' <summary>
	''' Draws the border of the vista menu window
	''' </summary>
	''' <param name="g"></param>
	''' <param name="r"></param>
	Private Sub DrawVistaMenuBorder(g As Graphics, r As Rectangle)
		Using p As New Pen(ColorTable.BackgroundBorder)
			g.DrawRectangle(p, New Rectangle(r.Left, r.Top, r.Width - 1, r.Height - 1))
		End Using
	End Sub

	#End Region

	Protected Overrides Sub Initialize(toolStrip As ToolStrip)
		MyBase.Initialize(toolStrip)

		toolStrip.AutoSize = False
		toolStrip.Height = 35
		toolStrip.ForeColor = ColorTable.Text
		toolStrip.GripStyle = ToolStripGripStyle.Hidden
	End Sub

	Protected Overrides Sub InitializeItem(item As ToolStripItem)
		MyBase.InitializeItem(item)

		'Don't Affect ForeColor of TextBoxes and ComboBoxes
		If Not ((TypeOf item Is ToolStripTextBox) OrElse (TypeOf item Is ToolStripComboBox)) Then
			item.ForeColor = ColorTable.Text
		End If

		item.Padding = New Padding(5)

		If TypeOf item Is ToolStripSplitButton Then
			Dim btn As ToolStripSplitButton = TryCast(item, ToolStripSplitButton)
			btn.DropDownButtonWidth = 18

			For Each subitem As ToolStripItem In btn.DropDownItems
				If TypeOf subitem Is ToolStripMenuItem Then
					InitializeToolStripMenuItem(TryCast(subitem, ToolStripMenuItem))
				End If
			Next
		End If

		If TypeOf item Is ToolStripDropDownButton Then
			Dim btn As ToolStripDropDownButton = TryCast(item, ToolStripDropDownButton)
			btn.ShowDropDownArrow = False

			For Each subitem As ToolStripItem In btn.DropDownItems
				If TypeOf subitem Is ToolStripMenuItem Then
					InitializeToolStripMenuItem(TryCast(subitem, ToolStripMenuItem))
				End If
			Next
		End If
	End Sub

	Protected Overrides Sub OnRenderToolStripBorder(e As ToolStripRenderEventArgs)

		If TypeOf e.ToolStrip Is ToolStripDropDownMenu Then
			'#Region "Draw Rectangled Border"


				'#End Region
			DrawVistaMenuBorder(e.Graphics, New Rectangle(Point.Empty, e.ToolStrip.Size))
		Else
			'#Region "Draw Rounded Border"
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

			Using path As GraphicsPath = GetToolStripRectangle(e.ToolStrip)
				Using p As New Pen(ColorTable.BackgroundBorder)
					e.Graphics.DrawPath(p, path)
				End Using
				'#End Region
			End Using
		End If


	End Sub

	Protected Overrides Sub OnRenderToolStripBackground(e As ToolStripRenderEventArgs)
		If TypeOf e.ToolStrip Is ToolStripDropDownMenu Then
			Return
		End If

		'#Region "Background"

		Using b As New LinearGradientBrush(Point.Empty, New PointF(0, e.ToolStrip.Height), ColorTable.BackgroundNorth, ColorTable.BackgroundSouth)
			Using border As GraphicsPath = GetToolStripRectangle(e.ToolStrip)
				e.Graphics.FillPath(b, border)
			End Using
		End Using

		'#End Region

		If GlossyEffect Then
			'#Region "Glossy Effect"


				'#End Region
			DrawGlossyEffect(e.Graphics, e.ToolStrip, 1)
		End If

		If BackgroundGlow Then
			'#Region "BackroundGlow"

			Dim glowSize As Integer = Convert.ToInt32(Convert.ToSingle(e.ToolStrip.Height) * 0.15F)
			Dim glow As New Rectangle(0, e.ToolStrip.Height - glowSize - 1, e.ToolStrip.Width - 1, glowSize)

			Using b As New LinearGradientBrush(New Point(0, glow.Top - 1), New PointF(0, glow.Bottom), Color.FromArgb(0, ColorTable.BackgroundGlow), ColorTable.BackgroundGlow)
				Using border As GraphicsPath = GraphicsTools.CreateBottomRoundRectangle(glow, ToolStripRadius)
					e.Graphics.FillPath(b, border)
				End Using


				'#End Region
			End Using
		End If

	End Sub

	Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)

		If TypeOf e.Item Is ToolStripButton Then
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
		End If

		If TypeOf e.Item Is ToolStripMenuItem AndAlso Not (TypeOf e.Item.Owner Is MenuStrip) Then
			Dim r As New Rectangle(e.TextRectangle.Location, New Size(e.TextRectangle.Width, 24))
			e.TextRectangle = r
			e.TextColor = ColorTable.MenuText
		End If

		MyBase.OnRenderItemText(e)
	End Sub

	Protected Overrides Sub OnRenderButtonBackground(e As ToolStripItemRenderEventArgs)
		DrawVistaButtonBackground(e)
	End Sub

	Protected Overrides Sub OnRenderDropDownButtonBackground(e As ToolStripItemRenderEventArgs)
		DrawVistaButtonBackground(e)

		Dim item As ToolStripDropDownButton = TryCast(e.Item, ToolStripDropDownButton)
		If item Is Nothing Then
			Return
		End If

		Dim arrowBounds As New Rectangle(item.Width - 18, 0, 18, item.Height)

		DrawArrow(New ToolStripArrowRenderEventArgs(e.Graphics, e.Item, arrowBounds, ColorTable.DropDownArrow, ArrowDirection.Down))
	End Sub

	Protected Overrides Sub OnRenderSplitButtonBackground(e As ToolStripItemRenderEventArgs)
		DrawVistaButtonBackground(e)

		Dim item As ToolStripSplitButton = TryCast(e.Item, ToolStripSplitButton)
		If item Is Nothing Then
			Return
		End If

		Dim arrowBounds As Rectangle = item.DropDownButtonBounds
		Dim buttonBounds As New Rectangle(item.ButtonBounds.Location, New Size(item.ButtonBounds.Width + 2, item.ButtonBounds.Height))
		Dim dropDownBounds As Rectangle = item.DropDownButtonBounds

		DrawVistaButtonBackground(e.Graphics, buttonBounds, item.ButtonSelected, item.ButtonPressed, False)

		DrawVistaButtonBackground(e.Graphics, dropDownBounds, item.DropDownButtonSelected, item.DropDownButtonPressed, False)

		DrawArrow(New ToolStripArrowRenderEventArgs(e.Graphics, e.Item, arrowBounds, ColorTable.DropDownArrow, ArrowDirection.Down))
	End Sub

	Protected Overrides Sub OnRenderItemImage(e As ToolStripItemImageRenderEventArgs)
		If Not e.Item.Enabled Then
			MyBase.OnRenderItemImage(e)
		Else
			If e.Image IsNot Nothing Then
				e.Graphics.DrawImage(e.Image, e.ImageRectangle)
			End If
		End If

	End Sub

	Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
		If TypeOf e.Item.Owner Is MenuStrip Then
			DrawVistaButtonBackground(e)
		Else

			DrawVistaMenuBackground(e.Graphics, New Rectangle(Point.Empty, e.Item.Size), e.Item.Selected, TypeOf e.Item.Owner Is MenuStrip)
		End If


	End Sub

	Protected Overrides Sub OnRenderSeparator(e As ToolStripSeparatorRenderEventArgs)



		If e.Item.IsOnDropDown Then
			Dim left As Integer = 20
			Dim right As Integer = e.Item.Width - 3
			Dim top As Integer = e.Item.Height \ 2
			top -= 1

			'e.Graphics.Clear(ColorTable.MenuBackground);

			Using p As New Pen(ColorTable.MenuDark)
				e.Graphics.DrawLine(p, New Point(left, top), New Point(right, top))
			End Using

			Using p As New Pen(ColorTable.MenuLight)
				e.Graphics.DrawLine(p, New Point(left, top + 1), New Point(right, top + 1))
			End Using
		Else
			Dim top As Integer = 3
			Dim left As Integer = e.Item.Width \ 2
			left -= 1
			Dim height As Integer = e.Item.Height - top * 2
			Dim separator As New RectangleF(left, top, 0.5F, height)

			Using b As Brush = New LinearGradientBrush(separator.Location, New Point(Convert.ToInt32(separator.Left), Convert.ToInt32(separator.Bottom)), ColorTable.SeparatorNorth, ColorTable.SeparatorSouth)
				e.Graphics.FillRectangle(b, separator)
			End Using
		End If
	End Sub

	Protected Overrides Sub OnRenderOverflowButtonBackground(e As ToolStripItemRenderEventArgs)
		DrawVistaButtonBackground(e)

		'Chevron is obtained from the character: » (Alt+0187)
		Using b As Brush = New SolidBrush(e.Item.ForeColor)
			Dim sf As New StringFormat()
			sf.Alignment = StringAlignment.Center
			sf.LineAlignment = StringAlignment.Center

			Dim f As New Font(e.Item.Font.FontFamily, 15)

			e.Graphics.DrawString("»", f, b, New RectangleF(Point.Empty, e.Item.Size), sf)
		End Using

	End Sub

	Protected Overrides Sub OnRenderArrow(e As ToolStripArrowRenderEventArgs)
		If TypeOf e.Item Is ToolStripMenuItem AndAlso e.Item.Selected Then
			e.ArrowColor = ColorTable.MenuText
		End If

		MyBase.OnRenderArrow(e)
	End Sub
End Class
