Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module

Module CLI

    <ExportAPI("/API1",
               Info:="Puts the brief description of this API command at here.",
               Usage:="/API1 /msg ""Puts the CLI usage syntax at here""",
               Example:="/API1 /msg ""Hello world!!!""")>
    Public Function API1(args As CommandLine) As Integer
        Call Console.WriteLine(args("/msg"))
        Return 0
    End Function
End Module