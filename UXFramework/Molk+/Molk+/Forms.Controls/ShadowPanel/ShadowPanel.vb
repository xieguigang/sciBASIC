#Region "Microsoft.VisualBasic::19c5ac3f45b9c0c7d9af5aa0e00ba3e8, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ShadowPanel\ShadowPanel.vb"

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
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Transparent drop shadow in C# (GDI+)  
''' http://www.codeproject.com/Articles/19258/Transparent-drop-shadow-in-C-GDI
''' </summary>
''' <remarks></remarks>
Public Class ShadowPanel : Inherits Panel

    Public Property PanelColor As Color = Color.White
    'Public Property BorderColor As Color

    Const shadowSize As Integer = 5
    Const shadowMargin As Integer = 2

#Region "Static for good perfomance"

    Shared ReadOnly shadowDownRight As Image = My.Resources.tshadowdownright
    Shared ReadOnly shadowDownLeft As Image = My.Resources.tshadowdownleft
    Shared ReadOnly shadowDown As Image = My.Resources.tshadowdown
    Shared ReadOnly shadowRight As Image = My.Resources.tshadowright
    Shared ReadOnly shadowTopRight As Image = My.Resources.tshadowtopright
    Shared ReadOnly shadowTop As Image = My.Resources.tshadowtop
    Shared ReadOnly shadowTopLeft As Image = My.Resources.tshadowtopleft
    Shared ReadOnly shadowLeft As Image = My.Resources.tshadowleft

#End Region

    Public Sub New()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Call MyBase.OnPaint(e)

        ' Get the graphics object. We need something to draw with ;-)
        Dim g As Graphics = e.Graphics

        ' Create tiled brushes for the shadow on the right and at the bottom.
        Dim shadowRightBrush As New TextureBrush(shadowRight, WrapMode.Tile)
        Dim shadowDownBrush As New TextureBrush(shadowDown, WrapMode.Tile)
        Dim shadowTopBrush As New TextureBrush(shadowTop, WrapMode.Tile)
        Dim shadowLeftBrush As New TextureBrush(shadowLeft, WrapMode.Tile)

        ' Translate (move) the brushes so the top or left of the image matches the top or left of the
        ' area where it's drawed. If you don't understand why this is necessary, comment it out. 
        ' Hint: The tiling would start at 0,0 of the control, so the shadows will be offset a little.
        Call shadowDownBrush.TranslateTransform(0, Height - shadowSize)
        Call shadowRightBrush.TranslateTransform(Width - shadowSize, 0)
        Call shadowTopBrush.TranslateTransform(0, shadowSize)
        Call shadowLeftBrush.TranslateTransform(shadowSize, 0)

        ' Define the rectangles that will be filled with the brush.
        ' (where the shadow is drawn)
        ' X
        ' Y
        ' width (stretches)
        ' height
        Dim shadowDownRectangle As New Rectangle(shadowSize + shadowMargin, Height - shadowSize, Width - (shadowSize * 2 + shadowMargin), shadowSize)

        ' X
        ' Y
        ' width
        ' height (stretches)
        Dim shadowRightRectangle As New Rectangle(Width - shadowSize, shadowSize + shadowMargin, shadowSize, Height - (shadowSize * 2 + shadowMargin))
        Dim shadowTopRectangle As New Rectangle(shadowSize + shadowMargin, 0, Width - (shadowSize * 2 + shadowMargin), shadowSize)
        Dim shadowLeftRectangle As New Rectangle(0, shadowSize, shadowSize, Height - (shadowSize * 2))

        ' And draw the shadow on the right and at the bottom.
        Call g.FillRectangle(shadowDownBrush, shadowDownRectangle)
        Call g.FillRectangle(shadowRightBrush, shadowRightRectangle)
        Call g.FillRectangle(shadowTopBrush, shadowTopRectangle)
        Call g.FillRectangle(shadowLeftBrush, shadowLeftRectangle)

        ' Now for the corners, draw the 3 5x5 pixel images.
        Call g.DrawImage(shadowTopRight, New Rectangle(Width - shadowSize, shadowMargin, shadowSize, shadowSize))
        Call g.DrawImage(shadowDownRight, New Rectangle(Width - shadowSize, Height - shadowSize, shadowSize, shadowSize))
        Call g.DrawImage(shadowDownLeft, New Rectangle(shadowMargin, Height - shadowSize, shadowSize, shadowSize))
        Call g.DrawImage(shadowTopLeft, New Rectangle(shadowMargin, 0, shadowSize, shadowSize))

        ' Fill the area inside with the color in the PanelColor property.
        ' 1 pixel is added to everything to make the rectangle smaller. 
        ' This is because the 1 pixel border is actually drawn outside the rectangle.
        ' X
        ' Y
        ' Width
        ' Height
        Dim fullRectangle As New Rectangle(1, 1, Width - (shadowSize + 2), Height - (shadowSize + 2))

        If PanelColor <> Nothing Then
            Dim bgBrush As New SolidBrush(_PanelColor)
            Call g.FillRectangle(bgBrush, fullRectangle)
        End If

        ' Draw a nice 1 pixel border it a BorderColor is specified
        'If _BorderColor <> Nothing Then
        '    Dim borderPen As New Pen(BorderColor)
        '    Call g.DrawRectangle(borderPen, fullRectangle)
        'End If

        ' Memory efficiency
        Call shadowDownBrush.Dispose()
        Call shadowRightBrush.Dispose()

        shadowDownBrush = Nothing
        shadowRightBrush = Nothing
    End Sub

    ''' <summary>
    ''' Correct resizing
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnResize(e As EventArgs)
        Call MyBase.Invalidate()
        Call MyBase.OnResize(e)
    End Sub
End Class
