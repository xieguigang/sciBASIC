#Region "Microsoft.VisualBasic::59ea10586fcbceaa66120606c12f7a7e, ..\visualbasic_App\UXFramework\MetroUI Form\MetroUI Form\PopupNotifierForm.vb"

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
' *	Created/modified in 2011 by Simon Baer
' *	
' *  Based on the Code Project article by Nicolas Wälti:
' *  http://www.codeproject.com/KB/cpp/PopupNotifier.aspx
' * 
' *  Licensed under the Code Project Open License (CPOL).
' 


Imports System.Windows.Forms
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Collections.Generic

''' <summary>
''' This is the form of the actual notification window.
''' </summary>
Friend Class PopupNotifierForm : Inherits Microsoft.VisualBasic.Forms.MetroUI.Form
    ''' <summary>
    ''' Event that is raised when the text is clicked.
    ''' </summary>
    Public Event LinkClick As EventHandler

    ''' <summary>
    ''' Event that is raised when the notification window is manually closed.
    ''' </summary>
    Public Event CloseClick As EventHandler

    Friend Event ContextMenuOpened As EventHandler
    Friend Event ContextMenuClosed As EventHandler

    Private mouseOnClose As Boolean = False
    Private mouseOnLink As Boolean = False
    Private mouseOnOptions As Boolean = False
    Private heightOfTitle As Integer
    Private mouseinside As Boolean = False
    Public painting_require As Boolean = False

#Region "GDI objects"

    Private gdiInitialized As Boolean = False
    Private rcBody As Rectangle
    Private rcHeader As Rectangle
    Private rcForm As Rectangle
    Private RectClose1 As Rectangle
    Private brushBody As LinearGradientBrush
    Private brushHeader As LinearGradientBrush
    Private brushButtonHover As Brush
    Private penButtonBorder As Pen
    Private penContent As Pen
    Private penBorder As Pen
    Private brushForeColor As Brush
    Private brushLinkHover As Brush
    Private brushContent As Brush
    Private brushTitle As Brush
    Public paintreq As [Boolean] = False

    Private _item As New Dictionary(Of String, String)()
    '  public List<string> _item = new List<string>();
    Public cont_post As Integer
    Public frm_hight As Integer = 70
    Public frm_wide As Integer = 220
    Private spacing As Integer = 50
    Public timelist_data As New List(Of Long)()
    Public message_type_show As New List(Of String)()
    Public message_cont_show As New List(Of String)()
    Public message_image_show As New List(Of Image)()

#End Region

    ''' <summary>
    ''' Create a new instance.
    ''' </summary>
    ''' <param name="parent__1">PopupNotifier</param>
    Public Sub New(parent__1 As PopupNotifier)
        Parent = parent__1
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.UserPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.ShowInTaskbar = False

        AddHandler Me.VisibleChanged, New EventHandler(AddressOf PopupNotifierForm_VisibleChanged)
        AddHandler Me.MouseMove, New MouseEventHandler(AddressOf PopupNotifierForm_MouseMove)
        AddHandler Me.MouseUp, New MouseEventHandler(AddressOf PopupNotifierForm_MouseUp)
        AddHandler Me.Paint, New PaintEventHandler(AddressOf PopupNotifierForm_Paint)
        AddHandler Me.MouseEnter, New EventHandler(AddressOf PopupNotifierForm_MouseEnter)


        'this.Paint -= new PaintEventHandler(PopupNotifierForm_Paint);
        AddHandler Me.MouseLeave, New EventHandler(AddressOf PopupNotifierForm_MouseLeave)
    End Sub





    Private Sub PopupNotifierForm_MouseEnter(sender As Object, e As EventArgs)
        mouseinside = True
        Invalidate()
    End Sub




    Private Sub PopupNotifierForm_MouseLeave(sender As Object, e As EventArgs)

        mouseinside = False
        Invalidate()
    End Sub

    ''' <summary>
    ''' The form is shown/hidden.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PopupNotifierForm_VisibleChanged(sender As Object, e As EventArgs)
        If Me.Visible Then
            mouseOnClose = False
            mouseOnLink = False
            mouseOnOptions = False
        End If
    End Sub

    ''' <summary>
    ''' Used in design mode.
    ''' </summary>
    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ClientSize = New System.Drawing.Size(392, 66)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "PopupNotifierForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Me.TopMost = True
        Me.ResumeLayout(False)


    End Sub

    ''' <summary>
    ''' Gets or sets the parent control.
    ''' </summary>
    Public Shadows Property Parent() As PopupNotifier
        Get
            Return m_Parent
        End Get
        Set(value As PopupNotifier)
            m_Parent = value
        End Set
    End Property
    Private Shadows m_Parent As PopupNotifier

    ''' <summary>
    ''' Add two values but do not return a value greater than 255.
    ''' </summary>
    ''' <param name="input">first value</param>
    ''' <param name="add">value to add</param>
    ''' <returns>sum of both values</returns>
    Private Function AddValueMax255(input As Integer, add As Integer) As Integer
        Return If((input + add < 256), input + add, 255)
    End Function

    ''' <summary>
    ''' Subtract two values but do not returns a value below 0.
    ''' </summary>
    ''' <param name="input">first value</param>
    ''' <param name="ded">value to subtract</param>
    ''' <returns>first value minus second value</returns>
    Private Function DedValueMin0(input As Integer, ded As Integer) As Integer
        Return If((input - ded > 0), input - ded, 0)
    End Function

    ''' <summary>
    ''' Returns a color which is darker than the given color.
    ''' </summary>
    ''' <param name="color">Color</param>
    ''' <returns>darker color</returns>
    Private Function GetDarkerColor(color As Color) As Color
        Return System.Drawing.Color.FromArgb(255, DedValueMin0(CInt(color.R), Parent.GradientPower), DedValueMin0(CInt(color.G), Parent.GradientPower), DedValueMin0(CInt(color.B), Parent.GradientPower))
    End Function

    ''' <summary>
    ''' Returns a color which is lighter than the given color.
    ''' </summary>
    ''' <param name="color">Color</param>
    ''' <returns>lighter color</returns>
    Private Function GetLighterColor(color As Color) As Color
        Return System.Drawing.Color.FromArgb(255, AddValueMax255(CInt(color.R), Parent.GradientPower), AddValueMax255(CInt(color.G), Parent.GradientPower), AddValueMax255(CInt(color.B), Parent.GradientPower))
    End Function

    ''' <summary>
    ''' Gets the rectangle of the content text.
    ''' </summary>
    Private ReadOnly Property RectContentText() As RectangleF
        Get
            If Parent.Image IsNot Nothing Then
                Return New RectangleF(Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right + Parent.ContentPadding.Left, Parent.HeaderHeight + Parent.TitlePadding.Top + heightOfTitle + Parent.TitlePadding.Bottom + Parent.ContentPadding.Top, Me.Width - Parent.ImagePadding.Left - Parent.ImageSize.Width - Parent.ImagePadding.Right - Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5, Me.Height - Parent.HeaderHeight - Parent.TitlePadding.Top - heightOfTitle - Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1)
            Else
                Return New RectangleF(Parent.ContentPadding.Left, Parent.HeaderHeight + Parent.TitlePadding.Top + heightOfTitle + Parent.TitlePadding.Bottom + Parent.ContentPadding.Top, Me.Width - Parent.ContentPadding.Left - Parent.ContentPadding.Right - 16 - 5, Me.Height - Parent.HeaderHeight - Parent.TitlePadding.Top - heightOfTitle - Parent.TitlePadding.Bottom - Parent.ContentPadding.Top - Parent.ContentPadding.Bottom - 1)
            End If
        End Get
    End Property

    ''' <summary>
    ''' gets the rectangle of the close button.
    ''' </summary>
    Private ReadOnly Property RectClose() As Rectangle
        Get
            Return New Rectangle(Me.Width - 5 - 16, Parent.HeaderHeight + 3, 16, 16)
        End Get
    End Property

    ''' <summary>
    ''' Gets the rectangle of the options button.
    ''' </summary>
    Private ReadOnly Property RectOptions() As Rectangle
        Get
            Return New Rectangle(Me.Width - 5 - 16, Parent.HeaderHeight + 3 + 16 + 5, 16, 16)
        End Get
    End Property

    ''' <summary>
    ''' Update form to display hover styles when the mouse moves over the notification form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PopupNotifierForm_MouseMove(sender As Object, e As MouseEventArgs)
        If Parent.ShowCloseButton Then
            mouseOnClose = RectClose1.Contains(e.X, e.Y)
        End If
        If Parent.ShowOptionsButton Then
            mouseOnOptions = RectOptions.Contains(e.X, e.Y)
        End If
        mouseOnLink = RectContentText.Contains(e.X, e.Y)
        Invalidate()
    End Sub

    ''' <summary>
    ''' A mouse button has been released, check if something has been clicked.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PopupNotifierForm_MouseUp(sender As Object, e As MouseEventArgs)
        If e.Button = System.Windows.Forms.MouseButtons.Left Then
            If RectClose.Contains(e.X, e.Y) Then
                RaiseEvent CloseClick(Me, EventArgs.Empty)
            End If
            If RectContentText.Contains(e.X, e.Y) Then
                RaiseEvent LinkClick(Me, EventArgs.Empty)
            End If
            If RectOptions.Contains(e.X, e.Y) AndAlso (Parent.OptionsMenu IsNot Nothing) Then
                RaiseEvent ContextMenuOpened(Me, EventArgs.Empty)
                Parent.OptionsMenu.Show(Me, New Point(RectOptions.Right - Parent.OptionsMenu.Width, RectOptions.Bottom))
                AddHandler Parent.OptionsMenu.Closed, New ToolStripDropDownClosedEventHandler(AddressOf OptionsMenu_Closed)
            End If
        End If
    End Sub

    ''' <summary>
    ''' The options popup menu has been closed.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub OptionsMenu_Closed(sender As Object, e As ToolStripDropDownClosedEventArgs)
        RemoveHandler Parent.OptionsMenu.Closed, New ToolStripDropDownClosedEventHandler(AddressOf OptionsMenu_Closed)

        RaiseEvent ContextMenuClosed(Me, EventArgs.Empty)
    End Sub


    ''' <summary>
    ''' Free all GDI objects.
    ''' </summary>
    Private Sub DisposeGDIObjects()
        If gdiInitialized Then
            brushBody.Dispose()
            brushHeader.Dispose()
            brushButtonHover.Dispose()
            penButtonBorder.Dispose()
            penContent.Dispose()
            penBorder.Dispose()
            brushForeColor.Dispose()
            brushLinkHover.Dispose()
            brushContent.Dispose()
            brushTitle.Dispose()
        End If
    End Sub


    ''' <summary>
    ''' Draw the notification form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' 




    Private Sub PopupNotifierForm_Paint(sender As Object, e As PaintEventArgs)
        If True = painting_require Then

            ' draw window
            If cont_post = 1 Then
                rcBody = New Rectangle(0, 0, Me.Width, frm_hight)
                rcHeader = New Rectangle(0, 0, Me.Width, Parent.HeaderHeight)
                rcForm = New Rectangle(0, 0, Me.Width - 1, (frm_hight - 1))
                '     RectClose1 = new Rectangle(this.Width - 5 - 16, Parent.HeaderHeight + 3 , 16, 16);
                'frmPopup_CloseClick

                RectClose1 = New Rectangle(Me.Width - 5 - 16, 2, 16, 16)
            Else

                rcBody = New Rectangle(0, 0, Me.Width, ((cont_post) * 75))
                rcHeader = New Rectangle(0, 0, Me.Width, Parent.HeaderHeight)
                '  RectClose1 = new Rectangle(this.Width - 5 - 16, Parent.HeaderHeight + 3, 16, 16);

                RectClose1 = New Rectangle(Me.Width - 5 - 16, 2, 16, 16)
            End If

            Dim brushBody As Brush = New SolidBrush(Parent.BodyColor)
            Dim brushHeader As Brush = New SolidBrush(Parent.HeaderColor)
            e.Graphics.FillRectangle(brushBody, rcBody)
            e.Graphics.FillRectangle(brushHeader, rcHeader)

            '   System.Diagnostics.Debug.WriteLine("paint called");
            For i As Integer = 0 To cont_post - 1


                showcontent(e, _item, i)
            Next
        End If





    End Sub

    Public Sub increasehight(current_heightvalue As Integer)
        Me.Height = Screen.PrimaryScreen.WorkingArea.Bottom - current_heightvalue
    End Sub



    Private paint_once As Boolean = True

    Private Sub showcontent(e As PaintEventArgs, _item As Dictionary(Of String, String), cont_position As Integer)




        brushButtonHover = New SolidBrush(Parent.ButtonHoverColor)
        penButtonBorder = New Pen(Parent.ButtonBorderColor)
        penContent = New Pen(Parent.ContentColor, 2)
        penBorder = New Pen(Parent.BorderColor)
        brushForeColor = New SolidBrush(ForeColor)
        brushLinkHover = New SolidBrush(Parent.ContentHoverColor)
        brushContent = New SolidBrush(Parent.ContentColor)
        brushTitle = New SolidBrush(Parent.TitleColor)
        Dim pen_divde As New Pen(Color.White)
        gdiInitialized = True


        'Dispatcher Test
        If Parent.ShowGripText Then



            e.Graphics.DrawString(Parent.HeaderText, Parent.HeaderFont, brushTitle, CInt((rcHeader.X)), CInt(rcHeader.Y) + 2)
        End If


        'paint of close button
        If Parent.ShowCloseButton Then

            If mouseOnClose Then
                e.Graphics.FillRectangle(brushButtonHover, RectClose1)
                e.Graphics.DrawRectangle(penButtonBorder, RectClose1)
            End If

            If mouseinside = True Then
                e.Graphics.DrawLine(penContent, RectClose1.Left + 4, RectClose1.Top + 4, RectClose1.Right - 4, RectClose1.Bottom - 4)

                e.Graphics.DrawLine(penContent, RectClose1.Left + 4, RectClose1.Bottom - 4, RectClose1.Right - 4, RectClose1.Top + 4)
            End If
        End If



        ' draw icon
        If Parent.Image IsNot Nothing Then

            e.Graphics.DrawImage(message_image_show(cont_position), Parent.ImagePadding.Left, Parent.HeaderHeight + Parent.ImagePadding.Top + (spacing * cont_position), Parent.ImageSize.Width, Parent.ImageSize.Height)
        End If



        ' calculate height of title
        heightOfTitle = CInt(Math.Truncate(e.Graphics.MeasureString("A", Parent.TitleFont).Height))
        Dim titleX As Integer = Parent.TitlePadding.Left
        If Parent.Image IsNot Nothing Then
            titleX += Parent.ImagePadding.Left + Parent.ImageSize.Width + Parent.ImagePadding.Right
        End If

        ' draw message type

        e.Graphics.DrawString(message_type_show(cont_position), Parent.TitleFont, brushTitle, titleX + 5, Parent.HeaderHeight + Parent.TitlePadding.Top + (spacing * cont_position))
        'draw message detail;
        Dim temp_string_length As Integer = message_cont_show(cont_position).Length
        If temp_string_length < 25 Then
            e.Graphics.DrawString(message_cont_show(cont_position).Substring(0, 25), Parent.ContentFont, brushContent, titleX + 5, Parent.HeaderHeight + 18 + Parent.TitlePadding.Top + (spacing * cont_position))
        Else
            e.Graphics.DrawString(message_cont_show(cont_position).Substring(0, 25).Trim(), Parent.ContentFont, brushContent, titleX + 5, Parent.HeaderHeight + 18 + Parent.TitlePadding.Top + (spacing * cont_position))



            e.Graphics.DrawString(message_cont_show(cont_position).Substring(26).Trim(), Parent.ContentFont, brushContent, titleX + 5, Parent.HeaderHeight + 28 + Parent.TitlePadding.Top + (spacing * cont_position))
        End If
        '   e.Graphics.DrawString(Parent.ContentText, Parent.ContentFont, brushContent, RectContentText);
        If cont_position > 0 Then
            Dim start_point As New Point(Parent.ImagePadding.Left, 19 + (50 * cont_position))
            Dim end_point As New Point(Parent.ImagePadding.Left + 200, 19 + (50 * cont_position))
            e.Graphics.DrawLine(pen_divde, start_point, end_point)
        End If
    End Sub


    ''' <summary>
    ''' Dispose GDI objects.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overrides Sub Dispose(disposing As Boolean)

        MyBase.Dispose(disposing)
    End Sub
End Class
