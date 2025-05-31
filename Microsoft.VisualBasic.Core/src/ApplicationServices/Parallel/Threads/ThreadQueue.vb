#Region "Microsoft.VisualBasic::45c27d54b350ff978293a280495b58c3, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\ThreadQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 170
    '    Code Lines: 88 (51.76%)
    ' Comment Lines: 56 (32.94%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 26 (15.29%)
    '     File Size: 5.52 KB


    '     Class ThreadQueue
    ' 
    '         Properties: lockQueue, MultiThreadSupport, RunningTask, Sleep
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetPendingTasks
    ' 
    '         Sub: AddToQueue, (+2 Overloads) Dispose, ExecQueue, ThreadLoop, WaitQueue
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
        Dim xThread As New Thread(AddressOf ExecQueue)

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

        Public Property lockQueue As Boolean = False

        ''' <summary>
        ''' get the task that is running now.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RunningTask As Action

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
                    If Not xThread.IsAlive Then
                        QSolverRunning = False

                        xThread = New Thread(AddressOf ExecQueue)
                        xThread.Name = "Multi-Thread Worker"
                        xThread.Start()
                    End If
                End SyncLock
            Else
                ' 等待线程任务的执行完毕
                Call ExecQueue()
            End If
        End Sub

        ''' <summary>
        ''' Wait for all thread queue job done.(Needed if you are using multiThreaded queue)
        ''' </summary>
        Public Sub WaitQueue()
            While QSolverRunning = True
                Call Thread.Sleep(10)
            End While
        End Sub

        ''' <summary>
        ''' Execute the queue list
        ''' </summary>
        Private Sub ExecQueue()
            QSolverRunning = True

            While queue IsNot Nothing AndAlso queue.Count > 0
                Call Thread.MemoryBarrier()
                Call ThreadLoop()
            End While

            QSolverRunning = False
        End Sub

        ''' <summary>
        ''' running all task inside current <see cref="queue"/>
        ''' </summary>
        Private Sub ThreadLoop()
            While App.Running
                If lockQueue Then
                    Continue While
                End If

                SyncLock queue
                    If queue.Count = 0 Then
                        Exit While
                    Else
                        _RunningTask = queue.Dequeue()
                    End If
                End SyncLock

                If _RunningTask IsNot Nothing Then
                    Call _RunningTask()
                End If
            End While
        End Sub

        Public Function GetPendingTasks() As Action()
            SyncLock queue
                Return queue.AsEnumerable.ToArray
            End SyncLock
        End Function

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
