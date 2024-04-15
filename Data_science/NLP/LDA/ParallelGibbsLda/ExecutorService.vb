Imports Microsoft.VisualBasic.Parallel

Namespace LDA

    Public Class ExecutorService : Inherits VectorTask

        ReadOnly workers As GibbsWorker()

        Public Sub New(gibbsWorks As GibbsWorker(), Optional workers As Integer? = Nothing)
            MyBase.New(nsize:=gibbsWorks.Length, verbose:=False, workers)
            Me.workers = gibbsWorks
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Call workers(cpu_id).run()
        End Sub
    End Class
End Namespace