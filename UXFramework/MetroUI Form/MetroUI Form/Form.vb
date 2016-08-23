#Region "Microsoft.VisualBasic::58d58de5f03bf9179e8a8abd65dc8fa1, ..\visualbasic_App\UXFramework\MetroUI Form\MetroUI Form\Form.vb"

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

Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Drawing
Imports Microsoft.VisualBasic.Forms.MetroUI.API
Imports Microsoft.VisualBasic.Forms.MetroUI.API.WinApi

Public Class Form

#Region "Window Behavior"

#Region "Internal Fields"
    Dim _dwmMargins As Dwm.MARGINS
    Dim _marginOk As Boolean
    Dim _aeroEnabled As Boolean = False
    Dim _InternalSizeLock As Size
    Dim _FormMoveEnabled As Boolean = True
#End Region
#Region "Ctor"
    Public Sub New()
        Call MyBase.New()

        Call SetStyle(ControlStyles.ResizeRedraw, True)
        Call InitializeComponent()

        DoubleBuffered = True
    End Sub
#End Region
#Region "Props"
    Public ReadOnly Property AeroEnabled() As Boolean
        Get
            Return _aeroEnabled
        End Get
    End Property

    Public Overridable Property EnableFormMove As Boolean
        Get
            Return _FormMoveEnabled
        End Get
        Set(value As Boolean)
            _FormMoveEnabled = value
        End Set
    End Property

    Public Property DrawBorderFrame As Boolean = False
