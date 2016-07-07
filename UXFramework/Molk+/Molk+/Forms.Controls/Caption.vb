#Region "Microsoft.VisualBasic::597235af8b409518e74ca802377592ba, ..\VisualBasic_AppFramework\UXFramework\Molk+\Molk+\Forms.Controls\Caption.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports Microsoft.VisualBasic.Imaging

Namespace Windows.Forms.Controls

    Public Class Caption

#Region "Events"

        Public Class WinFormStateChangeEventArgs
            Public Property PreviousState As FormWindowState
            Public Property CurrentState As FormWindowState

            Public Overrides Function ToString() As String
                Return $"{PreviousState} ==> {CurrentState}"
            End Function
        End Class

        Public Event CallFormClose(sender As Object)
        Public Event CallFormMinimize(sender As Object, args As WinFormStateChangeEventArgs)
        Public Event CallFormMaximize(sender As Object, args As WinFormStateChangeEventArgs)
        Public Event CallFromRestore(sender As Object, args As WinFormStateChangeEventArgs)
        Public Event LoadResources(sender As Object)

#End Region

#Region "Properties & Fields"

        Dim __uiResources As New Visualise.Elements.CaptionResources
        Dim _subCaption As String

        Public ReadOnly Property MoveScreenHandle As MolkPlusTheme.API.MoveScreen

        Public Property ShowMinimizedButton As Boolean
            Get
                Return Minimize.Visible
            End Get
            Set(value As Boolean)
                Minimize.Visible = value
            End Set
        End Property

        Public Property ShowMaximizeButton As Boolean
            Get
                Return Maximize.Visible
            End Get
            Set(value As Boolean)
                Maximize.Visible = value
                Call __resizeControlbox(0)
            End Set
        End Property

        Public Sub SetIcon()
            If __uiResources.Icon Is Nothing Then
                Return
            End If
            ParentForm.Icon = __uiResources.GetIcon
        End Sub

        ''' <summary>
        ''' 可以通过设置这个属性也可以设置到父窗体的图标
        ''' </summary>
        ''' <returns></returns>
        <Category("Appearance")> <Description("The icon of this window.(当前的目标窗体的窗体图标)")>
        Public Property Icon As Drawing.Image
            Get
                Return __uiResources.Icon
            End Get
            Set(value As Drawing.Image)
                If value Is Nothing Then
                    Return
                End If

                __uiResources.Icon = value
                Call Update()
            End Set
        End Property

        <Localizable(True)>
        <Category("Appearance")> <Description("The main title of this window.(当前的目标窗体的显示标题)")>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
                If Not ParentForm Is Nothing Then ParentForm.Text = value
                Call Update()
            End Set
        End Property

        <Localizable(True)>
        <Category("Appearance")> <Description("The sub title of this window.(当前的目标窗体的副显示标题)")>
        Public Property SubCaption As String
            Get
                Return _subCaption
            End Get
            Set(value As String)
                _subCaption = value
                Call Update()
            End Set
        End Property

        Public Property AutoHandleFormCloseEvent As Boolean = True
        Public Property AutoHandleFormMinimizeEvent As Boolean = True
        Public Property AutoHandleFormMaximizeEvent As Boolean = True

        ''' <summary>
        ''' 当前的控件主题在这里更换
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property UIThemes As Visualise.Elements.CaptionResources
            Get
                Return __uiResources
            End Get
            Set(value As Visualise.Elements.CaptionResources)
                If value Is Nothing Then
                    value = New Visualise.Elements.CaptionResources
                End If

                __uiResources = value

                Call Me.Update()
                Call Me.ResizedLayout()
                Call Me.ToolTip1.SetToolTip(Minimize, __uiResources.MinButtonTooltip)
                Call Me.ToolTip1.SetToolTip(Maximize, __uiResources.MaxButtonTooltip)
                Call Me.ToolTip1.SetToolTip(Close, __uiResources.CloseButtonTooltip)
            End Set
        End Property

        <DefaultValue(True)>
        <Category("Behavior")> <Description("Enable the target form move on the screen or not.(允许目标窗体对象是否可以在屏幕之上移动)")>
        Public Property FormMove As Boolean
            Get
                If MoveScreenHandle Is Nothing Then
                    Return False
                End If
                Return MoveScreenHandle.Enabled
            End Get
            Set(value As Boolean)
                If Not MoveScreenHandle Is Nothing Then
                    MoveScreenHandle.Enabled = value
                End If
            End Set
        End Property

        Dim _transparentToParentForm As Boolean

        Public Property TransparentToParentForm As Boolean
            Get
                Return _transparentToParentForm
            End Get
            Set(value As Boolean)
                _transparentToParentForm = value

                If TransparentToParentForm AndAlso Not ParentForm Is Nothing AndAlso Not ParentForm.BackgroundImage Is Nothing Then
                    Call UpdateTransparentToParent()
                Else
                    Call Update()
                End If
            End Set
        End Property

        Public Property ShowCaptionText As Boolean
            Get
                Return __uiResources.ShowText
            End Get
            Set(value As Boolean)
                __uiResources.ShowText = value
                Call Update()
            End Set
        End Property

        Public Property ShowIcon As Boolean
            Get
                Return __uiResources.ShowIcon
            End Get
            Set(value As Boolean)
                __uiResources.ShowIcon = value
                Call Update()
            End Set
        End Property
