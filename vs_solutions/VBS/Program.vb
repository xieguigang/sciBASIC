Imports Microsoft.VisualBasic.CommandLine

Module Program

    ''' <summary>
    ''' vbs ./script.vb --arg1=xxx --arg2=xxx
    ''' </summary>
    ''' <param name="args"></param>
    Public Function Main(args As String()) As Integer
        Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
        Dim scriptFile As String = cmdl.Name
        Dim vbs As ScriptParseResult = VBScriptHost.Script.ParseScript(scriptFile)
        Dim script = vbs.CompileScript
        Dim exitCode As Integer = script.asm.Run(cmdl)

        Call script.ctx.Unload()

        Return exitCode
    End Function
End Module
