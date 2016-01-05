Imports System.Text.RegularExpressions

Public Class Ping

    Public Property Interval As Integer
        Get
            Return Timer1.Interval
        End Get
        Set(value As Integer)
            Timer1.Interval = value
        End Set
    End Property

    Dim _IPAddress As String = Microsoft.VisualBasic.Net.AsynInvoke.LocalIPAddress

    ''' <summary>
    ''' Ping操作的目标机器的IP地址
    ''' </summary>
    ''' <returns></returns>
    Public Property IPAddress As String
        Get
            Return _IPAddress
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) OrElse Not System.Net.IPAddress.TryParse(value, Nothing) Then
                Return
            End If

            _IPAddress = value
            Call Timer1_Tick(Nothing, Nothing)
        End Set
    End Property

    Public Property HostName As String
        Get
            Return _IPAddress
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then
                Return
            End If
#Disable Warning
            IPAddress = System.Net.Dns.Resolve(value).AddressList(0).ToString
#Enable Warning
        End Set
    End Property

    Public Sub Start()
        If String.IsNullOrEmpty(IPAddress) OrElse Interval = 0 Then
            Return
        End If

        Timer1.Enabled = True
        Timer1.Start()
    End Sub

    Public Sub [Stop]()
        Timer1.Stop()
        Timer1.Stop()
    End Sub

    Public Overloads Sub Invoke()
        Call Timer1_Tick(Nothing, Nothing)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim IPAddress As System.Net.IPAddress = System.Net.IPAddress.Parse(Me.IPAddress)

        If Not String.Equals(IPAddress.ToString, Regex.Match(IPAddress.ToString, "\d+(\.\d+){3}").Value) Then
            Return
        End If

        Dim Ping As Double = Microsoft.VisualBasic.Net.PingUtility.Ping(IP:=IPAddress)
        Dim Level As Integer

        If Ping < 50 Then
            'Good 满格信号，蓝色
            Level = 5
            Call ToolTip1.SetToolTip(Me, $"网络延时 {Ping}ms, 与服务器的通信质量非常好")

        ElseIf Ping < 100 Then
            Level = 4
            Call ToolTip1.SetToolTip(Me, $"网络延时 {Ping}ms, 体验比较好")

        ElseIf Ping < 800 Then
            Level = 3
            Call ToolTip1.SetToolTip(Me, $"网络延时 {Ping}ms, 有一点卡")

        ElseIf Ping < 1200 Then
            Level = 2
            Call ToolTip1.SetToolTip(Me, $"网络延时 {Ping}ms, 体验较差")

        Else
            Level = 1
            Call ToolTip1.SetToolTip(Me, $"网络延时 {Ping}ms, 很可能经常性的与服务器失去联系，请考虑更换网络")

            RaiseEvent NetworkFailure(Ping)

        End If

        Call Updates(Level, Ping)
    End Sub

    ''' <summary>
    ''' 假设网络延迟很高的话，就会触发这个事件，提示用户
    ''' </summary>
    ''' <param name="ping"></param>
    Public Event NetworkFailure(ping As Double)

    Public Sub Updates(Level As Integer, Ping As Double)
        Dim RECT = __createFramework()
        Dim Gr As GDIPlusDeviceHandle = Me.Size.CreateGDIDevice(BackColor)

        For Each Rectange As Rectangle In RECT
            Call Gr.Gr_Device.FillRectangle(Brushes.LightGray, Rectange)
        Next

        Dim Color As SolidBrush

        If Level = 5 Then
            Color = New SolidBrush(Drawing.Color.FromArgb(1, 124, 217))
        ElseIf Level = 4 Then
            Color = New SolidBrush(Drawing.Color.FromArgb(68, 209, 9))
        ElseIf Level = 3 Then
            Color = New SolidBrush(Drawing.Color.FromArgb(255, 157, 19))
        ElseIf Level = 2 Then
            Color = New SolidBrush(Drawing.Color.FromArgb(255, 119, 0))
        Else
            Color = New SolidBrush(Drawing.Color.FromArgb(238, 13, 7))
        End If

        For i As Integer = Level - 1 To 0 Step -1
            Call Gr.Gr_Device.FillRectangle(Color, RECT(i))
        Next

        Dim Text As String = Ping & "ms"
        Dim Font As Font = New Font(FONT_FAMILY_MICROSOFT_YAHEI, 9)
        Dim Size = Gr.Gr_Device.MeasureString(Text, Font)

        Call Gr.Gr_Device.DrawString(Text, Font, Brushes.Black, New Point(RECT.Last.Right + 3, 0.5 * (Height - RECT.Last.Y - Size.Height) + RECT.Last.Y))

        Me.BackgroundImage = Gr.ImageResource
    End Sub

    ''' <summary>
    ''' 生成5个信号格子
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function __createFramework() As Rectangle()

        Dim Y As Integer = Me.Height * 0.95
        Dim X As Integer = Me.Width * 0.05
        Dim H As Integer = Me.Height * 0.2
        Dim Width As Integer = Me.Width / 18
        Dim Interval As Integer = Width / 4

        Width = Width - Interval

        Dim GetRECT = Function() New Drawing.Rectangle(New Point(X, Y - H), New Size(Width, H))

        '最弱的信号
        Dim Lowest As Rectangle = GetRECT()

        H += Me.Height * 0.1
        X += Width + Interval

        Dim Low As Rectangle = GetRECT()

        H += Me.Height * 0.15
        X += Width + Interval

        Dim Bad As Rectangle = GetRECT()

        H += Me.Height * 0.2
        X += Width + Interval

        Dim Middle As Rectangle = GetRECT()

        H += Me.Height * 0.1
        X += Width + Interval

        Dim Good As Rectangle = GetRECT()

        Return {Lowest, Low, Bad, Middle, Good}
    End Function

    Private Sub Ping_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        Call Invoke()
    End Sub
End Class
