Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace Protocols

    Module UserAPI

        Public Enum Protocols
            ''' <summary>
            ''' 获取得到推送的消息
            ''' </summary>
            GetData
        End Enum

        Public ReadOnly Property ProtocolEntry As Long =
        New Protocol(GetType(Protocols)).EntryPoint

        ''' <summary>
        ''' 在服务器端调用得到用户的唯一标识符
        ''' </summary>
        ''' <param name="sId"></param>
        ''' <returns></returns>
        Public Function Uid(sId As String) As Long
            sId &= Now.ToBinary.ToString
            sId = SecurityString.GetMd5Hash(sId)
            Return SecurityString.ToLong(sId)
        End Function

    End Module
End Namespace