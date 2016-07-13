Imports System.Threading
Imports Microsoft.VisualBasic.Language

Namespace Parallel.Threads

    Public MustInherit Class CodeThread : Inherits ClassObject

        Protected ReadOnly __thread As Thread

        Sub New()
            __thread = New Thread(AddressOf __run)
        End Sub

        Protected MustOverride Sub __run()

        Public Shared Function GetThread(x As CodeThread) As Thread
            Return x.__thread
        End Function

        Public Shared Sub Run(x As CodeThread)
            Call x.__thread.Start()
        End Sub

        Public Shared Sub Pause(x As CodeThread)
            Call x.__thread.Suspend()
        End Sub

        Public Shared Sub [Resume](x As CodeThread)
            Call x.__thread.Resume()
        End Sub
    End Class
End Namespace