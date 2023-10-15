Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Parallel

    ''' <summary>
    ''' the parallel task helper
    ''' </summary>
    Public MustInherit Class VectorTask

        Protected workLen As Integer
        ''' <summary>
        ''' set this flag value to value TRUE for run algorithm debug
        ''' </summary>
        Protected sequenceMode As Boolean = False

        ''' <summary>
        ''' <see cref="n_threads"/>
        ''' </summary>
        Protected ReadOnly cpu_count As Integer = n_threads

        Public Shared n_threads As Integer = 4

        Sub New(nsize As Integer)
            ThreadPool.SetMaxThreads(n_threads)

            workLen = nsize
            cpu_count = n_threads
        End Sub

        Protected MustOverride Sub Solve(start As Integer, ends As Integer)

        ''' <summary>
        ''' Run in sequence
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Solve() As VectorTask
            sequenceMode = True
            Solve(0, workLen - 1)
            Return Me
        End Function

        ''' <summary>
        ''' Run in parallel
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As VectorTask
            Dim span_size As Integer = workLen / cpu_count
            '#If NET48 Then
            '            span_size = 0
            '#End If
            If sequenceMode OrElse span_size < 1 Then
                ' run in sequence
                Call Solve()
            Else
                Call ParallelFor(span_size)
            End If

            Return Me
        End Function

        ''' <summary>
        ''' implements the parallel for use the thread pool
        ''' </summary>
        ''' <param name="span_size"></param>
        Private Sub ParallelFor(span_size As Integer)
            Dim err As Boolean = False
            Dim exp As Exception = Nothing

            For cpu As Integer = 0 To cpu_count
                Dim start As Integer = cpu * span_size
                Dim ends As Integer = start + span_size - 1
                Dim thread_id As Integer = cpu

                If start >= workLen Then
                    Exit For
                End If
                If ends >= workLen Then
                    ends = workLen - 1
                End If

                Call ThreadPool.QueueUserWorkItem(
                    Sub()
                        Try
                            Call Solve(start, ends)
                        Catch ex As Exception
                            ' just ignores of this error, or the task
                            ' flag check code will be a dead loop
                            exp = New Exception($"Error while execute the ParallelFor task in range from {start} to {ends}. (thread offset {thread_id})", ex)
                        End Try
                    End Sub)
            Next

            Do While Not ThreadPool.JobComplete
                Thread.Sleep(1)
            Loop

            If Not exp Is Nothing Then
                Throw exp
            End If
        End Sub

        Private Class ThreadPool

            Shared ReadOnly threads As New List(Of Task)
            Shared max As Integer

            Public Shared ReadOnly Property JobComplete As Boolean
                Get
                    Return threads.All(Function(t) Not t.isRunning)
                End Get
            End Property

            Public Shared Sub QueueUserWorkItem(task As Action)
                Dim t = GetAvaiableThread()
                t.SetTask(task)
            End Sub

            Public Shared Sub SetMaxThreads(n As Integer)
                max = n

                If max > threads.Count Then
                    For i As Integer = threads.Count To max - 1
                        Call threads.Add(Task.Start)
                    Next
                End If
            End Sub

            Private Shared Function GetAvaiableThread() As Task
re0:
                For i As Integer = 0 To max - 1
                    If Not threads(i).isRunning Then
                        Return threads(i)
                    End If
                Next

                Call Thread.Sleep(1)

                GoTo re0
            End Function

            Private Class Task

                Dim t As Thread
                Dim task As Action
                Dim running As Boolean = False

                Public ReadOnly Property isRunning As Boolean
                    Get
                        Return running
                    End Get
                End Property

                Public Sub SetTask(task As Action)
                    Me.task = task
                End Sub

                Public Shared Function Start() As Task
                    Return New Task With {.t = Parallel.RunTask(AddressOf .Run)}
                End Function

                Private Sub Run()
                    Do While App.Running
                        If Not task Is Nothing Then
                            running = True
                            task()
                            task = Nothing
                            running = False
                        End If

                        Thread.Sleep(1)
                    Loop
                End Sub
            End Class
        End Class

        Public Shared Function CopyMemory(Of T)(v As T(), start As Integer, ends As Integer) As T()
            Dim copy As T() = New T(start - ends - 1) {}
            Array.ConstrainedCopy(v, start, copy, Scan0, copy.Length)
            Return copy
        End Function

        ''' <summary>
        ''' copy all data of <paramref name="span"/> to the target region inside <paramref name="v"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="v"></param>
        ''' <param name="span">may be a part of region inside <paramref name="v"/></param>
        ''' <param name="start"></param>
        Public Shared Sub CopyMemory(Of T)(v As T(), span As T(), start As Integer)
            Array.ConstrainedCopy(span, Scan0, v, start, span.Length)
        End Sub
    End Class
End Namespace