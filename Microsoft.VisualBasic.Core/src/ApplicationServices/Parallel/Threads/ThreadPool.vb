#Region "Microsoft.VisualBasic::770ded09dc182a6fe5ffc77b0cce8418, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\ThreadPool.vb"

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

    '   Total Lines: 353
    '    Code Lines: 200
    ' Comment Lines: 97
    '   Blank Lines: 56
    '     File Size: 13.14 KB


    '     Class ThreadPool
    ' 
    '         Properties: FullCapacity, NumOfThreads, WorkingThreads
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetAvaliableThread, GetStatus, OperationTimeOut, Start, ToString
    ' 
    '         Sub: [Exit], allocate, (+2 Overloads) Dispose, (+2 Overloads) RunTask, WaitAll
    '         Structure __taskInvoke
    ' 
    '             Function: Run
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Parallel.Tasks.TaskQueue(Of Long)
Imports TaskBinding = Microsoft.VisualBasic.ComponentModel.Binding(Of
    System.Action(Of Microsoft.VisualBasic.Parallel.Tasks.TaskQueue(Of Long).TaskWorker),
    System.Action(Of Long)
)

Namespace Parallel.Threads

    ''' <summary>
    ''' 使用多条线程来执行任务队列，推荐在编写Web服务器的时候使用这个模块来执行任务
    ''' </summary>
    Public Class ThreadPool : Implements IDisposable

        ReadOnly threads As TaskQueue(Of Long)()

        ''' <summary>
        ''' 临时的句柄缓存
        ''' </summary>
        ReadOnly pendings As New Queue(Of TaskBinding)(capacity:=10240)

        ''' <summary>
        ''' 线程池之中的线程数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumOfThreads As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return threads.Length
            End Get
        End Property

        ''' <summary>
        ''' 返回当前正在处于工作状态的线程数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property WorkingThreads As Integer
            Get
                Dim n As Integer

                For Each t In threads
                    If t.Tasks > 0 OrElse t.RunningTask Then
                        n += 1
                    End If
                Next

                Return n
            End Get
        End Property

        '''' <summary>
        '''' Returns the server load.
        '''' </summary>
        '''' <returns></returns>
        'Public ReadOnly Property ServerLoad As Double
        '    Get
        '        Dim works# = WorkingThreads / NumOfThreads
        '        Dim CPU_load# = Win32.TaskManager.ProcessUsage
        '        Dim load# = works * CPU_load

        '        Return load
        '    End Get
        'End Property

        ''' <summary>
        ''' 是否所有的线程都是处于工作状态的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FullCapacity As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return WorkingThreads = threads.Length
            End Get
        End Property

        Dim totalTask As Integer
        Dim popoutTask As Integer

        Sub New(maxThread As Integer,
                Optional maxQueueSize As Integer = 2,
                Optional exceptionCallback As Action(Of String, Exception) = Nothing)

            threads = New TaskQueue(Of Long)(maxThread) {}

            For i As Integer = 0 To threads.Length - 1
                threads(i) = New TaskQueue(Of Long)(
                    queueSize:=maxQueueSize,
                    exceptionCallback:=exceptionCallback
                )
            Next
        End Sub

        Sub New(Optional exceptionCallback As Action(Of String, Exception) = Nothing)
            Me.New(LQuerySchedule.CPU_NUMBER, exceptionCallback:=exceptionCallback)
        End Sub

        ''' <summary>
        ''' Start the background task, and stop until the <see cref="Dispose()"/> method has been called
        ''' </summary>
        ''' <returns></returns>
        Public Function Start() As ThreadPool
            Call ParallelExtension.RunTask(AddressOf allocate)
            Return Me
        End Function

        ''' <summary>
        ''' 获取当前的这个线程池对象的状态的摘要信息
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStatus() As Dictionary(Of String, String)
            Dim out As New Dictionary(Of String, String)

            Call out.Add(NameOf(Me.FullCapacity), FullCapacity)
            Call out.Add(NameOf(Me.NumOfThreads), NumOfThreads)
            Call out.Add(NameOf(Me.WorkingThreads), WorkingThreads)
            Call out.Add(NameOf(Me.pendings), pendings.Count)

            For Each t As SeqValue(Of TaskQueue(Of Long)) In threads.SeqIterator
                With t.value
                    Call out.Add("thread___" & t.i & "___" & .uid, .Tasks)
                End With
            Next

            Return out
        End Function

        ''' <summary>
        ''' Push a new task into the parallel task queue.
        ''' (使用线程池里面的空闲线程来执行任务)
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="callback">回调函数里面的参数是任务的执行的时间长度</param>
        ''' <param name="name">the name of current task</param>
        Public Sub RunTask(task As Action(Of TaskWorker),
                           Optional callback As Action(Of Long) = Nothing,
                           Optional name As String = Nothing)

            Dim pends As New TaskBinding With {
                .Bind = task,
                .Target = callback,
                .name = name
            }

            totalTask += 1

            SyncLock pendings
                Call pendings.Enqueue(pends)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Push a new task into the parallel task queue.
        ''' (使用线程池里面的空闲线程来执行任务)
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="callback">回调函数里面的参数是任务的执行的时间长度</param>
        ''' <param name="name">the name of current task</param>
        Public Sub RunTask(task As Action,
                           Optional callback As Action(Of Long) = Nothing,
                           Optional name As String = Nothing)

            Dim pends As New TaskBinding With {
                .Bind = Sub(worker) task(),
                .Target = callback,
                .name = name
            }

            totalTask += 1

            SyncLock pendings
                Call pendings.Enqueue(pends)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Run a speicifc task with assert of operation 
        ''' is time out or not.
        ''' </summary>
        ''' <param name="task">
        ''' A specific task to run
        ''' </param>
        ''' <param name="timeout">
        ''' wait timeout in unit milliseconds
        ''' </param>
        ''' <returns>
        ''' + true means timeout, the task has not been finished;
        ''' + false means the task has been execute success without timeout
        ''' </returns>
        Public Function OperationTimeOut(task As Action, timeout As Integer) As Boolean
            Dim done As Boolean = False

            Call RunTask(task, callback:=Sub() done = True)

            For i As Integer = 0 To timeout
                If done Then
                    Return False
                Else
                    Call Thread.Sleep(1)
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' allocate the task into the task thread pool
        ''' </summary>
        Private Sub allocate()
            Do While Not Me.disposedValue
                Dim task As TaskBinding = Nothing
                Dim taskThread As TaskQueue(Of Long) = GetAvaliableThread()

                If Not taskThread.MaximumQueue Then
                    SyncLock pendings
                        If pendings.Count > 0 Then
                            task = pendings.Dequeue
                            popoutTask += 1
                        End If
                    End SyncLock

                    If Not task.IsEmpty Then
                        Dim h As Func(Of TaskWorker, Long) = AddressOf New __taskInvoke With {.task = task.Bind}.Run
                        Dim callback As Action(Of Long) = task.Target

                        ' 当线程池里面的线程数量非常多的时候，这个事件会变长，
                        ' 所以讲分配的代码单独放在线程里面执行，以提神web
                        ' 服务器的响应效率
                        Call taskThread.Enqueue(h, callback, name:=task.name)
                    End If
                End If

                Call Thread.Sleep(1)
            Loop
        End Sub

        Private Structure __taskInvoke

            Dim task As Action(Of TaskWorker)

            ''' <summary>
            ''' 不清楚是不是因为lambda有问题，所以导致计时器没有正常的工作，所以在这里使用内部类来工作
            ''' </summary>
            ''' <returns></returns>
            Public Function Run(worker As TaskWorker) As Long
                Dim time& = App.NanoTime
                Call task(worker)
                Return App.NanoTime - time
            End Function
        End Structure

        ''' <summary>
        ''' 这个函数总是会返回一个线程对象的
        ''' 
        ''' + 当有空闲的线程，会返回第一个空闲的线程
        ''' + 当没有空闲的线程，则会返回任务队列最短的线程
        ''' </summary>
        ''' <returns></returns>
        Private Function GetAvaliableThread() As TaskQueue(Of Long)
            Dim [short] As TaskQueue(Of Long) = threads.First

            If Not [short].RunningTask Then
                Return [short]
            End If

            For Each t As TaskQueue(Of Long) In threads
                If Not t.RunningTask Then
                    Return t
                ElseIf t.Tasks = 0 Then
                    Return t
                ElseIf t.MaximumQueue Then
                    Continue For
                Else
                    If [short].Tasks > t.Tasks Then
                        [short] = t
                    End If
                End If
            Next

            Return [short]
        End Function

        Public Sub WaitAll(Optional verbose As Boolean = False)
            Call Thread.Sleep(1000)

            Do While threads.Any(Function(t) t.RunningTask)
                Call Thread.Sleep(5000)
                Call Console.WriteLine()
                Call Console.WriteLine(ToString)
            Loop
        End Sub

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"   ------========== {Now.ToString} ==========------")
            Call sb.AppendLine($"{NameOf(Me.FullCapacity)}: {FullCapacity}")
            Call sb.AppendLine($"{NameOf(Me.NumOfThreads)}: {NumOfThreads}")
            Call sb.AppendLine($"{NameOf(Me.WorkingThreads)}: {WorkingThreads}")
            Call sb.AppendLine($"{NameOf(Me.pendings)}: {pendings.Count}")
            Call sb.AppendLine($"{NameOf(Me.totalTask)}: {totalTask}")
            Call sb.AppendLine($"Progress: [{Program.ProgressText(popoutTask / totalTask, 32)}] {(100 * popoutTask / totalTask).ToString("F2")}%")
            Call sb.AppendLine()
            Call sb.AppendLine(threads.JoinBy(vbCrLf))

            Return sb.ToString
        End Function

        Public Sub [Exit]()
            For Each task In threads
                Call task.Dispose()
            Next
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call [Exit]()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
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
