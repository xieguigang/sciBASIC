#Region "Microsoft.VisualBasic::419d7f3ccdb3ca94fbc7d52c78545778, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ListControl\ListControlItem.vb"

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

Imports System.Drawing.Drawing2D

Public Class ListControlItem

    Public Event SelectionChanged(sender As Object)

    ''' <summary>
    ''' 与这个入口点相关的对象标签页的哈希句柄
    ''' </summary>
    ''' <returns></returns>
    Public Property BindDataHandle As Integer

    Friend WithEvents tmrMouseLeave As New System.Windows.Forms.Timer With {.Interval = 10}

#Region "Properties"

    Dim _InternalIconImage As Image
    Public Overridable Property Image() As Image
        Get
            Return _InternalIconImage
        End Get
        Set(value As Image)
            _InternalIconImage = value
            Call Refresh()
        End Set
    End Property

    Protected _Selected As Boolean
    Public Overridable Property Selected() As Boolean
        Get
            Return _Selected
        End Get
        Set(value As Boolean)
            _Selected = value
            Refresh()
        End Set
    End Property

    Dim _MetroColorSchema As MetroColorSchemes = Nothing

    Public Property MetroColorSchema As MetroColorSchemes
        Get
            Return _MetroColorSchema
        End Get
        Set(value As MetroColorSchemes)
            If value Is Nothing Then
                Return
            End If

            _MetroColorSchema = value
            MyBase.BackColor = value.UnSelectedNormal(0)
        End Set
    End Property

#End Region

