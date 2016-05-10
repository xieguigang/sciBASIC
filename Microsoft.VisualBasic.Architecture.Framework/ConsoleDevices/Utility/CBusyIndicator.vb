Imports System.Runtime.CompilerServices

Namespace Terminal.Utility

    Public Class CBusyIndicator : Implements System.IDisposable

        Dim _IndicatorStyle As Char
        Dim _OnRunningState As Boolean = False
        Dim _TicksCount As Integer

        Sub New(Optional IndicatorStyle As Char = "."c, Optional _start As Boolean = False, Optional Ticks As Integer = -1)
            _IndicatorStyle = IndicatorStyle
            If _start Then Call Start(Ticks)
        End Sub

        Private Sub DoEvents()

            Do While _OnRunningState = True

                Call System.Threading.Thread.Sleep(1000)
                Call Console.Write(_IndicatorStyle)

                If _TicksCount > 0 Then
                    _TicksCount -= 1
                Else
                    If _TicksCount <> -1 Then
                        _OnRunningState = False
                    End If
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 运行进度条
        ''' </summary>
        ''' <param name="Ticks">The total ticking counts of the indicator, Unit is [second].</param>
        ''' <remarks></remarks>
        Public Sub Start(Optional Ticks As Integer = -1)
            If _OnRunningState = True Then
                Return
            End If

            _TicksCount = Ticks
            _OnRunningState = True
            Call New System.Threading.Thread(AddressOf DoEvents).Start()
        End Sub

        Public Sub [Stop]()
            _OnRunningState = False
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    _OnRunningState = False
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(      disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(      disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace