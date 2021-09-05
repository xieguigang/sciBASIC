Imports System.Runtime.CompilerServices
Imports ParallelTask = System.Threading.Tasks.Parallel

Namespace Parallel.Threads

    Public MustInherit Class ThreadStart

        Public MustOverride Sub run()

        ''' <summary>
        ''' Run parallel task
        ''' </summary>
        ''' <param name="task"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub execute(task As IEnumerable(Of ThreadStart))
            Call ParallelTask.ForEach(task, Sub(thread) thread.run())
        End Sub
    End Class
End Namespace