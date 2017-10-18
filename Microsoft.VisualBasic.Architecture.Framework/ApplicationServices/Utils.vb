#Region "Microsoft.VisualBasic::c0f04ba4a368e85f6d19aa3268681428, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Utils.vb"

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
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace ApplicationServices

    Public Module Utils

        <Extension>
        Public Sub TryRun(task As Action, <CallerMemberName> Optional stack$ = Nothing)
            Try
                Call task()
            Catch ex As Exception
                Call $"[{stack}] {task.Method.ToString} failure!".Warning
                Call App.LogException(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Run background task, if the <see cref="AsyncHandle(Of Exception).GetValue()"/> returns nothing, 
        ''' then means the task run no errors.
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="stack">进行调用堆栈的上一层的栈名称</param>
        ''' <returns></returns>
        <Extension> Public Function TaskRun(task As Action, <CallerMemberName> Optional stack$ = Nothing) As AsyncHandle(Of Exception)
            Dim handle = Function() As Exception
                             Try
                                 Call task()
                             Catch ex As Exception
                                 Return New Exception(stack, ex)
                             End Try

                             Return Nothing
                         End Function
            Return New AsyncHandle(Of Exception)(handle).Run
        End Function

        ''' <summary>
        ''' Returns the total executation time of the target <paramref name="work"/>.
        ''' (性能测试工具，函数之中会自动输出整个任务所经历的处理时长)
        ''' </summary>
        ''' <param name="work">
        ''' Function pointer of the task work that needs to be tested.(需要测试性能的工作对象)
        ''' </param>
        ''' <returns>Returns the total executation time of the target <paramref name="work"/>. ms</returns>
        Public Function Time(work As Action) As Long
            Dim startTick As Long = App.NanoTime

            ' -------- start worker ---------
            Call work()
            ' --------- end worker ---------

            Dim endTick As Long = App.NanoTime
            Dim t& = (endTick - startTick) / TimeSpan.TicksPerMillisecond
            Return t
        End Function

        Public Function Time(Of T)(work As Func(Of T), Optional ByRef ms& = 0, Optional tick As Boolean = True, Optional trace$ = Nothing) As T
            Dim tickTask As AsyncHandle(Of Exception)

            If tick Then
                tickTask = Utils.TaskRun(
                    Sub()
                        Do While tick
                            Call Console.Write(".")
                            Call Thread.Sleep(1000)
                        Loop
                    End Sub)
            End If

            Dim value As T
            Dim task As Action = Sub() value = work()

            task.BENCHMARK(trace)
            tick = False  ' 需要使用这个变量的变化来控制 tickTask 里面的过程

            Return value
        End Function

        ''' <summary>
        ''' Format ``ms`` for content print.
        ''' </summary>
        ''' <param name="ms"></param>
        ''' <returns></returns>
        <Extension> Public Function FormatTicks(ms&) As String
            If ms > 1000 Then
                Dim s = ms / 1000

                If s < 1000 Then
                    Return s & "s"
                Else
                    Dim min = s \ 60
                    Return $"{min}min{s Mod 60}s"
                End If
            Else
                Return ms & "ms"
            End If
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
                Call Thread.Sleep(10)
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
                Call Thread.Sleep(10)
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
End Namespace