#End Region

        Private Sub UpdateTransparentToParent()
            Dim Bitmap As New Bitmap(CInt(Me.ParentForm.Width), CInt(Me.Height))
            Dim Gr = Graphics.FromImage(Bitmap)

            Gr.CompositingQuality = Drawing2D.CompositingQuality.HighQuality

            Dim Crop = Me.ParentForm.BackgroundImage.ImageCrop(New Point, Bitmap.Size)
            Call Gr.DrawImage(Crop, New Point)

            Me.BackgroundImage = Bitmap
        End Sub

        Private Sub Caption_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
            If Not AutoHandleFormMaximizeEvent Then
                Return
            End If

            Dim Args As New WinFormStateChangeEventArgs With {
                .PreviousState = ParentForm.WindowState
            }
            If ParentForm.WindowState = FormWindowState.Maximized Then
                ParentForm.WindowState = FormWindowState.Normal
                Args.CurrentState = FormWindowState.Normal
                RaiseEvent CallFromRestore(Me, Args)
            Else
                ParentForm.WindowState = FormWindowState.Maximized
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0};  size={1},  minimum_size={2}", Me.Text, Size.ToString, MinimumSize.ToString)
        End Function

        Private Sub Caption_FormClose() Handles Me.CallFormClose
            If AutoHandleFormCloseEvent Then
                Call ParentForm.Close()
            End If
        End Sub

        Private Sub Caption_Load(sender As Object, e As EventArgs) Handles MyBase.Load

            If Me.__uiResources Is Nothing Then
                Me.__uiResources = New Visualise.Elements.CaptionResources
            End If

            Me.Location = New Point With {.X = 0, .Y = 0}
            Me.Dock = DockStyle.Top
            Me.Size = New Size(ParentForm.Width, Height)
            Me._MoveScreenHandle = New MolkPlusTheme.API.MoveScreen(Me)
            Me._MoveScreenHandle.JoinHandle(Me.InformationArea)

            Call Me.Update()

            AddHandler ParentForm.TextChanged, Sub() Me.Text = ParentForm.Text
            AddHandler ParentForm.SizeChanged, Sub() ToolTip1.SetToolTip(Maximize, IIf(ParentForm.WindowState = FormWindowState.Maximized, "Restore", "Maximize"))

            RaiseEvent LoadResources(Me)
        End Sub

        ''' <summary>
        ''' Updates the ui visualize of this caption bar.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overloads Sub Update()
            If __uiResources.BackgroundImage Is Nothing Then
                __uiResources.BackgroundImage = New Visualise.Elements.ImageResource(BackColor)
            End If
            Call __uiResources.BackgroundImage.SetResources(Me)
            If __uiResources.InformationArea Is Nothing Then
                __uiResources.InformationArea = New Visualise.Elements.ImageResource(BackColor)
            End If
            Call __uiResources.DrawCaption(Text, SubCaption, Me)
            InformationArea.Width = __uiResources.TitleLength(Me.Text, Me.SubCaption)

            If Not String.IsNullOrEmpty(_subCaption) Then
                Call ToolTip1.SetToolTip(InformationArea, String.Format("{0} - {1}", Me.Text, Me.SubCaption))
            Else
                Call ToolTip1.SetToolTip(InformationArea, Text)
            End If

            Minimize.UI = __uiResources.Minimize
            Maximize.UI = __uiResources.Maximum
            Close.UI = __uiResources.Close

            If __uiResources.ControlBox Is Nothing Then
                __uiResources.ControlBox = New Visualise.Elements.ImageResource(BackColor)
            End If
            Call __uiResources.ControlBox.SetResources(Controlbox)
        End Sub

