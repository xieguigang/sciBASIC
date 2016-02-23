Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Module UserProtocols

    Public Enum Protocols As Long
        PushInit
    End Enum

    Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

End Module
