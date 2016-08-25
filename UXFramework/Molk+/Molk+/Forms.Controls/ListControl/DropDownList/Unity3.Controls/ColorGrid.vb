#Region "Microsoft.VisualBasic::30c901fa73db160c3df0e4437dc60a5a, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorGrid.vb"

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

Namespace Unity3.Controls
	Public Class ColorGrid
		Inherits UserControl
		Public Items As List(Of NamedColor)

		Private ttp As ToolTip
		Private hoverIndex As Integer = -1
		Private mouseLeft As Boolean

		Private _GridSize As New Size(40, 20)
		Public Property GridSize() As Size
			Get
				Return _GridSize
			End Get
			Set
				_GridSize = value
				Me.Invalidate()
			End Set
		End Property

		Private _GridPadding As Byte = 4
		Public Property GridPadding() As Byte
			Get
				Return _GridPadding
			End Get
			Set
				_GridPadding = value
				Me.Invalidate()
			End Set
		End Property


		Private _SelectedIndex As Integer = -1
		Public Property SelectedIndex() As Integer
			Get
				Return _SelectedIndex
			End Get
			Set
				If _SelectedIndex <> value Then
					_SelectedIndex = value
					If _SelectedIndex <> -1 Then
						_Color = Items(_SelectedIndex).Color
					End If
					Me.Invalidate()

					OnSelectedIndexChange()
				End If
			End Set
		End Property

		Private _Color As Color
		Public Property Color() As Color
			Get
				Return _Color
			End Get
			Set
				_Color = value
				If value = Color.Empty Then
					Me.SelectedIndex = -1
				Else
					Me.SelectedIndex = IndexOf(value, True)
				End If
			End Set
		End Property


		Private _ClipColors As Boolean
		Public Property ClipColors() As Boolean
			Get
				Return _ClipColors
			End Get
			Set
				_ClipColors = value
			End Set
		End Property


		Private XOffset As Integer
        Private toolTip1 As System.Windows.Forms.ToolTip
        Private components As System.ComponentModel.IContainer
		Private YOffset As Integer



		Public Sub New()
			Me.InitializeComponent()
			Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
			Me.Items = New List(Of NamedColor)(36)
			Me.ttp = New ToolTip()
            Me.LoadDefaultColors(True)
        End Sub

		Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
			MyBase.OnMouseMove(e)
			Dim pos As Point = Me.PointToClient(Control.MousePosition)
			Dim index As Integer = IndexOf(pos)
			If index <> -1 AndAlso index <> hoverIndex Then
				hoverIndex = index
				toolTip1.SetToolTip(Me, Items(index).ToString())
			End If
		End Sub

		Protected Overrides Sub OnMouseEnter(e As EventArgs)
			MyBase.OnMouseEnter(e)
		End Sub

        Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
            MyBase.OnMouseClick(e)
            Dim index As Integer = IndexOf(e.Location)
            If index <> -1 Then
                SelectedIndex = index
            End If
        End Sub



        Protected Sub LoadDefaultColors(clearExistingColors As Boolean)
			If clearExistingColors Then
				Items.Clear()
			End If

			Items.Capacity = 36
			Dim arrStrHues As String() = New String() {"Red", "Yellow", "Green", "Cyan", "Blue", "Magenta"}

			Dim c As Unity3.Controls.ColorManager.HSL = New ColorManager.HSL()
			Dim increment As Single = 1F / 6
			Dim value As Single = 0
			'do the black to white colors first
			c.S = 0F

			value = 0
			While value <= 1F
				c.L = value
				If value = 0 OrElse value = 1F Then
					Items.Add(New NamedColor(ColorManager.HSL_to_RGB(c), If(value = 0, "Black", "White")))
				Else
					Items.Add(New NamedColor(ColorManager.HSL_to_RGB(c), "Black (" & Math.Round(value * 100F).ToString() & "% Light)"))
				End If
				value += increment
			End While
			increment = (1F - 0.3F) / 3F
			Dim sat_increment As Single = (0.7F - 0.1F) / 2F



			'now do the default colors
			c.H = 0F
			c.S = 1F
			Dim i As Integer = 0
			While i < arrStrHues.Length
				value = 0.3F
				While value <= 1F
					c.L = value
					Items.Add(New NamedColor(ColorManager.HSL_to_RGB(c), arrStrHues(i) & (If(value < 1F, " (" & Math.Round((1F - value) * 100F).ToString() & "% Dark)", ""))))
					value += increment
				End While

				c.L = 1F
				value = 0.7F
				While value >= 0.1F
					c.S = value
					Items.Add(New NamedColor(ColorManager.HSL_to_RGB(c), arrStrHues(i) & " (" & Math.Round((1F - value) * 100F).ToString() & "% Light)"))
					value -= sat_increment
				End While
				i += 1
				c.H += 0.16F
				c.S = 1F
			End While
		End Sub

		Public Event SelectedIndexChange As EventHandler
		Protected Sub OnSelectedIndexChange()
			RaiseEvent SelectedIndexChange(Nothing, Nothing)
		End Sub

		Public Function IndexOf(point As Point) As Integer
			point.Offset(XOffset, YOffset)
			Dim colorsPerLine As Integer = CInt(Me.Width \ (_GridSize.Width + _GridPadding))
			Dim column As Integer = CInt(Math.Truncate(Math.Round(CSng(point.X \ (_GridSize.Width + _GridPadding)))))
			Dim line As Integer = CInt(Math.Truncate(Math.Round(CSng(point.Y \ (_GridSize.Height + _GridPadding))))) + 1

			Dim index As Integer = colorsPerLine * (line - 1) + column
			Return If(index < Items.Count, index, -1)
		End Function

		Public Function IndexOf(color As Color, ignoreAlpha As Boolean) As Integer
			Dim intCount As Integer = Items.Count
			For i As Integer = 0 To intCount - 1
				If (If(ignoreAlpha, True, Items(i).Color.A = color.A)) AndAlso Items(i).Color.R = color.R AndAlso Items(i).Color.G = color.G AndAlso Items(i).Color.B = color.B Then
					Return i
				End If
			Next
			Return -1
		End Function


		Protected Overrides Sub OnPaint(e As PaintEventArgs)
			Dim minArea As Size = Me.Size
			Dim intCount As Integer = Items.Count

			e.Graphics.TranslateTransform(-XOffset, -YOffset)

			Dim colorsPerLine As Integer = CInt(minArea.Width \ (_GridSize.Width + _GridPadding))
			Dim startingLine As Integer = If(YOffset = 0, 0, CInt((_GridSize.Height + _GridPadding) \ YOffset))
			Dim index As Integer = startingLine * colorsPerLine

			Dim gridBounds As New Rectangle(_GridPadding, _GridPadding, _GridSize.Width, _GridSize.Height)


			Dim brush As New SolidBrush(Color.Black)
			While index < intCount
				If Items(index).Color.A <> 255 Then
					drawTransparencyGrid(e.Graphics, gridBounds)
				End If
				brush.Color = Items(index).Color
				'paint the color
				e.Graphics.FillRectangle(brush, gridBounds)


				e.Graphics.DrawRectangle(Pens.Gray, gridBounds)
				If index = _SelectedIndex Then
					Dim r As Rectangle = gridBounds
					r.Inflate(2, 2)
					e.Graphics.DrawRectangle(Pens.Blue, r)
				End If
				'update the gridBounds
				gridBounds.X += (_GridSize.Width + _GridPadding)
				If gridBounds.X + _GridSize.Width > minArea.Width Then
					gridBounds.X = If(XOffset = 0, _GridPadding, CInt((_GridSize.Width + _GridPadding) \ XOffset))
				End If

				If gridBounds.X = _GridPadding Then
					gridBounds.Y += (_GridSize.Height + _GridPadding)
					'if (gridBounds.Y + _GridSize.Width > minArea.Width)
					'    gridBounds.X = (int)((_GridSize.Width + _GridPadding) / Math.Abs(XOffset));

				End If
				index += 1
			End While
		End Sub

		Private Sub drawTransparencyGrid(g As System.Drawing.Graphics, area As Rectangle)
			Dim b As Boolean = False
			Dim r As New Rectangle(0, 0, 8, 8)
			g.FillRectangle(Brushes.White, area)
			r.Y = area.Y
			While r.Y < area.Bottom
				r.X = (If((InlineAssignHelper(b, Not b)), area.X, 8))
				While r.X < area.Right
					g.FillRectangle(Brushes.LightGray, r)
					r.X += 16
				End While
				r.Y += 8
			End While
		End Sub

		Private Sub InitializeComponent()
			Me.components = New System.ComponentModel.Container()
			Me.toolTip1 = New System.Windows.Forms.ToolTip(Me.components)
			Me.SuspendLayout()
			' 
			' ColorGrid
			' 
			Me.Name = "ColorGrid"
			Me.ResumeLayout(False)

		End Sub
		Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
			target = value
			Return value
		End Function

	End Class

	Public Structure NamedColor
		Private _Color As Color
		Public Property Color() As Color
			Get
				Return _Color
			End Get
			Set
				_Color = value
			End Set
		End Property

		Private _Name As String
		Public Property Name() As String
			Get
				Return _Name
			End Get
			Set
				_Name = value
			End Set
		End Property

		Public Sub New(color As Color, name As String)
			_Color = color
			_Name = name
		End Sub

		Public Overrides Function ToString() As String
			If Not String.IsNullOrEmpty(_Name) Then
				Return _Name
			ElseIf _Color.IsKnownColor Then
				Return _Color.ToKnownColor().ToString()
			Else
				Return GetFormattedColorString(_Color)
			End If
		End Function

		Public Shared Function GetColorName(color As Color) As String
			If color.A <> 255 Then
				Return GetFormattedColorString(color)
			End If

			If color.R = 0 AndAlso color.G = 0 AndAlso color.B = 0 Then
				Return "Black"
			End If
			If color.R = 255 AndAlso color.G = 0 AndAlso color.B = 0 Then
				Return "Red"
			End If
			If color.R = 0 AndAlso color.G = 255 AndAlso color.B = 0 Then
				Return "Green"
			End If
			If color.R = 0 AndAlso color.G = 0 AndAlso color.B = 255 Then
				Return "Blue"
			End If
			If color.R = 255 AndAlso color.G = 102 AndAlso color.B = 0 Then
				Return "Orange"
			End If


			Return GetFormattedColorString(color)
		End Function

		Public Shared Function GetFormattedColorString(color As Color) As String
			Return color.R & ", " & color.G & ", " & color.B
		End Function
	End Structure
End Namespace
