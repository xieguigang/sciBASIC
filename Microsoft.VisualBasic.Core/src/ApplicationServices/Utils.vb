﻿#Region "Microsoft.VisualBasic::15771871aee69b9c82b72f91dcebade0, Microsoft.VisualBasic.Core\src\ApplicationServices\Utils.vb"

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

    '   Total Lines: 255
    '    Code Lines: 148 (58.04%)
    ' Comment Lines: 74 (29.02%)
    '    - Xml Docs: 87.84%
    ' 
    '   Blank Lines: 33 (12.94%)
    '     File Size: 9.97 KB


    '     Module Utils
    ' 
    '         Function: Shell, TaskRun, (+2 Overloads) Time
    ' 
    '         Sub: TryRun
    '         Delegate Function
    ' 
    '             Function: CLIPath, CLIToken, FileMimeType, GetMIMEDescrib
    ' 
    '             Sub: print, printf, (+2 Overloads) Wait
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Text.Parser

Namespace ApplicationServices

    ''' <summary>
    ''' App utils
    ''' </summary>
    Public Module Utils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="commandLine"></param>
        ''' <param name="windowStyle"></param>
        ''' <param name="waitForExit">
        ''' If NOT, then the function returns the associated process id value,
        ''' else returns the process exit code.
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Shell(commandLine$, Optional windowStyle As ProcessWindowStyle = ProcessWindowStyle.Normal, Optional waitForExit As Boolean = False) As Integer
            Dim tokens As String() = DelimiterParser.GetTokens(commandLine)
            Dim command As String = tokens.First
            Dim arguments As String = tokens.Skip(1).Select(AddressOf Utils.CLIToken).JoinBy(" ")

            Using child As New Process
                Dim pInfo As New ProcessStartInfo(command, arguments)

                child.StartInfo = pInfo
                child.StartInfo.WindowStyle = windowStyle

                Call child.Start()

                If Not waitForExit Then
                    Return child.Id
                Else
                    Call child.WaitForExit()
                    Return child.ExitCode
                End If
            End Using
        End Function

        ''' <summary>
        ''' Call target <see cref="Action"/> delegate, if exception occurs in the action, 
        ''' then this function will logs the exception and exit without thorw an exception. 
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="stack$"></param>
        <Extension>
        Public Sub TryRun(task As Action, <CallerMemberName> Optional stack$ = Nothing)
            Try
                Call task()
            Catch ex As Exception
                Call $"[{stack}] {task.Method.ToString} failure!".warning
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
        <Extension>
        Public Function TaskRun(task As Action, <CallerMemberName> Optional stack$ = Nothing) As AsyncHandle(Of Exception)
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
            Dim startTick As DateTime = Now
            ' -------- start worker ---------
            Call work()
            ' --------- end worker ---------
            Dim endTick As DateTime = Now
            Dim t& = (endTick - startTick).TotalMilliseconds

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

            task.benchmark(trace)
            tick = False  ' 需要使用这个变量的变化来控制 tickTask 里面的过程

            Return value
        End Function

        Public Delegate Function TaskWaitHandle() As Boolean

        ''' <summary>
        ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
        ''' </summary>
        ''' <param name="handle"></param>
        <Extension>
        Public Sub Wait(handle As Func(Of Boolean))
            If handle Is Nothing Then
                Return
            End If

            Do While handle() = False
                Call Thread.Sleep(10)
            Loop
        End Sub

        ''' <summary>
        ''' 假若条件判断<paramref name="handle"/>不为真的话，函数会一直阻塞线程，直到条件判断<paramref name="handle"/>为真
        ''' </summary>
        ''' <param name="handle"></param>
        <Extension>
        Public Sub Wait(handle As TaskWaitHandle)
            If handle Is Nothing Then
                Return
            End If

            Do While handle() = False
                Call Thread.Sleep(10)
            Loop
        End Sub

        ''' <summary>
        ''' If the path string value is already wrappered by quot, then this function will returns the original string (DO_NOTHING).
        ''' (假若命令行之中的文件名参数之中含有空格的话，则可能会造成错误，需要添加一个双引号来消除歧义)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns>
        ''' A unix system compatible file path
        ''' </returns>
        <ExportAPI("CLI_PATH")>
        <Extension> Public Function CLIPath(path As String) As String
            If String.IsNullOrEmpty(path) Then
                Return ""
            Else
                ' 这个是R、Java、Perl等程序对路径的要求所导致的
                path = path.Replace("\", "/")
            End If

            Return path.CLIToken
        End Function

        ''' <summary>
        ''' <see cref="CLIPath(String)"/>函数为了保持对Linux系统的兼容性会自动替换\为/符号，这个函数则不会执行这个替换
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CLIToken(token As String) As String
            If token.StringEmpty Then
                ' empty string or white space token should be wrapped via the quot symbol
                ' or it will be ignored in the commandline string
                Return $"""{token}"""
            ElseIf Not Len(token) > 2 Then
                Return token
            End If

            ' the current token is already been wrapped via a quote symbol
            ' no needs for the furthur processing
            If token.First = """"c AndAlso token.Last = """"c Then
                Return token
            End If
            If token.Contains(" "c) Then
                token = $"""{token}"""
            End If

#If NETCOREAPP Then
            ' 20210819 fix for docker command on unix platform
            If token.TextEquals("$PWD") Then
                token = """$PWD"""
            ElseIf token.TextEquals("$PWD:$PWD") Then
                token = """$PWD:$PWD"""
            End If
#End If

            Return token
        End Function

        ''' <summary>
        ''' ``*.txt -> text``，这个函数是作用于文件的拓展名之上的
        ''' </summary>
        ''' <param name="ext"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetMIMEDescrib(ext$, Optional defaultUnknown As Boolean = True) As ContentType
            Dim key$ = LCase(ext).Trim("*"c)

            If MIME.SuffixTable.ContainsKey(key) Then
                Return MIME.SuffixTable(key)
            ElseIf defaultUnknown Then
                Return MIME.UnknownType
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 与<see cref="GetMIMEDescrib(String,Boolean)"/>所不同的是，这个函数是直接作用于文件路径之上的。
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FileMimeType(path As String, Optional defaultUnknown As Boolean = True) As ContentType
            Return ("*." & path.ExtensionSuffix).GetMIMEDescrib(defaultUnknown)
        End Function

        <Extension>
        Public Sub printf(dev As TextWriter, format$, ParamArray args As Object())
            Call dev.Write(sprintf(format, args))
        End Sub

        <Extension>
        Public Sub print(dev As TextWriter, line As String)
            Call dev.Write(line)
        End Sub
    End Module
End Namespace
