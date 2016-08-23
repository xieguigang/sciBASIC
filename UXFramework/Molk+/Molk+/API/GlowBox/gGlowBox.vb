#Region "Microsoft.VisualBasic::bfc7c2b4bcac6391821fce12f7247127, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\GlowBox\gGlowBox.vb"

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

#Region "Imports"
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Windows.Forms
#End Region

''' <summary>
''' gGlowBox is a Panel control to add glow effect to a focused child control
''' </summary>
''' <remarks>
''' v1.0.1
''' SSDiver2112 ©2012
''' </remarks>
<ToolboxItem(True), ToolboxBitmap(GetType(gGlowBox), "gGlowBox.gGlowBox.bmp")> _
<System.Diagnostics.DebuggerStepThrough()> _
Public Class gGlowBox
    Inherits Panel

#Region "Initialize"

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        SetStyle(ControlStyles.AllPaintingInWmPaint, True)
    End Sub
#End Region

#Region "Fields"
    Private _glowColor As Color = Color.Maroon
    Private _glowOn As Boolean
#End Region

#Region "Properties"

    ''' <summary>
    ''' Get or Set the color of the Glow
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("gGlowBox")>
    <Description("Get or Set the color of the Glow")>
    <DefaultValue(GetType(Color), "Maroon")>
    Public Property GlowColor As Color
        Get
            Return _glowColor
        End Get
        Set( Value As Color)
            _glowColor = Value
            Invalidate()
        End Set
    End Property


    ''' <summary>
    ''' Turn the Glow effect on or off
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("gGlowBox")>
    <Description("Turn the Glow effect on or off")>
    <DefaultValue(False)>
    Public Property GlowOn As Boolean
        Get
            Return _glowOn
        End Get
        Set( Value As Boolean)
            _glowOn = Value
            Invalidate()
        End Set
    End Property
#End Region

#Region "Paint"

    Protected Overrides Sub OnPaintBackground( e As PaintEventArgs)
        MyBase.OnPaintBackground(e)

        If DesignMode = True AndAlso Controls.Count = 0 Then
            TextRenderer.DrawText(e.Graphics,
                                  String.Format("Drop a control{0}on the gGlowBox", vbNewLine),
                                  New Font("Arial", 8, FontStyle.Bold),
                                  New Point(20, 20),
                                  Color.DarkBlue)
            TextRenderer.DrawText(e.Graphics,
                                 "SSDiver2112",
                                 New Font("Arial", 7, FontStyle.Bold),
                                 New Point(Width - 75, Height - 17),
                                 Color.LightGray)
        ElseIf _glowOn Then

            Using gp As New GraphicsPath
                Dim _Glow = 15
                Dim _Feather = 50
                'Get a Rectangle a little smaller than the Panel's
                'and make a GraphicsPath with it
                Dim rect As Rectangle = DisplayRectangle
                rect.Inflate(-5, -5)
                gp.AddRectangle(rect)

                'Draw multiple rectangles with increasing thickness and transparency
                For i As Integer = 1 To _Glow Step 2
                    Dim aGlow As Integer = CInt(_Feather -
                      ((_Feather / _Glow) * i))
                    Using pen As Pen =
                        New Pen(Color.FromArgb(aGlow, _glowColor), i) With
                        {.LineJoin = LineJoin.Round}

                        e.Graphics.DrawPath(pen, gp)

                    End Using

                Next i

            End Using
        End If

    End Sub
#End Region

#Region "Sizing"

    Private Sub gGlowBox_Layout( sender As Object,  e As LayoutEventArgs) Handles Me.Layout

        'Resize the gGlowBox to fit in the Child Control size
        If Controls.Count > 0 Then
            If e.AffectedControl Is Controls(0) Then
                Size = New Size(Controls(0).Width + 7, Controls(0).Height + 7)
                Controls(0).Location = New Point(4, 4)
                Invalidate()
            End If

        End If

    End Sub

    Private Sub gGlowBox_Resize( sender As Object,  e As System.EventArgs) Handles Me.Resize

        'This is needed to avoid resizing an Anchored gGlowBox when the parent Form is Minimized 
        If IsNothing(FindForm) OrElse FindForm.WindowState = FormWindowState.Minimized Then Exit Sub

        'Resize the Child Control to fit the size of the gGlowBox
        If Controls.Count > 0 Then
            Controls(0).Size = New Size(Width - 7, Height - 7)
        End If

    End Sub
#End Region

#Region "Control Focus Event"

    Private Sub gGlowBox_ControlAdded( sender As Object,  e As ControlEventArgs) Handles Me.ControlAdded
        ' Add handlers to let the gGlowBox know when the child control gets Focus 
        AddHandler e.Control.GotFocus, AddressOf ChildGotFocus
        AddHandler e.Control.LostFocus, AddressOf ChildLostFocus
    End Sub

    Private Sub ChildGotFocus()

        If Controls.Count > 0 Then
            'Check if the control has the ReadOnly property and if so, its value.
            If Not IsNothing(Controls(0).GetType().GetProperty("ReadOnly")) Then
                GlowOn = Not CallByName(Controls(0), "ReadOnly", CallType.Get)
            Else
                GlowOn = True
            End If

        End If

    End Sub

    Private Sub ChildLostFocus()
        GlowOn = False
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
#End Region

End Class
