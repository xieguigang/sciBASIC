Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/config.output", Usage:="")>
    Public Function ConfigOutputPath(args As CommandLine) As Integer

    End Function
End Module