#End Region
#Region "Methods"
    Public Shared Function LoWord(ByVal dwValue As Integer) As Integer
        Return dwValue And &HFFFF
    End Function
    ''' <summary>
    ''' Equivalent to the HiWord C Macro
    ''' </summary>
    ''' <param name="dwValue"></param>
    ''' <returns></returns>
    Public Shared Function HiWord(ByVal dwValue As Integer) As Integer
        Return (dwValue >> 16) And &HFFFF
    End Function
#End Region

    Private Sub Form1_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Activated
        If Environment.OSVersion.Platform = PlatformID.Win32NT Then
            Call Dwm.DwmExtendFrameIntoClientArea(Me.Handle, _dwmMargins)
        End If
    End Sub

    Protected Overloads Overrides Sub WndProc(ByRef m As Message)
        Try
            Call Me.InternalWndProcHandle(m)
        Catch ex As Exception
            Call Debug.WriteLine(ex.ToString)
        End Try
    End Sub

    Protected Sub InternalWndProcHandle(ByRef m As Message)
        Dim WM_NCCALCSIZE As Integer = &H83
        Dim WM_NCHITTEST As Integer = &H84
        Dim result As IntPtr

        Dim dwmHandled As Integer = Dwm.DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, result)

        If dwmHandled = 1 Then
            m.Result = result
            Exit Sub
        End If

        If m.Msg = WM_NCCALCSIZE AndAlso CInt(m.WParam) = 1 Then
            Dim nccsp As NCCALCSIZE_PARAMS =
              DirectCast(Marshal.PtrToStructure(m.LParam,
              GetType(NCCALCSIZE_PARAMS)), NCCALCSIZE_PARAMS)

            ' Adjust (shrink) the client rectangle to accommodate the border:
            nccsp.rect0.Top += 0
            nccsp.rect0.Bottom += 0
            nccsp.rect0.Left += 0
            nccsp.rect0.Right += 0

            If Not _marginOk Then
                'Set what client area would be for passing to DwmExtendIntoClientArea. Also remember that at least one of these values NEEDS TO BE > 1, else it won't work.
                _dwmMargins.cyTopHeight = 0
                _dwmMargins.cxLeftWidth = 0
                _dwmMargins.cyBottomHeight = 3
                _dwmMargins.cxRightWidth = 0
                _marginOk = True
            End If

            Marshal.StructureToPtr(nccsp, m.LParam, False)

            m.Result = IntPtr.Zero
        ElseIf m.Msg = WM_NCHITTEST AndAlso CInt(m.Result) = 0 Then
            m.Result = HitTestNCA(m.HWnd, m.WParam, m.LParam)
        Else
            MyBase.WndProc(m)
        End If
    End Sub

    Private Function HitTestNCA(ByVal hwnd As IntPtr, ByVal wparam _
                                      As IntPtr, ByVal lparam As IntPtr) As IntPtr
        Dim HTNOWHERE As Integer = 0
        Dim HTCLIENT As Integer = 1
        Dim HTCAPTION As Integer = 2
        Dim HTGROWBOX As Integer = 4
        Dim HTSIZE As Integer = HTGROWBOX
        Dim HTMINBUTTON As Integer = 8
        Dim HTMAXBUTTON As Integer = 9
        Dim HTLEFT As Integer = 10
        Dim HTRIGHT As Integer = 11
        Dim HTTOP As Integer = 12
        Dim HTTOPLEFT As Integer = 13
        Dim HTTOPRIGHT As Integer = 14
        Dim HTBOTTOM As Integer = 15
        Dim HTBOTTOMLEFT As Integer = 16
        Dim HTBOTTOMRIGHT As Integer = 17
        Dim HTREDUCE As Integer = HTMINBUTTON
        Dim HTZOOM As Integer = HTMAXBUTTON
        Dim HTSIZEFIRST As Integer = HTLEFT
        Dim HTSIZELAST As Integer = HTBOTTOMRIGHT

        Dim p As New Point(LoWord(CInt(lparam)), HiWord(CInt(lparam)))

        Dim topleft As Rectangle = RectangleToScreen(New Rectangle(0, 0, _
                                   _dwmMargins.cxLeftWidth, _dwmMargins.cxLeftWidth))

        If topleft.Contains(p) Then
            Return New IntPtr(HTTOPLEFT)
        End If

        Dim topright As Rectangle = _
          RectangleToScreen(New Rectangle(Width - _dwmMargins.cxRightWidth, 0, _
          _dwmMargins.cxRightWidth, _dwmMargins.cxRightWidth))

        If topright.Contains(p) Then
            Return New IntPtr(HTTOPRIGHT)
        End If

        Dim botleft As Rectangle = _
           RectangleToScreen(New Rectangle(0, Height - _dwmMargins.cyBottomHeight, _
           _dwmMargins.cxLeftWidth, _dwmMargins.cyBottomHeight))

        If botleft.Contains(p) Then
            Return New IntPtr(HTBOTTOMLEFT)
        End If

        Dim botright As Rectangle = _
            RectangleToScreen(New Rectangle(Width - _dwmMargins.cxRightWidth, _
            Height - _dwmMargins.cyBottomHeight, _
            _dwmMargins.cxRightWidth, _dwmMargins.cyBottomHeight))

        If botright.Contains(p) Then
            Return New IntPtr(HTBOTTOMRIGHT)
        End If

        Dim top As Rectangle = _
            RectangleToScreen(New Rectangle(0, 0, Width, _dwmMargins.cxLeftWidth))

        If top.Contains(p) Then
            Return New IntPtr(HTTOP)
        End If

        Dim cap As Rectangle = _
            RectangleToScreen(New Rectangle(0, _dwmMargins.cxLeftWidth, _
            Width, _dwmMargins.cyTopHeight - _dwmMargins.cxLeftWidth))

        If cap.Contains(p) Then
            Return New IntPtr(HTCAPTION)
        End If

        Dim left As Rectangle = _
            RectangleToScreen(New Rectangle(0, 0, _dwmMargins.cxLeftWidth, Height))

        If left.Contains(p) Then
            Return New IntPtr(HTLEFT)
        End If

        Dim right As Rectangle = _
            RectangleToScreen(New Rectangle(Width - _dwmMargins.cxRightWidth, _
            0, _dwmMargins.cxRightWidth, Height))

        If right.Contains(p) Then
            Return New IntPtr(HTRIGHT)
        End If

        Dim bottom As Rectangle = _
            RectangleToScreen(New Rectangle(0, Height - _dwmMargins.cyBottomHeight, _
            Width, _dwmMargins.cyBottomHeight))

        If bottom.Contains(p) Then
            Return New IntPtr(HTBOTTOM)
        End If

        Return New IntPtr(HTCLIENT)
    End Function

    ''' <summary>
    ''' 使用鼠标改变窗口大小的时候所需要使用的一个用于检测的边界大小值
    ''' </summary>
    ''' <remarks></remarks>
    Const BORDER_WIDTH As Integer = 6

    Private _resizeDir As ResizeDirection = ResizeDirection.None

    Private Sub Form1_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Left Then
            If Me.Width - BORDER_WIDTH > e.Location.X AndAlso e.Location.X > BORDER_WIDTH AndAlso e.Location.Y > BORDER_WIDTH Then
                If _FormMoveEnabled Then Call MoveControl(Me.Handle)
            Else
                If Me.WindowState <> FormWindowState.Maximized Then
                    ResizeForm(resizeDir)
                End If
            End If
        End If
    End Sub

    Public Enum ResizeDirection
        None = 0
        Left = 1
        TopLeft = 2
        Top = 4
        TopRight = 8
        Right = 16
        BottomRight = 32
        Bottom = 64
        BottomLeft = 128
    End Enum

    Private Property resizeDir() As ResizeDirection
        Get
            Return _resizeDir
        End Get
        Set(ByVal value As ResizeDirection)
            _resizeDir = value

            'Change cursor
            Select Case value
                Case ResizeDirection.Left
                    Me.Cursor = Cursors.SizeWE

                Case ResizeDirection.Right
                    Me.Cursor = Cursors.SizeWE

                Case ResizeDirection.Top
                    Me.Cursor = Cursors.SizeNS

                Case ResizeDirection.Bottom
                    Me.Cursor = Cursors.SizeNS

                Case ResizeDirection.BottomLeft
                    Me.Cursor = Cursors.SizeNESW

                Case ResizeDirection.TopRight
                    Me.Cursor = Cursors.SizeNESW

                Case ResizeDirection.BottomRight
                    Me.Cursor = Cursors.SizeNWSE

                Case ResizeDirection.TopLeft
                    Me.Cursor = Cursors.SizeNWSE

                Case Else
                    Me.Cursor = Cursors.Default
            End Select
        End Set
    End Property

    ''' <summary>
    ''' Calculate which direction to resize based on mouse position
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Form1_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseMove
        If e.Location.X < BORDER_WIDTH And e.Location.Y < BORDER_WIDTH Then
            resizeDir = ResizeDirection.TopLeft

        ElseIf e.Location.X < BORDER_WIDTH And e.Location.Y > Me.Height - BORDER_WIDTH Then
            resizeDir = ResizeDirection.BottomLeft

        ElseIf e.Location.X > Me.Width - BORDER_WIDTH And e.Location.Y > Me.Height - BORDER_WIDTH Then
            resizeDir = ResizeDirection.BottomRight

        ElseIf e.Location.X > Me.Width - BORDER_WIDTH And e.Location.Y < BORDER_WIDTH Then
            resizeDir = ResizeDirection.TopRight

        ElseIf e.Location.X < BORDER_WIDTH Then
            resizeDir = ResizeDirection.Left

        ElseIf e.Location.X > Me.Width - BORDER_WIDTH Then
            resizeDir = ResizeDirection.Right

        ElseIf e.Location.Y < BORDER_WIDTH Then
            resizeDir = ResizeDirection.Top

        ElseIf e.Location.Y > Me.Height - BORDER_WIDTH Then
            resizeDir = ResizeDirection.Bottom

        Else
            resizeDir = ResizeDirection.None
        End If
    End Sub

    Private Sub MoveControl(ByVal hWnd As IntPtr)
        ReleaseCapture()
        SendMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, 0)
    End Sub

    Private Sub ResizeForm(ByVal direction As ResizeDirection)
        Dim dir As Integer = -1
        Select Case direction
            Case ResizeDirection.Left
                dir = HTLEFT
            Case ResizeDirection.TopLeft
                dir = HTTOPLEFT
            Case ResizeDirection.Top
                dir = HTTOP
            Case ResizeDirection.TopRight
                dir = HTTOPRIGHT
            Case ResizeDirection.Right
                dir = HTRIGHT
            Case ResizeDirection.BottomRight
                dir = HTBOTTOMRIGHT
            Case ResizeDirection.Bottom
                dir = HTBOTTOM
            Case ResizeDirection.BottomLeft
                dir = HTBOTTOMLEFT
        End Select

        If dir <> -1 Then
            ReleaseCapture()
            SendMessage(Me.Handle, WM_NCLBUTTONDOWN, dir, 0)
        End If

    End Sub

    <DllImport("user32.dll")> Public Shared Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll")> Public Shared Function SendMessage(hWnd As IntPtr, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function

    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTBORDER As Integer = 18
    Private Const HTBOTTOM As Integer = 15
    Private Const HTBOTTOMLEFT As Integer = 16
    Private Const HTBOTTOMRIGHT As Integer = 17
    Private Const HTCAPTION As Integer = 2
    Private Const HTLEFT As Integer = 10
    Private Const HTRIGHT As Integer = 11
    Private Const HTTOP As Integer = 12
    Private Const HTTOPLEFT As Integer = 13
    Private Const HTTOPRIGHT As Integer = 14
#End Region

    Dim _InternalWindowState As FormWindowState
    Dim _InternalBorderWidth As Integer = 1
    Dim _InternalBorderPen As System.Drawing.Pen = Pens.Gray

    Public Property BorderWidth As Integer
        Get
            Return _InternalBorderWidth
        End Get
        Set(value As Integer)
            _InternalBorderWidth = value
            Call Me.Invalidate()
        End Set
    End Property

    Public Property BorderColor As Color
        Get
            Return _InternalBorderPen.Color
        End Get
        Set(value As Color)
            If value = Nothing OrElse value.IsEmpty Then
                value = Color.Gray
            End If
            _InternalBorderPen = New Pen(value, BorderWidth)
        End Set
    End Property

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Call MyBase.OnPaintBackground(e)

        If Not DrawBorderFrame Then
            Return
        End If

        Dim rect As Rectangle

        If BorderWidth = 1 Then
            rect = New Rectangle(0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
        ElseIf BorderWidth <= 3 Then
            rect = New Rectangle(0, 0, Me.ClientSize.Width - BorderWidth, Me.ClientSize.Height - BorderWidth)
        Else
            rect = New Rectangle(0, 0, Me.ClientSize.Width - BorderWidth / 3, Me.ClientSize.Height - BorderWidth / 3)
        End If
        Call e.Graphics.DrawRectangle(_InternalBorderPen, rect)
    End Sub

    Protected Overrides Sub OnSizeChanged(e As EventArgs)
        If Me._InternalWindowState <> WindowState Then
            _InternalWindowState = WindowState
            Size = _InternalSizeLock
        Else
            _InternalSizeLock = Size
        End If

        Call MyBase.OnSizeChanged(e)
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _InternalWindowState = WindowState
    End Sub
End Class
