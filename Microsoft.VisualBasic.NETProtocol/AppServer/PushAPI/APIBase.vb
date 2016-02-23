Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace PushAPI

    Public MustInherit Class APIBase

        Public ReadOnly Property PushServer As PushServer

        Protected __protocols As ProtocolHandler

        Sub New(push As PushServer)
            PushServer = push
        End Sub

        ''' <summary>
        ''' <see cref="DataRequestHandler"/>
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function Handler(CA As Long, request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream

    End Class
End Namespace