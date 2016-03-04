Namespace Parallel

    ''' <summary>
    ''' Running a specific <see cref="System.Action"/> in the background periodically.
    ''' (比较适合用于在服务器上面执行周期性的计划任务)
    ''' </summary>
    Public Class UpdateThread : Implements System.IDisposable

        ''' <summary>
        ''' ms
        ''' </summary>
        ''' <returns></returns>
        Public Property Periods As Integer
        Public ReadOnly Property Updates As System.Action
        ''' <summary>
        ''' If this exception handler is null, then when the unhandled exception occurring, 
        ''' this thread object will throw the exception and then stop working.
        ''' </summary>
        ''' <returns></returns>
        Public Property ErrHandle As Action(Of Exception)
        ''' <summary>
        ''' 指示当前的这个任务处理对象是否处于运行状态
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Running As Boolean

        ''' <summary>
        ''' Running a specific action in the background periodically. The time unit of the parameter <paramref name="Periods"/> is ms or Ticks.
        ''' </summary>
        ''' <param name="Periods">ms</param>
        ''' <param name="updates"></param>
        Sub New(Periods As Integer, updates As System.Action)
            Me.Periods = Periods
            Me.Updates = updates
            Call RunTask(AddressOf __updates)
        End Sub

        Private Sub __updates()
            Do While Running
                Call __invoke()
            Loop
        End Sub

        ''' <summary>
        ''' 运行这条线程，假若更新线程已经在运行了，则会自动忽略这次调用
        ''' </summary>
        Public Sub Start()
            If Running Then
                Return
            Else
                _Running = True
                Call RunTask(AddressOf __updates)
            End If
        End Sub

        ''' <summary>
        ''' 停止更新线程的运行
        ''' </summary>
        Public Sub [Stop]()
            _Running = False
        End Sub

        Private Sub __invoke()
            Try
                Call _Updates()
            Catch ex As Exception
                If Not ErrHandle Is Nothing Then
                    Call _ErrHandle(ex)
                Else
                    Throw
                End If
            Finally
                Call Threading.Thread.Sleep(Periods)
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Dim state As String = If(Running, NameOf(Running), NameOf([Stop]))
            Return $"[{state}, {Me.Periods}ms]  => {Me.Updates.ToString}"
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call [Stop]()  ' TODO: dispose managed state (managed objects).
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