#Region "Resource => Layouts"

        Private Sub __resizeControlbox(borderWidth As Integer)
            Dim btn_Height As Integer = __uiResources.ControlboxButtonSize.Height
            Dim btn_Width As Integer = __uiResources.ControlboxButtonSize.Width

            Close.Size = New Size(btn_Width, btn_Height)
            Minimize.Size = New Size(btn_Width, btn_Height)
            Maximize.Size = New Size(btn_Width, btn_Height)
            Controlbox.Size = New Size(btn_Width * 3, btn_Height)
            Controlbox.Location = New Point With {.X = Width - Controlbox.Width - borderWidth, .Y = borderWidth}
            Close.Location = New Point With {.X = Controlbox.Width - Close.Width, .Y = 1}

            If ShowMaximizeButton Then '对于最大化而言，在修改了是否可见之后，最小化的按钮需要调整位置

                '正常位置
                Minimize.Location = New Point With {.X = Controlbox.Width - btn_Width * 3 - borderWidth, .Y = 1}
                Maximize.Location = New Point With {.X = Controlbox.Width - btn_Width * 2 - borderWidth, .Y = 1}

            Else

                '没有显示最大化按钮，最小化按钮会与关闭按钮显示在临近的位置
                Minimize.Location = New Point With {.X = Controlbox.Width - btn_Width * 2 - borderWidth, .Y = 1}

            End If
        End Sub

        ''' <summary>
        ''' 只在加载完UI资源的时候执行一次，其他的情况靠系统来自动排版
        ''' </summary>
        Public Sub ResizedLayout() ' Handles Me.Resize
            On Error Resume Next

            Dim BorderWidth As Integer = If(__uiResources.BorderPen Is Nothing, 0, __uiResources.BorderPen.Width)

            Me.Dock = DockStyle.Top

            If Me.Height <> 44 Then
                Me.Size = New Size(ParentForm.Width - 2 * BorderWidth - Location.X, __uiResources.HeightLimits - 1)
            Else
                Me.Size = New Size(ParentForm.Width - 2 * BorderWidth - Location.X, Height - 1)
            End If

            Call __resizeControlbox(BorderWidth)

            InformationArea.Height = Height - 1
            InformationArea.Location = New Point With {.X = BorderWidth, .Y = BorderWidth}
        End Sub
#End Region

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)

            If Not __uiResources.BorderPen Is Nothing Then
                Call MolkPlusTheme.Windows.Forms.Form.PaintBorder(e.Graphics, __uiResources.BorderPen, Me.Size, Bottom:=False)
            End If
        End Sub

        Private Sub Minimize_Click(sender As Object, e As EventArgs) Handles Minimize.Click
            Dim args As New WinFormStateChangeEventArgs With {.PreviousState = ParentForm.WindowState}
            If AutoHandleFormMinimizeEvent Then ParentForm.WindowState = FormWindowState.Minimized
            args.CurrentState = ParentForm.WindowState
            RaiseEvent CallFormMinimize(Me, args)
        End Sub

        Private Sub Close_Click(sender As Object, e As EventArgs) Handles Close.Click
            RaiseEvent CallFormClose(Me)
        End Sub

        Private Sub Maximize_Click(sender As Object, e As EventArgs) Handles Maximize.Click
            Dim args As New WinFormStateChangeEventArgs With {.PreviousState = ParentForm.WindowState}
            RaiseEvent CallFormMaximize(Me, args)

            If Not AutoHandleFormMaximizeEvent Then
                Return
            End If

            If ParentForm.WindowState = FormWindowState.Maximized Then
                ParentForm.WindowState = FormWindowState.Normal
            Else
                ParentForm.WindowState = FormWindowState.Maximized
            End If
        End Sub

        Private Sub Caption_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
            Me.Maximize.ResetToNormal()
            Me.Minimize.ResetToNormal()
            Me.Close.ResetToNormal()
        End Sub

        Private Sub Close_MouseEnter(sender As Object, e As EventArgs) Handles Close.MouseEnter
            Me.Maximize.ResetToNormal()
            Me.Minimize.ResetToNormal()
        End Sub

        Private Sub Maximize_MouseEnter(sender As Object, e As EventArgs) Handles Maximize.MouseEnter
            Me.Close.ResetToNormal()
            Me.Minimize.ResetToNormal()
        End Sub

        Private Sub Minimize_MouseEnter(sender As Object, e As EventArgs) Handles Minimize.MouseEnter
            Me.Close.ResetToNormal()
            Me.Maximize.ResetToNormal()
        End Sub
    End Class
End Namespace
