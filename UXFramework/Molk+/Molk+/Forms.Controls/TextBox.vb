#Region "Microsoft.VisualBasic::3469fad5f3245ae043aef1d632187703, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\TextBox.vb"

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

'Use it as you want :)

Imports System.Windows.Forms
Imports System.Drawing
Imports System.ComponentModel

Namespace Windows.Forms.Controls

    Public Class TextBox
        Inherits System.Windows.Forms.TextBox
#Region "Fields"

#Region "Protected Fields"

        Protected _waterMarkText As String = "Default Watermark..."
        'The watermark text
        Protected _waterMarkColor As Color
        'Color of the watermark when the control does not have focus
        Protected _waterMarkActiveColor As Color
        'Color of the watermark when the control has focus
#End Region

#Region "Private Fields"

        Private waterMarkContainer As Panel
        'Container to hold the watermark
        Private m_waterMarkFont As Font
        'Font of the watermark
        Private waterMarkBrush As SolidBrush
        'Brush for the watermark
#End Region

#End Region

#Region "Constructors"

        Public Sub New()
            Initialize()
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Initializes watermark properties and adds CtextBox events
        ''' </summary>
        Private Sub Initialize()
            'Sets some default values to the watermark properties
            _waterMarkColor = Color.LightGray
            _waterMarkActiveColor = Color.Gray
            m_waterMarkFont = Me.Font
            waterMarkBrush = New SolidBrush(_waterMarkActiveColor)
            waterMarkContainer = Nothing

            'Draw the watermark, so we can see it in design time
            DrawWaterMark()

            'Eventhandlers which contains function calls. 
            'Either to draw or to remove the watermark
            AddHandler Enter, AddressOf ThisHasFocus
            AddHandler Leave, AddressOf ThisWasLeaved
            AddHandler TextChanged, AddressOf ThisTextChanged
        End Sub

        ''' <summary>
        ''' Removes the watermark if it should
        ''' </summary>
        Private Sub RemoveWaterMark()
            If waterMarkContainer IsNot Nothing Then
                Me.Controls.Remove(waterMarkContainer)
                waterMarkContainer = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Draws the watermark if the text length is 0
        ''' </summary>
        Private Sub DrawWaterMark()
            If Me.waterMarkContainer Is Nothing AndAlso Me.TextLength <= 0 Then
                waterMarkContainer = New Panel()
                ' Creates the new panel instance
                AddHandler waterMarkContainer.Paint, AddressOf waterMarkContainer_Paint
                waterMarkContainer.Invalidate()
                AddHandler waterMarkContainer.Click, AddressOf waterMarkContainer_Click
                ' adds the control
                Me.Controls.Add(waterMarkContainer)
            End If
        End Sub

#End Region

#Region "Eventhandlers"

#Region "WaterMark Events"

        Private Sub waterMarkContainer_Click(sender As Object, e As EventArgs)
            Me.Focus()
            'Makes sure you can click wherever you want on the control to gain focus
        End Sub

        Private Sub waterMarkContainer_Paint(sender As Object, e As PaintEventArgs)
            'Setting the watermark container up
            waterMarkContainer.Location = New Point(2, 0)
            ' sets the location
            waterMarkContainer.Height = Me.Height
            ' Height should be the same as its parent
            waterMarkContainer.Width = Me.Width
            ' same goes for width and the parent
            waterMarkContainer.Anchor = AnchorStyles.Left Or AnchorStyles.Right
            ' makes sure that it resizes with the parent control


            If Me.ContainsFocus Then
                'if focused use normal color
                waterMarkBrush = New SolidBrush(Me._waterMarkActiveColor)
            Else

                'if not focused use not active color
                waterMarkBrush = New SolidBrush(Me._waterMarkColor)
            End If

            'Drawing the string into the panel 
            Dim g As Graphics = e.Graphics
            g.DrawString(Me._waterMarkText, m_waterMarkFont, waterMarkBrush, New PointF(-2.0F, 1.0F))
            'Take a look at that point
            'The reason I'm using the panel at all, is because of this feature, that it has no limits
            'I started out with a label but that looked very very bad because of its paddings 
        End Sub

#End Region

#Region "CTextBox Events"

        Private Sub ThisHasFocus(sender As Object, e As EventArgs)
            'if focused use focus color
            waterMarkBrush = New SolidBrush(Me._waterMarkActiveColor)

            'The watermark should not be drawn if the user has already written some text
            If Me.TextLength <= 0 Then
                RemoveWaterMark()
                DrawWaterMark()
            End If
        End Sub

        Private Sub ThisWasLeaved(sender As Object, e As EventArgs)
            'if the user has written something and left the control
            If Me.TextLength > 0 Then
                'Remove the watermark
                RemoveWaterMark()
            Else
                'But if the user didn't write anything, Then redraw the control.
                Me.Invalidate()
            End If
        End Sub

        Private Sub ThisTextChanged(sender As Object, e As EventArgs)
            'If the text of the textbox is not empty
            If Me.TextLength > 0 Then
                'Remove the watermark
                RemoveWaterMark()
            Else
                'But if the text is empty, draw the watermark again.
                DrawWaterMark()
            End If
        End Sub

#Region "Overrided Events"

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)
            'Draw the watermark even in design time
            DrawWaterMark()
        End Sub

        Protected Overrides Sub OnInvalidated(e As InvalidateEventArgs)
            MyBase.OnInvalidated(e)
            'Check if there is a watermark
            If waterMarkContainer IsNot Nothing Then
                'if there is a watermark it should also be invalidated();
                waterMarkContainer.Invalidate()
            End If
        End Sub

#End Region

#End Region

#End Region

#Region "Properties"
        <Localizable(True)>
        <Category("Watermark attribtues")>
        <Description("Sets the text of the watermark")>
        Public Property WaterMark() As String
            Get
                Return Me._waterMarkText
            End Get
            Set
                Me._waterMarkText = Value
                Me.Invalidate()
            End Set
        End Property

        <Localizable(True)>
        <Category("Watermark attribtues")>
        <Description("When the control gaines focus, this color will be used as the watermark's forecolor")>
        Public Property WaterMarkActiveForeColor() As Color
            Get
                Return Me._waterMarkActiveColor
            End Get

            Set
                Me._waterMarkActiveColor = Value
                Me.Invalidate()
            End Set
        End Property

        <Localizable(True)>
        <Category("Watermark attribtues")>
        <Description("When the control looses focus, this color will be used as the watermark's forecolor")>
        Public Property WaterMarkForeColor() As Color
            Get
                Return Me._waterMarkColor
            End Get

            Set
                Me._waterMarkColor = Value
                Me.Invalidate()
            End Set
        End Property

        <Localizable(True)>
        <Category("Watermark attribtues")>
        <Description("The font used on the watermark. Default is the same as the control")>
        Public Property WaterMarkFont() As Font
            Get
                Return Me.m_waterMarkFont
            End Get

            Set
                Me.m_waterMarkFont = Value
                Me.Invalidate()
            End Set
        End Property

#End Region
    End Class
End Namespace
