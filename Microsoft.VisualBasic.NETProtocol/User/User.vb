Public Class User

    ReadOnly __updateThread As Persistent.Application.USER

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="remote">User API的接口</param>
    Sub New(remote As IPEndPoint)

    End Sub

    ''' <summary>
    ''' 在消息推送服务器上面注册自己的句柄
    ''' </summary>
    ''' <returns></returns>
    Private Function __register() As Boolean
        Dim port As Integer
        __updateThread = New Persistent.Application.USER
    End Function
End Class
