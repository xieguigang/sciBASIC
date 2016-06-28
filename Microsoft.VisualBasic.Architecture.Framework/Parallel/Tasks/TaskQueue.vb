#Region "fffe6d4292b80ccc80503548e3a8d0d9, ..\Microsoft.VisualBasic.Architecture.Framework\Parallel\Tasks\TaskQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace Parallel.Tasks

    Public Interface ITaskHandle(Of T)
        Function Run() As T
    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class TaskQueue(Of T) : Implements IDisposable

        ReadOnly __tasks As New Queue(Of __task)

        Sub New()
            Call RunTask(AddressOf __taskQueueEXEC)
        End Sub

        ''' <summary>
        ''' 函数会被插入一个队列之中，之后线程会被阻塞在这里直到函数执行完毕，这个主要是用来控制服务器上面的任务并发的
        ''' </summary>
        ''' <param name="handle"></param>
        ''' <returns>假若本对象已经开始Dispose了，则为完成的任务都会返回Nothing</returns>
        Public Function Join(handle As Func(Of T)) As T
            Dim task As New __task With {
                .handle = handle
            }
            Call __tasks.Enqueue(task)
            Call task.receiveDone.WaitOne()
            Call task.receiveDone.Reset()

            Return task.Value
        End Function

        Public Function Join(handle As ITaskHandle(Of T)) As T
            Return Join(AddressOf handle.Run)
        End Function

        Public Sub Enqueue(handle As ITaskHandle(Of T), Optional callback As Action(Of T) = Nothing)
            Call Enqueue(AddressOf handle.Run, callback)
        End Sub

        ''' <summary>
        ''' 这个函数只会讲任务添加到队列之中，而不会阻塞线程
        ''' </summary>
        ''' <param name="handle"></param>
        Public Sub Enqueue(handle As Func(Of T), Optional callback As Action(Of T) = Nothing)
            Dim task As New __task With {
                .handle = handle,
                .callback = callback
            }
            Call __tasks.Enqueue(task)

            If Not callback Is Nothing Then
                Call RunTask(AddressOf task.TriggerCallback)
            End If
        End Sub

        Public ReadOnly Property RunningTask As Boolean

        Private Sub __taskQueueEXEC()
            Do While Not disposedValue
                If Not __tasks.Count = 0 Then
                    Dim task As __task = __tasks.Dequeue
                    _RunningTask = True
                    Call task.Run()
                    Call task.receiveDone.Set()
                    _RunningTask = False
                Else
                    Call Thread.Sleep(1)
                End If
            Loop
        End Sub

        Private Class __task

            Public callback As Action(Of T)
            Public handle As Func(Of T)
            Public receiveDone As New ManualResetEvent(False)

            Public ReadOnly Property Value As T

            Sub Run()
                Try
                    _Value = handle()
                Catch ex As Exception
                    Call App.LogException(ex)
                    Call ex.PrintException
                End Try
            End Sub

            Sub TriggerCallback()
                Call receiveDone.WaitOne()
                Call receiveDone.Reset()
                Call callback(Value)
            End Sub
        End Class

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    For Each x As __task In __tasks
                        ' 释放所有的线程阻塞
                        Call x.receiveDone.Set()
                    Next
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
