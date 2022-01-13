Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' https://github.com/coderespawn/permutations
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Friend Class Combination(Of T)

        Private data As T()
        Private selectionCount As Integer
        Private stack As New Stack(Of StackState)()

        Friend Structure StackState
            Public buffer As T()
            Public startIndex As Integer
        End Structure

        Public ReadOnly Property CanRun As Boolean
            Get
                Return stack.Count > 0
            End Get
        End Property

        Public Sub New(data As T(), selectionCount As Integer)
            Me.data = data
            Me.selectionCount = selectionCount

            stack.Push(New StackState With {
                .buffer = New T(-1) {},
                .startIndex = 0
            })
        End Sub

        Public Function Execute() As T()
            While stack.Count > 0
                Dim top = stack.Pop()

                If top.buffer.Length = selectionCount Then
                    Return top.buffer
                Else
                    Dim bufferLength = top.buffer.Length
                    Dim itemsLeft = selectionCount - bufferLength - 1
                    Dim endIndex = data.Length - itemsLeft

                    For i = top.startIndex To endIndex - 1
                        Dim nextBuffer = New List(Of T)(top.buffer)
                        nextBuffer.Add(data(i))
                        Dim nextState = New StackState With {
                            .buffer = nextBuffer.ToArray(),
                            .startIndex = i + 1
                        }
                        stack.Push(nextState)
                    Next
                End If
            End While

            Return Nothing
        End Function
    End Class
End Namespace
