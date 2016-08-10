Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Parallel.Linq
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Parallel.Threads

    ''' <summary>
    ''' 使用多条线程来执行任务队列
    ''' </summary>
    Public Class ThreadPool

        ReadOnly __threads As TaskQueue(Of Long)()

        Sub New(maxThread As Integer)
            __threads = New TaskQueue(Of Long)(maxThread) {}

            For i As Integer = 0 To __threads.Length - 1
                __threads(i) = New TaskQueue(Of Long)
            Next
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
            Dim h As Func(Of Long) = Function() Time(work:=task)
            Call GetAvaliableThread.Enqueue(h, callback)
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
    End Class
End Namespace