#Region "Microsoft.VisualBasic::f670e7b0aae96b896c75027ff35cd808, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\WindowsVista\WindowsVistaColorTable.vb"

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
Imports System.Drawing

''' <summary>
''' Provides colors used by WindowsVista style rendering
''' </summary>
''' <remarks>
''' 2007 José Manuel Menéndez Poo
''' Visit my blog for upgrades and other renderers - www.menendezpoo.com
''' </remarks>
Public Class WindowsVistaColorTable
	#Region "Fields"
	Private _bgNorth As Color
	Private _bgSouth As Color
	Private _glossyNorth As Color
	Private _glossySouth As Color
	Private _bgborder As Color
	Private _bgglow As Color
	Private _text As Color
	Private _buttonInnerBorder As Color
	Private _buttonBorder As Color
	Private _buttonOuterBorder As Color
	Private _buttonFill As Color
	Private _buttonFillPressed As Color
	Private _glow As Color
	Private _buttonInnerBorderPressed As Color
	Private _buttonFillSouth As Color
	Private _buttonFillSouthPressed As Color
	Private _dropDownArrow As Color
	Private _menuHighlight As Color
	Private _menuHiglightNorth As Color
	Private _menuHighlightSouth As Color
	Private _menuBackground As Color
	Private _menuDark As Color
	Private _menuLight As Color
	Private _separatorNorth As Color
	Private _separatorSouth As Color
	Private _menuText As Color
	Private _checkedGlow As Color
	Private _checkedButtonFill As Color
	Private _checkedButtonFillHot As Color
	Private _checkedGlowHot As Color

	#End Region

	#Region "Ctor"

	Public Sub New()
		BackgroundNorth = Color.Black
		BackgroundSouth = Color.Black

		GlossyEffectNorth = Color.FromArgb(217, &H68, &H7c, &Hac)
		GlossyEffectSouth = Color.FromArgb(74, &Haa, &Hb5, &Hd0)

		BackgroundBorder = Color.FromArgb(&H85, &H85, &H87)
		BackgroundGlow = Color.FromArgb(&H43, &H53, &H7a)

		Text = Color.White

		ButtonOuterBorder = Color.FromArgb(&H75, &H7d, &H95)
		ButtonInnerBorder = Color.FromArgb(&Hbf, &Hc4, &Hce)
		ButtonInnerBorderPressed = Color.FromArgb(&H4b, &H4b, &H4b)
		ButtonBorder = Color.FromArgb(&H3, &H7, &Hd)
		ButtonFillNorth = Color.FromArgb(85, Color.White)
		ButtonFillSouth = Color.FromArgb(1, Color.White)
		ButtonFillNorthPressed = Color.FromArgb(150, Color.Black)
		ButtonFillSouthPressed = Color.FromArgb(100, Color.Black)

		Glow = Color.FromArgb(&H30, &H73, &Hce)
		DropDownArrow = Color.White

		MenuHighlight = Color.FromArgb(&Ha8, &Hd8, &Heb)
		MenuHighlightNorth = Color.FromArgb(25, MenuHighlight)
		MenuHighlightSouth = Color.FromArgb(102, MenuHighlight)
		MenuBackground = Color.FromArgb(&Hf1, &Hf1, &Hf1)
		MenuDark = Color.FromArgb(&He2, &He3, &He3)
		MenuLight = Color.White

		SeparatorNorth = BackgroundSouth
		SeparatorSouth = GlossyEffectNorth

		MenuText = Color.Black

		CheckedGlow = Color.FromArgb(&H57, &Hc6, &Hef)
		CheckedGlowHot = Color.FromArgb(&H70, &Hd4, &Hff)
		CheckedButtonFill = Color.FromArgb(&H18, &H38, &H9e)

		CheckedButtonFillHot = Color.FromArgb(&Hf, &H3a, &Hbf)
	End Sub

	#End Region

	#Region "Properties"


	Public Property CheckedGlowHot() As Color
		Get
			Return _checkedGlowHot
		End Get
		Set
			_checkedGlowHot = value
		End Set
	End Property


	Public Property CheckedButtonFillHot() As Color
		Get
			Return _checkedButtonFillHot
		End Get
		Set
			_checkedButtonFillHot = value
		End Set
	End Property


	Public Property CheckedButtonFill() As Color
		Get
			Return _checkedButtonFill
		End Get
		Set
			_checkedButtonFill = value
		End Set
	End Property


	Public Property CheckedGlow() As Color
		Get
			Return _checkedGlow
		End Get
		Set
			_checkedGlow = value
		End Set
	End Property


	Public Property MenuText() As Color
		Get
			Return _menuText
		End Get
		Set
			_menuText = value
		End Set
	End Property


	Public Property SeparatorNorth() As Color
		Get
			Return _separatorNorth
		End Get
		Set
			_separatorNorth = value
		End Set
	End Property


	Public Property SeparatorSouth() As Color
		Get
			Return _separatorSouth
		End Get
		Set
			_separatorSouth = value
		End Set
	End Property


	Public Property MenuLight() As Color
		Get
			Return _menuLight
		End Get
		Set
			_menuLight = value
		End Set
	End Property


	Public Property MenuDark() As Color
		Get
			Return _menuDark
		End Get
		Set
			_menuDark = value
		End Set
	End Property


	Public Property MenuBackground() As Color
		Get
			Return _menuBackground
		End Get
		Set
			_menuBackground = value
		End Set
	End Property


	Public Property MenuHighlightSouth() As Color
		Get
			Return _menuHighlightSouth
		End Get
		Set
			_menuHighlightSouth = value
		End Set
	End Property


	Public Property MenuHighlightNorth() As Color
		Get
			Return _menuHiglightNorth
		End Get
		Set
			_menuHiglightNorth = value
		End Set
	End Property


	Public Property MenuHighlight() As Color
		Get
			Return _menuHighlight
		End Get
		Set
			_menuHighlight = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the color for the dropwown arrow
	''' </summary>
	Public Property DropDownArrow() As Color
		Get
			Return _dropDownArrow
		End Get
		Set
			_dropDownArrow = value
		End Set
	End Property


	''' <summary>
	''' Gets or sets the south color of the button fill when pressed
	''' </summary>
	Public Property ButtonFillSouthPressed() As Color
		Get
			Return _buttonFillSouthPressed
		End Get
		Set
			_buttonFillSouthPressed = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the south color of the button fill
	''' </summary>
	Public Property ButtonFillSouth() As Color
		Get
			Return _buttonFillSouth
		End Get
		Set
			_buttonFillSouth = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the color of the inner border when pressed
	''' </summary>
	Public Property ButtonInnerBorderPressed() As Color
		Get
			Return _buttonInnerBorderPressed
		End Get
		Set
			_buttonInnerBorderPressed = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the glow color
	''' </summary>
	Public Property Glow() As Color
		Get
			Return _glow
		End Get
		Set
			_glow = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the buttons fill color
	''' </summary>
	Public Property ButtonFillNorth() As Color
		Get
			Return _buttonFill
		End Get
		Set
			_buttonFill = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the buttons fill color when pressed
	''' </summary>
	Public Property ButtonFillNorthPressed() As Color
		Get
			Return _buttonFillPressed
		End Get
		Set
			_buttonFillPressed = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the buttons inner border color
	''' </summary>
	Public Property ButtonInnerBorder() As Color
		Get
			Return _buttonInnerBorder
		End Get
		Set
			_buttonInnerBorder = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the buttons border color
	''' </summary>
	Public Property ButtonBorder() As Color
		Get
			Return _buttonBorder
		End Get
		Set
			_buttonBorder = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the buttons outer border color
	''' </summary>
	Public Property ButtonOuterBorder() As Color
		Get
			Return _buttonOuterBorder
		End Get
		Set
			_buttonOuterBorder = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the color of the text
	''' </summary>
	Public Property Text() As Color
		Get
			Return _text
		End Get
		Set
			_text = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the background glow color
	''' </summary>
	Public Property BackgroundGlow() As Color
		Get
			Return _bgglow
		End Get
		Set
			_bgglow = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the color of the background border
	''' </summary>
	Public Property BackgroundBorder() As Color
		Get
			Return _bgborder
		End Get
		Set
			_bgborder = value
		End Set
	End Property

	''' <summary>
	''' Background north part
	''' </summary>
	Public Property BackgroundNorth() As Color
		Get
			Return _bgNorth
		End Get
		Set
			_bgNorth = value
		End Set
	End Property

	''' <summary>
	''' Background south color
	''' </summary>
	Public Property BackgroundSouth() As Color
		Get
			Return _bgSouth
		End Get
		Set
			_bgSouth = value
		End Set
	End Property

	''' <summary>
	''' Gets ors sets the glossy effect north color
	''' </summary>
	Public Property GlossyEffectNorth() As Color
		Get
			Return _glossyNorth
		End Get
		Set
			_glossyNorth = value
		End Set
	End Property

	''' <summary>
	''' Gets or sets the glossy effect south color
	''' </summary>
	Public Property GlossyEffectSouth() As Color
		Get
			Return _glossySouth
		End Get
		Set
			_glossySouth = value
		End Set
	End Property


	#End Region
End Class
