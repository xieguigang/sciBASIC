#Region "Microsoft.VisualBasic::62c220eb4d5efb98e794884ce4ac51b7, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger\DebuggerArgs.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 202
    '    Code Lines: 102 (50.50%)
    ' Comment Lines: 74 (36.63%)
    '    - Xml Docs: 28.38%
    ' 
    '   Blank Lines: 26 (12.87%)
    '     File Size: 12.53 KB


    '     Module DebuggerArgs
    ' 
    '         Properties: AutoPaused, ErrLogs
    ' 
    '         Sub: __logShell, InitDebuggerEnvir, SaveErrorLog
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports CLI = Microsoft.VisualBasic.CommandLine.CommandLine

Namespace ApplicationServices.Debugging

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
        Private Sub __logShell(args As CLI)
            Dim CLI As String = App.ExecutablePath & " " & args.cliCommandArgvs
            Dim log As String = $"{PS1.Fedora12.ToString} {CLI}"
            Dim logFile As String = App.LogErrDIR.ParentPath & "/.shells.log"

            If InStr(logFile.ParentPath, "/sbin/") = 1 Then
                ' 当程序运行在apache+linux web服务器上面的时候，
                ' 对于apache用户，linux服务器上面得到的文件夹是/sbin，则会出现权限错误，这个时候重定向到应用程序自身的文件夹之中
                logFile = App.HOME & "/.logs/.shells.log"
            End If

            Try
                If My.File.Wait(file:=logFile) Then
                    Call FileIO.FileSystem.CreateDirectory(logFile.ParentPath)
                    Call FileIO.FileSystem.WriteAllText(logFile, log & vbCrLf, True)
                End If
            Catch ex As Exception
                ' 连日志记录都出错了，已经没有地方可以写日志了，则只能够直接将错误信息以警告的方式打印出来
                Call ex.Message.Warning

                '[ERROR 09/07/2017 1052:12] :System.Exception : LogException ---> System.Exception: Exception of type 'System.Exception' was thrown. ---> System.ArgumentNullException: Value cannot be null.
                'Parameter name: path
                '                at System.IO.DirectoryInfo.CheckPath(System.String path) [0x00003] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo..ctor(System.String path, System.Boolean simpleOriginalPath) [0x00006] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo..ctor(System.String path) [0x00000] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:.ctor(Of String)
                '                at Microsoft.VisualBasic.FileIO.FileSystem.GetDirectoryInfo(System.String directory) [0x00000] in <828807dda9f14f24a7db780c6c644162>:0 
                '  at Microsoft.VisualBasic.ProgramPathSearchTool.GetDirectoryFullPath(System.String dir) [0x00000] in :0 
                '   --- End of inner exception stack trace ---
                '   --- End of inner exception stack trace ---
                '[ERROR 09/07/2017 10:52:12] :System.Exception : GetDirectoryFullPath ---> System.Exception: Exception of type 'System.Exception' was thrown. ---> System.ArgumentNullException: Value cannot be null.
                'Parameter name: path
                '                at System.IO.DirectoryInfo.CheckPath(System.String path) [0x00003] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo..ctor(System.String path, System.Boolean simpleOriginalPath) [0x00006] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo..ctor(System.String path) [0x00000] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:.ctor(Of String)
                '                at Microsoft.VisualBasic.FileIO.FileSystem.GetDirectoryInfo(System.String directory) [0x00000] in <828807dda9f14f24a7db780c6c644162>:0 
                '  at Microsoft.VisualBasic.ProgramPathSearchTool.GetDirectoryFullPath(System.String dir) [0x00000] in :0 
                '   --- End of inner exception stack trace ---
                '   --- End of inner exception stack trace ---
                '[ERROR 09/07/2017 10:52:12] :System.Exception : InitDebuggerEnvir ---> System.UnauthorizedAccessException: Access to the path "/sbin/.local" Is denied.
                '  at System.IO.Directory.CreateDirectoriesInternal(System.String path) [0x0005e] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.Directory.CreateDirectory(System.String path) [0x0008f] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo.Create() [0x00000] in <902ab9e386384bec9c07fa19aa938869>: 0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:Create()
                '                at System.IO.Directory.CreateDirectoriesInternal(System.String path) [0x00036] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.Directory.CreateDirectory(System.String path) [0x0008f] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo.Create() [0x00000] in <902ab9e386384bec9c07fa19aa938869>: 0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:Create()
                '                at System.IO.Directory.CreateDirectoriesInternal(System.String path) [0x00036] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.Directory.CreateDirectory(System.String path) [0x0008f] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo.Create() [0x00000] in <902ab9e386384bec9c07fa19aa938869>: 0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:Create()
                '                at System.IO.Directory.CreateDirectoriesInternal(System.String path) [0x00036] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.Directory.CreateDirectory(System.String path) [0x0008f] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.DirectoryInfo.Create() [0x00000] in <902ab9e386384bec9c07fa19aa938869>: 0 
                '  at(wrapper remoting-invoke-With-check) System.IO.DirectoryInfo:Create()
                '                at System.IO.Directory.CreateDirectoriesInternal(System.String path) [0x00036] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at System.IO.Directory.CreateDirectory(System.String path) [0x0008f] in <902ab9e386384bec9c07fa19aa938869>:0 
                '  at Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(System.String directory) [0x00025] in <828807dda9f14f24a7db780c6c644162>:0 
                '  at Microsoft.VisualBasic.Debugging.DebuggerArgs.__logShell(Microsoft.VisualBasic.CommandLine.CommandLine args) [0x00056] in :0 
                '  at Microsoft.VisualBasic.Debugging.DebuggerArgs.InitDebuggerEnvir(Microsoft.VisualBasic.CommandLine.CommandLine args, System.String caller) [0x00018] in :0 
                '   --- End of inner exception stack trace ---
            End Try
        End Sub

        ''' <summary>
        ''' Some optional VisualBasic debugger parameter help information.(VisualBasic调试器的一些额外的开关参数的帮助信息)
        ''' </summary>
        Public Const DebuggerHelps As String =
        "Additional VisualBasic App debugger arguments:   --echo on/off/all/warn/error /mute /auto-paused --err <filename.log> /ps1 <bash_PS1> /@set ""var1='value1';var2='value2'""

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

    [/mute]  This boolean flag will mute all debugger output.

    [/auto-paused] This boolean flag will makes the program paused after the command is executed done. and print a message on the console:
                       ""Press any key to continute..."" 

    [/@set]  This option will be using for settings of the interval environment variable.

    ** Additionally, you can using ""/linux-bash"" command for generates the bash shortcuts on linux system.
"
        Public ReadOnly Property AutoPaused As Boolean

        ''' <summary>
        ''' Initialize the global environment variables in this App process.
        ''' </summary>
        ''' <param name="args">--echo on/off/all/warn/error --err &lt;path.log></param>
        <Extension> Public Sub InitDebuggerEnvir(args As CLI, <CallerMemberName> Optional caller$ = Nothing)
            If Not String.Equals(caller, "Main") Then
                ' 这个调用不是从Main出发的，则不设置环境了
                ' 因为这个环境可能在其他的代码上面设置过了
                Return
            Else
                Try
                    Call __logShell(args)
                Catch ex As Exception
                    ' 因为只是进行命令行的调用历史的记录，所以实在不行的话就放弃这次的调用记录
                    Call ex.PrintException
                End Try
            End If

            Dim cultureInfo$ = args <= "--cultureinfo"

            If cultureInfo.StringEmpty Then
                ' 强制抛出英文错误消息或者用户设置其他语言
                Thread.CurrentThread.CurrentUICulture = New CultureInfo("en-US")
            Else
                ' 强制抛出英文错误消息或者用户设置其他语言
                Thread.CurrentThread.CurrentUICulture = New CultureInfo(cultureInfo)
            End If

            Dim opt As String = args <= "--echo"
            Dim log As String = args <= "--err"

            If Not String.IsNullOrEmpty(log) Then
                _ErrLogs = Function() log
            Else

            End If

            Dim config As Config = Config.Load

            If String.IsNullOrEmpty(opt) Then
                ' 默认的on参数
                VBDebugger.m_level = config.level
            Else
                Select Case opt.ToLower
                    Case "on"
                        VBDebugger.m_level = DebuggerLevels.On
                    Case "off"
                        VBDebugger.m_level = DebuggerLevels.Off
                    Case "all"
                        VBDebugger.m_level = DebuggerLevels.All
                    Case "warn", "warning"
                        VBDebugger.m_level = DebuggerLevels.Warning
                    Case "err", "error"
                        VBDebugger.m_level = DebuggerLevels.Error
                    Case Else
                        VBDebugger.m_level = DebuggerLevels.On
                        Call $"The debugger argument value --echo:={opt} is invalid, using default settings.".Warning
                End Select
            End If

            _AutoPaused = args.IsTrue("/auto-paused")
            VBDebugger.m_inDebugMode = args.IsTrue("--debug")

            If args.IsTrue("/mute") Then
                VBDebugger.Mute = True
            Else
                VBDebugger.Mute = config.mute
            End If

            Call config.ConfigFrameworkRuntime(args)
        End Sub
    End Module
End Namespace
