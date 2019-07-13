#Region "Microsoft.VisualBasic::c4dfe9987f7ba498fcb37a3994b3db9e, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel.CLI\Program.vb"

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

    ' Module Program
    ' 
    '     Function: Main
    ' 
    ' /********************************************************************************/

#End Region

Module Program

    Public Const CsvTools$ = "Comma-Separated Values CLI Helpers"
    Public Const XlsxTools$ = "Microsoft Xlsx File CLI Tools"

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module
