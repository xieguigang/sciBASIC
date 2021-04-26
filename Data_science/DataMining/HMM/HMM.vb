Public Class HMM

    Dim states
    Dim transMatrix
    Dim initialProb
    Dim observables
    Dim emissionMatrix

    Dim Bayes As Bayes
    Dim Viterbi As Viterbi
    Dim Forward As Forward
    Dim Backward As Backward
    Dim BaumWelch As BaumWelch

    Sub New(states, observables, init)
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
