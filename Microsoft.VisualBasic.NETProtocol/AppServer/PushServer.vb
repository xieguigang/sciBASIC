Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' 保持长连接，向用户客户端发送更新消息的服务器
''' </summary>
Public Class PushServer : Implements IDisposable

    ''' <summary>
    ''' Push update notification to user client
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property UserSocket As Persistent.Application.MessagePushServer

    ''' <summary>
    ''' 其他的服务器模块对消息推送模块进行操作更新的通道
    ''' </summary>
    ReadOnly __invokeAPI As TcpSynchronizationServicesSocket
    ''' <summary>
    ''' 客户端进行数据读取的通道
    ''' </summary>
    ReadOnly __userAPI As TcpSynchronizationServicesSocket
    ''' <summary>
    ''' 用户数据缓存池
    ''' </summary>
    ReadOnly __msgs As PushAPI.UserMsgPool = New PushAPI.UserMsgPool

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="services">长连接socket的端口</param>
    ''' <param name="invoke">服务器模块工作端口</param>
    ''' <param name="userAPI">用户端口</param>
    Sub New(services As Integer, invoke As Integer, userAPI As Integer)
        UserSocket = New Persistent.Application.MessagePushServer(services)
        __invokeAPI = New TcpSynchronizationServicesSocket(invoke) With {
            .Responsehandler = AddressOf New PushAPI.InvokeAPI(Me).Handler
        }
        __userAPI = New TcpSynchronizationServicesSocket(userAPI) With {
            .Responsehandler = AddressOf New PushAPI.UserAPI(Me).Handler
        }
    End Sub

    Sub Run()
        Call RunTask(AddressOf UserSocket.Run)
        Call RunTask(AddressOf __userAPI.Run)
        Call __invokeAPI.Run() ' 需要使用这一个代码来保持线程的阻塞
    End Sub

    Public Function GetMsg(uid As Long) As RequestStream
        Dim msg = __msgs.Pop(uid)
        If msg Is Nothing Then
            Return NullMsg()
        Else
            Return msg
        End If
    End Function

    Public Function SendMessage(uid As Long, msg As RequestStream) As Boolean
        Try
            Call UserSocket.SendMessage(-1L, uid, msg)
        Catch ex As Exception
            Call App.LogException(ex)
            Return False
        End Try

        Return True
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call _UserSocket.Free
                Call __invokeAPI.Free
                Call __userAPI.Free
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
