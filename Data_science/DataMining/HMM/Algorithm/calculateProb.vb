Imports Microsoft.VisualBasic.My.JavaScript

Public Class calculateProb

    Dim stateTrans2 As Double()()
    Dim init As Double()
    Dim states As statesObject()

    Sub New(stateTrans2 As Double()(), init As Double(), states As statesObject())
        Me.states = states
        Me.init = init
        Me.stateTrans2 = stateTrans2
    End Sub

    Public Function sequenceProb(sequence As Chain) As Double
        Return findSequence(sequence, states) _
            .reduce(Function(total As Double, curr As Integer, i As Integer, arr As Integer())
                        If (i = 0) Then
                            total += init(curr)
                        Else
                            total *= stateTrans2(arr(i - 1))(curr)
                        End If

                        Return total
                    End Function, 0.0)
    End Function
End Class
