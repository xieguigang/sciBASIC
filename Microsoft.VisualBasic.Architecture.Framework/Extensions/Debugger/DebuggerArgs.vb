Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language

Module DebuggerArgs

    Private Sub __logShell(args As CommandLine.CommandLine)
        Dim CLI As String = App.ExecutablePath & " " & args.CLICommandArgvs
        Dim log As String = $"{Now.ToString & vbTab}  {CLI}"
        Dim logFile As String = App.LogErrDIR.ParentPath & "/.shell.log"

        If FileHandles.Wait(file:=logFile) Then
            Call FileIO.FileSystem.CreateDirectory(logFile.ParentPath)
            Call FileIO.FileSystem.WriteAllText(logFile, log & vbCrLf, True)
        End If
    End Sub

    ''' <summary>
    ''' Initialize the global environment variables in this App process.
    ''' </summary>
    ''' <param name="args">--echo on/off/all/warn/error</param>
    <Extension> Public Sub InitDebuggerEnvir(args As CommandLine.CommandLine, <CallerMemberName> Optional caller As String = Nothing)
        If Not String.Equals(caller, "Main") Then
            Return  ' 这个调用不是从Main出发的，则不设置环境了，因为这个环境可能在其他的代码上面设置过了
        End If

        Dim opt As String = args <= "--echo"

        If String.IsNullOrEmpty(opt) Then

        End If
    End Sub
End Module
