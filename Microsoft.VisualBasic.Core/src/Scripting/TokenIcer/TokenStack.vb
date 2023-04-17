
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' A general script token stack helper
    ''' </summary>
    Public Class TokenStack(Of Tokens As IComparable)

        ReadOnly stack As New Stack(Of (index As Integer, token As CodeToken(Of Tokens)))

        ''' <summary>
        ''' current stack is empty?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEmpty As Boolean
            Get
                Return stack.Count = 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return stack.Select(Function(t) t.token.text).JoinBy(" -> ")
        End Function

        Public Sub Push(c As CodeToken(Of Tokens), Optional index As Integer = -1)
            stack.Push((index, c))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="c">
        ''' should be a stack ``close`` token
        ''' </param>
        ''' <returns></returns>
        Public Function Pop(c As CodeToken(Of Tokens), Optional index As Integer = -1) As StackStates
            If stack.Count = 0 Then
                Return New StackStates With {.MisMatched = True}
            End If

            Dim peek = stack.Peek
            Dim matched As Boolean = False

            Select Case c.text
                Case "}" : matched = peek.token.text = "{"
                Case ")" : matched = peek.token.text = "("
                Case "]" : matched = peek.token.text = "["
                Case Else
                    Throw New NotImplementedException($"({peek.index}){peek.token.text} -- ({index}){c.text}")
            End Select

            If matched Then
                Call stack.Pop()
                Return New StackStates With {
                    .MisMatched = False,
                    .Range = New IntRange({peek.index, index})
                }
            Else
                Return New StackStates With {.MisMatched = True}
            End If
        End Function
    End Class

    Public Class StackStates

        ''' <summary>
        ''' syntax error?
        ''' </summary>
        Public Property MisMatched As Boolean
        Public Property Range As IntRange

        Public Function GetRange(Of T)(data As IEnumerable(Of T)) As IEnumerable(Of T)
            Return data.Skip(Range.Min).Take(Range.Length + 1)
        End Function
    End Class
End Namespace