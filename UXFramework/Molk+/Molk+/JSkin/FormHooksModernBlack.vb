#Region "Microsoft.VisualBasic::1f858ddf8b6d4dbe00e275badc912d77, ..\visualbasic_App\UXFramework\Molk+\Molk+\JSkin\FormHooksModernBlack.vb"

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
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing.Drawing2D
Imports System.ComponentModel.Design
Imports System.Drawing.Text



'
'    Copyright (c) 2013 jn4kim
'
'    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
'    (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, 
'    merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished 
'    to do so, subject to the following conditions:
'    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
'    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
'    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
'    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
'    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'    
'    http://jskin.codeplex.com/
'    http://developert.com/
'     
' 
'     * You can alter the Form title & ICON by simply altering the property of the Parent Form.
'     * If you want to enable Run-time resize, set the Stretch Property of the jSkin 'true',
'     * set the minimumsize property of the Parent Form
'     * and insert following code in the constructor of the Form.
'     * 
'     * MaximizedBounds = Screen.GetWorkingArea(this);

'ControlContainer
<Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", GetType(IDesigner))> _
Partial Public Class FormHooksModernBlack
    Inherits UserControl

#Region "- Property -"
    Private _Stretch As Boolean = False
    <Category("Stretch"), Description("Runtime Form Resizing")> _
    Public Property Stretch() As Boolean
        Get
            Return _Stretch
        End Get
        Set(value As Boolean)
            _Stretch = value
            If value Then
                FixedSingle = False
            End If
            Me.Invalidate()
        End Set
    End Property

    Private _FixedSingle As Boolean = False
    <Category("FixedSingle"), Description("")> _
    Public Property FixedSingle() As Boolean
        Get
            Return _FixedSingle
        End Get
        Set(value As Boolean)
            _FixedSingle = value
            If value Then
                Stretch = False
            End If
            Me.Invalidate()
        End Set
    End Property
#End Region

#Region "- CheckFont -"

    Private Function IsFontInstalled(fontName As String) As Boolean
        Using testFont = New Font(fontName, 8)
            Return 0 = String.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase)
        End Using
    End Function

#End Region

