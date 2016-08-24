#Region "Microsoft.VisualBasic::3ff49b0c1fc3a093f2f76860971bb733, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\GlowBox\gGlowGroupBox.vb"

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
''' Panel Control to add Glow Effect to all of the Child Controls
''' </summary>
''' <remarks>v1.0.2</remarks>
<ToolboxItem(True), ToolboxBitmap(GetType(gGlowBox), "gGlowBox.gGlowGroupBox.bmp")> _
<System.Diagnostics.DebuggerStepThrough()> _
Public Class gGlowGroupBox
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
    Private _EffectType As eEffectType = eEffectType.Glow
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

    Public Enum eEffectType
        Glow
        Shadow
    End Enum


    ''' <summary>
    ''' Choose Glow or Shadow
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Category("gGlowBox")>
    <Description("Choose Glow or Shadow")>
    <DefaultValue("Glow")>
    Public Property EffectType As eEffectType
        Get
            Return _EffectType
        End Get
        Set( Value As eEffectType)
            _EffectType = Value
        End Set
    End Property

#End Region

#Region "Paint"

    Protected Overrides Sub OnPaintBackground( e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        If DesignMode = True AndAlso Controls.Count = 0 Then
            TextRenderer.DrawText(e.Graphics,
                                  String.Format("Drop controls{0}on the gGlowGroupBox", vbNewLine),
                                  New Font("Arial", 8, FontStyle.Bold),
                                  New Point(20, 20),
                                  Color.DarkBlue)
            TextRenderer.DrawText(e.Graphics,
                                 "SSDiver2112",
                                 New Font("Arial", 7, FontStyle.Bold),
                                 New Point(Width - 75, Height - 17),
                                 Color.LightGray)
        ElseIf _glowOn Then

            For Each _control As Control In Controls

                If _control.Focused = True Then

                    Dim GlowK As Boolean = True

                    'Check if the control has the ReadOnly property and if so, its value.
                    If Not IsNothing(_control.GetType().GetProperty("ReadOnly")) Then
                        GlowK = Not CallByName(_control, "ReadOnly", CallType.Get)
                    End If

                    If GlowK Then

                        If EffectType = eEffectType.Glow Then

                            Using gp As New GraphicsPath
                                'Change these to Properties if you want Design Control of the Values 
                                Dim _Glow = 15
                                Dim _Feather = 50
                                'Get a Rectangle a little smaller than the control's
                                'and make a GraphicsPath with it
                                Dim rect As Rectangle = New Rectangle(
                                                        _control.Bounds.X,
                                                        _control.Bounds.Y,
                                                        _control.Bounds.Width - 1,
                                                        _control.Bounds.Height - 1)
                                rect.Inflate(-1, -1)
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

                        Else
                            Using shadowpath As New GraphicsPath
                                'Change these to Properties if you want Design Control of the Values 
                                Dim _ShadowOffset As New Point(3, 3)
                                Dim _ShadowColor As Color = _glowColor
                                Dim _ShadowBlur As Integer = 2
                                Dim _ShadowFeather As Integer = 100

                                Dim rect As Rectangle = New Rectangle(
                                                           _control.Bounds.X + 4 + _ShadowOffset.X,
                                                           _control.Bounds.Y + 4 + _ShadowOffset.Y,
                                                           _control.Bounds.Width - 8,
                                                           _control.Bounds.Height - 8)
                                shadowpath.AddRectangle(rect)

                                Dim x As Integer = 6
                                For i As Integer = 1 To x
                                    Using pen As Pen = New Pen(Color.FromArgb(
                                                               CInt(_ShadowFeather - ((_ShadowFeather / x) * i)), _ShadowColor),
                                                               CSng(i * (_ShadowBlur)))
                                        pen.LineJoin = LineJoin.Round
                                        e.Graphics.DrawPath(pen, shadowpath)
                                    End Using
                                Next i

                                e.Graphics.FillPath(New SolidBrush(_ShadowColor), shadowpath)
                            End Using
                        End If


                    End If
                End If
            Next

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
        Invalidate()
    End Sub

    Private Sub ChildLostFocus()
        Invalidate()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub
#End Region

End Class
