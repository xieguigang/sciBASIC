
Namespace Scripting.TokenIcer

    ''' <summary>
    ''' A general script token stack helper
    ''' </summary>
    Public Class TokenStack(Of Tokens As IComparable)

        ReadOnly stack As New Stack(Of CodeToken(Of Tokens))

        ''' <summary>
        ''' current stack is empty?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEmpty As Boolean
            Get
                Return stack.Count = 0
            End Get
        End Property

        Public Sub Push(c As CodeToken(Of Tokens))
            stack.Push(c)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="c">
        ''' should be a stack ``close`` token
        ''' </param>
        ''' <returns></returns>
        Public Function Pop(c As CodeToken(Of Tokens)) As StackStates
            If stack.Count = 0 Then
                Return StackStates.MisMatched
            End If

            Dim peek = stack.Peek
            Dim matched As Boolean = False

            Select Case c.text
                Case "}" : matched = peek.text = "{"
                Case ")" : matched = peek.text = "("
                Case "]" : matched = peek.text = "["
                Case Else
                    Throw New NotImplementedException($"{peek.text} -- {c.text}")
            End Select

            If matched Then
                Call stack.Pop()
                Return StackStates.Closed
            Else
                Return StackStates.MisMatched
            End If
        End Function
    End Class

    Public Enum StackStates
        Closed
        ''' <summary>
        ''' syntax error?
        ''' </summary>
        MisMatched
    End Enum
End Namespace