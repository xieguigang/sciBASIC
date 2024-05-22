#Region "Microsoft.VisualBasic::bb553da908397724f06eb57e88178862, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Tasks\TaskQueue.vb"

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

    '   Total Lines: 257
    '    Code Lines: 149 (57.98%)
    ' Comment Lines: 66 (25.68%)
    '    - Xml Docs: 74.24%
    ' 
    '   Blank Lines: 42 (16.34%)
    '     File Size: 9.31 KB


    '     Interface ITaskHandle
    ' 
    '         Function: Run
    ' 
    '     Class TaskQueue
    ' 
    '         Properties: MaximumQueue, RunningTask, Tasks, uid
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) Join, ToString
    ' 
    '         Sub: __calls, __taskQueueEXEC, (+2 Overloads) Dispose, (+2 Overloads) Enqueue
    '         Class TaskWorker
    ' 
    '             Properties: Value
    ' 
    '             Function: ToString
    ' 
    '             Sub: Run
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar

Namespace Parallel.Tasks

    Public Interface ITaskHandle(Of T)

        Function Run() As T

    End Interface

    ''' <summary>
    ''' 这个只有一条线程来执行
    ''' </summary>
    Public Class TaskQueue(Of T) : Implements IDisposable

        ''' <summary>
        ''' ###### 2018-1-27
        ''' 
        ''' 如果直接在这里使用<see cref="App.BufferSize"/>的话，极端的情况下会导致服务器的内存直接被耗尽
        ''' 所以在这里使用一个较小的常数值
        ''' </summary>
        ReadOnly __tasks As Queue(Of TaskWorker)

        ''' <summary>
        ''' 返回当前的任务池之中的任务数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Tasks As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                SyncLock __tasks
                    Return __tasks.Count + If(RunningTask, 1, 0)
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property MaximumQueue As Boolean
            Get
                Return __tasks.Count >= queueSize
            End Get
        End Property

        ''' <summary>
        ''' 当这个属性为False的时候说明没有任务在执行，此时为空闲状态
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RunningTask As Boolean
        ''' <summary>
        ''' the unique name of current task
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property uid As String

        ''' <summary>
        ''' the name of the task is running
        ''' </summary>
        Dim task_name As String
        Dim queueSize As Integer
        Dim exceptionCallback As Action(Of String, Exception)
        Dim worker As TaskWorker

        ''' <summary>
        ''' 会单独启动一条新的线程来用来执行任务队列
        ''' </summary>
        Sub New(Optional name As String = Nothing,
                Optional queueSize As Integer = 16,
                Optional exceptionCallback As Action(Of String, Exception) = Nothing)

            __tasks = New Queue(Of TaskWorker)(queueSize)

#If DEBUG Then
            Call $"Using default buffer_size={App.BufferSize}".__DEBUG_ECHO
#End If
            Call RunTask(AddressOf __taskQueueEXEC)

            Me.exceptionCallback = exceptionCallback
            Me.queueSize = queueSize
            Me.uid = If(name, Me.GetHashCode.ToHexString)
        End Sub

        Public Overrides Function ToString() As String
            If worker IsNot Nothing AndAlso worker.progress >= 0 Then
                Return $"[{uid}{task_name}] {If(RunningTask, "running", "stop")}, queue {Tasks} tasks. [current '{worker.ToString}' {worker.progress.ToString("F2")}%]"
            Else
                Return $"[{uid}{task_name}] {If(RunningTask, "running", "stop")}, queue {Tasks} tasks."
            End If
        End Function

        ''' <summary>
        ''' 函数会被插入一个队列之中，之后线程会被阻塞在这里直到函数执行完毕，这个主要是用来控制服务器上面的任务并发的
        ''' 一般情况下不会使用这个方法，这个方法主要是控制服务器资源的利用程序的，当线程处于忙碌的状态的时候，
        ''' 当前线程会被一直阻塞，直到线程空闲
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <returns>假若本对象已经开始Dispose了，则为完成的任务都会返回Nothing</returns>
        Public Function Join(handle As Func(Of TaskWorker, T)) As T
            Dim task As New TaskWorker With {
                .handle = handle,
                .receiveDone = New ManualResetEvent(False)
            }

            SyncLock __tasks
                Call __tasks.Enqueue(task)
            End SyncLock

            Call task.receiveDone.WaitOne()
            Call task.receiveDone.Reset()

            Return task.Value
        End Function

        Public Function Join(handle As ITaskHandle(Of T)) As T
            Return Join(AddressOf handle.Run)
        End Function

        Public Sub Enqueue(handle As ITaskHandle(Of T), Optional callback As Action(Of T) = Nothing)
            Call Enqueue(AddressOf handle.Run, callback)
        End Sub

        ''' <summary>
        ''' 这个函数只会任务添加到队列之中，而不会阻塞线程
        ''' </summary>
        ''' <param name="handle"></param>
        Public Sub Enqueue(handle As Func(Of TaskWorker, T),
                           Optional callback As Action(Of T) = Nothing,
                           Optional name As String = Nothing)

            Dim task As New TaskWorker With {
                .handle = handle,
                .callback = callback,
                .name = name
            }

            SyncLock __tasks
                Call __tasks.Enqueue(task)
            End SyncLock
        End Sub

        ''' <summary>
        ''' 有一条线程单独执行这个任务队列
        ''' </summary>
        Private Sub __taskQueueEXEC()
            Do While Not disposedValue
                If Not Tasks = 0 Then
                    Call __calls()
                Else
                    ' 当前的线程处于空闲的状态
                    Call Thread.Sleep(3)
                End If
            Loop
        End Sub

        Private Sub __calls()
            Dim task As TaskWorker

            SyncLock __tasks
                task = __tasks.Dequeue
                worker = task
            End SyncLock

            task_name = If(task.name.StringEmpty, "", $"::{task.name}")
            _RunningTask = True
            Call task.Run(Me)
            If Not task.receiveDone Is Nothing Then
                Call task.receiveDone.Set()
            End If
            _RunningTask = False
        End Sub

        ''' <summary>
        ''' A task
        ''' </summary>
        Public Class TaskWorker

            Public callback As Action(Of T)
            Public handle As Func(Of TaskWorker, T)
            Public receiveDone As ManualResetEvent
            Public name As String

            ''' <summary>
            ''' [0,100]
            ''' </summary>
            ''' <remarks>
            ''' negative value means no progress report
            ''' </remarks>
            Public progress As Double = -1

            Public ReadOnly Property Value As T

            Sub Run(q As TaskQueue(Of T))
                Try
                    _Value = handle(Me)
                Catch ex As Exception
                    Call App.LogException(ex)
                    Call ex.PrintException

                    If Not q.exceptionCallback Is Nothing Then
                        Call q.exceptionCallback(name, ex)
                    End If
                End Try

                If Not callback Is Nothing Then
                    Call callback(Value)
                End If
            End Sub

            Public Overrides Function ToString() As String
                If progress < 0 Then
                    Return "<TASK>"
                Else
                    Return Program.ProgressText(progress / 100, 16)
                End If
            End Function
        End Class

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    For Each x As TaskWorker In __tasks
                        If Not x.receiveDone Is Nothing Then
                            ' 释放所有的线程阻塞
                            Call x.receiveDone.Set()
                        End If
                    Next
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

End Namespace
