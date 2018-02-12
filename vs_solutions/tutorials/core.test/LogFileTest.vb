#Region "Microsoft.VisualBasic::32bf033d93b65934e75dc90194c7a96a, core.test"

    ' Author:
    ' 
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 


    ' Source file summaries:

    ' Module LogFileTest
    ' 
    '     Sub: Main, PrintTest, WriteTes
    ' 
    ' 
    ' 

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Module LogFileTest

    Sub Main()
        WriteTes()
        PrintTest()
    End Sub

    Const path$ = "./test.log"

    Sub WriteTes()
        Using log As New LogFile(path)
            Call log.writeline("123", "test")
        End Using
    End Sub

    Sub PrintTest()

    End Sub
End Module
