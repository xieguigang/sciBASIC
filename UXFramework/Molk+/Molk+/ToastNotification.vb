Public Class ToastNotification

    ''' <summary>
    ''' 通过这个对象来显示打开的动画
    ''' </summary>
    ''' <returns></returns>
    Public Property InvokeDisplay As Action(Of ToastNotification)
    ''' <summary>
    ''' 通过这个对象来显示关闭的动画
    ''' </summary>
    ''' <returns></returns>
    Public Property InvokeClose As Action(Of ToastNotification)

    ''' <summary>
    ''' 显示一行提示消息
    ''' </summary>
    ''' <param name="Message"></param>
    Public Sub ShowMessage(Message As String)
        Label1.Text = Message

        If Not InvokeDisplay Is Nothing Then
            Call _InvokeDisplay(Me)
        Else
            Me.Visible = True
            BringToFront()
        End If
    End Sub

    Private Sub CloseButton_Click(sender As Object, e As EventArgs) Handles CloseButton.Click
        If Not InvokeClose Is Nothing Then
            Call _InvokeClose(Me)
        Else
            Visible = False
        End If
    End Sub

    Private Sub ToastNotification_Load(sender As Object, e As EventArgs) Handles Me.Load
        On Error Resume Next

        AddHandler Parent.Resize, AddressOf ParentResize

        CloseButton.Size = New Size(12, 12)
        CloseButton.Location = New Point(Width - CloseButton.Width - 10, (Height - CloseButton.Height) / 2.5)
        CloseButton.BringToFront()

        Visible = False
    End Sub

    Private Sub ParentResize()
        Size = New Size(Parent.Width, Height)
    End Sub

    Private Sub ToastNotification_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Try
            CloseButton.Location = New Point(Width - CloseButton.Width - 10, (Height - CloseButton.Height) / 2.5)
            Location = New Point(0, Parent.Height - Height)
        Catch ex As Exception

        End Try
    End Sub
End Class
