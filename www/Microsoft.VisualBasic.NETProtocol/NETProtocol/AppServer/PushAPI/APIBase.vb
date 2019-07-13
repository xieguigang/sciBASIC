#Region "Microsoft.VisualBasic::165f63cf526f6fb898356e124cfe3dd1, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\AppServer\PushAPI\APIBase.vb"

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

    '     Class APIBase
    ' 
    '         Properties: PushServer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Abstract
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace NETProtocol.PushAPI

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
        Public MustOverride Function Handler(request As RequestStream, remote As System.Net.IPEndPoint) As RequestStream

    End Class
End Namespace
