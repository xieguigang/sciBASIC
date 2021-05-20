Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm

    Public Class CalculateProb

        Dim stateTrans2 As Double()()
        Dim init As Double()
        Dim states As StatesObject()

        Sub New(stateTrans2 As Double()(), init As Double(), states As StatesObject())
            Me.states = states
            Me.init = init
            Me.stateTrans2 = stateTrans2
        End Sub

        Public Function SequenceProb(sequence As Chain) As Double
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
End Namespace