Imports Microsoft.VisualBasic.CommandLine
Imports VBScriptHost.Script

Module Program

    ''' <summary>
    ''' vbs ./script.vb --arg1=xxx --arg2=xxx
    ''' </summary>
    ''' <param name="args"></param>
    Public Function Main(args As String()) As Integer
        Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
        Dim scriptFile As String = args(0)
        Dim verbose As Boolean = cmdl("--verbose")
        Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose)

        Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
            Return script.Run(args)
        End Using
    End Function
End Module
