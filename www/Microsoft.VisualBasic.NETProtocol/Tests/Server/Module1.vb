#Region "Microsoft.VisualBasic::cb34d805a44eabeb65b2e5741bdc2c91, www\Microsoft.VisualBasic.NETProtocol\Tests\Server\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Module1

    Sub Main()
        Using svr As New Microsoft.VisualBasic.Net.NETProtocol.PushServer(123, 8852, 6354)
            Call svr.Run()
        End Using
    End Sub

End Module
