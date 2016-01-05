Imports System.CodeDom.Compiler
Imports System.Reflection
Imports System.Text

Public Module VBC

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="code">源代码</param>
    ''' <returns></returns>
    Public Function CompileCode(code As String, Optional ByRef errInfo As String = "") As Assembly
        Dim codeProvider As New VBCodeProvider()
#Disable Warning
        Dim icc As ICodeCompiler = codeProvider.CreateCompiler
#Enable Warning
        Dim Output As String = "Out.exe"
        Dim parameters As New CompilerParameters()
        Dim results As CompilerResults

        'Make sure we generate an EXE, not a DLL
        parameters.GenerateExecutable = True
        parameters.OutputAssembly = Output
        results = icc.CompileAssemblyFromSource(parameters, code)

        If results.Errors.Count > 0 Then            'There were compiler errors
            Dim Err As StringBuilder =
                New StringBuilder("There were compiler errors:")

            Call Err.AppendLine()
            Call Err.AppendLine()

            For Each CompErr As CompilerError In results.Errors
                Dim errDetail As String = "Line number " & CompErr.Line &
                ", Error Number: " & CompErr.ErrorNumber &
                ", '" & CompErr.ErrorText & ";"
                Call Err.AppendLine(errDetail)
                Call Err.AppendLine()
            Next

            errInfo = Err.ToString

            Return Nothing
        Else
            'Successful Compile
            Return results.CompiledAssembly
        End If
    End Function

End Module
