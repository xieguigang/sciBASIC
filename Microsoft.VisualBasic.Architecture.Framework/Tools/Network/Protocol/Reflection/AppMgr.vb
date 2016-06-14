Imports System.Net
Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Http

Namespace Net.Protocols.Reflection

    ''' <summary>
    ''' 能够处理多种协议数据
    ''' </summary>
    Public Class AppMgr : Inherits IProtocolHandler

        Public ReadOnly Property ProtocolApps As Dictionary(Of Long, ProtocolHandler) =
            New Dictionary(Of Long, ProtocolHandler)

        Public Overrides ReadOnly Property ProtocolEntry As Long
            Get
                Return -1
            End Get
        End Property

        Sub New()

        End Sub

        Public Function Register(app As Object, [overrides] As Boolean) As Boolean
            Dim Protocol = ProtocolHandler.SafelyCreateObject(app)

            If Protocol Is Nothing Then
                Return False
            End If
            If ProtocolApps.ContainsKey(Protocol.ProtocolEntry) Then
                If [overrides] Then
                    Call ProtocolApps.Remove(Protocol.ProtocolEntry)  ' 覆盖掉原有的协议数据
                Else
                    Return False ' 没有被注册
                End If
            End If

            Call ProtocolApps.Add(Protocol.ProtocolEntry, Protocol)

            Return True
        End Function

        ''' <summary>
        ''' 有点多此一举？？
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="App"></param>
        ''' <param name="[overrides]"></param>
        ''' <returns></returns>
        Public Function RegisterApp(Of T As Class)(App As T, [overrides] As Boolean) As Boolean
            Return Register(DirectCast(App, Object), [overrides])
        End Function

        Public Overrides Function HandleRequest(CA As Long, request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
            If Not ProtocolApps.ContainsKey(request.ProtocolCategory) Then
                Return NetResponse.RFC_NOT_FOUND
            End If

            Dim Protocol As ProtocolHandler = ProtocolApps(request.ProtocolCategory)
            Return Protocol.HandleRequest(CA, request, remoteDevcie)
        End Function
    End Class
End Namespace