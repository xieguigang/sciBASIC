Imports System.Drawing
Imports System.Threading

Namespace Terminal

    Public Module TerminalEvents

        Public Delegate Sub ResizeEventHandle(size As Size, oldSize As Size)

        Dim __resizeHandles As New List(Of ResizeEventHandle)
        Dim _old As Size
        Dim _eventThread As Thread

        Public ReadOnly Property CurrentSize As Size
            Get
                Return _old
            End Get
        End Property

        Private Sub __detects()
            Do While App.Running
                If Console.WindowHeight <> _old.Height Then
                    RaiseEvent Resize()
                ElseIf Console.WindowWidth <> _old.Width Then
                    RaiseEvent Resize()
                End If

                Thread.Sleep(10)
            Loop
        End Sub

        ''' <summary>
        ''' Terminal resize event for [<see cref="Console.WindowWidth"/>, <see cref="Console.WindowHeight"/>]
        ''' </summary>
        Public Custom Event Resize As ResizeEventHandle
            AddHandler(value As ResizeEventHandle)
                If __resizeHandles.IndexOf(value) = -1 Then
                    __resizeHandles += value
                End If

                If _eventThread Is Nothing Then
                    _old = New Size(Console.WindowWidth, Console.WindowHeight)
                    _eventThread = New Thread(AddressOf __detects)
                    _eventThread.Start()
                End If
            End AddHandler
            RemoveHandler(value As ResizeEventHandle)
                If __resizeHandles.IndexOf(value) > -1 Then
                    __resizeHandles.Remove(value)
                End If

                If __resizeHandles.Count = 0 Then
                    _eventThread.Abort()
                    _eventThread = Nothing
                End If
            End RemoveHandler
            RaiseEvent()
                Dim [new] As New Size(Console.WindowWidth, Console.WindowHeight)

                For Each h As ResizeEventHandle In __resizeHandles
                    Call h([new], _old)
                Next

                _old = [new]
            End RaiseEvent
        End Event
    End Module
End Namespace