#Region "Microsoft.VisualBasic::de395b104175dba64d26f62a00488f07, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\BatchTasks.vb"

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

    '     Module BatchTasks
    ' 
    '         Function: (+2 Overloads) BatchTask
    ' 
    '         Sub: BatchTask
    '         Structure __threadHelper
    ' 
    '             Function: __task
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks

Namespace Parallel.Threads

    ''' <summary>
    ''' Parallel batch task tool for processor
    ''' </summary>
    Public Module BatchTasks

        ''' <summary>
        ''' 当所需要进行计算的数据量比较大的时候，建议分块使用本函数生成多个进程进行批量计算以获得较好的计算效率
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="getCLI"></param>
        ''' <param name="getExe"></param>
        ''' <param name="numThreads">-1表示使用系统自动配置的参数，一次性提交所有的计算任务可能会是计算效率变得很低，所以需要使用这个参数来控制计算的线程数量</param>
        ''' <param name="TimeInterval">默认的任务提交时间间隔是一秒钟提交一个新的计算任务</param>
        Public Sub BatchTask(Of T)(source As IEnumerable(Of T),
                                   getCLI As Func(Of T, String),
                                   getExe As Func(Of String),
                                   Optional numThreads As Integer = -1,
                                   Optional TimeInterval As Integer = 1000)

            Dim srcArray As Func(Of Integer)() =
                LinqAPI.Exec(Of Func(Of Integer)) <= From x As T In source
                                                     Let task As IORedirectFile =
                                                         New IORedirectFile(getExe(), getCLI(x))
                                                     Let runTask As Func(Of Integer) = AddressOf task.Run
                                                     Select runTask
            Call BatchTask(srcArray, numThreads, TimeInterval)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="TIn"></typeparam>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="getTask"></param>
        ''' <param name="numThreads">
        ''' 可以在这里手动的控制任务的并发数，这个数值小于或者等于零则表示自动配置线程的数量，如果想要单线程，请将这个参数设置为1
        ''' </param>
        ''' <param name="TimeInterval"></param>
        ''' <returns></returns>
        <Extension>
        Public Function BatchTask(Of TIn, T)(source As IEnumerable(Of TIn),
                                             getTask As Func(Of TIn, T),
                                             Optional numThreads As Integer = -1,
                                             Optional TimeInterval As Integer = 1000) As T()
            Dim taskHelper As New __threadHelper(Of TIn, T) With {
                .__invoke = getTask
            }
            Return source _
                .Select(AddressOf taskHelper.__task) _
                .ToArray _
                .BatchTask(numThreads, TimeInterval)
        End Function

        Private Structure __threadHelper(Of TIn, T)

            Public __invoke As Func(Of TIn, T)

            Public Function __task(obj As TIn) As Func(Of T)
                Dim __invoke As Func(Of TIn, T) = Me.__invoke
                Return Function() __invoke(obj)
            End Function
        End Structure

        ''' <summary>
        ''' Using parallel linq that may stuck the program when a linq task partion wait a long time task to complete. 
        ''' By using this parallel function that you can avoid this problem from parallel linq, and also you can 
        ''' controls the task thread number manually by using this parallel task function.
        ''' (由于LINQ是分片段来执行的，当某个片段有一个线程被卡住之后整个进程都会被卡住，所以执行大型的计算任务的时候效率不太好，
        ''' 使用这个并行化函数可以避免这个问题，同时也可以自己手动控制线程的并发数)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="actions">Tasks collection</param>
        ''' <param name="numThreads">
        ''' You can controls the parallel tasks number from this parameter, smaller or equals to ZERO means auto 
        ''' config the thread number, If want single thread, not parallel, set this value to 1, and positive 
        ''' value greater than 1 will makes the tasks parallel.
        ''' (可以在这里手动的控制任务的并发数，这个数值小于或者等于零则表示自动配置线程的数量, 1为单线程)
        ''' </param>
        ''' <param name="TimeInterval">The task run loop sleep time, unit is **ms**</param>
        ''' <param name="smart">
        ''' ZERO or negative value will turn off this smart mode, default value is ZERO, mode was turn off.
        ''' If this parameter value is set to any positive value, that means this smart mode will be turn on.
        ''' then, if the CPU load is higher than the value of this parameter indicated, then no additional 
        ''' task thread would be added, if CPU load lower than this parameter value, then some additional 
        ''' task thread will be added for utilize the CPU resources and save the computing time. 
        ''' (假若开启smart模式的话，在CPU负载较高的时候会保持在限定的线程数量来执行批量任务，
        ''' 假若CPU的负载较低的话，则会开启超量的线程，以保持执行效率充分利用计算资源来节省总任务的执行时间
        ''' 任意正实数都将会开启smart模式
        ''' 小于等于零的数将不会开启，默认值为零，不开启)
        ''' </param>
        <Extension>
        Public Function BatchTask(Of T)(actions As Func(Of T)(), Optional numThreads% = -1%, Optional timeInterval% = 1000%, Optional smart# = 0#) As T()
            Dim taskPool As New List(Of AsyncHandle(Of T))
            Dim p As New Pointer
            Dim resultList As New List(Of T)
            ' Dim CPU#

            If numThreads <= 0 Then
                numThreads = LQuerySchedule.CPU_NUMBER * 2
            End If

            Call $"num_threads: {numThreads}".__INFO_ECHO
            Call $"CPU allocates: {smart}".__INFO_ECHO

            Do While p <= (actions.Length - 1)
                If taskPool.Count < numThreads Then
                    ' 向任务池里面添加新的并行任务
                    ' 任务数量小于指定值的情况下，会直接添加计算任务直到满足数量条件
                    taskPool += New AsyncHandle(Of T)(actions(++p)).Run
                Else
                    'If smart > 0# Then
                    '    ' 这里是smart模式
                    '    ' CPU的负载在指定值之内，则smart模式开启的情况下会添加新的额外的计算任务
                    '    CPU = Win32.TaskManager.ProcessUsage

                    '    If CPU < smart Then
                    '        taskPool += New AsyncHandle(Of T)(actions(++p)).Run
                    '        Call $"CPU:{CPU}% <= {smart}, join an additional task thread...".__DEBUG_ECHO
                    '    End If
                    'End If
                End If

                ' 在这里获得完成的任务
                Dim LQuery As AsyncHandle(Of T)() =
                    LinqAPI.Exec(Of AsyncHandle(Of T)) _
 _
                    () <= From task As AsyncHandle(Of T)
                          In taskPool
                          Where task.IsCompleted
                          Select task

                For Each completeTask As AsyncHandle(Of T) In LQuery
                    ' 将完成的任务从任务池之中移除然后获取返回值
                    Call taskPool.Remove(completeTask)
                    Call resultList.Add(completeTask.GetValue)
                Next

                If timeInterval > 0 Then
                    Call Thread.Sleep(timeInterval)
                End If
            Loop

            ' 等待剩余的计算任务完成计算过程
            Dim waitForExit As T() =
                LinqAPI.Exec(Of T) <= From task As AsyncHandle(Of T)
                                      In taskPool.AsParallel
                                      Let cli As T = task.GetValue
                                      Select cli
            resultList += waitForExit

            Return resultList.ToArray
        End Function
    End Module
End Namespace
