Imports Microsoft.VisualBasic.My.JavaScript

Public Class HMM

    Friend ReadOnly states()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly observables()
    Friend ReadOnly emissionMatrix As Double()()

    Friend ReadOnly Bayes As Bayes
    Friend ReadOnly Viterbi As Viterbi
    Friend ReadOnly Forward As Forward
    Friend ReadOnly Backward As Backward
    Friend ReadOnly BaumWelch As BaumWelch

    Sub New(states As statesObject(), observables As observables(), init As Double())
        Me.states = states.map(Function(s) s.state)
        Me.transMatrix = states.map(Function(s) s.prob)
        Me.initialProb = init
        Me.observables = observables.map(Function(o) o.obs)
        Me.emissionMatrix = observables.map(Function(o) o.prob)

        Me.Bayes = New Bayes(Me)
        Me.Viterbi = New Viterbi(Me)
        Me.Forward = New Forward(Me)
        Me.Backward = New Backward(Me)
        Me.BaumWelch = New BaumWelch(Me)
    End Sub
End Class

Public Class statesObject

    Public Property state As Object
    Public Property prob As Double()

End Class

Public Class observables

    Public Property obs As Object
    Public Property prob As Double()

End Class