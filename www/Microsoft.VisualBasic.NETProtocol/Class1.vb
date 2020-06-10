Imports Microsoft.VisualBasic.Net.Net.Protocols
Imports Microsoft.VisualBasic.Net.Net.Protocols.Reflection

''' <summary>
''' Object for handles the request <see cref="Protocol"/>.
''' </summary>
Public MustInherit Class IProtocolHandler

    MustOverride ReadOnly Property ProtocolEntry As Long
    MustOverride Function HandleRequest(request As RequestStream, remoteDevcie As System.Net.IPEndPoint) As RequestStream
End Class