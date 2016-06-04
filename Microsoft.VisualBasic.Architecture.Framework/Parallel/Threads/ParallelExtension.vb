Imports System.Runtime.CompilerServices

Namespace Parallel

    Public Module ParallelExtension

        ''' <summary>
        ''' Start a new thread and then returns the background thread task handle.
        ''' </summary>
        ''' <param name="start"></param>
        ''' <returns></returns>
        <Extension> Public Function RunTask(start As Threading.ThreadStart) As Threading.Thread
            Dim Thread As New Threading.Thread(start)
            Call Thread.Start()
            Return Thread
        End Function
    End Module
End Namespace