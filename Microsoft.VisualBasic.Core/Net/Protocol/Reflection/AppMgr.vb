#Region "Microsoft.VisualBasic::d39cd878fc06a992f1453c699e3e6462, Microsoft.VisualBasic.Core\Net\Protocol\Reflection\AppMgr.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class AppMgr
    ' 
    '         Properties: ProtocolApps, ProtocolEntry
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: HandleRequest, Register, RegisterApp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Http
Imports TcpEndPoint = System.Net.IPEndPoint

Namespace Net.Protocols.Reflection

    ''' <summary>
    ''' 能够处理多种协议数据
    ''' </summary>
    Public Class AppMgr : Inherits IProtocolHandler

        Public ReadOnly Property ProtocolApps As New Dictionary(Of Long, ProtocolHandler)

        Public Overrides ReadOnly Property ProtocolEntry As Long
            Get
                Return -1
            End Get
        End Property

        Sub New()

        End Sub

        Public Function Register(app As Object, [overrides] As Boolean) As Boolean
            Dim protocol = ProtocolHandler.SafelyCreateObject(app)

            If protocol Is Nothing Then
                Return False
            End If
            If ProtocolApps.ContainsKey(protocol.ProtocolEntry) Then
                If [overrides] Then
                    ' 覆盖掉原有的协议数据
                    Call ProtocolApps.Remove(protocol.ProtocolEntry)
                Else
                    ' 没有被注册
                    Return False
                End If
            End If

            Call ProtocolApps.Add(protocol.ProtocolEntry, protocol)

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

        Public Overrides Function HandleRequest(request As RequestStream, remoteDevcie As TcpEndPoint) As RequestStream
            If Not ProtocolApps.ContainsKey(request.ProtocolCategory) Then
                Return NetResponse.RFC_NOT_FOUND
            End If

            Dim protocol As ProtocolHandler = ProtocolApps(request.ProtocolCategory)
            Return protocol.HandleRequest(request, remoteDevcie)
        End Function
    End Class
End Namespace
