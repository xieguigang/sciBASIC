Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Protocols
Imports System.Runtime.CompilerServices

Module UserProtocols

    Public Enum Protocols As Long
        PushInit
    End Enum

    Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

    Public Function NullMsg() As RequestStream
        Return New RequestStream(HTTP_RFC.RFC_NO_CONTENT, HTTP_RFC.RFC_OK, "")
    End Function

    <Extension> Public Function IsNull(msg As RequestStream) As Boolean
        Return msg.ProtocolCategory = HTTP_RFC.RFC_NO_CONTENT
    End Function

End Module
