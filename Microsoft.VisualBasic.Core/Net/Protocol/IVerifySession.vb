#Region "Microsoft.VisualBasic::5b32dfee69476e40543c71e3742dca5b, Microsoft.VisualBasic.Core\Net\Protocol\IVerifySession.vb"

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

    '     Interface IVerifySession
    ' 
    '         Properties: SessionID, ValueInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Net.Protocols

    ''' <summary>
    ''' 这个组件用来为用户输入一些验证信息
    ''' </summary>
    Public Interface IVerifySession
        ReadOnly Property SessionID As Long
        ReadOnly Property ValueInput As String
    End Interface
End Namespace