#Region "- Resizing API & Const -"

    <System.Runtime.InteropServices.DllImport("User32")> _
    Private Shared Function SendMessage(hWnd As Integer, hMsg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function

    <System.Runtime.InteropServices.DllImport("user32.dll")> _
    Private Shared Function DefWindowProc(hWnd As IntPtr, uMsg As UInteger, wParam As UIntPtr, lParam As IntPtr) As IntPtr
    End Function

    <System.Runtime.InteropServices.DllImport("User32")> _
    Private Shared Function ReleaseCapture() As Integer
    End Function

    Const WM_SYSCOMMAND As UInteger = &H112
    Const SC_DRAG_RESIZEL As UInteger = &HF001
    ' left resize
    Const SC_DRAG_RESIZER As UInteger = &HF002
    ' right resize
    Const SC_DRAG_RESIZEU As UInteger = &HF003
    ' upper resize
    Const SC_DRAG_RESIZEUL As UInteger = &HF004
    ' upper-left resize
    Const SC_DRAG_RESIZEUR As UInteger = &HF005
    ' upper-right resize
    Const SC_DRAG_RESIZED As UInteger = &HF006
    ' down resize
    Const SC_DRAG_RESIZEDL As UInteger = &HF007
    ' down-left resize
    Const SC_DRAG_RESIZEDR As UInteger = &HF008
    ' down-right resize
    Const SC_DRAG_MOVE As UInteger = &HF012
    ' move
#End Region

#Region "- FormSkin Setting -"
    Const _layoutW As Integer = 64
    Const _upCornerW As Integer = 10
    Const _upH As Integer = 32
    Const _downH As Integer = 10
    Const _barH As Integer = 22
    Const _btH As Integer = 21
    Const _btL As Integer = 113
    Const _btMinW As Integer = 29
    Const _btMaxW As Integer = 27
    Const _btExitW As Integer = 49
    Const _btRight As Integer = _btL - _btMinW - _btMaxW - _btExitW
    Const _TitleBarT As Integer = 30
#End Region
#Region "- Button Variables -"
    Private _isMin As Boolean = False, _isMax As Boolean = False, _isExit As Boolean = False
    Public _isMaxed As Boolean = False
    Private _isReset As Boolean = False
#End Region
#Region "- Mouse API & Variables -"
    Private _isMouseIn As Boolean = False
    Private _isClicked As Boolean = False
    Private _RightX As Integer = 0
    Private _DownY As Integer = 0
#End Region

#Region "- Private Methods -"
    Private Sub ctlSkin_Paint(sender As Object, e As PaintEventArgs)
        Dim buff As BufferedGraphics = BufferedGraphicsManager.Current.Allocate(e.Graphics, Me.ClientRectangle)

        buff.Graphics.Clear(Me.BackColor)
        buff.Graphics.InterpolationMode = InterpolationMode.High


        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(0, 0, _upCornerW, _upH), 0, 0, _upCornerW, _upH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(_upCornerW, 0, Me.Width - 2 * _upCornerW, _upH), _upCornerW, 0, _layoutW - 2 * _upCornerW, _upH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(Me.Width - _upCornerW, 0, _upCornerW, _upH), _layoutW - _upCornerW, 0, _upCornerW, _upH, _
            GraphicsUnit.Pixel)


        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(0, _upH, _upCornerW, Me.Height - _upH - _downH), 0, _upH, _upCornerW, _barH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(_upCornerW, _upH, Me.Width - 2 * _upCornerW, Me.Height - _upH - _downH), _upCornerW, _upH, _layoutW - 2 * _upCornerW, _barH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(Me.Width - _upCornerW, _upH, _upCornerW, Me.Height - _upH - _downH), _layoutW - _upCornerW, _upH, _upCornerW, _barH, _
            GraphicsUnit.Pixel)


        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(0, Me.Height - _downH, _upCornerW, _downH), 0, _layoutW - _downH, _upCornerW, _downH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(_upCornerW, Me.Height - _downH, Me.Width - 2 * _upCornerW, _downH), _upCornerW, _layoutW - _downH, _layoutW - 2 * _upCornerW, _downH, _
            GraphicsUnit.Pixel)
        buff.Graphics.DrawImage(pLayout.Image, New Rectangle(Me.Width - _upCornerW, Me.Height - _downH, _upCornerW, _downH), _layoutW - _upCornerW, _layoutW - _downH, _upCornerW, _downH, _
            GraphicsUnit.Pixel)

        Dim frm As System.Windows.Forms.Form = TryCast(Me.Parent, System.Windows.Forms.Form)


        Dim ico As New Icon(frm.Icon, New Size(16, 16))
        buff.Graphics.DrawIcon(ico, New Rectangle(7, 7, 16, 16))

        Dim fontName As String = ""
        Dim fontSize As Integer = 10
        Dim fonttop As Integer = 7
        Dim fontStyle__1 As FontStyle = FontStyle.Regular
        Dim myBrush As New SolidBrush(Color.WhiteSmoke)


        If IsFontInstalled("맑은 고딕") OrElse IsFontInstalled("Malgun Gothic") Then
            fontName = "Malgun Gothic"
            fonttop = 4
            fontSize = 10
            fontStyle__1 = FontStyle.Bold
        ElseIf IsFontInstalled("나눔고딕") OrElse IsFontInstalled("NanumGothic") Then
            fontName = "NanumGothic"
            fonttop = 7
            fontSize = 10
            fontStyle__1 = FontStyle.Bold
        ElseIf IsFontInstalled("Segoe UI Symbol") Then
            fontStyle__1 = FontStyle.Bold
            fontName = "Segoe UI Symbol"
            fonttop = 5
            fontSize = 10
        Else
            fontName = DefaultFont.Name
            fontSize = 9
            fonttop = 10
        End If

        buff.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault
        buff.Graphics.DrawString(frm.Text, New Font(fontName, fontSize, fontStyle__1), myBrush, 25, fonttop)

        If _isMouseIn Then
          
            If _RightX - _btRight <= _btExitW Then

                _isExit = True
                _isMin = False
                _isMax = False

                If _isClicked Then
                    buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW, 0, _btExitW, _btH), _btMinW + _btMaxW, 2 * _btH, _btExitW, _btH, _
                        GraphicsUnit.Pixel)
                Else
                    buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW, 0, _btExitW, _btH), _btMinW + _btMaxW, 1 * _btH, _btExitW, _btH, _
                        GraphicsUnit.Pixel)
                End If

                If Not FixedSingle Then
                    If Stretch Then
                        If _isMaxed Then
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), 0, 4 * _btH, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        Else
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 0, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        End If
                    Else
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 3 * _btH, _btMaxW, _btH, _
                            GraphicsUnit.Pixel)
                    End If

                    buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL, 0, _btMinW, _btH), 0, 0, _btMinW, _btH, _
                        GraphicsUnit.Pixel)
                End If
            ElseIf _RightX - _btRight <= _btExitW + _btMaxW Then

                _isMin = False
                _isExit = False
                If Not FixedSingle Then
                    _isMax = True

                    If Stretch Then
                        If _isClicked Then
                            If _isMaxed Then
                                buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMaxW * 2, 4 * _btH, _btMaxW, _btH, _
                                    GraphicsUnit.Pixel)
                            Else
                                buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 2 * _btH, _btMaxW, _btH, _
                                    GraphicsUnit.Pixel)
                            End If
                        ElseIf _isMaxed Then
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMaxW, 4 * _btH, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        Else
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 1 * _btH, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        End If
                    Else
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 3 * _btH, _btMaxW, _btH, _
                            GraphicsUnit.Pixel)
                    End If

                    buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL, 0, _btMinW, _btH), 0, 0, _btMinW, _btH, _
                        GraphicsUnit.Pixel)
                End If


                buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW, 0, _btExitW, _btH), _btMinW + _btMaxW, 0, _btExitW, _btH, _
                    GraphicsUnit.Pixel)
            ElseIf _RightX - Right <= _btExitW + _btMaxW + _btMinW Then

                If Not FixedSingle Then
                    _isMin = True
                    _isMax = False
                    _isExit = False

                    If _isClicked Then
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL, 0, _btMinW, _btH), 0, 2 * _btH, _btMinW, _btH, _
                            GraphicsUnit.Pixel)
                    Else
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL, 0, _btMinW, _btH), 0, 1 * _btH, _btMinW, _btH, _
                            GraphicsUnit.Pixel)
                    End If

                    If Stretch Then
                        If _isMaxed Then
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), 0, 4 * _btH, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        Else
                            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 0, _btMaxW, _btH, _
                                GraphicsUnit.Pixel)
                        End If
                    Else
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 3 * _btH, _btMaxW, _btH, _
                            GraphicsUnit.Pixel)
                    End If
                End If
                buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW, 0, _btExitW, _btH), _btMinW + _btMaxW, 0, _btExitW, _btH, _
                    GraphicsUnit.Pixel)
            End If
        Else

            If Not FixedSingle Then
                buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL, 0, _btMinW, _btH), 0, 0, _btMinW, _btH, _
                    GraphicsUnit.Pixel)
                If Stretch Then
                    If _isMaxed Then
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), 0, 4 * _btH, _btMaxW, _btH, _
                            GraphicsUnit.Pixel)
                    Else
                        buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 0, _btMaxW, _btH, _
                            GraphicsUnit.Pixel)
                    End If
                Else
                    buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW, 0, _btMaxW, _btH), _btMinW, 3 * _btH, _btMaxW, _btH, _
                        GraphicsUnit.Pixel)
                End If
            End If
            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW, 0, _btExitW, _btH), _btMinW + _btMaxW, 0, _btExitW, _btH, _
                GraphicsUnit.Pixel)
        End If

        If FixedSingle Then
            buff.Graphics.DrawImage(pButtons.Image, New Rectangle(Width - _btL + _btMinW + _btMaxW - 2, 0, 2, _btH), 0, 0, 2, _btH, _
                GraphicsUnit.Pixel)
        End If



        buff.Render(e.Graphics)
        buff.Dispose()
    End Sub

    Private Sub ctlSkin_MouseDown(sender As Object, e As MouseEventArgs)
        _RightX = Me.Width - e.X
        _DownY = Me.Height - e.Y

        If e.Button = MouseButtons.Left Then
            _isClicked = True
            Me.Invalidate()
        End If

        'Resizing
        If e.Button = MouseButtons.Left AndAlso Not _isMouseIn AndAlso Not _isMaxed AndAlso Stretch Then
            Dim SysCommWparam As UInteger = 0

            If (e.X < 4) AndAlso (e.Y < 4) Then
                SysCommWparam = SC_DRAG_RESIZEUL
                ' ↖
            ElseIf (_RightX < 4) AndAlso (_DownY < 4) Then
                SysCommWparam = SC_DRAG_RESIZEDR
                ' ↘
            ElseIf (e.X < 4) AndAlso (_DownY < 4) Then
                SysCommWparam = SC_DRAG_RESIZEDL
                ' ↙
            ElseIf (_RightX < 4) AndAlso (e.Y < 4) Then
                SysCommWparam = SC_DRAG_RESIZEUR
                ' ↗
            ElseIf (e.X < 4) Then
                SysCommWparam = SC_DRAG_RESIZEL
                ' ←
            ElseIf (_RightX < 4) Then
                SysCommWparam = SC_DRAG_RESIZER
                ' →
            ElseIf (e.Y < 4) Then
                SysCommWparam = SC_DRAG_RESIZEU
                ' ↑
            ElseIf (_DownY < 4) Then
                SysCommWparam = SC_DRAG_RESIZED
            End If
            ' ↓

            If SysCommWparam <> 0 Then
                ReleaseCapture()
                DefWindowProc(Parent.Handle, WM_SYSCOMMAND, CType(SysCommWparam, UIntPtr), IntPtr.Zero)
                Me.Cursor = Cursors.[Default]
            End If
        End If

    End Sub

    Private Sub ctlSkin_MouseUp(sender As Object, e As MouseEventArgs)
        If Not _isMouseIn OrElse e.Button <> MouseButtons.Left Then
            Return
        End If

        _isClicked = False

        Dim frm As System.Windows.Forms.Form = TryCast(Me.Parent, System.Windows.Forms.Form)
        If _isExit Then
            frm.Close()
        End If

        If _isMin Then
            frm.WindowState = FormWindowState.Minimized
        End If

        If _isMax AndAlso Stretch Then
            If Not _isMaxed Then
                _isMaxed = True
                frm.WindowState = FormWindowState.Maximized
            Else
                _isMaxed = False
                frm.WindowState = FormWindowState.Normal

            End If
        End If

        If TryCast(Me.Parent, System.Windows.Forms.Form) Is Nothing Then
            Return
        End If

        Me.Invalidate()
    End Sub

    Private Sub ctlSkin_MouseLeave(sender As Object, e As EventArgs)
        Me.Cursor = Cursors.[Default]
        _isMouseIn = False
        _isClicked = False
        Me.Invalidate()
    End Sub

    Private Sub ctlSkin_MouseDoubleClick(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If Stretch Then
                Dim frm As System.Windows.Forms.Form = TryCast(Me.Parent, System.Windows.Forms.Form)

                If Not _isMaxed Then
                    _isMaxed = True
                    frm.WindowState = FormWindowState.Maximized
                Else
                    _isMaxed = False
                    frm.WindowState = FormWindowState.Normal
                End If
            End If
        End If
    End Sub

    Private Sub ctlSkin_MouseMove(sender As Object, e As MouseEventArgs)
        _RightX = Me.Width - e.X
        _DownY = Me.Height - e.Y

        If 0 < e.Y AndAlso e.Y < _btH AndAlso _btRight < _RightX AndAlso _RightX < _btL Then
            _isMouseIn = True
            Me.Cursor = Cursors.[Default]
        Else
            _isMouseIn = False
        End If


        If _isMouseIn Then
            Me.Invalidate()
            _isReset = True
        End If

        If _isReset AndAlso Not _isMouseIn Then
            Me.Invalidate()
            _isReset = False
        End If




        If Not _isMaxed AndAlso Not _isMouseIn AndAlso e.Y < _TitleBarT AndAlso e.X > 4 AndAlso e.X < Width - 4 AndAlso e.Button = MouseButtons.Left Then
            DefWindowProc(Parent.Handle, WM_SYSCOMMAND, CType(SC_DRAG_MOVE, UIntPtr), IntPtr.Zero)
            ReleaseCapture()
        End If


        If Not _isMaxed AndAlso Stretch AndAlso Not _isMouseIn Then

            If (e.X < 4) AndAlso (e.Y < 4) OrElse (_RightX < 4) AndAlso (_DownY < 4) Then
                Me.Cursor = Cursors.SizeNWSE
            ElseIf (_RightX < 4) AndAlso (e.Y < 4) OrElse ((e.X < 4) AndAlso _DownY < 4) Then
                Me.Cursor = Cursors.SizeNESW
            ElseIf (e.X < 4) OrElse (_RightX < 4) Then
                Me.Cursor = Cursors.SizeWE
            ElseIf (e.Y < 4) OrElse (_DownY < 4) Then
                Me.Cursor = Cursors.SizeNS
            Else
                Me.Cursor = Cursors.[Default]
            End If
        End If
    End Sub

    Private Sub ctlSkin_Resize(sender As Object, e As EventArgs)
        If TryCast(Me.Parent, System.Windows.Forms.Form) Is Nothing Then
            Return
        End If

        Me.Invalidate()
    End Sub

    Private Sub ParentForm_TextChanged(sender As Object, e As EventArgs)
        Me.Invalidate()
    End Sub

#End Region

#Region "- Constructor -"
    Public Sub New()
        InitializeComponent()
    End Sub
#End Region

#Region "- Load -"
    Private Sub ctlModernBlack_Load(sender As Object, e As EventArgs)
        AddHandler ParentForm.TextChanged, New System.EventHandler(AddressOf ParentForm_TextChanged)
        ParentForm.FormBorderStyle = FormBorderStyle.None
        SendToBack()
        Dock = DockStyle.Fill

    End Sub
#End Region

End Class
