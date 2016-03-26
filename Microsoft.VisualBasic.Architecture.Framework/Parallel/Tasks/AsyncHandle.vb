Namespace Parallel.Tasks

    ''' <summary>
    ''' Represents the status of an asynchronous operation.(背景线程加载数据)
    ''' </summary>
    ''' <typeparam name="TOut"></typeparam>
    Public Class AsyncHandle(Of TOut)

        Public ReadOnly Property Task As Func(Of TOut)
        Public ReadOnly Property Handle As IAsyncResult

        ''' <summary>
        ''' Gets a value that indicates whether the asynchronous operation has completed.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsCompleted As Boolean
            Get
                If Handle Is Nothing Then
                    Return True
                End If

                Return Handle.IsCompleted
            End Get
        End Property

        Sub New(Task As Func(Of TOut))
            Me.Task = Task
        End Sub

        ''' <summary>
        ''' Start the background task thread.(启动后台背景线程)
        ''' </summary>
        ''' <returns></returns>
        Public Function Run() As AsyncHandle(Of TOut)
            If IsCompleted Then
                Me._Handle = Task.BeginInvoke(Nothing, Nothing) ' 假若没有执行完毕也调用的话，会改变handle
            End If

            Return Me
        End Function

        ''' <summary>
        ''' 没有完成会一直阻塞线程在这里
        ''' </summary>
        ''' <returns></returns>
        Public Function GetValue() As TOut
            If Handle Is Nothing Then
                Return Me._Task()
            Else
                Return Me.Task.EndInvoke(Handle)
            End If
        End Function
    End Class
End Namespace