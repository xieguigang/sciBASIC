Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Module InvokeAPI

    Public Enum Protocols
        ''' <summary>
        ''' Push data to user
        ''' </summary>
        PushToUser
    End Enum

    Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

    Public Function PushData(data As Byte()) As RequestStream

    End Function

End Module
