#Region "Microsoft.VisualBasic::a09ea9faf654ed74a9d2eb825c57ec76, ..\visualbasic_App\UXFramework\Molk+\Molk+\Forms.Controls\ToolTips\ToolTip.vb"

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

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Design
Imports System.Windows.Forms.Design
''' <summary>
''' Custom tooltip.
''' </summary>
<ProvideProperty("ToolTip", GetType(System.Windows.Forms.Control)),
ProvideProperty("ToolTipTitle", GetType(System.Windows.Forms.Control)),
ProvideProperty("ToolTipImage", GetType(System.Windows.Forms.Control))>
Public Class ToolTip
    Inherits System.ComponentModel.Component
    Implements IExtenderProvider
#Region "Public Events"
    ''' <summary>
    ''' Event before tooltip is displayed.  This event is raised when OwnerDraw property is set to true.
    ''' </summary>
    Public Event Popup( sender As Object,  e As PopupEventArgs)
    ''' <summary>
    ''' Event when the tooltip surface is drawn.  This event is raised when OwnerDraw property is set to true.
    ''' </summary>
    Public Event Draw( sender As Object,  e As DrawEventArgs)
    ''' <summary>
    ''' Event when the tooltip background is drawn.  This event is raised when OwnerDrawBackground property is set to true.
    ''' </summary>
    Public Event DrawBackground( sender As Object,  e As DrawEventArgs)
#End Region
#Region "Declarations"
    Private _parent As System.Windows.Forms.Control = Nothing ' Owner of this tooltip
    Private _control As System.Windows.Forms.Control = Nothing
    Private _animationSpeed As Integer = 20 ' In milliseconds, interval to fade - in / out
    Private _showShadow As Boolean = True
    Private _form As TooltipForm ' Form to display the tooltip
    Private _autoClose As Integer = 3000 ' In milliseconds, tooltip will automatically closed if this period passed
    Private _enableAutoClose As Boolean = True
    Private _ownerDraw As Boolean = False
    Private _ownerDrawBackground As Boolean = False
    Private _location As ToolTipLocation = ToolTipLocation.Auto
    Private _customLocation As Point = New Point(0, 0)
    ' Support for IExtenderProvider
    Private _texts As Hashtable
    Private _titles As Hashtable
    Private _images As Hashtable
#End Region

    Public ReadOnly Property Control As System.Windows.Forms.Control
        Get
            Return _control
        End Get
    End Property

