Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.GeneralMatrix

Public Class MarkovChain

    Friend ReadOnly states As String()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly prob As CalculateProb

    Sub New(states As StatesObject(), init As Double())
        Me.states = states.map(Function(s) s.state)
        Me.transMatrix = states.map(Function(s) s.prob)
        Me.initialProb = init
        Me.prob = New CalculateProb(transMatrix, init, states)
    End Sub

    Public Function GetTransMatrix() As Matrix
        Return New Matrix(transMatrix)
    End Function

    Public Function SequenceProb(sequence As Chain) As Double
        Return prob.SequenceProb(sequence)
    End Function
End Class

