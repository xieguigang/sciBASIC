Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ConsoleDevice
Imports Microsoft.VisualBasic.ConsoleDevice.Utility
Imports Microsoft.VisualBasic.Linq.Extensions

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
    '''
    <Extension> Public Function __DEBUG_ECHO(MSG As String, Optional Indent As Integer = 0) As String
        Dim str = $"[DEBUG {Now.ToString}]{_Indent(Indent)} {MSG}"

        If Not Mute AndAlso __level < DebuggerLevels.Warning Then
            Call Console.WriteLine(str)
#If DEBUG Then
            Call Debug.WriteLine(str)
#End If
        End If

        Return str
    End Function

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

    Public Function PrintException(msg As String, <CallerMemberName> Optional memberName As String = "") As Boolean
        Dim exMsg As String = $"[ERROR {Now.ToString}]  @{memberName}::{msg}"
        Call VBDebugger.WriteLine(exMsg, ConsoleColor.Red)
        Return False
    End Function

    Public Sub WriteLine(msg As String, color As ConsoleColor)
        If Mute Then
            Return
        End If

        Dim cl As ConsoleColor = Console.ForegroundColor

        Console.ForegroundColor = color
        Console.WriteLine(msg)
        Console.ForegroundColor = cl

#If DEBUG Then
        Call Debug.WriteLine(msg)
#End If
    End Sub

    Public Function Warning(msg As String, <CallerMemberName> Optional calls As String = "") As String
        Dim str = $"[WARN@{calls} {Now.ToString}] {msg}"

        If Not Mute Then
            Call WriteLine(str, ConsoleColor.Yellow)
        End If

        Return str
    End Function

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

    <Extension>
    Public Sub Assertion(test As String, level As Logging.MSG_TYPES, <CallerMemberName> Optional calls As String = "")
        Call VBDebugger.Assertion(Not (String.IsNullOrEmpty(test) OrElse String.IsNullOrWhiteSpace(test)), test, level, calls)
    End Sub

    ''' <summary>
    ''' If test is false(means this assertion test failure), then throw exception.
    ''' </summary>
    ''' <param name="test"></param>
    ''' <param name="msg"></param>
    Public Sub Assertion(test As Boolean, msg As String, <CallerMemberName> Optional calls As String = "")
        If False = test Then
            Throw VisualBasicAppException.Creates(msg, calls)
        End If
    End Sub

    Public Class VisualBasicAppException : Inherits Exception

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
    <Extension> Public Function __DEBUG_ECHO(MSG As System.Text.StringBuilder, Optional Indent As Integer = 0) As String
        Return MSG.ToString.__DEBUG_ECHO(Indent)
    End Function

    <Extension> Public Function __DEBUG_ECHO(Of T)(value As T, <CallerMemberName> Optional memberName As String = "") As String
        Return (Scripting.InputHandler.ToString(value) & "              @" & memberName).__DEBUG_ECHO
    End Function

    Public Function WhoseThere(<CallerMemberName> Optional memberName As String = "") As String
        Return memberName
    End Function

    <Extension> Public Function Echo(Of T)(array As Generic.IEnumerable(Of T), <CallerMemberName> Optional memberName As String = "") As String
        Return String.Join(", ", array.ToArray(Function(obj) Scripting.ToString(obj))).__DEBUG_ECHO
    End Function

    ''' <summary>
    ''' Alias for <see cref="Console.Write"/>
    ''' </summary>
    ''' <param name="s"></param>
    <Extension> Public Sub Echo(s As String)
        Call Console.Write(s)
    End Sub

    ''' <summary>
    ''' Alias for <see cref="Console.Write"/>
    ''' </summary>
    ''' <param name="c"></param>
    <Extension> Public Sub Echo(c As Char)
        Call Console.Write(c)
    End Sub
End Module
