#Region "Microsoft.VisualBasic::428f68cffb11bf82c69327e04c881853, ApplicationServices\Parallel\Threads\ThreadPool.vb"

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

    '     Class ThreadPool
    ' 
    '         Properties: FullCapacity, NumOfThreads, ServerLoad, WorkingThreads
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetAvaliableThread, GetStatus, ToString
    ' 
    '         Sub: __allocate, (+2 Overloads) Dispose, OperationTimeOut, RunTask
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
Imports System.Threading
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON
Imports taskBind = Microsoft.VisualBasic.ComponentModel.Binding(Of System.Action, System.Action(Of Long))

Namespace Parallel.Threads

    ''' <summary>
    ''' 使用多条线程来执行任务队列，推荐在编写Web服务器的时候使用这个模块来执行任务
    ''' </summary>
    Public Class ThreadPool : Implements IDisposable

        ReadOnly __threads As TaskQueue(Of Long)()
        ''' <summary>
        ''' 临时的句柄缓存
        ''' </summary>
        ReadOnly __pendings As New Queue(Of taskBind)(capacity:=10240)

        ''' <summary>
        ''' 线程池之中的线程数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumOfThreads As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return __threads.Length
            End Get
        End Property

        ''' <summary>
        ''' 返回当前正在处于工作状态的线程数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property WorkingThreads As Integer
            Get
                Dim n As Integer

                For Each t In __threads
                    If t.Tasks > 0 Then
                        n += 1
                    End If
                Next

                Return n
            End Get
        End Property

        ''' <summary>
        ''' Returns the server load.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ServerLoad As Double
            Get
                Dim works# = WorkingThreads / NumOfThreads
                Dim CPU_load# = Win32.TaskManager.ProcessUsage
                Dim load# = works * CPU_load

                Return load
            End Get
        End Property

        ''' <summary>
        ''' 是否所有的线程都是处于工作状态的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FullCapacity As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return WorkingThreads = __threads.Length
            End Get
        End Property

        Sub New(maxThread As Integer)
            __threads = New TaskQueue(Of Long)(maxThread) {}

            For i As Integer = 0 To __threads.Length - 1
                __threads(i) = New TaskQueue(Of Long)
            Next

            Call ParallelExtension.RunTask(AddressOf __allocate)
        End Sub

        Sub New()
            Me.New(LQuerySchedule.Recommended_NUM_THREADS)
        End Sub

        ''' <summary>
        ''' 获取当前的这个线程池对象的状态的摘要信息
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStatus() As Dictionary(Of String, String)
            Dim out As New Dictionary(Of String, String)

            Call out.Add(NameOf(Me.FullCapacity), FullCapacity)
            Call out.Add(NameOf(Me.NumOfThreads), NumOfThreads)
            Call out.Add(NameOf(Me.WorkingThreads), WorkingThreads)
            Call out.Add(NameOf(Me.__pendings), __pendings.Count)

            For Each t As SeqValue(Of TaskQueue(Of Long)) In __threads.SeqIterator
                With (+t)
                    Call out.Add("thread___" & t.i & "___" & .uid, .Tasks)
                End With
            Next

            Return out
        End Function

        ''' <summary>
        ''' 使用线程池里面的空闲线程来执行任务
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="callback">回调函数里面的参数是任务的执行的时间长度</param>
        Public Sub RunTask(task As Action, Optional callback As Action(Of Long) = Nothing)
            Dim pends As New taskBind With {
                .Bind = task,
                .Target = callback
            }
            SyncLock __pendings
                Call __pendings.Enqueue(pends)
            End SyncLock
        End Sub

        Public Sub OperationTimeOut(task As Action, timeout As Integer)
            Dim done As Boolean = False

            Call RunTask(task, Sub() done = True)

            For i As Integer = 0 To timeout
                If done Then
                    Exit For
                Else
                    Thread.Sleep(1)
                End If
            Next
        End Sub

        Private Sub __allocate()
            Do While Not Me.disposedValue
                SyncLock __pendings
                    If __pendings.Count > 0 Then
                        Dim task As taskBind = __pendings.Dequeue
                        Dim h As Func(Of Long) = AddressOf New __taskInvoke With {.task = task.Bind}.Run
                        Dim callback As Action(Of Long) = task.Target
                        Call GetAvaliableThread.Enqueue(h, callback)  ' 当线程池里面的线程数量非常多的时候，这个事件会变长，所以讲分配的代码单独放在线程里面执行，以提神web服务器的响应效率
                    Else
                        Call Thread.Sleep(1)
                    End If
                End SyncLock
            Loop
        End Sub

        Private Structure __taskInvoke
            Dim task As Action

            ''' <summary>
            ''' 不清楚是不是因为lambda有问题，所以导致计时器没有正常的工作，所以在这里使用内部类来工作
            ''' </summary>
            ''' <returns></returns>
            Public Function Run() As Long
                Dim time& = App.NanoTime
                Call task()
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
            Dim [short] As TaskQueue(Of Long) = __threads.First

            For Each t In __threads
                If Not t.RunningTask Then
                    Return t
                Else
                    If [short].Tasks > t.Tasks Then
                        [short] = t
                    End If
                End If
            Next

            Return [short]
        End Function

        Public Overrides Function ToString() As String
            Return __threads.GetJson
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
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
