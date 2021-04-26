Imports Microsoft.VisualBasic.My.JavaScript

Public Class MarkovChain

    Friend ReadOnly states As Object()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly prob As calculateProb

    Sub New(states As statesObject(), init As Double())
        Me.states = states.map(Function(s) s.state)
        Me.transMatrix = states.map(Function(s) s.prob)
        Me.initialProb = init
        Me.prob = New calculateProb(transMatrix, init, states)
    End Sub
End Class

