Imports System.Collections

Namespace Runtime

    ''' <summary>
    ''' Node.js <c>process</c> simulation: <c>platform</c>/<c>arch</c>/<c>pid</c>/<c>argv</c>/
    ''' <c>env</c> as properties and <c>cwd()</c> as a method. The working directory and
    ''' script path are injected by the host (the LiteJs.Runner) before the engine starts,
    ''' mirroring Node's <c>process.cwd()</c> and the <c>__dirname</c>/<c>__filename</c> globals.
    ''' </summary>
    Public NotInheritable Class ProcessModule

        Private Sub New()
        End Sub

        Public Shared Cwd As String = Environment.CurrentDirectory
        Public Shared ScriptDir As String = ""
        Public Shared ScriptFile As String = ""
        Public Shared Argv As New List(Of Object)()
        Public Shared ReadOnly Platform As String = If(Environment.OSVersion.Platform = PlatformID.Win32NT, "win32", "linux")
        Public Shared ReadOnly Arch As String = If(Environment.Is64BitProcess, "x64", "ia32")
        Public Shared ReadOnly Pid As Integer = Environment.ProcessId
        Public Shared ReadOnly Version As String = "v10.0.0-litejs"

        ''' <summary>Set the run context from the host before constructing the interpreter.</summary>
        Public Shared Sub SetContext(cwd As String, scriptDir As String, scriptFile As String, argv As List(Of Object))
            ProcessModule.Cwd = If(cwd, Environment.CurrentDirectory)
            ProcessModule.ScriptDir = If(scriptDir, "")
            ProcessModule.ScriptFile = If(scriptFile, "")
            ProcessModule.Argv = If(argv, New List(Of Object)())
        End Sub

        Public Shared Function MakeModule() As JsModule
            Dim env As New Dictionary(Of String, Object)()
            Try
                Dim vars = Environment.GetEnvironmentVariables()
                For Each de As DictionaryEntry In vars
                    env(CStr(de.Key)) = CStr(de.Value)
                Next
            Catch
            End Try
            Dim props As New Dictionary(Of String, Object) From {
                {"platform", Platform},
                {"arch", Arch},
                {"pid", CDbl(Pid)},
                {"version", Version},
                {"argv", Argv},
                {"env", env}
            }
            Return New JsModule("process", props)
        End Function

        Public Shared Function Dispatch(name As String, args As Object()) As Object
            Select Case name
                Case "cwd" : Return Cwd
                Case Else : Throw JsRuntimeException.TypeError("process." & name & " is not a function")
            End Select
        End Function

    End Class

End Namespace
