#Region "Microsoft.VisualBasic::101ee968f9cdc4161724ea01d84d4ec0, www\Microsoft.VisualBasic.NETProtocol\Tests\user2\Module1.vb"

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
        Dim clint As New Microsoft.VisualBasic.Net.NETProtocol.InternetTime.SNTPClient("time.apple.com")

        Call clint.__DEBUG_ECHO


        clint.Connect(False)
        Call clint.__DEBUG_ECHO
        Pause()

        Dim user As New Microsoft.VisualBasic.Net.NETProtocol.User(New Microsoft.VisualBasic.Net.IPEndPoint("127.0.0.1", 6354), 12234)



        Pause()
    End Sub

End Module
