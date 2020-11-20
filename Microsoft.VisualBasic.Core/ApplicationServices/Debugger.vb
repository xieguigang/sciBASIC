#Region "Microsoft.VisualBasic::1a0f42e369feeec6ff303771d5761c55, Microsoft.VisualBasic.Core\ApplicationServices\Debugger.vb"

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

    ' Module VBDebugger
    ' 
    '     Properties: debugMode
    ' 
    '     Function: die, LinqProc
    '     Delegate Sub
    ' 
    '         Properties: ForceSTDError, Mute, UsingxConsole
    ' 
    '         Function: __DEBUG_ECHO, Assert, BENCHMARK, BugsFormatter, (+2 Overloads) PrintException
    '                   Warning
    ' 
    '         Sub: (+2 Overloads) __DEBUG_ECHO, __INFO_ECHO, (+3 Overloads) Assertion, AttachLoggingDriver, cat
    '              (+3 Overloads) Echo, EchoLine, WaitOutput, WriteLine
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Settings
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text

''' <summary>
''' Debugger helper module for VisualBasic Enterprises System.
''' </summary>
Public Module VBDebugger

    Friend m_inDebugMode As Boolean

    ''' <summary>
    ''' if in the debug profile(which means ``DEBUG`` constant is defined for the compiler)
    ''' then this function will always returns value ``true``;
    ''' otherwise, return value by command line config argument ``--debug``
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property debugMode As Boolean
        Get
#If DEBUG Then
            Return True
#Else
            Return m_inDebugMode
#End If
        End Get
    End Property

    ''' <summary>
    ''' Assert that the expression value is correctly or not?
    ''' </summary>
    ''' <param name="message$">The exception message</param>
    ''' <param name="failure">If this expression test is True, then die expression will raise an exception</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function die(message$, Optional failure As Predicate(Of Object) = Nothing, <CallerMemberName> Optional caller$ = Nothing) As ExceptionHandle
        Return New ExceptionHandle With {
            .message = message,
            .failure = failure Or defaultAssert
        }
    End Function

    ''' <summary>
    ''' 当在执行大型的数据集合的时候怀疑linq里面的某一个任务进入了死循环状态，可以使用这个方法来检查是否如此
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="tag"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Function LinqProc(Of T)(source As IEnumerable(Of T), <CallerMemberName> Optional tag$ = Nothing) As EventProc
        Return New EventProc(source.Count, tag)
    End Function

    ''' <summary>
    ''' 当前的调试器的信息输出登记，默认是输出所有的信息
    ''' </summary>
    Friend m_level As DebuggerLevels = DebuggerLevels.On
    ''' <summary>
    ''' 是否静默掉所有的调试器输出信息？默认不是
    ''' </summary>
    Friend m_mute As Boolean = False

    ''' <summary>
    ''' 对外部开放的调试日志的获取接口类型的申明
    ''' </summary>
    ''' <param name="header">消息的类型的头部标签</param>
    ''' <param name="message">消息文本内容，一般为一行文本</param>
    ''' <param name="level">日志消息的错误等级</param>
    Public Delegate Sub LoggingDriver(header$, message$, level As MSG_TYPES)

    ''' <summary>
    ''' Disable the debugger information outputs on the console if this <see cref="Mute"/> property is set to 
    ''' <see cref="Boolean.True"/>, and enable the output if this property is set to <see cref="Boolean.False"/>. 
    ''' NOTE: this debugger option property can be overrides by the debugger parameter from the CLI parameter 
    ''' named ``--echo``
    ''' </summary>
    ''' <returns></returns>
    Public Property Mute As Boolean
        Get
            Return m_mute
        End Get
        Set(value As Boolean)
            ' off的时候，不会输出任何信息
            If m_level = DebuggerLevels.Off Then
                m_mute = True
            Else
                m_mute = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Force the app debugging output redirect into the std_error device.
    ''' </summary>
    ''' <returns></returns>
    Public Property ForceSTDError As Boolean = False

    ''' <summary>
    ''' Test how long this <paramref name="test"/> will takes.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="trace$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function BENCHMARK(test As Action, <CallerMemberName> Optional trace$ = Nothing) As Long
        Dim start = Now.ToLongTimeString
        Dim ms& = Utils.Time(test)
        Dim end$ = Now.ToLongTimeString

        If Not Mute AndAlso m_level < DebuggerLevels.Warning Then
            Dim head$ = $"Benchmark `{ms.FormatTicks}` {start} - {[end]}"
            Dim str$ = " " & $"{trace} -> {CStrSafe(test.Target, "null")}::{test.Method.Name}"

            Call My.Log4VB.Print(head, str, ConsoleColor.Magenta, ConsoleColor.Magenta)
        End If

        Return ms
    End Function

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="msg">
    ''' The message fro output to the debugger console, this function will add a time 
    ''' stamp automaticly To the leading position Of the message.
    ''' </param>
    ''' <param name="indent"></param>
    ''' <param name="waitOutput">
    ''' 等待调试器输出工作线程将内部的消息队列输出完毕
    ''' </param>
    ''' <returns>其实这个函数是不会返回任何东西的，只是因为为了Linq调试输出的需要，所以在这里是返回Nothing的</returns>
    <Extension> Public Function __DEBUG_ECHO(msg$,
                                             Optional indent% = 0,
                                             Optional mute As Boolean = False,
                                             Optional waitOutput As Boolean = False) As String
        Static indents$() = {
            "",
            New String(" ", 1), New String(" ", 2), New String(" ", 3), New String(" ", 4),
            New String(" ", 5), New String(" ", 6), New String(" ", 7), New String(" ", 8),
            New String(" ", 9), New String(" ", 10)
        }

        If Not mute AndAlso Not VBDebugger.Mute AndAlso m_level < DebuggerLevels.Warning Then
            Dim head As String = $"DEBUG {Now.ToString}"
            Dim str As String = $"{indents(indent)} {msg}"

            Call My.Log4VB.Print(head, str, ConsoleColor.White, MSG_TYPES.DEBUG)

#If DEBUG Then
            Call Debug.WriteLine($"[{head}]{str}")
#End If
        End If
        If waitOutput Then
            Call VBDebugger.WaitOutput()
        End If

        Return Nothing
    End Function

    <Extension> Public Sub __INFO_ECHO(msg$, Optional silent As Boolean = False)
        If Not Mute AndAlso m_level < DebuggerLevels.Warning Then
            Dim head As String = $"INFOM {Now.ToString}"
            Dim str As String = " " & msg

            If Not silent Then
                Call My.Log4VB.Print(head, str, ConsoleColor.White, MSG_TYPES.INF)
            End If

#If DEBUG Then
            Call Debug.WriteLine($"[{head}]{str}")
#End If
        End If
    End Sub

    ''' <summary>
    ''' Add additional user logging driver
    ''' </summary>
    ''' <param name="driver"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AttachLoggingDriver(driver As LoggingDriver)
        My.Log4VB.logs.Add(driver)
    End Sub

    ''' <summary>
    ''' The function will print the exception details information on the standard <see cref="console"/>, <see cref="debug"/> console, and system <see cref="trace"/> console.
    ''' (分别在标准终端，调试终端，系统调试终端之中打印出错误信息，请注意，函数会直接返回False可以用于指定调用者函数的执行状态，这个函数仅仅是在终端上面打印出错误，不会保存为日志文件)
    ''' </summary>
    ''' <typeparam name="ex"></typeparam>
    ''' <param name="exception"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function PrintException(Of ex As Exception)(exception As ex, <CallerMemberName> Optional memberName$ = "") As Boolean
        Dim lines = New Exception(memberName, exception).ToString.LineTokens
        Dim exceptions$() = Strings.Split(lines.First, "--->")
        Dim formats = exceptions(0) & vbCrLf &
            vbCrLf &
            exceptions.Skip(1).JoinBy(vbCrLf) & vbCrLf &
            vbCrLf &
            lines.Skip(1).JoinBy(vbCrLf)

        Return formats.PrintException(memberName)
    End Function

    ''' <summary>
    ''' 可以使用这个方法<see cref="MethodBase.GetCurrentMethod"/>.<see cref="GetFullName"/>获取得到<paramref name="memberName"/>所需要的参数信息
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="memberName"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function PrintException(msg$, <CallerMemberName> Optional memberName$ = "") As Boolean
        If My.Log4VB.redirectError Is Nothing Then
            Return My.Log4VB.Print($"ERROR {Now.ToString}", $"<{memberName}>::{msg}", ConsoleColor.Red, MSG_TYPES.ERR)
        Else
            Call My.Log4VB.redirectError(memberName, msg, MSG_TYPES.ERR)
            Return False
        End If
    End Function

    ''' <summary>
    ''' 等待调试器输出工作线程将内部的消息队列输出完毕
    ''' </summary>
    Public Sub WaitOutput()
        Call My.InnerQueue.WaitQueue()
    End Sub

    ''' <summary>
    ''' 使用<see cref="xConsole"/>输出消息
    ''' </summary>
    ''' <returns></returns>
    Public Property UsingxConsole As Boolean = False

    ''' <summary>
    ''' 输出的终端消息带有指定的终端颜色色彩，当<see cref="UsingxConsole"/>为True的时候，
    ''' <paramref name="msg"/>参数之中的文本字符串兼容<see cref="xConsole"/>语法，
    ''' 而<paramref name="color"/>将会被<see cref="xConsole"/>覆盖而不会起作用
    ''' </summary>
    ''' <param name="msg">兼容<see cref="xConsole"/>语法</param>
    ''' <param name="color">当<see cref="UsingxConsole"/>参数为True的时候，这个函数参数将不会起作用</param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub WriteLine(msg$, color As ConsoleColor)
        My.Log4VB.Println(msg, color)
    End Sub

    ''' <summary>
    ''' Display the wraning level(YELLOW color) message on the console.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="calls"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Warning(msg As String, <CallerMemberName> Optional calls As String = "") As String
        If Not My.Log4VB.redirectWarning Is Nothing Then
            Call My.Log4VB.redirectError(calls, msg, MSG_TYPES.WRN)
        ElseIf Not Mute Then
            Dim head As String = $"WARNG <{calls}> {Now.ToString}"

            Call My.Log4VB.Print(head, " " & msg, ConsoleColor.Yellow, MSG_TYPES.DEBUG)
#If DEBUG Then
            Call Debug.WriteLine($"[{head}]{msg}")
#End If
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' If <paramref name="test"/> boolean value is False, then the assertion test failure. If the test is failure the specific message will be output on the console.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="fails"></param>
    ''' <param name="level"></param>
    ''' <param name="calls"></param>
    <Extension>
    Public Sub Assertion(test As Boolean, fails As String, level As MSG_TYPES, <CallerMemberName> Optional calls As String = "")
        If Not test = True Then
            If level = MSG_TYPES.DEBUG Then
                If m_level < DebuggerLevels.Warning Then
                    Call fails.__DEBUG_ECHO(memberName:=calls)
                End If
            ElseIf level = MSG_TYPES.ERR Then
                If m_level <> DebuggerLevels.Off Then
                    Call WriteLine(fails, ConsoleColor.Red)
                End If
            ElseIf level = MSG_TYPES.WRN Then
                If m_level <> DebuggerLevels.Error Then
                    Call Warning(fails, calls)
                End If
            Else
                If m_level < DebuggerLevels.Warning Then
                    Call Console.WriteLine($"@{calls}::" & fails)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' If the <paramref name="test"/> message is not null or empty string, then the console will output the message.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="level"></param>
    ''' <param name="calls"></param>
    <Extension>
    Public Sub Assertion(test As String, level As MSG_TYPES, <CallerMemberName> Optional calls As String = "")
        Call VBDebugger.Assertion((String.IsNullOrEmpty(test) OrElse String.IsNullOrWhiteSpace(test)), test, level, calls)
    End Sub

    ''' <summary>
    ''' Use an assert statement to disrupt normal execution if a boolean condition is false.
    ''' If <paramref name="test"/> is false(means this assertion test failure), then throw exception.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="msg"></param>
    Public Sub Assertion(test As Boolean, msg As String, <CallerMemberName> Optional calls As String = "")
        Dim null = test Or die(message:=msg, caller:=calls)
    End Sub

    Public Function Assert(test As Boolean,
                           failed$,
                           Optional success$ = Nothing,
                           Optional failedLevel As MSG_TYPES = MSG_TYPES.ERR,
                           <CallerMemberName> Optional calls As String = "") As Boolean
        If test Then
            If Not String.IsNullOrEmpty(success) Then
                Call success.__DEBUG_ECHO
            End If

            Return True
        Else
            Select Case failedLevel
                Case MSG_TYPES.DEBUG
                    Call failed.__DEBUG_ECHO
                Case MSG_TYPES.ERR
                    Call failed.PrintException(calls)
                Case MSG_TYPES.WRN
                    Call failed.Warning(calls)
                Case Else
                    Call failed.Echo(calls)
            End Select

            Return False
        End If
    End Function

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="MSG">The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.</param>
    ''' <param name="Indent"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension> Public Sub __DEBUG_ECHO(MSG As StringBuilder, Optional Indent As Integer = 0)
        Call MSG.ToString.__DEBUG_ECHO(Indent)
    End Sub

    <Extension> Public Sub __DEBUG_ECHO(Of T)(value As T, <CallerMemberName> Optional memberName As String = "")
        Call ($"<{memberName}> {Scripting.InputHandler.ToString(value)}").__DEBUG_ECHO
    End Sub

    <Extension> Public Sub Echo(Of T)(array As IEnumerable(Of T), <CallerMemberName> Optional memberName As String = "")
        Call String.Join(", ", array.Select(Function(obj) Scripting.ToString(obj)).ToArray).__DEBUG_ECHO
    End Sub

    <Extension> Public Sub Echo(lines As IEnumerable(Of String))
        For Each line$ In lines
            Call Console.WriteLine(line)
        Next
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.WriteLine"/>
    ''' </summary>
    ''' <param name="s$"></param>
    <Extension> Public Sub EchoLine(s$)
        If Not Mute Then
            Call My.InnerQueue.AddToQueue(
                Sub()
                    Call Console.WriteLine(s)
                End Sub)
        End If
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.Write"/>
    ''' </summary>
    ''' <param name="c"></param>
    <Extension> Public Sub Echo(c As Char)
        If Not Mute Then
            Call Console.Write(c)
        End If
    End Sub

    ''' <summary>
    ''' print message, alias for <see cref="Console.Write(String)"/>.(支持``sprintf``之中的转义字符)
    ''' </summary>
    ''' <param name="s$"></param>
    Public Sub cat(s$)
        If Not Mute Then
            Call My.InnerQueue.AddToQueue(
                Sub()
                    Call Console.Write(s.ReplaceMetaChars)
                End Sub)
        End If
    End Sub

    ''' <summary>
    ''' Generates the formatted error log file content.(生成简单的日志板块的内容)
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="trace"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("Bugs.Formatter")>
    <Extension>
    Public Function BugsFormatter(ex As Exception, <CallerMemberName> Optional trace$ = "") As String
        Dim logs = ex.ToString.LineTokens
        Dim stackTrace = logs _
            .Where(Function(s)
                       Return InStr(s, "   在 ") = 1 OrElse InStr(s, "   at ") = 1
                   End Function) _
            .AsList
        Dim message = logs _
            .Where(Function(s)
                       Return Not s.IsPattern("\s+[-]{3}.+?[-]{3}\s*") AndAlso stackTrace.IndexOf(s) = -1
                   End Function) _
            .JoinBy(ASCII.LF) _
            .Trim _
            .StringSplit("\s[-]{3}>\s")

        Return New StringBuilder() _
            .AppendLine("TIME:  " & Now.ToString) _
            .AppendLine("TRACE: " & trace) _
            .AppendLine(New String("=", 120)) _
            .Append(LogFile.SystemInfo) _
            .AppendLine(New String("=", 120)) _
            .AppendLine() _
            .AppendLine($"Environment Variables from {GetType(App).FullName}:") _
            .AppendLine(ConfigEngine.Prints(App.GetAppVariables)) _
            .AppendLine(New String("=", 120)) _
            .AppendLine() _
            .AppendLine(ex.GetType.FullName & ":") _
            .AppendLine() _
            .AppendLine(message _
                .Select(Function(s) "    ---> " & s) _
                .JoinBy(ASCII.LF)) _
            .AppendLine() _
            .AppendLine(stackTrace _
                .Select(Function(s)
                            If InStr(s, "   在 ") = 1 Then
                                Return Mid(s, 6).Trim
                            ElseIf InStr(s, "   at ") = 1 Then
                                Return Mid(s, 7).Trim
                            Else
                                Return s
                            End If
                        End Function) _
                .Select(Function(s) "   at " & s) _
                .JoinBy(ASCII.LF)) _
            .ToString()
    End Function
End Module
