Imports VBScriptHost.Script

Module Program

    ''' <summary>
    ''' vbs ./script.vb --arg1=xxx --arg2=xxx
    ''' </summary>
    ''' <param name="args"></param>
    Public Function Main(args As String()) As Integer
        Dim scriptFile As String = args(0)
        Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile)

        Using script As ScriptRuntime = vbs.CompileScript
            Return script.Run(args)
        End Using
    End Function
End Module
