Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module
