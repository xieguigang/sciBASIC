#Region "Microsoft.VisualBasic::b5edf7c18091acc65875bb2d1cb928a6, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\NotificationBar.vb"

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

'
' * This file is part of the NotificationBar Library.
' * 
' * InformationBar is free software: you can redistribute it and/or modify
' * it under the terms of the GNU General Public License as published by
' * the Free Software Foundation, either version 3 of the License, or
' * (at your option) any later version.
'
' * InformationBar is distributed in the hope that it will be useful,
' * but WITHOUT ANY WARRANTY; without even the implied warranty of
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' * GNU General Public License for more details.
'
' * You should have received a copy of the GNU General Public License
' * along with the InformationBar Library.  If not, see <http://www.gnu.org/licenses/>.
' * 
' * Author: Cory Borrow
' * Date: June 03, 2008 9:50pm
'


Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Media
Imports System.Threading

''' <summary>
''' Notification Bar
''' A small and easy to use notification bar like that found in IE6+ and Firefox web browsers.
''' http://www.codeproject.com/Articles/26627/Notification-Bar
''' </summary>
Public Class NotificationBar : Inherits Control

    Dim _InternalFlashTimer As New System.Windows.Forms.Timer()
    Dim imageKey As Integer = 0

    Dim closeButtonSize As New Size(20, 20)
    Dim closeButtonPadding As Integer = 6
    Dim textY As Integer = 5

    Dim playSoundOnVisible As Boolean = True
    Dim mouseInBounds As Boolean = False
    Dim controlHighlighted As Boolean = False
    Dim inAnimation As Boolean = False

    Dim tickCount As Integer = 0
    Dim flashCount As Integer = 0
    Dim flashTo As Integer = 0

    <Description("Image list containg images shown on the left side of the control"), Category("Appearance")>
    Public Property SmallImageList As ImageList

    <DefaultValue(0)> _
    <Description("The index of an image contained in the SmallImageList in which to display on the control"), Category("Appearance")>
    Public Property ImageIndex As Integer
        Get
            Return imageKey
        End Get
        Set(value As Integer)
            imageKey = value
            Me.Invalidate()
        End Set
    End Property

    Public Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            MyBase.Text = value
            Me.Invalidate()
        End Set
    End Property

    <DefaultValue(True)>
    <Description("Weather or not a sound should be played when the control is shown"), Category("Behavior")>
    Public Property PlaySoundWhenShown As Boolean
        Get
            Return playSoundOnVisible
        End Get
        Set(value As Boolean)
            playSoundOnVisible = value
        End Set
    End Property

    Public Sub New()
        Me.BackColor = SystemColors.Info

        _InternalFlashTimer.Interval = 1000
        AddHandler _InternalFlashTimer.Tick, New EventHandler(AddressOf flashTimer_Tick)

        Call SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Call SetStyle(ControlStyles.ResizeRedraw, True)
    End Sub

    Public Sub Flash(interval As Integer, times As Integer)
        If Me.Visible Then
            flashTo = times
            tickCount = 0

            _InternalFlashTimer.Interval = interval
            _InternalFlashTimer.Start()
        End If
    End Sub

    Public Sub Flash(numberOfTimes As Integer)
        Flash(1000, numberOfTimes)
    End Sub

    Public Sub FlashOnce(milliseconds As Integer)
        Flash(milliseconds, 1)
    End Sub

    Public Overloads Sub Show(animate As Boolean)
        If animate Then
            Dim origHeight As Integer = Me.Height
            Me.Height = 1
            Me.Show()
            inAnimation = True

            For height As Integer = Me.Height To origHeight - 1 Step 5
                textY += 2
                Me.Height = height
                Me.Refresh()
                Call Thread.Sleep(5)
            Next

            inAnimation = False
        Else
            Me.Show()
        End If

        Me.Refresh()
    End Sub

    Public Overloads Sub Hide(animate As Boolean)
        If animate Then
            Dim origHeight As Integer = Me.Height
            inAnimation = True

            For height As Integer = Me.Height To 2 Step -5
                textY -= 2
                Me.Height = height
                Me.Refresh()
                Thread.Sleep(5)
            Next
            Me.Hide()
            Me.Height = origHeight
            inAnimation = False
        Else
            Me.Hide()
        End If

        Me.Refresh()
    End Sub

    Private Sub flashTimer_Tick(sender As Object, e As EventArgs)
        If controlHighlighted Then
            Me.ForeColor = SystemColors.ControlText
            Me.BackColor = SystemColors.Info
            controlHighlighted = False
            flashCount += 1

            If flashCount = flashTo Then
                _InternalFlashTimer.[Stop]()
                flashCount = 0
            End If
        Else
            Me.ForeColor = SystemColors.HighlightText
            Me.BackColor = SystemColors.Highlight
            controlHighlighted = True
        End If

        tickCount += 1
        Me.Refresh()
    End Sub

#Region "Protected Methods"
    Protected Sub DrawText(e As PaintEventArgs)
        Dim leftPadding As Integer = 1

        If _SmallImageList IsNot Nothing AndAlso _SmallImageList.Images.Count > 0 AndAlso _SmallImageList.Images.Count > imageKey Then
            leftPadding = _SmallImageList.ImageSize.Width + 4
            e.Graphics.DrawImage(_SmallImageList.Images(imageKey), New Point(2, 5))
        End If

        Dim textSize As Size = TextRenderer.MeasureText(e.Graphics, Me.Text, Me.Font)
        Dim maxTextWidth As Integer = (Me.Width - (closeButtonSize.Width + (closeButtonPadding * 2)))
        Dim lineHeight As Integer = textSize.Height + 2
        Dim numLines As Integer = 1

        If textSize.Width > maxTextWidth Then
            numLines = textSize.Width \ maxTextWidth + 1
        End If

        Dim textRect As New Rectangle()
        textRect.Width = Me.Width - (closeButtonSize.Width + closeButtonPadding) - leftPadding
        textRect.Height = (numLines * lineHeight)
        textRect.X = leftPadding
        textRect.Y = 5

        If Me.Height < (numLines * lineHeight) + 10 Then
            If Not inAnimation Then
                textRect.Y = 5
                Me.Height = (numLines * lineHeight) + 10
            Else
                textRect.Y = textY
            End If
        End If

        TextRenderer.DrawText(e.Graphics, Me.Text, Me.Font, textRect, Me.ForeColor, TextFormatFlags.WordBreak Or TextFormatFlags.Left Or TextFormatFlags.Top)
    End Sub

    Protected Sub DrawCloseButton(e As PaintEventArgs)
        Dim closeButtonColor As Color = Color.Black

        If mouseInBounds Then
            closeButtonColor = Color.White
        End If

        Dim linePen As New Pen(Me.ForeColor, 2)
        Dim line1Start As New Point((Me.Width - (closeButtonSize.Width - closeButtonPadding)), closeButtonPadding)
        Dim line1End As New Point((Me.Width - closeButtonPadding), (closeButtonSize.Height - closeButtonPadding))
        Dim line2Start As New Point((Me.Width - closeButtonPadding), closeButtonPadding)
        Dim line2End As New Point((Me.Width - (closeButtonSize.Width - closeButtonPadding)), (closeButtonSize.Height - closeButtonPadding))

        e.Graphics.DrawLine(linePen, line1Start, line1End)
        e.Graphics.DrawLine(linePen, line2Start, line2End)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        DrawText(e)
        DrawCloseButton(e)

        MyBase.OnPaint(e)
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        Me.ForeColor = SystemColors.HighlightText
        Me.BackColor = SystemColors.Highlight
        mouseInBounds = True

        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        If controlHighlighted Then
            Me.ForeColor = SystemColors.HighlightText
            Me.BackColor = SystemColors.Highlight
        Else
            Me.ForeColor = SystemColors.ControlText
            Me.BackColor = SystemColors.Info
        End If

        Me.ForeColor = SystemColors.ControlText
        mouseInBounds = False

        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        If e.X >= (Me.Width - (closeButtonSize.Width + closeButtonPadding)) AndAlso e.Y <= 12 Then
            Me.Hide()
        Else
            If ContextMenuStrip IsNot Nothing Then
                ContextMenuStrip.Show(Me, e.Location)
            End If
        End If

        MyBase.OnMouseClick(e)
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        If Me.Visible AndAlso playSoundOnVisible Then
            Call SystemSounds.Question.Play()
        End If

        MyBase.OnVisibleChanged(e)
    End Sub
#End Region
End Class
