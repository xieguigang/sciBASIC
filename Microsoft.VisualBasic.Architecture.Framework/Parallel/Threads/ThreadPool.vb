Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Parallel.Threads

    ''' <summary>
    ''' 使用多条线程来执行任务队列，推荐在编写Web服务器的时候使用这个模块来执行任务
    ''' </summary>
    Public Class ThreadPool
        Implements IDisposable

        ReadOnly __threads As TaskQueue(Of Long)()
        ''' <summary>
        ''' 临时的句柄缓存
        ''' </summary>
        ReadOnly __pendings As New Queue(Of KeyValuePair(Of Action, Action(Of Long)))

        ''' <summary>
        ''' 线程池之中的线程数量
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumOfThreads As Integer
            Get
                Return __threads.Length
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
        ''' 使用线程池里面的空闲线程来执行任务
        ''' </summary>
        ''' <param name="task"></param>
        ''' <param name="callback">回调函数里面的参数是任务的执行的时间长度</param>
        Public Sub RunTask(task As Action, Optional callback As Action(Of Long) = Nothing)
            Call __pendings.Enqueue(New KeyValuePair(Of Action, Action(Of Long))(task, callback))
        End Sub

        Private Sub __allocate()
            Do While Not Me.disposedValue
                If __pendings.Count > 0 Then
                    Dim task = __pendings.Dequeue
                    Dim h As Func(Of Long) = Function() Time(work:=task.Key)
                    Dim callback = task.Value
                    Call GetAvaliableThread.Enqueue(h, callback)  ' 当线程池里面的线程数量非常多的时候，这个事件会变长，所以讲分配的代码单独放在线程里面执行，以提神web服务器的响应效率
                Else
                    Call Thread.Sleep(1)
                End If
            Loop
        End Sub

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