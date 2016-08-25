#Region "Microsoft.VisualBasic::141396ecb0473d4189cbf62c6d8c6e7d, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ToolTips\Renderer\Button.vb"

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
Imports System.Drawing.Drawing2D

Namespace Renderer

    ''' <summary>
    ''' Class for rendering button and menu item, just a little part that have supports for BlackBlue color theme.
    ''' </summary>
    Public Class Button
#Region "Enumeration"
        ''' <summary>
        ''' Enumeration to determine a split button location.
        ''' </summary>
        ''' <remarks><seealso cref="drawSplit"/></remarks>
        Public Enum SplitLocation
            Top
            Left
            Right
            Bottom
        End Enum
        ''' <summary>
        ''' Enumeration to determine where(e.g. highlited, pressed) an effect occurs.
        ''' </summary>
        ''' <remarks><seealso cref="drawSplit"/></remarks>
        Public Enum SplitEffectLocation
            None
            Button
            Split
        End Enum
#End Region
#Region "Color Blend"
        ''' <summary>
        ''' Represent a color blend on a disabled button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property DisabledBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim aBlend As ColorBlend
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                aBlend = New ColorBlend
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(244, 249, 251)
                        colors(1) = Color.FromArgb(173, 211, 230)
                        colors(2) = Color.FromArgb(145, 192, 217)
                        colors(3) = Color.FromArgb(213, 236, 247)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(102, 115, 124)
                        colors(1) = Color.FromArgb(76, 88, 104)
                        colors(2) = Color.FromArgb(51, 65, 81)
                        colors(3) = Color.FromArgb(35, 42, 61)
                End Select
                pos(0) = 0
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                aBlend.Colors = colors
                aBlend.Positions = pos
                Return aBlend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a normal button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property NormalBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(217, 240, 251)
                        colors(1) = Color.FromArgb(159, 212, 240)
                        colors(2) = Color.FromArgb(126, 188, 224)
                        colors(3) = Color.FromArgb(189, 233, 254)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(216, 216, 216)
                        colors(1) = Color.FromArgb(75, 77, 76)
                        colors(2) = Color.FromArgb(1, 3, 2)
                        colors(3) = Color.FromArgb(0, 0, 0)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a focused button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property FocusedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(220, 252, 255)
                        colors(1) = Color.FromArgb(124, 194, 236)
                        colors(2) = Color.FromArgb(91, 172, 220)
                        colors(3) = Color.FromArgb(190, 247, 255)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(11, 146, 255)
                        colors(1) = Color.FromArgb(75, 77, 76)
                        colors(2) = Color.FromArgb(1, 3, 2)
                        colors(3) = Color.FromArgb(0, 0, 0)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a highlited (mouse hover) button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property HLitedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(255, 253, 232)
                        colors(1) = Color.FromArgb(255, 235, 159)
                        colors(2) = Color.FromArgb(255, 213, 67)
                        colors(3) = Color.FromArgb(255, 222, 90)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(116, 193, 255)
                        colors(1) = Color.FromArgb(0, 46, 92)
                        colors(2) = Color.FromArgb(1, 2, 7)
                        colors(3) = Color.FromArgb(0, 0, 0)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a highlited button, but mouse didn't hover on the button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property HLitedLightBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(255, 254, 246)
                        colors(1) = Color.FromArgb(255, 248, 221)
                        colors(2) = Color.FromArgb(255, 241, 186)
                        colors(3) = Color.FromArgb(255, 243, 197)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(116, 193, 255)
                        colors(1) = Color.FromArgb(64, 110, 156)
                        colors(2) = Color.FromArgb(65, 66, 71)
                        colors(3) = Color.FromArgb(64, 64, 64)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a pressed button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property PressedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(244, 179, 120)
                        colors(1) = Color.FromArgb(253, 158, 67)
                        colors(2) = Color.FromArgb(253, 132, 18)
                        colors(3) = Color.FromArgb(254, 161, 80)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(60, 159, 212)
                        colors(1) = Color.FromArgb(34, 89, 133)
                        colors(2) = Color.FromArgb(17, 44, 82)
                        colors(3) = Color.FromArgb(39, 108, 148)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a selected button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property SelectedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(244, 221, 199)
                        colors(1) = Color.FromArgb(254, 200, 146)
                        colors(2) = Color.FromArgb(254, 161, 66)
                        colors(3) = Color.FromArgb(253, 229, 136)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(77, 101, 152)
                        colors(1) = Color.FromArgb(32, 77, 146)
                        colors(2) = Color.FromArgb(3, 58, 137)
                        colors(3) = Color.FromArgb(5, 90, 176)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend on a highlited selected button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>ColorBlend.</value>
        Public Shared ReadOnly Property SelectedHLiteBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 3) As Color
                Dim pos(0 To 3) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 0.4F
                pos(2) = 0.4F
                pos(3) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(253, 187, 106)
                        colors(1) = Color.FromArgb(252, 176, 89)
                        colors(2) = Color.FromArgb(250, 160, 47)
                        colors(3) = Color.FromArgb(252, 181, 17)
                    Case Drawing.ColorTheme.BlackBlue
                        colors(0) = Color.FromArgb(106, 148, 186)
                        colors(1) = Color.FromArgb(33, 102, 144)
                        colors(2) = Color.FromArgb(0, 81, 128)
                        colors(3) = Color.FromArgb(4, 169, 235)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
#End Region
#Region "Color Pen"
        ''' <summary>
        ''' Represent a border pen on a normal button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property NormalBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(141, 173, 194))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New Pen(Color.FromArgb(0, 0, 0))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen on a disabled button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property DisabledBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(161, 189, 207))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New Pen(Color.FromArgb(31, 31, 31))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen on a focused button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property FocusedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(121, 157, 182))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen on a highlited button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property HLitedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(219, 206, 153))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen on a selected button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property SelectedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(158, 130, 85))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a separator pen on a normal button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property NormalSeparatorPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(216, 194, 122))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New Pen(Color.FromArgb(0, 146, 198))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a separator pen on a highlited button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property HLitedSeparatorPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(205, 181, 131))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a separator pen on a selected button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property SelectedSeparatorPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(176, 145, 96))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a separator pen on a pressed button.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <value>Pen</value>
        Public Shared ReadOnly Property PressedSeparatorPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(168, 131, 86))
                End Select
                Return Pens.Black
            End Get
        End Property
#End Region
#Region "Glowing Color"
        ''' <summary>
        ''' Represent a glowing color on a normal button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property NormalGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(213, 236, 247)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(91, 95, 98)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a disabled button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property DisabledGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(244, 249, 251)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(115, 124, 132)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a focused button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property FocusedGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(189, 233, 254)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(60, 159, 180)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a highlited button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property HLitedGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(255, 235, 173)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(3, 143, 196)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a pressed button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property PressedGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(254, 160, 77)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(53, 156, 177)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a selected button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property SelectedGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(253, 241, 176)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(151, 240, 239)
                End Select
            End Get
        End Property
        ''' <summary>
        ''' Represent a glowing color on a selected highlited button.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <value>Color</value>
        Public Shared ReadOnly Property SelectedHLiteGlow(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) As Color
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return Color.FromArgb(251, 179, 15)
                    Case Drawing.ColorTheme.BlackBlue
                        Return Color.FromArgb(62, 187, 235)
                End Select
            End Get
        End Property
#End Region
#Region "Drawing"
        ''' <summary>
        ''' Draw a button on a Graphics object.
        ''' </summary>
        ''' <param name="g">Graphics object where the button to be drawn.</param>
        ''' <param name="rect">Bounding rectangle of the button.</param>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <param name="rounded">Rounded range of the corners of the rectangle.</param>
        ''' <param name="enabled">Determine whether the button is enabled.</param>
        ''' <param name="pressed">Determine whether the button is pressed.</param>
        ''' <param name="selected">Determine whether the button is selected.</param>
        ''' <param name="hlited">Determine whether the button is highlited.</param>
        ''' <param name="focused">Determine whether the button has input focus.</param>
        Public Shared Sub draw( g As Graphics,  rect As Rectangle, Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue,
        Optional  rounded As Integer = 2, Optional  enabled As Boolean = True,
        Optional  pressed As Boolean = False, Optional  selected As Boolean = False,
        Optional  hlited As Boolean = False, Optional  focused As Boolean = False)
            If g Is Nothing Then Return
            If rect.Width > 2 * rounded And rect.Width > 0 And rect.Height > 2 * rounded And rect.Height > 0 Then
                Dim btnPath As GraphicsPath = Drawing.roundedRectangle(rect, rounded, rounded, rounded, rounded)
                Dim borderPen As Pen = Nothing
                Dim glowColor As Color
                Dim shadowPath As GraphicsPath = Nothing
                Dim bgBrush As LinearGradientBrush = New LinearGradientBrush(rect, Color.Black, Color.White, LinearGradientMode.Vertical)
                If enabled Then
                    If pressed Then
                        bgBrush.InterpolationColors = PressedBlend(theme)
                        glowColor = PressedGlow(theme)
                        shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                    Else
                        If selected Then
                            If hlited Then
                                bgBrush.InterpolationColors = SelectedHLiteBlend(theme)
                                glowColor = SelectedHLiteGlow(theme)
                                shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                            Else
                                bgBrush.InterpolationColors = SelectedBlend(theme)
                                glowColor = SelectedGlow(theme)
                                borderPen = SelectedBorderPen(theme)
                                shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                            End If
                        Else
                            If hlited Then
                                bgBrush.InterpolationColors = HLitedBlend(theme)
                                glowColor = HLitedGlow(theme)
                                borderPen = HLitedBorderPen(theme)
                            Else
                                If focused Then
                                    bgBrush.InterpolationColors = FocusedBlend(theme)
                                    glowColor = FocusedGlow(theme)
                                    borderPen = FocusedBorderPen(theme)
                                Else
                                    bgBrush.InterpolationColors = NormalBlend(theme)
                                    glowColor = NormalGlow(theme)
                                    borderPen = NormalBorderPen(theme)
                                End If
                            End If
                        End If
                    End If
                Else
                    bgBrush.InterpolationColors = DisabledBlend(theme)
                    glowColor = DisabledGlow(theme)
                    borderPen = DisabledBorderPen(theme)
                End If
                Dim glowPath As GraphicsPath
                If rounded >= 0 Then
                    glowPath = Drawing.getGlowingPath(New Rectangle(rect.X + rounded, rect.Y + (0.4 * rect.Height),
                    rect.Width - (2 * rounded), 0.6 * rect.Height), Drawing.LightingGlowPoint.BottomCenter)
                Else
                    glowPath = Drawing.getGlowingPath(New Rectangle(rect.X, rect.Y + (0.4 * rect.Height),
                    rect.Width, 0.6 * rect.Height), Drawing.LightingGlowPoint.BottomCenter)
                End If
                Dim glowBrush As PathGradientBrush = New PathGradientBrush(glowPath)
                Dim sColors(0 To 1) As Color
                sColors(0) = Color.Transparent
                sColors(1) = Color.Transparent
                glowBrush.CenterColor = glowColor
                glowBrush.CenterPoint = New PointF(rect.X + (rect.Width / 2), rect.Bottom - 2)
                glowBrush.SurroundColors = sColors
                g.FillPath(bgBrush, btnPath)
                g.FillPath(glowBrush, glowPath)
                If shadowPath IsNot Nothing Then
                    Dim shadowBrush As LinearGradientBrush = New LinearGradientBrush(New Rectangle(rect.X, rect.Y, rect.Width, 5),
                    Color.FromArgb(63, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), LinearGradientMode.Vertical)
                    g.FillPath(shadowBrush, shadowPath)
                    shadowBrush.Dispose()
                    shadowPath.Dispose()
                End If
                If borderPen IsNot Nothing Then
                    g.DrawPath(borderPen, btnPath)
                    borderPen.Dispose()
                End If
                glowBrush.Dispose()
                glowPath.Dispose()
                btnPath.Dispose()
                bgBrush.Dispose()
            End If
        End Sub
        ''' <summary>
        ''' Draw a split button on a Graphics object.
        ''' </summary>
        ''' <param name="g">Graphics object where the button to be drawn.</param>
        ''' <param name="rect">Bounding rectangle of the button.</param>
        ''' <param name="split">Split location of the split button.</param>
        ''' <param name="splitSize">Size of the split.</param>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <param name="rounded">Rounded range of the corners of the rectangle.</param>
        ''' <param name="enabled">Determine whether the button is enabled.</param>
        ''' <param name="pressed">Determine where the pressed state takes effect.</param>
        ''' <param name="selected">Determine whether the button is selected.</param>
        ''' <param name="hlited">Determine where the highlited state takes effect.</param>
        ''' <param name="focused">Determine whether the button has input focus.</param>
        Public Shared Sub drawSplit( g As Graphics,  rect As Rectangle,
         split As SplitLocation,  splitSize As Integer,
        Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue,
        Optional  rounded As Integer = 2, Optional  enabled As Boolean = True,
        Optional  pressed As SplitEffectLocation = SplitEffectLocation.None,
        Optional  selected As Boolean = False,
        Optional  hlited As SplitEffectLocation = SplitEffectLocation.None,
        Optional  focused As Boolean = False)
            If g Is Nothing Then Return
            If rounded < 0 Then rounded = 0
            Select Case split
                Case SplitLocation.Bottom, SplitLocation.Top
                    If rect.Height <= splitSize + rounded Then Return
                Case SplitLocation.Left, SplitLocation.Right
                    If rect.Width <= splitSize + rounded Then Return
            End Select
            If splitSize <= rounded Then Return
            ' Creating path
            Dim splitRect As Rectangle
            Dim btnPath As GraphicsPath, splitPath As GraphicsPath = Nothing
            btnPath = Drawing.roundedRectangle(rect, rounded, rounded, rounded, rounded)
            With rect
                Select Case split
                    Case SplitLocation.Top
                        splitRect = New Rectangle(.X, .Y, .Width, splitSize)
                        splitPath = Drawing.roundedRectangle(splitRect, rounded, rounded, 0, 0)
                    Case SplitLocation.Left
                        splitRect = New Rectangle(.X, .Y, splitSize, .Height)
                        splitPath = Drawing.roundedRectangle(splitRect, rounded, 0, rounded, 0)
                    Case SplitLocation.Right
                        splitRect = New Rectangle(.Right - splitSize, .Y, splitSize, .Height)
                        splitPath = Drawing.roundedRectangle(splitRect, 0, rounded, 0, rounded)
                    Case SplitLocation.Bottom
                        splitRect = New Rectangle(.X, .Bottom - splitSize, .Width, splitSize)
                        splitPath = Drawing.roundedRectangle(splitRect, 0, 0, rounded, rounded)
                End Select
            End With
            ' Creating background brush
            Dim btnBrush As LinearGradientBrush = New LinearGradientBrush(rect, Color.Black, Color.White,
            LinearGradientMode.Vertical)
            Dim splitBrush As LinearGradientBrush = Nothing
            Dim shadowPath As GraphicsPath = Nothing
            Dim borderPen As Pen = Nothing
            Dim glowColor As Color
            Dim separatorPen As Pen = Nothing
            If enabled Then
                If pressed <> SplitEffectLocation.None Then
                    splitBrush = New LinearGradientBrush(rect, Color.Black, Color.White, LinearGradientMode.Vertical)
                    If pressed = SplitEffectLocation.Button Then
                        btnBrush.InterpolationColors = PressedBlend(theme)
                        splitBrush.InterpolationColors = HLitedLightBlend(theme)
                        shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                        glowColor = PressedGlow(theme)
                    Else
                        btnBrush.InterpolationColors = HLitedLightBlend(theme)
                        splitBrush.InterpolationColors = PressedBlend(theme)
                        glowColor = PressedGlow(theme)
                        Select Case split
                            Case SplitLocation.Top
                                shadowPath = Drawing.getInnerShadowPath(splitRect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded)
                            Case SplitLocation.Left
                                shadowPath = Drawing.getInnerShadowPath(splitRect, Drawing.ShadowPoint.Top, 4, 4, rounded, , rounded)
                            Case SplitLocation.Right
                                shadowPath = Drawing.getInnerShadowPath(splitRect, Drawing.ShadowPoint.Top, 4, 4, , rounded, , rounded)
                        End Select
                    End If
                    separatorPen = PressedSeparatorPen(theme)
                Else
                    If selected Then
                        If hlited <> SplitEffectLocation.None Then
                            splitBrush = New LinearGradientBrush(rect, Color.Black, Color.White, LinearGradientMode.Vertical)
                            If hlited = SplitEffectLocation.Button Then
                                btnBrush.InterpolationColors = SelectedHLiteBlend(theme)
                                splitBrush.InterpolationColors = HLitedLightBlend(theme)
                                glowColor = SelectedHLiteGlow(theme)
                                shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                            Else
                                btnBrush.InterpolationColors = SelectedHLiteBlend(theme)
                                splitBrush.InterpolationColors = HLitedBlend(theme)
                                glowColor = SelectedHLiteGlow(theme)
                                shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                            End If
                        Else
                            btnBrush.InterpolationColors = SelectedBlend(theme)
                            glowColor = SelectedGlow(theme)
                            shadowPath = Drawing.getInnerShadowPath(rect, Drawing.ShadowPoint.Top, 4, 4, rounded, rounded, rounded, rounded)
                        End If
                        separatorPen = SelectedSeparatorPen(theme)
                    Else
                        If hlited <> SplitEffectLocation.None Then
                            splitBrush = New LinearGradientBrush(rect, Color.Black, Color.White, LinearGradientMode.Vertical)
                            If hlited = SplitEffectLocation.Button Then
                                btnBrush.InterpolationColors = HLitedBlend(theme)
                                splitBrush.InterpolationColors = HLitedLightBlend(theme)
                                glowColor = HLitedGlow(theme)
                                separatorPen = HLitedSeparatorPen(theme)
                            Else
                                btnBrush.InterpolationColors = HLitedLightBlend(theme)
                                splitBrush.InterpolationColors = HLitedBlend(theme)
                                glowColor = HLitedGlow(theme)
                                separatorPen = HLitedSeparatorPen(theme)
                            End If
                            borderPen = HLitedBorderPen(theme)
                        Else
                            If focused Then
                                btnBrush.InterpolationColors = FocusedBlend(theme)
                                glowColor = FocusedGlow(theme)
                                borderPen = FocusedBorderPen(theme)
                            Else
                                btnBrush.InterpolationColors = NormalBlend(theme)
                                glowColor = NormalGlow(theme)
                                borderPen = NormalBorderPen(theme)
                            End If
                        End If
                    End If
                End If
            Else
                btnBrush.InterpolationColors = DisabledBlend(theme)
                glowColor = DisabledGlow(theme)
                borderPen = DisabledBorderPen(theme)
            End If
            Dim glowPath As GraphicsPath
            If rounded >= 0 Then
                glowPath = Drawing.getGlowingPath(New Rectangle(rect.X + rounded, rect.Y + (0.4 * rect.Height),
                rect.Width - (2 * rounded), 0.6 * rect.Height), Drawing.LightingGlowPoint.BottomCenter)
            Else
                glowPath = Drawing.getGlowingPath(New Rectangle(rect.X, rect.Y + (0.4 * rect.Height),
                rect.Width, 0.6 * rect.Height), Drawing.LightingGlowPoint.BottomCenter)
            End If
            Dim glowBrush As PathGradientBrush = New PathGradientBrush(glowPath)
            Dim sColors(0 To 1) As Color
            sColors(0) = Color.Transparent
            sColors(1) = Color.Transparent
            glowBrush.CenterColor = glowColor
            glowBrush.CenterPoint = New PointF(rect.X + (rect.Width / 2), rect.Bottom - 2)
            glowBrush.SurroundColors = sColors
            g.FillPath(btnBrush, btnPath)
            If pressed = SplitEffectLocation.Split Then
                If splitBrush IsNot Nothing Then
                    g.FillPath(splitBrush, splitPath)
                    splitBrush.Dispose()
                    splitBrush = Nothing
                End If
            End If
            If shadowPath IsNot Nothing Then
                Dim shadowBrush As LinearGradientBrush = New LinearGradientBrush(New Rectangle(rect.X, rect.Y, rect.Width, 5),
                Color.FromArgb(63, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), LinearGradientMode.Vertical)
                g.FillPath(shadowBrush, shadowPath)
                shadowBrush.Dispose()
                shadowPath.Dispose()
            End If
            If splitBrush IsNot Nothing Then
                g.FillPath(splitBrush, splitPath)
                splitBrush.Dispose()
                splitBrush = Nothing
            End If
            g.FillPath(glowBrush, glowPath)
            ' Drawing separator line
            If separatorPen IsNot Nothing Then
                Dim p1 As Point, p2 As Point
                Select Case split
                    Case SplitLocation.Top
                        p1 = New Point(splitRect.X + 2, splitRect.Bottom - 1)
                        p2 = New Point(splitRect.Right - 3, splitRect.Bottom - 1)
                    Case SplitLocation.Bottom
                        p1 = New Point(splitRect.X + 2, splitRect.Y)
                        p2 = New Point(splitRect.Right - 3, splitRect.Y)
                    Case SplitLocation.Left
                        p1 = New Point(splitRect.Right - 1, splitRect.Y + 2)
                        p2 = New Point(splitRect.Right - 1, splitRect.Bottom - 3)
                    Case SplitLocation.Right
                        p1 = New Point(splitRect.X, splitRect.Y + 2)
                        p2 = New Point(splitRect.X, splitRect.Bottom - 3)
                End Select
                g.DrawLine(separatorPen, p1, p2)
                separatorPen.Dispose()
            End If
            If borderPen IsNot Nothing Then
                g.DrawPath(borderPen, btnPath)
                borderPen.Dispose()
            End If
            glowBrush.Dispose()
            glowPath.Dispose()
            btnPath.Dispose()
            splitPath.Dispose()
            btnBrush.Dispose()
        End Sub
#End Region
    End Class

End Namespace
