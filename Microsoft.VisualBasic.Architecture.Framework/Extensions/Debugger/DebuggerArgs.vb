Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine

Module DebuggerArgs

    ''' <summary>
    ''' Initialize the global environment variables in this App process.
    ''' </summary>
    ''' <param name="args">--echo on/off/all/warn/error</param>
    <Extension> Public Sub InitDebuggerEnvir(args As CommandLine.CommandLine, <CallerMemberName> Optional caller As String = Nothing)

    End Sub
End Module
