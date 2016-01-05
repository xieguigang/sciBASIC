Imports System.Runtime.CompilerServices

Public Module PBSThreading

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Handles"></param>
    ''' <param name="NumOfThreads">同时执行的句柄的数目</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Invoke([Handles] As System.Action(), NumOfThreads As Integer) As Integer
        Dim CurrentThreads As Integer
        Dim InternalStartThread As System.Action(Of System.Action) = Sub(Handle As System.Action)
                                                                         CurrentThreads += 1
                                                                         Call Handle()
                                                                         CurrentThreads -= 1
                                                                     End Sub '启动计算线程
        Dim InternalCreateThread As System.Action(Of System.Action) =
            Sub(Handle As System.Action) Call (New System.Threading.Thread(start:=Sub() InternalStartThread([Handle]))).Start()
        Dim St As Stopwatch = Stopwatch.StartNew

        For Each Handle As System.Action In [Handles]
_AWAIT:     Call System.Threading.Thread.Sleep(1)

            If CurrentThreads > NumOfThreads Then
                Call System.Threading.Thread.Sleep(10)
                GoTo _AWAIT
            End If

            Call InternalCreateThread(Handle)
        Next

        Do While CurrentThreads > 0
            Call System.Threading.Thread.Sleep(10)
        Loop

        Return St.ElapsedTicks
    End Function
End Module
