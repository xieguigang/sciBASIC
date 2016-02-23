Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Module UserAPI

    Public Enum Protocols
        ''' <summary>
        ''' 获取得到推送的消息
        ''' </summary>
        GetData
    End Enum

    Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

End Module
