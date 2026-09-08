Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine

Module Program

    ''' <summary>
    ''' vbs ./script.vb --arg1=xxx --arg2=xxx
    ''' </summary>
    ''' <param name="args"></param>
    Public Function Main(args As String()) As Integer
        Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
        Dim scriptFile As String = cmdl.Name
        Dim vbs As ScriptParseResult = Script.ParseScript(scriptFile)
        Dim asm As Assembly = vbs.CompileScript
        Dim exitCode As Integer = asm.Run(cmdl)

        Return exitCode
    End Function
End Module
