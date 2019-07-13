#Region "Microsoft.VisualBasic::ee7085a89d6da3f3e69575abf655dfa5, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\Threads\ThreadQueue.vb"

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

    '     Class ThreadQueue
    ' 
    '         Properties: MultiThreadSupport, Sleep
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: AddToQueue, (+2 Overloads) Dispose, exeQueue, WaitQueue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading

Namespace Parallel

    ''' <summary>
    ''' 任务线程队列
    ''' </summary>
    Public Class ThreadQueue : Implements IDisposable

        ''' <summary>
        ''' If TRUE, the Writing process will be separated from the main thread.
        ''' </summary>
        Public Property MultiThreadSupport As Boolean = True

        ''' <summary>
        ''' Just my queue
        ''' </summary>
        ReadOnly queue As New Queue(Of Action)(6666)
        ''' <summary>
        ''' lock
        ''' </summary>
        ReadOnly dummy As New Object()

        ''' <summary>
        ''' Writer Thread ☺
        ''' </summary>
        Dim myThread As New Thread(AddressOf exeQueue)

        ''' <summary>
        ''' Is thread running?
        ''' hum
        ''' </summary>
        Dim QSolverRunning As Boolean = False

        ''' <summary>
        ''' 任务队列是否是处于休眠状态?(任务队列的内容是空的)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Sleep As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not QSolverRunning
            End Get
        End Property

        Sub New()
            QSolverRunning = False
        End Sub

        ''' <summary>
        ''' Add an Action to the queue.
        ''' </summary>
        ''' <param name="A">``() => { .. }``, 任务的执行内容</param>
        Public Sub AddToQueue(A As Action)
            SyncLock queue
                Call queue.Enqueue(A)
            End SyncLock

            If MultiThreadSupport Then
                ' 只需要将任务添加到队列之中就行了
                SyncLock dummy
                    If Not myThread.IsAlive Then
                        QSolverRunning = False
                        myThread = New Thread(AddressOf exeQueue)

                        myThread.Name = "xConsole · Multi-Thread Writer"
                        myThread.Start()
                    End If
                End SyncLock
            Else
                ' 等待线程任务的执行完毕
                exeQueue()
            End If
        End Sub

        ''' <summary>
        ''' Wait for all thread queue job done.(Needed if you are using multiThreaded queue)
        ''' </summary>
        Public Sub WaitQueue()
            While QSolverRunning = True
                Thread.Sleep(10)
            End While
        End Sub

        ''' <summary>
        ''' Execute the queue list
        ''' </summary>
        Private Sub exeQueue()
            QSolverRunning = True

            While queue IsNot Nothing AndAlso queue.Count > 0
                Call Thread.MemoryBarrier()

                While True
                    Dim a As Action

                    SyncLock queue
                        If queue.Count = 0 Then
                            Exit While
                        Else
                            a = queue.Dequeue()
                        End If
                    End SyncLock

                    If a IsNot Nothing Then
                        a.Invoke()
                    End If
                End While
            End While

            QSolverRunning = False
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    Call WaitQueue()
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub

        ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码以正确实现可释放模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
            Dispose(True)
            ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
