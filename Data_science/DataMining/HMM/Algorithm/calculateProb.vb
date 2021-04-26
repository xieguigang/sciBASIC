Imports Microsoft.VisualBasic.My.JavaScript

Public Class calculateProb(Of T)

    Dim stateTrans2 As Array, init As Array, states As T()

    Sub New(stateTrans2 As Array, init As Array, states As Array)
        Me.states = states
        Me.init = init
        Me.stateTrans2 = stateTrans2
    End Sub

    Public Function sequenceProb(sequence As IEnumerable(Of T))
        Return findSequence(sequence, states).reduce(Function(total As Integer, curr As Array, i As Integer, arr As Array)
                                                         If (i = 0) Then
                                                             total += init(curr)
                                                         Else
                                                             total *= stateTrans2(arr(i - 1))(curr)
                                                         End If

                                                         Return total
                                                     End Function, 0)
    End Function
End Class
