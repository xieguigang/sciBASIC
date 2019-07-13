#Region "Microsoft.VisualBasic::1110699dc4dd396363970e2d12d42435, Microsoft.VisualBasic.Core\My\InnerQueue.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module InnerQueue
    ' 
    '         Properties: InnerThread
    ' 
    '         Sub: AddToQueue, WaitQueue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel

Namespace My

    ''' <summary>
    ''' Task action Queue for terminal QUEUE SOLVER 🙉
    ''' </summary>
    Module InnerQueue

        Public ReadOnly Property InnerThread As New ThreadQueue

        ''' <summary>
        ''' 添加终端输出的任务到任务队列之中
        ''' </summary>
        ''' <param name="task"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddToQueue(task As Action)
            Call InnerThread.AddToQueue(task)
        End Sub

        ''' <summary>
        ''' Wait for all thread queue job done.(Needed if you are using multiThreaded queue)
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WaitQueue()
            Call InnerThread.WaitQueue()
        End Sub
    End Module
End Namespace
