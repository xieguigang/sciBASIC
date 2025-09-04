#Region "Microsoft.VisualBasic::fb3ffd987afdd22e2c65c8445308b841, Microsoft.VisualBasic.Core\src\ApplicationServices\Debugger.vb"

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

    '   Total Lines: 444
    '    Code Lines: 256 (57.66%)
    ' Comment Lines: 145 (32.66%)
    '    - Xml Docs: 94.48%
    ' 
    '   Blank Lines: 43 (9.68%)
    '     File Size: 18.09 KB


    ' Module VBDebugger
    ' 
    '     Properties: debugMode
    ' 
    '     Function: die, LinqProc, MapLevels
    '     Delegate Sub
    ' 
    '         Properties: ForceSTDError, Mute, UsingxConsole
    ' 
    '         Function: Assert, benchmark, (+2 Overloads) PrintException
    ' 
    '         Sub: [error], (+3 Overloads) Assertion, AttachLoggingDriver, cat, (+3 Overloads) debug
    '              echo, (+3 Overloads) Echo, EchoLine, info, log
    '              logging, WaitOutput, warning, WriteLine
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
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Runtime

<Assembly: InternalsVisibleTo("REnv")>
<Assembly: InternalsVisibleTo("R#")>
<Assembly: InternalsVisibleTo("Microsoft.VisualBasic.Data.visualize.Network.Visualizer")>

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

    Private Function MapLevels(level As MSG_TYPES) As DebuggerLevels
        Select Case level
            Case MSG_TYPES.DEBUG : Return DebuggerLevels.Debug
            Case MSG_TYPES.ERR : Return DebuggerLevels.Error
            Case MSG_TYPES.INF : Return DebuggerLevels.Info
            Case MSG_TYPES.WRN : Return DebuggerLevels.Warning
            Case MSG_TYPES.FINEST : Return DebuggerLevels.All
            Case Else
                Return DebuggerLevels.On
        End Select
    End Function

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
    Public Function benchmark(test As Action, <CallerMemberName> Optional trace$ = Nothing) As Long
        Dim ms& = Utils.Time(test)
        Dim str$ = $"{trace} -> {CStrSafe(test.Target, "null")}::{test.Method.Name}"
        Call log(str, "benchmark", AnsiColor.BrightMagenta, MSG_TYPES.INF, Mute, $" + {StringFormats.ReadableElapsedTime(TimeSpan.FromMilliseconds(ms))}")
        Return ms
    End Function

    ''' <summary>
    ''' Display the wraning level(YELLOW color) message on the console.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="mute"></param>
    <Extension>
    Public Sub warning(msg As String, Optional mute As Boolean = False)
        Call log(msg, "warning", AnsiColor.BrightYellow, MSG_TYPES.WRN, mute, "")
    End Sub

    <Extension>
    Public Sub debug(msg As String, Optional mute As Boolean = False)
        Call log(msg, "debug", AnsiColor.Green, MSG_TYPES.DEBUG, mute, "")
    End Sub

    <Extension>
    Public Sub info(msg As String, Optional mute As Boolean = False)
        Call log(msg, "info", AnsiColor.BrightBlue, MSG_TYPES.INF, mute, "")
    End Sub

    <Extension>
    Public Sub [error](msg As String, Optional mute As Boolean = False)
        Call log(msg, "error", AnsiColor.Red, MSG_TYPES.ERR, mute, "")
    End Sub

    <Extension>
    Public Sub logging(msg As String, Optional mute As Boolean = False)
        Call log(msg, "log", AnsiColor.BrightWhite, MSG_TYPES.INF, mute, "")
    End Sub

    Public Sub log(msg As String, lv As String, color As AnsiColor, level As MSG_TYPES, mute As Boolean, tag As String)
        Dim elapsed As TimeSpan = TimeSpan.FromMilliseconds(App.ElapsedMilliseconds)
        Dim elapsedFormatted As String = StringFormats.Lanudry(elapsed)
        Dim header As String = $"{lv}, {elapsedFormatted}{tag} - "
        Dim log4vb As LoggingDriver = My.Log4VB.getLogger(level)

        If log4vb IsNot Nothing Then
            Call log4vb(header, msg, level)
        ElseIf Not mute AndAlso Not VBDebugger.Mute AndAlso m_level <= MapLevels(level) Then
            Call Console.WriteLine(New TextSpan(header & msg, color) & AnsiEscapeCodes.Reset)
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
    Public Function PrintException(Of ex As Exception)(exception As ex,
                                                       <CallerMemberName>
                                                       Optional memberName$ = "",
                                                       Optional enableRedirect As Boolean = True) As Boolean

        Dim lines = New Exception(memberName, exception).ToString.LineTokens
        Dim exceptions$() = Strings.Split(lines.First, "--->")
        Dim formats = exceptions(0) & vbCrLf &
            vbCrLf &
            exceptions.Skip(1).JoinBy(vbCrLf) & vbCrLf &
            vbCrLf &
            lines.Skip(1).JoinBy(vbCrLf)

        Return formats.PrintException(memberName, enableRedirect)
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
    Public Function PrintException(msg$,
                                   <CallerMemberName>
                                   Optional memberName$ = "",
                                   Optional enableRedirect As Boolean = True) As Boolean

        If My.Log4VB.redirectError Is Nothing OrElse Not enableRedirect Then
            Return My.Log4VB.Print($"ERROR {Now.ToString}", $"<{memberName}>::{msg}", ConsoleColor.Red, MSG_TYPES.ERR)
        Else
            Call My.Log4VB.redirectError(memberName, msg, MSG_TYPES.ERR)
            Return False
        End If
    End Function

    ''' <summary>
    ''' 等待调试器输出工作线程将内部的消息队列输出完毕
    ''' </summary>
    ''' <remarks>
    ''' ### 20230308
    ''' 
    ''' Do not combine of this method with the echo method
    ''' in this <see cref="VBDebugger"/> module! Or a thread
    ''' lock will be created and block the program running!
    ''' 
    ''' So i make this function access level from public to 
    ''' friend.
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
    ''' <remarks>
    ''' works based on <see cref="My.Log4VB.redirectInfo"/>
    ''' </remarks>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub WriteLine(msg$, Optional color As ConsoleColor = ConsoleColor.White)
        If Not My.redirectInfo Is Nothing Then
            My.Log4VB.redirectInfo(Now.ToString, msg, MSG_TYPES.INF)
        Else
            My.Log4VB.Println(msg, color)
        End If
    End Sub

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
                    Call fails.debug
                End If
            ElseIf level = MSG_TYPES.ERR Then
                If m_level <> DebuggerLevels.Off Then
                    Call WriteLine(fails, ConsoleColor.Red)
                End If
            ElseIf level = MSG_TYPES.WRN Then
                If m_level <> DebuggerLevels.Error Then
                    Call warning(fails, calls)
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

    Public Function Assert(test As Boolean, failed$,
                           Optional success$ = Nothing,
                           Optional failedLevel As MSG_TYPES = MSG_TYPES.ERR,
                           <CallerMemberName> Optional calls As String = "") As Boolean
        If test Then
            If Not String.IsNullOrEmpty(success) Then
                Call success.debug
            End If

            Return True
        Else
            Select Case failedLevel
                Case MSG_TYPES.DEBUG
                    Call failed.debug
                Case MSG_TYPES.ERR
                    Call failed.PrintException(calls)
                Case MSG_TYPES.WRN
                    Call failed.warning(calls)
                Case Else
                    Call failed.info
            End Select

            Return False
        End If
    End Function

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="msg">The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.</param>
    ''' <param name="Indent"></param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub debug(msg As StringBuilder, Optional indent As Integer = 0)
        Call (New String(" ", indent) & msg.ToString).debug
    End Sub

    <Extension>
    Public Sub debug(Of T)(value As T, <CallerMemberName> Optional memberName As String = "")
        Call ($"<{memberName}> {Scripting.InputHandler.ToString(value)}").debug
    End Sub

    <Extension>
    Public Sub echo(Of T)(array As IEnumerable(Of T))
        Call array.SafeQuery.Select(Function(x) Scripting.ToString(x)).JoinBy(", ").debug
    End Sub

    <Extension>
    Public Sub Echo(lines As IEnumerable(Of String))
        For Each line$ In lines
            Call EchoLine(line)
        Next
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.WriteLine"/>
    ''' </summary>
    ''' <param name="s">the text content for write line</param>
    ''' <remarks>
    ''' works based on <see cref="My.Log4VB.redirectInfo"/>
    ''' </remarks>
    <Extension>
    Public Sub EchoLine(s As String)
        If Not Mute Then
            Call My.InnerQueue.AddToQueue(
                Sub()
                    Call WriteLine(s, ConsoleColor.White)
                End Sub)
        End If
    End Sub

    Public Sub Echo(str As String)
        If Not Mute Then
            Call My.InnerQueue.AddToQueue(
                Sub()
                    If Not My.redirectInfo Is Nothing Then
                        My.Log4VB.redirectInfo(Now.ToString, str & vbBack, MSG_TYPES.INF)
                    Else
                        My.Log4VB.Print(str, ConsoleColor.White)
                    End If
                End Sub)
        End If
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.Write"/>
    ''' </summary>
    ''' <param name="c"></param>
    <Extension>
    Public Sub Echo(c As Char)
        If Not Mute Then
            Call Console.Write(c)
        End If
    End Sub

    ''' <summary>
    ''' print message, alias for <see cref="Console.Write(String)"/>.(支持``sprintf``之中的转义字符)
    ''' </summary>
    ''' <param name="s$"></param>
    Public Sub cat(ParamArray s As String())
        If Not Mute Then
            Call My.InnerQueue.AddToQueue(
                Sub()
                    Call Console.Write(s.SafeQuery.Select(AddressOf ReplaceMetaChars).JoinBy(""))
                End Sub)
        End If
    End Sub
End Module
