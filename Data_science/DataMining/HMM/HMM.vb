Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript
Imports Matrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.GeneralMatrix

''' <summary>
''' HMM用于建模数据序列，无论是从连续概率分布还是从离散概率分布得出的。
''' 它们与状态空间和高斯混合模型相关，因为它们旨在估计引起观测的状态。
''' 状态是未知或“隐藏”的，并且HMM试图估计状态，类似于无监督聚类过程。
''' </summary>
Public Class HMM

    Friend ReadOnly states As String()
    Friend ReadOnly transMatrix As Double()()
    Friend ReadOnly initialProb As Double()
    Friend ReadOnly observables As String()
    Friend ReadOnly emissionMatrix As Double()()

    Friend ReadOnly Bayes As Bayes
    Friend ReadOnly Viterbi As Viterbi
    Friend ReadOnly Forward As Forward
    Friend ReadOnly Backward As Backward
    Friend ReadOnly BaumWelch As BaumWelch

    Sub New(states As StatesObject(), observables As Observable(), init As Double())
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

    Public Function GetTransMatrix() As Matrix
        Return New Matrix(transMatrix)
    End Function

    Public Function bayesTheorem(ob, hState) As Double
        Return Bayes.bayesTheorem(ob, hState)
    End Function

    Public Function forwardAlgorithm(obSequence As Chain) As Alpha
        Return Forward.forwardAlgorithm(obSequence)
    End Function

    Public Function backwardAlgorithm(obSequence As Chain) As Beta
        Return Backward.backwardAlgorithm(obSequence)
    End Function

    Public Function viterbiAlgorithm(obSequence As Chain) As viterbiSequence
        Return Viterbi.viterbiAlgorithm(obSequence)
    End Function

    Public Function baumWelchAlgorithm(obSequence As Chain) As HMM
        Return BaumWelch.baumWelchAlgorithm(obSequence)
    End Function
End Class
