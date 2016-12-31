#Region "Microsoft.VisualBasic::eb8f6e5c5c500eddcd7eb00b3927cbcf, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Debugger\Debugger.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Terminal.Utility
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Language
Imports System.Reflection
Imports Microsoft.VisualBasic.Debugging

''' <summary>
''' Debugger helper module for VisualBasic Enterprises System.
''' </summary>
Public Module VBDebugger

    ''' <summary>
    ''' 当在执行大型的数据集合的时候怀疑linq里面的某一个任务进入了死循环状态，可以使用这个方法来检查是否如此
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="TAG"></param>
    ''' <returns></returns>
    <Extension> Public Function LinqProc(Of T)(source As IEnumerable(Of T), <CallerMemberName> Optional TAG As String = "") As EventProc
        Return New EventProc(source.Count, TAG)
    End Function

    Dim __mute As Boolean = False
    Friend __level As DebuggerLevels = DebuggerLevels.On  ' 默认是输出所有的信息

    ''' <summary>
    ''' Disable the debugger information outputs on the console if this <see cref="Mute"/> property is set to True, 
    ''' and enable the output if this property is set to False. 
    ''' NOTE: this debugger option property can be overrides by the debugger parameter from the CLI parameter named '--echo'
    ''' </summary>
    ''' <returns></returns>
    Public Property Mute As Boolean
        Get
            Return __mute
        End Get
        Set(value As Boolean)
            If __level = DebuggerLevels.Off Then  ' off的时候，不会输出任何信息
                __mute = True
            Else
                __mute = value
            End If
        End Set
    End Property

    ''' <summary>
    ''' Force the app debugging output redirect into the std_error device.
    ''' </summary>
    ''' <returns></returns>
    Public Property ForceSTDError As Boolean = False

    ReadOnly _Indent As String() = {
        "",
        New String(" ", 1), New String(" ", 2), New String(" ", 3), New String(" ", 4),
        New String(" ", 5), New String(" ", 6), New String(" ", 7), New String(" ", 8),
        New String(" ", 9), New String(" ", 10)
    }

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="MSG">The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.</param>
    ''' <param name="Indent"></param>
    ''' <returns>其实这个函数是不会返回任何东西的，只是因为为了Linq调试输出的需要，所以在这里是返回Nothing的</returns>
    <Extension> Public Function __DEBUG_ECHO(MSG As String, Optional Indent As Integer = 0) As String
        If Not Mute AndAlso __level < DebuggerLevels.Warning Then
            Dim head As String = $"DEBUG {Now.ToString}"
            Dim str As String = $"{_Indent(Indent)} {MSG}"

            Call Terminal.AddToQueue(Sub() Call __print(head, str, ConsoleColor.White))
#If DEBUG Then
            Call Debug.WriteLine($"[{head}]{str}")
#End If
        End If

        Return Nothing
    End Function

    Private Sub __print(head As String, str As String, msgColor As ConsoleColor)
        If ForceSTDError Then
            Call Console.Error.WriteLine($"[{head}]{str}")
        Else
            Dim cl As ConsoleColor = Console.ForegroundColor

            Call Console.Write("[")
            Console.ForegroundColor = DebuggerTagColor
            Call Console.Write(head)
            Console.ForegroundColor = cl
            Call Console.Write("]")

            Call WriteLine(str, msgColor)
        End If
    End Sub

    ''' <summary>
    ''' The function will print the exception details information on the standard <see cref="console"/>, <see cref="debug"/> console, and system <see cref="trace"/> console.
    ''' (分别在标准终端，调试终端，系统调试终端之中打印出错误信息，请注意，函数会直接返回False可以用于指定调用者函数的执行状态，这个函数仅仅是在终端上面打印出错误，不会保存为日志文件)
    ''' </summary>
    ''' <typeparam name="ex"></typeparam>
    ''' <param name="exception"></param>
    <Extension> Public Function PrintException(Of ex As Exception)(exception As ex, <CallerMemberName> Optional memberName As String = "") As Boolean
        Dim exMsg As String = New Exception(memberName, exception).ToString
        Return PrintException(exMsg, memberName)
    End Function

    ''' <summary>
    ''' 可以使用这个方法<see cref="MethodBase.GetCurrentMethod"/>.<see cref="GetFullName"/>获取得到<paramref name="memberName"/>所需要的参数信息
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="memberName"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PrintException(msg As String, <CallerMemberName> Optional memberName As String = "") As Boolean
        Dim exMsg As String = $"[ERROR {Now.ToString}] <{memberName}>::{msg}"
        Call Terminal.AddToQueue(Sub() Call VBDebugger.WriteLine(exMsg, ConsoleColor.Red))
        Return False
    End Function

    Public Sub WaitOutput()
        Call Terminal.WaitQueue()
    End Sub

    <Extension>
    Public Sub WriteLine(msg As String, color As ConsoleColor)
        If Mute Then
            Return
        End If

        If ForceSTDError Then
            Console.Error.WriteLine(msg)
        Else
            Dim cl As ConsoleColor = Console.ForegroundColor

            Console.ForegroundColor = color
            Console.WriteLine(msg)
            Console.ForegroundColor = cl
        End If

