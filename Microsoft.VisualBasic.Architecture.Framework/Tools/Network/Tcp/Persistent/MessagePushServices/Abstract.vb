Imports Microsoft.VisualBasic.Net.Protocols

Namespace Net.Persistent

    ''' <summary>
    ''' 离线数据请求
    ''' </summary>
    ''' <param name="FromUSER_ID"></param>
    ''' <param name="USER_ID"></param>
    ''' <param name="Message"></param>
    Public Delegate Sub OffLineMessageSendHandler(FromUSER_ID As Long, USER_ID As Long, Message As RequestStream)
    Public Delegate Function PushMessage(USER_ID As Long, Message As RequestStream) As RequestStream
End Namespace