#Region "Public Methods"
    ''' <summary>
    ''' Constructor of the tooltip with an owner control specified.
    ''' </summary>
    Public Sub New( parent As System.Windows.Forms.Control)
        _parent = parent
        _texts = New Hashtable
        _titles = New Hashtable
        _images = New Hashtable
        _ownerDraw = True
    End Sub
    ''' <summary>
    ''' ToolTip constructor.
    ''' </summary>
    Public Sub New()
        _texts = New Hashtable
        _titles = New Hashtable
        _images = New Hashtable
        _ownerDraw = False
    End Sub
    ''' <summary>
    ''' Show ToolTip with specified control.
    ''' </summary>
    Public Sub show( control As System.Windows.Forms.Control)
        TooltipForm._showShadow = _showShadow
        _control = control
        If Not _form Is Nothing Then _form.invokeClose()
        Dim tooltipSize As Size
        If _ownerDraw Or _ownerDrawBackground Then
            Dim e As PopupEventArgs
            e = New PopupEventArgs
            RaiseEvent Popup(Me, e)
            tooltipSize = e.Size
        Else
            Dim tTitle As String = GetToolTipTitle(_control)
            Dim tText As String = GetToolTip(_control)
            Dim tImage As Image = GetToolTipImage(_control)
            tooltipSize = Renderer.ToolTip.measureSize(tTitle, tText, tImage)
        End If
        _form = New TooltipForm(Me, tooltipSize)
    End Sub
    ''' <summary>
    ''' Show ToolTip with specified control and location.  The ToolTip location is relative to the control.
    ''' </summary>
    Public Sub show( control As System.Windows.Forms.Control,  location As Point)
        TooltipForm._showShadow = _showShadow
        _control = control
        If Not _form Is Nothing Then _form.invokeClose()
        Dim tooltipSize As Size
        If _ownerDraw Or _ownerDrawBackground Then
            Dim e As PopupEventArgs
            e = New PopupEventArgs
            RaiseEvent Popup(Me, e)
            tooltipSize = e.Size
        Else
            Dim tTitle As String = GetToolTipTitle(_control)
            Dim tText As String = GetToolTip(_control)
            Dim tImage As Image = GetToolTipImage(_control)
            tooltipSize = Renderer.ToolTip.measureSize(tTitle, tText, tImage)
        End If
        _form = New TooltipForm(Me, tooltipSize, location)
    End Sub
    ''' <summary>
    ''' Show ToolTip with specified control and rectangle area.  This area is where the tooltip must avoid to cover.
    ''' </summary>
    Public Sub show( control As System.Windows.Forms.Control,  rect As Rectangle)
        TooltipForm._showShadow = _showShadow
        _control = control
        If Not _form Is Nothing Then _form.invokeClose()
        Dim tooltipSize As Size
        If _ownerDraw Or _ownerDrawBackground Then
            Dim e As PopupEventArgs
            e = New PopupEventArgs
            RaiseEvent Popup(Me, e)
            tooltipSize = e.Size
        Else
            Dim tTitle As String = GetToolTipTitle(_control)
            Dim tText As String = GetToolTip(_control)
            Dim tImage As Image = GetToolTipImage(_control)
            tooltipSize = Renderer.ToolTip.measureSize(tTitle, tText, tImage)
        End If
        _form = New TooltipForm(Me, tooltipSize, rect)
    End Sub
    ''' <summary>
    ''' Hide the ToolTip.
    ''' </summary>
    Public Sub hide()
        Try
            _form.DoClose()
        Catch ex As Exception
        End Try
    End Sub
    ' Extended property for ToolTip property.
    <EditorAttribute(GetType(System.ComponentModel.Design.MultilineStringEditor), _
        GetType(System.Drawing.Design.UITypeEditor)), DefaultValue("")> _
    Public Function GetToolTip( obj As Object) As String
        Dim tText As String = CType(_texts(obj), String)
        If tText Is Nothing Then
            tText = String.Empty
        End If
        Return tText
    End Function
    <EditorAttribute(GetType(System.ComponentModel.Design.MultilineStringEditor), _
        GetType(System.Drawing.Design.UITypeEditor))> _
    Public Sub SetToolTip( obj As Object,  value As String)
        If value Is Nothing Then
            value = String.Empty
        End If
        If value.Length = 0 Then
            _texts.Remove(obj)
        Else
            _texts(obj) = value
        End If
        If hasToolTip(obj) Then
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                AddHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                AddHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                AddHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                AddHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        Else
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                RemoveHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                RemoveHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                RemoveHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                RemoveHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        End If
    End Sub
    <DefaultValue("")>
    Public Function GetToolTipTitle( ctrl As System.Windows.Forms.Control) As String
        Dim tTitle As String = CType(_titles(ctrl), String)
        If tTitle Is Nothing Then
            tTitle = String.Empty
        End If
        Return tTitle
    End Function
    Public Sub SetToolTipTitle( obj As Object,  value As String)
        If value Is Nothing Then
            value = String.Empty
        End If
        If value.Length = 0 Then
            _titles.Remove(obj)
        Else
            _titles(obj) = value
        End If
        If hasToolTip(obj) Then
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                AddHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                AddHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                AddHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                AddHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        Else
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                RemoveHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                RemoveHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                RemoveHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                RemoveHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        End If
    End Sub
    <EditorAttribute(GetType(System.Drawing.Design.ImageEditor),
        GetType(System.Drawing.Design.UITypeEditor)), DefaultValue(GetType(Image), "Nothing")>
    Public Function GetToolTipImage( ctrl As System.Windows.Forms.Control) As Image
        Return CType(_images(ctrl), Image)
    End Function
    <EditorAttribute(GetType(System.Drawing.Design.ImageEditor), _
        GetType(System.Drawing.Design.UITypeEditor))> _
    Public Sub SetToolTipImage( obj As Object,  value As Image)
        If value Is Nothing Then
            _images.Remove(obj)
        Else
            _images(obj) = value
        End If
        If hasToolTip(obj) Then
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                AddHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                AddHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                AddHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                AddHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                AddHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        Else
            If TypeOf (obj) Is System.Windows.Forms.Control Then
                Dim ctrl As System.Windows.Forms.Control = DirectCast(obj, System.Windows.Forms.Control)
                RemoveHandler ctrl.MouseEnter, AddressOf ctrlMouseEnter
                RemoveHandler ctrl.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler ctrl.MouseDown, AddressOf ctrlMouseDown
            ElseIf TypeOf (obj) Is ToolStripItem Then
                Dim anItem As ToolStripItem = DirectCast(obj, ToolStripItem)
                RemoveHandler anItem.MouseEnter, AddressOf tsiMouseEnter
                RemoveHandler anItem.MouseLeave, AddressOf ctrlMouseLeave
                RemoveHandler anItem.MouseDown, AddressOf ctrlMouseDown
            End If
        End If
    End Sub
    Public Function CanExtend( extendee As Object) As Boolean Implements System.ComponentModel.IExtenderProvider.CanExtend
        If TypeOf (extendee) Is System.Windows.Forms.Control Then
            If TypeOf (extendee) Is System.Windows.Forms.Form Then
                Return False
            Else
                Return True
            End If
        End If
        If TypeOf (extendee) Is ToolStripItem Then Return True
        Return False
    End Function
    ' Disposing components
    Protected Overrides Sub Dispose( disposing As Boolean)
        If disposing Then
            ' Clear all resources
            _texts.Clear()
            _titles.Clear()
            _images.Clear()
            If _form IsNot Nothing Then
                _form.DoClose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub
#End Region
    ' This class, API calls, I got this from an article in CodeProject about layered window (but I forgot)
