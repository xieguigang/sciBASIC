Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash

Module Program

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/config.output", Usage:="/config.output /in <*.vbproj/DIR> /output <DIR> /c 'config=<Name>,platform=<type>'")>
    Public Function ConfigOutputPath(args As CommandLine) As Integer
        Dim [in] As String = args("/in")
        Dim output As String = args("/output")
        Dim c As Dictionary(Of String, String) = args.GetDictionary("/c")
        Dim files$()

        If [in].FileExists Then
            files = {[in]}
        Else
            files = (ls - l - r - wildcards("*.vbproj") <= [in]).ToArray
        End If

        For Each xml As String In files
            Dim vbproj As Project = xml.LoadXml(Of Project)
        Next

        Return 0
    End Function
End Module
