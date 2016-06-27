Imports System.Reflection

Namespace Parallel.Tasks

    ''' <summary>
    ''' 背景线程的任务抽象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Delegate Function IBackgroundTask(Of T)() As T

    Friend Class __backgroundTask(Of T) : Inherits IParallelTask

        ''' <summary>
        ''' 获取得到任务线程执行的输出结果
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As T
            Get
                If Not TaskComplete Then
                    Call Start()
                    Call WaitForExit()
                End If
                Return __getValue
            End Get
        End Property

        Public ReadOnly Property TaskHandle As IBackgroundTask(Of T)

        ReadOnly _taskThread As Threading.Thread

        Dim __getValue As T

        Public ReadOnly Property ExecuteException As Exception

        Public Overrides Function ToString() As String
            Return TaskHandle.ToString
        End Function

        Sub New(task As IBackgroundTask(Of T))
            _TaskHandle = task
            _taskThread = New Threading.Thread(AddressOf __invokeTask)
        End Sub

        ''' <summary>
        ''' 取消当前的任务的执行，在线程内部产生的异常可以在<see cref="ExecuteException"/>获取得到
        ''' </summary>
        Public Sub Abort()
            Call _taskThread.Abort()
            _TaskComplete = False
            _RunningTask = False
        End Sub

        Public Function Start() As __backgroundTask(Of T)
            If Not TaskRunning Then
                _taskThread.Start()
                _TaskComplete = False
            End If

            Return Me
        End Function

        Protected Overrides Sub __invokeTask()
            Me._RunningTask = True
            Me._TaskComplete = False
            Try
                __getValue = _TaskHandle()
            Catch ex As Exception
                _ExecuteException =
                    New Exception(MethodBase.GetCurrentMethod.GetFullName, ex)
            End Try
            Me._RunningTask = False
            Me._TaskComplete = True
        End Sub
    End Class
End Namespace