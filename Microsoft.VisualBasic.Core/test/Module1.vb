Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf

Module Module1

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
