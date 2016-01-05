Imports System.Reflection

Namespace Parallel

    Public Delegate Function BackgroundTask(Of T)() As T

    Friend Class __backgroundTask(Of T)

        Public ReadOnly Property IsCompleted As Boolean
        Public ReadOnly Property IsRunning As Boolean
        Public ReadOnly Property Value As T
            Get
                Call Start()
                Call __waitComplete()
                Return __getValue
            End Get
        End Property

        Public ReadOnly Property TaskHandle As BackgroundTask(Of T)

        ReadOnly _taskThread As Threading.Thread

        Dim __getValue As T

        Public ReadOnly Property ExecuteException As Exception

        Public Overrides Function ToString() As String
            Return TaskHandle.ToString
        End Function

        Private Sub __runTask()
            _IsRunning = True
            _IsCompleted = False
            Try
                __getValue = _TaskHandle()
            Catch ex As Exception
                _ExecuteException =
                    New Exception(MethodBase.GetCurrentMethod.GetFullName, ex)
            End Try
            _IsRunning = False
            _IsCompleted = True
        End Sub

        Private Sub __waitComplete()
            Do While IsRunning
                Call Threading.Thread.Sleep(10)
            Loop
        End Sub

        Sub New(task As BackgroundTask(Of T))
            _TaskHandle = task
            _taskThread = New Threading.Thread(AddressOf __runTask)
        End Sub

        Public Sub Abort()
            Call _taskThread.Abort()
        End Sub

        Public Function Start() As __backgroundTask(Of T)
            If Not IsRunning Then
                _taskThread.Start()
            End If

            Return Me
        End Function
    End Class

    ''' <summary>
    ''' 更加底层的线程模式，和LINQ相比不会受到CPU核心数目的限制
    ''' </summary>
    ''' <typeparam name="T">后台任务的执行参数</typeparam>
    ''' <typeparam name="TOut">后台任务的执行结果</typeparam>
    Public Class Task(Of T, TOut) : Inherits ParallelTaskCommon

        Dim _Handle As Func(Of T, TOut)
        Dim _Input As T

        ''' <summary>
        ''' 假若任务已经完成，则会返回计算值，假若没有完成，则只会返回空值，假若想要在任何情况之下都会得到后台任务所执行的计算结果，请使用<see cref="GetValue()"/>方法
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As TOut

        ''' <summary>
        ''' 假若后台任务还没有完成，则函数会一直阻塞在这里直到任务执行完毕，假若任务早已完成，则函数会立即返回数据
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Not Me.TaskRunning Then
                Call Start()
            End If

            Call WaitForExit()
            Return _Value
        End Function

        Sub New(Input As T, Handle As Func(Of T, TOut))
            _Handle = Handle
            _Input = Input
        End Sub

        Public Function Start() As Task(Of T, TOut)
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Return Me
        End Function

        Protected Overrides Sub __invokeTask()

            _TaskComplete = False
            _RunningTask = True
            _Value = _Handle(_Input)
            _TaskComplete = True
            _RunningTask = False

        End Sub
    End Class

    Public MustInherit Class ParallelTaskCommon

        Public ReadOnly Property TaskComplete As Boolean
            Get
                Return _TaskComplete
            End Get
        End Property

        Public ReadOnly Property TaskRunning As Boolean
            Get
                Return _RunningTask
            End Get
        End Property

        Protected _RunningTask As Boolean
        Protected _TaskComplete As Boolean = False

        Public Sub WaitForExit()
            Do While Not TaskComplete
                Call Threading.Thread.Sleep(1)
            Loop

            Call "Job DONE!".__DEBUG_ECHO
        End Sub

        Protected MustOverride Sub __invokeTask()
    End Class

    Public Class Task : Inherits ParallelTaskCommon

        Public Event TaskJobComplete()

        Dim _Handle As Action

        Sub New(Handle As Action)
            _Handle = Handle
        End Sub

        Protected Overrides Sub __invokeTask()
            _TaskComplete = False
            _RunningTask = True
            Call _Handle()
            _TaskComplete = True
            _RunningTask = False
        End Sub

        Public Function Start() As Task
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Call Threading.Thread.Sleep(1)
            Call New Threading.Thread(AddressOf RaisingEvent).Start()
            Return Me
        End Function

        Private Sub RaisingEvent()
            Call WaitForExit()
            RaiseEvent TaskJobComplete()
        End Sub
    End Class

    Public Class Task(Of T) : Inherits ParallelTaskCommon

        Dim _Input As T
        Dim _Handle As Action(Of T)

        Public Event TaskJobComplete()

        Sub New(Input As T, Handle As Action(Of T))
            _Input = Input
            _Handle = Handle
        End Sub

        Public Function Start() As Task(Of T)
            _TaskComplete = False
            _RunningTask = True
            Call New Threading.Thread(AddressOf __invokeTask).Start()
            Call Threading.Thread.Sleep(1)
            Call New Threading.Thread(AddressOf RaisingEvent).Start()
            Return Me
        End Function

        Private Sub RaisingEvent()
            Call WaitForExit()
            RaiseEvent TaskJobComplete()
        End Sub

        Protected Overrides Sub __invokeTask()
            _TaskComplete = False
            _RunningTask = True
            Call _Handle(_Input)
            _TaskComplete = True
            _RunningTask = False
        End Sub
    End Class
End Namespace