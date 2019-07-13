#Region "Microsoft.VisualBasic::17ed0afe998827fe008da60b118e5705, Data\DataFrame\test\MySQLImportsTest.vb"

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

    ' Module MySQLImportsTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DATA

Module MySQLImportsTest
    Sub Main()
        Dim result = MySQL.ImportsMySQLDump("C:\Users\xieguigang\Documents\dumps\Dump20171029\kb_go_term_synonym.sql").ToArray


        Pause()
    End Sub
End Module