#Region "Private Class"
#Region "Windows API"
    Private Structure BLENDFUNCTION
        Public BlendOp As Byte
        Public BlendFlags As Byte
        Public SourceConstantAlpha As Byte
        Public AlphaFormat As Byte
    End Structure
    Private Declare Function UpdateLayeredWindow Lib "user32.dll" ( _
         hWnd As IntPtr, _
         hdcDst As IntPtr, _
        ByRef pptDst As Point, _
        ByRef psize As Size, _
         hdcSrc As IntPtr, _
        ByRef pptSrc As Point, _
         crKey As Integer, _
        ByRef pBlend As BLENDFUNCTION, _
         dwFlags As Integer) As Boolean
    Private Declare Function GetDC Lib "user32.dll" ( hWnd As IntPtr) As IntPtr
    Private Declare Function ReleaseDC Lib "user32.dll" ( hWnd As IntPtr, _
         hDC As IntPtr) As Integer
    Private Declare Function CreateCompatibleDC Lib "gdi32.dll" ( hDC As IntPtr) As IntPtr
    Private Declare Function DeleteDC Lib "gdi32.dll" ( hDC As IntPtr) As Boolean
    Private Declare Function SelectObject Lib "gdi32.dll" ( hDC As IntPtr, _
         hObject As IntPtr) As IntPtr
    Private Declare Function DeleteObject Lib "gdi32.dll" ( hObject As IntPtr) As Boolean
    Private Declare Function ShowWindow Lib "user32.dll" ( hWnd As IntPtr, _
         nCmdShow As Integer) As Integer
    Private Declare Function SendMessage Lib "user32.dll" Alias "SendMessageA" ( hwnd As IntPtr, _
         wMsg As Integer,  wParam As Integer, _
         lParam As Integer) As Integer
    Private Declare Sub ReleaseCapture Lib "user32.dll" ()
    Private Const WS_EX_LAYERED = &H80000
    Private Const ULW_ALPHA = &H2
    Private Const AC_SRC_OVER = &H0
    Private Const AC_SRC_ALPHA = &H1
    Private Const WS_EX_TRANSPARENT = &H20&
