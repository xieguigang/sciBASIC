Public Class Viterbi : Inherits HMMAlgorithm

    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub
    Public Function viterbiAlgorithm(obSequence)
        Dim viterbi As New viterbiFactory(HMM, obSequence)
        Dim initTrellis = viterbi.initViterbi()
        Dim psiArrays = HMM.states.map(Function(s) {null}) ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
        Dim recTrellisPsi = viterbi.recViterbi(initTrellis, 1, psiArrays, {initTrellis})
        Dim pTerm = viterbi.termViterbi(recTrellisPsi)
        Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)
        Return New With {.stateSequence = backtrace, .trellisSequence = recTrellisPsi.trellisSequence, .terminationProbability = pTerm.maximizedProbability}
    End Function
End Class