#Region "Mouse coding"

    Private Enum MouseCapture
        Outside
        Inside
    End Enum
    Private Enum ButtonState
        ButtonUp
        ButtonDown
        Disabled
    End Enum

    Dim bState As ButtonState
    Dim bMouse As MouseCapture

    Private Sub ListControlItem_MouseClick(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseClick
        If Selected = False Then
            Selected = True
            RaiseEvent SelectionChanged(Me)
        End If
    End Sub

    Private Sub metroRadioGroup_MouseDown(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown ', rdButton.MouseDown
        bState = ButtonState.ButtonDown
        Refresh()
    End Sub

    Private Sub metroRadioGroup_MouseEnter(sender As Object, e As System.EventArgs) Handles Me.MouseEnter
        bMouse = MouseCapture.Inside
        tmrMouseLeave.Start()
        Refresh()
    End Sub

    Private Sub metroRadioGroup_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp ', rdButton.MouseUp
        bState = ButtonState.ButtonUp
        Refresh()
    End Sub

    Private Sub tmrMouseLeave_Tick(sender As System.Object, e As System.EventArgs) Handles tmrMouseLeave.Tick
        Dim scrPT = Control.MousePosition
        Dim ctlPT As Point = Me.PointToClient(scrPT)
        '
        If ctlPT.X < 0 Or ctlPT.Y < 0 Or ctlPT.X > Me.Width Or ctlPT.Y > Me.Height Then
            ' Stop timer
            tmrMouseLeave.Stop()
            bMouse = MouseCapture.Outside
            Refresh()
        Else
            bMouse = MouseCapture.Inside
        End If
    End Sub
#End Region

#Region "Painting"

    Private Sub InternalAssignAeroColorSchema(ByRef ColorScheme As Color(), ByRef brdr As SolidBrush)
        If bState = ButtonState.Disabled Then
            ' normal
            brdr = AeroColorSchemes.DisabledBorder
            ColorScheme = AeroColorSchemes.DisabledAllColor
        Else
            If _Selected Then
                ' Selected
                brdr = AeroColorSchemes.SelectedBorder

                If bState = ButtonState.ButtonUp And bMouse = MouseCapture.Outside Then
                    ' normal
                    ColorScheme = AeroColorSchemes.SelectedNormal

                ElseIf bState = ButtonState.ButtonUp And bMouse = MouseCapture.Inside Then
                    '  hover 
                    ColorScheme = AeroColorSchemes.SelectedHover

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Outside Then
                    ' no one cares!

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Inside Then
                    ' pressed
                    ColorScheme = AeroColorSchemes.SelectedPressed
                End If

            Else
                ' Not selected
                brdr = AeroColorSchemes.UnSelectedBorder

                If bState = ButtonState.ButtonUp And bMouse = MouseCapture.Outside Then
                    ' normal
                    brdr = AeroColorSchemes.DisabledBorder
                    ColorScheme = AeroColorSchemes.UnSelectedNormal

                ElseIf bState = ButtonState.ButtonUp And bMouse = MouseCapture.Inside Then
                    '  hover 
                    ColorScheme = AeroColorSchemes.UnSelectedHover

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Outside Then
                    ' no one cares!

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Inside Then
                    ' pressed
                    ColorScheme = AeroColorSchemes.UnSelectedPressed
                End If

            End If
        End If
    End Sub

    Private Sub InternalAssignColorScheme(ByRef ColorScheme As Color(), ByRef brdr As SolidBrush)
        If Not Me.MetroColorSchema Is Nothing Then
            Call InternalAssignMetroColorSchema(ColorScheme, brdr)
        Else
            Call InternalAssignAeroColorSchema(ColorScheme, brdr)
        End If
    End Sub

    Private Sub InternalAssignMetroColorSchema(ByRef ColorScheme As Color(), ByRef brdr As SolidBrush)
        If bState = ButtonState.Disabled Then
            ' normal
            brdr = MetroColorSchema.DisabledBorder
            ColorScheme = MetroColorSchema.DisabledAllColor
        Else
            If _Selected Then
                ' Selected
                brdr = MetroColorSchema.SelectedBorder

                If bState = ButtonState.ButtonUp And bMouse = MouseCapture.Outside Then
                    ' normal
                    ColorScheme = MetroColorSchema.SelectedNormal

                ElseIf bState = ButtonState.ButtonUp And bMouse = MouseCapture.Inside Then
                    '  hover 
                    ColorScheme = MetroColorSchema.SelectedHover

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Outside Then
                    ' no one cares!

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Inside Then
                    ' pressed
                    ColorScheme = MetroColorSchema.SelectedPressed
                End If

            Else
                ' Not selected
                brdr = MetroColorSchema.UnSelectedBorder

                If bState = ButtonState.ButtonUp And bMouse = MouseCapture.Outside Then
                    ' normal
                    brdr = MetroColorSchema.DisabledBorder
                    ColorScheme = MetroColorSchema.UnSelectedNormal

                ElseIf bState = ButtonState.ButtonUp And bMouse = MouseCapture.Inside Then
                    '  hover 
                    ColorScheme = MetroColorSchema.UnSelectedHover

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Outside Then
                    ' no one cares!

                ElseIf bState = ButtonState.ButtonDown And bMouse = MouseCapture.Inside Then
                    ' pressed
                    ColorScheme = MetroColorSchema.UnSelectedPressed
                End If

            End If
        End If
    End Sub

    Protected Overridable Sub PaintDrawBackground(gfx As Graphics)
        '
        Dim rect As New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
        Dim p As New GraphicsPath

        Call p.StartFigure()

        If MetroColorSchema Is Nothing Then

            '/// Build a rounded rectangle
            Const Roundness = 5

            Call p.StartFigure()
            p.AddArc(New Rectangle(rect.Left, rect.Top, Roundness, Roundness), 180, 90)
            p.AddLine(rect.Left + Roundness, 0, rect.Right - Roundness, 0)
            p.AddArc(New Rectangle(rect.Right - Roundness, 0, Roundness, Roundness), -90, 90)
            p.AddLine(rect.Right, Roundness, rect.Right, rect.Bottom - Roundness)
            p.AddArc(New Rectangle(rect.Right - Roundness, rect.Bottom - Roundness, Roundness, Roundness), 0, 90)
            p.AddLine(rect.Right - Roundness, rect.Bottom, rect.Left + Roundness, rect.Bottom)
            p.AddArc(New Rectangle(rect.Left, rect.Height - Roundness, Roundness, Roundness), 90, 90)

        Else

            Call p.AddLine(rect.Left, 0, rect.Right, 0)
            Call p.AddLine(rect.Right, 0, rect.Right, rect.Bottom)
            Call p.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom)

        End If

        Call p.CloseFigure()

        '/// Draw the background ///
        Dim ColorScheme As Color() = Nothing
        Dim brdr As SolidBrush = Nothing

        If bState <> ButtonState.Disabled AndAlso bState = ButtonState.ButtonDown AndAlso bMouse = MouseCapture.Outside Then
            Return
        Else
            Call InternalAssignColorScheme(ColorScheme, brdr)
        End If

        ' Draw
        Dim b As LinearGradientBrush = New LinearGradientBrush(rect, Color.White, Color.Black, LinearGradientMode.Vertical)
        Dim blend As ColorBlend = New ColorBlend
        blend.Colors = ColorScheme
        blend.Positions = New Single() {0.0F, 0.1, 0.9F, 0.95F, 1.0F}
        b.InterpolationColors = blend

        Call gfx.FillPath(b, p)
        '// Draw border
        Call gfx.DrawPath(New Pen(brdr), p)

        '// Draw bottom border if Normal state (not hovered)
        If _MetroColorSchema Is Nothing AndAlso bMouse = MouseCapture.Outside Then
            rect = New Rectangle(rect.Left, Me.Height - 1, rect.Width, 1)
            b = New LinearGradientBrush(rect, Color.Blue, Color.Yellow, LinearGradientMode.Horizontal)
            blend = New ColorBlend
            blend.Colors = New Color() {Color.White, Color.LightGray, Color.White}
            blend.Positions = New Single() {0.0F, 0.5F, 1.0F}
            b.InterpolationColors = blend
            '
            gfx.FillRectangle(b, rect)
        End If
    End Sub

    ''' <summary>
    ''' 这个函数只是绘制了头像而已
    ''' </summary>
    ''' <param name="gfx"></param>
    ''' <example>
    '''
    ''' gfx.CompositingQuality = CompositingQuality.HighQuality
    '''
    ''' ' Album Image
    ''' If _InternalIconImage IsNot Nothing Then
    ''' Dim hh As Integer = Height * 0.9
    '''    gfx.DrawImage(_InternalIconImage, CInt((Height - hh) / 2), CInt((Height - hh) / 2), hh, hh)
    ''' Else
    '''    gfx.DrawImage(ImageList1.Images(0), New Point(7, 7))
    ''' End If
    ''' </example>
    Protected Overridable Sub PaintDrawButton(gfx As Graphics)

        gfx.CompositingQuality = CompositingQuality.HighQuality

        ' Album Image
        If _InternalIconImage IsNot Nothing Then
            Dim hh As Integer = Height * 0.9
            gfx.DrawImage(_InternalIconImage, CInt((Height - hh) / 2), CInt((Height - hh) / 2), hh, hh)
        Else
            gfx.DrawImage(ImageList1.Images(0), New Point(7, 7))
        End If

    End Sub

    Private Sub PaintEvent(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        Dim gfx = e.Graphics
        '
        PaintDrawBackground(gfx)
        PaintDrawButton(gfx)
    End Sub

#End Region

End Class
