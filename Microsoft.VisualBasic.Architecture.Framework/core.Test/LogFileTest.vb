Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module LogFileTest

    Sub Main()
        WriteTes()
        PrintTest()
    End Sub

    Const path$ = "./test.log"

    Sub WriteTes()
        Using log As New LogFile(path)

        End Using
    End Sub

    Sub PrintTest()

    End Sub
End Module
