Partial Public Class Deque(Of T)
    Private Class Enumerator(Of S)
        Implements IEnumerator(Of S)
        'initialize with -1 to ensure that InvalidOperationException is thrown when Current is called befor the first call of MoveNext
        Private Property curIndex As Integer = -1
        ''' <summary>
        ''' version of Deque<T> this Enumerator is enumerating from the moment this enumerator has been created
        ''' </summary></summary>       
        ''' 
        Private Property version As Long
        ''' <summary>
        ''' Deque<T> this enumerator is enumerating
        ''' </summary></summary>    
        Private Property Que As Deque(Of S)

        Public Sub New(ByVal que As Deque(Of S), ByVal version As Long)
            Me.version = version
            Me.Que = que
        End Sub

        Public ReadOnly Property p_Current As S Implements IEnumerator(Of S).Current
            Get

                If curIndex < 0 OrElse curIndex >= Que.Count OrElse version <> Que.version Then
                    Throw New InvalidOperationException()
                Else
                    Return Que(curIndex)
                End If
            End Get
        End Property

        Private ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                Return CSharpImpl.__Throw(Of Object)(New NotImplementedException())
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            Que = Nothing
            curIndex = Nothing
            version = Nothing
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If version <> Que.version Then
                Throw New InvalidOperationException()
            End If

            curIndex += 1

            If curIndex >= Que.Count Then
                Return False
            End If

            Return True
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            curIndex = 0
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal throw statements")>
            Shared Function __Throw(Of T)(ByVal e As Exception) As T
                Throw e
            End Function
        End Class
    End Class

    Private Class CSharpImpl
        <Obsolete("Please refactor calling code to use normal throw statements")>
        Shared Function __Throw(Of T)(ByVal e As Exception) As T
            Throw e
        End Function
    End Class
End Class
