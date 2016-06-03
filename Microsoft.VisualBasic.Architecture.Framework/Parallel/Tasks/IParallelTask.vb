Namespace Parallel.Tasks

    Public MustInherit Class IParallelTask

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

        ''' <summary>
        ''' 这个函数会检查<see cref="TaskComplete"/>属性来判断任务是否执行完毕
        ''' </summary>
        Public Sub WaitForExit()
            Do While Not TaskComplete
                Call Threading.Thread.Sleep(1)
            Loop

            Call "Job DONE!".__DEBUG_ECHO
        End Sub

        Protected MustOverride Sub __invokeTask()
    End Class
End Namespace