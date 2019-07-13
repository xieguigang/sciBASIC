#Region "Microsoft.VisualBasic::4943544b9dbbfa61d047f49d765bd551, Microsoft.VisualBasic.Core\test\Module1.vb"

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
    '     Sub: iniTest, Main, mytest2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf
Imports Microsoft.VisualBasic.My.JavaScript.ES6

Module Module1

    Sub mytest2()
        Dim map As New Map





    End Sub

    Sub Main()
        Call iniTest()
    End Sub

    Sub iniTest()
        Using ini As New IniFile("./dddd.inf")
            Call ini.WriteValue("AAAA", "msg", "hello world", "what")
            Call ini.WriteValue("BBBB", 123, 9999, "op")
            Call ini.WriteValue("AAAA", "msg22222", "no!!!")
            Call ini.WriteValue("AAAA", "date", Now.ToString, "is now!")

            Call ini.WriteComment("BBBB", "HHHHHHHHHHHHHHHHHHHHHHHHHHHHW")
        End Using

        Dim debugView = New IniFile("./dddd.inf")

        Pause()
    End Sub
End Module
