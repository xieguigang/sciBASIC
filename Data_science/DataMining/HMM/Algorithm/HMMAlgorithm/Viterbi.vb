Imports Microsoft.VisualBasic.My.JavaScript

Public Class Viterbi : Inherits HMMAlgorithm

    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub

    Public Function viterbiAlgorithm(obSequence) As viterbiSequence
        Dim viterbi As New viterbiFactory(HMM, obSequence)
        Dim initTrellis = viterbi.initViterbi()
        ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
        Dim psiArrays = HMM.states.map(Function(s) New Integer() {})
        Dim recTrellisPsi = viterbi.recViterbi(initTrellis.ToArray, 1, psiArrays, {initTrellis})
        Dim pTerm = viterbi.termViterbi(recTrellisPsi)
        Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)

        Return New viterbiSequence With {
            .stateSequence = backtrace,
            .trellisSequence = recTrellisPsi.trellisSequence,
            .terminationProbability = pTerm.maximizedProbability
        }
    End Function
End Class

Public Class viterbiSequence
    Public Property trellisSequence As Double()()
    Public Property terminationProbability As Double
    Public Property stateSequence As Object()
End Class