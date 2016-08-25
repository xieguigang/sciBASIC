#Region "Microsoft.VisualBasic::eef75bb575ac67f3ec75cf5486b334d1, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\DropDownList\Unity3.Controls\ColorManager.vb"

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

Imports System.Drawing

Namespace Unity3.Controls
	Public Class ColorManager
		#Region "Constructors / Destructors"

		Public Sub New()
		End Sub


		#End Region

		#Region "Public Methods"

		''' <summary> 
		''' Sets the absolute brightness of a colour 
		''' </summary> 
		''' <param name="c">Original colour</param> 
		''' <param name="brightness">The luminance level to impose</param> 
		''' <returns>an adjusted colour</returns> 
		Public Shared Function SetBrightness(c As Color, brightness As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.L = brightness
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Modifies an existing brightness level 
		''' </summary> 
		''' <remarks> 
		''' To reduce brightness use a number smaller than 1. To increase brightness use a number larger tnan 1 
		''' </remarks> 
		''' <param name="c">The original colour</param> 
		''' <param name="brightness">The luminance delta</param> 
		''' <returns>An adjusted colour</returns> 
		Public Shared Function ModifyBrightness(c As Color, brightness As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.L *= brightness
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Sets the absolute saturation level 
		''' </summary> 
		''' <remarks>Accepted values 0-1</remarks> 
		''' <param name="c">An original colour</param> 
		''' <param name="Saturation">The saturation value to impose</param> 
		''' <returns>An adjusted colour</returns> 
		Public Shared Function SetSaturation(c As Color, Saturation As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.S = Saturation
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Modifies an existing Saturation level 
		''' </summary> 
		''' <remarks> 
		''' To reduce Saturation use a number smaller than 1. To increase Saturation use a number larger tnan 1 
		''' </remarks> 
		''' <param name="c">The original colour</param> 
		''' <param name="Saturation">The saturation delta</param> 
		''' <returns>An adjusted colour</returns> 
		Public Shared Function ModifySaturation(c As Color, Saturation As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.S *= Saturation
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Sets the absolute Hue level 
		''' </summary> 
		''' <remarks>Accepted values 0-1</remarks> 
		''' <param name="c">An original colour</param> 
		''' <param name="Hue">The Hue value to impose</param> 
		''' <returns>An adjusted colour</returns> 
		Public Shared Function SetHue(c As Color, Hue As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.H = Hue
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Modifies an existing Hue level 
		''' </summary> 
		''' <remarks> 
		''' To reduce Hue use a number smaller than 1. To increase Hue use a number larger tnan 1 
		''' </remarks> 
		''' <param name="c">The original colour</param> 
		''' <param name="Hue">The Hue delta</param> 
		''' <returns>An adjusted colour</returns> 
		Public Shared Function ModifyHue(c As Color, Hue As Double) As Color
			Dim hsl As HSL = RGB_to_HSL(c)
			hsl.H *= Hue
			Return HSL_to_RGB(hsl)
		End Function


		''' <summary> 
		''' Converts a colour from HSL to RGB 
		''' </summary> 
		''' <remarks>Adapted from the algoritm in Foley and Van-Dam</remarks> 
		''' <param name="hsl">The HSL value</param> 
		''' <returns>A Color structure containing the equivalent RGB values</returns> 
		Public Shared Function HSL_to_RGB(hsl As HSL) As Color
			Dim Max As Integer, Mid As Integer, Min As Integer
			Dim q As Double

			Max = Round(hsl.L * 255)
			Min = Round((1.0 - hsl.S) * (hsl.L / 1.0) * 255)
			q = CDbl(Max - Min) / 255

			If hsl.H >= 0 AndAlso hsl.H <= CDbl(1) / 6 Then
				Mid = Round(((hsl.H - 0) * q) * 1530 + Min)
				Return Color.FromArgb(Max, Mid, Min)
			ElseIf hsl.H <= CDbl(1) / 3 Then
				Mid = Round(-((hsl.H - CDbl(1) / 6) * q) * 1530 + Max)
				Return Color.FromArgb(Mid, Max, Min)
			ElseIf hsl.H <= 0.5 Then
				Mid = Round(((hsl.H - CDbl(1) / 3) * q) * 1530 + Min)
				Return Color.FromArgb(Min, Max, Mid)
			ElseIf hsl.H <= CDbl(2) / 3 Then
				Mid = Round(-((hsl.H - 0.5) * q) * 1530 + Max)
				Return Color.FromArgb(Min, Mid, Max)
			ElseIf hsl.H <= CDbl(5) / 6 Then
				Mid = Round(((hsl.H - CDbl(2) / 3) * q) * 1530 + Min)
				Return Color.FromArgb(Mid, Min, Max)
			ElseIf hsl.H <= 1.0 Then
				Mid = Round(-((hsl.H - CDbl(5) / 6) * q) * 1530 + Max)
				Return Color.FromArgb(Max, Min, Mid)
			Else
				Return Color.FromArgb(0, 0, 0)
			End If
		End Function


		''' <summary> 
		''' Converts RGB to HSL 
		''' </summary> 
		''' <remarks>Takes advantage of whats already built in to .NET by using the Color.GetHue, Color.GetSaturation and Color.GetBrightness methods</remarks> 
		''' <param name="c">A Color to convert</param> 
		''' <returns>An HSL value</returns> 
		Public Shared Function RGB_to_HSL(c As Color) As HSL
			Dim hsl As New HSL()

			Dim Max As Integer, Min As Integer, Diff As Integer, Sum As Integer

			'	Of our RGB values, assign the highest value to Max, and the Smallest to Min
			If c.R > c.G Then
				Max = c.R
				Min = c.G
			Else
				Max = c.G
				Min = c.R
			End If
			If c.B > Max Then
				Max = c.B
			ElseIf c.B < Min Then
				Min = c.B
			End If

			Diff = Max - Min
			Sum = Max + Min

			'	Luminance - a.k.a. Brightness - Adobe photoshop uses the logic that the
			'	site VBspeed regards (regarded) as too primitive = superior decides the 
			'	level of brightness.
			hsl.L = CDbl(Max) / 255

			'	Saturation
			If Max = 0 Then
				hsl.S = 0
			Else
				'	Protecting from the impossible operation of division by zero.
				hsl.S = CDbl(Diff) / Max
			End If
			'	The logic of Adobe Photoshops is this simple.
			'	Hue		R is situated at the angel of 360 eller noll degrees; 
			'			G vid 120 degrees
			'			B vid 240 degrees
			Dim q As Double
			If Diff = 0 Then
				q = 0
			Else
				' Protecting from the impossible operation of division by zero.
				q = CDbl(60) / Diff
			End If

			If Max = c.R Then
				If c.G < c.B Then
					hsl.H = CDbl(360 + q * (c.G - c.B)) / 360
				Else
					hsl.H = CDbl(q * (c.G - c.B)) / 360
				End If
			ElseIf Max = c.G Then
				hsl.H = CDbl(120 + q * (c.B - c.R)) / 360
			ElseIf Max = c.B Then
				hsl.H = CDbl(240 + q * (c.R - c.G)) / 360
			Else
				hsl.H = 0.0
			End If

			Return hsl
		End Function


		''' <summary>
		''' Converts RGB to CMYK
		''' </summary>
		''' <param name="c">A color to convert.</param>
		''' <returns>A CMYK object</returns>
		Public Shared Function RGB_to_CMYK(c As Color) As CMYK
			Dim _cmyk As New CMYK()
			Dim low As Double = 1.0

			_cmyk.C = CDbl(255 - c.R) / 255
			If low > _cmyk.C Then
				low = _cmyk.C
			End If

			_cmyk.M = CDbl(255 - c.G) / 255
			If low > _cmyk.M Then
				low = _cmyk.M
			End If

			_cmyk.Y = CDbl(255 - c.B) / 255
			If low > _cmyk.Y Then
				low = _cmyk.Y
			End If

			If low > 0.0 Then
				_cmyk.K = low
			End If

			Return _cmyk
		End Function


		''' <summary>
		''' Converts CMYK to RGB
		''' </summary>
		''' <param name="_cmyk">A color to convert</param>
		''' <returns>A Color object</returns>
		Public Shared Function CMYK_to_RGB(_cmyk As CMYK) As Color
			Dim red As Integer, green As Integer, blue As Integer

			red = Round(255 - (255 * _cmyk.C))
			green = Round(255 - (255 * _cmyk.M))
			blue = Round(255 - (255 * _cmyk.Y))

			Return Color.FromArgb(red, green, blue)
		End Function


		''' <summary>
		''' Custom rounding function.
		''' </summary>
		''' <param name="val">Value to round</param>
		''' <returns>Rounded value</returns>
		Private Shared Function Round(val As Double) As Integer
			Dim ret_val As Integer = CInt(Math.Truncate(val))

			Dim temp As Integer = CInt(Math.Truncate(val * 100))

			If (temp Mod 100) >= 50 Then
				ret_val += 1
			End If

			Return ret_val
		End Function


		#End Region

		#Region "Public Classes"

		Public Class HSL
			#Region "Class Variables"

			Public Sub New()
				_h = 0
				_s = 0
				_l = 0
			End Sub

			Private _h As Double
			Private _s As Double
			Private _l As Double

			#End Region

			#Region "Public Methods"

			Public Property H() As Double
				Get
					Return _h
				End Get
				Set
					_h = value
					_h = If(_h > 1, 1, If(_h < 0, 0, _h))
				End Set
			End Property


			Public Property S() As Double
				Get
					Return _s
				End Get
				Set
					_s = value
					_s = If(_s > 1, 1, If(_s < 0, 0, _s))
				End Set
			End Property


			Public Property L() As Double
				Get
					Return _l
				End Get
				Set
					_l = value
					_l = If(_l > 1, 1, If(_l < 0, 0, _l))
				End Set
			End Property


			#End Region
		End Class


		Public Class CMYK
			#Region "Class Variables"

			Public Sub New()
				_c = 0
				_m = 0
				_y = 0
				_k = 0
			End Sub


			Private _c As Double
			Private _m As Double
			Private _y As Double
			Private _k As Double

			#End Region

			#Region "Public Methods"

			Public Property C() As Double
				Get
					Return _c
				End Get
				Set
					_c = value
					_c = If(_c > 1, 1, If(_c < 0, 0, _c))
				End Set
			End Property


			Public Property M() As Double
				Get
					Return _m
				End Get
				Set
					_m = value
					_m = If(_m > 1, 1, If(_m < 0, 0, _m))
				End Set
			End Property


			Public Property Y() As Double
				Get
					Return _y
				End Get
				Set
					_y = value
					_y = If(_y > 1, 1, If(_y < 0, 0, _y))
				End Set
			End Property


			Public Property K() As Double
				Get
					Return _k
				End Get
				Set
					_k = value
					_k = If(_k > 1, 1, If(_k < 0, 0, _k))
				End Set
			End Property


			#End Region
		End Class


		#End Region
	End Class
End Namespace
