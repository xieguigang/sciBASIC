Namespace Net.Protocols

    ''' <summary>
    ''' 这个组件用来为用户输入一些验证信息
    ''' </summary>
    Public Interface IVerifySession
        ReadOnly Property SessionID As Long
        ReadOnly Property ValueInput As String
    End Interface
End Namespace