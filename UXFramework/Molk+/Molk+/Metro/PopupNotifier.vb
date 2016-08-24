#Region "Microsoft.VisualBasic::724efeffc70c71b37dc6241881861793, ..\visualbasic_App\UXFramework\Molk+\Molk+\Metro\PopupNotifier.vb"

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


Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Collections.Generic

''' <summary>
''' Non-visual component to show a notification window in the right lower
''' corner of the screen.
''' </summary>
<ToolboxBitmapAttribute(GetType(PopupNotifier), "Icon.ico")> _
<DefaultEvent("Click")> _
Public Class PopupNotifier : Inherits Component
    ''' <summary>
    ''' Event that is raised when the text in the notification window is clicked.
    ''' </summary>
    Public Event Click As EventHandler

    Private frmPopup As PopupNotifierForm
    Private tmrAnimation As Timer
    Private tmrWait As Timer
    Private timer1 As Timer

    Private isAppearing As Boolean = True
    Private mouseIsOn As Boolean = False
    Private maxPosition As Integer
    Private maxOpacity As Double
    Private posStart As Integer
    Private posStop As Integer
    Private opacityStart As Double
    Private opacityStop As Double
    Private sw As System.Diagnostics.Stopwatch
    Public timelist As New List(Of Long)()

    Public timelist_wait As New List(Of Long)()

    Public message_type As New List(Of String)()
    Public message_cont As New List(Of String)()
    Public message_image As New List(Of Image)()

    Public message_type_wait As New List(Of String)()
    Public message_cont_wait As New List(Of String)()
    Public message_image_wait As New List(Of Image)()


    Const wid As Integer = 220
    Const h_t As Integer = 50
    Private posCurrent As Integer = 0
    Private animation_working As Boolean = False

#Region "Properties"

    <Category("Header"), DefaultValue(GetType(Color), "ControlDark")> _
    <Description("Color of the window header.")> _
    Public Property HeaderColor() As Color
        Get
            Return m_HeaderColor
        End Get
        Set(value As Color)
            m_HeaderColor = Value
        End Set
    End Property
    Private m_HeaderColor As Color

    <Category("Appearance"), DefaultValue(GetType(Color), "Control")> _
    <Description("Color of the window background.")> _
    Public Property BodyColor() As Color
        Get
            Return m_BodyColor
        End Get
        Set(value As Color)
            m_BodyColor = Value
        End Set
    End Property
    Private m_BodyColor As Color

    <Category("Title"), DefaultValue(GetType(Color), "Gray")> _
    <Description("Color of the title text.")> _
    Public Property TitleColor() As Color
        Get
            Return m_TitleColor
        End Get
        Set(value As Color)
            m_TitleColor = Value
        End Set
    End Property
    Private m_TitleColor As Color

    <Category("Content"), DefaultValue(GetType(Color), "ControlText")> _
    <Description("Color of the content text.")> _
    Public Property ContentColor() As Color
        Get
            Return m_ContentColor
        End Get
        Set(value As Color)
            m_ContentColor = Value
        End Set
    End Property
    Private m_ContentColor As Color

    <Category("Appearance"), DefaultValue(GetType(Color), "WindowFrame")> _
    <Description("Color of the window border.")> _
    Public Property BorderColor() As Color
        Get
            Return m_BorderColor
        End Get
        Set(value As Color)
            m_BorderColor = Value
        End Set
    End Property
    Private m_BorderColor As Color

    <Category("Buttons"), DefaultValue(GetType(Color), "WindowFrame")> _
    <Description("Border color of the close and options buttons when the mouse is over them.")> _
    Public Property ButtonBorderColor() As Color
        Get
            Return m_ButtonBorderColor
        End Get
        Set(value As Color)
            m_ButtonBorderColor = Value
        End Set
    End Property
    Private m_ButtonBorderColor As Color

    <Category("Buttons"), DefaultValue(GetType(Color), "Highlight")> _
    <Description("Background color of the close and options buttons when the mouse is over them.")> _
    Public Property ButtonHoverColor() As Color
        Get
            Return m_ButtonHoverColor
        End Get
        Set(value As Color)
            m_ButtonHoverColor = Value
        End Set
    End Property
    Private m_ButtonHoverColor As Color

    <Category("Content"), DefaultValue(GetType(Color), "HotTrack")> _
    <Description("Color of the content text when the mouse is hovering over it.")> _
    Public Property ContentHoverColor() As Color
        Get
            Return m_ContentHoverColor
        End Get
        Set(value As Color)
            m_ContentHoverColor = Value
        End Set
    End Property
    Private m_ContentHoverColor As Color

    <Category("Appearance"), DefaultValue(50)> _
    <Description("Gradient of window background color.")> _
    Public Property GradientPower() As Integer
        Get
            Return m_GradientPower
        End Get
        Set(value As Integer)
            m_GradientPower = Value
        End Set
    End Property
    Private m_GradientPower As Integer

    <Category("Content")> _
    <Description("Font of the content text.")> _
    Public Property ContentFont() As Font
        Get
            Return m_ContentFont
        End Get
        Set(value As Font)
            m_ContentFont = Value
        End Set
    End Property
    Private m_ContentFont As Font

    <Category("Header")> _
    <Description("Font of the title.")> _
    Public Property HeaderFont() As Font
        Get
            Return m_HeaderFont
        End Get
        Set(value As Font)
            m_HeaderFont = Value
        End Set
    End Property
    Private m_HeaderFont As Font


    <Category("Title")> _
    <Description("Font of the title.")> _
    Public Property TitleFont() As Font
        Get
            Return m_TitleFont
        End Get
        Set(value As Font)
            m_TitleFont = Value
        End Set
    End Property
    Private m_TitleFont As Font

    <Category("Image")> _
    <Description("Size of the icon image.")> _
    Public Property ImageSize() As Size
        Get
            If m_imageSize.Width = 0 Then
                If Image IsNot Nothing Then
                    Return Image.Size
                Else
                    Return New Size(0, 0)
                End If
            Else
                Return m_imageSize
            End If
        End Get
        Set(value As Size)
            m_imageSize = Value
        End Set
    End Property

    Public Sub ResetImageSize()
        m_imageSize = Size.Empty
    End Sub

    Private Function ShouldSerializeImageSize() As Boolean
        Return (Not m_imageSize.Equals(Size.Empty))
    End Function

    Private m_imageSize As New Size(0, 0)

    <Category("Image")> _
    <Description("Icon image to display.")> _
    Public Property Image() As Image
        Get
            Return m_Image
        End Get
        Set(value As Image)
            m_Image = Value
        End Set
    End Property
    Private m_Image As Image

    <Category("Header"), DefaultValue(True)> _
    <Description("Whether to show a 'grip' image within the window header.")> _
    Public Property ShowGripText() As Boolean
        Get
            Return m_ShowGripText
        End Get
        Set(value As Boolean)
            m_ShowGripText = Value
        End Set
    End Property
    Private m_ShowGripText As Boolean

    <Category("Header")> _
    <Description("Title text to display.")> _
    Public Property HeaderText() As String
        Get
            Return m_HeaderText
        End Get
        Set(value As String)
            m_HeaderText = Value
        End Set
    End Property
    Private m_HeaderText As String


    <Category("Header")> _
    <Description("Padding of title text.")> _
    Public Property HeaderPadding() As Padding
        Get
            Return m_HeaderPadding
        End Get
        Set(value As Padding)
            m_HeaderPadding = Value
        End Set
    End Property
    Private m_HeaderPadding As Padding

    <Category("Behavior"), DefaultValue(True)> _
    <Description("Whether to scroll the window or only fade it.")> _
    Public Property Scroll() As Boolean
        Get
            Return m_Scroll
        End Get
        Set(value As Boolean)
            m_Scroll = Value
        End Set
    End Property
    Private m_Scroll As Boolean

    <Category("Content")> _
    <Description("Content text to display.")> _
    Public Property ContentText() As String
        Get
            Return m_ContentText
        End Get
        Set(value As String)
            m_ContentText = Value
        End Set
    End Property
    Private m_ContentText As String

    <Category("Title")> _
    <Description("Title text to display.")> _
    Public Property TitleText() As String
        Get
            Return m_TitleText
        End Get
        Set(value As String)
            m_TitleText = Value
        End Set
    End Property
    Private m_TitleText As String



    <Category("Title")> _
    <Description("Padding of title text.")> _
    Public Property TitlePadding() As Padding
        Get
            Return m_TitlePadding
        End Get
        Set(value As Padding)
            m_TitlePadding = Value
        End Set
    End Property
    Private m_TitlePadding As Padding

    Private Sub ResetTitlePadding()
        TitlePadding = Padding.Empty
    End Sub

    Private Function ShouldSerializeTitlePadding() As Boolean
        Return (Not TitlePadding.Equals(Padding.Empty))
    End Function

    <Category("Content")> _
    <Description("Padding of content text.")> _
    Public Property ContentPadding() As Padding
        Get
            Return m_ContentPadding
        End Get
        Set(value As Padding)
            m_ContentPadding = Value
        End Set
    End Property
    Private m_ContentPadding As Padding

    Private Sub ResetContentPadding()
        ContentPadding = Padding.Empty
    End Sub

    Private Function ShouldSerializeContentPadding() As Boolean
        Return (Not ContentPadding.Equals(Padding.Empty))
    End Function

    <Category("Image")> _
    <Description("Padding of icon image.")> _
    Public Property ImagePadding() As Padding
        Get
            Return m_ImagePadding
        End Get
        Set(value As Padding)
            m_ImagePadding = Value
        End Set
    End Property
    Private m_ImagePadding As Padding

    Private Sub ResetImagePadding()
        ImagePadding = Padding.Empty
    End Sub

    Private Function ShouldSerializeImagePadding() As Boolean
        Return (Not ImagePadding.Equals(Padding.Empty))
    End Function

    <Category("Header"), DefaultValue(9)> _
    <Description("Height of window header.")> _
    Public Property HeaderHeight() As Integer
        Get
            Return m_HeaderHeight
        End Get
        Set(value As Integer)
            m_HeaderHeight = Value
        End Set
    End Property
    Private m_HeaderHeight As Integer

    <Category("Buttons"), DefaultValue(True)> _
    <Description("Whether to show the close button.")> _
    Public Property ShowCloseButton() As Boolean
        Get
            Return m_ShowCloseButton
        End Get
        Set(value As Boolean)
            m_ShowCloseButton = Value
        End Set
    End Property
    Private m_ShowCloseButton As Boolean

    <Category("Buttons"), DefaultValue(False)> _
    <Description("Whether to show the options button.")> _
    Public Property ShowOptionsButton() As Boolean
        Get
            Return m_ShowOptionsButton
        End Get
        Set(value As Boolean)
            m_ShowOptionsButton = Value
        End Set
    End Property
    Private m_ShowOptionsButton As Boolean

    <Category("Behavior")> _
    <Description("Context menu to open when clicking on the options button.")> _
    Public Property OptionsMenu() As ContextMenuStrip
        Get
            Return m_OptionsMenu
        End Get
        Set(value As ContextMenuStrip)
            m_OptionsMenu = Value
        End Set
    End Property
    Private m_OptionsMenu As ContextMenuStrip

    <Category("Behavior"), DefaultValue(3000)> _
    <Description("Time in milliseconds the window is displayed.")> _
    Public Property Delay() As Integer
        Get
            Return m_Delay
        End Get
        Set(value As Integer)
            m_Delay = Value
        End Set
    End Property
    Private m_Delay As Integer

    <Category("Behavior"), DefaultValue(1000)> _
    <Description("Time in milliseconds needed to make the window appear or disappear.")> _
    Public Property AnimationDuration() As Integer
        Get
            Return m_AnimationDuration
        End Get
        Set(value As Integer)
            m_AnimationDuration = Value
        End Set
    End Property
    Private m_AnimationDuration As Integer

    <Category("Behavior"), DefaultValue(10)> _
    <Description("Interval in milliseconds used to draw the animation.")> _
    Public Property AnimationInterval() As Integer
        Get
            Return m_AnimationInterval
        End Get
        Set(value As Integer)
            m_AnimationInterval = Value
        End Set
    End Property
    Private m_AnimationInterval As Integer

    <Category("Appearance")> _
    <Description("Size of the window.")> _
    Public Property Size() As Size
        Get
            Return m_Size
        End Get
        Set(value As Size)
            m_Size = Value
        End Set
    End Property
    Private m_Size As Size

#End Region

    ''' <summary>
    ''' Create a new instance of the popup component.
    ''' </summary>
    Public Sub New()
        ' set default values

        HeaderColor = SystemColors.ControlDark
        BodyColor = SystemColors.Control
        TitleColor = System.Drawing.Color.Gray
        ContentColor = SystemColors.ControlText
        BorderColor = SystemColors.WindowFrame
        ButtonBorderColor = SystemColors.WindowFrame
        ButtonHoverColor = SystemColors.Highlight
        ContentHoverColor = SystemColors.HotTrack
        GradientPower = 50
        ContentFont = SystemFonts.DialogFont
        TitleFont = SystemFonts.CaptionFont
        ShowGripText = True
        Scroll = True
        TitlePadding = New Padding(0)
        ContentPadding = New Padding(0)
        ImagePadding = New Padding(0)
        HeaderHeight = 9
        ShowCloseButton = True
        ShowOptionsButton = False
        Delay = 3000
        AnimationInterval = 20
        AnimationDuration = 1000
        Size = New Size(400, 100)

        frmPopup = New PopupNotifierForm(Me)
        frmPopup.TopMost = True
        frmPopup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        frmPopup.StartPosition = System.Windows.Forms.FormStartPosition.Manual
        frmPopup.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        AddHandler frmPopup.MouseEnter, New EventHandler(AddressOf frmPopup_MouseEnter)
        AddHandler frmPopup.MouseLeave, New EventHandler(AddressOf frmPopup_MouseLeave)
        AddHandler frmPopup.MouseClick, New MouseEventHandler(AddressOf frmPopup_MouseClick)

        '    frmPopup.CloseClick += new EventHandler(frmPopup_CloseClick);
        '    frmPopup.LinkClick += new EventHandler(frmPopup_LinkClick);
        '    frmPopup.ContextMenuOpened += new EventHandler(frmPopup_ContextMenuOpened);
        '   frmPopup.ContextMenuClosed += new EventHandler(frmPopup_ContextMenuClosed);

        tmrAnimation = New Timer()
        AddHandler tmrAnimation.Tick, New EventHandler(AddressOf tmAnimation_Tick)

        tmrWait = New Timer()
        AddHandler tmrWait.Tick, New EventHandler(AddressOf tmWait_Tick)

        timer1 = New Timer()
        AddHandler timer1.Tick, New EventHandler(AddressOf timer1_Tick)

        timer1.Enabled = False
    End Sub

    Private Sub frmPopup_MouseClick(sender As [Object], e As MouseEventArgs)


        If e.X > frmPopup.Width - 5 - 16 And e.X < frmPopup.Width - 5 And e.Y > 2 And e.Y < 18 Then
            Hide()
        End If
    End Sub


    Private Function getcurrenttime() As Long
        Return (DateTime.Now.ToFileTime() \ 10000000)
    End Function

    ''' <summary>
    ''' Show the notification window if it is not already visible.
    ''' If the window is currently disappearing, it is shown again.
    ''' </summary>
    Private fresh_count As Integer = 0
    Private pop_show_time As Integer = 5
    'sec
    Private do_once As Boolean = True
    Private thisLock As New [Object]()
    Private image_type As New List(Of Image)()

    Public Sub popup(strTitle As String, strContent As String, nTimeToShow As Integer, nTimeToStay As Integer, nTimeToHide As Integer, notify As Integer, _
        imageshow As Image)


        System.Diagnostics.Debug.WriteLine("New message to show")

        isAppearing = True
        If do_once = True Then
            timer1.Enabled = False
            frmPopup.Opacity = 1
            frmPopup.Show()
            do_once = False

            animation_working = False
        End If

        '       tmrAnimation.Stop();



        If timelist.Count > 4 Then

            timelist_wait.Add(getcurrenttime())
            message_cont_wait.Add(strTitle)
            message_type_wait.Add(strContent)


            message_image_wait.Add(imageshow)
        Else



            timelist.Add(pop_show_time + getcurrenttime())
            message_type.Add(strTitle)
            message_cont.Add(strContent)

            message_image.Add(imageshow)
        End If



        frmPopup.cont_post = timelist.Count
        frmPopup.timelist_data = timelist
        frmPopup.message_cont_show = message_cont
        frmPopup.message_type_show = message_type
        frmPopup.message_image_show = message_image




        If timer1.Enabled <> True Then
            timer1.Enabled = True
            'milisecond
            timer1.Interval = pop_show_time * (1000)
        End If



        If timelist_wait.Count = 0 Then
            refreshform(True)
        End If




    End Sub

    Private Sub refreshform(anim As Boolean)



        '   tmrAnimation.Stop();

        Size = New Size(wid, 20 + (h_t * timelist.Count))

        System.Diagnostics.Debug.WriteLine(timelist.Count)
        If anim = False Then
            'while going down content need to be refreshed first then and then displayed on screen 
            frmPopup.Refresh()
            frmPopup.Size = Size
        Else
            'while animation is on form size refresh not require 
            frmPopup.Size = Size
        End If

        frmPopup.painting_require = False

        If Scroll = True And anim = True Then
            If animation_working = True Then
                posStart = posCurrent
                System.Diagnostics.Debug.WriteLine("Postion start ")
                System.Diagnostics.Debug.WriteLine(posStart)


                posStop = Screen.PrimaryScreen.WorkingArea.Bottom - (frmPopup.Height)
            Else
                posStart = Screen.PrimaryScreen.WorkingArea.Bottom - (h_t * (timelist.Count - 1) + 20)
                posStop = Screen.PrimaryScreen.WorkingArea.Bottom - (frmPopup.Height)
            End If
        Else
            posStart = Screen.PrimaryScreen.WorkingArea.Bottom - (frmPopup.Height)
            posStop = Screen.PrimaryScreen.WorkingArea.Bottom - (frmPopup.Height)
        End If

        opacityStart = 1
        opacityStop = 1

        frmPopup.Location = New Point(Screen.PrimaryScreen.WorkingArea.Right - frmPopup.Size.Width - 1, posStart)
        System.Diagnostics.Debug.WriteLine("value at refresh")
        System.Diagnostics.Debug.WriteLine(frmPopup.Location.Y.ToString())



        If anim = True Then
            animation_working = True
            frmPopup.Opacity = 1
            tmrAnimation.Interval = AnimationInterval
            tmrAnimation.Start()
            sw = System.Diagnostics.Stopwatch.StartNew()
            System.Diagnostics.Debug.WriteLine("Animation started.")
        Else
            frmPopup.Opacity = 1
        End If

        frmPopup.painting_require = True
        ' referesh require to fill netw







    End Sub



    Private timerLock As New [Object]()

    Private Sub timer1_Tick(sender As Object, e As EventArgs)

        timer1.Enabled = False


        If timelist.Count > 0 Then

            For i As Integer = 0 To timelist.Count - 1

                If i > timelist.Count - 1 Then
                    Exit For
                End If

                If timelist.Count > 0 AndAlso timelist(i) <= getcurrenttime() Then

                    timelist.RemoveAt(i)
                    message_cont.RemoveAt(i)
                    message_type.RemoveAt(i)
                    message_image.RemoveAt(i)


                    i -= 1
                End If
            Next
            System.Diagnostics.Debug.WriteLine("Current timeout removed list count" & timelist.Count)
            Dim temp_count As Integer = 5 - timelist.Count
            For j As Integer = 0 To temp_count - 1
                If timelist_wait.Count > 0 Then

                    timelist.Add(getcurrenttime() + pop_show_time)
                    message_type.Add(message_cont_wait(0))
                    message_cont.Add(message_type_wait(0))
                    message_image.Add(message_image_wait(0))

                    System.Diagnostics.Debug.WriteLine("waiting list count" & timelist_wait.Count)
                    System.Diagnostics.Debug.WriteLine(message_cont_wait(0))
                    timelist_wait.RemoveAt(0)
                    message_cont_wait.RemoveAt(0)
                    message_type_wait.RemoveAt(0)

                    message_image_wait.RemoveAt(0)
                Else
                    Exit For

                End If
            Next

            If timelist.Count > 0 Then

                System.Diagnostics.Debug.WriteLine("showing list count" & timelist.Count)
                Dim temp As Integer = CInt(timelist(0) - getcurrenttime())



                If temp > 0 Then
                    System.Diagnostics.Debug.WriteLine("Next timeout of list update system" & temp)
                    timer1.Interval = (temp * 1000)
                    timer1.Enabled = True
                Else


                    timer1.Interval = 1
                    timer1.Enabled = True


                End If
            Else

                'none





            End If
        Else
            'none

        End If






        frmPopup.cont_post = timelist.Count
        frmPopup.timelist_data = timelist
        frmPopup.message_cont_show = message_cont
        frmPopup.message_type_show = message_type
        frmPopup.message_image_show = message_image

        System.Diagnostics.Debug.WriteLine("refresh from function called" & timelist.Count)

        refreshform(False)
        ' while coming down downpart is cut and then bought down cause flicks

        ' Clearing form
        '  frmPopup.Show();
        If timelist.Count = 0 Then

            animation_working = False
            sw.Reset()
            tmrAnimation.[Stop]()
            do_once = True
            frmPopup.Hide()
            System.Diagnostics.Debug.WriteLine("from hide call")
            isAppearing = False
        End If



        '  listBox1.DataSource = null;

        '   listBox1.DataSource = timelist;

    End Sub


    ''' <summary>
    ''' Hide the notification window.
    ''' </summary>
    Public Sub Hide()
        frmPopup.painting_require = False
        '      System.Diagnostics.Debug.WriteLine("Animation stopped.");
        '     System.Diagnostics.Debug.WriteLine("Wait timer stopped.");

        tmrWait.[Stop]()

        animation_working = False
        sw.Reset()
        tmrAnimation.[Stop]()
        do_once = True
        frmPopup.Hide()
        '    System.Diagnostics.Debug.WriteLine("from hide call");
        isAppearing = False
        frmPopup.cont_post = 0
        frmPopup.timelist_data.Clear()
        frmPopup.message_cont_show.Clear()
        frmPopup.message_type_show.Clear()
        timelist.Clear()


        timelist_wait.Clear()

        message_type.Clear()
        message_cont.Clear()
        message_image.Clear()

        message_type_wait.Clear()
        message_cont_wait.Clear()
        message_image_wait.Clear()
    End Sub




    ''' <summary>
    ''' Update form position and opacity to show/hide the window.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>

    Private Sub tmAnimation_Tick(sender As Object, e As EventArgs)
        frmPopup.painting_require = False

        Dim elapsed As Long = sw.ElapsedMilliseconds

        posCurrent = CInt(posStart + ((posStop - posStart) * elapsed \ AnimationDuration))
        Dim neg As Boolean = (posStop - posStart) < 0
        If (neg AndAlso posCurrent < posStop) OrElse (Not neg AndAlso posCurrent > posStop) Then
            posCurrent = posStop
        End If



        frmPopup.increasehight(posCurrent)
        frmPopup.Top = posCurrent
        frmPopup.painting_require = True

        '        System.Diagnostics.Debug.WriteLine(frmPopup.Top);


        If elapsed > AnimationDuration Then

            animation_working = False
            sw.Reset()
            '     System.Diagnostics.Debug.WriteLine("Animation stopped.");

            tmrAnimation.[Stop]()
        End If


    End Sub

    ''' <summary>
    ''' The wait timer has elapsed, start the animation to hide the window.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub tmWait_Tick(sender As Object, e As EventArgs)
        System.Diagnostics.Debug.WriteLine("Wait timer elapsed.")
        tmrWait.[Stop]()
        tmrAnimation.Interval = AnimationInterval
        tmrAnimation.Start()
        sw.[Stop]()
        sw.Start()
        System.Diagnostics.Debug.WriteLine("Animation started.")
    End Sub

    ''' <summary>
    ''' Start wait timer if the mouse leaves the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' 
    Private Sub frmPopup_MouseLeave(sender As Object, e As EventArgs)

        If frmPopup.Visible Then
            timer1.Start()
        End If


        '  System.Diagnostics.Debug.WriteLine("MouseLeave");
        '            if (frmPopup.Visible && (OptionsMenu == null || !OptionsMenu.Visible))
        '            {
        '                tmrWait.Interval = Delay;
        '                tmrWait.Start();
        '                System.Diagnostics.Debug.WriteLine("Wait timer started.");
        '            }
        '            mouseIsOn = false;
        '           

    End Sub


    ''' <summary>
    ''' Stop wait timer if the mouse enters the form.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub frmPopup_MouseEnter(sender As Object, e As EventArgs)

        If isAppearing Then
            timer1.[Stop]()
        End If

        '
        '            System.Diagnostics.Debug.WriteLine("MouseEnter");
        '            if (!isAppearing)
        '            {
        '                frmPopup.Top = maxPosition;
        '                frmPopup.Opacity = maxOpacity;
        '                tmrAnimation.Stop();
        '                System.Diagnostics.Debug.WriteLine("Animation stopped.");
        '            }
        '
        '            tmrWait.Stop();
        '            System.Diagnostics.Debug.WriteLine("Wait timer stopped.");
        '
        '            mouseIsOn = true;
        '             

    End Sub

    Dim DisposedValue As Boolean = False

    ''' <summary>
    ''' Dispose the notification form.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overrides Sub Dispose(disposing As Boolean)
        If Not DisposedValue Then
            If disposing AndAlso frmPopup IsNot Nothing Then
                frmPopup.Dispose()
            End If
            DisposedValue = True
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