#End Region
    Private Class TooltipForm
        Inherits System.Windows.Forms.Form
        Public Shared _showShadow As Boolean
        Private _closing As Boolean = False
        Const BORDER_MARGIN As Integer = 1
        Dim _rect As Rectangle
        Dim _path As GraphicsPath
        Dim bgBitmap As Bitmap
        Dim tBitmap As Bitmap
        Private _timer As Timer
        Private _tmrClose As Timer
        Private mNormalPos As Point
        Private mCurrentBounds As Rectangle
        Private mPopup As ToolTip
        Private mTimerStarted As DateTime
        Private mProgress As Double
        Private Const CS_DROPSHADOW As Integer = &H20000
        Private Const SW_NOACTIVATE As Integer = 4
        Private Const WS_EX_TOOLWINDOW = &H80&
        Private Const SWP_NOSIZE As Integer = &H1
        Private Const SWP_NOMOVE As Integer = &H2
        Private Const SWP_NOACTIVATE As Integer = &H10
        Private Const WS_POPUP As Integer = &H80000000
        Private HWND_TOPMOST As IntPtr = New IntPtr(-1)
        Dim mx As Integer, _my As Integer
        Dim _alpha As Integer = 100
        Private Shared mBackgroundImage As Image
        Private Declare Function SetWindowPos Lib "user32.dll" ( _
             hWnd As IntPtr, _
             hWndInsertAfter As IntPtr, _
             x As Integer, _
             y As Integer, _
             cx As Integer, _
             cy As Integer, _
             flags As Integer) As Integer
        Sub New( popup As ToolTip,  size As System.Drawing.Size)
            Dim aPadding As System.Windows.Forms.Padding
            mPopup = popup
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
            StartPosition = FormStartPosition.Manual
            Me.ShowInTaskbar = False
            Me.DockPadding.All = BORDER_MARGIN
            aPadding.All = 3
            If mPopup._parent IsNot Nothing Then
                Dim parentForm As System.Windows.Forms.Form = Me.mPopup._parent.FindForm
                If Not parentForm Is Nothing Then
                    parentForm.AddOwnedForm(Me)
                End If
            Else
                If mPopup._control IsNot Nothing Then
                    Dim parentForm As System.Windows.Forms.Form = Me.mPopup._control.FindForm
                    If Not parentForm Is Nothing Then
                        parentForm.AddOwnedForm(Me)
                    End If
                End If
            End If
            Me.Padding = aPadding
            If mPopup._showShadow Then
                size.Width = size.Width + 10
                size.Height = size.Height + 10
            Else
                size.Width = size.Width + 6
                size.Height = size.Height + 6
            End If
            Me.MaximumSize = size
            Me.MinimumSize = size
            bgBitmap = New Bitmap(size.Width, size.Height)
            tBitmap = New Bitmap(size.Width, size.Height)
            ReLocate()
            ' Initialize the animation
            mProgress = 0
            Dim aRect As Rectangle = New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
            _path = Renderer.Drawing.roundedRectangle(aRect, 2, 2, 2, 2)
            Me.Location = mNormalPos
            _timer = New Timer
            _tmrClose = New Timer
            _tmrClose.Interval = mPopup._autoClose
            AddHandler _tmrClose.Tick, AddressOf AutoClosing
            drawBitmap()
            If mPopup._animationSpeed > 0 Then
                _alpha = 0
                ' I always aim 25 images per seconds.. seems to be a good value
                ' it looks smooth enough on fast computers and do not drain slower one
                _timer.Interval = mPopup._animationSpeed
                mTimerStarted = Now
                AddHandler _timer.Tick, AddressOf Showing
                _timer.Start()
                Showing(Nothing, Nothing)
            Else
                setBitmap(bgBitmap)
            End If
            'If mPopup.mDialog Then
            '    ShowDialog()
            'Else
            '    Show()
            'End If
            ShowWindow(Me.Handle, SW_NOACTIVATE)
            SetWindowPos(Me.Handle, HWND_TOPMOST, Me.Left, Me.Top, Me.Width, Me.Height, SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE)
            If mPopup._enableAutoClose Then _tmrClose.Start()
        End Sub
        Sub New( popup As ToolTip,  size As System.Drawing.Size,  location As Point)
            Dim aPadding As System.Windows.Forms.Padding
            mPopup = popup
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
            StartPosition = FormStartPosition.Manual
            Me.ShowInTaskbar = False
            Me.DockPadding.All = BORDER_MARGIN
            aPadding.All = 3
            If mPopup._parent IsNot Nothing Then
                Dim parentForm As System.Windows.Forms.Form = Me.mPopup._parent.FindForm
                If Not parentForm Is Nothing Then
                    parentForm.AddOwnedForm(Me)
                End If
            Else
                If mPopup._control IsNot Nothing Then
                    Dim parentForm As System.Windows.Forms.Form = Me.mPopup._control.FindForm
                    If Not parentForm Is Nothing Then
                        parentForm.AddOwnedForm(Me)
                    End If
                End If
            End If
            Me.Padding = aPadding
            If mPopup._showShadow Then
                size.Width = size.Width + 10
                size.Height = size.Height + 10
            Else
                size.Width = size.Width + 6
                size.Height = size.Height + 6
            End If
            Me.MaximumSize = size
            Me.MinimumSize = size
            bgBitmap = New Bitmap(size.Width, size.Height)
            tBitmap = New Bitmap(size.Width, size.Height)
            ReLocate(location)
            ' Initialize the animation
            mProgress = 0
            Dim aRect As Rectangle = New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
            _path = Renderer.Drawing.roundedRectangle(aRect, 2, 2, 2, 2)
            Me.Location = mNormalPos
            _timer = New Timer
            _tmrClose = New Timer
            _tmrClose.Interval = mPopup._autoClose
            AddHandler _tmrClose.Tick, AddressOf AutoClosing
            drawBitmap()
            If mPopup._animationSpeed > 0 Then
                _alpha = 0
                ' I always aim 25 images per seconds.. seems to be a good value
                ' it looks smooth enough on fast computers and do not drain slower one
                _timer.Interval = mPopup._animationSpeed
                mTimerStarted = Now
                AddHandler _timer.Tick, AddressOf Showing
                _timer.Start()
                Showing(Nothing, Nothing)
            Else
                setBitmap(bgBitmap)
            End If
            'If mPopup.mDialog Then
            '    ShowDialog()
            'Else
            '    Show()
            'End If
            ShowWindow(Me.Handle, SW_NOACTIVATE)
            SetWindowPos(Me.Handle, HWND_TOPMOST, Me.Left, Me.Top, Me.Width, Me.Height, SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE)
            If mPopup._enableAutoClose Then _tmrClose.Start()
        End Sub
        Sub New( popup As ToolTip,  size As System.Drawing.Size, _
             rect As Rectangle)
            Dim aPadding As System.Windows.Forms.Padding
            mPopup = popup
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
            StartPosition = FormStartPosition.Manual
            Me.ShowInTaskbar = False
            Me.DockPadding.All = BORDER_MARGIN
            aPadding.All = 3
            If mPopup._parent IsNot Nothing Then
                Dim parentForm As System.Windows.Forms.Form = Me.mPopup._parent.FindForm
                If Not parentForm Is Nothing Then
                    parentForm.AddOwnedForm(Me)
                End If
            Else
                If mPopup._control IsNot Nothing Then
                    Dim parentForm As System.Windows.Forms.Form = Me.mPopup._control.FindForm
                    If Not parentForm Is Nothing Then
                        parentForm.AddOwnedForm(Me)
                    End If
                End If
            End If
            Me.Padding = aPadding
            If mPopup._showShadow Then
                size.Width = size.Width + 10
                size.Height = size.Height + 10
            Else
                size.Width = size.Width + 6
                size.Height = size.Height + 6
            End If
            Me.MaximumSize = size
            Me.MinimumSize = size
            bgBitmap = New Bitmap(size.Width, size.Height)
            tBitmap = New Bitmap(size.Width, size.Height)
            ReLocate(rect)
            ' Initialize the animation
            mProgress = 0
            Dim aRect As Rectangle = New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
            _path = Renderer.Drawing.roundedRectangle(aRect, 2, 2, 2, 2)
            Me.Location = mNormalPos
            _timer = New Timer
            _tmrClose = New Timer
            _tmrClose.Interval = mPopup._autoClose
            AddHandler _tmrClose.Tick, AddressOf AutoClosing
            drawBitmap()
            If mPopup._animationSpeed > 0 Then
                _alpha = 0
                ' I always aim 25 images per seconds.. seems to be a good value
                ' it looks smooth enough on fast computers and do not drain slower one
                _timer.Interval = mPopup._animationSpeed
                mTimerStarted = Now
                AddHandler _timer.Tick, AddressOf Showing
                _timer.Start()
                Showing(Nothing, Nothing)
            Else
                setBitmap(bgBitmap)
            End If
            'If mPopup.mDialog Then
            '    ShowDialog()
            'Else
            '    Show()
            'End If
            ShowWindow(Me.Handle, SW_NOACTIVATE)
            SetWindowPos(Me.Handle, HWND_TOPMOST, Me.Left, Me.Top, Me.Width, Me.Height, SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE)
            If mPopup._enableAutoClose Then _tmrClose.Start()
        End Sub
        Private Sub drawTransparentBitmap()
            Dim g As Graphics = Graphics.FromImage(tBitmap)
            Dim y As Integer, x As Integer
            Dim aColor As Color, tColor As Color
            g.Clear(Color.Transparent)
            g.Dispose()
            y = 0
            While y < bgBitmap.Height
                x = 0
                While x < bgBitmap.Width
                    aColor = bgBitmap.GetPixel(x, y)
                    tColor = Color.FromArgb(_alpha * aColor.A / 100, aColor.R, aColor.G, aColor.B)
                    tBitmap.SetPixel(x, y, tColor)
                    x = x + 1
                End While
                y = y + 1
            End While
        End Sub
        Private Sub drawBackground( g As Graphics)
            If Not mPopup._ownerDrawBackground Then
                If mPopup._showShadow Then
                    Dim bgBrush As System.Drawing.Drawing2D.LinearGradientBrush
                    Dim aPath As GraphicsPath
                    Dim aRect As Rectangle = New Rectangle(0, 0, Me.Width - 4, Me.Height - 4)
                    Dim rectShadow As Rectangle = New Rectangle(4, 4, Me.Width - 4, Me.Height - 4)
                    Dim pathShadow As GraphicsPath = Renderer.Drawing.roundedRectangle(rectShadow, 4, 4, 4, 4)
                    Dim shadowBrush As PathGradientBrush = New PathGradientBrush(pathShadow)
                    Dim sColor(0 To 3) As Color
                    Dim sPos(0 To 3) As Single
                    Dim sBlend As ColorBlend = New ColorBlend
                    sColor(0) = Color.FromArgb(0, 0, 0, 0)
                    sColor(1) = Color.FromArgb(16, 0, 0, 0)
                    sColor(2) = Color.FromArgb(32, 0, 0, 0)
                    sColor(3) = Color.FromArgb(128, 0, 0, 0)
                    If rectShadow.Width > rectShadow.Height Then
                        sPos(0) = 0.0F
                        sPos(1) = 4 / rectShadow.Width
                        sPos(2) = 8 / rectShadow.Width
                        sPos(3) = 1.0F
                    Else
                        If rectShadow.Width < rectShadow.Height Then
                            sPos(0) = 0.0F
                            sPos(1) = 4 / rectShadow.Height
                            sPos(2) = 8 / rectShadow.Height
                            sPos(3) = 1.0F
                        Else
                            sPos(0) = 0.0F
                            sPos(1) = 4 / rectShadow.Width
                            sPos(2) = 8 / rectShadow.Width
                            sPos(3) = 1.0F
                        End If
                    End If
                    sBlend.Colors = sColor
                    sBlend.Positions = sPos
                    shadowBrush.InterpolationColors = sBlend
                    If rectShadow.Width > rectShadow.Height Then
                        shadowBrush.CenterPoint = New Point( _
                            rectShadow.X + (rectShadow.Width / 2), _
                            rectShadow.Bottom - (rectShadow.Width / 2))
                    Else
                        If rectShadow.Width = rectShadow.Height Then
                            shadowBrush.CenterPoint = New Point( _
                                rectShadow.X + (rectShadow.Width / 2), _
                                rectShadow.Y + (rectShadow.Height / 2))
                        Else
                            shadowBrush.CenterPoint = New Point( _
                                rectShadow.Right - (rectShadow.Height / 2), _
                                rectShadow.Y + (rectShadow.Height / 2))
                        End If
                    End If
                    aPath = Renderer.Drawing.roundedRectangle(aRect, 2, 2, 2, 2)
                    bgBrush = New System.Drawing.Drawing2D.LinearGradientBrush(aRect, _
                        Color.FromArgb(255, 255, 255), Color.FromArgb(201, 217, 239), Drawing2D.LinearGradientMode.Vertical)
                    g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    g.Clear(Color.Transparent)
                    g.FillPath(shadowBrush, pathShadow)
                    g.FillPath(bgBrush, aPath)
                    g.DrawPath(New Pen(Color.FromArgb(118, 118, 118)), aPath)
                    bgBrush.Dispose()
                    aPath.Dispose()
                    pathShadow.Dispose()
                    shadowBrush.Dispose()
                Else
                    Dim bgBrush As System.Drawing.Drawing2D.LinearGradientBrush
                    Dim aPath As GraphicsPath
                    Dim aRect As Rectangle = New Rectangle(0, 0, Me.Width, Me.Height)
                    aPath = Renderer.Drawing.roundedRectangle(aRect, 2, 2, 2, 2)
                    bgBrush = New System.Drawing.Drawing2D.LinearGradientBrush(New Rectangle(0, 0, Me.Width, Me.Height), _
                        Color.FromArgb(255, 255, 255), Color.FromArgb(201, 217, 239), Drawing2D.LinearGradientMode.Vertical)
                    g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                    g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                    g.Clear(Color.Transparent)
                    g.FillPath(bgBrush, aPath)
                    g.DrawPath(New Pen(Color.FromArgb(118, 118, 118)), aPath)
                    bgBrush.Dispose()
                    aPath.Dispose()
                End If
            Else
                g.Clear(Color.Transparent)
                mPopup.invokeDrawBackground(g, New Rectangle(0, 0, Me.Width - 1, Me.Height - 1))
            End If
        End Sub
        Private Sub drawBitmap()
            Dim g As Graphics = Graphics.FromImage(bgBitmap)
            Dim rect As Rectangle
            drawBackground(g)
            If Not mPopup.OwnerDrawBackground Then
                If mPopup._showShadow Then
                    rect.X = 3
                    rect.Y = 3
                    rect.Width = Me.Width - 10
                    rect.Height = Me.Height - 10
                Else
                    rect.X = 3
                    rect.Y = 3
                    rect.Width = Me.Width - 6
                    rect.Height = Me.Height - 6
                End If
            Else
                rect = New Rectangle(0, 0, Me.Width - 1, Me.Height - 1)
            End If
            mPopup.invokeDraw(g, rect)
            g.Dispose()
        End Sub
        Private Sub setBitmap( aBmp As Bitmap)
            Dim screenDC As IntPtr = GetDC(IntPtr.Zero)
            Dim memDC As IntPtr = CreateCompatibleDC(screenDC)
            Dim hBitmap As IntPtr = IntPtr.Zero
            Dim oldBitmap As IntPtr = IntPtr.Zero
            Try
                hBitmap = aBmp.GetHbitmap(Color.FromArgb(0))
                oldBitmap = SelectObject(memDC, hBitmap)

                Dim size As Size = New Size(aBmp.Width, aBmp.Height)
                Dim pointSource As Point = New Point(0, 0)
                Dim topPos As Point = New Point(Me.Left, Me.Top)
                Dim blend As BLENDFUNCTION = New BLENDFUNCTION
                blend.BlendOp = AC_SRC_OVER
                blend.BlendFlags = 0
                blend.SourceConstantAlpha = 255
                blend.AlphaFormat = AC_SRC_ALPHA
                UpdateLayeredWindow(Me.Handle, screenDC, topPos, _
                    size, memDC, pointSource, 0, blend, ULW_ALPHA)
            Catch ex As Exception
            Finally
                ReleaseDC(IntPtr.Zero, screenDC)
                If hBitmap <> IntPtr.Zero Then
                    SelectObject(memDC, oldBitmap)
                    DeleteObject(hBitmap)
                End If
                DeleteDC(memDC)
            End Try
        End Sub
        Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
            Get
                Dim cp As CreateParams = MyBase.CreateParams
                cp.ExStyle = cp.ExStyle Or WS_EX_LAYERED
                Return cp
            End Get
        End Property
        Protected Overloads Overrides Sub Dispose( disposing As Boolean)
            If disposing Then
                If _tmrClose IsNot Nothing Then
                    _tmrClose.Dispose()
                End If
                If _timer IsNot Nothing Then
                    _timer.Dispose()
                End If
                If bgBitmap IsNot Nothing Then
                    bgBitmap.Dispose()
                End If
                If tBitmap IsNot Nothing Then
                    tBitmap.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub
        Private Sub ReLocate()
            Dim rW, rH As Integer
            Dim workingArea As Rectangle = Screen.PrimaryScreen.WorkingArea
            Dim mCursor As Cursor = mPopup._control.Cursor
            rW = Me.Width
            rH = Me.Height
            mNormalPos = System.Windows.Forms.Control.MousePosition
            mNormalPos.X = mNormalPos.X + mCursor.Size.Width
            mNormalPos.Y = mNormalPos.Y + mCursor.Size.Height
            If mNormalPos.X + rW > workingArea.Width Then
                mNormalPos.X = mNormalPos.X - rW
            End If
            If mNormalPos.Y + rH > workingArea.Height Then
                mNormalPos.Y = mNormalPos.Y - (rH + mCursor.Size.Height)
            End If
        End Sub
        Private Sub ReLocate( location As Point)
            Dim rW, rH As Integer
            Dim workingArea As Rectangle = Screen.PrimaryScreen.WorkingArea
            rW = Me.Width
            rH = Me.Height
            mNormalPos = mPopup._control.PointToScreen(location)
            If mNormalPos.X + rW > workingArea.Width Then
                mNormalPos.X = mNormalPos.X - rW
            End If
            If mNormalPos.Y + rH > workingArea.Height Then
                mNormalPos.Y = mNormalPos.Y - rH
            End If
        End Sub
        Private Sub ReLocate( rect As Rectangle)
            Dim rW, rH As Integer
            Dim workingArea As Rectangle = Screen.PrimaryScreen.WorkingArea
            Dim askedLoc As Point
            askedLoc.X = rect.X
            askedLoc.Y = rect.Bottom + 5
            rW = Me.Width
            rH = Me.Height
            mNormalPos = mPopup._control.PointToScreen(askedLoc)
            If mNormalPos.X + rW > workingArea.Width Then
                mNormalPos.X = mNormalPos.X - (rW - rect.Width)
            End If
            If mNormalPos.Y + rH > workingArea.Height Then
                mNormalPos.Y = mNormalPos.Y - (rH + rect.Height + 10)
            End If
        End Sub
        Private Sub Showing( sender As Object,  e As EventArgs)
            If Not _closing Then
                If _alpha = 100 Then
                    _timer.Stop()
                Else
                    Try
                        _alpha = _alpha + 10
                        drawTransparentBitmap()
                        setBitmap(tBitmap)
                    Catch ex As Exception
                        _timer.Stop()
                    End Try
                End If
            Else
                If _alpha = 0 Then
                    _timer.Stop()
                    RemoveHandler _timer.Tick, AddressOf Showing
                    invokeClose()
                Else
                    Try
                        _alpha = _alpha - 10
                        drawTransparentBitmap()
                        setBitmap(tBitmap)
                    Catch ex As Exception
                        _timer.Stop()
                    End Try
                End If
            End If
        End Sub
        Friend Sub DoClose()
            If mPopup._animationSpeed > 0 Then
                _closing = True
                _timer.Start()
            Else
                invokeClose()
            End If
        End Sub
        Friend Sub invokeClose()
            With mPopup
                Try
                Finally
                    ._form.Close()
                    ._form = Nothing
                    If mPopup._parent IsNot Nothing Then
                        Dim parentForm As System.Windows.Forms.Form = Me.mPopup._parent.FindForm
                        If Not parentForm Is Nothing Then
                            parentForm.RemoveOwnedForm(Me)
                        End If
                    Else
                        If mPopup._control IsNot Nothing Then
                            Dim parentForm As System.Windows.Forms.Form = Me.mPopup._control.FindForm
                            If Not parentForm Is Nothing Then
                                parentForm.RemoveOwnedForm(Me)
                            End If
                        End If
                    End If
                    'parentForm.Focus()
                    Close()
                End Try
            End With
        End Sub
        Private Sub AutoClosing( sender As Object,  e As EventArgs)
            DoClose()
        End Sub
    End Class