#If DEBUG Then
        Call Debug.WriteLine(msg)
#End If
    End Sub

    Const DebuggerTagColor As ConsoleColor = ConsoleColor.DarkGreen

    ''' <summary>
    ''' Display the wraning level(YELLOW color) message on the console.
    ''' </summary>
    ''' <param name="msg"></param>
    ''' <param name="calls"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Warning(msg As String, <CallerMemberName> Optional calls As String = "") As String
        If Not Mute Then
            Dim head As String = $"WARN@{calls} {Now.ToString}"

            Call Terminal.AddToQueue(Sub() Call __print(head, " " & msg, ConsoleColor.Yellow))
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
    Public Sub Assertion(test As Boolean, fails As String, level As Logging.MSG_TYPES, <CallerMemberName> Optional calls As String = "")
        If Not test = True Then
            If level = Logging.MSG_TYPES.DEBUG Then
                If __level < DebuggerLevels.Warning Then
                    Call fails.__DEBUG_ECHO(memberName:=calls)
                End If
            ElseIf level = Logging.MSG_TYPES.ERR Then
                If __level <> DebuggerLevels.Off Then
                    Call WriteLine(fails, ConsoleColor.Red)
                End If
            ElseIf level = Logging.MSG_TYPES.WRN Then
                If __level <> DebuggerLevels.Error Then
                    Call Warning(fails, calls)
                End If
            Else
                If __level < DebuggerLevels.Warning Then
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
    Public Sub Assertion(test As String, level As Logging.MSG_TYPES, <CallerMemberName> Optional calls As String = "")
        Call VBDebugger.Assertion((String.IsNullOrEmpty(test) OrElse String.IsNullOrWhiteSpace(test)), test, level, calls)
    End Sub

    ''' <summary>
    ''' If <paramref name="test"/> is false(means this assertion test failure), then throw exception.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="msg"></param>
    Public Sub Assertion(test As Boolean, msg As String, <CallerMemberName> Optional calls As String = "")
        If False = test Then
            Throw VisualBasicAppException.Creates(msg, calls)
        End If
    End Sub

    Public Function Assert(test As Boolean,
                           failed$,
                           Optional success$ = Nothing,
                           Optional failedLevel As Logging.MSG_TYPES = Logging.MSG_TYPES.ERR,
                           <CallerMemberName> Optional calls As String = "") As Boolean
        If test Then
            If Not String.IsNullOrEmpty(success) Then
                Call success.__DEBUG_ECHO
            End If

            Return True
        Else
            Select Case failedLevel
                Case Logging.MSG_TYPES.DEBUG
                    Call failed.__DEBUG_ECHO
                Case Logging.MSG_TYPES.ERR
                    Call failed.PrintException(calls)
                Case Logging.MSG_TYPES.WRN
                    Call failed.Warning(calls)
                Case Else
                    Call failed.Echo(calls)
            End Select

            Return False
        End If
    End Function

    ''' <summary>
    ''' VisualBasic application exception wrapper
    ''' </summary>
    Public Class VisualBasicAppException : Inherits Exception

        ''' <summary>
        ''' <see cref="Exception"/> inner wrapper
        ''' </summary>
        ''' <param name="ex">The exception details</param>
        ''' <param name="calls">Method name where occurs this exception.</param>
        Sub New(ex As Exception, calls As String)
            MyBase.New("@" & calls, ex)
        End Sub

        Public Shared Function Creates(msg As String, calls As String) As VisualBasicAppException
            Return New VisualBasicAppException(New Exception(msg), calls)
        End Function
    End Class

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="MSG">The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.</param>
    ''' <param name="Indent"></param>
    '''
    <Extension> Public Sub __DEBUG_ECHO(MSG As StringBuilder, Optional Indent As Integer = 0)
        Call MSG.ToString.__DEBUG_ECHO(Indent)
    End Sub

    <Extension> Public Sub __DEBUG_ECHO(Of T)(value As T, <CallerMemberName> Optional memberName As String = "")
        Call (Scripting.InputHandler.ToString(value) & "              @" & memberName).__DEBUG_ECHO
    End Sub

    ''' <summary>
    ''' Returns the current function name.
    ''' </summary>
    ''' <param name="caller">
    ''' The caller function name, do not assign any value to this parameter! Just leave it blank.
    ''' </param>
    ''' <returns></returns>
    Public Function this(<CallerMemberName> Optional caller As String = "") As String
        Return caller
    End Function

    <Extension> Public Sub Echo(Of T)(array As IEnumerable(Of T), <CallerMemberName> Optional memberName As String = "")
        Call String.Join(", ", array.ToArray(Function(obj) Scripting.ToString(obj))).__DEBUG_ECHO
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.Write"/>
    ''' </summary>
    ''' <param name="s"></param>
    <Extension> Public Sub Echo(s As String, <CallerMemberName> Optional memberName As String = "")
        If Not Mute Then
            Call Console.Write($"[{memberName}] {s}")
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
End Module
