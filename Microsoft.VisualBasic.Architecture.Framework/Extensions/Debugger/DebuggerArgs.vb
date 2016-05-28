Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace Debugging

    ''' <summary>
    ''' 调试器设置参数模块
    ''' </summary>
    Module DebuggerArgs

        ''' <summary>
        ''' 错误日志的文件存储位置，默认是在AppData里面
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ErrLogs As Func(Of String) = Nothing

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="log">日志文本</param>
        Public Sub SaveErrorLog(log As String)
            If Not ErrLogs Is Nothing Then
                Call log.SaveTo(_ErrLogs())
            End If
        End Sub

        ''' <summary>
        ''' Logging command shell history.
        ''' </summary>
        ''' <param name="args"></param>
        Private Sub __logShell(args As CommandLine.CommandLine)
            Dim CLI As String = App.ExecutablePath & " " & args.CLICommandArgvs
            Dim log As String = $"[{Now.ToString & "]" & vbTab}  {App.CurrentDirectory}>  {CLI}"
            Dim logFile As String = App.LogErrDIR.ParentPath & "/.shells.log"

            If FileHandles.Wait(file:=logFile) Then
                Call FileIO.FileSystem.CreateDirectory(logFile.ParentPath)
                Call FileIO.FileSystem.WriteAllText(logFile, log & vbCrLf, True)
            End If
        End Sub

        ''' <summary>
        ''' Some optional VisualBasic debugger parameter help information.(VisualBasic调试器的一些额外的开关参数的帮助信息)
        ''' </summary>
        Public Const DebuggerHelps As String =
        "Additional VisualBasic App debugger arguments:   --echo on/off/all/warn/error /mute --err <filename.log>

    [--echo] The debugger echo options, it have 5 values:
             on     App will output all of the debugger echo message, but the VBDebugger.Mute option is enabled, disable echo options can be control by the program code;
             off    App will not output any debugger echo message, the VBDebugger.Mute option is disabled;
             all    App will output all of the debugger echo message, the VBDebugger.Mute option is disabled;
             warn   App will only output warning level and error level message;
             error  App will just only output error level message.

    [--err]  The error logs save copy:
             If there is an unhandled exception in your App, this will cause your program crashed, then the exception will be save to error log, 
             and by default the error log is saved in the AppData, then if this option is enabled, the error log will saved a copy to the 
             specific location at the mean time. 

"

        ''' <summary>
        ''' Initialize the global environment variables in this App process.
        ''' </summary>
        ''' <param name="args">--echo on/off/all/warn/error --err &lt;path.log></param>
        <Extension> Public Sub InitDebuggerEnvir(args As CommandLine.CommandLine, <CallerMemberName> Optional caller As String = Nothing)
            If Not String.Equals(caller, "Main") Then
                Return  ' 这个调用不是从Main出发的，则不设置环境了，因为这个环境可能在其他的代码上面设置过了
            Else
                Call __logShell(args)
            End If

            Dim opt As String = args <= "--echo"
            Dim log As String = args <= "--err"

            If Not String.IsNullOrEmpty(log) Then
                _ErrLogs = Function() log
            Else

            End If



            If String.IsNullOrEmpty(opt) Then ' 默认的on参数
                VBDebugger.__level = DebuggerLevels.On
            Else
                Select Case opt.ToLower
                    Case "on"
                        VBDebugger.__level = DebuggerLevels.On
                    Case "off"
                        VBDebugger.__level = DebuggerLevels.Off
                    Case "all"
                        VBDebugger.__level = DebuggerLevels.All
                    Case "warn", "warning"
                        VBDebugger.__level = DebuggerLevels.Warning
                    Case "err", "error"
                        VBDebugger.__level = DebuggerLevels.Error
                    Case Else
                        VBDebugger.__level = DebuggerLevels.On
                        Call Console.WriteLine($"[INFO] The debugger argument value --echo:={opt} is invalid, using default settings.")
                End Select
            End If
        End Sub
    End Module
End Namespace