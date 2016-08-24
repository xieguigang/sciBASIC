#Region "Microsoft.VisualBasic::4daaeb79e27c29fa894f495c8194f1be, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ToolTips\Renderer\Popup.vb"

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

    Public Class Popup
        Public Shared ReadOnly Property PlacementBrush(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As SolidBrush
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New SolidBrush(Color.FromArgb(233, 238, 238))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(125, 158, 201))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property SeparatorBrush(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As SolidBrush
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New SolidBrush(Color.FromArgb(221, 231, 238))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(163, 188, 218))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property BackgroundBrush(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As SolidBrush
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New SolidBrush(Color.FromArgb(250, 250, 250))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(84, 84, 84))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property NormalTextBrush(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As SolidBrush
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue
                        Return New SolidBrush(Color.FromArgb(85, 119, 163))
                    Case Drawing.ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(255, 255, 255))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property DisabledTextBrush(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As SolidBrush
            Get
                Select Case theme
                    Case Drawing.ColorTheme.Blue, Drawing.ColorTheme.BlackBlue
                        Return New SolidBrush(Color.FromArgb(118, 118, 118))
                End Select
                Return Brushes.Black
            End Get
        End Property
        Public Shared ReadOnly Property SeparatorPen(Optional  theme As Drawing.ColorTheme = Drawing.ColorTheme.Blue) _
        As Pen
            Get
                Return New Pen(Color.FromArgb(197, 197, 197))
            End Get
        End Property
    End Class
End Namespace
