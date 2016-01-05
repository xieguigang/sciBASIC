Namespace ConsoleDevice.Utility

    Public Class EventProcess

        Dim current As Integer
        Public Property Capacity As Integer
            Get
                Return _Capacity
            End Get
            Set(value As Integer)
                _Capacity = value
                delta = Capacity / 100
                current = CInt(_Capacity * p)
            End Set
        End Property

        Dim _Capacity As Integer
        Dim delta As Integer
        Dim TAG As String

        Sub New(n As Integer, Optional TAG As String = "")
            Me.Capacity = n
            Me.TAG = TAG

            If String.IsNullOrEmpty(Me.TAG) Then
                Me.TAG = vbTab
            End If
        End Sub

        Public Sub Tick()
            If delta = 0 Then
                Return
            End If

            current += 1
            If current Mod delta = 0 Then
                Call ToString.__DEBUG_ECHO
            End If
        End Sub

        Dim p As Double

        Public Overrides Function ToString() As String
            If Capacity = 0 Then
                Return ""
            End If

            p = current / Capacity

            Return $" [{TAG}] * ...... {Mid(100 * p, 1, 5)}%"
        End Function
    End Class

    Public Class ConsoleBusyIndicator : Implements System.IDisposable

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