#End Region
#Region "Private Methods"
    Private Sub invokeDraw( g As Graphics,  rect As Rectangle)
        If _ownerDraw Or _ownerDrawBackground Then
            Dim e As DrawEventArgs
            e = New DrawEventArgs(g, rect)
            RaiseEvent Draw(Me, e)
        Else
            Dim tTitle As String = GetToolTipTitle(_control)
            Dim tText As String = GetToolTip(_control)
            Dim tImage As Image = GetToolTipImage(_control)
            Renderer.ToolTip.drawToolTip(tTitle, tText, tImage, g, rect)
        End If
    End Sub
    Private Sub invokeDrawBackground( g As Graphics,  rect As Rectangle)
        Dim e As DrawEventArgs = New DrawEventArgs(g, rect)
        RaiseEvent DrawBackground(Me, e)
    End Sub
    Private Function hasToolTip( ctrl As System.Windows.Forms.Control) As Boolean
        Dim tText As String = GetToolTip(ctrl)
        Dim tTitle As String = GetToolTipTitle(ctrl)
        Dim tImage As Image = GetToolTipImage(ctrl)
        Return Renderer.ToolTip.containsToolTip(tTitle, tText, tImage)
    End Function
    ' Control's MouseEnter and MouseLeave event handler
    Private Sub ctrlMouseEnter( sender As Object,  e As EventArgs)
        _control = DirectCast(sender, System.Windows.Forms.Control)
        Select Case _location
            Case ToolTipLocation.Auto
                Dim ctrlRect As Rectangle = New Rectangle(0, 0, _control.Bounds.Width, _control.Bounds.Height)
                show(_control, ctrlRect)
            Case ToolTipLocation.MousePointer
                show(_control)
            Case ToolTipLocation.CustomClient
                show(_control, _customLocation)
            Case ToolTipLocation.CustomScreen
                Dim clientLocation As Point = _control.PointToClient(_customLocation)
                show(_control, clientLocation)
        End Select
    End Sub
    Private Sub ctrlMouseLeave( sender As Object,  e As EventArgs)
        If sender Is _control Then
            _control = Nothing
            hide()
        End If
    End Sub
    Private Sub ctrlMouseDown( sender As Object,  e As MouseEventArgs)
        hide()
    End Sub
    ' ToolStripItem's MouseEnter and MouseLeave event handler
    Private Sub tsiMouseEnter( sender As Object,  e As EventArgs)
        Dim anItem As ToolStripItem = DirectCast(sender, ToolStripItem)
        _control = anItem.GetCurrentParent
        Select Case _location
            Case ToolTipLocation.Auto
                Dim itemRect As Rectangle = New Rectangle(anItem.Bounds.X, 0, anItem.Bounds.Width, _control.Height - 2)
                show(_control, itemRect)
            Case ToolTipLocation.MousePointer
                show(_control)
            Case ToolTipLocation.CustomClient
                show(_control, _customLocation)
            Case ToolTipLocation.CustomScreen
                Dim clientLocation As Point = _control.PointToClient(_customLocation)
                show(_control, clientLocation)
        End Select
    End Sub
#End Region
#Region "Public Properties"
    ''' <summary>
    ''' Specifies fade effect period when the tooltip is displayed or hiden, in milliseconds.
    ''' </summary>
    <DefaultValue(20)> _
    Public Property AnimationSpeed() As Integer
        Get
            Return _animationSpeed
        End Get
        Set( Value As Integer)
            _animationSpeed = Value
        End Set
    End Property
    ''' <summary>
    ''' Show the shadow effect of the tooltip.  This property is ignored when OwnerDrawBackground property is set to true.
    ''' </summary>
    <DefaultValue(True)> _
    Public Property ShowShadow() As Boolean
        Get
            Return _showShadow
        End Get
        Set( Value As Boolean)
            _showShadow = Value
        End Set
    End Property
    ''' <summary>
    ''' Period of time the ToolTip is displayed, in milliseconds.
    ''' </summary>
    <DefaultValue(3000)> _
    Public Property AutoClose() As Integer
        Get
            Return _autoClose
        End Get
        Set( value As Integer)
            _autoClose = value
        End Set
    End Property
    ''' <summary>
    ''' Automatically close the ToolTip when the specified time in AutoClose property has been passed.
    ''' </summary>
    <DefaultValue(True)> _
    Public Property EnableAutoClose() As Boolean
        Get
            Return _enableAutoClose
        End Get
        Set( value As Boolean)
            _enableAutoClose = value
        End Set
    End Property
    ''' <summary>
    ''' ToolTip surface will be manually drawn by your code.
    ''' </summary>
    <DefaultValue(False)> _
    Public Property OwnerDraw() As Boolean
        Get
            Return _ownerDraw
        End Get
        Set( value As Boolean)
            _ownerDraw = value
        End Set
    End Property
    ''' <summary>
    ''' ToolTip background will be manually drawn by your code.
    ''' If this property is set to true, the Draw and Popup event will be raised as well, 
    ''' and the whole ToolTip will be drawn by your code.
    ''' </summary>
    <DefaultValue(False)> _
    Public Property OwnerDrawBackground() As Boolean
        Get
            Return _ownerDrawBackground
        End Get
        Set( value As Boolean)
            _ownerDrawBackground = value
        End Set
    End Property
    ''' <summary>
    ''' Determine how the ToolTip will be located.
    ''' </summary>
    <DefaultValue(GetType(ToolTipLocation), "Auto")> _
    Public Property Location() As ToolTipLocation
        Get
            Return _location
        End Get
        Set( value As ToolTipLocation)
            _location = value
        End Set
    End Property
    ''' <summary>
    ''' Custom location where the ToolTip will be displayed.
    ''' Used when the Location property is set CustomScreen or CustomClient.
    ''' </summary>
    <DefaultValue(GetType(Point), "0,0")> _
    Public Property CustomLocation() As Point
        Get
            Return _customLocation
        End Get
        Set( value As Point)
            _customLocation = value
        End Set
    End Property
#End Region
End Class
Public Class PopupEventArgs
    Inherits EventArgs
    Dim _size As System.Drawing.Size
    Public Sub New()
        MyBase.New()
    End Sub
    Public Property Size() As System.Drawing.Size
        Get
            Return _size
        End Get
        Set( value As System.Drawing.Size)
            _size = value
        End Set
    End Property
End Class
Public Class DrawEventArgs
    Inherits EventArgs
    Dim _g As System.Drawing.Graphics
    Dim _rect As Rectangle
    Public Sub New( g As System.Drawing.Graphics,  rect As System.Drawing.Rectangle)
        MyBase.New()
        _g = g
        _rect = rect
    End Sub
    Public ReadOnly Property Graphics() As System.Drawing.Graphics
        Get
            Return _g
        End Get
    End Property
    Public ReadOnly Property Rectangle() As System.Drawing.Rectangle
        Get
            Return _rect
        End Get
    End Property
End Class
''' <summary>
''' Specifies the location of the tooltip will be shown.
''' </summary>
Public Enum ToolTipLocation
    Auto ' Tooltip location will automatically calculated based on caller(Control, ToolStripItem) bounds, usually under the caller.
    MousePointer ' Tooltip will be shown around mouse pointer.
    CustomScreen ' Tooltip will be shown on a location in the screen specified by CustomLocation
    CustomClient ' Tooltip will be shown on a location relative to the client area on the caller.
End Enum
