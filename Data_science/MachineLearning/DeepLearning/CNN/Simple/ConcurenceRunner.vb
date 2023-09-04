Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace CNN

    ''' <summary>
    ''' the parallel task helper
    ''' </summary>
    Friend MustInherit Class VectorTask

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Solve()
            Call Solve(0, workLen - 1)
        End Sub

        Public Sub Run()
            Dim span_size As Integer = workLen / n_threads
#If NET48 Then
            span_size = 0
#End If
            If sequenceMode OrElse span_size < 1 Then
                ' run in sequence
                Call Solve(0, workLen - 1)
            Else
                For cpu As Integer = 0 To n_threads
                    Dim start As Integer = cpu * span_size
                    Dim ends As Integer = start + span_size - 1

                    If start >= workLen Then
                        Exit For
                    End If
                    If ends >= workLen Then
                        ends = workLen - 1
                    End If

                    ThreadPool.QueueUserWorkItem(Sub() Solve(start, ends))
                Next

#If NETCOREAPP Then
                Do While ThreadPool.PendingWorkItemCount > 0
                    Thread.Sleep(1)
                Loop
#Else
                Throw New NotImplementedException
#End If
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