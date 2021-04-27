Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class Viterbi : Inherits HMMAlgorithmBase

        Sub New(HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function viterbiAlgorithm(obSequence As Chain) As viterbiSequence
            Dim viterbi As New viterbiFactory(HMM, obSequence)
            Dim initTrellis = viterbi.initViterbi()
            ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
            Dim psiArrays As New PsiArray(HMM.states.map(Function(s) New List(Of Integer)))
            Dim recTrellisPsi = viterbi.recViterbi(initTrellis.ToArray, 1, psiArrays, New List(Of Double()) From {initTrellis.ToArray})
            Dim pTerm = viterbi.termViterbi(recTrellisPsi)
            Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)

            Return New viterbiSequence With {
                .stateSequence = backtrace,
                .trellisSequence = recTrellisPsi.trellisSequence,
                .terminationProbability = pTerm.maximizedProbability
            }
        End Function
    End Class
End Namespace
