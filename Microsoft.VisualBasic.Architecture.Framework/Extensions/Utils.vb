Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes

Public Module Utils

    ''' <summary>
    ''' Returns the total executation time of the target <paramref name="work"/>.
    ''' (性能测试工具，函数之中会自动输出整个任务所经历的处理时长)
    ''' </summary>
    ''' <param name="work">
    ''' Function pointer of the task work that needs to be tested.(需要测试性能的工作对象)
    ''' </param>
    ''' <returns>Returns the total executation time of the target <paramref name="work"/>. ms</returns>
    Public Function Time(work As Action, Optional echo As Boolean = True) As Long
        Dim startTick As Long = App.NanoTime
        Call work()
        Dim endTick As Long = App.NanoTime
        Dim t& = (endTick - startTick) / TimeSpan.TicksPerMillisecond
        If echo Then
            Call $"[{work.Method.Name}] takes {t}ms...".__DEBUG_ECHO
        End If
        Return t
    End Function

    Public Function Time(Of T)(work As Func(Of T), Optional ByRef ms& = 0) As T
        Dim startTick As Long = App.NanoTime
        Dim value As T = work()
        Dim endTick As Long = App.NanoTime
        ms& = (endTick - startTick) / TimeSpan.TicksPerMillisecond
        Call $"Work takes {ms}ms...".__DEBUG_ECHO
        Return value
    End Function

    Public Delegate Function WaitHandle() As Boolean

    ''' <summary>
    ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
    ''' </summary>
    ''' <param name="handle"></param>
    <Extension> Public Sub Wait(handle As Func(Of Boolean))
        If handle Is Nothing Then
            Return
        End If

        Do While handle() = False
            Call Threading.Thread.Sleep(10)
            Call Application.DoEvents()
        Loop
    End Sub

    ''' <summary>
    ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
    ''' </summary>
    ''' <param name="handle"></param>
    <Extension> Public Sub Wait(handle As WaitHandle)
        If handle Is Nothing Then
            Return
        End If

        Do While handle() = False
            Call Threading.Thread.Sleep(10)
            Call Application.DoEvents()
        Loop
    End Sub

    ''' <summary>
    ''' If the path string value is already wrappered by quot, then this function will returns the original string (DO_NOTHING).
    ''' (假若命令行之中的文件名参数之中含有空格的话，则可能会造成错误，需要添加一个双引号来消除歧义)
    ''' </summary>
    ''' <param name="Path"></param>
    ''' <returns></returns>
    '''
    <ExportAPI("CLI_PATH")>
    <Extension> Public Function CLIPath(Path As String) As String
        If String.IsNullOrEmpty(Path) Then
            Return ""
        Else
            Path = Path.Replace("\", "/")  '这个是R、Java、Perl等程序对路径的要求所导致的
            Return Path.CLIToken
        End If
    End Function

    ''' <summary>
    ''' <see cref="CLIPath(String)"/>函数为了保持对Linux系统的兼容性会自动替换\为/符号，这个函数则不会执行这个替换
    ''' </summary>
    ''' <param name="Token"></param>
    ''' <returns></returns>
    <Extension> Public Function CLIToken(Token As String) As String
        If String.IsNullOrEmpty(Token) OrElse Not Len(Token) > 2 Then
            Return Token
        End If

        If Token.First = """"c AndAlso Token.Last = """"c Then
            Return Token
        End If
        If Token.Contains(" "c) Then Token = $"""{Token}"""
        Return Token
    End Function

    ''' <summary>
    ''' *.txt -> text
    ''' </summary>
    ''' <param name="ext$"></param>
    ''' <returns></returns>
    <Extension> Public Function GetMIMEDescrib(ext$) As ContentType
        Dim key$ = LCase(ext).Trim("*"c)

        If MIME.SuffixTable.ContainsKey(key) Then
            Return MIME.SuffixTable(key)
        Else
            Return MIME.UnknownType
        End If
    End Function
End Module
