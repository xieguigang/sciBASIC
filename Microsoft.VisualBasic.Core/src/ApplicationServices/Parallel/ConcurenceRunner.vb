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

        Public Shared n_threads As Integer = 4

        Sub New(nsize As Integer)
            ThreadPool.SetMaxThreads(n_threads, 8)
            ThreadPool.SetMinThreads(n_threads, 2)

            workLen = nsize
        End Sub

        Protected MustOverride Sub Solve(start As Integer, ends As Integer)

        ''' <summary>
        ''' Run in sequence
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Solve() As VectorTask
            Solve(0, workLen - 1)
            Return Me
        End Function

        ''' <summary>
        ''' Run in parallel
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As VectorTask
            Dim span_size As Integer = workLen / n_threads
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

        Private Sub ParallelFor(span_size As Integer)
            Dim flags As New List(Of Boolean)
            Dim err As Boolean = False
            Dim exp As Exception = Nothing

            For cpu As Integer = 0 To n_threads
                Dim start As Integer = cpu * span_size
                Dim ends As Integer = start + span_size - 1
                Dim thread_id As Integer = cpu

                If start >= workLen Then
                    Exit For
                End If
                If ends >= workLen Then
                    ends = workLen - 1
                End If

                Call flags.Add(False)
                Call ThreadPool.QueueUserWorkItem(
                    Sub()
                        Try
                            Call Solve(start, ends)
                        Catch ex As Exception
                            ' just ignores of this error, or the task
                            ' flag check code will be a dead loop
                            exp = New Exception($"Error while execute the ParallelFor task in range from {start} to {ends}. (thread offset {thread_id})", ex)
                        End Try

                        Try
                            ' set flag for task complete
                            SyncLock flags
                                flags(thread_id) = True
                            End SyncLock
                        Catch ex As Exception
                            ' try to avoid the possible dead loop
                            err = True
                        End Try
                    End Sub)
            Next

            '#If NETCOREAPP Then
            '                Do While ThreadPool.PendingWorkItemCount > 0
            '                    Thread.Sleep(1)
            '                Loop
            '#Else
            '                Throw New NotImplementedException
            '#End If
            Dim check As Boolean()

            ' 20231011 try to avoid the collection was modified, enumerator
            ' will not executate problem
            SyncLock flags
                check = flags.ToArray
            End SyncLock

            Do While check.Any(Function(b) b = False)
                Call Thread.Sleep(1)

                If err Then
                    Exit Do
                Else
                    SyncLock flags
                        check = flags.ToArray
                    End SyncLock
                End If
            Loop

            If Not exp Is Nothing Then
                Throw exp
            End If
        End Sub

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