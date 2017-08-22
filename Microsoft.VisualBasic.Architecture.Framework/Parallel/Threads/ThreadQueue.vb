#Region "Microsoft.VisualBasic::1a96ced7ac9c88cf4da30e3b21ee7a90, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Parallel\Threads\ThreadQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Threading

Namespace Parallel

    ''' <summary>
    ''' 任务线程队列
    ''' </summary>
    Public Class ThreadQueue : Implements IDisposable

        ''' <summary>
        ''' Writer Thread ☺
        ''' </summary>
        Dim MyThread As New Thread(AddressOf exeQueue)

        ''' <summary>
        ''' If TRUE, the Writing process will be separated from the main thread.
        ''' </summary>
        Public Property MultiThreadSupport As Boolean = True

        ''' <summary>
        ''' Just my queue
        ''' </summary>
        ReadOnly Queue As New Queue(Of Action)(6666)

        ''' <summary>
        ''' Is thread running?
        ''' hum
        ''' </summary>
        Dim QSolverRunning As Boolean = False

        Dim waitForExit As Boolean = False

        ''' <summary>
        ''' lock
        ''' </summary>
        Dim dummy As New Object()

        Sub New()
            QSolverRunning = False
        End Sub

        ''' <summary>
        ''' Add an Action to the queue.
        ''' </summary>
        ''' <param name="A">()=>{ .. }</param>
        Public Sub AddToQueue(A As Action)
            SyncLock Queue
                Call Queue.Enqueue(A)
            End SyncLock

            If MultiThreadSupport Then ' 只需要将任务添加到队列之中就行了
                SyncLock dummy
                    If Not MyThread.IsAlive Then
                        QSolverRunning = False
                        MyThread = New Thread(AddressOf exeQueue)

                        MyThread.Name = "xConsole · Multi-Thread Writer"
                        MyThread.Start()
                    End If
                End SyncLock
            Else
                exeQueue()  ' 等待线程任务的执行完毕
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

            While Queue IsNot Nothing AndAlso Queue.Count > 0
                Call Thread.MemoryBarrier()

                While True
                    Dim a As Action

                    SyncLock Queue
                        If Queue.Count = 0 Then
                            Exit While
                        Else
                            a = Queue.Dequeue()
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
                    waitForExit = True
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
