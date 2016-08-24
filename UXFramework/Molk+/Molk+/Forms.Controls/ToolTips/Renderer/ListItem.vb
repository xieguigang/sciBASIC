#Region "Microsoft.VisualBasic::57a9ba714bf45622e9b98007e2b1da52, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ToolTips\Renderer\ListItem.vb"

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
    ''' Class for rendering list item.
    ''' </summary>
    Public Class ListItem
#Region "Color Blend"
        ''' <summary>
        ''' Represent a color blend for selected item in a list that lost it focus input.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedBlurBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(248, 248, 248)
                        colors(1) = Color.FromArgb(229, 229, 229)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for selected item in focused list.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(239, 246, 252)
                        colors(1) = Color.FromArgb(212, 239, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for selected and highlighted item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property SelectedHLiteBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(236, 245, 255)
                        colors(1) = Color.FromArgb(208, 229, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for highlighted item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property HLitedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(252, 253, 255)
                        colors(1) = Color.FromArgb(237, 245, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
        ''' <summary>
        ''' Represent a color blend for pressed item.
        ''' </summary>
        ''' <param name="theme">Them used to paint.</param>
        ''' <returns>ColorBlend.</returns>
        Public Shared ReadOnly Property PressedBlend(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As ColorBlend
            Get
                Dim colors(0 To 1) As Color
                Dim pos(0 To 1) As Single
                Dim blend As ColorBlend = New ColorBlend
                pos(0) = 0.0F
                pos(1) = 1.0F
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        colors(0) = Color.FromArgb(160, 189, 227)
                        colors(1) = Color.FromArgb(255, 255, 255)
                End Select
                blend.Colors = colors
                blend.Positions = pos
                Return blend
            End Get
        End Property
#End Region
#Region "Border Pen"
        ''' <summary>
        ''' Represent a border pen for selected item in a list that lost it focus input.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedBlurBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(217, 217, 217))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for selected item in a focused list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(177, 217, 229))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for highlighted item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property HLitedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(185, 215, 252))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for selected and highlighted item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property SelectedHLiteBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(132, 172, 221))
                End Select
                Return Pens.Black
            End Get
        End Property
        ''' <summary>
        ''' Represent a border pen for pressed item in a list.
        ''' </summary>
        ''' <param name="theme">Theme used to paint.</param>
        ''' <returns>Pen.</returns>
        Public Shared ReadOnly Property PressedBorderPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New Pen(Color.FromArgb(104, 140, 175))
                End Select
                Return Pens.Black
            End Get
        End Property
#End Region
    End Class
End Namespace
