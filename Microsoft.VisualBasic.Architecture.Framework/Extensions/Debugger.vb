Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ConsoleDevice

''' <summary>
''' Debugger helper module for VisualBasic Enterprises System.
''' </summary>
Public Module VBDebugger

    Private ReadOnly _Indent As String() = {
        "",
        New String(" ", 1), New String(" ", 2), New String(" ", 3), New String(" ", 4),
        New String(" ", 5), New String(" ", 6), New String(" ", 7), New String(" ", 8),
        New String(" ", 9), New String(" ", 10)}

    ''' <summary>
    ''' Output the full debug information while the project is debugging in debug mode.
    ''' (向标准终端和调试终端输出一些带有时间戳的调试信息)
    ''' </summary>
    ''' <param name="MSG">The message fro output to the debugger console, this function will add a time stamp automaticly To the leading position Of the message.</param>
    ''' <param name="Indent"></param>
    ''' 
    <Extension> Public Function __DEBUG_ECHO(MSG As String, Optional Indent As Integer = 0) As String
        Dim str = $"{_Indent(Indent)}[DEBUG {Now.ToString}]  {MSG}"
        Call Console.WriteLine(str)
#If DEBUG Then
        Call Debug.WriteLine(str)
        Call Trace.WriteLine(str)
#End If
        Return str
    End Function

    ''' <summary>
    ''' The function will print the exception details information on the standard <see cref="console"/>, <see cref="debug"/> console, and system <see cref="trace"/> console.
    ''' (分别在标准终端，调试终端，系统调试终端之中打印出错误信息，请注意，函数会直接返回False可以用于指定调用者函数的执行状态)
    ''' </summary>
    ''' <typeparam name="ex"></typeparam>
    ''' <param name="exception"></param>
    <Extension> Public Function PrintException(Of ex As Exception)(exception As ex, <CallerMemberName> Optional memberName As String = "") As Boolean
        Dim exMsg As String = New Exception($"[DEBUG {Now.ToString}]  @{memberName}", exception).ToString

        Call xConsole.WriteLine($"^r{exMsg}^!")
#If DEBUG Then
        Call Debug.WriteLine(exMsg)
        Call Trace.WriteLine(exMsg)
#End If

        Return False
    End Function

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
End Module
