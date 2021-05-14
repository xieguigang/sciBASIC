Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Algorithm.HMMChainAlgorithm
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Algorithm.HMMAlgorithm

    Public Class Backward : Inherits HMMAlgorithmBase

        Sub New(HMM As HMM)
            Call MyBase.New(HMM)
        End Sub

        Public Function backwardAlgorithm(obSequence As Chain) As Beta
            Dim backward As New backwardFactory(HMM, obSequence)
            Dim initBetas = HMM.states.map(Function(s) 1.0)
            Dim allBetas = backward.recBackward(initBetas, obSequence.length - 1, New List(Of Double()) From {initBetas})

            Return New Beta With {
                .betas = allBetas,
                .betaF = backward.termBackward(allBetas)
            }
        End Function
    End Class
End Namespace